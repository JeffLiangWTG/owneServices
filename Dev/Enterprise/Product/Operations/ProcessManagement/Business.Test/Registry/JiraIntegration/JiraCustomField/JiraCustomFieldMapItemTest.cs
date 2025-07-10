using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(JiraCustomFieldMapItem))]
	class JiraCustomFieldMapItemTest : RegistryBusinessObjectTemplateTestCase<JiraCustomFieldMapItem>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override JiraCustomFieldMapItem GetBusinessObjectToClone()
			=> new JiraCustomFieldMap().JiraClassificationMap.AddNew();

		protected override JiraCustomFieldMapItem GetBusinessObjectToSerialise()
			=> GetBusinessObjectToClone();
	}
}
