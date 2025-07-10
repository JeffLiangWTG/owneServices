using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingName : SterlingRecord
	{
		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "N01";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(NameType);
			AddField(OrganisationEdiCode);
			AddField(AddressSequenceRef);
			AddField(Name);
			AddField(AddressLine1);
			AddField(AddressLine2);
			AddField(City);
			AddField(State);
			AddField(PostCode);
			AddField(Country);
			AddField(ContactType);
			AddField(Contact);
			AddField(Phone);
			AddField(Email);
			AddField(SCAC);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Set Name

		internal void SetName(string nameType, Xsd.Organisation org, Xsd.OrgAddress orgAddress, OrgContact orgContact)
		{
			NameType = nameType;
			OrganisationEdiCode = org.EDICode.ToString();
			AddressSequenceRef = orgAddress.Sequence.ToString();
			Name = !orgAddress.CompanyName.IsEmpty ? orgAddress.CompanyName : org.OrganisationDetails.Name;
			AddressLine1 = orgAddress.AddressLine1;
			AddressLine2 = orgAddress.AddressLine2;
			City = orgAddress.CityOrSuburb;
			State = orgAddress.StateOrProvince;
			PostCode = orgAddress.PostCode;
			Country = orgAddress.Location.IsSpecified ? orgAddress.Location.Value : orgAddress.Location.Country;
			Country = Country.Substring(0, 2);

			ContactType = "MAIN";
			if (orgContact != null)
			{
				Contact = orgContact.OC_ContactName;
			}
			Phone = orgAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business);
			Email = orgAddress.Email;

			var regNum = org.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.CCC, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (regNum != null)
			{
				SCAC = regNum.Number;
			}
		}

		#endregion

		#region Properties

		ZString NameType { get; set; }
		ZString OrganisationEdiCode { get; set; }
		ZString AddressSequenceRef { get; set; }

		ZString Name { get; set; }
		ZString AddressLine1 { get; set; }
		ZString AddressLine2 { get; set; }
		ZString City { get; set; }
		ZString State { get; set; }
		ZString PostCode { get; set; }
		ZString Country { get; set; }

		ZString Contact { get; set; }
		ZString ContactType { get; set; }
		ZString Phone { get; set; }
		ZString Email { get; set; }

		ZString SCAC { get; set; }

		#endregion

	}
}
