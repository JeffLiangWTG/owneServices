namespace Enterprise.Customs.ZA.GUI
{
	partial class VAT404DocumentUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.ProofOfPaymentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ProofOfPaymentsGrid = new Enterprise.ZArchitecture.ZGrid();
            this.DocDeliveryContactsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.RecipientGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ImportersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ImportersGrid = new Enterprise.ZArchitecture.ZGrid();
            this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.PrinterPKDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
            this.StartDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ProofOfPaymentsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ProofOfPaymentsGrid)).BeginInit();
            this.ProofOfPaymentsGrid.SuspendLayout();
            this.DocDeliveryContactsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecipientGrid)).BeginInit();
            this.RecipientGrid.SuspendLayout();
            this.ImportersGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImportersGrid)).BeginInit();
            this.ImportersGrid.SuspendLayout();
            this.EndDateEdit.SuspendLayout();
            this.PrinterPKDropEdit.SuspendLayout();
            this.StartDateDateEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.zPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.VAT404DocumentInstruction);
            // 
            // ProofOfPaymentsGroupBox
            // 
            this.ProofOfPaymentsGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("057ef5c9-e6f4-4871-be63-97976c0c9cf8", "Proof of Payments");
            this.ProofOfPaymentsGroupBox.Controls.Add(this.ProofOfPaymentsGrid);
            this.ProofOfPaymentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProofOfPaymentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
            this.ProofOfPaymentsGroupBox.Name = "ProofOfPaymentsGroupBox";
            this.ProofOfPaymentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 170, true);
            this.ProofOfPaymentsGroupBox.TabIndex = 2;
            this.ProofOfPaymentsGroupBox.TabStop = false;
            // 
            // ProofOfPaymentsGrid
            // 
            this.ProofOfPaymentsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ProofOfPaymentsGrid, "VAT404Documents.ProofOfPayments");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).LRNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).MRNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).C9_PaymentAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).C9_PaymentDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).C9_PaymentReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).C9_ReceiptDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).FANumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).AgentsReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).Importer.OH_FullName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ProofOfPayments)).SyncRoot)).JobNumber)));
            this.ProofOfPaymentsGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2F222886-6E1E-4799-9AB6-C60B1E06B08B", "Local Ref No", "Local Reference Number", "");
            zTextBoxColumnStyleInfo1.ColumnName = "LRNumber";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsMandatory = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(184);
            zTextBoxColumnStyleInfo2.ColumnName = "MRNumber";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsMandatory = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(184);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("1a86cc03-d60f-497c-97eb-bf4a7f7e8d64", "Amount");
            zCalcEditColumnStyleInfo1.ColumnName = "C9_PaymentAmount";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.IsMandatory = true;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("bfd4d261-6be2-48f4-91ce-5ad901dadf1b", "Trn. Date", "Transaction Date", "");
            zDateEditColumnStyleInfo1.ColumnName = "C9_PaymentDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.IsMandatory = true;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7b639bda-62e6-4d34-a3e0-6a86317f6420", "Receipt No", "Receipt Number", "");
            zTextBoxColumnStyleInfo3.ColumnName = "C9_PaymentReference";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsMandatory = true;
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7baaa7b2-61f0-43be-b7e2-260520f3a575", "Receipt Date");
            zDateEditColumnStyleInfo2.ColumnName = "C9_ReceiptDate";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.IsMandatory = true;
            zDateEditColumnStyleInfo2.IsReadOnly = true;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("70abae14-d56e-4bc3-bd73-540506ae23f8", "FAN", "Financial Account Number");
            zTextBoxColumnStyleInfo4.ColumnName = "FANumber";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsVisible = false;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("A2FCDDDC-22E9-4B7B-A796-2754E1D25870", "Agents Ref", "Agents Reference", "");
            zTextBoxColumnStyleInfo5.ColumnName = "AgentsReference";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.IsMandatory = true;
            zTextBoxColumnStyleInfo5.IsReadOnly = true;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("B3A23259-B4A0-45AB-A1D0-860F9AEF7F9B", "Importer");
            zTextBoxColumnStyleInfo6.ColumnName = "Importer+OH_FullName";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.IsMandatory = true;
            zTextBoxColumnStyleInfo6.IsReadOnly = true;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("04880D3B-87DE-48A0-B288-0DF4DB14D57D", "Job No", "Job Number", "");
            zTextBoxColumnStyleInfo7.ColumnName = "JobNumber";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.IsMandatory = true;
            zTextBoxColumnStyleInfo7.IsReadOnly = true;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.ProofOfPaymentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.ProofOfPaymentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProofOfPaymentsGrid.GridId = "4be10cfa-4eec-4979-bf1a-d6d1a172a0ff";
            this.ProofOfPaymentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ProofOfPaymentsGrid.LayoutKey = "ProofOfPaymentsGrid";
            this.ProofOfPaymentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.ProofOfPaymentsGrid.Name = "ProofOfPaymentsGrid";
            this.ProofOfPaymentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 151, true);
            this.ProofOfPaymentsGrid.TabIndex = 0;
            // 
            // DocDeliveryContactsGroupBox
            // 
            this.DocDeliveryContactsGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e9473957-a58d-46e7-91dd-62e42d1b2d28", "Recipients");
            this.DocDeliveryContactsGroupBox.Controls.Add(this.RecipientGrid);
            this.DocDeliveryContactsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.DocDeliveryContactsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DocDeliveryContactsGroupBox.Name = "DocDeliveryContactsGroupBox";
            this.DocDeliveryContactsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 111, true);
            this.DocDeliveryContactsGroupBox.TabIndex = 1;
            this.DocDeliveryContactsGroupBox.TabStop = false;
            // 
            // RecipientGrid
            // 
            this.RecipientGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.RecipientGrid, "VAT404Documents.DeliveryContacts");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).DeliveryContacts)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).DeliveryContacts)).SyncRoot)).Name)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).DeliveryContacts)).SyncRoot)).DeliveryMethod)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).DeliveryContacts)).SyncRoot)).DeliveryMethodDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).DeliveryContacts)).SyncRoot)).AttachmentType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).DeliveryContacts)).SyncRoot)).DeliveryAddress)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).DeliveryContacts)).SyncRoot)).EmailCarbonCopyRecipientsAsString)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).DeliveryContacts)).SyncRoot)).EmailBlindCarbonCopyRecipientsAsString)));
            this.RecipientGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.ColumnName = "Name";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("a390552e-29d9-4873-83f5-1cab94957585", "Delivery");
            zDropEditColumnStyleInfo2.ColumnName = "DeliveryMethod";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.IsMandatory = true;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(61);
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2e9ef81d-d55b-48bd-af34-a8a30f956544", "Type");
            zTextBoxColumnStyleInfo8.ColumnName = "DeliveryMethodDescription";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.IsReadOnly = true;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("4c82cc96-d88f-4272-a917-86f0bdc71950", "Attachment");
            zDropEditColumnStyleInfo3.ColumnName = "AttachmentType";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.IsMandatory = true;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(61);
            zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e4a1e5a0-6a4a-42fe-acfe-6705133186d1", "Address");
            zTextBoxColumnStyleInfo9.ColumnName = "DeliveryAddress";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.IsMandatory = true;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2a7b4c6f-e2cc-4a71-ba0f-f1a598ee9a2e", "CC");
            zTextBoxColumnStyleInfo10.ColumnName = "EmailCarbonCopyRecipientsAsString";
            zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("485340b1-59d5-4a1a-bd10-df20cfc8e676", "BCC");
            zTextBoxColumnStyleInfo11.ColumnName = "EmailBlindCarbonCopyRecipientsAsString";
            zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.RecipientGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.RecipientGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.RecipientGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.RecipientGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.RecipientGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.RecipientGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.RecipientGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
            this.RecipientGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecipientGrid.GridId = "17fe5a01-784f-4da5-9ab8-c97daeb16208";
            this.RecipientGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.RecipientGrid.LayoutKey = "RecipientGrid";
            this.RecipientGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.RecipientGrid.Name = "RecipientGrid";
            this.RecipientGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 92, true);
            this.RecipientGrid.TabIndex = 0;
            // 
            // ImportersGroupBox
            // 
            this.ImportersGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2876f73f-a541-4bbc-a969-8ecd99f26f44", "Importers");
            this.ImportersGroupBox.Controls.Add(this.ImportersGrid);
            this.ImportersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImportersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ImportersGroupBox.Name = "ImportersGroupBox";
            this.ImportersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 148, true);
            this.ImportersGroupBox.TabIndex = 0;
            this.ImportersGroupBox.TabStop = false;
            // 
            // ImportersGrid
            // 
            this.ImportersGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ImportersGrid, "VAT404Documents");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).ImporterPK)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).Importer.OH_FullName)));
            this.ImportersGrid.CaptionVisible = false;
            zGuidFindBoxColumnStyleInfo1.ColumnName = "ImporterPK";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo12.ColumnName = "Importer+OH_FullName";
            zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo12.IsMandatory = true;
            zTextBoxColumnStyleInfo12.IsReadOnly = true;
            zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
            this.ImportersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.ImportersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
            this.ImportersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImportersGrid.GridId = "be33a20f-fd6e-472f-b03f-023e0b005788";
            this.ImportersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ImportersGrid.LayoutKey = "ImportersGrid";
            this.ImportersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.ImportersGrid.Name = "ImportersGrid";
            this.ImportersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 129, true);
            this.ImportersGrid.TabIndex = 0;
            // 
            // PreviewButton
            // 
            this.PreviewButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ad6583aa-144d-4e80-96ca-7036c3918c52", "Pre&view");
            this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(637, 4, true);
            this.PreviewButton.Name = "PreviewButton";
            this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 25, true);
            this.PreviewButton.TabIndex = 3;
            this.PreviewButton.ToolTipCaption = null;
            this.PreviewButton.UseVisualStyleBackColor = true;
            this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
            // 
            // EndDateEdit
            // 
            this.EndDateEdit.AllowDrop = true;
            this.EndDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EndDateEdit, "VAT404Documents.PaymentEndDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).PaymentEndDate)));
            this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 8, true);
            this.EndDateEdit.Name = "EndDateEdit";
            this.EndDateEdit.TabIndex = 1;
            // 
            // PrinterPKDropEdit
            // 
            this.PrinterPKDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PrinterPKDropEdit, "PrinterPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).PrinterPK)));
            this.PrinterPKDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 8, true);
            this.PrinterPKDropEdit.Name = "PrinterPKDropEdit";
            this.PrinterPKDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.PrinterPKDropEdit.TabIndex = 2;
            // 
            // StartDateDateEdit
            // 
            this.StartDateDateEdit.AllowDrop = true;
            this.StartDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.StartDateDateEdit, "VAT404Documents.PaymentStartDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.VAT404Document)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.VAT404DocumentInstruction)(null)).VAT404Documents)).SyncRoot)).PaymentStartDate)));
            this.StartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 8, true);
            this.StartDateDateEdit.Name = "StartDateDateEdit";
            this.StartDateDateEdit.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ImportersGroupBox);
            this.splitContainer1.Panel1.Controls.Add(this.zPanel1);
            this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 469, true);
            this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(154);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ProofOfPaymentsGroupBox);
            this.splitContainer1.Panel2.Controls.Add(this.DocDeliveryContactsGroupBox);
            this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(184);
            this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(184);
            this.splitContainer1.TabIndex = 5;
            // 
            // zPanel1
            // 
            this.zPanel1.Controls.Add(this.PreviewButton);
            this.zPanel1.Controls.Add(this.PrinterPKDropEdit);
            this.zPanel1.Controls.Add(this.EndDateEdit);
            this.zPanel1.Controls.Add(this.StartDateDateEdit);
            this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 148, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 36, true);
            this.zPanel1.TabIndex = 1;
            // 
            // VAT404DocumentUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.splitContainer1);
            this.Name = "VAT404DocumentUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 469, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ProofOfPaymentsGroupBox.ResumeLayout(false);
            this.ProofOfPaymentsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ProofOfPaymentsGrid)).EndInit();
            this.ProofOfPaymentsGrid.ResumeLayout(false);
            this.ProofOfPaymentsGrid.PerformLayout();
            this.DocDeliveryContactsGroupBox.ResumeLayout(false);
            this.DocDeliveryContactsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecipientGrid)).EndInit();
            this.RecipientGrid.ResumeLayout(false);
            this.RecipientGrid.PerformLayout();
            this.ImportersGroupBox.ResumeLayout(false);
            this.ImportersGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImportersGrid)).EndInit();
            this.ImportersGrid.ResumeLayout(false);
            this.ImportersGrid.PerformLayout();
            this.EndDateEdit.ResumeLayout(true);
            this.EndDateEdit.PerformLayout();
            this.PrinterPKDropEdit.ResumeLayout(true);
            this.PrinterPKDropEdit.PerformLayout();
            this.StartDateDateEdit.ResumeLayout(true);
            this.StartDateDateEdit.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer1.PerformLayout();
            this.zPanel1.ResumeLayout(false);
            this.zPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ProofOfPaymentsGroupBox;
		private ZArchitecture.ZGrid ProofOfPaymentsGrid;
		private ZArchitecture.GUI.ZGroupBox DocDeliveryContactsGroupBox;
		private ZArchitecture.ZGrid RecipientGrid;
		private ZArchitecture.GUI.ZGroupBox ImportersGroupBox;
		private ZArchitecture.GUI.ZDateEdit EndDateEdit;
		private ZArchitecture.GUI.ZGuidDropEdit PrinterPKDropEdit;
		private ZArchitecture.GUI.ZDateEdit StartDateDateEdit;
		private ZArchitecture.ZGrid ImportersGrid;
		private ZArchitecture.GUI.ZButton PreviewButton;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZPanel zPanel1;
	}
}
