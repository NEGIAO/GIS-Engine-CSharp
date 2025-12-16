namespace 自定义窗体控件
{
    partial class UserControl1
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

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.open = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblLenth = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(106, 25);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(238, 315);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // open
            // 
            this.open.Location = new System.Drawing.Point(325, 371);
            this.open.Name = "open";
            this.open.Size = new System.Drawing.Size(79, 49);
            this.open.TabIndex = 4;
            this.open.Text = "打开";
            this.open.UseVisualStyleBackColor = true;
            this.open.Click += new System.EventHandler(this.open_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(104, 365);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "文件名称：";
            // 
            // labName
            // 
            this.labName.AutoSize = true;
            this.labName.Location = new System.Drawing.Point(172, 365);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(41, 12);
            this.labName.TabIndex = 5;
            this.labName.Text = "label1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(104, 389);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "文件大小：";
            // 
            // lblLenth
            // 
            this.lblLenth.AutoSize = true;
            this.lblLenth.Location = new System.Drawing.Point(172, 389);
            this.lblLenth.Name = "lblLenth";
            this.lblLenth.Size = new System.Drawing.Size(41, 12);
            this.lblLenth.TabIndex = 5;
            this.lblLenth.Text = "label1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(104, 415);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "文件尺寸：";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(172, 415);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(41, 12);
            this.lblSize.TabIndex = 5;
            this.lblSize.Text = "label1";
            // 
            // UserControl1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.lblLenth);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.open);
            this.Controls.Add(this.pictureBox);
            this.Name = "UserControl1";
            this.Size = new System.Drawing.Size(444, 522);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Button open;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblLenth;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSize;
    }
}
