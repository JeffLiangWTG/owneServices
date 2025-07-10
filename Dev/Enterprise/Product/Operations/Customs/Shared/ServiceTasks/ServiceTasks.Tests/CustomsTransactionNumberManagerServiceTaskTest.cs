using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	[TestedType(typeof(CustomsTransactionNumberManagerServiceTask))]
	class CustomsTransactionNumberManagerServiceTaskTest : ServiceTaskTestCase<CustomsTransactionNumberManagerServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestCleanUpOldData()
		{
			var externalPassword = Factory.New<GlbExternalPassword>();
			externalPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			externalPassword.GP_PasswordType = PasswordTypesList.Codes.IEM;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetExistingRefSysConfigType("IETIDDAY", "No of days", "No of days");
			helper.CreateRefSysConfig("IETIDDAY", 4, ZDateTime.BrettsBirthday, ZDateTime.Empty);
			var date = ZDateTime.UtcToday.AddDays(-11);

			// IECustoms
			var transactionNumber1 = Factory.New<CusTransactionNumber>();
			transactionNumber1.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber1.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber1.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber1.TN_SystemCreateTimeUtc = date.AddDays(-1);
			var transactionNumber2 = Factory.New<CusTransactionNumber>();
			transactionNumber2.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber2.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber2.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber2.TN_SystemCreateTimeUtc = date;
			var transactionNumber3 = Factory.New<CusTransactionNumber>();
			transactionNumber3.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber3.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber3.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber3.TN_SystemCreateTimeUtc = date.AddMinutes(1);
			var transactionNumber4 = Factory.New<CusTransactionNumber>();
			transactionNumber4.TN_Type = CusTransactionNumberTypeList.Codes.IECustoms;
			transactionNumber4.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber4.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber4.TN_SystemCreateTimeUtc = date.AddDays(1);

			// ColombiaManifest
			var transactionNumber5 = Factory.New<CusTransactionNumber>();
			transactionNumber5.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			transactionNumber5.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber5.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber5.TN_SystemCreateTimeUtc = date.AddDays(-1);
			var transactionNumber6 = Factory.New<CusTransactionNumber>();
			transactionNumber6.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			transactionNumber6.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber6.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber6.TN_SystemCreateTimeUtc = date.AddMonths(-1);

			// IECustomsEMCS
			var transactionNumber7 = Factory.New<CusTransactionNumber>();
			transactionNumber7.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber7.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber7.TN_GP_ExternalPassword = externalPassword.PK;
			transactionNumber7.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber7.TN_SystemCreateTimeUtc = date.AddDays(-1);
			var transactionNumber8 = Factory.New<CusTransactionNumber>();
			transactionNumber8.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber8.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber8.TN_GP_ExternalPassword = externalPassword.PK;
			transactionNumber8.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber8.TN_SystemCreateTimeUtc = date;
			var transactionNumber9 = Factory.New<CusTransactionNumber>();
			transactionNumber9.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber9.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber9.TN_GP_ExternalPassword = externalPassword.PK;
			transactionNumber9.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber9.TN_SystemCreateTimeUtc = date.AddMinutes(1);
			var transactionNumber10 = Factory.New<CusTransactionNumber>();
			transactionNumber10.TN_Type = CusTransactionNumberTypeList.Codes.IECustomsEMCS;
			transactionNumber10.TN_GC_Company = GlbCompany.CurrentCompany.PK;
			transactionNumber10.TN_GP_ExternalPassword = externalPassword.PK;
			transactionNumber10.TN_TransactionReference = ZGuid.NewZGuid().ToString();
			transactionNumber10.TN_SystemCreateTimeUtc = date.AddDays(1);
			Factory.Save();
			CombineAssertions(() =>
			{
				var serviceTask = new CustomsTransactionNumberManagerServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
				var anotherFactory = new BusinessObjectFactory();
				AssertNull("IEC - transactionNumber1 should be deleted as it's older than 11 days", anotherFactory.Load<CusTransactionNumber>(transactionNumber1.PK));
				AssertNull("IEC - transactionNumber2 should be deleted as it's 11 days old", anotherFactory.Load<CusTransactionNumber>(transactionNumber2.PK));
				AssertNull("IEC - transactionNumber3 should be deleted as it's 11 days old based on date part only", anotherFactory.Load<CusTransactionNumber>(transactionNumber3.PK));
				AssertNotNull("IEC - transactionNumber4 should not be deleted as it's not older than 11 days", anotherFactory.Load<CusTransactionNumber>(transactionNumber4.PK));

				AssertNotNull("COM - transactionNumber5 should not be deleted as it's not older than 1 month", anotherFactory.Load<CusTransactionNumber>(transactionNumber5.PK));
				AssertNull("COM - transactionNumber6 should be deleted as it's older than 1 month", anotherFactory.Load<CusTransactionNumber>(transactionNumber6.PK));

				AssertNull("IEM - transactionNumber7 should be deleted as it's older than 11 days", anotherFactory.Load<CusTransactionNumber>(transactionNumber7.PK));
				AssertNull("IEM - transactionNumber8 should be deleted as it's 11 days old", anotherFactory.Load<CusTransactionNumber>(transactionNumber8.PK));
				AssertNull("IEM - transactionNumber9 should be deleted as it's 11 days old based on date part only", anotherFactory.Load<CusTransactionNumber>(transactionNumber9.PK));
				AssertNotNull("IEM - transactionNumber10 should not be deleted as it's not older than 11 days", anotherFactory.Load<CusTransactionNumber>(transactionNumber10.PK));
			});
		}
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
