
namespace Enterprise.Customs.US.GUI
{
	partial class StatementAndACHRerouteForm
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
		private new void InitializeComponent()
		{
			this.TranmissionDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ImporterOfRecordNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatementNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientBranchTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentsToRequestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PeriodicStatementPaymentAuthorizationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACHPaymentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ProcessingPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PreliminaryStatementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FinalStatementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GiveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RerouteTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocumentsToRequestGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 301, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 24, true);
			this.MainStatusBar.TabIndex = 13;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute);
			// 
			// TranmissionDateDateEdit
			// 
			this.TranmissionDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.TranmissionDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TranmissionDateDateEdit, "Z9_TranmissionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_TranmissionDate)));
			this.TranmissionDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 64, true);
			this.TranmissionDateDateEdit.Name = "TranmissionDateDateEdit";
			this.TranmissionDateDateEdit.TabIndex = 3;
			// 
			// ImporterOfRecordNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterOfRecordNumberTextBox, "Z9_ImportOfRecordNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_ImportOfRecordNumber)));
			this.ImporterOfRecordNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 90, true);
			this.ImporterOfRecordNumberTextBox.Name = "ImporterOfRecordNumberTextBox";
			this.ImporterOfRecordNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.ImporterOfRecordNumberTextBox.TabIndex = 5;
			// 
			// StatementNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatementNumberTextBox, "Z9_StatementNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_StatementNumber)));
			this.StatementNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 142, true);
			this.StatementNumberTextBox.Name = "StatementNumberTextBox";
			this.StatementNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.StatementNumberTextBox.TabIndex = 9;
			// 
			// ClientBranchTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientBranchTextBox, "Z9_ClientBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_ClientBranch)));
			this.ClientBranchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 116, true);
			this.ClientBranchTextBox.Name = "ClientBranchTextBox";
			this.ClientBranchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.ClientBranchTextBox.TabIndex = 7;
			// 
			// DocumentsToRequestGroupBox
			// 
			this.DocumentsToRequestGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.DocumentsToRequestGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("StatementAndACHRerouteForm|ef26e232-d39c-4a77-8616-c8678c0d4681", "Request Type", "Request Type", "");
			this.DocumentsToRequestGroupBox.Controls.Add(this.PeriodicStatementPaymentAuthorizationCheckBox);
			this.DocumentsToRequestGroupBox.Controls.Add(this.ACHPaymentCheckBox);
			this.DocumentsToRequestGroupBox.Controls.Add(this.ProcessingPortFindBox);
			this.DocumentsToRequestGroupBox.Controls.Add(this.PreliminaryStatementCheckBox);
			this.DocumentsToRequestGroupBox.Controls.Add(this.FinalStatementCheckBox);
			this.DocumentsToRequestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 167, true);
			this.DocumentsToRequestGroupBox.Name = "DocumentsToRequestGroupBox";
			this.DocumentsToRequestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 131, true);
			this.DocumentsToRequestGroupBox.TabIndex = 10;
			this.DocumentsToRequestGroupBox.TabStop = false;
			// 
			// PeriodicStatementPaymentAuthorizationCheckBox
			// 
			this.PeriodicStatementPaymentAuthorizationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PeriodicStatementPaymentAuthorizationCheckBox, "Z9_PeriodicStatementPaymentAuthorizationRequest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_PeriodicStatementPaymentAuthorizationRequest)));
			this.PeriodicStatementPaymentAuthorizationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PeriodicStatementPaymentAuthorizationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 109, true);
			this.PeriodicStatementPaymentAuthorizationCheckBox.Name = "PeriodicStatementPaymentAuthorizationCheckBox";
			this.PeriodicStatementPaymentAuthorizationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 17, true);
			this.PeriodicStatementPaymentAuthorizationCheckBox.TabIndex = 9;
			this.PeriodicStatementPaymentAuthorizationCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACHPaymentCheckBox
			// 
			this.ACHPaymentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ACHPaymentCheckBox, "Z9_ACHPaymentRequest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_ACHPaymentRequest)));
			this.ACHPaymentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ACHPaymentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 86, true);
			this.ACHPaymentCheckBox.Name = "ACHPaymentCheckBox";
			this.ACHPaymentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 17, true);
			this.ACHPaymentCheckBox.TabIndex = 7;
			this.ACHPaymentCheckBox.UseVisualStyleBackColor = true;
			// 
			// ProcessingPortFindBox
			// 
			this.BindingSource.SetBindingMember(this.ProcessingPortFindBox, "Z9_ProcessingPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_ProcessingPort)));
			this.ProcessingPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 16, true);
			this.ProcessingPortFindBox.Name = "ProcessingPortFindBox";
			this.ProcessingPortFindBox.BindToList = "Lookups+Z9_ProcessingPortList";
			this.ProcessingPortFindBox.PreBoundMaxLength = 4;
			this.ProcessingPortFindBox.ShowDescriptionBox = true;
			this.ProcessingPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.ProcessingPortFindBox.TabIndex = 3;
			// 
			// PreliminaryStatementCheckBox
			// 
			this.PreliminaryStatementCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PreliminaryStatementCheckBox, "Z9_PreliminaryStatementRequest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_PreliminaryStatementRequest)));
			this.PreliminaryStatementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PreliminaryStatementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 42, true);
			this.PreliminaryStatementCheckBox.Name = "PreliminaryStatementCheckBox";
			this.PreliminaryStatementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 100, true);
			this.PreliminaryStatementCheckBox.TabIndex = 3;
			this.PreliminaryStatementCheckBox.UseVisualStyleBackColor = true;
			// 
			// FinalStatementCheckBox
			// 
			this.FinalStatementCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FinalStatementCheckBox, "Z9_FinalStatementRequest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_FinalStatementRequest)));
			this.FinalStatementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FinalStatementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 64, true);
			this.FinalStatementCheckBox.Name = "FinalStatementCheckBox";
			this.FinalStatementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.FinalStatementCheckBox.TabIndex = 5;
			this.FinalStatementCheckBox.UseVisualStyleBackColor = true;
			// 
			// GiveUpButton
			// 
			this.GiveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GiveUpButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("StatementAndACHRerouteForm|d985a237-db24-40db-856b-8cdb9d4fdc40", "&Cancel");
			this.GiveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GiveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 298, true);
			this.GiveUpButton.Name = "GiveUpButton";
			this.GiveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GiveUpButton.TabIndex = 12;
			this.GiveUpButton.UseVisualStyleBackColor = true;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("StatementAndACHRerouteForm|342c7191-55dd-415d-908f-fc8e1fc75dce", "&Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 298, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 11;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// RerouteTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.RerouteTypeDropEdit, "Z9_RerouteType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_RerouteType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Lookups.Z9_RerouteTypeList)));
			this.RerouteTypeDropEdit.BindToList = "Lookups+Z9_RerouteTypeList";
			this.RerouteTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 38, true);
			this.RerouteTypeDropEdit.Name = "RerouteTypeDropEdit";
			this.RerouteTypeDropEdit.PreBoundMaxLength = 1;
			this.RerouteTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.RerouteTypeDropEdit.TabIndex = 2;
			// 
			// MessageTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "Z9_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Z9_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute)(null)).Lookups.Z9_MessageTypeList)));
			this.MessageTypeDropEdit.BindToList = "Lookups+Z9_MessageTypeList";
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 12, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.PreBoundMaxLength = 1;
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.MessageTypeDropEdit.TabIndex = 1;
			// 
			// StatementAndACHRerouteForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 350, true);
			this.Controls.Add(this.MessageTypeDropEdit);
			this.Controls.Add(this.RerouteTypeDropEdit);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.GiveUpButton);
			this.Controls.Add(this.DocumentsToRequestGroupBox);
			this.Controls.Add(this.ClientBranchTextBox);
			this.Controls.Add(this.StatementNumberTextBox);
			this.Controls.Add(this.ImporterOfRecordNumberTextBox);
			this.Controls.Add(this.TranmissionDateDateEdit);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.MessageBuilders.StatementAndACHPaymentReroute";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 385, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 385, true);
			this.Name = "StatementAndACHRerouteForm";
			this.Text = "Re-Route Request";
			this.Controls.SetChildIndex(this.TranmissionDateDateEdit, 0);
			this.Controls.SetChildIndex(this.ImporterOfRecordNumberTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.StatementNumberTextBox, 0);
			this.Controls.SetChildIndex(this.ClientBranchTextBox, 0);
			this.Controls.SetChildIndex(this.DocumentsToRequestGroupBox, 0);
			this.Controls.SetChildIndex(this.GiveUpButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.RerouteTypeDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocumentsToRequestGroupBox.ResumeLayout(false);
			this.DocumentsToRequestGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDateEdit TranmissionDateDateEdit;
		private Enterprise.ZArchitecture.ZTextBox ImporterOfRecordNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox StatementNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox ClientBranchTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DocumentsToRequestGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox PeriodicStatementPaymentAuthorizationCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox ACHPaymentCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox FinalStatementCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox PreliminaryStatementCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ProcessingPortFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton GiveUpButton;
		private Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RerouteTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
	}
}
