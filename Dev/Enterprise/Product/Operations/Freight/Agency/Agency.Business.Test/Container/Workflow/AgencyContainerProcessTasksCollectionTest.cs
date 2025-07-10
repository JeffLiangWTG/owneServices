using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyContainerProcessTaskCollection))]
	internal class AgencyContainerProcessTasksCollectionTest : ProcessTaskCollectionTest<AgencyContainerProcessTaskCollection>
	{
		#region Implementation

		protected override AgencyContainerProcessTaskCollection GetCollectionToTestCore()
		{
			return new AgencyContainerProcessTaskCollection(Container);
		}

		AgencyShipmentContainer Container
		{
			get
			{
				return container ??= Factory.NewWithValidTestData<AgencyShipmentContainer>();
			}
		}
		AgencyShipmentContainer container;

		#endregion
	}
}
