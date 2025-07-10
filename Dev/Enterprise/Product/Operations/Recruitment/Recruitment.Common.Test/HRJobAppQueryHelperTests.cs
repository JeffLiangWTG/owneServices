using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Common
{
	sealed class HRJobAppQueryHelperTests : TransactionedTestCase
	{
		public void TestWithinDatesQuery()
		{
			// arrange

			// act
			var query = ApplicationQueryHelper.WithinDatesQuery(ZDateTime.Now.ToDateTime(), ZDateTime.Now.AddDays(-5).ToDateTime());

			//assert
			AssertEquals($"{nameof(HRJobApplicationSchema.HP_SubmissionTimeUtc)} DESC ", query.OrderBy);
			AssertEquals($"{nameof(HRJobApplicationSchema.HP_SubmissionTimeUtc)} >= @CWO1_ and {nameof(HRJobApplicationSchema.HP_SubmissionTimeUtc)} < @CWO2_", query.FilterString);
			AssertNull(query.MaximumRows);
		}

		public void TestWithinDates()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			const int appCount = 10;
			for (var i = 0; i < appCount; ++i)
			{
				var app = factory.NewWithValidTestData<HRJobApplication>();
				app.HP_SubmissionTimeUtc = ZDateTime.Now.AddDays(-i);
			}
			factory.Save();

			// act
			var results = ApplicationQueryHelper.WithinDates(ZDateTime.Now.ToDateTime(), ZDateTime.Now.AddDays(-5).ToDateTime());

			// assert
			AssertEquals(5, results.Length);
		}

		public void TestBatchConvertResumes()
		{
			// arrange
			var logger = new MockLogger();
			var factory = new BusinessObjectFactory();
			var applications = new Queue<HRJobApplication>();
			var app1 = factory.NewWithValidTestData<HRJobApplication>();
			var app2 = factory.NewWithValidTestData<HRJobApplication>();
			applications.Enqueue(app1);
			applications.Enqueue(app2);
			factory.Save();

			var converter = new MockResumeConverter();

			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute("IResumeConverter", converter))
			{
				// pre-assert
				AssertEquals("Nothing in the queue should have been converted yet", 2, applications.Count);

				// act
				var result = ApplicationQueryHelper.BatchConvertResumes(logger, applications, null, new CancellationToken(false));

				//assert
				AssertEquals("Everything in the queue should have been converted", 0, applications.Count);
				Assert("Result should be true", result);
				AssertEquals(2, converter.CallCount);
				AssertEquals("___", app1.HP_CurrentStatus);
				AssertEquals("___", app2.HP_CurrentStatus);
			}
		}

		public void TestBatchConvertResumes_ReturnsCompleteWhenRecruitmentDisabled()
		{
			// arrange
			var logger = new MockLogger();
			var factory = new BusinessObjectFactory();
			var applications = new Queue<HRJobApplication>();
			applications.Enqueue(factory.NewWithValidTestData<HRJobApplication>());
			factory.Save();

			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// act
				var result = ApplicationQueryHelper.BatchConvertResumes(logger, applications, null, new CancellationToken(false));

				//assert
				AssertEquals(true, result);
			}
		}

		public void TestBatchConvertResumes_FailsCancellationToken()
		{
			// arrange
			var logger = new MockLogger();
			var factory = new BusinessObjectFactory();
			var applications = new Queue<HRJobApplication>();
			applications.Enqueue(factory.NewWithValidTestData<HRJobApplication>());
			factory.Save();

			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// act
				var result = ApplicationQueryHelper.BatchConvertResumes(logger, applications, null, new CancellationToken(true));

				//assert
				AssertEquals("Should fail because token was cancelled", false, result);
			}
		}

		public void TestBatchConvertResumes_LoggerNull()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var applications = new Queue<HRJobApplication>();
			applications.Enqueue(factory.NewWithValidTestData<HRJobApplication>());
			factory.Save();

			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// act
				var result = false;
				AssertNoExceptionThrown(() => result = ApplicationQueryHelper.BatchConvertResumes(null, applications, null, new CancellationToken(false)));

				//assert
				AssertEquals(true, result);
			}
		}

		public void TestBatchConvertResumes_ApplicationsNullOrEmpty()
		{
			// arrange
			var logger = new MockLogger();
			var factory = new BusinessObjectFactory();
			var applications = new Queue<HRJobApplication>();

			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// act
				var result = false;
				AssertNoExceptionThrown(() => result = ApplicationQueryHelper.BatchConvertResumes(logger, null, null, new CancellationToken(true)));
				AssertEquals(true, result);

				AssertEquals(0, applications.Count);
				AssertNoExceptionThrown(() => result = ApplicationQueryHelper.BatchConvertResumes(logger, applications, null, new CancellationToken(false)));
				AssertEquals(true, result);
			}
		}

		public void TestBatchConvertResumes_ConversionEndFunctionCalled()
		{
			// arrange
			var logger = new MockLogger();
			var factory = new BusinessObjectFactory();
			var applications = new Queue<HRJobApplication>();
			applications.Enqueue(factory.NewWithValidTestData<HRJobApplication>());
			factory.Save();

			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var called = false;
				// act
				var result = ApplicationQueryHelper.BatchConvertResumes(logger, applications, (app) => called = true, new CancellationToken(false));

				//assert
				Assert("Expected post-PDF-conversion function to be called, but it wasn't", called);
				AssertEquals(true, result);
			}
		}
	}
}
