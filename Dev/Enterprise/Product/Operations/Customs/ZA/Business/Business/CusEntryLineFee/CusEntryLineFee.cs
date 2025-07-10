using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryLineFee : Customs.Business.CusEntryLineFee, Integration.Customs.ZA.ICusEntryLineFee, IDutyFeeInformation
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsRebate => Factory.GetRateCodesWithRateType(Universal.Constants.RateTypes.Rebate).Any(x => x.ZY1_RateCode == CF_ChargeType);

		public bool IsPayableDuty => CusTariff?.IsPayableDuty() ?? false;

		public ZString RateTypeCode => CusTariff?.Rates.FirstOrDefault()?.ZZ2_ZZR_RateTypeCode ?? ZString.Empty;

		public RefCusTariffType CusTariffType
		{
			get
			{
				RefCusTariffType result = null;
				var chargeType = CF_ChargeType;
				if (!chargeType.IsEmpty)
				{
					var list = RefCusTariffTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica);
					if (list.ContainsCode(chargeType))
					{
						var query = new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, chargeType);
						query.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
						result = Factory.LoadTop1<RefCusTariffType>(query);
					}
				}
				return result;
			}
		}

		public TariffView CusTariff
		{
			get
			{
				TariffView result = null;
				var tariffType = CusTariffType;
				if (tariffType != null)
				{
					var query = new ZQuery(TariffViewSchema.ZZ1_ZZI_TariffType, tariffType.PK);
					query.AddToFilter(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica);
					result = Factory.LoadTop1<TariffView>(query);
				}
				return result;
			}
		}

		public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;

		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation()
		{
			return new CusEntryLineFeeValidation(this);
		}

		public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;

		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups()
		{
			return new CusEntryLineFeeLookups(this);
		}

		protected override bool ShouldResetDataOnMergingCore => true;

		protected override bool ShouldDeleteIfChargeAmountIsZero => shouldDeleteIfChargeAmountIsZero;

		internal void MarkDoNotDeleteIfChargeAmountIsZero()
		{
			shouldDeleteIfChargeAmountIsZero = false;
		}

		internal bool shouldDeleteIfChargeAmountIsZero
		{
			get;
			private set;
		}

		protected override void ResetDataCore()
		{
			shouldDeleteIfChargeAmountIsZero = true;
		}

		[ReadOnlyMember(nameof(HasAtLeastOneRejection))]
		public override ZDecimal CF_ChargeAmount
		{
			get => base.CF_ChargeAmount;
			set
			{
				base.CF_ChargeAmount = value;
			}
		}

		[ReadOnlyMember(nameof(HasAtLeastOneRejection))]
		public override ZBool CF_IsLandedCostOnly
		{
			get => base.CF_IsLandedCostOnly;
			set
			{
				base.CF_IsLandedCostOnly = value;
			}
		}

		[ReadOnlyMember(nameof(HasAtLeastOneRejection))]
		public override ZString CF_ChargeType
		{
			get => base.CF_ChargeType;
			set
			{
				base.CF_ChargeType = value;
			}
		}

		[ReadOnlyMember(nameof(HasAtLeastOneRejection))]
		public override ZString CF_RateOverrideReasonCode
		{
			get => base.CF_RateOverrideReasonCode;
			set
			{
				base.CF_RateOverrideReasonCode = value;
			}
		}

		public virtual bool HasAtLeastOneRejection => !(EntryLine?.Header?.Messages.OfType<ZAMessage>().Count(msg => msg.EntryStatus.Equals("6")) >= 1);

		#region IDutyFeeInformation

		ZString IDutyFeeInformation.Code => CF_ChargeType;

		ZDecimal IDutyFeeInformation.Value => CF_ChargeAmount;

		#endregion
	}
}
