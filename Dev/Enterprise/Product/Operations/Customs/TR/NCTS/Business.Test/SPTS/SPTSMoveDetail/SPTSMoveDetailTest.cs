using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSMoveDetail))]
	public class SPTSMoveDetailTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var sptsHeader = Factory.New<SPTSHeader>();
			var departureMovement = sptsHeader.MovementHeader;
			moveDetail = departureMovement.MovementDetails.AddNew();
		}
		BusinessObject moveDetail;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => moveDetail;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => moveDetail;
	}
}
