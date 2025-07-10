using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Business
{
	public class ClientRateProcessTask : RatingHeaderProcessTask<ClientRate>, IClientRateProcessTask
	{
		public ClientRateProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.ClientRates; }
		}
	}
}

