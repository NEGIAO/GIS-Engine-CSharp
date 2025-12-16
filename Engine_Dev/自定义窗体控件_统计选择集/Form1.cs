using System;
using System.Collections.Generic; // 使用泛型集合 List
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace 自定义窗体控件_统计选择集
{
    public partial class FormStatistics : Form
    {
        private IMap currentMap;

        // 【修改1】使用 List 代替 Hashtable，通过索引直接获取对象，避免重名冲突和查找失败
        private List<IFeatureLayer> layerList;

        private IFeatureLayer currentFeatureLayer = null;

        /// <summary>
        /// 获得当前MapControl控件中的Map对象。
        /// </summary>
        public IMap CurrentMap
        {
            set { currentMap = value; }
        }

        public FormStatistics()
        {
            InitializeComponent();
            // 初始化列表
            layerList = new List<IFeatureLayer>();
        }

        private void FormStatistics_Load(object sender, EventArgs e)
        {
            if (currentMap == null) return;

            // 清空旧数据
            layerList.Clear();
            comboBoxLayers.Items.Clear();
            comboxBoxFields.Items.Clear(); 

            int layersCount = 0; // 具有选择要素的图层数
            int allSelectedFeatures = 0; // 被选择要素的总数

            // 遍历地图中的所有图层
            for (int i = 0; i < currentMap.LayerCount; i++)
            {
                ILayer tempLayer = currentMap.get_Layer(i);

                // 如果是图层组 (GroupLayer)，需要递归遍历里面的图层
                if (tempLayer is GroupLayer)
                {
                    ICompositeLayer compositeLayer = tempLayer as ICompositeLayer;
                    for (int j = 0; j < compositeLayer.Count; j++)
                    {
                        // 调用辅助函数处理单个图层
                        ProcessLayer(compositeLayer.get_Layer(j), ref layersCount, ref allSelectedFeatures);
                    }
                }
                else
                {
                    // 处理普通图层
                    ProcessLayer(tempLayer, ref layersCount, ref allSelectedFeatures);
                }
            }

            // 显示统计概况
            labelSelection.Text = string.Format("当前地图选择集共有 {0} 个图层的 {1} 个要素被选中。", layersCount, allSelectedFeatures);

            // 【关键】选中第一个图层，这将自动触发 comboBoxLayers_SelectedIndexChanged 事件
            // 从而自动给 currentFeatureLayer 赋值
            if (comboBoxLayers.Items.Count > 0)
            {
                comboBoxLayers.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 辅助方法：判断图层是否为矢量图层且有选中要素，如果有则添加到列表和界面
        /// </summary>
        private void ProcessLayer(ILayer pLayer, ref int count, ref int featureCount)
        {
            // 使用 as 进行安全转换，如果是栅格图层会返回 null，不会报错
            IFeatureLayer featureLayer = pLayer as IFeatureLayer;

            // 必须是矢量图层
            if (featureLayer != null)
            {
                IFeatureSelection featureSelection = featureLayer as IFeatureSelection;
                // 必须有被选中的要素
                if (featureSelection != null && featureSelection.SelectionSet.Count > 0)
                {
                    // 1. 添加名称到界面下拉框
                    comboBoxLayers.Items.Add(pLayer.Name);
                    // 2. 添加对象到后台列表 (索引位置与界面保持一致)
                    layerList.Add(featureLayer);

                    count++;
                    featureCount += featureSelection.SelectionSet.Count;
                }
            }
        }

        // 当“图层”下拉框发生变化时 -> 更新 currentFeatureLayer 并填充字段
        private void comboBoxLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 防止索引越界
            if (comboBoxLayers.SelectedIndex < 0 || comboBoxLayers.SelectedIndex >= layerList.Count)
                return;

            // 直接通过索引从 List 获取图层，绝对不会为空
            currentFeatureLayer = layerList[comboBoxLayers.SelectedIndex];

            // 清空字段下拉框
            comboxBoxFields.Items.Clear();

            // 获取图层字段
            if (currentFeatureLayer.FeatureClass != null)
            {
                IFields iFields = currentFeatureLayer.FeatureClass.Fields;
                for (int i = 0; i < iFields.FieldCount; i++)
                {
                    IField field = iFields.get_Field(i);
                    // 排除非数值字段和系统保留字段
                    if (field.Name.ToUpper() != "OBJECTID" && field.Name.ToUpper() != "SHAPE" && field.Name.ToUpper() != "FID")
                    {
                        // 只添加数值类型
                        if (field.Type == esriFieldType.esriFieldTypeInteger ||
                            field.Type == esriFieldType.esriFieldTypeDouble ||
                            field.Type == esriFieldType.esriFieldTypeSingle ||
                            field.Type == esriFieldType.esriFieldTypeSmallInteger)
                        {
                            comboxBoxFields.Items.Add(field.Name);
                        }
                    }
                }
            }

            // 如果有可用的字段，默认选中第一个，这将触发字段统计事件
            if (comboxBoxFields.Items.Count > 0)
                comboxBoxFields.SelectedIndex = 0;
            else
                labelStatisticResults.Text = "该图层没有可用于统计的数值字段。";
        }

        // 当“字段”下拉框发生变化时 -> 执行统计计算
        // 注意：这里的方法名对应的是 comboxBoxFeilds (字段控件)
        private void comboBoxFeilds_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 安全检查：图层或字段为空则不执行
            if (currentFeatureLayer == null || comboxBoxFields.SelectedItem == null)
                return;

            try
            {
                IDataStatistics dataStatistics = new DataStatisticsClass();

                dataStatistics.Field = comboxBoxFields.SelectedItem.ToString();

                // 获取选择集游标
                IFeatureSelection featureSelection = currentFeatureLayer as IFeatureSelection;
                ICursor cursor = null;
                // 使用 null 作为查询过滤器，获取所有选中的要素
                featureSelection.SelectionSet.Search(null, false, out cursor);

                dataStatistics.Cursor = cursor;

                // 执行统计
                IStatisticsResults statisticsResults = dataStatistics.Statistics;

                // 构建结果字符串
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("统计字段： " + dataStatistics.Field);
                stringBuilder.AppendLine("统计总数： " + statisticsResults.Count.ToString());
                stringBuilder.AppendLine("最小值： " + statisticsResults.Minimum.ToString());
                stringBuilder.AppendLine("最大值： " + statisticsResults.Maximum.ToString());
                stringBuilder.AppendLine("总计： " + statisticsResults.Sum.ToString());
                stringBuilder.AppendLine("平均值： " + statisticsResults.Mean.ToString());
                stringBuilder.AppendLine("标准差： " + statisticsResults.StandardDeviation.ToString());

                // 显示结果
                labelStatisticResults.Text = stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                labelStatisticResults.Text = "统计过程中发生错误：\n" + ex.Message;
            }
        }
    }
}