namespace Enterprise.Customs.NZ.GUI.Declaration
{
	partial class NZStaffCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BrokerGroupNZ = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GS_BrokerIDBoundTextNZ = new Enterprise.ZArchitecture.ZTextBox();
			this.GS_Calc_DecryptBrokerPasswordBoundTextNZ = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BrokerGroupNZ.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.NZGlbStaffWrapper);
			// 
			// BrokerGroupNZ
			// 
			this.BrokerGroupNZ.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("GlbStaffForm|acc618fb-48bf-49f0-95f4-6004291d9136", "Broker");
			this.BrokerGroupNZ.Controls.Add(this.GS_BrokerIDBoundTextNZ);
			this.BrokerGroupNZ.Controls.Add(this.GS_Calc_DecryptBrokerPasswordBoundTextNZ);
			this.BrokerGroupNZ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.BrokerGroupNZ.Name = "BrokerGroupNZ";
			this.BrokerGroupNZ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 72, true);
			this.BrokerGroupNZ.TabIndex = 4;
			this.BrokerGroupNZ.TabStop = false;
			// 
			// GS_BrokerIDBoundTextNZ
			// 
			this.BindingSource.SetBindingMember(this.GS_BrokerIDBoundTextNZ, "NZBPassword.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.NZGlbStaffWrapper)(null)).NZBPassword.GP_UserID)));
			this.GS_BrokerIDBoundTextNZ.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("GlbStaffForm|0e8bfb12-aa06-4769-b2ee-b9a0c60a929e", "Broker ID");
			this.GS_BrokerIDBoundTextNZ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 20, true);
			this.GS_BrokerIDBoundTextNZ.Name = "GS_BrokerIDBoundTextNZ";
			this.GS_BrokerIDBoundTextNZ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GS_BrokerIDBoundTextNZ.TabIndex = 0;
			// 
			// GS_Calc_DecryptBrokerPasswordBoundTextNZ
			// 
			this.BindingSource.SetBindingMember(this.GS_Calc_DecryptBrokerPasswordBoundTextNZ, "NZBPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.NZGlbStaffWrapper)(null)).NZBPassword.CurrentDecryptedPassword)));
			this.GS_Calc_DecryptBrokerPasswordBoundTextNZ.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("GlbStaffForm|8a3b74a1-3764-47ae-a90c-4150068c7fca", "Password");
			this.GS_Calc_DecryptBrokerPasswordBoundTextNZ.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GS_Calc_DecryptBrokerPasswordBoundTextNZ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 44, true);
			this.GS_Calc_DecryptBrokerPasswordBoundTextNZ.Name = "GS_Calc_DecryptBrokerPasswordBoundTextNZ";
			this.GS_Calc_DecryptBrokerPasswordBoundTextNZ.PasswordChar = '*';
			this.GS_Calc_DecryptBrokerPasswordBoundTextNZ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GS_Calc_DecryptBrokerPasswordBoundTextNZ.TabIndex = 1;
			// 
			// NZStaffCredentialsUserControl
			// 
			this.Controls.Add(this.BrokerGroupNZ);
			this.Name = "NZStaffCredentialsUserControl";
			this.Controls.SetChildIndex(this.BrokerGroupNZ, 0);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BrokerGroupNZ.ResumeLayout(false);
			this.BrokerGroupNZ.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		Enterprise.ZArchitecture.GUI.ZGroupBox BrokerGroupNZ;
		Enterprise.ZArchitecture.ZTextBox GS_BrokerIDBoundTextNZ;
		Enterprise.ZArchitecture.ZTextBox GS_Calc_DecryptBrokerPasswordBoundTextNZ;

		#endregion
	}
}
