using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class AgencyBookingFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				JobShipmentSchema.JS_RL_NKOrigin.Name,
				JobShipmentSchema.JS_RL_NKDestination.Name,
				AgencyShipment.Schema.JS_NKDischargePort,
				AgencyShipment.Schema.JS_NKLoadPort,
				JobShipmentSchema.JS_PackingMode.Name,
				AgencyShipment.Schema.ConsignorPK,
				AgencyShipment.Schema.ConsigneePK,
				AgencyShipment.Schema.ShipmentJobHeaderPK
			};
		}
	}
}
