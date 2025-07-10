using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxConfiguration : AutoAccOrgTaxConfiguration
	{
		public AccOrgTaxConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateLastValidOTC_ETC();
		}

		#region OTC_ETC

		public override ZGuid OTC_ETC
		{
			get => base.OTC_ETC;
			set
			{
				base.OTC_ETC = value;

				if (TaxConfiguration != null)
				{
					if (!TaxConfiguration.IsInDatabase)
					{
						throw new InvalidOperationException("TaxConfiguration must be saved.");
					}

					if (TaxConfiguration.ETC_Ledger != Ledger)
					{
						throw new InvalidOperationException("TaxConfiguration Ledger is not correct.");
					}
				}

				if (!base.OTC_ETCInfo.HasErrors())
				{
					UpdateLastValidOTC_ETC();
				}

				SetTaxRateReadOnly();
				Validation.ValidateOTC_RecoverTax();
				Validation.ValidateOTC_IsThresholdUsed();
			}
		}

		void UpdateLastValidOTC_ETC()
		{
			lastValidOTC_ETC = OTC_ETC;
		}
		internal ZGuid lastValidOTC_ETC { get; private set; }

		#endregion

		public ZString OTC_ETC_Description => TaxConfiguration != null ? TaxConfiguration.ETC_Description : ZString.Empty;

		public override ZBool OTC_IsActive
		{
			get => base.OTC_IsActive;
			set
			{
				base.OTC_IsActive = value;

				Validation.ValidateOTC_RecoverTax();
				Validation.ValidateOTC_IsThresholdUsed();
			}
		}

		public void CopyEditableColumns(AccOrgTaxConfiguration sourceOrgTaxConfig)
		{
			OTC_IsActive = sourceOrgTaxConfig.OTC_IsActive;
			OTC_IsThresholdUsed = sourceOrgTaxConfig.OTC_IsThresholdUsed;
			OTC_RecoverTax = sourceOrgTaxConfig.OTC_RecoverTax;
		}

		#region Tax Rates

		[ChildEditable(true)]
		public AccOrgTaxRateCollection TaxRates
		{
			get
			{
				if (taxRates == null)
				{
					taxRates = new AccOrgTaxRateCollection(this);
					RegisterEditableChildObject(taxRates);
					SetTaxRateReadOnly();
				}
				return taxRates;
			}
		}
		AccOrgTaxRateCollection taxRates;

		void SetTaxRateReadOnly()
		{
			var readOnly = true;

			if (TaxConfiguration != null && !OTC_ETCInfo.HasErrors())
			{
				var allowedTaxRateSourceCodes = new HashSet<ZString> { TaxRateSources.OrganisationOnly.Code, TaxRateSources.OrganisationFallbackToTaxGroup.Code, TaxRateSources.OrganisationFallbackToTaxID.Code };
				var taxSystemsConfigurations = TaxConfiguration.TaxSystem;
				if (taxSystemsConfigurations != null)
				{
					readOnly = !allowedTaxRateSourceCodes.Contains(taxSystemsConfigurations.TaxRateSource);
				}
			}

			if (!readOnly && CompanyData?.Header?.SecurityProvider != null)
			{
				if (Ledger == TaxConfigurationLedgers.AccountsReceivable.Code)
				{
					readOnly = !CompanyData.Header.SecurityProvider.HasModifyReceivablesTaxConfigurationRatesSecurity;
				}
				else if (Ledger == TaxConfigurationLedgers.AccountsPayable.Code)
				{
					readOnly = !CompanyData.Header.SecurityProvider.HasModifyPayablesTaxConfigurationRatesSecurity;
				}
			}

			TaxRates.SetReadOnlyIncludingChildren(readOnly);
		}

		#endregion

		#region Ledger

		public virtual ZString Ledger
		{
			get
			{
				InitializeLedgerAfterLoad();

				return ledger;
			}
			set
			{
				if (!IsInDatabase && ledger.IsEmpty)
				{
					ledger = value;
				}
				else
				{
					throw new InvalidOperationException("Ledger must not be changed.");
				}
			}
		}
		ZString ledger;

		void InitializeLedgerAfterLoad()
		{
			if (IsInDatabase && !isLedgerInitialized)
			{
				ledger = TaxConfiguration.ETC_Ledger;
				isLedgerInitialized = true;
			}
		}

		bool isLedgerInitialized;

		#endregion

		public virtual GlbCompany GetParentCompany() => CompanyData?.Company;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (OTC_ETC.IsEmpty)
			{
				var config = new BusinessObjectFactory().New<AccTaxConfiguration>();
				if (!Ledger.IsEmpty)
				{
					config.ETC_Ledger = Ledger;
				}
				config.FillWithValidTestData();
				config.Factory.Save();

				if (Ledger.IsEmpty)
				{
					Ledger = config.ETC_Ledger;
				}
				OTC_ETC = config.PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);

			if (OTC_OB.IsEmpty)
			{
				OTC_OB = Factory.NewWithValidTestData<OrgCompanyData>().PK;
			}
		}

		public ZGuid LastValidOTC_ETC_ForTestOnly() => lastValidOTC_ETC;
#endif
	}
}
