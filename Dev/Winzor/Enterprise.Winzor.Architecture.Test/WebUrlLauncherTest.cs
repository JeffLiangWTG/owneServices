using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class WebUrlLauncherTest
{
	const string ExampleEdiEntCS01839659 = "edient:Command=ShowEditForm&LicenceCode=HYEDUKCM2&ControllerID=EMCS&BusinessEntityPK=2258b2fb-bb82-4fc4-a824-792a3251dd9a&VersionNumber=25.1.6.183&Domain=wtg.zone&Instance=UAT+CMR+Message+Testing+2&Hash=%2bT6wa4hTNcC%2fRP%2bpKj6nyRiTzd7sP9tTE";
	const string ExampleUrlNotExpectedDomain = "https://notaexpecteddomain/link/ShowEditForm/WorkItem/c3055c73-83a5-4ba2-9c8c-b012b4042338";
	const string ExampleUrlExpectedDomain = "[expectedDomain]link/ShowEditForm/WorkItem/c3055c73-83a5-4ba2-9c8c-b012b4042338";
	const string ExampleUrlExpectedDomainResult = "edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=c3055c73-83a5-4ba2-9c8c-b012b4042338";

	[Test]
	public async Task TestLaunch()
	{
		Control control = null;
		var url = "http://wisetech.com";

		using var ctx = new EnterpriseTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control();
			return control;
		});

		await control.InvokeWinzorDispatcherAsync(() => WebUrlLauncher.Launch(url));

		Assert.That(ctx.JSInterop.Invocations.Count(i => i.Identifier == "openUrl"), Is.EqualTo(1));
		Assert.That(WebUrlLauncher.LastUrlLaunched, Is.EqualTo(url));
	}

	[Test]
	public async Task TestClearLastUrlLaunched()
	{
		Control control = null;
		var url = "http://wisetech.com";

		using var ctx = new EnterpriseTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control();
			return control;
		});

		await control.InvokeWinzorDispatcherAsync(() => WebUrlLauncher.Launch(url));

		WebUrlLauncher.ClearLastUrlLaunched();
		Assert.That(WebUrlLauncher.LastUrlLaunched, Is.EqualTo(string.Empty));
	}

	// note this will duplicate other, more directed unit-tests so keep it simple and just focus on unique functionality.
	// Basic use-case tests
	[TestCase(ExampleUrlNotExpectedDomain, true, false, false, ExampleUrlNotExpectedDomain, TestName = "Basic - Not Expected Domain")]
	[TestCase(ExampleUrlExpectedDomain, true, false, true, ExampleUrlExpectedDomainResult, TestName = "Basic - Expected Domain")]
	// Test from issue in CS01839659
	[TestCase(ExampleEdiEntCS01839659, true, false, false, ExampleEdiEntCS01839659, TestName = "CS01839659 - Testing a example from incident")]
	// Variation of issue in CS01839659
	[TestCase(ExampleUrlExpectedDomain, true, true, false, ExampleUrlExpectedDomainResult, TestName = "CS01839659 - Testing a variation from incident")]
	public async Task TestLaunchEdient_MultiTest(string testUrl, bool handleCoreResult, bool handleCoreThrowsException, bool expectDirectInvoke,string expectedUrl)
	{
		// Setup
		Control control = null;
		testUrl = testUrl.Replace("[expectedDomain]", WebDataRegistry.Instance.RootServicesUri.Value);

		using var ctx = new EnterpriseTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new Control();
			return control;
		});

		var handlers = EnterpriseUrlHandlerService.UrlHandlers;
		foreach (var handler in handlers)
		{
			EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
		}

		var testUrlHandler = new TestUrlHandler(handleCoreResult, handleCoreThrowsException);
		EnterpriseUrlHandlerService.RegisterUrlHandler(testUrlHandler);

		WebUrlLauncher.ClearLastUrlLaunched();

		// Invoke
		await control.InvokeWinzorDispatcherAsync(() => WebUrlLauncher.Launch(testUrl));

		// Assert
		Assert.That(WebUrlLauncher.LastUrlLaunched, Is.EqualTo(expectedUrl));
		Assert.That(EnterpriseUrlHandlerService.UrlHandlers.Length, Is.EqualTo(1));

		if (expectDirectInvoke)
		{
			// EdiEnt URLs are not launched via JSInterop but directly via EnterpriseUrlHandlerService.Instance.ExecuteUrl
			Assert.That(ctx.JSInterop.Invocations.Count(i => i.Identifier == "openUrl"), Is.EqualTo(0));
		}
		else
		{
			Assert.That(ctx.JSInterop.Invocations.Count(i => i.Identifier == "openUrl"), Is.EqualTo(1));
		}

		if (handleCoreThrowsException || expectDirectInvoke)
		{
			Assert.That(testUrlHandler.InvokeCount, Is.EqualTo(1));
		}
		else
		{
			Assert.That(testUrlHandler.InvokeCount, Is.EqualTo(0));
		}
	}

	public class TestUrlHandler : UrlHandler
	{
		public int InvokeCount { get; internal set; }
		public bool HandleCoreResult { get; internal set; }
		public bool HandleCoreThrowsException { get; internal set; }

		public TestUrlHandler(bool handleCoreResult, bool handleCoreThrowsException)
		{
			HandleCoreResult = handleCoreResult;
			HandleCoreThrowsException = handleCoreThrowsException;
		}

		protected override string ExpectedCommandText => "ShowEditForm";
		protected override bool HandleCore(QueryString queryString)
		{
			InvokeCount++;
			if (HandleCoreThrowsException)
			{
				throw new EnterpriseUrlHandlerException("Test exception");
			}
			return HandleCoreResult;
		}
	}
}
