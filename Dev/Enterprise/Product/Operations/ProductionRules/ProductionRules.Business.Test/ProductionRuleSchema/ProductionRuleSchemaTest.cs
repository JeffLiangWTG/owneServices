using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProductionRules.Business.Testing
{
	class ProductionRuleSchemaTest : TestCase
	{
		public void TestProductionRuleSetPLSNameSize()
		{
			AssertEquals(60, ProductionRuleSetSchema.PRS_Name.MaxLength);
		}

		public void TestProductionRulePRLNameSize()
		{
			AssertEquals(60, ProductionRuleSchema.PRL_Name.MaxLength);
		}
		public void TestGenCustomColumnDefinitionXCNameSize()
		{
			AssertEquals(60, GenCustomColumnDefinitionSchema.XC_Name.MaxLength);
		}
		public void TestGenCustomAddOnValueXVNameSize()
		{
			AssertEquals(60, GenCustomAddOnValueSchema.XV_Name.MaxLength);
		}
	}
}
