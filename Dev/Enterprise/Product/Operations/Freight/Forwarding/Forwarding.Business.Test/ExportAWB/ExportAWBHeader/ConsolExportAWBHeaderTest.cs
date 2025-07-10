using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.AWB;
#if WINZOR
using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using Moq;
#endif

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolExportAWBHeader))]
	public class ConsolExportAWBHeaderTest : EnterpriseBusinessObjectTestCase
	{
		#region Loader

		public void TestLoadOrCreate()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => ConsolExportAWBHeader.LoadOrCreate(null));

			var consol1 = Factory.New<ForwardingConsol>();
			var header1 = Factory.New<ConsolExportAWBHeader>();
			header1.EH_ParentID = consol1.PK;
			header1.EH_Table = JobConsolSchema.Constants.TableName;

			AssertEquals(header1, ConsolExportAWBHeader.LoadOrCreate(consol1));

			var consol2 = Factory.New<ForwardingConsol>();
			var header2 = ConsolExportAWBHeader.LoadOrCreate(consol2);

			AssertEquals(true, typeof(ConsolExportAWBHeader).IsAssignableFrom(header2.GetType()));
			AssertEquals(consol2.PK, header2.EH_ParentID);
			AssertEquals(JobConsolSchema.Constants.TableName, header2.EH_Table);

			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_OverrideWaybillDefaults = true;

			AssertEquals("Precondition", true, header2.IsSavedByFactory);
			Factory.Save();

			var consol2InAnotherFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol2.PK);
			AssertEquals(header2.PK, ConsolExportAWBHeader.LoadOrCreate(consol2InAnotherFactory).PK);
		}

		#endregion

		public void TestNatureAndQtyOfGoodsMaxLength()
		{
			AWBHeader.AWBRateLine5.NatureAndQtyOfGoods.Text = "> 20 characters but < 35";
			AssertHasWarning(AWBHeader.AWBRateLine5.NatureAndQtyOfGoods.TextInfo, "This field is longer than 20 characters and will be wrapped in the message.");
		}

		public void TestPopulateOtherChargesWhenConsolIsGateway()
		{
			SetUpForDeparturePort("AUSYD");

			var consol = AWBHeader.Consol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPorts1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUSYD";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.MO;
			chargeCode1.AC_Desc = "Charge MO";

			consol.JK_AgentType = Constants.AgentType.Agent;
			Assert(consol.IsGateway());
			AssertEquals(1, consol.Shipments.Count);

			using (var consolJob = new JobHeader.Loader(consol).TryLoadOrCreate())
			{
				using (var shipmentJob = new JobHeader.Loader(consol.Shipments[0]).TryLoadOrCreate())
				{
					var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					chargeCode.AC_IATA_ChargeCodeMap = "AC";
					chargeCode.AC_Code = "AAA";

					var consolCharge = Factory.NewWithValidTestData<JobCharge>();
					consolCharge.JR_AC = chargeCode.PK;
					consolCharge.JR_JH = consolJob.PK;
					consolCharge.JR_RX_NKSellCurrency = "AUD";
					consolCharge.JR_RX_NKCostCurrency = "AUD";
					consolCharge.JR_LocalCostAmt = 20;
					consolCharge.JR_OSCostAmt = 20;
					consolCharge.JR_LocalSellAmt = 25;
					consolCharge.JR_OSSellAmt = 25;
					consolCharge.JR_JH_InternalJob = consolJob.PK;

					var shipmentCharge = Factory.NewWithValidTestData<JobCharge>();
					shipmentCharge.JR_AC = chargeCode.PK;
					shipmentCharge.JR_JH = shipmentJob.PK;
					shipmentCharge.JR_RX_NKSellCurrency = "AUD";
					shipmentCharge.JR_LocalCostAmt = 10;
					shipmentCharge.JR_OSCostAmt = 10;
					shipmentCharge.JR_LocalSellAmt = 15;
					shipmentCharge.JR_OSSellAmt = 15;
					shipmentCharge.JR_JH_InternalJob = shipmentJob.PK;

					Factory.Save();

					var consolCost1 = CreateConsolCost(chargeCode1.PK, 300m, 30m);

					AWBHeader.AWBOtherCharges.RemoveAll();
					AssertEquals("Pre-condition: no other charges", 0, AWBHeader.AWBOtherCharges.Count);

					AssertNoExceptionThrown(() => AWBHeader.Populate());
					AssertEquals(2, AWBHeader.AWBOtherCharges.Count);
					AssertEquals(20m, AWBHeader.AWBOtherCharges[0].EO_Amount);
					AssertEquals(300m, AWBHeader.AWBOtherCharges[1].EO_Amount);
				}
			}
		}

		public void TestPopulateWithNoRefContainer()
		{
			var uldContainer = AWBHeader.Consol.Containers.AddNew();
			uldContainer.JC_ContainerMode = Core.Constants.ContainerModes.ULD;

			AssertEquals(ZGuid.Empty, uldContainer.JC_RC);
			AssertNoExceptionThrown(() => AWBHeader.Populate());
		}

		public void TestPopulateSpecialHandlingCode_ConsolLoadPortHasNullCountryCode()
		{
			RefUNLOCO newUNLOCO = Factory.New<RefUNLOCO>();
			newUNLOCO.RL_Code = "JM";
			newUNLOCO.RL_PortName = "Country-less UNLOCO";
			newUNLOCO.RL_RN_NKCountryCode = ZString.Empty;
			Factory.Save();

			AWBHeader.Consol.JK_RL_NKLoadPort = "JM";
			AWBHeader.Consol.JK_RL_NKDischargePort = "USBOS";
			AWBHeader.Populate();

			Assert("Pre-condition", AWBHeader.Consol.IsExport());
			AssertNull("Pre-condition", newUNLOCO.Country);
			AssertEquals("Expected to return an empty string as ConsolIsCommonTransitCountryExport has returned false without an exception", ZString.Empty, AWBHeader.SpecialHandlingCode);
		}

		#region TestGetShippingLoadAndCount_ShipmentsForTotallingHaveBeenModified_NoExceptionThrown

		class ForwardingPackLineForTest : ForwardingPackLine
		{
			public ForwardingPackLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			void RemoveShipmentFromShipmentsForTotalling()
			{
				CurrentConsol.ShipmentsForTotalling.Remove(Shipment);
			}

			public override ZInt JL_PackageCount
			{
				get
				{
					RemoveShipmentFromShipmentsForTotalling();

					return base.JL_PackageCount;
				}
				set => base.JL_PackageCount = value;
			}
		}

		public void TestGetShippingLoadAndCount_ShipmentsForTotallingHaveBeenModified_NoExceptionThrown()
		{
			Factory.RefreshEnabled = false;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var leadShipment = Factory.New<ForwardingShipment>();
			leadShipment.JS_RL_NKOrigin = "GBLHR";
			leadShipment.JS_RL_NKDestination = "CNSHA";
			leadShipment.JS_TransportMode = "SEA";
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var packlineInLeadShipment = Factory.New<ForwardingPackLineForTest>();
			packlineInLeadShipment.CurrentConsol = consol;
			leadShipment.InnerPackLines.Add(packlineInLeadShipment);

			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_RL_NKOrigin = "GBLHR";
			subShipment.JS_RL_NKDestination = "CNSHA";
			subShipment.JS_TransportMode = "SEA";
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var packlineInSubShipment = Factory.New<ForwardingPackLineForTest>();
			packlineInSubShipment.CurrentConsol = consol;
			subShipment.InnerPackLines.Add(packlineInSubShipment);

			consol.Shipments.Add(leadShipment);
			leadShipment.CoLoadShipments.Add(subShipment);

			Factory.Save();

			AssertNoExceptionThrown(() => ConsolExportAWBHeader.LoadOrCreate(consol));
		}

		#endregion

		public void TestHSCode_NoDuplicateHSCode()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_OverrideWaybillDefaults = ZBool.False;
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_HarmonisedCode = "123456";

			var packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_HarmonisedCode = "123457";

			var packLine3 = shipment1.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 1;
			packLine3.JL_HarmonisedCode = "123456";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_OverrideWaybillDefaults = ZBool.False;
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";

			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_PackageCount = 1;
			packLine4.JL_HarmonisedCode = "123456";

			var packLine5 = shipment2.OuterPackLines.AddNew();
			packLine5.JL_PackageCount = 2;
			packLine5.JL_HarmonisedCode = "123457";

			var consol = shipment1.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "ID6DI";
			shipment2.Consols.Add(consol);

			awbHeader.EH_ParentID = consol.PK;
			awbHeader.Consol.JK_AgentType = Constants.AgentType.Agent;

			awbHeader.Populate();

			var hsCodeRateLines = awbHeader.AWBRateLines.Cast<ExportAWBRateLine>().Where(e => e.IsHSCodeLine).ToList();
			var hSCodes = hsCodeRateLines.Select(c => c.NatureAndQtyOfGoods.Text.ToString()).ToList();
			var expectedHSCodes = new List<string> { "HS Code: 123456", "HS Code: 123457" };

			AssertContainsExactElementsInAnyOrder(expectedHSCodes, hSCodes);
		}

		public void TestVATNumberRequired_Discharge_VN()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.VietNam, CountryCodes.VietNam, 1, "VAT", ZString.Empty, "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKDischargePort = "VN7BR";
			AWBHeader.Consol.Shipments[0].Consignee.MainAddress.OA_RN_NKCountryCode = "VN";
			AWBHeader.Populate();

			AssertEquals("No Tax type should be displayed when no tax number.", ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "Company ID, VAT (Government VAT Code) is required to comply with Vietnam Customs notice No. 6889/TCHQ-GSQL");

			AWBHeader.EH_ConsigneeTraderNo = "12364";
			AssertNoWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "Company ID, VAT (Government VAT Code) is required to comply with Vietnam Customs notice No. 6889/TCHQ-GSQL");
		}

		public void TestVATNumberRequired_Discharge_ID()
		{
			CreateRefDocOrgCusCode("PPN", CountryCodes.Indonesia, CountryCodes.Indonesia, 1, "NPWP", "NPWP tax identification number", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKDischargePort = "ID6DI";
			AWBHeader.Consol.Shipments[0].Consignee.MainAddress.OA_RN_NKCountryCode = "ID";
			AWBHeader.Populate();

			AssertEquals("No Tax type should be displayed when no tax number.", ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "Consignee PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");

			AWBHeader.EH_ConsigneeTraderNo = "12364";
			AssertNoWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "Consignee PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");
		}

		public void TestPopulateConsigneeAddress_ShouldHaveCANWhenAdvanceCargoReportingSelfFilerTickedForConsolToIndia()
		{
			CreateRefDocOrgCusCode(IndiaOrgCusCodeInfo.OrgCusCodes.CAN, CountryCodes.India, CountryCodes.India, 1, IndiaOrgCusCodeInfo.OrgCusCodes.CAN, ZString.Empty, "AWB");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "INDEL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "INDEL";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var consigneeCode = consignee.CustomsCodes.AddNew();
			consigneeCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			consigneeCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			consigneeCode.OK_CustomsRegNo = "997755331";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var receivingAgentCode = receivingAgent.CustomsCodes.AddNew();
			receivingAgentCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			receivingAgentCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			receivingAgentCode.OK_CustomsRegNo = "123456789";

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			AWBHeader.SetConsol(consol);

			AWBHeader.Populate();

			AssertEquals("997755331", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("CAN", AWBHeader.EH_ConsigneeTraderNoType);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			AWBHeader.Populate();

			AssertEquals("123456789", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("CAN", AWBHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateConsigneeAddress_ShouldNotHaveCANWhenAdvanceCargoReportingSelfFilerUntickedForConsolToIndia()
		{
			CreateRefDocOrgCusCode(IndiaOrgCusCodeInfo.OrgCusCodes.CAN, CountryCodes.India, CountryCodes.India, 1, IndiaOrgCusCodeInfo.OrgCusCodes.CAN, ZString.Empty, "AWB");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "INDEL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "INDEL";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var consigneeCode = consignee.CustomsCodes.AddNew();
			consigneeCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			consigneeCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			consigneeCode.OK_CustomsRegNo = "997755331";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var receivingAgentCode = receivingAgent.CustomsCodes.AddNew();
			receivingAgentCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			receivingAgentCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			receivingAgentCode.OK_CustomsRegNo = "123456789";

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			AWBHeader.SetConsol(consol);

			AWBHeader.Populate();

			AssertEquals("", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("", AWBHeader.EH_ConsigneeTraderNoType);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			AWBHeader.Populate();

			AssertEquals("", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("", AWBHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateShipperAddress_ShouldNotHaveCANEvenThoughAdvanceCargoReportingSelfFilerTickedForConsolFromIndia()
		{
			CreateRefDocOrgCusCode(IndiaOrgCusCodeInfo.OrgCusCodes.CAN, CountryCodes.India, CountryCodes.India, 1, IndiaOrgCusCodeInfo.OrgCusCodes.CAN, ZString.Empty, "AWB");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "INDEL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKLoadPort = "INDEL";
			shipment.JS_RL_NKDischargePort = "AUSYD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var code = consignor.CustomsCodes.AddNew();
			code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			code.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			code.OK_CustomsRegNo = "997755331";

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var sendingAgentCode = sendingAgent.CustomsCodes.AddNew();
			sendingAgentCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			sendingAgentCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			sendingAgentCode.OK_CustomsRegNo = "123456789";

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			AssertEquals("", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("", AWBHeader.EH_ShipperTraderNoType);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			AWBHeader.Populate();

			AssertEquals("", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("", AWBHeader.EH_ShipperTraderNoType);
		}

		public void TestPopulateAlsoNotifyAddress_ShouldNotHaveCAN()
		{
			CreateRefDocOrgCusCode(IndiaOrgCusCodeInfo.OrgCusCodes.CAN, CountryCodes.India, CountryCodes.India, 1, IndiaOrgCusCodeInfo.OrgCusCodes.CAN, ZString.Empty, "AWB");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "INDEL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "INDEL";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var consigneeCode = consignee.CustomsCodes.AddNew();
			consigneeCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			consigneeCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			consigneeCode.OK_CustomsRegNo = "997755331";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			notifyParty.Organisation.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var notifyPartyCode = notifyParty.Organisation.CustomsCodes.AddNew();
			notifyPartyCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			notifyPartyCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			notifyPartyCode.OK_CustomsRegNo = "123456789";

			AWBHeader.SetConsol(consol);

			AWBHeader.Populate();

			AssertEquals("997755331", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("CAN", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("", AWBHeader.EH_AlsoNotifyTraderNo);
			AssertEquals("", AWBHeader.EH_AlsoNotifyTraderNoType);
		}

		public void TestEORITaxNumber_DischargeInEU_Consignee()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "FRPAR";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "FRPAR";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("DE123456789", AWBHeader.EH_ConsigneeTraderNo);

			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "HKHKG";
			AWBHeader.Populate();
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestMVATaxNumber_DischargeInNorway_Consignee()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "NOGRI";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "NOGRI";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.NorwayCodeTypes.MVA;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Norway;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.NorwayCodeTypes.MVA, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("NO123456789", AWBHeader.EH_ConsigneeTraderNo);

			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "HKHKG";
			AWBHeader.Populate();
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestUIDTaxNumber_DischargeInSwitzerland_Consignee()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "CHALE";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "CHALE";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.SwissCodeTypes.UID;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.SwissCodeTypes.UID, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("CH123456789", AWBHeader.EH_ConsigneeTraderNo);

			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "HKHKG";
			AWBHeader.Populate();
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestUIDTaxNumber_DischargeInLiechtenstein_Consignee()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "LIBAZ";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "LIBAZ";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.SwissCodeTypes.UID;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Liechtenstein;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.SwissCodeTypes.UID, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("CH123456789", AWBHeader.EH_ConsigneeTraderNo);

			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.SwissCodeTypes.UID, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("CH123456789", AWBHeader.EH_ConsigneeTraderNo);

			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "HKHKG";
			AWBHeader.Populate();
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestICETaxNumber_DischargeInMorocco_Consignee_Direct()
		{
			CreateRefDocOrgCusCode("ICE", "MA", "MA", 1, "ICE", "ICE", "AWB");
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "MACAS";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "MACAS";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Morocco;
			code1.OK_CustomsRegNo = "123456789123456";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.MoroccoCodeTypes.ICE, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals(code1.OK_CustomsRegNo, AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestICETaxNumber_DischargeInMorocco_Consignee_NotDirect()
		{
			CreateRefDocOrgCusCode("ICE", "MA", "MA", 1, "ICE", "ICE", "AWB");
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "MACAS";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "MACAS";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Morocco;
			code1.OK_CustomsRegNo = "123456789123456";

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			var agt1 = receivingForwarder.CustomsCodes.AddNew();
			agt1.OK_RN_NKCodeCountry = CountryCodes.Morocco;
			agt1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			agt1.OK_CustomsRegNo = "111111111111111";

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Populate();

			AssertEquals(OrgCusCode.MoroccoCodeTypes.ICE, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals(code1.OK_CustomsRegNo, AWBHeader.EH_ConsigneeTraderNo);

			var nonDirectAgentTypes = new List<string> { "AGT", "CLD", "CHT", "COU", "OTH", "CLA", "CLM" };
			foreach (var agentType in nonDirectAgentTypes)
			{
				AWBHeader.Consol.JK_AgentType = agentType;
				AWBHeader.Populate();

				AssertEquals(OrgCusCode.MoroccoCodeTypes.ICE, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals(agt1.OK_CustomsRegNo, AWBHeader.EH_ConsigneeTraderNo);
			}
		}

		public void TestEORITaxNumber_DischargeInEU_AlsoNotify()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "FRPAR";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "FRPAR";

			var notifyParty = AWBHeader.Consol.Shipments[0].DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("DE123456789", AWBHeader.EH_AlsoNotifyTraderNo);

			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "HKHKG";
			AWBHeader.Populate();
			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestICETaxNumber_DischargeInMorocco_AlsoNotify()
		{
			CreateRefDocOrgCusCode("ICE", "MA", "MA", 1, "ICE", "ICE", "AWB");
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "MAAGA";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "MACAS";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "MACAS";
			transport2.JW_RL_NKDiscPort = "MAAGA";

			var notifyParty = AWBHeader.Consol.Shipments[0].DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Morocco;
			code1.OK_CustomsRegNo = "111111111111111";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.MoroccoCodeTypes.ICE, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals(code1.OK_CustomsRegNo, AWBHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestShipperTradeNoHasNoCountryCodePrefix()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "DEHAM";

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";

			var consignor = AWBHeader.Consol.Shipments[0].Consignor;
			var customCode = consignor.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			customCode.OK_CustomsRegNo = "XI123456789";

			AWBHeader.Populate();
			AssertEquals("XI123456789", AWBHeader.EH_ShipperTraderNo);

			customCode.OK_CustomsRegNo = "A123456789";
			AWBHeader.Populate();
			AssertEquals("DEA123456789", AWBHeader.EH_ShipperTraderNo);

			customCode.OK_CustomsRegNo = "2";
			AWBHeader.Populate();
			AssertEquals("DE2", AWBHeader.EH_ShipperTraderNo);
		}

		public void TestConsigneeTraderNoHasNoCountryCodePrefix_EOR()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "CHALE";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "CHALE";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var customCode = consignee.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
			customCode.OK_CustomsRegNo = "XI123456789";

			AWBHeader.Populate();
			AssertEquals("XI123456789", AWBHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "A123456789";
			AWBHeader.Populate();
			AssertEquals("CHA123456789", AWBHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "2";
			AWBHeader.Populate();
			AssertEquals("CH2", AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestConsigneeTraderNoHasNoCountryCodePrefix_MVA()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "NOGRI";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "NOGRI";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var customCode = consignee.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.NorwayCodeTypes.MVA;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Norway;
			customCode.OK_CustomsRegNo = "XX123456789";

			AWBHeader.Populate();
			AssertEquals("XX123456789", AWBHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "X123456789";
			AWBHeader.Populate();
			AssertEquals("NOX123456789", AWBHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "2";
			AWBHeader.Populate();
			AssertEquals("NO2", AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestConsigneeTraderNoHasNoCountryCodePrefix_UID()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "LIBAZ";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "LIBAZ";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var customCode = consignee.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.SwissCodeTypes.UID;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Liechtenstein;
			customCode.OK_CustomsRegNo = "EI123456789";

			AWBHeader.Populate();
			AssertEquals("EI123456789", AWBHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "E123456789";
			AWBHeader.Populate();
			AssertEquals("CHE123456789", AWBHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "2";
			AWBHeader.Populate();
			AssertEquals("CH2", AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestPopulateConsigneeAddress_ShouldUseAINOnTaxInfoForNonDirectConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BDDAC";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			consol.ReceivingForwarderWithContact.OrgPK = consignee.PK;
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			ainCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			ainCode.OK_CustomsRegNo = "123AIN";
			var vatCode = consignee.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			vatCode.OK_CustomsRegNo = "123BIN";

			AWBHeader.SetConsol(consol);

			CreateRefDocOrgCusCode("VAT", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "BIN", ZString.Empty, "AWB");

			CreateRefDocOrgCusCode("AIN", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "AIN", ZString.Empty, "AWB");

			Factory.Save();

			AWBHeader.Populate();

			AssertEquals("123AIN", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("AIN", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("BD", AWBHeader.EH_ConsigneeTraderNoCountryCode);
			AssertEquals("123AIN", AWBHeader.EH_AlsoNotifyTraderNo);
			AssertEquals("AIN", AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("BD", AWBHeader.EH_AlsoNotifyTraderNoCountryCode);
		}

		public void TestPopulateConsigneeAddress_ShouldUseNITOnTaxInfoForDirectConsolToBolivia()
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Bolivia, CountryCodes.Bolivia, 1, "NIT", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BOLPB";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bolivia Consignee";
			consignee.OH_RL_NKClosestPort = "BOLPB";
			consignee.MainAddress.Address1 = "Unit 717";
			consignee.MainAddress.Address2 = "11 Beautiful Street";
			consignee.MainAddress.City = "La Paz";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BO";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			AWBHeader.SetConsol(consol);
			Factory.Save();

			AWBHeader.Populate();

			string expectedErrorMessageConsignee = "The Consignee's NIT number is required for shipments to Bolivia.";
			AssertHasMessageError(AWBHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);

			var nitCode = consignee.CustomsCodes.AddNew();
			nitCode.OK_CodeType = OrgCusCode.BoliviaCodeTypes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bolivia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();
			AWBHeader.Populate();

			AssertNoMessageError(AWBHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertEquals("997755331", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("NIT", AWBHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateConsigneeAddress_ShouldUseNITOnTaxInfoForDirectConsolToColombia()
		{
			SetupAndAssertShouldUseNITOnTaxInfoForConsolToColombia(Core.Constants.AgentType.Direct);
		}

		public void TestPopulateConsigneeAddress_ShouldUseNITOnTaxInfoForIndirectConsolToColombia()
		{
			SetupAndAssertShouldUseNITOnTaxInfoForConsolToColombia(Core.Constants.AgentType.Other);
		}

		void SetupAndAssertShouldUseNITOnTaxInfoForConsolToColombia(string agentType)
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Colombia, CountryCodes.Colombia, 1, "NIT", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = agentType;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CO8SG";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Colombia Consignee";
			consignee.OH_RL_NKClosestPort = "CO8SG";
			consignee.MainAddress.Address1 = "Unit 717";
			consignee.MainAddress.Address2 = "11 Beautiful Street";
			consignee.MainAddress.City = "Bogota";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "CO";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			if (agentType == Core.Constants.AgentType.Direct)
			{
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			}
			else
			{
				consol.JK_OA_ReceivingForwarderAddress = consignee.MainAddress.PK;
			}
			AWBHeader.SetConsol(consol);
			Factory.Save();

			AWBHeader.Populate();

			string expectedWarningMessage = "NIT is required for Colombia imports.";
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoInfo, expectedWarningMessage);

			var nitCode = consignee.CustomsCodes.AddNew();
			nitCode.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Colombia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();
			AWBHeader.Populate();

			AssertNoMessageError(AWBHeader.EH_ConsigneeTraderNoInfo, expectedWarningMessage);
			AssertEquals(nitCode.OK_CustomsRegNo, AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, AWBHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateConsignorAddress_ShouldUseNITOnTaxInfoForDirectConsolFromColombia()
		{
			SetupAndAssertShouldUseNITOnTaxInfoForConsolFromColombia(Core.Constants.AgentType.Direct);
		}

		public void TestPopulateConsignorAddress_ShouldUseNITOnTaxInfoForIndirectConsolFromColombia()
		{
			SetupAndAssertShouldUseNITOnTaxInfoForConsolFromColombia(Core.Constants.AgentType.Agent);
		}

		void SetupAndAssertShouldUseNITOnTaxInfoForConsolFromColombia(string agentType)
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Colombia, CountryCodes.Colombia, 1, "NIT", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = agentType;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "CO8SG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Colombia Consignor";
			consignor.OH_RL_NKClosestPort = "CO8SG";
			consignor.MainAddress.Address1 = "Unit 717";
			consignor.MainAddress.Address2 = "11 Beautiful Street";
			consignor.MainAddress.City = "Bogota";
			consignor.MainAddress.Postcode = "2215";
			consignor.MainAddress.OA_RN_NKCountryCode = "CO";

			if (agentType == Core.Constants.AgentType.Direct)
			{
				shipment.ConsignorPK = consignor.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			}
			else
			{
				consol.JK_OA_SendingForwarderAddress = consignor.MainAddress.PK;
			}
			AWBHeader.SetConsol(consol);
			Factory.Save();

			AWBHeader.Populate();

			string expectedWarningMessage = "NIT is required for Colombia exports.";
			AssertHasWarning(AWBHeader.EH_ShipperTraderNoInfo, expectedWarningMessage);

			var nitCode = consignor.CustomsCodes.AddNew();
			nitCode.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Colombia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();
			AWBHeader.Populate();

			AssertNoMessageError(AWBHeader.EH_ShipperTraderNoInfo, expectedWarningMessage);
			AssertEquals(nitCode.OK_CustomsRegNo, AWBHeader.EH_ShipperTraderNo);
			AssertEquals(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, AWBHeader.EH_ShipperTraderNoType);
		}

		public void TestPopulateConsigneeAddress_ShouldNotHaveNITForNonDirectConsolToBolivia()
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Bolivia, CountryCodes.Bolivia, 1, "NIT", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BOLPB";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bolivia Consignee";
			consignee.OH_RL_NKClosestPort = "BOLPB";
			consignee.MainAddress.Address1 = "Unit 717";
			consignee.MainAddress.Address2 = "11 Beautiful Street";
			consignee.MainAddress.City = "La Paz";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BO";

			var nitCode = consignee.CustomsCodes.AddNew();
			nitCode.OK_CodeType = OrgCusCode.BoliviaCodeTypes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bolivia;
			nitCode.OK_CustomsRegNo = "997755331";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			AWBHeader.SetConsol(consol);
			Factory.Save();
			AWBHeader.Populate();

			AssertEquals("", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("", AWBHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateShipperAddress_ShouldUseNITOnTaxInfoForDirectConsolFromBolivia()
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Bolivia, CountryCodes.Bolivia, 1, "NIT", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "BOLPB";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_FullName = "Bolivia Shipper";
			shipper.OH_RL_NKClosestPort = "BOLPB";
			shipper.MainAddress.Address1 = "Unit 717";
			shipper.MainAddress.Address2 = "11 Beautiful Street";
			shipper.MainAddress.City = "La Paz";
			shipper.MainAddress.Postcode = "2215";
			shipper.MainAddress.OA_RN_NKCountryCode = "BO";

			shipment.ConsignorPK = shipper.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;
			AWBHeader.SetConsol(consol);
			Factory.Save();

			AWBHeader.Populate();

			string expectedErrorMessageShipper = "The Consignor's NIT number is required for shipments from Bolivia.";
			AssertHasMessageError(AWBHeader.EH_ShipperTraderNoInfo, expectedErrorMessageShipper);

			var nitCode = shipper.CustomsCodes.AddNew();
			nitCode.OK_CodeType = OrgCusCode.BoliviaCodeTypes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bolivia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();
			AWBHeader.Populate();

			AssertNoMessageError(AWBHeader.EH_ShipperTraderNoInfo, expectedErrorMessageShipper);
			AssertEquals("997755331", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("NIT", AWBHeader.EH_ShipperTraderNoType);
		}

		public void TestPopulateShipperAddress_ShouldNotHaveNITForNonDirectConsolFromBolivia()
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Bolivia, CountryCodes.Bolivia, 1, "NIT", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "BOLPB";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_FullName = "Bolivia Shipper";
			shipper.OH_RL_NKClosestPort = "BOLPB";
			shipper.MainAddress.Address1 = "Unit 717";
			shipper.MainAddress.Address2 = "11 Beautiful Street";
			shipper.MainAddress.City = "La Paz";
			shipper.MainAddress.Postcode = "2215";
			shipper.MainAddress.OA_RN_NKCountryCode = "BO";

			shipment.ConsignorPK = shipper.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;
			AWBHeader.SetConsol(consol);

			var nitCode = shipper.CustomsCodes.AddNew();
			nitCode.OK_CodeType = OrgCusCode.BoliviaCodeTypes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bolivia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();
			AWBHeader.Populate();

			AssertEquals("", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("", AWBHeader.EH_ShipperTraderNoType);
		}

		public void TestPopulateConsigneeAddress_ShouldHaveNoErrorsForNonDirectConsolToBolivia()
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Bolivia, CountryCodes.Bolivia, 1, "NIT", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BOLPB";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bolivia Consignee";
			consignee.OH_RL_NKClosestPort = "BOLPB";
			consignee.MainAddress.Address1 = "Unit 717";
			consignee.MainAddress.Address2 = "11 Beautiful Street";
			consignee.MainAddress.City = "La Paz";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BO";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			AWBHeader.SetConsol(consol);
			Factory.Save();

			AWBHeader.Populate();

			AssertNoMessageErrors(AWBHeader.EH_ConsigneeTraderNoInfo);
			AssertNoWarnings(AWBHeader.EH_ConsigneeTraderNoInfo);
		}

		public void TestPopulateShipperAddress_ShouldHaveNoErrorsForNonDirectConsolFromBolivia()
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Bolivia, CountryCodes.Bolivia, 1, "NIT", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "BOLPB";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_FullName = "Bolivia Shipper";
			shipper.OH_RL_NKClosestPort = "BOLPB";
			shipper.MainAddress.Address1 = "Unit 717";
			shipper.MainAddress.Address2 = "11 Beautiful Street";
			shipper.MainAddress.City = "La Paz";
			shipper.MainAddress.Postcode = "2215";
			shipper.MainAddress.OA_RN_NKCountryCode = "BO";

			shipment.ConsignorPK = shipper.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;
			AWBHeader.SetConsol(consol);
			Factory.Save();

			AWBHeader.Populate();

			AssertNoMessageErrors(AWBHeader.EH_ShipperTraderNoInfo);
			AssertNoWarnings(AWBHeader.EH_ShipperTraderNoInfo);
		}

		public void TestPopulateConsigneeAddress_ShouldUseBINOnTaxInfoForBangladeshDirectConsol()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "BIN", ZString.Empty, "AWB");

			CreateRefDocOrgCusCode("AIN", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "AIN", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BDDAC";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bangladesh Consignee";
			consignee.OH_RL_NKClosestPort = "BDDAC";
			consignee.MainAddress.Address1 = "Unit 155";
			consignee.MainAddress.Address2 = "55 Lost Lane";
			consignee.MainAddress.City = "Bangladesh City";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BD";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			AWBHeader.SetConsol(consol);
			Factory.Save();

			AWBHeader.Populate();

			string expectedErrorMessageConsignee = "The Consignee VAT (BIN Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
			string expectedErrorMessageNotifyParty = "The Notify Party VAT (BIN Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
			AssertHasMessageError(AWBHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertHasMessageError(AWBHeader.EH_AlsoNotifyTraderNoInfo, expectedErrorMessageNotifyParty);

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			ainCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			ainCode.OK_CustomsRegNo = "123AIN";
			var vatCode = consignee.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			vatCode.OK_CustomsRegNo = "123BIN";

			Factory.Save();
			AWBHeader.Populate();

			AssertNoMessageError(AWBHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertNoMessageError(AWBHeader.EH_AlsoNotifyTraderNoInfo, expectedErrorMessageNotifyParty);
			AssertEquals("123BIN", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("BIN", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("123BIN", AWBHeader.EH_AlsoNotifyTraderNo);
			AssertEquals("BIN", AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("BD", AWBHeader.EH_AlsoNotifyTraderNoCountryCode);
		}

		public void TestPopulateConsigneeAddress_ShouldUseRTNOnTaxInfoForDirectConsolToHonduras()
		{
			CreateRefDocOrgCusCode("RTN", CountryCodes.Honduras, CountryCodes.Honduras, 1, "RTN", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HNTGU";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Honduras Consignee";
			consignee.OH_RL_NKClosestPort = "HNTGU";
			consignee.MainAddress.Address1 = "Unit 717";
			consignee.MainAddress.Address2 = "11 Beautiful Street";
			consignee.MainAddress.City = "Honduras City";
			consignee.MainAddress.Postcode = "32110";
			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Honduras;

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			Factory.Save();
			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			var expectedErrorMessageConsignee = "The Consignee's RTN number is required for inbound shipments to Honduras.";
			AssertHasMessageError(AWBHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);

			var rtnCode = consignee.CustomsCodes.AddNew();
			rtnCode.OK_CodeType = OrgCusCode.HondurasCodeTypes.RTN;
			rtnCode.OK_RN_NKCodeCountry = CountryCodes.Honduras;
			rtnCode.OK_CustomsRegNo = "5678999";
			Factory.Save();
			AWBHeader.Populate();

			AssertNoMessageError(AWBHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertEquals("5678999", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("RTN", AWBHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateConsignorAddress_ShouldNotUseRTNOnTaxInfoForDirectConsolFromHonduras()
		{
			CreateRefDocOrgCusCode("RTN", CountryCodes.Honduras, CountryCodes.Honduras, 1, "RTN", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "HNTGU";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Honduras Consignor";
			consignor.OH_RL_NKClosestPort = "HNTGU";
			consignor.MainAddress.Address1 = "Unit 717";
			consignor.MainAddress.Address2 = "11 Beautiful Street";
			consignor.MainAddress.City = "Honduras City";
			consignor.MainAddress.Postcode = "32110";
			consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.Honduras;

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignor.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			AWBHeader.SetConsol(consol);

			var rtnCode = consignor.CustomsCodes.AddNew();
			rtnCode.OK_CodeType = OrgCusCode.HondurasCodeTypes.RTN;
			rtnCode.OK_RN_NKCodeCountry = CountryCodes.Honduras;
			rtnCode.OK_CustomsRegNo = "5678999";
			Factory.Save();
			AWBHeader.Populate();

			AssertEquals(string.Empty, AWBHeader.EH_ShipperTraderNo);
			AssertEquals(string.Empty, AWBHeader.EH_ShipperTraderNoType);
		}

		public void TestAlsoNotifyTraderNoHasNoCountryCodePrefix()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "FRPAR";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport = AWBHeader.Consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "DEHAM";
			transport.JW_RL_NKDiscPort = "FRPAR";

			var notifyParty = AWBHeader.Consol.Shipments[0].DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;

			var customCode = notifyParty.Organisation.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			customCode.OK_CustomsRegNo = "XI123456789";

			AWBHeader.Populate();
			AssertEquals("XI123456789", AWBHeader.EH_AlsoNotifyTraderNo);

			customCode.OK_CustomsRegNo = "A123456789";
			AWBHeader.Populate();
			AssertEquals("DEA123456789", AWBHeader.EH_AlsoNotifyTraderNo);

			customCode.OK_CustomsRegNo = "2";
			AWBHeader.Populate();
			AssertEquals("DE2", AWBHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestVATTaxNumberRequired_Direct_DischargeInIL_Consignee()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Israel, CountryCodes.Israel, 1, "VAT", "VAT", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "IL2LL";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "IL2LL";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);

			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Israel;
			code1.OK_CustomsRegNo = "12345";
			AWBHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.VATCode, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("12345", AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestVATTaxNumber_NonDirect_DischargeInIL_Consignee()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Israel, CountryCodes.Israel, 1, "VAT", "VAT", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "IL2LL";

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "IL2LL";

			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			var consignee = AWBHeader.Consol.ReceivingForwarder;
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);

			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Israel;
			code1.OK_CustomsRegNo = "12345";
			AWBHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.VATCode, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("12345", AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestVATTaxNumberRequired_Direct_DischargeInIL_AlsoNotify()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Israel, CountryCodes.Israel, 1, "VAT", "VAT", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "IL2LL";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "IL2LL";

			var notifyParty = AWBHeader.Consol.Shipments[0].DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNo);
			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNoType);

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Israel;
			code1.OK_CustomsRegNo = "678910";
			AWBHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.VATCode, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("678910", AWBHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestVATTaxNumber_NonDirect_DischargeInIL_AlsoNotify()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Israel, CountryCodes.Israel, 1, "VAT", "VAT", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "IL2LL";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "IL2LL";

			var notifyParty = AWBHeader.Consol.NotifyPartyDocumentaryAddress;
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNo);
			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNoType);

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Israel;
			code1.OK_CustomsRegNo = "678910";
			AWBHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.VATCode, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("678910", AWBHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestCCCTaxNumber_NonDirect_DischargeInCA_AlsoNotify()
		{
			CreateRefDocOrgCusCode("CCC", CountryCodes.Canada, CountryCodes.Canada, 1, "CCC", "CCC", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "CA2KS";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "CA2KS";

			var notifyParty = AWBHeader.Consol.NotifyPartyDocumentaryAddress;
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNo);
			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNoType);

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			code1.OK_CustomsRegNo = "678910";
			AWBHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("678910", AWBHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestCCCTaxNumber_NonDirect_DischargeInCA_Consignee()
		{
			CreateRefDocOrgCusCode("CCC", CountryCodes.Canada, CountryCodes.Canada, 1, "CCC", "CCC", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "CA2KS";

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "CA2KS";

			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			var consignee = AWBHeader.Consol.ReceivingForwarder;
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);

			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			code1.OK_CustomsRegNo = "12345";
			AWBHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("12345", AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestCCCTaxNumber_NonDirect_LoadInCA_Consignee()
		{
			CreateRefDocOrgCusCode("CCC", CountryCodes.Canada, CountryCodes.Canada, 1, "CCC", "CCC", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;
			AWBHeader.Consol.JK_RL_NKLoadPort = "CA2KS";
			AWBHeader.Consol.JK_RL_NKDischargePort = "AUSYD";

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "CA2KS";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			var consignee = AWBHeader.Consol.ReceivingForwarder;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);

			code1.OK_CustomsRegNo = "12345";
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestCCCTaxNumber_NonDirect_LoadInCA_Consignee_NonCarrier()
		{
			CreateRefDocOrgCusCode("RUC", CountryCodes.Peru, CountryCodes.Peru, 1, "RUC", "RUC", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;
			AWBHeader.Consol.JK_RL_NKLoadPort = "CA2KS";
			AWBHeader.Consol.JK_RL_NKDischargePort = "PELIM";

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "CA2KS";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "PELIM";

			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			var consignee = AWBHeader.Consol.ReceivingForwarder;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Peru;
			code1.OK_CustomsRegNo = "12345";
			AWBHeader.Populate();

			AssertEquals(OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("12345", AWBHeader.EH_ConsigneeTraderNo);
		}

		public void TestCCCTaxNumber_NonDirect_LoadInCA_AlsoNotify()
		{
			CreateRefDocOrgCusCode("CCC", CountryCodes.Canada, CountryCodes.Canada, 1, "CCC", "CCC", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;
			AWBHeader.Consol.JK_RL_NKLoadPort = "CA2KS";
			AWBHeader.Consol.JK_RL_NKDischargePort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "CA2KS";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = AWBHeader.Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			var notifyParty = AWBHeader.Consol.NotifyPartyDocumentaryAddress;
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNo);
			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNoType);

			code1.OK_CustomsRegNo = "678910";
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals(ZString.Empty, AWBHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestEORITaxNumber_OneDischargeLegInEU_WithPriorityOverDocumentOrgRegMapping()
		{
			CreateRefDocOrgCusCode("UST", CountryCodes.Germany, CountryCodes.Germany, 1, "UST", "UST", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "DEHAM";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();
			AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("DE123456789", AWBHeader.EH_ConsigneeTraderNo);
		}

		void SetupNorthernIrelandZone()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "NORTHERN IRELAND";
			}
		}

		public void TestGeneralEORITaxNumber_OneDischargeLegInEU_AnyEU_GBNorthernIreland_NO_CH_Zones()
		{
			CreateRefDocOrgCusCode("UST", CountryCodes.Germany, CountryCodes.Germany, 1, "UST", "UST", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "DEHAM";

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";

			GeneralEORITaxNumber_OneDischargeLegInEU_AnyEU_GBNorthernIreland_NO_CH_Zones();

			AWBHeader.Consol.JK_RL_NKDischargePort = "NOOSL";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "NOOSL";
			GeneralEORITaxNumber_OneDischargeLegInEU_AnyEU_GBNorthernIreland_NO_CH_Zones();

			AWBHeader.Consol.JK_RL_NKDischargePort = "CHBSL";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "CHBSL";
			GeneralEORITaxNumber_OneDischargeLegInEU_AnyEU_GBNorthernIreland_NO_CH_Zones();

			SetupNorthernIrelandZone();
			AWBHeader.Consol.JK_RL_NKDischargePort = "GBBEL";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "GBBEL";
			GeneralEORITaxNumber_OneDischargeLegInEU_AnyEU_GBNorthernIreland_NO_CH_Zones();

			void GeneralEORITaxNumber_OneDischargeLegInEU_AnyEU_GBNorthernIreland_NO_CH_Zones()
			{
				var consignee = AWBHeader.Consol.Shipments[0].Consignee;
				var code1 = consignee.CustomsCodes.AddNew();
				code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
				code1.OK_CustomsRegNo = "123456789";

				AWBHeader.Populate();
				AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals("CH123456789", AWBHeader.EH_ConsigneeTraderNo);
				AssertEquals("CH", AWBHeader.EH_ConsigneeTraderNoCountryCode);

				code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Norway;

				AWBHeader.Populate();
				AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals("NO123456789", AWBHeader.EH_ConsigneeTraderNo);
				AssertEquals("NO", AWBHeader.EH_ConsigneeTraderNoCountryCode);

				code1.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;

				AWBHeader.Populate();
				AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, AWBHeader.EH_ConsigneeTraderNoType);
				AssertEquals("GB123456789", AWBHeader.EH_ConsigneeTraderNo);
				AssertEquals("GB", AWBHeader.EH_ConsigneeTraderNoCountryCode);
				consignee.CustomsCodes.RemoveAll();
			}
		}

		public void TestShipperTraderNoTypeIsEORIAndIsImport2ICSMember()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "DEHAM";

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";

			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			AWBHeader.Populate();

			AssertEquals("ShipperTraderNoType", "EOR", AWBHeader.EH_ShipperTraderNoType);
			AssertEquals("TAX number with prefix", "DE123456789", AWBHeader.EH_ShipperTraderNo);
		}

		public void TestVATNumberRequired_Load_ID()
		{
			CreateRefDocOrgCusCode("PPN", CountryCodes.Indonesia, CountryCodes.Indonesia, 1, "NPWP", "NPWP tax identification number", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "ID6DI";
			AWBHeader.Consol.Shipments[0].Consignor.MainAddress.OA_RN_NKCountryCode = "ID";
			AWBHeader.Populate();

			AssertEquals(ZString.Empty, AWBHeader.EH_ShipperTraderNoType);
			AssertHasWarning(AWBHeader.EH_ShipperTraderNoInfo, "Shipper PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");

			AWBHeader.EH_ShipperTraderNo = "12364";
			AssertNoWarning(AWBHeader.EH_ShipperTraderNoInfo, "Shipper PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");
		}

		public void TestVATNumberRequired_Discharge_KE()
		{
			CreateRefDocOrgCusCode("PIN", CountryCodes.Kenya, CountryCodes.Kenya, 1, "PIN", "Personal Identification Number", "AWB");

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKDischargePort = "KEARI";
			AWBHeader.Consol.Shipments[0].Consignee.MainAddress.OA_RN_NKCountryCode = "KE";
			AWBHeader.Populate();

			AssertEquals("No Tax type should be displayed when no tax number.", ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "PIN is required for Kenya imports.");

			AWBHeader.EH_ConsigneeTraderNo = "12364";
			AssertNoWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "PIN is required for Kenya imports.");
		}

		public void TestCUITaxNumber_DirectConsol_Discharge_Argentina()
		{
			CreateRefDocOrgCusCode("CUI", CountryCodes.Argentina, CountryCodes.Argentina, 1, "CUIT", "Tax Identification Number", "AWB");
			AWBHeader.Consol.JK_AgentType = AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKDischargePort = "ARBUE";
			var consignee = AWBHeader.Consol.Shipments[0].Consignee;
			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Argentina;
			AWBHeader.Populate();

			AssertEquals("No Tax type should be displayed when no tax number.", ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "CUI (CUIT) is required for Argentina imports.");
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoTypeInfo, "CUI (CUIT) is required for Argentina imports.");

			var taxCode = consignee.CustomsCodes.AddNew();
			taxCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			taxCode.OK_RN_NKCodeCountry = CountryCodes.Argentina;
			taxCode.OK_CustomsRegNo = "12364";
			AWBHeader.Populate();

			AssertEquals("CUIT", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("12364", AWBHeader.EH_ConsigneeTraderNo);
			AssertNoWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "CUI (CUIT) is required for Argentina imports.");
			AssertNoWarning(AWBHeader.EH_ConsigneeTraderNoTypeInfo, "CUI (CUIT) is required for Argentina imports.");
		}

		public void TestCUITaxNumber_AgentConsol_Discharge_Argentina()
		{
			CreateRefDocOrgCusCode("CUI", CountryCodes.Argentina, CountryCodes.Argentina, 1, "CUIT", "Tax Identification Number", "AWB");

			AWBHeader.Consol.JK_AgentType = AgentType.Agent;
			AWBHeader.Consol.JK_RL_NKDischargePort = "ARBUE";
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.MainAddress.OA_RN_NKCountryCode = CountryCodes.Argentina;
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;
			AWBHeader.Populate();

			AssertEquals("No Tax type should be displayed when no tax number.", ZString.Empty, AWBHeader.EH_ConsigneeTraderNoType);
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "CUI (CUIT) is required for Argentina imports.");
			AssertHasWarning(AWBHeader.EH_ConsigneeTraderNoTypeInfo, "CUI (CUIT) is required for Argentina imports.");

			var taxCode = agent.CustomsCodes.AddNew();
			taxCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			taxCode.OK_RN_NKCodeCountry = CountryCodes.Argentina;
			taxCode.OK_CustomsRegNo = "12364";
			AWBHeader.Populate();

			AssertEquals("CUIT", AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("12364", AWBHeader.EH_ConsigneeTraderNo);
			AssertNoWarning(AWBHeader.EH_ConsigneeTraderNoInfo, "CUI (CUIT) is required for Argentina imports.");
			AssertNoWarning(AWBHeader.EH_ConsigneeTraderNoTypeInfo, "CUI (CUIT) is required for Argentina imports.");
		}

		public void TestPopulateSpecialHandling_EAWBAgreement()
		{
			AWBHeader.Populate();
			AssertEquals("AWBHeader.AWBSpecialHandlingItems.Count", 0, AWBHeader.AWBSpecialHandlingItems.Count);

			var consolSpecialHandlingItem = AWBHeader.Consol.AWBSpecialHandlingItems.AddNew();
			consolSpecialHandlingItem.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments;
			AWBHeader.Populate();
			AssertEquals("AWBHeader.AWBSpecialHandlingItems.Count", 1, AWBHeader.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be EFreightConsignmentWithNoAccompanyingPaperDocuments"
				, AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments
				, AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

			consolSpecialHandlingItem.JKH_Code = Constants.EFreightStatus.Code.ECC;
			AWBHeader.Populate();
			AssertEquals("AWBHeader.AWBSpecialHandlingItems.Count", 1, AWBHeader.AWBSpecialHandlingItems.Count);
			AssertEquals("Special Handling Item should be default from By1stCarrier's rule at first"
				, Constants.EFreightStatus.Code.ECC
				, AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
		}

		public void TestPopulateSpecialHandling_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				AWBHeader.Consol.JK_TransportMode = TransportModes.Sea;
				AWBHeader.Consol.Transports[0].JW_TransportMode = TransportModes.Sea;
				Factory.Save();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("No default security status special handling items on non-Air transport mode.", 0, AWBHeader.AWBSpecialHandlingItems.Count);

				AWBHeader.Consol.JK_TransportMode = TransportModes.Air;
				AWBHeader.Consol.Shipments.RemoveAll();
				var transport = AWBHeader.Consol.Transports[0];
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_RL_NKLoadPort = "USLAX";
				transport.JW_RL_NKDiscPort = "DEFRA";
				transport.JW_IsCargoOnly = false;
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("No default security status special handling items, when there are no shipments.", 0, AWBHeader.AWBSpecialHandlingItems.Count);

				transport.JW_RL_NKLoadPort = "DEHAM";
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("No default security status special handling items, when there are no shipments.", 0, AWBHeader.AWBSpecialHandlingItems.Count);

				SetUpForDeparturePort("DEFRA");
				AWBHeader.Consol.Transports[0].JW_IsCargoOnly = false;
				AWBHeader.Consol.Shipments.RemoveAll();

				var shipment1 = AWBHeader.Consol.Shipments.AddNew();
				shipment1.JS_InspectionTypeCode = "XRY";
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "DEFRA";
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("One default security status special handling items, when shipment inspection is not unknown.", 1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Security status should default to 'SPX'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				transport = AWBHeader.Consol.Transports[0];
				transport.JW_RL_NKLoadPort = "USLAX";
				transport.JW_RL_NKDiscPort = "TRANK";
				var transport2 = AWBHeader.Consol.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "TRANK";
				transport2.JW_RL_NKDiscPort = "DEFRA";
				transport2.JW_TransportMode = Constants.TransportModes.Road;
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("One default security status special handling item.", 1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Security status should default to 'SPX'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				transport2.JW_TransportMode = Constants.TransportModes.Air;
				var shipment2 = AWBHeader.Consol.Shipments.AddNew();
				shipment2.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("One default security status special handling items, when shipment inspection is unknown.", 1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Security status should default to 'NSC'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				shipment2.JS_InspectionTypeCode = "APP";
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("One default security status special handling items, when all shipments inspection is not unknown.", 1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Security status should default to 'SPX'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
			}
		}

		public void TestPopulateSpecialHandling_EU_HighRisk()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				SetUpForDeparturePort("DEFRA");
				AWBHeader.Consol.JK_TransportMode = TransportModes.Air;
				AWBHeader.Consol.Shipments.RemoveAll();
				AWBHeader.Consol.Transports[0].JW_IsCargoOnly = false;
				AWBHeader.Consol.Shipments.RemoveAll();

				var shipment = AWBHeader.Consol.Shipments.AddNew();
				shipment.JS_InspectionTypeCode = "XRY";
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_IsHighRisk = true;

				AssertEquals("Precondition: JS_AdditionalInspectionTypeCode", "UNK", shipment.JS_AdditionalInspectionTypeCode);

				Factory.Save();
				AWBHeader.Populate();

				AssertEquals("Security status should default to 'NSC'.", "NSC", AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				shipment.JS_AdditionalInspectionTypeCode = "PHS";

				Factory.Save();
				AWBHeader.Populate();

				AssertEquals("Security status should default to 'SHR'.", "SHR", AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
			}
		}

		public void TestGetAviationSecurityCodeDefaultToSPXWhenAtleastOneSPXAndMoreThanOneSHR()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				SetUpForDeparturePort("DEFRA");
				AWBHeader.Consol.JK_TransportMode = TransportModes.Air;
				AWBHeader.Consol.Shipments.RemoveAll();
				AWBHeader.Consol.Transports[0].JW_IsCargoOnly = false;
				AWBHeader.Consol.Shipments.RemoveAll();

				var shipment1 = AWBHeader.Consol.Shipments.AddNew();
				shipment1.JS_InspectionTypeCode = "XRY";
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_IsHighRisk = true;
				shipment1.JS_AdditionalInspectionTypeCode = "PHS";

				var shipment2 = AWBHeader.Consol.Shipments.AddNew();
				shipment2.JS_InspectionTypeCode = "XRY";
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_RL_NKOrigin = "DEFRA";
				shipment2.JS_IsHighRisk = false;

				AssertEquals("Precondition: JS_AdditionalInspectionTypeCode", "UNK", shipment2.JS_AdditionalInspectionTypeCode);

				Factory.Save();
				AWBHeader.Populate();

				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("Security status should default to 'SPX'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				shipment2.JS_AdditionalInspectionTypeCode = "PHS";
				shipment2.JS_IsHighRisk = true;
				Factory.Save();
				AWBHeader.Populate();

				AssertEquals("Security status should default to 'SHR'.", "SHR", AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
			}
		}

		public void TestPopulateSpecialHandling_UK_SPX()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				AWBHeader.Consol.JK_TransportMode = TransportModes.Sea;
				AWBHeader.Consol.Transports[0].JW_TransportMode = TransportModes.Sea;
				Factory.Save();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("No default security status special handling items on non-Air transport mode.", 0, AWBHeader.AWBSpecialHandlingItems.Count);

				AWBHeader.Consol.JK_TransportMode = TransportModes.Air;
				AWBHeader.Consol.Shipments.RemoveAll();
				var transport = AWBHeader.Consol.Transports[0];
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_RL_NKLoadPort = "USLAX";
				transport.JW_RL_NKDiscPort = "GBLON";
				transport.JW_IsCargoOnly = true;
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("No default security status special handling items, when there are no shipments.", 0, AWBHeader.AWBSpecialHandlingItems.Count);

				transport.JW_RL_NKLoadPort = "GBMAN";
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("No default security status special handling items, when there are no shipments.", 0, AWBHeader.AWBSpecialHandlingItems.Count);

				SetUpForDeparturePort("GBLON");
				AWBHeader.Consol.Transports[0].JW_IsCargoOnly = false;
				AWBHeader.Consol.Shipments.RemoveAll();

				var shipment1 = AWBHeader.Consol.Shipments.AddNew();
				shipment1.JS_InspectionTypeCode = "XRY";
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "GBLON";
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("SPX status does not default in the UK", 0, AWBHeader.AWBSpecialHandlingItems.Count);

				var shipment2 = AWBHeader.Consol.Shipments.AddNew();
				shipment2.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("One default security status special handling items, when shipment inspection is unknown.", 1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Security status should default to 'NSC'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				shipment2.JS_InspectionTypeCode = "APP";
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("SPX does not default in the UK for approved shipments.", 0, AWBHeader.AWBSpecialHandlingItems.Count);
			}
		}

		public void TestPopulateSpecialHandling_UK_SCO()
		{
			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			((ShipmentInspectionType)inspectionTypes.Types.FindByCode("NUC")).AllowedOnPassengerFlights = false;

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				SetUpForDeparturePort("GBLON");

				AWBHeader.Consol.JK_RL_NKLoadPort = "CHGVA";
				AWBHeader.EH_GS_NKSecurityStatusIssuedByCode = "E";
				var shipment1 = AWBHeader.Consol.Shipments[0];
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "GBLON";
				shipment1.JS_RL_NKDestination = "USLAX";

				var transport = AWBHeader.Consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "CHGVA";
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_IsCargoOnly = false;

				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("CH"))
				{
					shipment1.JS_InspectionTypeCode = "NUC";
					AWBHeader.Populate();

					AssertEquals("SCO defaults for consols loading in Switzerland", "SCO", AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
				}

				AWBHeader.Consol.JK_RL_NKLoadPort = "GBLHR";
				transport.JW_RL_NKLoadPort = "GBLHR";

				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
				{
					shipment1.JS_InspectionTypeCode = "NUC";
					AWBHeader.Populate();

					AssertEquals("SCO does not default for consols loading in the UK", "NSC", AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
				}
			}
		}

		public void TestPopulateSpecialHandling_JP()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				SetUpForDeparturePort("JPADO");

				AWBHeader.Consol.JK_TransportMode = Constants.TransportModes.Air;
				AWBHeader.Consol.Shipments.RemoveAll();

				var transport = AWBHeader.Consol.Transports[0];
				transport.JW_TransportMode = "AIR";
				transport.JW_RL_NKLoadPort = "JPOSA";
				transport.JW_RL_NKDiscPort = "USLAX";
				transport.JW_IsCargoOnly = false;
				transport.JW_ETA = ZDate.Today.AddDays(1);

				var shipment1 = AWBHeader.Consol.Shipments.AddNew();
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "JPADO";
				shipment1.JS_InspectionTypeCode = "XRY";
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("One default security status special handling items, when shipment inspection is not unknown.", 1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Security status should default to 'SPX'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_RL_NKDiscPort = "CNSHA";

				var transport2 = AWBHeader.Consol.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "CNSHA";
				transport2.JW_RL_NKDiscPort = "USLAX";
				transport2.JW_TransportMode = Constants.TransportModes.Air;
				transport2.JW_ETA = ZDate.Today.AddDays(2);
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("No default security status special handling items, when the consol is not leaving JP by Air.", 0, AWBHeader.AWBSpecialHandlingItems.Count);

				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_TransportType = "FL1";
				transport2.JW_TransportType = "FL2";

				var shipment2 = AWBHeader.Consol.Shipments.AddNew();
				shipment2.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				Factory.Save();
				AWBHeader.Populate();
				AssertEquals("No changes made to Special Handling Items on AWB.", false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals("One default security status special handling items, when shipment inspection is unknown.", 1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Security status should default to 'NSC'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
			}
		}

		public void TestPopulateSpecialHandling_CargoSecureForAllCargoAircraftOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				SetUpForDeparturePort("HKHKG");

				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_OH_OrgHeader = accountConsignor.PK;

				AWBHeader.Consol.Shipments.RemoveAll();
				var shipment1 = AWBHeader.Consol.Shipments.AddNew();
				shipment1.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment1.JS_TransportMode = Constants.TransportModes.Air;
				shipment1.JS_RL_NKOrigin = "HKHKG";
				shipment1.JS_RL_NKDestination = "DEHAM";
				shipment1.JS_InspectionTypeCode = "APP";

				var transport = AWBHeader.Consol.Transports[0];
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "DEHAM";
				transport.JW_TransportMode = Constants.TransportModes.Air;
				Factory.Save();
				AWBHeader.Populate();

				Assert("Precondition", !shipment1.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights);
				AssertEquals(false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals(1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Security status should default to 'SCO' as there is a shipment where the relevent org isn't for passenger flight shipping",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				var shipment2 = AWBHeader.Consol.Shipments.AddNew();
				shipment2.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				Factory.Save();
				AWBHeader.Populate();

				AssertEquals(false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals(1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("As soon as any shipment has a unknown security code the awb should always be 'NSC'.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				shipment2.JS_InspectionTypeCode = "APP";
				Factory.Save();
				AWBHeader.Populate();

				AssertEquals(false, AWBHeader.AWBSpecialHandlingItems.HasChanges);
				AssertEquals(1, AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("Regardless of the number of shipments, the 'SCO' special handling only requires one to be flagged as not approved for passenger flight shipping",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
					AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
			}
		}

		public void TestPopulateExportAWBSecurityStatusLines_WithIsOrgLevelApprovalSupport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				SetUpForDeparturePort("HKHKG");

				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				knownShipperDetails.OV_EXApprovalNumber = "RA1234";
				knownShipperDetails.OV_OH_OrgHeader = accountConsignor.PK;

				AWBHeader.Consol.Shipments.RemoveAll();
				var shipment1 = AWBHeader.Consol.Shipments.AddNew();
				shipment1.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment1.JS_TransportMode = Constants.TransportModes.Air;
				shipment1.JS_RL_NKOrigin = "HKHKG";
				shipment1.JS_RL_NKDestination = "DEHAM";
				shipment1.JS_InspectionTypeCode = "APP";

				var transport = AWBHeader.Consol.Transports[0];
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "DEHAM";
				transport.JW_TransportMode = Constants.TransportModes.Air;
				Factory.Save();
				shipment1.JS_InspectionTypeCode = "APP";
				AWBHeader.Populate();
				AssertEquals(1, AWBHeader.ExportAWBSecurityStatusLines.Count);
				AssertEquals("1234", AWBHeader.ExportAWBSecurityStatusLines[0].ApprovalNumber);

				var unapprovedAddress = accountConsignor.Addresses.AddNew();
				unapprovedAddress.OA_Address1 = "Address 1";

				shipment1.ConsignorDocumentaryAddress.E2_OA_Address = unapprovedAddress.PK;
				shipment1.JS_InspectionTypeCode = "APP";
				AWBHeader.Populate();
				AssertEquals(1, AWBHeader.ExportAWBSecurityStatusLines.Count);
				AssertEquals("RA", AWBHeader.ExportAWBSecurityStatusLines[0].ApprovalCategory);
				AssertEquals("1234", AWBHeader.ExportAWBSecurityStatusLines[0].ApprovalNumber);
			}
		}

		public void TestPopulateSpecialHandlingCodesFromConsolSecurityStatusCodeAndAWBSpecialHandlingItems()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			consol.SecurityStatusCode = "SCO";
			var handling1 = consol.AWBSpecialHandlingItems.AddNew();
			handling1.JKH_Code = "CAT";

			var handling2 = consol.AWBSpecialHandlingItems.AddNew();
			handling2.JKH_Code = "PEB";

			consol.AWBHeader.Populate();
			AssertContainsExactElementsInAnyOrder("Export AWB Header should copy special handling and security status from the consol.", new[] { "SCO", "CAT", "PEB" }, consol.AWBHeader.AWBSpecialHandlingItems.Cast<ExportAWBSpecialHandling>().Select(item => item.EP_SpecialHandling));
		}

		#region Extra Customizable Text

		public void TestMultilineExtraAccountionInfoMacrosSplitToMultipleLines()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_GoodsDescription = "THE\nMULTILINE GOODS" + System.Environment.NewLine + System.Environment.NewLine + "DESCRIPTION";

			FreightDataRegistry.Instance.MAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList
			{
				new CodeDescriptionPair("ABC", "FIRST DESCRIPTION"),
				new CodeDescriptionPair("BBC", "<DocConsol.DirectShipment.DetailedDescriptionOfGoods>"),
				new CodeDescriptionPair("CNN", "LAST DESCRIPTION")
			});
			ExportAWBHeader awb = consol.AWBHeader;
			awb.Populate();

			ZGuid pk = awb.AWBAccountingInformations[0].PK;

			AssertEquals(5, awb.AWBAccountingInformations.Count);
			AssertEquals("ABC", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("FIRST DESCRIPTION", awb.AWBAccountingInformations[0].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[1].EA_InformationID);
			AssertEquals("THE", awb.AWBAccountingInformations[1].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[2].EA_InformationID);
			AssertEquals("MULTILINE GOODS", awb.AWBAccountingInformations[2].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[3].EA_InformationID);
			AssertEquals("DESCRIPTION", awb.AWBAccountingInformations[3].EA_Information);
			AssertEquals("CNN", awb.AWBAccountingInformations[4].EA_InformationID);
			AssertEquals("LAST DESCRIPTION", awb.AWBAccountingInformations[4].EA_Information);

			FreightDataRegistry.Instance.MAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new CodeDescriptionPairList
					{
						new CodeDescriptionPair("ABC", "FIRST DESCRIPTION"),
						new CodeDescriptionPair("CBS", ""),
						new CodeDescriptionPair("BBC", "<DocConsol.DirectShipment.DetailedDescriptionOfGoods>"),
						new CodeDescriptionPair("CNN", "LAST DESCRIPTION")
					});

			awb.Populate();

			AssertEquals(6, awb.AWBAccountingInformations.Count);
			AssertEquals("First accounting info has not changed and should be reused", pk, awb.AWBAccountingInformations[0].PK);
			AssertEquals("ABC", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("FIRST DESCRIPTION", awb.AWBAccountingInformations[0].EA_Information);
			AssertEquals("CBS", awb.AWBAccountingInformations[1].EA_InformationID);
			AssertEquals("", awb.AWBAccountingInformations[1].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[2].EA_InformationID);
			AssertEquals("THE", awb.AWBAccountingInformations[2].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[3].EA_InformationID);
			AssertEquals("MULTILINE GOODS", awb.AWBAccountingInformations[3].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[4].EA_InformationID);
			AssertEquals("DESCRIPTION", awb.AWBAccountingInformations[4].EA_Information);
			AssertEquals("CNN", awb.AWBAccountingInformations[5].EA_InformationID);
			AssertEquals("LAST DESCRIPTION", awb.AWBAccountingInformations[5].EA_Information);
		}

		public void TestEmptyExtraTextLinesAreExcluded()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Real Handling Info");
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "XYZ";
			consol.JK_RL_NKDischargePort = "AUSYD";

			FreightDataRegistry.Instance.MAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("", "<AirportOfDestinationCode>"), new CodeDescriptionPair("BLA", "Description for the BLA") });
			ExportAWBHeader awb = consol.AWBHeader;

			awb.Populate();
			AssertEquals(2, awb.AWBAccountingInformations.Count);

			AssertEquals((ZByte)1, awb.AWBAccountingInformations[0].EA_Sequence);
			AssertEquals("", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("SYD", awb.AWBAccountingInformations[0].EA_Information);

			AssertEquals((ZByte)2, awb.AWBAccountingInformations[1].EA_Sequence);
			AssertEquals("BLA", awb.AWBAccountingInformations[1].EA_InformationID);
			AssertEquals("Description for the BLA", awb.AWBAccountingInformations[1].EA_Information);

			consol.JK_RL_NKDischargePort = ZString.Empty;

			awb.Populate();
			AssertEquals(1, awb.AWBAccountingInformations.Count);

			AssertEquals((ZByte)2, awb.AWBAccountingInformations[0].EA_Sequence);
			AssertEquals("BLA", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("Description for the BLA", awb.AWBAccountingInformations[0].EA_Information);
		}

		public void TestExtraCustomizableText()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Real Handling Info");
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "XYZ";
			consol.JK_RL_NKDischargePort = "AUSYD";

			ExportAWBHeader awb = consol.AWBHeader;
			awb.Populate();
			Assert(!awb.HasChanges);
			AssertEquals("No Accounting Infos", 0, awb.AWBAccountingInformations.Count);
			AssertEquals("Default nature and qty of goods", "Consolidation as per attached list\nNo Dimensions Available\n\n\n\n\n\n\n\n\n\n", awb.NatureAndQtyOfGoods);
			AssertEquals("No optional info", "", awb.EH_OptionalShippingInformation);
			AssertEquals("No optional info", "", awb.EH_OptionalShippingInformation2);
			AssertEquals("Handling info from note only", "Real Handling Info", awb.EH_HandlingInformation);

			FreightDataRegistry.Instance.MAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("Acc", "<AirportOfDestinationCode>") });
			FreightDataRegistry.Instance.MAWBHandlingInformationExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Handling <AirportOfDestinationText>");
			FreightDataRegistry.Instance.MAWBNatureAndQtyOfGoodsExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Nature <ServiceLevel>");
			FreightDataRegistry.Instance.MAWBOptionalShippingInfoOneExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1<CompanyCountry>");
			FreightDataRegistry.Instance.MAWBOptionalShippingInfoTwoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2<CompanyCode>");

			awb.Populate();
			Assert(!awb.HasChanges);
			AssertEquals("1 Accounting Infos", 1, awb.AWBAccountingInformations.Count);
			AssertEquals("1 Accounting Infos", (ZByte)1, awb.AWBAccountingInformations[0].EA_Sequence);
			AssertEquals("1 Accounting Infos", "Acc", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("1 Accounting Infos", "SYD", awb.AWBAccountingInformations[0].EA_Information);
			AssertEquals("Nature and qty of goods", "Consolidation as per attached list\nNature XYZ\nNo Dimensions Available\n\n\n\n\n\n\n\n\n", awb.NatureAndQtyOfGoods);
			AssertEquals("Optional info 1", "1" + GlbCompany.CurrentCompany.Country.RN_Desc, awb.EH_OptionalShippingInformation);
			AssertEquals("Optional info 2", "2" + GlbCompany.CurrentCompany.GC_Code, awb.EH_OptionalShippingInformation2);
			AssertEquals("Handling info", "Real Handling Info\r\nHandling SYDNEY", awb.EH_HandlingInformation);
		}

		public void TestExtraCustomizableText_AccountingInfoExtraText_VariableValues_DoesNotTriggerHasChanges()
		{
			Test_AccountingInfoExtraText_VariableValues("<DateTimeStart>");
			Test_AccountingInfoExtraText_VariableValues("TestWithSpaces  ");

			void Test_AccountingInfoExtraText_VariableValues(string accountingInfoExtraText)
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_AWBServiceLevel = "XYZ";
				consol.JK_RL_NKDischargePort = "AUSYD";

				FreightDataRegistry.Instance.MAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
					new CodeDescriptionPairList { new CodeDescriptionPair("", accountingInfoExtraText) });
				ExportAWBHeader awb = consol.AWBHeader;
				awb.Populate();
				Assert(!awb.HasChanges);

				awb.EH_AreRateLinesOverridden = true;
				awb.Populate();
				Assert(awb.HasChanges);

				Factory.Save();
				Assert(!awb.HasChanges);

				awb.Populate();
				Assert(!awb.HasChanges);
				AssertEquals("1 Accounting Infos", 1, awb.AWBAccountingInformations.Count);

				FreightDataRegistry.Instance.MAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("", "Test") });
				awb.Populate();
				Assert(!awb.HasChanges);
				AssertEquals("1 Accounting Infos", 1, awb.AWBAccountingInformations.Count);
			}
		}

		#endregion

		public void TestAllowRecogniseAndUpdateNatureAndQtyOfGoodsTypeFromText()
		{
			AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "VOL 1.00 M3";
			AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "VOL 1.00 M3";
			AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "VOL 1.00 M3";
			AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "VOL 1.00 M3";
			AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 IN x 4";
			AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 IN x 4";
			AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 IN x 4";
			AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 IN x 4";
			AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription = "AAA";
			AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription = "AAA";
			AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription = "AAA";
			AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription = "AAA";

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsType);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestPopulateValues()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			ForwardingShipment shipment = consol.Shipments.AddNew();

			shipment.JS_GoodsValue = 100m;
			shipment.JS_RX_NKGoodsValueCurr = "INR";

			shipment.JS_InsuranceValue = 300m;
			shipment.JS_RX_NKInsuranceCurrency = "USD";

			consol.AWBHeader.Populate();
			AssertEquals(100m, consol.AWBHeader.EH_CustomsValue);
			AssertEquals("INR", consol.AWBHeader.EH_HouseCustomsValueCurrency);
			AssertEquals(300m, consol.AWBHeader.EH_InsuranceValue);
			AssertEquals("USD", consol.AWBHeader.EH_HouseInsuranceValueCurrency);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			consol.AWBHeader.Populate();
			AssertEquals(0m, consol.AWBHeader.EH_CustomsValue);
			AssertEquals("", consol.AWBHeader.EH_HouseCustomsValueCurrency);
			AssertEquals(0m, consol.AWBHeader.EH_CustomsValue);
			AssertEquals("", consol.AWBHeader.EH_HouseInsuranceValueCurrency);
		}

		public void TestDefaultingCustomsValue_ControlledByRegistry()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_GoodsValue = 100m;

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consol.AWBHeader.Populate();
				AssertEquals("If registry is set to 'No', the CustomsValue should return 0.", 0m, consol.AWBHeader.EH_CustomsValue);
			}

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_RL_NKDischargePort = ZString.Empty;
				consol.AWBHeader.Populate();
				AssertEquals("If registry is set to 'Yes', the CustomsValue should return 100.", 100m, consol.AWBHeader.EH_CustomsValue);

				consol.JK_AgentType = AgentType.Agent;
				consol.AWBHeader.Populate();
				AssertEquals("If registry is set to 'Yes' and non-Direct Consol, the CustomsValue should return 0.", 0m, consol.AWBHeader.EH_CustomsValue);
			}
		}

		public void TestDefaultingCustomsValue_DefaultsSumOfShipmentGoodsValue_WhenBangladeshImportAndConsolIsDirect_RegardlessOfRegistryValue()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Direct;
			consol.JK_RL_NKDischargePort = "BDKHL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_GoodsValue = 1m;
			shipment.JS_RL_NKDestination = "BDKHL";

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consol.AWBHeader.Populate();
				AssertEquals("If registry is set to 'No' and Direct consol, the CustomsValue should return 1.", 1m, consol.AWBHeader.EH_CustomsValue);

				consol.JK_AgentType = AgentType.Agent;
				consol.AWBHeader.Populate();
				AssertEquals("If registry is set to 'No' and non-Direct consol, the CustomsValue should return 0.", 0m, consol.AWBHeader.EH_CustomsValue);
			}

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_AgentType = AgentType.Direct;
				consol.AWBHeader.Populate();
				AssertEquals("If registry is set to 'Yes' and Direct consol, the CustomsValue should return 1.", 1m, consol.AWBHeader.EH_CustomsValue);

				consol.JK_AgentType = AgentType.Agent;
				consol.AWBHeader.Populate();
				AssertEquals("If registry is set to 'Yes' and non-Direct consol, the CustomsValue should return 0.", 0m, consol.AWBHeader.EH_CustomsValue);
			}
		}

		public void TestNoExceptionThrownForToArraryOnGetAvailableDimensions()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();

			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				foreach (CommonShipment consolShipment in consol.ShipmentsForTotalling.ToArray())
				{
					consol.ShipmentsForTotalling.AddNew();

					foreach (PackLine packLine in consolShipment.OuterPackLines.ToArray())
					{
						consolShipment.OuterPackLines.AddNew();
					}
				}
			});
		}

		public void TestRateLineNoPieces()
		{
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 50;
			AWBHeader.Populate();
			AssertEquals("50", AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);

			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 9999;
			AWBHeader.Populate();
			AssertEquals("9999", AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);

			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 10000;
			AWBHeader.Populate();
			AssertEquals("9999", AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
		}

		#region Test PopulateOtherCharges

		public void TestPopulateOtherCharges()
		{
			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			chargeCode1.AC_Desc = "Charge Code 1";

			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.TV;
			chargeCode2.AC_Desc = "Charge Code 2";

			AccChargeCode chargeCode3 = Factory.New<AccChargeCode>();
			chargeCode3.AC_Desc = "Charge Code 3";

			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BusinessObject consolCost1 = CreateConsolCost(chargeCode1.PK, 300m, 30m);
			BusinessObject consolCost2 = CreateConsolCost(Env.Registry.FreightChargeCode, 500m, 50m);
			BusinessObject consolCost3 = CreateConsolCost(chargeCode2.PK, 100m, 10m);
			BusinessObject consolCost4 = CreateConsolCost(chargeCode3.PK, 900m, 90m);

			AWBHeader.Populate();
			AssertEquals(2, AWBHeader.AWBOtherCharges.Count);

			AssertEquals(300m, AWBHeader.AWBOtherCharges[0].EO_Amount);
			AssertEquals(Core.Constants.AWB.ChargeCodes.AC, AWBHeader.AWBOtherCharges[0].EO_ChargeCode);
			AssertEquals("Charge Code 1", AWBHeader.AWBOtherCharges[0].EO_ChargeDescription.Trim());

			AssertEquals(100m, AWBHeader.AWBOtherCharges[1].EO_Amount);
			AssertEquals(Core.Constants.AWB.ChargeCodes.TV, AWBHeader.AWBOtherCharges[1].EO_ChargeCode);
			AssertEquals("Charge Code 2", AWBHeader.AWBOtherCharges[1].EO_ChargeDescription.Trim());

			//display options
			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.TV].Entitlement = "C";
			collection[Core.Constants.AWB.ChargeCodes.AC].Visibility = "Hide";

			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = "C";
			collection[Core.Constants.AWB.ChargeCodes.TV].Visibility = "Hide";

			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertEquals("consol has no PPD CLT set so it should show both charges", 2, AWBHeader.AWBOtherCharges.Count);

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			AWBHeader.Populate();
			AssertEquals("should use PPD registry", 1, AWBHeader.AWBOtherCharges.Count);
			AssertEquals("should use PPD registry", "C", AWBHeader.AWBOtherCharges[0].EO_EntitlementCode);
			AssertEquals("should use PPD registry", "AC", AWBHeader.AWBOtherCharges[0].EO_ChargeCode);
			AssertEquals(80m, AWBHeader.EH_TaxesPPD);
			AssertEquals(0m, AWBHeader.EH_TaxesCOL);

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;

			AWBHeader.Populate();
			AssertEquals("should use CLT registry", 1, AWBHeader.AWBOtherCharges.Count);
			AssertEquals("should use CLT registry", "C", AWBHeader.AWBOtherCharges[0].EO_EntitlementCode);
			AssertEquals("should use CLT registry", "TV", AWBHeader.AWBOtherCharges[0].EO_ChargeCode);
			AssertEquals(0m, AWBHeader.EH_TaxesPPD);
			AssertEquals(60m, AWBHeader.EH_TaxesCOL);

			collection[Core.Constants.AWB.ChargeCodes.TV].Visibility = "Hide";
			collection[Core.Constants.AWB.ChargeCodes.AC].Visibility = "Hide";
			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertEquals("should use CLT registry", 0, AWBHeader.AWBOtherCharges.Count);
		}

		public void TestPopulateTaxAmount_WhenTaxIsNotOverriden()
		{
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			chargeCode.AC_Desc = "Charge Code";

			CreateConsolCost(chargeCode.PK, 300m, 30m, false);

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			AWBHeader.Populate();
			AssertEquals(57m, AWBHeader.EH_TaxesPPD);
			AssertEquals(0m, AWBHeader.EH_TaxesCOL);

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;

			AWBHeader.Populate();
			AssertEquals(0m, AWBHeader.EH_TaxesPPD);
			AssertEquals(57m, AWBHeader.EH_TaxesCOL);
		}

		public void TestPopulateOtherChargesWhenThereAreMoreThanMaxOtherChargesCount()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;
			chargeCode.AC_Desc = "Charge Code";

			for (int i = 0; i < AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB; i++)
			{
				CreateConsolCost(chargeCode.PK, (ZDecimal)(i + 1));

				AWBHeader.Populate();

				AssertEquals(i + 1, AWBHeader.AWBOtherCharges.Count);

				var expectedAmounts = new List<ZDecimal>();

				for (int j = 0; j <= i; j++)
				{
					expectedAmounts.Add(i - j + 1);
				}

				AssertContainsExactElementsInAnyOrder(expectedAmounts,
					AWBHeader.AWBOtherCharges.Cast<ExportAWBOtherCharges>().Select(charge => charge.EO_Amount));

				AssertEquals("Charge Code", AWBHeader.AWBOtherCharges[i].EO_ChargeDescription.Trim());
				AssertEquals(Core.Constants.AWB.ChargeCodes.DB, AWBHeader.AWBOtherCharges[i].EO_ChargeCode);
			}

			for (int i = 0; i < 5; i++)
			{
				CreateConsolCost(chargeCode.PK, 10m);
			}

			AWBHeader.Populate();

			AssertEquals("Should not exceed MaxOtherChargesCount", AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB, AWBHeader.AWBOtherCharges.Count);
			AssertEquals("Only one misc charge", 1, AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>().Count(charge => charge.EO_ChargeCode == Core.Constants.AWB.ChargeCodes.MB));

			var lastOtherCharge = AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.First(charge => charge.EO_ChargeCode == Core.Constants.AWB.ChargeCodes.MB);

			AssertEquals("Other Misc Charges", lastOtherCharge.EO_ChargeDescription);
			AssertEquals("Amount should be accumulated", (ZDecimal)(6 + AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB), lastOtherCharge.EO_Amount);
		}

		BusinessObject CreateConsolCost(ZGuid chargeCodePK, ZDecimal amount)
		{
			return CreateConsolCost(chargeCodePK, amount, 0m);
		}

		BusinessObject CreateConsolCost(ZGuid chargeCodePK, ZDecimal amount, ZDecimal taxAmount, bool overrideTax = true)
		{
			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCodePK;
			consolCost[JobConsolCostSchema.E6_LocalCostAmount] = amount;
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost.SetContext(Enterprise.Integration.Accounting.BusinessContext.EnableDirectSettingConsolCostParent);

			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = AWBHeader.Consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(Enterprise.Integration.Accounting.BusinessContext.EnableDirectSettingConsolCostParent);
			}

			if (overrideTax)
			{
				consolCost[JobConsolCostSchema.E6_IsTaxAmountOverridden] = true;
				consolCost["E6_OSGSTAmount_Calc"] = taxAmount;
			}
			else
			{
				var taxRate = CreateTaxRate("IVA", "Mexico IVA", 19);
				consolCost[JobConsolCostSchema.E6_OSCostAmount] = amount;
				consolCost[JobConsolCostSchema.E6_RX_NKCurrency] = Core.Constants.CurrencyCodes.UnitedStates;
				consolCost[JobConsolCostSchema.E6_ExchangeRate] = 664.987M;
				consolCost[JobConsolCostSchema.E6_AT_TaxRate] = taxRate.PK;
			}

			return consolCost;
		}

		AccTaxRate CreateTaxRate(string code, string description, int rateNum)
		{
			var codeWithZZ = "ZZ" + code;
			var query = new ZQuery(AccTaxRateSchema.AT_Code, codeWithZZ);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var result = Factory.LoadTop1<AccTaxRate>(query);
			if (result == null)
			{
				result = Factory.New<AccTaxRate>();
				result.AT_Code = codeWithZZ;
				result.AT_Description = description;
				result.AT_IsActive = true;
				result.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				result.AT_Type = result.AT_Type.IsEmpty ? (ZString)AccTaxRate.Types.Rated : result.AT_Type;
				result.SetRate_ForTestOnly(rateNum, 1);
			}
			return result;
		}

		#endregion

		public void TestAWBType()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("AWBType", ExportAWBHeader.TypeOfAWB.DirectMaster, AWBHeader.AWBType);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("AWBType", ExportAWBHeader.TypeOfAWB.AgentMaster, AWBHeader.AWBType);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.AWBCoload;
			AssertEquals("AWBType", ExportAWBHeader.TypeOfAWB.AgentMaster, AWBHeader.AWBType);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("AWBType", ExportAWBHeader.TypeOfAWB.MasterHouse, AWBHeader.AWBType);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Other;
			AssertEquals("AWBType", ExportAWBHeader.TypeOfAWB.UndefinedMaster, AWBHeader.AWBType);

			AWBHeader.Consol.Delete();
			AssertEquals("AWBType", ExportAWBHeader.TypeOfAWB.UndefinedMaster, AWBHeader.AWBType);
		}

		public void TestShippingLoadAndCount()
		{
			ForwardingConsol consol = AWBHeader.Consol;
			ForwardingShipment shipment1 = consol.Shipments[0];
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			shipment1.JS_OuterPacks = 2;
			shipment2.JS_OuterPacks = 4;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			AWBHeader.Populate();
			AssertEquals("Consol is LSE, no inner packs specified: should be zero", 0, AWBHeader.EH_ShippingLoadAndCount);

			shipment1.JS_TotalPackageCount = 10;
			AWBHeader.Populate();
			AssertEquals("Consol is LSE, shipment1 has inner packs specified: should be the sum of shipment1's inners and shipment2's outers", 14, (int)AWBHeader.EH_ShippingLoadAndCount);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			AWBHeader.Populate();
			AssertEquals("Consol is ULD: should be the sum of outer packs", 6, AWBHeader.EH_ShippingLoadAndCount);
		}

		public void TestShippingLoadAndCountWhenBiggerThanZShortRange()
		{
			var consol = AWBHeader.Consol;
			var shipment1 = consol.Shipments[0];
			var shipment2 = consol.Shipments.AddNew();

			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			shipment1.JS_OuterPacks = 32768;
			shipment2.JS_OuterPacks = 40000;
			AWBHeader.Populate();
			AssertEquals(72768, AWBHeader.EH_ShippingLoadAndCount);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			shipment1.JS_TotalPackageCount = 50000;
			shipment2.JS_TotalPackageCount = 40000;
			AWBHeader.Populate();
			AssertEquals(90000, AWBHeader.EH_ShippingLoadAndCount);
		}

		public void TestShippingLoadAndCount_SubShipmentNotCounted()
		{
			CommonShipment shipmentMaster = AWBHeader.Consol.Shipments.AddNew();
			shipmentMaster.JS_ActualWeight = 30;
			shipmentMaster.JS_OuterPacks = 9;
			shipmentMaster.JS_TotalPackageCount = 9;

			CommonShipment shipmentSub1 = AWBHeader.Consol.Shipments.AddNew();
			shipmentSub1.JS_ActualWeight = 10;
			shipmentSub1.JS_OuterPacks = 4;
			shipmentSub1.JS_TotalPackageCount = 5;
			shipmentSub1.JS_JS_ColoadMasterShipmentForBinding = shipmentMaster.PK;

			CommonShipment shipmentSub2 = AWBHeader.Consol.Shipments.AddNew();
			shipmentSub2.JS_ActualWeight = 20;
			shipmentSub2.JS_OuterPacks = 5;
			shipmentSub2.JS_TotalPackageCount = 5;
			shipmentSub2.JS_JS_ColoadMasterShipmentForBinding = shipmentMaster.PK;

			AWBHeader.Populate();

			AssertEquals(9, (int)AWBHeader.EH_ShippingLoadAndCount);
			AssertEquals(30, (int)AWBHeader.EH_TotalGrossWeight);
		}

		public void TestRateLineChargeableWeight()
		{
			CommonShipment shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 1M;

			var m3kg = Utilities.Round(1000000 / ConversionFactor.Standard.Metric.Air.Factor, 3);
			AssertEquals(m3kg, AWBHeader.RateLineChargeableWeight);

			AWBHeader.Consol.JK_OverrideConsolChargeable = true;
			AWBHeader.Consol.JK_ConsolChargeable = 14M;
			AssertEquals(14M, AWBHeader.RateLineChargeableWeight);
		}

		public void TestRateLineGrossWeight()
		{
			CommonShipment shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 10M;

			shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 20M;

			AssertEquals("Measures are not overriden", false, AWBHeader.Consol.JK_OverrideConsolChargeable);
			AssertEquals(30M, AWBHeader.RateLineGrossWeight);

			AWBHeader.Consol.JK_OverrideConsolChargeable = true;
			AWBHeader.Consol.JK_CorrectedConsolWeight = 32m;

			AssertEquals("Measures are overriden", true, AWBHeader.Consol.JK_OverrideConsolChargeable);
			AssertEquals(32M, AWBHeader.RateLineGrossWeight);

			AWBHeader.Consol.JK_CorrectedConsolWeight = 230M;
			AWBHeader.Consol.JK_CorrectedConsolWeightUnit = Weight.Grams;
			AssertEquals("230 G => 0.23 KG", 0.23m, AWBHeader.RateLineGrossWeight);
			AssertEquals("KG", "K", AWBHeader.RateLineWeightUnit);
		}

		public void TestRateLineGrossWeightUnit()
		{
			Env.Registry.FreightWeightUnit = Core.Constants.Weight.Kilograms;

			CommonShipment shipment1 = AWBHeader.Consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 10M;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			CommonShipment shipment2 = AWBHeader.Consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 20M;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			AssertEquals(30M, AWBHeader.RateLineGrossWeight);

			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AssertEquals(13.608m, AWBHeader.RateLineGrossWeight);
		}

		public void TestRateLineWeightUnit()
		{
			AWBHeader.Consol.Shipments.RemoveAll();
			CommonShipment shipment1 = AWBHeader.Consol.Shipments.AddNew();

			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AssertEquals("LB", "L", AWBHeader.RateLineWeightUnit);

			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			AssertEquals("KG", "K", AWBHeader.RateLineWeightUnit);

			AWBHeader.Consol.JK_OverrideConsolChargeable = true;
			AWBHeader.Consol.JK_CorrectedConsolWeightUnit = Weight.Pounds;
			AssertEquals("LB", "L", AWBHeader.RateLineWeightUnit);

			AWBHeader.Consol.JK_CorrectedConsolWeightUnit = Weight.Grams;
			AssertEquals("KG", "K", AWBHeader.RateLineWeightUnit);
		}

		public void TestEH_WeightPrepaidCollect()
		{
			AWBHeader.Consol.JK_PrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			AWBHeader.EH_WeightVPPDCOL = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;

			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, AWBHeader.EH_WeightPrepaidCollect);

			AWBHeader.Consol.JK_OverrideWaybillDefaults = false;
			AssertEquals("prepaid/collect status from shipment", ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, AWBHeader.EH_WeightPrepaidCollect);
		}

		public void TestEH_OtherPrepaidCollect()
		{
			AWBHeader.Consol.JK_PrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			AWBHeader.EH_OtherPPDCOL = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;

			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, AWBHeader.EH_OtherPrepaidCollect);

			AWBHeader.Consol.JK_OverrideWaybillDefaults = false;
			AssertEquals("prepaid/collect status from AWB shipment", ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, AWBHeader.EH_OtherPrepaidCollect);
		}

		public void TestEH_WeightPPD()
		{
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AWBHeader.EH_WeightPrepaidCollect = "";
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_WeightPPD);

			AWBHeader.EH_WeightPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_WeightPPD);

			AWBHeader.EH_WeightPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals("prepaid/collect status from AWB header", ZBool.True, AWBHeader.EH_WeightPPD);
		}

		public void TestEH_WeightCOL()
		{
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AWBHeader.EH_WeightPrepaidCollect = "";
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_WeightCOL);

			AWBHeader.EH_WeightPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_WeightCOL);

			AWBHeader.EH_WeightPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals("prepaid/collect status from AWB header", ZBool.True, AWBHeader.EH_WeightCOL);
		}

		public void TestEH_OtherPPD()
		{
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AWBHeader.EH_OtherPrepaidCollect = "";
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_OtherPPD);

			AWBHeader.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_OtherPPD);

			AWBHeader.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals("prepaid/collect status from AWB header", ZBool.True, AWBHeader.EH_OtherPPD);
		}

		public void TestEH_OtherCOL()
		{
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AWBHeader.EH_OtherPrepaidCollect = "";
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_OtherCOL);

			AWBHeader.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_OtherCOL);

			AWBHeader.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals("prepaid/collect status from AWB header", ZBool.True, AWBHeader.EH_OtherCOL);
		}

		public void TestPopulateIssuedBy()
		{
			AWBHeader.Consol.JK_MasterBillNum = "08112345678";
			AWBHeader.Populate();

			AssertEquals("QANTAS AIRWAYS LIMITED", AWBHeader.EH_IssuingAgentName);
			AssertEquals("ABN 16 009 661 901", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("QANTAS CENTRE, 203 COWARD STREET, MASCOT, NEW SOUT", AWBHeader.EH_IssuingAgentAddress2);

			// sending forwarder
			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AWBHeader.Consol.SendingForwarder.OH_FullName = "Full Name";
			AWBHeader.Consol.SendingForwarder.MainAddress.OA_Address1 = "Address 1";
			AWBHeader.Consol.SendingForwarder.MainAddress.OA_Address2 = "Address 2";
			AWBHeader.Consol.SendingForwarder.MainAddress.OA_City = "City";
			AWBHeader.Consol.SendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			AWBHeader.Consol.SendingForwarder.MainAddress.OA_PostCode = "Post Code";

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBHeader.Populate();

			AssertEquals("FULL NAME", AWBHeader.EH_IssuingAgentName);
			AssertEquals("ADDRESS 1", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("ADDRESS 2, CITY, NSW, POST CODE, AUSTRALIA", AWBHeader.EH_IssuingAgentAddress2);

			AWBHeader.Consol.SendingForwarder.MainAddress.OA_Address1 = "Address 1 Some address";
			AWBHeader.Consol.SendingForwarder.MainAddress.OA_Address2 = "Address 2 that is longer than 50 chars long";
			AWBHeader.Consol.SendingForwarder.MainAddress.OA_City = "City Name Loooong";
			AWBHeader.Consol.SendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			AWBHeader.Consol.SendingForwarder.MainAddress.OA_PostCode = "Post Code";

			AWBHeader.Populate();

			AssertEquals("FULL NAME", AWBHeader.EH_IssuingAgentName);
			AssertEquals("ADDRESS 1 SOME ADDRESS", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("ADDRESS 2 THAT IS LONGER THAN 50 CHARS LONG, CITY", AWBHeader.EH_IssuingAgentAddress2);
		}

		public void TestHandlingInformation()
		{
			var consolNote = AWBHeader.Consol.Notes.AddNew();
			consolNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consolNote.ST_NoteDataAsText = "CONSOL NOTE";
			AWBHeader.Populate();
			AssertEquals("CONSOL NOTE", AWBHeader.EH_HandlingInformation);

			consolNote.ST_NoteDataAsText = new string('X', 500);
			AWBHeader.Populate();
			AssertEquals(new string('X', 65 * 3), AWBHeader.EH_HandlingInformation);

			consolNote.Delete();

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			var agentNote = agent.Notes.AddNew();
			agentNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			agentNote.ST_NoteDataAsText = "SENDING AGENT NOTE";
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals("SENDING AGENT NOTE", AWBHeader.EH_HandlingInformation);

			agentNote.Delete();
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_HandlingInformation);

			var shipmentNote = AWBHeader.Consol.Shipments[0].Notes.AddNew();
			shipmentNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			shipmentNote.ST_NoteDataAsText = "SHIPMENT NOTE";
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_HandlingInformation);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Populate();
			AssertEquals("SHIPMENT NOTE", AWBHeader.EH_HandlingInformation);

			shipmentNote.Delete();
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_HandlingInformation);

			var consignee = Factory.New<OrgHeader>();
			var consigneeNote = consignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_NoteDataAsText = "CONSIGNEE NOTE";
			AWBHeader.Consol.Shipments[0].ConsigneePK = consignee.PK;
			AWBHeader.Populate();
			AssertEquals("CONSIGNEE NOTE", AWBHeader.EH_HandlingInformation);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_HandlingInformation);

			consigneeNote.Delete();
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_HandlingInformation);

			Transport transport = AWBHeader.Consol.Transports[0];
			transport.JW_VoyageFlight = "QW132";
			transport.JW_ETD = new ZDateTime(2007, 8, 1);

			transport = AWBHeader.Consol.Transports.AddNew();
			transport.JW_VoyageFlight = "QW232";
			transport.JW_ETD = new ZDateTime(2007, 8, 2);

			AssertEquals("", AWBHeader.HandlingInformationForTest);

			transport = AWBHeader.Consol.Transports.AddNew();
			transport.JW_VoyageFlight = "QW332";
			transport.JW_ETD = new ZDateTime(2007, 8, 3);

			AssertEquals("QW332/3", AWBHeader.HandlingInformationForTest);

			transport.JW_VoyageFlight = "";
			transport.JW_ETD = new ZDateTime(2007, 8, 3);
			AssertEquals("/3", AWBHeader.HandlingInformationForTest);

			transport.JW_VoyageFlight = "QF123";
			transport.JW_ETD = ZDateTime.Empty;
			AssertEquals("QF123", AWBHeader.HandlingInformationForTest);
		}

		public void TestHandlingInformation_RegistryExtraTextWithNewLineDelimiter()
		{
			using (FreightDataRegistry.Instance.MAWBHandlingInformationExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MAWB REGISTRY TEXT"))
			{
				var consolNote = AWBHeader.Consol.Notes.AddNew();
				consolNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
				consolNote.ST_NoteDataAsText = "CONSOL NOTE";

				AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
				AWBHeader.Consol.JK_RL_NKDischargePort = "EGCAI";

				var entryNum1 = AWBHeader.Consol.Numbers.AddNew();
				entryNum1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
				entryNum1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
				entryNum1.CE_RN_NKCountryCode = CountryCodes.Egypt;
				entryNum1.CE_EntryNum = "6AU123456789D0VAHK11N";

				AWBHeader.Populate();
				AssertEquals("CONSOL NOTE\r\nACID Number:6AU123456789D0VAHK11N\r\nMAWB REGISTRY TEXT", AWBHeader.EH_HandlingInformation);
			}
		}

		public void TestHandlingInformation_DangerousGoods_DisplaysWhenPacklineContainsNonExcludedDG()
		{
			var undgExcludedSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgExcludedSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgExcludedSubstance.DG_UNNO = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First();

			var excludedDangerousGood1 = Factory.NewWithValidTestData<UNDGDataItem>();
			excludedDangerousGood1.DI_DG = undgExcludedSubstance.PK;
			excludedDangerousGood1.DI_PackageCount = 1;
			var excludedDangerousGood2 = Factory.NewWithValidTestData<UNDGDataItem>();
			excludedDangerousGood2.DI_DG = undgExcludedSubstance.PK;
			excludedDangerousGood2.DI_PackageCount = 1;

			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			var dangerousGood1 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood1.DI_DG = undgIATASubstance.PK;
			dangerousGood1.DI_PackageCount = 1;
			var dangerousGood2 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood2.DI_DG = undgIATASubstance.PK;
			dangerousGood2.DI_PackageCount = 1;
			var dangerousGood3 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood3.DI_DG = undgIATASubstance.PK;
			dangerousGood3.DI_PackageCount = 1;

			var safeShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			safeShipment1.JS_UniqueConsignRef = "SafeShipment1";
			safeShipment1.OuterPackLines.AddNew();

			var safeShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			safeShipment2.JS_UniqueConsignRef = "SafeShipment2";
			safeShipment2.OuterPackLines.AddNew().UNDGs.Add(excludedDangerousGood1);

			var dangerousShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment1.JS_UniqueConsignRef = "DangerousShipment1";
			dangerousShipment1.OuterPackLines.AddNew().UNDGs.Add(dangerousGood1);

			var dangerousShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment2.JS_UniqueConsignRef = "DangerousShipment2";
			dangerousShipment2.OuterPackLines.AddNew();
			dangerousShipment2.OuterPackLines.AddNew().UNDGs.Add(dangerousGood2);

			var dangerousShipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment3.JS_UniqueConsignRef = "DangerousShipment3";
			dangerousShipment3.OuterPackLines.AddNew();
			dangerousShipment3.OuterPackLines.AddNew().UNDGs.Add(excludedDangerousGood2);
			dangerousShipment3.OuterPackLines.AddNew().UNDGs.Add(dangerousGood3);

			var dangerousShipments = new List<ForwardingShipment>() { dangerousShipment1, dangerousShipment2, dangerousShipment3 };
			var safeShipments = new List<ForwardingShipment>() { safeShipment1, safeShipment2 };

			var dgHandlingInfoMessage = "Dangerous Goods as per associated Shipper's Declaration";

			CombineAssertions("Dangerous Goods handling message", () =>
			{
				foreach (var dangerousShipment in dangerousShipments)
				{
					AWBHeader.Consol.Shipments.RemoveAll();
					AWBHeader.Consol.Shipments.Add(dangerousShipment);
					AWBHeader.Populate();

					AssertContains("Handling info should contain DG message for " + dangerousShipment.JS_UniqueConsignRef,
						dgHandlingInfoMessage, AWBHeader.EH_HandlingInformation);
				}

				foreach (var safeShipment in safeShipments)
				{
					AWBHeader.Consol.Shipments.RemoveAll();
					AWBHeader.Consol.Shipments.Add(safeShipment);
					AWBHeader.Populate();

					AssertNotContains("Handling info should not contain DG message for " + safeShipment.JS_UniqueConsignRef,
						dgHandlingInfoMessage, AWBHeader.EH_HandlingInformation);
				}
			});
		}

		#region TestConsolHasShipmentsWithRadioactiveSubstance

		public void TestGoodsDescription_Displayed_WhenNotDirectConsol_WhenShipmentHasRadioactiveSubstanceInExceptedQuantities()
		{
			var dangerousGood1 = CreateDangerousGoodWithUndgSubstance("7", "2908", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "PKG", 5);
			dangerousGood1.DI_ApprovalCertificateType = ApprovalCertificateTypeList.Codes.LowDispersibleMaterial;
			dangerousGood1.DI_ApprovalCertificateIDMark = "123-45";

			var dangerousGood2 = CreateDangerousGoodWithUndgSubstance("7", "2778", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "PKG", 6);

			var dangerousShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment1.JS_UniqueConsignRef = "DangerousShipment1";
			dangerousShipment1.OuterPackLines.AddNew().UNDGs.Add(dangerousGood1);
			dangerousShipment1.OuterPackLines.AddNew().UNDGs.Add(dangerousGood2);

			var dangerousGood3 = CreateDangerousGoodWithUndgSubstance("7", "2911", "B", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "BOX", 1);
			var dangerousGood4 = CreateDangerousGoodWithUndgSubstance("6.1", "2911", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "BOX", 2);
			var dangerousGood5 = CreateDangerousGoodWithUndgSubstance("7", "2911", "B", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN, "BOX", 3);

			var dangerousShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment2.JS_UniqueConsignRef = "DangerousShipment2";
			dangerousShipment2.OuterPackLines.AddNew().UNDGs.Add(dangerousGood3);
			dangerousShipment2.OuterPackLines.AddNew().UNDGs.Add(dangerousGood4);
			dangerousShipment2.OuterPackLines.AddNew().UNDGs.Add(dangerousGood5);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "ACCU GREEN MANUFACTURING (NZ) CORPORATION";

			consignor.MainAddress.Address1 = "1 TRUGOOD DRIVE";
			consignor.MainAddress.Address2 = "EAST TAMAKI";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "BBIT (AE) CORPORATION";

			consignee.MainAddress.Address1 = "100 BUSH STREET";
			consignee.MainAddress.Address2 = "SUITE 1850";
			consignee.MainAddress.City = "California";
			consignee.MainAddress.OA_RN_NKCountryCode = "US";

			dangerousShipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			dangerousShipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			dangerousShipment1.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			dangerousShipment2.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			AWBHeader.Consol.Shipments.RemoveAll();
			AWBHeader.Consol.Shipments.Add(dangerousShipment1);
			AWBHeader.Consol.Shipments.Add(dangerousShipment2);
			AWBHeader.Populate();

			const string expectedGoodsDescription = "Consolidation as per attached list\nShipper: ACCU GREEN MANUFACTURING (NZ) CORPORATION,1 TRUGOOD DRIVE,EAST TAMAKI,Sydney,Australia Consignee: BBIT (AE) CORPORATION,100 BUSH STREET,SUITE 1850,California,United States\nUN2908,\nLow dispersible material certificate\n123-45\n5 Package\nShipper: ACCU GREEN MANUFACTURING (NZ) CORPORATION,1 TRUGOOD DRIVE,EAST TAMAKI,Sydney,Australia Consignee: BBIT (AE) CORPORATION,100 BUSH STREET,SUITE 1850,California,United States\nUN2911,\n1 Box\n";
			AssertContains(expectedGoodsDescription, AWBHeader.GoodsDescription);

			var nonDirectAgentTypes = new List<string> { "AGT", "CLD", "CHT", "COU", "OTH", "CLA", "CLM" };
			foreach (var agentType in nonDirectAgentTypes)
			{
				AWBHeader.Consol.JK_AgentType = agentType;
				AWBHeader.Populate();
				AssertContains(expectedGoodsDescription, AWBHeader.GoodsDescription);
			}
		}

		public void TestGoodsDescription_Displayed_WhenDirectConsol_WhenShipmentHasRadioactiveSubstanceInExceptedQuantities()
		{
			var dangerousGood1 = CreateDangerousGoodWithUndgSubstance("7", "2908", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "PKG", 5);
			dangerousGood1.DI_ApprovalCertificateType = ApprovalCertificateTypeList.Codes.LowDispersibleMaterial;
			dangerousGood1.DI_ApprovalCertificateIDMark = "123-45";

			var dangerousGood2 = CreateDangerousGoodWithUndgSubstance("7", "2778", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "PKG", 6);

			var dangerousShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment1.JS_UniqueConsignRef = "DangerousShipment1";
			dangerousShipment1.OuterPackLines.AddNew().UNDGs.Add(dangerousGood1);
			dangerousShipment1.OuterPackLines.AddNew().UNDGs.Add(dangerousGood2);

			var dangerousGood3 = CreateDangerousGoodWithUndgSubstance("7", "2911", "B", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "BOX", 1);
			var dangerousGood4 = CreateDangerousGoodWithUndgSubstance("6.1", "2911", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "BOX", 2);
			var dangerousGood5 = CreateDangerousGoodWithUndgSubstance("7", "2911", "B", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN, "BOX", 3);

			var dangerousShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment2.JS_UniqueConsignRef = "DangerousShipment2";
			dangerousShipment2.OuterPackLines.AddNew().UNDGs.Add(dangerousGood3);
			dangerousShipment2.OuterPackLines.AddNew().UNDGs.Add(dangerousGood4);
			dangerousShipment2.OuterPackLines.AddNew().UNDGs.Add(dangerousGood5);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "ACCU GREEN MANUFACTURING (NZ) CORPORATION";

			consignor.MainAddress.Address1 = "1 TRUGOOD DRIVE";
			consignor.MainAddress.Address2 = "EAST TAMAKI";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "BBIT (AE) CORPORATION";

			consignee.MainAddress.Address1 = "100 BUSH STREET";
			consignee.MainAddress.Address2 = "SUITE 1850";
			consignee.MainAddress.City = "California";
			consignee.MainAddress.OA_RN_NKCountryCode = "US";

			dangerousShipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			dangerousShipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			dangerousShipment1.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			dangerousShipment2.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			AWBHeader.Consol.Shipments.RemoveAll();
			AWBHeader.Consol.Shipments.Add(dangerousShipment1);
			AWBHeader.Consol.Shipments.Add(dangerousShipment2);
			AWBHeader.Consol.JK_AgentType = "DRT";
			AWBHeader.Populate();
			AssertContains("UN2908,\nLow dispersible material certificate\n123-45\n5 Package\nUN2911,\n1 Box\n", AWBHeader.GoodsDescription);
		}

		public void TestHandlingInformation_DangerousGoods_NotDisplaysWhenShipmentHasRadioactiveSubstanceInExceptedQuantities()
		{
			var dangerousGood = CreateDangerousGoodWithUndgSubstance("7", "2908", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "BOX", 1);

			var dangerousShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment.JS_UniqueConsignRef = "DangerousShipment1";
			dangerousShipment.OuterPackLines.AddNew().UNDGs.Add(dangerousGood);

			var dgHandlingInfoMessage = "Dangerous Goods as per associated Shipper's Declaration";

			AWBHeader.Consol.Shipments.RemoveAll();
			AWBHeader.Consol.Shipments.Add(dangerousShipment);

			var agentTypes = new List<string> { "DRT", "AGT", "CLD", "CHT", "COU", "OTH", "CLA", "CLM" };
			foreach (var agentType in agentTypes)
			{
				AWBHeader.Consol.JK_AgentType = agentType;
				AWBHeader.Populate();
				AssertEquals(ZString.Empty, AWBHeader.EH_HandlingInformation);
			}

			AWBHeader.Consol.JK_AgentType = "AGT";
			var undgSubstance = dangerousGood.Substance;
			undgSubstance.DG_UNNO = "9434";
			dangerousGood.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgSubstance);
			AWBHeader.Populate();

			AssertContains(dgHandlingInfoMessage, AWBHeader.EH_HandlingInformation);
		}

		public void TestGoodsDescription_DisplayDryIceInPopulatingNatureAndQuantityOfGoods_WhenShipmentHasRadioactiveSubstanceAndDryIceInExceptedQuantities()
		{
			var dangerousGood1 = CreateDangerousGoodWithUndgSubstance("7", "2910", "", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "PKG", 5);
			dangerousGood1.DI_ApprovalCertificateType = ApprovalCertificateTypeList.Codes.LowDispersibleMaterial;
			dangerousGood1.DI_ApprovalCertificateIDMark = "123-45";

			var dangerousGood2 = CreateDangerousGoodWithUndgSubstance("9", "1845", "b", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "PKG", 10);

			var dangerousShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			dangerousShipment1.JS_UniqueConsignRef = "DangerousShipment";
			dangerousShipment1.OuterPackLines.AddNew().UNDGs.Add(dangerousGood1);
			dangerousShipment1.OuterPackLines.AddNew().UNDGs.Add(dangerousGood2);

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "ACE INDUSTRIES (FR) CORPORATION";

			consignor.MainAddress.Address1 = "8 RUE DES DEUX CEDRES";
			consignor.MainAddress.City = "ROISSY";
			consignor.MainAddress.OA_RN_NKCountryCode = "FR";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "SG 1 IMPORTER/EXPORTER CORPORATION";

			consignee.MainAddress.Address1 = "20 TUAS AVENUE 1";
			consignee.MainAddress.City = "SINGAPORE";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";

			dangerousShipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			AWBHeader.Consol.Shipments.RemoveAll();
			AWBHeader.Consol.Shipments.Add(dangerousShipment1);
			AWBHeader.Consol.JK_AgentType = "AGT";
			AWBHeader.Populate();
			AssertContains("Consolidation as per attached list\nShipper: ACE INDUSTRIES (FR) CORPORATION,8 RUE DES DEUX CEDRES,ROISSY,France \n" +
				"UN2910,\nLow dispersible material certificate\n123-45\n5 Package\nUN1845,\n10 Package\n", AWBHeader.GoodsDescription);
		}

		public void TestAddRadioactiveUndgMessage_DoesNotThrowException_WhenShipmentContainsUndgWithSubstanceNull()
		{
			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood.UNDGSubstancePivotCollection.UpdateDefaultPivot(null);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.OuterPackLines.AddNew().UNDGs.Add(dangerousGood);

			AssertNoExceptionThrown(() => AWBHeader.AddRadioactiveUndgMessage(string.Empty, shipment));
		}

		UNDGDataItem CreateDangerousGoodWithUndgSubstance(ZString dgClass, ZString dgUNNO, ZString dgVariant, ZString dgStandard, ZString packType, ZInt packageCount)
		{
			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Standard = dgStandard;
			undgSubstance.DG_Class = dgClass;
			undgSubstance.DG_UNNO = dgUNNO;
			undgSubstance.DG_Variant = dgVariant;

			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood.DI_DG = undgSubstance.PK;
			dangerousGood.DI_PackageCount = packageCount;
			dangerousGood.DI_F3_NKPackType = packType;

			return dangerousGood;
		}

		#endregion

		public void TestHandlingInformation_DangerousGoods_DisplaysWhenPacklineContainsNonExcludedDG_AfterRating()
		{
			var excludedUNNOCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !RequiresMeasurementDetailsUNDGs(x));
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = excludedUNNOCode;
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var excludedDangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			excludedDangerousGood.DI_DG = substance.PK;
			excludedDangerousGood.LinkDefault(substance);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Shipment";
			shipment.OuterPackLines.AddNew().UNDGs.Add(excludedDangerousGood);

			var consolCost = CreateConsolCost(Env.Registry.FreightChargeCode, 500m, 50m);

			var calculationLog = new CalculationLog();
			calculationLog.Unit = Weight.Kilograms;
			calculationLog.Minimum = 100m;
			var wrapper = new CalculationLogsWrapper();
			wrapper.Logs.Add(calculationLog);
			CalculationLogsLoader.Save(AWBHeader.Consol, wrapper);

			AWBHeader.Consol.Shipments.RemoveAll();
			AWBHeader.Consol.Shipments.Add(shipment);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("AWBRateLine1", "Consolidation as per attached list", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
				AssertEquals("AWBRateLine2", $"UN {excludedUNNOCode} (0 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
				AssertEquals("AWBRateLine3", "No Dimensions Available", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			});
		}

		public void TestHandlingInformation_DangerousGoods_DisplaysBeforeNotes()
		{
			var consolNote = AWBHeader.Consol.Notes.AddNew();
			consolNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consolNote.ST_NoteDataAsText = "CONSOL NOTE";

			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood.DI_DG = undgIATASubstance.PK;
			dangerousGood.DI_PackageCount = 1;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew().UNDGs.Add(dangerousGood);

			var dgHandlingInfoMessage = "Dangerous Goods as per associated Shipper's Declaration";

			AWBHeader.Populate();

			CombineAssertions("Dangerous Goods handling message should appear before other handling information notes", () =>
			{
				Assert("Handling info begins with DG message", AWBHeader.EH_HandlingInformation.StartsWith(dgHandlingInfoMessage));
				AssertContains("Handling info contains other notes", consolNote.ST_NoteDataAsText, AWBHeader.EH_HandlingInformation);
			});
		}

		public void TestHandlingInformation_DangerousGoods_DoesntDisplayForDGExclusions()
		{
			var undgExclusionList = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList;

			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(dangerousGood);

			var dgHandlingInfoMessage = "Dangerous Goods as per associated Shipper's Declaration";

			CombineAssertions("Dangerous Goods handling message should not display for a DG exclusion", () =>
			{
				foreach (var undgExclusion in undgExclusionList)
				{
					var subs = Factory.New<UNDGSubstance>();
					subs.DG_Code = undgExclusion;
					subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
					dangerousGood.DI_DG = subs.PK;
					dangerousGood.LinkDefault(subs);

					AWBHeader.Populate();

					AssertNotContains("Packline containing UNDG UN" + undgExclusion, dgHandlingInfoMessage, AWBHeader.EH_HandlingInformation);
				}
			});
		}

		public void TestHandlingInformation_DangerousGoods_DisplaysAmountAndPackType_WhenContainDGPackLineAndNonDGPackLine()
		{
			// Arrange
			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood.DI_DG = undgIATASubstance.PK;
			dangerousGood.DI_PackageCount = 1;

			var packType1 = Factory.NewWithValidTestData<RefPackType>();
			packType1.F3_Description = "Utes";

			var packType2 = Factory.NewWithValidTestData<RefPackType>();
			packType2.F3_Description = "Pinatas";

			var safePackline = shipment.OuterPackLines.AddNew();
			safePackline.JL_F3_NKPackType = packType1.F3_Code;
			safePackline.JL_PackageCount = 100;

			var unsafePackline = shipment.OuterPackLines.AddNew();
			unsafePackline.JL_F3_NKPackType = packType2.F3_Code;
			unsafePackline.UNDGs.Add(dangerousGood);
			unsafePackline.JL_PackageCount = 12;

			// Act
			AWBHeader.Populate();

			// Assert
			AssertEquals("Dangerous Goods handling message contains pack amount and type when there are mixed safe and dangerous packlines",
				"12 x Pack(s) Dangerous Goods as per associated Shipper's Declaration", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformation_DangerousGoods_DoesNotDisplaysAmountAndPackType_WhenContainsDGAndNonDGForAllPackLine()
		{
			// Arrange
			var shipment = AWBHeader.Consol.Shipments.AddNew();
			var mixedPackLine = shipment.OuterPackLines.AddNew();
			mixedPackLine.JL_PackageCount = 12;

			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood.DI_DG = undgIATASubstance.PK;
			dangerousGood.DI_PackageCount = 3;
			mixedPackLine.UNDGs.Add(dangerousGood);

			// Act
			AWBHeader.Populate();

			// Assert
			AssertEquals("Dangerous Goods handling message displays DG statement only without pack count for mixed safe and dangerous packline",
				"Dangerous Goods as per associated Shipper's Declaration", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformation_DangerousGoods_DisplaysCargoOnlySuffix_ExceedsQuantityPerPack()
		{
			// Arrange
			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_LQ2OrPaxMaxAmt = 5m;
			undgIATASubstance.DG_LQ2OrPaxMaxAmtUQ = "KG";

			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood.DI_DG = undgIATASubstance.PK;
			dangerousGood.DI_DGWeight = 20;
			dangerousGood.DI_UnitOfWeight = "KG";
			dangerousGood.DI_PackageCount = 2;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(dangerousGood);

			// Act
			AWBHeader.Populate();

			// Assert
			AssertEquals("Dangerous Goods handling message contains CAO when exceeds Quantity Per Pack weight",
				"Dangerous Goods as per associated Shipper's Declaration – Cargo Aircraft Only", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformation_DangerousGoods_NoCargoOnlySuffix_QuantityPerPackForPAXAndCAOIsSame()
		{
			// Arrange
			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_LQ2OrPaxMaxAmt = 5m;
			undgIATASubstance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			undgIATASubstance.DG_CargoMaxAmt = 5m;
			undgIATASubstance.DG_CargoMaxAmtUQ = "KG";

			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood.DI_DG = undgIATASubstance.PK;
			dangerousGood.DI_DGWeight = 20;
			dangerousGood.DI_UnitOfWeight = "KG";
			dangerousGood.DI_PackageCount = 2;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();
			packline.UNDGs.Add(dangerousGood);

			// Act
			AWBHeader.Populate();

			// Assert
			AssertEquals("Dangerous Goods handling message contains no CAO when Quantity Per Pack for PAX and CAO is the same",
				"Dangerous Goods as per associated Shipper's Declaration", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationWithVATNumberForArgentina()
		{
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);

			AWBHeader.Consol.JK_RL_NKDischargePort = "ARBUE";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_IMDocumentAddressPreference = OrgConstants.AddressType.Office;

			var taxCode = consignee.CustomsCodes.AddNew();
			taxCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Argentina;
			taxCode.OK_CustomsRegNo = "1234";

			CreateRefDocOrgCusCode(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, CountryCodes.Argentina, CountryCodes.Argentina, 1, "CUIT", "CUIT", "AWB");

			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = consignee.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals("CUIT: 1234", (string)AWBHeader.EH_HandlingInformation);

			AWBHeader.Consol.JK_RL_NKDischargePort = "CNSHA";
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationWithRUCNumberForBrazil()
		{
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);

			AWBHeader.Consol.JK_RL_NKLoadPort = "BRSAO";
			AWBHeader.Consol.JK_RL_NKDischargePort = "AUSYD";
			Assert(AWBHeader.Consol.IsExportFrom(CountryCodes.Brazil));
			var consolEntryNum = AWBHeader.Consol.Numbers.AddNew();
			consolEntryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			consolEntryNum.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			consolEntryNum.CE_RN_NKCountryCode = CountryCodes.Brazil;
			consolEntryNum.CE_EntryNum = "6AU123456789D0VAHK11N";
			AWBHeader.Populate();
			AssertEquals("RUC:6AU123456789D0VAHK11N", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationWithACINumberForEgypt()
		{
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);

			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "EGCAI";

			var entryNum1 = AWBHeader.Consol.Numbers.AddNew();
			entryNum1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			entryNum1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNum1.CE_RN_NKCountryCode = CountryCodes.Egypt;
			entryNum1.CE_EntryNum = "6AU123456789D0VAHK11N";

			var entryNum2 = AWBHeader.Consol.Numbers.AddNew();
			entryNum2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			entryNum2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNum2.CE_RN_NKCountryCode = CountryCodes.Egypt;
			entryNum2.CE_EntryNum = "7AU234567890E1WBIL22M";

			Assert(AWBHeader.Consol.IsImportTo(CountryCodes.Egypt));

			AWBHeader.Populate();
			AssertCollectionContains("Both ACID numbers should be displayed. (Order is not important)", AWBHeader.EH_HandlingInformation, new ZString[] { "ACID Number:6AU123456789D0VAHK11N,7AU234567890E1WBIL22M", "ACID Number:7AU234567890E1WBIL22M,6AU123456789D0VAHK11N" });
		}

		public void TestNonDirectConsolHandlingInformationWithCCCNumberForCanada()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Canada))
			{
				AWBHeader.Populate();
				AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);

				AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
				AWBHeader.Consol.JK_RL_NKDischargePort = "CA2KS";
				Assert(AWBHeader.IsImportToCanada);

				var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				var agt1 = receivingForwarder.CustomsCodes.AddNew();
				agt1.OK_RN_NKCodeCountry = CountryCodes.Canada;
				agt1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				agt1.OK_CustomsRegNo = "8100";
				AWBHeader.Populate();
				AssertEquals(ZString.Empty, AWBHeader.EH_HandlingInformation);
			}
		}

		public void TestDirectConsolHandlingInformationWithCCCNumberForCanada()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Canada))
			{
				AWBHeader.Populate();
				AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);

				AWBHeader.Consol.JK_AgentType = AgentType.Direct;
				var shipment = AWBHeader.Consol.DirectShipment;
				AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
				AWBHeader.Consol.JK_RL_NKDischargePort = "CA2KS";
				Assert(AWBHeader.IsImportToCanada);

				var consignee = Factory.New<OrgHeader>();
				shipment.ConsigneePK = consignee.PK;
				var agt1 = consignee.CustomsCodes.AddNew();
				agt1.OK_RN_NKCodeCountry = CountryCodes.Canada;
				agt1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				agt1.OK_CustomsRegNo = "8100";
				AWBHeader.Populate();
				AssertEquals(ZString.Empty, AWBHeader.EH_HandlingInformation);
			}
		}

		public void TestLithiumBatteriesAndOtherDGHandlingInformationWhetherContainsCargoAircraftOnly()
		{
			const string handlingInfo = "Dangerous Goods as per associated Shipper's Declaration";
			const string handlingInfoWithCargoOnly = "Dangerous Goods as per associated Shipper's Declaration – Cargo Aircraft Only";

			Test(BuildUNDGSubstance("3166", "A", 0, "", "FOB", 0, "", "NLT"), "I", 5, "KG", false, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3166", "A", 0, "", "FOB", 0, "", "NLT"), "I", 5, "KG", true, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("0004", "", 0, "", "FOB", 0, "", "FOB"), "I", 5, "KG", false, handlingInfo);
			Test(BuildUNDGSubstance("0004", "", 0, "", "FOB", 75, "", "FOB"), "I", 5, "KG", true, handlingInfo);

			Test(BuildUNDGSubstance("2917", "", 0, "", "FOB", 0, "", "NLM"), "I", 5, "KG", false, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("2917", "", 0, "", "FOB", 0, "", "NLM"), "I", 5, "KG", true, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("0050", "", 0, "", "FOB", 75, "KG", "NLM"), "I", 74, "KG", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("0050", "", 0, "", "FOB", 75, "kg", "NLM"), "I", 75, "KG", false, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("0050", "", 0, "", "FOB", 75, "KG", "NLM"), "I", 76, "KG", true, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("2857", "", 0, "", "NLT", 0, "", "NLT"), "I", 60, "L", true, handlingInfo);
			Test(BuildUNDGSubstance("2857", "", 0, "", "NLT", 0, "", "NLT"), "I", 61, "L", true, handlingInfo);

			Test(BuildUNDGSubstance("8000", "", 30, "kg", "GLM", 30, "KG", "GLM"), "I", 30, "KG", false, handlingInfo);
			Test(BuildUNDGSubstance("8000", "", 30, "kg", "GLM", 30, "KG", "GLM"), "I", 30, "KG", true, handlingInfo);
			Test(BuildUNDGSubstance("8000", "", 30, "kg", "GLM", 30, "KG", "GLM"), "I", 31, "KG", false, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("8000", "", 30, "kg", "GLM", 30, "KG", "GLM"), "I", 31, "KG", true, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("2008", "C", 25, "KG", "NLM", 100, "KG", "NLM"), "I", 5, "KG", true, handlingInfo);
			Test(BuildUNDGSubstance("2008", "C", 25, "KG", "NLM", 100, "KG", "NLM"), "I", 5, "KG", false, handlingInfo);
			Test(BuildUNDGSubstance("2008", "C", 25, "KG", "NLM", 100, "KG", "NLM"), "I", 26, "KG", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("2008", "C", 25, "KG", "NLM", 100, "KG", "NLM"), "I", 26, "KG", false, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("1263", "C", 60, "L", "NLM", 220, "L", "NLM"), "I", 60, "L", true, handlingInfo);
			Test(BuildUNDGSubstance("1263", "C", 60, "L", "NLM", 220, "L", "NLM"), "I", 61, "L", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("1263", "C", 60, "L", "NLM", 220, "L", "NLM"), "I", 60, "L", false, handlingInfo);
			Test(BuildUNDGSubstance("1263", "C", 60, "L", "NLM", 220, "L", "NLM"), "I", 61, "L", false, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("2910", "C", 0, "L", "NLM", 0, "L", "NLM"), "I", 60, "L", false, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("1000", "", 5, "KG", "NLM", 0, "", "NLM"), "I", 5, "KG", false, handlingInfo);
			Test(BuildUNDGSubstance("1000", "", 5, "KG", "NLM", 0, "", "NLM"), "I", 5, "KG", true, handlingInfo);
			Test(BuildUNDGSubstance("1000", "", 5, "KG", "NLM", 0, "", "NLM"), "I", 6, "KG", false, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("1000", "", 5, "KG", "NLM", 0, "", "NLM"), "I", 6, "KG", true, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("2908", "", 0, "", "NLM", 0, "", "NLM"), "I", 5, "KG", false, handlingInfo);
			Test(BuildUNDGSubstance("2908", "", 0, "", "NLM", 0, "", "NLM"), "I", 5, "KG", true, handlingInfo);

			//LITHIUM BATTERY
			Test(BuildUNDGSubstance("3480", "", 0, "", "FOB", 35, "kg", "NLM"), "IA", 36, "KG", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3480", "", 0, "", "FOB", 35, "kg", "NLM"), "IA", 36, "KG", false, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3480", "", 0, "", "FOB", 35, "kg", "NLM"), "IA", 35, "KG", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3480", "", 0, "", "FOB", 35, "kg", "NLM"), "IA", 35, "KG", false, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("3090", "", 0, "", "FOB", 35, "kg", "NLM"), "IA", 36, "KG", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3090", "", 0, "", "FOB", 35, "kg", "NLM"), "IA", 36, "KG", false, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3090", "", 0, "", "FOB", 35, "kg", "NLM"), "IA", 35, "KG", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3090", "", 0, "", "FOB", 35, "kg", "NLM"), "IA", 35, "KG", false, handlingInfoWithCargoOnly);

			Test(BuildUNDGSubstance("3481", "A", 5, "kg", "NLM", 35, "kg", "NLM"), "I", 5, "KG", false, handlingInfo);
			Test(BuildUNDGSubstance("3481", "A", 5, "kg", "NLM", 35, "kg", "NLM"), "I", 177, "OZ", false, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3481", "A", 5, "KG", "NLM", 35, "KG", "NLM"), "II", 5, "KG", false, "");
			Test(BuildUNDGSubstance("3481", "A", 5, "KG", "NLM", 35, "KG", "NLM"), "II", 6, "KG", false, "");
			Test(BuildUNDGSubstance("3481", "B", 5, "KG", "NLM", 35, "KG", "NLM"), "I", 3000, "G", true, handlingInfo);
			Test(BuildUNDGSubstance("3481", "B", 5, "KG", "NLM", 35, "KG", "NLM"), "I", 6000, "G", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3481", "B", 5, "KG", "NLM", 35, "kg", "NLM"), "II", 11, "LB", true, "");
			Test(BuildUNDGSubstance("3481", "B", 5, "KG", "NLM", 35, "kg", "NLM"), "II", 12, "LB", true, "");

			Test(BuildUNDGSubstance("3091", "A", 5, "KG", "NLM", 35, "KG", "NLM"), "I", 5, "KG", false, handlingInfo);
			Test(BuildUNDGSubstance("3091", "A", 5, "kg", "NLM", 35, "KG", "NLM"), "I", 6, "KG", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3091", "A", 5, "KG", "NLM", 35, "KG", "NLM"), "II", 5, "KG", false, "");
			Test(BuildUNDGSubstance("3091", "A", 5, "KG", "NLM", 35, "KG", "NLM"), "II", 6, "KG", false, "");
			Test(BuildUNDGSubstance("3091", "B", 5, "KG", "NLM", 35, "KG", "NLM"), "I", 11m, "LB", true, handlingInfo);
			Test(BuildUNDGSubstance("3091", "B", 5, "KG", "NLM", 35, "KG", "NLM"), "I", 11.1m, "LB", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3091", "B", 5, "KG", "NLM", 35, "KG", "NLM"), "I", 6, "KG", true, handlingInfoWithCargoOnly);
			Test(BuildUNDGSubstance("3091", "B", 5, "KG", "NLM", 35, "KG", "NLM"), "II", 176, "OZ", true, "");
			Test(BuildUNDGSubstance("3091", "B", 5, "KG", "NLM", 35, "KG", "NLM"), "II", 177, "OZ", true, "");

			void Test(UNDGSubstance substance, string packingSection, Decimal amount, string amountUnit, bool isCargoOnly, string expectedHandlingInfo)
			{
				AWBHeader.Consol.Shipments.RemoveAll();
				var shipment = AWBHeader.Consol.Shipments.AddNew();
				var packLine = shipment.OuterPackLines.AddNew();
				var dangerousGood = packLine.UNDGs.AddNew();
				AWBHeader.Consol.Transports.Cast<Transport>().Where(x => x.TransportMode == TransportModes.Air).ForEach(leg => leg.JW_IsCargoOnly = isCargoOnly);
				dangerousGood.DI_DG = substance.PK;
				dangerousGood.DI_PackageCount = 1;
				dangerousGood.DI_PackingInstructionSection = packingSection;

				if (Weight.ContainsCode(amountUnit))
				{
					dangerousGood.DI_DGWeight = amount;
					dangerousGood.DI_UnitOfWeight = amountUnit;
				}
				else
				{
					dangerousGood.DI_DGVolume = amount;
					dangerousGood.DI_UnitOfVolume = amountUnit;
				}

				AWBHeader.Populate();

				if (string.IsNullOrEmpty(expectedHandlingInfo))
				{
					AssertNullOrEmpty(AWBHeader.EH_HandlingInformation);
				}
				else
				{
					AssertEquals(expectedHandlingInfo, AWBHeader.EH_HandlingInformation);
				}
			}

			UNDGSubstance BuildUNDGSubstance(string unNo, string variant, Decimal paxLimit, string paxLimitUnit, string paxLimitType, Decimal cargoLimit = 0m, string cargoLimitUnit = "", string cargoLimitType = "")
			{
				var substance = Factory.New<UNDGSubstance>();
				substance.DG_Standard = "IAT";
				substance.DG_UNNO = unNo;
				substance.DG_Variant = variant;
				substance.DG_LQ2OrPaxMaxAmt = paxLimit;
				substance.DG_LQ2OrPaxMaxAmtUQ = paxLimitUnit;
				substance.DG_LQ2OrPaxMaxAmtType = paxLimitType;
				substance.DG_CargoMaxAmt = cargoLimit;
				substance.DG_CargoMaxAmtUQ = cargoLimitUnit;
				substance.DG_CargoPackAmtType = cargoLimitType;

				return substance;
			}
		}

		public void TestHandlingInformation_LithiumBatteries_DisplaysAmountAndPackType()
		{
			var undgIATASubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgIATASubstance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries;

			var packType1 = Factory.NewWithValidTestData<RefPackType>();
			packType1.F3_Description = "Utes";

			var dangerousGood1 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood1.DI_DG = undgIATASubstance.PK;
			dangerousGood1.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionIA;
			dangerousGood1.DI_F3_NKPackType = packType1.F3_Code;
			dangerousGood1.DI_PackageCount = 2;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			var nonDgPackLine = shipment.OuterPackLines.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.UNDGs.Add(dangerousGood1);

			AWBHeader.Populate();
			AssertEquals("The pack line has one lithium battery DG", "12 x Pack(s) Dangerous Goods as per associated Shipper's Declaration", AWBHeader.EH_HandlingInformation);

			var dangerousGood2 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood2.DI_DG = undgIATASubstance.PK;
			dangerousGood2.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionIA;
			dangerousGood2.DI_F3_NKPackType = packType1.F3_Code;
			dangerousGood2.DI_PackageCount = 3;
			packLine.UNDGs.Add(dangerousGood2);

			AWBHeader.Populate();
			AssertEquals("The pack line has lithium battery DGs with the same pack type", "12 x Pack(s) Dangerous Goods as per associated Shipper's Declaration", AWBHeader.EH_HandlingInformation);

			var packType2 = Factory.NewWithValidTestData<RefPackType>();
			packType2.F3_Description = "Pinatas";

			var dangerousGood3 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood3.DI_DG = undgIATASubstance.PK;
			dangerousGood3.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionIA;
			dangerousGood3.DI_F3_NKPackType = packType2.F3_Code;
			dangerousGood3.DI_PackageCount = 4;
			packLine.UNDGs.Add(dangerousGood3);

			AWBHeader.Populate();
			AssertEquals("The pack line has lithium battery DGs with different pack types", "12 x Pack(s) Dangerous Goods as per associated Shipper's Declaration", AWBHeader.EH_HandlingInformation);

			var dangerousGood4 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood4.DI_DG = undgIATASubstance.PK;
			dangerousGood4.DI_PackingInstructionSection = PackingInstructionSectionTypeList.Codes.SectionI;
			dangerousGood4.DI_F3_NKPackType = packType2.F3_Code;
			dangerousGood4.DI_PackageCount = 2;
			packLine.UNDGs.Add(dangerousGood4);

			AWBHeader.Populate();
			AssertEquals("The pack line has a mix of lithium battery and other DGs", "12 x Pack(s) Dangerous Goods as per associated Shipper's Declaration", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationExclusiveUse()
		{
			var shipment = AWBHeader.Consol.Shipments.AddNew();
			AWBHeader.Consol.Transports.Cast<Transport>().Where(x => x.TransportMode == Constants.TransportModes.Air).ForEach(leg => leg.JW_IsCargoOnly = true);
			var packLine = shipment.OuterPackLines.AddNew();

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_IsExclusiveUse = true;

			packLine.UNDGs.Add(undgDataItem);

			Factory.Save();

			AWBHeader.Populate();
			AssertContains("Exclusive Use", AWBHeader.EH_HandlingInformation);

			undgDataItem.DI_IsExclusiveUse = false;
			AWBHeader.Populate();
			AssertNotContains("Exclusive Use", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationWithMixedDG()
		{
			var packType1 = Factory.NewWithValidTestData<RefPackType>();
			packType1.F3_Description = "PLT";

			var packType2 = Factory.NewWithValidTestData<RefPackType>();
			packType2.F3_Description = "BOX";

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = packType1.F3_Code;
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = packType2.F3_Code;
			var dangerousGood = packLine1.UNDGs.AddNew();
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = "IAT";
			substance.DG_UNNO = "1043";
			substance.DG_Code = "1043a";
			substance.DG_LQ2OrPaxMaxAmt = 5m;
			substance.DG_LQ2OrPaxMaxAmtUQ = "KG";
			dangerousGood.DI_DG = substance.PK;
			dangerousGood.DI_PackageCount = 1;
			dangerousGood.DI_F3_NKPackType = packType2.F3_Code;
			dangerousGood.DI_DGWeight = 6m;
			dangerousGood.DI_UnitOfWeight = "KG";
			AWBHeader.Populate();
			AssertEquals("1 x Pack(s) Dangerous Goods as per associated Shipper's Declaration – Cargo Aircraft Only", AWBHeader.EH_HandlingInformation);
		}

		void CreateRefDocOrgCusCode(ZString code, ZString regulatingCountry, ZString codeCountry, ZByte priority, ZString shortLabel, ZString longLabel, ZString documentType, ZString notes = default)
		{
			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = code;
			orgCusCode.DOC_RN_NKRegulatingCountry = regulatingCountry;
			orgCusCode.DOC_RN_NKCodeCountry = codeCountry;
			orgCusCode.DOC_Priority = priority;
			orgCusCode.DOC_ShortLabel = shortLabel;
			orgCusCode.DOC_LongLabel = longLabel;
			orgCusCode.DOC_DocumentType = documentType;
			orgCusCode.DOC_Notes = notes;
		}

		public void TestAWBDestinationCode()
		{
			var uNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBBHM");
			AWBHeader.Consol.JK_RL_NKDischargePort = uNLOCO.RL_Code;
			AWBHeader.Populate();
			AssertEquals("BHX", AWBHeader.EH_AirportOfDestinationCode);
		}

		JobMawb AddMawb()
		{
			JobMawb result = Factory.New<JobMawb>();
			result.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			result.JM_ParentID = AWBHeader.Consol.PK;
			result.JM_MAWB = "12345678";
			return result;
		}

		public void TestOriginCode()
		{
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "CNSHA";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUMEL";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "CNSHA";

			AWBHeader.Populate();
			AssertEquals("Origin code comes from consol load port", "SYD", AWBHeader.EH_AWBOriginCode);

			OrgHeader localCarrier = Factory.NewWithValidTestData<OrgHeader>();
			Transport preCarriage = AWBHeader.Consol.Transports.AddNew();
			preCarriage.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			preCarriage.JW_RL_NKLoadPort = "AUSYD";
			preCarriage.JW_RL_NKDiscPort = "AUMEL";
			preCarriage.JW_TransportMode = Core.Constants.TransportModes.Road;

			AWBHeader.Consol.JK_OA_ShippingLineAddress = ZGuid.Empty;

			AWBHeader.Populate();
			AssertEquals("Origin code comes from first flight when there is pre-carriage with a different carrier", "MEL", AWBHeader.EH_AWBOriginCode);

			AWBHeader.Consol.Transports.RemoveAndDeleteAll();
			AWBHeader.Populate();
			AssertEquals("Origin code comes from consol load port", "SYD", AWBHeader.EH_AWBOriginCode);

			AWBHeader.Consol.JK_RL_NKLoadPort = ZString.Empty;
			AWBHeader.Populate();
			AssertEquals("Origin code should be empty with no consol load port", ZString.Empty, AWBHeader.EH_AWBOriginCode);
		}

		public void TestAirportOfDeparture()
		{
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "CNSHA";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUMEL";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "CNSHA";

			AWBHeader.Populate();
			AssertEquals("Airport of departure comes from consol load port", "Sydney", AWBHeader.EH_AirportOfDepartureAndRequestRouteText);

			OrgHeader localCarrier = Factory.NewWithValidTestData<OrgHeader>();
			Transport preCarriage = AWBHeader.Consol.Transports.AddNew();
			preCarriage.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			preCarriage.JW_RL_NKLoadPort = "AUSYD";
			preCarriage.JW_RL_NKDiscPort = "AUMEL";
			preCarriage.JW_TransportMode = Core.Constants.TransportModes.Road;

			AWBHeader.Consol.JK_OA_ShippingLineAddress = ZGuid.Empty;

			AWBHeader.Populate();
			AssertEquals("Airport of departure comes from first flight when there is pre-carriage with a different carrier", "Melbourne", AWBHeader.EH_AirportOfDepartureAndRequestRouteText);

			AWBHeader.Consol.Transports.RemoveAndDeleteAll();
			AWBHeader.Populate();
			AssertEquals("Airport of departure comes from consol load port", "Sydney", AWBHeader.EH_AirportOfDepartureAndRequestRouteText);

			AWBHeader.Consol.JK_RL_NKLoadPort = ZString.Empty;
			AWBHeader.Populate();
			AssertEquals("Airport of departure should be empty should be empty with no consol load port", ZString.Empty, AWBHeader.EH_AirportOfDepartureAndRequestRouteText);
		}

		public void TestAirportOfDepartureAndOriginCode()
		{
			OrgHeader mainCarrier = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader localCarrier = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_ShippingLineAddress = mainCarrier.MainAddress.PK;

			ExportAWBHeader awbHeader = consol.AWBHeader;
			AssertEquals("Airport of departure comes from consol load port", "Sydney", awbHeader.EH_AirportOfDepartureAndRequestRouteText);
			AssertEquals("Origin code comes from consol load port", "SYD", awbHeader.EH_AWBOriginCode);

			Transport flight1 = consol.Transports[0];
			flight1.JW_RL_NKLoadPort = "AUBNE";
			flight1.JW_RL_NKDiscPort = "NZAKL";

			Transport flight2 = consol.Transports.AddNew();
			flight2.JW_RL_NKLoadPort = "NZAKL";
			flight2.JW_RL_NKDiscPort = "USLAX";
			flight2.JW_TransportMode = Core.Constants.TransportModes.Air;
			flight2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;

			Transport otherTransport = consol.Transports.AddNew();
			otherTransport.JW_RL_NKLoadPort = "AUMEL";
			otherTransport.JW_RL_NKDiscPort = "AUBNE";
			otherTransport.JW_TransportMode = Core.Constants.TransportModes.Road;
			otherTransport.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;

			awbHeader.Populate();
			AssertEquals("Airport of departure comes from consol load port, should ignore transports", "Sydney", awbHeader.EH_AirportOfDepartureAndRequestRouteText);
			AssertEquals("Origin code comes from consol load port, should ignore transports", "SYD", awbHeader.EH_AWBOriginCode);

			Transport preCarriageTransport = consol.Transports.AddNew();
			preCarriageTransport.JW_RL_NKLoadPort = "AUSYD";
			preCarriageTransport.JW_RL_NKDiscPort = "AUBNE";
			preCarriageTransport.JW_TransportMode = Core.Constants.TransportModes.Road;
			preCarriageTransport.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			preCarriageTransport.JW_OA_CarrierAddress = ZGuid.Empty;

			awbHeader.Populate();
			AssertEquals("When pre-carriage carrier is not specified, airport of departure comes from consol load port", "Sydney", awbHeader.EH_AirportOfDepartureAndRequestRouteText);
			AssertEquals("When pre-carriage carrier is not specified, origin code comes from consol load port", "SYD", awbHeader.EH_AWBOriginCode);

			preCarriageTransport.JW_OA_CarrierAddress = localCarrier.MainAddress.PK;
			awbHeader.Populate();
			AssertEquals("When there is a pre-carriage leg with a carrier different to the consol carrier, airport of departure comes from first flight", "Brisbane", awbHeader.EH_AirportOfDepartureAndRequestRouteText);
			AssertEquals("When there is a pre-carriage leg with a carrier different to the consol carrier, origin code comes from first flight", "BNE", awbHeader.EH_AWBOriginCode);

			preCarriageTransport.JW_OA_CarrierAddress = mainCarrier.MainAddress.PK;
			awbHeader.Populate();
			AssertEquals("Airport of departure comes from consol load port when pre-carriage has same carrier", "Sydney", awbHeader.EH_AirportOfDepartureAndRequestRouteText);
			AssertEquals("Origin code comes from consol origin load port pre-carriage has same carrier", "SYD", awbHeader.EH_AWBOriginCode);

			flight1.JW_RL_NKLoadPort = "";
			awbHeader.Populate();
			AssertEquals("When first flight load port is empty, airport of departure comes from consol load port", "Sydney", awbHeader.EH_AirportOfDepartureAndRequestRouteText);
			AssertEquals("When first flight load port is empty, origin code comes from consol load port", "SYD", awbHeader.EH_AWBOriginCode);

			consol.Transports.RemoveAndDeleteAll();
			awbHeader.Populate();
			AssertEquals("When first flight is not specified, airport of departure comes from consol load port", "Sydney", awbHeader.EH_AirportOfDepartureAndRequestRouteText);
			AssertEquals("When first flight is not specified, origin code comes from consol load port", "SYD", awbHeader.EH_AWBOriginCode);
		}

		public void TestAirportOfDestinationAndDestinationCodeInAWBForMultiRoutes()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var awbHeader = consol.AWBHeader;

			awbHeader.Populate();

			AssertEquals("Airport of destination should be empty when there is no transport", "", awbHeader.EH_AirportOfDestinationText);
			AssertEquals("Origin code should be empty when there is no transport", "", awbHeader.EH_AirportOfDestinationCode);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			awbHeader.Populate();

			AssertEquals("Airport of destination should be discharge port when there is only one transport", "Los Angeles", awbHeader.EH_AirportOfDestinationText);
			AssertEquals("Origin code should be discharge port when there is only one transport", "LAX", awbHeader.EH_AirportOfDestinationCode);

			var flight1 = consol.Transports[0];
			flight1.JW_RL_NKLoadPort = "AUSYD";
			flight1.JW_RL_NKDiscPort = "NZAKL";
			flight1.JW_TransportMode = Core.Constants.TransportModes.Air;

			var flight2 = consol.Transports.AddNew();
			flight2.JW_RL_NKLoadPort = "NZAKL";
			flight2.JW_RL_NKDiscPort = "USLAX";
			flight2.JW_TransportMode = Core.Constants.TransportModes.Road;

			awbHeader.Populate();

			AssertEquals("Airport of destination comes from last air transport", "Auckland", awbHeader.EH_AirportOfDestinationText);
			AssertEquals("Origin code comes from last air transport, should ignore transports", "AKL", awbHeader.EH_AirportOfDestinationCode);
		}

		public void TestAgentIATACode()
		{
			JobMawb mawb = AddMawb();

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "";
			AssertEquals("", AWBHeader.EH_AgentIATACodeFormatted);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567";
			AssertEquals("12-3 4567", AWBHeader.EH_AgentIATACodeFormatted);

			OrgHeader org = Factory.New<OrgHeader>();
			mawb.JM_OA_From = org.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_AgentIATACodeFormatted);

			org.MiscServ.OM_FWIATACode = "7654321";
			AssertEquals("76-5 4321", AWBHeader.EH_AgentIATACodeFormatted);
		}

		public void TestAgentAccountNo()
		{
			JobMawb mawb = AddMawb();

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "Reg AcctNo";
			AssertEquals("Reg AcctNo", AWBHeader.EH_AgentAccountNo);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_RM_Airline = Factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081").PK;
			org.CompanyData.OB_APAirlineAccountNumber = "Org AcctNo";
			org.OH_Code = "ZZ2";
			Factory.Save();

			AssertEquals("Reg AcctNo", AWBHeader.EH_AgentAccountNo);
			AWBHeader.Consol.JK_MasterBillNum = "08112345678";
			AssertEquals("Org AcctNo", AWBHeader.EH_AgentAccountNo);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			AWBHeader.Consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var branchAccount = carrier.OrgAirlineBranchAccounts.AddNew();
			branchAccount.OAA_GB_Branch = GlbBranch.CurrentBranch.PK;
			branchAccount.OAA_APAirlineAccountNumber = "123456";
			AssertEquals("123456", AWBHeader.EH_AgentAccountNo);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ZZ3";

			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_MAWB = "10000001";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_ParentTableCode = "JK";
			mawb.JM_ParentID = AWBHeader.Consol.PK;
			mawb.JM_OA_From = org.MainAddress.PK;

			AWBHeader.Populate();
			AssertEquals("Reg AcctNo", AWBHeader.EH_AgentAccountNo);

			org.MiscServ.OM_FWIATAAccountNumber = "SF AcctNo";
			AssertEquals("SF AcctNo", AWBHeader.EH_AgentAccountNo);
		}

		public void TestAgentAccountNoWhenMultipleWithAirlinePrefix()
		{
			BusinessObjectFactory savableFactory = new BusinessObjectFactory();
			OrgHeader org = savableFactory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_RM_Airline = Factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081").PK;
			org.CompanyData.OB_APAirlineAccountNumber = "Org AcctNo";

			org = savableFactory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SDF";
			org.MiscServ.OM_RM_Airline = Factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081").PK;
			org.CompanyData.OB_APAirlineAccountNumber = "Org AcctNo";
			savableFactory.Save();

			AWBHeader.Consol.JK_MasterBillNum = "08112345678";
			AssertEquals("Org AcctNo", AWBHeader.EH_AgentAccountNo);
		}

		public void TestAgentName()
		{
			JobMawb mawb = AddMawb();

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "";
			AssertEquals("", AWBHeader.EH_AgentName);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "AGT NAME";
			AssertEquals("AGT NAME", AWBHeader.EH_AgentName);

			OrgHeader org = Factory.New<OrgHeader>();
			mawb.JM_OA_From = org.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_AgentName);

			org.OH_FullName = "AGT FULL NAME";
			AssertEquals("AGT FULL NAME", AWBHeader.EH_AgentName);
		}

		public void TestAgentPlace()
		{
			JobMawb mawb = AddMawb();

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "";
			AssertEquals("", AWBHeader.EH_AgentPlace);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "AGT CITY";
			AssertEquals("AGT CITY", AWBHeader.EH_AgentPlace);

			OrgHeader org = Factory.New<OrgHeader>();
			mawb.JM_OA_From = org.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_AgentPlace);

			org.OH_RL_NKClosestPort = "ZACPT";
			AssertEquals("CAPE TOWN", AWBHeader.EH_AgentPlace);
		}

		public void TestAgentDetailsInBorrowFromCase()
		{
			ConsolExportAWBHeader header = Factory.New<ConsolExportAWBHeader>();
			AssertEquals(ZString.Empty, header.EH_AgentName);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.MasterBillAirlinePrefix = "666";
			consol.MasterBillMAWB = "10000001";
			header.EH_ParentID = consol.PK;
			AssertEquals(ZString.Empty, header.EH_AgentName);

			OrgHeader borrow = Factory.New<OrgHeader>();
			borrow.OH_FullName = "SOME AGENT NAME";
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_OA_From = borrow.MainAddress.PK;
			mawb.JM_Airline3DigitPrefix = "666";
			mawb.JM_MAWB = "10000001";
			mawb.JM_IsPaper = ZBool.True;
			header.Populate();
			AssertEquals("SOME AGENT NAME", header.EH_AgentName);
		}

		public void TestAWBRateLinesType()
		{
			AssertEquals(typeof(ConsolExportAWBRateLineCollection), AWBHeader.AWBRateLines.GetType());
		}

		public void TestULDContainers()
		{
			AssertEquals("Empty by default", 0, AWBHeader.ULDContainers.Count());

			CommonContainer container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = Core.Constants.ContainerModes.ULD;

			CommonContainer container2 = AWBHeader.Consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.AIR;

			CommonContainer container3 = AWBHeader.Consol.Containers.AddNew();
			container3.JC_ContainerMode = "XXX";

			CommonContainer container4 = AWBHeader.Consol.Containers.AddNew();
			container4.JC_ContainerMode = Core.Constants.ContainerModes.ULD;

			AssertContainsExactElementsInAnyOrder(new CommonContainer[] { container1, container4 }, AWBHeader.ULDContainers);
		}

		#region Populating AWB with ULD

		public void TestPopulateRateLinesWhenULD()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			RefContainer refULD1 = Factory.New<RefContainer>();
			refULD1.RC_IATARateClass = "123";

			CommonContainer uLD = AWBHeader.Consol.Containers.AddNew();
			uLD.JC_RC = refULD1.PK;
			uLD.JC_ContainerNum = "TESTULD1";
			uLD.JC_ContainerMode = Core.Constants.ContainerModes.ULD;
			uLD.JC_SealNum = "11111111111";
			uLD.JC_TareWeight = 150;
			uLD.JC_GrossWeight = 200;

			RefContainer refULD2 = Factory.New<RefContainer>();
			refULD2.RC_IATARateClass = "2H";

			uLD = AWBHeader.Consol.Containers.AddNew();
			uLD.JC_RC = refULD2.PK;
			uLD.JC_ContainerNum = "TESTULD2";
			uLD.JC_ContainerMode = Core.Constants.ContainerModes.ULD;
			uLD.JC_SealNum = "1H";
			uLD.JC_TareWeight = 200;
			uLD.JC_GrossWeight = 300;

			AWBHeader.Populate();

			var rateLine = AWBHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)1];
			AssertEquals("2", rateLine.ER_NoOfPiecesOrRCP);
			AssertEquals(Core.Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, rateLine.ER_RateClass);

			rateLine = AWBHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)2];
			AssertEquals("TESTULD1", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("123", rateLine.ER_CommodityItemNumber);
			AssertEquals(150M, rateLine.ER_GrossWeight);
			AssertEquals("K", rateLine.ER_WeightInLBsOrKGs);

			rateLine = AWBHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)3];
			AssertEquals("TESTULD2", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("2H", rateLine.ER_CommodityItemNumber);
			AssertEquals(200M, rateLine.ER_GrossWeight);
			AssertEquals("K", rateLine.ER_WeightInLBsOrKGs);

			rateLine = AWBHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)4];
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("", rateLine.ER_WeightInLBsOrKGs);
		}

		public void TestPopulateAWBNatureAndQtyOfGoodsWithExcessOfULDContainers()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			CreateAndPackULDContainers(AWBHeader, 20, false);

			AssertEquals(AWBHeader.Consol.Containers[0].JC_ContainerNum, "TESTULD1");
			AssertEquals(AWBHeader.Consol.Containers[1].JC_ContainerNum, "TESTULD2");
			AssertEquals(AWBHeader.Consol.Containers[19].JC_ContainerNum, "TESTULD20");

			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods =
	@"1|C|Consolidation as per attached list
2|U|TESTULD1
3|U|TESTULD2
4|U|TESTULD3
5|U|TESTULD4
6|U|TESTULD5
7|U|TESTULD6
8|U|TESTULD7
9|U|TESTULD8
10|U|TESTULD9
11|U|TESTULD10
12|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestPopulateAWBWeightsWithExcessOfULDContainers()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

			CreateAndPackULDContainers(AWBHeader, 20, false);

			AssertEquals(AWBHeader.Consol.Containers[1].JC_ContainerNum, "TESTULD2");
			AssertEquals(AWBHeader.Consol.Containers[1].ContainerWeightUnit, Core.Constants.Weight.Kilograms);
			AssertEquals(AWBHeader.Consol.Containers[1].JC_GrossWeight, 500m);

			AWBHeader.Populate();

			var rateLine = AWBHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)1];
			AssertEquals(Core.Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, rateLine.ER_RateClass);
			AssertEquals("K", rateLine.ER_WeightInLBsOrKGs);
			AssertEquals("20", rateLine.ER_NoOfPiecesOrRCP);
			AssertEquals(5600m, AWBHeader.RateLineGrossWeight);

			rateLine = AWBHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)3];
			AssertEquals(0m, rateLine.ER_GrossWeight);
			AssertEquals("K", rateLine.ER_WeightInLBsOrKGs);
			AssertEquals(AWBHeader.EH_TotalGrossWeight, 10000m);
		}

		public void TestPopulateAdditionalRatelines()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			CreateAndPackULDContainers(awbHeader, 6, true);
			awbHeader.Consol.Shipments[0].OuterPackLines[0].JL_PackageCount = 0;

			AssertNoExceptionThrown("PopulateAdditionalRatelines does NOT cause IndexOutOfBounds exception", () => awbHeader.Populate());
		}

		public void TestPopulateAdditionalRateLines_ConsolDischargePortHasNullCountryCode()
		{
			RefUNLOCO newUNLOCO = Factory.New<RefUNLOCO>();
			newUNLOCO.RL_Code = "TS123";
			newUNLOCO.RL_PortName = "Country-less UNLOCO";
			newUNLOCO.RL_RN_NKCountryCode = ZString.Empty;
			Factory.Save();

			AWBHeader.Consol.JK_RL_NKLoadPort = "USBOS";
			AWBHeader.Consol.JK_RL_NKDischargePort = "TS123";
			AssertNoExceptionThrown("PopulateAdditionalRatelines does NOT cause Null Reference exception", () => AWBHeader.Populate());
		}

		public void TestPopulateAdditionalRatelines_DoesNotThrowIndexOutOfBoundsExceptionWhenNoEmptyRateLinesAvailable()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			CreateAndPackULDContainers(awbHeader, 20, true);
			awbHeader.Consol.Shipments[0].OuterPackLines[0].JL_PackageCount = 0;

			AssertNoExceptionThrown("PopulateAdditionalRatelines does NOT cause IndexOutOfBounds exception", () => awbHeader.Populate());
		}

		public void TestPopulateAdditionalRatelines_FirstRateLineInEachTypeContainInfomation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			CreateAndPackULDContainers(awbHeader, 5, true);
			awbHeader.Populate();

			var containersGroup = from container in awbHeader.ULDContainers group container by container.GetType();

			var rateLine = awbHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)3];
			AssertEquals("The GrossWeight of first rateline in each type must equal to sum of the tare weight", (ZDecimal)220 * 5, rateLine.ER_GrossWeight);
			var subRateLine = awbHeader.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)5];
			AssertEquals("The GrossWeight of other ratelines in each type must equal to zero", (ZDecimal)0, subRateLine.ER_GrossWeight);
			AssertEquals("The commodity item umber of other ratelines in each type must be empty", ZString.Empty, subRateLine.ER_CommodityItemNumber);
		}

		public void TestPopulateAdditionalRatelines_SuppressULDTareWeightRegistryOn()
		{
			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

				var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
				awbHeader.EH_ParentID = consol.PK;

				CreateAndPackULDContainers(awbHeader, 1, true);
				awbHeader.Populate();
				AssertEquals("Tare weight should not be included in gross weight", 280, (int)awbHeader.EH_TotalGrossWeight);
			}
		}

		public void TestPopulateAdditionalRatelines_SuppressULDTareWeightRegistryOff()
		{
			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

				var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
				awbHeader.EH_ParentID = consol.PK;

				CreateAndPackULDContainers(awbHeader, 1, true);
				awbHeader.Populate();
				AssertEquals("Tare weight should be included in gross weight", 500, (int)awbHeader.EH_TotalGrossWeight);
			}
		}

		public void TestPopulateAdditionalRatelines_SuppressULDTareWeightRegistryOn_OverContainerLimit()
		{
			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

				var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
				awbHeader.EH_ParentID = consol.PK;

				CreateAndPackULDContainers(awbHeader, 12, true);
				awbHeader.Populate();
				AssertEquals("Tare weight should not be included in gross weight", 3360, (int)awbHeader.EH_TotalGrossWeight);
			}
		}

		public void TestPopulateAdditionalRatelines_SuppressULDTareWeightRegistryOff_OverContainerLimit()
		{
			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

				var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
				awbHeader.EH_ParentID = consol.PK;

				CreateAndPackULDContainers(awbHeader, 12, true);
				awbHeader.Populate();
				AssertEquals("Tare weight should be included in gross weight", 6000, (int)awbHeader.EH_TotalGrossWeight);
			}
		}

		public void TestPopulateAdditionalRatelines_SummarizeULDSLAC_WitMoreThan5Containers()
		{
			AssertPopulateAdditionalRatelines_SummarizeULDSLAC_WitMoreThan5Containers(false);
		}

		public void TestPopulateAdditionalRatelines_SummarizeULDSLAC_WitMoreThan5Containers_AfterAutoRating()
		{
			AssertPopulateAdditionalRatelines_SummarizeULDSLAC_WitMoreThan5Containers(true);
		}

		public void AssertPopulateAdditionalRatelines_SummarizeULDSLAC_WitMoreThan5Containers(bool createAndCalculateLog)
		{
			using (FreightDataRegistry.Instance.SummarizeULDSLACs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SummarizeULDSLACConfigCollection()))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

				var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
				awbHeader.EH_ParentID = consol.PK;

				CreateAndPackULDContainers(awbHeader, 6, true);

				if (createAndCalculateLog)
				{
					CreateCalculationLog();
				}

				awbHeader.Populate();

				const string expectedNatureAndQtyOfGoods = @"1|C|Consolidation as per attached list
2|U|TESTULD1
3|U|TESTULD2
4|U|TESTULD3
5|U|TESTULD4
6|U|TESTULD5
7|U|TESTULD6
8|G|No Dimensions Available
9|S|6 SLAC";
				AssertNatureAndQtyOfGoods(awbHeader, expectedNatureAndQtyOfGoods);
			}
		}

		public void TestPopulateAdditionalRatelines_SummarizeULDSLAC_WithMoreThan5Containers_WithNoTotalSLAC()
		{
			AssertPopulateAdditionalRatelines_SummarizeULDSLAC_WithMoreThan5Containers_WithNoTotalSLAC(false);
		}

		public void TestPopulateAdditionalRatelines_SummarizeULDSLAC_WithMoreThan5Containers_WithNoTotalSLAC_AfterAutoRating()
		{
			AssertPopulateAdditionalRatelines_SummarizeULDSLAC_WithMoreThan5Containers_WithNoTotalSLAC(true);
		}

		public void AssertPopulateAdditionalRatelines_SummarizeULDSLAC_WithMoreThan5Containers_WithNoTotalSLAC(bool createAndCalculateLog)
		{
			var configCollection = new SummarizeULDSLACConfigCollection();
			var config = new SummarizeULDSLACConfig();
			configCollection.Add(config);
			config.DestinationCountry = "AU";
			config.TotalSLAC = false;
			config.UseShipmentInners = false;

			using (FreightDataRegistry.Instance.SummarizeULDSLACs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configCollection))
			{
				var consol = AWBHeader.Consol;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
				consol.JK_RL_NKDischargePort = "AUSYD";
				CreateAndPackULDContainers(AWBHeader, 6, true);

				if (createAndCalculateLog)
				{
					CreateCalculationLog();
				}

				AWBHeader.Populate();

				const string expectedNatureAndQtyOfGoods = @"1|C|Consolidation as per attached list
2|U|TESTULD1
3|U|TESTULD2
4|U|TESTULD3
5|U|TESTULD4
6|U|TESTULD5
7|U|TESTULD6
8|G|No Dimensions Available
9|S|6 SLAC";
				AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
			}
		}

		public void TestPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers()
		{
			AssertPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers(false);
		}

		public void TestPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers_AfterAutoRating()
		{
			AssertPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers(true);
		}

		public void AssertPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers(bool createAndCalculateLog)
		{
			using (FreightDataRegistry.Instance.SummarizeULDSLACs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SummarizeULDSLACConfigCollection()))
			{
				var consol = AWBHeader.Consol;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
				consol.JK_RL_NKDischargePort = "AUSYD";

				CreateAndPackULDContainers(AWBHeader, 5, true);

				if (createAndCalculateLog)
				{
					CreateCalculationLog();
				}

				AWBHeader.Populate();

				const string expectedNatureAndQtyOfGoods = @"1|C|Consolidation as per attached list
2|S|1 SLAC
3|U|TESTULD1
4|S|1 SLAC
5|U|TESTULD2
6|S|1 SLAC
7|U|TESTULD3
8|S|1 SLAC
9|U|TESTULD4
10|S|1 SLAC
11|U|TESTULD5
12|S|5 SLAC";
				AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
			}
		}

		public void TestPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers_WithTotalSLAC()
		{
			AssertPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers_WithTotalSLAC(false);
		}

		public void TestPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers_WithTotalSLAC_AfterRating()
		{
			AssertPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers_WithTotalSLAC(true);
		}

		void AssertPopulateAdditionalRatelines_SummarizeULDSLAC_With5Containers_WithTotalSLAC(bool createAndCalculateLog)
		{
			var configCollection = new SummarizeULDSLACConfigCollection();
			var config = new SummarizeULDSLACConfig();
			configCollection.Add(config);
			config.DestinationCountry = "AU";
			config.TotalSLAC = true;
			config.UseShipmentInners = false;

			using (FreightDataRegistry.Instance.SummarizeULDSLACs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configCollection))
			{
				var consol = AWBHeader.Consol;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
				consol.JK_RL_NKDischargePort = "AUSYD";
				CreateAndPackULDContainers(AWBHeader, 5, true);

				if (createAndCalculateLog)
				{
					CreateCalculationLog();
				}

				AWBHeader.Populate();

				const string expectedNatureAndQtyOfGoods = @"1|C|Consolidation as per attached list
2|U|TESTULD1
3|U|TESTULD2
4|U|TESTULD3
5|U|TESTULD4
6|U|TESTULD5
7|G|No Dimensions Available
8|S|5 SLAC";
				AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
			}
		}

		public void TestPopulateAdditionalRatelines_UseShipmentInners_SummarizeULDSLAC()
		{
			AssertPopulateAdditionalRatelines_UseShipmentInners_SummarizeULDSLAC(false);
		}

		public void TestPopulateAdditionalRatelines_UseShipmentInners_SummarizeULDSLAC_AfterRating()
		{
			AssertPopulateAdditionalRatelines_UseShipmentInners_SummarizeULDSLAC(true);
		}

		public void AssertPopulateAdditionalRatelines_UseShipmentInners_SummarizeULDSLAC(bool createAndCalculateLog)
		{
			var configCollection = new SummarizeULDSLACConfigCollection();
			var config = new SummarizeULDSLACConfig();
			configCollection.Add(config);
			config.DestinationCountry = "AU";
			config.TotalSLAC = true;
			config.UseShipmentInners = true;

			using (FreightDataRegistry.Instance.SummarizeULDSLACs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, configCollection))
			{
				var consol = AWBHeader.Consol;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
				consol.JK_RL_NKDischargePort = "AUSYD";

				CreateAndPackULDContainers(AWBHeader, 5, true);
				consol.Shipments[0].JS_TotalPackageCount = 42;

				if (createAndCalculateLog)
				{
					CreateCalculationLog();
				}

				AWBHeader.Populate();

				const string expectedNatureAndQtyOfGoods = @"1|C|Consolidation as per attached list
2|U|TESTULD1
3|U|TESTULD2
4|U|TESTULD3
5|U|TESTULD4
6|U|TESTULD5
7|G|No Dimensions Available
8|S|42 SLAC";
				AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
			}
		}

		CalculationLogsWrapper CreateCalculationLog()
		{
			CreateConsolCost(Env.Registry.FreightChargeCode, 500m, 50m);
			var calculationLog = new CalculationLog();
			calculationLog.Unit = Weight.Kilograms;
			calculationLog.Minimum = 100m;
			calculationLog.RateMode = Constants.ContainerModes.ULD;
			calculationLog.AddPerUnitCalculation(100m, Constants.Weight.Kilograms, 1.28m);
			AssertEquals("Preconditon", 1, calculationLog.Steps.Count);

			var wrapper = new CalculationLogsWrapper();
			wrapper.Logs.Add(calculationLog);
			CalculationLogsLoader.Save(AWBHeader.Consol, wrapper);

			return wrapper;
		}

		[ExpectNoExceptions]
		public void TestShouldSuppressSlacLines_WhenConsolIsDeleted()
		{
			AWBHeader.Consol.Delete();
			AssertEquals(false, AWBHeader.ShouldSuppressSlacLines());
		}

		[ExpectNoExceptions]
		public void TestShouldSuppressSlacLines_WhenCountryOfDischargePortIsDeleted()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_RN_NKCountryCode = country.RN_Code;
			AWBHeader.Consol.JK_RL_NKDischargePort = port.RL_Code;

			country.Delete();
			AssertEquals(false, AWBHeader.ShouldSuppressSlacLines());
		}

		public void TestULDContainersWithSecurityStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				AWBHeader.Consol.JK_ConsolMode = "ULD";

				var refULD1 = Factory.New<RefContainer>();
				refULD1.RC_IATARateClass = "123";

				var uld1 = AWBHeader.Consol.Containers.AddNew();
				uld1.JC_RC = refULD1.PK;
				uld1.JC_ContainerNum = "TESTULD1";
				uld1.JC_ContainerMode = Core.Constants.ContainerModes.ULD;

				var refULD2 = Factory.New<RefContainer>();
				refULD2.RC_IATARateClass = "2H";

				var uld2 = AWBHeader.Consol.Containers.AddNew();
				uld2.JC_RC = refULD2.PK;
				uld2.JC_ContainerNum = "TESTULD2";

				AWBHeader.Populate();

				AssertEquals("2", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
				AssertEquals(Core.Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, AWBHeader.AWBRateLine1.ER_RateClass);

				const string expectedNatureAndQtyOfGoods =
	@"1|C|Consolidation as per attached list
2|U|TESTULD1
3|G|SPX
4|U|TESTULD2
5|G|SPX
6|G|No Dimensions Available";

				AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
			}
		}

		void CreateAndPackULDContainers(ConsolExportAWBHeader awbHeader, int numberOfContainers, bool setPackageCount)
		{
			CommonShipment shipment = awbHeader.Consol.Shipments.AddNew();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.OuterPackLines.RemoveAll();

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			Func<int, ForwardingContainer> createContainer = (i) =>
			{
				ForwardingContainer container = awbHeader.Consol.Containers.AddNew();
				container.JC_RC = refULD.PK;
				container.JC_ContainerNum = "TESTULD" + (i + 1);
				container.JC_ContainerMode = Core.Constants.ContainerModes.ULD;
				container.WeightUnitForBinding = Core.Constants.Weight.Kilograms;
				container.JC_TareWeight = 220;
				return container;
			};

			for (int i = 0; i < numberOfContainers; i++)
			{
				ForwardingContainer container = createContainer(i);
				PackLine packline = shipment.OuterPackLines.AddNew();
				packline.JL_ActualWeight = 280m;
				packline.JL_PackageCount = (setPackageCount) ? 1 : 0;
				packline.SetContainer(container.PK);
			}

			shipment.UpdateShipmentFromOuterPackLines();
			AssertEquals(shipment.OuterPackLines.Count, numberOfContainers);
			AssertEquals(shipment.JS_ActualWeight, (280m * numberOfContainers));
		}

		public void TestPopulateCommodityItemNumberFromULDContainer_GetsFromWhenPackLineHasEmptyCommodities()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var emptyCommodity = Factory.New<RefCommodityCode>();
			emptyCommodity.RH_Code = "ABCD";
			emptyCommodity.RH_IATACommodityItem = ZString.Empty;

			var commodityWithItem = Factory.New<RefCommodityCode>();
			commodityWithItem.RH_Code = "ZZZZ";
			commodityWithItem.RH_IATACommodityItem = "1234";

			var container = awbHeader.Consol.Containers.AddNew();
			container.JC_RC = refULD.PK;
			container.JC_ContainerNum = "TESTULD1";
			container.JC_ContainerMode = ContainerModes.ULD;
			container.WeightUnitForBinding = Weight.Kilograms;
			container.JC_TareWeight = 220;
			container.JC_RH_NKContainerCommodityCode = commodityWithItem.RH_Code;

			var packLine1 = Factory.New<ForwardingPackLine>();
			packLine1.JL_RH_NKCommodityCode = emptyCommodity.RH_Code;
			container.PackLines.Add(packLine1);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", commodityWithItem.RH_IATACommodityItem, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDPackLine_AllSameCommodity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "ABCD";
			commodity.RH_IATACommodityItem = "1111";

			var container = awbHeader.Consol.Containers.AddNew();
			container.JC_RC = refULD.PK;
			container.JC_ContainerNum = "TESTULD1";
			container.JC_ContainerMode = ContainerModes.ULD;
			container.WeightUnitForBinding = Weight.Kilograms;
			container.JC_TareWeight = 220;

			var packLine1 = Factory.New<ForwardingPackLine>();
			packLine1.JL_RH_NKCommodityCode = commodity.RH_Code;
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine1);

			var packLine2 = Factory.New<ForwardingPackLine>();
			packLine2.JL_RH_NKCommodityCode = commodity.RH_Code;
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine2);

			AssertEquals("Precondition", 2, container.PackLines.Count);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", commodity.RH_IATACommodityItem, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDPackLine_DifferentCommodities()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAAA";
			commodity1.RH_IATACommodityItem = "1111";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBBB";
			commodity2.RH_IATACommodityItem = "2222";

			var container = awbHeader.Consol.Containers.AddNew();
			container.JC_RC = refULD.PK;
			container.JC_ContainerNum = "TESTULD1";
			container.JC_ContainerMode = ContainerModes.ULD;
			container.WeightUnitForBinding = Weight.Kilograms;
			container.JC_TareWeight = 220;

			var packLine1 = Factory.New<ForwardingPackLine>();
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine1);

			var packLine2 = Factory.New<ForwardingPackLine>();
			packLine2.JL_RH_NKCommodityCode = commodity2.RH_Code;
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine2);

			AssertEquals("Precondition", 2, container.PackLines.Count);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			var defaultCommodityCode = "9999";
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", defaultCommodityCode, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDPackLine_DifferentCommodities_OneEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAAA";
			commodity1.RH_IATACommodityItem = "1111";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBBB";
			commodity2.RH_IATACommodityItem = ZString.Empty;

			var container = awbHeader.Consol.Containers.AddNew();
			container.JC_RC = refULD.PK;
			container.JC_ContainerNum = "TESTULD1";
			container.JC_ContainerMode = ContainerModes.ULD;
			container.WeightUnitForBinding = Weight.Kilograms;
			container.JC_TareWeight = 220;

			var packLine1 = Factory.New<ForwardingPackLine>();
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine1);

			var packLine2 = Factory.New<ForwardingPackLine>();
			packLine2.JL_RH_NKCommodityCode = commodity2.RH_Code;
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine2);

			AssertEquals("Precondition", 2, container.PackLines.Count);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			var defaultCommodityCode = "9999";
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", defaultCommodityCode, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDPackLine_EmptyCommodities()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var container = awbHeader.Consol.Containers.AddNew();
			container.JC_RC = refULD.PK;
			container.JC_ContainerNum = "TESTULD1";
			container.JC_ContainerMode = ContainerModes.ULD;
			container.WeightUnitForBinding = Weight.Kilograms;
			container.JC_TareWeight = 220;

			var packLine1 = Factory.New<ForwardingPackLine>();
			packLine1.JL_RH_NKCommodityCode = ZString.Empty;
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine1);

			var packLine2 = Factory.New<ForwardingPackLine>();
			packLine2.JL_RH_NKCommodityCode = ZString.Empty;
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine2);

			AssertEquals("Precondition", 2, container.PackLines.Count);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", ZString.Empty, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDPackLine_DefaultCommodities()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAAA";
			commodity.RH_IATACommodityItem = ZString.Empty;

			var container = awbHeader.Consol.Containers.AddNew();
			container.JC_RC = refULD.PK;
			container.JC_ContainerNum = "TESTULD1";
			container.JC_ContainerMode = ContainerModes.ULD;
			container.WeightUnitForBinding = Weight.Kilograms;
			container.JC_TareWeight = 220;

			var packLine = Factory.New<ForwardingPackLine>();
			packLine.JL_RH_NKCommodityCode = commodity.RH_Code;
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine);

			AssertEquals("Precondition", 1, container.PackLines.Count);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			var defaultCommodityCode = "9999";
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", defaultCommodityCode, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDContainer_AllSameCommodity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "ABCD";
			commodity.RH_IATACommodityItem = "1111";

			Func<int, string, ForwardingContainer> createContainer = (i, code) =>
			{
				var container = awbHeader.Consol.Containers.AddNew();
				container.JC_RC = refULD.PK;
				container.JC_ContainerNum = "TESTULD1";
				container.JC_ContainerMode = Core.Constants.ContainerModes.ULD;
				container.WeightUnitForBinding = Core.Constants.Weight.Kilograms;
				container.JC_TareWeight = 220;
				container.JC_RH_NKContainerCommodityCode = code;
				return container;
			};

			createContainer(1, commodity.RH_Code);
			createContainer(2, commodity.RH_Code);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", commodity.RH_IATACommodityItem, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDContainer_DifferentCommodities()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAAA";
			commodity1.RH_IATACommodityItem = "1111";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBBB";
			commodity2.RH_IATACommodityItem = "2222";

			Func<int, string, ForwardingContainer> createContainer = (i, code) =>
			{
				var container = awbHeader.Consol.Containers.AddNew();
				container.JC_RC = refULD.PK;
				container.JC_ContainerNum = "TESTULD1";
				container.JC_ContainerMode = ContainerModes.ULD;
				container.WeightUnitForBinding = Weight.Kilograms;
				container.JC_TareWeight = 220;
				container.JC_RH_NKContainerCommodityCode = code;
				return container;
			};

			createContainer(1, commodity1.RH_Code);
			createContainer(2, commodity2.RH_Code);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			var defaultCommodityCode = "9999";
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", defaultCommodityCode, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDContainer_EmptyCommodities()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			Func<int, string, ForwardingContainer> createContainer = (i, code) =>
			{
				var container = awbHeader.Consol.Containers.AddNew();
				container.JC_RC = refULD.PK;
				container.JC_ContainerNum = "TESTULD1";
				container.JC_ContainerMode = ContainerModes.ULD;
				container.WeightUnitForBinding = Weight.Kilograms;
				container.JC_TareWeight = 220;
				container.JC_RH_NKContainerCommodityCode = code;
				return container;
			};

			createContainer(1, ZString.Empty);
			createContainer(2, ZString.Empty);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", ZString.Empty, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberFromULDContainer_DefaultCommodity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			RefContainer refULD = Factory.New<RefContainer>();
			refULD.RC_IATARateClass = "123";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "ABCD";
			commodity.RH_IATACommodityItem = ZString.Empty;

			Func<int, string, ForwardingContainer> createContainer = (i, code) =>
			{
				var container = awbHeader.Consol.Containers.AddNew();
				container.JC_RC = refULD.PK;
				container.JC_ContainerNum = "TESTULD1";
				container.JC_ContainerMode = Core.Constants.ContainerModes.ULD;
				container.WeightUnitForBinding = Core.Constants.Weight.Kilograms;
				container.JC_TareWeight = 220;
				container.JC_RH_NKContainerCommodityCode = code;
				return container;
			};

			createContainer(1, commodity.RH_Code);

			awbHeader.Populate();

			var defaultRateClass = RateClass.UnitLoadDeviceBasicCharge;
			var defaultCommodityCode = "9999";
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", defaultCommodityCode, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be default", defaultRateClass, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberNonULDPackLineCommodityItem_AllSameCommodity()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			ForwardingShipment shipment = awbHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.OuterPackLines.RemoveAll();

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "ABCD";
			commodity.RH_IATACommodityItem = "1234";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_RH_NKCommodityCode = commodity.RH_Code;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_RH_NKCommodityCode = commodity.RH_Code;

			awbHeader.Populate();

			AssertEquals("Expected Commodity Item Number to be same as generated commodity", commodity.RH_IATACommodityItem, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be base", RateClass.NormalCharge, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberNonULDPackLineCommodityItem_DifferentCommodities()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			ForwardingShipment shipment = awbHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.OuterPackLines.RemoveAll();

			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAAA";
			commodity1.RH_IATACommodityItem = "1111";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBBB";
			commodity2.RH_IATACommodityItem = "2222";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_RH_NKCommodityCode = commodity2.RH_Code;

			awbHeader.Populate();

			var defaultCommodityCode = "9999";
			AssertEquals("Expected Commodity Item Number to be same as generated commodity", defaultCommodityCode, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be base", RateClass.NormalCharge, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberNonULDPackLineCommodityItem_EmptyCommodities()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			ForwardingShipment shipment = awbHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.OuterPackLines.RemoveAll();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_RH_NKCommodityCode = ZString.Empty;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_RH_NKCommodityCode = ZString.Empty;

			awbHeader.Populate();

			AssertEquals("Expected Commodity Item Number to be same as generated commodity", ZString.Empty, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be base", RateClass.NormalCharge, awbHeader.AWBRateLine1.ER_RateClass);
		}

		public void TestPopulateCommodityItemNumberNonULDPackLineCommodityItem_EmptyCommodityItems()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			ForwardingShipment shipment = awbHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.OuterPackLines.RemoveAll();

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "ABCD";
			commodity.RH_IATACommodityItem = ZString.Empty;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_RH_NKCommodityCode = commodity.RH_Code;

			awbHeader.Populate();

			AssertEquals("Expected Commodity Item Number to be same as generated commodity", ZString.Empty, awbHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals("Expected Commodity Rate Class to be base", RateClass.NormalCharge, awbHeader.AWBRateLine1.ER_RateClass);
		}

		#endregion

		public void TestCalculationLogsAnalyzerType()
		{
			AssertEquals(typeof(ConsolCalculationLogsAnalyzer), AWBHeader.CalculationLogsAnalyzer.GetType());
		}

		#region Nature And Quantity of Goods

		#region Default Behaviour of Nature and Qty of Goods

		public void TestNatureAndQtyOfGoods_Default_NoDimensions_NoVolume_WithGoodsDescription()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].DetailedGoodsDescriptionNoteText = "Some Long Description\nWith a new line";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods =
	@"1|G|Some Long Description
2|G|With a new line
3|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_Default_NoDimensions_NoVolume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods = @"1|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_Default_NoDimensions_Volume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_Default_NoDimensions_NoOfOuterPacksSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_Default_EmptyDimensions_NoOfOuterPacksSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			PackLine outerPackLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_Default_Dimensions()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_Default_Dimensions_DimensionsDontFit()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AddOuterPackLine(6, 7, 8, 8, "CM");
			AddOuterPackLine(6, 7, 8, 7, "CM");
			AddOuterPackLine(6, 7, 8, 6, "CM");
			AddOuterPackLine(6, 7, 8, 5, "CM");
			AddOuterPackLine(6, 7, 8, 4, "CM");
			AddOuterPackLine(6, 7, 8, 3, "CM");
			AddOuterPackLine(6, 7, 8, 2, "CM");
			AddOuterPackLine(6, 7, 8, 1, "CM");
			AddOuterPackLine(6, 7, 7, 1, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 8", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 7", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 6", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 5", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 4", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 3", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 2", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 1", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x7 CM x 1", AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals("54 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("63 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_NoDimensionsAvailable()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.NDA;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_NoDimensionsAvailable_WhenPackageCountIsZero()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 0, "M");
			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_AchievedQuantities()
		{
			AssertNatureAndQtyOfGoods_AchievedQuantities(Dimensions.DEF);
			AssertNatureAndQtyOfGoods_AchievedQuantities(Dimensions.ALL);
			AssertNatureAndQtyOfGoods_AchievedQuantities(Dimensions.M3);
		}

		void AssertNatureAndQtyOfGoods_AchievedQuantities(string printOptionForPackagesOnAWB)
		{
			AssertEquals("Default", Core.WeightAndVolumeDisplayTypes.Codes.Actual, Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay);
			AWBHeader.Consol.JK_OverrideConsolChargeable = false;
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = printOptionForPackagesOnAWB;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualWeight = 100M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfWeight = Weight.Pounds;
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 100M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = Volume.CubicFeet;
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			if (printOptionForPackagesOnAWB == Dimensions.ALL)
			{
				AddOuterPackLine(5, 6, 7, 20, "M");
			}
			AWBHeader.Populate();
			AssertEquals("100m", AWBHeader.Consol.Shipments[0].JS_ActualWeight, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("LB", "L", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			if (printOptionForPackagesOnAWB == Dimensions.ALL)
			{
				AssertEquals("100m", AWBHeader.Consol.Shipments[0].JS_ActualVolume, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsVolume.Volume);
				AssertEquals("CF", AWBHeader.Consol.Shipments[0].JS_UnitOfVolume, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsVolume.Unit);
			}
			else
			{
				AssertEquals("100m", AWBHeader.Consol.Shipments[0].JS_ActualVolume, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsVolume.Volume);
				AssertEquals("CF", AWBHeader.Consol.Shipments[0].JS_UnitOfVolume, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsVolume.Unit);
			}

			AWBHeader.Consol.JK_OverrideConsolChargeable = true;
			AWBHeader.Consol.JK_CorrectedConsolWeight = 200M;
			AWBHeader.Consol.JK_CorrectedConsolWeightUnit = Weight.Kilograms;
			AWBHeader.Consol.JK_CorrectedConsolVolume = 200M;
			AWBHeader.Consol.JK_CorrectedConsolVolumeUnit = Volume.CubicMetres;
			AWBHeader.Populate();
			AssertEquals("200m", AWBHeader.Consol.JK_CorrectedConsolWeight, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("KG", "K", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			if (printOptionForPackagesOnAWB == Dimensions.ALL)
			{
				AssertEquals("200m", AWBHeader.Consol.JK_CorrectedConsolVolume, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsVolume.Volume);
				AssertEquals("M3", AWBHeader.Consol.JK_CorrectedConsolVolumeUnit, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsVolume.Unit);
			}
			else
			{
				AssertEquals("200m", AWBHeader.Consol.JK_CorrectedConsolVolume, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsVolume.Volume);
				AssertEquals("M3", AWBHeader.Consol.JK_CorrectedConsolVolumeUnit, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsVolume.Unit);
			}	
		}

		#endregion

		#region ALL selected - Behavious of Nature and Qty of Goods

		public void TestNatureAndQtyOfGoods_ALL_VolumeSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Consol.Shipments[0].DetailedGoodsDescriptionNoteText = "Goods Description";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 100M;
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 90;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("Goods Description", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 100.00 M3", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("90 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_ALL_DimensionsSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Consol.Shipments[0].DetailedGoodsDescriptionNoteText = "Goods Description";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 45;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AddOuterPackLine(6, 7, 8, 45, "CM");

			AWBHeader.Populate();

			AssertEquals("Goods Description", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 45", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("45 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_ALL_VolumeAndDimenisionsAreSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Consol.Shipments[0].DetailedGoodsDescriptionNoteText = "Some Long Description\nWith a new line";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 100M;
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AddOuterPackLine(5, 6, 7, 20, "M");
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods =
	@"1|G|Some Long Description
2|G|With a new line
3|D|DIMS 500x600x700 CM x 20
4|V|VOL 100.00 M3
5|S|20 SLAC";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_ALL_VolumeAndDimenisionsAreNotSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Consol.Shipments[0].DetailedGoodsDescriptionNoteText = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods = "1|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_ALL_IncHAWBNumbers()
		{
			Guid[] countrys = new Guid[4];
			countrys[0] = Core.Constants.CountryGuids.China;
			Enterprise.Registry.Business.FreightDataRegistry.Instance.PrintHawbNumbersInBodyOfMawb.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, countrys);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Consol.Shipments[0].DetailedGoodsDescriptionNoteText = "Goods Description";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 100M;
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 90;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "CNSHI";
			AWBHeader.Consol.Shipments[0].JS_HouseBill = "HOU3333";
			AWBHeader.Consol.Shipments[0].ConsigneePK = consignee.PK;
			AWBHeader.Populate();

			AssertEquals("Goods Description", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("HAWBS: HOU3333", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 100.00 M3", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("90 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		#endregion

		#region VOL selected - Behaviour of Nature and Qty of Goods

		public void TestNatureAndQtyOfGoods_VOL_NoVolume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.M3;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 12;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AddOuterPackLine(1, 2, 3, 4, "M");
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods = "1|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_VOL_VolumeSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.M3;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AddOuterPackLine(1, 2, 3, 4, "M");
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		#endregion

		#region PKS selected - Behaviour of Nature and Qty of Goods

		public void TestNatureAndQtyOfGoods_GoodsDescriptionNoNote()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "Some crap";
			AWBHeader.Populate();

			AssertEquals("Some crap", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_PKS_NoDimensions_NoVolume_WithGoodsDescription()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Consol.Shipments[0].DetailedGoodsDescriptionNoteText = "Some Long Description\nWith a new line";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods =
	@"1|G|Some Long Description
2|G|With a new line
3|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_PKS_NoDimensions_NoVolume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods = "1|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_PKS_NoDimensions_Volume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods = "1|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_PKS_NoDimensions_NoOfOuterPacksSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods = "1|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_PKS_EmptyDimensions_NoOfOuterPacksSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			PackLine outerPackLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods = "1|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		public void TestNatureAndQtyOfGoods_PKS_Dimensions()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_PKS_Dimensions_DimensionsDontFit()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AddOuterPackLine(6, 7, 8, 8, "CM");
			AddOuterPackLine(6, 7, 8, 7, "CM");
			AddOuterPackLine(6, 7, 8, 6, "CM");
			AddOuterPackLine(6, 7, 8, 5, "CM");
			AddOuterPackLine(6, 7, 8, 4, "CM");
			AddOuterPackLine(6, 7, 8, 3, "CM");
			AddOuterPackLine(6, 7, 8, 2, "CM");
			AddOuterPackLine(6, 7, 8, 1, "CM");
			AddOuterPackLine(6, 7, 7, 1, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 8", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 7", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 6", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 5", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 4", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 3", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 2", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 1", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x7 CM x 1", AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals("54 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 8", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 7", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 6", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 5", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 4", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 3", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 2", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 1", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x7 CM x 1", AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals("63 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		#endregion

		#region PopulateLithiumBatteryStatements

		public void TestPopulateLithiumBatteryStatements_GetsPackingInstructionsForLithiumUNNOCodes()
		{
			var expectedValue = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Value;
			AWBHeader.Consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "BOX";

			var undgDataItem = packLine.UNDGs.AddNew();

			foreach (var unno in LithiumBatteryConstants.UNNOCodes.CodesList)
			{
				var lithiumDGSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
				lithiumDGSubstance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;
				lithiumDGSubstance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;
				lithiumDGSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				lithiumDGSubstance.DG_UNNO = unno;

				undgDataItem.DI_DG = lithiumDGSubstance.PK;
				undgDataItem.DI_PackingInstructionSection = "II";

				AWBHeader.Consol.PopulateAWB();

				Assert($"AWB RateLines should add a lithium battery statement for a dangerous good with UNNO code {unno}",
					DoesAnyAWBRateLineContainLithiumBatteryPackType(expectedValue));
			}
		}

		public void TestPopulateLithiumBatteryStatements_GetsPackingInstructionsForLithiumUNNOCodes_WhenRateLinesAreAutorated()
		{
			AWBHeader.Consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "BOX";

			var lithiumDGSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			lithiumDGSubstance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			lithiumDGSubstance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;
			lithiumDGSubstance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;
			lithiumDGSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			var undgDataItem = packLine.UNDGs.AddNew();
			undgDataItem.DI_DG = lithiumDGSubstance.PK;
			undgDataItem.DI_PackingInstructionSection = "II";

			var packingInstruction = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Value;

			var wrapper = new CalculationLogsWrapper();
			var log = new CalculationLog();
			log.Unit = Constants.Weight.Kilograms;
			log.RateMode = Constants.ContainerModes.ULD;
			log.BaseRate = 12m;
			log.IsCosting = false;
			wrapper.Logs.Add(log);

			var autoRatingLogRevenue = AWBHeader.Consol.Notes.AddNew();
			autoRatingLogRevenue.ST_Description = "AutoRating Calculation Log Revenue";
			autoRatingLogRevenue.ST_NoteData = ZBlob.FromUTF8(wrapper.Serialize());

			Factory.Save();

			AWBHeader.Consol.PopulateAWB();
			Assert("Lithium battery statement should be populated",
				DoesAnyAWBRateLineContainLithiumBatteryPackType(packingInstruction));
		}

		public void TestPopulateLithiumBatteryStatements_TranslatesPackingInstructionToLithiumBatteryPackType()
		{
			AWBHeader.Consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "BOX";

			var lithiumDGSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			lithiumDGSubstance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			lithiumDGSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			var undgDataItem = packLine.UNDGs.AddNew();
			undgDataItem.DI_DG = lithiumDGSubstance.PK;
			undgDataItem.DI_PackingInstructionSection = "II";

			foreach (var packingInstructionPair in LithiumBatteryConstants.RefPackingInstructionCodeDictionary)
			{
				lithiumDGSubstance.DG_CargoPackIns = packingInstructionPair.Key;
				lithiumDGSubstance.DG_PaxPackIns = packingInstructionPair.Key;

				AWBHeader.Consol.PopulateAWB();

				Assert($"AWB RateLines should translate the packaging instruction '{packingInstructionPair.Key}' to code {packingInstructionPair.Value}",
					DoesAnyAWBRateLineContainLithiumBatteryPackType(packingInstructionPair.Value));
			}
		}

		public void TestPopulateLithiumBatteryStatements_MutliplePackingInstructions()
		{
			AWBHeader.Consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "BOX";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = "BAG";

			var lithiumDGSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			lithiumDGSubstance1.DG_UNNO = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			lithiumDGSubstance1.DG_Variant = "A";
			lithiumDGSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			lithiumDGSubstance1.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;
			lithiumDGSubstance1.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;

			var packingInstruction1 = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Value;

			var lithiumDGSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			lithiumDGSubstance2.DG_UNNO = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			lithiumDGSubstance1.DG_Variant = "B";
			lithiumDGSubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			lithiumDGSubstance2.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.Last().Key;
			lithiumDGSubstance2.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.Last().Key;

			var packingInstruction2 = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.Last().Value;

			var undgDataItem1 = packLine1.UNDGs.AddNew();
			undgDataItem1.DI_DG = lithiumDGSubstance1.PK;
			undgDataItem1.DI_PackingInstructionSection = "II";

			var undgDataItem2 = packLine2.UNDGs.AddNew();
			undgDataItem2.DI_DG = lithiumDGSubstance2.PK;
			undgDataItem2.DI_PackingInstructionSection = "II";

			AssertNotEquals("PRE: Packing instruction types are different", packingInstruction1, packingInstruction2);

			AWBHeader.Consol.PopulateAWB();

			Assert("AWB RateLine with two lithium battery packing instructions should display details about first instruction",
				DoesAnyAWBRateLineContainLithiumBatteryPackType(packingInstruction1));
			Assert("AWB RateLine with two lithium battery packing instructions should display details about second instruction",
				DoesAnyAWBRateLineContainLithiumBatteryPackType(packingInstruction2));
		}

		public void TestPopulateLithiumBatteryStatements_MutliplePackingInstructions_DoesntRepeatSameInstruction()
		{
			AWBHeader.Consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "BOX";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = "BAG";

			var lithiumDGSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			lithiumDGSubstance1.DG_UNNO = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			lithiumDGSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			lithiumDGSubstance1.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;
			lithiumDGSubstance1.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;

			var packingInstruction1 = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Value;

			var lithiumDGSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			lithiumDGSubstance2.DG_UNNO = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			lithiumDGSubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			lithiumDGSubstance2.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;
			lithiumDGSubstance2.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Key;

			var packingInstruction2 = LithiumBatteryConstants.RefPackingInstructionCodeDictionary.First().Value;

			var undgDataItem1 = packLine1.UNDGs.AddNew();
			undgDataItem1.DI_DG = lithiumDGSubstance1.PK;
			undgDataItem1.DI_PackingInstructionSection = "II";

			AssertEquals("PRE: Packing instruction types are the same", packingInstruction1, packingInstruction2);

			AWBHeader.Consol.PopulateAWB();
			var numberOfRateLines1 = AWBHeader.AWBRateLines.Count;

			Assert("AWB RateLine with one lithium battery packing instruction should display details about the first instruction",
				DoesAnyAWBRateLineContainLithiumBatteryPackType(packingInstruction1));

			var undgDataItem2 = packLine2.UNDGs.AddNew();
			undgDataItem2.DI_DG = lithiumDGSubstance2.PK;
			undgDataItem2.DI_PackingInstructionSection = "II";

			AWBHeader.Consol.PopulateAWB();
			var numberOfRateLines2 = AWBHeader.AWBRateLines.Count;

			AssertEquals("AWB RateLine with a second identical lithium battery packing instruction should not repeat the first instruction",
				numberOfRateLines1, numberOfRateLines2);
		}

		public void TestPopulateLithiumBatteryStatements_PI965orPI968_AddsCargoAircraftOnlyMessage()
		{
			AWBHeader.Consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "BOX";

			var lithiumDGSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			lithiumDGSubstance.DG_UNNO = LithiumBatteryConstants.UNNOCodes.CodesList.First();
			lithiumDGSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			var undgDataItem = packLine.UNDGs.AddNew();
			undgDataItem.DI_DG = lithiumDGSubstance.PK;
			undgDataItem.DI_PackingInstructionSection = "II";

			lithiumDGSubstance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			lithiumDGSubstance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			AWBHeader.Consol.PopulateAWB();

			Assert("AWB with lithium battery packing type 'PI966' should not add a 'Cargo Aircraft Only' line",
				!DoesAnyAWBRateLineContainDescription("Cargo Aircraft Only"));

			undgDataItem.Substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			undgDataItem.Substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
			AWBHeader.Consol.PopulateAWB();

			Assert("AWB with lithium battery packing type 'PI965' should add a 'Cargo Aircraft Only' line",
				DoesAnyAWBRateLineContainDescription("Cargo Aircraft Only"));

			undgDataItem.Substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI968;
			undgDataItem.Substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI968;
			AWBHeader.Consol.PopulateAWB();

			Assert("AWB with lithium battery packing type 'PI968' should add a 'Cargo Aircraft Only' line",
				DoesAnyAWBRateLineContainDescription("Cargo Aircraft Only"));
		}

		public void TestTestPopulateLithiumBatteryStatements_ShowWhilePackInsSectionII()
		{
			AWBHeader.Consol.JK_TransportMode = Constants.TransportModes.Air;

			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "BOX";

			var undgDataItem = packLine.UNDGs.AddNew();

			foreach (var unno in LithiumBatteryConstants.UNNOCodes.CodesList)
			{
				var substance = Factory.NewWithValidTestData<UNDGSubstance>();
				substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
				substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
				substance.DG_UNNO = unno;
				undgDataItem.DI_DG = substance.PK;
				undgDataItem.DI_PackingInstructionSection = "II";
				AWBHeader.Consol.PopulateAWB();
				Assert("when Section II is specified, 'L' line statement should show", AWBHeader.AWBRateLines.Cast<ExportAWBRateLine>().Any(line => line.NatureAndQtyOfGoodsType == "L"));
			}

			foreach (var unno in new string[] { "3480", "3090" })
			{
				var substance = Factory.NewWithValidTestData<UNDGSubstance>();
				substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
				substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
				substance.DG_UNNO = unno;
				undgDataItem.DI_DG = substance.PK;
				undgDataItem.DI_PackingInstructionSection = "IA";
				AWBHeader.Consol.PopulateAWB();
				Assert("when Section IA is specified, 'L' line statement should not show", AWBHeader.AWBRateLines.Cast<ExportAWBRateLine>().All(line => line.NatureAndQtyOfGoodsType != "L"));

				undgDataItem.DI_PackingInstructionSection = "IB";
				AWBHeader.Consol.PopulateAWB();
				Assert("when Section IB is specified, 'L' line statement should not show", AWBHeader.AWBRateLines.Cast<ExportAWBRateLine>().All(line => line.NatureAndQtyOfGoodsType != "L"));
			}

			foreach (var unno in new string[] { "3481", "3091" })
			{
				var substance = Factory.NewWithValidTestData<UNDGSubstance>();
				substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
				substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
				substance.DG_UNNO = unno;
				undgDataItem.DI_DG = substance.PK;
				undgDataItem.DI_PackingInstructionSection = "I";
				AWBHeader.Consol.PopulateAWB();
				Assert("when Section I is specified, 'L' line statement should not show", AWBHeader.AWBRateLines.Cast<ExportAWBRateLine>().All(line => line.NatureAndQtyOfGoodsType != "L"));
			}

			foreach (var unno in new string[] { "2395", "3274" })
			{
				var substance = Factory.NewWithValidTestData<UNDGSubstance>();
				substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
				substance.DG_PaxPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965;
				substance.DG_UNNO = unno;
				undgDataItem.DI_DG = substance.PK;
				AWBHeader.Consol.PopulateAWB();
				Assert("when UNNO is not UN 3480, UN 3481, UN 3090, or UN 3091, 'L' line statement should not show", AWBHeader.AWBRateLines.Cast<ExportAWBRateLine>().All(line => line.NatureAndQtyOfGoodsType != "L"));
			}
		}

		public bool DoesAnyAWBRateLineContainLithiumBatteryPackType(string lithiumBatteryPackType)
			=> AWBHeader.Consol.AWBHeader.AWBRateLines.OfType<ExportAWBRateLine>()
				.Any(rateLine => rateLine.NatureAndQtyOfGoodsType == Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery
					&& rateLine.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType == lithiumBatteryPackType);

		public bool DoesAnyAWBRateLineContainDescription(string description)
			=> AWBHeader.Consol.AWBHeader.AWBRateLines.OfType<ExportAWBRateLine>()
				.Any(rateLine => rateLine.NatureAndQtyOfGoodsType == Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription
					&& rateLine.NatureAndQtyOfGoodsDescription == description);

		#endregion

		#region Excluded Dangerous Goods

		static bool RequiresMeasurementDetailsUNDGs(ZString unno)
		{
			return unno == ShippersDeclarationUNDGExclusions.UNNOCodes.UN1845;
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			DGSubstanceTestHelper.Create(firstShippersDeclarationUNDGExclusionCode, "Z", "IMO");
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, firstShippersDeclarationUNDGExclusionCode, standard: "IMO", variant: "Z").FirstOrDefault();
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var undg2 = packLineA.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 15;
			undg2.DI_DGVolume = 7.986;
			undg2.DI_UnitOfVolume = Volume.MegaLitre;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			packLineB.JL_PackageCount = 9;
			packLineB.JL_ActualWeight = 5.555;
			packLineB.JL_ActualWeightUQ = Weight.Grams;

			var undg3 = packLineB.UNDGs.AddNew();
			undg3.DI_DG = undgExcludedSubstance1.PK;
			undg3.LinkDefault(undgExcludedSubstance1);

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot1.DP_Variant = "Z";

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot2.DP_Variant = "Z";

			var uNDGSubstancePivot3 = undg3.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot3.DP_IsDefault = true;
			uNDGSubstancePivot3.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot3.DP_Variant = "Z";

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);
			AssertEquals("Precondition", undg3.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO, pack count and weight of the first dangerous good",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (5x1.234KG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line is UNNO, pack count and weight of the next dangerous good",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (15x7.986ML)", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);

				AssertEquals("4th line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);

				AssertEquals("5th line is UNNO, pack count and weight of the next dangerous good",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (9x5.555G)", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsText.Text);

				AssertEquals("6th line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_ExceptedQuantity()
		{
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "ZZZ";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "ZZZ", standard: "IMO").FirstOrDefault();
			undgExcludedSubstance1.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E1;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "XXX";
			subs2.DG_Variant = "";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgExcludedSubstance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "XXX", standard: "IMO").FirstOrDefault();
			undgExcludedSubstance2.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E5;
			undgExcludedSubstance2.DG_PSN = "Cyanide";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Grams;

			var undg2 = packLineA.UNDGs.AddNew();
			undg2.DI_DG = subs2.PK;
			undg2.LinkDefault(subs2);
			undg2.DI_PackageCount = 15;
			undg2.DI_DGWeight = 7.986;
			undg2.DI_UnitOfWeight = Weight.Grams;

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by Excepted Quantity)", () =>
			{
				AssertEquals("1st line is UNNO, pack count and weight of the first dangerous good (excluded by Excepted Quantity)",
					$"UN {undg1.Substance.DG_UNNO} (5 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line is part of the excepted quantity disclaimer",
					"Dangerous Goods in", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);

				AssertEquals("4th part of the excepted quantity disclaimer",
					"Excepted Quantities", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);

				AssertEquals("5th line is UNNO of the second dangerous good (excluded by Excepted Quantity)",
					$"UN {undg2.Substance.DG_UNNO} (15 PKG)", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsText.Text);

				AssertEquals("6th line is the Proper Shipping Name of the dangerous good",
					"Cyanide", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsText.Text);

				AssertEquals("7th line is part of the excepted quantity disclaimer",
					"Dangerous Goods in", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsText.Text);

				AssertEquals("8th part of the excepted quantity disclaimer",
					"Excepted Quantities", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_DoesNotOverflowRateLines()
		{
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			DGSubstanceTestHelper.Create("XXX", "", "IMO");
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "XXX", standard: "IMO").FirstOrDefault();
			undgExcludedSubstance1.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E1;
			undgExcludedSubstance1.DG_PSN = "Poison";

			DGSubstanceTestHelper.Create("ZZZ", "", "IMO");
			var undgExcludedSubstance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "ZZZ", standard: "IMO").FirstOrDefault();
			undgExcludedSubstance2.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E5;
			undgExcludedSubstance2.DG_PSN = "Cyanide";

			DGSubstanceTestHelper.Create("AAA", "", "IMO");
			var undgExcludedSubstance3 = UNDGSubstanceLoader.LoadSubstances(Factory, "AAA", standard: "IMO").FirstOrDefault();
			undgExcludedSubstance3.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E5;
			undgExcludedSubstance3.DG_PSN = "Chemicals";

			DGSubstanceTestHelper.Create("BBB", "", "IMO");
			var undgExcludedSubstance4 = UNDGSubstanceLoader.LoadSubstances(Factory, "BBB", standard: "IMO").FirstOrDefault();
			undgExcludedSubstance4.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E5;
			undgExcludedSubstance4.DG_PSN = "Hydrogen Peroxide";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Grams;

			var undg2 = packLineA.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance2.PK;
			undg2.LinkDefault(undgExcludedSubstance2);
			undg2.DI_PackageCount = 15;
			undg2.DI_DGWeight = 7.986;
			undg2.DI_UnitOfWeight = Weight.Grams;

			var undg3 = packLineA.UNDGs.AddNew();
			undg3.DI_DG = undgExcludedSubstance3.PK;
			undg3.LinkDefault(undgExcludedSubstance3);
			undg3.DI_PackageCount = 1;
			undg3.DI_DGWeight = 0.001;
			undg3.DI_UnitOfWeight = Weight.Grams;

			var undg4 = packLineA.UNDGs.AddNew();
			undg4.DI_DG = undgExcludedSubstance4.PK;
			undg4.LinkDefault(undgExcludedSubstance4);
			undg4.DI_PackageCount = 1;
			undg4.DI_DGWeight = 0.123;
			undg4.DI_UnitOfWeight = Weight.Grams;

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines does not overflow ratelines", () =>
			{
				AssertEquals("UNNO of the first dangerous good (excluded by Excepted Quantity)",
					$"UN {undg1.Substance.DG_UNNO} (5 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("UNNO of the second dangerous good (excluded by Excepted Quantity)",
					$"UN {undg2.Substance.DG_UNNO} (15 PKG)", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsText.Text);

				AssertEquals("Ending of the second dangerous good excepted quantity disclaimer",
					"Excepted Quantities", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsText.Text);

				AssertNotEquals("UNNO and other details of the third dangerous good should not be displayed as it would cause the rate lines to overflow",
					$"UN {undg3.Substance.DG_UNNO} (1 PKG)", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_DisplaysAfterGoodsDesc()
		{
			var unnoCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = unnoCode;
			subs.DG_Variant = "Z";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			AWBHeader.Consol.JK_AgentType = AgentType.Direct;
			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "I am a goods desc";
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			var packLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			var undg = packLine.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines display after shipment goods description", () =>
			{
				AssertEquals("First detail line is shipment goods description",
						"I am a goods desc", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsText.Text);

				AssertEquals("Second detail line is start of excluded dangerous goods detail",
					$"UN {unnoCode} (0 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_NoDimensionsAvailable()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			subs.DG_Variant = "Z";
			subs.DG_Standard = "IMO";
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, firstShippersDeclarationUNDGExclusionCode, standard: "IMO", variant: "Z").FirstOrDefault();
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_PackageCount = 0;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var undg2 = packLine.UNDGs.AddNew();
			undg2.DI_DG = subs.PK;
			undg2.LinkDefault(subs);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGVolume = 0;
			undg2.DI_UnitOfVolume = Volume.MegaLitre;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot1.DP_Variant = "Z";

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot2.DP_Variant = "Z";

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and does not contain weight information",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (6 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line states no dimensions available",
					"No Dimensions Available", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_WithMeasurementDetails_SamePackline_SameUNNOIsCombined()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, firstShippersDeclarationUNDGExclusionCode, standard: "IMO").FirstOrDefault();
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var undg2 = packLine.UNDGs.AddNew();
			undg2.DI_DG = subs.PK;
			undg2.LinkDefault(subs);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGWeight = 1.234;
			undg2.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and combined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (11x1.234KG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_WithMeasurementDetails_SamePackline_DifferentSubstanceWeightVolumeNotCombined()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x));
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, firstShippersDeclarationUNDGExclusionCode, standard: "IMO").FirstOrDefault();
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var undg2 = packLine.UNDGs.AddNew();
			undg2.DI_DG = subs.PK;
			undg2.LinkDefault(subs);
			undg2.DI_PackageCount = 15;
			undg2.DI_DGVolume = 7.986;
			undg2.DI_UnitOfVolume = Volume.MegaLitre;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and uncombined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (5x1.234KG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line is UNNO of the next dangerous good and uncombined volume",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (15x7.986ML)", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);

				AssertEquals("4th line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_WithMeasurementDetails_MultiplePacklines_SubstanceWeightVolumeIsCombined()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			DGSubstanceTestHelper.Create(firstShippersDeclarationUNDGExclusionCode, "", "IMO");
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, firstShippersDeclarationUNDGExclusionCode, standard: "IMO").FirstOrDefault();
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = subs.PK;
			undg2.LinkDefault(subs);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGWeight = 1.234;
			undg2.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and combined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (11x1.234KG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_WithMeasurementDetails_MultiplePacklines_DifferentSubstanceWeightVolumeNotCombined()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions
				.UNNOCodes
				.ExclusionList
				.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, firstShippersDeclarationUNDGExclusionCode, standard: "IMO").FirstOrDefault();
			undgExcludedSubstance1.DG_PSN = "Poison";

			var packLineA = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLineA.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLineB = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg2 = packLineB.UNDGs.AddNew();
			undg2.DI_DG = subs.PK;
			undg2.LinkDefault(subs);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGVolume = 2.345;
			undg2.DI_UnitOfVolume = Volume.MegaLitre;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and uncombined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (5x1.234KG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line is UNNO of the next dangerous good and uncombined volume",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (6x2.345ML)", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);

				AssertEquals("4th line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_WithMeasurementDetails_MultipleShipments_WeightVolumeNotCombined()
		{
			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "JMKIN";

			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && RequiresMeasurementDetailsUNDGs(x));
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Consol.Shipments[1].OuterPackLines.RemoveAndDeleteAll();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgExcludedSubstance1 = UNDGSubstanceLoader.LoadSubstances(Factory, firstShippersDeclarationUNDGExclusionCode, standard: "IMO").FirstOrDefault();
			undgExcludedSubstance1.DG_PSN = "Poison";

			var shipment1_PackLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = shipment1_PackLine.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var shipment2_PackLine = AWBHeader.Consol.Shipments[1].OuterPackLines.AddNew();

			var undg2 = shipment2_PackLine.UNDGs.AddNew();
			undg2.DI_DG = subs.PK;
			undg2.LinkDefault(subs);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGWeight = 1.234;
			undg2.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and uncombined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (5x1.234KG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line is UNNO of the next dangerous good and uncombined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (6x1.234KG)", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);

				AssertEquals("4th line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_WithoutMeasurementDetails_SamePackline_SameUNNOIsCombined()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));
			var lastShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.Last(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Variant = "Z";
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var undgExcludedSubstance2 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance2.DG_UNNO = lastShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance2.DG_Variant = "Z";
			undgExcludedSubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance2.DG_PSN = "Cyanide";

			var packLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var undg2 = packLine.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGVolume = 3.545;
			undg2.DI_UnitOfVolume = Volume.CubicMetres;

			var undg3 = packLine.UNDGs.AddNew();
			undg3.DI_DG = undgExcludedSubstance2.PK;
			undg3.LinkDefault(undgExcludedSubstance2);
			undg3.DI_PackageCount = 6;
			undg3.DI_DGWeight = 1.234;
			undg3.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot1.DP_Variant = "Z";

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot2.DP_Variant = "Z";

			var uNDGSubstancePivot3 = undg3.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot3.DP_IsDefault = true;
			uNDGSubstancePivot3.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot3.DP_Variant = "Z";

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);
			AssertEquals("Precondition", undg3.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and combined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (11 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line is UNNO of the next dangerous good and uncombined weight",
					$"UN {lastShippersDeclarationUNDGExclusionCode} (6 PKG)", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);

				AssertEquals("4th line is the Proper Shipping Name of the dangerous good",
					"Cyanide", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_WithoutMeasurementDetails_DifferentPackline_SameUNNOIsCombined()
		{
			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));
			var lastShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.Last(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Variant = "Z";
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var undgExcludedSubstance2 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance2.DG_UNNO = lastShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance2.DG_Variant = "Z";
			undgExcludedSubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance2.DG_PSN = "Cyanide";

			var packLine1 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLine1.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var packLine2 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg2 = packLine2.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGVolume = 3.545;
			undg2.DI_UnitOfVolume = Volume.CubicMetres;

			var packLine3 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg3 = packLine3.UNDGs.AddNew();
			undg3.DI_DG = undgExcludedSubstance2.PK;
			undg3.LinkDefault(undgExcludedSubstance2);
			undg3.DI_PackageCount = 6;
			undg3.DI_DGWeight = 1.234;
			undg3.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot1.DP_Variant = "Z";

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot2.DP_Variant = "Z";

			var uNDGSubstancePivot3 = undg3.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot3.DP_IsDefault = true;
			uNDGSubstancePivot3.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot3.DP_Variant = "Z";

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);
			AssertEquals("Precondition", undg3.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and combined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (11 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line is UNNO of the next dangerous good and uncombined weight",
					$"UN {lastShippersDeclarationUNDGExclusionCode} (6 PKG)", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);

				AssertEquals("4th line is the Proper Shipping Name of the dangerous good",
					"Cyanide", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);
			});
		}

		public void TestNatureAndQtyOfGoods_ExcludedDangerousGoodDetailsLines_WithoutMeasurementDetails_DifferentShipment_SameUNNOIsNotCombined()
		{
			var shipment = AWBHeader.Consol.Shipments.AddNew();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "JMKIN";

			var firstShippersDeclarationUNDGExclusionCode = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First(x => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(x) && !RequiresMeasurementDetailsUNDGs(x));
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Consol.Shipments[1].OuterPackLines.RemoveAndDeleteAll();

			var undgExcludedSubstance1 = Factory.New<UNDGSubstance>();
			undgExcludedSubstance1.DG_UNNO = firstShippersDeclarationUNDGExclusionCode;
			undgExcludedSubstance1.DG_Variant = "Z";
			undgExcludedSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgExcludedSubstance1.DG_PSN = "Poison";

			var shipment1_PackLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = shipment1_PackLine.UNDGs.AddNew();
			undg1.DI_DG = undgExcludedSubstance1.PK;
			undg1.LinkDefault(undgExcludedSubstance1);
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var shipment2_PackLine = AWBHeader.Consol.Shipments[1].OuterPackLines.AddNew();

			var undg2 = shipment2_PackLine.UNDGs.AddNew();
			undg2.DI_DG = undgExcludedSubstance1.PK;
			undg2.LinkDefault(undgExcludedSubstance1);
			undg2.DI_PackageCount = 6;
			undg2.DI_DGWeight = 1.234;
			undg2.DI_UnitOfWeight = Weight.Kilograms;

			var uNDGSubstancePivot1 = undg1.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot1.DP_IsDefault = true;
			uNDGSubstancePivot1.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot1.DP_Variant = "Z";

			var uNDGSubstancePivot2 = undg2.UNDGSubstancePivotCollection.AddNew();
			uNDGSubstancePivot2.DP_IsDefault = true;
			uNDGSubstancePivot2.DP_UNNO = firstShippersDeclarationUNDGExclusionCode;
			uNDGSubstancePivot2.DP_Variant = "Z";

			AssertEquals("Precondition", undg1.Substance != null, true);
			AssertEquals("Precondition", undg2.Substance != null, true);

			AWBHeader.Populate();

			CombineAssertions("Excluded dangerous goods detail lines (excluded by UNNO)", () =>
			{
				AssertEquals("1st line is UNNO of the first dangerous good and uncombined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (5 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

				AssertEquals("2nd line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);

				AssertEquals("3rd line is UNNO of the next dangerous good and uncombined weight",
					$"UN {firstShippersDeclarationUNDGExclusionCode} (6 PKG)", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);

				AssertEquals("4th line is the Proper Shipping Name of the dangerous good",
					"Poison", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);
			});
		}

		#endregion

		public void TestNatureAndQtyOfGoods_IsLoadConsolAsPerList()
		{
			var agentTypes = new List<string>
			{
				AgentType.Direct,
				AgentType.CoLoad,
				AgentType.Agent,
				AgentType.Charter,
				AgentType.OnBoardCourier,
				AgentType.Other,
				AgentType.AWBCoload
			};
			var printOptionForPackagesOnAWBs = new List<string>
			{
				Constants.AWB.Dimensions.M3,
				Constants.AWB.Dimensions.PKS,
				Constants.AWB.Dimensions.DEF,
				Constants.AWB.Dimensions.NDA,
				""
			};

			foreach (var agentType in agentTypes)
			{
				foreach (var printOptionForPackagesOnAWB in printOptionForPackagesOnAWBs)
				{
					TestNatureAndQtyOfGoods_IsLoadConsolAsPerList(agentType, printOptionForPackagesOnAWB);
				}
			}
		}

		public void TestNatureAndQtyOfGoods_IsLoadConsolAsPerList(string agentType, string printOptionForPackagesOnAWB)
		{
			AWBHeader.Consol.JK_AgentType = agentType;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = printOptionForPackagesOnAWB;

			AWBHeader.Consol.Shipments[0].JS_GoodsDescription = "";
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 0M;
			AWBHeader.Consol.Shipments[0].JS_OuterPacks = 0;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			AWBHeader.Populate();
			if (agentType == AgentType.Direct)
			{
				AssertNatureAndQtyOfGoods(AWBHeader, "1|G|No Dimensions Available");
			}
			else
			{
				AssertNatureAndQtyOfGoods(AWBHeader, @"1|C|Consolidation as per attached list
2|G|No Dimensions Available");
			}

			if (new List<string> { Constants.AWB.Dimensions.M3, Constants.AWB.Dimensions.DEF, "" }.Contains(printOptionForPackagesOnAWB))
			{
				AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
				AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";

				AWBHeader.Populate();
				if (agentType == AgentType.Direct)
				{
					AssertNatureAndQtyOfGoods(AWBHeader, "1|V|VOL 10.00 M3");
				}
				else
				{
					AssertNatureAndQtyOfGoods(AWBHeader, @"1|C|Consolidation as per attached list
2|V|VOL 10.00 M3");
				}
			}
		}

		public void TestSLAC_ULD()
		{
			AWBHeader.Consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			AWBHeader.Consol.Shipments[0].JS_PackingMode = Core.Constants.ContainerModes.ULD;

			ForwardingContainer container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.ULD;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ForwardingContainer container2 = AWBHeader.Consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.ULD;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			PackLine packLine1 = AddOuterPackLine(5, 5, 5, 8, "CM");
			packLine1.JL_JC = container1.PK;
			PackLine packLine2 = AddOuterPackLine(6, 6, 6, 4, "CM");
			packLine2.JL_JC = container2.PK;

			AWBHeader.Populate();

			const string expectedNatureAndQtyOfGoods =
	@"1|C|Consolidation as per attached list
2|S|8 SLAC
3|U|CONT1111111
4|S|4 SLAC
5|U|CONT2222222
6|D|DIMS 5x5x5 CM x 8
7|D|DIMS 6x6x6 CM x 4";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		PackLine AddOuterPackLine(int length, int width, int height, int packageCount, string unitOfDimension)
		{
			PackLine outerPackLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			outerPackLine.JL_Length = length;
			outerPackLine.JL_Width = width;
			outerPackLine.JL_Height = height;
			outerPackLine.JL_PackageCount = packageCount;
			outerPackLine.JL_UnitOfDimension = unitOfDimension;

			return outerPackLine;
		}

		public void TestExportStatementIsAddedToNatureAndQtyOfGoods()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ForwardingShipment shipment1 = AWBHeader.Consol.Shipments[0];
			ForwardingShipment shipment2 = AWBHeader.Consol.Shipments.AddNew();
			shipment2.ConsignorPK = shipment1.ConsignorPK;
			shipment2.ConsigneePK = shipment1.ConsigneePK;
			shipment1.JS_RL_NKOrigin = "JMKIN";
			shipment2.JS_RL_NKOrigin = "JMKIN";

			ExportStatementSetting1.UseOnConsolidationMawb = true;
			shipment1.DocsAndCartage.JP_ExportStatement = ExportStatementSetting1.Code;
			ExportStatementSetting2.UseOnConsolidationMawb = false;
			shipment2.DocsAndCartage.JP_ExportStatement = ExportStatementSetting2.Code;
			UpdateCountryStatementSettingsToRegistry();
			AWBHeader.Populate();

			var expectedNatureAndQtyOfGoods =
	@"1|C|Consolidation as per attached list
2|G|STATEMENT FOR TESTING
3|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);

			ExportStatementSetting2.UseOnConsolidationMawb = true;
			UpdateCountryStatementSettingsToRegistry();
			AWBHeader.Populate();

			expectedNatureAndQtyOfGoods =
	@"1|C|Consolidation as per attached list
2|G|STATEMENT FOR TESTING
3|G|STATEMENT1 FOR TESTING
4|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);

			shipment1.DocsAndCartage.JP_ExportStatement = "";
			AWBHeader.Populate();

			expectedNatureAndQtyOfGoods =
	@"1|C|Consolidation as per attached list
2|G|STATEMENT1 FOR TESTING
3|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);

			ExportStatementSetting1.UseOnDirectIATAMawb = true;
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			shipment1.DocsAndCartage.JP_ExportStatement = ExportStatementSetting1.Code;
			UpdateCountryStatementSettingsToRegistry();
			AWBHeader.Populate();

			expectedNatureAndQtyOfGoods =
	@"1|G|STATEMENT FOR TESTING
2|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);

			ZString oldNatureAndQtyOfGoods = AWBHeader.NatureAndQtyOfGoods;
			ExportStatementSetting2.UseOnDirectIATAMawb = true;
			UpdateCountryStatementSettingsToRegistry();
			AWBHeader.Populate();

			expectedNatureAndQtyOfGoods =
	@"1|G|STATEMENT FOR TESTING
2|G|STATEMENT1 FOR TESTING
3|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);

			ZString oldNatureAndQtyOfGoods2 = AWBHeader.NatureAndQtyOfGoods;
			ExportStatementSetting2.Statement = ExportStatementSetting1.Statement;
			UpdateCountryStatementSettingsToRegistry();
			AWBHeader.Populate();

			expectedNatureAndQtyOfGoods =
	@"1|G|STATEMENT FOR TESTING
2|G|No Dimensions Available";

			AssertNatureAndQtyOfGoods(AWBHeader, expectedNatureAndQtyOfGoods);
		}

		#endregion

		public void TestBillNumber()
		{
			AWBHeader.Consol.JK_MasterBillNum = "02711111111";
			AWBHeader.Populate();

			AssertEquals("02711111111", AWBHeader.BillNumber);
		}

		public void TestEH_OtherChargesDueCarrierPPD()
		{
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AWBHeader.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals(0M, AWBHeader.EH_OtherChargesDueCarrierPPD);
			AddOtherChargeToAWBHeader(10.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier);
			AddOtherChargeToAWBHeader(1.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent);
			AddOtherChargeToAWBHeader(2.4m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier);

			AssertEquals(12.7M, AWBHeader.EH_OtherChargesDueCarrierPPD);
		}

		public void TestAsAgreed1stAnd2nd_DefaultCorrectly()
		{
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB = Core.Constants.AWB.AsAgreedTypes.Codes.Collect;
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB = Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			Env.Registry.Freight.AirWaybill.AllowAsAgreed = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";

			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignor.PK;
			consignor.OH_Code = "ZZ1";

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeader.EH_AsAgreed1st);
			AssertEquals(Core.Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeader.EH_AsAgreed2nd);
		}

		public void TestAsAgreed1stAnd2nd_DefaultToNonForBrazil()
		{
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DKBLL";

			var awbHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = consol.PK;

			AssertEquals("Precondition: Should still default to All for DK discharge", Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Precondition: Should still default to All for DK discharge", Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			consol.JK_RL_NKDischargePort = "BRRIO";
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			awbHeader.Populate();

			AssertEquals("Precondition: Failed to set consol discharge port to Brazil", "BRRIO", consol.JK_RL_NKDischargePort);

			AssertEquals("As Agreed 1st failed to default to None with default value set to All",
				Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed1st);
			AssertEquals("As Agreed 2nd failed to default to None with default value set to All",
				Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);
		}

		public void TestEH_OtherChargesDueAgentPPD()
		{
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AWBHeader.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals(0M, AWBHeader.EH_OtherChargesDueCarrierPPD);
			AddOtherChargeToAWBHeader(10.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier);
			AddOtherChargeToAWBHeader(1.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent);
			AddOtherChargeToAWBHeader(2.4m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier);
			AddOtherChargeToAWBHeader(2.5m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent);

			AssertEquals(3.8M, AWBHeader.EH_OtherChargesDueAgentPPD);
		}

		public void TestEH_OtherChargesDueCarrierCOL()
		{
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AWBHeader.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals(0M, AWBHeader.EH_OtherChargesDueCarrierCOL);
			AddOtherChargeToAWBHeader(10.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier);
			AddOtherChargeToAWBHeader(1.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent);
			AddOtherChargeToAWBHeader(2.4m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier);

			AssertEquals(12.7M, AWBHeader.EH_OtherChargesDueCarrierCOL);
		}

		public void TestEH_OtherChargesDueAgentCOL()
		{
			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AWBHeader.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals(0M, AWBHeader.EH_OtherChargesDueCarrierCOL);
			AddOtherChargeToAWBHeader(10.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier);
			AddOtherChargeToAWBHeader(1.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent);
			AddOtherChargeToAWBHeader(2.4m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier);
			AddOtherChargeToAWBHeader(2.5m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent);

			AssertEquals(3.8M, AWBHeader.EH_OtherChargesDueAgentCOL);
		}

		void AddOtherChargeToAWBHeader(ZDecimal amount, ZString chargeCode, ZString description, EntitlementCodes entitlementCode)
		{
			var otherCharge = AWBHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_Amount = amount;
			otherCharge.EO_ChargeCode = chargeCode;
			otherCharge.EO_ChargeDescription = description;
			otherCharge.EO_EntitlementCode = entitlementCode == EntitlementCodes.Agent ? Core.Constants.AWB.EntitlementCode.Agent : Core.Constants.AWB.EntitlementCode.Carrier;
		}

		public void TestReferenceNumber()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_MasterBillNum = "02722222222";
			AWBHeader.Populate();
			AssertEquals("027-22222222", AWBHeader.ReferenceNumber);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.AWBCoload;
			AWBHeader.Consol.JK_MasterBillNum = "02722222222";
			AWBHeader.Populate();
			AssertEquals("027-22222222", AWBHeader.ReferenceNumber);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBHeader.Consol.JK_UniqueConsignRef = "TEST";
			AWBHeader.Populate();
			AssertEquals("MASTER HAWB:TEST", AWBHeader.ReferenceNumber);

			AWBHeader.Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AWBHeader.Consol.JK_UniqueConsignRef = "AAA";
			AWBHeader.Populate();
			AssertEquals("MASTER HAWB:AAA", AWBHeader.ReferenceNumber);
		}

		public void TestConsol()
		{
			AWBHeader.Consol.Delete();
			AssertNull(AWBHeader.Consol);
		}

		public void TestConsolNumber()
		{
			AWBHeader.Consol.JK_UniqueConsignRef = "TEST";
			AssertEquals("TEST", AWBHeader.ConsolNumber);
		}

		public void TestIsTaxAutoCalculated()
		{
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "AUBNE";
			AssertEquals(true, AWBHeader.IsTaxAutoCalculated);

			AWBHeader.Consol.JK_OverrideWaybillDefaults = true;
			AssertEquals(false, AWBHeader.IsTaxAutoCalculated);

			AWBHeader.Consol.JK_OverrideWaybillDefaults = false;
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, AWBHeader.IsTaxAutoCalculated);

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(true, AWBHeader.IsTaxAutoCalculated);

			AWBHeader.Consol.JK_RL_NKDischargePort = "USCHI";

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AWBHeader.IsTaxAutoCalculated);

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AWBHeader.IsTaxAutoCalculated);
		}

		public void TestMAWBPrintingStatus()
		{
			SetUpForDeparturePort("AUPER");

			AssertEquals("Printing Final MAWB false by default", ZBool.False, AWBHeader.IsPrintingFinalNeutralMAWB);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUPER";
			JobMawb jobMawb = CreateMawbForConsol();
			Factory.Save();
			ForwardingConsol consol = CreateConsolForMAWB();
			consol.MasterBillAirlinePrefix = "176";
			Factory.Save();

			AssertEquals("Printing Final MAWB false by default", ZBool.False, AWBHeader.IsPrintingFinalNeutralMAWB);
			AssertEquals("IsReprintingNeutralMAWB should be false", ZBool.False, AWBHeader.IsReprintingNeutralMAWB);
			AssertEquals("Printing draft neutral MAWB is true", ZBool.True, AWBHeader.IsPrintingDraftNeutralMAWB);

			AWBHeader.IsPrintingFinalNeutralMAWB = ZBool.True;
			AssertEquals("Printing Final MAWB set to true", ZBool.True, AWBHeader.IsPrintingFinalNeutralMAWB);
			AssertEquals("IsReprintingNeutralMAWB should be false", ZBool.False, AWBHeader.IsReprintingNeutralMAWB);
			AssertEquals("Printing draft neutral MAWB is false", ZBool.False, AWBHeader.IsPrintingDraftNeutralMAWB);

			jobMawb.JM_IsPrinted = ZBool.True;

			AWBHeader.IsPrintingFinalNeutralMAWB = ZBool.False;

			AssertEquals("Final MAWB printed, printing final MAWB should be false", ZBool.False, AWBHeader.IsPrintingFinalNeutralMAWB);
			AssertEquals("IsReprintingNeutralMAWB should be true", ZBool.True, AWBHeader.IsReprintingNeutralMAWB);
			AssertEquals("Printing draft neutral MAWB is false", ZBool.False, AWBHeader.IsPrintingDraftNeutralMAWB);
		}

		public void TestListExportStatement_CargoIMP_USTerritories()
		{
			TestListExportStatements_CargoIMP(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestListExportStatements_CargoIMP(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var defaultValue = new CountryExportStatementSettingCollection();
					var exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PRF",
						"AES", "AES Proof of Filing Citation", CusEntryNumberTypes.UnitedStates.ITN, "", "UDF", true,
						true, true, true, true, true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PDU",
						"AESPOST", "Postdeparture Citation-USPPI", "SHP", "DOE", "UDF", true, true, true, true, true,
						true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "LOW",
						"NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)", "", "", "UDF", true, true, true,
						true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

					var filer = new ExportEntryFilerID();
					filer.EntryFilerID = "111111111";
					filer.EntryFilerIDType = "D";

					ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

					var consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = TransportModes.Air;

					var header = Factory.New<ConsolExportAWBHeader>();
					header.EH_ParentID = consol.PK;

					var shipment1 = header.Consol.Shipments.AddNew();
					shipment1.JS_RL_NKOrigin = origin;
					shipment1.JS_RL_NKDestination = destination;
					shipment1.DocsAndCartage.JP_ExportStatement = "PRF";

					var cusEntryNumber1 = shipment1.CusEntryNumbers.AddNew();
					cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
					cusEntryNumber1.CE_EntryNum = "X20100101987654";

					var shipment2 = header.Consol.Shipments.AddNew();
					shipment2.JS_RL_NKOrigin = origin;
					shipment2.JS_RL_NKDestination = destination;
					shipment2.DocsAndCartage.JP_ExportStatement = "PDU";
					shipment2.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var consignor = Factory.New<OrgHeader>();
					consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345678912");
					shipment2.ConsignorPK = consignor.PK;

					var shipment3 = header.Consol.Shipments.AddNew();
					shipment3.JS_RL_NKOrigin = origin;
					shipment3.JS_RL_NKDestination = destination;
					shipment3.DocsAndCartage.JP_ExportStatement = "DWN";
					shipment3.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var shipment4 = header.Consol.Shipments.AddNew();
					shipment4.JS_RL_NKOrigin = origin;
					shipment4.JS_RL_NKDestination = destination;
					shipment4.DocsAndCartage.JP_ExportStatement = "LOW";
					shipment4.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var exportStatements = header.ExportStatements_CargoIMP;

					AssertEquals(4, exportStatements.Count);
					AssertEquals("PRF", exportStatements[0].Code);
					AssertEquals("PDU", exportStatements[1].Code);
					AssertEquals("DWN", exportStatements[2].Code);
					AssertEquals("LOW", exportStatements[3].Code);

					AssertEquals("X20100101987654", exportStatements[0].Statement.Trim());
					AssertEquals("12345678912 20101001", exportStatements[1].Statement.Trim());
					AssertEquals("111111111 20101001", exportStatements[2].Statement.Trim());
					AssertEquals(String.Empty, exportStatements[3].Statement.Trim());
				}
			}
		}

		public void TestListExportStatement_CargoIMP_USTerritories_EntryFilerIdNotSet()
		{
			TestListExportStatements_CargoIMP(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestListExportStatements_CargoIMP(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = TransportModes.Air;

					var header = Factory.New<ConsolExportAWBHeader>();
					header.EH_ParentID = consol.PK;

					var shipment = header.Consol.Shipments.AddNew();
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;
					shipment.DocsAndCartage.JP_ExportStatement = "DWN";
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);
					
					var exportStatements = header.ExportStatements_CargoIMP;

					AssertEquals("DWN", exportStatements[0].Code);
					AssertEquals("20101001", exportStatements[0].Statement.Trim());
				}
			}
		}

		JobMawb CreateMawbForConsol()
		{
			JobMawb jobMawb = base.Factory.New<JobMawb>();
			jobMawb.JM_Airline3DigitPrefix = "176";
			jobMawb.JM_MAWB = "10000001";
			jobMawb.JM_ServiceLevel = "STD";
			jobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			return jobMawb;
		}

		ForwardingConsol CreateConsolForMAWB()
		{
			ForwardingConsol consol = AWBHeader.Consol;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.True;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			return consol;
		}

		public void TestExtraCarrierInfoLine2()
		{
			AssertEquals("Carrier line info should be blank", "", AWBHeader.ExtraCarrierInfoLine2);

			var airline1 = Factory.New<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";
			airline1.RM_TwoCharacterCode = "ZZ";
			airline1.RM_AirlineName1 = "AIR ZZ";

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "ZOrganisation 1";
			org1.OH_Code = "ZZZZ";
			org1.MainAddress.OA_Address1 = "Addr1";
			org1.MiscServ.OM_RM_Airline = airline1.PK;

			AWBHeader.Consol.JK_MasterBillNum = "6663434";

			Env.Registry.Freight.AirWaybill.MAWBDefaultCarrierText = "Test Carrier Text";
			AssertEquals("Extra Carrier Info should equal", "Test Carrier Text: AIR ZZ", AWBHeader.ExtraCarrierInfoLine2);

			Env.Registry.Freight.AirWaybill.MAWBDefaultCarrierText = "Carrier Text that is longer than 64 characters blah blah this";
			AssertEquals("Extra Carrier Info should equal", "Carrier Text that is longer than 64 characters blah blah this: A", AWBHeader.ExtraCarrierInfoLine2);
		}

		public void TestExtraShipperInfoLine1()
		{
			AssertEquals("Shipper line info should be blank", "", AWBHeader.EH_ExtraShipperInfoLine1);

			Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText = "Test Shipper Text";
			AssertEquals("Extra Carrier Info should equal", "Test Shipper Text", AWBHeader.ExtraShipperInfoLine1);

			Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText = "Shipper Text that is longer than 64 characters blah blah.trumpet this should not appear";
			AssertEquals("Extra Carrier Info should equal", "Shipper Text that is longer than 64 characters blah blah.trumpet", AWBHeader.ExtraShipperInfoLine1);
		}

		public void TestExtraShipperInfoLine2()
		{
			AssertEquals("Shipper line info should be blank", "", AWBHeader.EH_ExtraShipperInfoLine2);

			Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText = "Test Shipper Text less than 64 char";
			AssertEquals("Shipper line info should be blank", "", AWBHeader.EH_ExtraShipperInfoLine2);

			Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText = "Shipper Text that is longer than 64 characters blah blah.trumpetthis should appear line 2";
			AssertEquals("Extra Carrier Info should equal", "this should appear line 2", AWBHeader.ExtraShipperInfoLine2);

			Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText += " Some more characters to push the string over 128 characters";
			AssertEquals("Extra Carrier Info should equal", "this should appear line 2 Some more characters to push the strin", AWBHeader.ExtraShipperInfoLine2);
		}

		public void TestRegistrationNumber()
		{
			AWBHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			var shipment = Factory.New<ForwardingShipment>();
			AWBHeader.EH_ParentID = shipment.Consols.AddNew().PK;
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			var consignee = Factory.New<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			var brazil = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			code1.OK_RN_NKCodeCountry = brazil.Code;
			code1.OK_CustomsRegNo = "REG1111";

			var code2 = consignee.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.CodeTypes.RebateUserCode;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code2.OK_CustomsRegNo = "REG2222";
			AWBHeader.Populate();
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);
			CreateRefDocOrgCusCode(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, CountryCodes.Brazil, CountryCodes.Brazil, 1, "CNPJ", "CNPJ", "AWB");

			AWBHeader.Consol.JK_RL_NKDischargePort = "BRBSE";
			AWBHeader.Populate();
			AssertEquals("Registration Number", "CNPJ: REG1111", AWBHeader.RegistrationNumber);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Populate();
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = consignee.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals("Registration Number", "CNPJ: REG1111", AWBHeader.RegistrationNumber);
		}

		public void TestRegistrationNumber_BRVia_EU()
		{
			CreateRefDocOrgCusCode(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, CountryCodes.Brazil, CountryCodes.Brazil, 1, "CNPJ", "CNPJ", "AWB");

			AWBHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			var shipment = Factory.New<ForwardingShipment>();
			AWBHeader.EH_ParentID = shipment.Consols.AddNew().PK;
			AWBHeader.Consol.JK_AgentType = AgentType.Agent;
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			var receivingForwarder = Factory.New<OrgHeader>();
			var code1 = receivingForwarder.CustomsCodes.AddNew();
			code1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			code1.OK_RN_NKCodeCountry = CountryCodes.Brazil;
			code1.OK_CustomsRegNo = "REG1111";

			var code2 = receivingForwarder.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.CodeTypes.RebateUserCode;
			code2.OK_RN_NKCodeCountry = CountryCodes.Australia;
			code2.OK_CustomsRegNo = "REG2222";

			var code3 = receivingForwarder.CustomsCodes.AddNew();
			code3.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code3.OK_RN_NKCodeCountry = CountryCodes.Germany;
			code3.OK_CustomsRegNo = "REG3333";

			var consol = AWBHeader.Consol;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var leg1 = consol.Transports[0];
			leg1.JW_TransportMode = TransportModes.Air;
			leg1.JW_RL_NKLoadPort = "AUBNE";
			leg1.JW_RL_NKDiscPort = "DEFRA";
			leg1.JW_ETD = ZDateTime.Now.AddDays(1);

			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = TransportModes.Air;
			leg2.JW_RL_NKLoadPort = "DEFRA";
			leg2.JW_RL_NKDiscPort = "BRSAO";
			leg2.JW_ETD = ZDateTime.Now.AddDays(2);

			AWBHeader.Populate();
			AssertEquals("Registration Number", "CNPJ: REG1111", AWBHeader.RegistrationNumber);
		}

		public void TestExtraShipperData()
		{
			AWBHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			AssertEquals("Extra Shipper Data", ZString.Empty, AWBHeader.ExtraShipperData);
		}

		public void TestSaving()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			ForwardingConsol consol = newFactory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var aWB = consol.AWBHeader;
			AssertEquals("Not overridden new AWB is never saved", false, aWB.IsSavedByFactory);
			newFactory.Save();
			AssertEquals(false, aWB.IsInDatabase);

			consol.JK_OverrideWaybillDefaults = true;
			consol.JK_OverrideWaybillDefaults = false;
			aWB.EH_AreRateLinesOverridden = false;

			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed", false, aWB.IsSavedByFactory);

			aWB.ForceSavingByFactory = true;
			AssertEquals("Forced AWB is always saved", true, aWB.IsSavedByFactory);
			newFactory.Save();
			AssertEquals(true, aWB.IsInDatabase);

			aWB.ForceSavingByFactory = false;
			AssertEquals("Once saved but not overridden is not saved next time", false, aWB.IsSavedByFactory);

			consol.JK_OverrideWaybillDefaults = true;
			AssertEquals("Saved when 'Override' is ticked", true, aWB.IsSavedByFactory);
			newFactory.Save();

			AssertEquals("Overridden is not saved if doesn't have changes", false, aWB.IsSavedByFactory);

			aWB.HasChanges = true;
			AssertEquals("Overridden is saved when has changes", true, aWB.IsSavedByFactory);
			newFactory.Save();

			consol.JK_OverrideWaybillDefaults = false;
			AssertEquals("Overridden is saved when 'Override' has changes and already in the database", true, aWB.IsSavedByFactory);
			newFactory.Save();

			aWB.HasChanges = true;
			AssertEquals("Not overridden is not saved when has changes", false, aWB.IsSavedByFactory);

			consol.JK_OverrideWaybillDefaults = true;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Non-air transport modes not saved", false, aWB.IsSavedByFactory);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			aWB.EH_AreRateLinesOverridden = true;
			AssertEquals("Saved when 'Override Rate Section' has changed", true, aWB.IsSavedByFactory);
		}

		public void TestValidation()
		{
			AssertNotNull("Should be of type " + typeof(ConsolExportAWBHeaderValidation).FullName, AWBHeader.Validation);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Master Air Waybill for " + AWBHeader.Consol.HumanReadableName, AWBHeader.HumanReadableName);

			AWBHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			AssertEquals("Master Air Waybill", AWBHeader.HumanReadableName);
		}

		public void TestCustomsEntryNumber()
		{
			SetUpForDeparturePort("AUSYD");

			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);

			CusEntryNumber consolEntryNumber = AWBHeader.Consol.CusEntryNums.AddNew();
			consolEntryNumber.CE_EntryType = "CAN";
			consolEntryNumber.CE_EntryNum = "123";
			AssertEquals("CAN: 123", AWBHeader.EH_ECNCRNNumber);

			consolEntryNumber.CE_EntryType = "CRN";
			consolEntryNumber.CE_EntryNum = "456";
			AssertEquals("CAN: 456", AWBHeader.EH_ECNCRNNumber);

			consolEntryNumber.CE_EntryType = Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.Get3CharCode(Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code);
			consolEntryNumber.CE_EntryNum = "987";
			AssertEquals("CAN: EXLV", AWBHeader.EH_ECNCRNNumber);

			consolEntryNumber.CE_EntryType = "XYZ";
			AssertEquals("XYZ: 987", AWBHeader.EH_ECNCRNNumber);

			ForwardingShipment shipment = AWBHeader.Consol.Shipments[0];
			shipment.CustomsEntryNumberType = "ZUB";
			shipment.CustomsEntryNumber = "456";

			AssertEquals("XYZ: 987", AWBHeader.EH_ECNCRNNumber);

			AWBHeader.Consol.CusEntryNums.RemoveAndDeleteAll();
			AssertEquals("ZUB: 456", AWBHeader.EH_ECNCRNNumber);

			shipment.CustomsEntryNumberType = Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.Get3CharCode(Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.EXML.Code);
			shipment.CustomsEntryNumber = "456";
			AssertEquals("CAN: EXML", AWBHeader.EH_ECNCRNNumber);
		}

		public void TestMultipleCustomsEntryNumbers()
		{
			ForwardingConsol consol = AWBHeader.Consol;

			CusEntryNumber cusEntryNum1 = consol.CusEntryNums.AddNew();
			cusEntryNum1.CE_EntryNum = "111";
			cusEntryNum1.CE_EntryType = "CUS";

			CusEntryNumber cusEntryNum2 = consol.CusEntryNums.AddNew();
			cusEntryNum2.CE_EntryNum = "222";
			cusEntryNum2.CE_EntryType = "NUM";

			CusEntryNumber cusEntryNum3 = consol.CusEntryNums.AddNew();
			cusEntryNum3.CE_EntryNum = "333";
			cusEntryNum3.CE_EntryType = "BER";

			CusEntryNumber cusEntryNum4 = consol.CusEntryNums.AddNew();
			cusEntryNum4.CE_EntryNum = "";
			cusEntryNum4.CE_EntryType = "X";

			AssertEquals("CustomsEntryNumbers taken from consol and formatted",
				"CUS: 111,\r\nNUM: 222,\r\nBER: 333", AWBHeader.EH_ECNCRNNumber);

			AssertContainsExactElementsInAnyOrder("CustomsEntryNumbers",
				new[]
				{
					"CUS|111",
					"NUM|222",
					"BER|333"
				},
				AWBHeader.CustomsEntryNumbers.Select(n => string.Format("{0}|{1}", n.Type, n.Number)));
		}

		public void TestUnitedStatesCustomsEntryNumber()
		{
			SetUpForDeparturePort("AUSYD");

			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				CusEntryNumber consolEntryNumber = AWBHeader.Consol.CusEntryNums.AddNew();
				consolEntryNumber.CE_EntryType = "ITN";
				consolEntryNumber.CE_EntryNum = "456";
				AssertEquals("Custom entry number with 'ITN' type in United States should show 'AES' instead in AWB", "AES: 456", AWBHeader.EH_ECNCRNNumber);
			}
		}

		public void TestMultipleCustomsEntryNumbersNotShowingMoreThan4()
		{
			ForwardingConsol consol = AWBHeader.Consol;

			CusEntryNumber cusEntryNum1 = consol.CusEntryNums.AddNew();
			cusEntryNum1.CE_EntryNum = "111";
			cusEntryNum1.CE_EntryType = "CUS";

			CusEntryNumber cusEntryNum2 = consol.CusEntryNums.AddNew();
			cusEntryNum2.CE_EntryNum = "222";
			cusEntryNum2.CE_EntryType = "NUM";

			CusEntryNumber cusEntryNum3 = consol.CusEntryNums.AddNew();
			cusEntryNum3.CE_EntryNum = "333";
			cusEntryNum3.CE_EntryType = "BER";

			CusEntryNumber cusEntryNum4 = consol.CusEntryNums.AddNew();
			cusEntryNum4.CE_EntryNum = "444";
			cusEntryNum4.CE_EntryType = "CAN";

			CusEntryNumber cusEntryNum5 = consol.CusEntryNums.AddNew();
			cusEntryNum5.CE_EntryNum = "555";
			cusEntryNum5.CE_EntryType = "CCN";

			AssertEquals("CustomsEntryNumbers taken from consol and formatted",
				"", AWBHeader.EH_ECNCRNNumber);

			AssertContainsExactElementsInAnyOrder("CustomsEntryNumbers",
				new[]
				{
					"CUS|111",
					"NUM|222",
					"BER|333",
					"CAN|444",
					"CCN|555"
				},
				AWBHeader.CustomsEntryNumbers.Select(n => string.Format("{0}|{1}", n.Type, n.Number)));
		}

		public void TestCustomsEntryNumbersFromShipment()
		{
			ForwardingConsol consol = AWBHeader.Consol;
			ForwardingShipment shipment = consol.Shipments[0];

			AssertEquals("Precondition", 0, consol.CusEntryNums.Count);

			CusEntryNumber cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = "111";
			cusEntryNum1.CE_EntryType = "CUS";

			CusEntryNumber cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum2.CE_EntryNum = "222";
			cusEntryNum2.CE_EntryType = "NUM";

			CusEntryNumber cusEntryNum3 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum3.CE_EntryNum = "333";
			cusEntryNum3.CE_EntryType = "BER";

			CusEntryNumber cusEntryNum4 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum4.CE_EntryNum = "";
			cusEntryNum4.CE_EntryType = "X";

			AssertEquals("CustomsEntryNumbers taken from shipment and formatted",
				"CUS: 111,\r\nNUM: 222,\r\nBER: 333", AWBHeader.EH_ECNCRNNumber);

			AssertContainsExactElementsInAnyOrder("CustomsEntryNumbers",
				new[]
				{
					"CUS|111",
					"NUM|222",
					"BER|333"
				},
				AWBHeader.CustomsEntryNumbers.Select(n => string.Format("{0}|{1}", n.Type, n.Number)));
		}

		public void TestCustomsEntryNumberIgnoresEmptyElements()
		{
			ForwardingConsol consol = AWBHeader.Consol;
			consol.CusEntryNums.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);
			CusEntryNumber cusEntryNum1 = consol.CusEntryNums.AddNew();
			cusEntryNum1.CE_EntryNum = ZString.Empty;
			cusEntryNum1.CE_EntryType = ZString.Empty;
			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);
			CusEntryNumber cusEntryNum2 = consol.CusEntryNums.AddNew();
			cusEntryNum2.CE_EntryNum = "666";
			cusEntryNum2.CE_EntryType = "NOB";
			AssertEquals("NOB: 666", AWBHeader.EH_ECNCRNNumber);
		}

		public void TestHAWBNumbers()
		{
			AWBHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.AddNew();
			AWBHeader.EH_ParentID = shipment.Consols[0].PK;
			AssertEquals("Should return Empty string", "", AWBHeader.HAWBNumbers);

			Guid currentCompany = GlbCompany.CurrentCompany.PK.ToGuid();
			Guid[] countrys = new Guid[4];
			countrys[0] = Core.Constants.CountryGuids.China;

			Enterprise.Registry.Business.FreightDataRegistry.Instance.PrintHawbNumbersInBodyOfMawb.SetValue(currentCompany, Guid.Empty, Guid.Empty, countrys);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "AUSYD";
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_HouseBill = "HOU3333";
			AssertEquals("Should return Empty string", "", AWBHeader.HAWBNumbers);

			consignee.OH_RL_NKClosestPort = "CNSHI";
			AssertEquals("Should return Shipments Housebill", "HAWBS: HOU3333", AWBHeader.HAWBNumbers);

			var shipment2 = Factory.New<ForwardingShipment>();
			AWBHeader.Consol.Shipments.Add(shipment2);
			shipment2.ConsigneePK = consignee.PK;
			shipment2.JS_HouseBill = "567856";

			AssertEquals("Should return Shipments Housebills", "HAWBS: HOU3333, 567856", AWBHeader.HAWBNumbers);
		}

		#region Goods Declaration Reference Number

		public void TestGoodsDeclarationReferenceNumbers_AuthorizedSenderNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CH"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();

				var orgCusCode = org.CustomsCodes.AddNew();
				orgCusCode.OK_RN_NKCodeCountry = "CH";
				orgCusCode.OK_CustomsRegNo = "CH123";
				orgCusCode.OK_CodeType = "ASN";

				Factory.Save();

				GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;

				var branch = GlbCompany.CurrentCompany.Branches.AddNew();
				branch.GB_OH_OrgProxy = org.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "CHGVA";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertContainsExactElementsInAnyOrder("Authorized Sender",
					new[] { "CH|EXP|EA FILED" },
					header.GoodsDeclarationReferenceNumbers.Select(FormatGoodsDeclarationReferenceNumber));

				var address = Factory.NewWithValidTestData<OrgAddress>();
				orgCusCode.OK_OA_PremisesAddress = address.PK;

				Factory.Save();

				AssertContainsExactElementsInAnyOrder("Authorized Sender with approved location",
					new[] { "CH|EXP|LA FILED" },
					header.GoodsDeclarationReferenceNumbers.Select(FormatGoodsDeclarationReferenceNumber));

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "CHGVA";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_CommunityTransitStatus = "TD";

				Factory.Save();

				AssertContainsExactElementsInAnyOrder("Linked shipment has a CusEntryNumber with type of TD",
					Array.Empty<GoodsDeclarationReferenceNumber>(),
					header.GoodsDeclarationReferenceNumbers.Select(FormatGoodsDeclarationReferenceNumber));
			}
		}

		public void TestGoodsDeclarationReferenceNumbers_TrimLeadingEntryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CH"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment1.JS_RL_NKOrigin = "AUSYD";
				shipment1.JS_RL_NKDestination = "USCHI";
				var cusEntryNum1 = shipment1.CusEntryNumbers.AddNew();
				cusEntryNum1.CE_EntryNum = "GDRG11111";
				cusEntryNum1.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment2.JS_RL_NKOrigin = "CHBSL";
				shipment2.JS_RL_NKDestination = "USCHI";
				var cusEntryNum21 = shipment2.CusEntryNumbers.AddNew();
				cusEntryNum21.CE_EntryNum = "GDRNG22222";
				cusEntryNum21.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;
				var cusEntryNum22 = shipment2.CusEntryNumbers.AddNew();
				cusEntryNum22.CE_EntryNum = "GDRNG33333";
				cusEntryNum22.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment3.JS_RL_NKOrigin = "USCHI";
				shipment3.JS_RL_NKDestination = "CHBSL";
				var cusEntryNum3 = shipment3.CusEntryNumbers.AddNew();
				cusEntryNum3.CE_EntryNum = "GGG33333";
				cusEntryNum3.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;

				var shipment4 = consol.Shipments.AddNew();
				shipment4.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment4.JS_RL_NKOrigin = "CHBSL";
				shipment4.JS_RL_NKDestination = "USCHI";
				var cusEntryNum4 = shipment4.CusEntryNumbers.AddNew();
				cusEntryNum4.CE_EntryNum = "PMT44444";
				cusEntryNum4.CE_EntryType = CusEntryNumberTypes.Standard.ClearancePermitNumber;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"CH|TRA|G11111",
						"CH|EXP|G22222, G33333",
						"CH|IMP|GGG33333"
					},
					header.GoodsDeclarationReferenceNumbers.Select(FormatGoodsDeclarationReferenceNumber));
			}
		}

		string FormatGoodsDeclarationReferenceNumber(GoodsDeclarationReferenceNumber goodsDeclarationReferenceNumber)
		{
			return string.Format("{0}|{1}|{2}", goodsDeclarationReferenceNumber.CountryOfIssue, goodsDeclarationReferenceNumber.MovementCode, string.Join(", ", goodsDeclarationReferenceNumber.Numbers));
		}

		#endregion

		#region Movement Reference Numbers

		public void TestMovementReferenceNumbers_NonDirectConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "AAAA";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment1.JS_RL_NKOrigin = "HUBUD";
				shipment1.JS_RL_NKDestination = "PLWRO";
				shipment1.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment1.CustomsEntryNumber = "11111";

				var packingline = shipment1.OuterPackLines.AddNew();
				packingline.SetContainer(consol, container);

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment2.JS_HouseBill = "HBL22222";
				shipment2.JS_RL_NKOrigin = "USCHI";
				shipment2.JS_RL_NKDestination = "PLWRO";
				shipment2.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment2.CustomsEntryNumber = "22222";

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment3.JS_HouseBill = "HBL33333";
				shipment3.JS_RL_NKOrigin = "USCHI";
				shipment3.JS_RL_NKDestination = "RUMOV";
				shipment3.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment3.CustomsEntryNumber = "33333";

				var shipment4 = consol.Shipments.AddNew();
				shipment4.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment4.JS_HouseBill = "HBL44444";
				shipment4.JS_RL_NKOrigin = "GBLON";
				shipment4.JS_RL_NKDestination = "PLWRO";
				shipment4.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment4.CustomsEntryNumber = "44444";

				var shipment5 = consol.Shipments.AddNew();
				shipment5.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment5.JS_HouseBill = "HBL55555";
				shipment5.JS_RL_NKOrigin = "GBLON";
				shipment5.JS_RL_NKDestination = "PLWRO";
				shipment5.CustomsEntryNumberType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
				shipment5.CustomsEntryNumber = "55555";

				var shipment6 = consol.Shipments.AddNew();
				shipment6.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment6.JS_HouseBill = "HBL66666";
				shipment6.JS_RL_NKOrigin = "GBLON";
				shipment6.JS_RL_NKDestination = "PLWRO";
				shipment6.CustomsEntryNumberType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
				shipment6.CustomsEntryNumber = "66666";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"HU|EXP|11111",
						"GB|IMP|22222*HWB|HBL22222",
						"GB|TRA|33333*HWB|HBL33333",
						"GB|EXP|44444*HWB|HBL44444"
					},
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));
			}
		}

		public void TestMovementReferenceNumbers_PopulateULDContainers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "AAAA";

				var container2 = consol.Containers.AddNew();
				container2.JC_ContainerNum = "BBBB";

				var container3 = consol.Containers.AddNew();
				container3.JC_ContainerNum = "CCCC";

				var container4 = consol.Containers.AddNew();
				container4.JC_ContainerNum = "DDDD";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment1.JS_HouseBill = "HBL11111";
				shipment1.JS_RL_NKOrigin = "HUBUD";
				shipment1.JS_RL_NKDestination = "PLWRO";
				shipment1.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment1.CustomsEntryNumber = "11111";

				var packingline11 = shipment1.OuterPackLines.AddNew();
				packingline11.SetContainer(consol, container1);

				var packingline12 = shipment1.OuterPackLines.AddNew();
				packingline12.SetContainer(consol, container1);

				var packingline13 = shipment1.OuterPackLines.AddNew();
				packingline13.SetContainer(consol, container2);

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment2.JS_HouseBill = "HBL22222";
				shipment2.JS_RL_NKOrigin = "HUBUD";
				shipment2.JS_RL_NKDestination = "PLWRO";
				shipment2.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment2.CustomsEntryNumber = "22222";

				var packingline22 = shipment2.OuterPackLines.AddNew();
				packingline22.SetContainer(consol, container3);

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment3.JS_HouseBill = "HBL33333";
				shipment3.JS_RL_NKOrigin = "HUBUD";
				shipment3.JS_RL_NKDestination = "PLWRO";
				shipment3.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment3.CustomsEntryNumber = "33333";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"HU|EXP|11111*HWB|HBL11111*ULD|AAAA*ULD|BBBB",
						"HU|EXP|22222*HWB|HBL22222*ULD|CCCC",
						"HU|EXP|33333*HWB|HBL33333"
					},
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));
			}
		}

		public void TestMovementReferenceNumbers_TrimLeadingEntryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HBL11111";
				shipment.JS_RL_NKOrigin = "HUBUD";
				shipment.JS_RL_NKDestination = "PLWRO";
				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment.CustomsEntryNumber = "MRNM11111";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment2.JS_HouseBill = "HBL22222";
				shipment2.JS_RL_NKOrigin = "HUBUD";
				shipment2.JS_RL_NKDestination = "PLWRO";
				shipment2.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment2.CustomsEntryNumber = "MRNM22222";

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment3.JS_HouseBill = "HBL33333";
				shipment3.JS_RL_NKOrigin = "HUBUD";
				shipment3.JS_RL_NKDestination = "PLWRO";
				shipment3.CustomsEntryNumberType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
				shipment3.CustomsEntryNumber = "PMTP33333";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"HU|EXP|M11111*HWB|HBL11111",
						"HU|EXP|M22222*HWB|HBL22222"
					},
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HBL11111";
				shipment.JS_RL_NKOrigin = "HUBUD";
				shipment.JS_RL_NKDestination = "PLWRO";

				var cusEntryNum11 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum11.CE_EntryNum = "MRN11111S1";
				cusEntryNum11.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				var cusEntryNum12 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum12.CE_EntryNum = "MRN22222S1";
				cusEntryNum12.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				var cusEntryNum13 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum13.CE_EntryNum = "PMT33333S1";
				cusEntryNum13.CE_EntryType = CusEntryNumberTypes.Standard.ClearancePermitNumber;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment2.JS_HouseBill = "HBL22222";
				shipment2.JS_RL_NKOrigin = "HUBUD";
				shipment2.JS_RL_NKDestination = "PLWRO";

				var cusEntryNum21 = shipment2.CusEntryNumbers.AddNew();
				cusEntryNum21.CE_EntryNum = "MRN11111S2";
				cusEntryNum21.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				var cusEntryNum22 = shipment2.CusEntryNumbers.AddNew();
				cusEntryNum22.CE_EntryNum = "DCI22222S2";
				cusEntryNum22.CE_EntryType = CusEntryNumberTypes.Standard.DrawbackClaim;

				var cusEntryNum23 = shipment2.CusEntryNumbers.AddNew();
				cusEntryNum23.CE_EntryNum = "MRN33333S2";
				cusEntryNum23.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment3.JS_HouseBill = "HBL33333";
				shipment3.JS_RL_NKOrigin = "HUBUD";
				shipment3.JS_RL_NKDestination = "PLWRO";

				var cusEntryNum31 = shipment3.CusEntryNumbers.AddNew();
				cusEntryNum31.CE_EntryNum = "MRN11111S3";
				cusEntryNum31.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				var cusEntryNum32 = shipment3.CusEntryNumbers.AddNew();
				cusEntryNum32.CE_EntryNum = "MRN22222S3";
				cusEntryNum32.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertEquals(ZString.Empty, shipment.CustomsEntryNumberType);
				AssertEquals(ZString.Empty, shipment2.CustomsEntryNumberType);
				AssertEquals(CusEntryNumberTypes.Standard.MovementReferenceNumber, shipment3.CustomsEntryNumberType);
				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"HU|EXP|11111S1, 22222S1*HWB|HBL11111",
						"HU|EXP|11111S2, 33333S2*HWB|HBL22222",
						"HU|EXP|11111S3, 22222S3*HWB|HBL33333"
					},
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));
			}
		}

		public void TestMovementReferenceNumbers_DoNotPopulateForNonEUCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HBL11111";
				shipment.JS_RL_NKOrigin = "HUBUD";
				shipment.JS_RL_NKDestination = "PLWRO";
				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment.CustomsEntryNumber = "MRN11111";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					Array.Empty<string>(),
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));
			}
		}

		public void TestMovementReferenceNumbers_DoNotPopulateHouseBillForDirectConsolidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_AgentType = Core.Constants.AgentType.Direct;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HBL11111";
				shipment.JS_RL_NKOrigin = "HUBUD";
				shipment.JS_RL_NKDestination = "PLWRO";
				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment.CustomsEntryNumber = "11111";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"HU|EXP|11111"
					},
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));
			}
		}

		string FormatMovementReferenceNumber(MovementReferenceNumber number)
		{
			var result = string.Format("{0}|{1}|{2}", number.CountryOfIssue, number.MovementCode, string.Join(", ", number.Numbers));

			if (number.RelatedNumbers != null)
			{
				var related = number
					.RelatedNumbers
					.Select(n => string.Format("{0}|{1}", n.Type, n.Number))
					.ToArray();

				if (related.Any())
				{
					result = string.Concat(result, "*", string.Join("*", related));
				}
			}

			return result;
		}

		#endregion

		public void TestSecurityStatusAWBVisibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				AssertEquals("Enabled for HKG", true, AWBHeader.SecurityStatusAWBVisibility);
			}

			AWBHeader.ResetSupplyChainSecurityConfigurationForTesting();

			AssertEquals("Not enabled otherwise", false, AWBHeader.SecurityStatusAWBVisibility);
		}

		public void TestAWBRANumber()
		{
			using (FreightDataRegistry.Instance.RegulatedAgentNumber_TW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "RA12345"))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("TW"))
				{
					AssertEquals("Shows for Taiwan export", "RA12345", AWBHeader.SupplyChainSecurityConfiguration.AWBRANumber);
				}

				AWBHeader.ResetSupplyChainSecurityConfigurationForTesting();

				AssertEquals("Not shown otherwise", "", AWBHeader.SupplyChainSecurityConfiguration.AWBRANumber);
			}
		}

		public void TestGetULDAviationSecurityStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_ConsolMode = "ULD";

				var container1 = consol.Containers.AddNew();
				container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "PM-2H").PK;

				var container2 = consol.Containers.AddNew();
				container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-26").PK;

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_InspectionTypeCode = "UNK";

				var pack1 = shipment1.OuterPackLines.AddNew();
				container1.PackLines.Add(pack1);

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_InspectionTypeCode = "XRY";

				var pack2 = shipment2.OuterPackLines.AddNew();
				container1.PackLines.Add(pack2);

				var pack3 = shipment2.OuterPackLines.AddNew();
				container2.PackLines.Add(pack3);

				var header = consol.AWBHeader as ConsolExportAWBHeader;

				AssertEquals("Container 1 contains packlines from UNK and XRY shipments", "UNK", header.GetULDAviationSecurityStatus(container1));
				AssertEquals("Container 2 only contains packlines from the XRY shipment", "SPX", header.GetULDAviationSecurityStatus(container2));

				var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.Value;
				((ShipmentInspectionType)inspectionTypes.Types.FindByCode("XRY")).AllowedOnPassengerFlights = false;

				using (FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
				{
					AssertEquals("Container 1 is still Unknown", "UNK", header.GetULDAviationSecurityStatus(container1));
					AssertEquals("Container 2 is now only allowed on cargo flights", "SCO", header.GetULDAviationSecurityStatus(container2));
				}
			}
		}

		public void TestGetULDAviationSecurityStatus_HighRisk()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_ConsolMode = "ULD";
				consol.Transports[0].JW_RL_NKLoadPort = "GBLHR";
				consol.Transports[0].JW_RL_NKDiscPort = "CNSHA";

				var container1 = consol.Containers.AddNew();
				container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "PM-2H").PK;

				var container2 = consol.Containers.AddNew();
				container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "LD-26").PK;

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_IsHighRisk = true;
				shipment1.JS_InspectionTypeCode = "PHS";
				shipment1.JS_AdditionalInspectionTypeCode = "UNK";

				var pack1 = shipment1.OuterPackLines.AddNew();
				container1.PackLines.Add(pack1);

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_IsHighRisk = true;
				shipment2.JS_InspectionTypeCode = "PHS";
				shipment2.JS_AdditionalInspectionTypeCode = "XRY";

				var pack2 = shipment2.OuterPackLines.AddNew();
				container1.PackLines.Add(pack2);

				var pack3 = shipment2.OuterPackLines.AddNew();
				container2.PackLines.Add(pack3);

				AssertEquals("Shipment 2 inspection code changed to UNK because of the packlines with empty inspection code", "UNK", shipment2.JS_InspectionTypeCode);
				shipment2.JS_InspectionTypeCode = "PHS";

				var header = consol.AWBHeader as ConsolExportAWBHeader;

				AssertEquals("Container 1 contains packlines from high risk shipments with UNK and XRY additional inspection codes", "UNK", header.GetULDAviationSecurityStatus(container1));
				AssertEquals("Container 2 only contains packlines from the XRY high risk shipment", "SPX", header.GetULDAviationSecurityStatus(container2));

				var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
				((ShipmentInspectionType)inspectionTypes.Types.FindByCode("XRY")).AllowedOnPassengerFlights = false;

				using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
				{
					AssertEquals("Container 1 is still Unknown", "UNK", header.GetULDAviationSecurityStatus(container1));
					AssertEquals("Container 2 is now only allowed on cargo flights", "SCO", header.GetULDAviationSecurityStatus(container2));
				}
			}
		}

		public void TestEH_KnownConsignorCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "SGSIN";
				consol.Transports[0].JW_TransportMode = "AIR";
				consol.Transports[0].JW_TransportType = "FL1";
				consol.Transports[0].JW_RL_NKLoadPort = "SGSIN";

				void AssertEH_KnownConsignorCode()
				{
					consol.Shipments.DeleteAll();
					consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
					consol.JK_AgentType = AgentType.Agent;

					var shipment1 = consol.Shipments.AddNew();
					var shipment2 = consol.Shipments.AddNew();
					shipment1.JS_InspectionTypeCode = "UNK";
					shipment2.JS_InspectionTypeCode = "UNK";
					AssertEquals("Unknown shipments", "RCAR-UC", consol.AWBHeader.EH_KnownConsignorCode);

					shipment1.JS_InspectionTypeCode = "APP";
					AssertEquals("One shipment is still unknown", "RCAR-UC", consol.AWBHeader.EH_KnownConsignorCode);

					shipment2.JS_InspectionTypeCode = "XRY";
					AssertNull("Precondition: SendingForwarder is null", consol.SendingForwarder);
					AssertEquals("All shipments are known and sending agent is empty", "RCAR-KC", consol.AWBHeader.EH_KnownConsignorCode);

					var unknownAddress1 = Factory.NewWithValidTestData<OrgAddress>();
					unknownAddress1.OA_RN_NKCountryCode = "SG";
					consol.JK_OA_SendingForwarderAddress = unknownAddress1.PK;
					AssertEquals("All shipments are known and sending agent is not known", "RCAR-UC", consol.AWBHeader.EH_KnownConsignorCode);

					consol.JK_AgentType = AgentType.Direct;
					AssertEquals("All shipments are known and sender is direct", "RCAR-KC", consol.AWBHeader.EH_KnownConsignorCode);

					shipment1.JS_InspectionTypeCode = "UNK";
					shipment2.JS_InspectionTypeCode = "UNK";

					var approvedAddress1 = Factory.NewWithValidTestData<OrgAddress>();
					approvedAddress1.OA_RN_NKCountryCode = "SG";
					var countryData1 = approvedAddress1.KnownShipperDetails.AddNew();
					countryData1.OV_OH_OrgHeader = approvedAddress1.OA_OH;
					countryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
					AssertEquals("Precondition: OV_EXApprovalExpiryDate is empty", ZDate.Empty, countryData1.OV_EXApprovalExpiryDate);

					consol.JK_AgentType = AgentType.Agent;
					consol.JK_OA_SendingForwarderAddress = approvedAddress1.PK;
					AssertEquals("Sending agent is known but not all shipments are known", "RCAR-UC", consol.AWBHeader.EH_KnownConsignorCode);

					shipment1.JS_InspectionTypeCode = "XRY";
					shipment2.JS_InspectionTypeCode = "APP";
					AssertEquals("All shipments are known and sending agent is RA and expiry date is empty", "RCAR-KC", consol.AWBHeader.EH_KnownConsignorCode);

					countryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
					AssertEquals("All shipments are known and sending agent is RA, but approval has expired", "RCAR-UC", consol.AWBHeader.EH_KnownConsignorCode);

					countryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddMonths(1);
					AssertEquals("All shipments are known and sending agent is RA", "RCAR-KC", consol.AWBHeader.EH_KnownConsignorCode);

					var approvedAddress2 = Factory.NewWithValidTestData<OrgAddress>();
					approvedAddress2.OA_RN_NKCountryCode = "SG";
					var countryData2 = approvedAddress2.KnownShipperDetails.AddNew();
					countryData2.OV_OH_OrgHeader = approvedAddress2.OA_OH;
					countryData2.OV_EXApprovalExpiryDate = ZDate.Today.AddMonths(1);
					countryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
					countryData2.OV_EXApprovalNumber = "RCA/0001/2021";
					consol.JK_OA_SendingForwarderAddress = approvedAddress2.PK;
					AssertEquals("All shipments are known and sending agent is KC", "RCAR-KC", consol.AWBHeader.EH_KnownConsignorCode);

					consol.JK_AgentType = AgentType.Direct;
					shipment1.JS_InspectionTypeCode = "UNK";
					shipment2.JS_InspectionTypeCode = "UNK";
					AssertEquals("Agent Type is Direct and shipments are not known", "RCAR-UC", consol.AWBHeader.EH_KnownConsignorCode);
				}

				consol.Transports[0].JW_IsCargoOnly = false;
				AssertEH_KnownConsignorCode();

				consol.Transports[0].JW_IsCargoOnly = true;
				AssertEH_KnownConsignorCode();
			}
		}

		public void TestConsigneeAccount()
		{
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_Code = "AAA123";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "BBB123";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "USLAX";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			shipment.ConsigneePK = consignee.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();
			AssertEquals("AAA123", AWBHeader.EH_ConsigneeAccount);

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Populate();
			AssertEquals("BBB123", AWBHeader.EH_ConsigneeAccount);

			consol.JK_RL_NKDischargePort = "SGSIN";
			AWBHeader.Populate();
			AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeAccount);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBHeader.Populate();
			AssertEquals("AAA123", AWBHeader.EH_ConsigneeAccount);
		}

		#region Address Tests

		#region Consignee Address

		public void TestReceivingForwarder()
		{
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertEquals(AWBHeader.Consol.ReceivingForwarder, AWBHeader.ReceivingForwarder);
		}

		public void TestConsigneeDocumentaryAddress()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertNotNull("DirectShipment should not be null", AWBHeader.DirectShipment);

			OrgAddress consigneeAddress = AWBHeader.DirectShipment.Consignee.MainAddress;
			consigneeAddress.OA_Address1 = "Consignee Main Address";
			AssertEquals("Should be Consignee Main Address", "Consignee Main Address", AWBHeader.ConsigneeDocumentaryAddress.E2_Address1);

			OrgAddress consigneeDocumentAddress = AWBHeader.DirectShipment.Consignee.Addresses.AddNew();
			consigneeDocumentAddress.OA_Address1 = "ConsigneeDocumentAddress";
			consigneeDocumentAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Consignee postal address added, Still should be ConsigneeAddress", "Consignee Main Address", AWBHeader.ConsigneeDocumentaryAddress.E2_Address1);

			AWBHeader.DirectShipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentAddress.PK;
			AssertEquals("ConsigneeDocumentaryAddress set, should be ConsigneeDocumentAddress", "ConsigneeDocumentAddress", AWBHeader.ConsigneeDocumentaryAddress.E2_Address1);
		}

		public void TestReceivingForwarderDocAddress()
		{
			AssertEquals("Prerequisite", AWBHeader.Consol.JK_AgentType, Core.Constants.AgentType.Agent);

			var forwarder = CreateForwarderWithTwoAddresses();

			AssertEquals("Prerequisite", 2, forwarder.Addresses.Count);
			AssertEquals("Prerequisite", true, forwarder.MainAddress.IsAddressOfType(OrgAddressType.Office));

			var additionalAddress = forwarder.Addresses[1];

			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = additionalAddress.PK;

			AssertEquals("Prerequisite", forwarder.PK, AWBHeader.Consol.ReceivingForwarderPK);
			AssertEquals(AWBHeader.ConsigneeDocumentaryAddress.Address.PK, additionalAddress.PK);

			AssertEquals("Prerequisite", OrgConstants.AddressType.Documentary, Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo);
			AssertEquals("Prerequisite", string.Empty, forwarder.MiscServ.OM_IMDocumentAddressPreference);

			AWBHeader.Populate();

			AssertEquals(forwarder.OH_FullName, AWBHeader.EH_ConsigneeName);
			AssertEquals(additionalAddress.OA_Address1, AWBHeader.EH_ConsigneeAddress);

			try
			{
				forwarder.MiscServ.OM_IMDocumentAddressPreference = OrgConstants.AddressType.Office;

				AWBHeader.Populate();

				AssertEquals(forwarder.OH_FullName, AWBHeader.EH_ConsigneeName);
				AssertEquals(forwarder.MainAddress.OA_Address1, AWBHeader.EH_ConsigneeAddress);

				forwarder.MiscServ.OM_IMDocumentAddressPreference = string.Empty;

				AWBHeader.Populate();

				AssertEquals(forwarder.OH_FullName, AWBHeader.EH_ConsigneeName);
				AssertEquals(additionalAddress.OA_Address1, AWBHeader.EH_ConsigneeAddress);

				Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Office;

				AWBHeader.Populate();

				AssertEquals(forwarder.OH_FullName, AWBHeader.EH_ConsigneeName);
				AssertEquals(forwarder.MainAddress.OA_Address1, AWBHeader.EH_ConsigneeAddress);
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			}
		}

		public void TestSendingAgentDocAddress()
		{
			AssertEquals("Prerequisite", AWBHeader.Consol.JK_AgentType, Core.Constants.AgentType.Agent);

			var forwarder = CreateForwarderWithTwoAddresses();

			AssertEquals("Prerequisite", 2, forwarder.Addresses.Count);
			AssertEquals("Prerequisite", true, forwarder.MainAddress.IsAddressOfType(OrgAddressType.Office));

			var additionalAddress = forwarder.Addresses[1];

			AWBHeader.Consol.JK_OA_SendingForwarderAddress = additionalAddress.PK;

			AssertEquals("Prerequisite", forwarder.PK, AWBHeader.Consol.SendingForwarderPK);
			AssertEquals(AWBHeader.ShipperDocumentaryAddress.Address.PK, additionalAddress.PK);

			AssertEquals("Prerequisite", OrgConstants.AddressType.Documentary, Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo);
			AssertEquals("Prerequisite", string.Empty, forwarder.MiscServ.OM_EXDocumentAddressPreference);

			AWBHeader.Populate();

			AssertEquals(forwarder.OH_FullName, AWBHeader.EH_ShipperName);
			AssertEquals(additionalAddress.OA_Address1, AWBHeader.EH_ShipperAddress);

			try
			{
				forwarder.MiscServ.OM_EXDocumentAddressPreference = OrgConstants.AddressType.Office;

				AWBHeader.Populate();

				AssertEquals(forwarder.OH_FullName, AWBHeader.EH_ShipperName);
				AssertEquals(forwarder.MainAddress.OA_Address1, AWBHeader.EH_ShipperAddress);

				forwarder.MiscServ.OM_EXDocumentAddressPreference = string.Empty;

				AWBHeader.Populate();

				AssertEquals(forwarder.OH_FullName, AWBHeader.EH_ShipperName);
				AssertEquals(additionalAddress.OA_Address1, AWBHeader.EH_ShipperAddress);

				Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;

				AWBHeader.Populate();

				AssertEquals(forwarder.OH_FullName, AWBHeader.EH_ShipperName);
				AssertEquals(forwarder.MainAddress.OA_Address1, AWBHeader.EH_ShipperAddress);
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			}
		}

		OrgHeader CreateForwarderWithTwoAddresses()
		{
			OrgHeader forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "TEST FORWARDER";
			forwarder.OH_RL_NKClosestPort = "AUSYD";
			forwarder.MainAddress.OA_Address1 = "MAIN ADDRESS";
			forwarder.OH_IsForwarder = true;

			OrgAddress additionalAddress = forwarder.Addresses.AddNew();
			additionalAddress.OA_Code = "XXX";
			additionalAddress.OA_Address1 = "ADDITIONAL ADDRESS";

			return forwarder;
		}

		public void TestDefaultConsigneeAddressType()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Documentary, AWBHeader.DefaultConsigneeAddressType);
			AWBHeader.ReceivingForwarder.MiscServ.OM_IMDocumentAddressPreference = OrgConstants.AddressType.Delivery;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Delivery, AWBHeader.DefaultConsigneeAddressType);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsigneePK = Factory.New<OrgHeader>().PK;
			AWBHeader.Consol.Shipments[0].Consignee.MiscServ.OM_IMDocumentAddressPreference = "";
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Documentary, AWBHeader.DefaultConsigneeAddressType);
			AWBHeader.Consol.Shipments[0].Consignee.MiscServ.OM_IMDocumentAddressPreference = OrgConstants.AddressType.Delivery;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Delivery, AWBHeader.DefaultConsigneeAddressType);
		}

		public void TestConsigneeOfficeAddress()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertEquals(AWBHeader.Consol.ReceivingForwarder.MainAddress, AWBHeader.ConsigneeOfficeAddress);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsigneePK = Factory.New<OrgHeader>().PK;
			AssertEquals(AWBHeader.Consol.Shipments[0].Consignee.MainAddress, AWBHeader.ConsigneeOfficeAddress);
		}

		public void TestConsigneeDeliveryAddress()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AddAddress(AWBHeader.Consol.ReceivingForwarder, OrgConstants.AddressType.Delivery);
			AssertEquals(AWBHeader.Consol.ReceivingForwarder.Addresses.DefaultAddressOfType(OrgAddressType.Delivery, false), AWBHeader.ConsigneeDeliveryAddress);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsigneePK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Consol.Shipments[0].Consignee, OrgConstants.AddressType.Delivery);
			AssertEquals(AWBHeader.Consol.Shipments[0].Consignee.Addresses.DefaultAddressOfType(OrgAddressType.Delivery, false), AWBHeader.ConsigneeDeliveryAddress);
		}

		public void TestDefaultConsigneeCompanyName()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AWBHeader.Consol.ReceivingForwarder.OH_FullName = "TEST";
			AssertEquals("TEST", AWBHeader.DefaultConsigneeCompanyName);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsigneePK = Factory.New<OrgHeader>().PK;
			AWBHeader.Consol.Shipments[0].Consignee.OH_FullName = "TEST1";
			AssertEquals("TEST1", AWBHeader.DefaultConsigneeCompanyName);
		}

		public void TestGetConsigneeAddreses()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AddAddress(AWBHeader.Consol.ReceivingForwarder, OrgConstants.AddressType.Delivery);
			AssertEquals(AWBHeader.Consol.ReceivingForwarder.Addresses.Count, AWBHeader.GetConsigneeAddresses().Count);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsigneePK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Consol.Shipments[0].Consignee, OrgConstants.AddressType.Delivery);
			AddAddress(AWBHeader.Consol.Shipments[0].Consignee, OrgConstants.AddressType.Pickup);
			AssertEquals(AWBHeader.Consol.Shipments[0].Consignee.Addresses.Count, AWBHeader.GetConsigneeAddresses().Count);
		}

		#endregion

		#region Shipper Address

		public void TestSendingForwarder()
		{
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertEquals(AWBHeader.Consol.SendingForwarder, AWBHeader.SendingForwarder);
		}

		public void TestShipperDocumentary()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertNotNull("DirectShipment should not be null", AWBHeader.DirectShipment);

			OrgAddress shipperAddress = AWBHeader.DirectShipment.Consignor.MainAddress;
			shipperAddress.OA_Address1 = "Shipper Main Address";
			AssertEquals("Should be Shipper Main Address", "Shipper Main Address", AWBHeader.ShipperDocumentaryAddress.E2_Address1);

			OrgAddress shipperDocumentAddress = AWBHeader.DirectShipment.Consignor.Addresses.AddNew();
			shipperDocumentAddress.OA_Address1 = "ShipperDocumentAddress";
			shipperDocumentAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);

			AssertEquals("Shipper postal address added, Still should be ShipperAddress", "Shipper Main Address", AWBHeader.ShipperDocumentaryAddress.E2_Address1);

			AWBHeader.DirectShipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperDocumentAddress.PK;
			AssertEquals("ShipperDocumentaryAddress set, should be ShipperDocumentAddress", "ShipperDocumentAddress", AWBHeader.ShipperDocumentaryAddress.E2_Address1);
		}

		public void TestShipperDocumentary_ForDirectConsolWithAssemblyMasterAsDirectShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "STD";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var master = consol.Shipments.AddNew();
			master.JS_UniqueConsignRef = "ASM";
			master.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			shipment.JS_JS_ColoadMasterShipment = master.PK;

			var header = Factory.New<ConsolExportAWBHeaderForTest>();

			header.EH_ParentID = consol.PK;

			AssertEquals("Should default to assembly master", header.DirectShipment, master);
		}

		public void TestDefaultShipperAddressType()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Documentary, AWBHeader.DefaultShipperAddressType);
			AWBHeader.SendingForwarder.MiscServ.OM_EXDocumentAddressPreference = OrgConstants.AddressType.Delivery;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Delivery, AWBHeader.DefaultShipperAddressType);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsignorPK = Factory.New<OrgHeader>().PK;
			AWBHeader.Consol.Shipments[0].Consignor.MiscServ.OM_EXDocumentAddressPreference = "";
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Documentary, AWBHeader.DefaultShipperAddressType);
			AWBHeader.Consol.Shipments[0].Consignor.MiscServ.OM_EXDocumentAddressPreference = OrgConstants.AddressType.Delivery;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Delivery, AWBHeader.DefaultShipperAddressType);
		}

		public void TestShipperAccount()
		{
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_Code = "SHPCODE";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNRCODE";

			AWBHeader.Consol.JK_OA_SendingForwarderAddress = shipper.MainAddress.PK;
			AWBHeader.Consol.Shipments[0].ConsignorPK = consignor.PK;

			AWBHeader.Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AWBHeader.Populate();
			AssertEquals("SHPCODE", AWBHeader.EH_ShipperAccount);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_ShipperAccount);

			AWBHeader.Consol.JK_RL_NKDischargePort = "USLAX";
			AWBHeader.Populate();
			AssertEquals("CNRCODE", AWBHeader.EH_ShipperAccount);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Populate();
			AssertEquals("SHPCODE", AWBHeader.EH_ShipperAccount);
		}

		public void TestShipperOfficeAddress()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AssertEquals(AWBHeader.Consol.SendingForwarder.MainAddress, AWBHeader.ShipperOfficeAddress);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsignorPK = Factory.New<OrgHeader>().PK;
			AssertEquals(AWBHeader.Consol.Shipments[0].Consignor.MainAddress, AWBHeader.ShipperOfficeAddress);
		}

		public void TestShipperDeliveryAddress()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AddAddress(AWBHeader.Consol.SendingForwarder, OrgConstants.AddressType.Pickup);
			AssertEquals(AWBHeader.Consol.SendingForwarder.Addresses.DefaultAddressOfType(OrgAddressType.Pickup, false), AWBHeader.ShipperPickupAddress);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsignorPK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Consol.Shipments[0].Consignor, OrgConstants.AddressType.Pickup);
			AssertEquals(AWBHeader.Consol.Shipments[0].Consignor.Addresses.DefaultAddressOfType(OrgAddressType.Pickup, false), AWBHeader.ShipperPickupAddress);
		}

		public void TestDefaultShipperCompanyName()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AWBHeader.Consol.SendingForwarder.OH_FullName = "TEST";
			AssertEquals("TEST", AWBHeader.DefaultShipperCompanyName);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsignorPK = Factory.New<OrgHeader>().PK;
			AWBHeader.Consol.Shipments[0].Consignor.OH_FullName = "TEST1";
			AssertEquals("TEST1", AWBHeader.DefaultShipperCompanyName);
		}

		public void TestGetShipperAddreses()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
			AddAddress(AWBHeader.Consol.SendingForwarder, OrgConstants.AddressType.Delivery);
			AssertEquals(AWBHeader.Consol.SendingForwarder.Addresses.Count, AWBHeader.GetShipperAddresses().Count);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].ConsignorPK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Consol.Shipments[0].Consignor, OrgConstants.AddressType.Delivery);
			AddAddress(AWBHeader.Consol.Shipments[0].Consignor, OrgConstants.AddressType.Pickup);
			AssertEquals(AWBHeader.Consol.Shipments[0].Consignor.Addresses.Count, AWBHeader.GetShipperAddresses().Count);
		}

		public void TestPopulateContactDetailsDefaultsPhone()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_Phone = "123123";

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_Phone = "789789";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Populate();

			AssertEquals("Sender", AWBHeader.EH_ShipperContactName);
			AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
			AssertEquals("123123", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("Receiver", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("789789", AWBHeader.EH_ConsigneeContactDetail);
		}

		public void TestAWBShipperConsigneeEmailWithContact()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.OA_Email = "Org@wtg.com";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_Phone = "123123";
			sendingForwarderContact.OC_Email = "Sender@wtg.com";

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_Phone = "789789";
			receivingForwarderContact.OC_Email = "Receiver@wtg.com";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Organisation.Addresses.Add(orgAddress);

			AssertEquals("Em: Sender@wtg.com", AWBHeader.EH_ShipperContactEmail);
			AssertEquals("Em: Receiver@wtg.com", AWBHeader.EH_ConsigneeContactEmail);
		}

		public void TestAWBShipperConsigneeEmailFallBackToOrgWithoutContact()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.OA_Email = "Org@wtg.com";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgAddress.PK;
			AWBHeader.SetConsol(consol);
			AWBHeader.Organisation.Addresses.Add(orgAddress);

			AssertEquals("Org@wtg.com", AWBHeader.EH_ShipperContactEmail);
			AssertEquals("Org@wtg.com", AWBHeader.EH_ConsigneeContactEmail);
		}

		public void TestPopulateContactDetailsDefaultsFax()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_Fax = "123123";

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_Fax = "789789";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Organisation.Addresses.Add(orgAddress);
			AWBHeader.Organisation.MainAddress.OA_Phone = ZString.Empty;
			AWBHeader.Populate();

			AssertEquals("Shipper name", "Sender", AWBHeader.EH_ShipperContactName);
			AssertEquals("Shipper contact code", Core.Constants.AWB.ContactCodes.FAX, AWBHeader.EH_ShipperContactCode);
			AssertEquals("Shipper contact details", "123123", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("Consignee name", "Receiver", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("Consignee contact code", Core.Constants.AWB.ContactCodes.FAX, AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("Consignee contact details", "789789", AWBHeader.EH_ConsigneeContactDetail);

			receivingForwarderContact.OC_Fax = ZString.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = orgAddress.PK;
			AWBHeader.Populate();

			AssertEquals("Consignee contact code", ZString.Empty, AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("Consignee contact details", ZString.Empty, AWBHeader.EH_ConsigneeContactDetail);
		}

		public void TestPopulateContactDetailsDefaultsPhone_FallbackFromOrganization()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_OA_OrgAddress = orgAddress.PK;
			sendingForwarderContact.OC_OH = orgHeader.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_OA_OrgAddress = orgAddress.PK;
			receivingForwarderContact.OC_OH = orgHeader.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			AssertEquals("Sender", AWBHeader.EH_ShipperContactName);
			AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
			AssertEquals("+96852", AWBHeader.EH_ShipperContactDetail);
			AssertEquals("Receiver", AWBHeader.EH_ConsigneeContactName);
			AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
			AssertEquals("+96852", AWBHeader.EH_ConsigneeContactDetail);
		}

		public void TestPopulateContactDetails_FallbackFromDirectShipment()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var consignor = GetOrganization(Factory, "CNR01");
			var consignee = GetOrganization(Factory, "CNE01");

			var consigneeContact = consignee.Contacts.AddNew();
			consigneeContact.OC_ContactName = "Receiver";
			consigneeContact.OC_Phone = "123456";
			consigneeContact.OC_OA_OrgAddress = consignee.MainAddress.PK;

			var consignorContact = Factory.New<OrgContact>();
			consignorContact.OC_ContactName = "Sender";
			consignorContact.OC_Phone = "789789";
			consignorContact.OC_OA_OrgAddress = consignor.MainAddress.PK;

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.ContactPK = consigneeContact.PK;
			shipment.ConsignorDocumentaryAddress.ContactPK = consignorContact.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("Sender", AWBHeader.EH_ShipperContactName);
				AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
				AssertEquals("789789", AWBHeader.EH_ShipperContactDetail);
				AssertEquals("Receiver", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("123456", AWBHeader.EH_ConsigneeContactDetail);
			});

			consignorContact.OC_Phone = ZString.Empty;
			consignorContact.OC_Fax = ZString.Empty;
			consigneeContact.OC_Phone = ZString.Empty;
			consigneeContact.OC_Fax = ZString.Empty;
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("Shipper contact name", "Sender", AWBHeader.EH_ShipperContactName);
				AssertEquals("Shipper contact code", ZString.Empty, AWBHeader.EH_ShipperContactCode);
				AssertEquals("Shipper contact details", ZString.Empty, AWBHeader.EH_ShipperContactDetail);
				AssertEquals("Consignee contact name", "Receiver", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("Consignee contact code", ZString.Empty, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("Consignee contact details", ZString.Empty, AWBHeader.EH_ConsigneeContactDetail);
			});
		}

		public void TestPopulateContactDetails_WhenDirectShipmentAndNoContact_FallbackToOrgDetails()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var consignor = GetOrganization(Factory, "CNR01");
			var consignee = GetOrganization(Factory, "CNE01");
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			consignee.MainAddress.OA_Phone = "111111111";
			consignor.MainAddress.OA_Phone = "222222222";

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("111111111", AWBHeader.EH_ConsigneeContactDetail);
				AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
				AssertEquals("222222222", AWBHeader.EH_ShipperContactDetail);
			});

			consignee.MainAddress.OA_Phone = string.Empty;
			consignee.MainAddress.OA_Fax = "33333333";
			consignor.MainAddress.OA_Phone = string.Empty;
			consignor.MainAddress.OA_Fax = "44444444";

			AWBHeader.Populate();
			CombineAssertions(() =>
			{
				AssertEquals("FX", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("33333333", AWBHeader.EH_ConsigneeContactDetail);
				AssertEquals("FX", AWBHeader.EH_ShipperContactCode);
				AssertEquals("44444444", AWBHeader.EH_ShipperContactDetail);
			});

			consignee.MainAddress.OA_Fax = ZString.Empty;
			consignor.MainAddress.OA_Fax = ZString.Empty;
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("Shipper contact code", ZString.Empty, AWBHeader.EH_ShipperContactCode);
				AssertEquals("Shipper contact details", ZString.Empty, AWBHeader.EH_ShipperContactDetail);
				AssertEquals("Consignee contact code", ZString.Empty, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("Consignee contact details", ZString.Empty, AWBHeader.EH_ConsigneeContactDetail);
			});
		}

		public void TestPopulateContactDetailsDefaultsFax_FallbackFromOrganization()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Fax = "123456";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_OA_OrgAddress = orgAddress.PK;
			sendingForwarderContact.OC_OH = orgHeader.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_OA_OrgAddress = orgAddress.PK;
			receivingForwarderContact.OC_OH = orgHeader.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("Sender", AWBHeader.EH_ShipperContactName);
				AssertEquals("FX", AWBHeader.EH_ShipperContactCode);
				AssertEquals("123456", AWBHeader.EH_ShipperContactDetail);
				AssertEquals("Receiver", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("FX", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("123456", AWBHeader.EH_ConsigneeContactDetail);
			});

			orgAddress.OA_Fax = ZString.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = orgAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = orgAddress.PK;
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("Shipper contact code", ZString.Empty, AWBHeader.EH_ShipperContactCode);
				AssertEquals("Shipper contact details", ZString.Empty, AWBHeader.EH_ShipperContactDetail);
				AssertEquals("Consignee contact code", ZString.Empty, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("Consignee contact details", ZString.Empty, AWBHeader.EH_ConsigneeContactDetail);
			});
		}

		public void TestPopulateContactDetailsDefaultsPhone_FallbackFromAddress()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Phone = "+96852";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var nonMainAddress = orgHeader.Addresses.AddNew();
			nonMainAddress.OA_Phone = "+96999";

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_OA_OrgAddress = nonMainAddress.PK;
			sendingForwarderContact.OC_OH = orgHeader.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_OA_OrgAddress = nonMainAddress.PK;
			receivingForwarderContact.OC_OH = orgHeader.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			consol.JK_OA_SendingForwarderAddress = nonMainAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
			consol.JK_OA_ReceivingForwarderAddress = nonMainAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("Sender", AWBHeader.EH_ShipperContactName);
				AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
				AssertEquals("+96999", AWBHeader.EH_ShipperContactDetail);
				AssertEquals("Receiver", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("+96999", AWBHeader.EH_ConsigneeContactDetail);
			});

			consol.JK_OC_SendingForwarderContact = ZGuid.Empty;
			consol.JK_OC_ReceivingForwarderContact = ZGuid.Empty;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactName);
				AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
				AssertEquals("+96999", AWBHeader.EH_ShipperContactDetail);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactName);
				AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("+96999", AWBHeader.EH_ConsigneeContactDetail);
			});

			nonMainAddress.OA_Phone = ZString.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = nonMainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = nonMainAddress.PK;
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactName);
				AssertEquals("TE", AWBHeader.EH_ShipperContactCode);
				AssertEquals("+96852", AWBHeader.EH_ShipperContactDetail);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactName);
				AssertEquals("TE", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("+96852", AWBHeader.EH_ConsigneeContactDetail);
			});

			orgAddress.OA_Phone = ZString.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = nonMainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = nonMainAddress.PK;
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactName);
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactCode);
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactDetail);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactName);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactDetail);
			});
		}

		public void TestPopulateContactDetailsDefaultsFax_FallbackFromAddress()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Fax = "123456";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var nonMainAddress = orgHeader.Addresses.AddNew();
			nonMainAddress.OA_Fax = "654321";

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_OA_OrgAddress = nonMainAddress.PK;
			sendingForwarderContact.OC_OH = orgHeader.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_OA_OrgAddress = nonMainAddress.PK;
			receivingForwarderContact.OC_OH = orgHeader.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			consol.JK_OA_SendingForwarderAddress = nonMainAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
			consol.JK_OA_ReceivingForwarderAddress = nonMainAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("Sender", AWBHeader.EH_ShipperContactName);
				AssertEquals("FX", AWBHeader.EH_ShipperContactCode);
				AssertEquals("654321", AWBHeader.EH_ShipperContactDetail);
				AssertEquals("Receiver", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("FX", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("654321", AWBHeader.EH_ConsigneeContactDetail);
			});

			consol.JK_OC_SendingForwarderContact = ZGuid.Empty;
			consol.JK_OC_ReceivingForwarderContact = ZGuid.Empty;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactName);
				AssertEquals("FX", AWBHeader.EH_ShipperContactCode);
				AssertEquals("654321", AWBHeader.EH_ShipperContactDetail);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactName);
				AssertEquals("FX", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("654321", AWBHeader.EH_ConsigneeContactDetail);
			});

			nonMainAddress.OA_Fax = ZString.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = nonMainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = nonMainAddress.PK;
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactName);
				AssertEquals("FX", AWBHeader.EH_ShipperContactCode);
				AssertEquals("123456", AWBHeader.EH_ShipperContactDetail);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactName);
				AssertEquals("FX", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("123456", AWBHeader.EH_ConsigneeContactDetail);
			});

			orgAddress.OA_Fax = ZString.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = nonMainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = nonMainAddress.PK;
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactName);
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactCode);
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactDetail);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactName);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactCode);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactDetail);
			});
		}

		public void TestPopulateContactDetailsDefaultsContactName_WhenDirectShipmentAndNoContact()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_CompanyNameOverride = "AWBCOMPANYNAME";
			orgAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			orgAddress.OA_Address1 = "ADDRESSAWB";
			orgAddress.OA_Address2 = "ADDRESSAWB2";
			orgAddress.OA_City = "BRISBANE";
			orgAddress.OA_PostCode = "2266";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_Fax = "123456";
			orgAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);

			var nonMainAddress = orgHeader.Addresses.AddNew();
			nonMainAddress.OA_Fax = "654321";

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_ContactName = "Sender";
			sendingForwarderContact.OC_OA_OrgAddress = nonMainAddress.PK;
			sendingForwarderContact.OC_OH = orgHeader.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_ContactName = "Receiver";
			receivingForwarderContact.OC_OA_OrgAddress = nonMainAddress.PK;
			receivingForwarderContact.OC_OH = orgHeader.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;
			consol.JK_OA_SendingForwarderAddress = nonMainAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;
			consol.JK_OA_ReceivingForwarderAddress = nonMainAddress.PK;

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();
			CombineAssertions(() =>
			{
				AssertEquals("Sender", AWBHeader.EH_ShipperContactName);
				AssertEquals("Receiver", AWBHeader.EH_ConsigneeContactName);
			});

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var consignor = GetOrganization(Factory, "CNR01");
			var consignee = GetOrganization(Factory, "CNE01");
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			AWBHeader.Populate();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, AWBHeader.EH_ShipperContactName);
				AssertEquals(ZString.Empty, AWBHeader.EH_ConsigneeContactName);
			});

			var consignorContact = consignor.Contacts.AddNew();
			consignorContact.OC_ContactName = "John.Snow";
			var consigneeContact = consignee.Contacts.AddNew();
			consigneeContact.OC_ContactName = "LittleFinger";
			shipment.ConsignorDocumentaryAddress.ContactPK = consignorContact.PK;
			shipment.ConsigneeDocumentaryAddress.ContactPK = consigneeContact.PK;
			AWBHeader.Populate();
			CombineAssertions(() =>
			{
				AssertEquals("John.Snow", AWBHeader.EH_ShipperContactName);
				AssertEquals("LittleFinger", AWBHeader.EH_ConsigneeContactName);
			});
		}

		#endregion

		public void TestPopulateContactDetails_FromOverriddenAddress()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Office;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var consignor = GetOrganization(Factory, "CNR01");
			var consignee = GetOrganization(Factory, "CNE01");

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_Contact = "LittleFinger";
			shipment.ConsigneeDocumentaryAddress.E2_Phone = "+10155355548";

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_Contact = "John.Snow";
			shipment.ConsignorDocumentaryAddress.E2_Phone = "+8615635554286";

			AWBHeader.SetConsol(consol);
			AWBHeader.Populate();

			CombineAssertions(() =>
			{
				AssertEquals("EH_ShipperContactName", "John.Snow", AWBHeader.EH_ShipperContactName);
				AssertEquals("EH_ShipperContactCode", "TE", AWBHeader.EH_ShipperContactCode);
				AssertEquals("EH_ShipperContactDetail", "+8615635554286", AWBHeader.EH_ShipperContactDetail);
				AssertEquals("EH_ConsigneeContactName", "LittleFinger", AWBHeader.EH_ConsigneeContactName);
				AssertEquals("EH_ConsigneeContactCode", "TE", AWBHeader.EH_ConsigneeContactCode);
				AssertEquals("EH_ConsigneeContactDetail", "+10155355548", AWBHeader.EH_ConsigneeContactDetail);
			});
		}

		#region Notify Address

		public void TestNotifyPartyDocumentaryAddress()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgAddress org1Address = org1.MainAddress;

			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgAddress org2Address = org2.MainAddress;

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.Shipments[0].NotifyPartyDocumentaryAddress.OrganisationPK = org1.PK;
			AWBHeader.Consol.NotifyPartyDocumentaryAddress.OrganisationPK = org2.PK;

			AssertEquals("prerequsite", org1Address, AWBHeader.Consol.Shipments[0].NotifyPartyDocumentaryAddress.Address);
			AssertEquals(org1Address, AWBHeader.NotifyPartyDocumentaryAddress.Address);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("prerequsite", org2Address, AWBHeader.Consol.NotifyPartyDocumentaryAddress.Address);
			AssertEquals(org2Address, AWBHeader.NotifyPartyDocumentaryAddress.Address);
		}

		public void TestGetAlsoNotifyAddreses()
		{
			AWBHeader.Consol.Shipments[0].ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;

			AssertEquals("prerequisite", 1, AWBHeader.Consol.Shipments[0].Consignee.Addresses.Count);
			OrgAddress consigneeAddress = AWBHeader.Consol.Shipments[0].Consignee.MainAddress;

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			OrgAddress receivingForwarderAddress = receivingForwarder.MainAddress;

			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgAddress org1Address1 = org1.MainAddress;
			OrgAddress org1Address2 = org1.Addresses.AddNew();

			AWBHeader.Consol.Shipments[0].NotifyPartyDocumentaryAddress.OrganisationPK = org1.PK;

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertContainsExactElementsInAnyOrder(new[] { org1Address1, org1Address2, consigneeAddress }, AWBHeader.GetAlsoNotifyAddresses());

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddress.PK;
			AssertEquals("prerequisite", receivingForwarder, AWBHeader.Consol.ReceivingForwarder);
			AssertContainsExactElementsInAnyOrder(new[] { receivingForwarderAddress }, AWBHeader.GetAlsoNotifyAddresses());

			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgAddress org2Address1 = org2.MainAddress;
			OrgAddress org2Address2 = org2.Addresses.AddNew();

			AWBHeader.Consol.NotifyPartyDocumentaryAddress.OrganisationPK = org2.PK;

			OrgHeader org3 = Factory.New<OrgHeader>();
			OrgAddress org3Address1 = org3.MainAddress;
			OrgAddress org3Address2 = org3.Addresses.AddNew();

			AWBHeader.Consol.NotifyParty2DocumentaryAddress.OrganisationPK = org3.PK;

			OrgHeader org4 = Factory.New<OrgHeader>();
			OrgAddress org4Address1 = org4.MainAddress;
			OrgAddress org4Address2 = org4.Addresses.AddNew();

			AWBHeader.Consol.NotifyParty3DocumentaryAddress.OrganisationPK = org4.PK;

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertContainsExactElementsInAnyOrder(new[] { org1Address1, org1Address2, consigneeAddress }, AWBHeader.GetAlsoNotifyAddresses());

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;

			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddress.PK;
			AssertEquals("prerequisite", receivingForwarder, AWBHeader.Consol.ReceivingForwarder);

			AWBHeader.Consol.NotifyPartyDocumentaryAddress.OrganisationPK = org2.PK;
			AssertEquals("prerequisite", org2, AWBHeader.Consol.NotifyParty);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				org2Address1, org2Address2,
				org3Address1, org3Address2,
				org4Address1, org4Address2,
				receivingForwarderAddress
			}, AWBHeader.GetAlsoNotifyAddresses());
		}

		#endregion

		#region Addresses Common

		void AddAddress(OrgHeader org, ZString addressType)
		{
			OrgAddress address = org.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(addressType);
			address.OA_CompanyNameOverride = "COMPANYNAME";
		}

		#endregion

		#endregion

		#region Security Declaration

		public void TestPopulatePermitDetails_HongKong()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "SGSIN";

				var approvedAddress1 = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_RN_NKCountryCode, "HK"));
				var countryData1 = approvedAddress1.KnownShipperDetails.AddNew();
				countryData1.OV_OH_OrgHeader = approvedAddress1.OA_OH;
				countryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData1.OV_EXApprovalNumber = "KC00012";
				countryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var approvedOrg = Factory.Load<OrgHeader>(approvedAddress1.OA_OH);
				var approvedAddress2 = approvedOrg.Addresses.AddNew();
				approvedAddress2.Address1 = "83 Des Voeux Road Central";

				var countryData2 = approvedAddress2.KnownShipperDetails.AddNew();
				countryData2.OV_OH_OrgHeader = approvedAddress2.OA_OH;
				countryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData2.OV_EXApprovalNumber = "KC00012";
				countryData2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment1.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress1.PK;
				shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment2.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress2.PK;
				shipment2.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				Factory.Save();

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertEquals("ExportAWBSecurityStatusLines should only have one entry", 1, header.ExportAWBSecurityStatusLines.Count);
				AssertEquals("EAS_ApprovalNumber", "KC00012", header.ExportAWBSecurityStatusLines[0].EAS_ApprovalNumber);
				AssertEquals("EAS_RN_NKCountryCode comes from the approval address", "HK", header.ExportAWBSecurityStatusLines[0].EAS_RN_NKCountryCode);
				AssertEquals("EAS_ApprovalExpiryDate", ZDate.Today.AddDays(1), header.ExportAWBSecurityStatusLines[0].EAS_ApprovalExpiryDate);
			}
		}

		[TestDate(2023, 7, 20, 1, 1, 0)]
		public void TestPopulateSecurityDeclaration()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			header.Populate();

			AssertEquals("EH_AgentApprovalCategory", AviationSecuritySchemeMembership.Codes.RegulatedAgent, header.EH_AgentApprovalCategory);
			AssertEquals("EH_AgentApprovalNumber", ZString.Empty, header.EH_AgentApprovalNumber);
			AssertEquals("EH_RN_NKAgentApprovalCountryCode", GlbCompany.CurrentCompany.Country.Code, header.EH_RN_NKAgentApprovalCountryCode);
			AssertEquals("EH_SecurityStatusIssueDate", ZDateTime.Now, header.EH_SecurityStatusIssueDate);
			AssertEquals("EH_SecurityStatusIssuedBy", GlbStaff.CurrentUser.GS_FullName, header.EH_SecurityStatusIssuedBy);
			AssertEquals("EH_GS_NKSecurityStatusIssuedByCode", GlbStaff.CurrentUser.GS_Code, header.EH_GS_NKSecurityStatusIssuedByCode);
		}

		public void TestPopulateRAPrefixOnHKAgentApprovalNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				sendingForwarder.OH_Code = "TESTORG1";

				var sendingForwarderAddress = Factory.New<OrgAddress>();
				sendingForwarderAddress.OA_OH = sendingForwarder.PK;

				sendingForwarder.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				sendingForwarder.CountryData.OV_EXApprovalNumber = "RA34576";
				sendingForwarder.CountryData.OV_OA_ApprovedLocation = sendingForwarderAddress.PK;

				consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertEquals("RA34576", header.EH_AgentApprovalNumber);
			}
		}

		public void TestPopulateSecurityDeclaration_ExtractExemptionCodesPopulatesWithOldEXMCodes()
		{
			FreightDataRegistry.Instance.EXMExemptionCodeRemovalDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2015, 1, 1));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = "EXM";
			shipment.JS_SystemCreateTimeUtc = new DateTime(2011, 1, 1);
			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			header.Populate();
			Assert(header.ExportAWBSecurityStatusLines[0] != null);
			AssertEquals("EXM", header.ExportAWBSecurityStatusLines[0].EAS_ExemptionGround);

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_InspectionTypeCode = "EXM";
			shipment2.JS_SystemCreateTimeUtc = new DateTime(2016, 1, 1);
			var header2 = Factory.New<ConsolExportAWBHeader>();
			header2.EH_ParentID = consol2.PK;

			header2.Populate();
			Assert(header2.ExportAWBSecurityStatusLines[0] == null);
		}

		public void TestPopulateSecurityDeclaration_ExtractExemptionCodesReturnsIATACodes()
		{
			string[] invalidCodes = { ExemptionCodes.Codes.SmallUndersizedShipments, ExemptionCodes.Codes.Mail, ExemptionCodes.Codes.BiomedicalSamples, ExemptionCodes.Codes.DiplomaticBagsOrDiplomaticMail, ExemptionCodes.Codes.LifeSavingMaterials, ExemptionCodes.Codes.NuclearMaterial, ExemptionCodes.Codes.TransferOrTransshipment };
			string[] iATACodes = ShipmentInspectionType.GetIATAExemptionCodes();

			for (int i = 0; i < invalidCodes.Length; i++)
			{
				AssertCodeInputtedIsMappedToIATACode(invalidCodes[i], iATACodes[i]);
			}
		}

		public void TestPopulateSecurityDeclaration_PackLevelScreening()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment1.JS_InspectionTypeCode = "NUC";

				var pack1_1 = shipment1.OuterPackLines.AddNew();
				pack1_1.JL_InspectionTypeCode = "UNK";
				var pack1_2 = shipment1.OuterPackLines.AddNew();
				pack1_2.JL_InspectionTypeCode = "MAI";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment2.JS_InspectionTypeCode = "EMD";

				var pack2_1 = shipment1.OuterPackLines.AddNew();
				pack2_1.JL_InspectionTypeCode = "XRY";
				var pack2_2 = shipment1.OuterPackLines.AddNew();
				pack2_2.JL_InspectionTypeCode = "PHS";

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_TransportMode = TransportModes.Air;
				shipment3.JS_InspectionTypeCode = "SCR";

				var pack3_1 = shipment3.OuterPackLines.AddNew();
				pack3_1.JL_InspectionTypeCode = "XRY";
				var pack3_2 = shipment3.OuterPackLines.AddNew();
				pack3_2.JL_InspectionTypeCode = "MAI";
				AssertHasError(shipment3.JS_InspectionTypeCodeInfo, "SCR - Screened cannot be chosen here as it applies when all Packing Inspections are entered. Packing Inspections are not available on this Shipment.");

				shipment3.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
				shipment3.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
				AssertHasError(shipment3.JS_InspectionTypeCodeInfo, "The Inspection Type of SCR cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

				var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;
				var addressProxy = orgProxy.MainAddress;
				GlbBranch.CurrentBranch.GB_OA_AddressProxy = addressProxy.PK;
				var addressProxyApproval = addressProxy.KnownShipperDetails.AddNew();
				addressProxyApproval.OV_OH_OrgHeader = addressProxy.OA_OH;
				addressProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
				addressProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);
				addressProxyApproval.Factory.Save();
				consol.JK_OA_SendingForwarderAddress = addressProxy.PK;
				shipment3.Validation.ValidateJS_InspectionTypeCode();
				AssertNoErrors(shipment3.JS_InspectionTypeCodeInfo);

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertCollectionContains("UNK", consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ScreeningMethod));
				AssertCollectionContains("XRY", consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ScreeningMethod));
				AssertCollectionContains("PHS", consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ScreeningMethod));
				AssertCollectionNotContains("Shipment level inspection codes are not included", "EMD", consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ScreeningMethod));

				AssertCollectionContains("MAIL", consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ExemptionGround));
				AssertCollectionNotContains("Shipment level exemption codes are not included", "NUCL", consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ExemptionGround));

				AssertCollectionNotContains("SCR is not included", "SCR", consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ScreeningMethod));
			}
		}

		public void TestPopulateSecurityDeclaration_PackLevelScreening_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "FRPAR";
				consol.JK_RL_NKDischargePort = "HKHKG";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = TransportModes.Air;
				shipment1.JS_InspectionTypeCode = "NUC";

				var pack1_1 = shipment1.OuterPackLines.AddNew();
				pack1_1.JL_InspectionTypeCode = "";
				pack1_1.JL_AdditionalInspectionTypeCode = "";
				var pack1_2 = shipment1.OuterPackLines.AddNew();
				pack1_2.JL_InspectionTypeCode = "";
				pack1_2.JL_AdditionalInspectionTypeCode = "";

				AssertEquals("Shipment 1 inspection code changed to UNK because of the packlines with empty inspection", "UNK", shipment1.JS_InspectionTypeCode);
				shipment1.JS_InspectionTypeCode = "NUC";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = TransportModes.Air;
				shipment2.JS_InspectionTypeCode = "SCR";

				var pack2_1 = shipment2.OuterPackLines.AddNew();
				pack2_1.JL_InspectionTypeCode = "XRY";
				pack2_1.JL_AdditionalInspectionTypeCode = "MAI";
				var pack2_2 = shipment2.OuterPackLines.AddNew();
				pack2_2.JL_InspectionTypeCode = "MAI";
				pack2_2.JL_IsHighRisk = true;
				pack2_2.JL_AdditionalInspectionTypeCode = "EDS";

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_TransportMode = TransportModes.Air;
				shipment3.JS_InspectionTypeCode = "UNK";

				var pack3_1 = shipment3.OuterPackLines.AddNew();
				pack3_1.JL_InspectionTypeCode = "UNK";
				pack3_1.JL_AdditionalInspectionTypeCode = "XRY";
				var pack3_2 = shipment3.OuterPackLines.AddNew();
				pack3_2.JL_InspectionTypeCode = "XRY";
				pack3_2.JL_IsHighRisk = true;
				pack3_2.JL_AdditionalInspectionTypeCode = "EDS";

				var shipment4 = consol.Shipments.AddNew();
				shipment4.JS_TransportMode = TransportModes.Air;
				shipment4.JS_InspectionTypeCode = "EDS";

				var pack4_1 = shipment4.OuterPackLines.AddNew();
				pack4_1.JL_InspectionTypeCode = "";
				pack4_1.JL_AdditionalInspectionTypeCode = "";
				var pack4_2 = shipment4.OuterPackLines.AddNew();
				pack4_2.JL_InspectionTypeCode = "";
				pack4_2.JL_AdditionalInspectionTypeCode = "";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertContainsExactElementsInAnyOrder("Both pack level and shipment level exemptions are included",
					new[] { "MAIL", "NUCL" }, consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Where(x => !x.EAS_ExemptionGround.IsEmpty).Select(x => x.EAS_ExemptionGround));
				AssertContainsExactElementsInAnyOrder("Both pack level and shipment level methods are included. SCR is excluded.",
					new[] { "XRY", "UNK", "EDS" }, consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Where(x => !x.EAS_ScreeningMethod.IsEmpty).Select(x => x.EAS_ScreeningMethod));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "FRPAR";
				consol.JK_RL_NKDischargePort = "HKHKG";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_InspectionTypeCode = "MAI";

				var pack1_1 = shipment.OuterPackLines.AddNew();
				pack1_1.JL_InspectionTypeCode = "XRY";
				pack1_1.JL_IsHighRisk = true;
				pack1_1.JL_AdditionalInspectionTypeCode = "MAI";
				AssertEquals(true, shipment.JS_IsHighRisk);
				shipment.JS_IsHighRisk = false;

				var pack1_2 = shipment.OuterPackLines.AddNew();
				pack1_2.JL_InspectionTypeCode = "EDS";
				pack1_2.JL_AdditionalInspectionTypeCode = "UNK";

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();
				AssertContainsExactElementsInAnyOrder("Both pack level and shipment level exemptions are included",
					new[] { "MAIL" }, consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Where(x => !x.EAS_ExemptionGround.IsEmpty).Select(x => x.EAS_ExemptionGround));
				var c = consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Where(x => !x.EAS_ScreeningMethod.IsEmpty).Select(x => x.EAS_ScreeningMethod);
				AssertContainsExactElementsInAnyOrder("Both pack level and shipment level methods are included. UNK is excluded because pack1_2's Is High Risk is not flaged",
					new[] { "XRY", "EDS" }, consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Where(x => !x.EAS_ScreeningMethod.IsEmpty).Select(x => x.EAS_ScreeningMethod));
			}
		}

		void AssertCodeInputtedIsMappedToIATACode(string code, string expected)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = code;

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			header.Populate();
			Assert("Is an IATA type", ShipmentInspectionType.IsIATAExemptionCode(header.ExportAWBSecurityStatusLines[0].EAS_ExemptionGround));
			AssertEquals("EAS_ExemptionGround is IATA code", header.ExportAWBSecurityStatusLines[0].EAS_ExemptionGround, expected);
		}

		public void TestPopulateSecurityDeclaration_SendingForwarderSecurityStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_IsCreditor = true;

			var sendingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			sendingForwarderAddress.OA_Code = "XXX";
			sendingForwarderAddress.OA_OH = sendingForwarder.PK;

			sendingForwarder.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
			sendingForwarder.CountryData.OV_EXApprovalNumber = "123";

			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			header.Populate();

			AssertEquals("EH_AgentApprovalCategory", AviationSecuritySchemeMembership.Codes.AccountConsignor, header.EH_AgentApprovalCategory);
			AssertEquals("EH_AgentApprovalNumber", "123", header.EH_AgentApprovalNumber);
		}

		public void TestPopulateSecurityDeclaration_SendingForwarderSecurityStatus_ResetEH_AgentApprovalNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_IsCreditor = true;

			var sendingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			sendingForwarderAddress.OA_Code = "XXX";
			sendingForwarderAddress.OA_OH = sendingForwarder.PK;

			sendingForwarder.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
			sendingForwarder.CountryData.OV_EXApprovalNumber = "123";

			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			header.Populate();

			AssertEquals("EH_AgentApprovalCategory", AviationSecuritySchemeMembership.Codes.AccountConsignor, header.EH_AgentApprovalCategory);
			AssertEquals("EH_AgentApprovalNumber", "123", header.EH_AgentApprovalNumber);

			consol.JK_OA_SendingForwarderAddress = Guid.Empty;
			header.Populate();
			AssertEquals("EH_AgentApprovalNumber", "", header.EH_AgentApprovalNumber);
		}

		public void TestPopulateSecurityDeclaration_RegulatedAgentIsBranchOrCompanyOrgProxy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var countryData = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = "RA";
				countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);
				countryData.OV_OH_OrgHeader = GlbCompany.CurrentCompany.OrgProxy.PK;

				AssertEquals("RA", GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipper.OV_EXApprovedOrMajorExporter);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;
				header.EH_RN_NKAgentApprovalCountryCode = "SG";
				header.Parent.IsAWBValuesOverriddenProperty = true;

				header.PopulateIfNotOverridden();

				AssertEquals("EH_AgentApprovalCategory", AviationSecuritySchemeMembership.Codes.RegulatedAgent, header.EH_AgentApprovalCategory);
				AssertEquals("EH_AgentApprovalNumber", ZString.Empty, header.EH_AgentApprovalNumber);
				AssertEquals("EH_RN_NKAgentApprovalCountryCode", "SG", header.EH_RN_NKAgentApprovalCountryCode);
				AssertEquals("EH_AgentApprovalExpiryDate", ZDate.Today.AddDays(100), header.EH_AgentApprovalExpiryDate);
				AssertEquals("EH_SecurityStatusIssueDate", header.EH_AWBIssueDate, header.EH_SecurityStatusIssueDate);
				AssertEquals("EH_GS_NKSecurityStatusIssuedByCode", GlbStaff.CurrentUser.GS_Code, header.EH_GS_NKSecurityStatusIssuedByCode);
				AssertEquals("EH_SecurityStatusIssuedBy", GlbStaff.CurrentUser.GS_FullName, header.EH_SecurityStatusIssuedBy);
			}
		}

		public void TestPopulateSecurityDeclaration_AgentApprovalCountryCode_EU()
		{
			AssertAgentApprovalCountryCodePopulated("DE", true, "DE");
		}

		public void TestPopulateSecurityDeclaration_AgentApprovalCountryCode_NonEU()
		{
			AssertAgentApprovalCountryCodePopulated("AU", false, "AU");
		}

		public void TestPopulateSecurityDeclaration_AgentApprovalCountryCode_EUWithIssuingAuthorityCountry()
		{
			AssertAgentApprovalCountryCodePopulated("DE", true, "LI", "LI");
		}

		void AssertAgentApprovalCountryCodePopulated(string loginCountryCode, bool isEU, string expectedAgentApprovalCountryCode, string overridenIssuingAuthorityCountry = "")
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(loginCountryCode))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isEU))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				orgProxy.MainAddress.OA_RN_NKCountryCode = expectedAgentApprovalCountryCode;
				var approval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = orgProxy.PK;
				approval.OV_EXApprovedOrMajorExporter = "RA";
				approval.OV_EXApprovalNumber = "12345-67";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				if (!string.IsNullOrEmpty(overridenIssuingAuthorityCountry))
				{
					approval.OV_RN_NKIssuingAuthorityCountry = overridenIssuingAuthorityCountry;
				}
				approval.Factory.Save();

				AssertEquals("Precondition", isEU ? "EU" : loginCountryCode, approval.OV_RN_NKClientCountryRelation);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;
				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertEquals("EH_RN_NKAgentApprovalCountryCode", expectedAgentApprovalCountryCode, header.EH_RN_NKAgentApprovalCountryCode);
			}
		}

		public void TestEH_AgentApprovalNumber_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				consol.JK_OverrideSecurityDeclarationDefaults = false;
				Assert("Read-only for AU when !JK_OverrideSecurityDeclarationDefaults", header.EH_AgentApprovalNumberInfo.ReadOnly);

				consol.JK_OverrideSecurityDeclarationDefaults = true;
				Assert("Cannot override Agent Approval Number for AU", header.EH_AgentApprovalNumberInfo.ReadOnly);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				consol.JK_OverrideSecurityDeclarationDefaults = false;
				Assert("Read-only for US when !JK_OverrideSecurityDeclarationDefaults", header.EH_AgentApprovalNumberInfo.ReadOnly);

				consol.JK_OverrideSecurityDeclarationDefaults = true;
				Assert("Can override Agent Approval Number for US", !header.EH_AgentApprovalNumberInfo.ReadOnly);
			}
		}

		public void TestEH_AgentApprovalNumber_HiddenOnSecurityDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var approval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_RN_NKClientCountryRelation = "AU";
				approval.OV_OH_OrgHeader = orgProxy.PK;
				approval.OV_EXApprovedOrMajorExporter = "RA";
				approval.OV_EXApprovalNumber = "12345-67";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				approval.Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "MYKUL";
				consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();
				AssertEquals("AgentApprovalNumber populated for RA", "12345-67", header.EH_AgentApprovalNumber);

				approval.OV_EXApprovedOrMajorExporter = "AA";
				approval.Factory.Save();

				header.Populate();
				AssertEquals("Approval number is still valid", "12345-67", approval.OV_EXApprovalNumber);
				AssertEquals("It is not populated on AWB Header for AA", ZString.Empty, header.EH_AgentApprovalNumber);
			}
		}

		public void TestPopulateSecurityStatus_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var approval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = orgProxy.PK;
				approval.OV_EXApprovedOrMajorExporter = "RA";
				approval.OV_EXApprovalNumber = "12345-67";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				approval.OV_RN_NKIssuingAuthorityCountry = "CH";
				approval.Factory.Save();

				AssertEquals("Precondition", "EU", approval.OV_RN_NKClientCountryRelation);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();
				AssertEquals("AgentApprovalNumber Security Status populated for EU", "12345-67", header.EH_AgentApprovalNumber);
				AssertEquals("AgentApprovalCountryCode Security Status populated for EU", "CH", header.EH_RN_NKAgentApprovalCountryCode);
			}
		}

		public void TestTemporarilySuspendCargoSecurityValidation()
		{
			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_AgentApprovalNumber = ZString.Empty;

			header.Validation.ValidateEH_AgentApprovalNumber();
			AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "Enter the identifier of the Regulated Agent issuing the security status.");

			using (header.TemporarilySuspendCargoSecurityValidation())
			{
				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertNoMessageError(header.EH_AgentApprovalNumberInfo, "Enter the identifier of the Regulated Agent issuing the security status.");
			}
		}

		public void TestPopulateScreeningMethods()
		{
			var consol = CreateConsolForSecurityLinesTest();

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			header.Populate();

			AssertPopulateExportAWBSecurityStatusLines(header);
		}

		public void TestPopulateSecurityStatusLines_CachedValuesAreClean()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_TransportMode = TransportModes.Air;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				consol.JK_OverrideSecurityDeclarationDefaults = true;
				var statusLine = header.ExportAWBSecurityStatusLines.AddNew();
				statusLine.EAS_ApprovalCategory = AviationSecuritySchemeMembership.Codes.KnownConsignor;

				Factory.Save();

				var shipmentAPP1 = consol.Shipments.AddNew();
				shipmentAPP1.JS_RL_NKOrigin = "DEFRA";
				shipmentAPP1.JS_RL_NKDestination = "CNSHA";
				shipmentAPP1.JS_TransportMode = TransportModes.Air;

				var shipmentAPP2 = consol.Shipments.AddNew();
				shipmentAPP2.JS_RL_NKOrigin = "DEFRA";
				shipmentAPP2.JS_RL_NKDestination = "CNSHA";
				shipmentAPP2.JS_TransportMode = TransportModes.Air;
				shipmentAPP2.JS_InspectionTypeCode = "APP";

				var consignor = Factory.NewWithValidTestData<OrgHeader>();

				var countryData = consignor.CountryDataCollectionForThisCompany.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalNumber = "";
				countryData.OV_RN_NKClientCountryRelation = "";
				countryData.OV_RN_NKIssuingAuthorityCountry = "";

				shipmentAPP1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipmentAPP2.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

				shipmentAPP1.JS_InspectionTypeCode = "APP";
				shipmentAPP2.JS_InspectionTypeCode = "APP";

				header.Populate();

				CombineAssertions(() =>
				{
					AssertEquals(2, header.ExportAWBSecurityStatusLines.OfType<ExportAWBSecurityStatusLine>().First().Shipments.Count);
					AssertNotNull(header.ExportAWBSecurityStatusLines.OfType<ExportAWBSecurityStatusLine>().First().Organization);
				});

				consol.Shipments.RemoveAll();
				header.Populate();

				CombineAssertions(() =>
				{
					AssertEquals("Cached Shipments should be cleared", 0, header.ExportAWBSecurityStatusLines.OfType<ExportAWBSecurityStatusLine>().First().Shipments.Count);
					AssertNull("Cached Organization should be cleared", header.ExportAWBSecurityStatusLines.OfType<ExportAWBSecurityStatusLine>().First().Organization);
				});
			}
		}

		public void TestPopulateScreeningMethods_HighRiskShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "FRPAR";
				consol.JK_RL_NKDischargePort = "CAMON";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "CAMON";
				shipment.JS_IsHighRisk = true;
				shipment.JS_InspectionTypeCode = "PHS";
				shipment.JS_AdditionalInspectionTypeCode = "XRY";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_RL_NKOrigin = "FRPAR";
				shipment2.JS_RL_NKDestination = "CAMON";
				shipment2.JS_IsHighRisk = true;
				shipment2.JS_InspectionTypeCode = "MAI";
				shipment2.JS_AdditionalInspectionTypeCode = "DIP";

				consol.AWBHeader.Populate();

				var screeningMethods = consol.AWBHeader.ExportAWBSecurityStatusLines
					.Cast<ExportAWBSecurityStatusLine>()
					.Where(line => line.Type == SecurityStatusLineType.ScreeningMethod)
					.Select(line => line.EAS_ScreeningMethod);

				AssertContainsExactElementsInAnyOrder("screening methods", new ZString[] { "PHS", "XRY" }, screeningMethods);

				var exemptionCodes = consol.AWBHeader.ExportAWBSecurityStatusLines
					.Cast<ExportAWBSecurityStatusLine>()
					.Where(line => line.Type == SecurityStatusLineType.ExceptionCode)
					.Select(line => line.EAS_ExemptionGround);

				AssertContainsExactElementsInAnyOrder("exemption codes", new ZString[] { ShipmentInspectionType.GetIATAExemptionCode("MAI"), ShipmentInspectionType.GetIATAExemptionCode("DIP") }, exemptionCodes);
			}
		}

		public void TestNonAOMScreeningMethodClearsAdditionalScreeningMethods()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "JMMBP";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			shipment.JS_RL_NKOrigin = "AUMBJ";
			shipment.JS_RL_NKDestination = "DEHAM";
			shipment.JS_InspectionTypeCode = ScreeningMethods.Codes.SubjectedToAnyOtherMeans;

			consol.AWBHeader.Populate();
			AssertCollectionContains("Precondition - Additional screening method is AOM.", ScreeningMethods.Codes.SubjectedToAnyOtherMeans, consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ScreeningMethod));
			Assert("Precondition - Additional screening method should be read only.", consol.AWBHeader.EH_AdditionalScreeningMethodsInfo.ReadOnly);

			consol.AWBHeader.EH_AdditionalScreeningMethods = "bla bla BLA";
			shipment.JS_InspectionTypeCode = ScreeningMethods.Codes.XRayEquipment;

			consol.AWBHeader.Populate();
			AssertCollectionNotContains("Precondition - Additional screening method is not AOM.", ScreeningMethods.Codes.SubjectedToAnyOtherMeans, consol.AWBHeader.ExportAWBSecurityStatusLines.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ScreeningMethod));
			AssertEquals(ZString.Empty, consol.AWBHeader.EH_AdditionalScreeningMethods);
		}

		public void TestEH_AdditionalScreeningMethods_EditableAndSavedCorrectly()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "JMMBP";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			shipment.JS_RL_NKOrigin = "JMMBP";
			shipment.JS_RL_NKDestination = "DEHAM";
			shipment.JS_InspectionTypeCode = ScreeningMethods.Codes.SubjectedToAnyOtherMeans;

			consol.AWBHeader.Populate();
			Assert("Other Screening Method should be read only", consol.AWBHeader.EH_AdditionalScreeningMethodsInfo.ReadOnly);

			consol.JK_OverrideSecurityDeclarationDefaults = true;
			consol.AWBHeader.EH_AdditionalScreeningMethods = "TEST";
			Factory.Save();
			consol.AWBHeader.Populate();
			Assert("Other Screening Method should not be read only", !consol.AWBHeader.EH_AdditionalScreeningMethodsInfo.ReadOnly);

			consol.AWBHeader.Populate();
			var updatedHeader = new BusinessObjectFactory().LoadTop1<ExportAWBHeader>(new ZQuery(ExportAWBHeaderSchema.EH_ParentID, consol.AWBHeader.EH_ParentID));
			AssertEquals("Other Screening Method should be correctly displayed", "TEST", consol.AWBHeader.EH_AdditionalScreeningMethods);
			AssertEquals("Other Screening Method should be correctly saved", "TEST", updatedHeader.EH_AdditionalScreeningMethods);

			consol.JK_OverrideSecurityDeclarationDefaults = false;
			consol.AWBHeader.Populate();
			Assert("Other Screening Method should be read only", consol.AWBHeader.EH_AdditionalScreeningMethodsInfo.ReadOnly);
			AssertEquals("Other Screening Method should be correctly emptied", ZString.Empty, consol.AWBHeader.EH_AdditionalScreeningMethods);
		}

		void AssertPopulateExportAWBSecurityStatusLines(ConsolExportAWBHeader header)
		{
			AssertEquals("Should not find any permit detail if supply chain security is disabled.", 0, header.CargoSecurityKnownShippers.Count);

			var screeningMethods = header
				.ExportAWBSecurityStatusLines
				.Cast<ExportAWBSecurityStatusLine>()
				.Where(line => line.Type == SecurityStatusLineType.ScreeningMethod)
				.Select(line => line.EAS_ScreeningMethod);

			AssertContainsExactElementsInAnyOrder("screening methods",
				new ZString[] { ScreeningMethods.Codes.VisualCheck, FreightDataRegistry.AviationSecurity_Unknown_Code, "BBB" },
				screeningMethods);

			var exemptionCodes = header
				.ExportAWBSecurityStatusLines
				.Cast<ExportAWBSecurityStatusLine>()
				.Where(line => line.Type == SecurityStatusLineType.ExceptionCode)
				.Select(line => line.EAS_ExemptionGround);

			AssertContainsExactElementsInAnyOrder("exemption codes",
				new ZString[] { ShipmentInspectionType.GetIATAExemptionCode(ExemptionCodes.Codes.SmallUndersizedShipments), ShipmentInspectionType.GetIATAExemptionCode(ExemptionCodes.Codes.NuclearMaterial) },
				exemptionCodes);
		}

		ForwardingConsol CreateConsolForSecurityLinesTest()
		{
			var typesValue = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			typesValue.Types.Add("BBB", (NoResString)"Just for test", false, false);
			FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typesValue);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "JMKIN";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			// screening methods
			var shipmentSCR1 = consol.Shipments.AddNew();
			shipmentSCR1.JS_RL_NKOrigin = "JMKIN";
			shipmentSCR1.JS_RL_NKDestination = "CNSHA";
			shipmentSCR1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentSCR1.JS_InspectionTypeCode = ScreeningMethods.Codes.VisualCheck;

			var shipmentSCR2 = consol.Shipments.AddNew();
			shipmentSCR2.JS_RL_NKOrigin = "JMKIN";
			shipmentSCR2.JS_RL_NKDestination = "CNSHA";
			shipmentSCR2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentSCR2.JS_InspectionTypeCode = ScreeningMethods.Codes.VisualCheck;

			var shipmentSCR3 = consol.Shipments.AddNew();
			shipmentSCR3.JS_RL_NKOrigin = "JMKIN";
			shipmentSCR3.JS_RL_NKDestination = "CNSHA";
			shipmentSCR3.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentSCR3.JS_InspectionTypeCode = "BBB";

			// exemption codes
			var shipmentEX1 = consol.Shipments.AddNew();
			shipmentEX1.JS_RL_NKOrigin = "JMKIN";
			shipmentEX1.JS_RL_NKDestination = "CNSHA";
			shipmentEX1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentEX1.JS_InspectionTypeCode = ExemptionCodes.Codes.SmallUndersizedShipments;

			var shipmentEX2 = consol.Shipments.AddNew();
			shipmentEX2.JS_RL_NKOrigin = "JMKIN";
			shipmentEX2.JS_RL_NKDestination = "CNSHA";
			shipmentEX2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentEX2.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			var shipmentEX3 = consol.Shipments.AddNew();
			shipmentEX3.JS_RL_NKOrigin = "JMKIN";
			shipmentEX3.JS_RL_NKDestination = "CNSHA";
			shipmentEX3.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentEX3.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			// approved
			var shipmentAPP1 = consol.Shipments.AddNew();
			shipmentAPP1.JS_RL_NKOrigin = "JMKIN";
			shipmentAPP1.JS_RL_NKDestination = "CNSHA";
			shipmentAPP1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentAPP1.JS_InspectionTypeCode = "APP";

			var shipmentAPP2 = consol.Shipments.AddNew();
			shipmentAPP2.JS_RL_NKOrigin = "JMKIN";
			shipmentAPP2.JS_RL_NKDestination = "CNSHA";
			shipmentAPP2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentAPP2.JS_InspectionTypeCode = "APP";

			var shipmentAPP3 = consol.Shipments.AddNew();
			shipmentAPP3.JS_RL_NKOrigin = "JMKIN";
			shipmentAPP3.JS_RL_NKDestination = "CNSHA";
			shipmentAPP3.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var countryData = consignor.CountryDataCollectionForThisCompany.AddNew();
			countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
			countryData.OV_EXApprovalNumber = "1234";
			countryData.OV_RN_NKClientCountryRelation = GlbCompany.CurrentCompany.Country.Code;

			shipmentAPP3.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			shipmentAPP3.JS_InspectionTypeCode = "APP";

			// non-approved
			var shipmentUNK1 = consol.Shipments.AddNew();
			shipmentUNK1.JS_RL_NKOrigin = "JMKIN";
			shipmentUNK1.JS_RL_NKDestination = "CNSHA";
			shipmentUNK1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentUNK1.JS_InspectionTypeCode = "UNK";

			Factory.Save();

			return consol;
		}

		public void TestPopulateScreeningMethods_ShipmentPickupFrom()
		{
			var regValue = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.Value;
			regValue["PCU"].ValidationCode = "WARN";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_TransportMode = TransportModes.Air;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				var consignorApproval = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_OA_ApprovedLocation = consignor.MainAddress.PK;
				consignorApproval.OV_RN_NKClientCountryRelation = "EU";
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(20);
				consignorApproval.OV_EXApprovalNumber = "13256";

				var pickupFrom = Factory.NewWithValidTestData<OrgHeader>();
				var pickupFromApproval = pickupFrom.MainAddress.KnownShipperDetails.AddNew();
				pickupFromApproval.OV_OH_OrgHeader = pickupFrom.PK;
				pickupFromApproval.OV_OA_ApprovedLocation = pickupFrom.MainAddress.PK;
				pickupFromApproval.OV_RN_NKClientCountryRelation = "EU";
				pickupFromApproval.OV_EXApprovedOrMajorExporter = "NO";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.ConsignorPickupAddress.E2_OA_Address = pickupFrom.MainAddress.PK;
				shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				Factory.Save();
				header.Populate();

				AssertEquals("Pickup from is not approved, so permit is from Consignor", "KC|13256", FormatPermitDetails(header.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>()).First());

				pickupFromApproval.OV_EXApprovedOrMajorExporter = "KC";
				pickupFromApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-20);
				pickupFromApproval.OV_EXApprovalNumber = "58867";

				Factory.Save();
				header.Populate();

				AssertEquals("Pickup from approval is expired, so permit is from Consignor", "KC|13256", FormatPermitDetails(header.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>()).First());

				pickupFromApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				Factory.Save();
				header.Populate();

				AssertEquals("Pickup location is approved, so permit is from Pickup location", "KC|58867", FormatPermitDetails(header.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>()).First());
			}
		}

		public void TestPopulateScreeningMethods_ShouldOnlyPopulateScreeningMethod_WhenIsExportForAviationSecurityPurposesIsTrue()
		{
			var regValue = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.Value;
			regValue["PCU"].ValidationCode = "WARN";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "USLAX";
				consol.JK_RL_NKDischargePort = "DEFRA";
				consol.JK_TransportMode = TransportModes.Air;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				var consignorApproval = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_OA_ApprovedLocation = consignor.MainAddress.PK;
				consignorApproval.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
				consignorApproval.OV_RN_NKIssuingAuthorityCountry = Constants.CountryCodes.UnitedStates;
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(20);
				consignorApproval.OV_EXApprovalNumber = "13256";

				var pickupFrom = Factory.NewWithValidTestData<OrgHeader>();
				var pickupFromApproval = pickupFrom.MainAddress.KnownShipperDetails.AddNew();
				pickupFromApproval.OV_OH_OrgHeader = pickupFrom.PK;
				pickupFromApproval.OV_OA_ApprovedLocation = pickupFrom.MainAddress.PK;
				pickupFromApproval.OV_RN_NKClientCountryRelation = Constants.CountryCodes.UnitedStates;
				pickupFromApproval.OV_RN_NKIssuingAuthorityCountry = Constants.CountryCodes.UnitedStates;
				pickupFromApproval.OV_EXApprovedOrMajorExporter = "NO";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "DEFRA";
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.ConsignorPickupAddress.E2_OA_Address = pickupFrom.MainAddress.PK;
				shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				Factory.Save();
				header.Populate();

				AssertEquals("Should not populate Security Lines", 0, header.CargoSecurityKnownShippers.Count);

				pickupFromApproval.OV_EXApprovedOrMajorExporter = "KC";
				pickupFromApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-20);
				pickupFromApproval.OV_EXApprovalNumber = "58867";

				Factory.Save();
				header.Populate();

				AssertEquals("Should not populate Security Known Shpper", 0, header.CargoSecurityKnownShippers.Count);
			}
		}

		public void TestPopulatePermitDetails_DoNotUseShipmentPickupFrom()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				AssertEquals("Precondition", "NO", FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.Value["PCU"].ValidationCode);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_TransportMode = TransportModes.Air;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				var consignorApproval = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_OA_ApprovedLocation = consignor.MainAddress.PK;
				consignorApproval.OV_RN_NKClientCountryRelation = "EU";
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(20);
				consignorApproval.OV_EXApprovalNumber = "13256";

				var pickupFrom = Factory.NewWithValidTestData<OrgHeader>();
				var pickupFromApproval = pickupFrom.MainAddress.KnownShipperDetails.AddNew();
				pickupFromApproval.OV_OH_OrgHeader = pickupFrom.PK;
				pickupFromApproval.OV_OA_ApprovedLocation = pickupFrom.MainAddress.PK;
				pickupFromApproval.OV_RN_NKClientCountryRelation = "EU";
				pickupFromApproval.OV_EXApprovedOrMajorExporter = "KC";
				pickupFromApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(25);
				pickupFromApproval.OV_EXApprovalNumber = "88394";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.ConsignorPickupAddress.E2_OA_Address = pickupFrom.MainAddress.PK;
				shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				Factory.Save();
				header.Populate();

				AssertEquals("Pickup from is approved, but it is not configured in OrgsToUse registry, so permit is from Consignor", "KC|13256", FormatPermitDetails(header.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>()).First());
			}
		}

		public void TestPopulatePermitDetails_ShipmentsHaveSameConsignor()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_TransportMode = TransportModes.Air;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				var approval = consignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = consignor.PK;
				approval.OV_OA_ApprovedLocation = consignor.MainAddress.PK;
				approval.OV_RN_NKClientCountryRelation = "EU";
				approval.OV_EXApprovedOrMajorExporter = "KC";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(20);
				approval.OV_EXApprovalNumber = "13256";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USLAX";
				shipment1.JS_TransportMode = TransportModes.Air;
				shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_RL_NKOrigin = "DEFRA";
				shipment2.JS_RL_NKDestination = "USLAX";
				shipment2.JS_TransportMode = TransportModes.Air;
				shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment2.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				Factory.Save();

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertContainsExactElementsInAnyOrder("one permit detail per consignor",
					new[]
					{
					"KC|13256"
					},
					FormatPermitDetails(header.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>()));

				var line = header
					.CargoSecurityKnownShippers
					.Cast<ExportAWBSecurityStatusLine>()
					.First();

				AssertContainsExactElementsInAnyOrder("shipments related to the permit",
					new[]
					{
					shipment1, shipment2
					},
					line.Shipments);

				AssertEquals("organization related to the permit", consignor, line.Organization);
			}
		}

		public void TestPopulatePermitDetails_ShipmentsHaveDifferentConsignor()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_TransportMode = TransportModes.Air;

				var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
				var approval1 = consignor1.MainAddress.KnownShipperDetails.AddNew();
				approval1.OV_OH_OrgHeader = consignor1.PK;
				approval1.OV_OA_ApprovedLocation = consignor1.MainAddress.PK;
				approval1.OV_RN_NKClientCountryRelation = "EU";
				approval1.OV_EXApprovedOrMajorExporter = "KC";
				approval1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(20);
				approval1.OV_EXApprovalNumber = "13256";

				var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
				var approval2 = consignor2.MainAddress.KnownShipperDetails.AddNew();
				approval2.OV_OH_OrgHeader = consignor2.PK;
				approval2.OV_OA_ApprovedLocation = consignor2.MainAddress.PK;
				approval2.OV_RN_NKClientCountryRelation = "EU";
				approval2.OV_EXApprovedOrMajorExporter = "KC";
				approval2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(25);
				approval2.OV_EXApprovalNumber = "88394";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USLAX";
				shipment1.JS_TransportMode = TransportModes.Air;
				shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor1.MainAddress.PK;
				shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_RL_NKOrigin = "DEFRA";
				shipment2.JS_RL_NKDestination = "USLAX";
				shipment2.JS_TransportMode = TransportModes.Air;
				shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor2.MainAddress.PK;
				shipment2.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				Factory.Save();

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertContainsExactElementsInAnyOrder("one permit detail per consignor",
					new[]
					{
					"KC|13256",
					"KC|88394"
					},
					FormatPermitDetails(header.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>()));

				var line1 = header
					.CargoSecurityKnownShippers
					.Cast<ExportAWBSecurityStatusLine>()
					.First(line => line.Organization == consignor1);

				AssertNotNull("line related to consignor1 has been found", consignor1);

				var line2 = header
					.CargoSecurityKnownShippers
					.Cast<ExportAWBSecurityStatusLine>()
					.First(line => line.Organization == consignor2);

				AssertNotNull("line related to consignor2 has been found", consignor2);

				AssertContainsExactElementsInAnyOrder("shipments related to the permit 1",
					new[] { shipment1 },
					line1.Shipments);

				AssertContainsExactElementsInAnyOrder("shipments related to the permit 2",
					new[] { shipment2 },
					line2.Shipments);
			}
		}

		public void TestPopulatePermitDetails_DirectConsolWithAsemblyMaster()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Direct;
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_TransportMode = TransportModes.Air;

				var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
				var approval1 = consignor1.MainAddress.KnownShipperDetails.AddNew();
				approval1.OV_OH_OrgHeader = consignor1.PK;
				approval1.OV_OA_ApprovedLocation = consignor1.MainAddress.PK;
				approval1.OV_RN_NKClientCountryRelation = "EU";
				approval1.OV_EXApprovedOrMajorExporter = "KC";
				approval1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(20);
				approval1.OV_EXApprovalNumber = "13256";

				var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
				var approval2 = consignor2.MainAddress.KnownShipperDetails.AddNew();
				approval2.OV_OH_OrgHeader = consignor2.PK;
				approval2.OV_OA_ApprovedLocation = consignor2.MainAddress.PK;
				approval2.OV_RN_NKClientCountryRelation = "EU";
				approval2.OV_EXApprovedOrMajorExporter = "KC";
				approval2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(25);
				approval2.OV_EXApprovalNumber = "88394";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USLAX";
				shipment1.JS_TransportMode = TransportModes.Air;
				shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor1.MainAddress.PK;
				shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;
				shipment2.JS_RL_NKOrigin = "DEFRA";
				shipment2.JS_RL_NKDestination = "USLAX";
				shipment2.JS_TransportMode = TransportModes.Air;
				shipment2.ConsignorDocumentaryAddress.E2_OA_Address = consignor2.MainAddress.PK;
				shipment2.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				shipment1.JS_JS_ColoadMasterShipment = shipment2.PK;

				Factory.Save();

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertContainsExactElementsInAnyOrder("one permit detail",
					new[]
					{
					"KC|88394"
					},
					FormatPermitDetails(header.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>()));

				var line = header
					.CargoSecurityKnownShippers
					.Cast<ExportAWBSecurityStatusLine>()
					.First();

				AssertContainsExactElementsInAnyOrder("shipments related to the permit should contain ASM shipment only",
					new[]
					{
					shipment2
					},
					line.Shipments);

				AssertEquals("organization related to the permit comes from ASM", consignor2, line.Organization);
			}
		}

		IEnumerable<string> FormatPermitDetails(IEnumerable<ExportAWBSecurityStatusLine> collection)
		{
			return collection
				.Where(line => line.Type == SecurityStatusLineType.KnownConsignor)
				.Select(line => string.Concat(line.EAS_ApprovalCategory, "|", line.EAS_ApprovalNumber));
		}

		public void TestPopulateExemptionCodes()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEFRA";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			header.Populate();

			var exemptionCodes = header
				.ExportAWBSecurityStatusLines
				.Cast<ExportAWBSecurityStatusLine>()
				.Where(line => line.Type == SecurityStatusLineType.ExceptionCode)
				.Select(line => line.EAS_ExemptionGround);

			AssertContainsExactElementsInAnyOrder("exemption codes",
				new ZString[] { ShipmentInspectionType.GetIATAExemptionCode(ExemptionCodes.Codes.NuclearMaterial) },
				exemptionCodes);
		}

		public void TestPopulateExemptionCodes_HongKong()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "HKHKG";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_InspectionTypeCode = ExemptionCodesHK.Codes.HumanRemainsOrAshes;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				var exemptionCodes = header
					.ExportAWBSecurityStatusLines
					.Cast<ExportAWBSecurityStatusLine>()
					.Where(line => line.Type == SecurityStatusLineType.ExceptionCode)
					.Select(line => line.EAS_ExemptionGround);

				AssertContainsExactElementsInAnyOrder("Hong Kong specific exemption codes",
					new ZString[] { ExemptionCodesHK.Codes.HumanRemainsOrAshes },
					exemptionCodes);
			}
		}

		public void TestPopulateExemptionCodes_Japan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "JPOSA";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "JPOSA";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_InspectionTypeCode = ExemptionCodesJP.Codes.LiveAnimal;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				var exemptionCodes = header
					.ExportAWBSecurityStatusLines
					.Cast<ExportAWBSecurityStatusLine>()
					.Where(line => line.Type == SecurityStatusLineType.ExceptionCode)
					.Select(line => line.EAS_ExemptionGround);

				AssertContainsExactElementsInAnyOrder("Japan specific exemption codes",
					new ZString[] { ExemptionCodesJP.Codes.LiveAnimal },
					exemptionCodes);
			}
		}

		public void TestFetchHintsForConsolsOnPopulate()
		{
			var factory = new BusinessObjectFactory();

			var loadedConsol = factory.Load<ForwardingModuleConsol>(CreateTestConsol());

			var count = loadedConsol.Shipments.Count;

			factory.ResetDatabaseLoadCount();

			var header = loadedConsol.AWBHeader;

			AssertMaxDbHits(70, factory);

			//CusEntryNum: 11
			//ExportAWBHeader: 11
			//JobDeclaration: 11
			//OrgAddress: 9
			//StmNote: 4
			//JobConsolCost: 3
			//RefAirline: 3
			//RefCountry: 3
			//JobDocAddress: 2
			//JobMawb: 2
			//RefUNLOCO: 2
			//GenPivot: 1
			//JobConsolTransport: 1
			//JobContainer: 1
			//JobDocsAndCartage: 1
			//JobPackLines: 1
			//OrgAddressCapability: 1
			//OrgHeader: 1
			//OrgMiscServ: 1
			//RefCurrency: 1
		}

		ZGuid CreateTestConsol()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "08112345111";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08112345111";
			consol.JK_PrepaidCollect = "PP";

			var cusEntryNum = consol.CusEntryNums.AddNew();
			cusEntryNum.CE_EntryNum = "CX34555";
			cusEntryNum.CE_ParentTable = consol.TableName;

			for (int i = 0; i < 10; i++)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.JS_InspectionTypeCode = "APP";

				var consignor = GetOrganization(factory, "CNR" + i);
				var consignee = GetOrganization(factory, "CNE" + i);

				var consignorAddr = consignor.Addresses.AddNew();
				consignorAddr.OA_Address1 = "aaa";

				var consigneeAddr = consignor.Addresses.AddNew();
				consigneeAddr.OA_Address1 = "bbb";

				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;

				if (i == 1)
				{
					consignorAddr.OA_RL_NKRelatedPortCode = "TCGDT";
					consigneeAddr.OA_RL_NKRelatedPortCode = "REANN";

					consignor.OH_RL_NKClosestPort = "JPTYO";
					consignee.OH_RL_NKClosestPort = "CAAAB";

					shipment.JS_RL_NKOrigin = "DEMIO";
					shipment.JS_RL_NKDestination = "CAAAB";
				}

				if (i == 2)
				{
					shipment.JS_RL_NKOrigin = "AUMEL";
					shipment.JS_RL_NKDestination = "USMIA";

					consignor.OH_RL_NKClosestPort = "INABG";
					consignee.OH_RL_NKClosestPort = "PAABA";

					consignorAddr.OA_RL_NKRelatedPortCode = "DE222";
					consigneeAddr.OA_RL_NKRelatedPortCode = "REANN";
				}

				var job = new JobHeader.Loader(shipment).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
			}

			factory.Save();

			return consol.PK;
		}

		OrgHeader GetOrganization(BusinessObjectFactory factory, ZString code)
		{
			var organization = factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = code;
			organization.MainAddress.OA_Code = "ADD1";

			var miscServ = organization.MiscServ;

			var contact = organization.Contacts.AddNew();
			var document = contact.Documents.AddNew();

			var orgCountryData = factory.NewWithValidTestData<OrgCountryData>();
			orgCountryData.OV_OH_OrgHeader = organization.PK;
			orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
			orgCountryData.OV_EXApprovalNumber = "A123";

			return organization;
		}

		public void TestPopulateSecurityDeclaration_StripRAPrefix()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_IsCreditor = true;

			var sendingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			sendingForwarderAddress.OA_Code = "XXX";
			sendingForwarderAddress.OA_OH = sendingForwarder.PK;

			sendingForwarder.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			sendingForwarder.CountryData.OV_EXApprovalNumber = "RA14523";

			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_InspectionTypeCode = ExemptionCodes.Codes.NuclearMaterial;

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;

			header.Populate();

			AssertEquals("EH_AgentApprovalCategory", AviationSecuritySchemeMembership.Codes.RegulatedAgent, header.EH_AgentApprovalCategory);
			AssertEquals("OV_EXApprovalNumber", "RA14523", sendingForwarder.CountryData.OV_EXApprovalNumber);
			AssertEquals("EH_AgentApprovalNumber", "14523", header.EH_AgentApprovalNumber);
		}

		public void TestPopulateSecurityDeclaration_OverrideDefaultValues_ReceivedFrom()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "FRCDG";
				consol.JK_OverrideSecurityDeclarationDefaults = true;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "FRCDG";

				var securityStatusLine = consol.AWBHeader.ExportAWBSecurityStatusLines.AddNew();
				securityStatusLine.EAS_ApprovalCategory = "KC";
				securityStatusLine.EAS_RN_NKCountryCode = CountryCodes.Italy;
				securityStatusLine.EAS_ApprovalNumber = "12345";

				consol.AWBHeader.Populate();

				var knownShippers = consol.AWBHeader.CargoSecurityKnownShippers;
				AssertEquals("Known Shipper Count", 1, knownShippers.Count);

				var knownShipper = knownShippers.First() as ExportAWBSecurityStatusLine;
				AssertEquals("EAS_ApprovalCategory", "KC", knownShipper.EAS_ApprovalCategory);
				AssertEquals("EAS_RN_NKCountryCode", CountryCodes.Italy, knownShipper.EAS_RN_NKCountryCode);
				AssertEquals("EAS_ApprovalNumber", "12345", knownShipper.EAS_ApprovalNumber);
			}
		}

		public void TestPopulatePermitDetails_StripRAPrefix()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_TransportMode = TransportModes.Air;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				var countryData = consignor.CountryDataCollectionForThisCompany.AddNew();
				countryData.OV_OH_OrgHeader = consignor.PK;
				countryData.OV_OA_ApprovedLocation = consignor.MainAddress.PK;
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				countryData.OV_EXApprovalNumber = "RA14523";
				countryData.OV_RN_NKClientCountryRelation = "EU";
				countryData.OV_RN_NKIssuingAuthorityCountry = "AG";
				countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USLAX";
				shipment1.JS_TransportMode = TransportModes.Air;
				shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				Factory.Save();

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertContainsExactElementsInAnyOrder("one permit detail per consignor",
					new[]
					{
					"RA|14523"
					},
					FormatPermitDetails(header.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>()));

				var line = header
					.CargoSecurityKnownShippers
					.Cast<ExportAWBSecurityStatusLine>()
					.First();

				AssertEquals("countryData.OV_EXApprovalNumber", "RA14523", countryData.OV_EXApprovalNumber);
				AssertEquals("EAS_ApprovalNumber", "14523", line.EAS_ApprovalNumber);
				AssertEquals("EAS_RN_NKCountryCode", "AG", line.EAS_RN_NKCountryCode);
				AssertEquals("EAS_ApprovalExpiryDate", ZDate.Today.AddDays(1), line.EAS_ApprovalExpiryDate);
			}
		}

		public void TestPopulatePermitDetails_EuropeanUnion()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_TransportMode = TransportModes.Air;

				var approvedAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_RN_NKCountryCode, "FR"));
				var countryData = approvedAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = approvedAddress.OA_OH;
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalNumber = "14523";
				countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				countryData.OV_RN_NKIssuingAuthorityCountry = "FR";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_RL_NKOrigin = "DEFRA";
				shipment1.JS_RL_NKDestination = "USLAX";
				shipment1.JS_TransportMode = TransportModes.Air;
				shipment1.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
				shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

				Factory.Save();

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				var line = header
					.CargoSecurityKnownShippers
					.Cast<ExportAWBSecurityStatusLine>()
					.First();

				AssertEquals("EAS_ApprovalNumber", "14523", line.EAS_ApprovalNumber);
				AssertEquals("countryData.OV_RN_NKClientCountryRelation", "EU", countryData.OV_RN_NKClientCountryRelation);
				AssertEquals("EAS_RN_NKCountryCode comes from the approval address", "FR", line.EAS_RN_NKCountryCode);
				AssertEquals("EAS_ApprovalExpiryDate", ZDate.Today.AddDays(1), line.EAS_ApprovalExpiryDate);
			}
		}

		public void TestPopulatePermitDetails_SupplyChainSecurityDisabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertPermitDetailDefaulting("DEFRA", "USLAX", "EU", "AG", AviationSecuritySchemeMembership.Codes.RegulatedAgent, 0);
			}
		}

		public void TestPopulatePermitDetails_SupplyChainSecurityForJapan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Japan))
			{
				AssertPermitDetailDefaulting("JPTYO", "USLAX", "JP", "JP", AviationSecuritySchemeMembership.Codes.KnownConsignor, 1);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				AssertPermitDetailDefaulting("USLAX", "JPTYO", "US", "US", AviationSecuritySchemeMembershipEx.Codes.No, 0);
			}
		}

		void AssertPermitDetailDefaulting(ZString loadPort, ZString dischargePort, ZString clientCountry, ZString issuingCountry, ZString approvalType, ZInt knownShippersCount)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_TransportMode = TransportModes.Air;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var countryData = consignor.CountryDataCollectionForThisCompany.AddNew();
			countryData.OV_OH_OrgHeader = consignor.PK;
			countryData.OV_OA_ApprovedLocation = consignor.MainAddress.PK;
			countryData.OV_EXApprovedOrMajorExporter = approvalType;
			countryData.OV_EXApprovalNumber = "RA14523";
			countryData.OV_RN_NKClientCountryRelation = clientCountry;
			countryData.OV_RN_NKIssuingAuthorityCountry = issuingCountry;
			countryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = loadPort;
			shipment1.JS_RL_NKDestination = dischargePort;
			shipment1.JS_TransportMode = TransportModes.Air;
			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			Factory.Save();

			var header = Factory.New<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			shipment1.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;

			header.Populate();

			AssertEquals("Should not find any permit detail if supply chain security is disabled.", knownShippersCount, header.CargoSecurityKnownShippers.Count);
		}

		public void TestEH_AgentApprovedExporterNumber_BorrowedMaster()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var companyCountryData = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				companyCountryData.OV_EXApprovedOrMajorExporter = "RA";
				companyCountryData.OV_EXApprovalNumber = "RA12345";

				Factory.Save();
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_AgentType = "AGT";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_IsNeutralMaster = true;

				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "176";
				mawb.JM_MAWB = "10000001";
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_ServiceLevel = "STD";
				mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
				mawb.JM_ParentID = consol.PK;

				Factory.Save();

				var awbHeader = consol.AWBHeader;
				var securityStatus = awbHeader.AWBSpecialHandlingItems.AddNew();
				securityStatus.EP_SpecialHandling = "SPX";

				var borrowedFrom = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var borrowedAddress = borrowedFrom.Addresses.AddNew();
				mawb.JM_OA_From = borrowedAddress.PK;
				AssertEquals("Borrowed without code", ZString.Empty, awbHeader.EH_AgentApprovedExporterNumber);

				var borrowedCountryData = borrowedAddress.KnownShipperDetails.AddNew();
				borrowedCountryData.OV_EXApprovedOrMajorExporter = "RA";
				borrowedCountryData.OV_EXApprovalNumber = "RA99999";

				AssertEquals("Borrowed with code", "RA99999", awbHeader.EH_AgentApprovedExporterNumber);
			}
		}

		public void TestEH_AgentApprovedExporterNumber_Taiwan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TW"))
			using (FreightDataRegistry.Instance.RegulatedAgentNumber_TW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "RA99999"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_AgentType = "AGT";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "TWTPE";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_IsNeutralMaster = true;

				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "176";
				mawb.JM_MAWB = "10000001";
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_ServiceLevel = "STD";
				mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
				mawb.JM_ParentID = consol.PK;

				AssertEquals("AgentApprovedExporterNumber is shown", "RA99999", consol.AWBHeader.EH_AgentApprovedExporterNumber);

				consol.JK_RL_NKLoadPort = "DEFRA";
				consol.JK_RL_NKDischargePort = "TWTPE";

				AssertEquals("AgentApprovedExporterNumber is not shown for import", "", consol.AWBHeader.EH_AgentApprovedExporterNumber);

				consol.JK_RL_NKLoadPort = "TWTPE";
				consol.JK_RL_NKDischargePort = "DEFRA";

				var borrowedFrom = Factory.LoadTop1<OrgHeader>(new ZQuery());
				mawb.JM_OA_From = borrowedFrom.MainAddress.PK;
				AssertEquals("Borrowed MAWB - AgentApprovedExporterNumber is shown", "RA99999", consol.AWBHeader.EH_AgentApprovedExporterNumber);
			}
		}

		public void TestEH_AgentApprovedExporterNumber_HongKong()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var branchOrg = Factory.NewWithValidTestData<OrgHeader>();
				var branchAddress = branchOrg.Addresses.AddNew();

				var storedPk = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrg.PK;
				branchAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
				branchAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

				var companyCountryData = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				companyCountryData.OV_OH_OrgHeader = GlbCompany.CurrentCompany.OrgProxy.PK;
				companyCountryData.OV_EXApprovedOrMajorExporter = "RA";
				companyCountryData.OV_EXApprovalNumber = "RA12345";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_AgentType = "AGT";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_IsNeutralMaster = true;

				var handlingItem = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
				handlingItem.EP_SpecialHandling = "NSC";
				AssertEquals("AgentApprovedExporterNumber is shown for NSC", "RA12345", consol.AWBHeader.EH_AgentApprovedExporterNumber);

				handlingItem.EP_SpecialHandling = "SCO";
				AssertEquals("AgentApprovedExporterNumber is shown for SCO", "RA12345", consol.AWBHeader.EH_AgentApprovedExporterNumber);

				handlingItem.EP_SpecialHandling = "SPX";
				AssertEquals("AgentApprovedExporterNumber is shown for SPX", "RA12345", consol.AWBHeader.EH_AgentApprovedExporterNumber);

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = storedPk;
				companyCountryData.Delete();
			}
		}

		#endregion

		public void TestIsImportCountryChina()
		{
			AWBHeader.Consol.JK_RL_NKDischargePort = "AUSYD";
			Assert(!AWBHeader.IsImportToChina);

			AWBHeader.Consol.JK_RL_NKDischargePort = "CNSHA";
			Assert(AWBHeader.IsImportToChina);
		}

		public void TestIsImportExportCountryCanada()
		{
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			Assert(!AWBHeader.IsImportToCanada);
			Assert(!AWBHeader.IsExportFromCanada);

			AWBHeader.Consol.JK_RL_NKDischargePort = "CA2KS";
			Assert(AWBHeader.IsImportToCanada);
			Assert(!AWBHeader.IsExportFromCanada);

			AWBHeader.Consol.JK_RL_NKLoadPort = "CA2KS";
			Assert(!AWBHeader.IsImportToCanada);
			Assert(!AWBHeader.IsExportFromCanada);

			AWBHeader.Consol.JK_RL_NKDischargePort = "AUSYD";
			Assert(!AWBHeader.IsImportToCanada);
			Assert(AWBHeader.IsExportFromCanada);
		}

		public void TestFreightForwarderOrCarrierCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Canada))
			{
				AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
				AWBHeader.Consol.JK_RL_NKDischargePort = "CA2KS";
				Assert(AWBHeader.IsImportToCanada);

				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				AWBHeader.Consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				var cusCode1 = carrier.CustomsCodes.AddNew();
				cusCode1.OK_RN_NKCodeCountry = CountryCodes.Canada;
				cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode1.OK_CustomsRegNo = "8000";
				AssertEquals(ZString.Empty, AWBHeader.FreightForwarderOrCarrierCode);

				var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				var cusCode2 = receivingForwarder.CustomsCodes.AddNew();
				cusCode2.OK_RN_NKCodeCountry = CountryCodes.Canada;
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				cusCode2.OK_CustomsRegNo = "8100";
				AssertEquals(ZString.Empty, AWBHeader.FreightForwarderOrCarrierCode);
			}
		}

		public void TestIsTransitingThroughChina()
		{
			AWBHeader.Consol.JK_RL_NKLoadPort = "CNSHA";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "CNSHA";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			Assert(!AWBHeader.IsTransitingThroughChina);

			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			Assert(!AWBHeader.IsTransitingThroughChina);

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "CNSHA";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			Assert(AWBHeader.IsTransitingThroughChina);

			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.Transports[0].JW_RL_NKDiscPort = "CNSHA";
			Assert(AWBHeader.IsTransitingThroughChina);
		}

		#region Harmonized Code

		public void TestHarmonizedCode()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1000", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var packLines = AWBHeader.Consol.Shipments[0].OuterPackLines;
			var packLine = packLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_HarmonisedCode = "1000";
			AWBHeader.Populate();

			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 3;

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 4;
			packLine.JL_HarmonisedCode = "2000";
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 2000", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 5;
			packLine.JL_HarmonisedCode = "2000";

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 6;
			packLine.JL_HarmonisedCode = "3000";
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("Only one HS code will be shown for multiple duplicated HS codes", "HS Code: 2000", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 3000", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
		}

		public void TestHarmonizedCode_DoNotFillAvailableRateLinesWhenNoTariffFound()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var packLines = AWBHeader.Consol.Shipments[0].OuterPackLines;
			for (var i = 1; i < 30; i++)
			{
				var packLine = packLines.AddNew();
				packLine.JL_PackageCount = i;
				packLine.JL_HarmonisedCode = i.ToString();
			}

			AWBHeader.Populate();

			AssertEquals("435 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			for (var i = 1; i < 10; i++)
			{
				AssertEquals("HS Code: " + i.ToString(), AWBHeader.AWBRateLines[i].NatureAndQtyOfGoodsDescription);
				AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLines[i].NatureAndQtyOfGoodsType);
			}
		}

		public void TestHarmonizedCode_CountrySpecificHSCode()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1000", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var packLines = AWBHeader.Consol.Shipments[0].OuterPackLines;
			var packLine = packLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_HarmonisedCode = "1000";
			AWBHeader.Populate();

			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 3;
			packLine.JL_HarmonisedCode = "2000";
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 2000", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);

			var hc1 = packLine.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "JM";
			hc1.JLH_Code = "3333";
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 3333", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 4;
			packLine.JL_HarmonisedCode = "4000";
			var hc2 = packLine.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "SG";
			hc2.JLH_Code = "4444";
			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 3333", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 4000", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);

			hc1.JLH_Code = "100000";
			AWBHeader.Populate();
			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("Only one HS code will be shown for multiple duplicated HS codes", "HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 4000", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
		}

		public void TestHarmonizedCode_CountrySpecificHSCode_MultipleCountries()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var packLines = AWBHeader.Consol.Shipments[0].OuterPackLines;

			var packLine1 = packLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_HarmonisedCode = "100000";

			var hc1 = packLine1.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "HK";
			hc1.JLH_Code = "200000";

			AWBHeader.Populate();

			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);

			var packLine2 = packLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_HarmonisedCode = "200000";

			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 200000", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);

			var hc2 = packLine2.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "JM";
			hc2.JLH_Code = "333333";

			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 333333", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);

			var hc3 = packLine2.HarmonisedCodes.AddNew();
			hc3.JLH_RN_NKCountry = "JM";
			hc3.JLH_Code = "444444";

			var hc4 = packLine2.HarmonisedCodes.AddNew();
			hc4.JLH_RN_NKCountry = "SG";
			hc4.JLH_Code = "555555";

			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 333333", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 444444", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
		}

		public void TestHarmonizedCode_CountrySpecificHSCode_MultipleCountries_Validation()
		{
			AssertEquals(false, AWBHeader.EH_AreRateLinesOverridden);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Consol.JK_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Consol.Shipments[0].JS_ActualVolume = 10M;
			AWBHeader.Consol.Shipments[0].JS_UnitOfVolume = "M3";
			AWBHeader.Consol.Shipments[0].JS_TotalPackageCount = 20;
			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();

			var packLines = AWBHeader.Consol.Shipments[0].OuterPackLines;

			var packLine1 = packLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_HarmonisedCode = "100000";

			var hc1 = packLine1.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "HK";
			hc1.JLH_Code = "200000";

			AWBHeader.Populate();

			AssertEquals("VOL 10.00 M3", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertNoWarnings(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo);

			var packLine2 = packLines.AddNew();
			packLine2.JL_PackageCount = 3;
			packLine2.JL_HarmonisedCode = "222222";

			for (int i = 0; i < 12; i++)
			{
				var hc2 = packLine2.HarmonisedCodes.AddNew();
				hc2.JLH_RN_NKCountry = "JM";
				hc2.JLH_Code = (900000 + i).ToString();
			}

			AWBHeader.Populate();

			AssertEquals("HS Code: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 900000", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 900001", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 900002", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 900003", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 900004", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 900005", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsType);
			AssertEquals("HS Code: 900006", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsType);

			AssertEquals("HS Code: 900007", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType);
			AssertNoWarnings(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo);

			var message = "More HS Codes exist but cannot be shown due to lack of space.";
			AssertEquals("HS Code: 900008", AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsType);
			AssertHasWarning(AWBHeader.AWBRateLine11.NatureAndQtyOfGoods.TextInfo, message);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount, AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsType);

			AWBHeader.EH_AreRateLinesOverridden = true;
			AssertNoWarnings("No warning when EH_AreRateLinesOverridden is true", AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo);
			AssertNoWarnings("No warning when EH_AreRateLinesOverridden is true", AWBHeader.AWBRateLine11.NatureAndQtyOfGoods.TextInfo);

			AWBHeader.EH_AreRateLinesOverridden = false;
			AssertNoWarnings("Only last HS Code line has the warning when EH_AreRateLinesOverridden is false", AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo);
			AssertHasWarning("Only last HS Code line has the warning when EH_AreRateLinesOverridden is false", AWBHeader.AWBRateLine11.NatureAndQtyOfGoods.TextInfo, message);
		}

		#endregion

		#region As Agreed

		public void TestAsAgreedWhenImportToBrazil()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.ALL;
			consol.JK_AgentType = AgentType.CoLoad;
			AssertEquals("Precondition: Expected consol field to be ALL", ChargesApplyHelper.ChargesApplyConstants.ALL, consol.JK_MBLAWBChargesDisplay);

			var awbHeader = consol.AWBHeader;
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			consol.JK_AgentType = AgentType.Direct;
			consol.JK_RL_NKDischargePort = "BR6MO";
			Assert(consol.IsImportTo(CountryCodes.Brazil));

			awbHeader.Populate();
			AssertEquals("Expected awb header to be set to none", Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be set to none", Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);

			consol.JK_AgentType = AgentType.CoLoad;
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			consol.JK_AgentType = AgentType.Agent;
			awbHeader.Populate();
			AssertEquals("Expected awb header to be set to none", Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be set to none", Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);
		}

		public void TestAsAgreedDefaultsFromRegistryWhenConsolUnchanged()
		{
			var oldFirst = Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB;
			var oldSecond = Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB;
			try
			{
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB = Constants.AWB.AsAgreedTypes.Codes.All;
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB = Constants.AWB.AsAgreedTypes.Codes.All;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_AgentType = AgentType.CoLoad;
				AssertEquals("Precondition: Expected consol field to be ALL", ChargesApplyHelper.ChargesApplyConstants.ALL, consol.JK_MBLAWBChargesDisplay);

				var awbHeader = consol.AWBHeader;
				awbHeader.Populate();
				AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
				AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB = oldFirst;
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB = oldSecond;
			}
		}

		public void TestAsAgreedSynchronisedFromConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.ALL;
			AssertEquals("Precondition: Expected consol field to be ALL", ChargesApplyHelper.ChargesApplyConstants.ALL, consol.JK_MBLAWBChargesDisplay);

			var awbHeader = consol.AWBHeader;
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.CPD;
			AssertEquals("Precondition: Expected consol field to be CPD", ChargesApplyHelper.ChargesApplyConstants.CPD, consol.JK_MBLAWBChargesDisplay);
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeader.EH_AsAgreed2nd);

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NON;
			AssertEquals("Precondition: Expected consol field to be NON", ChargesApplyHelper.ChargesApplyConstants.NON, consol.JK_MBLAWBChargesDisplay);
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);
		}

		public void TestAsAgreedNotSynchronisedFromConsolWhenOverriden()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.ALL;
			AssertEquals("Precondition: Expected consol field to be ALL", ChargesApplyHelper.ChargesApplyConstants.ALL, consol.JK_MBLAWBChargesDisplay);

			var awbHeader = consol.AWBHeader as ConsolExportAWBHeader;
			AssertNotNull(awbHeader);
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			consol.IsAWBValuesOverriddenProperty = true;
			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.CPD;
			AssertEquals("Precondition: Expected consol field to be CPD", ChargesApplyHelper.ChargesApplyConstants.CPD, consol.JK_MBLAWBChargesDisplay);
			AssertEquals("Expected awb header to not be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to not be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NON;
			AssertEquals("Precondition: Expected consol field to be NON", ChargesApplyHelper.ChargesApplyConstants.NON, consol.JK_MBLAWBChargesDisplay);
			AssertEquals("Expected awb header to not be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to not be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);
		}

		public void TestIssuingAgentEditableWhenOverriden()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Agent;

			var awbHeader = consol.AWBHeader as ConsolExportAWBHeader;
			AssertNotNull(awbHeader);

			awbHeader.Populate();

			CombineAssertions(() =>
			{
				Assert("EH_IssuingAgentName", awbHeader.EH_IssuingAgentNameInfo.ReadOnly);
				Assert("EH_IssuingAgentAddress1", awbHeader.EH_IssuingAgentAddress1Info.ReadOnly);
				Assert("EH_IssuingAgentAddress2", awbHeader.EH_IssuingAgentAddress2Info.ReadOnly);
			});

			consol.IsAWBValuesOverriddenProperty = true;

			CombineAssertions(() =>
			{
				Assert("EH_IssuingAgentName", !awbHeader.EH_IssuingAgentNameInfo.ReadOnly);
				Assert("EH_IssuingAgentAddress1", !awbHeader.EH_IssuingAgentAddress1Info.ReadOnly);
				Assert("EH_IssuingAgentAddress2", !awbHeader.EH_IssuingAgentAddress2Info.ReadOnly);
			});
		}

		#endregion

		#region Override Airline IATA code

		public void TestOverrideIATACode()
		{
			SetUpForDeparturePort("AUSYD");

			var consol = AWBHeader.Consol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPorts1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUSYD";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);

			consol.JK_AgentType = Constants.AgentType.Agent;

			using (var consolJob = new JobHeader.Loader(consol).TryLoadOrCreate())
			{
				using (var shipmentJob = new JobHeader.Loader(consol.Shipments[0]).TryLoadOrCreate())
				{
					var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					chargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
					chargeCode.AC_Code = "AAA";

					var consolCharge = Factory.NewWithValidTestData<JobCharge>();
					consolCharge.JR_AC = chargeCode.PK;
					consolCharge.JR_JH = consolJob.PK;
					consolCharge.JR_RX_NKSellCurrency = "AUD";
					consolCharge.JR_RX_NKCostCurrency = "AUD";
					consolCharge.JR_LocalCostAmt = 20;
					consolCharge.JR_OSCostAmt = 20;
					consolCharge.JR_LocalSellAmt = 25;
					consolCharge.JR_OSSellAmt = 25;
					consolCharge.JR_JH_InternalJob = consolJob.PK;

					var shipmentCharge = Factory.NewWithValidTestData<JobCharge>();
					shipmentCharge.JR_AC = chargeCode.PK;
					shipmentCharge.JR_JH = shipmentJob.PK;
					shipmentCharge.JR_RX_NKSellCurrency = "AUD";
					shipmentCharge.JR_LocalCostAmt = 10;
					shipmentCharge.JR_OSCostAmt = 10;
					shipmentCharge.JR_LocalSellAmt = 15;
					shipmentCharge.JR_OSSellAmt = 15;
					shipmentCharge.JR_JH_InternalJob = shipmentJob.PK;
					Factory.Save();

					AWBHeader.AWBOtherCharges.RemoveAll();
					AssertNoExceptionThrown(() => AWBHeader.Populate());
					AssertEquals(Core.Constants.AWB.ChargeCodes.AC, AWBHeader.AWBOtherCharges[0].EO_ChargeCode);

					var company = Factory.New<OrgHeader>();
					var miscServ = company.MiscServ;
					company.OH_RL_NKClosestPort = "USLAX";
					company.OH_FullName = "TestCompany";
					company.MainAddress.OA_Address1 = "TestAddress";
					company.OH_IsShippingProvider = true;
					company.OH_IsAirLine = true;
					miscServ.OM_RM_Airline = Factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081").PK;

					consol.JK_OA_ShippingLineAddress = company.MainAddress.PK;
					var airoverride = chargeCode.AccChargeCodeCarrierIataMappings.AddNew();
					airoverride.ACI_OH_Carrier = company.PK;
					airoverride.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;
					AssertNoExceptionThrown(() => AWBHeader.Populate());
					AssertEquals(Core.Constants.AWB.ChargeCodes.DB, AWBHeader.AWBOtherCharges[0].EO_ChargeCode);
				}
			}
		}

		#endregion

		#region Lithium UNDGs Cease default in the N&QofGoods of AWB

		public void TestLithiumUNDGsCeaseDefault()
		{
			var substance3480 = Factory.New<UNDGSubstance>();
			substance3480.DG_Code = "3480";
			substance3480.DG_Standard = "IAT";
			substance3480.DG_PSN = "Lithium ion batteries";
			var substance3481 = Factory.New<UNDGSubstance>();
			substance3481.DG_Code = "3481";
			substance3481.DG_Standard = "IAT";
			substance3481.DG_PSN = "Lithium ion batteries";
			var substance3090 = Factory.New<UNDGSubstance>();
			substance3090.DG_Code = "3090";
			substance3090.DG_Standard = "IAT";
			substance3090.DG_PSN = "Lithium ion batteries";
			var substance3091 = Factory.New<UNDGSubstance>();
			substance3091.DG_Code = "3091";
			substance3091.DG_Standard = "IAT";
			substance3091.DG_PSN = "Lithium ion batteries";
			var substance3373 = Factory.New<UNDGSubstance>();
			substance3373.DG_Code = "3373";
			substance3373.DG_Standard = "IAT";
			var dangerousGood3480 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood3480.DI_DG = substance3480.PK;
			var dangerousGood3481 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood3481.DI_DG = substance3481.PK;
			var dangerousGood3090 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood3090.DI_DG = substance3090.PK;
			var dangerousGood3091 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood3091.DI_DG = substance3091.PK;
			var dangerousGood3373 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood3373.DI_DG = substance3373.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "ULD";
			consol.Transports[0].JW_RL_NKLoadPort = "DEFRA";
			consol.Transports[0].JW_RL_NKDiscPort = "CNSHA";

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "PM-2H").PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_IsHighRisk = true;
			shipment1.JS_InspectionTypeCode = "PHS";
			shipment1.JS_AdditionalInspectionTypeCode = "UNK";

			shipment1.OuterPackLines.RemoveAll();
			var pack1 = shipment1.OuterPackLines.AddNew();
			container1.PackLines.Add(pack1);
			pack1.JL_PackageCount = 3;
			pack1.JL_Width = 11;
			pack1.JL_Height = 12;
			pack1.JL_Length = 13;
			pack1.UNDGs.Add(dangerousGood3480);
			consol.AWBHeader.Populate();
			var header = consol.AWBHeader as ConsolExportAWBHeader;
			AssertEquals(true, header.AWBRateLines.Cast<ExportAWBRateLine>().All(line => line.ER_NatureAndQtyOfGoodsType != "G" || line.NatureAndQtyOfGoodsDescription.IsEmpty));

			pack1.UNDGs.RemoveAllFromRelationship();
			pack1.UNDGs.Add(dangerousGood3481);
			consol.AWBHeader.Populate();
			header = consol.AWBHeader as ConsolExportAWBHeader;
			AssertEquals(true, header.AWBRateLines.Cast<ExportAWBRateLine>().All(line => line.ER_NatureAndQtyOfGoodsType != "G" || line.NatureAndQtyOfGoodsDescription.IsEmpty));

			pack1.UNDGs.RemoveAllFromRelationship();
			pack1.UNDGs.Add(dangerousGood3090);
			consol.AWBHeader.Populate();
			header = consol.AWBHeader as ConsolExportAWBHeader;
			AssertEquals(true, header.AWBRateLines.Cast<ExportAWBRateLine>().All(line => line.ER_NatureAndQtyOfGoodsType != "G" || line.NatureAndQtyOfGoodsDescription.IsEmpty));

			pack1.UNDGs.RemoveAllFromRelationship();
			pack1.UNDGs.Add(dangerousGood3091);
			pack1.UNDGs.Add(dangerousGood3373);
			consol.AWBHeader.Populate();
			header = consol.AWBHeader as ConsolExportAWBHeader;
			AssertEquals(true, header.AWBRateLines.Cast<ExportAWBRateLine>().Any(line => line.ER_NatureAndQtyOfGoodsType == "G" && line.NatureAndQtyOfGoodsDescription.Contains("UN 3373")));
			AssertEquals(false, header.AWBRateLines.Cast<ExportAWBRateLine>().Any(line => line.ER_NatureAndQtyOfGoodsType == "G" && line.NatureAndQtyOfGoodsDescription.Contains("UN 3091")));
		}

		#endregion

		#region EH_SecurityStatusForNonBorrowedMAWBs

		public void TestEH_SecurityStatusForNonBorrowedMAWBs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var securityStatus = AWBHeader.AWBSpecialHandlingItems.AddNew();
				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertEquals("Precondition", AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft, AWBHeader.EH_SecurityStatus);
				AssertEquals(ZString.Empty, AWBHeader.EH_SecurityStatusForNonBorrowedMAWBs);
			}
		}

		#endregion

		#region TestHasValidScenarioToShowEOR

		public void TestHasValidScenarioToShowEOR()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.EH_ParentID = consol.PK;

			consol.JK_RL_NKDischargePort = "FRPAR";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "DEHAM";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "FRPAR";

			awbHeader.Populate();

			Assert(awbHeader.IsImportToICS2Zone);

			transport1.JW_RL_NKDiscPort = "HKHKG";

			awbHeader.Populate();

			Assert(!awbHeader.IsImportToICS2Zone);
		}

		public void TestHasNotValidScenarioToShowEOR()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.EH_ParentID = consol.PK;

			consol.JK_RL_NKDischargePort = "FRPAR";
			consol.JK_RL_NKLoadPort = "DEHAM";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "AUSYD";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "FRPAR";

			Assert(!awbHeader.IsImportToICS2Zone);
		}

		public void TestHasValidScenarioToShowEOR_TransferBySeaToIcs2Country()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			awbHeader.EH_ParentID = consol.PK;

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "USNYC";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "USNYC";
			transport2.JW_RL_NKDiscPort = "DEHAM";

			awbHeader.Populate();

			Assert(!awbHeader.IsImportToICS2Zone);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.Populate();

			Assert(awbHeader.IsImportToICS2Zone);
		}

		public void TestHasDestinationInIcs2Zone()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();

			var console = Factory.New<ForwardingConsol>();
			console.JK_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.EH_ParentID = console.PK;

			console.JK_RL_NKLoadPort = "AUSYD";
			console.JK_RL_NKDischargePort = "DEHAM";

			Assert(awbHeader.HasDestinationInIcs2Zone);

			SetupNorthernIrelandZone();
			console.JK_RL_NKDischargePort = "GBBEL";
			Assert(awbHeader.HasDestinationInIcs2Zone);

			console.JK_RL_NKDischargePort = "CHBSL";
			Assert(awbHeader.HasDestinationInIcs2Zone);

			console.JK_RL_NKDischargePort = "NOOSL";
			Assert(awbHeader.HasDestinationInIcs2Zone);

			console.JK_RL_NKDischargePort = "HKHKG";
			Assert(!awbHeader.HasDestinationInIcs2Zone);
		}

		#endregion

		public void TestSetParentIDWithoutValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

			OrgHeader sendingForwarder = Factory.New<OrgHeader>();

			sendingForwarder.OH_FullName = "Full Name";
			sendingForwarder.MainAddress.OA_Address1 = "Address 1 Some address";
			sendingForwarder.MainAddress.OA_Address2 = "Address 2 that is longer than 50 chars long";
			sendingForwarder.MainAddress.OA_City = "City Name Loooong";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.OA_PostCode = "Post Code";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var awbHeader = Factory.New<ConsolExportAWBHeader>();

			Factory.SuspendValidation();
			using (awbHeader.GetValidationSuspender())
			{
				awbHeader.EH_ParentID = consol.PK;
			}
			Factory.ResumeValidation();

			AssertEquals("ADDRESS 2 THAT IS LONGER THAN 50 CHARS LONG, CITY", awbHeader.EH_IssuingAgentAddress2);
		}

		#region Transform State For Japan

		public void TestTransformShipperStateForJapan()
		{
			var sendingForwarder = SetupJPOrg();

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				AWBHeader.Consol.JK_AgentType = AgentType.Agent;
				AWBHeader.Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
				AWBHeader.Populate();
				AssertEquals(sendingForwarder.MainAddress.PK, AWBHeader.EH_OA_ShipperAddress);
				AssertEquals("Tokyo", AWBHeader.EH_ShipperState);

				sendingForwarder.MainAddress.OA_State = "OSAKA-FU";
				AWBHeader.Populate();
				AssertEquals("OSAKA-FU", AWBHeader.EH_ShipperState);
			}
		}

		public void TestTransformConsigneeStateForJapan()
		{
			var receivingForwarder = SetupJPOrg();

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				AWBHeader.Consol.JK_AgentType = AgentType.Agent;
				AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				AWBHeader.Populate();
				AssertEquals(receivingForwarder.MainAddress.PK, AWBHeader.EH_OA_ConsigneeAddress);
				AssertEquals("Tokyo", AWBHeader.EH_ConsigneeState);

				receivingForwarder.MainAddress.OA_State = "OSAKA-FU";
				AWBHeader.Populate();
				AssertEquals("OSAKA-FU", AWBHeader.EH_ConsigneeState);
			}
		}

		public void TestTransformAlsoNotifyStateForJapan()
		{
			var notifyParty = SetupJPOrg();

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				AWBHeader.Consol.JK_AgentType = AgentType.Agent;
				AWBHeader.Consol.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;
				AWBHeader.Populate();
				AssertEquals("Tokyo", AWBHeader.EH_AlsoNotifyState);

				notifyParty.MainAddress.OA_State = "OSAKA-FU";
				AWBHeader.Populate();
				AssertEquals("OSAKA-FU", AWBHeader.EH_AlsoNotifyState);
			}
		}

		OrgHeader SetupJPOrg()
		{
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(
				new ZQuery(RefCountryStatesSchema.RW_Code, "13"), JoinCondition.And,
				new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, CountryCodes.Japan)));

			var translateJapanese = Factory.New<RefLanguageText>();
			translateJapanese.RLT_ColumnName = "RW_Description";
			translateJapanese.RLT_Language = SharedConstants.Languages.Japanese;
			translateJapanese.RLT_ParentId = state.PK;
			translateJapanese.RLT_ParentTableCode = "RW";
			translateJapanese.RLT_Text = "東京都";

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST 1";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_City = "Tokyo City";
			org.MainAddress.OA_State = "13";
			org.OH_RL_NKClosestPort = "JPTYO";

			return org;
		}

		#endregion

		#region DestinationShipperComment

		public void TestDestinationShipperComment()
		{
			CreateRefDocOrgCusCode(OrgCusCode.CodeTypes.CorporationCode, CountryCodes.Egypt, CountryCodes.Australia, 4, "ACN", "ACN Long Label", "AWB", "Registration ID");

			CreateRefDocOrgCusCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, CountryCodes.Egypt, CountryCodes.Australia, 5, "ABN", "ABN Long Label", "AWB", "Tax ID");
			Factory.Save();

			AssertShipperComment("EGALY", CountryCodes.Australia, "ACN", "Registration ID");
			AssertShipperComment("EGALY", CountryCodes.Australia, "ABN", "Tax ID");
			AssertShipperComment("EGALY", CountryCodes.Australia, ZString.Empty, ZString.Empty);

			void AssertShipperComment(ZString dischargePort, ZString shipperCountryCode, ZString shipperTraderNoType, ZString exptectedDestinationShipperComment)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
				{
					var consol1 = Factory.New<ForwardingConsol>();
					consol1.JK_AgentType = AgentType.Agent;
					consol1.JK_TransportMode = TransportModes.Air;
					consol1.JK_RL_NKLoadPort = "AUSYD";
					consol1.JK_RL_NKDischargePort = dischargePort;

					var header1 = Factory.New<ConsolExportAWBHeader>();
					header1.EH_ParentID = consol1.PK;
					header1.EH_Table = JobConsolSchema.Constants.TableName;
					header1.EH_ShipperCountryCode = shipperCountryCode;
					header1.EH_ShipperTraderNoType = shipperTraderNoType;
					header1.EH_ShipperTraderNo = "1111";

					AssertEquals(exptectedDestinationShipperComment, header1.DestinationShipperComment);
				}
			}
		}

		#endregion

		#region Transform State For China

		public void TestTransformShipperStateForChina()
		{
			var sendingForwarder = SetupCNOrg();

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals(sendingForwarder.MainAddress.PK, AWBHeader.EH_OA_ShipperAddress);
			AssertEquals("BEIJING", AWBHeader.EH_ShipperState);

			sendingForwarder.MainAddress.OA_State = "SHANG-ZN";
			AWBHeader.Populate();
			AssertEquals("SHANG-ZN", AWBHeader.EH_ShipperState);
		}

		public void TestTransformConsigneeStateForChina()
		{
			var receivingForwarder = SetupCNOrg();

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals(receivingForwarder.MainAddress.PK, AWBHeader.EH_OA_ConsigneeAddress);
			AssertEquals("BEIJING", AWBHeader.EH_ConsigneeState);

			receivingForwarder.MainAddress.OA_State = "SHANG-ZN";
			AWBHeader.Populate();
			AssertEquals("SHANG-ZN", AWBHeader.EH_ConsigneeState);
		}

		public void TestTransformAlsoNotifyStateForChina()
		{
			var notifyParty = SetupCNOrg();

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;
			AWBHeader.Populate();
			AssertEquals("BEIJING", AWBHeader.EH_AlsoNotifyState);

			notifyParty.MainAddress.OA_State = "SHANG-ZN";
			AWBHeader.Populate();
			AssertEquals("SHANG-ZN", AWBHeader.EH_AlsoNotifyState);
		}

		public void TestTransformStateForChinaShouldNotChangeDescriptionInRefCountryStates()
		{
			var sendingForwarder = SetupCNOrg();
			var state = Factory.New<RefCountryStates>();
			state.RW_Code = "110";
			state.RW_Description = "Beij";
			state.RW_RN_NKCountryCode = Constants.CountryCodes.China;
			sendingForwarder.MainAddress.OA_State = "110";
			Factory.Save();
			AssertEquals("Beij", state.RW_Description);

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AWBHeader.Populate();
			AssertEquals(sendingForwarder.MainAddress.PK, AWBHeader.EH_OA_ShipperAddress);
			AssertEquals("BEIJ", AWBHeader.EH_ShipperState);
			Factory.Save();

			AssertEquals("Beij", state.RW_Description);
		}

		OrgHeader SetupCNOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST 1";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_City = "Beijing City";
			org.MainAddress.OA_State = "11";
			org.OH_RL_NKClosestPort = "CNBJS";

			return org;
		}

		public void TestPopulateWithoutConsol()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			AssertNoExceptionThrown(() => awbHeader.Populate());
		}

		#endregion

		#region ULD Container Mode with ULD and loose cargo

		public void TestContainerModeULD_WithContainer1_LooseCargo1()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;

			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;

			var shipment1 = AWBHeader.Consol.Shipments[0];
			shipment1.JS_ActualWeight = 1000m;
			shipment1.JS_ActualChargeable = 1500m;

			var packedCargo = shipment1.OuterPackLines.AddNew();
			packedCargo.JL_PackageCount = 8;
			packedCargo.JL_F3_NKPackType = "CTN";
			packedCargo.JL_ActualWeight = 920m;
			packedCargo.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;
			var looseCargo1 = shipment1.OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 80m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			AWBHeader.Populate();

			AssertEquals("1", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(920m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(1091m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);

			AssertEquals("3", AWBHeader.AWBRateLine4.ER_NoOfPiecesOrRCP);
			AssertEquals(80m, AWBHeader.AWBRateLine4.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine4.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_CommodityItemNumber);
			AssertEquals(409.5m, AWBHeader.AWBRateLine4.ER_ChargeableWeight);

			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(3, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(4, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1000m, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestContainerModeULD_WithContainer1_LooseCargo1_Imperial()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;

			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;

			var shipment1 = AWBHeader.Consol.Shipments[0];
			shipment1.JS_ActualWeight = 1000m;
			shipment1.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment1.JS_ActualChargeable = 1500m;

			var packedCargo = shipment1.OuterPackLines.AddNew();
			packedCargo.JL_PackageCount = 8;
			packedCargo.JL_F3_NKPackType = "CTN";
			packedCargo.JL_ActualWeight = 920m;
			packedCargo.JL_ActualWeightUQ = Constants.Weight.Pounds;

			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;
			var looseCargo1 = shipment1.OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 80m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Pounds;

			AWBHeader.Populate();

			AssertEquals("1", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(920m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("L", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(1091m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);

			AssertEquals("3", AWBHeader.AWBRateLine4.ER_NoOfPiecesOrRCP);
			AssertEquals(80m, AWBHeader.AWBRateLine4.ER_GrossWeight);
			AssertEquals("L", AWBHeader.AWBRateLine4.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_CommodityItemNumber);
			AssertEquals(409.5m, AWBHeader.AWBRateLine4.ER_ChargeableWeight);

			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(3, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(4, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1000m, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestContainerModeULD_WithContainer1_LooseCargo2()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;

			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;

			var shipment1 = AWBHeader.Consol.Shipments[0];
			shipment1.JS_ActualWeight = 1050m;
			shipment1.JS_ActualChargeable = 1500m;

			var packedCargo = shipment1.OuterPackLines.AddNew();
			packedCargo.JL_PackageCount = 8;
			packedCargo.JL_F3_NKPackType = "CTN";
			packedCargo.JL_ActualWeight = 920m;
			packedCargo.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;
			var looseCargo1 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 80m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var looseCargo2 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo2.JL_PackageCount = 100;
			looseCargo2.JL_F3_NKPackType = "BOX";
			looseCargo2.JL_ActualWeight = 50m;
			looseCargo2.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			AWBHeader.Populate();

			AssertEquals("1", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(920m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(108.5m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);

			AssertEquals("103", AWBHeader.AWBRateLine4.ER_NoOfPiecesOrRCP);
			AssertEquals(130m, AWBHeader.AWBRateLine4.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine4.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_CommodityItemNumber);
			AssertEquals(1392m, AWBHeader.AWBRateLine4.ER_ChargeableWeight);

			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(103, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(104, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1050m, AWBHeader.EH_TotalGrossWeight);
		}

		BusinessObject CreateConsolCost(ZGuid chargeCodePK)
		{
			var result = (BusinessObject)Factory.New<IJobConsolCost>();
			result[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCodePK;
			result[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			result.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				result[JobConsolCostSchema.E6_ParentID] = AWBHeader.Consol.PK;
				result[JobConsolCostSchema.E6_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			}
			finally
			{
				result.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			return result;
		}

		CalculationLog CreateCalculationLog(ZString rateMode, ZString unit, ZDecimal weight, params (ZDecimal, ZDecimal)[] steps)
		{
			var log = new CalculationLog();
			log.RateMode = rateMode;
			log.Unit = unit;
			log.Weight = weight;
			log.IsCosting = true;
			log.CommodityCode = "GEN";

			foreach (var (unitCount, unitPrice) in steps)
			{
				var step = log.AddCalculationStep();
				step.UnitCount = unitCount;
				step.UnitPrice = unitPrice;
				step.Result = unitCount * unitPrice;
			}

			return log;
		}

		CalculationLogsWrapper CreateCalculationLogWrapper(CalculationLog calculationLog)
		{
			var logWrapper = new CalculationLogsWrapper();
			logWrapper.Logs.Add(calculationLog);
			return logWrapper;
		}

		public void TestContainerModeULD_WithContainer1_LooseCargo1_AutoRating()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;

			var containerTypeLd1 = new RefContainer.Loader(Factory).LoadFromCode("LD-1");
			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;
			container1.JC_RC = containerTypeLd1.PK;
			container1.JC_ContainerNum = "FRED123";
			container1.JC_TareWeight = 100m;

			var shipment1 = AWBHeader.Consol.Shipments[0];

			var packedCargo = shipment1.OuterPackLines.AddNew();
			packedCargo.JL_PackageCount = 8;
			packedCargo.JL_F3_NKPackType = "CTN";
			packedCargo.JL_ActualWeight = 920m;
			packedCargo.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;
			var looseCargo1 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 80m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var log1 = CreateCalculationLog(Constants.ContainerModes.ULD, "CN", 920m, (1m, 123m));
			log1.ContainerCode = "LD-1";

			var log2 = CreateCalculationLog(Constants.ContainerModes.Loose, Constants.Weight.Kilograms, 80m, (80m, 1.5m));
			log2.Chargeable = 80m;
			log2.ChargeableUnit = Constants.Weight.Kilograms;

			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log1));
			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log2));

			var refCommodityCode = Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "GEN")).FirstOrDefault();
			refCommodityCode.RH_IATACommodityItem = "9999";
			Factory.Save();

			AWBHeader.Populate();

			AssertEquals("1", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(920m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(0m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);
			AssertEquals(123m, AWBHeader.AWBRateLine1.ER_RateChargeOrDiscount);
			AssertEquals(123m, AWBHeader.AWBRateLine1.ER_Total);

			AssertEquals("S", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals(8, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("FRED123", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(100m, AWBHeader.AWBRateLine3.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine3.ER_WeightInLBsOrKGs);

			AssertEquals("3", AWBHeader.AWBRateLine4.ER_NoOfPiecesOrRCP);
			AssertEquals(80m, AWBHeader.AWBRateLine4.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine4.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_CommodityItemNumber);
			AssertEquals(80m, AWBHeader.AWBRateLine4.ER_ChargeableWeight);
			AssertEquals(1.5m, AWBHeader.AWBRateLine4.ER_RateChargeOrDiscount);
			AssertEquals(120m, AWBHeader.AWBRateLine4.ER_Total);

			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(3, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(4, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1100m, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestContainerModeULD_WithContainer1_LooseCargo1_Imperial_AutoRating()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;

			var containerTypeLd1 = new RefContainer.Loader(Factory).LoadFromCode("LD-1");
			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;
			container1.JC_RC = containerTypeLd1.PK;
			container1.JC_ContainerNum = "FRED123";
			container1.JC_TareWeight = 100m;

			var shipment1 = AWBHeader.Consol.Shipments[0];

			var packedCargo = shipment1.OuterPackLines.AddNew();
			packedCargo.JL_PackageCount = 8;
			packedCargo.JL_F3_NKPackType = "CTN";
			packedCargo.JL_ActualWeight = 1000m;
			packedCargo.JL_ActualWeightUQ = Constants.Weight.Pounds;

			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;
			var looseCargo1 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 100m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Pounds;

			var log1 = CreateCalculationLog(Constants.ContainerModes.ULD, "CN", 920m, (1m, 123m));
			log1.ContainerCode = "LD-1";

			var log2 = CreateCalculationLog(Constants.ContainerModes.Loose, Constants.Weight.Pounds, 100m, (80m, 1.5m));
			log2.Chargeable = 80m;
			log2.ChargeableUnit = Constants.Weight.Pounds;

			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log1));
			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log2));

			var refCommodityCode = Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "GEN")).FirstOrDefault();
			refCommodityCode.RH_IATACommodityItem = "9999";
			Factory.Save();

			AWBHeader.Populate();

			AssertEquals("1", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(1000m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("L", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(0m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);
			AssertEquals(123m, AWBHeader.AWBRateLine1.ER_RateChargeOrDiscount);
			AssertEquals(123m, AWBHeader.AWBRateLine1.ER_Total);

			AssertEquals("S", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals(8, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("FRED123", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(100m, AWBHeader.AWBRateLine3.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine3.ER_WeightInLBsOrKGs);

			AssertEquals("3", AWBHeader.AWBRateLine4.ER_NoOfPiecesOrRCP);
			AssertEquals(100m, AWBHeader.AWBRateLine4.ER_GrossWeight);
			AssertEquals("L", AWBHeader.AWBRateLine4.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine4.ER_CommodityItemNumber);
			AssertEquals(80m, AWBHeader.AWBRateLine4.ER_ChargeableWeight);
			AssertEquals(1.5m, AWBHeader.AWBRateLine4.ER_RateChargeOrDiscount);
			AssertEquals(120m, AWBHeader.AWBRateLine4.ER_Total);

			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(3, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(4, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1200m, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestContainerModeULD_WithContainer2_DifferentContainerType_LooseCargo2_AutoRating()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;
			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;

			var containerTypeLd1 = new RefContainer.Loader(Factory).LoadFromCode("LD-1");
			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;
			container1.JC_RC = containerTypeLd1.PK;
			container1.JC_ContainerNum = "FRED123";
			container1.JC_TareWeight = 100m;

			var shipment1 = AWBHeader.Consol.Shipments[0];

			var packedCargo1 = shipment1.OuterPackLines.AddNew();
			packedCargo1.JL_PackageCount = 8;
			packedCargo1.JL_F3_NKPackType = "CTN";
			packedCargo1.JL_ActualWeight = 920m;
			packedCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container1.PackLines.Add(packedCargo1);

			var containerTypeLd6 = new RefContainer.Loader(Factory).LoadFromCode("LD-6");
			var container2 = AWBHeader.Consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.ULD;
			container2.JC_RC = containerTypeLd6.PK;
			container2.JC_ContainerNum = "BARRYABC";
			container2.JC_TareWeight = 80m;

			var packedCargo2 = shipment1.OuterPackLines.AddNew();
			packedCargo2.JL_PackageCount = 5;
			packedCargo2.JL_F3_NKPackType = "BAG";
			packedCargo2.JL_ActualWeight = 120m;
			packedCargo2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container2.PackLines.Add(packedCargo2);

			var looseCargo1 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 80m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var looseCargo2 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo2.JL_PackageCount = 100;
			looseCargo2.JL_F3_NKPackType = "BOX";
			looseCargo2.JL_ActualWeight = 50m;
			looseCargo2.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var log1 = CreateCalculationLog(Constants.ContainerModes.ULD, "CN", 920m, (1, 345m));
			log1.ContainerCode = "LD-1";

			var log2 = CreateCalculationLog(Constants.ContainerModes.ULD, "CN", 120m, (1, 234m));
			log2.ContainerCode = "LD-6";

			var log3 = CreateCalculationLog(Constants.ContainerModes.Loose, Constants.Weight.Kilograms, 130m, (130m, 1.5m));
			log3.Chargeable = 130m;
			log3.ChargeableUnit = Constants.Weight.Kilograms;

			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log1));
			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log2));
			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log3));

			var refCommodityCode = Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "GEN")).FirstOrDefault();
			refCommodityCode.RH_IATACommodityItem = "9999";
			Factory.Save();

			AWBHeader.Populate();

			AssertEquals("1", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(920m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(0m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);
			AssertEquals(345m, AWBHeader.AWBRateLine1.ER_RateChargeOrDiscount);
			AssertEquals(345m, AWBHeader.AWBRateLine1.ER_Total);

			AssertEquals("S", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals(8, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("FRED123", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(100m, AWBHeader.AWBRateLine3.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine3.ER_WeightInLBsOrKGs);

			AssertEquals("1", AWBHeader.AWBRateLine4.ER_NoOfPiecesOrRCP);
			AssertEquals(120m, AWBHeader.AWBRateLine4.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine4.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine4.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine4.ER_CommodityItemNumber);
			AssertEquals(0m, AWBHeader.AWBRateLine4.ER_ChargeableWeight);
			AssertEquals(234m, AWBHeader.AWBRateLine4.ER_RateChargeOrDiscount);
			AssertEquals(234m, AWBHeader.AWBRateLine4.ER_Total);
			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(5, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsType);
			AssertEquals("BARRYABC", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals(80m, AWBHeader.AWBRateLine5.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine5.ER_WeightInLBsOrKGs);

			AssertEquals("103", AWBHeader.AWBRateLine6.ER_NoOfPiecesOrRCP);
			AssertEquals(130m, AWBHeader.AWBRateLine6.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine6.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine6.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine6.ER_CommodityItemNumber);
			AssertEquals(130m, AWBHeader.AWBRateLine6.ER_ChargeableWeight);
			AssertEquals(1.5m, AWBHeader.AWBRateLine6.ER_RateChargeOrDiscount);
			AssertEquals(195m, AWBHeader.AWBRateLine6.ER_Total);
			AssertEquals("S", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsType);
			AssertEquals(103, AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(105, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1350m, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestContainerModeULD_WithContainer2_SameContainerType_LooseCargo1_AutoRating()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;
			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;

			var containerTypeLd1 = new RefContainer.Loader(Factory).LoadFromCode("LD-1");
			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;
			container1.JC_RC = containerTypeLd1.PK;
			container1.JC_ContainerNum = "FRED123";
			container1.JC_TareWeight = 100m;

			var shipment1 = AWBHeader.Consol.Shipments[0];

			var packedCargo1 = shipment1.OuterPackLines.AddNew();
			packedCargo1.JL_PackageCount = 8;
			packedCargo1.JL_F3_NKPackType = "CTN";
			packedCargo1.JL_ActualWeight = 920m;
			packedCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container1.PackLines.Add(packedCargo1);

			var container2 = AWBHeader.Consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.ULD;
			container2.JC_RC = containerTypeLd1.PK;
			container2.JC_ContainerNum = "BARRYABC";
			container2.JC_TareWeight = 80m;

			var packedCargo2 = shipment1.OuterPackLines.AddNew();
			packedCargo2.JL_PackageCount = 5;
			packedCargo2.JL_F3_NKPackType = "BAG";
			packedCargo2.JL_ActualWeight = 120m;
			packedCargo2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container2.PackLines.Add(packedCargo2);

			var looseCargo1 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 80m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var log1 = CreateCalculationLog(Constants.ContainerModes.ULD, "CN", 1000m, (1m, 345m));
			log1.ContainerCode = "LD-1";

			var log2 = CreateCalculationLog(Constants.ContainerModes.Loose, Constants.Weight.Kilograms, 80m, (80m, 1.5m));
			log2.Chargeable = 80m;
			log2.ChargeableUnit = Constants.Weight.Kilograms;

			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log1));
			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log2));

			var refCommodityCode = Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "GEN")).FirstOrDefault();
			refCommodityCode.RH_IATACommodityItem = "9999";
			Factory.Save();

			AWBHeader.Populate();

			AssertEquals("2", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(1040m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(0m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);

			AssertEquals("S", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals(8, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("FRED123", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(180m, AWBHeader.AWBRateLine3.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine3.ER_WeightInLBsOrKGs);

			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(5, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsType);
			AssertEquals("BARRYABC", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals(0m, AWBHeader.AWBRateLine5.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine5.ER_WeightInLBsOrKGs);

			AssertEquals("3", AWBHeader.AWBRateLine6.ER_NoOfPiecesOrRCP);
			AssertEquals(80m, AWBHeader.AWBRateLine6.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine6.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine6.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine6.ER_CommodityItemNumber);
			AssertEquals(80m, AWBHeader.AWBRateLine6.ER_ChargeableWeight);
			AssertEquals(1.5m, AWBHeader.AWBRateLine6.ER_RateChargeOrDiscount);
			AssertEquals(120m, AWBHeader.AWBRateLine6.ER_Total);

			AssertEquals("S", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsType);
			AssertEquals(3, AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(5, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1300m, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestContainerModeULD_WithContainer3_SameContainerType_LooseCargo1_AutoRating()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;
			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;

			var containerTypeLd1 = new RefContainer.Loader(Factory).LoadFromCode("LD-1");
			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;
			container1.JC_RC = containerTypeLd1.PK;
			container1.JC_ContainerNum = "FRED123";
			container1.JC_TareWeight = 100m;

			var shipment1 = AWBHeader.Consol.Shipments[0];

			var packedCargo1 = shipment1.OuterPackLines.AddNew();
			packedCargo1.JL_PackageCount = 8;
			packedCargo1.JL_F3_NKPackType = "CTN";
			packedCargo1.JL_ActualWeight = 920m;
			packedCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container1.PackLines.Add(packedCargo1);

			var container2 = AWBHeader.Consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.ULD;
			container2.JC_RC = containerTypeLd1.PK;
			container2.JC_ContainerNum = "BARRYABC";
			container2.JC_TareWeight = 80m;

			var packedCargo2 = shipment1.OuterPackLines.AddNew();
			packedCargo2.JL_PackageCount = 5;
			packedCargo2.JL_F3_NKPackType = "BAG";
			packedCargo2.JL_ActualWeight = 120m;
			packedCargo2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container2.PackLines.Add(packedCargo2);

			var container3 = AWBHeader.Consol.Containers.AddNew();
			container3.JC_ContainerMode = ContainerModes.ULD;
			container3.JC_RC = containerTypeLd1.PK;
			container3.JC_ContainerNum = "WILMA123";
			container3.JC_TareWeight = 80m;

			var packedCargo3 = shipment1.OuterPackLines.AddNew();
			packedCargo3.JL_PackageCount = 5;
			packedCargo3.JL_F3_NKPackType = "BAG";
			packedCargo3.JL_ActualWeight = 120m;
			packedCargo3.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container3.PackLines.Add(packedCargo3);

			var looseCargo1 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 80m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var log1 = CreateCalculationLog(Constants.ContainerModes.ULD, "CN", 1120m, (1m, 345m));
			log1.ContainerCode = "LD-1";

			var log2 = CreateCalculationLog(Constants.ContainerModes.Loose, Constants.Weight.Kilograms, 80m, (80m, 1.5m));
			log2.Chargeable = 80m;
			log2.ChargeableUnit = Constants.Weight.Kilograms;

			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log1));
			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log2));

			var refCommodityCode = Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "GEN")).FirstOrDefault();
			refCommodityCode.RH_IATACommodityItem = "9999";
			Factory.Save();

			AWBHeader.Populate();

			AssertEquals("3", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(1160m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(0m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);

			AssertEquals("S", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals(8, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("FRED123", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(260m, AWBHeader.AWBRateLine3.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine3.ER_WeightInLBsOrKGs);

			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(5, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsType);
			AssertEquals("BARRYABC", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals(0m, AWBHeader.AWBRateLine5.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine5.ER_WeightInLBsOrKGs);

			AssertEquals("S", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsType);
			AssertEquals(5, AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsType);
			AssertEquals("WILMA123", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals(0m, AWBHeader.AWBRateLine7.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine7.ER_WeightInLBsOrKGs);

			AssertEquals("3", AWBHeader.AWBRateLine8.ER_NoOfPiecesOrRCP);
			AssertEquals(80m, AWBHeader.AWBRateLine8.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine8.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine8.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine8.ER_CommodityItemNumber);
			AssertEquals(80m, AWBHeader.AWBRateLine8.ER_ChargeableWeight);
			AssertEquals(1.5m, AWBHeader.AWBRateLine8.ER_RateChargeOrDiscount);
			AssertEquals(120m, AWBHeader.AWBRateLine8.ER_Total);

			AssertEquals("S", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsType);
			AssertEquals(3, AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(6, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1500m, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestContainerModeULD_WithContainer3_MixedContainerType_LooseCargo1_AutoRating()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;
			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;

			var containerTypeLd1 = new RefContainer.Loader(Factory).LoadFromCode("LD-1");
			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;
			container1.JC_RC = containerTypeLd1.PK;
			container1.JC_ContainerNum = "FRED123";
			container1.JC_TareWeight = 100m;

			var shipment1 = AWBHeader.Consol.Shipments[0];

			var packedCargo1 = shipment1.OuterPackLines.AddNew();
			packedCargo1.JL_PackageCount = 8;
			packedCargo1.JL_F3_NKPackType = "CTN";
			packedCargo1.JL_ActualWeight = 920m;
			packedCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container1.PackLines.Add(packedCargo1);

			var container2 = AWBHeader.Consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.ULD;
			container2.JC_RC = containerTypeLd1.PK;
			container2.JC_ContainerNum = "BARRYABC";
			container2.JC_TareWeight = 80m;

			var packedCargo2 = shipment1.OuterPackLines.AddNew();
			packedCargo2.JL_PackageCount = 5;
			packedCargo2.JL_F3_NKPackType = "BAG";
			packedCargo2.JL_ActualWeight = 120m;
			packedCargo2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container2.PackLines.Add(packedCargo2);

			var containerTypeLd6 = new RefContainer.Loader(Factory).LoadFromCode("LD-6");
			var container3 = AWBHeader.Consol.Containers.AddNew();
			container3.JC_ContainerMode = ContainerModes.ULD;
			container3.JC_RC = containerTypeLd6.PK;
			container3.JC_ContainerNum = "WILMA123";
			container3.JC_TareWeight = 80m;

			var packedCargo3 = shipment1.OuterPackLines.AddNew();
			packedCargo3.JL_PackageCount = 5;
			packedCargo3.JL_F3_NKPackType = "BAG";
			packedCargo3.JL_ActualWeight = 120m;
			packedCargo3.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			container3.PackLines.Add(packedCargo3);

			var looseCargo1 = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();
			looseCargo1.JL_PackageCount = 3;
			looseCargo1.JL_F3_NKPackType = "PLT";
			looseCargo1.JL_ActualWeight = 80m;
			looseCargo1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var log1 = CreateCalculationLog(Constants.ContainerModes.ULD, "CN", 1000m, (1m, 345m));
			log1.ContainerCode = "LD-1";

			var log2 = CreateCalculationLog(Constants.ContainerModes.ULD, "CN", 120m, (1m, 345m));
			log2.ContainerCode = "LD-6";

			var log3 = CreateCalculationLog(Constants.ContainerModes.Loose, Constants.Weight.Kilograms, 80m, (80m, 1.5m));
			log3.Chargeable = 80m;
			log3.ChargeableUnit = Constants.Weight.Kilograms;

			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log1));
			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log2));
			CalculationLogsLoader.Save(CreateConsolCost(Env.Registry.FreightChargeCode), CreateCalculationLogWrapper(log3));

			var refCommodityCode = Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, "GEN")).FirstOrDefault();
			refCommodityCode.RH_IATACommodityItem = "9999";
			Factory.Save();

			AWBHeader.Populate();

			AssertEquals("2", AWBHeader.AWBRateLine1.ER_NoOfPiecesOrRCP);
			AssertEquals(1040m, AWBHeader.AWBRateLine1.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine1.ER_WeightInLBsOrKGs);
			AssertEquals("U", AWBHeader.AWBRateLine1.ER_RateClass);
			AssertEquals("9999", AWBHeader.AWBRateLine1.ER_CommodityItemNumber);
			AssertEquals(0m, AWBHeader.AWBRateLine1.ER_ChargeableWeight);

			AssertEquals("S", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals(8, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals("FRED123", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(180m, AWBHeader.AWBRateLine3.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine3.ER_WeightInLBsOrKGs);

			AssertEquals("S", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(5, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsType);
			AssertEquals("BARRYABC", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals(0m, AWBHeader.AWBRateLine5.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine5.ER_WeightInLBsOrKGs);

			AssertEquals("1", AWBHeader.AWBRateLine6.ER_NoOfPiecesOrRCP);
			AssertEquals(120m, AWBHeader.AWBRateLine6.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine6.ER_WeightInLBsOrKGs);
			AssertEquals("S", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsType);
			AssertEquals(5, AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals("U", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsType);
			AssertEquals("WILMA123", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals(80m, AWBHeader.AWBRateLine7.ER_GrossWeight);
			AssertEquals("", AWBHeader.AWBRateLine7.ER_WeightInLBsOrKGs);

			AssertEquals("3", AWBHeader.AWBRateLine8.ER_NoOfPiecesOrRCP);
			AssertEquals(80m, AWBHeader.AWBRateLine8.ER_GrossWeight);
			AssertEquals("K", AWBHeader.AWBRateLine8.ER_WeightInLBsOrKGs);
			AssertEquals("", AWBHeader.AWBRateLine8.ER_RateClass);
			AssertEquals("", AWBHeader.AWBRateLine8.ER_CommodityItemNumber);
			AssertEquals(80m, AWBHeader.AWBRateLine8.ER_ChargeableWeight);
			AssertEquals(1.5m, AWBHeader.AWBRateLine8.ER_RateChargeOrDiscount);
			AssertEquals(120m, AWBHeader.AWBRateLine8.ER_Total);

			AssertEquals("S", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsType);
			AssertEquals(3, AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(6, AWBHeader.EH_TotalNoOfPieces);
			AssertEquals(1500m, AWBHeader.EH_TotalGrossWeight);
		}

		public void TestMixedULDAndLSEButAllRateLinesAreUsed()
		{
			AWBHeader.Consol.JK_ConsolMode = ContainerModes.ULD;

			var container1 = AWBHeader.Consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.ULD;

			var container2 = AWBHeader.Consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.ULD;

			var container3 = AWBHeader.Consol.Containers.AddNew();
			container3.JC_ContainerMode = ContainerModes.ULD;

			var container4 = AWBHeader.Consol.Containers.AddNew();
			container4.JC_ContainerMode = ContainerModes.ULD;

			var container5 = AWBHeader.Consol.Containers.AddNew();
			container5.JC_ContainerMode = ContainerModes.ULD;

			var shipment1 = AWBHeader.Consol.Shipments[0];
			AWBHeader.Consol.AutomaticallyUpdatePackLineContainers = false;

			var undgExcludedSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgExcludedSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgExcludedSubstance.DG_UNNO = ShippersDeclarationUNDGExclusions.UNNOCodes.ExclusionList.First();
			var packLineWithDG = CreatePackLine(shipment1, container1);
			var undgDataItem = packLineWithDG.UNDGs.AddNew();
			undgDataItem.DI_DG = undgExcludedSubstance.PK;
			CreatePackLine(shipment1, container2);
			CreatePackLine(shipment1, container3);
			CreatePackLine(shipment1, container4);
			CreatePackLine(shipment1, container5);
			CreatePackLine(shipment1);

			AssertNoExceptionThrown("No exception is thrown when populating AWB", () => AWBHeader.Populate());
			AssertEquals("All ratelines are used", 0, AWBHeader.LineNumberOfFirstEmptyRateLine);
			AssertEquals("All ratelines are used", 0, AWBHeader.LineNumberOfFirstEmptyNatureAndQtyOfGoods);

			ForwardingPackLine CreatePackLine(ForwardingShipment shipment, ForwardingContainer container = null)
			{
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;
				packLine.JL_F3_NKPackType = "CTN";
				packLine.JL_ActualWeight = 100m;
				packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
				packLine.SetContainer(container?.PK ?? ZGuid.Empty);
				return packLine;
			}
		}

		public void TestDGVariantNotShown() =>
			CheckDGVariant("UN9434");

		void CheckDGVariant(params ZString[] expected)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var dangerousGood = CreateDangerousGoodWithUndgSubstance("7", "9434", "A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, "BOX", 1);

				var dangerousShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				dangerousShipment.JS_UniqueConsignRef = "DangerousShipment1";
				dangerousShipment.OuterPackLines.AddNew().UNDGs.Add(dangerousGood);

				AWBHeader.Consol.Shipments.Add(dangerousShipment);

				AWBHeader.Populate();

				var array = AWBHeader.DGCodes.ToArray();
				Array.Sort(array);
				Array.Sort(expected);
				AssertArrayEqualsByElements("DG Codes", expected, array);
			}
		}

		#endregion

		#region Load port is in EU, Discharge port is not in EU

		public void TestShipperTraderNoWhenTypeIsEORIAndIsNotImport2ICSMember()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Consol.JK_RL_NKLoadPort = "PTLIS";
			AWBHeader.Consol.JK_RL_NKDischargePort = "AUSYD";
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.CoLoad;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Full Name";
			sendingForwarder.MainAddress.OA_Address1 = "Address 1";
			sendingForwarder.MainAddress.OA_Address2 = "Address 2";
			sendingForwarder.MainAddress.OA_City = "City";
			sendingForwarder.OH_RL_NKClosestPort = "PTLIS";
			sendingForwarder.MainAddress.OA_PostCode = "Post Code";
			var customCode = sendingForwarder.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Portugal;
			customCode.OK_CustomsRegNo = "XI123456789";
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var code = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			code.DOC_DocumentType = "AWB";
			code.DOC_Direction = "BTH";
			code.DOC_RN_NKCodeCountry = Constants.CountryCodes.Portugal;
			code.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Portugal;
			code.DOC_ShortLabel = "EORI NO.";
			code.DOC_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;

			Factory.Save();
			AWBHeader.Populate();

			AssertEquals(string.Empty, AWBHeader.EH_ShipperTraderNo);
			AssertEquals(string.Empty, AWBHeader.EH_ShipperTraderNoType);
		}

		#endregion

		ExportAWBAccountingInformation NewAccountingInformation(string informationID, string information)
		{
			var accountingInformation = Factory.NewWithValidTestData<ExportAWBAccountingInformation>();
			accountingInformation.EA_EH = AWBHeader.PK;
			accountingInformation.EA_InformationID = informationID;
			accountingInformation.EA_Information = information;

			return accountingInformation;
		}

		public void TestCalculateACASOverrideFields()
		{
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.OH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			var forwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			forwarderAddress.OA_OH = forwarder.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_SendingForwarderAddress = forwarderAddress.PK;

			AWBHeader = Factory.New<ConsolExportAWBHeaderForTest>();
			AWBHeader.EH_ParentID = consol.PK;
			AWBHeader.EH_ShipperContactEmail = "";
			AWBHeader.EH_ConsigneeContactEmail = "";

			var acasCountryHandler = AWBHeader.GetACASCountryHandler();
			acasCountryHandler.GetCustomerAccountHolderAndName(out var accountHolder, out var accountName);
			acasCountryHandler.GetCustomerAccountIssuerAndNumber(out var accountIssuer, out var accountNumber);
			var customerShippingFrequency = acasCountryHandler.GetCustomerAccountShippingFrequency();
			var verifiedKnownConsignor = acasCountryHandler.IsVerifiedKnownConsignor();
			acasCountryHandler.GetCustomerAccountEstablishmentDate(out var establishmentDate);
			acasCountryHandler.GetCustomerAccountBillingType(out var billingType);
			(_, ZString idType, ZString idIssuer, ZString idNumber) = acasCountryHandler.GetBiographicData();

			var changeVerifiedConsignor = !verifiedKnownConsignor;

			CombineAssertions("Assert Preconditions", () =>
			{
				Assert(!AWBHeader.EH_Calculated_ACASInfoOverridden);
				AssertEquals(AWBHeader.EH_ShipperContactEmail, AWBHeader.EH_Calculated_ACASShipperEmail);
				AssertEquals(AWBHeader.EH_ConsigneeContactEmail, AWBHeader.EH_Calculated_ACASConsigneeEmail);
				AssertEquals(accountName, AWBHeader.EH_Calculated_ACASCustomerAccountName);
				AssertEquals(accountIssuer, AWBHeader.EH_Calculated_ACASCustomerAccountIssuer);
				AssertEquals(accountNumber, AWBHeader.EH_Calculated_ACASCustomerAccountNumber);
				AssertEquals(accountHolder, AWBHeader.EH_Calculated_ACASCustomerAccountHolder);
				AssertEquals(customerShippingFrequency, AWBHeader.EH_Calculated_ACASCustomerAccountShippingFrequency);
				AssertEquals(verifiedKnownConsignor, AWBHeader.EH_Calculated_ACASVerifiedKnownConsignor);
				AssertEquals(establishmentDate, AWBHeader.EH_Calculated_ACASCustomerAccountEstablishmentDate.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture));
				AssertEquals(billingType, AWBHeader.EH_Calculated_ACASCustomerAccountBillingType);
				AssertEquals(idType, AWBHeader.EH_Calculated_ACASBiographicDataType);
				AssertEquals(idIssuer, AWBHeader.EH_Calculated_ACASBiographicDataCountry);
				AssertEquals(idNumber, AWBHeader.EH_Calculated_ACASBiographicDataNumber);
			});

			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.SPE, "shipper@abc.com.au"));
			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.ANM, "Customer Name"));
			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.ASF, "X"));
			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.VKC, changeVerifiedConsignor.ToString()));
			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.AED, ZDate.Today.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture)));

			CombineAssertions("ACAS Calculated Properties should have been changed", () =>
			{
				Assert(AWBHeader.EH_Calculated_ACASInfoOverridden);
				AssertEquals("shipper@abc.com.au", AWBHeader.EH_Calculated_ACASShipperEmail);
				AssertEquals("Customer Name", AWBHeader.EH_Calculated_ACASCustomerAccountName);
				AssertEquals("X", AWBHeader.EH_Calculated_ACASCustomerAccountShippingFrequency);
				AssertEquals(changeVerifiedConsignor, AWBHeader.EH_Calculated_ACASVerifiedKnownConsignor);
				AssertEquals(ZDate.Today, AWBHeader.EH_Calculated_ACASCustomerAccountEstablishmentDate);
			});
		}

		public void TestNatureAndQtyOfGoods_SplitProperShippingName()
		{
			var subs1 = Factory.New<UNDGSubstance>();
			subs1.DG_UNNO = "1835";
			subs1.DG_Variant = ZString.Empty;
			subs1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs1.DG_PSN = "TETRAMETHYLAMMONIUM HYDROXIDE1 TETRAMETHYLAMMONIUM HYDROXIDE2 TETRAMETHYLAMMONIUM HYDROXIDE3 TETRAMETHYLAMMONIUM HYDROXIDE4 TETRAMETHYLAMMONIUM HYDROXIDE5";

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "1836";
			subs2.DG_Variant = ZString.Empty;
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs2.DG_PSN = "TETRAMETHYLAMMONIUM HYDROXIDE6 TETRAMETHYLAMMONIUM HYDROXIDE7 TETRAMETHYLAMMONIUM HYDROXIDE8 TETRAMETHYLAMMONIUM HYDROXIDE9 TETRAMETHYLAMMONIUM HYDROXIDE10";

			AWBHeader.Consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			var packLine = AWBHeader.Consol.Shipments[0].OuterPackLines.AddNew();

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_DG = subs1.PK;
			undg1.DI_PackageCount = 5;
			undg1.DI_DGWeight = 1.234;
			undg1.DI_UnitOfWeight = Weight.Kilograms;

			var undg2 = packLine.UNDGs.AddNew();
			undg2.DI_DG = subs2.PK;
			undg2.DI_PackageCount = 6;
			undg2.DI_DGWeight = 1.234;
			undg2.DI_UnitOfWeight = Weight.Kilograms;

			AWBHeader.Populate();
			AssertEquals("UN 1835 (5 PKG)", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsText.Text);
			AssertEquals("TETRAMETHYLAMMONIUM HYDROXIDE1", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsText.Text);
			AssertEquals("TETRAMETHYLAMMONIUM HYDROXIDE2", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsText.Text);
			AssertEquals("TETRAMETHYLAMMONIUM HYDROXIDE3", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsText.Text);
			AssertEquals("TETRAMETHYLAMMONIUM HYDROXIDE4", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsText.Text);
			AssertEquals("TETRAMETHYLAMMONIUM HYDROXIDE5", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsText.Text);
			AssertNotEquals("UN 1836 (6 PKG)", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsText.Text);
			AssertNotEquals("TETRAMETHYLAMMONIUM HYDROXIDE6", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsText.Text);
			AssertNotEquals("TETRAMETHYLAMMONIUM HYDROXIDE7", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsText.Text);
			AssertNotEquals("TETRAMETHYLAMMONIUM HYDROXIDE8", AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsText.Text);
			AssertNotEquals("TETRAMETHYLAMMONIUM HYDROXIDE9", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsText.Text);
		}

		#region Test Consol Agent Signature For HongKong Export Borrowed MAWB

		public void TestConsolAgentSignature_ForHongKongExportBorrowedMAWB()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK") )
			{
				{
					var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));

					var knownShipperDetails1 = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
					knownShipperDetails1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
					knownShipperDetails1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					knownShipperDetails1.OV_OH_OrgHeader = accountConsignor.PK;

					var consol = Factory.NewWithValidTestData<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					var shipment = consol.Shipments.AddNew();
					shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "HKHKG";
					shipment.JS_RL_NKDestination = "DEHAM";
					shipment.JS_InspectionTypeCode = "APP";

					var transport = consol.Transports[0];
					transport.JW_RL_NKLoadPort = "HKHKG";
					transport.JW_RL_NKDiscPort = "DEHAM";
					transport.JW_TransportMode = Constants.TransportModes.Air;

					var mawb = Factory.NewWithValidTestData<JobMawb>();
					mawb.JM_Airline3DigitPrefix = "176";
					mawb.JM_MAWB = "10000001";
					mawb.JM_GB = GlbBranch.CurrentBranch.PK;
					mawb.JM_ServiceLevel = "STD";
					mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
					mawb.JM_ParentID = consol.PK;

					var borrowedFrom = Factory.LoadTop1<OrgHeader>(new ZQuery());
					borrowedFrom.OH_Code = "BRW";
					borrowedFrom.OH_FullName = "12345678901234567890123456789012345678901234567890";
					var truncatedName = "12345678901234567890123456789012345";

					var knownShipperDetails = borrowedFrom.MainAddress.KnownShipperDetails.AddNew();
					knownShipperDetails.OV_EXApprovedOrMajorExporter = "RA";
					knownShipperDetails.OV_EXApprovalNumber = "RA11111";
					knownShipperDetails.OV_OH_OrgHeader = borrowedFrom.PK;

					mawb.JM_OA_From = borrowedFrom.MainAddress.PK;

					Factory.Save();

					Assert("Precondition", !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights);
					AssertEquals("Security status should default to 'SCO' as there is a shipment where the relevent org isn't for passenger flight shipping",
						"SCO",
						consol.SecurityStatusCode);
					Assert("No JobConsolAWBSpecialHandling created when get from defaulting", !Factory.Load<JobConsolAWBSpecialHandling>(new ZQuery()).Any());

					consol.SecurityStatusCode = "SPX";
					var specialHandling = Factory.LoadTop1<SecurityJobConsolAWBSpecialHandling>(new ZQuery());
					AssertNotNull("JobConsolAWBSpecialHandling is created when override security status", specialHandling);
					AssertEquals("SecurityStatusCode is from dbo.JobConsolAWBSpecialHandling", "SPX", specialHandling.JKH_Code);

					consol.PopulateAWB();
					consol.AWBHeader.Populate();

					AssertEquals(
						": For HK login companies, when a Consol contains a borrowed MAWB, the AWB's Signature of Issuing Carrier or its Agent section should show the Lender's/Borrowed From Company name and its RA number when the consol is SPX.",
						truncatedName,
						consol.AWBHeader.EH_AWBAgentsSignature);
					AssertEquals(
						": For HK login companies, when a Consol contains a borrowed MAWB, the AWB's Signature of Issuing Carrier or its Agent section should show the Lender's/Borrowed From Company name and its RA number when the consol is SPX.",
						knownShipperDetails.OV_EXApprovalNumber,
						consol.AWBHeader.EH_AgentApprovedExporterNumber);

					consol.SecurityStatusCode = "NSC";
					consol.AWBHeader.Populate();
					AssertEquals(
						": For HK login companies, when a Consol contains a borrowed MAWB, the AWB's Signature of Issuing Carrier or its Agent section should only show the Lender's/Borrowed From Company name when the consol is NSC.",
						truncatedName,
						consol.AWBHeader.EH_AWBAgentsSignature);
					AssertEquals(
						": For HK login companies, when a Consol contains a borrowed MAWB, the AWB's Signature of Issuing Carrier or its Agent section should only show the Lender's/Borrowed From Company name when the consol is NSC.",
						"",
						consol.AWBHeader.EH_AgentApprovedExporterNumber);
				}
			}
		}

		#endregion

		public void TestEH_AgentApprovedExporterNumberIsEmptyWhenKnownShipperIsNull_SPX()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				{
					Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "AGENT SIGNATURE";
					var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));

					var knownShipperDetails1 = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
					knownShipperDetails1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
					knownShipperDetails1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					knownShipperDetails1.OV_OH_OrgHeader = accountConsignor.PK;

					var consol = Factory.NewWithValidTestData<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					var shipment = consol.Shipments.AddNew();
					shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "HKHKG";
					shipment.JS_RL_NKDestination = "DEHAM";
					shipment.JS_InspectionTypeCode = "APP";

					var transport = consol.Transports[0];
					transport.JW_RL_NKLoadPort = "HKHKG";
					transport.JW_RL_NKDiscPort = "DEHAM";
					transport.JW_TransportMode = Constants.TransportModes.Air;

					var mawb = Factory.NewWithValidTestData<JobMawb>();
					mawb.JM_Airline3DigitPrefix = "176";
					mawb.JM_MAWB = "10000001";
					mawb.JM_GB = GlbBranch.CurrentBranch.PK;
					mawb.JM_ServiceLevel = "STD";
					mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
					mawb.JM_ParentID = consol.PK;

					Factory.Save();

					Assert("Precondition", !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights);
					AssertEquals("Security status should default to 'SCO' as there is a shipment where the relevent org isn't for passenger flight shipping",
						"SCO",
						consol.SecurityStatusCode);
					Assert("No JobConsolAWBSpecialHandling created when get from defaulting", !Factory.Load<JobConsolAWBSpecialHandling>(new ZQuery()).Any());

					consol.SecurityStatusCode = "SPX";
					var specialHandling = Factory.LoadTop1<SecurityJobConsolAWBSpecialHandling>(new ZQuery());
					AssertNotNull("JobConsolAWBSpecialHandling is created when override security status", specialHandling);
					AssertEquals("SecurityStatusCode is from dbo.JobConsolAWBSpecialHandling", "SPX", specialHandling.JKH_Code);

					consol.PopulateAWB();
					consol.AWBHeader.Populate();

					AssertEquals(
						": For HK login companies, when a Consol contains a borrowed MAWB but it doesn't have a known shipper, the AWB's Signature of Issuing Carrier or its Agent section should only show the Lender's/Borrowed From Company name when the consol is SPX.",
						"AGENT SIGNATURE",
						consol.AWBHeader.EH_AWBAgentsSignature);
					AssertEquals(
						": For HK login companies, when a Consol contains a borrowed MAWB but it doesn't have a known shipper, the AWB's Signature of Issuing Carrier or its Agent section should only show the Lender's/Borrowed From Company name when the consol is SPX.",
						"",
						consol.AWBHeader.EH_AgentApprovedExporterNumber);
				}
			}
		}

		#region Advance Cargo Reporting Self-Filer

		public void TestPopulateEH_IsConsigneeDeclarantForAdvancedCargoReporting()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			awbHeader.EH_ParentID = consol.PK;
			awbHeader.Populate();
			Assert(!awbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			consol.JK_RL_NKDischargePort = "FRPAR";

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			var misc = receivingForwarder.MiscServ;

			awbHeader.Populate();
			Assert(!awbHeader.HasInboundToICS2Zone);

			misc.OM_FWAdvanceCargoReportingSelfFiler = true;
			Assert(consol.IsAdvanceCargoReportingSelfFiler);
			Assert("Should return false when HasInboundToICS2Zone is false", !awbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			transport.JW_RL_NKDiscPort = "DEHAM";
			awbHeader.Populate();
			Assert(awbHeader.HasInboundToICS2Zone);

			awbHeader.Populate();
			Assert("Should get value from consol.IsAdvanceCargoReportingSelfFiler when is not direct shipment and HasInboundToICS2Zone is true", awbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			misc.OM_FWAdvanceCargoReportingSelfFiler = false;
			Assert(!consol.IsAdvanceCargoReportingSelfFiler);
			awbHeader.Populate();
			Assert("Should get value from consol.IsAdvanceCargoReportingSelfFiler when is not direct shipment and HasInboundToICS2Zone is true", !awbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			awbHeader.Consol.JK_AgentType = AgentType.Direct;
			awbHeader.Consol.Shipments.AddNew();
			var shipment = awbHeader.Consol.DirectShipment;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			misc = consignee.MiscServ;

			misc.OM_IMAdvanceCargoReportingSelfFiler = true;
			Assert(shipment.IsAdvanceCargoReportingSelfFiler);
			awbHeader.Populate();
			Assert("Should get value from Directshipment.IsAdvanceCargoReportingSelfFiler when is direct shipment and HasInboundToICS2Zone is true", awbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			misc.OM_IMAdvanceCargoReportingSelfFiler = false;
			Assert(!shipment.IsAdvanceCargoReportingSelfFiler);
			awbHeader.Populate();
			Assert("Should get value from Directshipment.IsAdvanceCargoReportingSelfFiler when is direct shipment and HasInboundToICS2Zone is true", !awbHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);
		}

		#endregion

		#region Implementation

		void AssertNatureAndQtyOfGoods(ExportAWBHeader header, string expected)
		{
			AssertNotNull("header should not be null", header);

			var natureAndQtyOfGoods = header
				.AWBRateLines
				.Cast<ExportAWBRateLine>()
				.Where(line => line.NatureAndQtyOfGoodsType != Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription
					|| !line.NatureAndQtyOfGoodsDescription.IsEmpty)
				.Select((line, index) => string.Format("{0}|{1}|{2}", index + 1, line.NatureAndQtyOfGoodsType, line.NatureAndQtyOfGoodsDescription));

			AssertMultilineASCIIEquals("NatureAndQtyOfGoods",
				expected,
				string.Join("\r\n", natureAndQtyOfGoods));
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return AWBHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpForDeparturePort("JMKIN");
		}

		void SetUpForDeparturePort(ZString departurePort)
		{
			if (savedCountry.IsEmpty)
			{
				savedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}

			var departureCountry = departurePort.Left(2);

			GlbCompany.CurrentCompany.SetCountry(departureCountry);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = departurePort;

			AWBHeader = Factory.New<ConsolExportAWBHeaderForTest>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = departurePort;

			shipment.Consols.AddNew();
			shipment.Consols[0].JK_RL_NKLoadPort = departurePort;

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignor.PK;

			CountryStatementSettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			var countrySetting = CountryStatementSettingCollection.AddNew();
			countrySetting.CountryCode = departureCountry;
			ExportStatementSetting1 = countrySetting.Statements.AddNew();
			ExportStatementSetting1.Code = "NDR";
			ExportStatementSetting1.Statement = "STATEMENT FOR TESTING";
			ExportStatementSetting1.Visibility = "UDF";

			ExportStatementSetting2 = countrySetting.Statements.AddNew();
			ExportStatementSetting2.Code = "AES";
			ExportStatementSetting2.Statement = "STATEMENT1 FOR TESTING";
			ExportStatementSetting2.Visibility = "UDF";
			UpdateCountryStatementSettingsToRegistry();

			AWBHeader.EH_ParentID = shipment.Consols[0].PK;
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;

			AWBHeader.AWBOtherCharges.AddNew();
			AWBHeader.AWBOtherCharges.AddNew();

			AWBHeader.AWBAccountingInformations.AddNew();
			AWBHeader.AWBAccountingInformations.AddNew();

			AWBHeader.ResetSupplyChainSecurityConfigurationForTesting();
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.SetCountry(savedCountry);
		}

		ZString savedCountry;

		void UpdateCountryStatementSettingsToRegistry()
		{
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CountryStatementSettingCollection);
		}

		CountryExportStatementSettingCollection CountryStatementSettingCollection;
		ExportStatementSetting ExportStatementSetting1;
		ExportStatementSetting ExportStatementSetting2;
		ConsolExportAWBHeaderForTest AWBHeader;

		#endregion
	}
}
