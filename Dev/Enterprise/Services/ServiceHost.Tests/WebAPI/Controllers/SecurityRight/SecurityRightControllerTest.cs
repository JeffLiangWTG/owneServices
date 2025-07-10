#if NETFRAMEWORK
using System.Web.Http;
using System.Web.Http.Results;
#elif NET
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.NetCore;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class SecurityRightControllerTest : TestCaseWithFactory
	{
		public void TestGetRefDocTypeSecurities()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResult(controller.GetRefDocTypeSecurities()).ToArray();

				AssertEquals(1, result.Count(r => r.Description == "Document XZY for ABC (Do Stuff)"));
				AssertEquals(0, result.Count(r => r.Description == "RepHRReports: Test HR Report"));
				AssertEquals(0, result.Count(r => r.Description == "RepCustomsReport: Test custom report"));
			}
		}

		public void TestGetReportSecurities()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResult(controller.GetReportSecurities()).ToArray();

				AssertEquals(0, result.Count(r => r.Description == "Document XZY for ABC (Do Stuff)"));
				AssertEquals(1, result.Count(r => r.Description == "RepHRReports: Test HR Report"));
				AssertEquals(1, result.Count(r => r.Description == "RepCustomsReport: Test custom report"));
			}
		}

		public void TestGetRefDocTypeSecuritiesShouldBeInAlphabeticalOrder()
		{
			var newDocType = Factory.NewWithValidTestData<RefDocType>();
			newDocType.RT_DocType = "AAA";
			newDocType.RT_ReferenceType = "ABC";
			newDocType.RT_Desc = "Do Stuff";
			Factory.Save();

			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResult(controller.GetRefDocTypeSecurities()).ToArray();

				AssertEquals("Results should be returned in alphabetical order", "RefDocType:ABC:AAA", result[0].Code);
				AssertEquals("Results should be returned in alphabetical order", "RefDocType:ABC:XZY", result[1].Code);
			}
		}

		public void TestGetReportSecuritiesShouldBeInAlphabeticalOrder()
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepHRReports";
			report.SU_MenuName = "AAA Test HR Report";
			report.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			Factory.Save();

			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResult(controller.GetReportSecurities()).ToArray();

				var testResults = result.Where(x => x.Description.EndsWith("Test HR Report")).ToArray();
				AssertEquals("Precondition: Should only return the records created in this test", 2, testResults.Length);
				AssertEquals("Results should be returned in alphabetical order", "RepHRReports: AAA Test HR Report", testResults[0].Description);
				AssertEquals("Results should be returned in alphabetical order", "RepHRReports: Test HR Report", testResults[1].Description);
			}
		}

#if NETFRAMEWORK
		protected CodeDescriptionPairList GetUrlResult(IHttpActionResult result)
		{
			if (result is OkNegotiatedContentResult<CodeDescriptionPairList> okResult)
			{
				return okResult.Content;
			}
			throw new InvalidCastException("The result is not of type OkNegotiatedContentResult<CodeDescriptionPairList>");
		}
#elif NET
		protected CodeDescriptionPairList GetUrlResult(IActionResult result)
		{
			if (result is ObjectResult objectResult)
			{
				return objectResult.Value as CodeDescriptionPairList;
			}
			throw new InvalidCastException("The result is not of type ObjectResult");
		}
#endif

		void CreateSecurityRights()
		{
			var newDocType = Factory.NewWithValidTestData<RefDocType>();
			newDocType.RT_DocType = "XZY";
			newDocType.RT_ReferenceType = "ABC";
			newDocType.RT_Desc = "Do Stuff";

			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepHRReports";
			report.SU_MenuName = "Test HR Report";
			report.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;

			var command = Factory.New<ReportCommand>();
			command.SU_BusinessContext = "RepCustomsReport";
			command.SU_MenuName = "Test custom report";
			command.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;

			Factory.Save();
		}

		protected override void SetUp()
		{
			controller = new SecurityRightController();
			userPk = Guid.NewGuid();

			identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(true);
			identityMock.SetupGet(x => x.ProviderType).Returns("OC");
			identityMock.SetupGet(x => x.ProviderKey).Returns(userPk);
#if NETFRAMEWORK
			controller.User = new GenericPrincipal(identityMock.Object, null);
#elif NET
			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = new GenericPrincipal(identityMock.Object, null) }
			};
#endif

			CreateSecurityRights();
		}

		SecurityRightController controller;
		static Guid userPk;
		Mock<IGlowAuthenticationTicketIdentity> identityMock;

		class MockClientHookForTypeDeciderTest : IClientHook
		{
			public ITypeDeciderDictionary ClientTypeDeciders
			{
				get
				{
					return new TypeDeciderDictionary(new Dictionary<Type, ITypeDecider>());
				}
			}

			#region IClientHook Members

			bool IClientHook.IsInitialised
			{
				get { return IsInitialised; }
			}
			bool IsInitialised;

			void IClientHook.Initialise()
			{
				IsInitialised = true;
			}

			void IClientHook.Initialise(bool loggedIn)
			{
				IsInitialised = true;
			}

			void IClientHook.Uninitialise()
			{
				IsInitialised = false;
			}

			object IClientHook.GetTableSchema(string tableName)
			{
				return null;
			}

			bool IClientHook.HasCompanySpecificOverrides { get { return false; } }

			#endregion

			public string UniqueId
			{
				get { return string.Empty; }
			}

			public bool IsUpgrading { get; set; }
		}
	}
}
