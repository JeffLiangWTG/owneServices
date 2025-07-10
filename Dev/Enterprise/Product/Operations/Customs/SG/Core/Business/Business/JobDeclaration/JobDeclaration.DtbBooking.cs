using Enterprise.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public partial class JobDeclaration
	{
		protected override string TransportBookingTransportModeCore
		{
			get
			{
				if (IsExport)
				{
					return IsOutwardTransportModeAir ? Constants.TransportModes.Air
						 : IsOutwardTransportModeSea ? Constants.TransportModes.Sea
						 : IsOutwardTransportModeRoad ? Constants.TransportModes.Road
						 : IsOutwardTransportModeRail ? Constants.TransportModes.Rail
						 : IsOutwardTransportModeMail ? Constants.TransportModes.Mail
						 : "";
				}
				else
				{
					return base.TransportBookingTransportModeCore;
				}
			}
		}
	}
}
