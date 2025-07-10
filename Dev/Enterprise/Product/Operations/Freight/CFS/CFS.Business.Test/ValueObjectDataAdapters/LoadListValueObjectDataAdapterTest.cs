using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(LoadListValueObjectDataAdapter))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class LoadListValueObjectDataAdapterTest : BaseConsolValueObjectDataAdapterTest<CFSLoadListConsol>
	{
		#region Import

		public override void TestOrganisationTypeOnImport()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.PortOfLoading.Port.Value = "AUBNE";
			consolValue.ConsolDetail.PortOfDischarge.Port.Value = "NZAKL";
			consolValue.ConsolDetail.SendingAgent = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "SendingAgent", OrganisationTypes.Forwarder);
			consolValue.ConsolDetail.ReceivingAgent = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "ReceivingAgent", OrganisationTypes.Forwarder);
			consolValue.ConsolDetail.Carrier = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Carrier", OrganisationTypes.Carrier);
			consolValue.ConsolDetail.Creditor = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Creditor", OrganisationTypes.Creditor);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CFSLoadListConsol importedConsol = GetNewBizObjXmlDataAdapter().CreateOrUpdateFromValueObject(consolValue, context);
			AssertEquals("SendingAgent", "SendingAgent", importedConsol.SendingForwarder.OH_FullName);
			AssertEquals("SendingAgent is forwarder", true, importedConsol.SendingForwarder.OH_IsForwarder);

			AssertEquals("Carrier", "Carrier", importedConsol.ShippingLine.OH_FullName);
			AssertEquals("Carrier is shipping provider", true, importedConsol.ShippingLine.OH_IsShippingProvider);
			AssertEquals("Creditor", "Creditor", importedConsol.Creditor.OH_FullName);
			AssertEquals("Creditor is Creditor", true, importedConsol.Creditor.OH_IsCreditor);
		}

		public void TestImportReferenceNumbers()
		{
			CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
			entryTypeList.Add("AAA", (NoResString)"AAA Test Entry Type").IsUnique = true;
			entryTypeList.Add("BBB", (NoResString)"BBB Test Entry Type").IsUnique = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

			Xsd.Consol xmlConsol = new Xsd.Consol();
			xmlConsol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			Xsd.ReferenceNumber xsdNumber1 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber1.Type = "AAA";
			xsdNumber1.Number = "33333";
			xsdNumber1.Country.Value = "ZW";

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			xmlShipment.ShipmentDetails.Consignor = new Xsd.Organisation { EDICode = "EDICUS" };
			xmlShipment.ShipmentDetails.Consignee = new Xsd.Organisation { EDICode = "EDICUS" };
			xmlShipment.ShipmentDetails.PortOfOrigin.Port.Value = "AUBNE";
			xmlShipment.ShipmentDetails.PortofDestination.Port.Value = "SGSIN";
			xmlShipment.ShipmentIdentifier.AddNew(Xsd.ShipmentIdentifierType.Housebill, "HBL1");

			Xsd.ReferenceNumber xsdNumber2 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber2.Type = "BBB";
			xsdNumber2.Number = "44444";
			xsdNumber2.Country.Value = "BR";

			xmlConsol.Shipments.Add(xmlShipment);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CFSLoadListConsol importedConsol = GetNewBizObjXmlDataAdapter().CreateOrUpdateFromValueObject(xmlConsol, context);

			AssertEquals("No Customs Entry numbers from Consol should be imported!", 0, importedConsol.Numbers.Count);
			AssertEquals(1, importedConsol.Shipments.Count);
			AssertEquals("No Customs Entry numbers from Shipment should be imported!", 0, importedConsol.Shipments[0].Numbers.Count);
		}

		public void TestImportOnlyFieldsVisibleOnCFSShipmentForm()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var triggeredByEvents = new EventsWithSourceType(EventsWithSourceType.SourceType.Shipment, Factory.New<ProcessTaskNotification>(), null);
			var adpter = new CFSShipmentValueObjectDataAdapter<CFSShipment>(loadList, triggeredByEvents);

			var shipment = new Xsd.Shipment();

			var now = ZDateTime.Now;

			#region Import

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SHP");
			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			shipment.ShipmentDetails.AgentReference = "S00002015";

			shipment.ShipmentDetails.Consignee = new Xsd.Organisation { EDICode = "EDICUS" };
			shipment.ShipmentDetails.Consignor = new Xsd.Organisation { EDICode = "EDICUS" };

			shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipment.ShipmentDetails.PackingMode = Xsd.ContainerMode.FCL;
			shipment.ShipmentDetails.ForwardingShipmentType = Xsd.ForwardingShipmentType.STD;

			var org = new Xsd.Movement
			{
				Port = GetUnloco("AUSYD"),
				EstimatedDateTime = now
			};

			var des = new Xsd.Movement
			{
				Port = GetUnloco("SGSIN"),
				EstimatedDateTime = now.AddDays(2)
			};

			shipment.ShipmentDetails.PortOfOrigin = org;
			shipment.ShipmentDetails.PortofDestination = des;

			shipment.ShipmentDetails.InterimReceipt = "Interim Receipt";
			shipment.ShipmentDetails.GoodsDescription = "Goods Desc";
			shipment.ShipmentDetails.ServiceLevel = "STD";
			shipment.ShipmentDetails.ShipmentType = Xsd.ShipmentType.IMP;
			shipment.ShipmentDetails.Pickup = new Xsd.ShipmentShipmentDetailsPickup
			{
				DateOfReceipt = now,
				CFS = { Location = "WHSLOCAL" }
			};

			var houseBill = shipment.ShipmentIdentifier.AddNew();
			houseBill.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseBill.Value = "House Bill";

			var masterShipment = Factory.NewWithValidTestData<CFSShipment>();
			masterShipment.JS_ShipmentType = "CLD";
			masterShipment.JS_HouseBill = "S00002016";

			var coloadMaster = shipment.ShipmentIdentifier.AddNew();
			coloadMaster.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.CoLoadMaster;
			coloadMaster.Value = "S00002016";

			shipment.ShipmentDetails.TotalOuterPacksQty = new Xsd.DimensionValue { Value = 11m, DimensionType = "PKG" };

			shipment.ShipmentDetails.Weight = new Xsd.DimensionValue { Value = 22m, DimensionType = "KG" };

			shipment.ShipmentDetails.Volume = new Xsd.DimensionValue { Value = 33m, DimensionType = "M3" };

			var outerPackage = shipment.ShipmentDetails.Packages.AddNew();
			outerPackage.Weight = new Xsd.DimensionValue { Value = 9m, DimensionType = "KG" };
			outerPackage.PackType = "PKG";
			outerPackage.MarksAndNumbers = "Marks For Outer Package";

			var innerPackage = shipment.ShipmentDetails.InnerPackages.AddNew();
			innerPackage.Weight = new Xsd.DimensionValue { Value = 13m, DimensionType = "KG" };
			innerPackage.PackType = "PKG";
			innerPackage.GoodsDescription = "Desc For Inner Package";

			var transport = shipment.ShipmentDetails.TransportPlan.AddNew();
			transport.TransportMode = Xsd.TransportMode.SEA;
			transport.TransportType = Xsd.PlannedLegTransportType.MainVessel;
			transport.PortOfLoading = org;
			transport.PortOfDischarge = des;

			shipment.ShipmentDetails.LocalClient = new Xsd.Organisation { EDICode = "EDICUS" };
			var charge = shipment.Billing.ChargeLines.AddNew();
			charge.ChargeCode = "FRT";
			charge.Branch = GlbBranch.CurrentBranch.GB_Code;
			charge.OSSellAmount = new Xsd.FinancialValue { CurrencyCode = "AUD", Value = 77m };
			charge.OSCostAmount = new Xsd.FinancialValue { CurrencyCode = "AUD", Value = 88m };

			var entryNumber = shipment.ShipmentDetails.CustomsEntryNumbers.AddNew();
			entryNumber.Type = "XXX";
			entryNumber.Number = "CUS0000";
			entryNumber.Country = "AU";

			#endregion

			#region Don't Import

			shipment.ShipmentDetails.BookedDate = now;
			shipment.ShipmentDetails.ChargeableWeight = new Xsd.DimensionValue { Value = 99m };
			shipment.ShipmentDetails.DeclarationStyle = "SAC";
			shipment.ShipmentDetails.ExportBroker = new Xsd.Organisation { EDICode = "EXB" };
			shipment.ShipmentDetails.ExporterStatement = "STM";
			shipment.ShipmentDetails.HBLContainerMode = "PLT";
			shipment.ShipmentDetails.GoodsValue = new Xsd.FinancialValue { CurrencyCode = "AUD", Value = 900m };
			shipment.ShipmentDetails.HBLIssueDate = now;
			shipment.ShipmentDetails.ImportBroker = new Xsd.Organisation { EDICode = "IXB" };
			shipment.ShipmentDetails.Incoterm = "FOB";
			shipment.ShipmentDetails.InsuranceValue = new Xsd.FinancialValue { CurrencyCode = "AUD", Value = 30000m };
			shipment.ShipmentDetails.LoadingMeters = 700m;
			shipment.ShipmentDetails.MarksAndNumbers = "TST";
			shipment.ShipmentDetails.NoCopyBills = "2";
			shipment.ShipmentDetails.NoOriginalBills = "1";
			shipment.ShipmentDetails.NotifyParty = new Xsd.ContactReference { Organisation = new Xsd.Organisation { EDICode = "EDICUS" } };
			shipment.ShipmentDetails.OnForwardTo = "FW To";
			shipment.ShipmentDetails.OrderReferences = new[] { "Order" };
			shipment.ShipmentDetails.OnForwardToETA = now.AddDays(5);
			shipment.ShipmentDetails.OwnerReference = "Owner";
			shipment.ShipmentDetails.ReleaseType = Xsd.ReleaseType.STD;
			shipment.ShipmentDetails.ShippedOnBoardDate = now;
			shipment.ShipmentDetails.ShippedOnBoardType = Xsd.ShippedOnBoardType.CLN;
			shipment.ShipmentDetails.TEU = 5;
			shipment.ShipmentDetails.AdditionalTerms = "TST Terms";
			shipment.ShipmentDetails.ConsignorCODAmount = 200m;
			shipment.ShipmentDetails.TotalInnerPacksQty = new Xsd.DimensionValue { Value = 20m };
			shipment.ShipmentDetails.FreightRate = new Xsd.FinancialValue { CurrencyCode = "AUD", Value = 280m };

			shipment.Declaration = new Xsd.Declaration();
			shipment.Orders.AddNew();
			shipment.CustomValues.AddNew();

			var number = shipment.ShipmentDetails.ReferenceNumbers.AddNew();
			number.Type = "XXX";
			number.Number = "20150715";
			number.Country.Value = "AU";

			#endregion

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var importShipment = adpter.CreateOrUpdateFromValueObject(shipment, context);

			#region Import

			AssertEquals("S00002015", importShipment.JS_UniqueConsignRef);
			AssertEquals("AUSYD", importShipment.JS_RL_NKOrigin);
			AssertEquals("SGSIN", importShipment.JS_RL_NKDestination);
			AssertEquals("Interim Receipt", importShipment.JS_InterimReceipt);
			AssertEquals("Goods Desc", importShipment.JS_GoodsDescription);
			AssertEquals("STD", importShipment.JS_ShipmentType);
			AssertEquals("STD", importShipment.JS_RS_NKServiceLevel);
			AssertEquals("HOUSE BILL", importShipment.JS_HouseBill);
			AssertEquals(masterShipment.PK, importShipment.JS_JS_ColoadMasterShipment);
			AssertEquals(11, importShipment.JS_OuterPacks);
			AssertEquals("PKG", importShipment.JS_F3_NKPackType);
			AssertEquals(22m, importShipment.JS_ActualWeight);
			AssertEquals("KG", importShipment.JS_UnitOfWeight);
			AssertEquals(33m, importShipment.JS_ActualVolume);
			AssertEquals("M3", importShipment.JS_UnitOfVolume);
			AssertEquals(33m, importShipment.JS_ActualChargeable);
			AssertEquals("CUS0000", importShipment.CustomsEntryNumber);
			AssertEquals("XXX", importShipment.CustomsEntryNumberType);
			AssertEquals("WHSLOCAL", importShipment.JS_WarehouseLocation);
			AssertEquals(now.ToSmallDateTime(), importShipment.JS_A_RCV);

			AssertEquals("EDI CUSTOMS BROKERS", importShipment.Consignee.OH_FullName);
			AssertEquals("EDI CUSTOMS BROKERS", importShipment.Consignor.OH_FullName);

			var outerPackLines = importShipment.OuterPackLines;
			Assert(outerPackLines.Any());
			AssertEquals(9m, outerPackLines[0].JL_ActualWeight);
			AssertEquals("KG", outerPackLines[0].JL_ActualWeightUQ);
			AssertEquals("PKG", outerPackLines[0].JL_F3_NKPackType);
			AssertEquals("Marks For Outer Package", outerPackLines[0].JL_MarksAndNumbers);

			var innerPackLines = importShipment.InnerPackLines;
			Assert(innerPackLines.Any());
			AssertEquals(13m, innerPackLines[0].JL_ActualWeight);
			AssertEquals("KG", innerPackLines[0].JL_ActualWeightUQ);
			AssertEquals("PKG", innerPackLines[0].JL_F3_NKPackType);
			AssertEquals("Desc For Inner Package", innerPackLines[0].JL_Description);

			var transports = importShipment.Transports;
			Assert(transports.Any());
			AssertEquals("SEA", transports[0].JW_TransportMode);
			AssertEquals("MAI", transports[0].JW_TransportType);
			AssertEquals("AUSYD", transports[0].JW_RL_NKLoadPort);
			AssertEquals("SGSIN", transports[0].JW_RL_NKDiscPort);

			var job = importShipment.Job;
			AssertNotNull(job);

			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			Assert(charges.Any());
			AssertEquals("FRT", charges[0].ChargeCode.AC_Code);
			AssertEquals(77m, charges[0].JR_OSSellAmt);
			AssertEquals("AUD", charges[0].JR_RX_NKSellCurrency);
			AssertEquals(88M, charges[0].JR_OSCostAmt);
			AssertEquals("AUD", charges[0].JR_RX_NKCostCurrency);

			#endregion

			#region Don't Import

			Assert(importShipment.JS_HouseBillIssueDate.IsDefault);
			Assert(importShipment.JS_A_BKD.IsDefault);
			Assert(importShipment.JS_OH_ImportBroker.IsEmpty);
			Assert(importShipment.JS_OH_ExportBroker.IsEmpty);
			Assert(importShipment.DocsAndCartage.JP_ExportStatement.IsEmpty);
			Assert(importShipment.JS_HBLContainerPackModeOverride.IsEmpty);
			Assert(importShipment.JS_GoodsValue.IsDefault);
			Assert(importShipment.JS_HouseBillIssueDate.IsDefault);
			Assert(importShipment.JS_INCO.IsEmpty);
			Assert(importShipment.JS_E_ARV.IsDefault);
			Assert(importShipment.JS_E_DEP.IsDefault);
			Assert(importShipment.JS_HouseBillIssueDate.IsDefault);
			Assert(importShipment.JS_InsuranceValue.IsDefault);
			Assert(importShipment.JS_UnitFreightRate.IsDefault);
			Assert(importShipment.JS_ShipperCODAmount.IsDefault);
			Assert(importShipment.JS_LoadingMeters.IsDefault);
			Assert(importShipment.JS_MarksAndNumbers.IsEmpty);
			Assert(importShipment.JS_NoCopyBills == 3);
			Assert(importShipment.JS_NoOriginalBills == 3);
			Assert(importShipment.NotifyPartyDocumentaryAddress.Organisation == null);
			Assert(importShipment.JS_OrderReferences.IsEmpty);
			Assert(importShipment.JS_ReleaseType.IsEmpty);
			Assert(importShipment.JS_ShippedOnBoardDate.IsDefault);
			Assert(importShipment.JS_ShippedOnBoard == "SHP");
			Assert(importShipment.JS_AdditionalTerms.IsEmpty);
			Assert(importShipment.JS_ShipperCODAmount.IsDefault);
			Assert(importShipment.JS_AdditionalTerms.IsEmpty);
			Assert(importShipment.JS_TotalPackageCount.IsDefault);
			Assert(importShipment.JS_UnitFreightRate.IsDefault);
			Assert(importShipment.Numbers.Cast<CusEntryNumber>().All(c => c.CE_EntryNum != "20150715"));
			Assert(importShipment.DestinationCFSDepartures.IsNullOrEmpty());
			Assert(importShipment.DestinationCFSArrivals.IsNullOrEmpty());

			#endregion
		}

		Xsd.UNLOCO GetUnloco(string name)
		{
			var refUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, name));
			if (refUnloco != null)
			{
				return Xsd.UNLOCO.FromPort(refUnloco);
			}

			return new Xsd.UNLOCO();
		}

		#endregion

		#region Overrides

		protected override bool QuickBookingShouldBeMatchedAndConverted
		{
			get { return false; }
		}

		protected override ValueObjectDataAdapter<CFSLoadListConsol, Xsd.Consol> GetNewBizObjXmlDataAdapter()
		{
			return new LoadListValueObjectDataAdapter();
		}

		#endregion

		#region LoadLists and expected outputs

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			CFSLoadListConsol emptyConsol = NewBusinessObject();
			return new BusinessObjectAndExpectedOutputFileName(emptyConsol, BaseTestFilePath + "EmptyLoadList.xml", ValidationKind.None, "Empty LoadList");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var populatedConsolWithEmptyFields = Factory.NewWithValidTestData<CFSLoadListConsol>(TestBusinessObjectKind.PopulateAllDependentAndRelatedObjectsDeeply);
			populatedConsolWithEmptyFields.Shipments.RemoveAndDeleteAll();
			populatedConsolWithEmptyFields.Containers.RemoveAndDeleteAll();
			populatedConsolWithEmptyFields.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport = populatedConsolWithEmptyFields.Transports[0];
			transport.JW_VoyageFlight = "QF109";

			populatedConsolWithEmptyFields.JK_UniqueConsignRef = "";
			populatedConsolWithEmptyFields.JK_AgentsReference = "";

			populatedConsolWithEmptyFields.JK_OA_ShippingLineAddress = Factory.New<OrgHeader>().MainAddress.PK;
			populatedConsolWithEmptyFields.ShippingLine.OH_FullName = "ShippingLine";
			populatedConsolWithEmptyFields.ShippingLine.MainAddress.OA_Address1 = "#1";
			populatedConsolWithEmptyFields.ShippingLine.PrimaryRegistrationNumber.Number = "ShippingLine";

			populatedConsolWithEmptyFields.JK_OA_CreditorAddress = Factory.New<OrgHeader>().MainAddress.PK;
			populatedConsolWithEmptyFields.Creditor.OH_FullName = "Creditor";
			populatedConsolWithEmptyFields.Creditor.MainAddress.OA_Address1 = "#1";
			populatedConsolWithEmptyFields.Creditor.PrimaryRegistrationNumber.Number = "Creditor";

			populatedConsolWithEmptyFields.JK_OA_ContainerYardEmptyPickupAddress = populatedConsolWithEmptyFields.ContainerYardEmptyPickupAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.ContainerYardEmptyPickupAddress.Header.OH_Code = "CNT Yard 1";
			populatedConsolWithEmptyFields.ContainerYardEmptyPickupAddress.OA_Address1 = "CNT Yard Address";

			populatedConsolWithEmptyFields.JK_OA_ContainerYardEmptyReturnAddress = populatedConsolWithEmptyFields.ContainerYardEmptyReturnAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.ContainerYardEmptyReturnAddress.Header.OH_Code = "CNT Yard 2";
			populatedConsolWithEmptyFields.ContainerYardEmptyReturnAddress.OA_Address1 = "CNT Yard Address";

			populatedConsolWithEmptyFields.JK_OA_DepartureCTOAddress = populatedConsolWithEmptyFields.DepartureCTOAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.DepartureCTOAddress.Header.OH_Code = "DepCTO";
			populatedConsolWithEmptyFields.DepartureCTOAddress.OA_Address1 = "Departure CTO Address";

			populatedConsolWithEmptyFields.JK_OA_ArrivalCTOAddress = populatedConsolWithEmptyFields.ArrivalCTOAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.ArrivalCTOAddress.Header.OH_Code = "ArrCTO";
			populatedConsolWithEmptyFields.ArrivalCTOAddress.OA_Address1 = "Arrival CTO Address";

			populatedConsolWithEmptyFields.JK_OA_UnpackDepotAddress = populatedConsolWithEmptyFields.UnpackDepotAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.UnpackDepotAddress.Header.OH_Code = "UnpackDepot";
			populatedConsolWithEmptyFields.UnpackDepotAddress.OA_Address1 = "Unpack Depot Address";

			populatedConsolWithEmptyFields.JK_OA_PackDepotAddress = populatedConsolWithEmptyFields.PackDepotAddress.Header.MainAddress.PK;
			populatedConsolWithEmptyFields.PackDepotAddress.Header.OH_Code = "PackDepot";
			populatedConsolWithEmptyFields.PackDepotAddress.OA_Address1 = "Pack Depot Address";

			populatedConsolWithEmptyFields.Numbers.RemoveAll();

			return new BusinessObjectAndExpectedOutputFileName(populatedConsolWithEmptyFields, BaseTestFilePath + "PopulatedLoadListWithEmptyFields.xml", ValidationKind.None, "Populated consol with empty fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			CFSLoadListConsol consol = GetNewConsolWithValidTestData(Core.Constants.TransportModes.Air, true);
			return new BusinessObjectAndExpectedOutputFileName(consol, BaseTestFilePath + "AirLoadList.xml", ValidationKind.Xsd | ValidationKind.FactorySave, "Air");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			ArrayList result = new ArrayList();
			bool isImport = true;
			bool isExport = false;
			result.Add(new BusinessObjectAndExpectedOutputFileName(GetNewConsolWithValidTestData(Core.Constants.TransportModes.Sea, isImport), BaseTestFilePath + "SeaImportLoadList.xml", ValidationKind.Xsd | ValidationKind.FactorySave, "SeaImport"));
			result.Add(new BusinessObjectAndExpectedOutputFileName(GetNewConsolWithValidTestData(Core.Constants.TransportModes.Sea, isExport), BaseTestFilePath + "SeaExportLoadList.xml", ValidationKind.Xsd | ValidationKind.FactorySave, "SeaExport"));
			result.Add(new BusinessObjectAndExpectedOutputFileName(GetNewConsolWithValidTestData(Core.Constants.TransportModes.Air, isImport), BaseTestFilePath + "AirLoadList.xml", ValidationKind.Xsd | ValidationKind.FactorySave, "AirImport"));
			result.Add(new BusinessObjectAndExpectedOutputFileName(GetNewConsolWithValidTestData(Core.Constants.TransportModes.Rail, isImport), BaseTestFilePath + "RailLoadList.xml", ValidationKind.Xsd | ValidationKind.FactorySave, "Rail"));
			return (BusinessObjectAndExpectedOutputFileName[])result.ToArray(typeof(BusinessObjectAndExpectedOutputFileName));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public static readonly string BaseTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Freight\CFS\CFS.Business\ValueObjectDataAdapters\Testing\";

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				List<string> result = new List<string>(base.XmlNodesToExcludeFromCoverageTest);
				result.Add("ConsolDetail/Containers");      // covered in base xml data adapter
				result.Add("ConsolDetail/PortOfDischarge/ActualDateTime");
				result.Add("ConsolDetail/ReferenceNumbers/Type");
				result.Add("ConsolDetail/ReferenceNumbers/Number");
				result.Add("ConsolDetail/ReferenceNumbers/Country/Name");
				result.Add("ConsolDetail/ReferenceNumbers/Country/Value");

				return result.ToArray();
			}
		}

		#endregion

		#region New LoadLists with valid test data

		protected override CFSLoadListConsol GetNewConsolWithValidTestDataPopulatedByFactory()
		{
			return Factory.NewWithValidTestData<CFSLoadListConsol>();
		}

		CFSLoadListConsol GetNewConsolWithValidTestData(string transportMode, bool isImport)
		{
			var loadList = NewBusinessObject();
			SetupTransportModeDependantData(loadList, transportMode);

			var transport = loadList.Transports[0];
			transport.JW_Vessel = "ADMIRALENGRACHT";

			if (transport.IsSea)
			{
				var carrier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "MAERSK"));
				if (carrier == null)
				{
					carrier = Factory.New<OrgHeader>();
					carrier.OH_Code = "MAERSK";
					carrier.OH_FullName = "MAERSK";
					carrier.MainAddress.OA_Address1 = "MAERSK Address";
					carrier.OH_IsShippingLine = true;
					carrier.OH_IsShippingProvider = true;
				}
				transport.CarrierPK = carrier.PK;
			}

			if (isImport)
			{
				loadList.JK_RL_NKLoadPort = "MYPKG";
				loadList.JK_RL_NKDischargePort = "AUPER";

				transport.JW_LegOrder = 1;
				transport.JW_ETD = new ZDateTime(2005, 1, 1);
				transport.JW_ATD = new ZDateTime(2005, 1, 1);
				transport.JW_ETA = new ZDateTime(2005, 1, 2);
				transport.JW_ATA = new ZDateTime(2005, 1, 2);
				transport.JW_RL_NKLoadPort = "MYPKG";
				transport.JW_RL_NKDiscPort = "AUSYD";
			}
			else
			{
				loadList.JK_RL_NKLoadPort = "AUSYD";
				loadList.JK_RL_NKDischargePort = "SGSIN";

				transport.JW_LegOrder = 1;
				transport.JW_ETD = new ZDateTime(2005, 1, 6);
				transport.JW_ATD = new ZDateTime(2005, 1, 6);
				transport.JW_ETA = new ZDateTime(2005, 1, 7);
				transport.JW_ATA = new ZDateTime(2005, 1, 7);
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "NZAKL";
			}

			loadList.Logs.AddNew(Events.Booked, "Booked Reference", new ZDateTimeOffset(2005, 3, 3));
			loadList.Notes.AddNew(true, "MyNote", "Note Text");

			loadList.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			if (loadList.JK_TransportMode != Core.Constants.TransportModes.Rail)
			{
				loadList.Schedule.Origin.JA_Berth = "1";
				loadList.Schedule.Destination.JB_Berth = "2";
				loadList.Schedule.Origin.JA_DocumentaryCutoff = new ZDateTime(2005, 1, 10);
			}

			var railTransport = loadList.Transports.AddNew();
			railTransport.JW_TransportMode = Core.Constants.TransportModes.Rail;

			railTransport.JW_LegOrder = 2;
			railTransport.JW_RL_NKLoadPort = isImport ? "AUSYD" : "NZAKL";
			railTransport.JW_ETD = new ZDateTime(2005, 1, 2);
			railTransport.JW_RL_NKDiscPort = isImport ? "AUMEL" : "NZCHC";
			railTransport.JW_ETA = new ZDateTime(2005, 1, 3);
			railTransport.JW_Vessel = "ANADYR";
			railTransport.JW_VoyageFlight = "trans_voy";

			var seaTransport = loadList.Transports.AddNew();
			seaTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;

			seaTransport.JW_LegOrder = 3;
			seaTransport.JW_RL_NKLoadPort = isImport ? "AUMEL" : "NZCHC";
			seaTransport.JW_ETD = new ZDateTime(2005, 1, 4);
			seaTransport.JW_RL_NKDiscPort = isImport ? "AUPER" : "SGSIN";
			seaTransport.JW_ETA = new ZDateTime(2005, 1, 5);
			seaTransport.JW_Vessel = "ANADYR";
			seaTransport.JW_VoyageFlight = "trans_voy";

			SetupImportExportDependant(loadList, isImport);

			var container = loadList.Containers.AddNew();
			container.JC_ContainerNum = "cont123";
			container.JC_SealNum = "sealnum";
			container.JC_SealParty = "CAR";

			loadList.JK_AgentType = Core.Constants.AgentType.Agent;

			loadList.JK_AgentsReference = "";
			loadList.JK_BookingReference = "bookingref";

			loadList.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			loadList.ShippingLine.OH_Code = "SHPLIN";
			loadList.ShippingLine.OH_FullName = "ShippingLine";
			loadList.ShippingLine.MainAddress.OA_Address1 = "ShippingLine";

			loadList.JK_DateFirstForeignPort = new ZDateTime(2005, 2, 1);
			loadList.JK_DatePortOfFirstArrival = new ZDateTime(2005, 2, 2);
			loadList.JK_DateLastForeignPort = new ZDateTime(2005, 2, 3);
			loadList.JK_RL_NKPortOfFirstArrival = "AUSYD";
			loadList.JK_RL_NKLastForeignPort = "AUMEL";
			loadList.JK_RL_NKFirstForeignPort = "AUBBE";

			loadList.JK_OA_CreditorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			loadList.Creditor.OH_Code = "Creditor";
			loadList.Creditor.OH_FullName = "Creditor";
			loadList.Creditor.MainAddress.OA_Address1 = "Creditor";

			loadList.JK_OA_ArrivalCTOAddress = NewTestAddress("ArrivalCTOAddress").PK;
			loadList.JK_OA_ContainerYardEmptyReturnAddress = NewTestAddress("ReturnContainerYardEmptyAddress").PK;

			loadList.JK_OA_DepartureCTOAddress = NewTestAddress("DepartureCTOAddress").PK;
			loadList.JK_OA_PackDepotAddress = NewTestAddress("PackDepotAddress").PK;
			loadList.JK_OA_ContainerYardEmptyPickupAddress = NewTestAddress("PickupContainerYardEmptyAddress").PK;
			loadList.JK_OA_UnpackDepotAddress = NewTestAddress("UnpackDepotAddress").PK;

			loadList.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			loadList.JK_NoOriginalBills = 3;
			loadList.JK_NoCopyBills = 3;

			return loadList;
		}

		void SetupTransportModeDependantData(CFSLoadListConsol loadList, string transportMode)
		{
			loadList.JK_TransportMode = transportMode;
			Transport transport = loadList.Transports[0];

			if (loadList.JK_TransportMode == Core.Constants.TransportModes.Air)
			{
				transport.JW_VoyageFlight = "QF109";
				loadList.JK_MasterBillNum = "081";
				loadList.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			}
			else
			{
				transport.JW_VoyageFlight = "voyageno";
				loadList.JK_MasterBillNum = "masterbill";
				loadList.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			}
		}

		void SetupImportExportDependant(CFSLoadListConsol loadList, bool isImport)
		{
			loadList.JK_OH_Forwarder = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			if (loadList.IsImport())
			{
				loadList.InvoicingSupporter.SendingAgent.OH_Code = "RCVFOR";
				loadList.InvoicingSupporter.SendingAgent.OH_FullName = "ReceivingForwarder";
				loadList.InvoicingSupporter.SendingAgent.MainAddress.OA_Address1 = "ReceivingForwarder";
			}
			else
			{
				loadList.InvoicingSupporter.SendingAgent.OH_Code = "SNDFOR";
				loadList.InvoicingSupporter.SendingAgent.OH_FullName = "SendingForwarder";
				loadList.InvoicingSupporter.SendingAgent.MainAddress.OA_Address1 = "SendingForwarder";
			}
		}

		#endregion
	}
}
