using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesOleDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem; 
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.SystemUI;
using System.Windows.Forms;

namespace MyAEApp2025
{
    /// <summary>
    /// 当类比较大时，定义接口，通过类实现接口，以便分类管理实现不同功能的属性和方法
    /// 该类GeoMapAO中定义了三个接口，分别负责控件的设置、地理数据的加载、地图数据的操作
    /// </summary>
    /// 

    //定义设置控件的接口
    interface IComControl
    {
        //主视图控件
        AxMapControl AxMapControl1 { get; set; }
        //鹰眼视图控件
        AxMapControl AxMapControl2 { get; set; }
        //版面视图控件
        AxPageLayoutControl AxPageLayoutControl1 { get; set; }
        //定义设置颜色的方法
        IRgbColor GetRGB(int r, int g, int b);
    }
    
    //定义管理地理数据加载的接口IAddGeoData，继承自IComControl
    interface IAddGeoData:IComControl
    {
        //存放用户选择的地理数据文件
        string[] StrFileName { get; set; }

        //加载地理数据的方法
        void AddGeoMap();

        //加载TIN数据集的方法
        void AddTINDataset();
    }

    //定义管理地图操作的接口IGeoDataOper,继承自IComControl
    interface IGeoDataOper:IComControl
    {
        //地图操作的类型
        string StrOperType { get; set; }

        //鼠标按下事件的参数
        IMapControlEvents2_OnMouseDownEvent E { get; set; }

        //实现地图交互操作的方法
        void OperMap();

        //实现地图文档操作的方法
        void OperateMapDoc();
    }

    //主控件与鹰眼控件的同步
    interface IEagOpt:IComControl
    {
        //定义属性字段信息
        //处理鼠标移动的事件参数
        IMapControlEvents2_OnMouseMoveEvent Em { get; set; }
        IMapControlEvents2_OnMouseDownEvent Ed { get; set; }
        IMapControlEvents2_OnExtentUpdatedEvent Eu { get; set; }
        void NewGeoMap();//主控件和鹰眼控件同步的方法
        void MoveEagl();//移动鹰眼视图中红色矩形框
        void MouseMov();//处理鹰眼视图中鼠标的移动
        void DrawrRec();//绘制红色矩形框的过程
    }

    //主控件与PageLayout控件的同步
    interface IMapCooper:IComControl
    {
        string StrMxdFile { get; set; }//用户选择的文档文件名
        void CopyAndWriteMap();//拷贝并复制地图
        void repGeoMap();//替换新的地图
        void AddGeoDoc();//向pagelayout控件加载地图文档
    }


    //由GeoMapAO类实现以上三个接口
    class GeoMapAO : IAddGeoData, IGeoDataOper, IEagOpt, IMapCooper
    {
        //实现地图控件的接口
        AxMapControl axMapControl1;
        public AxMapControl AxMapControl1
        {
            get
            {
                return axMapControl1;
            }
            set
            {
                axMapControl1 = value;
            }
        }
        AxMapControl axMapControl2;
        public AxMapControl AxMapControl2
        {
            get
            {
                return axMapControl2;
            }
            set
            {
                axMapControl2 = value;
            }
        }
        AxPageLayoutControl axPageLayoutControl1;
        public AxPageLayoutControl AxPageLayoutControl1
        {
            get
            {
                return axPageLayoutControl1;
            }
            set
            {
                axPageLayoutControl1 = value;
            }
        }

        //定义实现RGB颜色的方法
        public IRgbColor GetRGB(int r, int g, int b)
        {
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = r;
            pColor.Green = g;
            pColor.Blue = b;
            return pColor;
        }

        //实现地图加载的接口
        string[] strFileName;//保存用户选择地理数据文件名
        public string[] StrFileName
        {
            get
            {
                return strFileName;
            }
            set
            {
                strFileName = value;
            }
        }

