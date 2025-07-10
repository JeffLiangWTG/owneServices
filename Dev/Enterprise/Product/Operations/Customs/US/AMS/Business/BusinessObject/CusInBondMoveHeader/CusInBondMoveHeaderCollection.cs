using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondMoveHeaderCollection : Customs.Business.CusInBondMoveHeaderCollection
	{
		public CusInBondMoveHeaderCollection(CusInBondHeader master, Func<ZString> getSubApplicationCode, ZQuery additionalFilter)
			: base(master, GetSubApplicationCodeFilter(getSubApplicationCode(), additionalFilter))
		{
			this.getSubApplicationCode = getSubApplicationCode;
		}

		public CusInBondMoveHeaderCollection(CusInBondHeader master, ZString subApplicationCode)
			: this(master, () => subApplicationCode, null)
		{
		}

		public new CusInBondMoveHeader this[int index]
		{
			get { return (CusInBondMoveHeader)base[index]; }
		}

		public new CusInBondMoveHeader AddNew()
		{
			return (CusInBondMoveHeader)base.AddNew();
		}

		static ZQuery GetSubApplicationCodeFilter(ZString subApplicationCode, ZQuery additionalFilter)
		{
			var result = new ZQuery(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCode);
			if (additionalFilter != null)
			{
				result.AddToFilter(additionalFilter, JoinCondition.Or);
			}
			return result;
		}

		readonly Func<ZString> getSubApplicationCode;

		#region Implementation

		protected override void SetDefaultsForNewElementCore(Customs.Business.CusInBondMoveHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BM_SubApplicationCode = getSubApplicationCode();
		}

		protected override void EndNew(int index)
		{
			if (index != -1)
			{
				var moveHeader = this[index];
				if (IsNonCommittedElement(moveHeader) && moveHeader.BM_SubApplicationCode == SubApplicationCodeList.Codes.MasterInBond)
				{
					DefaultMoveDetailsForMasterInBond(moveHeader);
				}
			}

			base.EndNew(index);
		}

		void DefaultMoveDetailsForMasterInBond(CusInBondMoveHeader newElement)
		{
			var header = (CusInBondHeader)Relationship.Master;
			foreach (var bill in header.Bills)
			{
				if (bill.B0_MasterInBondIndicator && bill.MasterInBondMovement == null)
				{
					newElement.MovementDetails.AddNew(bill.PK);
				}
			}
		}

		#endregion
	}
}
