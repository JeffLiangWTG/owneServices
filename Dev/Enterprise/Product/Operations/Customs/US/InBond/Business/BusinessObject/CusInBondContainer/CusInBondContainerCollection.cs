using CargoWise.Types;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondContainerCollection : Customs.Business.CusInBondContainerCollection<CusInBondContainer>
	{
		public CusInBondContainerCollection(CusInBondMoveDetail master)
			: base(master)
		{
		}

		public ZInt TotalPieceCount
		{
			get
			{
				var result = ZInt.Zero;

				foreach (CusInBondContainer container in this)
				{
					result += container.BC_PieceCount == 0 ? container.Commodities.TotalPieceCount : container.BC_PieceCount;
				}
				return result;
			}
		}

		public void ResetPieceCount()
		{
			foreach (CusInBondContainer container in this)
			{
				container.BC_PieceCount = ZInt.Zero;
			}
		}

		protected override bool AllowNew
		{
			get
			{
				var master = Relationship.Master as CusInBondMoveDetail;
				return master != null && !master.IsDeleted && !master.ShouldSynchronise;
			}
		}

		protected override void EndNew(int index)
		{
			CusInBondContainer nonCommittedContainer = null;
			if (index != -1)
			{
				CusInBondContainer container = this[index];
				if (IsNonCommittedElement(container))
				{
					nonCommittedContainer = container;
				}
			}
			base.EndNew(index);
			if (nonCommittedContainer != null)
			{
				nonCommittedContainer.Commodities.RefreshBinding();
			}
		}
	}
}
