using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Registry;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing.ServiceTask
{
	[TestedType(typeof(DtbConsignmentActionPickedUpDeliveredLogSubscriber))]
	class DtbConsignmentActionPickedUpDeliveredLogSubscriberTest : LogSubscriberTest<DtbConsignmentActionPickedUpDeliveredLogSubscriber>
	{
		public void TestProcessLogQueueItems_WhenLogAddedToAction_DtbConsignmentActionActualTimeUpdatorShouldBeCalledOnce_WithCorrectParameter()
		{
			ObjectFactory.Substitute(mockDtbConsignmentActionActualTimeUpdater.Object);
			var consignment = transportConsignmentTestHelper.CreateConsignment();
			var address = transportConsignmentTestHelper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = transportConsignmentTestHelper.CreateConsignmentAction(address, InstructionTypes.Codes.PickUp);
			var log = action.Logs.AddNew(AutoEvents.PickedUp);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			mockDtbConsignmentActionActualTimeUpdater.Verify(x => x.ProcessLog(It.IsAny<BusinessObjectFactory>(), action.PK), Times.Once());
			Assert(true);
		}

		public void TestProcessLogQueueItems_WhenMultiLogsAddedToAction_DtbConsignmentActionActualTimeUpdatorShouldBeCalledMultiTimes_WithCorrectParameter()
		{
			ObjectFactory.Substitute(mockDtbConsignmentActionActualTimeUpdater.Object);
			var consignment = transportConsignmentTestHelper.CreateConsignment();
			var address = transportConsignmentTestHelper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var picAction = transportConsignmentTestHelper.CreateConsignmentAction(address, InstructionTypes.Codes.PickUp);
			var dlvAction = transportConsignmentTestHelper.CreateConsignmentAction(address, InstructionTypes.Codes.Delivery);
			var picLog = picAction.Logs.AddNew(AutoEvents.PickedUp);
			var dlvLog = dlvAction.Logs.AddNew(AutoEvents.Delivered);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			mockDtbConsignmentActionActualTimeUpdater.Verify(x => x.ProcessLog(It.IsAny<BusinessObjectFactory>(), picAction.PK), Times.Once());
			mockDtbConsignmentActionActualTimeUpdater.Verify(x => x.ProcessLog(It.IsAny<BusinessObjectFactory>(), dlvAction.PK), Times.Once());
			Assert(true);
		}

		public void TestProcessLogQueueItems_WhenLogSubscriberDisabled_DtbConsignmentActionActualTimeUpdatorShouldNotBeCalled()
		{
			ObjectFactory.Substitute(mockDtbConsignmentActionActualTimeUpdater.Object);
			LandTransportRegistry.Instance.DisableDtbConsignmentActionPickedUpDeliveredLogSubscriber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consignment = transportConsignmentTestHelper.CreateConsignment();
			var address = transportConsignmentTestHelper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var picAction = transportConsignmentTestHelper.CreateConsignmentAction(address, InstructionTypes.Codes.PickUp);
			var dlvAction = transportConsignmentTestHelper.CreateConsignmentAction(address, InstructionTypes.Codes.Delivery);
			var picLog = picAction.Logs.AddNew(AutoEvents.PickedUp);
			var dlvLog = dlvAction.Logs.AddNew(AutoEvents.Delivered);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			mockDtbConsignmentActionActualTimeUpdater.Verify(x => x.ProcessLog(It.IsAny<BusinessObjectFactory>(), picAction.PK), Times.Never());
			mockDtbConsignmentActionActualTimeUpdater.Verify(x => x.ProcessLog(It.IsAny<BusinessObjectFactory>(), dlvAction.PK), Times.Never());
			Assert(true);
		}

		readonly Mock<IDtbConsignmentActionPickedUpDeliveredUpdater> mockDtbConsignmentActionActualTimeUpdater = new Mock<IDtbConsignmentActionPickedUpDeliveredUpdater>();
		TransportConsignmentTestHelper transportConsignmentTestHelper;

		protected override void SetUp()
		{
			base.SetUp();
			transportConsignmentTestHelper = new TransportConsignmentTestHelper(Factory);
		}
	}
}
