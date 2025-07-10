using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterData.Business
{
	partial class DuplicationModelDetail
	{
		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));
		public ZString Name { get; private set; }

		public ZPropertyInfo BrandInfo => GetZPropertyInfo(nameof(Brand));
		public ZString Brand { get; private set; }

		public ZPropertyInfo ContactPhoneInfo => GetZPropertyInfo(nameof(ContactPhone));
		public ZString ContactPhone { get; private set; }

		public ZPropertyInfo AddressPhoneInfo => GetZPropertyInfo(nameof(AddressPhone));
		public ZString AddressPhone { get; private set; }

		public ZPropertyInfo ContactMobileInfo => GetZPropertyInfo(nameof(ContactMobile));
		public ZString ContactMobile { get; private set; }

		public ZPropertyInfo AddressMobileInfo => GetZPropertyInfo(nameof(AddressMobile));
		public ZString AddressMobile { get; private set; }

		public ZPropertyInfo EmailInfo => GetZPropertyInfo(nameof(Email));
		public ZString Email { get; private set; }

		public ZPropertyInfo BirthdayInfo => GetZPropertyInfo(nameof(Birthday));
		public ZString Birthday { get; private set; }

		public ZPropertyInfo OtherPhoneInfo => GetZPropertyInfo(nameof(OtherPhone));
		public ZString OtherPhone { get; private set; }

		public ZPropertyInfo HomePhoneInfo => GetZPropertyInfo(nameof(HomePhone));
		public ZString HomePhone { get; private set; }

		public ZPropertyInfo WebsiteInfo => GetZPropertyInfo(nameof(Website));
		public ZString Website { get; private set; }

		public ZPropertyInfo ContactFaxInfo => GetZPropertyInfo(nameof(ContactFax));
		public ZString ContactFax { get; private set; }

		public ZPropertyInfo AddressFaxInfo => GetZPropertyInfo(nameof(AddressFax));
		public ZString AddressFax { get; private set; }

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));
		public ZString Type { get; private set; }

		public ZPropertyInfo CountryInfo => GetZPropertyInfo(nameof(Country));
		public ZString Country { get; private set; }

		public ZPropertyInfo CusCodeCustomsRegNoInfo => GetZPropertyInfo(nameof(CusCodeCustomsRegNo));
		public ZString CusCodeCustomsRegNo { get; private set; }

		public ZPropertyInfo CoordinatesInfo => GetZPropertyInfo(nameof(Coordinates));
		public ZString Coordinates { get; private set; }

		public ZPropertyInfo AddressInfo => GetZPropertyInfo(nameof(Address));
		public ZString Address { get; private set; }

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));
		public ZString Code { get; private set; }

		public ZPropertyInfo DomainInfo => GetZPropertyInfo(nameof(Domain));
		public ZString Domain { get; private set; }

		public ZPropertyInfo PersonPhoneInfo => GetZPropertyInfo(nameof(PersonPhone));
		public ZString PersonPhone { get; private set; }

		public ZPropertyInfo PersonWorkPhoneInfo => GetZPropertyInfo(nameof(PersonWorkPhone));
		public ZString PersonWorkPhone { get; private set; }

		public ZPropertyInfo PersonMobileInfo => GetZPropertyInfo(nameof(PersonMobile));
		public ZString PersonMobile { get; private set; }

		public ZPropertyInfo PersonFaxInfo => GetZPropertyInfo(nameof(PersonFax));
		public ZString PersonFax { get; private set; }

		public ZPropertyInfo SimilarityInfo => GetZPropertyInfo(nameof(Similarity));
		public ZString Similarity { get; private set; }

		public ZPropertyInfo ConfidenceScoreInfo => GetZPropertyInfo(nameof(ConfidenceScore));
		public ZString ConfidenceScore { get; private set; }

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));
		public ZString Number { get; private set; }

		public ZPropertyInfo SourceInfo => GetZPropertyInfo(nameof(Source));
		public ZString Source { get; private set; }

		public ZPropertyInfo PrimaryWorkplaceInfo => GetZPropertyInfo(nameof(PrimaryWorkplace));
		public ZBool PrimaryWorkplace { get; private set; }

		public ZPropertyInfo UNLOCOInfo => GetZPropertyInfo(nameof(UNLOCO));
		public ZString UNLOCO { get; private set; }

		public ZPropertyInfo CityInfo => GetZPropertyInfo(nameof(City));
		public ZString City { get; private set; }

		public ZPropertyInfo StateInfo => GetZPropertyInfo(nameof(State));
		public ZString State { get; private set; }

		public ZPropertyInfo ActiveInfo => GetZPropertyInfo(nameof(Active));
		public ZBool Active { get; private set; }
	}
}
