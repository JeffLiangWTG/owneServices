namespace WinzorFramework.Samples
{
	partial class MainForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.CommonControlBtn = new System.Windows.Forms.Button();
			this.edocFormBtn = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.taskGridsFormBtn = new System.Windows.Forms.Button();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 5;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.CommonControlBtn, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.edocFormBtn, 4, 0);
			this.tableLayoutPanel1.Controls.Add(this.button1, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.taskGridsFormBtn, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1299, 893);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// CommonControlBtn
			// 
			this.CommonControlBtn.Location = new System.Drawing.Point(3, 3);
			this.CommonControlBtn.Name = "CommonControlBtn";
			this.CommonControlBtn.Size = new System.Drawing.Size(157, 59);
			this.CommonControlBtn.TabIndex = 1;
			this.CommonControlBtn.Text = "Common Controls Form";
			this.CommonControlBtn.UseVisualStyleBackColor = true;
			this.CommonControlBtn.Click += new System.EventHandler(this.CommonControlBtn_Click);
			// 
			// edocFormBtn
			// 
			this.edocFormBtn.Location = new System.Drawing.Point(492, 3);
			this.edocFormBtn.Name = "edocFormBtn";
			this.edocFormBtn.Size = new System.Drawing.Size(157, 59);
			this.edocFormBtn.TabIndex = 0;
			this.edocFormBtn.Text = "EDocs Form";
			this.edocFormBtn.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(166, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(157, 59);
			this.button1.TabIndex = 3;
			this.button1.Text = "WI Details Form";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.WIDetailsBtn_Click);
			// 
			// taskGridsFormBtn
			// 
			this.taskGridsFormBtn.Location = new System.Drawing.Point(329, 3);
			this.taskGridsFormBtn.Name = "taskGridsFormBtn";
			this.taskGridsFormBtn.Size = new System.Drawing.Size(157, 59);
			this.taskGridsFormBtn.TabIndex = 2;
			this.taskGridsFormBtn.Text = "Tasks Grid Form";
			this.taskGridsFormBtn.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1299, 893);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "MainForm";
			this.Text = "MainForm";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button edocFormBtn;
		private System.Windows.Forms.Button CommonControlBtn;
		private System.Windows.Forms.Button taskGridsFormBtn;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ColorDialog colorDialog1;
	}
}
