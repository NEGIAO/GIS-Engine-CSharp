namespace 自定义窗体控件_地图选择集
{
    partial class FormSelection
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("图层");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSelection));
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.treeViewLayers = new System.Windows.Forms.TreeView();
            this.labelLayerSelectionCount = new System.Windows.Forms.Label();
            this.labelMapSelectionCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new System.Drawing.Point(216, 12);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowTemplate.Height = 23;
            this.dataGridView.Size = new System.Drawing.Size(575, 391);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellClick);
            // 
            // treeViewLayers
            // 
            this.treeViewLayers.Location = new System.Drawing.Point(12, 12);
            this.treeViewLayers.Name = "treeViewLayers";
            treeNode1.Name = "Layers";
            treeNode1.Text = "图层";
            this.treeViewLayers.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewLayers.Size = new System.Drawing.Size(198, 422);
            this.treeViewLayers.TabIndex = 1;
            this.treeViewLayers.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            // 
            // labelLayerSelectionCount
            // 
            this.labelLayerSelectionCount.AutoSize = true;
            this.labelLayerSelectionCount.Location = new System.Drawing.Point(217, 410);
            this.labelLayerSelectionCount.Name = "labelLayerSelectionCount";
            this.labelLayerSelectionCount.Size = new System.Drawing.Size(143, 12);
            this.labelLayerSelectionCount.TabIndex = 2;
            this.labelLayerSelectionCount.Text = "当前图层选择了 0 个要素";
            // 
            // labelMapSelectionCount
            // 
            this.labelMapSelectionCount.AutoSize = true;
            this.labelMapSelectionCount.Location = new System.Drawing.Point(636, 410);
            this.labelMapSelectionCount.Name = "labelMapSelectionCount";
            this.labelMapSelectionCount.Size = new System.Drawing.Size(155, 12);
            this.labelMapSelectionCount.TabIndex = 3;
            this.labelMapSelectionCount.Text = "当前地图共选择了 0 个要素";
            // 
            // FormSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 446);
            this.Controls.Add(this.labelMapSelectionCount);
            this.Controls.Add(this.labelLayerSelectionCount);
            this.Controls.Add(this.treeViewLayers);
            this.Controls.Add(this.dataGridView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormSelection";
            this.Text = "地图选择集";
            this.Load += new System.EventHandler(this.FormSelection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.TreeView treeViewLayers;
        public System.Windows.Forms.Label labelLayerSelectionCount;
        public System.Windows.Forms.Label labelMapSelectionCount;
    }
}

