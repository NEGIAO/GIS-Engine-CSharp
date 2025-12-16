using System; 
using System.Collections.Generic; 
using System.Linq;
using System.Text; 
using ESRI.ArcGIS.SystemUI; 
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls; 
using System.Windows.Forms; 
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;

namespace GIS_2310130172_Engine
{
    class StopEditCommandClass:ICommand
    {
        private IMap m_Map = null; private bool bEnable = true;
        private IActiveView m_activeView = null;
        private IHookHelper m_hookHelper = null;
        private IEngineEditor m_EngineEditor = null;

        #region ICommand 成员
        public int Bitmap { get { return -1; } }
        public string Caption { get { return "停止编辑"; } }
        public string Category { get { return "编辑按钮"; } }
        public bool Checked { get { return false; } }
        public bool Enabled { get { return bEnable; } }
        public int HelpContextID { get { return -1; } }
        public string HelpFile { get { return ""; } }
        public string Message { get { return "停止编辑"; } }
        public string Name { get { return "StopEditCommand"; } }

        public void OnClick() 
        {
            m_Map = m_hookHelper.FocusMap; m_activeView = m_Map as IActiveView;
            m_EngineEditor = MapManager.EngineEditor; Boolean bSave = true;
            if (m_EngineEditor == null) return;
            if (m_EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)return;
            IWorkspaceEdit2 pWsEdit2 = m_EngineEditor.EditWorkspace as IWorkspaceEdit2;
            if (pWsEdit2 == null) return;
            if (pWsEdit2.IsBeingEdited())
            {
                Boolean bHasEdit = m_EngineEditor.HasEdits();
                if (bHasEdit) 
                {
                    if (MessageBox.Show("是否保存所做的编辑？ ", "提示", MessageBoxButtons.YesNo,MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        bSave = true; 
                    }
                    else 
                    {
                        bSave = false; 
                    }
                }
                m_EngineEditor.StopEditing(bSave);
            }
            m_Map.ClearSelection(); m_activeView.Refresh();
        }

        public void OnCreate(object Hook)
        {
            if (Hook == null) return;
            try 
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = Hook;
                if (m_hookHelper.ActiveView == null)
                    m_hookHelper = null;
            }
            catch 
            {
                m_hookHelper = null; 
            }
            if (m_hookHelper == null) 
                bEnable = false;
            else 
                bEnable = true;
        }
        public string Tooltip { get { return "停止编辑"; } }
        #endregion
    }
}
