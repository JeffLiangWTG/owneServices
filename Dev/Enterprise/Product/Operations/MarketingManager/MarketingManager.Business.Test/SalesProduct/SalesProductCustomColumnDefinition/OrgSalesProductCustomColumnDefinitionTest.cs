using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgSalesProductCustomColumnDefinition))]
	sealed class OrgSalesProductCustomColumnDefinitionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var salesProductColDef = Factory.New<OrgSalesProductCustomColumnDefinition>();
			AssertEquals(OrgSalesProductSchema.Constants.Prefix, salesProductColDef.XC_ParentTableCode);
		}

		public void TestValidation()
		{
			var salesProductColDef = Factory.New<OrgSalesProductCustomColumnDefinition>();
			AssertType(typeof(OrgSalesProductCustomColumnDefinitionValidation), salesProductColDef.Validation);
		}

		public void TestGetNewLookups()
		{
			var salesProductColDef = Factory.New<OrgSalesProductCustomColumnDefinition>();
			AssertType(typeof(OrgSalesProductCustomColumnDefinitionLookups), salesProductColDef.Lookups);
		}
	}
}
