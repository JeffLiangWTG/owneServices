using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TaxFrameworkAccTaxOverrideGroupValidation : AccTaxOverrideGroupValidation
	{
		public TaxFrameworkAccTaxOverrideGroupValidation(AutoAccTaxOverrideGroup parent) : base(parent)
		{
		}

		protected new AccTaxOverrideGroup Parent
		{
			get { return base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCommentTypeChargeCodesShouldNotBePresent();
			ValidateRecoveryChargeCodeNotAllowedForARLedgerTypeConfiguration();
			ValidateTaxOverrideGroupShouldHaveTaxConfiguration();
			ValidateDuplicateTaxConfigurations();
			ValidateTaxConfigurationsShouldHaveSameLedger();
		}

		public void ValidateCommentTypeChargeCodesShouldNotBePresent()
		{
			var errorMessage = Res.GetString("74B8F4D2-717B-4898-A5B9-EADE3F9A7372", "Comment type Charge Codes are not permitted on Tax Configuration Override Groups. Please detach any Comment Charge Codes from this Override Group.");

			foreach (var chargeCode in Parent.ChargeCodesLinkedToTaxFrameworkConfiguration)
			{
				chargeCode.RemoveRowError(errorMessage);
				if (chargeCode.AC_ChargeType == Core.Constants.ChargeType.Comment)
				{
					chargeCode.AddRowError(errorMessage);
				}
			}
		}

		public void ValidateRecoveryChargeCodeNotAllowedForARLedgerTypeConfiguration()
		{
			var errorMessage = Res.GetString("4D7DA216-7AE5-4A96-9A23-E06880353F1C", @"You cannot have Revenue Tax Expense Recovery Charge Code attached to a Tax Override Group with AR (Receivables Ledger) Tax Framework Configurations.
Please detach the Revenue Tax Expense Recovery Charge Code before saving.");
			foreach (var chargeCode in Parent.ChargeCodesLinkedToTaxFrameworkConfiguration)
			{
				chargeCode.RemoveRowError(errorMessage);
			}

			var pivots = Parent.TaxOverrideGroupTaxConfigurationPivots;
			if (pivots.Count > 0 && pivots.All(x => x.TaxConfiguration != null))
			{
				var registryChargeCode = AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.GetFallBackValueAtAllLevels(
					pivots.First().TaxConfiguration.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

				if (pivots.First().TaxConfiguration.ETC_Ledger == LedgerTypes.AccountsReceivable)
				{
					foreach (var chargeCode in Parent.ChargeCodesLinkedToTaxFrameworkConfiguration)
					{
						if (chargeCode.PK == registryChargeCode)
						{
							chargeCode.AddRowError(errorMessage);
						}
					}
				}
			}
		}

		public void ValidateTaxOverrideGroupShouldHaveTaxConfiguration()
		{
			var errorMessage = Res.GetString("d5cacb9d-2062-432f-98ba-4c3553e88032", "Please add at least one row in the Tax Configuration grid.");
			Parent.RemoveRowError(errorMessage);

			if (Parent.TaxOverrideGroupTaxConfigurationPivots.Count == 0)
			{
				Parent.AddRowError(errorMessage);
			}
		}

		public void ValidateDuplicateTaxConfigurations()
		{
			var errorMessage = Res.GetString("f24b7e46-daf4-4391-a10e-6675f06b5d6f", "You cannot have identical Tax Configurations.");
			foreach (var pivot in Parent.TaxOverrideGroupTaxConfigurationPivots)
			{
				pivot.RemoveRowError(errorMessage);
			}

			var collection = Parent.TaxOverrideGroupTaxConfigurationPivots;

			for (int i = 0; i < collection.Count; i++)
			{
				var pivot1 = collection[i];
				for (int y = i + 1; y < collection.Count; y++)
				{
					var pivot2 = collection[y];
					if (pivot1.PK != pivot2.PK && pivot2.AXP_ETC_TaxConfiguration == pivot1.AXP_ETC_TaxConfiguration)
					{
						pivot1.AddRowError(errorMessage);
						pivot2.AddRowError(errorMessage);
					}
				}
			}
		}

		public void ValidateTaxConfigurationsShouldHaveSameLedger()
		{
			var errorMessage = Res.GetString("74fd9c80-ef6a-4769-9fc6-0f2b77f94423", "A mix of Tax Configurations with different Ledgers is not permitted.");
			var pivots = Parent.TaxOverrideGroupTaxConfigurationPivots;

			foreach (var pivot in pivots)
			{
				pivot.RemoveRowError(errorMessage);
			}

			if (pivots.Count > 1 && pivots.All(x => x.TaxConfiguration != null))
			{
				var ledger = pivots.First().TaxConfiguration.ETC_Ledger;
				foreach (var pivot in pivots)
				{
					if (pivot.TaxConfiguration.ETC_Ledger != ledger)
					{
						pivot.AddRowError(errorMessage);
					}
				}
			}
		}
	}
}
