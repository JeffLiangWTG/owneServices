using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.MasterFiles.Business;

public static class CustomsFeatureControlHelper
{
	public static bool IsVietnamManifestFeatureEnabled
	{
		get
		{
			var featureData = ObjectFactory.Get<IFeatureControlManager>()
				.GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.VietnamForwarderManifest);

			if (featureData != null &&
				featureData.TryDeserializeParameterAsJson<VietnamManifestFeatureControlData>(
					out var vnManifestFeatureControlData))
			{
				return vnManifestFeatureControlData.IsVietnamManifestFeatureEnabled;
			}

			return false;
		}
	}
}
