using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class AWBAddressRecordForExport
	{
		public AWBAddressRecordForExport(ZString accountCode, ZString companyName, ZString addrLine1, ZString addrLine2, ZString countryCode, ZString city, ZString state, ZString postCode, ZString contactType, ZString contactDetail)
		{
			AccountCode = accountCode;
			CompanyName = companyName;
			AddrLine1 = addrLine1;
			AddrLine2 = addrLine2;
			CountryCode = countryCode;
			City = city;
			State = state;
			PostCode = postCode;
			ContactType = contactType;
			ContactDetail = contactDetail;
		}

		public readonly ZString AccountCode;
		public readonly ZString CompanyName;
		public readonly ZString AddrLine1;
		public readonly ZString AddrLine2;
		public readonly ZString CountryCode;
		public readonly ZString City;
		public readonly ZString State;
		public readonly ZString PostCode;
		public readonly ZString ContactType;
		public readonly ZString ContactDetail;
	}
}
