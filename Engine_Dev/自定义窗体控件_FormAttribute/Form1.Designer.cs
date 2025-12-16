namespace 自定义窗体控件_FormAttribute
{
    partial class FormAttribute
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAttribute));
            this.dataGridAttribute = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAttribute)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridAttribute
            // 
            this.dataGridAttribute.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridAttribute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridAttribute.Location = new System.Drawing.Point(0, 0);
            this.dataGridAttribute.Name = "dataGridAttribute";
            this.dataGridAttribute.RowTemplate.Height = 23;
            this.dataGridAttribute.Size = new System.Drawing.Size(639, 352);
            this.dataGridAttribute.TabIndex = 0;
            // 
            // FormAttribute
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 352);
            this.Controls.Add(this.dataGridAttribute);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormAttribute";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAttribute)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridAttribute;
    }
}

