using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveHeaderFilteredActiveCollection : ActiveBusinessObjectCollection<CusInBondMoveHeader>
	{
		public CusInBondMoveHeaderFilteredActiveCollection(CusInBondHeader header)
			: base(header.Factory, header, new ZQuery(), CusInBondMoveHeaderSchema.BM_BH)
		{
			this.header = header;
		}

		public readonly CusInBondHeader header;

		#region Implementation

		protected override bool MatchesFilterCore(CusInBondMoveHeader element, bool fetchOnlyFromLocalCache)
		{
			bool result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);

			if (result)
			{
				result = header.SelectedMovementHeader.IsEmpty ||
					element.PK == header.SelectedMovementHeader;
			}
			return result;
		}

		#endregion

	}
}
