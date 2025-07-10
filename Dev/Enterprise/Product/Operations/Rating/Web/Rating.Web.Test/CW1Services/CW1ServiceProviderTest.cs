using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Business.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Web.Model;
using Moq;

namespace Enterprise.Rating.Web.Test
{
	public class CWServiceProviderTest : TestCaseWithFactory
	{
		public void TestGetJobChargesShouldHandleAutoRaterException()
		{
			var mockRatesAPIAutoRater = new Mock<RatesAPIsAutoRater>();
			mockRatesAPIAutoRater
				.Setup(m => m.AutoRate(It.IsAny<BusinessObjectFactory>(), It.IsAny<RateQuery>(), It.IsAny<ILogger>()))
				.Throws(new AutoRaterException("Some Exception In AutoRating Process"));

			var logger = new ElementaryLogger();
			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var serviceProvider = new CWServiceProvider();

			var rates = serviceProvider.GetJobCharges("User", "SYD", "FEA", query, logger, mockRatesAPIAutoRater.Object);

			AssertEquals(0, rates.Count);
			AssertEquals(1, logger.Errors.Count);

			var log = logger.Errors.Single();
			AssertEquals("An exception occured during Autorating: Some Exception In AutoRating Process", log);
		}

		public void TestValidBranchAndDepartment()
		{
			var cwServiceProvider = new CWServiceProvider();

			AssertDoesntThrowInvalidException(SourceEndpoint.Costing, cwServiceProvider.GetCosts);
			AssertDoesntThrowInvalidException(SourceEndpoint.ClientRates, cwServiceProvider.GetClientRates);
			AssertDoesntThrowInvalidException(SourceEndpoint.CompanyTariffs, cwServiceProvider.GetCompanyTariffs);
			AssertDoesntThrowInvalidException(SourceEndpoint.IntercompanyTariffs, cwServiceProvider.GetIntercompanyTariffs);
			AssertDoesntThrowInvalidException(SourceEndpoint.JobCharges, (user, branch, department, query, logger) => cwServiceProvider.GetJobCharges(user, branch, department, query, logger));

			void AssertDoesntThrowInvalidException(SourceEndpoint endpoint, Func<string, string, string, RateQuery, ILogger, IReadOnlyCollection<Rate>> method)
			{
				var logger = new ElementaryLogger();
				AssertNoExceptionThrown(() => method(Env.CurrentUser.LoginName, "SYD", "FEA", RatesAPITestHelper.GetValidRateQuery(endpoint), logger));
			}
		}

		public void TestInvalidBranchThrowsInvalidBranchException()
		{
			var cwServiceProvider = new CWServiceProvider();

			AssertThrowsInvalidException(cwServiceProvider.GetCosts);
			AssertThrowsInvalidException(cwServiceProvider.GetClientRates);
			AssertThrowsInvalidException(cwServiceProvider.GetCompanyTariffs);
			AssertThrowsInvalidException(cwServiceProvider.GetIntercompanyTariffs);
			AssertThrowsInvalidException((user, branch, department, query, logger) => cwServiceProvider.GetJobCharges(user, branch, department, query, logger));

			void AssertThrowsInvalidException(Func<string, string, string, RateQuery, ILogger, IReadOnlyCollection<Rate>> method)
			{
				var logger = new ElementaryLogger();
				AssertExceptionThrown<InvalidBranchException>(() => method("TEST", "A VERY INVALID BRANCH", "FEA", new RateQuery(), logger));
			}
		}

		public void TestInvalidDepartmentThrowsInvalidDepartmentException()
		{
			var cwServiceProvider = new CWServiceProvider();

			AssertThrowsInvalidException(cwServiceProvider.GetCosts);
			AssertThrowsInvalidException(cwServiceProvider.GetClientRates);
			AssertThrowsInvalidException(cwServiceProvider.GetCompanyTariffs);
			AssertThrowsInvalidException(cwServiceProvider.GetIntercompanyTariffs);
			AssertThrowsInvalidException((user, branch, department, query, logger) => cwServiceProvider.GetJobCharges(user, branch, department, query, logger));

			void AssertThrowsInvalidException(Func<string, string, string, RateQuery, ILogger, IReadOnlyCollection<Rate>> method)
			{
				var logger = new ElementaryLogger();
				AssertExceptionThrown<InvalidDepartmentException>(() => method("TEST", "SYD", "A VERY INVALID DEPARTMENT", new RateQuery(), logger));
			}
		}

		public void TestReportUsage_GivenNoError_ReportsAllFields()
		{
			var username = Factory.NewWithValidTestData<GlbStaff>().GS_LoginName;
			Factory.Save();
			var branchCode = GlbBranch.CurrentBranch.GB_Code;
			var departmentCode = GlbDepartment.CurrentDepartment.GE_Code;
			var orgName = GlbCompany.CurrentCompany.OrgProxy.OH_FullName;
			var timeSpan = new TimeSpan(0, 0, 0, 0, 1000);

			var cwServiceProvider = new CWServiceProvider();

			cwServiceProvider.ReportUsage(username, branchCode, departmentCode, SourceEndpoint.ClientRates, HttpStatusCode.OK, Array.Empty<Rate>(), timeSpan);

			var helper = new UsageCollectorTestHelper(Factory);
			Assert(helper.AssertUsageMessagesContains("RAP", new List<(string, object)>
			{
				("RequestUrl", "ClientRates"),
				("FeatureCode", "RAP"),
				("Module", "Rating"),
				("FeatureDescription", "Rates API"),
				("OrganisationName", orgName),
				("StatusCode", "200"),
				("RateEntriesCount", "0"),
				("RateLinesCount", "0"),
				("ElapsedTime", 1000),
			}));
		}

		public void TestReportUsage_GivenErrorDuringReporting_HandlesError()
		{
			var username = Factory.NewWithValidTestData<GlbStaff>().GS_LoginName;
			Factory.Save();
			var branchCode = GlbBranch.CurrentBranch.GB_Code;
			var departmentCode = GlbDepartment.CurrentDepartment.GE_Code;
			var timeSpan = new TimeSpan(0);

			var exceptionMessage = "thing";
			var cwServiceProvider = new CWServiceProvider();
			cwServiceProvider.UsageReporter = _ => throw new Exception(exceptionMessage);

			cwServiceProvider.ReportUsage(username, branchCode, departmentCode, SourceEndpoint.ClientRates, HttpStatusCode.OK, Array.Empty<Rate>(), timeSpan);

			Assert(ErrorReporter.LastMessageReported.Contains($"Could not report usage. Error: {exceptionMessage}"));
			ErrorReporter.Clear();
		}
	}
}
