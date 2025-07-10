using System;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using CargoWise.RefDbRepo.CNReferenceData.CmdLine;
using CargoWise.RefDbRepo.CNReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	class TariffsUpdateProgramTest : TestBase
	{
		[Test]
		public void TestGetMainTask_CheckUpdates_HasUpdates()
		{
			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 25, 17, 25, 12);
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			var program = new TariffsUpdateProgramForTesting();
			program.EChinaAPIProxy.UpdateCountForTesting = 10;
			program.Run(new[] { "-CheckUpdates" });

			Assert.AreEqual($@"[20-07-25 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:-CheckUpdates
[20-07-25 17:25:12 Info]:Checking for updates...
[20-07-25 17:25:12 Info]:Count of updates: 10
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 1
[20-07-25 17:25:12 Info]:Count of updated tariffs: 2 = 2
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16800 - 9001100001 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16801 - 9001100002 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 2
[20-07-25 17:25:12 Info]:Count of updated tariffs: 0 = 0
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Debug]:Special Rate for 9001100001: 0.6元/平方米 => 0.6 * [032]
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100002, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Info]:<======================> 2 HSN, 2 CIQ Tariffs in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:<======================> 9 Additional Elements in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.AreEqual(@"{
  ""UpdatesDetectedTime"": ""2020-07-25 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-07-25 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-07-25 17:25:12"",
  ""CountOfUpdates"": 0
}", ReadTraceFile());
		}

		[Test]
		public void TestGetMainTask_CheckUpdates_GetUpdatesInParallel()
		{
			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 25, 17, 25, 12);
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			var program = new TariffsUpdateProgramForTesting();
			program.EChinaAPIProxy.UpdateCountForTesting = 30;
			program.Run(new[] { "-CheckUpdates" });

			Assert.AreEqual($@"[20-07-25 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:-CheckUpdates
[20-07-25 17:25:12 Info]:Checking for updates...
[20-07-25 17:25:12 Info]:Count of updates: 30
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 1
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 2
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 3
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 4
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 5
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 6
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 7
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 8
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 9
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 10
[20-07-25 17:25:12 Info]:Count of updated tariffs: 2 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 = 2
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16800 - 9001100001 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16801 - 9001100002 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Debug]:Special Rate for 9001100001: 0.6元/平方米 => 0.6 * [032]
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100002, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Info]:<======================> 2 HSN, 2 CIQ Tariffs in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:<======================> 9 Additional Elements in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.AreEqual(@"{
  ""UpdatesDetectedTime"": ""2020-07-25 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-07-25 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-07-25 17:25:12"",
  ""CountOfUpdates"": 0
}", ReadTraceFile());
		}

		[Test]
		public void TestGetMainTask_CheckUpdates_HasUpdatesOnTraceFile()
		{
			File.WriteAllText(TariffProducerTrace.TrackingFileLocation, @"{
  ""UpdatesDetectedTime"": ""2020-07-24 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-07-24 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-07-24 17:25:12"",
  ""CountOfUpdates"": 20
}");

			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 25, 17, 25, 12);
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			var program = new TariffsUpdateProgramForTesting();
			program.EChinaAPIProxy.UpdateCountForTesting = 0;
			program.Run(new[] { "-CheckUpdates" });

			Assert.AreEqual($@"[20-07-25 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:-CheckUpdates
[20-07-25 17:25:12 Info]:Checking for updates...
[20-07-25 17:25:12 Info]:Count of updates: 0
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 1
[20-07-25 17:25:12 Info]:Count of updated tariffs: 2 = 2
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16800 - 9001100001 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16801 - 9001100002 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 2
[20-07-25 17:25:12 Info]:Count of updated tariffs: 0 = 0
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Debug]:Special Rate for 9001100001: 0.6元/平方米 => 0.6 * [032]
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100002, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Info]:<======================> 2 HSN, 2 CIQ Tariffs in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:<======================> 9 Additional Elements in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.AreEqual(@"{
  ""UpdatesDetectedTime"": ""2020-07-24 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-07-25 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-07-25 17:25:12"",
  ""CountOfUpdates"": 0
}", ReadTraceFile());
		}

		[Test]
		public void TestGetMainTask_CheckUpdates_NoUpdates()
		{
			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 25, 17, 25, 12);
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			var program = new TariffsUpdateProgramForTesting();
			program.EChinaAPIProxy.UpdateCountForTesting = 0;
			program.Run(new[] { "-CheckUpdates" });

			Assert.AreEqual($@"[20-07-25 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:-CheckUpdates
[20-07-25 17:25:12 Info]:Checking for updates...
[20-07-25 17:25:12 Info]:Count of updates: 0
[20-07-25 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);
		}

		[Test]
		public void TestGetMainTask_GetUpdatesForSpecifiedDate()
		{
			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 30, 17, 25, 12);
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			var program = new TariffsUpdateProgramForTesting();
			program.EChinaAPIProxy.UpdateCountForTesting = 0;
			program.Run(new[] { "-date:2020-07-25" });

			Assert.AreEqual($@"[20-07-30 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:-date:2020-07-25
[20-07-30 17:25:12 Info]:Skip Checking Updates.
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 1
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 2
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 3
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 4
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 5
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 6
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 7
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 8
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 9
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 10
[20-07-30 17:25:12 Info]:Count of updated tariffs: 2 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 = 2
[20-07-30 17:25:12 Debug]:Updated Tariff: Tariff: 16800 - 9001100001 - BOOK:  DIGIT_MARK: 10
[20-07-30 17:25:12 Debug]:Updated Tariff: Tariff: 16801 - 9001100002 - BOOK:  DIGIT_MARK: 10
[20-07-30 17:25:12 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...
[20-07-30 17:25:12 Debug]:Special Rate for 9001100001: 0.6元/平方米 => 0.6 * [032]
[20-07-30 17:25:12 Info]:Populating XML for tariff HSData for 9001100002, Created at 2020-07-25, From 2020-07-25 ...
[20-07-30 17:25:12 Info]:<======================> 2 HSN, 2 CIQ Tariffs in total <======================>
[20-07-30 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200730172512.xml'
[20-07-30 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200730172512.xml'
[20-07-30 17:25:12 Info]:<======================> 9 Additional Elements in total <======================>
[20-07-30 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200730172512.xml'
[20-07-30 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200730172512.xml'
[20-07-30 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.IsFalse(File.Exists(TariffProducerTrace.TrackingFileLocation), "No trace file");
		}

		[Test]
		public void TestGetMainTask_GetUpdatesForSpecifiedDateAndDaysBack()
		{
			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 30, 17, 25, 12);
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			var program = new TariffsUpdateProgramForTesting();
			program.EChinaAPIProxy.UpdateCountForTesting = 0;
			program.Run(new[] { "-date:2020-07-25", "-daysBack:5" });

			Assert.AreEqual($@"[20-07-30 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:-date:2020-07-25 -daysBack:5
[20-07-30 17:25:12 Info]:Skip Checking Updates.
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 1
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 2
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 3
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 4
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 5
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 6
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 7
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 8
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 9
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-20 to 2020-07-25, page 10
[20-07-30 17:25:12 Info]:Count of updated tariffs: 2 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 = 2
[20-07-30 17:25:12 Debug]:Updated Tariff: Tariff: 16800 - 9001100001 - BOOK:  DIGIT_MARK: 10
[20-07-30 17:25:12 Debug]:Updated Tariff: Tariff: 16801 - 9001100002 - BOOK:  DIGIT_MARK: 10
[20-07-30 17:25:12 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...
[20-07-30 17:25:12 Debug]:Special Rate for 9001100001: 0.6元/平方米 => 0.6 * [032]
[20-07-30 17:25:12 Info]:Populating XML for tariff HSData for 9001100002, Created at 2020-07-25, From 2020-07-25 ...
[20-07-30 17:25:12 Info]:<======================> 2 HSN, 2 CIQ Tariffs in total <======================>
[20-07-30 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200720_20200725_CNRefCusTariff_20200730172512.xml'
[20-07-30 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200720_20200725_CNRefCusTariff_20200730172512.xml'
[20-07-30 17:25:12 Info]:<======================> 9 Additional Elements in total <======================>
[20-07-30 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200720_20200725_CNRefCusCodeList_20200730172512.xml'
[20-07-30 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200720_20200725_CNRefCusCodeList_20200730172512.xml'
[20-07-30 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.IsFalse(File.Exists(TariffProducerTrace.TrackingFileLocation), "No trace file");
		}

		[Test]
		public void TestGetMainTask_GetUpdatesForToday()
		{
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);
			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 25, 17, 25, 12);

			var program = new TariffsUpdateProgramForTesting();
			program.Run(new string[] { });

			Assert.AreEqual($@"[20-07-25 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:
[20-07-25 17:25:12 Info]:Skip Checking Updates.
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 1
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 2
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 3
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 4
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 5
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 6
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 7
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 8
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 9
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-25 to 2020-07-25, page 10
[20-07-25 17:25:12 Info]:Count of updated tariffs: 2 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 = 2
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16800 - 9001100001 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16801 - 9001100002 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Debug]:Special Rate for 9001100001: 0.6元/平方米 => 0.6 * [032]
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100002, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Info]:<======================> 2 HSN, 2 CIQ Tariffs in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:<======================> 9 Additional Elements in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200725_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.AreEqual(@"{
  ""UpdatesDetectedTime"": ""2020-07-25 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-07-25 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-07-25 17:25:12"",
  ""CountOfUpdates"": 0
}", ReadTraceFile());

			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 26, 17, 25, 12);

			program = new TariffsUpdateProgramForTesting();
			program.Run(new string[] { });

			Assert.AreEqual($@"[20-07-26 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:
[20-07-26 17:25:12 Info]:Skip Checking Updates.
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 1
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 2
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 3
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 4
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 5
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 6
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 7
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 8
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 9
[20-07-26 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-26, page 10
[20-07-26 17:25:12 Info]:Count of updated tariffs: 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 = 0
[20-07-26 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.AreEqual(@"{
  ""UpdatesDetectedTime"": ""2020-07-25 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-07-25 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-07-25 17:25:12"",
  ""CountOfUpdates"": 0
}", ReadTraceFile());

			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 30, 17, 25, 12);

			program = new TariffsUpdateProgramForTesting();
			program.Run(new string[] { });

			Assert.AreEqual($@"[20-07-30 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:
[20-07-30 17:25:12 Info]:Skip Checking Updates.
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 1
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 2
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 3
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 4
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 5
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 6
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 7
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 8
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 9
[20-07-30 17:25:12 Info]:Getting updates from 2020-07-26 to 2020-07-30, page 10
[20-07-30 17:25:12 Info]:Count of updated tariffs: 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 = 0
[20-07-30 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.AreEqual(@"{
  ""UpdatesDetectedTime"": ""2020-07-25 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-07-29 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-07-29 17:25:12"",
  ""CountOfUpdates"": 0
}", ReadTraceFile());
		}

		[Test]
		public void TestGetMainTask_GetUpdatesForToday_TraceFileTooOld()
		{
			File.WriteAllText(TariffProducerTrace.TrackingFileLocation, @"{
  ""UpdatesDetectedTime"": ""2020-06-24 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-06-24 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-06-24 17:25:12"",
  ""CountOfUpdates"": 0
}");

			GlobalOption.Instance.NowGetter = () => new DateTime(2020, 7, 25, 17, 25, 12);
			GlobalOption.Instance.LogGetter = () => new Logger(Logger.LogLevel.Debug);

			var program = new TariffsUpdateProgramForTesting();
			program.EChinaAPIProxy.UpdateCountForTesting = 0;
			program.Run(new string[] { });

			Assert.AreEqual($@"[20-07-25 17:25:12 Info]:==========> Run Tariffs Update Program with arguments:
[20-07-25 17:25:12 Info]:Skip Checking Updates.
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 1
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 2
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 3
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 4
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 5
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 6
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 7
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 8
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 9
[20-07-25 17:25:12 Info]:Getting updates from 2020-07-15 to 2020-07-25, page 10
[20-07-25 17:25:12 Info]:Count of updated tariffs: 2 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0 = 2
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16800 - 9001100001 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Debug]:Updated Tariff: Tariff: 16801 - 9001100002 - BOOK:  DIGIT_MARK: 10
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100001, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Debug]:Special Rate for 9001100001: 0.6元/平方米 => 0.6 * [032]
[20-07-25 17:25:12 Info]:Populating XML for tariff HSData for 9001100002, Created at 2020-07-25, From 2020-07-25 ...
[20-07-25 17:25:12 Info]:<======================> 2 HSN, 2 CIQ Tariffs in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200715_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200715_20200725_CNRefCusTariff_20200725172512.xml'
[20-07-25 17:25:12 Info]:<======================> 9 Additional Elements in total <======================>
[20-07-25 17:25:12 Info]:Start to write '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200715_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:Finish writing '{FileHelper.ExecutingPath}\CN.Tariffs\TestFiles\Output\20200715_20200725_CNRefCusCodeList_20200725172512.xml'
[20-07-25 17:25:12 Info]:==========> finished.", GlobalOption.Instance.Log.All);

			Assert.AreEqual(@"{
  ""UpdatesDetectedTime"": ""2020-07-25 17:25:12"",
  ""LatestCheckForUpdatesTime"": ""2020-07-25 17:25:12"",
  ""LatestGetUpdatesTime"": ""2020-07-25 17:25:12"",
  ""CountOfUpdates"": 0
}", ReadTraceFile());
		}

		string ReadTraceFile()
		{
			return File.ReadAllText(TariffProducerTrace.TrackingFileLocation);
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			if (File.Exists(TariffProducerTrace.TrackingFileLocation))
			{
				File.Delete(TariffProducerTrace.TrackingFileLocation);
			}
		}

		class TariffsUpdateProgramForTesting : TariffsUpdateProgram
		{
			public EChinaAPIProxyForTesting EChinaAPIProxy = new EChinaAPIProxyForTesting();

			protected override EChinaAPIProxy CreateEChinaAPIProxy(ILog logger)
			{
				EChinaAPIProxy.SetLogger(logger);
				return EChinaAPIProxy;
			}
		}

		class EChinaAPIProxyForTesting : EChinaAPIProxy
		{
			public EChinaAPIProxyForTesting() : base(GlobalOption.Instance.Setting, null)
			{
			}

			public void SetLogger(ILog logger) => Logger = logger;

			public int UpdateCountForTesting;

			public override async Task<int> GetUpdatedCountTask()
			{
				Logger.Info($"Checking for updates...");
				Logger.Info($"Count of updates: {UpdateCountForTesting}");

				return await Task.Run(() => UpdateCountForTesting);
			}

			public override async Task<string> GetUpdateDataTask(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
			{
				Logger.Info($@"Getting updates from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}, page {pageNumber}");

				var responseText = startDate > new DateTime(2020, 07, 25) || pageNumber > 1
					? "{\"STATE\":\"1\",\"STATE_INFO\":\"Success!\"}"
					: TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.CNReferenceData.Tests.CN.Tariffs.TestFiles.Input.20200725_01_Response.json");

				return await Task.Run(() => responseText);
			}
		}
	}
}
