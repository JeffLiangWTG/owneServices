using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInbondBillAddRefCollection : ActiveBusinessObjectCollection<CusInbondBillAddRef>
	{
		public CusInbondBillAddRefCollection(CusInBondBill master)
			: base(master)
		{
		}

		public CusInbondBillAddRef AddNew(ZString qualifier, ZString referenceNum)
		{
			var result = AddNew();
			result.BR_Qualifier = qualifier;
			result.BR_ReferenceNum = referenceNum;
			return result;
		}

		public CusInbondBillAddRef this[ZString qualifier]
		{
			get
			{
				CusInbondBillAddRef result = null;
				foreach (var billRef in this)
				{
					if (!billRef.IsDeleted && billRef.BR_Qualifier == qualifier)
					{
						result = billRef;
						break;
					}
				}
				return result;
			}
		}
	}
}
