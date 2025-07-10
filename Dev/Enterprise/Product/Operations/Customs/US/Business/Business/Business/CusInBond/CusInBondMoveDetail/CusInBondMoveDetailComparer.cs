using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	public class CusInBondMoveDetailComparer : Comparer<CusInBondMoveDetail>
	{
		public override int Compare(CusInBondMoveDetail x, CusInBondMoveDetail y)
		{
			int result = x.B9_SeqNo.CompareTo(y.B9_SeqNo);
			if (result == 0)
			{
				result = x.MasterBillNumber.CompareTo(y.MasterBillNumber);
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == GetType();
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}
	}
}
