using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	partial class SendCargoManifestEntryStatusQueryActionControl
	{
		private void InitializeComponent()
		{
			this.sendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.requestForReleatedBOL = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.updateEntryWithResults = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.action = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.outputOption = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.sendGroupBox.SuspendLayout();
			this.SuspendLayout();

			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Module.OperationalActions.SendCargoManifestEntryStatusQueryActionMethodApplicator);
			// 
			// SendGroupBox
			// 
			this.sendGroupBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("4a5a835a-1c1c-43a5-adb6-4535b5728066", "Send Messages");
			this.sendGroupBox.Controls.Add(this.requestForReleatedBOL);
			this.sendGroupBox.Controls.Add(this.updateEntryWithResults);
			this.sendGroupBox.Controls.Add(this.action);
			this.sendGroupBox.Controls.Add(this.outputOption);
			this.sendGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.sendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sendGroupBox.Name = "SendGroupBox";
			this.sendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 146, true);
			this.sendGroupBox.TabIndex = 0;
			this.sendGroupBox.TabStop = false;
			// 
			// Action
			//
			this.action.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.action, "Action");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.OperationalActions.SendCargoManifestEntryStatusQueryActionMethodApplicator)(null)).Action)));
			this.action.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("3717DCEF-FD62-4FBA-AF4A-77363B6F4C72", "Action");
			this.action.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 20, true);
			this.action.Name = "Action";
			this.action.PreBoundMaxLength = 3;
			this.action.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 16, true);
			this.action.TabIndex = 1;
			// 
			// RequestForReleatedBOL
			// 
			this.BindingSource.SetBindingMember(this.requestForReleatedBOL, "RequestForReleatedBOL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendCargoManifestEntryStatusQueryActionMethodApplicator)(null)).RequestForReleatedBOL)));
			this.requestForReleatedBOL.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("86F49ED5-2EBA-418F-A1BE-0F19F57520E9", "Request For Related BOL");
			this.requestForReleatedBOL.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.requestForReleatedBOL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 45, true);
			this.requestForReleatedBOL.Name = "RequestForReleatedBOL";
			this.requestForReleatedBOL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 16, true);
			this.requestForReleatedBOL.TabIndex = 2;
			this.requestForReleatedBOL.UseVisualStyleBackColor = true;
			// 
			// UpdateEntryWithResults
			// 
			this.BindingSource.SetBindingMember(this.updateEntryWithResults, "UpdateEntryWithResults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendCargoManifestEntryStatusQueryActionMethodApplicator)(null)).UpdateEntryWithResults)));
			this.updateEntryWithResults.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("26FB14A5-E386-4681-8053-14F5E23F4DA2", "Update Entry With Results");
			this.updateEntryWithResults.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.updateEntryWithResults.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 70, true);
			this.updateEntryWithResults.Name = "UpdateEntryWithResults";
			this.updateEntryWithResults.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 16, true);
			this.updateEntryWithResults.TabIndex = 3;
			this.updateEntryWithResults.UseVisualStyleBackColor = true;
			// 
			// OutputOption
			//
			this.outputOption.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.outputOption, "OutputOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.OperationalActions.SendCargoManifestEntryStatusQueryActionMethodApplicator)(null)).OutputOption)));
			this.outputOption.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("BEF3169B-A13B-49B3-9796-A9251C3C6B94", "Output Option");
			this.outputOption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 95, true);
			this.outputOption.Name = "OutputOption";
			this.outputOption.PreBoundMaxLength = 21;
			this.outputOption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 16, true);
			this.outputOption.TabIndex = 4;
			// 
			// USDeclarationOperationActionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.sendGroupBox);
			this.Name = "USDeclarationOperationActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.sendGroupBox.ResumeLayout(false);
			this.sendGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZGroupBox sendGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit action;
		Enterprise.ZArchitecture.GUI.ZDropEdit outputOption;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox requestForReleatedBOL;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox updateEntryWithResults;
	}
}
