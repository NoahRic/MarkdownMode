namespace MarkdownMode.OptionsPages
{
    partial class RenderingOptionsControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.GroupBox groupBox1;
            this.btnAuthorize = new System.Windows.Forms.Button();
            this.btnGitHub = new System.Windows.Forms.RadioButton();
            this.btnMarkdownSharp = new System.Windows.Forms.RadioButton();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this.btnAuthorize);
            groupBox1.Controls.Add(this.btnGitHub);
            groupBox1.Controls.Add(this.btnMarkdownSharp);
            groupBox1.Location = new System.Drawing.Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(332, 72);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Markdown Renderer";
            // 
            // btnAuthorize
            // 
            this.btnAuthorize.Location = new System.Drawing.Point(220, 40);
            this.btnAuthorize.Name = "btnAuthorize";
            this.btnAuthorize.Size = new System.Drawing.Size(106, 23);
            this.btnAuthorize.TabIndex = 2;
            this.btnAuthorize.Text = "Authorize...";
            this.btnAuthorize.UseVisualStyleBackColor = true;
            this.btnAuthorize.Click += new System.EventHandler(this.HandleAuthorize_Click);
            // 
            // btnGitHub
            // 
            this.btnGitHub.AutoSize = true;
            this.btnGitHub.Location = new System.Drawing.Point(7, 43);
            this.btnGitHub.Name = "btnGitHub";
            this.btnGitHub.Size = new System.Drawing.Size(58, 17);
            this.btnGitHub.TabIndex = 1;
            this.btnGitHub.TabStop = true;
            this.btnGitHub.Text = "GitHub";
            this.btnGitHub.UseVisualStyleBackColor = true;
            // 
            // btnMarkdownSharp
            // 
            this.btnMarkdownSharp.AutoSize = true;
            this.btnMarkdownSharp.Location = new System.Drawing.Point(6, 19);
            this.btnMarkdownSharp.Name = "btnMarkdownSharp";
            this.btnMarkdownSharp.Size = new System.Drawing.Size(106, 17);
            this.btnMarkdownSharp.TabIndex = 0;
            this.btnMarkdownSharp.TabStop = true;
            this.btnMarkdownSharp.Text = "Markdown Sharp";
            this.btnMarkdownSharp.UseVisualStyleBackColor = true;
            // 
            // RenderingOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(groupBox1);
            this.Name = "RenderingOptionsControl";
            this.Size = new System.Drawing.Size(338, 258);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAuthorize;
        private System.Windows.Forms.RadioButton btnGitHub;
        private System.Windows.Forms.RadioButton btnMarkdownSharp;
    }
}
