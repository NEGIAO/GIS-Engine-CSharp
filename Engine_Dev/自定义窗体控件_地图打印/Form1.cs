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
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;

namespace 自定义窗体控件_地图打印
{
    public partial class FormPrintPreview : Form
    {
        // 用于接收主窗体的 MapControl
        private MapControl m_sourceMapControl;

        // 构造函数传入主地图控件,初始化使用
        public FormPrintPreview(MapControl sourceMapControl)
        {
            InitializeComponent();
            this.m_sourceMapControl = sourceMapControl;
        }

        //主逻辑执行，先添加地图三要素，再实现主窗体地图数据的传送
        private void FormPrintPreview_Load(object sender, EventArgs e)
        {
            // 1. 安全检查
            if (m_sourceMapControl == null)
            {
                MessageBox.Show("错误：未获取到主地图控件！");
                return;
            }

            try
            {
                // 初始化界面文本
                rbA4.Checked = true;
                rbLandscape.Checked = true;
                txtTitle.Text = "My GIS Map Project";
                // 应用页面设置
                ApplyPageSettings();
                //数据拷贝
                CopyToPageLayout(); 
                // 刷新界面,确保所有元素都绘制
                axPageLayoutControl1.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("预览加载失败：" + ex.Message);
            }
        }

        //将mapcontrol复制到pagelayoutcontrol中
        private void CopyToPageLayout()
        {
            IObjectCopy objectCopy = new ObjectCopy();//对象拷贝接口
            object copyFromMap = m_sourceMapControl.Map;//地图对象
            object copyMap = objectCopy.Copy(copyFromMap);//将axMapControl1的地图对象拷贝
            object copyToMap = axPageLayoutControl1.ActiveView.FocusMap;//axPageLayoutControl1活动视图中的地图
            objectCopy.Overwrite(copyMap, ref copyToMap);//将axMapControl1地图对象覆盖axPageLayout1当前地图

            // 获取 PageLayout 中 MapFrame 的视图
            IActiveView pActiveView = (IActiveView)axPageLayoutControl1.ActiveView.FocusMap;

            // 同步坐标系
            if (m_sourceMapControl.Map.SpatialReference != null)
            {
                pActiveView.FocusMap.SpatialReference = m_sourceMapControl.Map.SpatialReference;
            }
            // 设置范围：将主地图的当前范围赋值给 PageLayout 的 MapFrame
            IDisplayTransformation pDTF = pActiveView.ScreenDisplay.DisplayTransformation;
            pDTF.VisibleBounds = m_sourceMapControl.Extent;

            // 刷新 MapFrame 内部显示的内容
            pActiveView.Refresh();
        }

        // A4/A3 或 横向/纵向 改变时触发,重新设置页面
        private void PageSetting_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                ApplyPageSettings();
                CopyToPageLayout();
            }
        }

        // 统一应用设置的方法
        private void ApplyPageSettings()
        {
            try
            {
                string size = rbA3.Checked ? "A3" : "A4";
                bool isLandscape = rbLandscape.Checked;

                // 1. 设置纸张
                LayoutHelper.SetupPage(axPageLayoutControl1, size, isLandscape);

                // 2. 重新生成制图要素（先清除旧的，再加新的，防止重叠）
                axPageLayoutControl1.GraphicsContainer.DeleteAllElements(); 
                LayoutHelper.AddSurroundElements(axPageLayoutControl1, txtTitle.Text);
            
                axPageLayoutControl1.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("应用页面设置失败: " + ex.Message);
            }
        }

        // 标题文本框修改后，刷新标题显示
        private void txtTitle_TextChanged(object sender, EventArgs e)
            {
                // 简单处理：重新应用一遍设置即可刷新标题
                ApplyPageSettings();
            }

        // 点击“打印”按钮
        private void btnPrint_Click(object sender, EventArgs e)
        {
            // 调用 PageLayoutControl 自带的打印功能
            // 参数：打印机名(null为默认), 起始页, 终止页, 份数, 重叠部分
            // 参数顺序：起始页(1), 终止页(1), 重叠部分(0)
            axPageLayoutControl1.PrintPageLayout(1, 1, 0);
        }

        // 点击“导出”按钮
        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "导出地图";
            // 自动填充文件名
            dlg.FileName = "地图导出_" + DateTime.Now.ToString("yyyyMMdd_HHmm");
            dlg.Filter = "PNG 图片|*.png|JPG 图片|*.jpg|PDF 文档|*.pdf";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(dlg.FileName).ToLower();
                LayoutHelper.ExportFormat fmt = LayoutHelper.ExportFormat.PNG;

                if (ext == ".jpg") fmt = LayoutHelper.ExportFormat.JPG;
                else if (ext == ".pdf") fmt = LayoutHelper.ExportFormat.PDF;

                try
                {
                    // 调用 Helper 导出
                    LayoutHelper.ExportLayout(axPageLayoutControl1.ActiveView, dlg.FileName, fmt);
                    MessageBox.Show("导出成功！\n路径：" + dlg.FileName, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

    }
}
