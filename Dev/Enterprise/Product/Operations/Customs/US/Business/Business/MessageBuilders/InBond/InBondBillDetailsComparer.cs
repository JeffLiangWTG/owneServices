using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	public class InBondBillDetailsComparer : IComparer<IInBondBillDetails>
	{
		public int Compare(IInBondBillDetails x, IInBondBillDetails y)
		{
			return x.MasterBillNumber.CompareTo(y.MasterBillNumber);
		}
	}
}
