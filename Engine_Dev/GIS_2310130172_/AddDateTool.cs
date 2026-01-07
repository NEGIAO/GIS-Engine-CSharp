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
            ArcGISCategoryRegistration(registerType);
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            ArcGISCategoryUnregistration(registerType);
        }

        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GMxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GMxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }
        #endregion

        // 使用 HookHelper 来管理与地图/布局的交互
        private IHookHelper m_hookHelper = null;

        // 构造函数不需要传控件，只做基本属性设置,否则会和toolbar冲突
        public AddDateTool()
        {
            base.m_category = "Custom Command";
            base.m_caption = "添加日期元素";
            base.m_message = "点击视图添加当前日期";
            base.m_toolTip = "添加日期元素";
            base.m_name = "AddDataTool";

            // 设置光标
            base.m_cursor = System.Windows.Forms.Cursors.Cross;
        }

        public override void OnCreate(object hook)
        {
            if (m_hookHelper == null)
                m_hookHelper = new HookHelperClass();

            // 只设置 Hook，不在这里 SetBuddyControl
            m_hookHelper.Hook = hook;
        }

        public override bool Enabled
        {
            get
            {
                // 确保 Hook 有效且 ActiveView 不为空
                if (m_hookHelper != null && m_hookHelper.ActiveView != null)
                {
                    return true;
                }
                return false;
            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // 确保是左键点击
            if (Button != 1) return;

            //确保仅pagelayout中使用,否则作提示
            IActiveView activeView = m_hookHelper.ActiveView;
            if (!(activeView is IPageLayout))
            {
                MessageBox.Show("此工具仅在【布局视图】下可用！");
                return;
            }

            try
            {
                if (activeView == null) return;

                ITextElement textElement = new TextElementClass();
                ITextSymbol textSymbol = new TextSymbolClass();

                // 简单的字体设置，防止默认字体报错
                stdole.IFontDisp font = new stdole.StdFontClass() as stdole.IFontDisp;
                font.Name = "Arial";
                font.Size = 12;
                textSymbol.Font = font;
                textSymbol.Size = 25;

                textElement.Symbol = textSymbol;
                textElement.Text = DateTime.Now.ToShortDateString();

                IElement element = textElement as IElement;

                IPoint point = activeView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                element.Geometry = point;

                activeView.GraphicsContainer.AddElement(element, 0);
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加日期失败: " + ex.Message);
            }
        }
    }
}