using System.Linq;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentHeaderValidation : AutoCarrierShipmentHeaderValidation
	{
		public CarrierShipmentHeaderValidation(AutoCarrierShipmentHeader parent) : base(parent)
		{
		}

		protected override void CheckCSH_ScreeningStatus()
		{
			base.CheckCSH_ScreeningStatus();
			var validStates = new ScreeningStatusesList().GetAllCodes().ToList();
			if (!validStates.Contains(Parent.CSH_ScreeningStatus))
			{
				Parent.CSH_ScreeningStatusInfo.AddError(Res.GetString("F3376161-77EA-4AF0-945A-E2A452F1B764", "Enter a valid Screening Status."));
			}
		}
	}
}
