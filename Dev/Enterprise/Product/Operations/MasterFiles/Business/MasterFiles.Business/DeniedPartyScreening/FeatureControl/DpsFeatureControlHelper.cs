using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class DpsFeatureControlHelper
	{
		public (bool isAddressOnlyScreeningIncluded, bool isAllAddressesIncluded, string matchingLevel) GetFeatureControlAddressOnlyFlags
		{
			get
			{
				var isAddressOnlyScreeningIncluded = false;
				var isAllAddressesIncluded = false;
				var matchingLevel = OrganisationsDataRegistry.Instance.AddressMatchingLevel.Value.MatchingLevel;
				var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.DpsAddressMatchingLevelFeature);

				if (featureData != null)
				{
					isAddressOnlyScreeningIncluded = true;
				}
				if (featureData != null && featureData.TryDeserializeParameterAsJson<DpsAddressMatchingProfilesFeatureControlData>(out var featureControlDataModel))
				{
					isAllAddressesIncluded = featureControlDataModel.IsAllAddressesIncluded;
				}

				return (isAddressOnlyScreeningIncluded, isAllAddressesIncluded, matchingLevel);
			}
		}
	}
}
