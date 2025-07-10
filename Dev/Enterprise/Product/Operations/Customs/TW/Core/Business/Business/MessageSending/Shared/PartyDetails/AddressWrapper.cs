using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class AddressWrapper : IAddress
	{
		public AddressWrapper(string line = null, string chineseLine = null, string countryCode = null, string countrySubDivisionID = null, string countrySubDivisionName = null)
		{
			Line = line;
			ChineseLine = chineseLine;
			CountryCode = countryCode;
			CountrySubDivisionID = countrySubDivisionID;
			CountrySubDivisionName = countrySubDivisionName;
		}

		public ZString Line { get; }

		public ZString ChineseLine { get; }

		public ZString CountryCode { get; }

		public ZString CountrySubDivisionID { get; }

		public ZString CountrySubDivisionName { get; }
	}
}
