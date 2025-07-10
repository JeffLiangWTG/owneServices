using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class DataSetUpdaterRunnerTest : TransactionedTestCase
	{
		public void TestObtainLockNotSuccessful()
		{
			var updater = new Mock<IDataSetUpdater>();
			var logger = new Mock<ILogger>();
			updater.Setup(x => x.TryToGetDataSetLock(It.IsAny<string>())).Returns((false, null));
			var runner = new DataSetUpdaterAsyncRunnerWithExceptionHandling(updater.Object, logger.Object);
			Task.Run(() => runner.RunAsync(dataSetVersionForTest, null, null)).Wait();
			updater.Verify(x => x.UpdateAsync(dataSetVersionForTest, It.IsAny<int?>(), null), Times.Never);
			Assert(true);
		}

		readonly DataSetVersion dataSetVersionForTest = new DataSetVersion("Updater", DateTime.UtcNow);
	}
}
