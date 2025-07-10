namespace Enterprise.Freight.Agency.GUI
{
	partial class AllowSendingBookingConfirmationControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PrincipalGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrincipalGrid)).BeginInit();
			this.PrincipalGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// PrincipalGrid
			// 
			this.PrincipalGrid.AllowNavigation = false;
			this.PrincipalGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
						| System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PrincipalGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AllowSendingBookingConfirmation)(null)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AllowSendingBookingConfirmation)(null)).Principals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AllowSendingBookingConfirmation)(null)).Enabled)));
			this.PrincipalGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Principals";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllowSendingBookingConfirmationControl|148676E9-C3A3-4F75-BA5F-DC8B9A94D803", "Principal Org.");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllowSendingBookingConfirmationControl|64B297F8-3341-434F-BA1C-9966C9D05AD8", "Enabled");
			zCheckBoxColumnStyleInfo1.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.PrincipalGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PrincipalGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PrincipalGrid.GridId = "32EAA9AB-56A6-403C-877F-61C17B22CD74";
			this.PrincipalGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PrincipalGrid.LayoutKey = "PrincipalGrid";
			this.PrincipalGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PrincipalGrid.Name = "PrincipalGrid";
			this.PrincipalGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 395, true);
			this.PrincipalGrid.TabIndex = 1;
			// 
			// AllowSendingBookingConfirmationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PrincipalGrid);
			this.Name = "AllowSendingBookingConfirmationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 395, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrincipalGrid)).EndInit();
			this.PrincipalGrid.ResumeLayout(false);
			this.PrincipalGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid PrincipalGrid;
	}
}
