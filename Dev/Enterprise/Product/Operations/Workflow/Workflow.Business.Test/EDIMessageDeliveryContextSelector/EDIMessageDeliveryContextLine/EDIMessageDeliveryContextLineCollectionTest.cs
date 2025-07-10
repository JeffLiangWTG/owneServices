using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageDeliveryContextLineCollection))]
	public class EDIMessageDeliveryContextLineCollectionTest : ActiveBusinessObjectCollectionTestCase<EDIMessageDeliveryContextLineCollection>
	{
		protected override EDIMessageDeliveryContextLineCollection GetCollectionToTest() => new EDIMessageDeliveryContextLineCollection(Factory.New<EDIMessageDeliveryContextSelector>());
	}
}
