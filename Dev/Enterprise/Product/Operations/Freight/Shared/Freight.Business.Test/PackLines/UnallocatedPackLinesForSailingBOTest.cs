using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(UnAllocatedPackLinesForSailing))]
	sealed class UnallocatedPackLinesForSailingBOTest : BusinessObjectCollectionViewTestCase<UnAllocatedPackLinesForSailing>
	{
		#region Implementation

		protected override UnAllocatedPackLinesForSailing GetCollectionToTest()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			Factory.Save();

			return voyage.Sailings[0].UnAllocatedPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CommonShipment>().OuterPackLines.AddNew();
		}

		#endregion
	}
}
