using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class CopyRecipientsEmailCollectionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;
		internal Enterprise.ZArchitecture.ZGrid Grid_CopyRecipients;
		Enterprise.ZArchitecture.GUI.ZButton Button_Close;
		Enterprise.ZArchitecture.GUI.ZButton Button_Ok;
		ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1;

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
		protected new void InitializeComponent()
		{
			this.Grid_CopyRecipients = new Enterprise.ZArchitecture.ZGrid();
			this.Button_Close = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Button_Ok = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid_CopyRecipients)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 234, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BusinessObjectCollection);
			// 
			// OrderItemsGrid
			// 
			this.Grid_CopyRecipients.AllowNavigation = false;
			this.Grid_CopyRecipients.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.Grid_CopyRecipients.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CopyRecipientsEmailCollectionForm|2A916993-FFB3-4792-8278-73644DB96007", "Email Address");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDropEditColumnStyleInfo1.ColumnName = "123";
			this.Grid_CopyRecipients.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid_CopyRecipients.GridId = "7330f16e-be25-4af9-9d67-92389c4de509";
			this.Grid_CopyRecipients.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid_CopyRecipients.LayoutKey = "Grid_CopyRecipients";
			this.Grid_CopyRecipients.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid_CopyRecipients.Name = "Grid_CopyRecipients";
			this.Grid_CopyRecipients.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 206, true);
			this.Grid_CopyRecipients.TabIndex = 1;
			this.Grid_CopyRecipients.AllowSorting = false;
			// 
			// OKButton
			// 
			this.Button_Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Button_Ok.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CopyRecipientsEmailCollectionForm|9330BEF3-3178-4EFB-BBBE-053F51F4A28E", "OK");
			this.Button_Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Button_Ok.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 211, true);
			this.Button_Ok.Name = "Button_Ok";
			this.Button_Ok.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.Button_Ok.TabIndex = 2;
			this.Button_Ok.Click += new System.EventHandler(Button_Ok_Click);
			// CloseButton
			this.Button_Close.Visible = false;
			// 
			// CopyRecipientsEmailCollectionForm
			// 
			this.AcceptButton = this.Button_Ok;
			this.CancelButton = this.Button_Close;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CopyRecipientsEmailCollectionForm|32CE8F7A-4D69-4873-ABA5-F3CB43AB552C", "Email Addresses");
			this.Controls.Add(this.Button_Ok);
			this.Controls.Add(this.Button_Close);
			this.Controls.Add(this.Grid_CopyRecipients);
			this.Name = "CopyRecipientsEmailCollectionForm";
			this.Controls.SetChildIndex(this.Grid_CopyRecipients, 0);
			this.Controls.SetChildIndex(this.Button_Close, 0);
			this.Controls.SetChildIndex(this.Button_Ok, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid_CopyRecipients)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}