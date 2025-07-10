using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	class ShipmentExportAWBHeaderValidationTest : ForwardingExportAWBHeaderValidationTest
	{
		public void TestNotificationLevelForFieldsRequiredForElectronicTransmission()
		{
			AssertEquals("Precondition: Registry.AllowUsersToSendFWBsWithErrors", false, ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.Value);
			var header1 = Factory.New<ShipmentExportAWBHeader>();
			header1.EH_ShipperName = "Something";
			header1.EH_ShipperAddress = ZString.Empty;
			header1.Validation.ValidateEH_ShipperAddress();
			AssertHasMessageErrors(header1.EH_ShipperAddressInfo);
			AssertNoWarnings(header1.EH_ShipperAddressInfo);

			ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var header2 = Factory.New<ShipmentExportAWBHeader>();
			header2.EH_ShipperName = "Something else";
			header2.EH_ShipperAddress = ZString.Empty;
			header2.Validation.ValidateEH_ShipperAddress();
			AssertNoMessageErrors(header2.EH_ShipperAddressInfo);
			AssertHasWarnings(header2.EH_ShipperAddressInfo);
		}

		public void TestMandatoryShipperConsigneeFields()
		{
			AWBHeader.Validation.ValidateAll();
			AssertNoMessageError(AWBHeader.EH_ShipperNameInfo, "Name is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperAddressInfo, "Address is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperPlaceInfo, "City is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperCountryCodeInfo, "Country/Region Code is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ConsigneeNameInfo, "Name is required when other shipper/consignee details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ConsigneeAddressInfo, "Address is required when other shipper/consignee details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ConsigneePlaceInfo, "City is required when other shipper/consignee details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ConsigneeCountryCodeInfo, "Country/Region Code is required when other shipper/consignee details are supplied.");

			AWBHeader.EH_ShipperAccount = "###";
			AWBHeader.Validation.ValidateAll();
			AssertNoMessageError(AWBHeader.EH_ShipperNameInfo, "Name is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperAddressInfo, "Address is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperPlaceInfo, "City is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperCountryCodeInfo, "Country/Region Code is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ConsigneeNameInfo, "Name is required when other shipper/consignee details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ConsigneeAddressInfo, "Address is required when other shipper/consignee details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ConsigneePlaceInfo, "City is required when other shipper/consignee details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ConsigneeCountryCodeInfo, "Country/Region Code is required when other shipper/consignee details are supplied.");

			AWBHeader.EH_ShipperAccount = "Bob";
			AWBHeader.Validation.ValidateAll();
			AssertHasMessageError(AWBHeader.EH_ShipperNameInfo, "Name is required when other shipper details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ShipperAddressInfo, "Address is required when other shipper details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ShipperPlaceInfo, "City is required when other shipper details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ShipperCountryCodeInfo, "Country/Region Code is required when other shipper details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ConsigneeNameInfo, "Name is required when other shipper/consignee details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ConsigneeAddressInfo, "Address is required when other shipper/consignee details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ConsigneePlaceInfo, "City is required when other shipper/consignee details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ConsigneeCountryCodeInfo, "Country/Region Code is required when other shipper/consignee details are supplied.");

			AWBHeader.EH_ShipperAccount = ZString.Empty;
			AWBHeader.EH_ConsigneeAccount = "Bob";
			AWBHeader.Validation.ValidateAll();
			AssertNoMessageError(AWBHeader.EH_ShipperNameInfo, "Name is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperAddressInfo, "Address is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperPlaceInfo, "City is required when other shipper details are supplied.");
			AssertNoMessageError(AWBHeader.EH_ShipperCountryCodeInfo, "Country/Region Code is required when other shipper details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ConsigneeNameInfo, "Name is required when other shipper/consignee details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ConsigneeAddressInfo, "Address is required when other shipper/consignee details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ConsigneePlaceInfo, "City is required when other shipper/consignee details are supplied.");
			AssertHasMessageError(AWBHeader.EH_ConsigneeCountryCodeInfo, "Country/Region Code is required when other shipper/consignee details are supplied.");
		}

		public void TestCheckEH_Currency()
		{
			AWBHeader.EH_Currency = "";
			AssertNoNotifications(AWBHeader.EH_CurrencyInfo);

			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AWBHeader.Validation.ValidateEH_Currency();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_CurrencyInfo);

			AWBHeader.EH_Currency = "AUD";
			AssertNoNotifications(AWBHeader.EH_CurrencyInfo);
		}

		public void TestCheckEH_OtherPrepaidCollect()
		{
			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both;
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_OtherPrepaidCollectInfo);
			AssertEquals(1, AWBHeader.EH_OtherPrepaidCollectInfo.GetMessageErrors().Count());
			ZString warning = "Payment type should be either Prepaid or Collect.";
			AssertHasMessageErrorContaining(AWBHeader.EH_OtherPrepaidCollectInfo, warning);

			AWBHeader.EH_OtherPrepaidCollect = "";
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_Currency = "AUD";
			AWBHeader.Validation.ValidateEH_OtherPrepaidCollect();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = "X";
			AssertHasErrors(AWBHeader.EH_OtherPrepaidCollectInfo);
		}

		public void TestCheckEH_WeightPrepaidCollect()
		{
			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertNoNotifications(AWBHeader.EH_WeightPrepaidCollectInfo);

			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertNoNotifications(AWBHeader.EH_WeightPrepaidCollectInfo);

			AWBHeader.EH_WeightPrepaidCollect = ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both;
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_WeightPrepaidCollectInfo);
			AssertEquals(1, AWBHeader.EH_WeightPrepaidCollectInfo.GetMessageErrors().Count());
			AssertHasMessageErrorContaining(AWBHeader.EH_WeightPrepaidCollectInfo, "Payment type should be either Prepaid or Collect.");

			AWBHeader.EH_WeightPrepaidCollect = "";
			AssertNoNotifications(AWBHeader.EH_WeightPrepaidCollectInfo);

			AWBHeader.EH_Currency = "AUD";
			AWBHeader.Validation.ValidateEH_WeightPrepaidCollect();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_WeightPrepaidCollectInfo);

			AWBHeader.EH_WeightPrepaidCollect = "X";
			AssertHasError(AWBHeader.EH_WeightPrepaidCollectInfo, "Enter a valid selection.");
		}

		public void TestCheckEH_CustomsValue()
		{
			var goodsValueMessageError = "Goods Value (per the commercial invoice) is required for imports to Bangladesh.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "BDKHL";
			AWBHeader.EH_ParentID = shipment.PK;
			AWBHeader.EH_CustomsValue = 0;

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertNoMessageError("No error expected because registry is set to 'No'.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);
			}

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertHasMessageError("Error expected when registry is set to 'Yes', non-Direct consol and BD import has no goods value specified.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);

				consol.JK_AgentType = Core.Constants.AgentType.Direct;
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertNoMessageError("No error expected because consol is direct.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);

				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				AWBHeader.EH_CustomsValue = 10;
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertNoMessageError("No error expected because BD import has goods value specified.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);

				shipment.JS_RL_NKDestination = "VNVNH";
				AWBHeader.EH_CustomsValue = 0;
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertNoMessageError("No error expected when non-BD import has no goods value specified.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);
			}
		}

		#region EH_ECNCRNNumber Validation

		public void TestCheckEH_ECNCRNNumberForMultipleCustomsEntryNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AWBHeader.EH_ParentID = shipment.PK;

			CusEntryNumber cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = "111";
			cusEntryNum1.CE_EntryType = "MRN";
			cusEntryNum1.CE_ParentTable = ForwardingShipment.Schema.TableName;

			CusEntryNumber cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum2.CE_EntryNum = "10AD00000117716870";
			cusEntryNum2.CE_EntryType = "MRN";
			cusEntryNum2.CE_ParentTable = ForwardingShipment.Schema.TableName;

			CusEntryNumber cusEntryNum3 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum3.CE_EntryNum = "333";
			cusEntryNum3.CE_EntryType = "CAN";
			cusEntryNum3.CE_ParentTable = ForwardingShipment.Schema.TableName;

			var expectedMessage0 = "This Shipment is not attached to a Consolidation";
			var expectedMessage1 = @"MRN does not conform to the correct format. Please enter the MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit";
			var expectedMessage2 = "MRN does not have a valid check (last) digit. The check digit should be 9";
			var expectedMessage3 = "CAN must be 9 characters.";
			var expectedMessage4 = "If more than four entry numbers present on a shipment, they will not be shown on this document but will be included in the FHL and FWB airline messages.";

			Factory.Save();

			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertHasWarning("EH_ECNCRNNumber should have all warnings of shipment customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage0);
			AssertHasWarning("EH_ECNCRNNumber should have all warnings of shipment customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage1);
			AssertHasWarning("EH_ECNCRNNumber should have all warnings of shipment customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage2);
			AssertHasWarning("EH_ECNCRNNumber should have all warnings of shipment customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage3);

			CusEntryNumber cusEntryNum4 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum4.CE_EntryNum = "444";
			cusEntryNum4.CE_EntryType = "CAN";
			cusEntryNum4.CE_ParentTable = ForwardingShipment.Schema.TableName;

			CusEntryNumber cusEntryNum5 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum5.CE_EntryNum = "555";
			cusEntryNum5.CE_EntryType = "CAN";
			cusEntryNum5.CE_ParentTable = ForwardingShipment.Schema.TableName;

			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertHasWarning("EH_ECNCRNNumber should have warning when shipment have more than 4 customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage4);
		}

		public void TestCheckEH_ECNCRNNumberForConsolFromSwitzerland()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CHABL";
			consol.JK_RL_NKDischargePort = "USCHI";
			var transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_RL_NKLoadPort = "CHABL";
			transport.JW_RL_NKDiscPort = "USCHI";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CHBSL";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AWBHeader.EH_ParentID = shipment.PK;

			var cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = ZString.Empty;
			cusEntryNum1.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;
			cusEntryNum1.CE_ParentTable = shipment.TableName;
			var cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum2.CE_EntryNum = ZString.Empty;
			cusEntryNum2.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;
			cusEntryNum2.CE_ParentTable = shipment.TableName;

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "CHBSL";
			consol2.JK_RL_NKDischargePort = "USCHI";
			var transport2 = consol2.Transports[0];
			transport2.JW_ETD = ZDateTime.Today.AddDays(2);
			transport2.JW_RL_NKLoadPort = "CHABL";
			transport2.JW_RL_NKDiscPort = "USCHI";

			Factory.Save();

			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors("Should not display error message if Consol is direct", AWBHeader.EH_ECNCRNNumberInfo);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			var expectedMessage = $"For exports from Switzerland by airfreight, to meet RFS (Road Feeder Service) customs filing requirements, either the GDRN (Goods Declaration Reference Number) or HS code(s) are required. The GDRN or HS Code is missing from at least one Packline of {shipment.JS_UniqueConsignRef}.";
			AssertHasMessageError(AWBHeader.EH_ECNCRNNumberInfo, expectedMessage);

			packline1.JL_HarmonisedCode = "123456";
			packline2.JL_HarmonisedCode = "654321";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors(AWBHeader.EH_ECNCRNNumberInfo);

			packline1.JL_HarmonisedCode = ZString.Empty;
			packline2.JL_HarmonisedCode = ZString.Empty;
			packline1.HarmonisedCodes.HSCodeManager.Value = "123456";
			packline1.HarmonisedCodes.HSCountryManager.Value = "CH";
			packline2.HarmonisedCodes.HSCodeManager.Value = "654321";
			packline2.HarmonisedCodes.HSCountryManager.Value = "CH";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors(AWBHeader.EH_ECNCRNNumberInfo);

			packline1.HarmonisedCodes.HSCodeManager.Value = ZString.Empty;
			packline1.HarmonisedCodes.HSCountryManager.Value = ZString.Empty;
			packline2.HarmonisedCodes.HSCodeManager.Value = ZString.Empty;
			packline2.HarmonisedCodes.HSCountryManager.Value = ZString.Empty;
			cusEntryNum1.CE_EntryNum = "GDRN1111";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors("cusEntryNum1 is a available GDRN", AWBHeader.EH_ECNCRNNumberInfo);

			cusEntryNum1.CE_EntryType = CusEntryNumberTypes.Switzerland.AccessCode;
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertHasMessageError(AWBHeader.EH_ECNCRNNumberInfo, expectedMessage);

			consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors("The consol is not from Switzerland", AWBHeader.EH_ECNCRNNumberInfo);
		}

		#endregion

		#region TraderTypeAndNo Validations

		class OriginDestExportAWBHeaderForTest : ShipmentExportAWBHeader
		{
			public OriginDestExportAWBHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public override RefCountry OriginCountry => Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, OriginCountryCodeForTesting);
			public override RefCountry DestinationCountry => Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, DestinationCountryCodeForTesting);

			public ZString OriginCountryCodeForTesting { get; set; }
			public ZString DestinationCountryCodeForTesting { get; set; }
		}

		public void TestShipperRequiredTraderTypeAndNoMessage()
		{
			var header = Factory.New<OriginDestExportAWBHeaderForTest>();

			header.OriginCountryCodeForTesting = Constants.CountryCodes.Indonesia;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_ShipperTraderNoTypeInfo, header.EH_ShipperTraderNoInfo, "Shipper PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");

			header.OriginCountryCodeForTesting = Constants.CountryCodes.VietNam;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_ShipperTraderNoTypeInfo, header.EH_ShipperTraderNoInfo, "Company ID, VAT (Government VAT Code) is required to comply with Vietnam Customs notice No. 6889/TCHQ-GSQL");

			header.OriginCountryCodeForTesting = Constants.CountryCodes.China;
			AssertNoWarnings(header.EH_ShipperTraderNoTypeInfo);
			AssertNoWarnings(header.EH_ShipperTraderNoInfo);

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.Egypt;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_ShipperTraderNoTypeInfo, header.EH_ShipperTraderNoInfo, "Exporter registration number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the relevant company number against the Consignor Organization.");
		}

		public void TestCheckEH_ShipperTraderNoType_TaxInfoFoundValidation()
		{
			var awbHeader = Factory.New<OriginDestExportAWBHeaderForTest>();
			awbHeader.EH_ShipperTraderNoType = "8888";
			awbHeader.OriginCountryCodeForTesting = Constants.CountryCodes.China;

			awbHeader.Validation.ValidateEH_ShipperTraderNoType();
			AssertNoWarnings("Validation for 8888 will no longer be shown.", awbHeader.EH_ShipperTraderNoTypeInfo);

			awbHeader.OriginCountryCodeForTesting = Constants.CountryCodes.Canada;

			awbHeader.Validation.ValidateEH_ShipperTraderNoType();
			AssertNoWarnings("Validation for 8888 will no longer be shown.", awbHeader.EH_ShipperTraderNoTypeInfo);
		}

		public void TestConsigneeRequiredTraderTypeAndNoMessage()
		{
			var header = Factory.New<OriginDestExportAWBHeaderForTest>();

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.VietNam;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_ConsigneeTraderNoTypeInfo, header.EH_ConsigneeTraderNoInfo, "Company ID, VAT (Government VAT Code) is required to comply with Vietnam Customs notice No. 6889/TCHQ-GSQL");

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.Indonesia;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_ConsigneeTraderNoTypeInfo, header.EH_ConsigneeTraderNoInfo, "Consignee PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.Egypt;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_ConsigneeTraderNoTypeInfo, header.EH_ConsigneeTraderNoInfo, "VAT number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the VAT against the Organization.");

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.Canada;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_ConsigneeTraderNoTypeInfo, header.EH_ConsigneeTraderNoInfo, "For imports to Canada, the Sending Forwarder’s code provided by the CBSA (Canada Border Services Agency) is required for eManifest reporting. Use the Organization Registration Number type ‘CCC’ (Carrier Code) to save this code.");

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.China;
			AssertNoWarnings(header.EH_ConsigneeTraderNoTypeInfo);
			AssertNoWarnings(header.EH_ConsigneeTraderNoInfo);

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.India;
			AssertNoWarnings(header.EH_ConsigneeTraderNoTypeInfo);
			AssertNoWarnings(header.EH_ConsigneeTraderNoInfo);
		}

		public void TestIndiaAlsoNotifyPartyNoWarning()
		{
			var header = Factory.New<OriginDestExportAWBHeaderForTest>();

			header.EH_AlsoNotifyName = "NotifyParty";
			header.DestinationCountryCodeForTesting = Constants.CountryCodes.India;
			header.Validation.ValidateAll();
			AssertNoWarnings(header.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(header.EH_AlsoNotifyTraderNoInfo);
		}

		public void TestCheckEH_ConsigneeTraderNoType_TaxInfoFoundValidation()
		{
			var awbHeader = Factory.New<OriginDestExportAWBHeaderForTest>();
			awbHeader.EH_ConsigneeTraderNoType = "8888";
			awbHeader.OriginCountryCodeForTesting = Constants.CountryCodes.China;

			awbHeader.Validation.ValidateEH_ConsigneeTraderNoType();
			AssertNoWarnings("Validation for 8888 will no longer be shown.", awbHeader.EH_ConsigneeTraderNoTypeInfo);
		}

		public void TestAlsoNotifyRequiredTraderTypeAndNoMessage()
		{
			var header = Factory.New<OriginDestExportAWBHeaderForTest>();

			header.EH_AlsoNotifyName = "NotifyParty";
			header.DestinationCountryCodeForTesting = Constants.CountryCodes.VietNam;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_AlsoNotifyTraderNoTypeInfo, header.EH_AlsoNotifyTraderNoInfo, "Company ID, VAT (Government VAT Code) is required to comply with Vietnam Customs notice No. 6889/TCHQ-GSQL");

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.Indonesia;
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_AlsoNotifyTraderNoTypeInfo, header.EH_AlsoNotifyTraderNoInfo, "Also Notify PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.Canada;
			const string canadaMessage = "For imports to Canada, the Sending Forwarder’s code provided by the CBSA (Canada Border Services Agency) is required for eManifest reporting. Use the Organization Registration Number type ‘CCC’ (Carrier Code) to save this code.";
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_AlsoNotifyTraderNoTypeInfo, header.EH_AlsoNotifyTraderNoInfo, canadaMessage);

			const string egyptMessage = "VAT number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the VAT against the Organization.";
			header.DestinationCountryCodeForTesting = Constants.CountryCodes.Egypt;
			header.EH_AlsoNotifyName = "";
			header.EH_AlsoNotifyTraderNoType = "";
			header.EH_AlsoNotifyTraderNo = "";
			AssertNoWarning("Warning is not shown when Also Notify org is not supplied.", header.EH_AlsoNotifyTraderNoTypeInfo, egyptMessage);
			AssertNoWarning("Warning is not shown when Also Notify org is not supplied.", header.EH_AlsoNotifyTraderNoInfo, egyptMessage);

			header.EH_AlsoNotifyName = "Notify Me";
			header.EH_AlsoNotifyAddress = "Notify St";
			header.EH_AlsoNotifyPlace = "Notify Pl";
			header.EH_AlsoNotifyCountryCode = "NT";
			AssertRequiredTraderTypeAndNoMessage(header, header.EH_AlsoNotifyTraderNoTypeInfo, header.EH_AlsoNotifyTraderNoInfo, egyptMessage);

			header.DestinationCountryCodeForTesting = Constants.CountryCodes.China;
			AssertNoWarnings(header.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(header.EH_AlsoNotifyTraderNoInfo);
		}

		public void TestCheckEH_AlsoNotifyTraderNoType_TaxInfoFoundValidation()
		{
			var awbHeader = Factory.New<OriginDestExportAWBHeaderForTest>();
			awbHeader.EH_AlsoNotifyTraderNoType = "8888";
			awbHeader.OriginCountryCodeForTesting = Constants.CountryCodes.China;

			awbHeader.Validation.ValidateEH_AlsoNotifyTraderNoType();
			AssertNoWarnings("Validation for 8888 will no longer be shown.", awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
		}

		public void TestShipperTraderTypeAndNoMessage_RequiredTraderTypes()
		{
			CreateRefDocOrgCusCodesForTesting();

			var expectedMessage = "VAT is required for Turkey exports.";
			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_RL_NKOrigin = "TRIST";
			forwardingShipment.JS_RL_NKDestination = "AUSYD";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = forwardingShipment.PK;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();

			AssertRequiredTraderTypeAndNoMessage(awbHeader, awbHeader.EH_ShipperTraderNoTypeInfo, awbHeader.EH_ShipperTraderNoInfo, expectedMessage);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes()
		{
			CreateRefDocOrgCusCodesForTesting();

			var expectedMessage = "VAT is required for Turkey imports.";
			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_RL_NKOrigin = "AUSYD";
			forwardingShipment.JS_RL_NKDestination = "TRIST";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = forwardingShipment.PK;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();

			AssertRequiredTraderTypeAndNoMessage(awbHeader, awbHeader.EH_ConsigneeTraderNoTypeInfo, awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes_OneDischargeLegInEU()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			var expectedMessage = "The Consignee’s EORI/CH UID/NO MVA number is required by some airlines for imports into EU, CH, LI, NO, XI unless the Consignee is a natural person/individual.";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "DEHAM";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			awbHeader.Populate();

			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			consignee.OH_Category = "NAT";

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_ConsigneeTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_ConsigneeTraderNoInfo);

			consignee.OH_Category = "BUS";
			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			shipment.JS_OverrideWaybillDefaults = true;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			shipment.JS_OverrideWaybillDefaults = false;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_ConsigneeTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_ConsigneeTraderNoInfo);

			code1.OK_CustomsRegNo = string.Empty;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "DEHAM";
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			receivingForwarder.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;
			awbHeader.Populate();
			Assert(awbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);
			AssertNoWarnings("Should have no warning when consignee is declarant for Advance Cargo Reporting", awbHeader.EH_ConsigneeTraderNoTypeInfo);
			AssertNoWarnings("Should have no warning when consignee is declarant for Advance Cargo Reporting", awbHeader.EH_ConsigneeTraderNoInfo);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredCompID_Israel()
		{
			CreateRefDocOrgCusCodesForTesting();
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IL2LL";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsigneePK = consignee.PK;

			var expectedMessage = @"VAT Number is required for imports to Israel to comply with Manifest reporting.";
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Israel;
			code1.OK_CustomsRegNo = "12345";
			awbHeader.Populate();

			AssertNoWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredTraderTypes_OneDischargeLegInEU()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			var expectedMessage = "The Notify Party’s EORI/CH UID/NO MVA number is required by some airlines for imports into EU, CH, LI, NO, XI unless the Notify Party is a natural person/individual.";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "DEHAM";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			awbHeader.Populate();

			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoInfo);

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = GenerateOrganisationIsIcs2Country().PK;
			awbHeader.Populate();

			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			notifyParty.Organisation.OH_Category = "NAT";

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoInfo);

			notifyParty.Organisation.OH_Category = "BUS";

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			shipment.JS_OverrideWaybillDefaults = true;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			shipment.JS_OverrideWaybillDefaults = false;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoInfo);

			code1.OK_CustomsRegNo = string.Empty;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "DEHAM";
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			receivingForwarder.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;
			awbHeader.Populate();
			Assert(awbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);
			AssertNoWarnings("Should have no warning when consignee is declarant for Advance Cargo Reporting", awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings("Should have no warning when consignee is declarant for Advance Cargo Reporting", awbHeader.EH_AlsoNotifyTraderNoInfo);
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_WhenNotifyPartyIsNotIcs2Member_HideMessage()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			var expectedMessage = "The Notify Party’s EORI/CH UID/NO MVA number is required by some airlines for imports into EU, CH, LI, NO, XI unless the Notify Party is a natural person/individual.";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "DEHAM";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;

			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = GenerateOrganisationIsIcs2Country().PK;
			Assert(shipment.NotifyPartyDocumentaryAddress.Organisation.Country.IsIcs2Member);
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = GenerateOrganisationIsNotIcs2Country().PK;
			Assert(!shipment.NotifyPartyDocumentaryAddress.Organisation.Country.IsIcs2Member);
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoInfo);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes_ImportChina()
		{
			var expectedMessage = "The Consignee’s USCI (Unified Social Credit Identifier) is required for imports to China.";
			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = "USC";
			orgCusCode.DOC_RN_NKRegulatingCountry = "CN";
			orgCusCode.DOC_RN_NKCodeCountry = "CN";
			orgCusCode.DOC_Priority = 1;
			orgCusCode.DOC_ShortLabel = "USCI";
			orgCusCode.DOC_LongLabel = "USCI";
			orgCusCode.DOC_DocumentType = "HAW";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "CNSHA";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			awbHeader.Populate();

			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			shipment.JS_OverrideWaybillDefaults = true;
			awbHeader.EH_ConsigneeTraderNo = "12345";
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			shipment.JS_OverrideWaybillDefaults = false;
			awbHeader.EH_ConsigneeTraderNo = "12345";
			AssertNoWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredTraderTypes_ImportChina()
		{
			var expectedMessage = "The Notify Party’s USCI (Unified Social Credit Identifier) is required for imports to China.";

			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = "USC";
			orgCusCode.DOC_RN_NKRegulatingCountry = "CN";
			orgCusCode.DOC_RN_NKCodeCountry = "CN";
			orgCusCode.DOC_Priority = 1;
			orgCusCode.DOC_ShortLabel = "USCI";
			orgCusCode.DOC_LongLabel = "USCI";
			orgCusCode.DOC_DocumentType = "HAW";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "CNSHA";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;

			awbHeader.Populate();
			AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = "CN";
			notifyParty.OrganisationPK = orgHeader.PK;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			shipment.JS_OverrideWaybillDefaults = true;
			awbHeader.EH_AlsoNotifyTraderNo = "12345";
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			shipment.JS_OverrideWaybillDefaults = false;
			awbHeader.EH_AlsoNotifyTraderNo = "12345";
			AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes_Morocco()
		{
			var expectedMessage = "The Consignee's ICE number is required for inbound shipments to Morocco.";
			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = "ICE";
			orgCusCode.DOC_RN_NKRegulatingCountry = "MA";
			orgCusCode.DOC_RN_NKCodeCountry = "MA";
			orgCusCode.DOC_Priority = 1;
			orgCusCode.DOC_ShortLabel = "ICE";
			orgCusCode.DOC_LongLabel = "ICE";
			orgCusCode.DOC_DocumentType = "HAW";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "MACAS";

			var consignor = Factory.New<OrgHeader>();
			shipment.ConsigneePK = consignor.PK;

			var consignee = shipment.Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Morocco;

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			awbHeader.Populate();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			code1.OK_CustomsRegNo = "123456789123456";
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredTraderTypes_Morocco()
		{
			var expectedMessage = "The Notify Party’s ICE number is required for inbound shipments to Morocco.";

			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = "ICE";
			orgCusCode.DOC_RN_NKRegulatingCountry = "MA";
			orgCusCode.DOC_RN_NKCodeCountry = "MA";
			orgCusCode.DOC_Priority = 1;
			orgCusCode.DOC_ShortLabel = "ICE";
			orgCusCode.DOC_LongLabel = "ICE";
			orgCusCode.DOC_DocumentType = "HAW";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "MACAS";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;

			awbHeader.Populate();
			AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = "MA";
			notifyParty.OrganisationPK = orgHeader.PK;

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Morocco;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			code1.OK_CustomsRegNo = "111111111111111";
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredTraderTypes()
		{
			CreateRefDocOrgCusCodesForTesting();

			var expectedMessage = "VAT is required for Turkey imports.";
			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_RL_NKOrigin = "AUSYD";
			forwardingShipment.JS_RL_NKDestination = "TRIST";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Notify Party";
			forwardingShipment.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = forwardingShipment.PK;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();

			AssertRequiredTraderTypeAndNoMessage(awbHeader, awbHeader.EH_AlsoNotifyTraderNoTypeInfo, awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredCompID_Israel()
		{
			CreateRefDocOrgCusCodesForTesting();
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IL2LL";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Notify Party";
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsigneePK = consignee.PK;

			var expectedMessage = @"VAT Number is required for imports to Israel to comply with Manifest reporting.";
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Israel;
			code1.OK_CustomsRegNo = "678910";
			awbHeader.Populate();

			AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);
		}

		void CreateRefDocOrgCusCodesForTesting()
		{
			void CreateRefDocOrgCusCode(ZString code, ZString regulatingCountry, ZString codeCountry, ZByte priority, ZString shortLabel, ZString longLabel, ZString documentType)
			{
				var orgCusCode = Factory.New<RefDocOrgCusCode>();
				orgCusCode.DOC_CodeType = code;
				orgCusCode.DOC_RN_NKRegulatingCountry = regulatingCountry;
				orgCusCode.DOC_RN_NKCodeCountry = codeCountry;
				orgCusCode.DOC_Priority = priority;
				orgCusCode.DOC_ShortLabel = shortLabel;
				orgCusCode.DOC_LongLabel = longLabel;
				orgCusCode.DOC_DocumentType = documentType;
			}

			CreateRefDocOrgCusCode("VAT", "TR", "TR", 1, "VAT", "VAT", "HAW");
			CreateRefDocOrgCusCode("VAT", "IL", "IL", 1, "VAT", "VAT NUMBER", "HAW");
			CreateRefDocOrgCusCode("EOR", "GB", "GB", 1, "EOR", "EORI NUMBER", "HAW");
		}

		#endregion

		#region Handling Information

		public void TestHandlingInformationACIDNumberForEgypt()
		{
			const string expectedNotification = "The Shipment's ACID number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the ACI in the Shipment's Reference Numbers grid.";

			var forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_RL_NKOrigin = "AUBNE";
			forwardingShipment.JS_RL_NKDestination = "ECARE";

			var header = Factory.New<ShipmentExportAWBHeader>();
			header.EH_ParentID = forwardingShipment.PK;
			header.Populate();
			header.Validation.ValidateAll();

			AssertNoWarning(header.EH_HandlingInformationInfo, expectedNotification);

			forwardingShipment.JS_RL_NKDestination = "EGCAI";
			header.Populate();
			header.Validation.ValidateAll();

			AssertHasWarning(header.EH_HandlingInformationInfo, expectedNotification);

			header.EH_HandlingInformation = "Handling Information including ACID Number:12345 And extra information";
			header.Validation.ValidateAll();

			AssertNoWarning(header.EH_HandlingInformationInfo, expectedNotification);
		}

		#endregion

		#region AsAgreedAllowed Brasil

		const string validationErrorBrazil = "As Agreed cannot be selected on HAWB for Imports to Brazil";

		public void TestAsAgreed1st_Brasil()
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;

			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);

			shipment.JS_RL_NKDestination = "BRBRA";
			awbHeader.Validation.ValidateAll();
			AssertHasError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
		}

		public void TestAsAgreed2nd_Brasil()
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUSYD";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;

			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			awbHeader.Shipment.JS_RL_NKDestination = "BRRBA";
			awbHeader.Validation.ValidateAll();

			AssertHasError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);
		}

		public void TestAsAgreedValidatedOnOriginChange_Brasil()
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUBNE";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "BRBRA";
			shipment.JS_RL_NKDestination = "BRRBA";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;

			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			shipment.JS_RL_NKOrigin = "BRBSB";
			awbHeader.Validation.ValidateAll();
			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			shipment.JS_RL_NKOrigin = "AUSYD";
			awbHeader.Validation.ValidateAll();
			AssertHasError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertHasError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);
		}

		public void TestAsAgreedResetOnDestinationChange_Brasil()
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUBNE";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;

			shipment.JS_RL_NKDestination = "BRBSB";
			awbHeader.Validation.ValidateAll();
			AssertHasError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertHasError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			shipment.JS_RL_NKOrigin = "BRBRA";
			shipment.JS_RL_NKDestination = "BRRBA";
			awbHeader.Validation.ValidateAll();
			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);
		}

		#endregion

		#region Shipper

		public void TestShipper_China()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AWBHeader.EH_ParentID = shipment.PK;
			AWBHeader.EH_Table = JobShipmentSchema.Constants.TableName;
			AWBHeader.Shipment.Transports.AddNew();

			AssertEH_ShipperContactName_China();
			AssertEH_ShipperContactCode_NotPhone_China();
			AssertEH_ShipperContactDetail_Empty_China();
		}

		void AssertEH_ShipperContactName_China()
		{
			const string message = "Contact Name is required for China transit";
			SetIsTransitingThroughChinaForTest(true);
			Assert(AWBHeader.IsTransitingThroughChina);

			AWBHeader.EH_ShipperContactName = ZString.Empty;
			AssertHasWarning(AWBHeader.EH_ShipperContactNameInfo, message);

			AWBHeader.EH_ShipperContactName = "%$)";
			AssertHasWarning(AWBHeader.EH_ShipperContactNameInfo, message);

			AWBHeader.EH_ShipperContactName = "Some Guy";
			AssertNoWarning(AWBHeader.EH_ShipperContactNameInfo, message);

			SetIsTransitingThroughChinaForTest(false);
			Assert(!AWBHeader.IsTransitingThroughChina);

			AWBHeader.EH_ShipperContactName = ZString.Empty;
			AssertNoWarning(AWBHeader.EH_ShipperContactNameInfo, message);
		}

		void AssertEH_ShipperContactCode_NotPhone_China()
		{
			const string message = "Contact type \"Telephone\" is required for China transit";
			SetIsTransitingThroughChinaForTest(true);

			AWBHeader.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertHasWarning(AWBHeader.EH_ShipperContactCodeInfo, message);

			AWBHeader.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertHasWarning(AWBHeader.EH_ShipperContactCodeInfo, message);

			AWBHeader.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			AssertNoWarning(AWBHeader.EH_ShipperContactCodeInfo, message);

			AWBHeader.EH_ShipperContactCode = ZString.Empty;
			AssertHasWarning(AWBHeader.EH_ShipperContactCodeInfo, message);

			SetIsTransitingThroughChinaForTest(false);

			AWBHeader.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertNoWarning(AWBHeader.EH_ShipperContactCodeInfo, message);

			AWBHeader.EH_ShipperContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertNoWarning(AWBHeader.EH_ShipperContactCodeInfo, message);
		}

		void AssertEH_ShipperContactDetail_Empty_China()
		{
			const string message = "Contact telephone number is required for China transit";
			SetIsTransitingThroughChinaForTest(true);

			AWBHeader.EH_ShipperContactDetail = ZString.Empty;
			AssertHasWarning(AWBHeader.EH_ShipperContactDetailInfo, message);

			AWBHeader.EH_ShipperContactDetail = "^%$";
			AssertHasWarning(AWBHeader.EH_ShipperContactDetailInfo, message);

			AWBHeader.EH_ShipperContactDetail = "111";
			AssertNoWarning(AWBHeader.EH_ShipperContactDetailInfo, message);

			SetIsTransitingThroughChinaForTest(false);

			AWBHeader.EH_ShipperContactDetail = ZString.Empty;
			AssertNoWarning(AWBHeader.EH_ShipperContactDetailInfo, message);
		}

		#endregion

		#region Consignee

		public void TestCheckConsignee_China()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AWBHeader.EH_ParentID = shipment.PK;
			AWBHeader.EH_Table = JobShipmentSchema.Constants.TableName;
			AWBHeader.Shipment.Transports.AddNew();

			setHeaderToChinaPortFunc = () => { SetIsImportToChinaForTest(true); };
			setHeaderToNonChinaPortFunc = () => { SetIsImportToChinaForTest(false); };
			AssertEH_ConsigneeContactName_China("Contact Name is required for China imports");
			AssertEH_ConsigneeContactCode_NotPhone_China("Contact type \"Telephone\" is required for China imports", false);
			AssertEH_ConsigneeContactDetail_Empty_China("Contact telephone number is required for China imports");

			setHeaderToChinaPortFunc = () => { SetIsTransitingThroughChinaForTest(true); };
			setHeaderToNonChinaPortFunc = () => { SetIsTransitingThroughChinaForTest(false); };
			AssertEH_ConsigneeContactName_China("Contact Name is required for China transit");
			AssertEH_ConsigneeContactCode_NotPhone_China("Contact type \"Telephone\" is required for China transit", true);
			AssertEH_ConsigneeContactDetail_Empty_China("Contact telephone number is required for China transit");
		}

		void AssertEH_ConsigneeContactName_China(string message)
		{
			setHeaderToChinaPortFunc();

			AWBHeader.EH_ConsigneeContactName = ZString.Empty;
			AssertHasWarning(AWBHeader.EH_ConsigneeContactNameInfo, message);

			AWBHeader.EH_ConsigneeContactName = "%$)";
			AssertHasWarning(AWBHeader.EH_ConsigneeContactNameInfo, message);

			AWBHeader.EH_ConsigneeContactName = "Some Guy";
			AssertNoWarning(AWBHeader.EH_ConsigneeContactNameInfo, message);

			setHeaderToNonChinaPortFunc();
			AWBHeader.EH_ConsigneeContactName = ZString.Empty;
			AssertNoWarning(AWBHeader.EH_ConsigneeContactNameInfo, message);
		}

		void AssertEH_ConsigneeContactCode_NotPhone_China(string message, bool warnWhenEmpty)
		{
			setHeaderToChinaPortFunc();

			AWBHeader.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertHasWarning(AWBHeader.EH_ConsigneeContactCodeInfo, message);

			AWBHeader.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertHasWarning(AWBHeader.EH_ConsigneeContactCodeInfo, message);

			AWBHeader.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			AssertNoWarning(AWBHeader.EH_ConsigneeContactCodeInfo, message);

			if (warnWhenEmpty)
			{
				AWBHeader.EH_ConsigneeContactCode = ZString.Empty;
				AssertHasWarning(AWBHeader.EH_ConsigneeContactCodeInfo, message);
			}
			else
			{
				AWBHeader.EH_ConsigneeContactCode = ZString.Empty;
				AssertNoWarning(AWBHeader.EH_ConsigneeContactCodeInfo, message);
			}

			setHeaderToNonChinaPortFunc();

			AWBHeader.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertNoWarning(AWBHeader.EH_ConsigneeContactCodeInfo, message);

			AWBHeader.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertNoWarning(AWBHeader.EH_ConsigneeContactCodeInfo, message);
		}

		void AssertEH_ConsigneeContactDetail_Empty_China(string message)
		{
			setHeaderToChinaPortFunc();

			AWBHeader.EH_ConsigneeContactDetail = ZString.Empty;
			AssertHasWarning(AWBHeader.EH_ConsigneeContactDetailInfo, message);

			AWBHeader.EH_ConsigneeContactDetail = "^%$";
			AssertHasWarning(AWBHeader.EH_ConsigneeContactDetailInfo, message);

			AWBHeader.EH_ConsigneeContactDetail = "111";
			AssertNoWarning(AWBHeader.EH_ConsigneeContactDetailInfo, message);

			setHeaderToNonChinaPortFunc();

			AWBHeader.EH_ConsigneeContactDetail = ZString.Empty;
			AssertNoWarning(AWBHeader.EH_ConsigneeContactDetailInfo, message);
		}

		#endregion

		#region Notify Party

		public void TestCheckAlsoNotify_China()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AWBHeader.EH_ParentID = shipment.PK;
			AWBHeader.EH_Table = JobShipmentSchema.Constants.TableName;
			AWBHeader.Shipment.Transports.AddNew();

			setHeaderToChinaPortFunc = () => { SetIsImportToChinaForTest(true); };
			setHeaderToNonChinaPortFunc = () => { SetIsImportToChinaForTest(false); };

			AssertEH_AlsoNotifyContactName_China("Contact Name is required for China imports");
			AssertEH_AlsoNotifyContactCode_NotPhone_China("Contact type \"Telephone\" is required for China imports", false);
			AssertEH_AlsoNotifyContactDetail_Empty_China("Contact telephone number is required for China imports");

			setHeaderToChinaPortFunc = () => { SetIsTransitingThroughChinaForTest(true); };
			setHeaderToNonChinaPortFunc = () => { SetIsTransitingThroughChinaForTest(false); };
			AssertEH_AlsoNotifyContactName_China("Contact Name is required for China transit");
			AssertEH_AlsoNotifyContactCode_NotPhone_China("Contact type \"Telephone\" is required for China transit", true);
			AssertEH_AlsoNotifyContactDetail_Empty_China("Contact telephone number is required for China transit");
		}

		void AssertEH_AlsoNotifyContactName_China(string message)
		{
			setHeaderToChinaPortFunc();

			AWBHeader.EH_AlsoNotifyContactName = ZString.Empty;
			AssertHasWarning(AWBHeader.EH_AlsoNotifyContactNameInfo, message);

			AWBHeader.EH_AlsoNotifyContactName = "%$)";
			AssertHasWarning(AWBHeader.EH_AlsoNotifyContactNameInfo, message);

			AWBHeader.EH_AlsoNotifyContactName = "Some Guy";
			AssertNoWarning(AWBHeader.EH_AlsoNotifyContactNameInfo, message);

			setHeaderToNonChinaPortFunc();

			AWBHeader.EH_AlsoNotifyContactName = ZString.Empty;
			AssertNoWarning(AWBHeader.EH_AlsoNotifyContactNameInfo, message);
		}

		void AssertEH_AlsoNotifyContactCode_NotPhone_China(string message, bool warnWhenEmpty)
		{
			setHeaderToChinaPortFunc();

			AWBHeader.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertHasWarning(AWBHeader.EH_AlsoNotifyContactCodeInfo, message);

			AWBHeader.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertHasWarning(AWBHeader.EH_AlsoNotifyContactCodeInfo, message);

			AWBHeader.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
			AssertNoWarning(AWBHeader.EH_AlsoNotifyContactCodeInfo, message);

			AWBHeader.EH_AlsoNotifyContactCode = ZString.Empty;

			if (warnWhenEmpty)
			{
				AssertHasWarning(AWBHeader.EH_AlsoNotifyContactCodeInfo, message);
			}
			else
			{
				AssertNoWarning(AWBHeader.EH_AlsoNotifyContactCodeInfo, message);
			}

			setHeaderToNonChinaPortFunc();

			AWBHeader.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.FAX;
			AssertNoWarning(AWBHeader.EH_AlsoNotifyContactCodeInfo, message);

			AWBHeader.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.TELEX;
			AssertNoWarning(AWBHeader.EH_AlsoNotifyContactCodeInfo, message);
		}

		void AssertEH_AlsoNotifyContactDetail_Empty_China(string message)
		{
			setHeaderToChinaPortFunc();

			AWBHeader.EH_AlsoNotifyContactDetail = ZString.Empty;
			AssertHasWarning(AWBHeader.EH_AlsoNotifyContactDetailInfo, message);

			AWBHeader.EH_AlsoNotifyContactDetail = "^%$";
			AssertHasWarning(AWBHeader.EH_AlsoNotifyContactDetailInfo, message);

			AWBHeader.EH_AlsoNotifyContactDetail = "111";
			AssertNoWarning(AWBHeader.EH_AlsoNotifyContactDetailInfo, message);

			setHeaderToNonChinaPortFunc();

			AWBHeader.EH_AlsoNotifyContactDetail = ZString.Empty;
			AssertNoWarning(AWBHeader.EH_AlsoNotifyContactDetailInfo, message);
		}

		#endregion

		#region Implementation

		void AssertRequiredTraderTypeAndNoMessage(Forwarding.AWB.Business.ExportAWBHeader header, ZPropertyInfo traderTypeInfo, ZPropertyInfo traderNoInfo, string expectedMessage)
		{
			traderTypeInfo.Value = (ZString)"";
			traderNoInfo.Value = (ZString)"";

			header.Validation.ValidateAll();

			AssertHasWarning(traderTypeInfo, expectedMessage);
			AssertHasWarning(traderNoInfo, expectedMessage);

			traderTypeInfo.Value = (ZString)"ABC";
			traderNoInfo.Value = (ZString)"111";

			AssertNoWarning(traderTypeInfo, expectedMessage);
			AssertNoWarning(traderNoInfo, expectedMessage);
		}

		Action setHeaderToChinaPortFunc = () => { };
		Action setHeaderToNonChinaPortFunc = () => { };

		void SetIsImportToChinaForTest(bool isImportToChina)
		{
			if (isImportToChina)
			{
				AWBHeader.Shipment.JS_RL_NKOrigin = "AUSYD";
				AWBHeader.Shipment.JS_RL_NKDestination = "CNSHA";
				Assert(AWBHeader.IsImportToChina);
			}
			else
			{
				AWBHeader.Shipment.JS_RL_NKOrigin = "AUSYD";
				AWBHeader.Shipment.JS_RL_NKDestination = "USLAX";
				Assert(!AWBHeader.IsImportToChina);
			}
		}

		void SetIsTransitingThroughChinaForTest(bool isTransitingThroughChina)
		{
			if (isTransitingThroughChina)
			{
				AWBHeader.Shipment.JS_RL_NKOrigin = "AUSYD";
				AWBHeader.Shipment.JS_RL_NKDestination = "USLAX";
				AWBHeader.Shipment.Transports[0].JW_RL_NKLoadPort = "AUSYD";
				AWBHeader.Shipment.Transports[0].JW_RL_NKDiscPort = "CNSHA";
			}
			else
			{
				AWBHeader.Shipment.JS_RL_NKOrigin = "AUSYD";
				AWBHeader.Shipment.JS_RL_NKDestination = "USLAX";
				AWBHeader.Shipment.Transports[0].JW_RL_NKLoadPort = "AUSYD";
				AWBHeader.Shipment.Transports[0].JW_RL_NKDiscPort = "USLAX";
			}
		}

		protected new ShipmentExportAWBHeader AWBHeader
		{
			get { return (ShipmentExportAWBHeader)base.AWBHeader; }
		}

		protected override Forwarding.AWB.Business.ExportAWBHeader GetNewAWBHeader()
		{
			return Factory.New<ShipmentExportAWBHeader>();
		}

		protected override INotificationType ExpectedAWBHeaderNotificationType
		{
			get { return CargoWise.EntityFramework.NotificationType.MessageError; }
		}

		#endregion
	}
}
