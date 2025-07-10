using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AirlineStatusNotification : IAIMAirlineStatusNotification
	{
		public AirlineStatusNotification(ZString statusCode)
		{
			this.statusCode = statusCode;
		}

		readonly ZString statusCode;
		public ZString StatusCode => statusCode;
		public ZString ActionExplanation => ZString.Empty;
	}
}
