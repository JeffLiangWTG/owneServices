#if NETFRAMEWORK
using System.Web.Http.Controllers;
#elif NET
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
#endif
using System;
using System.Security.Principal;
using CargoWise.Authentication.Glow.Ticketing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class AccountingRatingControllerTest : TestCaseWithFactory
	{
		public void TestAutoRateAndCreateJobHeader_SuccessAsStaff()
		{
			var successfulResult = new RatingResults();
			successfulResult.Logs = new[] { "Info: Something", "Warning: something to keep in mind" };

			mockedAdapter.Setup(f => f.AutoRateAndCreateJobHeader(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(successfulResult);

			var controller = GetController(GlbStaffSchema.Constants.Prefix);

			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = true;
			expectedResult.Results = new RatingResults();
			expectedResult.Results.Logs = new [] { "Info: Something", "Warning: something to keep in mind" };

			var actualResult = controller.AutoRateAndCreateJobHeader(Guid.Empty, "Some Job Table Code", Guid.Empty);
			actualResult.AssertJsonResultEquals(expectedResult);

			mockedAdapter.Verify(f => f.AutoRateAndCreateJobHeader(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), ticket.InteropContextBranchKey, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()));
		}

		public void TestAutoRateAndCreateJobHeader_SuccessAsNonStaff()
		{
			var successfulResult = new RatingResults();
			successfulResult.Logs = new[] { "Warning: something to keep in mind", "Info: Something" };

			mockedAdapter.Setup(f => f.AutoRateAndCreateJobHeader(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(successfulResult);

			var controller = GetController(OrgContactSchema.Constants.Prefix);

			var result = controller.AutoRateAndCreateJobHeader(Guid.Empty, "Some Job Table Code", Guid.Empty);
			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = true;
			result.AssertJsonResultEquals(expectedResult);

			mockedAdapter.Verify(f => f.AutoRateAndCreateJobHeader(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), ticket.InteropContextBranchKey, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()));
		}

		public void TestAutoRateAndCreateJobHeader_BranchPK()
		{
			var branchPK = Guid.NewGuid();
			mockedAdapter.Setup(f => f.AutoRateAndCreateJobHeader(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(new RatingResults());

			var controller = GetController(OrgContactSchema.Constants.Prefix);

			var result = new RestrictedRatingResults();
			result.Successful = true;

			var actualResult = controller.AutoRateAndCreateJobHeader(Guid.Empty, "Some Job Table Code", Guid.Empty, branchPK: branchPK);
			actualResult.AssertJsonResultEquals(result);

			mockedAdapter.Verify(f => f.AutoRateAndCreateJobHeader(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), branchPK, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()));
		}

		public void TestAutoRateAndCreateJobHeader_FailureAsStaff()
		{
			var failedResult = new RatingResults();
			failedResult.Logs = new [] { "Info: Something", "Error: Something bad happened", "Warning: something to keep in mind" };

			mockedAdapter.Setup(f => f.AutoRateAndCreateJobHeader(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(failedResult);

			var controller = GetController(GlbStaffSchema.Constants.Prefix);

			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = false;
			expectedResult.Results = new RatingResults();
			expectedResult.Results.Logs = new [] { "Info: Something", "Error: Something bad happened", "Warning: something to keep in mind" };

			var actualResult = controller.AutoRateAndCreateJobHeader(Guid.Empty, "Some Job Table Code", Guid.Empty);
			actualResult.AssertJsonResultEquals(expectedResult);
		}

		public void TestAutoRateAndCreateJobHeader_FailureAsNonStaff()
		{
			var failedResult = new RatingResults();
			failedResult.Logs = new[] { "Error: Something bad happened", "Warning: something to keep in mind", "Info: Something" };

			mockedAdapter.Setup(f => f.AutoRateAndCreateJobHeader(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(failedResult);

			var controller = GetController(OrgContactSchema.Constants.Prefix);

			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = false;

			var actualResult = controller.AutoRateAndCreateJobHeader(Guid.Empty, "Some Job Table Code", Guid.Empty);
			actualResult.AssertJsonResultEquals(expectedResult);
		}

		public void TestSearchForRates_SuccessAsStaff()
		{
			var successfulResult = new RatingResults();
			successfulResult.Logs = new[] { "Info: Something", "Warning: something to keep in mind" };

			mockedAdapter.Setup(f => f.SearchForRates(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(successfulResult);

			var controller = GetController(GlbStaffSchema.Constants.Prefix);

			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = true;
			expectedResult.Results = new RatingResults();
			expectedResult.Results.Logs = new[] { "Info: Something", "Warning: something to keep in mind" };

			var actualResult = controller.SearchForRates(Guid.Empty, "Some Job Table Code", Guid.Empty);
			actualResult.AssertJsonResultEquals(expectedResult);

			mockedAdapter.Verify(f => f.SearchForRates(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), ticket.InteropContextBranchKey, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()));
		}

		public void TestSearchForRates_SuccessAsNonStaff()
		{
			var successfulResult = new RatingResults();
			successfulResult.Logs = Array.Empty<string>();

			mockedAdapter.Setup(f => f.SearchForRates(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(successfulResult);

			var controller = GetController(OrgContactSchema.Constants.Prefix);

			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = true;

			var actualResult = controller.SearchForRates(Guid.Empty, "Some Job Table Code", Guid.Empty);
			actualResult.AssertJsonResultEquals(expectedResult);

			mockedAdapter.Verify(f => f.SearchForRates(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), ticket.InteropContextBranchKey, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()));
		}

		public void TestSearchForRates_BranchPK()
		{
			var branchPK = Guid.NewGuid();
			mockedAdapter.Setup(f => f.SearchForRates(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(new RatingResults());

			var controller = GetController(OrgContactSchema.Constants.Prefix);

			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = true;

			var result = controller.SearchForRates(Guid.Empty, "Some Job Table Code", Guid.Empty, branchPK: branchPK);
			result.AssertJsonResultEquals(expectedResult);

			mockedAdapter.Verify(f => f.SearchForRates(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), branchPK, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()));
		}

		public void TestSearchForRates_FailureAsStaff()
		{
			var failedResult = new RatingResults();
			failedResult.Logs = new[] { "Error: Something bad happened", "Info: Something", "Warning: something to keep in mind" };

			mockedAdapter.Setup(f => f.SearchForRates(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(failedResult);

			var controller = GetController(GlbStaffSchema.Constants.Prefix);

			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = false;
			expectedResult.Results = new RatingResults();
			expectedResult.Results.Logs = new[] { "Error: Something bad happened", "Info: Something", "Warning: something to keep in mind" };

			var actualResult = controller.SearchForRates(Guid.Empty, "Some Job Table Code", Guid.Empty);
			actualResult.AssertJsonResultEquals(expectedResult);
		}

		public void TestSearchForRates_FailureAsNonStaff()
		{
			var failedResult = new RatingResults();
			failedResult.Logs = new[] { "Error: Something bad happened", "Info: Something", "Warning: something to keep in mind" };

			mockedAdapter.Setup(f => f.SearchForRates(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<LogType>()))
				.Returns(failedResult);

			var controller = GetController(OrgContactSchema.Constants.Prefix);

			var expectedResult = new RestrictedRatingResults();
			expectedResult.Successful = false;

			var actualResult = controller.SearchForRates(Guid.Empty, "Some Job Table Code", Guid.Empty);
			actualResult.AssertJsonResultEquals(expectedResult);
		}

		AccountingRatingController GetController(string providerType)
		{
			var controller = new AccountingRatingController(mockedAdapter.Object);
			ticket.ProviderType = providerType;
#if NETFRAMEWORK
			controller.ControllerContext = new HttpControllerContext()
			{
				Controller = controller,
				Request = new System.Net.Http.HttpRequestMessage()
			};
			controller.Configuration = new System.Web.Http.HttpConfiguration();
			controller.User = new GenericPrincipal(new GlowAuthenticationTicketIdentity(ticket), null);
#elif NET
			controller.ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = new GenericPrincipal(new GlowAuthenticationTicketIdentity(ticket), null) } };
#endif
			return controller;
		}

		protected override void SetUp()
		{
			base.SetUp();

			userBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			ticket = new AuthenticationTicket();
			ticket.InteropContextBranchKey = userBranchPK;

			mockedAdapter = new Mock<IAccountingRatingService>();
		}
		AuthenticationTicket ticket;
		Guid userBranchPK;
		Mock<IAccountingRatingService> mockedAdapter;
	}
}
