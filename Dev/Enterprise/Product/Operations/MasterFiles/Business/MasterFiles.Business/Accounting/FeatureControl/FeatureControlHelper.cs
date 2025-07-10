using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.MasterFiles.Business
{
	public static class FeatureControlHelper
	{
		public static bool IsKoreaSouthComplianceSubTypeFeatureEnabled
		{
			get
			{
				var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingEInvoicingKoreaSouthComplianceSubTypeFeature);

				if (featureData != null && featureData.TryDeserializeParameterAsJson<AccountingEInvoicingKoreaSubTypesFeatureControlData>(out var subTypesFeatureControlData))
				{
					return subTypesFeatureControlData?.IsComplianceSubTypeFeatureEnabled ?? false;
				}

				return false;
			}
		}

		public static bool IsAdvancePaymentFeatureEnabled
		{
			get
			{
				var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingAdvancePaymentFeature);

				if (featureData != null && featureData.TryDeserializeParameterAsJson<AccAdvancePaymentFeatureControlData>(out var featureControlData))
				{
					return featureControlData?.IsAdvancePaymentFeatureEnabled ?? false;
				}

				return false;
			}
		}

		public static bool IsCWD365IntegrationFeatureEnabled => ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CWD365IntegrationFeature) != null;
	}
}
