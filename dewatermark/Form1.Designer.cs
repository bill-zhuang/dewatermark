namespace dewatermark
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnDeWaterMarkByChooseImage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(49, 44);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(119, 73);
            this.button1.TabIndex = 0;
            this.button1.Text = "DeWaterMark(choose folder)";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnDeWaterMarkByChooseImage
            // 
            this.btnDeWaterMarkByChooseImage.Location = new System.Drawing.Point(183, 45);
            this.btnDeWaterMarkByChooseImage.Name = "btnDeWaterMarkByChooseImage";
            this.btnDeWaterMarkByChooseImage.Size = new System.Drawing.Size(117, 72);
            this.btnDeWaterMarkByChooseImage.TabIndex = 1;
            this.btnDeWaterMarkByChooseImage.Text = "DeWaterMark(choose image)";
            this.btnDeWaterMarkByChooseImage.UseVisualStyleBackColor = true;
            this.btnDeWaterMarkByChooseImage.Click += new System.EventHandler(this.btnDeWaterMarkByChooseImage_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 195);
            this.Controls.Add(this.btnDeWaterMarkByChooseImage);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnDeWaterMarkByChooseImage;
    }
}

