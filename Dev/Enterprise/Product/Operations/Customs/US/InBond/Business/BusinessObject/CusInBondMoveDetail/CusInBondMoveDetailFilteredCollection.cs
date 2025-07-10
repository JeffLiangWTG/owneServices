using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveDetailFilteredCollection : BusinessObjectCollectionView<CusInBondMoveDetail>
	{
		public CusInBondMoveDetailFilteredCollection(CusInBondHeader header, bool isForMessage = false)
			: base(header.MovementDetails)
		{
			this.header = header;
			this.isForMessage = isForMessage;
			Rebuild();
		}
		readonly CusInBondHeader header;
		readonly bool isForMessage;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var newElement = (CusInBondMoveDetail)child;
			newElement.InBondHeaderPK = header.PK;
			if (!isForMessage)
			{
				newElement.B9_BM = !header.SelectedMovementHeader.IsEmpty ? header.SelectedMovementHeader : (header.MovementHeaders.Count == 1 ? header.MovementHeaders[0].PK : ZGuid.Empty);
				if (newElement.IsDetailedInBond && !header.IsAir)
				{
					newElement.Containers.AddNew();
				}
			}
		}

		protected override void RebuildOnConstruction()
		{
			//header is null at this point
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var moveDetail = (CusInBondMoveDetail)element;
			var result = false;
			if (!isForMessage)
			{
				result = header.SelectedMovementHeader.IsEmpty || moveDetail.B9_BM == header.SelectedMovementHeader;
			}
			else
			{
				result = moveDetail.PK == header.SelectedMovementDetail;
			}
			return result;
		}

		protected override bool AllowNewCore
		{
			get { return !header.ShouldSynchronise; }
		}
	}
}
