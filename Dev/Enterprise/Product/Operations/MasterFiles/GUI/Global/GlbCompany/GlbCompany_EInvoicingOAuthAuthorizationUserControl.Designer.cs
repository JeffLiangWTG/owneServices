namespace Enterprise.MasterFiles.GUI
{
	partial class GlbCompany_EInvoicingOAuthAuthorizationUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TokenManagementGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AuthorizeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ReloadButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TokenManagementGrid)).BeginInit();
			this.TokenManagementGrid.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.GlbCompany);
			// 
			// zPanel1
			// 
			this.zPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel1.Controls.Add(this.zGroupBox1);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1608, 673, true);
			this.zPanel1.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9d5657d5-2522-49b5-9d79-53830b2c91b4", "Token Management");
			this.zGroupBox1.Controls.Add(this.TokenManagementGrid);
			this.zGroupBox1.Controls.Add(this.zPanel2);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1608, 673, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.UseCompatibleTextRendering = true;
			// 
			// TokenManagementGrid
			//
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoiceOAuthCredential)(null)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoiceOAuthCredential)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.EInvoiceOAuthCredential)(null)).AuthorizationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.EInvoiceOAuthCredential)(null)).IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.EInvoiceOAuthCredential)(null)).ExpiryDate)));
			this.TokenManagementGrid.AllowNavigation = false;
			this.TokenManagementGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("397c78d5-bba8-4190-9f7c-42280e81fb52", "Token Status");
			zTextBoxColumnStyleInfo1.ColumnName = "Status";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("08995346-3af1-484b-b2ff-198d2d7028e7", "Authorization Date");
			zTextBoxColumnStyleInfo2.ColumnName = "AuthorizationDate";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("14026DDC-A2D5-4E83-AFE9-A7BCF826FF4C", "Token Issue Date");
			zTextBoxColumnStyleInfo3.ColumnName = "IssueDate";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1adf1c14-4720-4984-a401-bd9f8185e4a1", "Token Expiry Date");
			zTextBoxColumnStyleInfo4.ColumnName = "ExpiryDate";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F14CB49E-CEFA-415B-B935-9F208F8482D8", "Description");
			zTextBoxColumnStyleInfo5.ColumnName = "Description";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.TokenManagementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);	
			this.TokenManagementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TokenManagementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TokenManagementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TokenManagementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TokenManagementGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TokenManagementGrid.GridId = "e4968a24-f7c7-4a0d-927c-11f5b83a65b1";
			this.TokenManagementGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TokenManagementGrid.LayoutKey = "zGrid1";
			this.TokenManagementGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 32, true);
			this.TokenManagementGrid.Name = "TokenManagementGrid";
			this.TokenManagementGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1603, 589, true);
			this.TokenManagementGrid.TabIndex = 0;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.ReloadButton);
			this.zPanel2.Controls.Add(this.AuthorizeButton);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 620, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1602, 50, true);
			this.zPanel2.TabIndex = 1;
			// 
			// AuthorizeButton
			// 
			this.AuthorizeButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0bcb52b9-4b17-4ca7-b9d5-81f79a6c728b", "Authorize");
			this.AuthorizeButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.AuthorizeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1489, 0, true);
			this.AuthorizeButton.Name = "AuthorizeButton";
			this.AuthorizeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 50, true);
			this.AuthorizeButton.TabIndex = 1;
			this.AuthorizeButton.ToolTipCaption = null;
			this.AuthorizeButton.UseVisualStyleBackColor = true;
			this.AuthorizeButton.Click += new System.EventHandler(this.AuthorizeButton_Click);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
			// 
			// RefreshButton
			//
			this.ReloadButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("A4EEA892-3E00-477E-A4ED-27BF8EED61D4", "Reload");
			this.ReloadButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.ReloadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1376, 0, true);
			this.ReloadButton.Name = "ReloadButton";
			this.ReloadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 50, true);
			this.ReloadButton.TabIndex = 0;
			this.ReloadButton.ToolTipCaption = null;
			this.ReloadButton.UseVisualStyleBackColor = true;
			this.ReloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
			// 
			// GlbCompany_EInvoicingOAuthAuthorizationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Name = "GlbCompany_RomaniaCredentialUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1608, 673, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TokenManagementGrid)).EndInit();
			this.TokenManagementGrid.ResumeLayout(false);
			this.TokenManagementGrid.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private ZArchitecture.ZGrid TokenManagementGrid;
		private ZArchitecture.GUI.ZPanel zPanel2;
		private ZArchitecture.GUI.ZButton AuthorizeButton;
		private ZArchitecture.GUI.ZButton ReloadButton;
	}
}
