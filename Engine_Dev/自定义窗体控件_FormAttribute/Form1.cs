using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace 自定义窗体控件_FormAttribute
{
    public partial class FormAttribute : Form
    {
        public FormAttribute()
        {
            InitializeComponent();
        }

        //图层属性的生成
        private IFeatureLayer _curFeatureLayer;
        public IFeatureLayer CurFeatureLayer
        {
            get { return _curFeatureLayer; }
            set { _curFeatureLayer = value; }
        }

        public void InitUI()
        {
            if (_curFeatureLayer == null) return;
            IFeature pFeature = null;
            DataTable pFeatDT = new DataTable();
            DataRow pDataRow = null;
            DataColumn pDataColum = null;
            IField pField = null;
            for (int i = 0; i < _curFeatureLayer.FeatureClass.Fields.FieldCount; i++)
            {
                pDataColum = new DataColumn();
                pField = _curFeatureLayer.FeatureClass.Fields.get_Field(i);
                pDataColum.ColumnName = pField.AliasName;//列名等于字段名
                pDataColum.DataType = Type.GetType("System.Object");
                pFeatDT.Columns.Add(pDataColum);
            }
            IFeatureCursor pFeatureCursor = _curFeatureLayer.Search(null, true);
            pFeature = pFeatureCursor.NextFeature();
            while (pFeature != null)
            {
                pDataRow = pFeatDT.NewRow();
                for (int k = 0; k < pFeatDT.Columns.Count; k++)
                {
                    pDataRow[k] = pFeature.get_Value(k);
                }
                pFeatDT.Rows.Add(pDataRow);
                pFeature = pFeatureCursor.NextFeature();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
            dataGridAttribute.DataSource = pFeatDT;//绑定数据源
        }
    }
}
