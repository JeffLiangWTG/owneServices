using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public class SanitizedACEOceanManifestJobDocAddressWrapper : IDocAddress
	{
		public SanitizedACEOceanManifestJobDocAddressWrapper(IDocAddress docAddress)
		{
			if (docAddress == null)
			{
				throw new ArgumentNullException(nameof(docAddress));
			}
			this.docAddress = docAddress;
		}
		readonly IDocAddress docAddress;

		protected IDocAddress DocAddress
		{
			get { return docAddress; }
		}

		public ZString AddressCaption
		{
			get { return GetSanitizedString(DocAddress.AddressCaption); }
		}

		public ZString CountryCode
		{
			get { return GetSanitizedString(DocAddress.CountryCode); }
		}

		ZString IDocAddress.E2_RN_NKCountryCode
		{
			get { return ZString.Empty; }
		}

		public ZString E2_AdditionalAddressInformation
		{
			get { return GetSanitizedString(DocAddress.E2_AdditionalAddressInformation); }
		}

		public ZString E2_Address1
		{
			get { return GetSanitizedString(DocAddress.E2_Address1); }
		}

		public ZString E2_Address2
		{
			get { return GetSanitizedString(DocAddress.E2_Address2); }
		}

		public ZBool E2_AddressOverride
		{
			get { return DocAddress.E2_AddressOverride; }
		}

		public ZString E2_AddressType
		{
			get { return GetSanitizedString(DocAddress.E2_AddressType); }
		}

		public ZString E2_City
		{
			get { return GetSanitizedString(DocAddress.E2_City); }
		}

		public ZString E2_CompanyName
		{
			get { return GetSanitizedString(E2_CompanyNameTruncated); }
		}

		public ZString E2_CompanyNameTruncated
		{
			get { return GetSanitizedString(DocAddress.E2_CompanyName.Substring(0, JobDocAddress.Schema.E2_CompanyNameTruncatedLength)); }
		}

		public ZString E2_Fax
		{
			get { return GetSanitizedString(DocAddress.E2_Fax); }
		}

		public ZString E2_GovRegNum
		{
			get { return DocAddress.E2_GovRegNum; }
		}

		public ZString E2_GovRegNumType
		{
			get { return GetSanitizedString(DocAddress.E2_GovRegNumType); }
		}

		public ZGuid E2_OA_Address
		{
			get { return DocAddress.E2_OA_Address; }
		}

		public ZString E2_PassportCountryOfIssue
		{
			get { return GetSanitizedString(DocAddress.E2_PassportCountryOfIssue); }
		}

		public ZDateTime E2_PassportDateOfBirth
		{
			get { return DocAddress.E2_PassportDateOfBirth; }
		}

		public ZString E2_PassportID
		{
			get { return DocAddress.E2_PassportID; }
		}

		public ZString E2_Phone
		{
			get { return GetSanitizedString(DocAddress.E2_Phone); }
		}

		public ZString E2_PortCode
		{
			get { return GetSanitizedString(DocAddress.E2_PortCode); }
		}

		public ZString E2_Postcode
		{
			get { return GetSanitizedString(DocAddress.E2_Postcode); }
		}

		public ZString E2_State
		{
			get { return GetSanitizedString(DocAddress.E2_State); }
		}

		public IOrgHeader Organisation
		{
			get { return DocAddress.Organisation; }
		}

		public ZString ParentDescription
		{
			get { return GetSanitizedString(DocAddress.ParentDescription); }
		}

		public ZString AddressFull => GetSanitizedString(DocAddress.AddressFull);

		public ZString CountryDescription => GetSanitizedString(DocAddress.CountryDescription);

		protected ZString GetSanitizedString(ZString value)
		{
			return ACEOceanManifestIllegalCharacters.ReplaceIllegalCharacters(value);
		}
	}
}
