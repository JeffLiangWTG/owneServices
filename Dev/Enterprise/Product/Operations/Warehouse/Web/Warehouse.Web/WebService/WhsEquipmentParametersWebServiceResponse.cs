using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsEquipmentParametersWebServiceResponse : WebServiceResponse
	{
		#region Properties

		public WhsEquipmentParameters EquipmentParameters
		{
			get => equipmentParameters ?? new WhsEquipmentParameters();
			set => equipmentParameters = value;
		}

		#endregion

		#region Implementation

		WhsEquipmentParameters equipmentParameters;

		#endregion
	}
}


