using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection))]
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationCollectionTest :
		RegistryBusinessObjectCollectionTemplateTestCase<AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection GetCollectionToTest()
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AutoratingIntercompanyTariffsForGatewayJobConfiguration();
		}

		protected new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection Collection
		{
			get { return base.Collection; }
		}
	}
}
