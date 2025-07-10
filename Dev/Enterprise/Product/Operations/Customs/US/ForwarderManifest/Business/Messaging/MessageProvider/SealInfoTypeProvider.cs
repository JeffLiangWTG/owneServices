using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class SealInfoTypeProvider : ISealInfoType
	{
		readonly string sealNumber;

		public SealInfoTypeProvider(string sealNumber)
		{
			this.sealNumber = sealNumber;
		}

		public IManifestStringType SealNumber => new ManifestStringTypeProvider(sealNumber);

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());

		public Collection<IActionType> Action => new Collection<IActionType>();
	}
}
