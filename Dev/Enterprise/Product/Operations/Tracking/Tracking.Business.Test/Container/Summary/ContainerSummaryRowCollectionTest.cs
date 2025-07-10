using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(ContainerSummaryRowCollection))]
	public class ContainerSummaryRowCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ContainerSummaryRowCollection>
	{
		protected override ContainerSummaryRowCollection GetCollectionToTest()
		{
			return new ContainerSummaryRowCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContainerSummaryRow(ContainerStatusList.Codes.OnOrder,
				ContainerStatusList.Descriptions.OnOrder);
		}
	}
}
