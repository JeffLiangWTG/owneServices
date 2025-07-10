using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(UCMEDIMessageTestTypeCollection))]
	sealed class UCMEDIMessageTestTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<UCMEDIMessageTestTypeCollection>
	{
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => false;
		protected override UCMEDIMessageTestTypeCollection GetCollectionToTest() => new UCMEDIMessageTestTypeCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection() => new UCMEDIMessageTestType();
	}
}
