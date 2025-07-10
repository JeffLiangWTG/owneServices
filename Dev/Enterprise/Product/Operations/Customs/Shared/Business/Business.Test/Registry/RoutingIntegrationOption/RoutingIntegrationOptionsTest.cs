using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(RoutingIntegrationOptions))]
	sealed class RoutingIntegrationOptionsTest : RegistryBusinessObjectTemplateTestCase<RoutingIntegrationOptions>
	{
		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override RoutingIntegrationOptions GetBusinessObjectToClone()
		{
			var option = new RoutingIntegrationOptions();
			option.NeverLink = true;
			return option;
		}

		protected override RoutingIntegrationOptions GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
