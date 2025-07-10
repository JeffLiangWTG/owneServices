using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(AMSBrokerDownloadFilterStripBusinessObject))]
	sealed class AMSBrokerDownloadFilterStripBusinessObjectTest : MQEDIMessageCommonFilterStripBusinessObjectTest
	{
		public void TestSCACBillOfLadingNumberQuery()
		{
			var message = Factory.New<AMSBrokerDownloadMQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var billOfLading = Factory.New<IssuerAndBillNumber>();
			billOfLading.CY_Data = "AAAA12584";
			billOfLading.CY_ParentID = message.PK;
			billOfLading.CY_ParentTableCode = EDIMessageSchema.Constants.Prefix;
			var message1 = Factory.New<AMSBrokerDownloadMQEDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var billOfLading1 = Factory.New<IssuerAndBillNumber>();
			billOfLading1.CY_Data = "BBBB12584";
			billOfLading1.CY_ParentID = message1.PK;
			billOfLading1.CY_ParentTableCode = EDIMessageSchema.Constants.Prefix;
			Factory.Save();
			var filter = new AMSBrokerDownloadFilterStripBusinessObject();
			var billFilter = ((ModuleNumberFilter)filter[DeclarationFilterConstants.IssuerScacBol]);
			billFilter.Property = "AAAA12584";
			billFilter.IsActive = true;
			var coll = new AMSBrokerDownloadMQEDIMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(message, coll[0]);
		}

		public void TestActionStatusQuery()
		{
			var message = Factory.New<AMSBrokerDownloadMQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message1 = Factory.New<AMSBrokerDownloadMQEDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.SetToComplete();
			Factory.Save();
			var filter = new AMSBrokerDownloadFilterStripBusinessObject();
			var statusFilter = ((ModuleTextFilter)filter[DeclarationFilterConstants.ActionStatus]);
			statusFilter.Property = EM_ActionStatusList.Codes.Complete;
			statusFilter.IsActive = true;
			var coll = new AMSBrokerDownloadMQEDIMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(message1, coll[0]);
			statusFilter.Property = EM_ActionStatusList.Codes.Incomplete;
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(message, coll[0]);
			statusFilter.IsActive = false;
			coll.Load(filter.Filter);
			AssertEquals(2, coll.Count);
			AssertCollectionContains(message, coll);
			AssertCollectionContains(message1, coll);
		}

		protected override MQEDIMessage GetNewMessageWithSpecificBranchToTest(MasterFiles.Business.GlbBranch branch)
		{
			var result = Factory.New<AMSBrokerDownloadMQEDIMessage>();
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_GB = branch.PK;
			return result;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AMSBrokerDownloadFilterStripBusinessObject();
	}
}
