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
	class ConsolExportAWBHeaderValidationTest : ForwardingExportAWBHeaderValidationTest
	{
		#region EH_ShipperSignature

		public void TestCheckEH_ShippersSignature()
		{
			AWBHeader.EH_ShippersSignature = "AU12345678901234567890";
			AssertHasWarning("Agents signature length is more than 20", AWBHeader.EH_ShippersSignatureInfo, "Signature in FWB message cannot exceed 20 characters and therefore will be truncated for messaging purposes!");

			AWBHeader.EH_ShippersSignature = "AU1234567890";
			AssertEquals(false, AWBHeader.EH_ShippersSignatureInfo.HasWarnings());
		}

		#endregion

		#region EH_AirlinePrefix

		public void TestCheckEH_AirlinePrefix()
		{
			using (ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				AWBHeader.EH_ParentID = Factory.New<ForwardingConsol>().PK;
				AWBHeader.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				AWBHeader.Consol.JK_MasterBillNum = "0";
				AWBHeader.Populate();
				AWBHeader.Validation.ValidateEH_AirlinePrefix();
				AssertHasMessageError(AWBHeader.EH_AirlinePrefixInfo, "Airline Prefix must be 3 characters in length.");

				AWBHeader.Consol.JK_MasterBillNum = "XXX";
				AWBHeader.Populate();
				AWBHeader.Validation.ValidateEH_AirlinePrefix();
				AssertHasMessageError(AWBHeader.EH_AirlinePrefixInfo, "The Airline code number that is entered is not listed with IATA.");

				AWBHeader.Consol.JK_MasterBillNum = "081";
				AWBHeader.Populate();
				AWBHeader.Validation.ValidateEH_AirlinePrefix();
				AssertNoMessageErrors(AWBHeader.EH_AirlinePrefixInfo);
			}

			using (ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AWBHeader.EH_ParentID = Factory.New<ForwardingConsol>().PK;
				AWBHeader.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				AWBHeader.Consol.JK_MasterBillNum = "0";
				AWBHeader.Populate();
				AWBHeader.Validation.ValidateEH_AirlinePrefix();
				AssertHasWarning(AWBHeader.EH_AirlinePrefixInfo, "Airline Prefix must be 3 characters in length.");

				AWBHeader.Consol.JK_MasterBillNum = "XXX";
				AWBHeader.Populate();
				AWBHeader.Validation.ValidateEH_AirlinePrefix();
				AssertHasWarning(AWBHeader.EH_AirlinePrefixInfo, "The Airline code number that is entered is not listed with IATA.");

				AWBHeader.Consol.JK_MasterBillNum = "081";
				AWBHeader.Populate();
				AWBHeader.Validation.ValidateEH_AirlinePrefix();
				AssertNoWarnings(AWBHeader.EH_AirlinePrefixInfo);
			}
		}

		public void TestCheckEH_AirlinePrefix_WithDuplicateAirline()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "085";
			airline.RM_TwoCharacterCode = "ZZ";
			airline.RM_AirlineName1 = "AIR ZZ";

			RefAirline duplicateAirline = Factory.New<RefAirline>();
			duplicateAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "085";
			duplicateAirline.RM_TwoCharacterCode = "ZZ";
			duplicateAirline.RM_AirlineName1 = "AIR ZZ";

			AWBHeader.EH_ParentID = Factory.New<ForwardingConsol>().PK;
			AWBHeader.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AWBHeader.Consol.JK_MasterBillNum = "0";
			AWBHeader.Populate();
			AWBHeader.Validation.ValidateEH_AirlinePrefix();
			AssertHasMessageErrors(AWBHeader.EH_AirlinePrefixInfo);

			AWBHeader.Consol.JK_MasterBillNum = "XXX";
			AWBHeader.Populate();
			AWBHeader.Validation.ValidateEH_AirlinePrefix();
			AssertHasMessageErrors(AWBHeader.EH_AirlinePrefixInfo);

			AWBHeader.Consol.JK_MasterBillNum = "081";
			AWBHeader.Populate();
			AWBHeader.Validation.ValidateEH_AirlinePrefix();
			AssertNoMessageErrors(AWBHeader.EH_AirlinePrefixInfo);

			AWBHeader.Consol.JK_MasterBillNum = "085";
			AWBHeader.Populate();
			AWBHeader.Validation.ValidateEH_AirlinePrefix();
			AssertHasWarnings(AWBHeader.EH_AirlinePrefixInfo);
			AssertHasWarningContaining(AWBHeader.EH_AirlinePrefixInfo, "Please delete the duplicated Airline(s); Maintain > Reference Files > Airlines.");
		}

		#endregion

		#region EH_AWBSerialNo

		public void TestCheckEH_AWBSerialNo()
		{
			AWBHeader.EH_ParentID = Factory.New<ForwardingConsol>().PK;
			AWBHeader.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AWBHeader.Consol.JK_MasterBillNum = "08112";
			AWBHeader.Populate();
			AWBHeader.Validation.ValidateEH_AWBSerialNo();
			AssertHasWarnings(AWBHeader.EH_AWBSerialNoInfo);

			AWBHeader.Consol.JK_MasterBillNum = "08112345678";
			AWBHeader.Populate();
			AWBHeader.Validation.ValidateEH_AWBSerialNo();
			AssertNoNotifications(AWBHeader.EH_AWBSerialNoInfo);
		}

		#endregion

		#region EH_OtherPrepaidCollect

		public void TestValidateEH_OtherPrepaidCollect()
		{
			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = "";
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = "X";
			AssertHasErrors(AWBHeader.EH_OtherPrepaidCollectInfo);

			var parent = Factory.NewWithValidTestData<ForwardingConsol>();
			parent.JK_TransportMode = Core.Constants.TransportModes.Air;
			AWBHeader.EH_ParentID = parent.PK;
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;

			Enterprise.Registry.Business.FreightDataRegistry.Instance.ShowCollectOtherWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			Enterprise.Registry.Business.FreightDataRegistry.Instance.ShowCollectOtherWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertNoNotifications(AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertHasWarnings(AWBHeader.EH_OtherPrepaidCollectInfo);
		}

		#endregion

		#region AsAgreed

		const string generalValidationError = "As Agreed cannot be selected for Master Bill";

		public void TestAsAgreed1st()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			shipment.Consols.AddNew();
			shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignor.PK;
			consignor.OH_Code = "ZZ1";

			AWBHeader.EH_ParentID = shipment.Consols[0].PK;

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Env.Registry.Freight.AirWaybill.AllowAsAgreed = true;
			AssertEquals("Pre-condition", Core.Constants.AWB.AsAgreedTypes.Codes.None, AWBHeader.EH_AsAgreed1st);

			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AWBHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AssertNoError(AWBHeader.EH_AsAgreed1stInfo, generalValidationError);
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertNoError(AWBHeader.EH_AsAgreed1stInfo, generalValidationError);
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertNoError(AWBHeader.EH_AsAgreed1stInfo, generalValidationError);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Env.Registry.Freight.AirWaybill.AllowAsAgreed = false;
			AWBHeader.Validation.ValidateAll();
			AssertHasError(AWBHeader.EH_AsAgreed1stInfo, generalValidationError);
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Validation.ValidateAll();
			AssertHasError(AWBHeader.EH_AsAgreed1stInfo, generalValidationError);
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBHeader.Validation.ValidateAll();
			AssertNoError(AWBHeader.EH_AsAgreed1stInfo, generalValidationError);
		}

		public void TestAsAgreed2nd()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			shipment.Consols.AddNew();
			shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			OrgHeader consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignor.PK;
			consignor.OH_Code = "ZZ1";

			AWBHeader.EH_ParentID = shipment.Consols[0].PK;

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Env.Registry.Freight.AirWaybill.AllowAsAgreed = true;
			AssertEquals("Pre-condition", Core.Constants.AWB.AsAgreedTypes.Codes.None, AWBHeader.EH_AsAgreed2nd);

			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AWBHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AWBHeader.Validation.ValidateAll();
			AssertNoError(AWBHeader.EH_AsAgreed2ndInfo, generalValidationError);
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Validation.ValidateAll();
			AssertNoError(AWBHeader.EH_AsAgreed2ndInfo, generalValidationError);
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBHeader.Validation.ValidateAll();
			AssertNoError(AWBHeader.EH_AsAgreed2ndInfo, generalValidationError);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Env.Registry.Freight.AirWaybill.AllowAsAgreed = false;
			AWBHeader.Validation.ValidateAll();
			AssertHasError(AWBHeader.EH_AsAgreed2ndInfo, generalValidationError);
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Validation.ValidateAll();
			AssertHasError(AWBHeader.EH_AsAgreed2ndInfo, generalValidationError);
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBHeader.Validation.ValidateAll();
			AssertNoError(AWBHeader.EH_AsAgreed2ndInfo, generalValidationError);
		}

		#endregion

		#region RequiredTraderTypeAndNo Validations

		class OriginDestExportAWBHeaderForTest : ConsolExportAWBHeader
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
			CreateRefDocOrgCusCode("VAT", "TR", "TR", 1, "VAT", "VAT", "AWB");

			var expectedMessage = "VAT is required for Turkey exports.";
			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			forwardingConsol.JK_RL_NKLoadPort = "TRIST";
			forwardingConsol.JK_RL_NKDischargePort = "AUSYD";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = forwardingConsol.PK;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();

			AssertRequiredTraderTypeAndNoMessage(awbHeader, awbHeader.EH_ShipperTraderNoTypeInfo, awbHeader.EH_ShipperTraderNoInfo, expectedMessage);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes()
		{
			CreateRefDocOrgCusCode("VAT", "TR", "TR", 1, "VAT", "VAT", "AWB");

			var expectedMessage = "VAT is required for Turkey imports.";
			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
			forwardingConsol.JK_RL_NKDischargePort = "TRIST";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = forwardingConsol.PK;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();

			AssertRequiredTraderTypeAndNoMessage(awbHeader, awbHeader.EH_ConsigneeTraderNoTypeInfo, awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes_Canada()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				CreateRefDocOrgCusCode("CCC", "CA", "CA", 1, "CCC", "CARRIER CODE", "AWB");

				const string expectedMessage = "For imports to Canada, the Sending Forwarder’s code provided by the CBSA (Canada Border Services Agency) is required for eManifest reporting. Use the Organization Registration Number type ‘CCC’ (Carrier Code) to save this code.";
				var forwardingConsol = Factory.New<ForwardingConsol>();
				forwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
				forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
				forwardingConsol.JK_RL_NKDischargePort = "CA2KS";

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = forwardingConsol.PK;
				awbHeader.Populate();

				AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
				AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);
				AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

				awbHeader.EH_ConsigneeTraderNo = "8000";
				AssertNoWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);
			}
		}

		public void TestShipperTraderTypeAndNoMessage_RequiredTraderTypes_Canada()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				CreateRefDocOrgCusCode("CCC", "CA", "CA", 1, "CCC", "CARRIER CODE", "AWB");

				var forwardingConsol = Factory.New<ForwardingConsol>();
				forwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
				forwardingConsol.JK_RL_NKLoadPort = "CA2KS";
				forwardingConsol.JK_RL_NKDischargePort = "AUSYD";

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = forwardingConsol.PK;
				awbHeader.Populate();

				AssertEquals(ZString.Empty, awbHeader.EH_ShipperTraderNoType);
				AssertEquals(ZString.Empty, awbHeader.EH_ShipperTraderNo);
				AssertNoWarnings(awbHeader.EH_ShipperTraderNoInfo);
			}
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes_OneDischargeLegInEU_AgentType()
		{
			var expectedMessage = "The Consignee’s EORI/CH UID/NO MVA number is required by some airlines for imports into EU, CH, LI, NO, XI unless the Consignee is a natural person/individual.";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";

			var receivingAgent = Factory.New<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;
			awbHeader.Populate();

			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			receivingAgent.OH_Category = "NAT";

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_ConsigneeTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_ConsigneeTraderNoInfo);

			receivingAgent.OH_Category = "BUS";
			var code1 = receivingAgent.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			consol.JK_OverrideWaybillDefaults = true;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			consol.JK_OverrideWaybillDefaults = false;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_ConsigneeTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_ConsigneeTraderNoInfo);

			code1.OK_CustomsRegNo = string.Empty;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

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
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Israel))
			{
				CreateRefDocOrgCusCode("VAT", "IL", "IL", 1, "VAT", "VAT NUMBER", "AWB");

				var expectedMessage = @"VAT Number is required for imports to Israel to comply with Manifest reporting.";
				var forwardingConsol = Factory.New<ForwardingConsol>();
				forwardingConsol.JK_AgentType = Constants.AgentType.Agent;
				forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
				forwardingConsol.JK_RL_NKDischargePort = "IL2LL";

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = forwardingConsol.PK;
				awbHeader.Populate();

				AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

				awbHeader.EH_ConsigneeTraderNo = "12345";
				AssertNoWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);
			}
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredCompID_Israel()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Israel))
			{
				CreateRefDocOrgCusCode("VAT", "IL", "IL", 1, "VAT", "VAT NUMBER", "AWB");

				var expectedMessage = @"VAT Number is required for imports to Israel to comply with Manifest reporting.";
				var forwardingConsol = Factory.New<ForwardingConsol>();
				forwardingConsol.JK_AgentType = Constants.AgentType.Agent;
				forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
				forwardingConsol.JK_RL_NKDischargePort = "IL2LL";
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Notify Party";
				forwardingConsol.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = forwardingConsol.PK;
				awbHeader.Populate();

				AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

				awbHeader.EH_AlsoNotifyTraderNo = "678910";
				AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);
			}
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredTraderTypes_OneDischargeLegInEU_AgentType()
		{
			var expectedMessage = "The Notify Party’s EORI/CH UID/NO MVA number is required by some airlines for imports into EU, CH, LI, NO, XI unless the Notify Party is a natural person/individual.";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoInfo);

			var notifyParty = consol.DocAddresses.FindOrCreateWithDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty);
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
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			consol.JK_OverrideWaybillDefaults = true;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			consol.JK_OverrideWaybillDefaults = false;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoInfo);

			code1.OK_CustomsRegNo = string.Empty;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

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
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			awbHeader.EH_ParentID = consol.PK;

			consol.NotifyPartyDocumentaryAddress.OrganisationPK = GenerateOrganisationIsIcs2Country().PK;
			Assert(consol.NotifyPartyDocumentaryAddress.Organisation.Country.IsIcs2Member);
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			var expectedMessage = "The Notify Party’s EORI/CH UID/NO MVA number is required by some airlines for imports into EU, CH, LI, NO, XI unless the Notify Party is a natural person/individual.";
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoTypeInfo, expectedMessage);
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			consol.NotifyPartyDocumentaryAddress.OrganisationPK = GenerateOrganisationIsNotIcs2Country().PK;
			Assert(!consol.NotifyPartyDocumentaryAddress.Organisation.Country.IsIcs2Member);
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoTypeInfo);
			AssertNoWarnings(awbHeader.EH_AlsoNotifyTraderNoInfo);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes_ImportChina()
		{
			var expectedMessage = "The Consignee’s USCI (Unified Social Credit Identifier) is required for imports to China.";
			CreateRefDocOrgCusCode("USC", "CN", "CN", 1, "USCI", "USCI", "AWB");
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;
			awbHeader.Populate();

			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			consol.JK_OverrideWaybillDefaults = true;
			awbHeader.EH_ConsigneeTraderNo = "12345";
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);

			consol.JK_OverrideWaybillDefaults = false;
			awbHeader.EH_ConsigneeTraderNo = "12345";
			AssertNoWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedMessage);
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredTraderTypes_ImportChina()
		{
			var expectedMessage = "The Notify Party’s USCI (Unified Social Credit Identifier) is required for imports to China.";

			CreateRefDocOrgCusCode("USC", "CN", "CN", 1, "USCI", "USCI", "AWB");
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;

			awbHeader.Populate();
			AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			var notifyParty = consol.DocAddresses.FindOrCreateWithDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = "CN";
			notifyParty.OrganisationPK = orgHeader.PK;

			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			consol.JK_OverrideWaybillDefaults = true;
			awbHeader.EH_AlsoNotifyTraderNo = "12345";
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();
			AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			consol.JK_OverrideWaybillDefaults = false;
			awbHeader.EH_AlsoNotifyTraderNo = "12345";
			AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);
		}

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderTypes_Morocco()
		{
			var expectedMessage = "The Consignee's ICE number is required for inbound shipments to Morocco.";
			CreateRefDocOrgCusCode("ICE", "MA", "MA", 1, "ICE", "ICE", "AWB");
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "MACAS";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "DEHAM";
			shipment1.JS_RL_NKDestination = "MACAS";

			var consignor = Factory.New<OrgHeader>();
			shipment1.ConsigneePK = consignor.PK;

			var consignee = consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Morocco;

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;
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
			CreateRefDocOrgCusCode("ICE", "MA", "MA", 1, "ICE", "ICE", "AWB");
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "MACAS";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;

			awbHeader.Populate();
			AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);

			var notifyParty = consol.DocAddresses.FindOrCreateWithDocAddressType(MasterFiles.Integration.DocAddressType.NotifyParty);
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

		public void TestConsigneeTraderTypeAndNoMessage_RequiredTraderNo_ImportMauritius()
		{
			var validationErrorImportMauritius = "The Consignee's BRN(Business Registration Number) is required for imports to Mauritius to comply with Manifest reporting.";
			CreateRefDocOrgCusCode("BRN", "MU", "MU", 1, "BRN", "Business Registratian Number", "AWB");
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "MUPLU";
		
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;
			awbHeader.Populate();

			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, validationErrorImportMauritius);

			awbHeader.EH_ConsigneeTraderNo = "12345";
			AssertNoWarning(awbHeader.EH_ConsigneeTraderNoInfo, validationErrorImportMauritius);
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredTraderNo_ImportMauritius()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mauritius))
			{
				CreateRefDocOrgCusCode("BRN", "MU", "MU", 1, "BRN", "Business Registratian Number", "AWB");

				var validationErrorImportMauritius = "The Notify Party's BRN(Business Registration Number) is required for imports to Mauritius to comply with Manifest reporting.";
				var forwardingConsol = Factory.New<ForwardingConsol>();
				forwardingConsol.JK_AgentType = Constants.AgentType.Agent;
				forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
				forwardingConsol.JK_RL_NKDischargePort = "MUPLU";
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Notify Party";
				forwardingConsol.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = forwardingConsol.PK;
				awbHeader.Populate();

				AssertHasWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, validationErrorImportMauritius);

				awbHeader.EH_AlsoNotifyTraderNo = "678910";
				AssertNoWarning(awbHeader.EH_AlsoNotifyTraderNoInfo, validationErrorImportMauritius);
			}
		}

		public void TestShipperTypeAndNoMessage_RequireShipperNo_ExportMauritius()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mauritius))
			{
				CreateRefDocOrgCusCode("BRN", "MU", "MU", 1, "BRN", "Business Registratian Number", "AWB");

				var validationErrorExportMauritius = "The Shipper's BRN(Business Registration Number) is required for exports from Mauritius to comply with Manifest reporting.";
				var forwardingConsol = Factory.New<ForwardingConsol>();
				forwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
				forwardingConsol.JK_RL_NKLoadPort = "MUPLU";
				forwardingConsol.JK_RL_NKDischargePort = "AUSYD";

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = forwardingConsol.PK;
				awbHeader.Populate();

				AssertHasWarning(awbHeader.EH_ShipperTraderNoInfo, validationErrorExportMauritius);

				awbHeader.EH_ShipperTraderNo = "123456";
				AssertNoWarning(awbHeader.EH_ShipperTraderNoInfo, validationErrorExportMauritius);
			}
		}

		public void TestAlsoNotifyTraderTypeAndNoMessage_RequiredTraderTypes()
		{
			CreateRefDocOrgCusCode("VAT", "TR", "TR", 1, "VAT", "VAT", "AWB");

			var expectedMessage = "VAT is required for Turkey imports.";
			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			forwardingConsol.JK_RL_NKLoadPort = "AUSYD";
			forwardingConsol.JK_RL_NKDischargePort = "TRIST";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Notify Party";
			forwardingConsol.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = forwardingConsol.PK;
			awbHeader.Populate();
			awbHeader.Validation.ValidateAll();

			AssertRequiredTraderTypeAndNoMessage(awbHeader, awbHeader.EH_AlsoNotifyTraderNoTypeInfo, awbHeader.EH_AlsoNotifyTraderNoInfo, expectedMessage);
		}

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

		#endregion

		#region AsAgreedAllowed Brasil

		const string validationErrorBrazil = "As Agreed cannot be selected on Master Bill for Imports to Brazil";

		public void TestAsAgreed1st_Brasil()
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUBNE";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;

			Env.Registry.Freight.AirWaybill.AllowAsAgreed = true;
			AssertEquals("Pre-condition", Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed1st);
			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);

			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeader.EH_AsAgreed1st);
			awbHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeader.EH_AsAgreed1st);
			awbHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeader.EH_AsAgreed1st);

			consol.JK_RL_NKDischargePort = "BRBRA";
			awbHeader.Validation.ValidateAll();
			AssertHasError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
		}

		public void TestAsAgreed2nd_Brasil()
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUBNE";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;

			Env.Registry.Freight.AirWaybill.AllowAsAgreed = true;
			AssertEquals("Pre-condition", Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeader.EH_AsAgreed2nd);
			awbHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeader.EH_AsAgreed2nd);
			awbHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeader.EH_AsAgreed2nd);

			consol.JK_RL_NKDischargePort = "BRBRA";
			awbHeader.Validation.ValidateAll();
			AssertHasError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);
		}

		public void TestAsAgreedValidatedOnOriginChange_Brasil()
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUBNE";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "BRBRA";
			consol.JK_RL_NKDischargePort = "BRBSB";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			consol.JK_RL_NKLoadPort = "BRVCP";
			awbHeader.Validation.ValidateAll();
			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			consol.JK_RL_NKLoadPort = "AUSYD";
			awbHeader.Validation.ValidateAll();
			AssertHasError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertHasError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);
		}

		public void TestAsAgreedValidatedOnDestinationChange_Brasil()
		{
			GlbBranch currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUBNE";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = consol.PK;

			awbHeader.EH_AsAgreed1st = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			awbHeader.EH_AsAgreed2nd = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			consol.JK_RL_NKDischargePort = "BRBSB";
			awbHeader.Validation.ValidateAll();
			AssertHasError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertHasError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			consol.JK_RL_NKLoadPort = "BRBRA";
			awbHeader.Validation.ValidateAll();
			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);

			consol.JK_RL_NKDischargePort = "BRRBA";
			awbHeader.Validation.ValidateAll();
			AssertNoError(awbHeader.EH_AsAgreed1stInfo, validationErrorBrazil);
			AssertNoError(awbHeader.EH_AsAgreed2ndInfo, validationErrorBrazil);
		}

		#endregion

		#region EH_AWBIssueDate

		public void TestCheckEH_AWBIssueDate()
		{
			AWBHeader.EH_AWBIssueDate = ZDateTime.Empty;
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AWBIssueDateInfo);

			AWBHeader.EH_AWBIssueDate = ZDateTime.Now;
			AssertNoNotifications(AWBHeader.EH_AWBIssueDateInfo);
		}

		public void TestCheckEH_AWBIssueDateWarningIfFutureDate()
		{
			AWBHeader.EH_AWBIssueDate = ZDateTime.Empty;
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AWBIssueDateInfo);

			AWBHeader.EH_AWBIssueDate = ZDateTime.Now;
			AssertNoNotifications(AWBHeader.EH_AWBIssueDateInfo);

			var warningText = "Be aware that some airlines reject eAWB that are future dated, which may have operational impacts.";

			AWBHeader.EH_AWBIssueDate = ZDateTime.Now.AddDays(1);
			AssertHasWarning(AWBHeader.EH_AWBIssueDateInfo, warningText);

			AWBHeader.EH_AWBIssueDate = ZDateTime.Now;
			AssertNoWarning(AWBHeader.EH_AWBIssueDateInfo, warningText);
		}

		#endregion

		#region EH_CustomsValue

		public void TestCheckEH_CustomsValue()
		{
			var goodsValueMessageError = "Goods Value (per the commercial invoice) is required for imports to Bangladesh.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKDischargePort = "BDKHL";
			AWBHeader.EH_ParentID = consol.PK;
			AWBHeader.Consol.JK_RL_NKDischargePort = "BDKHL";
			AWBHeader.EH_CustomsValue = 0;

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertNoMessageError("No error expected because registry is set to 'No'.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);
			}

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertHasMessageError("Error expected when registry is set to 'Yes', Direct consol and BD import has no goods value specified.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);

				AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertNoMessageError("No error expected because consol is non-direct.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);

				consol.JK_AgentType = Constants.AgentType.Direct;
				AWBHeader.EH_CustomsValue = 10;
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertNoMessageError("No error expected because BD import has goods value specified.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);

				consol.JK_RL_NKDischargePort = "VNVNH";
				AWBHeader.EH_CustomsValue = 0;
				AWBHeader.Validation.ValidateEH_CustomsValue();
				AssertNoMessageError("No error expected when non-BD import has no goods value specified.", AWBHeader.EH_CustomsValueInfo, goodsValueMessageError);
			}
		}

		#endregion

		#region EH_By & EH_To

		public void TestCheckEH_By1stLength()
		{
			AWBHeader.EH_By1st = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_By1stInfo);

			AWBHeader.EH_By1st = "Q";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_By1stInfo);

			AWBHeader.EH_By1st = "Q-";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_By1stInfo);

			AWBHeader.EH_By1st = "QF";
			AssertNoNotifications(AWBHeader.EH_By1stInfo);
		}

		public void TestCheckEH_To1st()
		{
			AWBHeader.EH_To1st = "";
			AssertNoNotifications(AWBHeader.EH_To1stInfo);

			AWBHeader.EH_To1st = "LH";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_To1stInfo);

			AWBHeader.EH_To1st = "LH1";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_To1stInfo);

			AWBHeader.EH_To1st = "LHR";
			AssertNoNotifications(AWBHeader.EH_To1stInfo);
		}

		public void TestCheckEH_By2nd()
		{
			AWBHeader.EH_By2nd = "";
			AssertNoNotifications(AWBHeader.EH_By2ndInfo);

			AWBHeader.EH_By2nd = "B";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_By2ndInfo);

			AWBHeader.EH_By2nd = "B-";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_By2ndInfo);

			AWBHeader.EH_By2nd = "BA";
			AssertNoNotifications(AWBHeader.EH_By2ndInfo);
		}

		public void TestCheckEH_To2nd()
		{
			AWBHeader.EH_To2nd = "";
			AssertNoNotifications(AWBHeader.EH_To2ndInfo);

			AWBHeader.EH_To2nd = "";
			AWBHeader.EH_By2nd = "SQ";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_To2ndInfo);

			AWBHeader.EH_To2nd = "SI";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_To2ndInfo);

			AWBHeader.EH_To2nd = "SI1";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_To2ndInfo);

			AWBHeader.EH_To2nd = "SIN";
			AssertNoNotifications(AWBHeader.EH_To2ndInfo);
		}

		public void TestCheckEH_By3rd()
		{
			AWBHeader.EH_By3rd = "";
			AssertNoNotifications(AWBHeader.EH_By3rdInfo);

			AWBHeader.EH_By3rd = "A";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_By3rdInfo);

			AWBHeader.EH_By3rd = "A-";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_By3rdInfo);

			AWBHeader.EH_By3rd = "AA";
			AssertNoNotifications(AWBHeader.EH_By3rdInfo);
		}

		public void TestCheckEH_To3rd()
		{
			AWBHeader.EH_To3rd = "";
			AssertNoNotifications(AWBHeader.EH_To3rdInfo);

			AWBHeader.EH_To3rd = "";
			AWBHeader.EH_By3rd = "SQ";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_To3rdInfo);

			AWBHeader.EH_To3rd = "LA";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_To3rdInfo);

			AWBHeader.EH_To3rd = "LA1";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_To3rdInfo);

			AWBHeader.EH_To3rd = "LAX";
			AssertNoNotifications(AWBHeader.EH_To3rdInfo);
		}

		#endregion

		#region EH_Currency
		public void TestCheckEH_Currency()
		{
			AWBHeader.EH_Currency = "";
			AssertHasMessageError(AWBHeader.EH_CurrencyInfo, "Currency is Required for sending an AWB electronically.");

			AWBHeader.EH_Currency = "AU";
			AssertHasMessageError(AWBHeader.EH_CurrencyInfo, "Currency must be 3 characters in length.");

			AWBHeader.EH_Currency = "AUD";
			AssertNoNotifications(AWBHeader.EH_CurrencyInfo);
		}

		#endregion

		#region EH_ChargesCode

		public void TestCheckEH_ChargesCode()
		{
			AWBHeader.EH_ChargesCode = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ChargesCodeInfo);

			AWBHeader.EH_ChargesCode = "P";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ChargesCodeInfo);

			AWBHeader.EH_ChargesCode = "PP";
			AssertNoNotifications(AWBHeader.EH_ChargesCodeInfo);

			AWBHeader.EH_ChargesCode = "XX";
			AssertHasErrors(AWBHeader.EH_ChargesCodeInfo);
		}

		#endregion

		#region EH_AWBIssuePlace

		public void TestCheckEH_AWBIssuePlace()
		{
			AWBHeader.EH_AWBIssuePlace = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AWBIssuePlaceInfo);

			AWBHeader.EH_AWBIssuePlace = "Place";
			AssertNoNotifications(AWBHeader.EH_AWBIssuePlaceInfo);

			AWBHeader.EH_AWBIssuePlace = "Place##";
			AssertNoNotifications(AWBHeader.EH_AWBIssuePlaceInfo);

			AWBHeader.EH_AWBIssuePlace = "$%###";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AWBIssuePlaceInfo);
		}

		#endregion

		#region EH_ECNCRNNumber Validation

		public void TestCheckEH_ECNCRNNumberForMultipleCustomsEntryNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AWBHeader.EH_ParentID = consol.PK;

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

			CusEntryNumber cusEntryNum4 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum4.CE_EntryNum = "444";
			cusEntryNum4.CE_EntryType = "CAN";
			cusEntryNum4.CE_ParentTable = ForwardingShipment.Schema.TableName;

			CusEntryNumber cusEntryNum5 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum5.CE_EntryNum = "555";
			cusEntryNum5.CE_EntryType = "CAN";
			cusEntryNum5.CE_ParentTable = ForwardingShipment.Schema.TableName;

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
			AssertNoWarnings("Should not display error message if Consol is not direct", AWBHeader.EH_ECNCRNNumberInfo);

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertHasWarning("EH_ECNCRNNumber should have all warnings of shipment customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage1);
			AssertHasWarning("EH_ECNCRNNumber should have all warnings of shipment customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage2);
			AssertHasWarning("EH_ECNCRNNumber should have all warnings of shipment customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage3);
			AssertHasWarning("EH_ECNCRNNumber should have warning when shipment have more than 4 customs entry numbers", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage4);
		}

		public void TestCheckEH_ECNCRNNumberForConsolFromSwitzerland()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "CHABL";
			consol.JK_RL_NKDischargePort = "USCHI";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "CHABL";
			shipment1.JS_RL_NKDestination = "USCHI";
			AWBHeader.EH_ParentID = consol.PK;

			var cusEntryNum11 = shipment1.CusEntryNumbers.AddNew();
			cusEntryNum11.CE_EntryNum = ZString.Empty;
			cusEntryNum11.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;
			cusEntryNum11.CE_ParentTable = shipment1.TableName;
			var cusEntryNum12 = shipment1.CusEntryNumbers.AddNew();
			cusEntryNum12.CE_EntryNum = ZString.Empty;
			cusEntryNum12.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;
			cusEntryNum12.CE_ParentTable = shipment1.TableName;

			var packline11 = shipment1.OuterPackLines.AddNew();
			packline11.JL_HarmonisedCode = "123456";
			var packline12 = shipment1.OuterPackLines.AddNew();

			Factory.Save();

			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			var shipmentNumber = shipment1.JS_UniqueConsignRef;
			var expectedMessage = $"For exports from Switzerland by airfreight, to meet RFS (Road Feeder Service) customs filing requirements, either the GDRN (Goods Declaration Reference Number) or HS code(s) are required. The GDRN or HS Code is missing from at least one Packline of {shipment1.JS_UniqueConsignRef}.";
			AssertHasMessageError("packline12 does not have HS Code", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage);

			packline12.JL_HarmonisedCode = "654321";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors(AWBHeader.EH_ECNCRNNumberInfo);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "CHABL";
			shipment2.JS_RL_NKDestination = "USCHI";

			var cusEntryNum21 = shipment2.CusEntryNumbers.AddNew();
			cusEntryNum21.CE_EntryNum = ZString.Empty;
			cusEntryNum21.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;
			cusEntryNum21.CE_ParentTable = shipment2.TableName;
			var cusEntryNum22 = shipment2.CusEntryNumbers.AddNew();
			cusEntryNum22.CE_EntryNum = ZString.Empty;
			cusEntryNum22.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;
			cusEntryNum22.CE_ParentTable = shipment2.TableName;

			var packline21 = shipment2.OuterPackLines.AddNew();
			packline21.JL_HarmonisedCode = "123456";
			var packline22 = shipment2.OuterPackLines.AddNew();

			Factory.Save();

			expectedMessage = $"For exports from Switzerland by airfreight, to meet RFS (Road Feeder Service) customs filing requirements, either the GDRN (Goods Declaration Reference Number) or HS code(s) are required. The GDRN or HS Code is missing from at least one Packline of {shipment2.JS_UniqueConsignRef}.";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertHasMessageError("packline22 does not have HS Code", AWBHeader.EH_ECNCRNNumberInfo, expectedMessage);

			packline22.JL_HarmonisedCode = "654321";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors(AWBHeader.EH_ECNCRNNumberInfo);

			packline21.JL_HarmonisedCode = ZString.Empty;
			packline22.JL_HarmonisedCode = ZString.Empty;
			cusEntryNum21.CE_EntryNum = "GDRN111";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors("cusEntryNum11 is a available GDRN", AWBHeader.EH_ECNCRNNumberInfo);

			cusEntryNum21.CE_EntryType = CusEntryNumberTypes.Switzerland.AccessCode;
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertHasMessageError(AWBHeader.EH_ECNCRNNumberInfo, expectedMessage);

			packline21.JL_HarmonisedCode = "654321";
			packline22.JL_HarmonisedCode = "123456";
			AWBHeader.Validation.ValidateEH_ECNCRNNumber();
			AssertNoMessageErrors(AWBHeader.EH_ECNCRNNumberInfo);
		}

		#endregion

		#region Prepaid Collect

		public void TestCheckEH_WeightPrepaidCollect()
		{
			AWBHeader.EH_WeightPrepaidCollect = "";
			AWBHeader.EH_OtherPrepaidCollect = "";
			AssertNoNotifications(AWBHeader.EH_WeightPrepaidCollectInfo);

			AWBHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_WeightPrepaidCollectInfo);

			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertNoNotifications(AWBHeader.EH_WeightPrepaidCollectInfo);
		}

		public void TestCheckEH_OtherPrepaidCollect()
		{
			AWBHeader.EH_WeightPrepaidCollect = Core.Constants.AWB.PPDCollect.Prepaid;
			AWBHeader.EH_OtherPrepaidCollect = ZString.Empty;
			AssertHasMessageErrors("Weight Prepaid/Collect is provided - notification is expected", AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.AWBOtherCharges.AddNew();
			AWBHeader.EH_OtherPrepaidCollect = Core.Constants.AWB.PPDCollect.Prepaid;
			AssertNoMessageErrors("A valid value - no notification expected", AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.EH_WeightPrepaidCollect = ZString.Empty;
			AWBHeader.EH_OtherPrepaidCollect = ZString.Empty;
			AssertHasMessageErrors("An empty value and AWBOtherCharges exist - notification is expected", AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.AWBOtherCharges.RemoveAll();
			AssertNoMessageErrors("An empty value but no AWBOtherCharges - no notification is expected", AWBHeader.EH_OtherPrepaidCollectInfo);

			AWBHeader.AWBOtherCharges.AddNew();
			AssertHasMessageErrors("Validation is called when AWBOtherCharges count changed; event subscription is implemented in ExportAWBHeader.AWBOtherCharges", AWBHeader.EH_OtherPrepaidCollectInfo);
		}

		#endregion

		#region Shipper

		public override void TestCheckEH_ShipperCountryCode()
		{
			AWBHeader.EH_ShipperCountryCode = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperCountryCodeInfo);

			AWBHeader.EH_ShipperCountryCode = "A";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperCountryCodeInfo);

			AWBHeader.EH_ShipperCountryCode = "A2";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperCountryCodeInfo);

			AWBHeader.EH_ShipperCountryCode = "SG";
			AssertNoNotifications(AWBHeader.EH_ShipperCountryCodeInfo);
		}

		public void TestCheckEH_ShipperName()
		{
			AWBHeader.EH_ShipperName = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperNameInfo);

			AWBHeader.EH_ShipperName = "##";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperNameInfo);

			AWBHeader.EH_ShipperName = "Something";
			AssertNoNotifications(AWBHeader.EH_ShipperNameInfo);

			AWBHeader.EH_ShipperName = "Something##";
			AssertNoNotifications(AWBHeader.EH_ShipperNameInfo);
		}

		public void TestCheckEH_ShipperAddress()
		{
			AWBHeader.EH_ShipperAddress = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperAddressInfo);

			AWBHeader.EH_ShipperAddress = "##";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperAddressInfo);

			AWBHeader.EH_ShipperAddress = "Something";
			AssertNoNotifications(AWBHeader.EH_ShipperAddressInfo);

			AWBHeader.EH_ShipperAddress = "Something##";
			AssertNoNotifications(AWBHeader.EH_ShipperAddressInfo);
		}

		public void TestCheckEH_ShipperPlace()
		{
			AWBHeader.EH_ShipperPlace = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperPlaceInfo);

			AWBHeader.EH_ShipperPlace = "$$";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ShipperPlaceInfo);

			AWBHeader.EH_ShipperPlace = "Something";
			AssertNoNotifications(AWBHeader.EH_ShipperPlaceInfo);

			AWBHeader.EH_ShipperPlace = "Something##";
			AssertNoNotifications(AWBHeader.EH_ShipperPlaceInfo);
		}

		#endregion

		#region Consignee

		public override void TestCheckEH_ConsigneeCountryCode()
		{
			AWBHeader.EH_ConsigneeCountryCode = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeCountryCodeInfo);

			AWBHeader.EH_ConsigneeCountryCode = "A";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeCountryCodeInfo);

			AWBHeader.EH_ConsigneeCountryCode = "A2";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeCountryCodeInfo);

			AWBHeader.EH_ConsigneeCountryCode = "SG";
			AssertNoNotifications(AWBHeader.EH_ConsigneeCountryCodeInfo);
		}

		public void TestCheckEH_ConsigneeContactDetail_ToOrderConsignee()
		{
			var warningText = "Consignee communication number is empty.";

			AWBHeader.EH_ConsigneeName = "NOT TO ORDER";
			AWBHeader.EH_ConsigneeContactDetail = ZString.Empty;

			AssertHasWarning(AWBHeader.EH_ConsigneeContactDetailInfo, warningText);

			AWBHeader.EH_ConsigneeContactDetail = "123456";
			AssertNoWarning(AWBHeader.EH_ConsigneeContactDetailInfo, warningText);

			AWBHeader.EH_ConsigneeName = "TO ORDER";
			AWBHeader.EH_ConsigneeContactDetail = ZString.Empty;
			AssertNoWarning(AWBHeader.EH_ConsigneeContactDetailInfo, warningText);
		}

		public void TestCheckEH_ConsigneeName()
		{
			AWBHeader.EH_ConsigneeName = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeNameInfo);

			AWBHeader.EH_ConsigneeName = "##";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeNameInfo);

			AWBHeader.EH_ConsigneeName = "Something";
			AssertNoNotifications(AWBHeader.EH_ConsigneeNameInfo);

			AWBHeader.EH_ConsigneeName = "Something##";
			AssertNoNotifications(AWBHeader.EH_ConsigneeNameInfo);
		}

		public void TestCheckEH_ConsigneeAddress()
		{
			AWBHeader.EH_ConsigneeAddress = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeAddressInfo);

			AWBHeader.EH_ConsigneeAddress = "##";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneeAddressInfo);

			AWBHeader.EH_ConsigneeAddress = "Something";
			AssertNoNotifications(AWBHeader.EH_ConsigneeAddressInfo);

			AWBHeader.EH_ConsigneeAddress = "Something##";
			AssertNoNotifications(AWBHeader.EH_ConsigneeAddressInfo);
		}

		public void TestCheckEH_ConsigneePlace()
		{
			AWBHeader.EH_ConsigneePlace = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneePlaceInfo);

			AWBHeader.EH_ConsigneePlace = "##";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_ConsigneePlaceInfo);

			AWBHeader.EH_ConsigneePlace = "Something";
			AssertNoNotifications(AWBHeader.EH_ConsigneePlaceInfo);

			AWBHeader.EH_ConsigneePlace = "Something##";
			AssertNoNotifications(AWBHeader.EH_ConsigneePlaceInfo);
		}

		#endregion

		#region Agent

		public void TestCheckEH_AgentName()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "";
			AWBHeader.Validation.ValidateEH_AgentName();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AgentNameInfo);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "SomeOne";
			AWBHeader.Validation.ValidateEH_AgentName();
			AssertNoNotifications(AWBHeader.EH_AgentNameInfo);
		}

		public void TestCheckEH_AgentPlace()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "";
			AWBHeader.Validation.ValidateEH_AgentPlace();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AgentPlaceInfo);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "SomeOne";
			AWBHeader.Validation.ValidateEH_AgentPlace();
			AssertNoNotifications(AWBHeader.EH_AgentPlaceInfo);
		}

		public void TestCheckEH_AgentIATACode()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "";
			AWBHeader.Validation.ValidateEH_AgentIATACodeFormatted();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AgentIATACodeFormattedInfo);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567";
			AWBHeader.Validation.ValidateEH_AgentIATACodeFormatted();
			AssertNoNotifications(AWBHeader.EH_AgentIATACodeFormattedInfo);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "";
			AWBHeader.Validation.ValidateEH_AgentIATACodeFormatted();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AgentIATACodeFormattedInfo);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "Test";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "";
			AWBHeader.Validation.ValidateEH_AgentIATACodeFormatted();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AgentIATACodeFormattedInfo);
		}

		public void TestValidationOnAgentFields()
		{
			var mawbHeader = AWBHeader;
			mawbHeader.Validation.ValidateEH_AgentIATACodeFormatted();
			mawbHeader.Validation.ValidateEH_AgentName();
			mawbHeader.Validation.ValidateEH_AgentPlace();
			AssertHasMessageErrorContaining(mawbHeader.EH_AgentIATACodeFormattedInfo, "The Agent IATA Code is mandatory for a Master Air Waybill.");
			AssertHasMessageErrorContaining(mawbHeader.EH_AgentNameInfo, "You must have an Agent Name to go with the Agent IATA Code.");
			AssertHasMessageErrorContaining(mawbHeader.EH_AgentPlaceInfo, "You must have an Agent City to go with the Agent IATA Code.");
			AssertNoWarnings(mawbHeader.EH_AgentIATACodeFormattedInfo);
			AssertNoWarnings(mawbHeader.EH_AgentNameInfo);
			AssertNoWarnings(mawbHeader.EH_AgentPlaceInfo);

			AssertHasMessageErrorContaining(mawbHeader.EH_AgentIATACodeFormattedInfo, "The Agent's IATA Code can be entered as a default in two places:");
			AssertHasMessageErrorContaining(mawbHeader.EH_AgentNameInfo, "Agent Name can be entered as a default in the System Registry under");
			AssertHasMessageErrorContaining(mawbHeader.EH_AgentPlaceInfo, "Agent City can be entered as a default in the System Registry under");

			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);

			mawbHeader.Validation.ValidateEH_AgentIATACodeFormatted();
			mawbHeader.Validation.ValidateEH_AgentName();
			mawbHeader.Validation.ValidateEH_AgentPlace();
			AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
			AssertNoNotifications(mawbHeader.EH_AgentNameInfo);
			AssertNoNotifications(mawbHeader.EH_AgentPlaceInfo);
		}

		public void TestAgentIATACodeIsTheRightFormat()
		{
			var mawbHeader = AWBHeader;
			mawbHeader.EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;
			mawbHeader.EH_AgentIATACodeFormatted = "12-3 4567";
			AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);

			mawbHeader.EH_AgentIATACodeFormatted = "7654321";
			AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);

			mawbHeader.EH_AgentIATACodeFormatted = "123456";
			AssertHasMessageErrorContaining(mawbHeader.EH_AgentIATACodeFormattedInfo, ConsolExportAWBHeaderValidation.MessageErrorInvalidIATACode);

			mawbHeader.EH_AgentIATACodeFormatted = "12345678901";
			AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);

			mawbHeader.EH_AgentIATACodeFormatted = "12345678";
			AssertHasMessageErrorContaining(mawbHeader.EH_AgentIATACodeFormattedInfo, ConsolExportAWBHeaderValidation.MessageErrorInvalidIATACode);

			mawbHeader.EH_AgentIATACodeFormatted = "84-7 2947/3905";
			AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
		}

		public void TestAgentDetailsValidationWithAGTOptionalTurnedOff()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);

			var mawbHeader = AWBHeader;
			mawbHeader.EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;

			mawbHeader.Validation.ValidateAll();
			mawbHeader.EH_AgentPlace = "BEDROCK";

			CombineAssertions(delegate
			{
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentIATACodeFormattedInfo, "The Agent IATA Code is mandatory for a Master Air Waybill.");
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentNameInfo, "You must have an Agent Name to go with the Agent IATA Code.");
				AssertNoNotifications(mawbHeader.EH_AgentPlaceInfo);
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});

			mawbHeader.EH_AgentPlace = "";
			mawbHeader.EH_AgentName = "FLINTSTONES ROCKS";

			CombineAssertions(delegate
			{
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentIATACodeFormattedInfo, "The Agent IATA Code is mandatory for a Master Air Waybill.");
				AssertNoNotifications(mawbHeader.EH_AgentNameInfo);
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentPlaceInfo, "You must have an Agent City to go with the Agent IATA Code.");
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});

			mawbHeader.EH_AgentName = "";
			mawbHeader.EH_AgentAccountNo = "1234567";

			CombineAssertions(delegate
			{
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentIATACodeFormattedInfo, "The Agent IATA Code is mandatory for a Master Air Waybill.");
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentNameInfo, "You must have an Agent Name to go with the Agent IATA Code.");
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentPlaceInfo, "You must have an Agent City to go with the Agent IATA Code.");
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});

			mawbHeader.EH_AgentAccountNo = "";
			mawbHeader.EH_AgentIATACodeFormatted = "1234567";

			CombineAssertions(delegate
			{
				AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentNameInfo, "You must have an Agent Name to go with the Agent IATA Code.");
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentPlaceInfo, "You must have an Agent City to go with the Agent IATA Code.");
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});

			mawbHeader.EH_AgentIATACodeFormatted = "";

			CombineAssertions(delegate
			{
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentIATACodeFormattedInfo, "The Agent IATA Code is mandatory for a Master Air Waybill.");
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentNameInfo, "You must have an Agent Name to go with the Agent IATA Code.");
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentPlaceInfo, "You must have an Agent City to go with the Agent IATA Code.");
				AssertNoWarnings(mawbHeader.EH_AgentIATACodeFormattedInfo);
				AssertNoWarnings(mawbHeader.EH_AgentNameInfo);
				AssertNoWarnings(mawbHeader.EH_AgentPlaceInfo);
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});
		}

		public void TestAgentDetailsValidationWithAGTOptionalTurnedOn()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);

			var mawbHeader = AWBHeader;
			mawbHeader.EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;

			mawbHeader.Validation.ValidateAll();
			mawbHeader.EH_AgentPlace = "BEDROCK";

			CombineAssertions(delegate
			{
				AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
				AssertNoNotifications(mawbHeader.EH_AgentNameInfo);
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentPlaceInfo, "You cannot have an Agent City without an Agent IATA Code.");
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});

			mawbHeader.EH_AgentPlace = "";
			mawbHeader.EH_AgentName = "FLINTSTONES ROCKS";

			CombineAssertions(delegate
			{
				AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentNameInfo, "You cannot have an Agent Name without an Agent IATA Code.");
				AssertNoNotifications(mawbHeader.EH_AgentPlaceInfo);
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});

			mawbHeader.EH_AgentName = "";
			mawbHeader.EH_AgentAccountNo = "1234567";

			CombineAssertions(delegate
			{
				AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
				AssertNoNotifications(mawbHeader.EH_AgentNameInfo);
				AssertNoNotifications(mawbHeader.EH_AgentPlaceInfo);
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentAccountNoInfo, "You cannot have an Account No without an Agent IATA Code.");
			});

			mawbHeader.EH_AgentAccountNo = "";
			mawbHeader.EH_AgentIATACodeFormatted = "1234567";

			CombineAssertions(delegate
			{
				AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentNameInfo, "You must have an Agent Name to go with the Agent IATA Code.");
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentPlaceInfo, "You must have an Agent City to go with the Agent IATA Code.");
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});

			mawbHeader.EH_AgentIATACodeFormatted = "";

			CombineAssertions(delegate
			{
				AssertNoNotifications(mawbHeader.EH_AgentNameInfo);
				AssertNoNotifications(mawbHeader.EH_AgentPlaceInfo);
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
				AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
			});
		}

		public void TestIfIATACodeISRemovedWhenAgentAccountNoWasEntered()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);

			var mawbHeader = AWBHeader;
			mawbHeader.EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;

			mawbHeader.EH_AgentIATACodeFormatted = "1234567";
			mawbHeader.EH_AgentAccountNo = "1234567";

			CombineAssertions(delegate
			{
				AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
				AssertNoNotifications(mawbHeader.EH_AgentAccountNoInfo);
			});

			mawbHeader.EH_AgentIATACodeFormatted = "";

			CombineAssertions(delegate
			{
				AssertHasMessageErrorContaining(mawbHeader.EH_AgentAccountNoInfo, "You cannot have an Account No without an Agent IATA Code.");
				AssertNoNotifications(mawbHeader.EH_AgentIATACodeFormattedInfo);
			});
		}

		#endregion

		#region Also Notify

		public void TestValidateMandatoryAlsoNotifyFields()
		{
			AWBHeader.EH_AlsoNotifyName = "";
			AWBHeader.EH_AlsoNotifyAddress = "";
			AWBHeader.EH_AlsoNotifyPlace = "";
			AWBHeader.EH_AlsoNotifyCountryCode = "";
			AWBHeader.Validation.ValidateMandatoryAlsoNotifyFields();
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyNameInfo);
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyAddressInfo);
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyPlaceInfo);
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyCountryCodeInfo);

			AWBHeader.EH_AlsoNotifyState = "NSW";
			AWBHeader.Validation.ValidateMandatoryAlsoNotifyFields();
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyNameInfo);
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyAddressInfo);
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyPlaceInfo);
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyCountryCodeInfo);
		}

		public void TestNotificationLevelForFieldsRequiredForElectronicTransmission()
		{
			ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AWBHeader.EH_AlsoNotifyName = "FRED FLINTSTONE";
			AssertHasMessageErrors(AWBHeader.EH_AlsoNotifyAddressInfo);
			AssertNoWarnings(AWBHeader.EH_AlsoNotifyAddressInfo);

			ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AWBHeader.EH_AlsoNotifyName = "FRED FLINTSTONE";
			AssertNoMessageErrors(AWBHeader.EH_AlsoNotifyAddressInfo);
			AssertHasWarnings(AWBHeader.EH_AlsoNotifyAddressInfo);
		}

		public void TestWarningOnAlsoNotifyNameWhenCharactersExceedLimit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "SGSIN";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "AUSYD";

				const string expectedWarningMessage = "Although the FWB allows for 166 characters of Also Notify information to be sent in the NFY element, when the sum of characters in the Also Notify box, Handling Information box and SCI box values (as well as the Security Status box for SG login company) exceeds 216 characters, the Also Notify information will be omitted from the FWB message (NFY element).";

				AWBHeader.EH_ParentID = consol.PK;
				AWBHeader.EH_HandlingInformation = "THIS IS A REALLY LONG TEXT. THIS IS A REALLY LONG TEXT. THIS IS A REALLY LONG TEXT. THIS IS A REALLY LONG TEXT.";
				AWBHeader.EH_AlsoNotifyName = "SEKO WORLDWIDE JAPAN HARTRODT VERY LONG TEXT";
				AWBHeader.EH_AlsoNotifyAddress = "TORANOMON-SUZUKI BLD. 7F20-4,3- CHOMEMNATO-KU TOKY";
				AWBHeader.EH_AlsoNotifyPlace = "Sydney";
				AWBHeader.EH_AlsoNotifyState = "NSW";
				AWBHeader.EH_AlsoNotifyTraderNoCountryCode = "AU";
				AWBHeader.EH_AlsoNotifyPostCode = "1234";
				AWBHeader.EH_AlsoNotifyContactCode = "PHO";
				AWBHeader.EH_AlsoNotifyContactDetail = "13-3433";

				AssertHasWarningContaining(AWBHeader.EH_AlsoNotifyNameInfo, expectedWarningMessage);
			}
		}

		public void TestCheckEH_AlsoNotifyName()
		{
			AWBHeader.EH_AlsoNotifyName = "";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyNameInfo);

			AWBHeader.EH_AlsoNotifyName = "";
			AWBHeader.EH_AlsoNotifyAddress = "Address";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyNameInfo);

			AWBHeader.EH_AlsoNotifyName = "###";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyNameInfo);

			AWBHeader.EH_AlsoNotifyName = "Something";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyNameInfo);

			AWBHeader.EH_AlsoNotifyName = "Something###";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyNameInfo);
		}

		public void TestCheckEH_AlsoNotifyAddress()
		{
			AWBHeader.EH_AlsoNotifyAddress = "";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyAddressInfo);

			AWBHeader.EH_AlsoNotifyName = "Name";
			AWBHeader.EH_AlsoNotifyAddress = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyAddressInfo);

			AWBHeader.EH_AlsoNotifyAddress = "###";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyAddressInfo);

			AWBHeader.EH_AlsoNotifyAddress = "Something";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyAddressInfo);

			AWBHeader.EH_AlsoNotifyAddress = "Something###";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyAddressInfo);
		}

		public void TestCheckAlsoNotifyAddressFields()
		{
			CombineAssertions(() =>
			{
				AssertAddressWithInvalidCharacters(AWBHeader.EH_AlsoNotifyNameInfo, "Also Notify Name is not valid.");
				AssertAddressWithInvalidCharacters(AWBHeader.EH_AlsoNotifyAddressInfo, "Also Notify Address is not valid.");
				AssertAddressWithInvalidCharacters(AWBHeader.EH_AlsoNotifyAddress2Info, "Also Notify Address is not valid.");

				AssertAddressWithLongerLength(AWBHeader.EH_AlsoNotifyNameInfo, "Also Notify Name is too long, only the first 35 characters can be sent.");
				AssertAddressWithLongerLength(AWBHeader.EH_AlsoNotifyAddressInfo, "Also Notify Address is too long, only the first 35 characters can be sent.");
				AssertAddressWithLongerLength(AWBHeader.EH_AlsoNotifyAddress2Info, "Also Notify Address is too long, only the first 35 characters can be sent.");

				AssertAddressWithInvalidCharacters(AWBHeader.EH_AlsoNotifyPlaceInfo, "Also Notify City is not valid.");
				AssertState(AWBHeader.EH_AlsoNotifyStateInfo, "Also Notify State is not valid.");
				AssertPostCode(AWBHeader.EH_AlsoNotifyPostCodeInfo, "Also Notify Post Code is not valid.");
			});
		}

		public void TestCheckEH_AlsoNotifyPlace()
		{
			AWBHeader.EH_AlsoNotifyPlace = "";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyPlaceInfo);

			AWBHeader.EH_AlsoNotifyPlace = "";
			AWBHeader.EH_AlsoNotifyAddress = "Address";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyPlaceInfo);

			AWBHeader.EH_AlsoNotifyPlace = "###";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyPlaceInfo);

			AWBHeader.EH_AlsoNotifyPlace = "Place";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyPlaceInfo);

			AWBHeader.EH_AlsoNotifyPlace = "Place##";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyPlaceInfo);
		}

		public void TestCheckEH_AlsoNotifyCountryCode()
		{
			AWBHeader.EH_AlsoNotifyCountryCode = "";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyCountryCodeInfo);

			AWBHeader.EH_AlsoNotifyCountryCode = "";
			AWBHeader.EH_AlsoNotifyAddress = "Address";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyCountryCodeInfo);

			AWBHeader.EH_AlsoNotifyCountryCode = "A1";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyCountryCodeInfo);

			AWBHeader.EH_AlsoNotifyCountryCode = "SG";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyCountryCodeInfo);
		}

		public void TestCheckEH_AlsoNotifyContactCode()
		{
			AWBHeader.EH_AlsoNotifyContactCode = "";
			AWBHeader.EH_AlsoNotifyContactDetail = "";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyContactCodeInfo);

			AWBHeader.EH_AlsoNotifyContactDetail = "ABC123";
			AWBHeader.EH_AlsoNotifyContactCode = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyContactCodeInfo);

			AWBHeader.EH_AlsoNotifyContactCode = "##-";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyContactCodeInfo);

			AWBHeader.EH_AlsoNotifyContactCode = "TEL";
			AssertHasNotifications(AWBHeader.EH_AlsoNotifyContactCodeInfo);

			AWBHeader.EH_AlsoNotifyContactCode = "TE";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyContactCodeInfo);
		}

		public void TestCheckEH_AlsoNotifyContactDetail()
		{
			AWBHeader.EH_AlsoNotifyContactCode = "";
			AWBHeader.EH_AlsoNotifyContactDetail = "";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyContactDetailInfo);

			AWBHeader.EH_AlsoNotifyContactCode = "TEL";
			AWBHeader.EH_AlsoNotifyContactDetail = "";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyContactDetailInfo);

			AWBHeader.EH_AlsoNotifyContactDetail = "##-";
			AssertHasNotificationsForElectronicTransmission(AWBHeader.EH_AlsoNotifyContactDetailInfo);

			AWBHeader.EH_AlsoNotifyContactDetail = "1232123";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyContactDetailInfo);

			AWBHeader.EH_AlsoNotifyContactDetail = "+(1)232123.";
			AssertNoNotifications(AWBHeader.EH_AlsoNotifyContactDetailInfo);
		}

		#endregion

		#region Handling Information

		public void TestHandlingInformationACIDNumberForEgypt()
		{
			const string expectedNotification = "The ACID number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the ACI in the Reference Numbers grid.";

			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			forwardingConsol.JK_RL_NKLoadPort = "AUBNE";
			forwardingConsol.JK_RL_NKDischargePort = "CNSHA";

			var shipment1 = forwardingConsol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S1000001";
			shipment1.JS_RL_NKOrigin = "AUBNE";
			shipment1.JS_RL_NKDestination = "CNSHA";

			var shipment2 = forwardingConsol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S1000002";
			shipment2.JS_RL_NKOrigin = "AUBNE";
			shipment2.JS_RL_NKDestination = "CNSHA";

			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			awbHeader.EH_ParentID = forwardingConsol.PK;
			awbHeader.Populate();

			AssertNoWarningContaining("Not destined to Egypt", awbHeader.EH_HandlingInformationInfo, expectedNotification);

			forwardingConsol.JK_RL_NKDischargePort = "EGCAI";
			shipment1.JS_RL_NKDestination = "EGCAI";
			shipment2.JS_RL_NKDestination = "EGCAI";

			awbHeader.Populate();
			AssertHasWarningContaining("Has notification because consol does not have Egypt ACID", awbHeader.EH_HandlingInformationInfo, expectedNotification);

			var entryNum1 = shipment1.Numbers.AddNew();
			entryNum1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			entryNum1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNum1.CE_RN_NKCountryCode = Constants.CountryCodes.Egypt;
			entryNum1.CE_EntryNum = "6AU123456789D0VAHK11N";

			awbHeader.Populate();
			AssertHasWarningContaining("Has notification because consol does not have Egypt ACID", awbHeader.EH_HandlingInformationInfo, expectedNotification);

			var entryNum2 = shipment2.Numbers.AddNew();
			entryNum2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			entryNum2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNum2.CE_RN_NKCountryCode = Constants.CountryCodes.Egypt;
			entryNum2.CE_EntryNum = "7AU234567890E1WBIL22M";

			awbHeader.Populate();
			AssertHasWarningContaining("Has notification because consol does not have Egypt ACID", awbHeader.EH_HandlingInformationInfo, expectedNotification);

			var entryNum3 = forwardingConsol.Numbers.AddNew();
			entryNum3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			entryNum3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNum3.CE_RN_NKCountryCode = Constants.CountryCodes.Egypt;
			entryNum3.CE_EntryNum = "7AU234567890E1WBIL22M";

			awbHeader.Populate();
			AssertNoWarningContaining("Consol has ACID numbers", awbHeader.EH_HandlingInformationInfo, expectedNotification);

			awbHeader.Parent.IsAWBValuesOverriddenProperty = true;
			awbHeader.EH_HandlingInformation = "Handling info not containing required numbers.";
			AssertHasWarningContaining("Has notification for overridden AWB", awbHeader.EH_HandlingInformationInfo, expectedNotification);

			awbHeader.EH_HandlingInformation = "Handling Information including ACID Number:12345 And extra information";
			AssertNoWarningContaining("Overridden AWB has ACID Number text", awbHeader.EH_HandlingInformationInfo, expectedNotification);
		}

		#endregion

		#region Shipper

		public void TestShipper_China()
		{
			var consol = Factory.New<ForwardingConsol>();
			AWBHeader.EH_ParentID = consol.PK;
			AWBHeader.EH_Table = JobConsolSchema.Constants.TableName;
			AWBHeader.Consol.Transports.AddNew();

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
			var consol = Factory.New<ForwardingConsol>();
			AWBHeader.EH_ParentID = consol.PK;
			AWBHeader.EH_Table = JobConsolSchema.Constants.TableName;
			AWBHeader.Consol.Transports.AddNew();

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
			var consol = Factory.New<ForwardingConsol>();
			AWBHeader.EH_ParentID = consol.PK;
			AWBHeader.EH_Table = JobConsolSchema.Constants.TableName;
			AWBHeader.Consol.Transports.AddNew();

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

		void SetIsImportToChinaForTest(bool isImportToChina)
		{
			if (isImportToChina)
			{
				AWBHeader.Consol.JK_RL_NKDischargePort = "CNSHA";
			}
			else
			{
				AWBHeader.Consol.JK_RL_NKDischargePort = "AUSYD";
			}
		}

		void SetIsTransitingThroughChinaForTest(bool isTransitingThroughChina)
		{
			if (isTransitingThroughChina)
			{
				AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
				AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "CNSHA";
				AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
				Assert(AWBHeader.IsTransitingThroughChina);
			}
			else
			{
				AWBHeader.Consol.JK_RL_NKLoadPort = "CNSHA";
				AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "CNSHA";
				AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			}
		}

		Action setHeaderToChinaPortFunc = () => { };
		Action setHeaderToNonChinaPortFunc = () => { };

		protected override Forwarding.AWB.Business.ExportAWBHeader GetNewAWBHeader() => Factory.New<ConsolExportAWBHeader>();

		protected new ConsolExportAWBHeader AWBHeader => (ConsolExportAWBHeader)base.AWBHeader;

		protected override INotificationType ExpectedAWBHeaderNotificationType => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override INotificationType ExpectedBookingsNotificationType => CargoWise.EntityFramework.NotificationType.MessageError;

		#endregion
	}
}
