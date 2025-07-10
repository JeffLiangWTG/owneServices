using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class CommercialInvoiceFilterLookupsTest : Customs.Module.Testing.CommercialInvoiceFilterLookupsTest
	{
		public override void TestMessageTypesList()
		{
			var filterBizObj = new CommercialInvoiceFilterBusinessObject();
			var lookups = new CommercialInvoiceFilterLookups(filterBizObj);
			AssertEquals("Message Types", typeof(USJobMessageTypeList), lookups.MessageTypes.GetType());
			AssertEquals(false, lookups.MessageTypes.ContainsCode(Enterprise.Customs.Business.JobMessageTypeList.Codes.ExportDeclarationByExternalBroker));
			AssertEquals(true, lookups.MessageTypes.ContainsCode(Enterprise.Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker));
			AssertEquals(false, lookups.MessageTypes.ContainsCode(Enterprise.Customs.Business.JobMessageTypeList.Codes.ExWarehouse));
			AssertEquals(false, lookups.MessageTypes.ContainsCode(Enterprise.Customs.Business.JobMessageTypeList.Codes.Refund));
			AssertEquals(USJobMessageTypeList.Descriptions.Export, lookups.MessageTypes.GetDescriptionFromCode(JobMessageTypeList.Codes.Export));
			AssertEquals(USJobMessageTypeList.Descriptions.Import, lookups.MessageTypes.GetDescriptionFromCode(JobMessageTypeList.Codes.Import));
			AssertEquals(USJobMessageTypeList.Descriptions.ImportByExternalBroker, lookups.MessageTypes.GetDescriptionFromCode(JobMessageTypeList.Codes.ImportByExternalBroker));
			AssertEquals(USJobMessageTypeList.Descriptions.Miscellaneous, lookups.MessageTypes.GetDescriptionFromCode(JobMessageTypeList.Codes.Miscellaneous));
			AssertEquals(Common.Shared.SharedJobMessageTypeList.MoreDescriptions.AdvanceShippingNotice, lookups.MessageTypes.GetDescriptionFromCode(JobMessageTypeList.MoreCodes.AdvanceShippingNotice));
			var filterBizObj2 = new CommercialInvoiceFilterBusinessObject();
			var lookups2 = new CommercialInvoiceFilterLookups(filterBizObj2);
			AssertEquals("CommercialInvoiceFilterLookups.MessageTypes should be cached", lookups.MessageTypes, lookups2.MessageTypes);
		}
	}
}
