using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(TallyPackLine))]
	public class TallyPackLineBusinessObjectTest : CFSBusinessObjectTestCase
	{
		public void TestPackLocations()
		{
			TallyPackLine line = (TallyPackLine)GetNewBusinessObject();

			AssertNotNull(line.PackLocations);
			AssertEquals("TallyPackLine.PackLocations should be of type TallyPackLocationCollection.", typeof(TallyPackLocationCollection), ((PackLine)line).PackLocations.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var packLine = factory.NewWithValidTestData<TallyPackLine>();
			packLine.PackLocations.AddNew();

			return packLine;
		}

		#endregion
	}
}
