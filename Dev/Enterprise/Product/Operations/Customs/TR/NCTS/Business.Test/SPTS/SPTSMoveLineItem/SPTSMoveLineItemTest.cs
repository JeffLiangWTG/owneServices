using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(SPTSMoveLineItem))]
	public class SPTSMoveLineItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var sptsHeader = Factory.New<SPTSHeader>();
			var departureMovement = sptsHeader.MovementHeader;
			var moveDetail = departureMovement.MovementDetails.AddNew();

			lineItem = Factory.New<SPTSMoveLineItem>();
			lineItem.BI_B9 = moveDetail.PK;
		}
		SPTSMoveLineItem lineItem;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => lineItem;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => lineItem;
	}
}
