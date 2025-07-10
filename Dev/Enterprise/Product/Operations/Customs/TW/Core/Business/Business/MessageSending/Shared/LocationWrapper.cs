using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LocationWrapper : ILocation
	{
		public LocationWrapper(ZString id = new ZString(), ZString name = new ZString(), ZDate loadingDateTime = new ZDate(), ZString estimatedLoadingCode = new ZString())
		{
			this.id = id;
			this.name = name;
			this.loadingDateTime = loadingDateTime;
			this.estimatedLoadingCode = estimatedLoadingCode;
		}

		readonly ZString id;
		readonly ZString name;
		readonly ZDate loadingDateTime;
		readonly ZString estimatedLoadingCode;

		ZString ILocation.ID => id;

		ZString ILocation.Name => name;

		ZDate ILocation.LoadingDateTime => loadingDateTime;

		ZString ILocation.EstimatedLoadingCode => estimatedLoadingCode;
	}
}
