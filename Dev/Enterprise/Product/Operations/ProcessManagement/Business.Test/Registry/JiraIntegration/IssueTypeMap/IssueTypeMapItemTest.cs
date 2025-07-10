using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(IssueTypeMapItem))]
	class IssueTypeMapItemTest : RegistryBusinessObjectTemplateTestCase<IssueTypeMapItem>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override IssueTypeMapItem GetBusinessObjectToClone()
		{
			return new IssueTypeMap().JiraClassificationMap.AddNew();
		}

		protected override IssueTypeMapItem GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
