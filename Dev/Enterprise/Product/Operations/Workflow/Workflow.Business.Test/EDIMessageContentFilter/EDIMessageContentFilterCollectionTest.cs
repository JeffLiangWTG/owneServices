
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilterCollection))]
	class EDIMessageContentFilterCollectionTest : ActiveBusinessObjectCollectionTestCase<EDIMessageContentFilterCollection>
	{
		protected override EDIMessageContentFilterCollection GetCollectionToTest() => new EDIMessageContentFilterCollection(Factory);
	}
}
