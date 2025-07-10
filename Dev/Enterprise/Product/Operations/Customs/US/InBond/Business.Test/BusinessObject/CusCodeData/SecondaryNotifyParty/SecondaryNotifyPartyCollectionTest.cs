using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(SecondaryNotifyPartyCollection))]
	sealed class SecondaryNotifyPartyCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<SecondaryNotifyParty>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<SecondaryNotifyParty> GetCusCodeDataCollection() => MoveDetail.SecondaryNotifyParties;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			SecondaryNotifyParty result = Factory.New<SecondaryNotifyParty>();
			result.CY_ParentID = MoveDetail.PK;
			result.CY_ParentTableCode = MoveDetail.TablePrefix;
			return result;
		}

		CusInBondMoveDetail moveDetail;
		CusInBondMoveDetail MoveDetail
		{
			get
			{
				if (moveDetail == null)
				{
					var header = Factory.New<CusInBondHeader>();
					var moveHeader = header.MovementHeaders.AddNew();
					moveDetail = moveHeader.MovementDetails.AddNew();
				}

				return moveDetail;
			}
		}
	}
}
