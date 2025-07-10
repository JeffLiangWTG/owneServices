namespace Enterprise.MasterFiles.OrganizationMergeServiceClientApp
{
	partial class OrganizationMergeServiceClientForm
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrganizationMergeServiceClientForm));
			this.lNewOrgCode = new System.Windows.Forms.Label();
			this.newOrgCodeTextBox = new System.Windows.Forms.TextBox();
			this.oldOrgCodesGrid = new System.Windows.Forms.DataGridView();
			this.label1 = new System.Windows.Forms.Label();
			this.mergeButton = new System.Windows.Forms.Button();
			this.xmlTextbox = new System.Windows.Forms.TextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.passwordTextbox = new System.Windows.Forms.TextBox();
			this.nameTextbox = new System.Windows.Forms.TextBox();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.gridTabPage = new System.Windows.Forms.TabPage();
			this.xmlTabPage = new System.Windows.Forms.TabPage();
			this.label4 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.oldOrgCodesGrid)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.gridTabPage.SuspendLayout();
			this.xmlTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// lNewOrgCode
			// 
			this.lNewOrgCode.AutoSize = true;
			this.lNewOrgCode.Location = new System.Drawing.Point(2, 13);
			this.lNewOrgCode.Name = "lNewOrgCode";
			this.lNewOrgCode.Size = new System.Drawing.Size(122, 13);
			this.lNewOrgCode.TabIndex = 7;
			this.lNewOrgCode.Text = "New Organization Code:";
			// 
			// newOrgCodeTextBox
			// 
			this.newOrgCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.newOrgCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.newOrgCodeTextBox.Location = new System.Drawing.Point(131, 10);
			this.newOrgCodeTextBox.Name = "newOrgCodeTextBox";
			this.newOrgCodeTextBox.Size = new System.Drawing.Size(249, 20);
			this.newOrgCodeTextBox.TabIndex = 3;
			// 
			// oldOrgCodesGrid
			// 
			this.oldOrgCodesGrid.AllowUserToResizeRows = false;
			this.oldOrgCodesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.oldOrgCodesGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.oldOrgCodesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.oldOrgCodesGrid.DefaultCellStyle = dataGridViewCellStyle2;
			this.oldOrgCodesGrid.Location = new System.Drawing.Point(5, 51);
			this.oldOrgCodesGrid.Name = "oldOrgCodesGrid";
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.oldOrgCodesGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.oldOrgCodesGrid.Size = new System.Drawing.Size(375, 193);
			this.oldOrgCodesGrid.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(2, 35);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(126, 13);
			this.label1.TabIndex = 10;
			this.label1.Text = "Old Organizations Codes:";
			// 
			// mergeButton
			// 
			this.mergeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.mergeButton.Location = new System.Drawing.Point(321, 381);
			this.mergeButton.Name = "mergeButton";
			this.mergeButton.Size = new System.Drawing.Size(75, 23);
			this.mergeButton.TabIndex = 5;
			this.mergeButton.Text = "Merge";
			this.mergeButton.UseVisualStyleBackColor = true;
			this.mergeButton.Click += new System.EventHandler(this.bMerge_Click);
			// 
			// xmlTextbox
			// 
			this.xmlTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.xmlTextbox.Location = new System.Drawing.Point(5, 34);
			this.xmlTextbox.Multiline = true;
			this.xmlTextbox.Name = "xmlTextbox";
			this.xmlTextbox.Size = new System.Drawing.Size(234, 177);
			this.xmlTextbox.TabIndex = 3;
			this.xmlTextbox.Text = resources.GetString("xmlTextbox.Text");
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.passwordTextbox);
			this.groupBox2.Controls.Add(this.nameTextbox);
			this.groupBox2.Location = new System.Drawing.Point(12, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(394, 81);
			this.groupBox2.TabIndex = 16;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Login Details";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 49);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Password";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 23);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Name";
			// 
			// passwordTextbox
			// 
			this.passwordTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.passwordTextbox.Location = new System.Drawing.Point(65, 46);
			this.passwordTextbox.Name = "passwordTextbox";
			this.passwordTextbox.PasswordChar = '*';
			this.passwordTextbox.Size = new System.Drawing.Size(319, 20);
			this.passwordTextbox.TabIndex = 1;
			// 
			// nameTextbox
			// 
			this.nameTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.nameTextbox.Location = new System.Drawing.Point(65, 20);
			this.nameTextbox.Name = "nameTextbox";
			this.nameTextbox.Size = new System.Drawing.Size(319, 20);
			this.nameTextbox.TabIndex = 0;
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.gridTabPage);
			this.tabControl.Controls.Add(this.xmlTabPage);
			this.tabControl.Location = new System.Drawing.Point(12, 99);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(394, 276);
			this.tabControl.TabIndex = 2;
			// 
			// gridTabPage
			// 
			this.gridTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.gridTabPage.Controls.Add(this.lNewOrgCode);
			this.gridTabPage.Controls.Add(this.oldOrgCodesGrid);
			this.gridTabPage.Controls.Add(this.label1);
			this.gridTabPage.Controls.Add(this.newOrgCodeTextBox);
			this.gridTabPage.Location = new System.Drawing.Point(4, 22);
			this.gridTabPage.Name = "gridTabPage";
			this.gridTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.gridTabPage.Size = new System.Drawing.Size(386, 250);
			this.gridTabPage.TabIndex = 0;
			this.gridTabPage.Text = "Merge From Grid";
			// 
			// xmlTabPage
			// 
			this.xmlTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.xmlTabPage.Controls.Add(this.label4);
			this.xmlTabPage.Controls.Add(this.xmlTextbox);
			this.xmlTabPage.Location = new System.Drawing.Point(4, 22);
			this.xmlTabPage.Name = "xmlTabPage";
			this.xmlTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.xmlTabPage.Size = new System.Drawing.Size(245, 217);
			this.xmlTabPage.TabIndex = 1;
			this.xmlTabPage.Text = "Merge From XML";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(2, 13);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(32, 13);
			this.label4.TabIndex = 15;
			this.label4.Text = "XML:";
			// 
			// OrganizationMergeServiceClientForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(417, 416);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.mergeButton);
			this.Controls.Add(this.groupBox2);
			this.MinimumSize = new System.Drawing.Size(292, 422);
			this.Name = "OrganizationMergeServiceClientForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Organization Merge Client";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OrganizationMergeServiceClientForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.oldOrgCodesGrid)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.gridTabPage.ResumeLayout(false);
			this.gridTabPage.PerformLayout();
			this.xmlTabPage.ResumeLayout(false);
			this.xmlTabPage.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lNewOrgCode;
		private System.Windows.Forms.TextBox newOrgCodeTextBox;
		private System.Windows.Forms.DataGridView oldOrgCodesGrid;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button mergeButton;
		private System.Windows.Forms.TextBox xmlTextbox;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox passwordTextbox;
		private System.Windows.Forms.TextBox nameTextbox;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage gridTabPage;
		private System.Windows.Forms.TabPage xmlTabPage;
		private System.Windows.Forms.Label label4;
	}
}

