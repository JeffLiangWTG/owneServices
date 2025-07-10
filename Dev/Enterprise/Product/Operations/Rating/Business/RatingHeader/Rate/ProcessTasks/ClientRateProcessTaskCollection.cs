using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class ClientRateProcessTaskCollection : RatingHeaderProcessTaskCollection<ClientRate, ClientRateProcessTask>
	{
		public ClientRateProcessTaskCollection(ClientRate clientRate)
			: base(clientRate)
		{
		}

		protected override ProcessTaskCollection GetNewCollectionCore(ClientRate parent)
		{
			return new ClientRateProcessTaskCollection(parent);
		}
	}
}


