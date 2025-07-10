namespace Enterprise.Rating.GUI
{
	partial class MapChargeCodesForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ChargeCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeGrid)).BeginInit();
			this.ChargeCodeGrid.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.Rating.Services.UniversalChargeCodeMapBizo);
			// 
			// ChargeCodeGrid
			// 
			this.ChargeCodeGrid.AllowNavigation = false;
			this.ChargeCodeGrid.AllowSorting = false;
			this.ChargeCodeGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChargeCodeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.Rating.Services.UniversalChargeCodeMapBizo)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.Services.UniversalChargeCodeMapBizo)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.Rating.Services.UniversalChargeCodeMapBizo)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.Rating.Services.UniversalChargeCodeMapBizo)(null)).GlobalChargeCodePk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.Rating.Services.UniversalChargeCodeMapBizo)(null)).LocalChargeCodePk)));
			this.ChargeCodeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("7bcaf638-057e-4c89-b3d9-620959af19ad", "Universal Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("c391d21d-47d6-4699-abed-6bf4a5af7702", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("dee4e444-3f66-442b-bdf5-aa7187fe0b40", "Global Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GlobalChargeCodePk";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccGlobalChargeCode;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("8c864eec-fd20-47cb-a0ff-92285e4c2fc8", "Charge Code");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "LocalChargeCodePk";
			zGuidFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCode;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ChargeCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargeCodeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargeCodeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ChargeCodeGrid.GridId = "64f6b0c2-ee73-4bdc-85f2-1ad06d74d0bc";
			this.ChargeCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodeGrid.LayoutKey = "RateLineItemsGridHousebill";
			this.ChargeCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 63, true);
			this.ChargeCodeGrid.Name = "ChargeCodeGrid";
			this.ChargeCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 179, true);
			this.ChargeCodeGrid.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 249, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 25, true);
			this.PostingButtonsUserControl.TabIndex = 10;
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("711d3852-d553-4909-a535-31515e15ecde", "The Universal Code of the following Charges have NOT been assigned to any Global Charge Code or Charge Code.  Would you like to complete the assignment for applying the Rates to the job?");
			this.InstructionsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 6, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 45, true);
			this.InstructionsLabel.TabIndex = 11;
			// 
			// MapChargeCodesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("6d1049b2-731f-4c26-ae8b-da06bcc6f7ff", "Map Charge Codes");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 300, true);
			this.Controls.Add(this.InstructionsLabel);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.ChargeCodeGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.Rating.Services.UniversalChargeCodeMapBizo);
			this.IsPostOnly = true;
			this.Name = "MapChargeCodesForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ChargeCodeGrid, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.InstructionsLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeGrid)).EndInit();
			this.ChargeCodeGrid.ResumeLayout(false);
			this.ChargeCodeGrid.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ChargeCodeGrid;
		internal Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.ZLabel InstructionsLabel;
	}
}