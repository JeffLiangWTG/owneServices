using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class LocationTypeProvider : ILocationType
	{
		public LocationTypeProvider(string location, string type)
		{
			LocationTypeCode = new ManifestStringTypeProvider(type);
			Location = new ManifestStringTypeProvider(location);
		}
		public IManifestStringType LocationTypeCode { get; }

		public IManifestStringType Location { get; }

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());
	}
}
