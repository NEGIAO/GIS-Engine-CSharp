using System;//系统引用文件
using System.IO;
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
using 自定义窗体控件_符号系统;
using 自定义窗体控件_地图打印;

namespace GIS_2310130172_Engine
{
    public partial class FrmMain : Form
    {
        #region 全局标识变量

        // 存储当前地图文档的路径
        private string m_mapDocumentName = string.Empty; 

        // 全局标志位：控制当前鼠标的功能
        string pMouseOperate = "";

        // 专门用于“测距”
        private INewLineFeedback pNewLineFeedback = null;
        // 专门用于“测面” 
        private INewPolygonFeedback pNewPolygonFeedback = null;

        // 测量辅助变量
        private FormMeasureResult frmMeasureResult = null;  // 量算结果窗体
        private IPoint pMovePt = null;                      // 鼠标移动时的当前点
        private double dToltalLength = 0;                   // 测量的总长度
        private double dSegmentLength = 0;                  // 片段距离
        private IPointCollection pAreaPointCol = new MultipointClass(); // 面积量算点集
        private object missing = Type.Missing;

        //辅助功能变量 (鹰眼、TOC、导出)
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

        private 自定义窗体控件_FormAttribute.FormAttribute frmattribute = null;

        // 定制化 Toolbar
        private ICustomizeDialog cd = new CustomizeDialogClass();
        private ICustomizeDialogEvents_OnStartDialogEventHandler startDialogE;
        private ICustomizeDialogEvents_OnCloseDialogEventHandler closeDialogE;

        // 编辑功能变量
        private IMap pMap = null;
        private IActiveView pActiveView = null;
        private List<ILayer> plstLayers = null;
        private IFeatureLayer pCurrentLyr = null;

        // 编辑器核心对象 (在构造函数中初始化)
        private IEngineEditor pEngineEditor = null;
        private IEngineEditTask pEngineEditTask = null;
        private IEngineEditLayers pEngineEditLayers = null;

        #endregion

        #region 程序入口
        //构造函数，初始化
        public FrmMain()
        {
            InitializeComponent();

            //初始化只用绑定map、toolbar、toc等三个控件
            axTOCControl1.SetBuddyControl(axMapControl1.Object);
            axToolbarControl1.SetBuddyControl(axMapControl1.Object);

            InitObject();//初始化编辑器
        }

        //主窗体的加载事件：定制对话框、自定义工具的载入
        private void FrmMain_Load(object sender, EventArgs e)
        {
            #region 调试时，注释掉该代码，可以加载mxd文档，方便debug;正式运行时，保留该代码，使地图默认打开为空；
            // 1. 实例化一个新的 IMap 对象
            IMap newMap = new MapClass();
            // 2. 将新的空 Map 对象赋给 AxMapControl
            axMapControl1.Map = newMap;
            // 3. 确保显示刷新
            axMapControl1.ActiveView.Refresh();
            #endregion

            //定制对话框
            chkCustomize.Checked = false;
            chkCustomize.CheckOnClick = true;
            CreateCusDialog();

            //自定义工具是添加新的类来实现的，这里是添加到mapcontrol中
            //1、清除当前工具命令
            axToolbarControl1.AddItem(new ClearCurrentToolCMD(), -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconAndText);
            
            //2、添加日期工具（修改：取消在addDataTool中传入参数，避免冲突)
            AddDateTool addDateTool = new AddDateTool();
            axToolbarControl1.AddItem(addDateTool, -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconAndText);
            
            //3、DLL文件扩展
            //自定义的dll菜单项，内部应有六个子菜单，用于扩展功能使用
            IMenuDef menuDef = new 自定义DLL_SymbologyMenu.SymbologyMenu();
            axToolbarControl1.AddItem(menuDef, -1, -1, false, -1, esriCommandStyles.esriCommandStyleIconAndText);
        }
        #endregion


