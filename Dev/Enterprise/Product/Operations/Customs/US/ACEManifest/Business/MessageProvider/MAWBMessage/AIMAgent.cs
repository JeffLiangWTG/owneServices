using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMAgent : IAIMAgent
	{
		public AIMAgent(AdditionalMessageInformation additionalMessageInformation)
		{
			this.additionalMessageInformation = additionalMessageInformation;
		}

		readonly AdditionalMessageInformation additionalMessageInformation;

		#region IAIMAgent Implementation

		public ZString AirAMSParticipantCode => additionalMessageInformation.AM_Agent;

		#endregion
	}
}
