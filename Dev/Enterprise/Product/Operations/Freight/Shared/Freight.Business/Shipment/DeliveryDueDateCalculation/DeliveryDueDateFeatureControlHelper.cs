using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Business
{
	public class DeliveryDueDateFeatureControlHelper : IDeliveryDueDateFeatureControlHelper
	{
		public DeliveryDueDateFeatureControlHelper()
		{
		}

		public bool Enabled => ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.ForwardingDeliveryDueDateFeature) != null;
	}
}
