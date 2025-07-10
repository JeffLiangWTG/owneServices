using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.DailyTariff
{
	[TestFixture]
	class DailyFilesHelperFixture
	{
		[Test]
		public void CheckFileLastTimeWrite()
		{
			var filesToLocate = new[] { @"MonthlyDutiesImport.xlsx", @"MonthlyMeasureConditions.xlsx", @"MonthlyMeasureExclusions.xlsx" };
			ApplicationConfig.SetDownloadsFolder(DailyTariffTestFiles.BaseDirectory);

			//current month
			DailyTariffTestFiles.SetLastWriteTime(DateTime.UtcNow, filesToLocate);
			Assert.False(DailyFilesHelper.CheckFileLastTimeWrite(filesToLocate));

			//last month
			DailyTariffTestFiles.SetLastWriteTime(DateTime.UtcNow.AddMonths(-1), filesToLocate);
			Assert.False(DailyFilesHelper.CheckFileLastTimeWrite(filesToLocate));

			//last month
			DailyTariffTestFiles.SetLastWriteTime(DateTime.UtcNow.AddMonths(-2), filesToLocate);
			Assert.True(DailyFilesHelper.CheckFileLastTimeWrite(filesToLocate));
		}
	}
}
