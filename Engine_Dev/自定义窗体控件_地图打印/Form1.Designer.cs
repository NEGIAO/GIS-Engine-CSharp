namespace 自定义窗体控件_地图打印
{
    partial class FormPrintPreview
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrintPreview));
            this.btPrint = new System.Windows.Forms.Button();
            this.btExport = new System.Windows.Forms.Button();
            this.rbA4 = new System.Windows.Forms.RadioButton();
            this.rbPortrit = new System.Windows.Forms.RadioButton();
            this.rbA3 = new System.Windows.Forms.RadioButton();
            this.rbLandscape = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.axPageLayoutControl1 = new ESRI.ArcGIS.Controls.AxPageLayoutControl();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axPageLayoutControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // btPrint
            // 
            this.btPrint.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btPrint.Location = new System.Drawing.Point(0, 393);
            this.btPrint.Name = "btPrint";
            this.btPrint.Size = new System.Drawing.Size(638, 33);
            this.btPrint.TabIndex = 1;
            this.btPrint.Text = "打印";
            this.btPrint.UseVisualStyleBackColor = true;
            this.btPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btExport
            // 
            this.btExport.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btExport.Location = new System.Drawing.Point(0, 360);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(638, 33);
            this.btExport.TabIndex = 2;
            this.btExport.Text = "导出";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // rbA4
            // 
            this.rbA4.AutoSize = true;
            this.rbA4.Location = new System.Drawing.Point(119, 21);
            this.rbA4.Name = "rbA4";
            this.rbA4.Size = new System.Drawing.Size(65, 16);
            this.rbA4.TabIndex = 0;
            this.rbA4.TabStop = true;
            this.rbA4.Text = "A4 纸张";
            this.rbA4.UseVisualStyleBackColor = true;
            this.rbA4.CheckedChanged += new System.EventHandler(this.PageSetting_CheckedChanged);
            // 
            // rbPortrit
            // 
            this.rbPortrit.AutoSize = true;
            this.rbPortrit.Location = new System.Drawing.Point(311, 20);
            this.rbPortrit.Name = "rbPortrit";
            this.rbPortrit.Size = new System.Drawing.Size(47, 16);
            this.rbPortrit.TabIndex = 1;
            this.rbPortrit.TabStop = true;
            this.rbPortrit.Text = "纵向";
            this.rbPortrit.UseVisualStyleBackColor = true;
            this.rbPortrit.CheckedChanged += new System.EventHandler(this.PageSetting_CheckedChanged);
            // 
            // rbA3
            // 
            this.rbA3.AutoSize = true;
            this.rbA3.Location = new System.Drawing.Point(119, 62);
            this.rbA3.Name = "rbA3";
            this.rbA3.Size = new System.Drawing.Size(65, 16);
            this.rbA3.TabIndex = 2;
            this.rbA3.TabStop = true;
            this.rbA3.Text = "A3 纸张";
            this.rbA3.UseVisualStyleBackColor = true;
            this.rbA3.CheckedChanged += new System.EventHandler(this.PageSetting_CheckedChanged);
            // 
            // rbLandscape
            // 
            this.rbLandscape.AutoSize = true;
            this.rbLandscape.Location = new System.Drawing.Point(311, 62);
            this.rbLandscape.Name = "rbLandscape";
            this.rbLandscape.Size = new System.Drawing.Size(47, 16);
            this.rbLandscape.TabIndex = 3;
            this.rbLandscape.TabStop = true;
            this.rbLandscape.Text = "横向";
            this.rbLandscape.UseVisualStyleBackColor = true;
            this.rbLandscape.CheckedChanged += new System.EventHandler(this.PageSetting_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtTitle);
            this.groupBox1.Controls.Add(this.rbLandscape);
            this.groupBox1.Controls.Add(this.rbA3);
            this.groupBox1.Controls.Add(this.rbPortrit);
            this.groupBox1.Controls.Add(this.rbA4);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 426);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(638, 96);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "页面设置";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(452, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "布局标题设置：";
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(421, 46);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(153, 21);
            this.txtTitle.TabIndex = 4;
            this.txtTitle.Text = "地图默认标题";
            this.txtTitle.TextChanged += new System.EventHandler(this.PageSetting_CheckedChanged);
            // 
            // axPageLayoutControl1
            // 
            this.axPageLayoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axPageLayoutControl1.Location = new System.Drawing.Point(0, 0);
            this.axPageLayoutControl1.Name = "axPageLayoutControl1";
            this.axPageLayoutControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axPageLayoutControl1.OcxState")));
            this.axPageLayoutControl1.Size = new System.Drawing.Size(638, 360);
            this.axPageLayoutControl1.TabIndex = 4;
            // 
            // FormPrintPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 522);
            this.Controls.Add(this.axPageLayoutControl1);
            this.Controls.Add(this.btExport);
            this.Controls.Add(this.btPrint);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPrintPreview";
            this.Text = "地图打印窗口";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormPrintPreview_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axPageLayoutControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btPrint;
        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.RadioButton rbA4;
        private System.Windows.Forms.RadioButton rbPortrit;
        private System.Windows.Forms.RadioButton rbA3;
        private System.Windows.Forms.RadioButton rbLandscape;
        private System.Windows.Forms.GroupBox groupBox1;
        private ESRI.ArcGIS.Controls.AxPageLayoutControl axPageLayoutControl1;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label label1;

    }
}

