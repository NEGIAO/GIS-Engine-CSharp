using System;
//using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace DLL_符号化
{
    public class SymbologyHelper
    {
        // 定义颜色方案枚举
        public enum ColorStyle
        {
            Reds,       // 浅红 -> 深红
            Greens,     // 浅绿 -> 深绿
            Blues,      // 浅蓝 -> 深蓝
            Heatmap,    // 浅黄 -> 深红
            Spectral    // 蓝 -> 绿 -> 黄 -> 红
        }

        // 定义分级方法枚举
        public enum ClassifyMethod
        {
            EqualInterval, // 等距
            NaturalBreaks  // 自然间断
        }

        /// 执行分级渲染
        public static void RenderClassBreaks(IFeatureLayer featureLayer, string fieldName, int breakCount, ClassifyMethod method, ColorStyle colorStyle)
        {
            // 1. 基础校验
            if (featureLayer == null) throw new ArgumentNullException("图层不能为空");
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException("字段名不能为空");

            IFeatureClass featureClass = featureLayer.FeatureClass;
            ITable table = featureClass as ITable;

            // 2. 检查表是否为空
            if (table.RowCount(null) == 0)
            {
                throw new Exception("图层属性表中没有数据，无法渲染。");
            }

            // 3. 使用 DataStatistics 检查数据质量
            ICursor cursor = table.Search(null, false); // 获取游标
            IDataStatistics dataStatistics = new DataStatisticsClass();
            dataStatistics.Field = fieldName;
            dataStatistics.Cursor = cursor;

            IStatisticsResults statistics = dataStatistics.Statistics;

            // 检查 A: 是否全是 Null
            if (statistics.Count == 0)
            {
                // 释放游标
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                throw new Exception(string.Format("字段 '{0}' 的值全部为空(Null)，无法进行分级。", fieldName));
            }

            // 检查 B: 是否所有值都相同 
            if (statistics.Minimum == statistics.Maximum)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                throw new Exception(string.Format(
                    "无法分级：字段 '{0}' 的所有值都相同（均为 {1}）。\n" +
                    "分级渲染要求数据至少有两个不同的值。",
                    fieldName, statistics.Maximum));
            }

            // 释放游标，DataStatistics 任务完成
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

            // 4. 数据体检通过，开始正式计算直方图
            IBasicHistogram basicHistogram = new BasicTableHistogramClass();
            ITableHistogram tableHistogram = (ITableHistogram)basicHistogram;
            tableHistogram.Table = table;
            tableHistogram.Field = fieldName;

            object dataValues, dataFrequencies;
            basicHistogram.GetHistogram(out dataValues, out dataFrequencies);

            // 5. 计算断点
            IClassify classify = null;
            if (method == ClassifyMethod.EqualInterval)
                classify = new EqualIntervalClass();
            else
                classify = new NaturalBreaksClass();

            classify.SetHistogramData(dataValues, dataFrequencies);

            // 尝试分类
            classify.Classify(breakCount);
            double[] breaks = (double[])classify.ClassBreaks;

            // 6. 再次检查分类结果 (防止请求5级只分出2级的情况)
            if (breaks.Length == 0) return;

            // 实际计算出的级数 (数组长度 - 1)
            int actualClassCount = breaks.Length - 1;

            // 如果实际级数 < 用户要求的级数，抛出友好提示
            if (actualClassCount < breakCount)
            {
                throw new Exception(string.Format("数据分布不足：字段 '{0}' 数据过于集中，无法划分为 {1} 级（实际仅能计算出 {2} 级）。\n建议减少分级数量或更换字段。",fieldName, breakCount, actualClassCount));
            }

            // 7. 生成高级色
            IAlgorithmicColorRamp colorRamp = new AlgorithmicColorRampClass();
            // 使用 CIELab 算法
            colorRamp.Algorithm = esriColorRampAlgorithm.esriCIELabAlgorithm;
            colorRamp.Size = breakCount;

            switch (colorStyle)
            {
                case ColorStyle.Reds:
                    // 浅粉红 (255, 235, 235) -> 深红 (168, 0, 0)
                    colorRamp.FromColor = MakeRGB(255, 235, 235);
                    colorRamp.ToColor = MakeRGB(168, 0, 0);
                    break;

                case ColorStyle.Greens:
                    // 浅薄荷绿 (237, 248, 233) -> 深森林绿 (0, 109, 44)
                    colorRamp.FromColor = MakeRGB(237, 248, 233);
                    colorRamp.ToColor = MakeRGB(0, 109, 44);
                    break;

                case ColorStyle.Blues:
                    // 浅天蓝 (239, 243, 255) -> 深海蓝 (8, 81, 156)
                    colorRamp.FromColor = MakeRGB(239, 243, 255);
                    colorRamp.ToColor = MakeRGB(8, 81, 156);
                    break;

                case ColorStyle.Heatmap:
                    // 柔和黄 (255, 255, 178) -> 宝石红 (189, 0, 38)
                    colorRamp.FromColor = MakeRGB(255, 255, 178);
                    colorRamp.ToColor = MakeRGB(189, 0, 38);
                    break;

                case ColorStyle.Spectral:
                    colorRamp.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;
                    colorRamp.FromColor = MakeRGB(0, 0, 255);   // 蓝
                    colorRamp.ToColor = MakeRGB(255, 0, 0);     // 红
                    break;
            }

            bool ok;
            colorRamp.CreateRamp(out ok);
            IEnumColors enumColors = colorRamp.Colors;

            IClassBreaksRenderer renderer = new ClassBreaksRendererClass();
            renderer.Field = fieldName;
            renderer.BreakCount = breakCount;
            renderer.SortClassesAscending = true;

            enumColors.Reset();

            for (int i = 0; i < breakCount; i++)
            {
                renderer.set_Break(i, breaks[i + 1]);
                renderer.set_Label(i, string.Format("{0:0.##} - {1:0.##}", breaks[i], breaks[i + 1]));

                ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass();
                fillSymbol.Color = enumColors.Next();
                fillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;

                if (featureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                {
                    ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass();
                    lineSymbol.Color = fillSymbol.Color;
                    lineSymbol.Width = 2;
                    renderer.set_Symbol(i, (ISymbol)lineSymbol);
                }
                else
                {
                    renderer.set_Symbol(i, (ISymbol)fillSymbol);
                }
            }

            IGeoFeatureLayer geoFeatureLayer = featureLayer as IGeoFeatureLayer;
            if (geoFeatureLayer != null)
            {
                geoFeatureLayer.Renderer = (IFeatureRenderer)renderer;
            }
        }
            // 辅助方法：生成RGB颜色
        private static IColor MakeRGB(int r, int g, int b)
        {
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = r; rgb.Green = g; rgb.Blue = b;
            return rgb;
        }

        /// <summary>
        /// 辅助方法：判断字段是否为数值型
        /// </summary>
        public static bool IsNumericField(IField field)
        {
            return (field.Type == esriFieldType.esriFieldTypeInteger ||
                    field.Type == esriFieldType.esriFieldTypeSmallInteger ||
                    field.Type == esriFieldType.esriFieldTypeDouble ||
                    field.Type == esriFieldType.esriFieldTypeSingle);
        }
    }
}
