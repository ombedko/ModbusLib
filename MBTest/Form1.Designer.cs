namespace MBTest {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.lblData = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblData
			// 
			this.lblData.BackColor = System.Drawing.SystemColors.Control;
			this.lblData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblData.Location = new System.Drawing.Point(12, 9);
			this.lblData.Name = "lblData";
			this.lblData.Size = new System.Drawing.Size(260, 244);
			this.lblData.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.lblData);
			this.Name = "Form1";
			this.Text = "Form1";			
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblData;
	}
}

