using System;
using System.Linq;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class RefDataSetUpdaterManagerWrapperTest : TransactionedTestCase
	{
		public void TestGetAllUpdaters()
		{
			var mapper = new Mock<IDataSetUpdaterMapper>();

			mapper.Setup(x => x.GetDataSets()).Returns(new[]
			{
				new SharedDataSetUpdater("Updater1", new[] { "Updater1_DataSetVersion1", "Updater1_DataSetVersion2" }),
				new SharedDataSetUpdater("Updater2", new[] { "Updater2_DataSetVersion1" })
			});

			var wrapper = new RefDataSetUpdaterManagerWrapperForTest(mapper.Object);
			var dataSetUpdaters = wrapper.GetAllDataSetUpdater();

			Assert("Updater count do not match", dataSetUpdaters.Count() == 2);
		}

		public void TestUpdate()
		{
			var mapper = new Mock<IDataSetUpdaterMapper>();
			var manager = new Mock<IDataSetUpdaterManager>();
			var updater = new Mock<IDataSetUpdater>();
			var dataSetVersion = new DataSetVersion("Updater1_DataSetVersion2", DateTime.Now);

			mapper.Setup(x => x.GetDataSetUpdater("Updater1")).Returns(updater.Object);
			mapper.Setup(x => x.GetDataSetUpdaterManager("Updater1")).Returns(manager.Object);
			mapper.Setup(x => x.GetDataSetVersion("Updater1", "Updater1_DataSetVersion2")).Returns(dataSetVersion);

			var wrapper = new RefDataSetUpdaterManagerWrapperForTest(mapper.Object);
			wrapper.Update("Updater1", "Updater1_DataSetVersion2");

			manager.Verify(x => x.Update(dataSetVersion, updater.Object), Times.Once);
			Assert(true);
		}

		public void TestUpdateAll()
		{
			var mapper = new Mock<IDataSetUpdaterMapper>();
			var wrapper = new RefDataSetUpdaterManagerWrapperForTest(mapper.Object);

			AssertExceptionThrown<NotImplementedException>(
				"GetAllDataSetUpdater Method should not be called from ServiceTaskDataSetUpdateManagerWrapper",
				() => wrapper.UpdateAll());
		}
	}

	public class RefDataSetUpdaterManagerWrapperForTest : RefDataSetUpdaterManagerWrapper
	{
		public RefDataSetUpdaterManagerWrapperForTest(IDataSetUpdaterMapper dataSetUpdaterMapper)
		{
			base.dataSetUpdaterMapper = dataSetUpdaterMapper;
		}

		protected override bool preConditionCheck => true;
	}
}
