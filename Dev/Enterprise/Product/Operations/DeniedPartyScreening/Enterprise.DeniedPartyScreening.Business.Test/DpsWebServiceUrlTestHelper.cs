using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	static class DpsWebServiceUrlTestHelper
	{
		internal static DpsWebServiceItemCollection SetRegistryUrl(string url)
		{
			var testUrl1 = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}";
			var testUrl2 = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}";
			var collection = new DpsWebServiceItemCollection
			{
				new DpsWebServiceItem()
				{
					Code = "SYD1", WebServiceUrl = (NoResString)testUrl1, Role = RoleHelper.Code.Production
				},
				new DpsWebServiceItem()
				{
					Code = "STG1", WebServiceUrl = (NoResString)url, Role = RoleHelper.Code.Staging
				},
				new DpsWebServiceItem()
				{
					Code = "SYD2", WebServiceUrl = (NoResString)testUrl2, Role = RoleHelper.Code.ProductionFailover
				}
			};

			return collection;
		}
	}
}
