using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Web;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingGlobalTest : ZGlobalTest
	{
		[UseSnapshotProtection]
		public void TestApplicationErrorInitiatesDisposableDbConnection()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				using (var env = new TestWebDbEnvironment())
				{
					env.SetServingWebBasedApp(true);
					Db.ResetAlreadyReported_ForTest();

					var global = new TestGlobal();
					global.ExceptionShouldHandledOverrideForTesting = () => Db.Connection != null;
					ZArchitecture.Web.GUI.Testing.TestGlobal.InitZGlobalWithHttpContext(global);
					HttpContext.Current.AddError(new ViewStateException());
					global.ApplicationErrorForTesting(this, EventArgs.Empty);
					Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				}

				errorReporterMock.Verify(reporter => reporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction, It.IsAny<InvalidOperationException>()), Times.Never);
			}
		}

		#region User Context And Environment

		[ExpectNoExceptions]
		public void TestTheCorrectEnvironmentProviderIsBeingUsed()
		{
			using (ZArchitecture.Web.GUI.Testing.TestGlobal.TemporaryAppDomain(appDomain => { }, () =>
			{
				EnterpriseApplicationConfiguration.ConfigureObjectFactory();

				SetNewContext();
				var sharedContext = HttpContext.Current;

#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				var serverName = ConfigurationManager.AppSettings["ServerName"] = (string)AppDomain.CurrentDomain.GetData("ServerName");
				var databaseName = ConfigurationManager.AppSettings["DatabaseName"] = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
				Db.InitializeDatabaseDetails(serverName, databaseName);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

				var global = new TestGlobal();
				ZArchitecture.Web.GUI.Testing.TestGlobal.InitZGlobalWithHttpContext(global);
				global.ApplicationStartForTesting(null, EventArgs.Empty);

				AssertType(typeof(WebEnvironmentProvider), global.WebEnvProviderForTesting);
			}))
			{ }
		}

		[ExpectNoExceptions]
		public void TestMultipleThreadsShareUserContextAndEnvironmentInSession()
		{
			TestMultipleThreadsShareUserContextAndEnvironmentInSessionAppDomain();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		static void TestMultipleThreadsShareUserContextAndEnvironmentInSessionAppDomain()
		{
			using (ZArchitecture.Web.GUI.Testing.TestGlobal.TemporaryAppDomain(appDomain => { }, () =>
			{
				EnterpriseApplicationConfiguration.ConfigureObjectFactory();

				SetNewContext();
				var sharedContext = HttpContext.Current;
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				var serverName = ConfigurationManager.AppSettings["ServerName"] = (string)AppDomain.CurrentDomain.GetData("ServerName");
				var databaseName = ConfigurationManager.AppSettings["DatabaseName"] = (string)AppDomain.CurrentDomain.GetData("DatabaseName");

				Db.InitializeDatabaseDetails(serverName, databaseName);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

				var global = new TestGlobal();
				ZArchitecture.Web.GUI.Testing.TestGlobal.InitZGlobalWithHttpContext(global);
				global.ApplicationStartForTesting(null, EventArgs.Empty);

				var mainThreadUserContext = new Mock<UserContext>().Object;
				Env.SetUserContext(mainThreadUserContext);
				var mainThreadEnvironment = Env.Instance;

				CombineAssertions(() =>
				{
					AssertSame("The user context should be cached", mainThreadUserContext, Env.CurrentUserContext);
					AssertSame("The environment should be cached", mainThreadEnvironment, Env.Instance);
				});

				var threadEnviornment = default(BaseEnvironment);
				var threadDifferentSession = new ThreadStart(() =>
				{
					SetNewContext();
					var userContext = new Mock<UserContext>().Object;
					Env.SetUserContext(userContext);
					threadEnviornment = Env.Instance;
				});
				var thread = new Thread(threadDifferentSession);
				thread.Start();
				thread.Join();

				CombineAssertions(() =>
				{
					AssertNotSame("Should have created a new environment for the new session", threadEnviornment, Env.Instance);
					AssertSame("Should pull the user context from the old session", mainThreadUserContext, Env.CurrentUserContext);
					AssertSame("Should pull the environment from the old session", mainThreadEnvironment, Env.Instance);
				});

				var sharedUserContext = default(UserContext);
				var threadSameSession = new ThreadStart(() =>
				{
					HttpContext.Current = sharedContext;
					sharedUserContext = new Mock<UserContext>().Object;
					Env.SetUserContext(sharedUserContext);
					threadEnviornment = Env.Instance;
				});
				thread = new Thread(threadSameSession);
				thread.Start();
				thread.Join();

				CombineAssertions(() =>
				{
					AssertNotNull(sharedUserContext);
					AssertSame("Should have reused the environment from the session", threadEnviornment, Env.Instance);
					AssertSame("Should pull the user context from the session", sharedUserContext, Env.CurrentUserContext);
					AssertSame("Should pull the environment from the session", mainThreadEnvironment, Env.Instance);
				});
			}))
			{ }
		}

		static void SetNewContext()
		{
			var workerRequest = new DummyWorkerRequest("default.aspx", "", new StringWriter());
			HttpContext.Current = new HttpContext(workerRequest);
			HttpApplication testApplication = new DummyHttpApplication(workerRequest);
			typeof(HttpApplication).InvokeMember("InitInternal", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, testApplication, new object[] { HttpContext.Current, testApplication.Application, Array.Empty<MethodInfo>() });
			typeof(HttpApplication).InvokeMember("_context", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, testApplication, new object[] { HttpContext.Current });
			HttpContext.Current.ApplicationInstance = testApplication;
			HttpSessionStateContainer container = new HttpSessionStateContainer("DummySession", new SessionStateItemCollection(), new HttpStaticObjectsCollection(), 60, true, HttpCookieMode.AutoDetect, SessionStateMode.InProc, false);
			SessionStateUtility.AddHttpSessionStateToContext(HttpContext.Current, container);
			HttpContext.Current.Request.Browser = new HttpBrowserCapabilities();
			HttpContext.Current.Request.Browser.Capabilities = new HybridDictionary();
		}

		#endregion

		protected override int NumberOfLocations
		{
			get { return 51; }
		}

		protected override string WebConfigPath
		{
			get { return GetSupplementaryContentPath("Enterprise", "Product", "Operations", "Tracking", "Tracking.Web", "Web.config"); }
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWebRegistry_HandlesUrlsWithProtocol()
		{
			var tester = GetNewZGlobalForTesting();

			var factory = new BusinessObjectFactory();
			var branch = factory.LoadTop1<GlbBranch>(new ZQuery());
			tester.GlobalConfig.ConfigurationItemsForTesting.Add("Branch", branch.GB_Code);
			var webTrackerUrls = new[] { "HTTPS://www.TEST.com/", "Http://www.Test2.com/" };
			var webTrackerUrlsCount = webTrackerUrls.Length;

			using (WebDataRegistry.Instance.OldTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ThemeCodeDescriptionPairList.Codes.CUS))
			using (WebDataRegistry.Instance.WebTrackerUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, webTrackerUrls))
			using (WebDataRegistry.Instance.WebTrackerTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerTheme>()))
			using (WebDataRegistry.Instance.WebTrackerCustomImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomImage>()))
			using (WebDataRegistry.Instance.WebTrackerCustomCss.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomCss>()))
			{
				var url = "www.test.com";
				tester.PopulateWebRegistry(url);

				AssertEquals("Should not add new WebTrackerUrl if has https version", webTrackerUrlsCount, WebDataRegistry.Instance.WebTrackerUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);

				url = "www.test2.com";
				tester.PopulateWebRegistry(url);

				AssertEquals("Should not add new WebTrackerUrl if has http version", webTrackerUrlsCount, WebDataRegistry.Instance.WebTrackerUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWebRegistry()
		{
			ZGlobal tester = GetNewZGlobalForTesting();

			var factory = new BusinessObjectFactory();
			GlbBranch branch = factory.LoadTop1<GlbBranch>(new ZQuery());
			tester.GlobalConfig.ConfigurationItemsForTesting.Add("Branch", branch.GB_Code);

			using (WebDataRegistry.Instance.OldTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ThemeCodeDescriptionPairList.Codes.CUS))
			using (WebDataRegistry.Instance.WebTrackerUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
			using (WebDataRegistry.Instance.WebTrackerTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerTheme>()))
			using (WebDataRegistry.Instance.WebTrackerCustomImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomImage>()))
			using (WebDataRegistry.Instance.WebTrackerCustomCss.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomCss>()))
			{
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebTrackerUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebTrackerTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebTrackerCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebTrackerCustomCss.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);

				var url = "www.test.com";
				tester.PopulateWebRegistry(url);

				var urls = WebDataRegistry.Instance.WebTrackerUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertCollectionContains(url, urls);

				var themeObjects = WebDataRegistry.Instance.WebTrackerTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertEquals(true, themeObjects.Exists(x => x.Code == "CUS" && x.Url == url));

				var imageObjects = WebDataRegistry.Instance.WebTrackerCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertEquals(true, imageObjects.All(x => x.Url == url));

				var imagesFolder = Path.Combine(tester.ApplicationRoot, "Images");
				var imagePaths = new DirectoryInfo(tester.MapPath(imagesFolder)).GetFiles()
					.Where(x => WebTrackerCustomImage.IsSupportedFileType(x.Extension)).Select(x => x.FullName);
				AssertEquals(imageObjects.Count, imagePaths.Count());

				var webTrackerCustomCss = new WebTrackerCustomCss(url, File.ReadAllText(tester.MapPath(tester.BaseStyleSheet)));
				var cssObjects = WebDataRegistry.Instance.WebTrackerCustomCss.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertEquals(true, cssObjects.Exists(x => x.Url == url && x.Data == webTrackerCustomCss.Data));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWebRegistry_DoesNotOverrideAllUrlImages()
		{
			ZGlobal tester = GetNewZGlobalForTesting();

			var factory = new BusinessObjectFactory();
			GlbBranch branch = factory.LoadTop1<GlbBranch>(new ZQuery());
			tester.GlobalConfig.ConfigurationItemsForTesting.Add("Branch", branch.GB_Code);

			var customImage = new WebTrackerCustomImage("Logo.gif", string.Empty, Array.Empty<byte>());
			using (WebDataRegistry.Instance.OldTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ThemeCodeDescriptionPairList.Codes.CUS))
			using (WebDataRegistry.Instance.WebTrackerUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
			using (WebDataRegistry.Instance.WebTrackerTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerTheme>()))
			using (WebDataRegistry.Instance.WebTrackerCustomImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { customImage }))
			using (WebDataRegistry.Instance.WebTrackerCustomCss.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomCss>()))
			{
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebTrackerUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebTrackerTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should have one custom image", 1, WebDataRegistry.Instance.WebTrackerCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebTrackerCustomCss.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);

				var url = "www.test.com";
				tester.PopulateWebRegistry(url);

				var imageObjects = WebDataRegistry.Instance.WebTrackerCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertEquals(true, imageObjects.All(x => x.Url == url && x.Name != "Logo.gif" || (string.IsNullOrEmpty(x.Url) && x.Name == "Logo.gif")));
			}
		}

		public void TestBaseStyleSheet()
		{
			var global = GetNewZGlobalForTesting();
			var registryItem = WebDataRegistry.Instance.WebTrackerTheme;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerTheme>());
			AssertEquals("The style sheet file name should be the root stylesheet if no theme is selected.",
				global.ApplicationRoot + "BaseStyle.css",
				global.BaseStyleSheet);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.CUS) });
			AssertEquals("The style sheet file name should be the root stylesheet if the custom theme is selected.",
				global.ApplicationRoot + "BaseStyle.css",
				global.BaseStyleSheet);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.STD) });
			AssertEquals("The style sheet file name should be the themed stylesheet if a theme is selected.",
				global.ApplicationRoot + "App_Themes/Standard/BaseStyle.css",
				global.BaseStyleSheet);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.ALT) });
			AssertEquals("The style sheet file name should be the themed stylesheet if a theme is selected.",
				global.ApplicationRoot + "App_Themes/Alternate/BaseStyle.css",
				global.BaseStyleSheet);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.CLS) });
			AssertEquals("The style sheet file name should be the themed stylesheet if a theme is selected.",
				global.ApplicationRoot + "App_Themes/Classic/BaseStyle.css",
				global.BaseStyleSheet);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.STD) });
				AssertEquals("The style sheet file name should be the themed stylesheet if a theme is selected.",
					global.ApplicationRoot + "App_Themes/Standard/BaseStyle.css",
					global.BaseStyleSheet);
			}
		}

		public void TestLogoImage()
		{
			var global = GetNewZGlobalForTesting();
			var registryItem = WebDataRegistry.Instance.WebTrackerTheme;

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerTheme>());
			AssertEquals("The file name should be the root stylesheet if no theme is selected.",
				global.ApplicationRoot + "Images/Logo.gif",
				global.LogoImage);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.CUS) });
			AssertEquals("The file name should be the root stylesheet if the custom theme is selected.",
				global.ApplicationRoot + "Images/Logo.gif",
				global.LogoImage);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.STD) });
			AssertEquals("The file name should be the themed stylesheet if a theme is selected.",
				global.ApplicationRoot + "App_Themes/Standard/Images/Logo.gif",
				global.LogoImage);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.ALT) });
			AssertEquals("The file name should be the themed stylesheet if a theme is selected.",
				global.ApplicationRoot + "App_Themes/Alternate/Images/Logo.gif",
				global.LogoImage);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.CLS) });
			AssertEquals("The file name should be the themed stylesheet if a theme is selected.",
				global.ApplicationRoot + "App_Themes/Classic/Images/Logo.gif",
				global.LogoImage);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebTrackerTheme[] { new WebTrackerTheme("", ThemeCodeDescriptionPairList.Codes.STD) });
				AssertEquals("The file name should be the themed stylesheet if a theme is selected.",
					global.ApplicationRoot + "App_Themes/Standard/Images/Logo.gif",
					global.LogoImage);
			}
		}

		public override void AssertLocations(XmlNodeList locations)
		{
			var pages = new string[]
			{
				"Error.aspx",
				"Logout.aspx",
				"Admin/ResetPassword.aspx",
				"Admin/SetPassword.aspx",
				"Login/ForgotPassword.aspx",
				"Login/LoginSuperseded.aspx",
				"Login/LoginRedirection.aspx",
				"Admin/ResetMasterPassword.aspx",
				"Admin/SetMasterPassword.aspx",
				"Login/LoginComplete.aspx",
				"WebService",
				TrackingConstants.RelativePath.AutoLoginRequestHandler,
				"RunUrl.aspx",
				"ExchangeRates/ExchangeRates.aspx",
				"Preload.aspx"
			};
			var locationOffset = 5;

			for (var i = 0; i < pages.Length; i++, locationOffset++)
			{
				AssertEquals(pages[i], locations[locationOffset].Attributes["path"].Value);
				AssertEquals("?", locations[locationOffset].SelectSingleNode("system.web/authorization/allow").Attributes["users"].Value);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public void TestContentSecurityPolicy()
		{
			AssertCustomResponseHeader("Content-Security-Policy", "default-src 'none'; frame-src 'self'; frame-ancestors *; connect-src 'self'; font-src 'self'; img-src * data:; media-src 'self'; base-uri 'self'; object-src 'none'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline';");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public void TestReferrerPolicy()
		{
			AssertCustomResponseHeader("Referrer-Policy", "strict-origin-when-cross-origin");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public void AssertCustomResponseHeader(string headerName, string headerValue)
		{
			var config = new XmlDocument();
			config.Load(Path.Combine(BaseSourcePath, WebConfigPath));
			AssertNotNull("The web configuration path must be valid", config);

			var customHeadersSection = config.DocumentElement.SelectSingleNode("system.webServer/httpProtocol/customHeaders");
			AssertNotNull("The custom headers section is missing", customHeadersSection);

			var cspNode = customHeadersSection.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "add" && n.Attributes["name"].Value == headerName);
			AssertNotNull(headerName + " node is missing", cspNode);

			var expectedPolicy = headerValue;
			AssertEquals(headerName + " is incorrect", expectedPolicy, cspNode.Attributes["value"].Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public void TestHandlers()
		{
			XmlDocument config = new XmlDocument();
			config.Load(Path.Combine(BaseSourcePath, WebConfigPath));
			AssertNotNull("The web configuration path must be valid.", config);

			XmlNode handlersSection = config.DocumentElement.SelectSingleNode("location[not(@path)]/system.web/httpHandlers");
			AssertNotNull("The handlers section must be present.", handlersSection);

			Dictionary<string, string> types = new Dictionary<string, string>();

			int asmxRemoveHandlerCount = 0;
			int scriptHandlerFactoryAddHandlerCount = 0;

			foreach (XmlNode action in handlersSection.ChildNodes)
			{
				if (action.Name == "remove")
				{
					if (action.Attributes["path"].Value == "*.asmx")
					{
						asmxRemoveHandlerCount++;
					}
				}
				if (action.Attributes["type"] != null && action.Attributes["type"].Value != null)
				{
					if (!types.ContainsKey(action.Attributes["path"].Value) && !action.Attributes["type"].Value.StartsWith("System."))
					{
						types.Add(action.Attributes["path"].Value, null);
					}
					else
					{
						if (action.Name == "add")
						{
							if (action.Attributes["type"].Value == "System.Web.Script.Services.ScriptHandlerFactory" && action.Attributes["validate"].Value == "false" && action.Attributes["verb"].Value == "*")
							{
								scriptHandlerFactoryAddHandlerCount++;
							}
						}
					}
				}
			}

			AssertEquals(@"Expected one handler <remove verb=""*"" path=""*.asmx""/>", 1, asmxRemoveHandlerCount);
			AssertEquals(@"<add verb=""*"" path=""*.asmx"" type=""System.Web.Script.Services.ScriptHandlerFactory"" validate=""false""/>", 1, scriptHandlerFactoryAddHandlerCount);

			AssertContainsExactElementsInAnyOrder(
				new string[] {
					"*.css",
					"Images/*",
					new eDocsRequestHelper().BaseUrl,
					new InvoiceRequestHelper().BaseUrl,
					new StatementRequestHelper().BaseUrl,
					new QuoteRequestHelper().BaseUrl,
					new QuoteClientReplyRequestHelper().BaseUrl,
					TrackingConstants.RelativePath.AutoLoginRequestHandler,
					new ReportRequestHelper().BaseUrl,
					new DocumentRequestHelper().BaseUrl,
					AutoCompleteTextBoxRequestHandler.BaseUrl,
					"ResourceStringUsageData.axd",
					new FreightLabelRequestHelper().BaseUrl,
					new HouseBillRequestHelper().BaseUrl,
					"ModulePreloadRequestHandler.axd",
				}, types.Keys);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Baseline")]
		public void TestRequestQueueLimitPerSession()
		{
			var config = new XmlDocument();
			config.Load(Path.Combine(BaseSourcePath, WebConfigPath));
			AssertNotNull("The web configuration path must be valid", config);

			var appSettingsSection = config.DocumentElement.SelectSingleNode("appSettings");
			AssertNotNull("The appSettings section is missing", appSettingsSection);

			var requestQueueLimitNode = appSettingsSection.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "add" && n.Attributes["key"].Value == "aspnet:RequestQueueLimitPerSession");
			AssertNotNull("The request queue limit per session node is missing", requestQueueLimitNode);

			var expectedValue = "2147483647";
			AssertEquals("Should have value from https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/retargeting/4.6.2-4.7.1#throttle-concurrent-requests-per-session", expectedValue, requestQueueLimitNode.Attributes["value"].Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Baseline")]
		public void TestSessionState()
		{
			var config = new XmlDocument();
			config.Load(Path.Combine(BaseSourcePath, WebConfigPath));
			AssertNotNull("The web configuration path must be valid", config);

			var sessionStateSection = config.DocumentElement.SelectSingleNode("location[not(@path)]/system.web/sessionState");
			AssertNotNull("The session state section is missing", sessionStateSection);
			AssertEquals("The cookie name is incorrect", "WebTracker_SessionId", sessionStateSection.Attributes["cookieName"].Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Baseline")]
		public void TestHttpRuntime()
		{
			var config = new XmlDocument();
			config.Load(Path.Combine(BaseSourcePath, WebConfigPath));
			AssertNotNull("The web configuration path must be valid", config);

			var httpRuntimeSection = config.DocumentElement.SelectSingleNode("location[not(@path)]/system.web/httpRuntime");
			AssertNotNull("The http runtime section is missing", httpRuntimeSection);
			AssertEquals("The version header should be disabled", "false", httpRuntimeSection.Attributes["enableVersionHeader"].Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Baseline")]
		public void TestRemovalOfVersionInfoInHeaders()
		{
			var config = new XmlDocument();
			config.Load(Path.Combine(BaseSourcePath, WebConfigPath));
			AssertNotNull("The web configuration path must be valid", config);

			var customHeadersSection = config.DocumentElement.SelectSingleNode("system.webServer/httpProtocol/customHeaders");
			AssertNotNull("The custom headers section is missing", customHeadersSection);

			var poweredByHeader = customHeadersSection.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "remove" && n.Attributes["name"].Value == "X-Powered-By");
			AssertNotNull("The removal of X-Powered-By header is missing", poweredByHeader);

			var aspNetVersionHeader = customHeadersSection.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "remove" && n.Attributes["name"].Value == "X-AspNet-Version");
			AssertNotNull("The removal of X-AspNet-Version header is missing", aspNetVersionHeader);
		}

		#region Page path tests

		public void TestDefaultPage()
		{
			AssertEquals("/Default.aspx", TrackingGlobal.DefaultPage);
		}

		public void TestAutoLoginRequestHandler()
		{
			AssertEquals("/AutoLoginRequestHandler.axd", TrackingGlobal.AutoLoginRequestHandler);
		}

		public void TestLoginPage()
		{
			AssertEquals("/Login/Login.aspx", TrackingGlobal.LoginPage);
		}

		public void TestOrdersPage()
		{
			AssertEquals("/Orders/Orders.aspx", TrackingGlobal.OrdersPage);
		}

		public void TestOrderDetailsPage()
		{
			AssertEquals("/Orders/OrderDetails.aspx", TrackingGlobal.OrderDetailsPage);
		}

		public void TestEditOrderPage()
		{
			AssertEquals("/Orders/EditOrder.aspx", TrackingGlobal.EditOrderPage);
		}

		public void TestShipmentsPage()
		{
			AssertEquals("/Shipments/Shipments.aspx", TrackingGlobal.ShipmentsPage);
		}

		public void TestShipmentPage()
		{
			AssertEquals("/Shipments/Shipment.aspx", TrackingGlobal.ShipmentPage);
		}

		public void TestShipmentDetailsPage()
		{
			AssertEquals("/Shipments/ShipmentDetails.aspx", TrackingGlobal.ShipmentDetailsPage);
		}

		public void TestCFSShipmentsPage()
		{
			AssertEquals("/CFSShipments/CFSShipments.aspx", TrackingGlobal.CFSShipmentsPage);
		}

		public void TestCFSShipmentDetailsPage()
		{
			AssertEquals("/CFSShipments/CFSShipmentDetails.aspx", TrackingGlobal.CFSShipmentDetailsPage);
		}

		public void TestDeclarationDetailsPage()
		{
			AssertEquals("/Declaration/DeclarationDetails.aspx", TrackingGlobal.DeclarationDetailsPage);
		}

		public void TestQuotationPage()
		{
			AssertEquals("/Quotes/Quotation.aspx", TrackingGlobal.QuotationPage);
		}

		public void TestTransactionsPage()
		{
			AssertEquals("/Accounts/Transactions.aspx", TrackingGlobal.TransactionsPage);
		}

		public void TestBookingsPage()
		{
			AssertEquals("/Bookings/Bookings.aspx", TrackingGlobal.BookingsPage);
		}

		public void TestEditBookingPage()
		{
			AssertEquals("/Bookings/EditBooking.aspx", TrackingGlobal.EditBookingPage);
		}

		public void TestBookingDetailsPage()
		{
			AssertEquals("/Bookings/BookingDetails.aspx", TrackingGlobal.BookingDetailsPage);
		}

		public void TestLinerAndAgencyBookingsPage()
		{
			AssertEquals("/LinerAndAgency/Bookings/Bookings.aspx", TrackingGlobal.LinerAndAgencyBookingsPage);
		}

		public void TestLinerAndAgencyBookingDetailsPage()
		{
			AssertEquals("/LinerAndAgency/Bookings/BookingDetails.aspx", TrackingGlobal.LinerAndAgencyBookingDetailsPage);
		}

		public void TestLinerAndAgencyEditBookingPage()
		{
			AssertEquals("/LinerAndAgency/Bookings/EditBooking.aspx", TrackingGlobal.LinerAndAgencyEditBookingPage);
		}

		public void TestInventoryPage()
		{
			AssertEquals("/Warehousing/Inventory.aspx", TrackingGlobal.InventoryPage);
		}

		public void TestWarehouseOrders()
		{
			AssertEquals("/Warehousing/WarehouseOrders.aspx", TrackingGlobal.WarehouseOrders);
		}

		public void TestWarehouseOrderDetailsPage()
		{
			AssertEquals("/Warehousing/WarehouseOrderDetails.aspx", TrackingGlobal.WarehouseOrderDetailsPage);
		}

		public void TestWarehouseEditOrders()
		{
			AssertEquals("/Warehousing/EditWarehouseOrder.aspx", TrackingGlobal.WarehouseEditOrders);
		}

		public void TestWarehouseReceive()
		{
			AssertEquals("/Warehousing/WarehouseReceipts.aspx", TrackingGlobal.WarehouseReceipts);
		}

		public void TestWarehouseEditReceive()
		{
			AssertEquals("/Warehousing/EditWarehouseReceive.aspx", TrackingGlobal.WarehouseEditReceive);
		}

		public void TestWarehouseReceiveDetails()
		{
			AssertEquals("/Warehousing/WarehouseReceiveDetails.aspx", TrackingGlobal.WarehouseReceiveDetails);
		}

		public void TestProductProfiles()
		{
			AssertEquals("/Warehousing/OrgSupplierParts.aspx", TrackingGlobal.ProductProfiles);
		}

		public void TestProductProfileDetailsPage()
		{
			AssertEquals("/Warehousing/ProductDetails.aspx", TrackingGlobal.ProductProfileDetailsPage);
		}

		public void TestPWarehouseOrderLineAllocationPage()
		{
			AssertEquals("/Warehousing/WhsOrderLineAllocation.aspx", TrackingGlobal.WarehouseOrderLineAllocationPage);
		}

		public void TestChangePasswordPage()
		{
			AssertEquals("/Admin/ChangePassword.aspx", TrackingGlobal.ChangePasswordPage);
		}

		public void TestGlowRedirectPage()
		{
			AssertEquals("/Glow/GlowRedirect.aspx", TrackingGlobal.GlowRedirectPage);
		}

		#endregion

		public void TestApplicationCookieName()
		{
			AssertEquals("EDIWebTracker", TrackingGlobal.ApplicationCookie.CookieName);
		}

		public void TestILicenceUsageLogWriter()
		{
			Globals.IsWeb = true;
			try
			{
				ILicenceUsageLogWriter logWriter = TrackingGlobal;
				AssertNotNull("Should implement ILicenceUsageLogWriter", logWriter);

				BusinessObjectFactory factory = new BusinessObjectFactory();
				OrgContact contact = factory.NewWithValidTestData<OrgContact>();
				contact.OC_Email = "a@b.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("pass");
				factory.Save();

				ZQuery logQuery = new ZQuery(StmActivityLogSchema.S7_ControllerID, SQLComparisonOperator.StartsWith, LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString());
				int originalCount = factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);

				TrackingGlobal.SiteUser.Login(contact.OrganisationCode, contact.OC_Email, contact.PasswordForTesting);
				Assert("User should be LoggedIn", TrackingGlobal.SiteUser.IsLoggedIn);
				AssertNotNull("Session should exist", HttpContext.Current.Session);

				AssertNull("Logging should not be marked", HttpContext.Current.Session["LicenceWEB"]);

				logWriter.WriteLicenceUsageLog(Env.Licence.WebTracker);
				AssertEquals("One more Log record", originalCount + 1, factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
				AssertNotNull("Logging should be marked", HttpContext.Current.Session["LicenceWEB"]);

				logWriter.WriteLicenceUsageLog(Env.Licence.WebTracker);
				AssertEquals("No more Log records", originalCount + 1, factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
				AssertNotNull("Logging should be marked", HttpContext.Current.Session["LicenceWEB"]);
			}
			finally
			{
				Globals.IsWeb = false;
			}
		}

		#region TestTypeSubstitutions

		public void TestTypeSubstitutions()
		{
			ClearSubstitionsForTest();

			BusinessObjectFactory factory = new BusinessObjectFactory();

			AssertEquals(typeof(Freight.Forwarding.Business.ForwardingShipment), factory.New<Freight.Forwarding.Business.ForwardingShipment>().GetType());
			AssertEquals(typeof(Freight.Forwarding.Business.ForwardingShipmentProcessTask), factory.New<Freight.Forwarding.Business.ForwardingShipmentProcessTask>().GetType());

			AssertEquals(typeof(Freight.Forwarding.Orders.Business.Order), factory.New<Freight.Forwarding.Orders.Business.Order>().GetType());
			AssertEquals(typeof(Freight.Forwarding.Orders.Business.OrderProcessTasks), factory.New<Freight.Forwarding.Orders.Business.OrderProcessTasks>().GetType());

			TrackingGlobal.InitializeTypeSubstitutions();

			AssertEquals(typeof(TrackingShipment), factory.New<Freight.Forwarding.Business.ForwardingShipment>().GetType());
			AssertEquals(typeof(TrackingShipmentProcessTask), factory.New<Freight.Forwarding.Business.ForwardingShipmentProcessTask>().GetType());

			AssertEquals(typeof(TrackingOrder), factory.New<Freight.Forwarding.Orders.Business.Order>().GetType());
			AssertEquals(typeof(TrackingOrderProcessTasks), factory.New<Freight.Forwarding.Orders.Business.OrderProcessTasks>().GetType());

			AssertEquals(typeof(TrackingBillOfLading), factory.New<Freight.Agency.Business.BillOfLading>().GetType());
			AssertEquals(typeof(TrackingLinerAndAgencyBooking), factory.New<Freight.Agency.Business.AgencyBooking>().GetType());

			ClearSubstitionsForTest();
		}

		void ClearSubstitionsForTest()
		{
			TypeDecider.RemoveSubstitution(typeof(Freight.Forwarding.Business.ForwardingShipment));
			TypeDecider.RemoveSubstitution(typeof(Freight.Forwarding.Business.ForwardingShipmentProcessTask));
			TypeDecider.RemoveSubstitution(typeof(Freight.Forwarding.Orders.Business.Order));
			TypeDecider.RemoveSubstitution(typeof(Freight.Forwarding.Orders.Business.OrderProcessTasks));
			TypeDecider.RemoveSubstitution(typeof(Freight.Agency.Business.BillOfLading));
			TypeDecider.RemoveSubstitution(typeof(Freight.Agency.Business.AgencyBooking));
		}

		#endregion

		public void TestApplicationError()
		{
			var global = (TestGlobal)GetNewZGlobalForTesting();
			ZArchitecture.Web.GUI.Testing.TestGlobal.InitZGlobalWithHttpContext(global);
			global.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			Assert("Precondition", global.SiteUser.IsLoggedIn && ((TrackingSiteUser)global.SiteUser).IsShipmentQuickViewUser);

			HttpContext.Current.AddError(new ViewStateException());
			global.ApplicationErrorForTesting(null, EventArgs.Empty);

			Assert("Error should log out ShipmentQuickViewUser", !global.SiteUser.IsLoggedIn);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
		}

		public void TestApplicationError_NoSiteUser()
		{
			var global = new TestGlobal_NoSiteUser();
			ZArchitecture.Web.GUI.Testing.TestGlobal.InitZGlobalWithHttpContext(global);

			HttpContext.Current.AddError(new ViewStateException());
			global.ApplicationErrorForTesting(null, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
		}

		public void TestRenameServerHeaderValue()
		{
			var global = new TestGlobal_NoSiteUser();
			global.HeadersForTesting.Add("Server", "IIS Header");

			global.ApplicationPreSendRequestHeadersForTesting();
			AssertEquals("Should change the server header value", "Web Server", global.HeadersForTesting["Server"]);
		}

		#region Implementation

		Global TrackingGlobal
		{
			get
			{
				if (fTrackingGlobal == null)
				{
					fTrackingGlobal = (Global)GetNewZGlobalForTesting();
				}
				return fTrackingGlobal;
			}
		}

		Global fTrackingGlobal;

		protected override ZGlobal GetNewZGlobalForTesting()
		{
			return new TestGlobal();
		}

		#endregion
	}
}
