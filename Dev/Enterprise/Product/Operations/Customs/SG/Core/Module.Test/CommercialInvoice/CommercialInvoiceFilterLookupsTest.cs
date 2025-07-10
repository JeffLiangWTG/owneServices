using Enterprise.Customs.Business;
using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	sealed class CommercialInvoiceFilterLookupsTest : Customs.Module.Testing.CommercialInvoiceFilterLookupsTest
	{
		public override void TestMessageTypesList()
		{
			var filterBizObj = new CommercialInvoiceFilterBusinessObject();
			var lookups = new CommercialInvoiceFilterLookups(filterBizObj);
			AssertEquals("Message Types", typeof(MessageTypeCodeList), lookups.MessageTypes.GetType());
			AssertEquals(true, lookups.MessageTypes.ContainsCode(MessageTypeCodeList.Codes.COO));
			AssertEquals(MessageTypeCodeList.Descriptions.COO, lookups.MessageTypes.GetDescriptionFromCode(MessageTypeCodeList.Codes.COO));
			AssertEquals(true, lookups.MessageTypes.ContainsCode(MessageTypeCodeList.Codes.INP));
			AssertEquals(MessageTypeCodeList.Descriptions.INP, lookups.MessageTypes.GetDescriptionFromCode(MessageTypeCodeList.Codes.INP));
			AssertEquals(true, lookups.MessageTypes.ContainsCode(MessageTypeCodeList.Codes.IPT));
			AssertEquals(MessageTypeCodeList.Descriptions.IPT, lookups.MessageTypes.GetDescriptionFromCode(MessageTypeCodeList.Codes.IPT));
			AssertEquals(true, lookups.MessageTypes.ContainsCode(MessageTypeCodeList.Codes.OUT));
			AssertEquals(MessageTypeCodeList.Descriptions.OUT, lookups.MessageTypes.GetDescriptionFromCode(MessageTypeCodeList.Codes.OUT));
			AssertEquals(true, lookups.MessageTypes.ContainsCode(MessageTypeCodeList.Codes.TNP));
			AssertEquals(MessageTypeCodeList.Descriptions.TNP, lookups.MessageTypes.GetDescriptionFromCode(MessageTypeCodeList.Codes.TNP));
			AssertEquals(true, lookups.MessageTypes.ContainsCode(JobMessageTypeList.MoreCodes.AdvanceShippingNotice));
			AssertEquals(JobMessageTypeList.MoreDescriptions.AdvanceShippingNotice, lookups.MessageTypes.GetDescriptionFromCode(JobMessageTypeList.MoreCodes.AdvanceShippingNotice));
			var filterBizObj2 = new CommercialInvoiceFilterBusinessObject();
			var lookups2 = new CommercialInvoiceFilterLookups(filterBizObj2);
			AssertEquals("CommercialInvoiceFilterLookups.MessageTypes should be cached", lookups.MessageTypes, lookups2.MessageTypes);
		}
	}
}
