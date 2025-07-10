using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	[TestedType(typeof(CalculateGuaranteeBalanceServiceTask))]
	class CalculateGuaranteeBalanceServiceTaskTest : ServiceTaskTestCase<CalculateGuaranteeBalanceServiceTask>
	{
		public void TestOptions()
		{
			AssertEquals("15minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestNotAllowMultipleInstances()
		{
			var attribute = GetHostedServiceAttributes().Single();
			AssertEquals($"Disallow multiple instances of service task \"{attribute.Code}\" to protect against concurrency issues", false, attribute.AllowsMultipleInstances);
		}

		public void TestGuaranteeBalanceWorkingCorrectlyServiceTask()
		{
			using (CustomsDataRegistry.Instance.NoOfGuaranteesBGCServiceProcessPerBatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var factory = new BusinessObjectFactory();

				var permit1Header = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
				permit1Header.CPH_Number = "G1";
				permit1Header.CPH_Balance = 0;

				var permit1TransLine1 = permit1Header.AddTransaction("REF1", "CMT1", "APPID", "PROCEDURE", 100m, 200m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
				var permit1TransLine2 = permit1Header.AddTransaction("REF2", "CMT2", "APPID", "PROCEDURE", 50m, 100m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: false);
				var permit1TransLine3 = permit1Header.AddTransaction("REF3", "CMT3", "APPID", "PROCEDURE", 60m, 120m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: false);
				var permit2Header = Factory.NewWithValidTestData<BaseCusPermitHeader>();
				permit2Header.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;

				var permit2TransLine1 = permit2Header.AddTransaction("REFP21", "CMT21", "APPID", "PROCEDURE", 10m, 20m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
				var permit2TransLine2 = permit2Header.AddTransaction("REFP22", "CMT22", "APPID", "PROCEDURE", 100m, 200m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: false);
				var permit2TransLine3 = permit2Header.AddTransaction("REFP23", "CMT23", "APPID", "PROCEDURE", -100m, -200m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: false);
				Factory.Save();

				var logger2 = InitialiseAndRunTaskSchedule(new CalculateGuaranteeBalanceServiceTask());

				var newFactory = new BusinessObjectFactory();
				var permitHeaderLoaded = newFactory.Load<BaseCusGuaranteeHeader>(permit1Header.PK);

				AssertContains(@"Information|updated 1 permits.
Information|updated 1 permits.
Information|updated 0 permits.", logger2.ToString());
				AssertEquals(110m, permitHeaderLoaded.CPH_Balance);

				var permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit1TransLine1.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit1TransLine2.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit1TransLine3.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				var permit1TransLine4 = permit1Header.AddTransaction("REF4", "CMT4", "APPID", "PROCEDURE", -50m, -100m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
				var permit1TransLine5 = permit1Header.AddTransaction("REF5", "CMT5", "APPID", "PROCEDURE", -10m, -20m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: false);
				var permit1TransLine6 = permit1Header.AddTransaction("REF6", "CMT6", "APPID", "PROCEDURE", 20m, 40m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: false);
				Factory.Save();

				logger2 = InitialiseAndRunTaskSchedule(new CalculateGuaranteeBalanceServiceTask());

				newFactory = new BusinessObjectFactory();
				var permitHeaderLoaded2 = newFactory.Load<BaseCusGuaranteeHeader>(permit1Header.PK);

				AssertContains(@"Information|updated 1 permits.
Information|updated 0 permits.", logger2.ToString());
				AssertEquals(120m, permitHeaderLoaded2.CPH_Balance);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit1TransLine4.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit1TransLine5.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit1TransLine6.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				newFactory = new BusinessObjectFactory();
				permitHeaderLoaded = newFactory.Load<BaseCusGuaranteeHeader>(permit2Header.PK);

				AssertContains(@"Information|updated 1 permits.
Information|updated 0 permits.", logger2.ToString());
				AssertEquals(0m, permitHeaderLoaded.CPH_Balance);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit2TransLine1.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit2TransLine2.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit2TransLine3.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				var permit2TransLine4 = permit2Header.AddTransaction("REFP24", "CMT24", "APPID", "PROCEDURE", 25m, 50m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
				var permit2TransLine5 = permit2Header.AddTransaction("REFP25", "CMT25", "APPID", "PROCEDURE", 10m, 20m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: false);
				var permit2TransLine6 = permit2Header.AddTransaction("REFP26", "CMT26", "APPID", "PROCEDURE", 20m, 40m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: false);
				Factory.Save();

				logger2 = InitialiseAndRunTaskSchedule(new CalculateGuaranteeBalanceServiceTask());

				newFactory = new BusinessObjectFactory();
				permitHeaderLoaded = newFactory.Load<BaseCusGuaranteeHeader>(permit2Header.PK);
				AssertEquals(30m, permitHeaderLoaded.CPH_Balance);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit2TransLine4.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit2TransLine5.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitTransLoaded = newFactory.Load<BaseCusGuaranteeLineTransaction>(permit2TransLine6.PK);
				AssertEquals(true, permitTransLoaded.CPL_IsAggregated);

				permitHeaderLoaded2 = newFactory.Load<BaseCusGuaranteeHeader>(permit1Header.PK);
				AssertEquals(120m, permitHeaderLoaded2.CPH_Balance);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						CusPermitLineTransactionSchema.Constants.TableName,
						"Guarantee Balance Calculation Permit Line",
						CusPermitLineTransactionSchema.Constants.CPL_IsAggregated + "=" + CalculateGuaranteeBalanceServiceTask.IsNotAggregated,
						CusPermitLineTransactionSchema.Constants.CPL_TransactionStatus + "=" + CalculateGuaranteeBalanceServiceTask.TransactionStatus),
				};
			}
		}

		public void TestGuaranteeBalanceWorkingCorrectly_SkipCategoryVal()
		{
			var permit1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			permit1.TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			permit1.CPH_Number = "G1";
			permit1.CPH_Balance = 100;

			var line1 = permit1.CusGuaranteeLineTransactions.AddNew();
			line1.CPL_Reference = "ref1";
			line1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			line1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			line1.CPL_IsAggregated = false;
			line1.CPL_TranValue = -50;
			line1.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.VAL;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => { InitialiseAndRunTaskSchedule(new CalculateGuaranteeBalanceServiceTask()); });
				var permitReload = Factory.Load<BaseCusGuaranteeHeader>(permit1.PK);
				AssertEquals("Balance should be correct - skip VAL", (ZDecimal)100, permitReload.CPH_Balance);
				var lineReload = Factory.Load<BaseCusGuaranteeLineTransaction>(line1.PK);
				AssertEquals("IsAggregated should be false - skip VAL", false, lineReload.CPL_IsAggregated);
			});
		}

		public void TestGuaranteeBalanceWorkingCorrectly_SkipNegative()
		{
			var permit1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			permit1.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			permit1.CPH_Balance = 3;

			var line1 = permit1.CusGuaranteeLineTransactions.AddNew();
			line1.CPL_Reference = "ref2";
			line1.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			line1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			line1.CPL_IsAggregated = false;
			line1.CPL_TranValue = -5;
			line1.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => { InitialiseAndRunTaskSchedule(new CalculateGuaranteeBalanceServiceTask()); });
				var permitReload = Factory.Load<BaseCusGuaranteeHeader>(permit1.PK);
				AssertEquals("Balance should be correct - skip negative balance", (ZDecimal)3, permitReload.CPH_Balance);
				var lineReload = Factory.Load<BaseCusGuaranteeLineTransaction>(line1.PK);
				AssertEquals("IsAggregated should be false - skip negative balance", false, lineReload.CPL_IsAggregated);
			});
		}

		public void TestGuaranteeBalanceWorkingCorrectly_SkipIsAggregated()
		{
			var permit1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			permit1.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			permit1.CPH_Balance = 3;

			var line1 = permit1.CusGuaranteeLineTransactions.AddNew();
			line1.CPL_Reference = "ref2";
			line1.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			line1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			line1.CPL_IsAggregated = true;
			line1.CPL_TranValue = -1;
			line1.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => { InitialiseAndRunTaskSchedule(new CalculateGuaranteeBalanceServiceTask()); });
				var permitReload = Factory.Load<BaseCusGuaranteeHeader>(permit1.PK);
				AssertEquals("Balance should be correct - skip negative balance", (ZDecimal)3, permitReload.CPH_Balance);
				var lineReload = Factory.Load<BaseCusGuaranteeLineTransaction>(line1.PK);
				AssertEquals("IsAggregated should be true - skip negative balance", true, lineReload.CPL_IsAggregated);
			});
		}

		public void TestGuaranteeBalanceWorkingCorrectly_MultiRound()
		{
			var permit1 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			permit1.CPH_Number = "aaa";
			permit1.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			permit1.CPH_Balance = 3;

			var line1 = permit1.CusGuaranteeLineTransactions.AddNew();
			line1.CPL_Reference = "ref1";
			line1.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			line1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			line1.CPL_IsAggregated = false;
			line1.CPL_TranValue = -1;
			line1.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;

			var permit2 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			permit2.CPH_Number = "bbb";
			permit2.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			permit2.CPH_Balance = 56;

			var line2 = permit2.CusGuaranteeLineTransactions.AddNew();
			line2.CPL_Reference = "ref2";
			line2.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			line2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			line2.CPL_IsAggregated = false;
			line2.CPL_TranValue = -50;
			line2.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;

			Factory.Save();

			using (CustomsDataRegistry.Instance.NoOfGuaranteesBGCServiceProcessPerBatch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown(() => { InitialiseAndRunTaskSchedule(new CalculateGuaranteeBalanceServiceTask()); });
					var newFactory = new BusinessObjectFactory();
					var permit1Reload = newFactory.Load<BaseCusGuaranteeHeader>(permit1.PK);
					AssertEquals("Balance should be correct - skip negative balance", (ZDecimal)2, permit1Reload.CPH_Balance);
					var line1Reload = newFactory.Load<BaseCusGuaranteeLineTransaction>(line1.PK);
					AssertEquals("IsAggregated should be true - skip negative balance", true, line1Reload.CPL_IsAggregated);
					var permit2Reload = newFactory.Load<BaseCusGuaranteeHeader>(permit2.PK);
					AssertEquals("Balance should be correct - skip negative balance", (ZDecimal)6, permit2Reload.CPH_Balance);
					var line2Reload = newFactory.Load<BaseCusGuaranteeLineTransaction>(line2.PK);
					AssertEquals("IsAggregated should be true - skip negative balance", true, line2Reload.CPL_IsAggregated);
				});
			}
		}
	}
}
