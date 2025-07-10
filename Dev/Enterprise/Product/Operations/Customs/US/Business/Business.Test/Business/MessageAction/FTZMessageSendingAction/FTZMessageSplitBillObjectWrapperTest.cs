using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZMessageSplitBillObjectWrapperTest : TestCaseWithFactory
	{
		public void TestIFTZBillMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_MasterBill = "MB290421";
			declaration.JE_HouseBill = "HB124092";
			var houseBill = declaration.Bills.PrimaryHouseBill;
			houseBill.US_SESplitShip = true;
			houseBill.US_UC_NKCountryOfExport = "AU";
			houseBill.US_SchDLoading = "52001";
			houseBill.CU_NoOfPacks = 100m;
			houseBill.CU_PackType = "KG";
			houseBill.US_US_NKLocationOfGoods = "S001";
			var splitDetail1 = houseBill.ITAndSplitDetails.AddNew();
			splitDetail1.US_ITNumber = "V12456789";
			splitDetail1.US_NoOfPacks = 10;
			splitDetail1.US_CarrierCode = "A2";
			splitDetail1.US_FlightNumber = "QF001";
			splitDetail1.US_ArrivalDate = new ZDateTime(2021, 04, 30);
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice1.US_SplitShipmentDetail = "A2/QF001/30-APR-21";
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "4801042021";
			invoiceLine1.JI_LinePrice = 1000m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JZ_CU_RelatedHouseBill = houseBill.PK;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2904016920";
			invoiceLine2.JI_LinePrice = 1000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, declaration.FTZEntry.EntryLines.Count());
			var splitBillObject = (IFTZBill)new FTZMessageSplitBillObjectWrapper(splitDetail1);
			CombineAssertions(() =>
			{
				AssertEquals("splitBillObject.BillOfLading", "MB290421", splitBillObject.BillOfLading);
				AssertEquals("splitBillObject.HouseBill", "HB124092", splitBillObject.HouseBill);
				AssertEquals("splitBillObject.CountryOfExport", "AU", splitBillObject.CountryOfExport);
				AssertEquals("splitBillObject.ForeignLoadPort", "52001", splitBillObject.ForeignLoadPort);
				AssertEquals("splitBillObject.Lines.Count()", 1, splitBillObject.Lines.Count());
				AssertEquals("splitBillObject.Lines.First().Tariff", "4801042021", splitBillObject.Lines.First().Tariff);
				AssertEquals("splitBillObject.Quantity", 10m, splitBillObject.Quantity);
				AssertEquals("splitBillObject.FIRMSCode", "S001", splitBillObject.FIRMSCode);
				AssertEquals("splitBillObject.ITNumbers.Count()", 1, splitBillObject.ITNumbers.Count());
				AssertEquals("splitBillObject.ITNumbers.First().Tariff", "V12456789", splitBillObject.ITNumbers.First().ITNumber);
				AssertEquals("splitBillObject.IRSIdentifier", ZString.Empty, splitBillObject.IRSIdentifier);
				AssertEquals("splitBillObject.Containers.Count()", 0, splitBillObject.Containers.Count());
				AssertEquals("splitBillObject.CU_BillNum", "HB124092", splitBillObject.CU_BillNum);
				AssertEquals("splitBillObject.CU_PackType", "KG", splitBillObject.CU_PackType);
				AssertEquals("splitBillObject.CU_NoOfPacks", 10m, splitBillObject.CU_NoOfPacks);
				AssertEquals("splitBillObject.ITAndSplitDetails.Count()", 1, splitBillObject.ITAndSplitDetails.Count());
				AssertEquals("splitBillObject.FTZConcurrenceQty", 0m, splitBillObject.FTZConcurrenceQty);
			});
			splitBillObject = new FTZMessageSplitBillObjectWrapper(houseBill);
			CombineAssertions(() =>
			{
				AssertEquals("splitBillObject.BillOfLading", "MB290421", splitBillObject.BillOfLading);
				AssertEquals("splitBillObject.HouseBill", "HB124092", splitBillObject.HouseBill);
				AssertEquals("splitBillObject.CountryOfExport", "AU", splitBillObject.CountryOfExport);
				AssertEquals("splitBillObject.ForeignLoadPort", "52001", splitBillObject.ForeignLoadPort);
				AssertEquals("splitBillObject.Lines.Count()", 1, splitBillObject.Lines.Count());
				AssertEquals("splitBillObject.Lines.First().Tariff", "2904016920", splitBillObject.Lines.First().Tariff);
				AssertEquals("splitBillObject.Quantity", 100m, splitBillObject.Quantity);
				AssertEquals("splitBillObject.FIRMSCode", "S001", splitBillObject.FIRMSCode);
				AssertEquals("splitBillObject.ITNumbers.Count()", 1, splitBillObject.ITNumbers.Count());
				AssertEquals("splitBillObject.ITNumbers.First().Tariff", "V12456789", splitBillObject.ITNumbers.First().ITNumber);
				AssertEquals("splitBillObject.IRSIdentifier", ZString.Empty, splitBillObject.IRSIdentifier);
				AssertEquals("splitBillObject.Containers.Count()", 0, splitBillObject.Containers.Count());
				AssertEquals("splitBillObject.CU_BillNum", "HB124092", splitBillObject.CU_BillNum);
				AssertEquals("splitBillObject.CU_PackType", "KG", splitBillObject.CU_PackType);
				AssertEquals("splitBillObject.CU_NoOfPacks", 100m, splitBillObject.CU_NoOfPacks);
				AssertEquals("splitBillObject.ITAndSplitDetails.Count()", 1, splitBillObject.ITAndSplitDetails.Count());
				AssertEquals("splitBillObject.FTZConcurrenceQty", 0m, splitBillObject.FTZConcurrenceQty);
			});
		}
	}
}
