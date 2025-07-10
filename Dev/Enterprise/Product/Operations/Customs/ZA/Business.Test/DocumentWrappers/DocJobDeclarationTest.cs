using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		protected override DocDeclaration CreateDeclarationWrapper(JobDeclaration declaration)
		{
			var result = DocDeclaration.New(declaration, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>());
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		public override void TestHeadingTransportModeWithPackingMode()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_ContainerMode = Declaration.TransportModeAirCodeForTesting;

			ZString expected = "Maritime";
			AssertEquals("No packing mode in heading", expected, DeclarationWrapper.HeadingTransportMode);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			AssertEquals("Packing mode in heading", "FCL Maritime", DeclarationWrapper.HeadingTransportMode);
		}

		public override void TestHeadingTransportMode()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("Air", DeclarationWrapper.HeadingTransportMode);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("Maritime", DeclarationWrapper.HeadingTransportMode);
		}

		public void TestEntryHeaderCollection()
		{
			AssertEquals("EntryHeader Collection type", typeof(DocCusEntryHeaderCollection), DeclarationWrapper.RateEntryHeaders.GetType());
		}

		public void TestAgentCode()
		{
			MockDeclaration.Object.AgentCode = "AgentCode";
			AssertEquals("AgentCode", MockDeclarationWrapper.AgentCode);
		}

		public void TestAgentName()
		{
			MockDeclaration.Object.JE_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.OH_FullNameTruncated, MockDeclarationWrapper.AgentName);
		}

		public void TestAgentAddress1()
		{
			MockDeclaration.Object.JE_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.MainAddress.Address1, MockDeclarationWrapper.AgentAddress1);
		}

		public void TestAgentAddress2()
		{
			MockDeclaration.Object.JE_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.MainAddress.Address2, MockDeclarationWrapper.AgentAddress2);
		}

		public void TestDestination()
		{
			Assert(DeclarationWrapper.Destination.IsEmpty);
			Declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Destination", "Sydney", DeclarationWrapper.Destination);
		}

		public override void TestShipperDepartureNoticeDocumentHeader()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "Air Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "Maritime Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = Declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "FCL Maritime Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("ShipperDepartureNoticeDocumentHeader", "LCL Maritime Freight Report Name", DeclarationWrapper.ShipperDepartureNoticeDocumentHeader);
		}

		public override void TestTransportModeDescription()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TransportModeDescription", "Air", DeclarationWrapper.TransportModeDescription);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportModeDescription", "Maritime", DeclarationWrapper.TransportModeDescription);
		}

		[ExpectNoExceptions]
		public override void TestPortOfFirstArrival()
		{
		}

		[ExpectNoExceptions]
		public override void TestDateOfFirstArrival()
		{
		}

		#region ZDateTime Fields
		[TestDate(2005, 08, 10, 10, 0, 0)]
		public void TestIssuedAtDate()
		{
			Declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
			AssertEquals("2005-08-10 00:00", DeclarationWrapper.IssuedAtDate.ToString("yyyy-MM-dd HH:mm"));
		}

		[TestDate(2005, 08, 10, 10, 0, 0)]
		public void TestHouseBillIssuedDate()
		{
			Customs.Business.Bill bill = Declaration.Bills.AddNew();
			bill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_IssueDate = ZDateTime.Today;
			AssertEquals("2005-08-10 00:00", DeclarationWrapper.HouseBillIssuedDate.ToString("yyyy-MM-dd HH:mm"));
		}
		#endregion

		#region ZString Fields
		public void TestFirstEntryHeader()
		{
			AssertNull(DeclarationWrapper.FirstEntryHeader);
			CusEntryHeader header1 = Declaration.CustomsEntryHeaders.AddNew();
			header1.EntryNumber = "123";
			CusEntryHeader header2 = Declaration.CustomsEntryHeaders.AddNew();
			header2.EntryNumber = "456";

			AssertNotNull(DeclarationWrapper.FirstEntryHeader);
			AssertEquals("123", DeclarationWrapper.FirstEntryHeader.EntryNumber);
		}

		#region Supplier MICR

		public void TestSpiltingSupplierMICRNumber()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Supplier";
			Declaration.JE_OH_Supplier = header.PK;

			AssertEquals("MICR One", ZString.Empty, DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", ZString.Empty, DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", ZString.Empty, DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", ZString.Empty, DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", ZString.Empty, DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", ZString.Empty, DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", ZString.Empty, DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", ZString.Empty, DeclarationWrapper.MICREight);

			header.MiscServ.OM_IMEFTBankBSB = "1";
			Declaration.JE_OH_Supplier = header.PK;
			AssertEquals("MICR One", "1", DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", ZString.Empty, DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", ZString.Empty, DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", ZString.Empty, DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", ZString.Empty, DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", ZString.Empty, DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", ZString.Empty, DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", ZString.Empty, DeclarationWrapper.MICREight);

			header.MiscServ.OM_IMEFTBankBSB = "12";
			AssertEquals("MICR One", "1", DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", "2", DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", ZString.Empty, DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", ZString.Empty, DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", ZString.Empty, DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", ZString.Empty, DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", ZString.Empty, DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", ZString.Empty, DeclarationWrapper.MICREight);

			header.MiscServ.OM_IMEFTBankBSB = "123";
			AssertEquals("MICR One", "1", DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", "2", DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", "3", DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", ZString.Empty, DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", ZString.Empty, DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", ZString.Empty, DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", ZString.Empty, DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", ZString.Empty, DeclarationWrapper.MICREight);

			header.MiscServ.OM_IMEFTBankBSB = "1234";
			AssertEquals("MICR One", "1", DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", "2", DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", "3", DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", "4", DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", ZString.Empty, DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", ZString.Empty, DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", ZString.Empty, DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", ZString.Empty, DeclarationWrapper.MICREight);

			header.MiscServ.OM_IMEFTBankBSB = "12345";
			AssertEquals("MICR One", "1", DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", "2", DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", "3", DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", "4", DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", "5", DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", ZString.Empty, DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", ZString.Empty, DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", ZString.Empty, DeclarationWrapper.MICREight);

			header.MiscServ.OM_IMEFTBankBSB = "123456";
			AssertEquals("MICR One", "1", DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", "2", DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", "3", DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", "4", DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", "5", DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", "6", DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", ZString.Empty, DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", ZString.Empty, DeclarationWrapper.MICREight);

			header.MiscServ.OM_IMEFTBankBSB = "1234567";
			AssertEquals("MICR One", "1", DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", "2", DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", "3", DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", "4", DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", "5", DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", "6", DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", "7", DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", ZString.Empty, DeclarationWrapper.MICREight);

			header.MiscServ.OM_IMEFTBankBSB = "12345678";
			AssertEquals("MICR One", "1", DeclarationWrapper.MICROne);
			AssertEquals("MICR Two", "2", DeclarationWrapper.MICRTwo);
			AssertEquals("MICR Three", "3", DeclarationWrapper.MICRThree);
			AssertEquals("MICR Four", "4", DeclarationWrapper.MICRFour);
			AssertEquals("MICR Five", "5", DeclarationWrapper.MICRFive);
			AssertEquals("MICR Six", "6", DeclarationWrapper.MICRSix);
			AssertEquals("MICR Seven", "7", DeclarationWrapper.MICRSeven);
			AssertEquals("MICR Eight", "8", DeclarationWrapper.MICREight);
		}

		#endregion

		public void TestBOESightNumber()
		{
			Assert("Wrapper for BOE Sight Number should be empty", DeclarationWrapper.BOESightNumer.IsEmpty);
			Declaration.JE_BOESightNumber = "999";
			Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("BOE Sight Number", "999", DeclarationWrapper.BOESightNumer);
		}

		public void TestBOESightDate()
		{
			Assert("Wrapper for BOE Sight Date should be emtpty", DeclarationWrapper.BOESightDate.IsEmpty);
			Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			Declaration.JE_BOESightDate = ZDateTime.Today;
			AssertEquals("BOE Sight Year", ZDateTime.Today.Year.ToString().PadLeft(2, '0'), DeclarationWrapper.BOESightYear);
			AssertEquals("BOE Sight Month", ZDateTime.Today.Month.ToString().PadLeft(2, '0'), DeclarationWrapper.BOESightMonth);
			AssertEquals("BOE Sight Day", ZDateTime.Today.Day.ToString().PadLeft(2, '0'), DeclarationWrapper.BOESightDay);
		}

		public void TestMasterBillFormatting()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			Declaration.JE_MasterBill = "08111111111";
			Declaration.JE_CarrierCode = "MEA";
			AssertEquals("TransportDocumentNumber", "08111111111", DeclarationWrapper.TransportDocumentNumber);
		}

		public void TestTransportDocumentNumberSea()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_MasterBill = "08351925016";
			Declaration.JE_CarrierCode = "MEA";
			AssertEquals("Master Bill number", "MEA 08351925016", DeclarationWrapper.TransportDocumentNumber);
		}

		public void TestSpiltingTerms()
		{
			AssertEquals("Term Char 1", "", DeclarationWrapper.TermsCharOne);
			AssertEquals("Term Char 2", "", DeclarationWrapper.TermsCharTwo);
			AssertEquals("Term Char 3", "", DeclarationWrapper.TermsCharThree);
		}

		public void TestETDYear()
		{
			AssertEquals("ETDYear is empty", "", DeclarationWrapper.ETDYear);

			Declaration.JE_ExportDate = new ZDateTime(2004, 02, 04);
			AssertEquals("ETDYear is not empty", "2004", DeclarationWrapper.ETDYear);
		}

		public void TestETDMonth()
		{
			AssertEquals("ETDMonth is empty", "", DeclarationWrapper.ETDMonth);

			Declaration.JE_ExportDate = new ZDateTime(2004, 02, 04);
			AssertEquals("ETDMonth is not empty", "02", DeclarationWrapper.ETDMonth);
		}

		public void TestETDDate()
		{
			AssertEquals("ETDDate is empty", "", DeclarationWrapper.ETDDate);

			Declaration.JE_ExportDate = new ZDateTime(2004, 02, 04);
			AssertEquals("ETDDate is not empty", "04", DeclarationWrapper.ETDDate);
		}

		public void TestVehicleRegistration()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			Declaration.JE_VoyageFlightNo = "Rego";
			AssertEquals("VehicleRegistration", "Rego", DeclarationWrapper.VehicleRegistration);
		}

		public void TestVoyageFlightVehicleRegoNo()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_VoyageFlightNo = "Voyage";
			AssertEquals("VoyageFlightVehicleRegoNo", "Voyage", DeclarationWrapper.VoyageFlightVehicleRegoNo);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_VoyageFlightNo = "Flight";
			AssertEquals("VoyageFlightVehicleRegoNo", "Flight", DeclarationWrapper.VoyageFlightVehicleRegoNo);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			Declaration.JE_VoyageFlightNo = "Rego";
			AssertEquals("VoyageFlightVehicleRegoNo", "Rego", DeclarationWrapper.VoyageFlightVehicleRegoNo);
		}

		public void TestFirstCargoStatusCode()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "1";
			container.CO_FCL_LCL_AIR = "FCL";

			AssertEquals("FirstCargoStatusCode", "8", DeclarationWrapper.FirstCargoStatusCode);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("FirstCargoStatusCode", "11", DeclarationWrapper.FirstCargoStatusCode);
		}

		public void TestCargoStatusCodeForContainerMode()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "1";
			container.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("FirstCargoStatusCode", "8", DeclarationWrapper.FirstCargoStatusCode);

			container.CO_FCL_LCL_AIR = "LCL";
			AssertEquals("FirstCargoStatusCode", "7", DeclarationWrapper.FirstCargoStatusCode);

			container.CO_FCL_LCL_AIR = "FCG";
			AssertEquals("FirstCargoStatusCode", "5", DeclarationWrapper.FirstCargoStatusCode);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;
			AssertEquals("FirstCargoStatusCode", "4", DeclarationWrapper.FirstCargoStatusCode);
		}

		public void TestSecondCargoStatusCode()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";
			container1.CO_FCL_LCL_AIR = "FCL";

			CusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "2";
			container2.CO_FCL_LCL_AIR = "LCL";

			AssertEquals("SecondCargoStatusCode", "7", DeclarationWrapper.SecondCargoStatusCode);

			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("SecondCargoStatusCode", ZString.Empty, DeclarationWrapper.SecondCargoStatusCode);
		}

		public void TestSecondCargoStatusCodeForOnlyOneContainer()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "1";
			container1.CO_FCL_LCL_AIR = "FCL";
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("SecondCargoStatusCode", ZString.Empty, DeclarationWrapper.SecondCargoStatusCode);
		}

		public void TestIssuedAtYear()
		{
			AssertEquals("IssuedAtYear is empty", "", DeclarationWrapper.IssuedAtYear);

			Declaration.JE_MasterBillIssuedDate = new ZDateTime(2004, 02, 04);
			AssertEquals("IssuedAtYear is not empty", "2004", DeclarationWrapper.IssuedAtYear);
		}

		public void TestIssuedAtMonth()
		{
			AssertEquals("IssuedAtMonth is empty", "", DeclarationWrapper.IssuedAtMonth);

			Declaration.JE_MasterBillIssuedDate = new ZDateTime(2004, 02, 04);
			AssertEquals("IssuedAtMonth is not empty", "02", DeclarationWrapper.IssuedAtMonth);
		}

		public void TestIssuedAtDay()
		{
			AssertEquals("IssuedAtDay is empty", "", DeclarationWrapper.IssuedAtDay);

			Declaration.JE_MasterBillIssuedDate = new ZDateTime(2004, 02, 04);
			AssertEquals("IssuedAtDay is not empty", "04", DeclarationWrapper.IssuedAtDay);
		}

		public void TestContainerAndSealNumbers()
		{
			Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("ContainerAndSealNumbers", ZString.Empty, DeclarationWrapper.ContainerAndSealNumbers);

			CusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "Container1";
			container1.CO_Seal = "Seal";

			CusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "Container2";
			container2.CO_Seal = "Seal";

			CusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "Container3";
			container3.CO_Seal = "Seal";

			CusContainer container4 = Declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "Container4";
			container4.CO_Seal = "Seal";

			ZString expectedResult = "CONTAINER1 / SEAL\nCONTAINER2 / SEAL\nCONTAINER3 / SEAL\nCONTAINER4 / SEAL";
			AssertEquals("ContainerAndSealNumbers", expectedResult, DeclarationWrapper.ContainerAndSealNumbers);

			CusContainer container5 = Declaration.CusContainers.AddNew();
			container5.CO_ContainerNumber = "Container5";
			container5.CO_Seal = "Seal";

			expectedResult = "CONTAINER1 / SEAL "
				+ "CONTAINER2 / SEAL "
				+ "CONTAINER3 / SEAL "
				+ "CONTAINER4 / SEAL "
				+ "CONTAINER5 / SEAL";
			AssertEquals("ContainerAndSealNumbers", expectedResult, DeclarationWrapper.ContainerAndSealNumbers);

			Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			container2.CO_ContainerNumber = "TBA1";
			container4.CO_ContainerNumber = "TBA2";

			expectedResult = "CONTAINER1 / SEAL\nCONTAINER3 / SEAL\nCONTAINER5 / SEAL";
			AssertEquals("ContainerAndSealNumbers", expectedResult, DeclarationWrapper.ContainerAndSealNumbers);
		}

		public void TestCustomsOfficeCode()
		{
			Declaration.JE_CustomsOffice = "DD";
			AssertEquals("CustomsOfficeCode", "DD", DeclarationWrapper.CustomsOfficeCode);
		}

		public void TestCustomsOfficeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, "A", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, C.RefCusCodeListTypes.Codes.CustomsOffice, "B", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			var list = ZARefCusCodeListTypes.GetCustomsOfficeList(Factory);
			var code = list[0];
			Declaration.JE_CustomsOffice = code.Code;
			AssertEquals(code.Description, DeclarationWrapper.CustomsOfficeDescription);
		}

		public void TestETAYear()
		{
			AssertEquals("ETAYear is empty", "", DeclarationWrapper.ETAYear);

			Declaration.JE_DateOfArrival = new ZDateTime(2004, 02, 04);
			AssertEquals("ETAYear is not empty", "2004", DeclarationWrapper.ETAYear);
		}

		public void TestETAMonth()
		{
			AssertEquals("ETAMonth is empty", "", DeclarationWrapper.ETAMonth);

			Declaration.JE_DateOfArrival = new ZDateTime(2004, 02, 04);
			AssertEquals("ETAMonth is not empty", "02", DeclarationWrapper.ETAMonth);
		}

		public void TestETADate()
		{
			AssertEquals("ETADate is empty", "", DeclarationWrapper.ETADate);

			Declaration.JE_DateOfArrival = new ZDateTime(2004, 02, 04);
			AssertEquals("ETADate is not empty", "04", DeclarationWrapper.ETADate);
		}

		#endregion

		#region Collections

		public void TestEntryLinesCanBeAccessed()
		{
			Declaration.Invoices.DeleteAll();

			JobComInvoiceHeader invHeader1 = Declaration.Invoices.AddNew();
			JobComInvoiceHeader invHeader2 = Declaration.Invoices.AddNew();

			JobComInvoiceLine invLine11 = invHeader1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine12 = invHeader1.JobComInvoiceLines.AddNew();

			JobComInvoiceLine invLine21 = invHeader2.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine22 = invHeader2.JobComInvoiceLines.AddNew();

			SetupDataEligableForMerging(Declaration);

			invLine11.JI_Tariff = "1234.5";
			invLine12.JI_Tariff = "1234.5";

			invLine21.JI_Tariff = "3215.4";
			invLine22.JI_Tariff = "3215.4";

			new LineMerger(Declaration).DoMerge();

			CusEntryHeader header = Declaration.CustomsEntryHeaders[0];
			AssertEquals("There are two MergedLines", header.MergedLines.Count, 2);

			DocDeclaration docDeclaration = DocDeclaration.New(Declaration, Factory);

			AssertEquals(docDeclaration.EntryLines.Count, 2);
			AssertEquals(true, ((docDeclaration.EntryLines[1].Tariff == "12345") && (docDeclaration.EntryLines[0].Tariff == "32154")) || ((docDeclaration.EntryLines[1].Tariff == "32154") && (docDeclaration.EntryLines[0].Tariff == "12345")));
		}

		public void TestInvoiceHeaders()
		{
			Declaration.Invoices.DeleteAll();
			JobComInvoiceHeader header1 = Declaration.Invoices.AddNew();
			JobComInvoiceHeader header2 = Declaration.Invoices.AddNew();
			header1.JZ_InvoiceDate = new ZDateTime(2000, 1, 1);
			header2.JZ_InvoiceDate = new ZDateTime(2001, 2, 2);

			DocDeclaration docDeclaration = DocDeclaration.New(Declaration, Factory);
			AssertEquals("Must be two InvoiceHeaders", docDeclaration.InvoiceHeaders.Count, 2);
			AssertEquals(header1.JZ_InvoiceDate.ToShortDateString(), "01-Jan-00");
			AssertEquals(header2.JZ_InvoiceDate.ToShortDateString(), "02-Feb-01");
		}

		public void TestDA74Containers()
		{
			Declaration.CusContainers.RemoveAndDeleteAll();
			DocDeclaration testWrapper = DocDeclaration.New(Declaration, Factory);
			AssertEquals("DA74Container Collection always have at least 1 element", 1, testWrapper.DA74Containers.Count);
			AssertEquals("DA74Container container number blank", "", testWrapper.DA74Containers[0].FirstContainerNumber);
			AssertEquals("DA74Container container number blank", "", testWrapper.DA74Containers[0].SecondContainerNumber);
			AssertEquals("DA74ContainerFollowOn Collection should be empty", 0, testWrapper.DA74ContainersFollowOn.Count);

			CusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1234567";
			testWrapper = DocDeclaration.New(Declaration, Factory);
			AssertEquals("DA74Container Collection have 1 true element", 1, testWrapper.DA74Containers.Count);
			AssertEquals("DA74Container container number1 not blank", "CONT1234567", testWrapper.DA74Containers[0].FirstContainerNumber);
			AssertEquals("DA74Container container number2 blank", "", testWrapper.DA74Containers[0].SecondContainerNumber);

			CusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT7654321";
			testWrapper = DocDeclaration.New(Declaration, Factory);
			AssertEquals("DA74Container Collection have 1 true element", 1, testWrapper.DA74Containers.Count);
			AssertEquals("DA74Container container number1 not blank", "CONT1234567", testWrapper.DA74Containers[0].FirstContainerNumber);
			AssertEquals("DA74Container container number2 not blank", "CONT7654321", testWrapper.DA74Containers[0].SecondContainerNumber);

			CusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT8888888";
			testWrapper = DocDeclaration.New(Declaration, Factory);
			AssertEquals("DA74Container Collection have 2 elements", 2, testWrapper.DA74Containers.Count);
			AssertEquals("DA74Container container number1 not blank", "CONT8888888", testWrapper.DA74Containers[1].FirstContainerNumber);
			AssertEquals("DA74Container container number2 blank", "", testWrapper.DA74Containers[1].SecondContainerNumber);

			AssertEquals("DA74ContainerFollowOn Collection should be empty", 0, testWrapper.DA74ContainersFollowOn.Count);

			Declaration.CusContainers.RemoveAndDeleteAll();

			CusContainer container;
			for (int i = 1; i <= 5; i++)
			{
				container = Declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = "CONT" + i.ToString();
			}

			testWrapper = DocDeclaration.New(Declaration, Factory);
			testWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.FirstPageContainerRowCount, 2);
			AssertEquals("DA74Container Collection have 2 elements", 2, testWrapper.DA74Containers.Count);
			AssertEquals("DA74Container first row container number1 not blank", "CONT1", testWrapper.DA74Containers[0].FirstContainerNumber);
			AssertEquals("DA74Container first row container number2 blank", "CONT2", testWrapper.DA74Containers[0].SecondContainerNumber);
			AssertEquals("DA74Container second row container number1 not blank", "CONT3", testWrapper.DA74Containers[1].FirstContainerNumber);
			AssertEquals("DA74Container second row container number2 blank", "CONT4", testWrapper.DA74Containers[1].SecondContainerNumber);

			AssertEquals("DA74ContainerFollowOn Collection have 1 element", 1, testWrapper.DA74ContainersFollowOn.Count);
			AssertEquals("DA74ContainerFollowOn first row container number1 not blank", "CONT5", testWrapper.DA74ContainersFollowOn[0].FirstContainerNumber);
			AssertEquals("DA74ContainerFollowOn first row container number2 blank", "", testWrapper.DA74ContainersFollowOn[0].SecondContainerNumber);
		}
		#endregion

		#region Implementation
		void SetupDataEligableForMerging(Customs.Business.BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				line.JI_CEI = testInstruction.PK;
			}
		}

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.SouthAfrica; }
		}

		Mock<JobDeclaration> MockDeclaration
		{
			get
			{
				if (fMockDeclaration == null)
				{
					fMockDeclaration = Factory.NewMoq<JobDeclaration>();
				}
				return fMockDeclaration;
			}
		}
		Mock<JobDeclaration> fMockDeclaration;

		DocDeclaration MockDeclarationWrapper
		{
			get
			{
				if (fMockDeclarationWrapper == null)
				{
					fMockDeclarationWrapper = DocDeclaration.New(MockDeclaration.Object, Factory);
				}
				return fMockDeclarationWrapper;
			}
		}
		DocDeclaration fMockDeclarationWrapper;

		#endregion
	}
}
