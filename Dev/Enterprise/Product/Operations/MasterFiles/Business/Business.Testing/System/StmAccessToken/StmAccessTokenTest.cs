using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmAccessToken))]
	sealed class StmAccessTokenTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var token = (StmAccessToken)base.GetNewBusinessObjectForDeleteTest(factory);
			token.SAT_Type = "SIT";
			return token;
		}
	}
}
