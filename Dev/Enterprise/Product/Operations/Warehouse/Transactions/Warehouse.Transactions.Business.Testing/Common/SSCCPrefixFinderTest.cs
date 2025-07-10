using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class SSCCPrefixFinderTest : WhsTestCaseWithFactory
	{
		public void TestObjectFactoryRegistration()
		{
			AssertType<SSCCPrefixFinder>(ObjectFactory.Get<ISSCCPrefixFinder>());
		}

		#region GetSSCCPrefix

		public void TestGetSSCCPrefix()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var client = Factory.New<OrgHeader>();

			var ssccPrefixFinder = ObjectFactory.Get<ISSCCPrefixFinder>();
			receive.WD_OH_Client = client.PK;
			client.MiscServ.OM_WhsGenerateSSCCOnInbound = true;

			client.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1234567");
			AssertEquals("1234567", ssccPrefixFinder.GetSSCCPrefix(SSCCGenerationContext.CheckIfBarcodeIsSSCC, () => client, () => null, receive.NotificationSubscriber, shouldPrompt: false));
		}

		public void TestGetSSCCPrefix_NullNotifications_ShouldError()
		{
			var ssccPrefixFinder = ObjectFactory.Get<ISSCCPrefixFinder>();

			AssertExceptionThrown<ArgumentNullException>(() => ssccPrefixFinder.GetSSCCPrefix(SSCCGenerationContext.CheckIfBarcodeIsSSCC, () => null, () => null, null, shouldPrompt: false));
		}

		public void TestGetSSCCPrefix_NullClientGetter_ShouldError()
		{
			var ssccPrefixFinder = ObjectFactory.Get<ISSCCPrefixFinder>();

			AssertExceptionThrown<ArgumentNullException>(() => ssccPrefixFinder.GetSSCCPrefix(SSCCGenerationContext.CheckIfBarcodeIsSSCC, null, () => null, new TestNotificationBuffer(), shouldPrompt: false));
		}

		public void TestGetSSCCPrefix_NullWarehouseGetter_ShouldError()
		{
			var ssccPrefixFinder = ObjectFactory.Get<ISSCCPrefixFinder>();

			AssertExceptionThrown<ArgumentNullException>(() => ssccPrefixFinder.GetSSCCPrefix(SSCCGenerationContext.CheckIfBarcodeIsSSCC, () => null, null, new TestNotificationBuffer(), shouldPrompt: false));
		}

		public void TestGetSSCCPrefix_WhsOrder()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var client = Factory.New<OrgHeader>();
			var ssccPrefixFinder = ObjectFactory.Get<ISSCCPrefixFinder>();

			order.WD_OH_Client = client.PK;

			client.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1234567");
			AssertEquals("1234567", ssccPrefixFinder.GetSSCCPrefix(SSCCGenerationContext.CheckIfBarcodeIsSSCC, () => client, () => null, order.NotificationSubscriber, shouldPrompt: false));
		}

		public void TestGetSSCCPrefix_WhsReceive()
		{
			var receive = Factory.NewWithValidTestData<WhsReceive>();
			var client = Factory.New<OrgHeader>();
			var ssccPrefixFinder = ObjectFactory.Get<ISSCCPrefixFinder>();

			receive.WD_OH_Client = client.PK;

			client.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1234567");
			AssertEquals("1234567", ssccPrefixFinder.GetSSCCPrefix(SSCCGenerationContext.CheckIfBarcodeIsSSCC, () => client, () => null, receive.NotificationSubscriber, shouldPrompt: false));
		}

		#endregion
	}
}
