using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EUTaxIDDefaultingRule))]
	sealed class EUTaxIDDefaultingRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new EUTaxIDDefaultingRule();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EUTaxIDDefaultingRule();
		}
	}
}
