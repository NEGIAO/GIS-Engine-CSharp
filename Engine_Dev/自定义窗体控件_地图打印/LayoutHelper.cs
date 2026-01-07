using System;
using System.IO;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Output;

namespace 自定义窗体控件_地图打印
{
    class LayoutHelper
    {
        // 导出格式枚举
        public enum ExportFormat { JPG, PNG, PDF }

        /// <summary>
        /// 设置页面大小和方向
        /// </summary>
        public static void SetupPage(AxPageLayoutControl pageLayoutControl, string size, bool isLandscape)
        {
            IPageLayout pageLayout = pageLayoutControl.PageLayout;
            IPage page = pageLayout.Page;

            // 1. 设置纸张大小
            if (size == "A3")
                page.FormID = esriPageFormID.esriPageFormA3;
            else
                page.FormID = esriPageFormID.esriPageFormA4;

            // 2. 设置方向 (1: Portrait 纵向, 2: Landscape 横向)
            page.Orientation = isLandscape ? (short)2 : (short)1;

            // 3. 关键：调整页面后，必须让数据框(MapFrame)自适应页面大小
            pageLayoutControl.ActiveView.Refresh();

            // 获取 GraphicsContainer 里的 MapFrame
            IGraphicsContainer container = pageLayout as IGraphicsContainer;
            container.Reset();
            IElement element = container.Next();
            while (element != null)
            {
                if (element is IMapFrame)
                {
                    // 将数据框填充到页面边缘（留出一点边距）
                    Double margin = 2.0; // 假设单位是厘米
                    IEnvelope env = new EnvelopeClass();
                    Double width, height;
                    page.QuerySize(out width, out height); // 获取当前纸张宽高

                    env.PutCoords(margin, margin, width - margin, height - margin);
                    element.Geometry = env;
                    break; // 只有一个主数据框
                }
                element = container.Next();
            }

            pageLayoutControl.ZoomToWholePage();
        }

        /// <summary>
        /// 自动添加制图要素（标题、指北针、比例尺）
        /// </summary>
        public static void AddSurroundElements(AxPageLayoutControl pageLayoutControl, string mapTitle)
        {
            IPageLayout pageLayout = pageLayoutControl.PageLayout;
            IGraphicsContainer container = pageLayout as IGraphicsContainer;
            IActiveView activeView = (IActiveView)pageLayout;

            // 1. 获取 MapFrame（所有的要素都挂载在 MapFrame 上）
            // 使用 activeView 变量，它是 IActiveView 接口，它才有 FocusMap 属性
            IMapFrame mapFrame = (IMapFrame)container.FindFrame(activeView.FocusMap);
            if (mapFrame == null) return; // 防崩

            // --- A. 添加标题 (TextElement) ---
            ITextElement textElement = new TextElementClass();
            textElement.Text = mapTitle;

            // 设置字体
            ITextSymbol textSymbol = new TextSymbolClass();
            textSymbol.Size = 60;
            textSymbol.Font = (stdole.IFontDisp)new stdole.StdFontClass() { Name = "宋体", Bold = true };
            textElement.Symbol = textSymbol;

            IElement titleEle = (IElement)textElement;

            // 计算位置：放在页面顶部中间
            Double pW, pH;
            pageLayout.Page.QuerySize(out pW, out pH);
            IPoint point = new PointClass();
            point.PutCoords(pW / 2, pH - 2.0); // 顶部
            titleEle.Geometry = point;
            container.AddElement(titleEle, 0);

            // --- B. 添加指北针 (NorthArrow) ---
            IMapSurroundFrame northArrowFrame = mapFrame.CreateSurroundFrame(new UIDClass() { Value = "esriCarto.MarkerNorthArrow" }, null);
            IElement northArrowEle = (IElement)northArrowFrame;

            // 设置位置：右上角
            IEnvelope arrowEnv = new EnvelopeClass();
            arrowEnv.PutCoords(pW - 3, pH - 3, pW - 1, pH - 1); // 2x2的大小
            northArrowEle.Geometry = arrowEnv;
            container.AddElement(northArrowEle, 0);

            // --- C. 添加比例尺 (ScaleBar) ---
            IMapSurroundFrame scaleBarFrame = mapFrame.CreateSurroundFrame(new UIDClass() { Value = "esriCarto.ScaleLine" }, null);
            IElement scaleBarEle = (IElement)scaleBarFrame;

            // 设置位置：左下角
            IEnvelope scaleEnv = new EnvelopeClass();
            scaleEnv.PutCoords(2, 1, 8, 1.5);
            scaleBarEle.Geometry = scaleEnv;
            container.AddElement(scaleBarEle, 0);

            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        /// <summary>
        /// 导出功能实现
        /// </summary>
        public static void ExportLayout(IActiveView activeView, string savePath, ExportFormat format, int dpi = 450)
        {
            IExport export = null;
            switch (format)
            {
                case ExportFormat.JPG: export = new ExportJPEGClass(); break;
                case ExportFormat.PNG: export = new ExportPNGClass(); break;
                case ExportFormat.PDF: export = new ExportPDFClass(); break;
            }

            if (export == null) return;

            try
            {
                export.ExportFileName = savePath;
                export.Resolution = dpi;

                tagRECT exportRect = activeView.ExportFrame;
                IEnvelope env = new EnvelopeClass();
                env.PutCoords(exportRect.left, exportRect.bottom, exportRect.right, exportRect.top);
                export.PixelBounds = env;

                int hDC = export.StartExporting();
                activeView.Output(hDC, (int)export.Resolution, ref exportRect, null, null);
                export.FinishExporting();
                export.Cleanup();
            }
            catch (Exception ex)
            {
                throw new Exception("文件导出出错: " + ex.Message);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(export);
            }
        }
    }
}
