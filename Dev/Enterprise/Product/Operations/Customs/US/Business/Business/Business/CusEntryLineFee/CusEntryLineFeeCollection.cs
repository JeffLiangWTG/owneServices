using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryLineFeeCollection : Customs.Business.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>, IFees
	{
		public CusEntryLineFeeCollection(CusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		public ZDecimal GetExciseTax()
		{
			ZDecimal result = 0m;

			foreach (CusEntryLineFee fee in this)
			{
				if (CusFeeCodeConstants.IsExciseTax(fee.CF_ChargeType))
				{
					result += fee.CF_ChargeAmount;
				}
			}

			return result;
		}

		public ZDecimal TotalAmount
		{
			get { return this.Sum(x => ((CusEntryLineFee)x).CF_ChargeAmount); }
		}

		#region IFees Members

		IFee IFees.GetFeeFor(ZString code)
		{
			return GetElementWithThisCode(code);
		}

		IFee IFees.AddNew()
		{
			return AddNew();
		}

		#endregion
	}
}
