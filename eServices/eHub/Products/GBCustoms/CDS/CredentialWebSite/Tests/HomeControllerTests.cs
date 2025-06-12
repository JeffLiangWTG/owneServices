using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Controllers;
using CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.CredentialWebService;
using CargoWise.eHub.Shared.IssueManager;
using log4net;
using log4net.Appender;
using log4net.Config;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Tests
{
	[TestFixture]
	public class HomeControllerTests
	{
		[Test]
		public void TestIndex_ReceivedErrorFromGBCutsoms_ShowErrorView()
		{
			var controller = new HomeController();
			var actionResult = controller.Index("HYECMT.GB123456789000.ABC", null, "access_denied", "user denied the authorization", "USER_DENIED_AUTHORIZATION");
			var viewResult = (ViewResult)actionResult;

			Assert.AreEqual("Error", viewResult.ViewName);
		}

		[Test]
		public void TestIndex_InvalidNaturalKey_ShowErrorView()
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();
			mockLogger.Expect(x => x.Error("GBCustoms Credential Website - ArgumentException in method Index"));

			var controller = new HomeControllerForTesting(mockLogger);

			var actionResult = controller.Index("invalid", "12345", null, null, null);
			var viewResult = (ViewResult)actionResult;

			Assert.AreEqual("Error", viewResult.ViewName);

			mockLogger.VerifyAllExpectations();
		}

		[Test]
		public void TestIndex_NullNaturalKey_ShowErrorView()
		{
			var controller = new HomeController();
			var actionResult = controller.Index(null, "12345", null, null, null);
			var viewResult = (ViewResult)actionResult;

			Assert.AreEqual("Error", viewResult.ViewName);
		}

		[Test]
		public void TestIndex_CallWebServiceToPersistIsNotSuccessful_ShowErrorView()
		{
			var mockCredentialWebSerivce = MockRepository.GenerateStub<ICredentialWebService>();
			var response = new Response { IsSuccess = false };
			mockCredentialWebSerivce.Stub(_ => _.PersistAuthorisationToken(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(response);

			var controller = new HomeController { CredentialServiceClient = mockCredentialWebSerivce };
			var actionResult = controller.Index("HYECMT.GB123456789000.ABC", "12345", null, null, null);
			var viewResult = (ViewResult)actionResult;

			Assert.AreEqual("Error", viewResult.ViewName);
		}

		[Test]
		public void TestIndex_CallWebServiceToPersistIsSuccessful_ShowIndexView()
		{
			var mockCredentialWebSerivce = MockRepository.GenerateStub<ICredentialWebService>();
			var response = new Response { IsSuccess = true };
			mockCredentialWebSerivce.Stub(_ => _.PersistAuthorisationToken(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(response);

			var controller = new HomeController { CredentialServiceClient = mockCredentialWebSerivce };
			var actionResult = controller.Index("HYECMT.GB123456789000.ABC", "12345", null, null, null);
			var viewResult = (ViewResult)actionResult;

			Assert.AreEqual("Index", viewResult.ViewName);
		}

		[Test]
		public void TestIndex_ReceivedAuthorisationTokenFromGBCustoms_CallPersistAuthorisationToken()
		{
			var mockCredentialWebSerivce = MockRepository.GenerateStub<ICredentialWebService>();
			var response = new Response { IsSuccess = true };
			mockCredentialWebSerivce.Stub(_ => _.PersistAuthorisationToken(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(response);

			var controller = new HomeController { CredentialServiceClient = mockCredentialWebSerivce };
			controller.Index("HYECMT.GB123456789000.ABC", "12345", null, null, null);

			mockCredentialWebSerivce.AssertWasCalled(_ => _.PersistAuthorisationToken(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<DateTime>.Is.Anything));
		}

		[Test]
		public void TestIndex_ReceivedAuthorisationTokenThenWebExceptionThrown_CatchExceptionAndShowErrorView()
		{
			var mockCredentialWebSerivce = MockRepository.GenerateStub<ICredentialWebService>();
			mockCredentialWebSerivce
				.Stub(_ => _.PersistAuthorisationToken(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything,
					Arg<DateTime>.Is.Anything)).Throw(new WebException());

			var mockLogger = MockRepository.GenerateMock<ILog>();
			mockLogger.Expect(x => x.Error("GBCustoms Credential Website - CallWebServiceToPersistWebException"));

			var controller = new HomeControllerForTesting(mockLogger) { CredentialServiceClient = mockCredentialWebSerivce };

			var actionResult = controller.Index("HYECMT.GB123456789000.ABC", "12345", null, null, null);
			var viewResult = (ViewResult)actionResult;

			Assert.AreEqual("Error", viewResult.ViewName);

			mockLogger.VerifyAllExpectations();
		}

		[Test]
		public void TestIndex_ReceivedAuthorisationTokenThenCommunicationExceptionThrown_CatchExceptionAndShowErrorView()
		{
			var mockCredentialWebSerivce = MockRepository.GenerateStub<ICredentialWebService>();
			mockCredentialWebSerivce
				.Stub(_ => _.PersistAuthorisationToken(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything,
					Arg<DateTime>.Is.Anything)).Throw(new CommunicationException());

			var mockLogger = MockRepository.GenerateMock<ILog>();
			mockLogger.Expect(x => x.Error("GBCustoms Credential Website - CallWebServiceToPersistCommunicationException"));

			var controller = new HomeControllerForTesting(mockLogger) { CredentialServiceClient = mockCredentialWebSerivce };

			var actionResult = controller.Index("HYECMT.GB123456789000.ABC", "12345", null, null, null);
			var viewResult = (ViewResult)actionResult;

			Assert.AreEqual("Error", viewResult.ViewName);

			mockLogger.VerifyAllExpectations();
		}

		[Test]
		public void TestCallWebServiceToPersist_ReplaceUri()
		{
			var state = "HYEAYA.GB123456789000.ABC";
			var code = "59e6ec3e637147f48e93e8834ce121dd";

			var url = MockRepository.GenerateStub<Uri>("http://gbcds-test.wisegrid.net");

			var request = MockRepository.GenerateStub<HttpRequestBase>();
			request.Stub(_ => _.Url).Return(url);

			var context = MockRepository.GenerateStub<HttpContextBase>();
			context.Stub(_ => _.Request).Return(request);

			var credentialWebSerivce = MockRepository.GenerateStub<ICredentialWebService>();

			var controller = MockRepository.GenerateStub<HomeController>();
			controller.CredentialServiceClient = credentialWebSerivce ;
			controller.ControllerContext = new ControllerContext(context, new System.Web.Routing.RouteData(), controller);
			var utcNow = new DateTime(2019, 6, 19, 11, 48, 1);
			controller.Stub(_ => _.UtcNow).Return(utcNow);
			controller.CallWebServiceToPersist(state, code);

			credentialWebSerivce.AssertWasCalled(_ => _.PersistAuthorisationToken(state, code, "https://gbcds-test.wisegrid.net", utcNow));
		}

		[Test]
		public void TestIndex_CallWebServiceToPersistIsSuccessful_IncludeAdditionalInfoInLog()
		{
			var mockCredentialWebService = MockRepository.GenerateStub<ICredentialWebService>();
			var response = new Response { IsSuccess = true };
			mockCredentialWebService.Stub(_ => _.PersistAuthorisationToken(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(response);

			var controller = new HomeController { CredentialServiceClient = mockCredentialWebService };
			controller.Index("HYECMT.GB123456789000.ABC", "12345", null, null, null);

			var messagesList = memoryAppender.GetEvents().ToList();

			Assert.That(messagesList.Count, Is.EqualTo(2));
			Assert.That(messagesList[0].RenderedMessage.Contains("IPAddresses: '<UNKNOWN>', requestURL: 'REQUEST URL MISSING'"));
			Assert.That(messagesList[1].RenderedMessage.Contains("[IPAddresses: <UNKNOWN>] [Request URL: REQUEST URL MISSING]"));
			memoryAppender.Clear();
		}

		[Test]
		public void TestIndex_CallWebServiceToPersistIsNotSuccessful_IncludeAdditionalInfoInLog()
		{
			var mockCredentialWebService = MockRepository.GenerateStub<ICredentialWebService>();
			var response = new Response { IsSuccess = false };
			mockCredentialWebService.Stub(_ => _.PersistAuthorisationToken(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(response);

			var controller = new HomeController { CredentialServiceClient = mockCredentialWebService };
			controller.Index("HYECMT.GB123456789000.ABC", "12345", null, null, null);

			var messagesList = memoryAppender.GetEvents().ToList();

			Assert.That(messagesList.Count, Is.EqualTo(2));
			Assert.That(messagesList[0].RenderedMessage.Contains("IPAddresses: '<UNKNOWN>', requestURL: 'REQUEST URL MISSING'"));
			Assert.That(messagesList[1].RenderedMessage.Contains("[IPAddresses: <UNKNOWN>] [Request URL: REQUEST URL MISSING]"));
			memoryAppender.Clear();
		}

		[Test]
		public void TestIndex_CallWebServiceToPersist_StateOrCodeInvalid()
		{
			var controller = new HomeController();

			var actionResult = controller.Index("", "12345", null, null, null);
			var viewResult = (ViewResult)actionResult;
			Assert.AreEqual("Error", viewResult.ViewName);

			actionResult = controller.Index("ABC", "", null, null, null);
			viewResult = (ViewResult)actionResult;
			Assert.AreEqual("Error", viewResult.ViewName);

			actionResult = controller.Index("", "", null, null, null);
			viewResult = (ViewResult)actionResult;
			Assert.AreEqual("Error", viewResult.ViewName);
		}

		[SetUp]
		public void SetUp()
		{
			memoryAppender = new MemoryAppender();
			BasicConfigurator.Configure(memoryAppender);
		}

		MemoryAppender memoryAppender;
	}

	#region Implementation
	public class HomeControllerForTesting : HomeController
	{
		public HomeControllerForTesting(ILog mockLogger)
		{
			logger = mockLogger;
		}

		protected override IssueManager GetIssueManager()
		{
			return new IssueManagerForTesting();
		}
	}

	public class IssueManagerForTesting : IssueManager
	{
		public override void ReportToIssueManager(string subject, Exception exception, ILog logger, NameValueCollection appSettings, bool checkExcluding = true)
		{
			logger.Error(subject);
		}
	}
	#endregion
}
