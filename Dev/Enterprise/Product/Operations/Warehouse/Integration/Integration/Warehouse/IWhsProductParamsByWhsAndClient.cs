
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsProductParamsByWhsAndClient
	{
		object this[string propertyName] { get; set; }

		ZGuid W3_WW { get; set; }

		ZGuid W3_OH { get; set; }

		ZGuid W3_OP { get; set; }

		ZGuid W3_WA_DynamicPickFaceArea { get; set; }

		ZShort W3_MaximumShelfLife { get; set; }

		ZGuid W3_WPG_PutawayGroup { get; set; }

		ZInt W3_ExpiryNotificationPeriod { get; set; }
	}
}