        //定义加载地图的方法
        public void AddGeoMap()
        {
            for (int i = 0; i < strFileName.Length; i++)
            {
                string strExt = System.IO.Path.GetExtension(strFileName[i]);
                //判断文件类型然后采用不同的方法加载文件
                switch (strExt)
                {
                    case ".shp":
                        string strPath = System.IO.Path.GetDirectoryName(strFileName[i]);
                        string strFile = System.IO.Path.GetFileNameWithoutExtension (strFileName[i]);
                        //向控件加载地图
                        AxMapControl1.AddShapeFile(strPath, strFile);
                        AxMapControl2.AddShapeFile(strPath, strFile);
                        //进行全图显示
                        AxMapControl2.Extent = axMapControl2.FullExtent;
                        break;
                    case ".bmp":
                    case ".tif":
                    case ".jpg":
                    case ".img":
                        IRasterLayer pRasterLayer = new RasterLayerClass();
                        string pathName = System.IO.Path.GetDirectoryName(strFileName[i]);
                        string fileName = System.IO.Path.GetFileName (strFileName[i]);
                        IWorkspaceFactory pWSF = new RasterWorkspaceFactoryClass();
                        IWorkspace pWS = pWSF.OpenFromFile(pathName, 0);
                        IRasterWorkspace pRWS = pWS as IRasterWorkspace;
                        IRasterDataset pRasterDataset = pRWS.OpenRasterDataset(fileName);

                        //影像金字塔判断与创建
                        IRasterPyramid pRasPyrmid = pRasterDataset as IRasterPyramid;
                        if (pRasPyrmid != null)
                        {
                            if (!(pRasPyrmid.Present))
                            {
                                pRasPyrmid.Create();
                            }
                        }
                        IRaster pRaster = pRasterDataset.CreateDefaultRaster();
                        pRasterLayer.CreateFromRaster(pRaster);
                        ILayer pLayer = pRasterLayer as ILayer;
                        //向主控件中添加图像
                        AxMapControl1.AddLayer(pLayer, 0);
                        AxMapControl2.ClearLayers();
                        AxMapControl2.AddLayer(pLayer, 0);
                        AxMapControl2.Extent = axMapControl2.FullExtent;
                        break;
                    case ".dwg": //加载CAD格式文件
                        string strFilePath = System.IO.Path.GetDirectoryName(strFileName[i]);
                        string strSeleFileName = System.IO.Path.GetFileName(strFileName[i]);
                        AddCADFeatures(strFilePath, strSeleFileName);
                        break;
                    default:
                        break;
                }
            }
        }

        //加载.dwg格式CAD数据的方法
        private void AddCADFeatures(string strPath, string strCAD)
        {
            IWorkspaceFactory pCADWorksacefactory = new CadWorkspaceFactoryClass();
            IFeatureWorkspace pFeatureWorkspace;
            pFeatureWorkspace = pCADWorksacefactory.OpenFromFile(strPath, 0) as IFeatureWorkspace;
            //打开一个要素数据集
            IFeatureDataset pFeatDataset = pFeatureWorkspace.OpenFeatureDataset(strCAD);
            IFeatureClassContainer pFeatClassContainer = pFeatDataset as IFeatureClassContainer;
            IFeatureClass pFeatureClass;
            IFeatureLayer pFeatureLayer;
            //对CAD的要素集进行遍历操作
            for (int i = 0; i < pFeatClassContainer.ClassCount-1; i++)
            {
                pFeatureClass = pFeatClassContainer.get_Class(i);
                if (pFeatureClass.FeatureType == esriFeatureType.esriFTCoverageAnnotation)
                {
                    //设置为标注图层
                    pFeatureLayer = new CadAnnotationLayerClass();
                }
                else
                {
                    pFeatureLayer = new FeatureLayerClass();                
                }
                pFeatureLayer.Name = pFeatureClass.AliasName;
                pFeatureLayer.FeatureClass =pFeatureClass;
                AxMapControl1.AddLayer(pFeatureLayer, 0);
                //鹰眼视图
                AxMapControl2.ClearLayers();
                AxMapControl2.AddLayer(pFeatureLayer, 0);
                AxMapControl2.Extent = axMapControl2.FullExtent;

            }
        }


