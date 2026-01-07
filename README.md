# ArcGIS Engine Desktop GIS System

## 📖 项目简介 (Introduction)

本项目是基于 **ArcGIS Engine 10.2.2** 版本，在 **Visual Studio 2012** 中开发的项目文件。

这是我本学期 GIS 开发专业课的课程作业，使用 **C#** 语言在 ArcGIS Engine 基础上开发了常见的 GIS 功能。源代码中均有详细注释，旨在供大家学习交流，同时也作为项目的归档。

## 💻 开发环境 (Environment)

本项目对开发环境有严格要求，请确保您的环境满足以下条件：

- **IDE**: Visual Studio 2012
- **GIS SDK**: ArcGIS Engine 10.2.2
- **Language**: C# (.NET Framework)

## 📂 项目结构 (File Structure)

```text
📦 arcgis_engine_project
 ┣ 📂 Data # 项目测试数据
 ┃ ┣ 📂 CAD数据
 ┃ ┃ ┗ 📜 MillerRanch.dwg
 ┃ ┣ 📂 Mxd文件
 ┃ ┃ ┣ 📂 HuanbaoGeodatabase.gdb
 ┃ ┃ ┗ 📜 Qingdao.mxd
 ┃ ┣ 📂 SHP数据
 ┃ ┃ ┣ 📜 gonglu.dbf
 ┃ ┃ ┣ 📜 gonglu.prj
 ┃ ┃ ┣ 📜 gonglu.sbn
 ┃ ┃ ┣ 📜 gonglu.sbx
 ┃ ┃ ┣ 📜 gonglu.shp
 ┃ ┃ ┣ 📜 gonglu.shp.xml
 ┃ ┃ ┗ 📜 gonglu.shx
 ┃ ┣ 📂 个人地理数据库
 ┃ ┃ ┗ 📂 community.mdb
 ┃ ┣ 📂 文件地理数据库
 ┃ ┃ ┗ 📂 Representations.gdb
 ┃ ┣ 📂 文本文件
 ┃ ┃ ┣ 📜 111.dbf
 ┃ ┃ ┣ 📜 111.prj
 ┃ ┃ ┣ 📜 111.shp
 ┃ ┃ ┣ 📜 111.shx
 ┃ ┃ ┣ 📜 Beijing1954.txt
 ┃ ┃ ┣ 📜 Beijing19541.dbf
 ┃ ┃ ┣ 📜 Beijing19541.prj
 ┃ ┃ ┣ 📜 Beijing19541.shp
 ┃ ┃ ┣ 📜 Beijing19541.shx
 ┃ ┃ ┗ 📜 schema.ini
 ┃ ┣ 📂 栅格数据
 ┃ ┃ ┣ 📜 image.img
 ┃ ┃ ┣ 📜 image.img.aux.xml
 ┃ ┃ ┗ 📜 image.rrd
 ┃ ┗ 📜 Mxd文件.rar
 ┣ 📂 Engine_Dev # 核心代码解决方案目录
 ┃ ┣ 📂 DLL_符号化
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┗ 📜 AssemblyInfo.cs
 ┃ ┃ ┣ 📜 DLL_符号化.csproj
 ┃ ┃ ┗ 📜 SymbologyHelper.cs
 ┃ ┣ 📂 GIS_2310130172_ # 主程序项目
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📂 Service References
 ┃ ┃ ┣ 📜 AddDateTool.cs
 ┃ ┃ ┣ 📜 App.config
 ┃ ┃ ┣ 📜 avatar_24x24.ico
 ┃ ┃ ┣ 📜 ClassDiagram1.cd
 ┃ ┃ ┣ 📜 ClassDiagram2.cd
 ┃ ┃ ┣ 📜 ClearCurrentActiveToolCmd.bmp
 ┃ ┃ ┣ 📜 ClearCurrentToolCMD.cs
 ┃ ┃ ┣ 📜 Command1.bmp
 ┃ ┃ ┣ 📜 CreateFeatureToolClass.cs
 ┃ ┃ ┣ 📜 FrmMain.cs
 ┃ ┃ ┣ 📜 FrmMain.Designer.cs
 ┃ ┃ ┣ 📜 FrmMain.resx
 ┃ ┃ ┣ 📜 GeoMapAO.cs
 ┃ ┃ ┣ 📜 GIS_2310130172_Engine.csproj
 ┃ ┃ ┣ 📜 GIS_2310130172_Engine.csproj.user
 ┃ ┃ ┣ 📜 GIS_2310130172_Engine_TemporaryKey.pfx
 ┃ ┃ ┣ 📜 MapManager.cs
 ┃ ┃ ┣ 📜 Program.cs
 ┃ ┃ ┣ 📜 SaveEditCommandClass.cs
 ┃ ┃ ┣ 📜 StopEditCommandClass.cs
 ┃ ┃ ┗ 📜 Table.icon.ico
 ┃ ┣ 📂 自定义DLL_SymbologyMenu # 符号化功能类库
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┗ 📜 AssemblyInfo.cs
 ┃ ┃ ┣ 📜 BarChartRender.bmp
 ┃ ┃ ┣ 📜 BarChartRender.cs
 ┃ ┃ ┣ 📜 ClassBreakRender.bmp
 ┃ ┃ ┣ 📜 ClassBreakRender.cs
 ┃ ┃ ┣ 📜 DotDensityRender.bmp
 ┃ ┃ ┣ 📜 DotDensityRender.cs
 ┃ ┃ ┣ 📜 ProportionalSymbol.bmp
 ┃ ┃ ┣ 📜 ProportionalSymbol.cs
 ┃ ┃ ┣ 📜 SimpleRender.bmp
 ┃ ┃ ┣ 📜 SimpleRender.cs
 ┃ ┃ ┣ 📜 SymbologyMenu.cs
 ┃ ┃ ┣ 📜 UniqueValueRender.bmp
 ┃ ┃ ┣ 📜 UniqueValueRender.cs
 ┃ ┃ ┗ 📜 自定义DLL_Symbology.csproj
 ┃ ┣ 📂 自定义窗体控件 # 通用自定义控件
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Resources.resx
 ┃ ┃ ┣ 📜 UserControl1.cs
 ┃ ┃ ┣ 📜 UserControl1.Designer.cs
 ┃ ┃ ┣ 📜 UserControl1.resx
 ┃ ┃ ┣ 📜 自定义窗体控件.csproj
 ┃ ┃ ┗ 📜 自定义窗体控件_打开图片功能.csproj
 ┃ ┣ 📂 自定义窗体控件_Add_txt # 文本添加功能控件
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📂 Service References
 ┃ ┃ ┣ 📜 App.config
 ┃ ┃ ┣ 📜 ClassDiagram1.cd
 ┃ ┃ ┣ 📜 ClassDiagram2.cd
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┣ 📜 Program.cs
 ┃ ┃ ┗ 📜 自定义窗体控件_Add_txt.csproj
 ┃ ┣ 📂 自定义窗体控件_FormAttribute # 属性表查看控件
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📜 App.config
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┣ 📜 Program.cs
 ┃ ┃ ┗ 📜 自定义窗体控件_FormAttribute.csproj
 ┃ ┣ 📂 自定义窗体控件_地图导出窗口 # 地图导出功能控件
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📜 App.config
 ┃ ┃ ┣ 📜 ExportMap.cs
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┗ 📜 自定义窗体控件_地图导出窗口.csproj
 ┃ ┣ 📂 自定义窗体控件_地图打印
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📜 ClassDiagram1.cd
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┣ 📜 LayoutHelper.cs
 ┃ ┃ ┗ 📜 自定义窗体控件_地图打印.csproj
 ┃ ┣ 📂 自定义窗体控件_地图选择集 # 选择集管理控件
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┣ 📜 Program.cs
 ┃ ┃ ┗ 📜 自定义窗体控件_地图选择集.csproj
 ┃ ┣ 📂 自定义窗体控件_地图量测窗口 # 距离/面积量测控件
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📜 App.config
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┣ 📜 Program.cs
 ┃ ┃ ┗ 📜 自定义窗体控件_地图量测窗口.csproj
 ┃ ┣ 📂 自定义窗体控件_符号系统
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📜 ClassDiagram1.cd
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┣ 📜 SymbologyHelper.cs
 ┃ ┃ ┗ 📜 自定义窗体控件_符号系统.csproj
 ┃ ┣ 📂 自定义窗体控件_符号选择器
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┗ 📜 自定义窗体控件_符号选择器.csproj
 ┃ ┣ 📂 自定义窗体控件_统计选择集 # 统计分析控件
 ┃ ┃ ┣ 📂 Properties
 ┃ ┃ ┃ ┣ 📜 AssemblyInfo.cs
 ┃ ┃ ┃ ┣ 📜 Resources.Designer.cs
 ┃ ┃ ┃ ┣ 📜 Resources.resx
 ┃ ┃ ┃ ┣ 📜 Settings.Designer.cs
 ┃ ┃ ┃ ┗ 📜 Settings.settings
 ┃ ┃ ┣ 📜 Form1.cs
 ┃ ┃ ┣ 📜 Form1.Designer.cs
 ┃ ┃ ┣ 📜 Form1.resx
 ┃ ┃ ┣ 📜 Program.cs
 ┃ ┃ ┗ 📜 自定义窗体控件_统计选择集.csproj
 ┃ ┣ 📜 Engine_Dev.sln
 ┃ ┣ 📜 Engine_Dev.suo
 ┃ ┗ 📜 Engine_Dev.v11.suo
 ┣ 📂 课堂文件
 ┃ ┣ 📜 ArcMapObjectModel.pdf
 ┃ ┣ 📜 Symbol.rar
 ┃ ┣ 📜 地图导出.pdf
 ┃ ┣ 📜 地图选择集.pdf
 ┃ ┣ 📜 新建要素类.pdf
 ┃ ┣ 📜 笔记9_23.txt
 ┃ ┣ 📜 符号选择器.pdf
 ┃ ┣ 📜 统计选择集.pdf
 ┃ ┣ 📜 编辑菜单.pdf
 ┃ ┣ 📜 自定义命令和工具.pdf
 ┃ ┣ 📜 自定义菜单.pdf
 ┃ ┗ 📜 距离面积测算.pdf
 ┗ 📜 README.md
```

## ✨ 主要功能 (Features)

1.  **基础地图操作**：地图浏览（放大、缩小、漫游）、鹰眼导航、TOC 图层管理。
2.  **空间数据查询**：属性表查看、空间选择。
3.  **空间量算**：距离测量、面积测量。
4.  **专题图制作**：支持简单渲染、唯一值渲染、分级色彩、点密度、比例符号、柱状图等多种专题图。
5.  **数据编辑**：要素的创建与编辑功能。
6.  **制图输出**：地图整饰与图片导出。

## 🚀 使用说明 (Usage)

1.  克隆或下载本仓库。
2.  使用 **Visual Studio 2012** 打开 `Engine_Dev/Engine_Dev.sln`。
3.  **重要**：由于 ArcGIS Engine 版本差异，打开项目后可能需要重新引用 `ESRI.ArcGIS.*` 相关类库。
4.  编译并运行项目。

## 📝 备注

- 代码中包含详细注释，适合初学者阅读。
- `Data` 目录下提供了配套的测试数据，建议直接使用该数据进行功能测试。
