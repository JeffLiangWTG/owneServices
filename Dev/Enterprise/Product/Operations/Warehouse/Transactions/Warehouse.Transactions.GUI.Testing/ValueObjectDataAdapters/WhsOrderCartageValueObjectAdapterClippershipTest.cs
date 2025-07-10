using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

abstract class WhsOrderCartageValueObjectDataAdapterTest<TXsd> : ValueObjectDataAdapterTest<WhsOrder, TXsd>
	where TXsd : IValueObject
{
	protected abstract string EmptyOrderExpectedOutputFileName { get; }

	protected abstract string PopulatedOrderExpectedOutputFileName { get; }

	protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
	{
		return new BusinessObjectAndExpectedOutputFileName(EmptyOrderSample, EmptyOrderExpectedOutputFileName, ValidationKind.None, "Empty WhsOrder (Cartage)");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
	{
		return new BusinessObjectAndExpectedOutputFileName(EmptyOrderSample, EmptyOrderExpectedOutputFileName, ValidationKind.None, "Empty WhsOrder (Cartage)");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
	{
		return new BusinessObjectAndExpectedOutputFileName(PopulatedOrderSample, PopulatedOrderExpectedOutputFileName, ValidationKind.None, "Populated WhsOrder (Cartage)");
	}

	protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
	{
		return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
	}

	protected WhsOrder EmptyOrderSample
	{
		get
		{
			if (emptyOrderSample == null)
			{
				emptyOrderSample = Helper.CreateWhsOrder(Client, Warehouse);
				emptyOrderSample.WD_RequiredDate = new ZDateTimeOffset(2008, 5, 6);

				Factory.Save();
			}
			return emptyOrderSample;
		}
	}
	WhsOrder emptyOrderSample;

	protected virtual WhsOrder PopulatedOrderSample
	{
		get
		{
			if (populatedOrderSample == null)
			{
				populatedOrderSample = Helper.CreateWhsOrder(Client, Warehouse);
				populatedOrderSample.WD_RequiredDate = new ZDateTimeOffset(2008, 5, 6);

				// orderlines
				var product = Helper.CreateProduct(Client, "CBR600RR");
				Helper.CreateWhsOrderLine(populatedOrderSample, product, 2M);

				Helper.CreatePickNew(populatedOrderSample);

				// addresses
				populatedOrderSample.Client.MainAddress.OA_Address2 = "CBD";
				populatedOrderSample.Client.MainAddress.OA_City = "Melbourne";
				populatedOrderSample.Client.MainAddress.OA_State = "VIC";
				populatedOrderSample.Client.MainAddress.OA_PostCode = "3000";
				populatedOrderSample.Client.MainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
				populatedOrderSample.Client.MainAddress.OA_Email = "rara@rara.com";
				populatedOrderSample.Client.MainAddress.OA_Phone = "123";
				populatedOrderSample.Client.MainAddress.OA_Mobile = "1234567";
				populatedOrderSample.Client.MainAddress.OA_Fax = "012";
				populatedOrderSample.Client.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;

				populatedOrderSample.GoodsBillToDocAddress.E2_OA_Address = populatedOrderSample.Client.MainAddress.PK;
				populatedOrderSample.TransportBillToDocAddress.E2_OA_Address = populatedOrderSample.Client.MainAddress.PK;
				populatedOrderSample.TransportCoDocAddress.E2_OA_Address = populatedOrderSample.Client.MainAddress.PK;

				// consignee address (tests residential overrride)
				populatedOrderSample.ConsigneeDocAddress.E2_OA_Address = populatedOrderSample.Client.MainAddress.PK;
				populatedOrderSample.ConsigneeDocAddress.E2_AddressOverride = true;
				populatedOrderSample.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;
				populatedOrderSample.ConsigneeDocAddress.E2_IsResidential = true;

				populatedOrderSample.TransportCoDocAddress.Organisation.OH_FullName = "Movers";

				// cartage leg
				populatedOrderSample.WD_RequiredDate = new ZDateTimeOffset(2008, 2, 10);
				populatedOrderSample.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
				populatedOrderSample.WD_FinalisedDate = new ZDateTimeOffset(2008, 2, 14);

				// handling. instr.
				populatedOrderSample.WD_HandlingInstructions = "The main road entrance is blocked due to roadworks so enter via the side street.";

				// outer packs
				populatedOrderSample.WD_TotalCubicUnit = Core.Constants.Volume.CubicDecimetres; // don't use M3, we want to test conversion for IFS
				populatedOrderSample.WD_TotalWeightUnit = Core.Constants.Weight.Grams; // don't use KG, we want to test conversion for IFS
				populatedOrderSample.WD_CubicSent = 6.0M;
				populatedOrderSample.WD_TotalCubic = 6.0M;
				populatedOrderSample.WD_WeightSentUserEntered = 500M;
				populatedOrderSample.WD_TotalWeight = 500M;
				populatedOrderSample.CalculateTotalsEnabled = false;
				populatedOrderSample.WD_PalletsSent = 3;
				populatedOrderSample.WD_GoodsDescription = "Guitars";

				// other
				populatedOrderSample.WD_PL_NKCarrierServiceLevel = "DIR";
				populatedOrderSample.WD_ExternalReference = "Order123";
				populatedOrderSample.WD_LocalCartInsuranceCost = 70.5M;
				populatedOrderSample.WD_TransportReference = "Tracking No.";

				Factory.Save();
			}
			return populatedOrderSample;
		}
	}
	WhsOrder populatedOrderSample;

	protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
	WhsTestHelperFunctions helper;

	protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
	{
		Assert(true); // this adapter does not support importing of new objects
	}

	//#warning replace this with Dave B's generic exporter
	protected ValueObjectDataAdapter<WhsOrder, TXsd> Adapter => adapter ?? (adapter = GetNewBizObjXmlDataAdapter());
	ValueObjectDataAdapter<WhsOrder, TXsd> adapter;
	//#warning replace this with Dave B's generic exporter

	protected NotificationBuffer Notifications => notifications ?? (notifications = new NotificationBuffer());
	NotificationBuffer notifications;

	protected override void SetUp()
	{
		base.SetUp();
		resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	protected override void TearDown()
	{
		base.TearDown();
		if (resourceRetriever.IsValueCreated)
		{
			resourceRetriever.Value.Dispose();
		}
	}

	protected Lazy<EmbeddedResourceRetriever> resourceRetriever;

	OrgHeader Client
	{
		get
		{
			if (client == null)
			{
				client = Helper.CreateClient();
				client.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
				client.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
				client.OH_RL_NKClosestPort = "AUMEL";
				client.MainAddress.FillWithValidTestData();
			}
			return client;
		}
	}
	OrgHeader client;

	WhsWarehouse Warehouse
	{
		get
		{
			if (warehouse == null)
			{
				warehouse = Helper.CreateWarehouse("Honda Warehouse");
				warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AU2CO";
				warehouse.WarehouseAddress.FillWithValidTestData();
			}
			return warehouse;
		}
	}
	WhsWarehouse warehouse;
}

[TestedType(typeof(WhsOrderCartageValueObjectDataAdapterClippership))]
sealed class WhsOrderCartageValueObjectAdapterClippershipTest : WhsOrderCartageValueObjectDataAdapterTest<Xsd.CartageJob>
{
	#region XmlNodesToExcludeFromCoverageTest

	protected override string[] XmlNodesToExcludeFromCoverageTest
	{
		get
		{
			return new string[]
			{
				// Org fields are unsued, we use docaddress fields instead.
				"BillTo",
				"TransportBillToAddress/Language",
				"BillToAddress/Language",
				"Carrier/OrganisationDetails/Addresses/Language",
				"CarrierAddress/Language",
				"CartageOrg/OrganisationDetails/Addresses/Language",
				"Consignee/AddressReference/Organisation/OrganisationDetails/Addresses/Language",
				"Consignee/Language",
				"Consignor/Language",
				"CartageLegs/Pickup/DocAddress/Language",
				"CartageLegs/Pickup/DocAddress/RegistrationNumber",
				"CartageLegs/Delivery/DocAddress/Language",
				"CartageLegs/Delivery/DocAddress/RegistrationNumber",
				"CartageLegs/TransportCo/OrganisationDetails/Addresses/Language",
				"SailingInfo/Item/DepartureCTO/Organisation/OrganisationDetails/Addresses/Language",
				"SailingInfo/Item/ArrivalCTO/Organisation/OrganisationDetails/Addresses/Language",
				"Carrier",
				"JobType",
				"Action",
				"ActionType",
				"MessageDescription",
				"MessageResponseAddress",
				"MessageSystemType",
				"CartageContractorJobNumber",
				"ClientQuoteReference",
				"ClientTransportDocument",
				"RequestedServiceTime",
				"TransportBillTo",
				"FreightCharges/ChargeCode", // imported only
				"FreightCharges/Description", // imported only
				"FreightCharges/RateChargeUnits", // imported only
				"FreightCharges/TotalAmount/CurrencyCode", // imported only
				"Carrier",
				"CartageOrg",
				"Consignee",
				"Consignor",
				"CartageLegs/LegType",
				"CartageLegs/LegStatus",
				"CartageLegs/Pickup/DocAddress/AddressReference/Organisation",
				"CartageLegs/Pickup/DocAddress/CompanyName",
				"CartageLegs/Pickup/DocAddress/CountryCode",
				"CartageLegs/Pickup/DocAddress/ContactName",
				"CartageLegs/Pickup/DocAddress/AddressLine1",
				"CartageLegs/Pickup/DocAddress/AddressLine2",
				"CartageLegs/Pickup/DocAddress/AddressCode",
				"CartageLegs/Pickup/DocAddress/CityOrSuburb",
				"CartageLegs/Pickup/DocAddress/StateOrProvince",
				"CartageLegs/Pickup/DocAddress/PostCode",
				"CartageLegs/Pickup/DocAddress/TelephoneNumbers/Value",
				"CartageLegs/Pickup/DocAddress/Email",
				"CartageLegs/Pickup/AdditionalInstructions/LoadingUnloadingConstraints/AccessPoint",
				"CartageLegs/Pickup/AdditionalInstructions/LoadingUnloadingConstraints/Communication",
				"CartageLegs/Pickup/AdditionalInstructions/LoadingUnloadingConstraints/DockHeight",
				"CartageLegs/Pickup/AdditionalInstructions/LoadingUnloadingConstraints/ContainerHandling",
				"CartageLegs/Pickup/AdditionalInstructions/LoadingUnloadingConstraints/LabourRequired",
				"CartageLegs/Pickup/AdditionalInstructions/LoadingUnloadingConstraints/FurtherConstraints",
				"CartageLegs/Delivery/DocAddress/AddressReference/Organisation",
				"CartageLegs/Delivery/DocAddress/CompanyName",
				"CartageLegs/Delivery/DocAddress/CountryCode",
				"CartageLegs/Delivery/DocAddress/ContactName",
				"CartageLegs/Delivery/DocAddress/AddressLine1",
				"CartageLegs/Delivery/DocAddress/AddressLine2",
				"CartageLegs/Delivery/DocAddress/AddressCode",
				"CartageLegs/Delivery/DocAddress/CityOrSuburb",
				"CartageLegs/Delivery/DocAddress/StateOrProvince",
				"CartageLegs/Delivery/DocAddress/PostCode",
				"CartageLegs/Delivery/DocAddress/TelephoneNumbers/Value",
				"CartageLegs/Delivery/DocAddress/Email",
				"CartageLegs/Delivery/AdditionalInstructions/LoadingUnloadingConstraints/AccessPoint",
				"CartageLegs/Delivery/AdditionalInstructions/LoadingUnloadingConstraints/Communication",
				"CartageLegs/Delivery/AdditionalInstructions/LoadingUnloadingConstraints/DockHeight",
				"CartageLegs/Delivery/AdditionalInstructions/LoadingUnloadingConstraints/ContainerHandling",
				"CartageLegs/Delivery/AdditionalInstructions/LoadingUnloadingConstraints/LabourRequired",
				"CartageLegs/Delivery/AdditionalInstructions/LoadingUnloadingConstraints/FurtherConstraints",
				"CartageLegs/MostDangerousGoodsCode",
				"CartageLegs/DangerousGoods",
				"CartageLegs/MostDangerousGoodsStandard",
				"CartageLegs/CartageLegDates/PickupTimeInDate",
				"CartageLegs/CartageLegDates/PickupTimeOutDate",
				"CartageLegs/CartageLegDates/DeliverTimeOutDate",
				"CartageLegs/CartageLegDates/PickupDemurrage",
				"CartageLegs/CartageLegDates/DeliveryDemurrage",
				"CartageLegs/CartageLegDates/DeliverTimeInDate",
				"CartageLegs/CartageLegDates/PlannedPickupFrom",
				"CartageLegs/CartageLegDates/PlannedPickupTo",
				"CartageLegs/TransportCo",
				"OuterPacks/OuterPacksWeight/Description",
				"OuterPacks/OuterPacksVolume/Description",
				"SailingInfo/PortOfLoading",
				"SailingInfo/PortOfDischarge",
				"SailingInfo/Item/ETD",
				"SailingInfo/Item/ETA",
				"SailingInfo/Item/ATD",
				"SailingInfo/Item/ATA",
				"SailingInfo/Item/LoadPortETA",
				"SailingInfo/Item/LoadPortATA",
				"SailingInfo/Item/DepartureCTO",
				"SailingInfo/Item/DepartureBerth",
				"SailingInfo/Item/ArrivalCTO",
				"SailingInfo/Item/ArrivalBerth",
				"SailingInfo/Item/DocCutOffDate",
				"SailingInfo/Item/IsTranshipment",
				"SailingInfo/Item/IsPublished",
				"CustomAttributes/CustomAttr1",
				"CustomAttributes/CustomAttr2",
				"CustomAttributes/CustomDate1",
				"CustomAttributes/CustomDate2",
				"CustomAttributes/CustomDecimal1",
				"CustomAttributes/CustomDecimal2",
				"CustomAttributes/CustomFlag1",
				"CustomAttributes/CustomFlag2",
				"Notes/CustomNoteTypeName",
				"Notes/NoteCreatedDateTime",
				"DropMode",

				// doc/or gaddresses are tested by the DocAddress/OrgAddress adapters. We just test a few simple addressesto make sure they are exported.
				"TransportBillToAddress/AddressReference/Organisation",
				"TransportBillToAddress/CompanyName",
				"TransportBillToAddress/CountryCode",
				"TransportBillToAddress/ContactName",
				"TransportBillToAddress/AddressLine1",
				"TransportBillToAddress/AddressLine2",
				"TransportBillToAddress/AddressCode",
				"TransportBillToAddress/CityOrSuburb",
				"TransportBillToAddress/StateOrProvince",
				"TransportBillToAddress/PostCode",
				"TransportBillToAddress/TelephoneNumbers/Value",
				"TransportBillToAddress/RegistrationNumber",
				"BillToAddress/AddressReference/Organisation",
				"BillToAddress/CompanyName",
				"BillToAddress/CountryCode",
				"BillToAddress/ContactName",
				"BillToAddress/AddressLine1",
				"BillToAddress/AddressLine2",
				"BillToAddress/AddressCode",
				"BillToAddress/CityOrSuburb",
				"BillToAddress/StateOrProvince",
				"BillToAddress/PostCode",
				"BillToAddress/TelephoneNumbers/Value",
				"BillToAddress/RegistrationNumber",
				"CarrierAddress/AddressReference/Organisation",
				"CarrierAddress/CompanyName",
				"CarrierAddress/CountryCode",
				"CarrierAddress/ContactName",
				"CarrierAddress/AddressLine1",
				"CarrierAddress/AddressLine2",
				"CarrierAddress/AddressCode",
				"CarrierAddress/CityOrSuburb",
				"CarrierAddress/StateOrProvince",
				"CarrierAddress/PostCode",
				"CarrierAddress/TelephoneNumbers/Value",
				"CarrierAddress/RegistrationNumber",
				"TransportBillToAddress/IsResidential",
				"BillToAddress/IsResidential",
				"CarrierAddress/IsResidential",
				"Consignee/IsResidential",
				"Consignor/IsResidential",
				"CartageLegs/Pickup/DocAddress/IsResidential",

				// imported only
				"InvoiceNumber"
			};
		}
	}

	#endregion

	#region TestTransportRefImport

	public void TestTransportRefImport()
	{
		var xsdCartage = Adapter.ExportToValueObject(PopulatedOrderSample, null);
		var packages = new Xsd.CartageLegPackageRecords();
		xsdCartage.CartageLegs[0].Item = packages;

		var package1 = packages.Packs.AddNew();
		var package2 = packages.Packs.AddNew();
		package1.TransportRef = "Moo";
		package2.TransportRef = "Oink";

		Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals("Tracking No.", PopulatedOrderSample.WD_TransportReference);
		AssertEquals("Moo", PopulatedOrderSample.References[0].WX_Reference);
		AssertEquals("Oink", PopulatedOrderSample.References[1].WX_Reference);
		AssertEquals(WarehouseAdditionalReferenceTypes.Codes.TransportReference, PopulatedOrderSample.References[0].WX_RefType);
		AssertEquals(WarehouseAdditionalReferenceTypes.Codes.TransportReference, PopulatedOrderSample.References[1].WX_RefType);

		AssertEquals(false, Notifications.HasErrors);
	}

	#endregion

	#region TestOuterPacksWeightAndVolumeImport

	public void TestOuterPacksWeightAndVolumeImport()
	{
		var xsdCartage = Adapter.ExportToValueObject(PopulatedOrderSample, null);

		xsdCartage.OuterPacks.OuterPacksVolume.Value = 7M;
		xsdCartage.OuterPacks.OuterPacksVolume.DimensionType = Core.Constants.Volume.CubicYards;
		xsdCartage.OuterPacks.OuterPacksWeight.Value = 6M;
		xsdCartage.OuterPacks.OuterPacksWeight.DimensionType = "";
		xsdCartage.GoodsDescription = "6 Pallet(s)";

		Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));

		AssertEquals("6 Pallet(s)", PopulatedOrderSample.WD_GoodsDescription);
		AssertEquals(7M, PopulatedOrderSample.WD_CubicSent);
		AssertEquals(7M, PopulatedOrderSample.WD_TotalCubic);
		AssertEquals(Core.Constants.Volume.CubicYards, PopulatedOrderSample.WD_TotalCubicUnit);

		AssertEquals("Imported Weight Unit is empty, should not have overwritten Order.WD_WeightSentUserEntered", 500M, PopulatedOrderSample.WD_WeightSentUserEntered);
		AssertEquals("Imported Weight Unit is empty, should not have overwritten Order.WD_TotalWeight", 500M, PopulatedOrderSample.WD_TotalWeight);
		AssertEquals("Imported Weight Unit is empty, should not have overwritten Order.WD_TotalWeightUnit", Core.Constants.Weight.Grams, PopulatedOrderSample.WD_TotalWeightUnit);

		AssertEquals(false, Notifications.HasErrors);
	}

	#endregion

	#region TestOuterPacksTotalsImport

	public void TestOuterPacksTotalsImport()
	{
		var xsdCartage = Adapter.ExportToValueObject(PopulatedOrderSample, null);
		var outers = xsdCartage.OuterPacks;

		AssertOuterPacksTotalsImport(xsdCartage, 0, "Something", 3, 0, 0, "");
		AssertOuterPacksTotalsImport(xsdCartage, 1, "Pallet", 1, 0, 0, "");
		AssertOuterPacksTotalsImport(xsdCartage, 10, "Box", 3, 10, 0, "");
		AssertOuterPacksTotalsImport(xsdCartage, 100, "Random Units", 3, 0, 100, "Ran");
	}

	void AssertOuterPacksTotalsImport(Xsd.CartageJob xsdCartage, ZInt packageCount, ZString packageType,
		ZShort expectedWD_PalletsSent, ZDecimal expectedWD_UnitsSent, ZInt expectedWD_PackagesSent, ZString expectedPackageType)
	{
		// reset values
		PopulatedOrderSample.WD_PalletsSent = 3;
		PopulatedOrderSample.WD_UnitsSent = 0;
		PopulatedOrderSample.WD_PackagesSent = 0;
		PopulatedOrderSample.WD_F3_NKTotalPackType = "";

		// import
		var outers = xsdCartage.OuterPacks;
		outers.OuterPacksNo = packageCount;
		outers.OuterPacksType = packageType;
		Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));

		AssertEquals(expectedWD_PalletsSent, PopulatedOrderSample.WD_PalletsSent);
		AssertEquals(expectedWD_UnitsSent, PopulatedOrderSample.WD_UnitsSent);
		AssertEquals(expectedWD_PackagesSent, PopulatedOrderSample.WD_PackagesSent);
		AssertEquals(expectedPackageType, PopulatedOrderSample.WD_F3_NKTotalPackType);

		AssertEquals(false, Notifications.HasErrors);
	}

	#endregion

	#region TestServiceLevelImport

	public void TestServiceLevelImport()
	{
		var xsdCartage = Adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdCartage.ServiceLevel = "D2D";

		AssertNotEquals("Precondition", "D2D", PopulatedOrderSample.WD_PL_NKCarrierServiceLevel);

		Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals("D2D", PopulatedOrderSample.WD_PL_NKCarrierServiceLevel);
		AssertEquals(false, Notifications.HasErrors);
	}

	#endregion

	#region TestChargesImport

	public void TestChargesImport()
	{
		var xsdCartage = Adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdCartage.InvoiceNumber = "abc";

		var charge1 = xsdCartage.FreightCharges.AddNew();
		charge1.ChargeCode = "INSUR";
		charge1.Description = "Some Insurance";
		charge1.TotalAmount.Value = 70M;
		charge1.TotalAmount.CurrencyCode = "USD";

		var charge2 = xsdCartage.FreightCharges.AddNew();
		charge2.ChargeCode = "";
		charge2.Description = "Description with no code";
		charge2.TotalAmount.Value = 85M;

		Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));

		var job = (Job)PopulatedOrderSample.JobHeader;

		AssertEquals(2, job.Charges.Count);
		AssertEquals("INSUR", job.Charges[0].ChargeCode.AC_Code);
		AssertEquals("Some Insurance", job.Charges[0].JR_Desc);
		AssertEquals("USD", job.Charges[0].JR_RX_NKCostCurrency);
		AssertEquals(70M, job.Charges[0].JR_OSCostAmt);
		AssertEquals(job.Charges[0].JR_OH_CostAccount, PopulatedOrderSample.TransportCoPK);
		AssertEquals("Cost must be posted on import so that AutoRating does not overwrite it.", true, job.Charges[0].IsCostPosted);

		AssertEquals("FRT", job.Charges[1].ChargeCode.AC_Code);
		AssertEquals("International Freight", job.Charges[1].ChargeCode.AC_Desc);
		AssertEquals("Description with no code", job.Charges[1].JR_Desc);
		AssertEquals(Env.CurrentCompany.Country.Currency.Code, job.Charges[1].JR_RX_NKCostCurrency);
		AssertEquals(85M, job.Charges[1].JR_OSCostAmt);
		AssertEquals(job.Charges[1].JR_OH_CostAccount, PopulatedOrderSample.TransportCoPK);
		AssertEquals("Cost must be posted on import so that AutoRating does not overwrite it.", true, job.Charges[1].IsCostPosted);

		AssertEquals(false, Notifications.HasErrors);
	}

	public void TestChargesImportDoesNotPostCostsWithoutInvoiceNumber()
	{
		var adapter = new WhsOrderCartageValueObjectDataAdapterClippership();
		var xsdCartage = adapter.ExportToValueObject(PopulatedOrderSample, null);

		var charge = xsdCartage.FreightCharges.AddNew();
		charge.ChargeCode = "INSUR";
		charge.Description = "Some Insurance";
		charge.TotalAmount.Value = 70M;
		charge.TotalAmount.CurrencyCode = "USD";

		xsdCartage.InvoiceNumber = "";

		adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals(false, ((Job)PopulatedOrderSample.JobHeader).Charges[0].IsCostPosted);
		AssertEquals(false, Notifications.HasErrors);
	}

	public void TestChargesImportErrorsIfOrderHasNoTransportCo()
	{
		PopulatedOrderSample.TransportCoDocAddress.E2_OA_Address = ZGuid.Empty;

		var xsdCartage = Adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdCartage.InvoiceNumber = "abc";

		var charge = xsdCartage.FreightCharges.AddNew();
		charge.ChargeCode = "INSUR";
		charge.Description = "Some Insurance";
		charge.TotalAmount.Value = 70M;
		charge.TotalAmount.CurrencyCode = "USD";

		Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals("Order has no Transport Company.", true, Notifications.HasErrors);
	}

	#endregion

	#region TestImportFinalisesOrder

	public void TestImportFinalisesOrder()
	{
		PopulatedOrderSample.WD_FinalisedDate = ZDateTimeOffset.Empty;
		PopulatedOrderSample.WD_DocketStatus = "ENT";
		PopulatedOrderSample.WD_WP = ZGuid.Empty;
		var xsdCartage = Adapter.ExportToValueObject(PopulatedOrderSample, null);

		Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals("Registry setting not set, Order should not be finalised.", false, PopulatedOrderSample.IsFinalised);
		AssertNull("Registry setting not set, Pick should not be created.", PopulatedOrderSample.Pick);

		SystemDataRegistry.Instance.FinaliseOrderOnCartageImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

		using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
		{
			Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));
		}
		AssertEquals(true, PopulatedOrderSample.IsFinalised);
		AssertEquals(true, PopulatedOrderSample.Pick.IsFinalised);
		AssertEquals(false, Notifications.HasErrors);
		AssertEquals(false, Notifications.HasWarnings);
	}

	public void TestImportFinaliseFailureCleanUpAndWarning()
	{
		SystemDataRegistry.Instance.FinaliseOrderOnCartageImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

		PopulatedOrderSample.Lines[0].WE_TransactionQuantity = -1m; // create an error on the Order
		PopulatedOrderSample.WD_WP = ZGuid.Empty;

		using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
		{
			var xsdCartage = Adapter.ExportToValueObject(PopulatedOrderSample, null);

			Adapter.ImportFromValueObject(PopulatedOrderSample, xsdCartage, new ValueObjectImportContext(Factory, Notifications));
			AssertEquals("Validation error, Order should not be finalised.", false, PopulatedOrderSample.IsFinalised);
			AssertNull("Validation error, Pick should not be created.", PopulatedOrderSample.Pick);
			AssertEquals("Order should still be imported when finalise fails.", false, Notifications.HasErrors);
			AssertEquals("A warning should be added when the Order finalisation fails.", true, Notifications.HasWarnings);
		}
	}

	#endregion

	#region Implementation

	protected override string ExpectedRootCollectionElementName => "CartageJobs";

	protected override string ExpectedRootElementName => "CartageJob";

	protected override string EmptyOrderExpectedOutputFileName => resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsOrderCartage.xml");

	protected override string PopulatedOrderExpectedOutputFileName => resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsOrderCartage.xml");

	protected override ValueObjectDataAdapter<WhsOrder, Xsd.CartageJob> GetNewBizObjXmlDataAdapter()
	{
		return new WhsOrderCartageValueObjectDataAdapterClippership();
	}

	#endregion
}
