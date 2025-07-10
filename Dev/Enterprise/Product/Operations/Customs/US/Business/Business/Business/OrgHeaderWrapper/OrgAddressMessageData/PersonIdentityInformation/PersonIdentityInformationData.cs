using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class PersonIdentityInformationData : AutoPersonIdentityInformationData, IHugeSequenceNumberLine
	{
		public PersonIdentityInformationData(BusinessObjectFactory factory, OrgAddressMessageData messageData)
			: base(factory)
		{
			this.messageData = messageData;
		}
		internal readonly OrgAddressMessageData messageData;

		[MaxLength(2)]
		[ReadOnly(true)]
		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set { base.US_LineNo = value; }
		}

		[List(nameof(Lookups) + "." + nameof(PersonIdentityInformationDataLookups.Contacts))]
		[RelatedBusinessObject("Contact")]
		public override ZGuid US_OC_Contact
		{
			get { return base.US_OC_Contact; }
			set
			{
				var oldValue = US_OC_Contact;
				base.US_OC_Contact = value;
				if (oldValue != value && !IsCopying)
				{
					var contact = Contact;
					if (contact != null)
					{
						US_Name = ContactNameHelper.GetFormattedName(contact.OC_ContactName);
						US_Title = contact.OC_Title.Left(Schema.US_TitleMaxLength);
						US_PhoneNumber = contact.OC_Phone.KeepNumericCharacters().Left(Schema.US_PhoneNumberMaxLength);
						US_Email = contact.OC_Email.Left(Schema.US_EmailMaxLength);
						US_Extension = contact.OC_PhoneExtension.Left(Schema.US_ExtensionMaxLength);

						var passport = contact.Certificates.GetFirstCertificate(CertificateTypePairList.Codes.PA1);
						US_PassportNo = passport?.XZ_RefNumber.Left(Schema.US_PassportNoMaxLength) ?? ZString.Empty;
						US_ExpirationDate = passport?.XZ_ExpiryOrDueDate ?? ZDateTime.Empty;
						US_CountryOfIssuance = passport?.XZ_RN_NKCountryOfIssuance ?? ZString.Empty;
					}
					else
					{
						US_Name = ZString.Empty;
						US_Title = ZString.Empty;
						US_PassportNo = ZString.Empty;
						US_ExpirationDate = ZDateTime.Empty;
						US_CountryOfIssuance = ZString.Empty;
						US_PhoneNumber = ZString.Empty;
						US_Extension = ZString.Empty;
						US_Email = ZString.Empty;
					}

					US_SSN = ZString.Empty;
					US_PassportType = ZString.Empty;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(PersonIdentityInformationDataLookups.CountryList))]
		public override ZString US_CountryOfIssuance
		{
			get { return base.US_CountryOfIssuance; }
			set { base.US_CountryOfIssuance = value; }
		}

		[List(nameof(Lookups) + "." + nameof(PersonIdentityInformationDataLookups.PassportTypeList))]
		public override ZString US_PassportType
		{
			get { return base.US_PassportType; }
			set { base.US_PassportType = value; }
		}

		#region Related Objects

		public OrgContact Contact
		{
			get { return Factory.Load<OrgContact>(US_OC_Contact); }
		}

		#endregion

		public PersonIdentityInformationDataLookups Lookups
		{
			get { return new PersonIdentityInformationDataLookups(this); }
		}

		public override void Delete()
		{
			base.Delete();
			messageData.PIISequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		#region IHugeSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => messageData.wrapper.CountryData.PK;

		ZInt ISequenceNumberLine<ZInt>.SequenceNumber
		{
			get => US_LineNo;
			set => US_LineNo = value;
		}

		#endregion
	}
}
