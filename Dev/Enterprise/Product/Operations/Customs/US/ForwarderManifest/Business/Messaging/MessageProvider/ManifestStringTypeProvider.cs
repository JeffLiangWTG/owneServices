using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class ManifestStringTypeProvider : IManifestStringType
	{
		public ManifestStringTypeProvider(string value)
		{
			Value = value;
		}

		public string Value { get; }

		public Collection<IErrorType> ErrorList => new Collection<IErrorType>();
	}
}
