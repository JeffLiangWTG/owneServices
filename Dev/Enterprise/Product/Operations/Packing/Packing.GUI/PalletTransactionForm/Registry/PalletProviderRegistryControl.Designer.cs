using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.GUI
{
	partial class PalletProviderRegistryControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ProvidersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProvidersGrid)).BeginInit();
			this.ProvidersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Packing.Business.PalletTypeParent);
			// 
			// ProvidersGrid
			// 
			this.ProvidersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProvidersGrid, "Types");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Packing.Business.PalletTypeParent)(null)).Types)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PalletType)(((System.Collections.IList)(((Enterprise.Packing.Business.PalletTypeParent)(null)).Types)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PalletType)(((System.Collections.IList)(((Enterprise.Packing.Business.PalletTypeParent)(null)).Types)).SyncRoot)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PalletType)(((System.Collections.IList)(((Enterprise.Packing.Business.PalletTypeParent)(null)).Types)).SyncRoot)).ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PalletType)(((System.Collections.IList)(((Enterprise.Packing.Business.PalletTypeParent)(null)).Types)).SyncRoot)).EquipmentCode)));
			this.ProvidersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("b423a896-aee2-44f5-a933-d1b16f7045cb", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("2b434155-5f8b-4c97-8577-16217be089ac", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("b5ea0ba4-6e97-4577-831c-a00c03e1665a", "Provider Code");
			zTextBoxColumnStyleInfo3.ColumnName = "ProviderCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("7cf9d268-6026-442c-9cab-e3d6c287cc6f", "Equipment Code");
			zTextBoxColumnStyleInfo4.ColumnName = "EquipmentCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.ProvidersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProvidersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProvidersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProvidersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ProvidersGrid.CopySelectedRowsAllowed = true;
			this.ProvidersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProvidersGrid.GridId = "98b84ad2-aff8-4145-83f8-97a7226bcc5a";
			this.ProvidersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProvidersGrid.LayoutKey = "zGrid1";
			this.ProvidersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProvidersGrid.Name = "ProvidersGrid";
			this.ProvidersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			this.ProvidersGrid.TabIndex = 0;
			// 
			// PalletProviderRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProvidersGrid);
			this.Name = "PalletProviderRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProvidersGrid)).EndInit();
			this.ProvidersGrid.ResumeLayout(false);
			this.ProvidersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid ProvidersGrid;

	}
}
