namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class DiscrepancyForm
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
		new void InitializeComponent()
		{
			this.UseQuotedPriceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReRateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HoldForLaterAnalysisButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShipmentCarrierFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.QuoteCarrierFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zCodeFindBox2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ValueOfInsuranceCurrencyFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ValueOfGoodsCurrencyFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShipmentChargeableCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShipmentInsuranceValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShipmentValueOfGoodsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShipmentVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentActualWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShipmentServiceLevelFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QuoteChargeableCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ShipmentDeliveryAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.QuoteDeliveryAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ShipmentPickupAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.QuotePickupAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ShipmentIncoTermTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QuoteIncoTermTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentTransportModeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentContainerModeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QuoteTransportModeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QuoteContainerModeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentClientFindBox = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.QuoteClientFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.QuoteDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QuoteInsuranceValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QuoteValueOfGoodsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QuoteVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QuoteActualWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QuoteOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QuoteServiceLevelFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.ShipmentCarrierFindBox.SuspendLayout();
			this.QuoteCarrierFindBox.SuspendLayout();
			this.zCodeFindBox1.SuspendLayout();
			this.zCodeFindBox2.SuspendLayout();
			this.ValueOfInsuranceCurrencyFindbox.SuspendLayout();
			this.ValueOfGoodsCurrencyFindbox.SuspendLayout();
			this.ShipmentChargeableCalcDropEdit.SuspendLayout();
			this.ShipmentDestinationCodeFindBox.SuspendLayout();
			this.ShipmentVolumeCalcDropEdit.SuspendLayout();
			this.ShipmentActualWeightDropEdit.SuspendLayout();
			this.ShipmentOriginCodeFindBox.SuspendLayout();
			this.ShipmentServiceLevelFindBox.SuspendLayout();
			this.QuoteChargeableCalcDropEdit.SuspendLayout();
			this.ShipmentDeliveryAddressControl.SuspendLayout();
			this.QuoteDeliveryAddressControl.SuspendLayout();
			this.ShipmentPickupAddressControl.SuspendLayout();
			this.QuotePickupAddressControl.SuspendLayout();
			this.ShipmentClientFindBox.SuspendLayout();
			this.QuoteClientFindBox.SuspendLayout();
			this.QuoteDestinationCodeFindBox.SuspendLayout();
			this.QuoteVolumeCalcDropEdit.SuspendLayout();
			this.QuoteActualWeightDropEdit.SuspendLayout();
			this.QuoteOriginCodeFindBox.SuspendLayout();
			this.QuoteServiceLevelFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 547, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// UseQuotedPriceButton
			// 
			this.UseQuotedPriceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UseQuotedPriceButton.CaptionResourceString = Res.GetData("DiscrepancyForm|8eb68282-24d3-4227-a5dc-b1e144546ef9", "Use Quoted Price");
			this.UseQuotedPriceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 518, true);
			this.UseQuotedPriceButton.Name = "UseQuotedPriceButton";
			this.UseQuotedPriceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.UseQuotedPriceButton.TabIndex = 1;
			this.UseQuotedPriceButton.UseVisualStyleBackColor = true;
			this.UseQuotedPriceButton.Click += new System.EventHandler(this.UseQuotedPriceButton_Click);
			// 
			// ReRateButton
			// 
			this.ReRateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ReRateButton.CaptionResourceString = Res.GetData("DiscrepancyForm|5b7ae87e-6ccd-4614-95a6-3cb4da456eaf", "Re-Rate");
			this.ReRateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 518, true);
			this.ReRateButton.Name = "ReRateButton";
			this.ReRateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.ReRateButton.TabIndex = 2;
			this.ReRateButton.UseVisualStyleBackColor = true;
			this.ReRateButton.Click += new System.EventHandler(this.ReRateButton_Click);
			// 
			// HoldForLaterAnalysisButton
			// 
			this.HoldForLaterAnalysisButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.HoldForLaterAnalysisButton.CaptionResourceString = Res.GetData("DiscrepancyForm|b298af50-85dc-481d-9c98-51806fa38c60", "Hold for Later Analysis");
			this.HoldForLaterAnalysisButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 518, true);
			this.HoldForLaterAnalysisButton.Name = "HoldForLaterAnalysisButton";
			this.HoldForLaterAnalysisButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.HoldForLaterAnalysisButton.TabIndex = 3;
			this.HoldForLaterAnalysisButton.UseVisualStyleBackColor = true;
			this.HoldForLaterAnalysisButton.Click += new System.EventHandler(this.HoldForLaterAnalysisButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Res.GetData("DiscrepancyForm|0ad8688a-b086-49f3-bd22-717696b042ed", "Field");
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.zLabel1.TabIndex = 0;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Res.GetData("DiscrepancyForm|74aab94b-b9b7-4274-ab5a-6acf11e82157", "Quoted Value");
			this.zLabel2.IsFontBold = true;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 9, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 13, true);
			this.zLabel2.TabIndex = 1;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Res.GetData("DiscrepancyForm|24a46b93-c48b-4f48-bc7f-3f4442d84f55", "Booked Value");
			this.zLabel3.IsFontBold = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 9, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.zLabel3.TabIndex = 2;
			// 
			// MainPanel
			// 
			this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainPanel.AutoScroll = true;
			this.MainPanel.Controls.Add(this.ShipmentCarrierFindBox);
			this.MainPanel.Controls.Add(this.QuoteCarrierFindBox);
			this.MainPanel.Controls.Add(this.zCodeFindBox1);
			this.MainPanel.Controls.Add(this.zCodeFindBox2);
			this.MainPanel.Controls.Add(this.ValueOfInsuranceCurrencyFindbox);
			this.MainPanel.Controls.Add(this.ValueOfGoodsCurrencyFindbox);
			this.MainPanel.Controls.Add(this.ShipmentChargeableCalcDropEdit);
			this.MainPanel.Controls.Add(this.ShipmentDestinationCodeFindBox);
			this.MainPanel.Controls.Add(this.ShipmentInsuranceValueCalcEdit);
			this.MainPanel.Controls.Add(this.ShipmentValueOfGoodsCalcEdit);
			this.MainPanel.Controls.Add(this.ShipmentVolumeCalcDropEdit);
			this.MainPanel.Controls.Add(this.ShipmentActualWeightDropEdit);
			this.MainPanel.Controls.Add(this.ShipmentOriginCodeFindBox);
			this.MainPanel.Controls.Add(this.ShipmentServiceLevelFindBox);
			this.MainPanel.Controls.Add(this.QuoteChargeableCalcDropEdit);
			this.MainPanel.Controls.Add(this.ShipmentDeliveryAddressControl);
			this.MainPanel.Controls.Add(this.QuoteDeliveryAddressControl);
			this.MainPanel.Controls.Add(this.ShipmentPickupAddressControl);
			this.MainPanel.Controls.Add(this.QuotePickupAddressControl);
			this.MainPanel.Controls.Add(this.ShipmentIncoTermTextBox);
			this.MainPanel.Controls.Add(this.QuoteIncoTermTextBox);
			this.MainPanel.Controls.Add(this.ShipmentTransportModeTextBox);
			this.MainPanel.Controls.Add(this.ShipmentContainerModeTextBox);
			this.MainPanel.Controls.Add(this.QuoteTransportModeTextBox);
			this.MainPanel.Controls.Add(this.QuoteContainerModeTextBox);
			this.MainPanel.Controls.Add(this.ShipmentClientFindBox);
			this.MainPanel.Controls.Add(this.QuoteClientFindBox);
			this.MainPanel.Controls.Add(this.QuoteDestinationCodeFindBox);
			this.MainPanel.Controls.Add(this.QuoteInsuranceValueCalcEdit);
			this.MainPanel.Controls.Add(this.QuoteValueOfGoodsCalcEdit);
			this.MainPanel.Controls.Add(this.QuoteVolumeCalcDropEdit);
			this.MainPanel.Controls.Add(this.QuoteActualWeightDropEdit);
			this.MainPanel.Controls.Add(this.QuoteOriginCodeFindBox);
			this.MainPanel.Controls.Add(this.QuoteServiceLevelFindBox);
			this.MainPanel.Controls.Add(this.zLabel1);
			this.MainPanel.Controls.Add(this.zLabel3);
			this.MainPanel.Controls.Add(this.zLabel2);
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 512, true);
			this.MainPanel.TabIndex = 0;
			// 
			// ShipmentCarrierFindBox
			// 
			this.ShipmentCarrierFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentCarrierFindBox, "OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).OH_Carrier)));
			this.ShipmentCarrierFindBox.Enabled = false;
			this.ShipmentCarrierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 301, true);
			this.ShipmentCarrierFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ShipmentCarrierFindBox.Name = "ShipmentCarrierFindBox";
			this.ShipmentCarrierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ShipmentCarrierFindBox.TabIndex = 47;
			// 
			// QuoteCarrierFindBox
			// 
			this.QuoteCarrierFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteCarrierFindBox, "Quote+CurrentOneOffQuote+TT_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_OH_Carrier)));
			this.QuoteCarrierFindBox.CaptionResourceString = Res.GetData("3438109c-df70-42d9-9af3-87346d92200b", "Carrier");
			this.QuoteCarrierFindBox.Enabled = false;
			this.QuoteCarrierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 301, true);
			this.QuoteCarrierFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.QuoteCarrierFindBox.Name = "QuoteCarrierFindBox";
			this.QuoteCarrierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.QuoteCarrierFindBox.TabIndex = 46;
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "InsuranceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).InsuranceCurrency)));
			this.zCodeFindBox1.Enabled = false;
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 489, true);
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PreBoundMaxLength = 3;
			this.zCodeFindBox1.ShowDescriptionBox = false;
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.zCodeFindBox1.TabIndex = 45;
			// 
			// zCodeFindBox2
			// 
			this.zCodeFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox2, "GoodsCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).GoodsCurrency)));
			this.zCodeFindBox2.Enabled = false;
			this.zCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 467, true);
			this.zCodeFindBox2.Name = "zCodeFindBox2";
			this.zCodeFindBox2.PreBoundMaxLength = 3;
			this.zCodeFindBox2.ShowDescriptionBox = false;
			this.zCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.zCodeFindBox2.TabIndex = 40;
			// 
			// ValueOfInsuranceCurrencyFindbox
			// 
			this.ValueOfInsuranceCurrencyFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValueOfInsuranceCurrencyFindbox, "Quote+CurrentOneOffQuote+TT_RX_NKInsureValCurr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_RX_NKInsureValCurr)));
			this.ValueOfInsuranceCurrencyFindbox.Enabled = false;
			this.ValueOfInsuranceCurrencyFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 489, true);
			this.ValueOfInsuranceCurrencyFindbox.Name = "ValueOfInsuranceCurrencyFindbox";
			this.ValueOfInsuranceCurrencyFindbox.PreBoundMaxLength = 3;
			this.ValueOfInsuranceCurrencyFindbox.ShowDescriptionBox = false;
			this.ValueOfInsuranceCurrencyFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.ValueOfInsuranceCurrencyFindbox.TabIndex = 43;
			// 
			// ValueOfGoodsCurrencyFindbox
			// 
			this.ValueOfGoodsCurrencyFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValueOfGoodsCurrencyFindbox, "Quote+CurrentOneOffQuote+TT_RX_NKGoodsCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency)));
			this.ValueOfGoodsCurrencyFindbox.Enabled = false;
			this.ValueOfGoodsCurrencyFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 467, true);
			this.ValueOfGoodsCurrencyFindbox.Name = "ValueOfGoodsCurrencyFindbox";
			this.ValueOfGoodsCurrencyFindbox.PreBoundMaxLength = 3;
			this.ValueOfGoodsCurrencyFindbox.ShowDescriptionBox = false;
			this.ValueOfGoodsCurrencyFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.ValueOfGoodsCurrencyFindbox.TabIndex = 38;
			// 
			// ShipmentChargeableCalcDropEdit
			// 
			this.ShipmentChargeableCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentChargeableCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Chargeable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ChargeableUnit)));
			this.ShipmentChargeableCalcDropEdit.BindToAmount = "Chargeable";
			this.ShipmentChargeableCalcDropEdit.BindToUnit = "ChargeableUnit";
			this.ShipmentChargeableCalcDropEdit.Enabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentChargeableCalcDropEdit, false);
			this.ShipmentChargeableCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 441, true);
			this.ShipmentChargeableCalcDropEdit.Name = "ShipmentChargeableCalcDropEdit";
			this.ShipmentChargeableCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ShipmentChargeableCalcDropEdit.TabIndex = 35;
			this.ShipmentChargeableCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentDestinationCodeFindBox
			// 
			this.ShipmentDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDestinationCodeFindBox, "Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Destination)));
			this.ShipmentDestinationCodeFindBox.Enabled = false;
			this.ShipmentDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 373, true);
			this.ShipmentDestinationCodeFindBox.Name = "ShipmentDestinationCodeFindBox";
			this.ShipmentDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ShipmentDestinationCodeFindBox.TabIndex = 26;
			// 
			// ShipmentInsuranceValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentInsuranceValueCalcEdit, "InsuranceValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).InsuranceValue)));
			this.ShipmentInsuranceValueCalcEdit.DecimalPlaces = 2;
			this.ShipmentInsuranceValueCalcEdit.Enabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentInsuranceValueCalcEdit, false);
			this.ShipmentInsuranceValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 489, true);
			this.ShipmentInsuranceValueCalcEdit.Name = "ShipmentInsuranceValueCalcEdit";
			this.ShipmentInsuranceValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ShipmentInsuranceValueCalcEdit.TabIndex = 44;
			this.ShipmentInsuranceValueCalcEdit.Text = "0.00";
			this.ShipmentInsuranceValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentValueOfGoodsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentValueOfGoodsCalcEdit, "GoodsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).GoodsValue)));
			this.ShipmentValueOfGoodsCalcEdit.DecimalPlaces = 2;
			this.ShipmentValueOfGoodsCalcEdit.Enabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentValueOfGoodsCalcEdit, false);
			this.ShipmentValueOfGoodsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 467, true);
			this.ShipmentValueOfGoodsCalcEdit.Name = "ShipmentValueOfGoodsCalcEdit";
			this.ShipmentValueOfGoodsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ShipmentValueOfGoodsCalcEdit.TabIndex = 39;
			this.ShipmentValueOfGoodsCalcEdit.Text = "0.00";
			this.ShipmentValueOfGoodsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentVolumeCalcDropEdit
			// 
			this.ShipmentVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).VolumeUnit)));
			this.ShipmentVolumeCalcDropEdit.BindToAmount = "Volume";
			this.ShipmentVolumeCalcDropEdit.BindToUnit = "VolumeUnit";
			this.ShipmentVolumeCalcDropEdit.Enabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentVolumeCalcDropEdit, false);
			this.ShipmentVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 419, true);
			this.ShipmentVolumeCalcDropEdit.Name = "ShipmentVolumeCalcDropEdit";
			this.ShipmentVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ShipmentVolumeCalcDropEdit.TabIndex = 32;
			this.ShipmentVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentActualWeightDropEdit
			// 
			this.ShipmentActualWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentActualWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).WeightUnit)));
			this.ShipmentActualWeightDropEdit.BindToAmount = "Weight";
			this.ShipmentActualWeightDropEdit.BindToUnit = "WeightUnit";
			this.ShipmentActualWeightDropEdit.Enabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentActualWeightDropEdit, false);
			this.ShipmentActualWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 396, true);
			this.ShipmentActualWeightDropEdit.Name = "ShipmentActualWeightDropEdit";
			this.ShipmentActualWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.ShipmentActualWeightDropEdit.TabIndex = 29;
			this.ShipmentActualWeightDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentOriginCodeFindBox
			// 
			this.ShipmentOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentOriginCodeFindBox, "Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Origin)));
			this.ShipmentOriginCodeFindBox.Enabled = false;
			this.ShipmentOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 350, true);
			this.ShipmentOriginCodeFindBox.Name = "ShipmentOriginCodeFindBox";
			this.ShipmentOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ShipmentOriginCodeFindBox.TabIndex = 23;
			// 
			// ShipmentServiceLevelFindBox
			// 
			this.ShipmentServiceLevelFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentServiceLevelFindBox, "ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ServiceLevel)));
			this.ShipmentServiceLevelFindBox.Enabled = false;
			this.ShipmentServiceLevelFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 327, true);
			this.ShipmentServiceLevelFindBox.Name = "ShipmentServiceLevelFindBox";
			this.ShipmentServiceLevelFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ShipmentServiceLevelFindBox.TabIndex = 20;
			// 
			// QuoteChargeableCalcDropEdit
			// 
			this.QuoteChargeableCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteChargeableCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_Chargeable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_ChargeableUnit)));
			this.QuoteChargeableCalcDropEdit.BindToAmount = "Quote+CurrentOneOffQuote+TT_Chargeable";
			this.QuoteChargeableCalcDropEdit.BindToUnit = "Quote+CurrentOneOffQuote+TT_ChargeableUnit";
			this.QuoteChargeableCalcDropEdit.CaptionResourceString = Res.GetData("DiscrepancyForm|6aefd1a1-fcbc-4a89-8eae-ff538e8c9b7d", "Chargeable");
			this.QuoteChargeableCalcDropEdit.Decimals = 3;
			this.QuoteChargeableCalcDropEdit.Enabled = false;
			this.QuoteChargeableCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 441, true);
			this.QuoteChargeableCalcDropEdit.Name = "QuoteChargeableCalcDropEdit";
			this.QuoteChargeableCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.QuoteChargeableCalcDropEdit.TabIndex = 34;
			this.QuoteChargeableCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentDeliveryAddressControl
			// 
			this.ShipmentDeliveryAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDeliveryAddressControl, "ConsigneeDocumentaryAddress+E2_OA_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ConsigneeDocumentaryAddress.E2_OA_Address)));
			this.ShipmentDeliveryAddressControl.BindToOrgList = "Organisations";
			this.ShipmentDeliveryAddressControl.Enabled = false;
			this.ShipmentDeliveryAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 239, true);
			this.ShipmentDeliveryAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 81, true);
			this.ShipmentDeliveryAddressControl.Name = "ShipmentDeliveryAddressControl";
			this.ShipmentDeliveryAddressControl.PopupCaption = "";
			this.ShipmentDeliveryAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 79, true);
			this.ShipmentDeliveryAddressControl.StackControls = true;
			this.ShipmentDeliveryAddressControl.TabIndex = 17;
			// 
			// QuoteDeliveryAddressControl
			// 
			this.QuoteDeliveryAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteDeliveryAddressControl, "Quote+CurrentOneOffQuote+DeliveryDocAddress+E2_OA_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.DeliveryDocAddress.E2_OA_Address)));
			this.QuoteDeliveryAddressControl.BindToOrgList = "Organisations";
			this.QuoteDeliveryAddressControl.CaptionResourceString = Res.GetData("DiscrepancyForm|a479bbb5-1572-4c84-b810-711219ff0f15", "Delivery Address");
			this.QuoteDeliveryAddressControl.Enabled = false;
			this.QuoteDeliveryAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 239, true);
			this.QuoteDeliveryAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 81, true);
			this.QuoteDeliveryAddressControl.Name = "QuoteDeliveryAddressControl";
			this.QuoteDeliveryAddressControl.PopupCaption = "";
			this.QuoteDeliveryAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 79, true);
			this.QuoteDeliveryAddressControl.StackControls = true;
			this.QuoteDeliveryAddressControl.TabIndex = 16;
			// 
			// ShipmentPickupAddressControl
			// 
			this.ShipmentPickupAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentPickupAddressControl, "ConsignorDocumentaryAddress+E2_OA_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ConsignorDocumentaryAddress.E2_OA_Address)));
			this.ShipmentPickupAddressControl.BindToOrgList = "Organisations";
			this.ShipmentPickupAddressControl.Enabled = false;
			this.ShipmentPickupAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 129, true);
			this.ShipmentPickupAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 81, true);
			this.ShipmentPickupAddressControl.Name = "ShipmentPickupAddressControl";
			this.ShipmentPickupAddressControl.PopupCaption = "";
			this.ShipmentPickupAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 79, true);
			this.ShipmentPickupAddressControl.StackControls = true;
			this.ShipmentPickupAddressControl.TabIndex = 14;
			// 
			// QuotePickupAddressControl
			// 
			this.QuotePickupAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotePickupAddressControl, "Quote+CurrentOneOffQuote+PickUpDocAddress+E2_OA_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.PickUpDocAddress.E2_OA_Address)));
			this.QuotePickupAddressControl.BindToOrgList = "Organisations";
			this.QuotePickupAddressControl.CaptionResourceString = Res.GetData("DiscrepancyForm|6409f4c8-5baa-4935-a9a6-8fbde15a82f7", "Pickup Address");
			this.QuotePickupAddressControl.Enabled = false;
			this.QuotePickupAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 129, true);
			this.QuotePickupAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 81, true);
			this.QuotePickupAddressControl.Name = "QuotePickupAddressControl";
			this.QuotePickupAddressControl.PopupCaption = "";
			this.QuotePickupAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 79, true);
			this.QuotePickupAddressControl.StackControls = true;
			this.QuotePickupAddressControl.TabIndex = 13;
			// 
			// ShipmentIncoTermTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentIncoTermTextBox, "PaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).PaymentTerms)));
			this.ShipmentIncoTermTextBox.Enabled = false;
			this.ShipmentIncoTermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 106, true);
			this.ShipmentIncoTermTextBox.Name = "ShipmentIncoTermTextBox";
			this.ShipmentIncoTermTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ShipmentIncoTermTextBox.TabIndex = 11;
			// 
			// QuoteIncoTermTextBox
			// 
			this.BindingSource.SetBindingMember(this.QuoteIncoTermTextBox, "Quote+CurrentOneOffQuote+TT_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_IncoTerm)));
			this.QuoteIncoTermTextBox.CaptionResourceString = Res.GetData("DiscrepancyForm|efef4c71-940c-40ae-468c-d3ca0cb349b3", "Incoterm");
			this.QuoteIncoTermTextBox.Enabled = false;
			this.QuoteIncoTermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 106, true);
			this.QuoteIncoTermTextBox.Name = "QuoteIncoTermTextBox";
			this.QuoteIncoTermTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.QuoteIncoTermTextBox.TabIndex = 10;
			// 
			// ShipmentTransportModeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTransportModeTextBox, "TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).TransportMode)));
			this.ShipmentTransportModeTextBox.Enabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentTransportModeTextBox, false);
			this.ShipmentTransportModeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 60, true);
			this.ShipmentTransportModeTextBox.Name = "ShipmentTransportModeTextBox";
			this.ShipmentTransportModeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ShipmentTransportModeTextBox.TabIndex = 8;
			// 
			// ShipmentContainerModeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentContainerModeTextBox, "ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ContainerMode)));
			this.ShipmentContainerModeTextBox.Enabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentContainerModeTextBox, false);
			this.ShipmentContainerModeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 83, true);
			this.ShipmentContainerModeTextBox.Name = "ShipmentContainerModeTextBox";
			this.ShipmentContainerModeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ShipmentContainerModeTextBox.TabIndex = 8;
			// 
			// QuoteTransportModeTextBox
			//
			this.BindingSource.SetBindingMember(this.QuoteTransportModeTextBox, "Quote+CurrentOneOffQuote+TT_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_TransportMode)));
			this.QuoteTransportModeTextBox.CaptionResourceString = Res.GetData("DiscrepancyForm|6d96f055-c68a-4736-aa5f-45bbdb0c732e", "Transport Mode");
			this.QuoteTransportModeTextBox.Enabled = false;
			this.QuoteTransportModeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 60, true);
			this.QuoteTransportModeTextBox.Name = "QuoteTransportModeTextBox";
			this.QuoteTransportModeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.QuoteTransportModeTextBox.TabIndex = 7;
			// 
			// QuoteContainerModeTextBox
			//
			this.BindingSource.SetBindingMember(this.QuoteContainerModeTextBox, "Quote+CurrentOneOffQuote+TT_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_ContainerMode)));
			this.QuoteContainerModeTextBox.CaptionResourceString = Res.GetData("DiscrepancyForm|63fd9838-4098-4a94-bd7f-f1931b5ce666", "Container Mode");
			this.QuoteContainerModeTextBox.Enabled = false;
			this.QuoteContainerModeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 83, true);
			this.QuoteContainerModeTextBox.Name = "QuoteContainerModeTextBox";
			this.QuoteContainerModeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.QuoteContainerModeTextBox.TabIndex = 7;
			// 
			// ShipmentClientFindBox
			// 
			this.ShipmentClientFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentClientFindBox, "ClientAddrPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ClientAddrPK)));
			this.ShipmentClientFindBox.Enabled = false;
			this.ShipmentClientFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 37, true);
			this.ShipmentClientFindBox.Name = "ShipmentClientFindBox";
			this.ShipmentClientFindBox.PopupCaption = "";
			this.ShipmentClientFindBox.ShowAddress = false;
			this.ShipmentClientFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ShipmentClientFindBox.TabIndex = 5;
			// 
			// QuoteClientFindBox
			// 
			this.QuoteClientFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteClientFindBox, "Quote.TH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.TH_OH)));
			this.QuoteClientFindBox.Enabled = false;
			this.QuoteClientFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 37, true);
			this.QuoteClientFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.QuoteClientFindBox.Name = "QuoteClientFindBox";
			this.QuoteClientFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.QuoteClientFindBox.TabIndex = 4;
			// 
			// QuoteDestinationCodeFindBox
			// 
			this.QuoteDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteDestinationCodeFindBox, "Quote+CurrentOneOffQuote+TT_RL_NKDeliveryLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation)));
			this.QuoteDestinationCodeFindBox.CaptionResourceString = Res.GetData("DiscrepancyForm|6687ea0b-9349-4e38-8c6c-0c55580d4047", "Destination");
			this.QuoteDestinationCodeFindBox.Enabled = false;
			this.QuoteDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 373, true);
			this.QuoteDestinationCodeFindBox.Name = "QuoteDestinationCodeFindBox";
			this.QuoteDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.QuoteDestinationCodeFindBox.TabIndex = 25;
			// 
			// QuoteInsuranceValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QuoteInsuranceValueCalcEdit, "Quote+CurrentOneOffQuote+TT_InsureVal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_InsureVal)));
			this.QuoteInsuranceValueCalcEdit.DecimalPlaces = 2;
			this.QuoteInsuranceValueCalcEdit.Enabled = false;
			this.QuoteInsuranceValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 489, true);
			this.QuoteInsuranceValueCalcEdit.Name = "QuoteInsuranceValueCalcEdit";
			this.QuoteInsuranceValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.QuoteInsuranceValueCalcEdit.TabIndex = 42;
			this.QuoteInsuranceValueCalcEdit.Text = "0.00";
			this.QuoteInsuranceValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QuoteValueOfGoodsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QuoteValueOfGoodsCalcEdit, "Quote+CurrentOneOffQuote+TT_ValueOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_ValueOfGoods)));
			this.QuoteValueOfGoodsCalcEdit.DecimalPlaces = 2;
			this.QuoteValueOfGoodsCalcEdit.Enabled = false;
			this.QuoteValueOfGoodsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 467, true);
			this.QuoteValueOfGoodsCalcEdit.Name = "QuoteValueOfGoodsCalcEdit";
			this.QuoteValueOfGoodsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.QuoteValueOfGoodsCalcEdit.TabIndex = 37;
			this.QuoteValueOfGoodsCalcEdit.Text = "0.00";
			this.QuoteValueOfGoodsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QuoteVolumeCalcDropEdit
			// 
			this.QuoteVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_UnitOfVolume)));
			this.QuoteVolumeCalcDropEdit.BindToAmount = "Quote+CurrentOneOffQuote+TT_ActualVolume";
			this.QuoteVolumeCalcDropEdit.BindToUnit = "Quote+CurrentOneOffQuote+TT_UnitOfVolume";
			this.QuoteVolumeCalcDropEdit.Decimals = 3;
			this.QuoteVolumeCalcDropEdit.Enabled = false;
			this.QuoteVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 419, true);
			this.QuoteVolumeCalcDropEdit.Name = "QuoteVolumeCalcDropEdit";
			this.QuoteVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.QuoteVolumeCalcDropEdit.TabIndex = 31;
			this.QuoteVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// QuoteActualWeightDropEdit
			// 
			this.QuoteActualWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteActualWeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).WeightUnit)));
			this.QuoteActualWeightDropEdit.BindToAmount = "Quote+CurrentOneOffQuote+TT_ActualWeight";
			this.QuoteActualWeightDropEdit.BindToUnit = "WeightUnit";
			this.QuoteActualWeightDropEdit.Decimals = 3;
			this.QuoteActualWeightDropEdit.Enabled = false;
			this.QuoteActualWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 396, true);
			this.QuoteActualWeightDropEdit.Name = "QuoteActualWeightDropEdit";
			this.QuoteActualWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.QuoteActualWeightDropEdit.TabIndex = 28;
			this.QuoteActualWeightDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// QuoteOriginCodeFindBox
			// 
			this.QuoteOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteOriginCodeFindBox, "Quote+CurrentOneOffQuote+TT_RL_NKReceivalLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation)));
			this.QuoteOriginCodeFindBox.CaptionResourceString = Res.GetData("DiscrepancyForm|2dd75760-dd95-4fd5-a5e7-751b6bce6a7c", "Origin");
			this.QuoteOriginCodeFindBox.Enabled = false;
			this.QuoteOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 350, true);
			this.QuoteOriginCodeFindBox.Name = "QuoteOriginCodeFindBox";
			this.QuoteOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.QuoteOriginCodeFindBox.TabIndex = 22;
			// 
			// QuoteServiceLevelFindBox
			// 
			this.QuoteServiceLevelFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuoteServiceLevelFindBox, "Quote+CurrentOneOffQuote+TT_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Quote.CurrentOneOffQuote.TT_RS_NKServiceLevel)));
			this.QuoteServiceLevelFindBox.Enabled = false;
			this.QuoteServiceLevelFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 327, true);
			this.QuoteServiceLevelFindBox.Name = "QuoteServiceLevelFindBox";
			this.QuoteServiceLevelFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.QuoteServiceLevelFindBox.TabIndex = 19;
			// 
			// DiscrepancyForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 571, true);
			this.ControlBox = false;
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.HoldForLaterAnalysisButton);
			this.Controls.Add(this.ReRateButton);
			this.Controls.Add(this.UseQuotedPriceButton);
			this.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			this.Name = "DiscrepancyForm";
			this.Text = "Discrepancy";
			this.Controls.SetChildIndex(this.UseQuotedPriceButton, 0);
			this.Controls.SetChildIndex(this.ReRateButton, 0);
			this.Controls.SetChildIndex(this.HoldForLaterAnalysisButton, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ShipmentCarrierFindBox.ResumeLayout(true);
			this.ShipmentCarrierFindBox.PerformLayout();
			this.QuoteCarrierFindBox.ResumeLayout(true);
			this.QuoteCarrierFindBox.PerformLayout();
			this.zCodeFindBox1.ResumeLayout(true);
			this.zCodeFindBox1.PerformLayout();
			this.zCodeFindBox2.ResumeLayout(true);
			this.zCodeFindBox2.PerformLayout();
			this.ValueOfInsuranceCurrencyFindbox.ResumeLayout(true);
			this.ValueOfInsuranceCurrencyFindbox.PerformLayout();
			this.ValueOfGoodsCurrencyFindbox.ResumeLayout(true);
			this.ValueOfGoodsCurrencyFindbox.PerformLayout();
			this.ShipmentChargeableCalcDropEdit.ResumeLayout(true);
			this.ShipmentChargeableCalcDropEdit.PerformLayout();
			this.ShipmentDestinationCodeFindBox.ResumeLayout(true);
			this.ShipmentDestinationCodeFindBox.PerformLayout();
			this.ShipmentVolumeCalcDropEdit.ResumeLayout(true);
			this.ShipmentVolumeCalcDropEdit.PerformLayout();
			this.ShipmentActualWeightDropEdit.ResumeLayout(true);
			this.ShipmentActualWeightDropEdit.PerformLayout();
			this.ShipmentOriginCodeFindBox.ResumeLayout(true);
			this.ShipmentOriginCodeFindBox.PerformLayout();
			this.ShipmentServiceLevelFindBox.ResumeLayout(true);
			this.ShipmentServiceLevelFindBox.PerformLayout();
			this.QuoteChargeableCalcDropEdit.ResumeLayout(true);
			this.QuoteChargeableCalcDropEdit.PerformLayout();
			this.ShipmentDeliveryAddressControl.ResumeLayout(true);
			this.ShipmentDeliveryAddressControl.PerformLayout();
			this.QuoteDeliveryAddressControl.ResumeLayout(true);
			this.QuoteDeliveryAddressControl.PerformLayout();
			this.ShipmentPickupAddressControl.ResumeLayout(true);
			this.ShipmentPickupAddressControl.PerformLayout();
			this.QuotePickupAddressControl.ResumeLayout(true);
			this.QuotePickupAddressControl.PerformLayout();
			this.ShipmentClientFindBox.ResumeLayout(true);
			this.ShipmentClientFindBox.PerformLayout();
			this.QuoteClientFindBox.ResumeLayout(true);
			this.QuoteClientFindBox.PerformLayout();
			this.QuoteDestinationCodeFindBox.ResumeLayout(true);
			this.QuoteDestinationCodeFindBox.PerformLayout();
			this.QuoteVolumeCalcDropEdit.ResumeLayout(true);
			this.QuoteVolumeCalcDropEdit.PerformLayout();
			this.QuoteActualWeightDropEdit.ResumeLayout(true);
			this.QuoteActualWeightDropEdit.PerformLayout();
			this.QuoteOriginCodeFindBox.ResumeLayout(true);
			this.QuoteOriginCodeFindBox.PerformLayout();
			this.QuoteServiceLevelFindBox.ResumeLayout(true);
			this.QuoteServiceLevelFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton UseQuotedPriceButton;
		private Enterprise.ZArchitecture.GUI.ZButton ReRateButton;
		private Enterprise.ZArchitecture.GUI.ZButton HoldForLaterAnalysisButton;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.ZTextBox ShipmentIncoTermTextBox;
		private Enterprise.ZArchitecture.ZTextBox QuoteIncoTermTextBox;
		private Enterprise.ZArchitecture.ZTextBox ShipmentTransportModeTextBox;
		private Enterprise.ZArchitecture.ZTextBox ShipmentContainerModeTextBox;
		private Enterprise.ZArchitecture.ZTextBox QuoteTransportModeTextBox;
		private Enterprise.ZArchitecture.ZTextBox QuoteContainerModeTextBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ShipmentClientFindBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox QuoteClientFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox QuoteDestinationCodeFindBox;
		private Enterprise.ZArchitecture.ZCalcEdit QuoteInsuranceValueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit QuoteValueOfGoodsCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit QuoteVolumeCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit QuoteActualWeightDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox QuoteOriginCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox QuoteServiceLevelFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit QuoteChargeableCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ShipmentDeliveryAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl QuoteDeliveryAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ShipmentPickupAddressControl;
		private Enterprise.ZArchitecture.GUI.ZAddressControl QuotePickupAddressControl;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit ShipmentChargeableCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ShipmentDestinationCodeFindBox;
		private Enterprise.ZArchitecture.ZCalcEdit ShipmentInsuranceValueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ShipmentValueOfGoodsCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit ShipmentVolumeCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit ShipmentActualWeightDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ShipmentOriginCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ShipmentServiceLevelFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ValueOfInsuranceCurrencyFindbox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ValueOfGoodsCurrencyFindbox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox2;
		private MasterFiles.GUI.ZOrganisationFindBox QuoteCarrierFindBox;
		private MasterFiles.GUI.ZOrganisationFindBox ShipmentCarrierFindBox;
	}
}
