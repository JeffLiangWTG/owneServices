using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveProcessTaskTemplateCollection))]
	class ActiveProcessTaskTemplateCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveProcessTaskTemplateCollection>
	{
		protected override ActiveProcessTaskTemplateCollection GetCollectionToTest() => new ActiveProcessTaskTemplateCollection(Factory, new ZQuery());
	}
}
