namespace Enterprise.ContractManagement.Module
{
	partial class LoadDischargeFilterControl
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
			this.OriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DestinationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShowRelatedUNLOCOsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DestinationFindBox.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ContractManagement.Module.AllocationRouteCoveringLocationFilter);
			// 
			// OriginFindBox
			//
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AllocationRouteCoveringLocationFilter)(null)).Property1)));
			this.OriginFindBox.AllowDrop = true;
			this.OriginFindBox.ShowDescriptionBox = false;
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 1, true);
			this.OriginFindBox.Name = "OriginFindBox";
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.OriginFindBox.TabIndex = 1;
			// 
			// DestinationFindBox
			//
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AllocationRouteCoveringLocationFilter)(null)).Property2)));
			this.DestinationFindBox.AllowDrop = true;
			this.DestinationFindBox.ShowDescriptionBox = false;
			this.DestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 1, true);
			this.DestinationFindBox.Name = "DestinationFindBox";
			this.DestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.DestinationFindBox.TabIndex = 2;
			// 
			// ShowRelatedUNLOCOsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ShowRelatedUNLOCOsCheckBox, "ShowRelatedUNLOCOs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ContractManagement.Module.AllocationRouteCoveringLocationFilter)(null)).ShowRelatedUNLOCOs)));
			this.ShowRelatedUNLOCOsCheckBox.CaptionResourceString = Enterprise.ContractManagement.Module.Res.GetData("54fc9ca4-5753-2f98-4645-766dbb67957e", "Show Related UNLOCOs");
			this.ShowRelatedUNLOCOsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 24, true);
			this.ShowRelatedUNLOCOsCheckBox.Name = "ShowRelatedUNLOCOsCheckBox";
			this.ShowRelatedUNLOCOsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 24, true);
			this.ShowRelatedUNLOCOsCheckBox.TabIndex = 3;
			//
			// LoadDischargeFilterControl
			// 
			this.Controls.Add(this.DestinationFindBox);
			this.Controls.Add(this.OriginFindBox);
			this.Controls.Add(this.ShowRelatedUNLOCOsCheckBox);
			this.Name = "LoadDischargeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 48, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DestinationFindBox.ResumeLayout(true);
			this.DestinationFindBox.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox OriginFindBox;
		private ZArchitecture.GUI.ZCodeFindBox DestinationFindBox;
		private ZArchitecture.GUI.ZCheckBox ShowRelatedUNLOCOsCheckBox;
	}
}
