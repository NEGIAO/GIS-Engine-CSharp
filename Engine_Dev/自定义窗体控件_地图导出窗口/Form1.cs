using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;//ArcGIS的引用文件
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;

namespace 自定义窗体控件_地图导出窗口
{
    public partial class FormExportMap : Form
    {
        #region 私有字段,属性等
        private string pSavePath = "";
        private IActiveView pActiveView;
        /// <summary>
        /// 只写属性
        /// </summary>
        private IGeometry pGeometry = null;
        public IGeometry GetGeometry
        {
            set { pGeometry = value; }
        }
        /// <summary>
        /// 只写属性，导出全局或者区域
        /// </summary>
        private bool bRegion = true;
        public bool IsRegion
        {
            set { bRegion = value; }
        }
        #endregion

        // -------------------------------------------------------------
        // 完善后的构造函数
        // -------------------------------------------------------------
        public FormExportMap(ESRI.ArcGIS.Controls.AxMapControl mapControl)
        {
            InitializeComponent();

            // 1. 将传入的 AxMapControl 的 ActiveView 赋值给成员变量 pActiveView
            if (mapControl != null)
            {
                pActiveView = mapControl.ActiveView;
            }
            else
            {
                // 可选：如果传入的控件为空，可以抛出异常或进行日志记录
                MessageBox.Show("错误：未传入有效的地图控件！", "初始化失败",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void btnExPath_Click(object sender, EventArgs e)
        {
            SaveFileDialog pSaveFileD = new SaveFileDialog();
            pSaveFileD.DefaultExt = "jpg|bmp|gif";
            pSaveFileD.Filter = "JPEF文件(*.jpg)|*.jpg|BMP文件(*.bmp)|*.bmp";
            pSaveFileD.OverwritePrompt = true;
            pSaveFileD.Title = "另存为";
            txtExPath.Text ="";
            if (pSaveFileD.ShowDialog() != DialogResult.Cancel)
	        {
		        pSavePath = pSaveFileD.FileName;
                txtExPath.Text = pSaveFileD.FileName;
	        }
        }

        private void FormExportMap_Load(object sender, EventArgs e)
        {
            cboResolution.Text = pActiveView.ScreenDisplay.DisplayTransformation.Resolution.ToString();
            cboResolution.Items.Add(cboResolution);
            if (bRegion)
	        {
		        IEnvelope pEnvelop = pGeometry.Envelope;
                tagRECT ptagRect = new tagRECT();
                pActiveView.ScreenDisplay.DisplayTransformation.TransformRect(pEnvelop,ref ptagRect,9);
                if (cboResolution.Text != "")
	            {
		            txtWidth.Text =ptagRect.right.ToString();
                    txtHeight.Text = ptagRect.bottom.ToString();
	            }
	        }
            else
	        {
                txtWidth.Text = pActiveView.ExportFrame.right.ToString();
                txtWidth.Text = pActiveView.ExportFrame.bottom.ToString();
	        }
        }

        //设定分辨率，计算导出区域的尺寸
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            double num = (int) Math.Round(pActiveView.ScreenDisplay.DisplayTransformation.Resolution);
            if (cboResolution.Text == "")
	        {
		        txtWidth.Text = "";
                txtHeight.Text = "";
                    return ;
	        }
            if (bRegion)
	        {
		        IEnvelope pEnvelop = pGeometry.Envelope;
                
                tagRECT pRECT = new tagRECT();
                //将地图坐标转换为屏幕坐标
                pActiveView.ScreenDisplay.DisplayTransformation.TransformRect(pEnvelop,ref pRECT,9);
                txtWidth.Text = Math.Round((double)(pRECT.right * (double.Parse(cboResolution.Text)/(double)num))).ToString();
	            txtHeight.Text = Math.Round((double)(pRECT.bottom * (double.Parse(cboResolution.Text)/(double)num))).ToString();
            }
            else
	        {
                tagRECT pRECT = new tagRECT();
                txtWidth.Text = Math.Round((double)(pRECT.right * (double.Parse(cboResolution.Text)/(double)num))).ToString();
	            txtHeight.Text = Math.Round((double)(pRECT.bottom * (double.Parse(cboResolution.Text)/(double)num))).ToString();
	        }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (txtExPath.Text == "")
            {
                MessageBox.Show("请先确定导出的路径！","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            int resolution = int.Parse(cboResolution.Text);//输出分辨率
            int width = int.Parse(txtWidth.Text);
            int height = int.Parse(txtHeight.Text);
            //地图导出的核心代码
            ExportMap.ExportView(pActiveView,pGeometry,resolution,width,height,pSavePath,bRegion);
            pActiveView.GraphicsContainer.DeleteAllElements();
            pActiveView.Refresh();
            MessageBox.Show("导出成功");
        }

        //取消按钮
        private void btnCancel_Click(object sender, EventArgs e)
        {
            pActiveView.GraphicsContainer.DeleteAllElements();
            pActiveView.Refresh();
            Dispose();
        }

        private void FormExportMap_FormClosed(object sender, FormClosedEventArgs e)
        {
            pActiveView.GraphicsContainer.DeleteAllElements();
            pActiveView.Refresh();
            Dispose();
        }

    }
}
