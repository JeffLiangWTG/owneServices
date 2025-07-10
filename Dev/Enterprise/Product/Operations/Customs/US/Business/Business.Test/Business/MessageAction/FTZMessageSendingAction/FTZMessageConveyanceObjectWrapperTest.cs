using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZMessageConveyanceObjectWrapperTest : TestCaseWithFactory
	{
		public void TestIFTZConveyanceMembers()
		{
			var carrierAB = Factory.New<USCarrierCombined>();
			carrierAB.UI_Code = "AB";
			carrierAB.UI_Name = "AB NAME";
			var carrierCD = Factory.New<USCarrierCombined>();
			carrierCD.UI_Code = "CD";
			carrierCD.UI_Name = "CD TEST";
			var carrierEF = Factory.New<USCarrierCombined>();
			carrierEF.UI_Code = "EF";
			carrierEF.UI_Name = "EFGHK";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_VoyageFlightNo = "QF001001";
			declaration.US_UI_NKCarrierSCAC = "AB";
			declaration.US_DateOfExport = new ZDateTime(2021, 04, 28);
			declaration.JE_DateOfArrival = new ZDateTime(2021, 04, 29);
			declaration.US_SchDArrival = "2904";
			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = "HB";
			houseBill1.CU_BillNum = "1234";
			houseBill1.US_SESplitShip = true;
			var splitDetail1 = houseBill1.ITAndSplitDetails.AddNew();
			splitDetail1.US_CarrierCode = "CD";
			splitDetail1.US_FlightNumber = "QF002";
			splitDetail1.US_ArrivalDate = new ZDateTime(2021, 04, 30);
			var splitDetail2 = houseBill1.ITAndSplitDetails.AddNew();
			splitDetail2.US_CarrierCode = "EF";
			splitDetail2.US_FlightNumber = "QF003";
			splitDetail2.US_ArrivalDate = new ZDateTime(2021, 04, 30);
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = "HB";
			houseBill2.CU_BillNum = "5678";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV";
			invoice.JZ_CU_RelatedHouseBill = houseBill1.PK;
			invoice.US_SplitShipmentDetail = "CD/QF002/30-APR-21";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("JE_DateOfFirstArrival", new ZDateTime(2021, 04, 29), declaration.JE_DateOfFirstArrival);
			CombineAssertions("When splitDetails is not passed in", () =>
			{
				IFTZConveyance conveyanceOject = new FTZMessageConveyanceObjectWrapper(declaration);
				AssertEquals("conveyanceOject.TransportMode", "40", conveyanceOject.TransportMode);
				AssertEquals("conveyanceOject.CarrierSCAC", "AB", conveyanceOject.CarrierSCAC);
				AssertEquals("conveyanceOject.ConveyanceName", "AB NAME", conveyanceOject.ConveyanceName);
				AssertEquals("conveyanceOject.VoyageNumber", "QF001001", conveyanceOject.VoyageNumber);
				AssertEquals("conveyanceOject.ExportDate", new ZDateTime(2021, 04, 28), conveyanceOject.ExportDate);
				AssertEquals("conveyanceOject.ImportDate", new ZDateTime(2021, 04, 29), conveyanceOject.ImportDate);
				AssertEquals("conveyanceOject.PortOfUnlading", "2904", conveyanceOject.PortOfUnlading);
				AssertEquals("conveyanceOject.EstimatedDateOfArrival", new ZDateTime(2021, 04, 29), conveyanceOject.EstimatedDateOfArrival);
				AssertEquals("conveyanceOject.Bills.Count", 1, conveyanceOject.Bills.Count());
				AssertEquals("conveyanceOject.Bills.First().CU_BillNum", "5678", conveyanceOject.Bills.First().CU_BillNum);
			});
			CombineAssertions("When first split detail is passed in", () =>
			{
				IFTZConveyance conveyanceOject = new FTZMessageConveyanceObjectWrapper(declaration, splitDetail1);
				AssertEquals("conveyanceOject.TransportMode", "40", conveyanceOject.TransportMode);
				AssertEquals("conveyanceOject.CarrierSCAC", "CD", conveyanceOject.CarrierSCAC);
				AssertEquals("conveyanceOject.ConveyanceName", "CD TEST", conveyanceOject.ConveyanceName);
				AssertEquals("conveyanceOject.VoyageNumber", "QF002", conveyanceOject.VoyageNumber);
				AssertEquals("conveyanceOject.ExportDate", new ZDateTime(2021, 04, 28), conveyanceOject.ExportDate);
				AssertEquals("conveyanceOject.ImportDate", new ZDateTime(2021, 04, 30), conveyanceOject.ImportDate);
				AssertEquals("conveyanceOject.PortOfUnlading", "2904", conveyanceOject.PortOfUnlading);
				AssertEquals("conveyanceOject.EstimatedDateOfArrival", new ZDateTime(2021, 04, 30), conveyanceOject.EstimatedDateOfArrival);
				AssertEquals("conveyanceOject.Bills.Count", 1, conveyanceOject.Bills.Count());
				AssertEquals("conveyanceOject.Bills.First().CU_BillNum", "1234", conveyanceOject.Bills.First().CU_BillNum);
			});
			CombineAssertions("When second split detail is passed in", () =>
			{
				IFTZConveyance conveyanceOject = new FTZMessageConveyanceObjectWrapper(declaration, splitDetail2);
				AssertEquals("conveyanceOject.TransportMode", "40", conveyanceOject.TransportMode);
				AssertEquals("conveyanceOject.CarrierSCAC", "EF", conveyanceOject.CarrierSCAC);
				AssertEquals("conveyanceOject.ConveyanceName", "EFGHK", conveyanceOject.ConveyanceName);
				AssertEquals("conveyanceOject.VoyageNumber", "QF003", conveyanceOject.VoyageNumber);
				AssertEquals("conveyanceOject.ExportDate", new ZDateTime(2021, 04, 28), conveyanceOject.ExportDate);
				AssertEquals("conveyanceOject.ImportDate", new ZDateTime(2021, 04, 30), conveyanceOject.ImportDate);
				AssertEquals("conveyanceOject.PortOfUnlading", "2904", conveyanceOject.PortOfUnlading);
				AssertEquals("conveyanceOject.EstimatedDateOfArrival", new ZDateTime(2021, 04, 30), conveyanceOject.EstimatedDateOfArrival);
				AssertEquals("conveyanceOject.Bills.Count", 0, conveyanceOject.Bills.Count());
			});
			invoice.US_SplitShipmentDetail = ZString.Empty;
			CombineAssertions("When house bill 1 is passed in", () =>
			{
				IFTZConveyance conveyanceOject = new FTZMessageConveyanceObjectWrapper(declaration, houseBill1);
				AssertEquals("conveyanceOject.TransportMode", "40", conveyanceOject.TransportMode);
				AssertEquals("conveyanceOject.CarrierSCAC", ZString.Empty, conveyanceOject.CarrierSCAC);
				AssertEquals("conveyanceOject.ConveyanceName", ZString.Empty, conveyanceOject.ConveyanceName);
				AssertEquals("conveyanceOject.VoyageNumber", ZString.Empty, conveyanceOject.VoyageNumber);
				AssertEquals("conveyanceOject.ExportDate", new ZDateTime(2021, 04, 28), conveyanceOject.ExportDate);
				AssertEquals("conveyanceOject.ImportDate", ZDate.Empty, conveyanceOject.ImportDate);
				AssertEquals("conveyanceOject.PortOfUnlading", "2904", conveyanceOject.PortOfUnlading);
				AssertEquals("conveyanceOject.EstimatedDateOfArrival", new ZDateTime(2021, 04, 29), conveyanceOject.EstimatedDateOfArrival);
				AssertEquals("conveyanceOject.Bills.Count", 1, conveyanceOject.Bills.Count());
			});
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.Domestic;
			CombineAssertions("When admission type is not 'A'", () =>
			{
				IFTZConveyance conveyanceOject = new FTZMessageConveyanceObjectWrapper(declaration, splitDetail1);
				AssertEquals("conveyanceOject.TransportMode", ZString.Empty, conveyanceOject.TransportMode);
				AssertEquals("conveyanceOject.CarrierSCAC", ZString.Empty, conveyanceOject.CarrierSCAC);
				AssertEquals("conveyanceOject.ConveyanceName", ZString.Empty, conveyanceOject.ConveyanceName);
				AssertEquals("conveyanceOject.VoyageNumber", ZString.Empty, conveyanceOject.VoyageNumber);
				AssertEquals("conveyanceOject.ExportDate", ZDateTime.Empty, conveyanceOject.ExportDate);
				AssertEquals("conveyanceOject.ImportDate", ZDateTime.Empty, conveyanceOject.ImportDate);
				AssertEquals("conveyanceOject.PortOfUnlading", ZString.Empty, conveyanceOject.PortOfUnlading);
				AssertEquals("conveyanceOject.EstimatedDateOfArrival", ZDateTime.Empty, conveyanceOject.EstimatedDateOfArrival);
				AssertEquals("conveyanceOject.Bills.Count", 2, conveyanceOject.Bills.Count());
			});
		}
	}
}
