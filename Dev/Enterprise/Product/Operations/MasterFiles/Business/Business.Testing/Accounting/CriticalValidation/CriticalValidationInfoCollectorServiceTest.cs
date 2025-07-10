using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CriticalValidationInfoCollectorServiceTest : TestCaseWithFactory
	{
		public void TestUseNeverClearedInfo()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), key1);

			var pk1 = ZGuid.NewZGuid();
			service.AddInfoWhenAllowed(pk1, key1, () => "Message to clear");
			service.AddInfoWhenAllowed(pk1, key1, () => "Message to keep", useNeverClearedInfo: true);

			AssertMultilineASCIIEquals("\r\n" + key1 + ":\r\nMessage to clear", service.GetInfo(pk1, key1));
			AssertMultilineASCIIEquals("\r\n" + key1 + ":\r\nMessage to keep", service.GetInfo(pk1, key1, useNeverClearedInfo: true));

			service.ClearServiceCache();

			AssertMultilineASCIIEquals("\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
			AssertMultilineASCIIEquals("\r\n" + key1 + ":\r\nMessage to keep", service.GetInfo(pk1, key1, useNeverClearedInfo: true));
			AssertMultilineASCIIEquals("\r\n" + key1 + ":\r\nMessage to keep", service.GetInfoSafe(pk1, key1, useNeverClearedInfo: true));
		}

		public void TestGetInfoSafe()
		{
			var service = CriticalValidationInfoCollectorService.GetService(Factory);
			AssertNull(service);
			var msg = service.GetInfoSafe(ZGuid.NewZGuid(), key1);
			AssertMultilineASCIIEquals("\r\n" + key1 + ": There was no attempt to collect any data.", msg);

			service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			AssertNotNull(service);
			msg = service.GetInfoSafe(ZGuid.NewZGuid(), key1);
			AssertMultilineASCIIEquals("\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", msg);
		}

		#region AddInfoWhenAllowed

		#region TestAddInfoWhenAllowed

		public void TestAddInfoWhenAllowed_CollectAlways()
		{
			AssertAddInfoWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
		}

		public void TestAddInfoWhenAllowed_CollectSecondTimeOnly()
		{
			AssertAddInfoWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
		}

		public void TestAddInfoWhenAllowed_CollectAlwaysInUnitTest()
		{
			AssertAddInfoWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport);
		}

		void AssertAddInfoWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency collectionFrequency)
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport)
			{
				using (Globals.TemporaryOverrideForIsTest(false))
				{
					PrepareTestInfo(service, pk1, collectionFrequency);
					PrepareTestInfo(service, pk2, collectionFrequency);

					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key2));
					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key3 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key3));
				}
			}

			PrepareTestInfo(service, pk1, collectionFrequency);
			PrepareTestInfo(service, pk2, collectionFrequency);

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession)
			{
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key2));
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key3 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key3));

				PrepareTestInfo(service, pk1, collectionFrequency);
				PrepareTestInfo(service, pk2, collectionFrequency);
			}

			AssertMultilineASCIIEquals("Get correct info from pk1, key1", "\r\n" + key1 + ":\r\nInfo1" + pk1, service.GetInfo(pk1, key1));
			AssertMultilineASCIIEquals("Get correct info from pk1, key2", "\r\n" + key2 + ":\r\nInfo2" + pk1, service.GetInfo(pk1, key2));
			AssertMultilineASCIIEquals("Get correct info from pk1, key3", "\r\n" + key3 + ": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key3));

			AssertMultilineASCIIEquals("Get correct info from pk2, key1", "\r\n" + key1 + ":\r\nInfo1" + pk2, service.GetInfo(pk2, key1));
			AssertMultilineASCIIEquals("Get correct info from pk2, key2", "\r\n" + key2 + ":\r\nInfo2" + pk2, service.GetInfo(pk2, key2));
			AssertMultilineASCIIEquals("Get correct info from pk2, key3", "\r\n" + key3 + ": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk2, key3));

			service.AddInfoWhenAllowed(pk1, key1, () => "new Info1", collectionFrequency);

			AssertMultilineASCIIEquals("pk1, key1 info should be append", "\r\n" + key1 + ":\r\nInfo1" + pk1 + "\r\nnew Info1", service.GetInfo(pk1, key1));
		}

		#endregion

		#region TestAddInfoToMaximumCollectorSize

		public void TestAddInfoToMaximumCollectorSize_CollectAlways()
		{
			AssertAddInfoToMaximumCollectorSize(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
		}

		public void TestAddInfoToMaximumCollectorSize_CollectSecondTimeOnly()
		{
			AssertAddInfoToMaximumCollectorSize(CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
		}

		public void TestAddInfoToMaximumCollectorSize_CollectAlwaysInUnitTest()
		{
			AssertAddInfoToMaximumCollectorSize(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport);
		}

		void AssertAddInfoToMaximumCollectorSize(CriticalValidationInfoCollectorService.CollectionFrequency collectionFrequency)
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var pk = ZGuid.NewZGuid();

			int maxLengthMultiplicator = collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession ? 10 : 1;
			int totalLengthMultiplicator = collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession ? 5 : 1;

			var strTotalLength = "1".PadLeft(totalLengthMultiplicator * 10 * 1024 * 1024 / 2, '1');
			var strMaxLengthSubOne = "1".PadLeft(maxLengthMultiplicator * 1024 * 1024 / 2 - 1, '1');
			var strMaxLength = strMaxLengthSubOne + "1";
			var strMaxLengthPlusOne = strMaxLength + "1";
			var maximumInfoStr = ": Maximum buffer size is reached.";

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport)
			{
				using (Globals.TemporaryOverrideForIsTest(false))
				{
					service.AddInfoWhenAllowed(pk, key1, () => strMaxLengthPlusOne, collectionFrequency);

					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key1));
					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key2));
				}
			}

			service.AddInfoWhenAllowed(pk, key1, () => strMaxLengthPlusOne, collectionFrequency);

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession)
			{
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key1));
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key2));

				service.AddInfoWhenAllowed(pk, key1, () => strMaxLengthPlusOne, collectionFrequency);
			}

			AssertMultilineASCIIEquals("Get correct info from pk, key1", "\r\n" + key1 + @": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected because maximum buffer size was reached for this key.", service.GetInfo(pk, key1));

			service.AddInfoWhenAllowed(pk, key1, () => strTotalLength, collectionFrequency);
			AssertMultilineASCIIEquals("Get correct info from pk, key1", "\r\n" + key1 + @": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected because maximum buffer size was reached for this key.", service.GetInfo(pk, key1));

			service.AddInfoWhenAllowed(pk, key2, () => "1", collectionFrequency);
			AssertMultilineASCIIEquals("another key info collection should not be impacted.", "\r\n" + key2 + ":\r\n1", service.GetInfo(pk, key2));

			service.ClearServiceCache();

			service.AddInfoWhenAllowed(pk, key1, () => strMaxLengthSubOne, collectionFrequency);
			AssertMultilineASCIIEquals("key info should contain info.", "\r\n" + key1 + ":\r\n" + strMaxLengthSubOne, service.GetInfo(pk, key1));

			service.AddInfoWhenAllowed(pk, key1, () => "1", collectionFrequency);
			AssertMultilineASCIIEquals("key info should contain info.", "\r\n" + key1 + ":\r\n" + strMaxLengthSubOne + "\r\n1", service.GetInfo(pk, key1));

			service.AddInfoWhenAllowed(pk, key2, () => "1", collectionFrequency);
			AssertMultilineASCIIEquals("another key info collection should not be impacted.", "\r\n" + key2 + ":\r\n1", service.GetInfo(pk, key2));

			service.ClearServiceCache();

			service.AddInfoWhenAllowed(pk, key1, () => strMaxLengthSubOne, collectionFrequency);
			AssertMultilineASCIIEquals("key info should contain info.", "\r\n" + key1 + ":\r\n" + strMaxLengthSubOne, service.GetInfo(pk, key1));

			service.AddInfoWhenAllowed(pk, key1, () => "11", collectionFrequency);
			AssertMultilineASCIIEquals("key info should not contain info, only maximum size message as not all info was collected and so this can be misleading.", "\r\n" + key1 + maximumInfoStr, service.GetInfo(pk, key1));

			service.ClearServiceCache();

			service.AddInfoOnceWhenAllowed(pk, key1, () => strTotalLength + "1", collectionFrequency);
			AssertMultilineASCIIEquals("key info should contain maximum size message.", "\r\n" + key1 + $@": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected because maximum buffer size was reached for this key.
Attention. Data may not be collected for the key if it was collected with next collection frequencies where maximum buffer size was reached: {collectionFrequency}.", service.GetInfo(pk, key1));

			service.AddInfoOnceWhenAllowed(pk, key2, () => "1", collectionFrequency);
			AssertMultilineASCIIEquals("key2 info should contain maximum size message as total size is reached.", "\r\n" + key2 + $@": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected for the key if it was collected with next collection frequencies where maximum buffer size was reached: {collectionFrequency}.", service.GetInfo(pk, key2));

			service.ClearServiceCache();

			service.ClearServiceCache();

			service.AddInfoOnceWhenAllowed(pk, key2, () => "1", collectionFrequency);
			AssertMultilineASCIIEquals("key2 info should contain maximum size message as total size is reached.", "\r\n" + key2 + ":\r\n1", service.GetInfo(pk, key2));

			service.AddInfoOnceWhenAllowed(pk, key1, () => strTotalLength, collectionFrequency);
			AssertMultilineASCIIEquals("key info should contain maximum size message.", "\r\n" + key1 + $@": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected because maximum buffer size was reached for this key.
Attention. Data may not be collected for the key if it was collected with next collection frequencies where maximum buffer size was reached: {collectionFrequency}.", service.GetInfo(pk, key1));

			service.ClearServiceCache();
		}

		#endregion

		#endregion

		#region AddLastInfoWhenAllowed

		#region TestAddLastInfoWhenAllowed

		public void TestAddLastInfoWhenAllowed_CollectAlways()
		{
			AssertAddLastInfoWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
		}

		public void TestAddLastInfoWhenAllowed_CollectSecondTimeOnly()
		{
			AssertAddLastInfoWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
		}

		public void TestAddLastInfoWhenAllowed_CollectAlwaysInUnitTest()
		{
			AssertAddLastInfoWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport);
		}

		void AssertAddLastInfoWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency collectionFrequency)
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			var pk1 = ZGuid.NewZGuid();

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport)
			{
				using (Globals.TemporaryOverrideForIsTest(false))
				{
					service.AddLastInfoWhenAllowed(pk1, key1, () => "Info1" + pk1);
					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
				}
			}

			service.AddLastInfoWhenAllowed(pk1, key1, () => "Info1" + pk1, collectionFrequency);

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession)
			{
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
				service.AddLastInfoWhenAllowed(pk1, key1, () => "Info1" + pk1);
			}

			AssertMultilineASCIIEquals("Service should only contain pk1, info1.", "\r\n" + key1 + ":\r\nInfo1" + pk1, service.GetInfo(pk1, key1));

			service.AddLastInfoWhenAllowed(pk1, key1, () => "Info2" + pk1, collectionFrequency);
			AssertMultilineASCIIEquals("Service should only contain pk1, info2.", "\r\n" + key1 + ":\r\nInfo2" + pk1, service.GetInfo(pk1, key1));
		}

		#endregion

		#region TestAddLastInfoToMaximumCollectorSize

		public void TestAddLastInfoToMaximumCollectorSize_CollectAlways()
		{
			AssertAddLastInfoToMaximumCollectorSize(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
		}

		public void TestAddLastInfoToMaximumCollectorSize_CollectSecondTimeOnly()
		{
			AssertAddLastInfoToMaximumCollectorSize(CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
		}

		public void TestAddLastInfoToMaximumCollectorSize_CollectAlwaysInUnitTest()
		{
			AssertAddLastInfoToMaximumCollectorSize(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport);
		}

		void AssertAddLastInfoToMaximumCollectorSize(CriticalValidationInfoCollectorService.CollectionFrequency collectionFrequency)
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var pk1 = ZGuid.NewZGuid();

			int maxLengthMultiplicator = collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession ? 10 : 1;
			int totalLengthMultiplicator = collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession ? 5 : 1;
			var strMaxLengthSubOne = "1".PadLeft(maxLengthMultiplicator * 1024 * 1024 / 2 - 1, '1');
			var maximumInfoStr = ": Maximum buffer size is reached.";

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport)
			{
				using (Globals.TemporaryOverrideForIsTest(false))
				{
					service.AddLastInfoWhenAllowed(pk1, key1, () => strMaxLengthSubOne, collectionFrequency);

					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key2));
					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key3 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key3));
				}
			}

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession)
			{
				service.AddLastInfoWhenAllowed(pk1, key1, () => strMaxLengthSubOne, collectionFrequency);

				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key2));
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key3 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key3));
			}

			service.AddLastInfoWhenAllowed(pk1, key1, () => strMaxLengthSubOne, collectionFrequency);
			AssertMultilineASCIIEquals("Service should reach maximum capacity for this key", "\r\n" + key1 + ":\r\n" + strMaxLengthSubOne, service.GetInfo(pk1, key1));

			service.AddLastInfoWhenAllowed(pk1, key1, () => "2", collectionFrequency);
			AssertMultilineASCIIEquals("Collection of new info should not override the fact that maximum capacity is reached to avoid excessive overheads by infinite collection of debug info even when last info is treated as useful.",
				"\r\n" + key1 + ":\r\n2", service.GetInfo(pk1, key1));

			service.AddLastInfoWhenAllowed(pk1, key1, () => "3", collectionFrequency);
			AssertMultilineASCIIEquals("When maximum capacity is reached we don't report last value at all as it can be not last value.", "\r\n" + key1 + maximumInfoStr, service.GetInfo(pk1, key1));

			service.ClearServiceCache();
			service.AddLastInfoWhenAllowed(pk1, key1, () => strMaxLengthSubOne, collectionFrequency);
			AssertMultilineASCIIEquals("Service should reach maximum capacity for this key.", "\r\n" + key1 + ":\r\n" + strMaxLengthSubOne, service.GetInfo(pk1, key1));

			service.AddLastInfoWhenAllowed(pk1, key1, () => "23", collectionFrequency);
			AssertMultilineASCIIEquals("When maximum capacity is reached we don't report last value at all as actual last value can be skipped after reaching maximum capacity.", "\r\n" + key1 + maximumInfoStr, service.GetInfo(pk1, key1));

			service.AddLastInfoWhenAllowed(pk1, key2, () => "Info2" + pk1, collectionFrequency);
			AssertMultilineASCIIEquals("Another key capacity calculated separately.", "\r\n" + key2 + ":\r\nInfo2" + pk1, service.GetInfo(pk1, key2));

			var strTotalLength = "1".PadLeft(totalLengthMultiplicator * 10 * 1024 * 1024 / 2, '1');
			service.AddLastInfoWhenAllowed(pk1, key2, () => strTotalLength, collectionFrequency);
			AssertMultilineASCIIEquals("Service should reach maximum capacity for all keys.", "\r\n" + key2 + maximumInfoStr, service.GetInfo(pk1, key2));

			service.AddLastInfoWhenAllowed(pk1, key3, () => "Info3", collectionFrequency);
			AssertMultilineASCIIEquals("Service should reach maximum capacity for all keys.", "\r\n" + key3 + $@": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected for the key if it was collected with next collection frequencies where maximum buffer size was reached: {collectionFrequency}.", service.GetInfo(pk1, key3));
		}

		#endregion

		#endregion

		#region AddInfoOnceWhenAllowed

		public void TestAddInfoOnceWhenAllowed_CollectAlways()
		{
			AssertAddInfoOnceWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
		}

		public void TestAddInfoOnceWhenAllowed_CollectSecondTimeOnly()
		{
			AssertAddInfoOnceWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
		}

		public void TestAddInfoOnceWhenAllowed_CollectAlwaysInUnitTest()
		{
			AssertAddInfoOnceWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport);
		}

		void AssertAddInfoOnceWhenAllowed(CriticalValidationInfoCollectorService.CollectionFrequency collectionFrequency)
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport)
			{
				using (Globals.TemporaryOverrideForIsTest(false))
				{
					PrpeareTestInfoOnce(service, pk1, collectionFrequency);
					PrpeareTestInfoOnce(service, pk2, collectionFrequency);

					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key2));
					AssertMultilineASCIIEquals("CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport would NOT collect info in non unit test context.", "\r\n" + key3 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key3));
				}
			}

			PrpeareTestInfoOnce(service, pk1, collectionFrequency);
			PrpeareTestInfoOnce(service, pk2, collectionFrequency);

			if (collectionFrequency == CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession)
			{
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key1));
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key2));
				AssertMultilineASCIIEquals("CollectOnlyAfterErrorReportForCurrentUserSession would NOT collect info when CV error report for the first time.", "\r\n" + key3 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key3));

				PrpeareTestInfoOnce(service, pk1, collectionFrequency);
				PrpeareTestInfoOnce(service, pk2, collectionFrequency);
			}

			AssertMultilineASCIIEquals("Get correct info from pk1, key1", "\r\n" + key1 + ":\r\nInfo1" + pk1, service.GetInfo(pk1, key1));
			AssertMultilineASCIIEquals("Get correct info from pk1, key2", "\r\n" + key2 + ":\r\nInfo2" + pk1, service.GetInfo(pk1, key2));
			AssertMultilineASCIIEquals("Get correct info from pk1, key3", "\r\n" + key3 + ": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk1, key3));

			AssertMultilineASCIIEquals("Get correct info from pk2, key1", "\r\n" + key1 + ":\r\nInfo1" + pk2, service.GetInfo(pk2, key1));
			AssertMultilineASCIIEquals("Get correct info from pk2, key2", "\r\n" + key2 + ":\r\nInfo2" + pk2, service.GetInfo(pk2, key2));
			AssertMultilineASCIIEquals("Get correct info from pk2, key3", "\r\n" + key3 + ": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk2, key3));

			service.AddInfoOnceWhenAllowed(pk1, key1, () =>
			{
				return "new Info1";
			}, collectionFrequency);

			AssertMultilineASCIIEquals("pk1, key1 info should not be append", "\r\n" + key1 + ":\r\nInfo1" + pk1, service.GetInfo(pk1, key1));
		}

		#endregion

		public void TestClearData()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			var pk = ZGuid.NewZGuid();

			PrepareTestInfo(service, pk, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			AssertMultilineASCIIEquals("Get correct info from key1", "\r\n" + key1 + ":\r\nInfo1" + pk, service.GetInfo(pk, key1));
			AssertMultilineASCIIEquals("Get correct info from key2", "\r\n" + key2 + ":\r\nInfo2" + pk, service.GetInfo(pk, key2));
			AssertMultilineASCIIEquals("Get correct info from key3", "\r\n" + key3 + ": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key3));

			service.ClearServiceCache();
			AssertMultilineASCIIEquals("Get correct info from key1", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key1));
			AssertMultilineASCIIEquals("Get correct info from key2", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key2));
			AssertMultilineASCIIEquals("Get correct info from key3", "\r\n" + key3 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key3));

			PrpeareTestInfoOnce(service, pk, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			AssertMultilineASCIIEquals("Get correct info from key1", "\r\n" + key1 + ":\r\nInfo1" + pk, service.GetInfo(pk, key1));
			AssertMultilineASCIIEquals("Get correct info from key2", "\r\n" + key2 + ":\r\nInfo2" + pk, service.GetInfo(pk, key2));
			AssertMultilineASCIIEquals("Get correct info from key3", "\r\n" + key3 + ": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key3));

			service.ClearServiceCache();
			AssertMultilineASCIIEquals("Get correct info from key1", "\r\n" + key1 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key1));
			AssertMultilineASCIIEquals("Get correct info from key2", "\r\n" + key2 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key2));
			AssertMultilineASCIIEquals("Get correct info from key3", "\r\n" + key3 + ": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", service.GetInfo(pk, key3));
		}

		public void TestGetOrCreateService()
		{
			var services1 = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			AssertNotNull("Service1 created", services1);

			var services2 = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			AssertNotNull("Service2 created", services2);
			Assert("Service2 is same as Service1", ReferenceEquals(services1, services2));
		}

		public void TestGetService()
		{
			var services1 = CriticalValidationInfoCollectorService.GetService(Factory);

			AssertNull("Service1 not created", services1);

			services1 = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			AssertNotNull("Service1 created", services1);

			var services2 = CriticalValidationInfoCollectorService.GetService(Factory);

			AssertNotNull("Service2 created", services2);
			Assert("Service2 is same as Service1", ReferenceEquals(services1, services2));
		}

		#region TestCollectInfoForWithDifferentCollectionFrequenciesReportsError

		public void TestCollectInfoForWithDifferentCollectionFrequenciesReportsError_AddInfoWhenAllowed()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			AssertCollectInfoForWithDifferentCollectionFrequenciesReportsError((key, collectionFrequency) => service.AddInfoWhenAllowed(ZGuid.NewZGuid(), key, () => "1", collectionFrequency));
		}

		public void TestCollectInfoForWithDifferentCollectionFrequenciesReportsError_AddLastInfoWhenAllowed()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			AssertCollectInfoForWithDifferentCollectionFrequenciesReportsError((key, collectionFrequency) => service.AddLastInfoWhenAllowed(ZGuid.NewZGuid(), key, () => "1", collectionFrequency));
		}

		void AssertCollectInfoForWithDifferentCollectionFrequenciesReportsError(Action<CriticalValidationInfoCollectorServiceKeyType, CriticalValidationInfoCollectorService.CollectionFrequency> collectInfo)
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), key1);

			collectInfo(key1, CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
			collectInfo(key1, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

			AssertEquals("CriticalValidationInfoCollectorService key 'DummyCollectorKeyForTest' must not be used with different collectionFrequency.", ErrorReporter.LastKeyReported);
			AssertEquals("Key 'DummyCollectorKeyForTest' was used with keys: CollectOnlyAfterErrorReportForCurrentUserSession and CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		#endregion

		#region TestMixedCollectionFrequencyInfoReachingMaxBufferSize

		public void TestMixedCollectionFrequencyInfoReachingMaxBufferSize_AddInfoWhenAllowed()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			AssertMixedCollectionFrequencyInfoReachingMaxBufferSize((pk, key, info, collectionFrequency) => service.AddInfoWhenAllowed(pk, key, () => info, collectionFrequency));
		}

		public void TestMixedCollectionFrequencyInfoReachingMaxBufferSize_AddLastInfoWhenAllowed()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			AssertMixedCollectionFrequencyInfoReachingMaxBufferSize((pk, key, info, collectionFrequency) => service.AddLastInfoWhenAllowed(pk, key, () => info, collectionFrequency));
		}

		public void AssertMixedCollectionFrequencyInfoReachingMaxBufferSize(Action<ZGuid, CriticalValidationInfoCollectorServiceKeyType, string, CriticalValidationInfoCollectorService.CollectionFrequency> collectInfo)
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.GetInfo(ZGuid.NewZGuid(), key1);
			service.GetInfo(ZGuid.NewZGuid(), key2);
			service.GetInfo(ZGuid.NewZGuid(), key3);

			var strMaxLength = "1".PadLeft(1 * 1024 * 1024 / 2, '1');
			var strMaxLengthOnSecondReportCollection = "1".PadLeft(10 * 1024 * 1024 / 2, '1');
			var strTotalLengthForOnSecondReportCollection = "1".PadLeft(50 * 1024 * 1024 / 2, '1');
			var maximumInfoStr = @": Maximum buffer size is reached.";
			var pkIsNotCollected = @": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected for the key if it was collected with next collection frequencies where maximum buffer size was reached: CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE.";
			var pkIsNotCollected2 = @": There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected for the key if it was collected with next collection frequencies where maximum buffer size was reached: CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE, CollectOnlyAfterErrorReportForCurrentUserSession.";
			var keyIsNotCollected = @": There is no data collected for this key. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Attention. Data may not be collected for the key if it was collected with next collection frequencies where maximum buffer size was reached: CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE, CollectOnlyAfterErrorReportForCurrentUserSession.";

			ZGuid pk1 = ZGuid.NewZGuid(), pk2 = ZGuid.NewZGuid(), pk3 = ZGuid.NewZGuid();
			collectInfo(pk1, key2, strMaxLength, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			collectInfo(pk2, key1, strMaxLengthOnSecondReportCollection, CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
			collectInfo(pk3, key2, "3", CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

			AssertMultilineASCIIEquals("Info must be collected as buffer size for 'on second report collection' is bigger.", "\r\n" + key2 + ":\r\n" + strMaxLength, service.GetInfo(pk1, key2));
			AssertMultilineASCIIEquals("Info must be collected as it is equals to buffer size.", "\r\n" + key1 + ":\r\n" + strMaxLengthOnSecondReportCollection, service.GetInfo(pk2, key1));
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key2 + pkIsNotCollected, service.GetInfo(pk3, key2));

			collectInfo(pk1, key2, "1", CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key2 + maximumInfoStr, service.GetInfo(pk1, key2));
			AssertMultilineASCIIEquals("Info must be collected as it is equals to buffer size.", "\r\n" + key1 + ":\r\n" + strMaxLengthOnSecondReportCollection, service.GetInfo(pk2, key1));
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key2 + pkIsNotCollected, service.GetInfo(pk3, key2));

			collectInfo(pk2, key3, strTotalLengthForOnSecondReportCollection, CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key2 + maximumInfoStr, service.GetInfo(pk1, key2));
			AssertMultilineASCIIEquals("Info must be collected as it is equals to buffer size.", "\r\n" + key1 + ":\r\n" + strMaxLengthOnSecondReportCollection, service.GetInfo(pk2, key1));
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key2 + pkIsNotCollected2, service.GetInfo(pk3, key2));
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key3 + keyIsNotCollected, service.GetInfo(pk2, key3));

			collectInfo(pk2, key1, "1", CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key2 + maximumInfoStr, service.GetInfo(pk1, key2));
			AssertMultilineASCIIEquals("Info must be collected as it is equals to buffer size.", "\r\n" + key1 + maximumInfoStr, service.GetInfo(pk2, key1));
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key2 + pkIsNotCollected2, service.GetInfo(pk3, key2));
			AssertMultilineASCIIEquals("Info must not be collected as maximum buffer size is reached.", "\r\n" + key3 + keyIsNotCollected, service.GetInfo(pk2, key3));
		}

		#endregion

		#region Implementation

		readonly CriticalValidationInfoCollectorServiceKeyType key1 = CriticalValidationInfoCollectorServiceKeyType.DummyCollectorKeyForTest;
		readonly CriticalValidationInfoCollectorServiceKeyType key2 = CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedLineAmount;
		readonly CriticalValidationInfoCollectorServiceKeyType key3 = CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount;

		void PrepareTestInfo(CriticalValidationInfoCollectorService service, ZGuid pk, CriticalValidationInfoCollectorService.CollectionFrequency collectionFrequency)
		{
			service.AddInfoWhenAllowed(pk, key1, () =>
			{
				return "Info1" + pk;
			}, collectionFrequency);

			service.AddInfoWhenAllowed(pk, key2, () =>
			{
				return "Info2" + pk;
			}, collectionFrequency);

			service.AddInfoWhenAllowed(pk, key3, () =>
			{
				return null;
			}, collectionFrequency);
		}

		void PrpeareTestInfoOnce(CriticalValidationInfoCollectorService service, ZGuid pk, CriticalValidationInfoCollectorService.CollectionFrequency collectionFrequency)
		{
			service.AddInfoOnceWhenAllowed(pk, key1, () =>
			{
				return "Info1" + pk;
			}, collectionFrequency);

			service.AddInfoOnceWhenAllowed(pk, key2, () =>
			{
				return "Info2" + pk;
			}, collectionFrequency);

			service.AddInfoOnceWhenAllowed(pk, key3, () =>
			{
				return null;
			}, collectionFrequency);
		}

		#endregion
	}
}
