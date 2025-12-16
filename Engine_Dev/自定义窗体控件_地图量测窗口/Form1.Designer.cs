namespace 自定义窗体控件_地图量测窗口
{
    partial class FormMeasureResult
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMeasureResult));
            this.IbIMeasureResult = new System.Windows.Forms.Label();
            this.IbIResult = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // IbIMeasureResult
            // 
            this.IbIMeasureResult.AutoSize = true;
            this.IbIMeasureResult.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.IbIMeasureResult.Location = new System.Drawing.Point(96, 54);
            this.IbIMeasureResult.Name = "IbIMeasureResult";
            this.IbIMeasureResult.Size = new System.Drawing.Size(190, 35);
            this.IbIMeasureResult.TabIndex = 0;
            this.IbIMeasureResult.Text = "量测结果：";
            // 
            // IbIResult
            // 
            this.IbIResult.AutoSize = true;
            this.IbIResult.Font = new System.Drawing.Font("宋体", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.IbIResult.Location = new System.Drawing.Point(291, 54);
            this.IbIResult.Name = "IbIResult";
            this.IbIResult.Size = new System.Drawing.Size(0, 35);
            this.IbIResult.TabIndex = 1;
            // 
            // FormMeasureResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(539, 145);
            this.Controls.Add(this.IbIResult);
            this.Controls.Add(this.IbIMeasureResult);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMeasureResult";
            this.Text = "测量结果";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMeasureResult_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label IbIMeasureResult;
        private System.Windows.Forms.Label IbIResult;
    }
}

