using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business
{
	public static class AccTaxConfigurationExtensionHelper
	{
		public static bool IsEnabledForTaxFrameworkConfiguration(this GlbCompany glbCompany, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			return (factory.Exists(typeof(AccTaxConfiguration), helper.GetCompanyTaxConfigurations(factory, glbCompany, new ZQuery()).CompleteFilter));
		}

		public static bool IsAPWithholdTaxEnabled(this GlbCompany company)
		{
			if (company != null)
			{
				var taxSystemCodes = AccountingMasterFilesRegistry.Instance.TaxSystems.Value
					.Cast<TaxSystemsConfiguration>()
					.Where(tsc => tsc.TaxSuperType == TaxSuperTypeList.StandardPaymentRetention.Code)
					.Select(tsc => tsc.Code)
					.ToList();

				if (taxSystemCodes.Any())
				{
					var query = new ZQuery(AccTaxConfigurationSchema.ETC_Ledger, LedgerTypes.AccountsPayable)
						.AddToFilter(AccTaxConfigurationSchema.ETC_IsActive, true)
						.AddToFilter(AccTaxConfigurationSchema.ETC_TaxSystemCode, taxSystemCodes);

					var factory = new ReadOnlyBusinessObjectFactory();
					return factory.Exists(typeof(AccTaxConfiguration), helper.GetCompanyTaxConfigurations(factory, company, query).CompleteFilter);
				}
			}

			return false;
		}

		public static bool IsSPR_AP_Configuration(this AccTaxConfiguration config)
		{
			Argument.NotNull(config, nameof(config));

			var taxSystem = config.TaxSystem;
			return taxSystem != null && taxSystem.TaxSuperType == TaxSuperTypeList.StandardPaymentRetention.Code && config.ETC_Ledger == TaxConfigurationLedgers.AccountsPayable.Code;
		}

		public static bool IsSPR_AR_Configuration(this AccTaxConfiguration config)
		{
			Argument.NotNull(config, nameof(config));

			var taxSystem = config.TaxSystem;
			return taxSystem != null && taxSystem.TaxSuperType == TaxSuperTypeList.StandardPaymentRetention.Code && config.ETC_Ledger == TaxConfigurationLedgers.AccountsReceivable.Code;
		}

		static ITaxFrameworkConfigurationHelper helper => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper();
	}
}
