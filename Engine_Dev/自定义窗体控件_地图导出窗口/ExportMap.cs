using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Output;

namespace 自定义窗体控件_地图导出窗口
{
    public class ExportMap
    {
        public static void ExportView(IActiveView view, IGeometry pGeo,int outPutResolution,int Width,int Height,string ExpPath,bool bRegion)
        {
            IExport pExport = null;//导出对象
            tagRECT exportRect = new tagRECT();//导出的屏幕区域 设备像素为单位
            // 保持 IGeometry 参数，但在内部进行类型转换
            IEnvelope pEnvelop = pGeo as IEnvelope;
            if (pEnvelop == null)
            {
                // 如果传入的不是 IEnvelope，使用它的最小边界框作为默认范围
                pEnvelop = pGeo.Envelope;
            }
            // pEnvelop 现在是导出的地图范围
            //IEnvelope pEnvelop = pGeo.Envelope;//地图的可见范围
            string sType = System.IO.Path.GetExtension(ExpPath);
            //获取扩展名，初始化导出对象pExport；
            switch (sType)
            {
                case ".jpg":
                    pExport = new ExportJPEGClass();
                    break;
                case ".bmp":
                    pExport = new ExportBMPClass();
                    break;
                case ".gif":
                    pExport = new ExportGIFClass();
                    break;
                default:
                    MessageBox.Show("没有设置导出格式，默认为JPEG格式！");
                    pExport = new ExportJPEGClass();
                    break;
            }
            pExport.ExportFileName = ExpPath;
            //确定屏幕范围
            exportRect.left = 0;
            exportRect.top = 0;
            exportRect.right = Width;
            exportRect.bottom = Height;
            if (bRegion)
            {
                view.GraphicsContainer.DeleteAllElements();
                view.Refresh();
            }
            IEnvelope envelop = new EnvelopeClass();
            envelop.PutCoords((double)exportRect.left, (double)exportRect.top, (double)exportRect.right, (double)exportRect.bottom);
            pExport.PixelBounds = envelop;
            view.Output(pExport.StartExporting(),outPutResolution,ref exportRect,pEnvelop,null);
            pExport.FinishExporting();
            pExport.Cleanup();
        }
        /// <summary>
        /// 获取rgb颜色
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static IRgbColor GetRgbColor(int r,int g,int b)
        {
            IRgbColor pRgbColor = new RgbColorClass();
            pRgbColor.Red = r;
            pRgbColor.Green = g;
            pRgbColor.Blue = b;
            return pRgbColor;
        }

        /// <summary>
        /// 绘制几何图形
        /// </summary>
        /// <param name="pGeometry"></param>
        /// <param name="activeView"></param>
        public static void AddElement(IGeometry pGeometry, IActiveView activeView)
        {
            IRgbColor fillcolor = GetRgbColor(204,175,235);
            IRgbColor linecolor = GetRgbColor(255, 0, 0);
            IElement pEle = CreateElement(pGeometry, linecolor, fillcolor);
            IGraphicsContainer pGC = activeView.GraphicsContainer;
            if (pGC != null)
            {
                pGC.AddElement(pEle, 0);
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, pEle, null);
            }
        }

        private static IElement CreateElement(IGeometry pGeometry, IRgbColor linecolor, IRgbColor fillcolor)
        {
            IElement pElement = null;
            if (pGeometry is IEnvelope)
            {
                pElement = new RectangleElementClass();
            }
            else if (pGeometry is IPolygon)
            {
                pElement = new PolygonElementClass();
            }
            else if (pGeometry is ICircularArc)
            {
                ISegment pSegment = pGeometry as ISegment;
                ISegmentCollection pSegCol = new PolygonClass();
                object o = Type.Missing;
                pSegCol.AddSegment(pSegment,ref o,ref o);
                IPolygon pPolygon = pSegCol as IPolygon;
                pGeometry = pPolygon as IGeometry;
                pElement = new CircleElementClass();
            }
            else if (pGeometry is IPolyline)
            {
                pElement = new LineElementClass();
            }
            pElement.Geometry = pGeometry;
            IFillShapeElement pFillEle = pElement as IFillShapeElement;
            ISimpleFillSymbol pSys = new SimpleFillSymbolClass();
            pSys.Color = fillcolor;
            pSys.Outline.Color = linecolor;
            pSys.Style = esriSimpleFillStyle.esriSFSCross;
            pFillEle.Symbol = pSys;
            return pElement;
        }
    }
}
