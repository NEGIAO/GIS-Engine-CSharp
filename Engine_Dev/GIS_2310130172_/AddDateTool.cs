using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.esriSystem;

namespace GIS_2310130172_Engine
{
    /// <summary>
    /// Command that works in ArcMap/Map/PageLayout, ArcScene/SceneControl
    /// or ArcGlobe/GlobeControl
    /// </summary>
    [Guid("9f0f5f2a-d60a-4109-a641-e70edc7272a3")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS_2310130172_.ClearCurrentActiveToolCmd")]
    public sealed class AddDateTool : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GMxCommands.Register(regKey);
            MxCommands.Register(regKey);
            SxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GMxCommands.Unregister(regKey);
            MxCommands.Unregister(regKey);
            SxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper = null;

        //定义两个变量
        AxToolbarControl toolbar1;
        AxPageLayoutControl pagelayout;

        public AddDateTool(AxToolbarControl axtoolbar,AxPageLayoutControl axpagelayout)
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Custom Command"; //localizable text
            base.m_caption = "添加日期元素(+_+)";  //localizable text
            base.m_message = "添加日期元素";   //localizable text 
            base.m_toolTip = "添加日期元素";  //localizable text 
            base.m_name = "AddDataTool";   //unique id, non-localizable (e.g. "MyCategory_MyCommand")

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }

            //传入控件参数
            toolbar1 = axtoolbar;
            pagelayout = axpagelayout;
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (m_hookHelper == null)
                m_hookHelper = new HookHelperClass();
            m_hookHelper.Hook = hook;
            toolbar1.SetBuddyControl(pagelayout);//设置伙伴控件
        }
        //重写enabled属性，判断添加日期元素工具是否激活
        public override bool Enabled
        {
            get
            {
                if (m_hookHelper.ActiveView != null)
                {
                    return true;
                }
                return false;
            }
        }
        //
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            base.OnMouseDown(Button, Shift, X, Y); // 保持调用基类方法
            //获取活动试图
            IActiveView activeView = m_hookHelper.ActiveView;
            //创建新的文本元素
            ITextElement textElement = new TextElementClass();
            //创建文本符号
            ITextSymbol textSymbol = new TextSymbolClass();
            textSymbol.Size = 25;
            //设置文本元素属性
            textElement.Symbol = textSymbol;
            textElement.Text = DateTime.Now.ToShortDateString();
            //对IElement QI
            IElement element = textElement as IElement;
            //创建点
            IPoint point = new PointClass();
            point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            //设置元素属性
            element.Geometry = point;
            //增加元素到图形绘制容器
            activeView.GraphicsContainer.AddElement(element, 0);
            //刷新图形
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        #endregion
    }
}
