using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class AddressInformationForTest : IAddressInformation
	{
		public ZString OrganizationCode { get; set; }

		public ZString OrganizationCodeQualifier { get; set; }

		public ZString Address { get; set; }

		public ZString City { get; set; }

		public ZString Name { get; set; }

		public ZString PostCode { get; set; }

		public ZString VATRegistrationNo { get; set; }
	}
}
