using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(BorderLineReleaseMessageFilterStripBusinessObject))]
	sealed class BorderLineReleaseMessageFilterStripBusinessObjectTest : MQEDIMessageCommonFilterStripBusinessObjectTest
	{
		public void TestActionStatusQuery()
		{
			var message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			var message1 = Factory.New<LineReleaseMQEDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message1.SetToComplete();
			Factory.Save();
			var filter = new BorderLineReleaseMessageFilterStripBusinessObject();
			var statusFilter = ((ModuleTextFilter)filter[DeclarationFilterConstants.ActionStatus]);
			statusFilter.Property = EM_ActionStatusList.Codes.Complete;
			statusFilter.IsActive = true;
			var coll = new BorderLineReleaseMessageCollection(Factory);
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

		public void TestImporterEINQuery()
		{
			var message1 = Factory.New<LineReleaseMQEDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message1.US_ImporterNumber = "123456";
			var message2 = Factory.New<LineReleaseMQEDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message2.US_ImporterNumber = "654321";
			Factory.Save();
			var filter = new BorderLineReleaseMessageFilterStripBusinessObject();
			var einFilter = ((ModuleTextFilter)filter["Importer EIN #"]);
			einFilter.Property = "654321";
			einFilter.IsActive = true;
			var coll = new BorderLineReleaseMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(message2.PK, coll[0].PK);
			einFilter.IsActive = false;
			coll.Load(filter.Filter);
			AssertEquals(2, coll.Count);
			AssertCollectionContains(message1, coll);
			AssertCollectionContains(message2, coll);
		}

		public void TestImporterSupplierQuery()
		{
			var message1 = Factory.New<LineReleaseMQEDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message1.US_ImporterNumber = "123456";
			message1.US_SupplierCode = "456789";
			var message2 = Factory.New<LineReleaseMQEDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message2.US_ImporterNumber = "654321";
			message2.US_SupplierCode = "987654";
			var message3 = Factory.New<LineReleaseMQEDIMessage>();
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message3.US_ImporterNumber = "123456";
			message3.US_SupplierCode = "987654";
			var message4 = Factory.New<LineReleaseMQEDIMessage>();
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message4.US_ImporterNumber = "654321";
			message4.US_SupplierCode = "456789";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew("EIN", "123456");
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew("MID", "456789");
			Factory.Save();
			var filter = new BorderLineReleaseMessageFilterStripBusinessObject();
			var moduleGuidsFilter = ((ModuleGuidsFilter)filter["Importer/Supplier"]);
			moduleGuidsFilter.Property1 = org1.PK;
			moduleGuidsFilter.Property2 = org2.PK;
			moduleGuidsFilter.IsActive = true;
			var coll = new BorderLineReleaseMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(message1.PK, coll[0].PK);
			moduleGuidsFilter.IsActive = false;
			coll.Load(filter.Filter);
			AssertEquals(4, coll.Count);
			AssertCollectionContains(message1, coll);
			AssertCollectionContains(message2, coll);
			AssertCollectionContains(message3, coll);
			AssertCollectionContains(message4, coll);
		}

		public void TestIssuerSCACBillOfLadingQuery()
		{
			var message1 = Factory.New<LineReleaseMQEDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message1.US_BillOfLading = "123456";
			var message2 = Factory.New<LineReleaseMQEDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message2.US_BillOfLading = "654321";
			Factory.Save();
			var filter = new BorderLineReleaseMessageFilterStripBusinessObject();
			var bolFilter = ((ModuleTextFilter)filter[DeclarationFilterConstants.IssuerScacBol]);
			bolFilter.Property = "654321";
			bolFilter.IsActive = true;
			var coll = new BorderLineReleaseMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(message2.PK, coll[0].PK);
			bolFilter.IsActive = false;
			coll.Load(filter.Filter);
			AssertEquals(2, coll.Count);
			AssertCollectionContains(message1, coll);
			AssertCollectionContains(message2, coll);
		}

		public void TestMessageTimeQuery()
		{
			var message1 = Factory.New<LineReleaseMQEDIMessage>();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message1.US_ReleaseDateTime = new ZDateTime(2018, 07, 01);
			var message2 = Factory.New<LineReleaseMQEDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message2.US_ReleaseDateTime = new ZDateTime(2017, 07, 01);
			Factory.Save();
			var filter = new BorderLineReleaseMessageFilterStripBusinessObject();
			var releaseTimeFilter = ((ModuleDateFilter)filter["Release Date"]);
			releaseTimeFilter.Property1 = new ZDateTime(2018, 01, 01);
			releaseTimeFilter.Property2 = new ZDateTime(2018, 12, 31);
			releaseTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			releaseTimeFilter.IsActive = true;
			var coll = new BorderLineReleaseMessageCollection(Factory);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(message1.PK, coll[0].PK);
			releaseTimeFilter.IsActive = false;
			coll.Load(filter.Filter);
			AssertEquals(2, coll.Count);
			AssertCollectionContains(message1, coll);
			AssertCollectionContains(message2, coll);
		}

		protected override MQEDIMessage GetNewMessageWithSpecificBranchToTest(GlbBranch branch)
		{
			var result = Factory.New<LineReleaseMQEDIMessage>();
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_GB = branch.PK;
			return result;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new BorderLineReleaseMessageFilterStripBusinessObject();
	}
}
