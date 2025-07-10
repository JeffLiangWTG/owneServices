using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class OriginWrapper : IOrigin
	{
		public OriginWrapper(string countryCode = null, IAdditionalDocument additionalDocument = null)
		{
			CountryCode = countryCode;
			AdditionalDocument = additionalDocument;
		}

		public ZString CountryCode { get; }

		public IAdditionalDocument AdditionalDocument { get; }
	}
}
