using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public class PortCallManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PortCallManager(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PortCallRequest Request => request ?? (request = new PortCallRequest(Factory));
		PortCallRequest request;

		public void SetupRequest(ZString port, ZDateTime estimatedTime, JobVoyage voyage, PortCallRequestType requestType)
		{
			Request.Port = port;
			Request.EstimatedTime = estimatedTime;
			Request.RequestType = requestType;

			if (voyage != null)
			{
				Request.CarrierPK = voyage.JV_OH_Line;
				Request.Voyage = voyage.JV_VoyageFlight;

				if (voyage.Vessel != null)
				{
					Request.CallSign = voyage.Vessel.RV_RadioCallSign;
					Request.IMO = voyage.Vessel.RV_LloydsNumber;
					Request.VesselPK = voyage.Vessel.PK;
				}
			}
		}

		public void LoadResponses(INotifications notifications)
		{
			Responses.Load(Request, notifications);
		}

		public PortCallResponseCollection Responses => responses ?? (responses = new PortCallResponseCollection(Factory));
		PortCallResponseCollection responses;
	}
}
