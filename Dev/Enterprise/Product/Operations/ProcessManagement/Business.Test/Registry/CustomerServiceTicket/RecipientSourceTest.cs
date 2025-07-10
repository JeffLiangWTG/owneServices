using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(RecipientSource))]
	class RecipientSourceTest : RegistryBusinessObjectTemplateTestCase<RecipientSource>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RecipientSource GetBusinessObjectToClone()
		{
			return new RecipientSourceFallbackHeader().SourceCollection.AddNew();
		}

		protected override RecipientSource GetBusinessObjectToSerialise()
		{
			return new RecipientSourceFallbackHeader().SourceCollection.AddNew();
		}
	}
}
