using System.Linq;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	static class UniversalShipmentExtension
	{
		public static bool ContainsServiceCode(this UniversalShipment dataObject, ServiceCodeType serviceCode)
		{
			return dataObject?.DataContext?.RecipientRoleCollection?.Any(c => c.ServiceCode == serviceCode) ?? false;
		}

		public static bool ContainsRecipientRole(this UniversalShipment dataObject, RecipientRoleType recipientRoleType)
		{
			return dataObject?.DataContext?.RecipientRoleCollection?.Any(c => c.Code == recipientRoleType) ?? false;
		}

		public static bool ContainsRecipientRoleAndServiceCode(this UniversalShipment dataObject, RecipientRoleType recipientRoleType, ServiceCodeType serviceCode)
		{
			return dataObject?.DataContext?.RecipientRoleCollection?.Any(c => c.Code == recipientRoleType && c.ServiceCode == serviceCode) ?? false;
		}

		public static bool ContainsDataTargetType(this UniversalShipment dataObject, DataContextType type)
		{
			return dataObject?.DataContext?.DataTargetCollection?.Any(c => c.Type.HasValue && c.Type.Value == type.ToString()) ?? false;
		}
	}
}
