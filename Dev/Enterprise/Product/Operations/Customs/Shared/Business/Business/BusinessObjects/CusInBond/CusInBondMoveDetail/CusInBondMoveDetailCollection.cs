using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusInBondMoveDetailCollection : IActiveBusinessObjectCollection
	{
		CusInBondMoveDetail AddNew(ZGuid billPK);

		new CusInBondMoveDetail this[int index] { get; }
	}

	public abstract class CusInBondMoveDetailCollection<T> : ActiveBusinessObjectCollection<T>, ICusInBondMoveDetailCollection
		where T : CusInBondMoveDetail
	{
		protected CusInBondMoveDetailCollection(CusInBondMoveHeader master)
			: base(master)
		{
		}

		protected CusInBondMoveDetailCollection(CusInBondBill master)
			: base(master)
		{
		}

		public T AddNew(ZGuid billPK)
		{
			var result = AddNew();
			result.B9_B0 = billPK;
			return result;
		}

		CusInBondMoveDetail ICusInBondMoveDetailCollection.AddNew(ZGuid billPK)
		{
			return this.AddNew(billPK);
		}

		CusInBondMoveDetail ICusInBondMoveDetailCollection.this[int index] => this[index];

		public T FindBillOfLading(ZString issuerCode, ZString billOfLading)
		{
			T result = null;

			if (Relationship.Master is CusInBondMoveHeader moveHeader)
			{
				var query = new ZDBOnlyQuery(typeof(T));
				query.AddToFilter(CusInBondMoveDetailSchema.B9_BM, moveHeader.PK);

				var billSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondMoveDetailSchema.B9_B0);
				billSubQuery.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, billOfLading);

				if (!issuerCode.IsEmpty)
				{
					billSubQuery.AddToFilter(CusInBondBillSchema.B0_IssuerCode, issuerCode);
				}

				query.AddSubQuery(billSubQuery, JoinCondition.And);
				query.OrderBy = CusInBondMoveDetailSchema.Constants.B9_SystemCreateTimeUtc + OrderByClause.Descending;
				result = Factory.LoadTop1<T>(query);
			}
			return result;
		}
	}
}
