using System;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.Shared;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class NudgeUpdaterManagerWrapperTest : TransactionedTestCase
	{
		public void TestGetAllUpdaters()
		{
			var refWrapper = new Mock<IRefDataSetUpdaterWrapper>();
			var sRDbWrapper = new Mock<ISRDbDataSetUpdaterWrapper>();
			var wrapper = new NudgeUpdaterManagerWrapperForTest(refWrapper.Object, sRDbWrapper.Object);

			AssertExceptionThrown<NotImplementedException>(
				"GetAllDataSetUpdater Method should not be called from NudgeUpdaterManagerWrapper",
				() => wrapper.GetAllDataSetUpdater());
		}

		public void TestUpdate()
		{
			var nudgeMapper = new Mock<IDataSetUpdaterMapper>();
			var refWrapper = new Mock<IRefDataSetUpdaterWrapper>();
			var sRDbWrapper = new Mock<ISRDbDataSetUpdaterWrapper>();

			var refDataSetUpdater = new Mock<ISharedDataSetUpdater>();
			refDataSetUpdater.SetupGet(x => x.Name).Returns("REF-Updater1");
			var sRDbDataSetUpdater = new Mock<ISharedDataSetUpdater>();
			sRDbDataSetUpdater.SetupGet(x => x.Name).Returns("RDU-Updater2");

			refWrapper.Setup(x => x.GetAllDataSetUpdater()).Returns(new[] { refDataSetUpdater.Object });
			sRDbWrapper.Setup(x => x.GetAllDataSetUpdater()).Returns(new[] { sRDbDataSetUpdater.Object });

			var wrapper = new NudgeUpdaterManagerWrapperForTest(refWrapper.Object, sRDbWrapper.Object);
			wrapper.Update("Updater1", "ds1");
			wrapper.Update("Updater2", "ds2");

			refWrapper.Verify(x => x.Update("Updater1", "ds1"), Times.Once);
			sRDbWrapper.Verify(x => x.Update("Updater2", "ds2"), Times.Once);
			Assert(true);
		}

		public void TestUpdateAll()
		{
			var refWrapper = new Mock<IRefDataSetUpdaterWrapper>();
			var sRDbWrapper = new Mock<ISRDbDataSetUpdaterWrapper>();
			var wrapper = new NudgeUpdaterManagerWrapperForTest(refWrapper.Object, sRDbWrapper.Object);

			AssertExceptionThrown<NotImplementedException>(
				"GetAllDataSetUpdater Method should not be called from ServiceTaskDataSetUpdateManagerWrapper",
				() => wrapper.UpdateAll());
		}

		public void TestIsSchemaUpgradeSuccessful()
		{
			var refWrapper = new Mock<IRefDataSetUpdaterWrapper>();
			var sRDbWrapper = new Mock<ISRDbDataSetUpdaterWrapper>();
			var wrapper = new NudgeUpdaterManagerWrapperForTest(refWrapper.Object, sRDbWrapper.Object);

			AssertExceptionThrown<NotImplementedException>(
				"IsSchemaUpgradeSuccessful Method should not be called from NudgeUpdaterManagerWrapper",
				() => wrapper.UpdateAll());
		}
	}

	public class NudgeUpdaterManagerWrapperForTest : NudgeUpdaterManagerWrapper
	{
		public NudgeUpdaterManagerWrapperForTest(IRefDataSetUpdaterWrapper refDataSetUpdaterWrapper, ISRDbDataSetUpdaterWrapper sRDbDataSetUpdaterWrapper)
		{
			base.refDataSetUpdaterWrapper = refDataSetUpdaterWrapper;
			base.sRDbDataSetUpdaterWrapper = sRDbDataSetUpdaterWrapper;
		}
	}
}
