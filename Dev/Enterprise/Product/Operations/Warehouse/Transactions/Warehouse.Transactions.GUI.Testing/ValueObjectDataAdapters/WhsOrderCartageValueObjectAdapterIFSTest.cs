using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

[TestedType(typeof(WhsOrderCartageValueObjectDataAdapterIFS))]
sealed class WhsOrderCartageValueObjectAdapterIFSTest : WhsOrderCartageValueObjectDataAdapterTest<Xsd.ConNote>
{
	#region TestImportTransportRef

	public void TestImportTransportRef()
	{
		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		AssertNotEquals("Precondition", "TOLL12345", PopulatedOrderSample.WD_TransportReference);

		xsdConNote.ConNo = "TOLL12345";
		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals("TOLL12345", PopulatedOrderSample.WD_TransportReference);
	}

	#endregion

	#region TestImportTransportCo

	public void TestImportTransportCo()
	{
		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.CarrierName = "ABC123 IN IFS";

		var newtransportCo = Factory.New<OrgHeader>();
		newtransportCo.OH_Code = "ABC123";
		newtransportCo.OH_IsLocalTransport = true;
		var service = newtransportCo.MiscServ.CarrierServiceLevels.AddNew();
		service.PL_Code = "DIR";
		service.PL_CarrierServiceLevelDescription = "Direct";

		var ifsOrg = Factory.Load<OrgHeader>(WarehouseDataRegistry.Instance.IFSOrgProxy.Value);
		var codeMap = ifsOrg.CreatePatternMatchOverrideForTest();
		codeMap.OO_LocalGuid = newtransportCo.PK;
		codeMap.OO_ForeignCode = "ABC123 IN IFS";

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals(newtransportCo.PK, PopulatedOrderSample.TransportCoPK);
		AssertEquals(false, Notifications.HasErrors);
	}

	public void TestImportTransportCo_WithInvalidData()
	{
		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.CarrierName = "ZZZZ";

		var transportCoPKBeforeImport = PopulatedOrderSample.TransportCoPK;

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals("Invalid data so transportCo should not update", transportCoPKBeforeImport, PopulatedOrderSample.TransportCoPK);
		AssertEquals(true, Notifications.HasErrors);
		AssertContains($"Transport Company (ZZZZ) does not exist in {Core.Constants.ProductName}.", Notifications.AsString);
	}

	#endregion

	#region TestImportServiceLevel

	public void TestImportServiceLevel()
	{
		var newService = PopulatedOrderSample.GetTransportCo().MiscServ.CarrierServiceLevels.AddNew();
		newService.PL_Code = "NEW";
		newService.PL_CarrierServiceLevelDescription = "NEW SERVICE";

		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.Service = "NEW SERVICE";

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals(newService.PL_Code, PopulatedOrderSample.WD_PL_NKCarrierServiceLevel);
		AssertEquals(false, Notifications.HasErrors);
	}

