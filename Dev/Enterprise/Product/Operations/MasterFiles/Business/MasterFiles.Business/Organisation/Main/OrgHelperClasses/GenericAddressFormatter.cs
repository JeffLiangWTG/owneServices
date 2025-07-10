using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.MasterFiles;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GenericAddressFormatter : IGenericAddressFormatter
	{
		public string PostalAddress(BusinessObjectFactory factory, string name, string additionalAddressInformation, string address1, string address2, string city, string state, string postCode, IMultilingualString countryName, bool includeCountryEvenIfSame, string countryCode)
		{
			var formatter = new AddressFormatter(factory, name, additionalAddressInformation, address1, address2, city, state, postCode, (NoResString)countryName, Core.SharedConstants.Languages.English, includeCountryEvenIfSame, countryCode);
			return formatter.PostalAddress();
		}
	}
}
