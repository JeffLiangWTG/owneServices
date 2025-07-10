using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class PGAContactDetailsWithID : IPGAContactDetailsWithID
	{
		public PGAContactDetailsWithID(IPGAContactDetails contact, ZString iDType, ZString iDNumber)
		{
			this.contact = contact;
			this.idtype = iDType;
			this.idnumber = iDNumber;
		}

		readonly IPGAContactDetails contact;
		readonly ZString idtype;
		readonly ZString idnumber;

		#region IPGAContactDetails

		ZString IPGAContactDetailsWithID.IDType
		{
			get { return idtype; }
		}

		ZString IPGAContactDetailsWithID.IDNumber
		{
			get { return idnumber; }
		}

		ZString IPGAContactDetails.Name
		{
			get { return contact.Name; }
		}

		ZString IPGAContactDetails.PhoneNumber
		{
			get { return contact.PhoneNumber; }
		}

		ZString IPGAContactDetails.EmailAddress
		{
			get { return contact.EmailAddress; }
		}

		ZString IPGAContactDetails.Fax
		{
			get { return contact.Fax; }
		}

		IAddressDetails IPGAContactDetails.CompanyAddress
		{
			get { return contact.CompanyAddress; }
		}

		#endregion
	}
}
