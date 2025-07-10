using CargoWise.Windows.UI;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	partial class BillOfLadingImageControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PrincipalGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ImageSelectionControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrincipalGrid)).BeginInit();
			this.PrincipalGrid.SuspendLayout();
			this.ImageSelectionControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BillOfLadingImageCollection);
			// 
			// PrincipalGrid
			// 
			this.PrincipalGrid.AllowNavigation = false;
			this.PrincipalGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PrincipalGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingImage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BillOfLadingImage)(null)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLadingImage)(null)).Principals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLadingImage)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.BillOfLadingImage)(null)).Enabled)));
			zGuidFindBoxColumnStyleInfo1.BindToList = "Principals";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingImageControl|ed27c785-8704-4557-9f0b-fe565976085a", "Principal Org.");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingImageControl|a2d7fc6a-9cb1-4dab-a7cb-508291547515", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingImageControl|cd4d6d8f-3c9b-49d2-97d8-8f602630a55a", "Enabled");
			zCheckBoxColumnStyleInfo1.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.PrincipalGrid.CaptionVisible = false;
			this.PrincipalGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PrincipalGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PrincipalGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PrincipalGrid.GridId = "6f521a13-866e-4b8a-af48-33792eb999bf";
			this.PrincipalGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PrincipalGrid.LayoutKey = "PrincipalGrid";
			this.PrincipalGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PrincipalGrid.Name = "PrincipalGrid";
			this.PrincipalGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 236, true);
			this.PrincipalGrid.TabIndex = 1;
			// 
			// ImageSelectionControl
			//
			this.ImageSelectionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImageSelectionControl, "Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.Freight.Agency.Business.BillOfLadingImage)(null)).Image)));
			this.ImageSelectionControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 169, true);
			this.ImageSelectionControl.Name = "ImageSelectionControl";
			this.ImageSelectionControl.ReadOnly = true;
			this.ImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 156, true);
			this.ImageSelectionControl.TabIndex = 2;
			// 
			// BillOfLadingImageControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ImageSelectionControl);
			this.Controls.Add(this.PrincipalGrid);
			this.Name = "BillOfLadingImageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 395, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrincipalGrid)).EndInit();
			this.PrincipalGrid.ResumeLayout(false);
			this.PrincipalGrid.PerformLayout();
			this.ImageSelectionControl.ResumeLayout(true);
			this.ImageSelectionControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid PrincipalGrid;
		private ImageSelectionControl ImageSelectionControl;
	}
}