        //实现IAddGeoData：AddTINDataset
        public void AddTINDataset()
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog ()==DialogResult.OK)
            {
                string strFullPath = openFolder.SelectedPath;
                string strPath = System.IO.Path.GetDirectoryName(strFullPath);
                string strTin = System.IO.Path.GetFileName (strFullPath);

                IWorkspaceFactory pTinWorkspaceFactory = new TinWorkspaceFactoryClass();
                IWorkspace pWS = pTinWorkspaceFactory.OpenFromFile(strPath, 0);
                ITinWorkspace pTinWorkspace = pWS as ITinWorkspace;
                ITin pTin = pTinWorkspace.OpenTin(strTin);
                ITinLayer pTinLayer = new TinLayerClass();
                pTinLayer.Dataset = pTin;
                AxMapControl1.AddLayer(pTinLayer as ILayer, 0);
            }
        }


        //实现IGeoDataOper：OperateMapDoc
        public void OperateMapDoc()
        {
            OpenFileDialog OpenFileDlg = new OpenFileDialog();
            SaveFileDialog SaveFileDlg = new SaveFileDialog();
            OpenFileDlg.Filter = "地图文档文件(*.mxd)|*.mxd";
            SaveFileDlg.Filter = "地图文档文件(*.mxd)|*.mxd";
            string strDocFileN = string.Empty;
            IMapDocument pMapDocument = new MapDocumentClass();

            //判断操作文档地图的类型
            switch (StrOperType)
            {
                case "NewDoc":
                    {
                        SaveFileDlg.Title = "输入新建地图文档的名称";
                        SaveFileDlg.ShowDialog();
                        strDocFileN = SaveFileDlg.FileName;
                        if (strDocFileN==string.Empty)
                        {
                            return;
                        }
                        pMapDocument.New(strDocFileN);
                        pMapDocument.Open(strDocFileN, "");
                        AxMapControl1.Map = pMapDocument.get_Map(0);
                        break;
                    }

                case "OpenDoc":
                    {
                        OpenFileDlg.Title = "选择需要加载的地图文档文件";
                        OpenFileDlg.ShowDialog();
                        strDocFileN = OpenFileDlg.FileName;
                        if (strDocFileN == string.Empty)
                        {
                            return;
                        }
                        //将数据加载进pMapDocument，并与map控件关联
                        pMapDocument.Open(strDocFileN, "");
                        for (int i = 0; i < pMapDocument.MapCount; i++)
                        {//遍历可能的map对象
                            AxMapControl1.Map = pMapDocument.get_Map(i);
                        }
                        AxMapControl1.Refresh();
                        break;
                    }

                case "SaveDoc":
                    {
                        //判断文档是否为只读文档
                        if (pMapDocument.get_IsReadOnly(pMapDocument.DocumentFilename )==true )
                        {
                            MessageBox.Show("此地图文档为只读文档！","信息提示");
                            return;
                        }
                        //利用相对路径保存地图文档
                        pMapDocument.Save(pMapDocument.UsesRelativePaths, true);
                        MessageBox.Show("保存成功", "信息提示");
                        break;
                    }

                case "SaveDocAs":
                    {
                        SaveFileDlg.Title = "地图文档另存";
                        SaveFileDlg.ShowDialog();
                        strDocFileN = SaveFileDlg.FileName;
                        if (strDocFileN ==string.Empty )
                        {
                            return;
                        }
                        if (strDocFileN ==pMapDocument.DocumentFilename)
                        {
                            //将修改后的文档保存在原路径中，用相对路径保存地图文档
                            pMapDocument.Save(pMapDocument.UsesRelativePaths, true);
                            MessageBox.Show("保存成功", "信息提示");
                            break;
                        }
                        else
                        {
                            //将修改后的地图文档保存为新文件
                            pMapDocument.SaveAs(strDocFileN, true,true );
                            MessageBox.Show("保存成功", "信息提示");
                        }
                        break;

                    }
                default:
                    break;
            }

        }

       //实现鼠标与地图交互的接口
        string strOperType;//标识地图操作类型
        public string StrOperType
        {
            get
            {
                return strOperType;
            }
            set
            {
                strOperType = value;
            }
        }
        
        //定义鼠标按下事件的变量
        IMapControlEvents2_OnMouseDownEvent e;
        public IMapControlEvents2_OnMouseDownEvent E
        {
            get
            {
                return e;
            }
            set
            {
                e = value;
            }
        }
        
        
        //定义鼠标与地图交互的方法
        public void OperMap()
        {
            IMap pMap=axMapControl1.Map ;
            IActiveView pActiveView=pMap as IActiveView;
            //判断地图操作的种类
            IEnvelope pEnv;

            switch (strOperType)
            {
                case "LKZoomIn"://拉框放大
                    {
                        pEnv = axMapControl1.TrackRectangle();
                        pActiveView.Extent = pEnv;
                        pActiveView.Refresh();
                        break;
                    }

                case "GeoMapLkShow"://拉框显示
                    {

                        axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                        axMapControl1.Extent = axMapControl1.TrackRectangle();
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                        break;
                    }
                case "MoveMap"://移动地图
                    {

                        axMapControl1.Pan();
                        break;
                    }
                case "DrawPoint"://绘制点
                    {
                        //新建点对象
                        IPoint pt = new PointClass();
                        pt.PutCoords(e.mapX, e.mapY);
                        //产生一个marker元素
                        IMarkerElement pMarkerElement = new MarkerElementClass();
                        //产生修饰Marker元素的Symbol
                        ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
                        pSimpleMarkerSymbol.Color = GetRGB(220, 120, 60);
                        pSimpleMarkerSymbol.Size = 2;//符号大小
                        pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;
                        IElement pElement = pMarkerElement as IElement;
                        //得到Element的接口对象，用于设置元素的Geometry
                        pElement.Geometry = pt;
                        pMarkerElement.Symbol = pSimpleMarkerSymbol;
                        IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                        //将元素添加到Map中
                        pGraphicsContainer.AddElement(pMarkerElement as IElement, 0);
                        pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                        break;

                    }

                case "DrawLine"://绘制线
                    {
                        IPolyline pPolyline = axMapControl1.TrackLine() as IPolyline;
                        //产生一个SimpleLineSymbol符号
                        ISimpleLineSymbol pSimpleLineSymbol = new SimpleLineSymbolClass();
                        //需要用户动态选择
                        pSimpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                        pSimpleLineSymbol.Color = GetRGB(120, 200, 180);//设置符号颜色
                        pSimpleLineSymbol.Width = 1;
                        //产生一个PolylineElement对象
                        ILineElement pLineElement = new LineElementClass();
                        IElement pElement = pLineElement as IElement;
                        pElement.Geometry = pPolyline;

                        //将元素添加到Map中
                        IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                        pGraphicsContainer.AddElement(pElement as IElement, 0);
                        pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                        break;                   
                    
                    }

                case "DrawPolygon"://绘制面
                    {
                        IPolygon pPolygon = axMapControl1.TrackPolygon() as IPolygon;
                        //产生一个SimpleFillSymbol
                        ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
                        pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSDiagonalCross;
                        pSimpleFillSymbol.Color = GetRGB(220, 112, 60);
                        //产生一个PolygonElement对象
                        IFillShapeElement pPolygonElement = new PolygonElementClass();
                        pPolygonElement.Symbol = pSimpleFillSymbol;
                        IElement pElement = pPolygonElement as IElement;
                        pElement.Geometry = pPolygon;
                       
                        //将元素添加到Map中
                        IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                        pGraphicsContainer.AddElement(pElement as IElement, 0);
                        pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                        break;   


                    }
                case "LabelMap"://地图标注
                    {
                        IElement pElement;
                        //建立文字符号对象，并设置相应的属性
                        ITextElement pTextElement = new TextElementClass();
                        pTextElement.Text = "河南大学地理与环境学院GIS";//需要动态设定
                        pElement = pTextElement as IElement;
                        //设置文字字符的几何形体属性
                        IPoint pPoint = new PointClass();
                        pPoint.PutCoords(e.mapX, e.mapY);
                        pElement.Geometry = pPoint;

                        //将元素添加到Map中，并刷新显示
                        IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
                        pGraphicsContainer.AddElement(pElement as IElement, 0);
                        pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                        //定义颜色的方法
                        break;
                    }
                case "SelectMap"://数据选择
                    {
                        //要素选择
                        pEnv = axMapControl1.TrackRectangle();//得到一个envelop对象
                        //新建选择集环境对象
                        ISelectionEnvironment pSelectionEnvironment = new SelectionEnvironmentClass();
                        //改变选择集默认颜色
                        pSelectionEnvironment.DefaultColor = GetRGB(110, 120, 210);
                        //选择要素，并将其放进选择集
                        axMapControl1.Map.SelectByShape(pEnv, pSelectionEnvironment, false);
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        //需要遍历所选元素
                        break;
                    }
                default:
                    break;
            }

        }

        IMapControlEvents2_OnMouseMoveEvent em;
        public IMapControlEvents2_OnMouseMoveEvent Em
        {
            get
            {
                return em;
            }
            set
            {
                em = value;
            }
        }

        IMapControlEvents2_OnMouseDownEvent ed;
        public IMapControlEvents2_OnMouseDownEvent Ed
        {
            get
            {
                return ed;
            }
            set
            {
                ed = value;
            }
        }

        IMapControlEvents2_OnExtentUpdatedEvent eu;
        public IMapControlEvents2_OnExtentUpdatedEvent Eu
        {
            get
            {
                return eu;
            }
            set
            {
                eu = value;
            }
        }

        //主控件与鹰眼控件地图同步
        public void NewGeoMap()
        {
            IMap pMap = axMapControl1.Map;
            for (int i = 0; i < pMap.LayerCount ; i++)
            {
                axMapControl2.Map.AddLayer(pMap.get_Layer(i));
            }
            //鹰眼控件中显示加载地图的全图
            axMapControl2.Extent = axMapControl2.FullExtent;
        }

        public void MoveEagl()
        {
            if (ed.button==1)
            {
                IPoint pPt = new PointClass();
                pPt.X = ed.mapX;
                pPt.Y = ed.mapY;
                IEnvelope pEnvelop = axMapControl1.Extent as IEnvelope;
                pEnvelop.CenterAt(pPt);
                axMapControl1.Extent = pEnvelop;
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

            }
            else if (ed.button==2)
            {
                IEnvelope pEnvelop = axMapControl2.TrackRectangle();
                axMapControl1.Extent = pEnvelop;
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }

        public void MouseMov()
        {
            if (em.button !=1)
            {
                return;
            }

            IPoint pPt = new PointClass();
            pPt.X = em.mapX;
            pPt.Y = em.mapY;
            axMapControl1.CenterAt(pPt);
            axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        public void DrawrRec()
        {
            //绘制鹰眼控件中红色矩形框
            IGraphicsContainer pGraphicsContainer = axMapControl2.Map as IGraphicsContainer;
            IActiveView pAv = pGraphicsContainer as IActiveView;
            //在绘制前清除鹰眼控件中的任何图形元素
            pGraphicsContainer.DeleteAllElements();
            IRectangleElement pRectangleElement = new RectangleElementClass();
            IElement pEle = pRectangleElement as IElement;
            IEnvelope pEnv = eu.newEnvelope as IEnvelope;
            pEle.Geometry = pEnv;
            //设置颜色
            IRgbColor pColor = new RgbColorClass();
            pColor = GetRGB(200,0,0);
            pColor.Transparency = 255;
            //产生一个线符号对象,作为矩形框的外框线
            ILineSymbol pLineSymbol = new SimpleLineSymbolClass();
            pLineSymbol.Width = 2;
            pLineSymbol.Color = pColor;
            //设置填充符号的属性
            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            //设置透明颜色
            pColor.Transparency = 0;
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pLineSymbol;
            IFillShapeElement pFillShapeElement=pRectangleElement as IFillShapeElement;
            pFillShapeElement.Symbol=pFillSymbol;
            pGraphicsContainer.AddElement(pEle, 0);
            axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        }

       

        public void CopyAndWriteMap()
        {
            IObjectCopy objectCopy = new ObjectCopyClass();
            object toCopyMap = axMapControl1.Map;
            object copiedMap = objectCopy.Copy(toCopyMap);//复制地图到copiedMap中
            //获取制图控件的焦点地图
            object toOverwriteMap = axPageLayoutControl1.ActiveView.FocusMap;
            objectCopy.Overwrite(copiedMap, ref toOverwriteMap);//复制地图
        }

        //替换新的地图
        public void repGeoMap()
        {
            IActiveView pActiveView = axPageLayoutControl1.ActiveView.FocusMap as IActiveView;
            IDisplayTransformation displayTransformation = pActiveView.ScreenDisplay.DisplayTransformation;
            //设置焦点地图的可是范围
            displayTransformation.VisibleBounds = axMapControl1.Extent;
            axPageLayoutControl1.ActiveView.Refresh();
            CopyAndWriteMap();
        }

        // 加载地图文档
        string strMxdFile;
        public string StrMxdFile
        {
            get
            {
                return strMxdFile;
            }
            set
            {
                strMxdFile = value;
            }
        }

        //向PageLayout控件中加载地图文档
        public void AddGeoDoc()
        {
            //定义IMAPDocument接口变量，并实例化
            IMapDocument pMapDocument = new MapDocumentClass();
            pMapDocument.Open(strMxdFile, "");
            //将MapDocument的数据传给控件
            axPageLayoutControl1.PageLayout = pMapDocument.PageLayout;
            axPageLayoutControl1.Refresh();
        }
    }
}
