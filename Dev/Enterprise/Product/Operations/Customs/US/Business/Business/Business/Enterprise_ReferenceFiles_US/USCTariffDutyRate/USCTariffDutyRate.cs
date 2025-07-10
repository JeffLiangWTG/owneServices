using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[DependentBusinessObject(typeof(USCTariff), "DutyRates")]
	public sealed class USCTariffDutyRate : AutoUSCTariffDutyRate
	{
		public USCTariffDutyRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString GetRequiredFeeCode()
		{
			return IsFeeRequired && UD_TaxFeeClassCode != Core.Constants.USCustoms.FeeCodes.Coffee ? UD_TaxFeeClassCode : ZString.Empty;//Tariff alone cannot tell if Coffee fee is mandatory
		}

		public bool IsFeeRequired
		{
			get { return UD_TaxFeeFlag == "1"; }
		}

		public bool IsFeeConditional
		{
			get { return UD_TaxFeeFlag == "2"; }
		}

		public bool IsFeeApplicable(params string[] feeCodes)
		{
			return CheckFeeApplicability(() => IsFeeRequired || IsFeeConditional, feeCodes);
		}

		public bool CheckFeeApplicability(Func<bool> apply, params string[] feeCodes)
		{
			if (apply())
			{
				foreach (string feeCode in feeCodes)
				{
					if (UD_TaxFeeClassCode == feeCode)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool RequiresFirstQuantity
		{
			get { return ComputationCodeList.IsFirstQuantityRequired(UD_TaxFeeComputationCode); }
		}

		public bool RequiresSecondQuantity
		{
			get { return ComputationCodeList.IsSecondQuantityRequired(UD_TaxFeeComputationCode); }
		}

		public bool RequiresThirdQuantity
		{
			get { return ComputationCodeList.IsThirdQuantityRequired(UD_TaxFeeComputationCode); }
		}

		public bool IsSpecificSpecificTaxFee
		{
			get { return UD_TaxFeeComputationCode == ComputationCodeList.Codes.SpecificSpecific; }
		}

		public override void OnSaving()
		{
			SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			base.OnSaving();
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			}
			base.Delete();
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		internal bool RemoveOnFactorySaving;

		protected override void OnFactorySaving()
		{
			if (RemoveOnFactorySaving && !IsDeleted)
			{
				Delete();
			}
			base.OnFactorySaving();
		}

		public USCTariff ImportTariff
		{
			get { return Factory.Load<USCTariff>(UD_UE); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(USCTariffDutyRate tariffDutyRate)
				: base(tariffDutyRate)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				USCTariffDutyRate tariffDutyRate = (USCTariffDutyRate)BusinessObject;
				Factory.AddFetchHint(typeof(USCTariff), USCTariffSchema.PK, tariffDutyRate.UD_UE);
			}
		}
	}
}
