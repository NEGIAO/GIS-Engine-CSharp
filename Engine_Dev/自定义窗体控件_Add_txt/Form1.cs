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
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Controls;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;


namespace 自定义窗体控件_Add_txt
{
    /// <summary>
    /// buddy控件属性
    /// 打开按钮，获取txt路径及点信息
    /// 保存按钮，获取SHP文件保存路径
    /// 创建按钮，生成并加载SHP数据到主窗体MapContrl控件当中
    /// 取消按钮，退出当前对话框
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();//自动形成相应语句，初始化函数
        }
        //伙伴控件属性：私有字段、公开属性
        private AxMapControl buddyMap;
        public AxMapControl BuddyMap
        {
            get { return buddyMap; }
            set { buddyMap = value; }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("OH!SEE YOU NEXT TIME!");
            this.Close();
        }
        //txt的打开按钮
        private void button1_Click(object sender, System.EventArgs e)
        {
            //获取txt路径，并读取txt内部的点信息并保存
            OpenFileDialog pOFD = new OpenFileDialog();
            pOFD.Multiselect = false;
            pOFD.Title = "打开坐标文件";
            pOFD.Filter = "坐标文件(*.txt)|*.txt";
            pOFD.InitialDirectory = Directory.GetCurrentDirectory();
            if (pOFD.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = pOFD.FileName;
            }
            //读取数据并保存
            pList = GetPoints(textBox1.Text);
           
        }

        //点结构，存储点数据
        public struct CPoint
        {
            public string Name;
            public double X;
            public double Y;
        }
        List<CPoint> pList = new List<CPoint>();
        List<string> pStr = new List<string>();

        public List<CPoint> GetPoints(string DataFullName)
        {
            // 在 try 块开始前声明并初始化 pList
            List<CPoint> pList = new List<CPoint>();

            try
            {

                //常用分割符号
                char[] charArray = new char[] { ',', ' ', '\t' };
                //读取文件信息，FileStream打开文件并读取
                FileStream fs = new FileStream(DataFullName, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                string strLine = sr.ReadLine();
                if (strLine != null)
                {
                    string[] strArray = strLine.Split(charArray);
                    if (strArray.Length > 0)
                    {
                        for (int i = 0; i < strArray.Length; i++)
                        {

                            pStr.Add(strArray[i]);
                        }
                        while ((strLine = sr.ReadLine()) != null)
                        {
                            //获取点信息
                            strArray = strLine.Split(charArray);
                            CPoint pCPoint = new CPoint();
                            pCPoint.Name = strArray[0];
                            pCPoint.X = Convert.ToDouble(strArray[1]);
                            pCPoint.Y = Convert.ToDouble(strArray[2]);
                            pList.Add(pCPoint);
                        }
                    }
                    else { return null; }
                    sr.Close();
                    return pList;
                }

                // 修正点：确保当文件为空 (strLine == null) 时有返回值
                sr.Close();
                return pList;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

        }

        //保存按钮，获取SHP文件的保存路径
        private void button2_Click(object sender, System.EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SHP文件（*.shp)|*.shp";
            if (File.Exists(textBox1.Text))
            {
                saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(textBox1.Text);
            }
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = saveFileDialog.FileName;
            }
        }

        //创建按钮，生成SHP图层，并加载到主窗体MapControl控件当中
        private void button3_Click(object sender, System.EventArgs e)
        {
            if (pList == null )
            {
                MessageBox.Show("文件为空，重新选择");
            }
            else
            {
                //生成SHP文件，并加载到MapControl控件当中


                //生成SHP Layer
                IFeatureLayer shpLayer = CreateSHPLayer(pList,textBox2.Text);

                IMap pMap = new MapClass();
                pMap.Name = "Map";
                buddyMap.DocumentFilename = string.Empty;
                buddyMap.Map = pMap;
                buddyMap.Map.AddLayer(shpLayer);
                buddyMap.ActiveView.Refresh();
                MessageBox.Show("OH MY GOD!\nYOU GOT IT!\nAWESOME!\nNICE!");
                this.Close();
            }
        }

        private IFeatureLayer CreateSHPLayer(System.Collections.Generic.List<CPoint> pList, string p)
        {
            int index = p.LastIndexOf('\\');
            string folder = p.Substring(0, index);
            string shpname = p.Substring(index + 1);
            //生成图层的时候，从工作空间入手
            IWorkspaceFactory pwsf = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace pfws = (IFeatureWorkspace)pwsf.OpenFromFile(folder,0);
            //字段集合
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;
            //几何字段
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "Shape";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            IGeometryDefEdit pGeometryDefEdit = new GeometryDefClass();
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            //定义坐标系:biejing 54
            ISpatialReferenceFactory pSRF = new SpatialReferenceEnvironmentClass();
            ISpatialReference pSR = pSRF.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_Beijing1954);
            pGeometryDefEdit.SpatialReference_2 = pSR;
            pFieldEdit.GeometryDef_2 = pGeometryDefEdit;
            //将几何字段添加到字段几何当中
            pFieldsEdit.AddField(pField);
            IFeatureClass pFeatureClass = pfws.CreateFeatureClass(shpname,pFields,null,null,esriFeatureType.esriFTSimple,"Shape",null);
            IPoint pPoint = new PointClass();
            for (int i = 0; i < pList.Count; i++)
            {
                pPoint.X = pList[i].X;
                pPoint.Y = pList[i].Y;

                //创建单个要素
                IFeature pFeature = pFeatureClass.CreateFeature();
                pFeature.Shape = pPoint;
                pFeature.Store();
            }
            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureClass;
            pFeatureLayer.Name = shpname;

            return pFeatureLayer;
        }
    }
}
