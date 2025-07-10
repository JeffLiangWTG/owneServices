using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(RecipientSourceFallbackHeader))]
	class RecipientSourceFallbackHeaderTest : RegistryBusinessObjectTemplateTestCase<RecipientSourceFallbackHeader>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RecipientSourceFallbackHeader GetBusinessObjectToClone() => new RecipientSourceFallbackHeader();

		protected override RecipientSourceFallbackHeader GetBusinessObjectToSerialise() => new RecipientSourceFallbackHeader();
	}
}
