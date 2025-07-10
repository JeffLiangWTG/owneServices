using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.PL.GUI
{
	partial class InvoiceLineDetailsUserControl
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
			this.CountryOfSupplyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CPCUserControl = new Enterprise.Customs.PL.GUI.CPCUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryOfSupplyCodeFindBox.SuspendLayout();
			this.CPCUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine);
			// 
			// CountryOfSupplyCodeFindBox
			// 
			this.CountryOfSupplyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfSupplyCodeFindBox, "ZG_CountryOfSupply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine)(null)).ZG_CountryOfSupply)));
			this.CountryOfSupplyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 30, true);
			this.CountryOfSupplyCodeFindBox.Name = "CountryOfSupplyCodeFindBox";
			this.CountryOfSupplyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryOfSupplyCodeFindBox.ParentType = null;
			this.CountryOfSupplyCodeFindBox.PreBoundMaxLength = 4;
			this.CountryOfSupplyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CountryOfSupplyCodeFindBox.TabIndex = 5;
			// 
			// CPCUserControl
			// 
			this.CPCUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCUserControl, ".");
			this.CPCUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 2, true);
			this.CPCUserControl.Name = "CPCUserControl";
			this.CPCUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.CPCUserControl.TabIndex = 0;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CountryOfSupplyCodeFindBox);
			this.Controls.Add(this.CPCUserControl);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryOfSupplyCodeFindBox.ResumeLayout(true);
			this.CountryOfSupplyCodeFindBox.PerformLayout();
			this.CPCUserControl.ResumeLayout(true);
			this.CPCUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfSupplyCodeFindBox;
		public PL.GUI.CPCUserControl CPCUserControl;
	}
}
