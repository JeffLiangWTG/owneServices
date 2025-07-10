using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class CommonNonApportionedCharge : BaseJobComInvHeaderCharge, ICurrencyProvider
	{
		protected CommonNonApportionedCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString J7_ChargeType
		{
			get { return base.J7_ChargeType; }
			set
			{
				var hasChanges = J7_ChargeType != value;
				base.J7_ChargeType = value;
				if (!IsCopying && hasChanges)
				{
					DefaultOverseasInsurancePrecentageIfApplicable();
				}
			}
		}

		void DefaultOverseasInsurancePrecentageIfApplicable()
		{
			if (J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasInsurance
				&& !J7_ParentTableCode.Equals(JobComInvoiceLineSchema.Constants.Prefix)
				&& J7_Percentage.IsEmpty
				&& (IncoTermAndChargeFactory?.GetCharge(J7_ChargeType)?.IsPercentageApplicable ?? false))
			{
				J7_Percentage = DefaultPercentageForOverseasInsurance();
			}
		}

		protected virtual ZDecimal DefaultPercentageForOverseasInsurance()
		{
			var declaration = Parent?.JobDeclaration;
			var companyPK = declaration?.RegistryCompanyPK ?? GlbCompany.CurrentCompany.PK.ToGuid();
			var branchPK = declaration?.RegistryBranchPK ?? GlbBranch.CurrentBranch.PK.ToGuid();
			return CustomsDataRegistry.Instance.DefaultInsuranceRate.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		protected override bool IsValidToImportForLC
		{
			get
			{
				bool result = base.IsValidToImportForLC;

				if (result)
				{
					if (J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight &&
						Parent != null &&
						Parent.JobDeclaration != null &&
						Parent.JobDeclaration.DoesJobInvoicingHaveFreightAmount)
					{
						result = false;
					}
				}

				return result;
			}
		}

		ZString ICurrencyProvider.CurrencyCode => J7_RX_NKCurrency;

		void ICurrencyProvider.SetExchangeRateIfNotUserOverridden()
		{
			SetExchangeRateIfNotUserOverridden();
		}

		void ICurrencyProvider.ValidateCurrencyCode()
		{
			Validation.ValidateJ7_RX_NKCurrency();
		}
	}
}
