namespace Enterprise.Customs.NL.GUI
{
	partial class ShipmentDetailsUserControl
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
			this.ShipmentDetailsIncoTermsUserControl = new Enterprise.Customs.NL.GUI.ShipmentDetailsIncoTermsUserControl();
			this.AgreedPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentDetailsIncoTermsUserControl.SuspendLayout();
			this.AgreedPlaceCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			// 
			// ShipmentDetailsIncoTermsUserControl
			// 
			this.ShipmentDetailsIncoTermsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsIncoTermsUserControl, ".");
			this.ShipmentDetailsIncoTermsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 67, true);
			this.ShipmentDetailsIncoTermsUserControl.Name = "ShipmentDetailsIncoTermsUserControl";
			this.ShipmentDetailsIncoTermsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 23, true);
			this.ShipmentDetailsIncoTermsUserControl.TabIndex = 1;
			// 
			// AgreedPlaceCodeFindBox
			// 
			this.AgreedPlaceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedPlaceCodeFindBox, "EUD_AgreedPlaceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).EUD_AgreedPlaceCode)));
			this.AgreedPlaceCodeFindBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("aace86eb-c625-4b82-8167-30f00ae05400", "", "Place Code", "Incoterm Place Code", "");
			this.AgreedPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 98, true);
			this.AgreedPlaceCodeFindBox.Name = "AgreedPlaceCodeFindBox";
			this.AgreedPlaceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AgreedPlaceCodeFindBox.ParentType = null;
			this.AgreedPlaceCodeFindBox.PreBoundMaxLength = 5;
			this.AgreedPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 17, true);
			this.AgreedPlaceCodeFindBox.TabIndex = 8;
			// 
			// ShipmentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AgreedPlaceCodeFindBox);
			this.Controls.Add(this.ShipmentDetailsIncoTermsUserControl);
			this.Name = "ShipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 124, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentDetailsIncoTermsUserControl.ResumeLayout(true);
			this.ShipmentDetailsIncoTermsUserControl.PerformLayout();
			this.AgreedPlaceCodeFindBox.ResumeLayout(true);
			this.AgreedPlaceCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ShipmentDetailsIncoTermsUserControl ShipmentDetailsIncoTermsUserControl;
		internal ZArchitecture.GUI.ZCodeFindBox AgreedPlaceCodeFindBox;
	}
}
