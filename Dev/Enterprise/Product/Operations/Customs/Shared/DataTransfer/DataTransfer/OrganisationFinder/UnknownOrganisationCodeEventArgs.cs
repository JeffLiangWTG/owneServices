using CargoWise.Types;

namespace Enterprise.Customs.DataTransfer
{
	public class UnknownOrganisationCodeEventArgs
	{
		public UnknownOrganisationCodeEventArgs()
		{
		}

		public UnknownOrganisationCodeEventArgs(ZString supplierCode, ZString name, ZString registrationNo, ZString phone,
			ZString street, ZString street2, ZString city, ZString postCode, ZString country)
		{
			Code = supplierCode;
			Name = name;
			RegistrationNo = registrationNo;
			Phone = phone;
			Street = street;
			Street2 = street2;
			City = city;
			PostCode = postCode;
			Country = country;
		}

		public ZString Code;
		public ZString Name;
		public ZString RegistrationNo;
		public ZString Phone;
		public ZString Street;
		public ZString Street2;
		public ZString City;
		public ZString PostCode;
		public ZString Country;
	}

	public delegate void UnknownOrganisationCodeEventHandler(object sender, UnknownOrganisationCodeEventArgs e);
}
