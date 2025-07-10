using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class IncidentRequestControllerTest : TransactionedTestCase
	{
		public void TestGetProductList()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResult(controller.GetProductList("EN-US"));

				AssertEquals(result, "ENT=Enterprise|SPH=Saphire|P&C=People & Culture");
			}
		}

		public void TestGetModuleList()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result1 = GetUrlResult(controller.GetModuleList("ENT", "CR5", "EN-US"));
				var result2 = GetUrlResult(controller.GetModuleList("SPH", "CR9", "EN-US"));
				var result3 = GetUrlResult(controller.GetModuleList("P&C", "CR*", "EN-US"));
				var result4 = GetUrlResult(controller.GetModuleList(string.Empty, string.Empty, "EN-US"));

				AssertEquals(result1, "ACC=Accounting|COR=Core");
				AssertEquals(result2, "FOR=Forwarding|PAY=Payroll");
				AssertEquals(result3, "MD1=Module 1|MD2=Module 2");
				AssertEquals(result4, "ACC=Accounting|COR=Core|FOR=Forwarding|PAY=Payroll");
			}
		}

		public void TestGetModuleList_NoLanguage()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				controller.Request = new HttpRequestMessage();
				controller.Request.Headers.Add(EnterpriseLanguageCodeHeaderKey, "CN");

				var result = GetUrlResult(controller.GetModuleList("ENT", "CR5"));

				AssertEquals(result, "OTH=LanguageIsFromRequest");
			}
		}

		public void TestGetModuleList_NoLanguageAndNoLanguageHeader()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				controller.Request = new HttpRequestMessage();

				var result = GetUrlResult(controller.GetModuleList("ENT", "CR5"));

				AssertEquals(result, "FBK=FinalFallback");
			}
		}

		public void TestGetProductList_NoLanguage()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				controller.Request = new HttpRequestMessage();
				controller.Request.Headers.Add(EnterpriseLanguageCodeHeaderKey, "CN");

				var result = GetUrlResult(controller.GetProductList());

				AssertEquals(result, "OTH=LanguageIsFromRequest");
			}
		}

		public void TestGetProductList_NoLanguageAndNoLanguageHeader()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				controller.Request = new HttpRequestMessage();

				var result = GetUrlResult(controller.GetProductList());

				AssertEquals(result, "FBK=FinalFallback");
			}
		}

		public void TestGetProductList_DifferentContact()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				identityMock.SetupGet(x => x.ProviderKey).Returns(Guid.NewGuid());

				var result = GetUrlResult(controller.GetProductList("EN-US"));

				AssertEquals(result, "UDF=Undefined");
			}
		}

		public void TestGetModuleList_DifferentContact()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				identityMock.SetupGet(x => x.ProviderKey).Returns(Guid.NewGuid());

				var result = GetUrlResult(controller.GetModuleList("ENT", "CR5", "EN-US"));

				AssertEquals(result, "UDF=Undefined");
			}
		}

		public void TestGetModuleList_Status()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result1 = GetUrlResult(controller.GetModuleList("A01", "CCC", "JPY", "CSV"));
				AssertEquals(result1, "INT=Internal USE ONLY");
			}
		}

		public void TestGetServiceTypeList()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result1 = GetUrlResult(controller.GetServiceTypeList("ENT", "CR5", "ARC", "S1"));
				var result2 = GetUrlResult(controller.GetServiceTypeList("SPH", "CR9", "INT", "S1"));
				var result3 = GetUrlResult(controller.GetServiceTypeList("ENT", "CR1", "INT", "S2"));
				var result4 = GetUrlResult(controller.GetServiceTypeList("ENT", "CR5", string.Empty, string.Empty));

				AssertEquals(result1, "ACC=Accounting|COR=Core");
				AssertEquals(result2, "FOR=Forwarding|PAY=Payroll");
				AssertEquals(result3, "MD1=Module 1|MD2=Module 2");
				AssertEquals(result4, "");
			}
		}

		public void TestGetFullProductList()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResult(controller.GetFullProductList());

				AssertEquals(result, "UDF=Undefined");
			}
		}

		public void TestGetFullModuleListByProduct()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResultForQuadrupleTuple(controller.GetFullModuleListByProduct());

				AssertEquals(result, "UDF,,Undefined,Undefined Description");
			}
		}

		public void TestNotificationWhenUnsubscribe()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = ((OkResult)controller.NotificationWhenUnsubscribe(new Guid(), "test", "test", new List<Guid>()));

				AssertNotNull(result);
			}
		}

		public void TestGetDocumentUrls()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResultForDictionary(controller.GetDocumentUrls(new List<string> { "test" }));

				AssertEquals("UDF=Undefined", result);
			}
		}

		protected string GetUrlResult(IHttpActionResult result)
		{
			var items = ((JsonResult<IEnumerable<CodeDescriptionPair>>)result).Content;

			return string.Join("|", items.Select(i => $"{i.Code}={i.Description}"));
		}

		protected string GetUrlResultForDictionary(IHttpActionResult result)
		{
			var items = ((JsonResult<Dictionary<string, string>>)result).Content;

			return string.Join("|", items.Select(i => $"{i.Key}={i.Value}"));
		}

		protected string GetUrlResultForQuadrupleTuple(IHttpActionResult result)
		{
			var items = ((JsonResult<IEnumerable<Tuple<string, string, string, string>>>)result).Content;

			return string.Join("|", items.Select(i => $"{i.Item1},{i.Item2},{i.Item3},{i.Item4}"));
		}

		protected override void SetUp()
		{
			controller = new IncidentRequestController();
			userPk = Guid.NewGuid();

			identityMock = new Mock<IGlowAuthenticationTicketIdentity>();
			identityMock.SetupGet(x => x.IsAuthenticated).Returns(true);
			identityMock.SetupGet(x => x.ProviderType).Returns("OC");
			identityMock.SetupGet(x => x.ProviderKey).Returns(userPk);
			controller.User = new GenericPrincipal(identityMock.Object, null);
		}

		IncidentRequestController controller;
		static Guid userPk;
		Mock<IGlowAuthenticationTicketIdentity> identityMock;

		class MockClientHookForTypeDeciderTest : IClientHook
		{
			public class MyTypeDecider : TypeDecider
			{
				public override Type GetTypeForNew()
				{
					return typeof(StubIncidentRequestService);
				}

				public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
				{
					throw new NotImplementedException();
				}

				public override Type GetTypeForBinding()
				{
					throw new NotImplementedException();
				}
			}

			public ITypeDeciderDictionary ClientTypeDeciders
			{
				get
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(IIncidentRequestService), new MyTypeDecider());
					return new TypeDeciderDictionary(result);
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

		class StubIncidentRequestService : IIncidentRequestService
		{
			public StubIncidentRequestService()
			{
			}

			public IEnumerable<Tuple<string, string>> GetModuleList(string product, string criticality, string language, string status, Guid contactPk)
			{
				if (contactPk != userPk)
				{
					yield return Tuple.Create("UDF", "Undefined");
				}
				else
				{
					if (product == "ENT" && criticality == "CR5" && language == "EN-US")
					{
						yield return Tuple.Create("ACC", "Accounting");
						yield return Tuple.Create("COR", "Core");
					}

					if (product == "SPH" && criticality == "CR9" && language == "EN-US")
					{
						yield return Tuple.Create("FOR", "Forwarding");
						yield return Tuple.Create("PAY", "Payroll");
					}

					if (product == "P&C" && criticality == "CR*" && language == "EN-US")
					{
						yield return Tuple.Create("MD1", "Module 1");
						yield return Tuple.Create("MD2", "Module 2");
					}

					if (string.IsNullOrEmpty(product) && string.IsNullOrEmpty(criticality) && language == "EN-US")
					{
						yield return Tuple.Create("ACC", "Accounting");
						yield return Tuple.Create("COR", "Core");
						yield return Tuple.Create("FOR", "Forwarding");
						yield return Tuple.Create("PAY", "Payroll");
					}

					if (language == "CN")
					{
						yield return Tuple.Create("OTH", "LanguageIsFromRequest");
					}
					else if (language == DefaultLanguageCode)
					{
						yield return Tuple.Create("FBK", "FinalFallback");
					}

					if (status == "CSV")
					{
						yield return Tuple.Create("INT", "Internal USE ONLY");
					}
				}
			}

			public IEnumerable<Tuple<string, string>> GetProductList(string language, Guid contactPk)
			{
				if (contactPk != userPk)
				{
					yield return Tuple.Create("UDF", "Undefined");
				}
				else if (language == "EN-US")
				{
					yield return Tuple.Create("ENT", "Enterprise");
					yield return Tuple.Create("SPH", "Saphire");
					yield return Tuple.Create("P&C", "People & Culture");
				}
				else if (language == "CN")
				{
					yield return Tuple.Create("OTH", "LanguageIsFromRequest");
				}
				else if (language == DefaultLanguageCode)
				{
					yield return Tuple.Create("FBK", "FinalFallback");
				}
			}

			public IEnumerable<Tuple<string, string>> GetServiceTypeList(string product, string criticality, string module, string sourceModuleId)
			{
				if (product == "ENT" && criticality == "CR5" && module == "ARC" && sourceModuleId == "S1")
				{
					yield return Tuple.Create("ACC", "Accounting");
					yield return Tuple.Create("COR", "Core");
				}

				if (product == "SPH" && criticality == "CR9" && module == "INT" && sourceModuleId == "S1")
				{
					yield return Tuple.Create("FOR", "Forwarding");
					yield return Tuple.Create("PAY", "Payroll");
				}

				if (product == "ENT" && criticality == "CR1" && module == "INT" && sourceModuleId == "S2")
				{
					yield return Tuple.Create("MD1", "Module 1");
					yield return Tuple.Create("MD2", "Module 2");
				}
			}

			public IEnumerable<Tuple<string, string, string, string>> GetFullModuleListByProduct()
			{
				return new Tuple<string, string, string, string>[] { Tuple.Create("UDF", string.Empty, "Undefined", "Undefined Description") }; // Need a default list in generic builds
			}

			public IEnumerable<Tuple<string, string>> GetFullProductList()
			{
				return new Tuple<string, string>[] { Tuple.Create("UDF", "Undefined") }; // Need a default list in generic builds
			}

			public void NotificationWhenUnsubscribe(Guid jobPK, string userType, ICollection<Guid> userPKs, string email)
			{
			}

			public Dictionary<string, string> GetDocumentUrls(Guid contactPk, ICollection<string> documentIds)
			{
				var result = new Dictionary<string, string>();
				result.Add("UDF", "Undefined");
				return result; // Need a default list in generic builds
			}
		}

		const string DefaultLanguageCode = "EN";
		const string EnterpriseLanguageCodeHeaderKey = "Enterprise-Language-Code";
	}
}
