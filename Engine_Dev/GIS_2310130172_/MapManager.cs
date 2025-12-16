using System; 
using System.Collections.Generic; 
using System.Linq;
using System.Text; 
using ESRI.ArcGIS.Carto; 
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display; 
using ESRI.ArcGIS.Geodatabase; 
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;

namespace GIS_2310130172_Engine
{
    class MapManager
    {
        public MapManager(){}
        private static IEngineEditor _engineEditor;
        public static IEngineEditor EngineEditor
        { 
            get { return MapManager._engineEditor; }
            set { MapManager._engineEditor = value; } 
        }

        //根据图层名获取图层
        public static ILayer GetLayerByName(IMap pMap, string sLyrName)
        {
            ILayer pLyr = null; ILayer pLayer = null;
            try
            {
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    pLyr = pMap.get_Layer(i);
                    if (pLyr.Name.ToUpper() == sLyrName.ToUpper())
                    {
                        pLayer = pLyr; 
                        break; 
                    }
                }
            } 
            catch (Exception ex){}
            return pLayer;
        }

        //获取当前地图文档所有图层集合
        public static List<ILayer> GetLayers(IMap pMap)
        {
            ILayer plyr = null; List<ILayer> pLstLayers = null;
            try
            {
                pLstLayers = new List<ILayer>();
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    plyr = pMap.get_Layer(i);
                    if (!pLstLayers.Contains(plyr))
                    {
                        pLstLayers.Add(plyr);
                    }
                }
            }
            catch (Exception ex) { }
            return pLstLayers;
        }
    }
}
