using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business.UniversalData;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class RelatedJobMatcherTest : TestCaseWithFactory
	{
		public void TestJobMatching()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MasterBill = "TEST_MB_1";
			var importer = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			importer.OH_Code = "OMRELEJMH";
			declaration.JE_OH_Importer = importer.PK;
			var supplier = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			supplier.OH_Code = "OMRONKYO";
			declaration.JE_OH_Supplier = supplier.PK;
			Factory.Save();
			var matchingCriteria = new Dictionary<RelatedJobMatcherKeys, ZString>();
			matchingCriteria.Add(RelatedJobMatcherKeys.BillOfLading, "TEST_MB_1");
			matchingCriteria.Add(RelatedJobMatcherKeys.Consignee, importer.PK.ToString());
			matchingCriteria.Add(RelatedJobMatcherKeys.Consignor, supplier.PK.ToString());
			matchingCriteria.Add(RelatedJobMatcherKeys.ITN, "");
			var matcher = RelatedBrokerageJobMatcher.New();
			AssertEquals(declaration.PK, matcher.GetMatchingJob(matchingCriteria).PK);
			matchingCriteria.Add(RelatedJobMatcherKeys.BookingRefNumber, "333");
			AssertEquals("No declaration with Transport Reference '333', fallback to base matching criteria", declaration.PK, matcher.GetMatchingJob(matchingCriteria).PK);
			declaration.JE_MasterBill = ZString.Empty;
			Factory.Save();
			AssertEquals("No declaration with Transport Reference '333' and do not match base matching criteria", null, matcher.GetMatchingJob(matchingCriteria));
			declaration.US_TransportReference = "333";
			Factory.Save();
			AssertEquals(declaration.PK, matcher.GetMatchingJob(matchingCriteria).PK);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration2.ActiveEntryHeaders[0].EntryNumber = "000005";
			Factory.Save();
			matchingCriteria.Remove(RelatedJobMatcherKeys.BookingRefNumber);
			matchingCriteria[RelatedJobMatcherKeys.ITN] = "000005";
			AssertEquals(declaration2.PK, matcher.GetMatchingJob(matchingCriteria).PK);
		}
	}
}
