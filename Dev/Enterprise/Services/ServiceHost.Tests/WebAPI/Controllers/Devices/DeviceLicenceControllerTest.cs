using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class DeviceLicenceControllerTest : TransactionedTestCase
	{
		public void TestGetEnterpriseCodeList()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResult(controller.GetEnterpriseCodeList());
				AssertEquals("ENT1=My first organization|ENT2=My second organization", result);
			}
		}

		public void TestGetServerCodeList()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var result = GetUrlResult(controller.GetServerCodeList("ENT1"));
				AssertEquals("SVR1=123 Test Street|SVR2=456 Unit Way", result);
			}
		}

		public void TestGetCustomer()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var response = controller.GetCustomer("ENT1", "SVR1");
				var result = ((JsonResult<Guid>)response).Content;
				AssertEquals(new Guid("B17BAE72-D66F-44C7-94F9-6FAD5B546E83"), result);
			}
		}

		public void TestGetCustomer_NoCustomer()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				var response = controller.GetCustomer("ENT1", "SVR2");
				var result = ((JsonResult<Guid>)response).Content;
				AssertEquals(Guid.Empty, result);
			}
		}

		protected string GetUrlResult(IHttpActionResult result)
		{
			var items = ((JsonResult<IEnumerable<UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair>>)result).Content;
			return string.Join("|", items.Select(i => $"{i.Code}={i.Description}"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			controller = new DeviceLicenceController();
		}

		DeviceLicenceController controller;

		class MockClientHookForTypeDeciderTest : IClientHook
		{
			public class MyTypeDecider : TypeDecider
			{
				public override Type GetTypeForNew() => typeof(StubDeviceLicenceService);

				public override Type GetTypeForBinding() => throw new NotImplementedException();

				public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => throw new NotImplementedException();
			}

			public ITypeDeciderDictionary ClientTypeDeciders
			{
				get
				{
					var result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(IDeviceLicenceService), new MyTypeDecider());
					return new TypeDeciderDictionary(result);
				}
			}

			public string UniqueId => string.Empty;

			public bool IsInitialised { get; private set; }

			public bool HasCompanySpecificOverrides => throw new NotImplementedException();

			public bool IsUpgrading { get; set; }

			public object GetTableSchema(string tableName) => null;

			public void Initialise() => IsInitialised = true;

			public void Initialise(bool loggedIn) => IsInitialised = true;

			public void Uninitialise() => IsInitialised = false;
		}

		class StubDeviceLicenceService : IDeviceLicenceService
		{
			public IEnumerable<CodeDescriptionPair> GetEnterpriseCodeList()
			{
				yield return new CodeDescriptionPair("ENT1", "My first organization");
				yield return new CodeDescriptionPair("ENT2", "My second organization");
			}

			public IEnumerable<CodeDescriptionPair> GetServerCodeList(string enterpriseCode)
			{
				if (enterpriseCode == "ENT1")
				{
					yield return new CodeDescriptionPair("SVR1", "123 Test Street");
					yield return new CodeDescriptionPair("SVR2", "456 Unit Way");
				}
			}

			public Guid GetCustomer(string enterpriseCode, string serverCode)
			{
				if (enterpriseCode == "ENT1" && serverCode == "SVR1")
				{
					return new Guid("B17BAE72-D66F-44C7-94F9-6FAD5B546E83");
				}
				return Guid.Empty;
			}
		}
	}
}
