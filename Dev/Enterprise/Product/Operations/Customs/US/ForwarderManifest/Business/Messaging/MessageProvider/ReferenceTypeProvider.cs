using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class ReferenceTypeProvider : IReferenceType
	{
		public ReferenceTypeProvider(string type, string info)
		{
			ReferenceTypeCode = new ManifestStringTypeProvider(type);
			ReferenceData = new ManifestStringTypeProvider(info);
		}

		public IManifestStringType ReferenceTypeCode { get; }

		public IManifestStringType ReferenceData { get; }

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());
	}
}
