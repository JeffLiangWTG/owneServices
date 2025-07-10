using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class BaseCusInBondMoveDetail : AutoCusInBondMoveDetail
	{
		protected BaseCusInBondMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new CusInBondMoveDetailTypeDecider();

		public BaseCusInBondMoveHeader MoveHeader => (moveHeader ?? (moveHeader = new CachedRelatedBusinessObject<BaseCusInBondMoveHeader>((ZPropertyInfoGuid)B9_BMInfo, GetMoveHeader))).Value;
		CachedRelatedBusinessObject<BaseCusInBondMoveHeader> moveHeader;

		protected virtual BaseCusInBondMoveHeader GetMoveHeader()
		{
			return Factory.Load<BaseCusInBondMoveHeader>(B9_BM);
		}
	}
}
