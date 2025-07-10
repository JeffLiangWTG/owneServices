namespace Enterprise.Customs.PL.ExitControl.GUI
{
	partial class HeaderDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TrainingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.StoringFlagCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.BrokerCodeFindBox.SuspendLayout();
            this.CertificateDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.ExitControl.Business.CusExitHeader);
            // 
            // BrokerCodeFindBox
            // 
            this.BrokerCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "CXH_GS_NKCustomsAgent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.ExitControl.Business.CusExitHeader)(null)).CXH_GS_NKCustomsAgent)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.ExitControl.Business.CusExitHeader)(null)).Lookups.CustomsAgents)));
            this.BrokerCodeFindBox.BindToList = "Lookups+CustomsAgents";
            this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.PL.ExitControl.GUI.Res.GetData("E8168F7D-FC9A-456B-B8F4-E25AE8121037", "Broker", "Broker", "Broker", "The broker selected will be the responsible of declarations to Customs in this Jo" +
        "b");
            this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 21, true);
            this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
            this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
            this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BrokerCodeFindBox.ParentType = null;
            this.BrokerCodeFindBox.PreBoundMaxLength = 3;
            this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 19, true);
            this.BrokerCodeFindBox.TabIndex = 3;
            // 
            // CertificateDropEdit
            // 
            this.CertificateDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CertificateDropEdit, "CXH_CustomsProfile");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.ExitControl.Business.CusExitHeader)(null)).CXH_CustomsProfile)));
            this.CertificateDropEdit.CaptionResourceString = Enterprise.Customs.PL.ExitControl.GUI.Res.GetData("B340EBE8-111F-46BB-81C1-DB914657DF7F", "Cert.", "Certif.", "Certificate", "The certificate selected will be used to sign and communicate with Customs to dec" +
        "lare all entries in this Job");
            this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 52, true);
            this.CertificateDropEdit.Name = "CertificateDropEdit";
            this.CertificateDropEdit.PreBoundMaxLength = 27;
            this.CertificateDropEdit.ShowDescriptionBox = false;
            this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 19, true);
            this.CertificateDropEdit.TabIndex = 4;
            // 
            // TrainingCheckBox
            // 
            this.BindingSource.SetBindingMember(this.TrainingCheckBox, "TrainingEntry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.PL.ExitControl.Business.CusExitHeader)(null)).TrainingEntry)));
            this.TrainingCheckBox.CaptionResourceString = Enterprise.Customs.PL.ExitControl.GUI.Res.GetData("1A690CDD-5311-40AE-8619-B7803791352E", "Training Entry", "Training Entry", "Training Entry", "When checked the declaration will be sent to Test");
            this.TrainingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
            this.TrainingCheckBox.Name = "TrainingCheckBox";
            this.TrainingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
            this.TrainingCheckBox.TabIndex = 4;
            this.TrainingCheckBox.UseVisualStyleBackColor = true;
            // 
            // StoringFlagCheckBox
            // 
            this.BindingSource.SetBindingMember(this.StoringFlagCheckBox, "StoringFlag");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.PL.ExitControl.Business.CusExitHeader)(null)).StoringFlag)));
            this.StoringFlagCheckBox.CaptionResourceString = Enterprise.Customs.PL.ExitControl.GUI.Res.GetData("0EACDB67-24BE-4F10-9A6F-316A7362FC15", "Storing Flag");
            this.StoringFlagCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 108, true);
            this.StoringFlagCheckBox.Name = "StoringFlagCheckBox";
            this.StoringFlagCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
            this.StoringFlagCheckBox.TabIndex = 5;
            this.StoringFlagCheckBox.UseVisualStyleBackColor = true;
            // 
            // HeaderDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.StoringFlagCheckBox);
            this.Controls.Add(this.BrokerCodeFindBox);
            this.Controls.Add(this.CertificateDropEdit);
            this.Controls.Add(this.TrainingCheckBox);
            this.Name = "HeaderDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 274, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.BrokerCodeFindBox.ResumeLayout(true);
            this.BrokerCodeFindBox.PerformLayout();
            this.CertificateDropEdit.ResumeLayout(true);
            this.CertificateDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
		internal ZArchitecture.GUI.ZCheckBox TrainingCheckBox;
		internal ZArchitecture.GUI.ZCheckBox StoringFlagCheckBox;
	}
}
