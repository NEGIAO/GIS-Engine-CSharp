using System;//系统引用文件
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
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using MyAEApp2025;//Frm中的扩展类文件GeoMapAO
using 自定义DLL_SymbologyMenu;//引入自定义的dll文件
using 自定义窗体控件_Add_txt;//自定义窗体控件
using 自定义窗体控件_FormAttribute;
using 自定义窗体控件_地图导出窗口;
using 自定义窗体控件_地图量测窗口;
using 自定义窗体控件_地图选择集;
using 自定义窗体控件_统计选择集;
using 自定义窗体控件_符号选择器;

namespace GIS_2310130172_Engine
{
    public partial class FrmMain : Form
    {
        #region 全局标识变量
        // ==========================================
        // 1. 核心控制开关
        // ==========================================
        // 全局标志位：控制当前鼠标的功能
        // "" 或 null 代表默认/浏览模式，非空代表具体功能(如 "MeasureLength", "SelFeature")
        string pMouseOperate = "";

        // ==========================================
        // 2. 测量与反馈工具
        // ==========================================
        // 专门用于“测距”的反馈对象 
        private INewLineFeedback pNewLineFeedback = null;
        // 专门用于“测面”的反馈对象 
        private INewPolygonFeedback pNewPolygonFeedback = null;

        // 测量辅助变量
        private FormMeasureResult frmMeasureResult = null;  // 量算结果窗体
        private IPoint pMovePt = null;                      // 鼠标移动时的当前点
        private double dToltalLength = 0;                   // 测量的总长度
        private double dSegmentLength = 0;                  // 片段距离
        private IPointCollection pAreaPointCol = new MultipointClass(); // 面积量算点集
        private object missing = Type.Missing;

        // ==========================================
        // 3. 辅助功能变量 (鹰眼、TOC、导出)
        // ==========================================
        private FormExportMap frmExpMap = null; // 导出地图窗体
        private IPoint pPointPt = null;         // 鼠标点击点

        // 鹰眼同步
        private bool bCanDrag;          // 可移动标志
        private IPoint pMoveRectPoint;  // 记录在矩形框中的鼠标位置
        private IEnvelope pEnv;         // 记录数据视图的Extent

        // TOC图层顺序调整
        IFeatureLayer pTocFeatureLayer = null;
        private ILayer pMoveLayer;
        private int toIndex;
        private Point pMoveLayerPoint = new Point();

        private 自定义窗体控件_FormAttribute.FormAttribute frmattirbute = null;

        // 定制化 Toolbar
        private ICustomizeDialog cd = new CustomizeDialogClass();
        private ICustomizeDialogEvents_OnStartDialogEventHandler startDialogE;
        private ICustomizeDialogEvents_OnCloseDialogEventHandler closeDialogE;

        // ==========================================
        // 4. 编辑功能变量
        // ==========================================
        private IMap pMap = null;
        private IActiveView pActiveView = null;
        private List<ILayer> plstLayers = null;
        private IFeatureLayer pCurrentLyr = null;

        // 编辑器核心对象 (在构造函数中初始化)
        private IEngineEditor pEngineEditor = null;
        private IEngineEditTask pEngineEditTask = null;
        private IEngineEditLayers pEngineEditLayers = null;

        #endregion

        //构造方法
        public FrmMain()
        {
            InitializeComponent();
            //属性绑定buddy,不一定成功
            //最好用代码来绑定
            axTOCControl1.SetBuddyControl(axMapControl1.Object);
            axToolbarControl1.SetBuddyControl(axMapControl1.Object);

            InitObject();//初始化编辑器
        }

        //主窗体的加载事件：定制对话框、自定义工具的载入
        private void FrmMain_Load(object sender, EventArgs e)
        {
            #region 调试时，注释掉该代码，可以加载mxd文档，方便debug;正式运行时，保留该代码，使地图默认打开为空；

            // 1. 实例化一个新的 IMap 对象 (MapClass 实现了 IMap)
            //    这个新的 IMap 对象是空的，不包含任何图层或数据。
            IMap newMap = new MapClass();
            // 2. 将这个新的空 Map 对象赋给 AxMapControl
            //    这会覆盖任何默认或先前加载的地图文档。
            axMapControl1.Map = newMap;
            // 3. 确保显示刷新
            axMapControl1.ActiveView.Refresh();

            #endregion

            //定制对话框
            chkCustomize.Checked = false;
            chkCustomize.CheckOnClick = true;
            CreateCusDialog();

            //ToolBar栏中的自定义菜单，随时注释掉即可

            //自定义工具是添加新的类来实现的，这里是添加到mapcontrol中
            //1、清除当前工具命令
            axToolbarControl1.AddItem(new ClearCurrentToolCMD(), -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconAndText);
            //2、添加日期工具
            ICommand addDate = new AddDateTool(axToolbarControl1, axPageLayoutControl1);
            axToolbarControl1.AddItem(addDate, -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconAndText);
            //3、DLL文件扩展
            //自定义的dll菜单项，内部应有六个子菜单，用于扩展功能使用
            IMenuDef menuDef = new 自定义DLL_SymbologyMenu.SymbologyMenu();
            axToolbarControl1.AddItem(menuDef,-1,-1,false,-1,esriCommandStyles.esriCommandStyleIconAndText);
        }

