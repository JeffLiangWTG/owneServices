using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class VINInfoTypeProvider : IVINInfoType
	{
		public VINInfoTypeProvider(string vin)
		{
			this.VehicleIdentificationNumber = new ManifestStringTypeProvider(vin);
		}

		public IManifestStringType VehicleIdentificationNumber { get; }

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());
	}
}
