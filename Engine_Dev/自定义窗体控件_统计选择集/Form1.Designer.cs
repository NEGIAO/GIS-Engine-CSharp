namespace 自定义窗体控件_统计选择集
{
    partial class FormStatistics
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStatistics));
            this.comboxBoxFields = new System.Windows.Forms.ComboBox();
            this.comboBoxLayers = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelStatisticResults = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelSelection = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboxBoxFields
            // 
            this.comboxBoxFields.FormattingEnabled = true;
            this.comboxBoxFields.Location = new System.Drawing.Point(249, 124);
            this.comboxBoxFields.Name = "comboxBoxFields";
            this.comboxBoxFields.Size = new System.Drawing.Size(278, 20);
            this.comboxBoxFields.TabIndex = 0;
            this.comboxBoxFields.SelectedIndexChanged += new System.EventHandler(this.comboBoxFeilds_SelectedIndexChanged);
            // 
            // comboBoxLayers
            // 
            this.comboBoxLayers.FormattingEnabled = true;
            this.comboBoxLayers.Location = new System.Drawing.Point(249, 72);
            this.comboBoxLayers.Name = "comboBoxLayers";
            this.comboBoxLayers.Size = new System.Drawing.Size(278, 20);
            this.comboBoxLayers.TabIndex = 1;
            this.comboBoxLayers.SelectedIndexChanged += new System.EventHandler(this.comboBoxLayers_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelStatisticResults);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(58, 173);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(571, 232);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "统计结果";
            // 
            // labelStatisticResults
            // 
            this.labelStatisticResults.AutoSize = true;
            this.labelStatisticResults.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelStatisticResults.Location = new System.Drawing.Point(112, 70);
            this.labelStatisticResults.Name = "labelStatisticResults";
            this.labelStatisticResults.Size = new System.Drawing.Size(69, 19);
            this.labelStatisticResults.TabIndex = 1;
            this.labelStatisticResults.Text = "label1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(105, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 12);
            this.label4.TabIndex = 0;
            // 
            // labelSelection
            // 
            this.labelSelection.AutoSize = true;
            this.labelSelection.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelSelection.Location = new System.Drawing.Point(144, 9);
            this.labelSelection.Name = "labelSelection";
            this.labelSelection.Size = new System.Drawing.Size(409, 20);
            this.labelSelection.TabIndex = 3;
            this.labelSelection.Text = "当前地图选择集共有n个图层的n个要素被选中";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(146, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "图层:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(146, 127);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "字段:";
            // 
            // FormStatistics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 442);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboxBoxFields);
            this.Controls.Add(this.labelSelection);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.comboBoxLayers);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormStatistics";
            this.Text = "统计选择集";
            this.Load += new System.EventHandler(this.FormStatistics_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboxBoxFields;
        private System.Windows.Forms.ComboBox comboBoxLayers;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelSelection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelStatisticResults;
    }
}

