using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using System; 
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses; 
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;

namespace 自定义DLL_SymbologyMenu
{
    /// <summary>
    /// Command that works in ArcMap/Map/PageLayout
    /// </summary>
    [Guid("e930e13b-1bc9-4f52-9834-175cd9300ef2")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SymbologyMenu.DotDensityRender")]
    public sealed class DotDensityRender : BaseCommand
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
            MxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper = null;
        public DotDensityRender()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Symbology.Item"; //localizable text
            base.m_caption = "DotDensityRender";  //localizable text 
            base.m_message = "DotDensityRender";  //localizable text
            base.m_toolTip = "点密度图";  //localizable text
            base.m_name = "DotDensityRender";   //unique id, non-localizable (e.g. "MyCategory_MyCommand")

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
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                if (m_hookHelper.ActiveView == null)
                    m_hookHelper = null;
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            try {
                    string strPopField = "OBJECTID"; //字段
                    IActiveView pActiveView = m_hookHelper.ActiveView;//活动视图
                    IMap pMap = m_hookHelper.FocusMap;//地图
                    IGeoFeatureLayer pGeoFeatureL = pMap.get_Layer(0) as IGeoFeatureLayer;//要素图层
                    IDotDensityRenderer pDotDensityRenderer = new DotDensityRendererClass(); //渲染
                    IRendererFields pRendererFields = (IRendererFields)pDotDensityRenderer; //渲染字段
                    pRendererFields.AddField(strPopField, strPopField);
                    IDotDensityFillSymbol pDotDensityFillS = new DotDensityFillSymbolClass();
                    pDotDensityFillS.DotSize = 5;
                    pDotDensityFillS.Color = GetRGB(0, 0, 0);
                    pDotDensityFillS.BackgroundColor = GetRGB(239, 228, 190);
                    ISymbolArray pSymbolArray = (ISymbolArray)pDotDensityFillS;
                    ISimpleMarkerSymbol pSimpleMarkerS = new SimpleMarkerSymbolClass(); //形状 symbol
                    pSimpleMarkerS.Style = esriSimpleMarkerStyle.esriSMSCircle;
                    pSimpleMarkerS.Size = 5;
                    pSimpleMarkerS.Color = GetRGB(128, 128, 255);
                    pSymbolArray.AddSymbol((ISymbol)pSimpleMarkerS);
                    pDotDensityRenderer.DotDensitySymbol = pDotDensityFillS;
                    pDotDensityRenderer.DotValue = 0.5;
                    pDotDensityRenderer.CreateLegend();
                    pGeoFeatureL.Renderer = (IFeatureRenderer)pDotDensityRenderer;
                    pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                    MessageBox.Show("debug");
                }
                catch (Exception ex) 
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString(), "错误信息提示", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Error); 
                } 
                }
                private IRgbColor GetRGB(int red, int green, int blue)
                {
                    IRgbColor rgbColor = new RgbColorClass();
                    rgbColor.Red = red; rgbColor.Green = green;
                    rgbColor.Blue = blue; return rgbColor; 
                }
        }

        #endregion
    }