        #region 选项卡代码
        #region 文件选项卡
        #region 地图mxd文档的IO操作
        private void 保存地图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 新建的地图，则执行“另存为”
            if (string.IsNullOrEmpty(m_mapDocumentName))
            {
                另存地图ToolStripMenuItem_Click(sender, e); // 调用另存为事件
                return;
            }

            // 已经有路径，直接覆盖保存
            try
            {
                // 1. 创建地图文档对象
                IMapDocument mapDoc = new MapDocumentClass();

                // 2. 打开现有文档
                mapDoc.Open(m_mapDocumentName, "");

                // 3. 将 MapControl 的当前状态替换进去
                mapDoc.ReplaceContents((IMxdContents)axMapControl1.Map);

                // 4. 保存更改
                mapDoc.Save(mapDoc.UsesRelativePaths, true);
                mapDoc.Close();

                MessageBox.Show("保存成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message);
            }
        }

        private void 加载地图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "打开地图文档";
            openFileDialog.Filter = "地图文档 (*.mxd)|*.mxd";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                // 检查文件是否有效
                if (axMapControl1.CheckMxFile(filePath))
                {
                    // 1. 加载地图
                    axMapControl1.LoadMxFile(filePath);

                    // 2. 记录当前文件路径
                    m_mapDocumentName = filePath;
                }
                else
                {
                    MessageBox.Show("无效的地图文档文件！");
                }
            }
        }

        private void 新建地图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 提示用户保存当前地图
            if (!string.IsNullOrEmpty(m_mapDocumentName))
            {
                MessageBox.Show("建议先保存当前的文档！！！");
            }

            try
            {
                // 1. 处理主地图控件 (MapControl)
                axMapControl1.ClearLayers();
                axMapControl1.Map.SpatialReference = null; // 重置坐标系
                axMapControl1.Map.MapUnits = esriUnits.esriUnknownUnits; // 重置单位

                // 清除主地图上的所有临时标注或高亮图形
                IActiveView activeViewMain = axMapControl1.ActiveView;
                activeViewMain.GraphicsContainer.DeleteAllElements();
                activeViewMain.Refresh();

                // 2. 处理鹰眼控件
                EagleEyeMapControl.ClearLayers();

                // 鹰眼上有红色的矩形框，通过 GraphicsContainer 清除
                IActiveView activeViewEagle = EagleEyeMapControl.ActiveView;
                activeViewEagle.GraphicsContainer.DeleteAllElements();
                activeViewEagle.Refresh();

                // 3. 处理布局控件 (PageLayoutControl)
                // 包含：MapFrame、Legend、指北等
                IPageLayout pageLayout = axPageLayoutControl1.PageLayout;
                IGraphicsContainer containerLayout = pageLayout as IGraphicsContainer;

                // 使用 Reset 和 Next 遍历所有元素
                containerLayout.Reset();
                IElement element = containerLayout.Next();
                List<IElement> elementsToDelete = new List<IElement>();

                while (element != null)
                {
                    // 如果是 MapFrame，保留，但要清空里面的图层
                    if (element is IMapFrame)
                    {
                        IMapFrame mapFrame = element as IMapFrame;
                    }
                    else
                    {
                        // 如果是 图例、指北针、文字 等，加入待删除列表
                        elementsToDelete.Add(element);
                    }
                    element = containerLayout.Next();
                }

                // 执行删除
                foreach (var delEle in elementsToDelete)
                {
                    containerLayout.DeleteElement(delEle);
                }

                axPageLayoutControl1.ActiveView.Refresh();

                // 4. 重置全局变量和 TOC
                m_mapDocumentName = string.Empty;

                // 重新设置 TOC 的伙伴控件，确保它指向正确的主地图
                axTOCControl1.SetBuddyControl(axMapControl1);
                axTOCControl1.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show("新建地图时出错：" + ex.Message);
            }
        }

        private void 另存地图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "另存为";
            saveFileDialog.Filter = "地图文档 (*.mxd)|*.mxd";
            saveFileDialog.OverwritePrompt = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    // 1. 创建地图文档对象
                    IMapDocument mapDoc = new MapDocumentClass();

                    // 2. 创建新文件
                    mapDoc.New(filePath);

                    // 3. 将 MapControl 中的当前内容替换到文档中
                    mapDoc.ReplaceContents((IMxdContents)axMapControl1.Map);

                    // 4. 保存并关闭
                    mapDoc.Save(mapDoc.UsesRelativePaths, true);
                    mapDoc.Close();

                    // 5. 更新当前路径变量
                    m_mapDocumentName = filePath;

                    MessageBox.Show("保存成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败：" + ex.Message);
                }
            }
        }
        #endregion

        #region 导出地图：两个选项
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
        #endregion

        #region 数据载入选项卡，调用多种方法载入数据
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
            openFileDialog1.Filter = "所有支持格式|*.tif;*.img;*.jpg;*.bmp|TIFF文件(*.tif)|*.tif|ERDAS IMG(*.img)|*.img";
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
            form.BuddyMap = axMapControl1;
            form.Show();
        }
        #endregion

        #region 要素选择选项卡
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
        }

        private void 清楚选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //清除要素
            ICommand cmd = new ControlsClearSelectionCommandClass();
            cmd.OnCreate(axMapControl1.Object);
            cmd.OnClick();
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
                ICommand pPanTool = new ESRI.ArcGIS.Controls.ControlsMapPanToolClass();
                pPanTool.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = pPanTool as ITool;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 地图量测选项卡

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

        #region 查询统计选项卡
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

        #region 创建要素类-自定义方法
        private void btnCreate_Click(object sender, EventArgs e)
        {
            // 1. 获取要素类名称 (使用 InputBox)
            string featureClassName = ShowInputBox("请输入要素类名称", "MyPoints");
            if (string.IsNullOrEmpty(featureClassName)) return;

            // 2. 选择存储位置 (MDB 文件 或 GDB/SHP 文件夹)
            string workspacePath = "";
            bool isMDB = false; // 标记是否为 Access 数据库

            // 询问用户类型
            DialogResult dr = MessageBox.Show(
                "您想保存在 Access (.mdb) 数据库中吗？\n\n" +
                "是(Yes) -> 选择 .mdb 文件\n" +
                "否(No)  -> 选择文件夹 (用于创建 Shapefile 或 GDB)",
                "选择存储类型", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (dr == DialogResult.Cancel) return;

            if (dr == DialogResult.Yes)
            {
                // 模式 A: 选择 .mdb 文件
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.Filter = "Access Geodatabase (*.mdb)|*.mdb";
                openDlg.Title = "选择目标 MDB 数据库";
                if (openDlg.ShowDialog() != DialogResult.OK) return;

                workspacePath = openDlg.FileName;
                isMDB = true;
            }
            else
            {
                // 模式 B: 选择文件夹 (GDB 或 SHP)
                FolderBrowserDialog folderDlg = new FolderBrowserDialog();
                folderDlg.Description = "请选择目标位置：\n1. 选择 .gdb 文件夹 -> 创建 GDB 要素\n2. 选择普通文件夹 -> 创建 Shapefile";
                if (folderDlg.ShowDialog() != DialogResult.OK) return;

                workspacePath = folderDlg.SelectedPath;
            }

            // 3. 执行创建逻辑 (含名称清洗)
            try
            {
                // A. 打开工作空间
                IFeatureWorkspace fws = OpenSmartWorkspace(workspacePath);
                if (fws == null) return;

                // B. 根据数据库类型清洗名称
                // GDB/MDB 和 Shapefile 对名字的要求截然不同，这里进行预处理

                if (isMDB || workspacePath.ToLower().EndsWith(".gdb"))
                {
                    // --- GDB/MDB 规则 ---
                    // 1. 不能有扩展名
                    if (featureClassName.ToLower().EndsWith(".shp"))
                    {
                        featureClassName = System.IO.Path.GetFileNameWithoutExtension(featureClassName);
                    }

                    // 2. 名称中不能有空格
                    featureClassName = featureClassName.Replace(" ", "_");
                }
                else
                {
                    // 必须带 .shp 后缀
                    if (!featureClassName.ToLower().EndsWith(".shp"))
                    {
                        featureClassName += ".shp";
                    }
                }

                // C. 调用创建方法
                IFeatureClass newClass = CreateGenericFeatureClass(featureClassName, null, fws);

                MessageBox.Show("创建成功！\n\n存储位置：" + workspacePath + "\n要素类名：" + newClass.AliasName);

                // D. (可选) 加载到地图
                LoadToMap(newClass);
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建失败：\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private IFeatureWorkspace OpenSmartWorkspace(string path)
        {
            IWorkspaceFactory workspaceFactory = null;
            string pathLower = path.ToLower();

            try
            {
                if (pathLower.EndsWith(".mdb") && File.Exists(path))
                {
                    // Access MDB
                    workspaceFactory = new AccessWorkspaceFactoryClass();
                }
                else if (pathLower.EndsWith(".gdb") && Directory.Exists(path))
                {
                    // File GDB
                    workspaceFactory = new FileGDBWorkspaceFactoryClass();
                }
                else if (Directory.Exists(path))
                {
                    // 普通文件夹 -> Shapefile
                    workspaceFactory = new ShapefileWorkspaceFactoryClass();
                }
                else
                {
                    MessageBox.Show("无效的路径或不支持的格式！");
                    return null;
                }

                return workspaceFactory.OpenFromFile(path, 0) as IFeatureWorkspace;
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开工作空间出错：" + ex.Message);
                return null;
            }
        }

        public IFeatureClass CreateGenericFeatureClass(string featureClassName, UID classExtensionUID, IFeatureWorkspace featureWorkspace)
        {
            // A. 定义字段
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            // 1. 定义几何字段 (Shape)
            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;

            // 空间参考 (WGS84)
            ISpatialReferenceFactory srFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference sr = srFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

            // 设置容差和精度 (GDB 必需，否则会创建失败)
            ISpatialReferenceResolution srRes = (ISpatialReferenceResolution)sr;
            srRes.ConstructFromHorizon();
            ISpatialReferenceTolerance srTol = (ISpatialReferenceTolerance)sr;
            srTol.SetDefaultXYTolerance();

            geometryDefEdit.SpatialReference_2 = sr;

            IField geometryField = new FieldClass();
            ((IFieldEdit)geometryField).Name_2 = "Shape";
            ((IFieldEdit)geometryField).Type_2 = esriFieldType.esriFieldTypeGeometry;
            ((IFieldEdit)geometryField).GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(geometryField);

            // 2. 自定义字段
            IField nameField = new FieldClass();
            ((IFieldEdit)nameField).Name_2 = "Name";
            ((IFieldEdit)nameField).Type_2 = esriFieldType.esriFieldTypeString;
            ((IFieldEdit)nameField).Length_2 = 50;
            fieldsEdit.AddField(nameField);

            // B. 字段验证 (FieldChecker)
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            // C. 提取字段并剔除 OID
            IFields finalFields = new FieldsClass();
            IFieldsEdit finalFieldsEdit = (IFieldsEdit)finalFields;
            string shapeFieldName = "Shape";

            // 遍历验证后的字段，剔除 OID
            for (int i = 0; i < validatedFields.FieldCount; i++)
            {
                IField currentField = validatedFields.get_Field(i);

                // 跳过 OID 字段 (防止重复创建)
                if (currentField.Type == esriFieldType.esriFieldTypeOID) continue;

                // 记录真实的几何字段名
                if (currentField.Type == esriFieldType.esriFieldTypeGeometry)
                    shapeFieldName = currentField.Name;

                IClone fieldClone = currentField as IClone;
                finalFieldsEdit.AddField((IField)fieldClone.Clone());
            }

            // D. 处理文件名后缀
            string cleanName = featureClassName;
            IWorkspace workspace = featureWorkspace as IWorkspace;

            // 处理重名和非法字符
            if (workspace.Type == esriWorkspaceType.esriFileSystemWorkspace)
            {
                // Shapefile
                if (!cleanName.ToLower().EndsWith(".shp")) cleanName += ".shp";
            }
            else
            {
                // GDB/MDB
                if (cleanName.ToLower().EndsWith(".shp"))
                    cleanName = System.IO.Path.GetFileNameWithoutExtension(cleanName);

                cleanName = cleanName.Replace(" ", "_").Replace("-", "_");

                // 防止数字开头
                if (cleanName.Length > 0 && char.IsDigit(cleanName[0]))
                    cleanName = "F_" + cleanName;
            }

            // E. 检查并删除已存在的同名要素 (防止报错)
            IWorkspace2 workspace2 = workspace as IWorkspace2;
            if (workspace2 != null && workspace2.get_NameExists(esriDatasetType.esriDTFeatureClass, cleanName))
            {
                    IFeatureClass oldFc = featureWorkspace.OpenFeatureClass(cleanName);
                    ((IDataset)oldFc).Delete();
            }

            // F. 执行创建
            return featureWorkspace.CreateFeatureClass(cleanName, finalFields, null, classExtensionUID, esriFeatureType.esriFTSimple, shapeFieldName, "");
        }

        // 将创建好的要素类加载到地图
        private void LoadToMap(IFeatureClass featureClass)
        {
            if (featureClass == null) return;

            try
            {
                // 1. 创建特征图层 (FeatureLayer)
                IFeatureLayer featureLayer = new FeatureLayerClass();
                featureLayer.FeatureClass = featureClass;

                // 设置图层在 TOC 中显示的名字
                featureLayer.Name = featureClass.AliasName;

                // 2. 添加到地图控件
                // 注意：axMapControl1 是你主窗体上的地图控件名
                axMapControl1.AddLayer(featureLayer);

                // 3. 刷新视图，让它显示出来
                axMapControl1.ActiveView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("自动加载图层失败：" + ex.Message);
            }
        }

        // 简易 InputBox 实现
        public static string ShowInputBox(string title, string defaultText)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = "请输入名称:";
            textBox.Text = defaultText;

            buttonOk.Text = "确定";
            buttonCancel.Text = "取消";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            return dialogResult == DialogResult.OK ? textBox.Text : null;
        }
        #endregion

        #region 编辑选项卡

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

            // 下拉框，让它始终可选
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
        #endregion


        #region 主窗体 鼠标点击事件
        //鼠标点击
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
        #region 数据视图与鹰眼的同步
        //鹰眼绘制图形
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
            if (EagleEyeMapControl.Map.LayerCount > 0) // 有图层数据的情況
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
                    IEnvelope pEnvelope = EagleEyeMapControl.TrackRectangle();
                    IPoint pTempPoint = new PointClass();
                    pTempPoint.PutCoords(pEnvelope.XMin + pEnvelope.Width / 2, pEnvelope.YMin + pEnvelope.Height / 2);
                    EagleEyeMapControl.Extent = pEnvelope;
                    // 矩形框的高宽和数据视图的高宽不一定成正比，这里做一个中心调整；
                    EagleEyeMapControl.CenterAt(pTempPoint);
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

            //调试用
            //MessageBox.Show("ALL DONE!\n数据加载成功！");
        }
        
        //同步数据视图到鹰眼中
        private void SynchronizeAxMapControl2()
        {
            //先清除已有图层
            if (EagleEyeMapControl.LayerCount > 0 )
            {
                EagleEyeMapControl.ClearLayers();
            }
            EagleEyeMapControl.SpatialReference = axMapControl1.SpatialReference;
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
                            EagleEyeMapControl.AddLayer(pSubLayer);
                        }
                    }
                }
                else
                {
                    //非图层组，直接遍历
                    IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                    if (pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint && pFeatureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryMultipoint)
                    {
                        EagleEyeMapControl.AddLayer(pLayer);
                    }
                }
                EagleEyeMapControl.Extent = axMapControl1.FullExtent;
                pEnv = axMapControl1.Extent as IEnvelope;
                DrawRectangle(pEnv);
                EagleEyeMapControl.ActiveView.Refresh();
            }
        }

        //绘制矩形框的方法
        private void DrawRectangle(IEnvelope pEnv)
        {
            //清除鹰眼控件的图形元素
            IGraphicsContainer pGraphicsContainer = EagleEyeMapControl.Map as IGraphicsContainer;
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
                EagleEyeMapControl.MousePointer = esriControlsMousePointer.esriPointerHand;
            }
            else
            {
                // 鼠标在其他位置，显示默认样式
                EagleEyeMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
            }

            // 2. 拖动逻辑
            if (bCanDrag) 
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

                this.axMapControl1.Extent = pEnv;

                // 刷新主地图，只刷新地理要素
                this.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);

                // 刷新鹰眼视图的图形层（显示移动后的矩形）
                EagleEyeMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
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
        #endregion

        #region 数据视图与布局视图的同步
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
        #endregion

        #region TOC 内容列表：符号选择、顺序的调整、双击显示符号（左键）、显示菜单（右键）
        //左键双击设置符号系统
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
                        // 先设置新的符号
                        if (SymbolSelectorFrm.pSymbol != null)
                        {
                            pLegendClass.Symbol = SymbolSelectorFrm.pSymbol;
                        }
                        axMapControl1.ActiveView.ContentsChanged();
                        // 刷新地理要素层
                        axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                        // 刷新 TOC 列表，使左侧图标更新
                        axTOCControl1.Update();
                    }
                }
            }
        }

        //左右键单击事件，右键弹出更多内容
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

        #region TOC 内容列表2:右击图层菜单的5个事件
        //右击属性表
        private void btnAttribute_Click(object sender, EventArgs e)
        {
            if (frmattribute == null || frmattribute.IsDisposed)
            {
                frmattribute = new 自定义窗体控件_FormAttribute.FormAttribute();
            }
            frmattribute.CurFeatureLayer = pTocFeatureLayer;
            frmattribute.m_activeView = axMapControl1.ActiveView;//传入视图
            frmattribute.InitUI();
            frmattribute.ShowDialog();
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
            DialogResult result = MessageBox.Show("删除？" + pTocFeatureLayer.Name, "提示",
            MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
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

        #region 任务一：分级色彩符号功能
        private void 分级符号ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ////窗体构造函数中直接传入map和toc
            FormSymbolize frm = new FormSymbolize(this.axMapControl1.Object as IMapControl3, this.axTOCControl1.Object as ITOCControl2);
            frm.ShowDialog(); 
        }
        #endregion

        #region 任务二：地图打印功能
        private void 地图打印ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. 检查是否有数据
            if (axMapControl1.LayerCount == 0)
            {
                MessageBox.Show("当前没有地图数据，无法打印！");
                return;
            }

            // 2. 实例化预览窗体，并传入当前的 MapControl
            FormPrintPreview previewForm = new FormPrintPreview(axMapControl1.Object as MapControl);

            // 3. 显示窗体
            previewForm.ShowDialog();
        }
        #endregion

        //Bug修复：解决ToolBar对应的Buddy动态切换，通过Tab的属性事件来实现，解决Toolbar的冲突问题
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                // 1. 切回数据视图：把工具条绑定给 MapControl
                axToolbarControl1.SetBuddyControl(axMapControl1);
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                // 2. 切回布局视图：把工具条绑定给 PageLayoutControl
                axToolbarControl1.SetBuddyControl(axPageLayoutControl1);
            }
        }
    }
}
