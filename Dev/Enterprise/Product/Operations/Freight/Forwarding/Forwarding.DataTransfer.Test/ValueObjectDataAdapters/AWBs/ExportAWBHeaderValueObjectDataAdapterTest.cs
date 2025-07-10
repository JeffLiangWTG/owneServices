using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ExportAWBHeaderValueObjectDataAdapter))]
	sealed class ExportAWBHeaderValueObjectDataAdapterTest : ValueObjectDataAdapterTest<ExportAWBHeader, Xsd.AWBHeader>
	{
		public void TestPopulateConsolAWBInfoFromCorrectBranch()
		{
			var buffer = new NotificationBuffer();
			ValueObjectExportContext exportContext = new ValueObjectExportContext(buffer);
			var dataAdapter = new ExportAWBHeaderValueObjectDataAdapter(Shipment.DepartureConsol);

			Shipment.DepartureConsol.PopulateAWB();

			Xsd.AWBHeader awbXsd = dataAdapter.ExportToValueObject(Shipment.DepartureConsol.AWBHeader, exportContext);
			AssertEquals("Agent City is taken from Branch KTT based on the departure leg's load port", "PERTH", awbXsd.Agent.City);
			AssertEquals("Agent Carrier Name is taken from Branch KTT based on the departure leg's load port", "PER CARRIER NAME", awbXsd.Agent.Name);
			AssertEquals("Agent IATA is taken from Branch KTT based on the departure leg's load port", "12-3 4567", awbXsd.Agent.IATACode);
			AssertEquals("Agent Account no is taken from Branch KTT based on the departure leg's load port", "19929", awbXsd.Agent.AccountNo);

			Shipment.DepartureConsol.MostInterestingTransportForBinding[0].JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			Shipment.DepartureConsol.JK_RL_NKLoadPort = "AUMEL";
			awbXsd = dataAdapter.ExportToValueObject(Shipment.DepartureConsol.AWBHeader, exportContext);
			AssertEquals("Agent City is taken from Branch MEL based on the consol's origin", "MELBOURNE", awbXsd.Agent.City);
			AssertEquals("Agent Carrier is taken from Branch MEL based on the consol's origin", "MEL CARRIER NAME", awbXsd.Agent.Name);
			AssertEquals("Agent IATA is taken from Branch MEL based on the consol's origin", "76-8 5934", awbXsd.Agent.IATACode);
			AssertEquals("Agent Account No", "58963", awbXsd.Agent.AccountNo);

			Shipment.DepartureConsol.JK_RL_NKLoadPort = "HKHKG";
			awbXsd = dataAdapter.ExportToValueObject(Shipment.DepartureConsol.AWBHeader, exportContext);
			AssertEquals("Consol's load port is not linked to any branch, Agent City is taken from Current Login Branch", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity, awbXsd.Agent.City);
			AssertEquals("Consol's load port is not linked to any branch, Agent Carrier Name is taken from Current Login Branch", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, awbXsd.Agent.Name);
			AssertEquals("Consol's load port is not linked to any branch, Agent IATA is taken from Current Login Branch", "12-3 45", awbXsd.Agent.IATACode);
			AssertEquals("Consol's load port is not linked to any branch, Agent Account No is taken from Current Login Branch", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber, awbXsd.Agent.AccountNo);
		}

		public void TestPopulateShipmentAWBInfoFromCorrectBranch()
		{
			var buffer = new NotificationBuffer();
			ValueObjectExportContext exportContext = new ValueObjectExportContext(buffer);
			var dataAdapter = new ExportAWBHeaderValueObjectDataAdapter(Shipment);

			Shipment.PopulateAWB();
			var awbXsd = dataAdapter.ExportToValueObject(Shipment.AWBHeader, exportContext);
			AssertEquals("Agent City is taken from Branch based on the consol's load port", "PERTH", awbXsd.Agent.City);
			AssertEquals("Agent Carrier is taken from Branch MEL based on the consol's load port", "PER CARRIER NAME", awbXsd.Agent.Name);
			AssertEquals("Agent IATA is taken from Branch MEL based on the consol's load port", "12-3 4567", awbXsd.Agent.IATACode);
			AssertEquals("Agent Account no is taken from Branch MEL based on the consol's load port", "19929", awbXsd.Agent.AccountNo);

			Shipment.Consols.RemoveAll();

			awbXsd = dataAdapter.ExportToValueObject(Shipment.AWBHeader, exportContext);
			AssertEquals("Agent City is taken from Branch based on the shipment's origin port", "MELBOURNE", awbXsd.Agent.City);
			AssertEquals("Agent Carrier is taken from Branch MEL based on the shipment port", "MEL CARRIER NAME", awbXsd.Agent.Name);
			AssertEquals("Agent IATA is taken from Branch MEL based on the shipment origin", "76-8 5934", awbXsd.Agent.IATACode);
			AssertEquals("Agent Account no is taken from Branch MEL based on the shipment origin", "58963", awbXsd.Agent.AccountNo);

			Shipment.JS_RL_NKOrigin = "AUTUR";

			awbXsd = dataAdapter.ExportToValueObject(Shipment.AWBHeader, exportContext);
			AssertEquals("Agent City is taken from Branch KTT based on the registry default", "carrier agent city", awbXsd.Agent.City);
			AssertEquals("Agent Name is taken from Branch KTT based on the registry default", "carrier agent name", awbXsd.Agent.Name);
			AssertEquals("Agent IATA is taken from Branch KTT based on the registry default", "12-3 45", awbXsd.Agent.IATACode);
			AssertEquals("Agent Account No is taken from Branch KTT based on the registry default", "123456", awbXsd.Agent.AccountNo);
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string fullyPopulatedAWBWithEmptyFieldsFileName;
		string FullyPopulatedAWBWithEmptyFieldsFileName
		{
			get
			{
				if (string.IsNullOrEmpty(fullyPopulatedAWBWithEmptyFieldsFileName))
				{
					fullyPopulatedAWBWithEmptyFieldsFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ValueObjectDataAdapters.AWBs.TestFiles.EmptyAWB.xml");
				}
				return fullyPopulatedAWBWithEmptyFieldsFileName;
			}
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override ValueObjectDataAdapter<ExportAWBHeader, Xsd.AWBHeader> GetNewBizObjXmlDataAdapter()
		{
			return new ExportAWBHeaderValueObjectDataAdapter(Shipment);
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			List<BusinessObjectAndExpectedOutputFileName> list = new List<BusinessObjectAndExpectedOutputFileName>();

			ExportAWBHeader header = ConsolWithEmptyFields.AWBHeader;

			header.EH_OtherPPDCOL = "PPD";
			header.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			header.EH_WeightPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			header.EH_ValuationPPD = 23.22m;
			header.EH_WeightVPPDCOL = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;

			ExportAWBOtherCharges otherCharge = header.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "AW";
			otherCharge.EO_EntitlementCode = "A";
			otherCharge.EO_Amount = 120M;

			otherCharge = header.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "XY";
			otherCharge.EO_EntitlementCode = "P";
			otherCharge.EO_Amount = 130M;

			otherCharge = header.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "AB";
			otherCharge.EO_EntitlementCode = "P";
			otherCharge.EO_Amount = 140M;

			otherCharge = header.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "AB";
			otherCharge.EO_EntitlementCode = "C";
			otherCharge.EO_Amount = 140M;

			//rateline
			ExportAWBRateLine rateLine = header.AWBRateLines.AddNew();

			rateLine.ER_NoOfPiecesOrRCP = "109";
			rateLine.ER_RateClass = "Q";
			rateLine.ER_CommodityItemNumber = "BEER";
			rateLine.ER_ChargeableWeight = 2.5m;
			rateLine.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine.ER_RateChargeOrDiscount = 0.23m;
			rateLine.ER_GrossWeight = 2.5m;
			rateLine.ER_LineCount = 1;
			rateLine.ER_Total = 39.3m;

			var temporaryMISCAWBFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ValueObjectDataAdapters.AWBs.TestFiles.MISCAWB.xml");
			list.Add(new BusinessObjectAndExpectedOutputFileName(header, temporaryMISCAWBFileName, ValidationKind.None, "MISC AWB"));
			return list.ToArray();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(ConsolWithEmptyFields.AWBHeader, FullyPopulatedAWBWithEmptyFieldsFileName, ValidationKind.None, "Empty AWB");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(ConsolWithEmptyFields.AWBHeader, FullyPopulatedAWBWithEmptyFieldsFileName, ValidationKind.None, "Empty AWB");
		}

		ForwardingConsol ConsolWithEmptyFields
		{
			get
			{
				if (consolWithEmptyFields == null)
				{
					consolWithEmptyFields = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
					consolWithEmptyFields.JK_MasterBillNum = "MAWB";
					consolWithEmptyFields.JK_RL_NKDischargePort = "HKHKG";
					consolWithEmptyFields.JK_RL_NKLoadPort = "AUSYD";
					consolWithEmptyFields.JK_TransportMode = Core.Constants.TransportModes.Air;
					consolWithEmptyFields.JK_UniqueConsignRef = "C100101";
					consolWithEmptyFields.JK_OA_SendingForwarderAddress = ZGuid.Empty;
					consolWithEmptyFields.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

					Transport tran = consolWithEmptyFields.MostInterestingTransportForBinding[0];
					tran.JW_RL_NKLoadPort = "AUPER";
					tran.JW_ETD = new ZDateTime(2009, 03, 24);
					tran.JW_TransportMode = Core.Constants.TransportModes.Air;
					tran.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
					tran.JW_VoyageFlight = "CX123";
					tran.JW_DepotCutOff = new ZDateTime(2009, 03, 23);

					consolWithEmptyFields.AWBHeader.Populate();
					consolWithEmptyFields.JK_OverrideWaybillDefaults = true;
				}
				return consolWithEmptyFields;
			}
		}
		ForwardingConsol consolWithEmptyFields;

		protected override string ExpectedRootElementName
		{
			get { return "AWBHeader"; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "AWBHeaders"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var temporaryFullyPopulatedAWBFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ValueObjectDataAdapters.AWBs.TestFiles.FullyPopulatedAWB.xml");
			return new BusinessObjectAndExpectedOutputFileName(GetFullyPopulatedAWBHeader(), temporaryFullyPopulatedAWBFileName, ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated AWB");
		}

		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					ForwardingConsol consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					consol.JK_AgentType = Core.Constants.AgentType.Direct;
					consol.JK_RL_NKLoadPort = "AUPER";
					consol.JK_RL_NKDischargePort = "NZAKL";

					Transport tran = consol.MostInterestingTransportForBinding[0];
					tran.JW_ETD = new ZDateTime(2009, 03, 24);
					tran.JW_TransportMode = Core.Constants.TransportModes.Air;
					tran.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
					tran.JW_VoyageFlight = "CX123";
					consol.MasterBillMAWB = "10000001";
					tran.JW_DepotCutOff = new ZDateTime(2009, 03, 23);
					tran.JW_RL_NKLoadPort = "AUKTT";

					shipment = consol.Shipments.AddNew();
					shipment.JS_HouseBill = "hawb";
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKDestination = "NZAKL";
					shipment.JS_RL_NKOrigin = "AUMEL";
					shipment.JS_INCO = "FOB";

					PackLine packline = shipment.OuterPackLines.AddNew();
					packline.JL_ActualVolume = 0.34m;
					packline.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
					packline.JL_ActualWeight = 34m;
					packline.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
					packline.JL_PackageCount = 190;
					packline.JL_RH_NKCommodityCode = "BEER";

					Factory.Save();
				}
				return shipment;
			}
		}
		ForwardingShipment shipment;

		ExportAWBHeader GetFullyPopulatedAWBHeader()
		{
			Shipment.AWBHeader.Populate();
			ExportAWBHeader header = Shipment.AWBHeader;
			header.EH_ShippingLoadAndCount = 108;
			Shipment.JS_OverrideWaybillDefaults = true;

			//notify party
			header.EH_AlsoNotifyAddress = "Notify addr 1";
			header.EH_AlsoNotifyAddress2 = "Notify addr 2";
			header.EH_AlsoNotifyContactCode = "FX";
			header.EH_AlsoNotifyContactDetail = "13-3433";
			header.EH_AlsoNotifyCountryCode = "AU";
			header.EH_AlsoNotifyName = "notify company";
			header.EH_AlsoNotifyPostCode = "1023";
			header.EH_AlsoNotifyState = "NSW";
			header.EH_AWBIssuePlace = "Sydney";
			header.EH_IssuingAgentName = "EDI CUSTOMS BROKERS";
			header.EH_IssuingAgentAddress1 = "10 HUTCHESON STREET, ALBION  QLD";
			header.EH_IssuingAgentAddress2 = "BRISBANE, AU, 4010";
			//shipper
			header.EH_ReferenceNumber = Shipment.Consols[0].JK_UniqueConsignRef;
			header.EH_IsShipperOverriden = true;
			header.EH_ShipperOverride1 = "shipper name";
			header.EH_ShipperOverride2 = "shipper street line 1";
			header.EH_ShipperOverride3 = "shipper street line 2";
			header.EH_ShipperOverride4 = "shipper street line 3";
			header.EH_ShipperOverride5 = "shipper town";

			//consignee
			header.EH_ConsigneeAccount = "Consignee";
			header.EH_ConsigneeName = "consignee name";
			header.EH_ConsigneeAddress = "consignee co street";
			header.EH_ConsigneeAddress2 = "consignee addr 2";
			header.EH_ConsigneeContactCode = "TE";
			header.EH_ConsigneeContactDetail = "3939-2";
			header.EH_ConsigneeCountryCode = "NZ";

			header.EH_OtherPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;

			header.EH_OptionalShippingInformation = "shipping info1";
			header.EH_OptionalShippingInformation2 = "shipping info2";
			header.EH_InsuranceValue = 1000m;
			header.AWBRateLine1.NatureAndQtyOfGoods.Text = "Consolidation as per";
			header.AWBRateLine2.NatureAndQtyOfGoods.Text = "manifest";
			header.EH_SpecialHandlingCode = "T1";
			header.EH_NetRateCode = "HYA3004";
			header.EH_TaxesCOL = 100.3m;
			header.EH_TaxesPPD = 203.345m;
			header.EH_To1st = "ACC";
			header.EH_By1st = "CX";
			header.EH_To2nd = "CHC";
			header.EH_By2nd = "BA";
			header.EH_By3rd = "CX";
			header.EH_To3rd = "AKL";
			header.EH_Booking1stFlight = "CX";
			header.EH_Booking1stFlightDate = "12";
			header.EH_Booking2ndCarrier = "BA";
			header.EH_Booking2ndFlight = "234";
			header.EH_Booking2ndFlightDate = "12";
			header.EH_ValuationCOL = 45.23m;
			header.EH_WeightPrepaidCollect = ConsolExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			header.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			header.EH_AirportOfDepartureAndRequestRouteText = "SYDNEY";
			header.EH_AirportOfDestinationCode = "AKL";
			header.EH_AirportOfDestinationText = "AUCKLAND";
			header.EH_DeclaredValue = 1933m;
			header.EH_HouseDeclaredValueCurrency = Core.Constants.CurrencyCodes.Australia;
			header.EH_HouseCustomsValueCurrency = Core.Constants.CurrencyCodes.Australia;
			header.EH_CustomsValue = 339.3m;
			header.EH_AWBOriginCode = "SYD";
			header.EH_ShippersSignature = "CARRIER AGENT NAME";
			header.EH_ExtraShipperInfoLine1 = "shipper extra1";
			header.EH_ExtraShipperInfoLine2 = "shipper extra2";
			header.EH_ExtraCarrierInfoLine2 = "carrier info";
			header.EH_AWBIssueDate = new ZDateTime(2009, 01, 20);
			header.EH_Currency = Core.Constants.CurrencyCodes.Australia;
			header.EH_ChargesCode = "CC";
			header.EH_HandlingInformation = "handling";
			header.EH_OtherPPDCOL = "COL";
			header.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			header.EH_FinalizationDate = new ZDateTime(2013, 02, 21);

			ExportAWBAccountingInformation acctInfo = header.AWBAccountingInformations.AddNew();
			acctInfo.EA_InformationID = "GEN";
			acctInfo.EA_Information = "akckc";

			//other charges
			ExportAWBOtherCharges otherCharge = header.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "AW";
			otherCharge.EO_EntitlementCode = "A";
			otherCharge.EO_Amount = 120M;

			otherCharge = header.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "XY";
			otherCharge.EO_EntitlementCode = "C";
			otherCharge.EO_Amount = 130M;

			otherCharge = header.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "AB";
			otherCharge.EO_EntitlementCode = "C";
			otherCharge.EO_Amount = 140M;

			otherCharge = header.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "QW";
			otherCharge.EO_EntitlementCode = "E";
			otherCharge.EO_Amount = 190.12M;
			otherCharge.EO_ChargeDescription = "QW Desc";
			otherCharge.EO_PPDCLT = "CLT";

			//rateline
			ExportAWBRateLine rateLine = header.AWBRateLines.AddNew();
			rateLine.ER_NoOfPiecesOrRCP = "109";
			rateLine.ER_RateClass = "Q";
			rateLine.ER_CommodityItemNumber = "BEER";
			rateLine.ER_ChargeableWeight = 2.45m;
			rateLine.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine.ER_RateChargeOrDiscount = 0.23m;
			rateLine.ER_GrossWeight = 2.45m;
			rateLine.ER_LineCount = 1;
			rateLine.ER_Total = 39.3m;

			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var branchAUPER = SetupBranch("AUPER", "BR1");
			var branchAUMEL = SetupBranch("AUMEL", "BR2", true);
			var branchAUKTT = SetupBranch("AUKTT", "BR3");
			var branchAUSYD = SetupBranch("AUSYD", "BR4");

			Factory.Save();

			SetupRegistryForBranch(branchAUSYD, "SYD CARRIER NAME", "8888888", "Sydney", "95958");
			SetupRegistryForBranch(branchAUPER, "PER CARRIER NAME", "1234567", "PERTH", "19929");
			SetupRegistryForBranch(branchAUMEL, "MEL CARRIER NAME", "7685934", "MELBOURNE", "58963");
			SetupRegistryForBranch(branchAUKTT, "KTT CARRIER NAME", "7654321", "Kent Town", "12121");

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "carrier agent name";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "12345";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "carrier agent city";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "123456";

			FreightConfigurationRegistry.Instance.AWBIssueDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate);
			Enterprise.MasterFiles.Business.GlbStaff.CurrentUser.GS_FullName = "Developer";
		}

		GlbBranch SetupBranch(ZString homeport, ZString branchCode)
		{
			return SetupBranch(homeport, branchCode, false);
		}

		GlbBranch SetupBranch(ZString homeport, ZString branchCode, bool isRelatedPort)
		{
			var result = Factory.New<GlbBranch>();
			result.GB_RL_NKHomePort = homeport;
			if (!isRelatedPort)
			{
				result.GB_Code = branchCode;
			}
			else
			{
				var extraPort = result.ExtraPorts.AddNew();
				extraPort.GY_RL_NKAdditionalBranchRelatedPort = homeport;
			}
			result.GB_GC = Env.CurrentCompany.PK;

			return result;
		}

		void SetupRegistryForBranch(GlbBranch branch, ZString carrierAgentName, ZString carrierIATA, ZString carrierAgentCity, ZString agentAccountNo)
		{
			using (branch.SetAsTemporaryContext())
			{
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = carrierAgentName;
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = carrierIATA;
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = carrierAgentCity;
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = agentAccountNo;
			}
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"RateLines/GrossWeight/Description",
					"RateLines/CharageableWeight/DimensionType",
					"RateLines/CharageableWeight/Description",
					"Agent/ApprovedExportNumber" //this is for hk customs and it has been tested on functional review
				};
			}
		}
		#endregion
	}
}
