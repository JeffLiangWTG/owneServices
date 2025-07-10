using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class LastModificationWriter : LastHoldWriter
	{
		public LastModificationWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType) : base(manager, recipientRoleType)
		{
		}

		protected override Shipment GetUniversalShipment(IWarehouseIntegrationSupporter declaration, RecipientRoleType recipientRoleType)
		{
			return declaration.GetLastModificationUniversalShipment(recipientRoleType);
		}
	}
}
