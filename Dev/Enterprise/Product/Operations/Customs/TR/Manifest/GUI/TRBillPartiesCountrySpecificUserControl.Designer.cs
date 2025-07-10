using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	partial class TRBillPartiesCountrySpecificUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ToOrderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NotOwnedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Manifest.Business.AsycudaBill);
			// 
			// ToOrderCheckBox
			// 
			this.ToOrderCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ToOrderCheckBox, "IsToOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).IsToOrder)));
			this.ToOrderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ToOrderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 0, true);
			this.ToOrderCheckBox.Name = "ToOrderCheckBox";
			this.ToOrderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ToOrderCheckBox.TabIndex = 11;
			this.ToOrderCheckBox.UseVisualStyleBackColor = true;
			// 
			// NotOwnedCheckBox
			// 
			this.NotOwnedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NotOwnedCheckBox, "NotOwned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).NotOwned)));
			this.NotOwnedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NotOwnedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 0, true);
			this.NotOwnedCheckBox.Name = "NotOwnedCheckBox";
			this.NotOwnedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.NotOwnedCheckBox.TabIndex = 12;
			this.NotOwnedCheckBox.UseVisualStyleBackColor = true;
			// 
			// TRBillPartiesCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ToOrderCheckBox);
			this.Controls.Add(this.NotOwnedCheckBox);
			this.Name = "TRBillPartiesCountrySpecificUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZCheckBox ToOrderCheckBox;
		internal ZCheckBox NotOwnedCheckBox;
	}
}