        #region 数据载入菜单
        private void loadMxFile方法ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog pOpenFileDialog = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    Title = "打开地图文档",
                    Filter = "Arcmap文档（*.mxd)|*.mxd",
                    Multiselect = false,
                    RestoreDirectory = true
                };
                if(pOpenFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string pFileName = pOpenFileDialog.FileName;
                    if(pFileName == "")
                    {
                        return;
                    }
                    if(axMapControl1.CheckMxFile(pFileName))
                    {
                        ClearAllData();
                        axMapControl1.LoadMxFile(pFileName);
                    }
                    else
                    {
                        MessageBox.Show(pFileName + "地图无效");

                    }
                }
            }
            catch(Exception)
            {

            }
        }

        private void ClearAllData()
        {
            if (axMapControl1.Map != null && axMapControl1.Map.LayerCount >0 )
            {
                //新建map
                IMap dataMap = new MapClass();
                dataMap.Name = "Map";
                axMapControl1.DocumentFilename = string.Empty;
                axMapControl1.Map = dataMap;
            }
        }

        private void iMapDocument方法ToolStripMenuItem_Click(object sender, EventArgs e)
        {
                OpenFileDialog pOpenFileDialog = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    Title = "打开地图文档",
                    Filter = "Arcmap文档（*.mxd)|*.mxd",
                    Multiselect = false,
                    RestoreDirectory = true
                };
                if(pOpenFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string pFileName = pOpenFileDialog.FileName;
                    if(pFileName == "")
                    {
                        return;
                    }
                    if(axMapControl1.CheckMxFile(pFileName))
                    {
                        IMapDocument pMapDocument = new MapDocumentClass();
                        pMapDocument.Open(pFileName);
                        //获取Map中激活的地图
                        axMapControl1.Map = pMapDocument.ActiveView.FocusMap;
                        axMapControl1.ActiveView.Refresh();
                    }
                    else
                    {
                        MessageBox.Show(pFileName + "地图无效");

                    }
                }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICommand cmd = new ESRI.ArcGIS.Controls.ControlsOpenDocCommand();
            cmd.OnCreate(axMapControl1.GetOcx());
            cmd.OnClick();
        }

        private void 通过工作空间加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 配置文件对话框
                openFileDialog1.Title = "加载SHP数据";
                openFileDialog1.Filter = "Shapefile文件 (*.shp)|*.shp";

                // 显示对话框并检查用户是否点击“确定”
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // 获取文件全路径、文件夹路径和不带扩展名的文件名
                string filePath = openFileDialog1.FileName;
                string folderPath = System.IO.Path.GetDirectoryName(filePath);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

                // 使用 ShapefileWorkspaceFactory 打开工作空间
                IWorkspaceFactory pWorkSpaceFactory = new ShapefileWorkspaceFactoryClass();
                IWorkspace pWorkSpace = pWorkSpaceFactory.OpenFromFile(folderPath, 0);

                // 将工作空间转换为 IFeatureWorkspace
                IFeatureWorkspace pFeatureWorkspace = pWorkSpace as IFeatureWorkspace;

                // 使用不带扩展名的文件名打开要素类
                IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(fileName);

                // 创建要素图层，并将其添加到地图中
                IFeatureLayer pFeatureLayer = new FeatureLayerClass();
                pFeatureLayer.FeatureClass = pFeatureClass;
                pFeatureLayer.Name = pFeatureClass.AliasName;

                IMap pMap = axMapControl1.Map;
                pMap.AddLayer(pFeatureLayer);

                // 刷新地图以显示新图层
                axMapControl1.Refresh();
            }
            catch (Exception ex)
            {
                // 只给用户友好的提示，将详细错误信息写入日志或在调试时查看
                MessageBox.Show("图层加载失败了，请检查数据文件是否完整。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 如果需要，可以重新抛出异常以供上层代码处理
                // throw;
            }
        }

        private void 通过AddShapefile方法加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "加载SHP数据";
            openFileDialog1.Filter = "ShapeFile文件(*.shp)|*.shp";
            DialogResult pDialogResultlog1 = openFileDialog1.ShowDialog();
            if (pDialogResultlog1 != DialogResult.OK)
            {
                return;
            }
            //获取全路径
            string path = openFileDialog1.FileName;
            //获取文件路径
            string folder = System.IO.Path.GetDirectoryName(path);
            //获取文件名
            string filename = System.IO.Path.GetFileName(path);
            axMapControl1.AddShapeFile(folder, filename);
            //区别，一个是全局变量，可多次使用，一个单次引用
        }

        private void 加载栅格数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "加载栅格数据";
            openFileDialog1.Filter = "栅格文件(*.*)|*.img|(*.tif)|*.tif(*.tif)";
            DialogResult pDialogResultlog1 = openFileDialog1.ShowDialog();
            if (pDialogResultlog1 != DialogResult.OK)
            {
                return;
            }
            //获取全路径
            string path = openFileDialog1.FileName;
            //获取文件路径
            string folder = System.IO.Path.GetDirectoryName(path);
            //获取文件名
            string filename = System.IO.Path.GetFileName(path);

            // 实例化一个栅格工作空间工厂
            IWorkspaceFactory pWorkSpaceFactory = new RasterWorkspaceFactoryClass();
            // 使用工厂打开工作空间，并强制转换为IRasterWorkspace接口
            IRasterWorkspace pRasterWorkSpace = pWorkSpaceFactory.OpenFromFile(folder, 0) as IRasterWorkspace;
            // 打开栅格数据集
            IRasterDataset pRasterDataset = pRasterWorkSpace.OpenRasterDataset(filename);
            // 强制转换，如果成功，pRasterPyramid将不再是null
            IRasterPyramid pRasterPyramid = pRasterDataset as IRasterPyramid;
            // 检查是否成功转换为IRasterPyramid，并且金字塔不存在
            if (pRasterPyramid != null && !pRasterPyramid.Present)
            {
                // 如果金字塔不存在，则创建它
                pRasterPyramid.Create();
            }

            IRasterLayer pRasterLayer = new RasterLayerClass();
            pRasterLayer.CreateFromDataset(pRasterDataset);
            axMapControl1.ClearLayers();
            axMapControl1.AddLayer(pRasterLayer);
            axMapControl1.Refresh();

        }

        private void 加载个人地理数据库数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "打开个人地理数据库(mdb)";
            openFileDialog1.Filter = "personel geodatabase(*.mdb)|*.mdb";
            openFileDialog1.ShowDialog();
            string path = openFileDialog1.FileName;
            if (path == "")
            {
                return;
            }
            IWorkspaceFactory pWorkSpaceFactory = new AccessWorkspaceFactory();
            IWorkspace pWorkSpace = pWorkSpaceFactory.OpenFromFile(path, 0);
            ClearAllData();
            AddAllDataset(pWorkSpace, axMapControl1);
        }
        /// <summary>
        /// 加载工作空间的要素或者是栅格数据
        /// </summary>
        /// <param name="pWorkSpace"> 工作空间</param>
        /// <param name="axMapControl1">要加载的数据的控件param>
        private void AddAllDataset(IWorkspace pWorkSpace, ESRI.ArcGIS.Controls.AxMapControl axMapControl1)
        {
            IEnumDataset pEnumDataset = pWorkSpace.get_Datasets(esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataSet = pEnumDataset.Next();
            //判断数据集是否包含有数据
            while(pDataSet != null)
            {
                //分三种情况，处理要素数据集、要素类、栅格数据集
                //注意一点，要素数据集是多个二维表，要素类是一个二维表

                if (pDataSet is IFeatureDataset)//要素数据集
                {
                    IFeatureWorkspace pFeatureWorkSpace = (IFeatureWorkspace)pWorkSpace;
                    IFeatureDataset pFeatureDataSet = pFeatureWorkSpace.OpenFeatureDataset(pDataSet.Name);
                    IEnumDataset pEnumDataSet1 = pFeatureDataSet.Subsets;
                    pEnumDataSet1.Reset();
                    IDataset pDataSet1 = pEnumDataSet1.Next();
                    IGroupLayer pGroupLayer = new GroupLayerClass();
                    pGroupLayer.Name = pFeatureDataSet.Name;
                    while (pDataSet1 != null)
                    {
                        if (pDataSet1 is IFeatureClass)//要素类
                        {
                            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
                            pFeatureLayer.FeatureClass = pFeatureWorkSpace.OpenFeatureClass(pDataSet1.Name);
                            if (pFeatureLayer.FeatureClass != null)
                            {
                                pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
                                pGroupLayer.Add(pFeatureLayer);
                                axMapControl1.Map.AddLayer(pFeatureLayer);
                            }
                        }
                        pDataSet1 = pEnumDataSet1.Next();
                    }

                }
                else if (pDataSet is IFeatureClass)//要素类
                {
                    IFeatureWorkspace pFeatureWorkSpace = (IFeatureWorkspace)pWorkSpace;
                    IFeatureLayer pFeatureLayer = new FeatureLayer();
                    pFeatureLayer.FeatureClass = pFeatureWorkSpace.OpenFeatureClass(pDataSet.Name);
                    pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
                    axMapControl1.Map.AddLayer(pFeatureLayer);

                }
                else if (pDataSet is IRasterDataset)
                {
                    IRasterWorkspaceEx pRasterWorkSpace = (IRasterWorkspaceEx)pWorkSpace;
                    IRasterDataset pRasterDataSet = pRasterWorkSpace.OpenRasterDataset(pDataSet.Name);
                    IRasterPyramid pRasterPyramid = pRasterDataSet as IRasterPyramid3;
                    if (pRasterPyramid != null)
                    {
                        if (!(pRasterPyramid.Present))
                        {
                            pRasterPyramid.Create();//创建影像金字塔
                        }   
                    }
                    IRasterLayer pRasterLayer = new RasterLayerClass();
                    pRasterLayer.CreateFromDataset(pRasterDataSet);
                    ILayer pLayer = pRasterLayer as ILayer;
                    axMapControl1.AddLayer(pLayer);
                }
                pDataSet = pEnumDataset.Next();
            }
            axMapControl1.ActiveView.Refresh();
        }

        private void 加载文件地理数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IWorkspaceFactory pFileGDBworkspaceFactory;

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            string pFullPath = dlg.SelectedPath;
            if (pFullPath == "") return;
            pFileGDBworkspaceFactory = new FileGDBWorkspaceFactoryClass();
            //新增删除数据
            ClearAllData();

            //获取工作空间
            IWorkspace pWorkSpace = pFileGDBworkspaceFactory.OpenFromFile(pFullPath, 0);
            AddAllDataset(pWorkSpace, axMapControl1);

        }

        private void 加载TXT文本数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            //
            form.BuddyMap = axMapControl1;
            form.Show();
        }
        #endregion

        #region 要素选择菜单
        private void 要素选择ToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            //要素选择
            axMapControl1.CurrentTool = null;
            //清理可能存在的旧操作
            ClearCurrentOperation();

            ControlsSelectFeaturesToolClass pTool = new ControlsSelectFeaturesToolClass();
            pTool.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = (ITool)pTool;
        }

        private void 缩放至要素选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //缩放至要素选择
            ICommand cmd = new ControlsZoomToSelectedCommandClass();
            cmd.OnCreate(axMapControl1.Object);
            cmd.OnClick();

            ////自定义方法
            //int pSelection = axMapControl1.Map.SelectionCount;
            //if (pSelection == 0)
            //{
            //    MessageBox.Show("请先选择要素", "要素");
            //}
            //ISelection sel = axMapControl1.Map.FeatureSelection;
            //IEnumFeature enumsel = sel as IEnumFeature;
            //enumsel.Reset();
            //IEnvelope pEnv = new EnvelopeClass();
            //IFeature pFeature = enumsel.Next();
            //while (pFeature != null)
            //{
            //    pEnv.Union(pFeature.Extent);
            //    pFeature = enumsel.Next();
            //}
            //pEnv.Expand(1.1, 1.1, true);
            //axMapControl1.ActiveView.Extent = pEnv;
            //axMapControl1.ActiveView.Refresh();
        }

        private void 清楚选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //清除要素
            ICommand cmd = new ControlsClearSelectionCommandClass();
            cmd.OnCreate(axMapControl1.Object);
            cmd.OnClick();
        }
        #endregion

        #region 主窗体 鼠标点击事件
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            // 如果标志位为空，说明当前是“浏览模式”或使用系统工具中
            // 直接返回，不执行下面的任何自定义代码！
            if (string.IsNullOrEmpty(pMouseOperate))
            {
                return;
            }

            //鼠标点击事件的fix，坐标转换
            pPointPt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);
            
            if (e.button == 1)
            {
                IActiveView pActiveView = axMapControl1.ActiveView;

                IEnvelope pEnvelope = new EnvelopeClass();
                switch (pMouseOperate)
                {
                    #region 导出区域
                    case "ExportRegion":
                        axMapControl1.ActiveView.GraphicsContainer.DeleteAllElements();//删除所有图形要素
                        axMapControl1.ActiveView.Refresh();
                        IPolygon pPolygon = DrawPolygon(axMapControl1);
                        if (pPolygon == null) return;
                        ExportMap.AddElement(pPolygon, axMapControl1.ActiveView);
                        if (frmExpMap == null || frmExpMap.IsDisposed)
                        {
                            frmExpMap = new FormExportMap(axMapControl1);
                        }
                        frmExpMap.IsRegion = true;
                        frmExpMap.GetGeometry = pPolygon as IGeometry;
                        frmExpMap.Show();
                        frmExpMap.Activate();
                        break;
                    #endregion

                    #region 选择要素
                    case "SelFeature":
                        IEnvelope pEnv = axMapControl1.TrackRectangle();
                        IGeometry pGeo = pEnv as IGeometry;
                        //矩形框为空，对点范围进行扩展
                        if (pEnv.IsEmpty == true)
                        {
                            tagRECT r;
                            r.left = e.x - 5;
                            r.right = e.x + 5;
                            r.top = e.y - 5;
                            r.bottom = e.y + 5;
                            pActiveView.ScreenDisplay.DisplayTransformation.TransformRect(pEnv, ref r, 4);
                        }
                        axMapControl1.Map.SelectByShape(pEnv, null, false);
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        break;
                    #endregion

                    #region 距离量算
                    case "MeasureLength":
                        //判断追踪线对象是否为空，若是空则实例化并设置当前鼠标点为起始点
                        if (pNewLineFeedback == null)
                        {
                            //实例化追踪线对象
                            pNewLineFeedback = new NewLineFeedbackClass();
                            pNewLineFeedback.Display = (axMapControl1.Map as IActiveView).ScreenDisplay;
                            //设置起点，开始动态线绘制
                            pNewLineFeedback.Start(pPointPt);
                            dToltalLength = 0;
                        }
                        else//对象不为空，则添加当前鼠标点
                        {
                            pNewLineFeedback.AddPoint(pPointPt);
                        }
                        if (dSegmentLength != 0)
                        {
                            dToltalLength = dToltalLength + dSegmentLength;
                        }
                        break;
                    #endregion

                    #region 面积量算
                    case "MeasureArea":
                        //判断追踪线对象是否为空，若是空则实例化并设置当前鼠标点为起始点
                        if (pNewPolygonFeedback == null)
                        {
                            //实例化追踪面对象
                            pNewPolygonFeedback = new NewPolygonFeedbackClass();
                            pNewPolygonFeedback.Display = (axMapControl1.Map as IActiveView).ScreenDisplay;

                            pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount);
                            //开始绘制多边形
                            pNewPolygonFeedback.Start(pPointPt);
                            pAreaPointCol.AddPoint(pPointPt, ref missing, ref missing);
                        }
                        else//对象不为空，则添加当前鼠标点
                        {
                            pNewPolygonFeedback.AddPoint(pPointPt);
                            pAreaPointCol.AddPoint(pPointPt, ref missing, ref missing);
                        }
                        break;
                    #endregion
                    default:
                        break;
                }
            }
            else if (e.button == 2)
            {
                // 右键点击时，取消当前操作，回归浏览状态
                ClearCurrentOperation();

                // 如果需要，可以在这里把 CurrentTool 设回默认（非漫游），或者保持空
                axMapControl1.CurrentTool = null; 
            }
        }

        //辅助函数，用于清除鼠标绑定的的事件
        private void ClearCurrentOperation()
        {
            // 1. 重置标志位
            pMouseOperate = "";

            // 2. 清理测距/测面的“半成品”
            if (pNewLineFeedback != null)
            {
                pNewLineFeedback.Stop();
                pNewLineFeedback = null;
            }
            if (pNewPolygonFeedback != null)
            {
                pNewPolygonFeedback.Stop();
                pNewPolygonFeedback = null;
            }

            // 3. 刷新视图（清除屏幕上的临时线）
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);

            // 4. 重置鼠标样式
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }
        #endregion

        #region 数据视图、鹰眼视图、布局视图的同步

        private IPolygon DrawPolygon(AxMapControl mapCtrl)
        {
            IGeometry pGeometry = null;
            if (mapCtrl == null)
            {
                return null;
            }
            IRubberBand rb = new RubberPolygonClass();
            pGeometry = rb.TrackNew(mapCtrl.ActiveView.ScreenDisplay,null);
            return pGeometry as IPolygon;
        }
        
        //鹰眼视图的鼠标点击
        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            // 假设 EagleEyeMapConrol 引用的是 axMapControl2
            if (EagleEyeMapConrol.Map.LayerCount > 0) // 有图层数据的情況
            {
                // 判断鼠标左右键，按下鼠标左键移动矩形框
                if (e.button == 1)
                {
                    // 如果指針落在鹰眼的矩形框中，标记可移动
                    // 【修正点】: 将 pEnv.YMax 改为 pEnv.XMax
                    if (e.mapX > pEnv.XMin && e.mapY > pEnv.YMin &&
                        e.mapX < pEnv.XMax && e.mapY < pEnv.YMax)
                    {
                        bCanDrag = true;
                    }

                    pMoveRectPoint = new PointClass();
                    pMoveRectPoint.PutCoords(e.mapX, e.mapY);
                }
                // 按下鼠标右键绘制矩形框
                else if (e.button == 2)
                {
                    IEnvelope pEnvelope = EagleEyeMapConrol.TrackRectangle();
                    IPoint pTempPoint = new PointClass();
                    pTempPoint.PutCoords(pEnvelope.XMin + pEnvelope.Width / 2, pEnvelope.YMin + pEnvelope.Height / 2);
                    EagleEyeMapConrol.Extent = pEnvelope;
                    // 矩形框的高宽和数据视图的高宽不一定成正比，这里做一个中心调整；
                    EagleEyeMapConrol.CenterAt(pTempPoint);
                }
            }
        }
        
        //主窗体与鹰眼同步
        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            // 每当底图更新时，自动刷新编辑下拉框
            List<ILayer> layers = MapManager.GetLayers(axMapControl1.Map);
            InitComboBox(layers);

            // 确保它是可用的
            cmbSelLayer.Enabled = true;

            //同步方法:主控件与鹰眼
            SynchronizeAxMapControl2();

            //同步数据视图和布局试图
            CopyToPageLayout();
            MessageBox.Show("ALL DONE!\n数据加载成功！");
        }
        
        private void SynchronizeAxMapControl2()
        {
            //先清除已有图层
            if (EagleEyeMapConrol.LayerCount > 0 )
            {
                EagleEyeMapConrol.ClearLayers();
            }
            EagleEyeMapConrol.SpatialReference = axMapControl1.SpatialReference;
            //遍历主控件的图层
            for (int i = axMapControl1.LayerCount - 1; i >= 0; i--)
            {
                ILayer pLayer = axMapControl1.get_Layer(i);
                //是图层组的情况
                if (pLayer is IGroupLayer || pLayer is ICompositeLayer)
                {
                    ICompositeLayer pCompositeLayer = (ICompositeLayer)pLayer;
                    for (int j = pCompositeLayer.Count - 1; j >= 0; j--)
                    {
                        ILayer pSubLayer = pCompositeLayer.get_Layer(j);
                        IFeatureLayer pFeatureLayer = pSubLayer as IFeatureLayer;
                        if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                        {
                            EagleEyeMapConrol.AddLayer(pSubLayer);
                        }
                    }
                }
                else
                {
                    //非图层组，直接遍历
                    IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                    if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                    {
                        EagleEyeMapConrol.AddLayer(pLayer);
                    }
                }
                EagleEyeMapConrol.Extent = axMapControl1.FullExtent;
                pEnv = axMapControl1.Extent as IEnvelope;
                DrawRectangle(pEnv);
                EagleEyeMapConrol.ActiveView.Refresh();
            }
        }
        //绘制矩形框的方法
        private void DrawRectangle(IEnvelope pEnv)
        {
            //清除鹰眼控件的图形元素
            IGraphicsContainer pGraphicsContainer = EagleEyeMapConrol.Map as IGraphicsContainer;
            pGraphicsContainer.DeleteAllElements();
            IActiveView pActiveView = pGraphicsContainer as IActiveView;
            //得到当前试图范围
            IRectangleElement pRectEle = new RectangleElementClass();
            IElement pEle = pRectEle as IElement;
            pEle.Geometry = pEnv;
            //绘制符号框线
            IRgbColor pColor = new RgbColorClass();
            pColor = GetRgbColor(255,0,0);
            pColor.Transparency = 255;
            ILineSymbol pOutLine = new SimpleLineSymbolClass();
            pOutLine.Color = pColor;
            pOutLine.Width = 2;

            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pColor = new RgbColorClass();
            pColor.Transparency = 0;
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pOutLine;
            //向鹰眼控件中添加矩形框
            IFillShapeElement pFillShpEle = pEle as IFillShapeElement;
            pFillShpEle.Symbol = pFillSymbol;
            pGraphicsContainer.AddElement((IElement)pFillShpEle,0);
            //刷新
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics,null,null);
        }

        private IRgbColor GetRgbColor(int R, int G, int B)
        {
            IRgbColor pRgbColor = null;
            //if(int R <0
            pRgbColor = new RgbColorClass();
            pRgbColor.Red = R;
            pRgbColor.Green = G;
            pRgbColor.Blue = B;
            return pRgbColor;
        }

        private void EagleEyeMapConrol_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            // 1. 鼠标光标控制逻辑
            if (e.mapX > pEnv.XMin && e.mapY > pEnv.YMin && e.mapX < pEnv.XMax && e.mapY < pEnv.YMax)
            {
                // 鼠标在矩形框内部，显示小手
                EagleEyeMapConrol.MousePointer = esriControlsMousePointer.esriPointerHand;

                // 注意：这里不用处理 e.button == 2 的逻辑
            }
            else
            {
                // 鼠标在其他位置，显示默认样式
                EagleEyeMapConrol.MousePointer = esriControlsMousePointer.esriPointerDefault;
            }

            // 2. 拖动逻辑
            if (bCanDrag) // bCanDrag 应该在 OnMouseDown (左键) 中被设置为 true
            {
                // 计算鼠标移动的地图距离 (Dx, Dy)
                double Dx, Dy;
                Dx = e.mapX - pMoveRectPoint.X;
                Dy = e.mapY - pMoveRectPoint.Y;

                // 移动矩形框的地理范围
                pEnv.Offset(Dx, Dy);

                // 更新新的起点坐标，准备下一次的 OnMouseMove
                pMoveRectPoint.PutCoords(e.mapX, e.mapY);

                // 绘制新的矩形框
                DrawRectangle(pEnv);

                // 【关键操作】：更新主地图的视图范围
                // 假设您的主地图控件是 axMapControl1
                this.axMapControl1.Extent = pEnv;

                // 刷新主地图，只刷新地理要素
                this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

                // 刷新鹰眼视图的图形层（显示移动后的矩形）
                EagleEyeMapConrol.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
            }
        }

        private void EagleEyeMapConrol_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            if (e.button == 1 && pMoveRectPoint != null)
            {
                if (e.mapX == pMoveRectPoint.X && e.mapY == pMoveRectPoint.Y)
                {
                    axMapControl1.CenterAt(pMoveRectPoint);
                }
                bCanDrag = false;
            }
        }

        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            //得到當前試圖範圍
            pEnv = (IEnvelope)e.newEnvelope;
            DrawRectangle(pEnv);
        }
  
        //将mapcontrol复制到pagelayoutcontrol中
        private void CopyToPageLayout()
        {
            IObjectCopy objectCopy = new ObjectCopy();//对象拷贝接口
            object copyFromMap = axMapControl1.Map;//地图对象
            object copyMap = objectCopy.Copy(copyFromMap);//将axMapControl1的地图对象拷贝
            object copyToMap = axPageLayoutControl1.ActiveView.FocusMap;//axPageLayoutControl1活动视图中的地图
            objectCopy.Overwrite(copyMap, ref copyToMap);//将axMapControl1地图对象覆盖axPageLayout1当前地图
        }
        
        //实时同步数据和布局视图
        private void axMapControl1_OnAfterScreenDraw(object sender, IMapControlEvents2_OnAfterScreenDrawEvent e)
        {
            //获取pagelayout的当前视图
            IActiveView pActiveView = (IActiveView)axPageLayoutControl1.ActiveView.FocusMap;
            //显示转换
            IDisplayTransformation pDTF = pActiveView.ScreenDisplay.DisplayTransformation;
            //设置范围
            pDTF.VisibleBounds = axMapControl1.Extent;
            axPageLayoutControl1.ActiveView.Refresh();

            CopyToPageLayout();
        }
        #endregion

        #region TOC 内容列表：符号选择、顺序的调整（左键）、显示菜单（右键）
        //TOC双击事件，选择符号
        //private void axTOCControl1_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        //{
        //    esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
        //    IBasicMap basicMap = null;
        //    ILayer layer = null;
        //    object unk = null;
        //    object data = null;
        //    axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);
        //    if (e.button == 1)
        //    {
        //        if (itemType == esriTOCControlItem.esriTOCControlItemLegendClass)
        //        {
        //            //取得图例
        //            ILegendClass pLegendClass = ((ILegendGroup)unk).get_Class((int)data);
        //            //创建符号选择器 SymbolSelector 实例
        //            frmSymbolSelector SymbolSelectorFrm = new frmSymbolSelector(pLegendClass, layer);
        //            if (SymbolSelectorFrm.ShowDialog() == DialogResult.OK)
        //            {
        //                //局部更新主 Map 控件
        //                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography,
        //                null, null);
        //                pLegendClass.Symbol = SymbolSelectorFrm.pSymbol; //设置新的符号
        //                this.axMapControl1.ActiveView.Refresh(); //更新主 Map 控件和图层控件
        //                this.axMapControl1.Refresh();
        //            }
        //        }
        //    }
        //}
        private void axTOCControl1_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap basicMap = null;
            ILayer layer = null;
            object unk = null;
            object data = null;

            // 1. 探测点击位置
            axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);

            if (e.button == 1) // 左键双击
            {
                if (itemType == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    // 2. 取得当前图例
                    ILegendClass pLegendClass = ((ILegendGroup)unk).get_Class((int)data);

                    // 3. 打开符号选择器
                    frmSymbolSelector SymbolSelectorFrm = new frmSymbolSelector(pLegendClass, layer);
                    if (SymbolSelectorFrm.ShowDialog() == DialogResult.OK)
                    {
                        // 【核心修正 A】：先设置新的符号
                        if (SymbolSelectorFrm.pSymbol != null)
                        {
                            pLegendClass.Symbol = SymbolSelectorFrm.pSymbol;
                        }

                        // 【核心修正 B】：声明内容已改变，这是同步 TOC 和 Map 的关键
                        axMapControl1.ActiveView.ContentsChanged();

                        // 【核心修正 C】：刷新地图和 TOC
                        // 刷新地理要素层
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                        // 刷新 TOC 列表，使左侧图标更新
                        axTOCControl1.Update();
                    }
                }
            }
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 1 )//鼠标左键
            {
                esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pMap = null;
                object unk = null;
                object data = null;
                ILayer pLayer = null;
                axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref unk, ref data);
                if (pLayer == null) return;
                pMoveLayerPoint.PutCoords(e.x, e.y);//记录鼠标单击的坐标位置
                if (pItem == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    if (pLayer is IAnnotationSublayer)
                    {
                        return;
                    }
                    else
                    {
                        pMoveLayer = pLayer;
                    }
                }

            }
            if (e.button ==2 )//鼠标右键
            {
                esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pMap = null;
                ILayer pLayer = null;
                object unk = null;
                object data = null;
                axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref unk, ref data);
                pTocFeatureLayer = (IFeatureLayer)pLayer;
                if (pItem == esriTOCControlItem.esriTOCControlItemLayer && pTocFeatureLayer != null)
                {
                    btnLayerSel.Enabled = !pTocFeatureLayer.Selectable;
                    btnLayerUnSel.Enabled = pTocFeatureLayer.Selectable;
                    contextMenuStrip1.Show(Control.MousePosition);//弹出右键菜单
                }
            }
        }

        private void axTOCControl1_OnMouseUp(object sender, ITOCControlEvents_OnMouseUpEvent e)
        {
            // 确保只处理鼠标左键释放事件，右键不需要处理图层移动
            if (e.button != 1) return;

            // 检查是否有图层处于被拖动状态（在 OnMouseDown 中被设置）
            if (pMoveLayer == null) return;

            esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap pMap = null;
            object unk = null;
            object data = null;
            ILayer pLayer = null;

            // 1. 进行 HitTest，确定鼠标松开的位置
            axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref unk, ref data);

            // 获取当前焦点地图
            IMap focusMap = axMapControl1.ActiveView.FocusMap;

            // 初始化目标索引为 -1 (无效值)
            toIndex = -1;

            // 2. 根据 HitTest 结果确定图层的新位置 (toIndex)

            // 情景 A: 鼠标松开在另一个图层上 (pLayer != null)
            if (pLayer != null)
            {
                // 确保被移动的图层不是当前鼠标下的图层 (即发生了有效的拖动)
                if (pMoveLayer != pLayer)
                {
                    // 循环遍历当前地图的所有图层，找到目标图层 pLayer 的索引
                    for (int i = 0; i < focusMap.LayerCount; i++)
                    {
                        ILayer pTempLayer = focusMap.get_Layer(i);
                        if (pTempLayer == pLayer)
                        {
                            // 获取目标图层的索引作为移动目标位置
                            toIndex = i;
                            break; // 找到即退出循环
                        }
                    }
                }
                else
                {
                    // 如果在同一个图层上释放，则视为无效移动，退出
                    pMoveLayer = null;
                    return;
                }
            }
            // 情景 B: 鼠标松开在 TOCControl 的空白处（将图层移到最底部）
            else if (pItem == esriTOCControlItem.esriTOCControlItemNone)
            {
                toIndex = focusMap.LayerCount - 1; // 移到最底层 (索引最大)
            }
            // 情景 C: 鼠标松开在 Map 节点上（将图层移到最顶部）
            else if (pItem == esriTOCControlItem.esriTOCControlItemMap)
            {
                toIndex = 0; // 移到最顶层 (索引最小)
            }

            // 3. 执行图层移动（确保 toIndex 是有效的）
            if (toIndex != -1)
            {
                focusMap.MoveLayer(pMoveLayer, toIndex); // 使用正确的 focusMap 变量
            }

            // 4. 重置状态并刷新
            pMoveLayer = null; // 重置 pMoveLayer，防止下次鼠标事件误判

            axMapControl1.ActiveView.Refresh();
            axTOCControl1.Update();
        }
        //有bug的事件处理，上边是generate by ai的内容，优化过了
        //private void axTOCControl1_OnMouseUp(object sender, ITOCControlEvents_OnMouseUpEvent e)
        //{
        //    esriTOCControlItem pItem = esriTOCControlItem.esriTOCControlItemNone;
        //    IBasicMap pMap = null;
        //    object unk = null;
        //    object data = null;
        //    ILayer pLayer = null;
        //    axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pMap, ref pLayer, ref unk, ref data);
        //    IMap focusMap = axMapControl1.ActiveView.FocusMap;
        //    // 【修正 A】：确保 pMoveLayer 被设置（即左键按下过图层）
        //    if (pMoveLayer == null) return;
        //    if (pItem == esriTOCControlItem.esriTOCControlItemLayer || pLayer != null)
        //    {
        //        if (pMoveLayer != pLayer)
        //        {
        //            ILayer pTempLayer;
        //            for (int i = 0; i < focusMap.LayerCount; i++)
        //            {
        //                pTempLayer = focusMap.get_Layer(i);
        //                if (pTempLayer == pLayer)
        //                {
        //                    toIndex = i;
        //                }
        //            }
        //        }
        //    }
        //    else if (pItem == esriTOCControlItem.esriTOCControlItemNone)
        //    {
        //        toIndex = focusMap.LayerCount - 1;
        //    }
        //    else if (pItem == esriTOCControlItem.esriTOCControlItemMap)
        //    {
        //        toIndex = 0;
        //    }
        //    focusMap.MoveLayer(pMoveLayer,toIndex);
        //    //这行代码有问题
        //    axMapControl1.ActiveView.Refresh();
        //    axTOCControl1.Update();
        //}
        #endregion

        #region 右击图层菜单的5个事件
        //右击属性表
        private void btnAttribute_Click(object sender, EventArgs e)
        {
            if (frmattirbute == null || frmattirbute.IsDisposed)
            {
                frmattirbute = new 自定义窗体控件_FormAttribute.FormAttribute();
            }
            frmattirbute.CurFeatureLayer = pTocFeatureLayer;
            frmattirbute.InitUI();
            frmattirbute.ShowDialog();
        }
        //右击缩放图层
        private void btnZoomToLayer_Click(object sender, EventArgs e)
        {
            if (pTocFeatureLayer == null) return;
            (axMapControl1.Map as IActiveView).Extent = pTocFeatureLayer.AreaOfInterest;
            (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }
        //右击删除图层
        private void btnRemovelLayer_Click(object sender, EventArgs e)
        {
            if (pTocFeatureLayer == null) return;
            DialogResult result = MessageBox.Show("删除？"+pTocFeatureLayer.Name,"提示",
            MessageBoxButtons.OKCancel,MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                axMapControl1.Map.DeleteLayer(pTocFeatureLayer);
            }
            axMapControl1.ActiveView.Refresh();
        }
        //右击可选
        private void btnLayerSel_Click(object sender, EventArgs e)
        {
            pTocFeatureLayer.Selectable = true;
            btnLayerSel.Enabled = !btnLayerSel.Enabled;
        }
        //右击不可选
        private void btnLayerUnSel_Click(object sender, EventArgs e)
        {
            pTocFeatureLayer.Selectable = false;
            btnLayerUnSel.Enabled = !btnLayerUnSel.Enabled;
        }
        #endregion

        #region 定制化对话框
        //初始化定制对话框的内容
        private void CreateCusDialog()
        {
            //定义事件的接口
            ICustomizeDialogEvents_Event pCusEvent = cd as ICustomizeDialogEvents_Event;
            //实例化事件委托
            startDialogE = new ICustomizeDialogEvents_OnStartDialogEventHandler(OnStartCusDialog);
            closeDialogE = new ICustomizeDialogEvents_OnCloseDialogEventHandler(OnCloseCusDialog);

            //将事件与委托绑定
            pCusEvent.OnStartDialog += startDialogE;
            pCusEvent.OnCloseDialog += closeDialogE;

            cd.SetDoubleClickDestination(axToolbarControl1);
        }
        //关闭对话框的对话方法
        private void OnCloseCusDialog()
        {
            axToolbarControl1.Customize = false;
            chkCustomize.Checked = false;
        }
        //打开对话框的调用方法
        private void OnStartCusDialog()
        {
            axToolbarControl1.Customize = true;
            chkCustomize.Checked = true;
        }
        
        private void chkCustomize_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCustomize.Checked == false)
            {
                cd.CloseDialog();
            }
            else
            {
                cd.StartDialog(axToolbarControl1.hWnd);
            }
        }
        #endregion

        #region 坐标实时更新显示
        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            
            string SMapUnits = GetMapUnits(axMapControl1.Map.MapUnits);
            BarCoorTxt.Text = string.Format("当前坐标为:X = {0:#.####} Y = {1:#.####}{2}",e.mapX,e.mapY,SMapUnits);

            pMovePt = (axMapControl1.Map as IActiveView).ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);

            #region 长度量算
            if (pMouseOperate == "MeasureLength")
            {
                if (pNewLineFeedback != null)
                {
                    pNewLineFeedback.MoveTo(pMovePt);//临时点的延长线
                }
                double deltaX = 0;//两点的x差值
                double deltaY = 0;//y差值
                if ((pPointPt != null) && (pNewLineFeedback != null))
                {
                    deltaX = pMovePt.X - pPointPt.X;
                    deltaY = pMovePt.Y - pPointPt.Y;
                    dSegmentLength = Math.Round(Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY)), 3);//勾股定理计算距离
                    dToltalLength = dToltalLength + dSegmentLength;
                    if (frmMeasureResult != null)
                    {
                        frmMeasureResult.IbIMeasureResult.Text = string.Format("当前线段的长度：{0:.###}{1}；\r \n总长度为:{2:.###}{1}", dSegmentLength, SMapUnits, dToltalLength);
                        dToltalLength = dToltalLength - dSegmentLength;//鼠标移动到新点重新开始计算
                    }
                    frmMeasureResult.frmClosed += new FormMeasureResult.FormClosedEventHandler(frmMeasureResult_frmColsed);
                }
            }
            #endregion

            #region 面积量算
            if (pMouseOperate == "MeasureArea")
            {
                if (pNewPolygonFeedback != null)
                {
                    pNewPolygonFeedback.MoveTo(pMovePt);
                }
                IPointCollection pPointCol = new Polygon();
                IPolygon pPolygon = new PolygonClass();
                IGeometry pGeo = null;
        
                ITopologicalOperator pTopo = null;
                for (int i = 0; i <= pAreaPointCol.PointCount - 1; i++)//遍历当前鼠标点击过的点
                {
                    pPointCol.AddPoint(pAreaPointCol.get_Point(i), ref missing, ref missing);
                }
                pPointCol.AddPoint(pMovePt, ref missing, ref missing);
        
                if (pPointCol.PointCount < 3) return;//多边形最少需要三个点
                pPolygon = pPointCol as IPolygon;
        
                if ((pPolygon != null))
                {
                    pPolygon.Close();//强制几何完美闭合，才可以计算面积
                    pGeo = pPolygon as IGeometry;
                    pTopo = pGeo as ITopologicalOperator;
                    //使几何图形的拓扑正确
                    pTopo.Simplify();
                    pGeo.Project(axMapControl1.Map.SpatialReference);//设置坐标
                    IArea pArea = pGeo as IArea;

                    frmMeasureResult.IbIMeasureResult.Text = String.Format("总面积为： {0:.####}平方{1};\r\n总长度为： {2:.####}{1}", pArea.Area, SMapUnits, pPolygon.Length);
                    pPolygon = null;
                }
            }
            #endregion
        }

        private string GetMapUnits(esriUnits esriUnits)
        {
            string sMapUnits = string.Empty;
            switch (esriUnits)
            {
                case esriUnits.esriCentimeters:
                    sMapUnits = "厘米";
                    break;
                case esriUnits.esriDecimalDegrees:
                    sMapUnits = "十进制度";
                    break;
                case esriUnits.esriDecimeters:
                    sMapUnits = "分米";
                    break;
                case esriUnits.esriFeet:
                    sMapUnits = "尺";
                    break;
                case esriUnits.esriInches:
                    sMapUnits = "英尺";
                    break;
                case esriUnits.esriKilometers:
                    sMapUnits = "千米";
                    break;
                case esriUnits.esriMeters:
                    sMapUnits = "米";
                    break;
                case esriUnits.esriMiles:
                    sMapUnits = "英里";
                    break;
                case esriUnits.esriMillimeters:
                    sMapUnits = "毫米";
                    break;
                case esriUnits.esriNauticalMiles:
                    sMapUnits = "海里";
                    break;
                case esriUnits.esriPoints:
                    sMapUnits = "点";
                    break;
                case esriUnits.esriUnitsLast:
                    sMapUnits = "UnitsLast";
                    break;
                case esriUnits.esriUnknownUnits:
                    sMapUnits = "未知";
                    break;
                case esriUnits.esriYards:
                    sMapUnits = "码";
                    break;
                default:
                    break;
            }
            return sMapUnits;
        }
        #endregion

        #region 创建要素类-自定义方法
        //创建要素类
        public IFeatureClass CreateFeatureClass2AccessDB(string featureClassName, UID classExtensionUID, IFeatureWorkspace featureWorkspace)
        {
            //创建字段集合
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;
            //添加object ID到字段集合中，创建要素类时必须包含该字段
            IField oidField = new FieldClass();
            IFieldEdit oidFieldEdit = (IFieldEdit)oidField;
            oidFieldEdit.Name_2 = "OID";
            oidFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(oidField);
            //为要素类创建集合定义和空间参考
            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_20N );
            ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
            spatialReferenceResolution.ConstructFromHorizon();
            ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
            spatialReferenceTolerance.SetDefaultXYTolerance();
            geometryDefEdit.SpatialReference_2 = spatialReference;
            // 将几何字段添加到字段集合
            IField geometryField = new FieldClass();
            IFieldEdit geometryFieldEdit = (IFieldEdit)geometryField;
            geometryFieldEdit.Name_2 = "Shape";
            geometryFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            geometryFieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(geometryField);
            // 创建字段"Name
            IField nameField = new FieldClass();
            IFieldEdit nameFieldEdit = (IFieldEdit)nameField;
            nameFieldEdit.Name_2 = "Name";
            nameFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            nameFieldEdit.Length_2 = 20;
            fieldsEdit.AddField(nameField);
            IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(featureClassName, fields, null, classExtensionUID,esriFeatureType.esriFTSimple, "Shape", "");
            return featureClass;
        }
        #endregion

        #region 地图导出两个选项
        private void 局部导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            pMouseOperate = "ExportRegion";
        }

        private void 全局导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //frmExpMap 以声明为全局变量
            if (frmExpMap == null || frmExpMap.IsDisposed)
            {
                frmExpMap = new FormExportMap(axMapControl1);
            }
            frmExpMap.IsRegion = false;
            frmExpMap.GetGeometry = axMapControl1.ActiveView.Extent;
            frmExpMap.Show();
            frmExpMap.Activate();
        }
        #endregion

        #region 地图量测选项

        //窗体关闭函数
        private void frmMeasureResult_frmColsed()
        {
            //清空线对象
            if (pNewLineFeedback != null)
            {
            pNewLineFeedback.Stop();
            pNewLineFeedback = null;
            }
            //清空面对象
            if (pNewPolygonFeedback != null)
            {
            pNewPolygonFeedback.Stop();
            pNewPolygonFeedback = null;
            pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount); //清空点集中所有点
            }
            //清空量算画的线、面对象
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
            //结束量算功能
            pMouseOperate = string.Empty;
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
        }

        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            string SMapUnits = GetMapUnits(axMapControl1.Map.MapUnits);
            #region 长度量算
            if (pMouseOperate == "MeasureLength")
            {
                if (frmMeasureResult != null)
                {
                    frmMeasureResult.IbIMeasureResult.Text = "线段总长度为： " + dToltalLength + SMapUnits;
                }
                if (pNewLineFeedback != null)
                {
                    pNewLineFeedback.Stop();
                    pNewLineFeedback = null;
                    //清空所画的线对象
                    (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                }
                dToltalLength = 0;
                dSegmentLength = 0;
            }
            #endregion
            
            #region 面积量算
            if (pMouseOperate == "MeasureArea")
            {
            if (pNewPolygonFeedback != null)
            {
            pNewPolygonFeedback.Stop();
            pNewPolygonFeedback = null;
            //清空所画的线对象
            (axMapControl1.Map as IActiveView).PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
            }
            pAreaPointCol.RemovePoints(0, pAreaPointCol.PointCount); //清空点集中所有点
            }
            #endregion
        }

        private void 距离量测_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "MeasureLength";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            if (frmMeasureResult == null || frmMeasureResult.IsDisposed)
            {
                frmMeasureResult = new FormMeasureResult();
                frmMeasureResult.frmClosed += new
                FormMeasureResult.FormClosedEventHandler(frmMeasureResult_frmColsed);
                frmMeasureResult.IbIMeasureResult.Text = "";
                frmMeasureResult.Text = "距离量测";
                frmMeasureResult.Show();
            }
            else
            {
                frmMeasureResult.Activate();
            }
        }

        private void 面积量测_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
            pMouseOperate = "MeasureArea";
            axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            if (frmMeasureResult == null || frmMeasureResult.IsDisposed)
            {
                frmMeasureResult = new FormMeasureResult();
                frmMeasureResult.frmClosed += new
                FormMeasureResult.FormClosedEventHandler(frmMeasureResult_frmColsed);
                frmMeasureResult.IbIMeasureResult.Text = "";
                frmMeasureResult.Text = "面积量测";
                frmMeasureResult.Show();
            }
            else
            {
                frmMeasureResult.Activate();
            }
        }
        #endregion

        #region 查询统计
        private void 地图选择_Click(object sender, EventArgs e)
        {
            //实例化类
            FormSelection formSelection = new FormSelection();
            //主窗体控件赋值给CurrentMap属性
            formSelection.CurrentMap = axMapControl1.Map;
            if (axMapControl1.Map.SelectionCount == 0)
            {
                MessageBox.Show("主窗体检测到：当前没有选中任何要素！");
            }
            //显示该窗体
            formSelection.Show();
        }

        private void 统计选择集_Click(object sender, EventArgs e)
        {
            //新建统计窗口
            FormStatistics formStatistics = new FormStatistics();
            //将当前主窗体中的MapControl1控件的Map对象赋值给FormStatistics窗体的CurrentMap属性，完成属性传递；
            formStatistics.CurrentMap = axMapControl1.Map;
            //显示统计窗体
            formStatistics.Show();
        }
        #endregion

        #region 编辑功能
        
        //初始化编辑功能
        private void InitObject()
        {
            try
            {
                ChangeButtonState(false);
                pEngineEditor = new EngineEditorClass();
                MapManager.EngineEditor = pEngineEditor;
                pEngineEditTask = pEngineEditor as IEngineEditTask;
                pEngineEditLayers = pEngineEditor as IEngineEditLayers;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //编辑图层改变
        private void ChangeButtonState(bool bEnable)
        {
            // 开始/结束按钮互斥
            tsmStartEdit.Enabled = !bEnable;
            tsmSaveEdit.Enabled = bEnable;
            tsmEndEdit.Enabled = bEnable;

            // 【修改点】不要禁用下拉框，让它始终可选
            // 这样用户才能在开始编辑前选中一个图层
            cmbSelLayer.Enabled = true;

            tsmAddFeature.Enabled = bEnable;
        }
        //初始化编辑图层列表
        private void InitComboBox(List<ILayer> plstLyr)
        {

            cmbSelLayer.Items.Clear();
            for (int i = 0; i < plstLyr.Count; i++)
            {
                if (!cmbSelLayer.Items.Contains(plstLyr[i].Name))
                {
                    cmbSelLayer.Items.Add(plstLyr[i].Name);
                }
            }
            if (cmbSelLayer.Items.Count != 0)
                cmbSelLayer.SelectedIndex = 0;
        }

        //开始编辑事件
        private void tsmStartEdit_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. 基础检查
                if (axMapControl1.Map.LayerCount == 0)
                {
                    MessageBox.Show("当前没有加载任何图层！");
                    return;
                }

                // 2. 【核心修改】这里不要再 InitComboBox 了！
                // 直接检查用户有没有选图层
                if (cmbSelLayer.SelectedItem == null)
                {
                    MessageBox.Show("请先在工具栏下拉框中选择一个要编辑的目标图层！\n(如果下拉框为空，请检查图层是否加载)", "提示");
                    return;
                }

                // 3. 获取选中的图层
                string layerName = cmbSelLayer.SelectedItem.ToString();
                IFeatureLayer startLayer = MapManager.GetLayerByName(axMapControl1.Map, layerName) as IFeatureLayer;

                if (startLayer == null)
                {
                    MessageBox.Show("无法获取选中的图层对象，请重新选择。");
                    return;
                }

                IDataset pDataset = startLayer.FeatureClass as IDataset;
                if (pDataset == null) return;
                IWorkspace pWs = pDataset.Workspace;

                // 4. 检查是否已经在编辑
                if (pEngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing)
                {
                    MessageBox.Show("当前正在编辑中！");
                    return;
                }

                // 5. 开始编辑会话
                pEngineEditor.EnableUndoRedo(true);
                pEngineEditor.StartEditing(pWs, axMapControl1.Map);

                // 6. 设置任务和目标
                IEngineEditTask pTask = pEngineEditor.GetTaskByUniqueName("ControlToolsEditing_CreateNewFeatureTask");
                if (pTask != null) pEngineEditor.CurrentTask = pTask;

                SetTargetLayerSafe(startLayer);
                ChangeButtonState(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("开始编辑失败: " + ex.Message);
            }
        }

        //comboBox事件
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 如果没有在编辑，或者控件没初始化，直接返回
            if (pEngineEditor == null) return;

            // 获取选中的图层
            string sLyrName = cmbSelLayer.SelectedItem.ToString();
            IFeatureLayer pSelectedLyr = MapManager.GetLayerByName(pMap, sLyrName) as IFeatureLayer;

            if (pSelectedLyr == null) return;

            // 如果正在编辑中，必须检查工作空间是否匹配！
            if (pEngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
            {
                IDataset pDS = pSelectedLyr.FeatureClass as IDataset;
                IWorkspace pLyrWs = pDS.Workspace;

                // 获取当前编辑器绑定的工作空间
                IWorkspace pEditWs = pEngineEditor.EditWorkspace; // 正确代码

                // 比较两个工作空间是否相同 (比较连接字符串或路径)
                if (pLyrWs.PathName != pEditWs.PathName)
                {
                    MessageBox.Show("无法切换目标图层！\n该图层与当前编辑会话不在同一个工作空间(Database/Folder)下。\n请先停止编辑。", "警告");

                    // 可以考虑在这里把 ComboBox 选回原来的图层，防止用户困惑
                    return;
                }

                // 工作空间一致，安全设置目标
                SetTargetLayerSafe(pSelectedLyr);
            }
            else
            {
                // 如果没在编辑，仅仅记录一下当前选的图层变量即可
                pCurrentLyr = pSelectedLyr;
            }
        }

        // 封装一个安全设置目标图层的方法
        private void SetTargetLayerSafe(IFeatureLayer layer)
        {
            try
            {
                if (pEngineEditLayers == null) return;
                pEngineEditLayers.SetTargetLayer(layer, 0);
                pCurrentLyr = layer; // 更新全局变量
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置目标图层失败: " + ex.Message);
            }
        }
        
        //结束编辑事件
        private void tsmEndEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (pEngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    // 询问是否保存
                    DialogResult dr = MessageBox.Show("是否保存已修改的数据？", "提示", MessageBoxButtons.YesNoCancel);

                    if (dr == DialogResult.Cancel) return;

                    bool save = (dr == DialogResult.Yes);

                    // StopEditing 参数: true=保存并停止, false=丢弃并停止
                    pEngineEditor.StopEditing(save);
                }

                axMapControl1.CurrentTool = null;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
                ChangeButtonState(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("停止编辑出错: " + ex.Message);
            }
        }

        //保存编辑事件
        private void tsmSaveEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (pEngineEditor.EditState == esriEngineEditState.esriEngineStateEditing)
                {
                    // true 表示只保存但不停止编辑
                    //pEngineEditor.SaveEdits();
                    MessageBox.Show("保存成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message);
            }
        }

        //创建要素事件
        private void 创建要素类ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pEngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)
                {
                    MessageBox.Show("请先开始编辑！");
                    return;
                }

                // 使用 Command 是可以的，因为它是工具(Tool)
                ICommand m_CreateFeatTool = new CreateFeatureToolClass();
                m_CreateFeatTool.OnCreate(axMapControl1.Object);

                // 确保工具可用
                if (m_CreateFeatTool.Enabled)
                {
                    axMapControl1.CurrentTool = m_CreateFeatTool as ITool;
                    m_CreateFeatTool.OnClick();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 地图浏览
        private void 地图制作ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. 清理之前的任何自定义操作（测量、选择等）
                ClearCurrentOperation();

                // 2. 激活 ArcEngine 自带的漫游工具
                // 这会自动接管鼠标事件，实现完美的拖动效果
                ICommand pPanTool = new ESRI.ArcGIS.Controls.ControlsMapPanToolClass();
                pPanTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = pPanTool as ITool;

                // 注意：一旦设置了 CurrentTool 为 PanTool，
                // MapControl 会优先响应工具的事件，自定义的 OnMouseDown 基本就被屏蔽了。
                // 但为了双重保险，我们在 OnMouseDown 里还是要做判断。
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

    }
}
