using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;

namespace 自定义窗体控件_FormAttribute
{
    public partial class FormAttribute : Form
    {
        public IActiveView m_activeView { get; set; }//map视图

        public FormAttribute()
        {
            InitializeComponent();
        }

        //图层属性的生成,当前的要素图层
        private IFeatureLayer _curFeatureLayer;
        public IFeatureLayer CurFeatureLayer
        {
            get { return _curFeatureLayer; }
            set { _curFeatureLayer = value; }
        }

        #region 初始化
        public void InitUI()
        {
            if (_curFeatureLayer == null) return;
            IFeature pFeature = null;
            DataTable pFeatDT = new DataTable();
            DataRow pDataRow = null;
            DataColumn pDataColum = null;
            IField pField = null;
            for (int i = 0; i < _curFeatureLayer.FeatureClass.Fields.FieldCount; i++)
            {
                pDataColum = new DataColumn();
                pField = _curFeatureLayer.FeatureClass.Fields.get_Field(i);
                pDataColum.ColumnName = pField.AliasName;//列名等于字段名
                pDataColum.DataType = Type.GetType("System.Object");
                pFeatDT.Columns.Add(pDataColum);
            }
            IFeatureCursor pFeatureCursor = _curFeatureLayer.Search(null, true);
            pFeature = pFeatureCursor.NextFeature();
            while (pFeature != null)
            {
                pDataRow = pFeatDT.NewRow();
                for (int k = 0; k < pFeatDT.Columns.Count; k++)
                {
                    pDataRow[k] = pFeature.get_Value(k);
                }
                pFeatDT.Rows.Add(pDataRow);
                pFeature = pFeatureCursor.NextFeature();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
            dataGridAttribute.DataSource = pFeatDT;//绑定数据源
        }
        #endregion

        #region 双击该行，缩放到当前要素
        private void dataGridAttribute_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1. 防止点击表头（RowIndex 为 -1）导致程序崩溃
            if (e.RowIndex < 0) return;

            try
            {
                // 获取当前所点击的行
                DataGridViewRow row = dataGridAttribute.Rows[e.RowIndex];

                // 2. 【数据类型安全】使用 TryParse 或判断 Value 是否为 null
                object idValue = row.Cells[0].Value;
                if (idValue == null) return;

                int objectID = Convert.ToInt32(idValue);

                // 获取要素
                IFeature feature = CurFeatureLayer.FeatureClass.GetFeature(objectID);

                // 3. 【优化】获取要素几何范围
                ESRI.ArcGIS.Geometry.IEnvelope outEnvelope = feature.Shape.Envelope;

                // 4. 处理“点”要素
                // 如果是点，Envelope 的宽高为 0，直接 SetExtent 会出错或无效
                if (outEnvelope.Width == 0 || outEnvelope.Height == 0)
                {
                    // 向四周扩展一定比例或固定距离（根据地图单位，这里假设是度或米）
                    // 参数说明：dx, dy, asRatio (是否按比例)
                    // 这里设置为 false，表示向四周各扩展 0.1 个地图单位
                    outEnvelope.Expand(0.01, 0.01, false);
                }
                else
                {
                    // 如果是面或线，为了美观，通常也会稍微放大一点，避免要素顶在屏幕边缘
                    outEnvelope.Expand(1.2, 1.2, true);
                }

                if (m_activeView != null)
                {
                    m_activeView.Extent = outEnvelope;
                    m_activeView.Refresh();
                }
                else
                {
                    MessageBox.Show("未获取到地图视图对象 (ActiveView)，无法缩放。");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("定位要素失败: " + ex.Message);
            }
        }
        #endregion
    }
}
