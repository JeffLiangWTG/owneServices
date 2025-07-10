using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EUTaxIDDefaultingRuleCollection))]
	sealed class EUTaxIDDefaultingRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EUTaxIDDefaultingRuleCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, new EUTaxIDDefaultingRuleCollection().AllowNew);
		}

		#region Implementation

		protected override EUTaxIDDefaultingRuleCollection GetCollectionToTest()
		{
			return new EUTaxIDDefaultingRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EUTaxIDDefaultingRule();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
