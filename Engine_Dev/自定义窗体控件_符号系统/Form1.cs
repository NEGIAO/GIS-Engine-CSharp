using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace 自定义窗体控件_符号系统
{
    public partial class FormSymbolize : Form
    {
        // 1. 声明成员变量，用于接收主程序的控件
        private IMapControl3 m_mapControl = null;
        private ITOCControl2 m_tocControl = null;

        // 2. 构造函数用于外部传入 Map和TOC
        public FormSymbolize(IMapControl3 mapControl, ITOCControl2 tocControl)
        {
            InitializeComponent();
            this.m_mapControl = mapControl;
            this.m_tocControl = tocControl;
        }


        // 窗体加载事件：初始化所有控件,赋初值
        private void FormSymbolize_Load(object sender, EventArgs e)
        {
            // 1. 安全检查
            if (m_mapControl == null || m_mapControl.LayerCount == 0)
            {
                MessageBox.Show("当前地图没有任何图层，无法进行符号化！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            // 2. 初始化图层下拉框
            cmbLayer.Items.Clear();
            for (int i = 0; i < m_mapControl.LayerCount; i++)
            {
                ILayer layer = m_mapControl.get_Layer(i);
                if (layer is IFeatureLayer && layer.Valid)
                {
                    IFeatureLayer fl = layer as IFeatureLayer;
                    if (fl.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon ||
                        fl.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                    {
                        cmbLayer.Items.Add(new LayerItem(layer.Name, fl));
                    }
                }
            }

            // 检查是否有可用图层
            if (cmbLayer.Items.Count == 0)
            {
                MessageBox.Show("当前地图中没有符合要求的要素图层（仅支持面或线图层）。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                return;
            }

            // 3. 初始化分级方法下拉框
            cmbMethod.Items.Clear();
            cmbMethod.Items.Add("等距分级");
            cmbMethod.Items.Add("自然间断");

            // 4. 初始化颜色方案 (cmbColor) 下拉框
            cmbColor.Items.Clear();
            cmbColor.Items.Add("红色系 (Reds)");
            cmbColor.Items.Add("绿色系 (Greens)");
            cmbColor.Items.Add("蓝色系 (Blues)");
            cmbColor.Items.Add("热力图 (黄->红)"); 
            cmbColor.Items.Add("彩虹色 (Spectral)");

            cmbColor.SelectedIndex = 0;

            // 5. 设置所有控件的默认选中值
            cmbLayer.SelectedIndex = 0; 
            cmbMethod.SelectedIndex = 0;
            cmbColor.SelectedIndex = 0;
            numClasses.Text = "5";
        }

        // 图层改变事件：联动加载字段
        private void cmbLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbField.Items.Clear();
            LayerItem selectedItem = cmbLayer.SelectedItem as LayerItem;
            if (selectedItem == null) return;

            IFields fields = selectedItem.Layer.FeatureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField field = fields.get_Field(i);
                if (SymbologyHelper.IsNumericField(field))
                {
                    cmbField.Items.Add(field.Name);
                }
            }
            if (cmbField.Items.Count > 0) cmbField.SelectedIndex = 0;
        }

        // 3. 点击应用按钮：调用 Helper 并刷新主地图
        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. 获取参数
                LayerItem selectedLayerItem = cmbLayer.SelectedItem as LayerItem;
                if (selectedLayerItem == null) throw new Exception("请选择图层！");
            
                string fieldName = cmbField.Text;
                if (string.IsNullOrEmpty(fieldName)) throw new Exception("请选择字段！");

                int classCount;
                if (!int.TryParse(numClasses.Text, out classCount)) throw new Exception("分级数必须是数字！");

                // 2. 解析枚举参数
                SymbologyHelper.ClassifyMethod method = SymbologyHelper.ClassifyMethod.EqualInterval;
                if (cmbMethod.Text == "自然间断") method = SymbologyHelper.ClassifyMethod.NaturalBreaks;

                SymbologyHelper.ColorStyle colorStyle = SymbologyHelper.ColorStyle.Reds;
                string selColor = cmbColor.SelectedItem.ToString();

                if (selColor.Contains("绿色")) colorStyle = SymbologyHelper.ColorStyle.Greens;
                else if (selColor.Contains("蓝色")) colorStyle = SymbologyHelper.ColorStyle.Blues;
                else if (selColor.Contains("热力图")) colorStyle = SymbologyHelper.ColorStyle.Heatmap;
                else if (selColor.Contains("彩虹色")) colorStyle = SymbologyHelper.ColorStyle.Spectral;

                // 3. 调用 Helper 类库进行渲染
                SymbologyHelper.RenderClassBreaks(selectedLayerItem.Layer, fieldName, classCount, method, colorStyle);

                // 4. 主地图刷新
                m_mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            
                // 刷新 TOC (图例)
                if (m_tocControl != null) m_tocControl.Update();

                MessageBox.Show("渲染成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("渲染失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 内部类：用于 ComboBox 存储图层对象
        private class LayerItem
        {
            public string Name { get; set; }
            public IFeatureLayer Layer { get; set; }
            public LayerItem(string name, IFeatureLayer layer) { Name = name; Layer = layer; }
            public override string ToString() { return Name; } // ComboBox 显示的内容
        }

        #region 设计器代码
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSymbolize));
            this.cmbLayer = new System.Windows.Forms.ComboBox();
            this.cmbField = new System.Windows.Forms.ComboBox();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.numClasses = new System.Windows.Forms.NumericUpDown();
            this.btnApply = new System.Windows.Forms.Button();
            this.cmbColor = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numClasses)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbLayer
            // 
            this.cmbLayer.FormattingEnabled = true;
            this.cmbLayer.Location = new System.Drawing.Point(193, 123);
            this.cmbLayer.Name = "cmbLayer";
            this.cmbLayer.Size = new System.Drawing.Size(121, 20);
            this.cmbLayer.TabIndex = 0;
            this.cmbLayer.SelectedIndexChanged += new System.EventHandler(this.cmbLayer_SelectedIndexChanged);
            // 
            // cmbField
            // 
            this.cmbField.FormattingEnabled = true;
            this.cmbField.Location = new System.Drawing.Point(603, 123);
            this.cmbField.Name = "cmbField";
            this.cmbField.Size = new System.Drawing.Size(121, 20);
            this.cmbField.TabIndex = 1;
            // 
            // cmbMethod
            // 
            this.cmbMethod.FormattingEnabled = true;
            this.cmbMethod.Location = new System.Drawing.Point(193, 230);
            this.cmbMethod.Name = "cmbMethod";
            this.cmbMethod.Size = new System.Drawing.Size(121, 20);
            this.cmbMethod.TabIndex = 2;
            // 
            // numClasses
            // 
            this.numClasses.Location = new System.Drawing.Point(603, 234);
            this.numClasses.Name = "numClasses";
            this.numClasses.Size = new System.Drawing.Size(120, 21);
            this.numClasses.TabIndex = 3;
            // 
            // btnApply
            // 
            this.btnApply.Font = new System.Drawing.Font("宋体", 42F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnApply.Location = new System.Drawing.Point(307, 397);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(183, 83);
            this.btnApply.TabIndex = 4;
            this.btnApply.Text = "应用";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // cmbColor
            // 
            this.cmbColor.FormattingEnabled = true;
            this.cmbColor.Location = new System.Drawing.Point(276, 346);
            this.cmbColor.Name = "cmbColor";
            this.cmbColor.Size = new System.Drawing.Size(233, 20);
            this.cmbColor.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(58, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 29);
            this.label1.TabIndex = 6;
            this.label1.Text = "选择图层";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(459, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 29);
            this.label2.TabIndex = 7;
            this.label2.Text = "选择字段";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(58, 221);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 29);
            this.label3.TabIndex = 8;
            this.label3.Text = "选择方法";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(459, 221);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 29);
            this.label4.TabIndex = 9;
            this.label4.Text = "选择类别";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(333, 314);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(129, 29);
            this.label5.TabIndex = 10;
            this.label5.Text = "选择颜色";
            // 
            // FormSymbolize
            // 
            this.ClientSize = new System.Drawing.Size(799, 541);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbColor);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.numClasses);
            this.Controls.Add(this.cmbMethod);
            this.Controls.Add(this.cmbField);
            this.Controls.Add(this.cmbLayer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormSymbolize";
            this.Text = "分级色彩符号";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormSymbolize_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numClasses)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}
