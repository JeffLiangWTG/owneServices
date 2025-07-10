using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.TR.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region TypeSafe

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups()
		{
			return new CusEntryLineFeeLookups(this);
		}

		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation()
		{
			return new CusEntryLineFeeValidation(this);
		}

		public new CusEntryLineFeeValidation Validation
		{
			get { return (CusEntryLineFeeValidation)GetNewValidation(); }
		}

		#endregion

		protected override EU.Business.Declaration.ChargeAmountRefresher GetNewChargeAmountRefresher() => new ChargeAmountRefresher(this);

		[DecimalPlaces(2)]
		public override ZDecimal CF_ChargeAmount { get => base.CF_ChargeAmount; set => base.CF_ChargeAmount = value; }

		[DecimalPlaces(2)]
		public override ZDecimal CF_BaseValue => base.CF_BaseValue;

		[DecimalPlaces(2)]
		public override ZDecimal CF_Rate => base.CF_Rate;

		[ResourceStringData("FAE37570-6C46-4658-A389-223D0EF38571", Caption = "Duty Type")]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.NationalFeeTypeCodeList))]
		public override ZString NationalFeeTypeCode
		{
			get => base.NationalFeeTypeCode;
			set
			{
				var oldValue = NationalFeeTypeCode;
				base.NationalFeeTypeCode = value;
				if (oldValue != NationalFeeTypeCode && !IsCopying)
				{
					CF_ChargeType = NationalFeeTypeCode == DeclarationHelper.NationalVatType.Code ? FeeTypeList.Codes.B00 : NationalFeeTypeCode;
				}
			}
		}

		public override ZString CF_ChargeType
		{
			get => base.CF_ChargeType;
			set
			{
				var oldValue = CF_ChargeType;
				base.CF_ChargeType = value;
				if (oldValue != CF_ChargeType && !IsCopying)
				{
					EntryLine?.Fees.MarkAsNeedingValidation();
				}
			}
		}
	}
}
