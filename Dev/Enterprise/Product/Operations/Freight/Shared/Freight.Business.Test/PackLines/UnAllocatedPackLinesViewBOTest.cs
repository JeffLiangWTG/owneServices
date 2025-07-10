using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(UnAllocatedPackLinesView))]
	sealed class UnAllocatedPackLinesViewBOTest : BusinessObjectCollectionViewTestCase<UnAllocatedPackLinesView>
	{
		#region Implementation

		protected override UnAllocatedPackLinesView GetCollectionToTest()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			PackLineNonDependentCollection packLines = new PackLineNonDependentCollection(Factory);
			return new UnAllocatedPackLinesViewForTest(packLines);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			PackLine outerPackLine = shipment.OuterPackLines.AddNew();
			return outerPackLine;
		}

		#endregion
	}
}
