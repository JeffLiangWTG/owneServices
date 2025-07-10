using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFNumberCustomisation))]
	sealed class ISFNumberCustomisationTest : RegistryBusinessObjectTemplateTestCase<ISFNumberCustomisation>
	{
		protected override ISFNumberCustomisation GetBusinessObjectToClone() => new ISFNumberCustomisation();

		protected override ISFNumberCustomisation GetBusinessObjectToSerialise() => new ISFNumberCustomisation();

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject() => new ISFNumberCustomisation();
	}
}
