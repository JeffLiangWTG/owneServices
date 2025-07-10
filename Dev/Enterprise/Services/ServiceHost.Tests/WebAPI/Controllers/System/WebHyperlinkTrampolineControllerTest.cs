using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WebHyperlinkTrampolineControllerTest : TestCaseWithFactory
	{
		public void TestCreatesResponsePageWithEdientUrlForWorkItem()
		{
			var pk = new Guid("2638AE3C-2DE6-4F8B-9D3C-C6CD7B658D87");
			var result = controller.GetTrampolineLandingPage("ShowEditForm", "WorkItem", pk);

			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("text/html", response.Content.Headers.ContentType.MediaType);

			var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			AssertContains("edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=2638ae3c-2de6-4f8b-9d3c-c6cd7b658d87&Hash=", text);
		}

		public void TestCreatesResponsePageWithEdientUrlForWorkItemAlt()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride()))
			{
				var wi = Factory.NewWithValidTestData<WorkItem>();
				Factory.Save();
				var result = controller.GetTrampolineLandingPage("WI", wi.WKI_WorkItemNumber);

				var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertEquals("text/html", response.Content.Headers.ContentType.MediaType);

				var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				AssertContains("edient:", text);
				AssertContains("Command=ShortCode", text);
				AssertContains($"Id={wi.WKI_WorkItemNumber}", text);
				AssertContains("Type=WI", text);
			}
		}

		public void TestCreatesResponsePageWithEdientUrlForOrganization()
		{
			var pk = new Guid("7815D11B-25EF-42DE-A881-0AC91DFEB7DB");
			var result = controller.GetTrampolineLandingPage("ShowEditForm", "Organisation", pk);

			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("text/html", response.Content.Headers.ContentType.MediaType);

			var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			AssertContains("edient:Command=ShowEditForm&ControllerID=Organisation&BusinessEntityPK=7815d11b-25ef-42de-a881-0ac91dfeb7db&Hash=", text);
		}

		public void TestCreatesResponsePageWithEdientUrlForCompanySpecificEntity()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());

			var pk = new Guid("7815D11B-25EF-42DE-A881-0AC91DFEB7DB");
			var result = controller.GetTrampolineLandingPage("ShowEditForm", "Organisation", pk, company: company.GC_Code);

			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertContains("edient:Command=ShowEditForm&LicenceCode=" + company.LicenceKeyIdentifier + "&ControllerID=Organisation&BusinessEntityPK=7815d11b-25ef-42de-a881-0ac91dfeb7db&Hash=", text);
		}

		public void TestCreatesResponsePageWithEdientUrlForLicenceSpecificEntity()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());

			var pk = new Guid("7815D11B-25EF-42DE-A881-0AC91DFEB7DB");
			var result = controller.GetTrampolineLandingPage("ShowEditForm", "Organisation", pk, license: company.LicenceKeyIdentifier);

			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertContains("edient:Command=ShowEditForm&LicenceCode=" + company.LicenceKeyIdentifier + "&ControllerID=Organisation&BusinessEntityPK=7815d11b-25ef-42de-a881-0ac91dfeb7db", text);
		}

		public void TestCreatesResponsePageWithEdientUrlForRequestedLanguage()
		{
			var mockResStrings = new Mock<IResourceStrings>();
			mockResStrings.Object.CurrentLanguage = Res.CurrentLanguage;
			mockResStrings.Setup(m => m.GetString(It.IsAny<ushort>(), "B49F1886-6DBB-4566-B0EB-9578B64C3F5E", It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(() => "A Message But In French");
			mockResStrings.Setup(m => m.GetString(It.IsAny<ushort>(), "9C322DFD-8C00-48E1-A646-30481833B157", It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(() => $"If {BrandingFactory.Instance.ProductName} does not start after a few seconds, click the button below to launch the loader again.");
			mockResStrings.Setup(m => m.GetString(It.IsAny<ushort>(), "34CB80F5-D22D-4040-A768-32C5FB89C969", It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(() => "Open");
			mockResStrings.Setup(m => m.GetLanguageInstance(SharedConstants.Languages.French.ToLowerInvariant())).Returns(() => mockResStrings.Object);

			using (new DisposableAction(() => Res.SetResourceStringsGetter(() => mockResStrings.Object), () => Res.SetResourceStringsGetter(null)))
			{
				var pk = new Guid("2638AE3C-2DE6-4F8B-9D3C-C6CD7B658D87");
				var result = controller.GetTrampolineLandingPage("ShowEditForm", "WorkItem", pk, lang: SharedConstants.Languages.French.ToLowerInvariant());

				var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
				var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				AssertContains("A Message But In French", text);
			}
		}
		public void TestCreatesResponsePageWithEdientUrlForRequestedLanguageAlt()
		{
			var mockResStrings = new Mock<IResourceStrings>();
			mockResStrings.Object.CurrentLanguage = Res.CurrentLanguage;
			mockResStrings.Setup(m => m.GetString(It.IsAny<ushort>(), "B49F1886-6DBB-4566-B0EB-9578B64C3F5E", It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(() => "Un vrai message français");
			mockResStrings.Setup(m => m.GetString(It.IsAny<ushort>(), "9C322DFD-8C00-48E1-A646-30481833B157", It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(() => $"If {BrandingFactory.Instance.ProductName} does not start after a few seconds, click the button below to launch the loader again.");
			mockResStrings.Setup(m => m.GetString(It.IsAny<ushort>(), "34CB80F5-D22D-4040-A768-32C5FB89C969", It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(() => "Open");
			mockResStrings.Setup(m => m.GetLanguageInstance(SharedConstants.Languages.French.ToLowerInvariant())).Returns(() => mockResStrings.Object);

			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride()))
			using (new DisposableAction(() => Res.SetResourceStringsGetter(() => mockResStrings.Object), () => Res.SetResourceStringsGetter(null)))
			{
				var wi = Factory.NewWithValidTestData<WorkItem>();
				Factory.Save();

				var result = controller.GetTrampolineLandingPage("WI", wi.WKI_WorkItemNumber, lang: SharedConstants.Languages.French.ToLowerInvariant());

				var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
				var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				AssertContains("Un vrai message français", text);
			}
		}

		WebHyperlinkTrampolineController controller;
		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			controller = new WebHyperlinkTrampolineController();

			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}
	}
	class TestClientOverride : ClientHook
	{
		public override Clients Client
		{
			get { return Clients.EDI; }
		}

		public override string ClientDisplayName
		{
			get { return "For Test"; }
		}

		public override bool IsValidLogin(string login, string password)
		{
			return true;
		}
	}

	class WebHyperlinkTrampolineControllerBrandingTest : TransactionedTestCase
	{
		public void TestCreatesResponsePageWithCargoWiseProductName()
		{
			using (new TemporaryProductivityWiseDisposable(value: false))
			{
				var text = GetGeneralWorkItemPageText();
				AssertContains("CargoWise", text);
				AssertNotContains("ProductivityWise", text);
			}
		}

		public void TestCreatesResponsePageWithProductivityWiseProductName()
		{
			using (new TemporaryProductivityWiseDisposable(value: true))
			{
				var text = GetGeneralWorkItemPageText();
				AssertContains("ProductivityWise", text);
				AssertNotContains("CargoWise", text);
			}
		}

		string GetGeneralWorkItemPageText()
		{
			var pk = new Guid("2638AE3C-2DE6-4F8B-9D3C-C6CD7B658D87");
			var result = controller.GetTrampolineLandingPage("ShowEditForm", "WorkItem", pk);

			var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			return text;
		}

		WebHyperlinkTrampolineController controller;

		protected override void SetUp()
		{
			base.SetUp();

			controller = new WebHyperlinkTrampolineController();

			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
		}

		sealed class TemporaryProductivityWiseDisposable : IDisposable
		{
			public TemporaryProductivityWiseDisposable(bool value)
			{
				originalValue = DataRegistry.Instance.ProductivityWiseModeEnabled;
				DataRegistry.Instance.ProductivityWiseModeEnabled = value;
			}

			readonly bool originalValue;

			public void Dispose()
			{
				DataRegistry.Instance.ProductivityWiseModeEnabled = originalValue;
			}
		}
	}
}
