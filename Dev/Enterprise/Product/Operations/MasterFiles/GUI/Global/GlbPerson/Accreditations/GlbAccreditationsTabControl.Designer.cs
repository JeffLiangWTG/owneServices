using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbAccreditationsTabControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.attemptsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.attemptGridToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.attemptEditButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.accreditationGroupTreeControl = new Enterprise.MasterFiles.GUI.GlbAccreditationGroupTreeControlBase();
			this.preReqGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.preReqGrid = new Enterprise.ZArchitecture.ZGrid();
			this.attemptsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.attemptDeleteButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.attemptsGrid)).BeginInit();
			this.attemptsGrid.SuspendLayout();
			this.attemptGridToolStrip.SuspendLayout();
			this.accreditationGroupTreeControl.SuspendLayout();
			this.preReqGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.preReqGrid)).BeginInit();
			this.preReqGrid.SuspendLayout();
			this.attemptsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPerson);
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.attemptsGrid);
			this.splitContainer.Panel1.Controls.Add(this.attemptGridToolStrip);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.accreditationGroupTreeControl);
			this.splitContainer.Panel2.Controls.Add(this.preReqGroupBox);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 534, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.splitContainer.TabIndex = 0;
			// 
			// attemptsGrid
			// 
			this.attemptsGrid.AllowNavigation = false;
			this.attemptsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.attemptsGrid, "AccreditationAttemptCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).AccreditationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).AccreditationDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).Progress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).HAA_CommencementDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).HAA_CompletionDueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).HAA_CompletionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).HAA_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).CertificateCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).CertificateNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).CertificateIssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).CertificateExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).AverageWeightedScore)));
			this.attemptsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationControl|214FD913-E0EC-4337-9D6A-9FBF30EDFECF", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "AccreditationCode";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F5FA77E0-9566-4B03-8AB8-C0738CE10D03", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "AccreditationDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationControl|D5434E07-42CA-416B-AE42-2A1AA25FA11C", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "Status";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationControl|229A5234-87E2-4981-861F-26123DA8D426", "Progress");
			zTextBoxColumnStyleInfo4.ColumnName = "Progress";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationControl|A986976C-486C-425A-B518-2622096D7C34", "Commence Date");
			zDateEditColumnStyleInfo1.ColumnName = "HAA_CommencementDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationControl|860BA637-DA71-4609-ACE7-99FC53166455", "Completion Due Date");
			zDateEditColumnStyleInfo2.ColumnName = "HAA_CompletionDueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationControl|59B5CB1B-4643-46CC-82C6-F4CFCB06ED7E", "Completion Date");
			zDateEditColumnStyleInfo3.ColumnName = "HAA_CompletionDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsMandatory = true;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationControl|9F0069D3-59F7-40AA-B6FD-CEBB77220FA4", "Attempt Expiry Date");
			zDateEditColumnStyleInfo4.ColumnName = "HAA_ExpiryDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.IsMandatory = true;
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4e43cc1f-91bc-45ca-9128-7f1a18e61b63", "Type");
			zTextBoxColumnStyleInfo5.ColumnName = "CertificateCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ee3d0c49-39d0-4504-a797-80d8909a512d", "Certificate Number");
			zTextBoxColumnStyleInfo6.ColumnName = "CertificateNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8b7be391-abad-45e0-9013-6cea5b25c707", "Issue Date");
			zTextBoxColumnStyleInfo7.ColumnName = "CertificateIssueDate";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d95810cd-b51f-4c1b-90c8-4afaffcc3143", "Certificate Expiry Date");
			zTextBoxColumnStyleInfo8.ColumnName = "CertificateExpiryDate";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bd1576aa-67a5-4f2d-851a-30527a07a900", "Average Weighted Score (%)");
			zCalcEditColumnStyleInfo1.ColumnName = "AverageWeightedScore";
			zCalcEditColumnStyleInfo1.MaxValue = 100;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.attemptsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.attemptsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.attemptsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.attemptsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.attemptsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.attemptsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.attemptsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.attemptsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.attemptsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.attemptsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.attemptsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.attemptsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.attemptsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.attemptsGrid.GridId = "A73B0966-ABBC-4364-8055-DCB50FC19031";
			this.attemptsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.attemptsGrid.LayoutKey = "attemptsGrid";
			this.attemptsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.attemptsGrid.Name = "attemptsGrid";
			this.attemptsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.attemptsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(894, 73, true);
			this.attemptsGrid.TabIndex = 0;
			this.attemptsGrid.DoubleClick += AttemptsGrid_DoubleClick;
			// 
			// attemptGridToolStrip
			// 
			this.attemptGridToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.attemptGridToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.attemptGridToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.attemptGridToolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 12, true);
			this.attemptGridToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.attemptDeleteButton,
				this.attemptEditButton
			});
			this.attemptGridToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 77, true);
			this.attemptGridToolStrip.Name = "attemptGridToolStrip";
			this.attemptGridToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 22, true);
			this.attemptGridToolStrip.TabIndex = 1;
			// 
			// attemptEditButton
			// 
			this.attemptEditButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.attemptEditButton.AutoToolTip = false;
			this.attemptEditButton.BackColor = System.Drawing.Color.Transparent;
			this.attemptEditButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F013163E-55AF-4C07-AFBD-E3E1177773E8", "Workflow && Tracking");
			this.attemptEditButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.attemptEditButton.Name = "attemptEditButton";
			this.attemptEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 22, true);
			this.attemptEditButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.attemptEditButton.Click += AttemptEditButton_Click;
			// 
			// attemptDeleteButton
			// 
			this.attemptDeleteButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.attemptDeleteButton.AutoToolTip = false;
			this.attemptDeleteButton.BackColor = System.Drawing.Color.Transparent;
			this.attemptDeleteButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6369C9DD-1B69-4375-9BE5-FBDF4727BAF3", "Delete");
			this.attemptDeleteButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.attemptDeleteButton.Name = "attemptDeleteButton";
			this.attemptDeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 22, true);
			this.attemptDeleteButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.attemptDeleteButton.Click += AttemptDeleteButton_Click;
			// 
			// accreditationGroupTreeControl
			// 
			this.accreditationGroupTreeControl.AllowDrop = true;
			this.accreditationGroupTreeControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			                                                                                   | System.Windows.Forms.AnchorStyles.Left)
			                                                                                  | System.Windows.Forms.AnchorStyles.Right)));
			this.accreditationGroupTreeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.accreditationGroupTreeControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.accreditationGroupTreeControl.Name = "accreditationGroupTreeControl";
			this.accreditationGroupTreeControl.NameOfATreeElement = null;
			this.accreditationGroupTreeControl.NameOfTreeElementsPlural = null;
			this.accreditationGroupTreeControl.ShowAttachButton = false;
			this.accreditationGroupTreeControl.ShowDetachButton = false;
			this.accreditationGroupTreeControl.ShowEditButton = false;
			this.accreditationGroupTreeControl.ShowNewButton = false;
			this.accreditationGroupTreeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 329, true);
			this.accreditationGroupTreeControl.TabIndex = 1;
			// 
			// preReqGroupBox
			// 
			this.preReqGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.preReqGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ED8A5A26-9358-4D64-8E17-8E29F6DB7151", "Pre-requisite Accreditations");
			this.preReqGroupBox.Controls.Add(this.preReqGrid);
			this.preReqGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 341, true);
			this.preReqGroupBox.Name = "preReqGroupBox";
			this.preReqGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 89, true);
			this.preReqGroupBox.TabIndex = 2;
			this.preReqGroupBox.TabStop = false;
			// 
			// preReqGrid
			// 
			this.preReqGrid.AllowDrop = true;
			this.preReqGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.preReqGrid, "AccreditationAttemptCollection.RequirementsProxyCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).RequirementsProxyCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IAccreditationPersonProxy)(((System.Collections.IList)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).RequirementsProxyCollection)).SyncRoot)).Accreditation.HAC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IAccreditationPersonProxy)(((System.Collections.IList)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).RequirementsProxyCollection)).SyncRoot)).Accreditation.HAC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Integration.Recruiter.IAccreditationPersonProxy)(((System.Collections.IList)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).RequirementsProxyCollection)).SyncRoot)).Attempt.CertificateNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Integration.Recruiter.IAccreditationPersonProxy)(((System.Collections.IList)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).RequirementsProxyCollection)).SyncRoot)).Attempt.CertificateIssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Integration.Recruiter.IAccreditationPersonProxy)(((System.Collections.IList)(((Enterprise.Integration.Recruiter.IGlbAccreditationAttempt)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).AccreditationAttemptCollection)).SyncRoot)).RequirementsProxyCollection)).SyncRoot)).Attempt.CertificateExpiryDate)));
			this.preReqGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationsTabControl|526416C6-7FC3-49F8-9F1E-4B72FA0CFA4D", "Code");
			zTextBoxColumnStyleInfo9.ColumnName = "Accreditation+HAC_Code";
			zTextBoxColumnStyleInfo9.IsMandatory = true;
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbAccreditationsTabControl|38246034-7834-4DCC-967C-F2BD129CBB65", "Description");
			zTextBoxColumnStyleInfo10.ColumnName = "Accreditation+HAC_Description";
			zTextBoxColumnStyleInfo10.IsMandatory = true;
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7B5C99AC-D444-4FAD-9725-18788F34EBFE", "Certificate Number");
			zTextBoxColumnStyleInfo11.ColumnName = "Attempt+CertificateNumber";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("13741473-E104-41FD-AFDE-B9270462A103", "Issue Date");
			zTextBoxColumnStyleInfo12.ColumnName = "Attempt+CertificateIssueDate";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("036796BF-FF38-4EBA-A3A2-CFE4C1297DC2", "Expires");
			zTextBoxColumnStyleInfo13.ColumnName = "Attempt+CertificateExpiryDate";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.preReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.preReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.preReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.preReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.preReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.preReqGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.preReqGrid.GridId = "ffc3c751-d909-4fa0-8262-1c580f5e0d3e";
			this.preReqGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.preReqGrid.IsWholeRowSelectedOnClick = true;
			this.preReqGrid.LayoutKey = "Grid";
			this.preReqGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.preReqGrid.Name = "preReqGrid";
			this.preReqGrid.ReadOnly = true;
			this.preReqGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 74, true);
			this.preReqGrid.TabIndex = 1;
			// 
			// attemptsGroupBox
			// 
			this.attemptsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("93b49792-36a1-4261-aaf2-ae5a3b96863d", "Accreditation Attempts");
			this.attemptsGroupBox.Controls.Add(this.splitContainer);
			this.attemptsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.attemptsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.attemptsGroupBox.Name = "attemptsGroupBox";
			this.attemptsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(903, 551, true);
			this.attemptsGroupBox.TabIndex = 0;
			this.attemptsGroupBox.TabStop = false;
			// 
			// GlbAccreditationsTabControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.attemptsGroupBox);
			this.Name = "GlbAccreditationsTabControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(903, 551, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel1.PerformLayout();
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.attemptsGrid)).EndInit();
			this.attemptsGrid.ResumeLayout(false);
			this.attemptsGrid.PerformLayout();
			this.attemptGridToolStrip.ResumeLayout(false);
			this.attemptGridToolStrip.PerformLayout();
			this.accreditationGroupTreeControl.ResumeLayout(true);
			this.accreditationGroupTreeControl.PerformLayout();
			this.preReqGroupBox.ResumeLayout(false);
			this.preReqGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.preReqGrid)).EndInit();
			this.preReqGrid.ResumeLayout(false);
			this.preReqGrid.PerformLayout();
			this.attemptsGroupBox.ResumeLayout(false);
			this.attemptsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGrid attemptsGrid;
		private GlbAccreditationGroupTreeControlBase accreditationGroupTreeControl;
		private ZGroupBox attemptsGroupBox;
		private ZGroupBox preReqGroupBox;
		private ZGrid preReqGrid;
		private KSplitContainer splitContainer;
		private ZArchitecture.GUI.ZToolStripButton attemptEditButton;
		private ZArchitecture.GUI.ZToolStrip attemptGridToolStrip;
		private ZArchitecture.GUI.ZToolStripButton attemptDeleteButton;
	}
}