	public void TestImportServiceLevel_WithInvalidData()
	{
		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.Service = "XXXX";

		var serviceBeforeImport = PopulatedOrderSample.WD_PL_NKCarrierServiceLevel;

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));
		AssertEquals("Invalid data so service should not update", serviceBeforeImport, PopulatedOrderSample.WD_PL_NKCarrierServiceLevel);
		AssertEquals(true, Notifications.HasErrors);
		AssertContains($"Service Level (XXXX) does not exist in {Core.Constants.ProductName}.", Notifications.AsString);
	}

	#endregion

	#region TestImportCharges

	public void TestImportChargesWithWarehousePaysTransport()
	{
		PopulatedOrderSample.Warehouse.WarehouseAddress.OA_OH = PopulatedOrderSample.TransportBillTo.PK;

		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.TotCost = 99.10;
		xsdConNote.TotCostPlusMrkup = 109.25;
		xsdConNote.ConNo = "TOLL12345";

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));

		var job = (Job)PopulatedOrderSample.JobHeader;

		AssertEquals(1, job.Charges.Count);
		AssertEquals("TOLL12345", job.Charges[0].JR_Desc);
		AssertEquals(99.10m, job.Charges[0].JR_OSCostAmt);
		AssertEquals(109.25m, job.Charges[0].JR_OSSellAmt);
		AssertEquals("FRT", job.Charges[0].ChargeCode.AC_Code);
		AssertEquals(Env.CurrentCompany.Country.Currency.Code, job.Charges[0].JR_RX_NKCostCurrency);
		AssertEquals(job.Charges[0].JR_OH_CostAccount, PopulatedOrderSample.TransportCoPK);
		job.Dispose();
	}

	public void TestImportChargesWithWarehousePaysTransportAndLastOrderToUpdate_ApportionsAnyRoundingDiscrepancies()
	{
		PopulatedOrderSample.Warehouse.WarehouseAddress.OA_OH = PopulatedOrderSample.TransportBillTo.PK;

		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		adapter.IsLastOrderToUpdate = true;

		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.TotCost = 99.10;
		xsdConNote.TotCostPlusMrkup = 109.25;
		xsdConNote.ConNo = "TOLL12345";
		var detailLine = xsdConNote.FreightLineDetails.AddNew();
		detailLine.Ref = "ANOTHER ORDER";

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));

		var job = (Job)PopulatedOrderSample.JobHeader;

		AssertEquals(1, job.Charges.Count);
		AssertEquals("TOLL12345", job.Charges[0].JR_Desc);
		AssertEquals(49.55m, job.Charges[0].JR_OSCostAmt);
		AssertEquals(54.62m, job.Charges[0].JR_OSSellAmt);
		AssertEquals("FRT", job.Charges[0].ChargeCode.AC_Code);
		AssertEquals(Env.CurrentCompany.Country.Currency.Code, job.Charges[0].JR_RX_NKCostCurrency);
		AssertEquals(job.Charges[0].JR_OH_CostAccount, PopulatedOrderSample.TransportCoPK);
		job.Dispose();
	}

	public void TestImportChargesWithWarehousePaysTransport_AndExistingUnPostedFreightCharge()
	{
		PopulatedOrderSample.Warehouse.WarehouseAddress.OA_OH = PopulatedOrderSample.TransportBillTo.PK;
		var job = new Job.Loader(PopulatedOrderSample).TryCreate();
		var charge = job.Charges.AddNew();
		var chargeCode = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.WarehouseCartageChargeCode.Value);
		charge.JR_AC = chargeCode.PK;
		charge.JR_OSCostAmt = 1234.99m;

		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.TotCost = 99.10;
		xsdConNote.TotCostPlusMrkup = 109.25;
		xsdConNote.ConNo = "TOLL12345";

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));

		AssertEquals(1, job.Charges.Count);
		AssertEquals("TOLL12345", job.Charges[0].JR_Desc);
		AssertEquals(99.10m, job.Charges[0].JR_OSCostAmt);
		AssertEquals(109.25m, job.Charges[0].JR_OSSellAmt);
		AssertEquals("FRT", job.Charges[0].ChargeCode.AC_Code);
		AssertEquals(Env.CurrentCompany.Country.Currency.Code, job.Charges[0].JR_RX_NKCostCurrency);
		AssertEquals(PopulatedOrderSample.TransportCoPK, job.Charges[0].JR_OH_CostAccount);
	}

	public void TestImportChargesWithWarehousePaysTransport_AndExistingPostedFreightCharge()
	{
		PopulatedOrderSample.Warehouse.WarehouseAddress.OA_OH = PopulatedOrderSample.TransportBillTo.PK;
		PopulatedOrderSample.TransportBillTo.MainAddress.OA_OH = PopulatedOrderSample.Client.PK;

		var job = new Job.Loader(PopulatedOrderSample).TryCreate();
		var charge = job.Charges.AddNew();
		var chargeCode = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.WarehouseCartageChargeCode.Value);
		var fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");

		charge.JR_AC = chargeCode.PK;
		charge.JR_Desc = "POSTED CHARGE";
		charge.JR_RX_NKCostCurrency = Env.CurrentCompany.Country.Currency.Code;
		charge.JR_OSCostAmt = 1234.99m;
		charge.JR_OH_CostAccount = PopulatedOrderSample.TransportCoPK;
		charge.JR_GE = fesDepartment.PK;

		var postManager = new InvoicingPostManager(job);
		postManager.CreateTransactions(JobInvoicingPostingOption.LocalClient);

		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.TotCostPlusMrkup = 109.25;
		xsdConNote.ConNo = "TOLL12345";

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));

		AssertEquals(1, job.Charges.Count);
		AssertEquals("POSTED CHARGE", job.Charges[0].JR_Desc);
		AssertEquals(1234.99m, job.Charges[0].JR_OSCostAmt);
		AssertEquals("FRT", job.Charges[0].ChargeCode.AC_Code);
		AssertEquals(Env.CurrentCompany.Country.Currency.Code, job.Charges[0].JR_RX_NKCostCurrency);
		AssertEquals(PopulatedOrderSample.TransportCoPK, job.Charges[0].JR_OH_CostAccount);
	}

	public void TestImportChargesWithOtherPartyPaysTransport()
	{
		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		xsdConNote.TotCostPlusMrkup = 109.25;
		xsdConNote.ConNo = "TOLL12345";

		adapter.OrderExternalReference = xsdConNote.FreightLineDetails[0].Ref;
		adapter.CreateOrUpdateFromValueObject(xsdConNote, new ValueObjectImportContext(Factory, Notifications));

		AssertNull(PopulatedOrderSample.JobHeader);
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

		PopulatedOrderSample.WD_FinalisedDate = ZDateTimeOffset.Empty;
		PopulatedOrderSample.WD_DocketStatus = "ENT";
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

	#region TestExportOfPackageDescription

	public void TestExportOfPackageDescription()
	{
		var packType = Factory.New<RefPackType>();
		packType.F3_Code = "XXX";
		packType.F3_Description = "PACKAGE XXX";

		PopulatedOrderSample.WD_F3_NKTotalPackType = "XXX";
		PopulatedOrderSample.WD_PalletsSent = 0;
		PopulatedOrderSample.WD_PackagesSent = 100;

		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		AssertEquals("PACKAGE XXX", xsdConNote.FreightLineDetails[0].Desc);
	}

	#endregion

	#region TestExportOfPackageDescription_WithInvalidPackCode

	public void TestExportOfPackageDescription_WithInvalidPackCode()
	{
		PopulatedOrderSample.WD_F3_NKTotalPackType = "ZZZ";
		PopulatedOrderSample.WD_PalletsSent = 0;
		PopulatedOrderSample.WD_PackagesSent = 100;

		var adapter = new WhsOrderCartageValueObjectDataAdapterIFS();
		var xsdConNote = adapter.ExportToValueObject(PopulatedOrderSample, null);
		AssertEquals("Parcel", xsdConNote.FreightLineDetails[0].Desc);
	}

	#endregion

	#region Implementation

	#region PopulatedOrderSample

	protected override WhsOrder PopulatedOrderSample
	{
		get
		{
			if (ifsPopulatedOrderSample == null)
			{
				ifsPopulatedOrderSample = base.PopulatedOrderSample;

				ifsPopulatedOrderSample.Client.OH_FullName = "Client Name";

				// set up transport details
				var transportCo = Factory.NewWithValidTestData<OrgHeader>();
				transportCo.OH_Code = "ACME";
				ifsPopulatedOrderSample.TransportCoPK = transportCo.PK;
				var service = transportCo.MiscServ.CarrierServiceLevels.AddNew();
				service.PL_Code = "DIR";
				service.PL_CarrierServiceLevelDescription = "Direct";
				ifsPopulatedOrderSample.WD_PL_NKCarrierServiceLevel = "DIR";

				// Create an IFS Org Proxy and update registry
				var ifsOrg = Factory.NewWithValidTestData<OrgHeader>();
				WarehouseDataRegistry.Instance.IFSOrgProxy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ifsOrg.PK.ToGuid());

				// create a code Mapping on the IFS Org for the transportCo
				OrgPatternMatchOverride transportCoCodeMap = ifsOrg.CreatePatternMatchOverrideForTest();
				transportCoCodeMap.OO_ForeignCode = "ACME IN IFS";
				transportCoCodeMap.OO_LocalGuid = transportCo.PK;

				// create a code Mapping on the IFS Org for the client 
				var clientCodeMap = ifsOrg.CreatePatternMatchOverrideForTest();
				clientCodeMap.OO_ForeignCode = "ABIGAS IN IFS";
				clientCodeMap.OO_LocalGuid = PopulatedOrderSample.Client.PK;

				ifsPopulatedOrderSample.Warehouse.WW_OA_WarehouseAddress = ifsPopulatedOrderSample.Client.MainAddress.PK;

				var transportBillTo = Factory.NewWithValidTestData<OrgHeader>();
				transportBillTo.OH_FullName = "Matthew Pascoe";
				transportBillTo.MainAddress.OA_Address1 = "Building 1";
				transportBillTo.MainAddress.OA_Address2 = "78 West St";
				transportBillTo.MainAddress.OA_City = "Malabar";
				transportBillTo.MainAddress.OA_State = "NSW";
				transportBillTo.MainAddress.OA_PostCode = "2036";
				transportBillTo.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				transportBillTo.CompanyData.OB_IsDebtor = true;

				ifsPopulatedOrderSample.TransportBillToDocAddress.E2_OA_Address = transportBillTo.MainAddress.PK;
				var accountCodeMap = transportCo.CreatePatternMatchOverrideForTest();
				accountCodeMap.OO_ForeignCode = "A56789";
				accountCodeMap.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
				accountCodeMap.OO_LocalGuid = ifsPopulatedOrderSample.TransportBillTo.PK;

				ifsPopulatedOrderSample.WD_CubicSent = 0.009m;
				ifsPopulatedOrderSample.WD_WeightSent = 7.50m;
				Factory.Save();
			}

			return ifsPopulatedOrderSample;
		}
	}

	WhsOrder ifsPopulatedOrderSample;

	#endregion

	#region XmlNodesToExcludeFromCoverageTest

	protected override string[] XmlNodesToExcludeFromCoverageTest
	{
		get
		{
			return new string[]
			{
				"ConNo", //imported only
				"ConPrfx", //imported only
				"FreightLineDetails/Len", //imported only
				"FreightLineDetails/Hgt", //imported only
				"FreightLineDetails/Wdt", //imported only
				"TotCost", //imported only
				"TotCostPlusMrkup" //imported only
			};
		}
	}

	#endregion

	protected override bool IsImportFromValueObjectSupported => true;

	protected override string ExpectedRootCollectionElementName => "ConNoteObject";

	protected override string ExpectedRootElementName => "ConNote";

	protected override string EmptyOrderExpectedOutputFileName => resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsOrderCartageIFS.xml");

	protected override string PopulatedOrderExpectedOutputFileName => resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsOrderCartageIFS.xml");

	protected override ValueObjectDataAdapter<WhsOrder, Xsd.ConNote> GetNewBizObjXmlDataAdapter()
	{
		return new WhsOrderCartageValueObjectDataAdapterIFS();
	}

	#endregion

}
