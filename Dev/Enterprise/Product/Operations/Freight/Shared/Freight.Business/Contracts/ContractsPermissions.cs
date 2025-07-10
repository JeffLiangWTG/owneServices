using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public class ContractsPermissions : IContractPermissions
	{
		public static bool IsCarrierAndClientContractModulesEnabled()
		{
			return IsCCAEnabledInFeatureControlModule()
				|| FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.Value;
		}

		public static bool IsAllocationsVisible()
		{
			return IsCarrierAndClientContractModulesEnabled()
				&& (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.Value || IsCCAAllocationsEnabledInFeatureControlModule());
		}

		public static bool IsTariffAndRatesVisible()
		{
			return IsCarrierAndClientContractModulesEnabled()
				&& (FreightConfigurationRegistry.Instance.EnableCarrierContractTariffsAndRates.Value || IsCCAEnabledInFeatureControlModule());
		}

		static bool IsCCAEnabledInFeatureControlModule()
		{
			return ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.CCAModules) != null;
		}

		static bool IsCCAAllocationsEnabledInFeatureControlModule()
		{
			return ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.CCAAllocationsFeature) != null;
		}

		#region IContractPermissions

		bool IContractPermissions.IsCarrierAndClientContractModulesEnabled() => IsCarrierAndClientContractModulesEnabled();

		bool IContractPermissions.IsAllocationsVisible() => IsAllocationsVisible();

		bool IContractPermissions.IsTariffAndRatesVisible() => IsTariffAndRatesVisible();

		#endregion
	}
}
