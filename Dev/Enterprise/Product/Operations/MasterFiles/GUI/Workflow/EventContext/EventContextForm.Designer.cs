namespace Enterprise.MasterFiles.GUI
{
	partial class EventContextForm
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.gridEventContext = new Enterprise.ZArchitecture.ZGrid();
			this.groupBoxEventContext = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.textBoxEventContext = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxEventContextDescriptive = new Enterprise.ZArchitecture.ZTextBox();
			this.buttonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridEventContext)).BeginInit();
			this.groupBoxEventContext.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 378, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.WorkflowEventContextBizo);
			// 
			// gridEventContext
			// 
			this.gridEventContext.AllowNavigation = false;
			this.gridEventContext.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.gridEventContext, "ContextCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.WorkflowEventContextBizo)(null)).ContextCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.WorkflowEventContext)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.WorkflowEventContextBizo)(null)).ContextCollection)).SyncRoot)).MasterClassifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.WorkflowEventContext)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.WorkflowEventContextBizo)(null)).ContextCollection)).SyncRoot)).MasterClassifierDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.WorkflowEventContext)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.WorkflowEventContextBizo)(null)).ContextCollection)).SyncRoot)).MasterType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.WorkflowEventContext)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.WorkflowEventContextBizo)(null)).ContextCollection)).SyncRoot)).MasterTypeDescription)));
			this.gridEventContext.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("de999ede-3177-4f92-a362-0e274def9fa3", "Classifier", "Context Classifier", "Classifier of related record.");
			zDropEditColumnStyleInfo1.ColumnName = "MasterClassifier";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1f2437df-bbe6-4772-8045-397abf7879bb", "Classifier Desc.", "Classifier Description", "Context Classifier Description", "Classifier description of related record.");
			zTextBoxColumnStyleInfo1.ColumnName = "MasterClassifierDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2aca712b-5b83-4b9e-aa55-82e64bd3ee5e", "Type", "Master Type", "Type of related record.");
			zDropEditColumnStyleInfo2.ColumnName = "MasterType";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("50e3afb0-3304-4b8d-91e1-3505eef3d444", "Type Desc.", "Type Description", "Master Type Description", "Type description of related record.");
			zTextBoxColumnStyleInfo2.ColumnName = "MasterTypeDescription";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.gridEventContext.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.gridEventContext.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridEventContext.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.gridEventContext.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gridEventContext.CopySelectedRowsAllowed = true;
			this.gridEventContext.GridId = "20066c05-c6e7-4ca5-8b89-4ae975939546";
			this.gridEventContext.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridEventContext.LayoutKey = "gridEventContext";
			this.gridEventContext.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.gridEventContext.Name = "gridEventContext";
			this.gridEventContext.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 224, true);
			this.gridEventContext.TabIndex = 1;
			// 
			// groupBoxEventContext
			// 
			this.groupBoxEventContext.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxEventContext.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("51436fe8-825c-401d-9dcc-83afc686ab80", "Event Context");
			this.groupBoxEventContext.Controls.Add(this.textBoxEventContext);
			this.groupBoxEventContext.Controls.Add(this.textBoxEventContextDescriptive);
			this.groupBoxEventContext.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 242, true);
			this.groupBoxEventContext.Name = "groupBoxEventContext";
			this.groupBoxEventContext.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 103, true);
			this.groupBoxEventContext.TabIndex = 2;
			this.groupBoxEventContext.TabStop = false;
			// 
			// textBoxEventContext
			// 
			this.textBoxEventContext.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxEventContext, "ContextPathCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.WorkflowEventContextBizo)(null)).ContextPathCode)));
			this.textBoxEventContext.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("64a2abbb-f181-42cb-8ae1-65163cf32957", "Short (Codes)", "Short coded event context.");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.textBoxEventContext, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.textBoxEventContext.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 75, true);
			this.textBoxEventContext.Name = "textBoxEventContext";
			this.textBoxEventContext.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 20, true);
			this.textBoxEventContext.TabIndex = 1;
			// 
			// textBoxEventContextDescriptive
			// 
			this.textBoxEventContextDescriptive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.textBoxEventContextDescriptive, "ContextPathDescriptive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.WorkflowEventContextBizo)(null)).ContextPathDescriptive)));
			this.textBoxEventContextDescriptive.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e379d1f7-2e3b-4e27-9f80-e1c11d11241a", "Event Context Description", "This trigger/milestone will fire when events are added to specified related records.");
			this.textBoxEventContextDescriptive.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.textBoxEventContextDescriptive, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.textBoxEventContextDescriptive.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 31, true);
			this.textBoxEventContextDescriptive.Name = "textBoxEventContextDescriptive";
			this.textBoxEventContextDescriptive.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 20, true);
			this.textBoxEventContextDescriptive.TabIndex = 0;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4702ce90-0f4e-4263-aa7a-0d0e2259e395", "OK");
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 351, true);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.buttonOK.TabIndex = 3;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5c5723e0-83d8-4914-b61a-a1d7a1f1d251", "Cancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 351, true);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// EventContextForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.buttonCancel;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7afb6e1-8326-4d6a-a74c-766844854ff1", "Event Context Builder");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 402, true);
			this.Controls.Add(this.gridEventContext);
			this.Controls.Add(this.groupBoxEventContext);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.WorkflowEventContextBizo);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 440, true);
			this.Name = "EventContextForm";
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.groupBoxEventContext, 0);
			this.Controls.SetChildIndex(this.gridEventContext, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridEventContext)).EndInit();
			this.groupBoxEventContext.ResumeLayout(false);
			this.groupBoxEventContext.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid gridEventContext;
		private ZArchitecture.GUI.ZGroupBox groupBoxEventContext;
		private ZArchitecture.ZTextBox textBoxEventContextDescriptive;
		private ZArchitecture.GUI.ZButton buttonOK;
		private ZArchitecture.GUI.ZButton buttonCancel;
		private ZArchitecture.ZTextBox textBoxEventContext;
	}
}