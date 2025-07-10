using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CountrySpecificDataObjectReaderHelperProvider : ICountrySpecificDataObjectReaderHelperProvider
	{
		public IUniversalDataObjectReaderHelper GetUniversalDataObjectReaderHelper(IUniversalObjectFactory factory, string countryCode)
		{
			var universalFactory = factory as UniversalObjectFactory;
			var objectProvider = universalFactory?.BOFactory.GetUniversalCustomsDataObjectProvider(countryCode);
			UniversalDataObjectReaderHelper result = null;
			if (objectProvider != null)
			{
				result = objectProvider.GetNewUniversalDataObjectReaderHelper(universalFactory, countryCode);
			}

			return result ?? (countryCode.IsNullOrEmpty()
				? null
				: new UniversalDataObjectReaderHelper((UniversalObjectFactory)factory, countryCode, countryCode));
		}
	}
}
