using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Business.Testing
{
	[TestedType(typeof(AutoFumigationFilterBusinessObject))]
	public class AutoFumigationFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		protected AutoFumigationFilterBusinessObject FilterObject => filterObject ?? (filterObject = (AutoFumigationFilterBusinessObject)GetNewBusinessObject());
		AutoFumigationFilterBusinessObject filterObject;

		public void TestFilter_DoesNotExceedMaxLengths()
		{
			var filter = FilterObject;
			var expectedContainerNum = new string('A', Autovw_List_Fumigation.Schema.LFV_ContainerNumMaxLength);
			var expectedVessel = new string('A', Autovw_List_Fumigation.Schema.LFV_VesselMaxLength);
			var expectedVoyageFlight = new string('A', Autovw_List_Fumigation.Schema.LFV_VoyageFlightMaxLength);

			filter.LFV_ContainerNum = expectedContainerNum + "A";
			filter.LFV_Vessel = expectedVessel + "A";
			filter.LFV_VoyageFlight = expectedVoyageFlight + "A";

			var expectedSubFilter = $"LFV_ContainerNum = '{expectedContainerNum}' and LFV_Vessel = '{expectedVessel}' and LFV_VoyageFlight = '{expectedVoyageFlight}'";
			AssertContains(expectedSubFilter, filter.Filter.LiteralTextADO);
		}
	}
}
