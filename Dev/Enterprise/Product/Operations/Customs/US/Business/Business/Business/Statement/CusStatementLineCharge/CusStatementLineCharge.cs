using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	[DependentBusinessObject(typeof(CusStatementLine), "Charges")]
	public class CusStatementLineCharge : Customs.Business.BaseCusStatementLineCharge, Integration.Customs.US.ICusStatementLineCharge
	{
		public CusStatementLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[DecimalPlaces(2)]
		public override ZDecimal B4_ChargeAmount
		{
			get { return base.B4_ChargeAmount; }
			set { base.B4_ChargeAmount = value; }
		}

		public ZString ChargeTypeDescription
		{
			get { return Lookups.EntryChargeTypeList.GetDescriptionFromCode(B4_ChargeType); }
		}

		public ZPropertyInfo ChargeTypeDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				return GetZPropertyInfo(nameof(ChargeTypeDescription));
			}
		}

		#region type safe

		public new CusStatementLineChargeValidation Validation
		{
			get { return (CusStatementLineChargeValidation)base.Validation; }
		}

		public new CusStatementLineChargeLookups Lookups
		{
			get { return (CusStatementLineChargeLookups)base.Lookups; }
		}

		protected override Customs.Business.CusStatementLineChargeLookups GetNewLookups()
		{
			return new CusStatementLineChargeLookups(this);
		}

		protected override Customs.Business.CusStatementLineChargeValidation GetNewValidation()
		{
			return new CusStatementLineChargeValidation(this);
		}

		#endregion

	}
}
