using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryLineFee : TypeSafeCusEntryLineFee, Integration.Customs.US.ICusEntryLineFee, IFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsLineFee
		{
			get { return CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory).ContainsCode(CF_ChargeType) && !CusFeeCodeConstants.IsExciseTax(CF_ChargeType); }
		}

		protected override bool ShouldResetDataOnMergingCore
		{
			get
			{
				var declaration = EntryLine?.Header?.Declaration;
				return declaration == null || !declaration.US_NoDutyCalc;
			}
		}

		#region IFee Members

		ZString IFee.Code
		{
			get { return CF_ChargeType; }
			set { CF_ChargeType = value; }
		}

		ZDecimal IFee.Amount
		{
			get { return CF_ChargeAmount; }
			set { CF_ChargeAmount = value; }
		}

		ZString IFee.SelectedRateType
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		ZBool IFee.IsOverridden => false;

		#endregion
	}
}
