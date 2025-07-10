using System;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class SuspendPutawayJobTest : WhsSecureServiceTestCase
	{
		[TestDate(2022, 8, 24, 13, 56, 7)]
		public void TestSuspendPutawayJob_ValidJobWithLines_SuspendSuccess()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1a = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			var putawayLine1b = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", true);
			var putawayLine1c = Helper.CreateWhsPutawayLine(putawayJob1, "PLT3", false);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.SuspendPutawayJob();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			CombineAssertions(() =>
			{
				AssertNotNull(WebServiceHelper.FindExistingLog(putawayJob1, Events.ServiceSuspended, putawayJob1.WPJ_EventFreeTextReference));
				Assert("Correct job suspension", !putawayLine1a.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine1b.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine1c.WPL_IsPuttingAway);
			});
		}

		[TestDate(2022, 8, 24, 13, 56, 7)]
		public void TestSuspendPutawayJob_MultipleStaffWithJobsExist_SuspendSuccess()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");
			var staff3 = Helper.CreateGlbStaff("S3", "Staff3");

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			var putawayJob2 = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			var putawayLine2 = Helper.CreateWhsPutawayLine(putawayJob2, "PLT-2", true);
			var putawayJob3 = Helper.CreateWhsPutawayJob(data.Whs1, staff3);
			var putawayLine3 = Helper.CreateWhsPutawayLine(putawayJob3, "PLT-3", true);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff2);
			var response1 = webService1.SuspendPutawayJob();
			Assert("Should be no error message.", string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response1.Error);

			CombineAssertions(() =>
			{
				AssertNull(WebServiceHelper.FindExistingLog(putawayJob1, Events.ServiceSuspended, putawayJob1.WPJ_EventFreeTextReference));
				AssertNotNull(WebServiceHelper.FindExistingLog(putawayJob2, Events.ServiceSuspended, putawayJob2.WPJ_EventFreeTextReference));
				AssertNull(WebServiceHelper.FindExistingLog(putawayJob3, Events.ServiceSuspended, putawayJob3.WPJ_EventFreeTextReference));
				Assert("Correct job suspension", putawayLine1.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine2.WPL_IsPuttingAway);
				Assert("Correct job suspension", putawayLine3.WPL_IsPuttingAway);
			});

			var webService2 = GetNewWebService(data.Whs1, staff3);
			var response2 = webService2.SuspendPutawayJob();
			Assert("Should be no error message.", string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response2.Error);

			CombineAssertions(() =>
			{
				AssertNull(WebServiceHelper.FindExistingLog(putawayJob1, Events.ServiceSuspended, putawayJob1.WPJ_EventFreeTextReference));
				AssertNotNull(WebServiceHelper.FindExistingLog(putawayJob2, Events.ServiceSuspended, putawayJob2.WPJ_EventFreeTextReference));
				AssertNotNull(WebServiceHelper.FindExistingLog(putawayJob3, Events.ServiceSuspended, putawayJob3.WPJ_EventFreeTextReference));
				Assert("Correct job suspension", putawayLine1.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine2.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine3.WPL_IsPuttingAway);
			});
		}

		[TestDate(2022, 8, 24, 13, 56, 7)]
		public void TestSuspendPutawayJob_MultipleWhsWithJobsExist_SuspendSuccess()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			Helper.Factory.Save();

			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1a = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			var putawayLine1b = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", true);

			var putawayJob2 = Helper.CreateWhsPutawayJob(whs2, staff1);
			var putawayLine2 = Helper.CreateWhsPutawayLine(putawayJob2, "NOTPLT", true);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.SuspendPutawayJob();
			Assert("Should be no error message.", string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response1.Error);

			CombineAssertions(() =>
			{
				AssertNotNull(WebServiceHelper.FindExistingLog(putawayJob1, Events.ServiceSuspended, putawayJob1.WPJ_EventFreeTextReference));
				AssertNull(WebServiceHelper.FindExistingLog(putawayJob2, Events.ServiceSuspended, putawayJob2.WPJ_EventFreeTextReference));
				Assert("Correct job suspension", !putawayLine1a.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine1b.WPL_IsPuttingAway);
				Assert("Correct job suspension", putawayLine2.WPL_IsPuttingAway);
			});

			var webService2 = GetNewWebService(whs2, staff1);
			var response2 = webService2.SuspendPutawayJob();
			Assert("Should be no error message.", string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response2.Error);

			CombineAssertions(() =>
			{
				AssertNotNull(WebServiceHelper.FindExistingLog(putawayJob1, Events.ServiceSuspended, putawayJob1.WPJ_EventFreeTextReference));
				AssertNotNull(WebServiceHelper.FindExistingLog(putawayJob2, Events.ServiceSuspended, putawayJob2.WPJ_EventFreeTextReference));
				Assert("Correct job suspension", !putawayLine1a.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine1b.WPL_IsPuttingAway);
				Assert("Correct job suspension", !putawayLine2.WPL_IsPuttingAway);
			});
		}

		[TestDate(2022, 8, 24, 13, 56, 7)]
		public void TestSuspendPutawayJob_NoLines_AddsSuspendedEvent()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.Factory.Save();

			AssertNull(WebServiceHelper.FindExistingLog(putawayJob, Events.ServiceSuspended, putawayJob.WPJ_EventFreeTextReference));

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.SuspendPutawayJob();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			AssertNotNull(WebServiceHelper.FindExistingLog(putawayJob, Events.ServiceSuspended, putawayJob.WPJ_EventFreeTextReference));
		}

		public void TestSuspendPutawayJob_NoJob_DoesNothing()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.SuspendPutawayJob();
			AssertEquals("Putaway Job cannot be found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestSuspendPutawayJob_FinalisedJob_DoesNothing()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", isFinalised: true);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", isFinalised: true);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT3", isFinalised: true);
			putawayJob1.WPJ_FinalizedTimeUtc = DateTime.Now;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.SuspendPutawayJob();
			AssertEquals("Putaway Job cannot be found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}
	}
}
