using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentActionPickedUpDeliveredUpdater))]
	class DtbConsignmentActionPickedUpDeliveredUpdaterTest : TestCaseWithFactory
	{
		public void TestProcessLogs_WhenActionNotFound_ShouldNotCallAddPickupDeliveryEvents()
		{
			AssertNoExceptionThrown(() =>
			{
				using (Factory.AddDisposableService())
				{
					DtbConsignmentAction action;
					PkgPackage package1, package2;
					Arrange(out action, out package1, out package2);
					action.LTA_ActualTime = DateTime.Now;

					dtbConsignmentActionPickedUpDeliveredUpdater.ProcessLog(action.Factory, new ZGuid("5fe4891c-0ef0-42a9-ba00-2b2536384097"));

					AssertEquals("Package 1 should have 0 log.", 0, package1.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "PUP").Count());
					AssertEquals("Package 2 should have 0 log.", 0, package2.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "PUP").Count());
					AssertEDIMessages(expectedMessagesCount: 0);
				}
			});
		}

		public void TestProcessLogs_WhenActualTimeIsSet_AndNoPackageDivot_EachPackageShouldCallAddPickupDeliveryEvents()
		{
			AssertNoExceptionThrown(() =>
			{
				using (Factory.AddDisposableService())
				{
					DtbConsignmentAction action;
					PkgPackage package1, package2;
					Arrange(out action, out package1, out package2);

					action.LTA_ActualTime = DateTime.Now;

					dtbConsignmentActionPickedUpDeliveredUpdater.ProcessLog(action.Factory, action.PK);
					action.Factory.Save();

					AssertEquals("Package 1 should have 1 log.", 1, package1.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "PUP").Count());
					AssertEquals("Package 2 should have 1 log.", 1, package2.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "PUP").Count());

					AssertEDIMessages(expectedMessagesCount: 2);
				}
			});
		}

		public void TestProcessLogs_WhenActualTimeIsNotSet_ShouldNotCallAddPickupDeliveryEvents()
		{
			AssertNoExceptionThrown(() =>
			{
				using (Factory.AddDisposableService())
				{
					DtbConsignmentAction action;
					PkgPackage package1, package2;
					Arrange(out action, out package1, out package2);

					action.LTA_ActualTime = ZDateTimeOffset.Empty;

					dtbConsignmentActionPickedUpDeliveredUpdater.ProcessLog(action.Factory, action.PK);

					AssertEquals("Package 1 should have 0 log.", 0, package1.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "PUP").Count());
					AssertEquals("Package 2 should have 0 log.", 0, package2.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "PUP").Count());
					AssertEDIMessages(expectedMessagesCount: 0);
				}
			});
		}

		public void TestProcessLogs_WhenActualTimeIsSetActionsInstructionIsDepot_ShouldNotCallAddPickupDeliveryEvents()
		{
			AssertNoExceptionThrown(() =>
			{
				using (Factory.AddDisposableService())
				{
					DtbConsignmentAction action;
					PkgPackage package1, package2;
					Arrange(out action, out package1, out package2);

					action.LTA_ActualTime = DateTime.Now;
					action.ConsignmentAddress.LTS_InstructionType = ConsignmentAddressTypes.Codes.Multi;

					dtbConsignmentActionPickedUpDeliveredUpdater.ProcessLog(action.Factory, action.PK);

					AssertEquals("Package 1 should have 0 log.", 0, package1.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "PUP").Count());
					AssertEquals("Package 2 should have 0 log.", 0, package2.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "PUP").Count());
					AssertEDIMessages(expectedMessagesCount: 0);
				}
			});
		}

		void Arrange(out DtbConsignmentAction action, out PkgPackage package1, out PkgPackage package2)
		{
			var consignment = transportConsignmentTestHelper.CreateConsignment();
			var address = transportConsignmentTestHelper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			action = transportConsignmentTestHelper.CreateConsignmentAction(address, InstructionTypes.Codes.PickUp);
			package1 = transportConsignmentTestHelper.CreatePackage(consignment, "P1", Constants.PkgUnit.Box);
			package2 = transportConsignmentTestHelper.CreatePackage(consignment, "P2", Constants.PkgUnit.Case);
			transportConsignmentTestHelper.CreatePackageDivot(action, package1);
			transportConsignmentTestHelper.CreatePackageDivot(action, package2);
		}

		void AssertEDIMessages(int expectedMessagesCount)
		{
			var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", expectedMessagesCount, messages.Length);

			for (int i = 0; i < expectedMessagesCount; i++)
			{
				AssertEquals("message.EM_MessageType", "XDC", messages[i].EM_MessageType);
				AssertEquals("message.EM_MessageSubType", "XUE", messages[i].EM_MessageSubType);
			}
		}

		TransportConsignmentTestHelper transportConsignmentTestHelper;
		IDtbConsignmentActionPickedUpDeliveredUpdater dtbConsignmentActionPickedUpDeliveredUpdater;

		protected override void SetUp()
		{
			base.SetUp();
			transportConsignmentTestHelper = new TransportConsignmentTestHelper(Factory);
			dtbConsignmentActionPickedUpDeliveredUpdater = new DtbConsignmentActionPickedUpDeliveredUpdater();
		}
	}
}
