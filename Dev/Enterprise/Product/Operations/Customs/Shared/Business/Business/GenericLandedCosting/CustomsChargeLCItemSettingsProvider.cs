using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CustomsChargeLCItemSettingsProvider : ICustomsChargeLCItemSettingsProvider
	{
		public IEnumerable<ICustomsChargeLCItemSetting> GetCustomsChargeLCItemSettings(string countryCode, bool? isIntegrated, ZGuid companyPK)
		{
			IEnumerable<ICustomsChargeLCItemSetting> result = null;

			if (isIntegrated.HasValue && isIntegrated.Value)
			{
				result = IntegratedCountryLandedCostingHelper.GetCustomsChargeLCItemSettings();
			}
			else if (GenericLandedCostingConfigProvider.Instance.SupportedCountries.Contains(countryCode))
			{
				result = GenericLandedCostingConfigProvider.Instance.GetGenericLandedCostingConfig(countryCode)?.LandedLineCostItemSettings;
			}
			else if (IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, countryCode) && !IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(countryCode))
			{
				result = IntegratedCountryLandedCostingHelper.GetCustomsChargeLCItemSettings();
			}

			#region To be replaced by GenericLandedCostingConfig.xml

			if (result == null)
			{
				result = new[]
				{
					new LandedLineCostItemSetting() { CostType = "TDT", Description = (NoResString)"Total Duty", NumberOfDecimals = 2, IsDuty = true, DocumentCustomLabelCode = "DutyAmount" }, // Will be replaced with GenericLandedCostingConfig
					new LandedLineCostItemSetting() { CostType = "EXC", Description = (NoResString)"Excise", NumberOfDecimals = 2, DocumentCustomLabelCode = "ExciseTax" }, // Will be replaced with GenericLandedCostingConfig
					new LandedLineCostItemSetting() { CostType = "ENT", Description = (NoResString)"Entry Fees", NumberOfDecimals = 2, DocumentCustomLabelCode = "EntryFee" }, // Will be replaced with GenericLandedCostingConfig
					new LandedLineCostItemSetting() { CostType = "OTH", Description = (NoResString)"OTH Duty",   NumberOfDecimals = 2, DocumentCustomLabelCode = "FlatOrOtherDutyAmount" }, // Will be replaced with GenericLandedCostingConfig
					new LandedLineCostItemSetting() { CostType = "ST1", Description = (NoResString)"Special Tax 1", NumberOfDecimals = 2, DocumentCustomLabelCode = "SpecialTax1" }, // Will be replaced with GenericLandedCostingConfig
					new LandedLineCostItemSetting() { CostType = "ST2", Description = (NoResString)"Special Tax 2", NumberOfDecimals = 2, DocumentCustomLabelCode = "SpecialTax2" }, // Will be replaced with GenericLandedCostingConfig
					new LandedLineCostItemSetting() { CostType = "ST3", Description = (NoResString)"Special Tax 3", NumberOfDecimals = 2, DocumentCustomLabelCode = "SpecialTax3" } // Will be replaced with GenericLandedCostingConfig
				};
			}

			#endregion

			return result;
		}
	}
}
