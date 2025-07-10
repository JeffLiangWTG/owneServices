using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Business.Testing
{
	[TestedType(typeof(AutoContainerAvailabilityFilterBusinessObject))]
	public class AutoContainerAvailabilityFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		protected AutoContainerAvailabilityFilterBusinessObject FilterObject => filterObject ?? (filterObject = (AutoContainerAvailabilityFilterBusinessObject)GetNewBusinessObject());
		AutoContainerAvailabilityFilterBusinessObject filterObject;

		public void TestFilter_DoesNotExceedMaxLengths()
		{
			var filter = FilterObject;
			var expectedContainerNum = new string('A', Autovw_List_ContainerAvailability.Schema.LCV_ContainerNumMaxLength);
			var expectedVessel = new string('A', Autovw_List_ContainerAvailability.Schema.LCV_VesselMaxLength);
			var expectedVoyageFlight = new string('A', Autovw_List_ContainerAvailability.Schema.LCV_VoyageFlightMaxLength);

			filter.LCV_ContainerNum = expectedContainerNum + "A";
			filter.LCV_Vessel = expectedVessel + "A";
			filter.LCV_VoyageFlight = expectedVoyageFlight + "A";

			var expectedSubFilter = $"LCV_ContainerNum = '{expectedContainerNum}' and LCV_Vessel = '{expectedVessel}' and LCV_VoyageFlight = '{expectedVoyageFlight}'";
			AssertContains(expectedSubFilter, filter.Filter.LiteralTextADO);
		}
	}
}
