using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PermitEntryLineGroupingTest : TestCaseWithFactory
	{
		public void TestPermitEntryLineGroupingByTariff()
		{
			var entry = CreateEntryForPermitTest();
			var mock = new Mock<PermitEntryLineGrouping>(entry);
			mock.CallBase = true;
			mock.Setup(m => m.IsDetailedTrackingEnabled).Returns(false);

			var permitGrouping = mock.Object;
			var lineGroups = permitGrouping.GetEntryLineGroups().ToList();
			AssertEquals("List of 1 Permits should be created", 1, lineGroups.Count);

			AssertEquals(ZString.Empty, lineGroups[0].CountryOfOrigin);
			AssertEquals(ZGuid.Empty, lineGroups[0].ManufacturerAddress);
			AssertEquals(ZString.Empty, lineGroups[0].ZoneStatus);
			AssertEquals("6602000000", lineGroups[0].TariffNumber);
			AssertEquals("PK", lineGroups[0].UnitOfMeasure);
			AssertEquals(ZString.Empty, lineGroups[0].ProductCode);
			AssertEquals("SV9-71026106", lineGroups[0].EntryNumber);
			AssertEquals("W235", lineGroups[0].FirmRegNo);
			AssertEquals((ZDecimal)45, lineGroups[0].FTZCustomsQty);
			AssertEquals((ZDecimal)1000, lineGroups[0].FTZCustomsValue);
		}

		public void TestPermitEntryLineGroupingByCombination()
		{
			var entry = CreateEntryForPermitTest();
			var mockForCombinationGrouping = new Mock<PermitEntryLineGrouping>(entry);
			mockForCombinationGrouping.CallBase = true;
			mockForCombinationGrouping.Setup(m => m.IsDetailedTrackingEnabled).Returns(true);

			var permitCombinationGrouping = mockForCombinationGrouping.Object;
			var lineGroupsCombination = permitCombinationGrouping.GetEntryLineGroups().ToList();
			AssertEquals("List of 2 Permits list should be created", 2, lineGroupsCombination.Count);

			AssertEquals("HK", lineGroupsCombination[0].CountryOfOrigin);
			AssertEquals(manufacturerAddressPK, lineGroupsCombination[0].ManufacturerAddress);
			AssertEquals("P", lineGroupsCombination[0].ZoneStatus);
			AssertEquals("6602000000", lineGroupsCombination[0].TariffNumber);
			AssertEquals("PK", lineGroupsCombination[0].UnitOfMeasure);
			AssertEquals(ZString.Empty, lineGroupsCombination[0].ProductCode);
			AssertEquals("SV9-71026106", lineGroupsCombination[0].EntryNumber);
			AssertEquals("W235", lineGroupsCombination[0].FirmRegNo);
			AssertEquals((ZDecimal)25, lineGroupsCombination[0].FTZCustomsQty);
			AssertEquals((ZDecimal)500, lineGroupsCombination[0].FTZCustomsValue);

			AssertEquals("HK", lineGroupsCombination[1].CountryOfOrigin);
			AssertEquals(manufacturerAddressPK2, lineGroupsCombination[1].ManufacturerAddress);
			AssertEquals("P", lineGroupsCombination[1].ZoneStatus);
			AssertEquals("6602000000", lineGroupsCombination[1].TariffNumber);
			AssertEquals("PK", lineGroupsCombination[1].UnitOfMeasure);
			AssertEquals(ZString.Empty, lineGroupsCombination[1].ProductCode);
			AssertEquals("SV9-71026106", lineGroupsCombination[1].EntryNumber);
			AssertEquals("W235", lineGroupsCombination[1].FirmRegNo);
			AssertEquals((ZDecimal)20, lineGroupsCombination[1].FTZCustomsQty);
			AssertEquals((ZDecimal)500, lineGroupsCombination[1].FTZCustomsValue);
		}

		public void TestGetOutwardEntryNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine = invoice1.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entryHeader);
			declaration.ImportEntryNumber = "00003877";
			Factory.Save();

			AssertEquals("XJ5-00003877", PermitEntryLineGrouping.GetOutwardEntryNumber(declaration));
		}

		public void TestDomesticLinesAreExcluded()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine = invoice1.InvoiceLines.AddNew();
			invoiceLine.US_ZoneStatus = "D";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			AssertNotNull(entryHeader);
			declaration.ImportEntryNumber = "00003877";
			Factory.Save();

			var lineGroups = new PermitEntryLineGrouping(declaration).GetEntryLineGroups();
			AssertEquals("1 line with ZoneStatus = D", 0, lineGroups.Count());
		}

		internal JobDeclaration CreateEntryForPermitTest()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			declaration.JE_ApplicationCode = "ACE";

			declaration.US_EntryFilerCode = "SV9";
			declaration.JE_TotalNoOfPacksPackType = "PK";
			declaration.US_PresentationDate = new ZDateTime(2017, 7, 7, 1, 2, 3);

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturerAddressPK = manufacturer.MainAddress.PK;

			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturerAddressPK2 = manufacturer2.MainAddress.PK;

			var warehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			warehouseOrg.OH_IsWarehouseClient = true;
			declaration.WarehouseDocAddress.OrganisationPK = warehouseOrg.PK;
			declaration.WarehouseAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "W235");

			declaration.Invoices.AddNew();

			var invoice_1 = declaration.InvoiceLines.AddNew();
			invoice_1.JI_Tariff = "9403200030";
			invoice_1.US_UC_NKCountryOfOrigin = "HK";
			invoice_1.JI_LinePrice = 4500;
			invoice_1.US_ManifestQty = 100;
			invoice_1.JI_PartNo = "21200";
			invoice_1.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoice_1.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice_2 = declaration.InvoiceLines.AddNew();
			invoice_2.JI_Tariff = "6602000000";
			invoice_2.US_UC_NKCountryOfOrigin = "HK";
			invoice_2.JI_LinePrice = 4500;
			invoice_2.US_ManifestQty = 100;
			invoice_2.JI_PartNo = "EACHESTEST";
			invoice_2.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoice_2.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice_3 = declaration.InvoiceLines.AddNew();
			invoice_3.JI_Tariff = "3920991000";
			invoice_3.US_UC_NKCountryOfOrigin = "HK";
			invoice_3.JI_LinePrice = 4000;
			invoice_3.US_ManifestQty = 100;
			invoice_3.JI_PartNo = "FTZ2";
			invoice_3.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoice_3.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice_4 = declaration.InvoiceLines.AddNew();
			invoice_4.JI_Tariff = "9102918010";
			invoice_4.US_UC_NKCountryOfOrigin = "HK";
			invoice_4.JI_LinePrice = 1000;
			invoice_4.US_ManifestQty = 10;
			invoice_4.JI_PartNo = "WATCH";
			invoice_4.US_SupTariff = "98";
			invoice_4.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoice_4.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice_5 = invoice_4.AddSecondaryInvoiceLine();
			AssertEquals(invoice_4, invoice_5.ParentTariffLine);
			invoice_5.JI_Tariff = "9102918020";
			invoice_5.US_UC_NKCountryOfOrigin = "HK";
			invoice_5.JI_LinePrice = 200;
			invoice_5.US_ManifestQty = 10;
			invoice_5.US_SupTariff = "98";
			invoice_5.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoice_5.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("IsChildLine", true, invoice_5.IsChildLine);

			var invoice_6 = invoice_4.AddSecondaryInvoiceLine();
			invoice_6.JI_Tariff = "9102918030";
			invoice_6.US_UC_NKCountryOfOrigin = "HK";
			invoice_6.JI_LinePrice = 200;
			invoice_6.US_ManifestQty = 10;
			invoice_6.US_SupTariff = "98";
			invoice_6.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoice_6.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("IsChildLine", true, invoice_6.IsChildLine);

			var invoice_7 = declaration.InvoiceLines.AddNew();
			invoice_7.JI_Tariff = "6602000000";
			invoice_7.US_UC_NKCountryOfOrigin = "HK";
			invoice_7.JI_LinePrice = 200;
			invoice_7.US_ManifestQty = 20;
			invoice_7.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoice_7.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice_8 = declaration.InvoiceLines.AddNew();
			invoice_8.JI_Tariff = "6602000000";
			invoice_8.US_UC_NKCountryOfOrigin = "HK";
			invoice_8.JI_LinePrice = 500;
			invoice_8.US_ManifestQty = 50;
			invoice_8.JI_PartNo = "ABC";
			invoice_8.US_SupTariff = "9802006000";
			invoice_8.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoice_8.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice_9 = declaration.InvoiceLines.AddNew();
			invoice_9.JI_Tariff = "6602000000";
			invoice_9.US_UC_NKCountryOfOrigin = "HK";
			invoice_9.JI_LinePrice = 500;
			invoice_9.US_ManifestQty = 25;
			invoice_9.JI_PartNo = "DEF";
			invoice_9.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoice_9.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoice_10 = declaration.InvoiceLines.AddNew();
			invoice_10.JI_Tariff = "6602000000";
			invoice_10.US_UC_NKCountryOfOrigin = "HK";
			invoice_10.JI_LinePrice = 500;
			invoice_10.US_ManifestQty = 20;
			invoice_10.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoice_10.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "71026106";

			return declaration;
		}

		ZGuid manufacturerAddressPK;
		ZGuid manufacturerAddressPK2;
	}
}
