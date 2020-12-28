namespace ConnectionPromptDialog
{
    partial class MainForm
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
            this.btn_DataConnectDialog = new System.Windows.Forms.Button();
            this.tb_ConnectionStr = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_DataConnectDialog
            // 
            this.btn_DataConnectDialog.Location = new System.Drawing.Point(479, 188);
            this.btn_DataConnectDialog.Name = "btn_DataConnectDialog";
            this.btn_DataConnectDialog.Size = new System.Drawing.Size(89, 35);
            this.btn_DataConnectDialog.TabIndex = 1;
            this.btn_DataConnectDialog.Text = "选择";
            this.btn_DataConnectDialog.UseVisualStyleBackColor = true;
            this.btn_DataConnectDialog.Click += new System.EventHandler(this.btn_DataConnectDialog_Click);
            // 
            // tb_ConnectionStr
            // 
            this.tb_ConnectionStr.Location = new System.Drawing.Point(6, 24);
            this.tb_ConnectionStr.Multiline = true;
            this.tb_ConnectionStr.Name = "tb_ConnectionStr";
            this.tb_ConnectionStr.ReadOnly = true;
            this.tb_ConnectionStr.Size = new System.Drawing.Size(550, 131);
            this.tb_ConnectionStr.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tb_ConnectionStr);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(562, 161);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "连接字符串";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 252);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_DataConnectDialog);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_DataConnectDialog;
        private System.Windows.Forms.TextBox tb_ConnectionStr;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

