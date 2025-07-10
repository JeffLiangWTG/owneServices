using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondPersonTest : TestCaseWithFactory
	{
		public void TestSync()
		{
			person.CP_OC_Contact = contact.PK;
			Factory.Save();

			AssertReadOnly(person, true, false, true, true, true, true, true);
			AssertPersonSychedWithContact(person);
			AssertPersonSychedWithContact(new BusinessObjectFactory().Load<CusInBondPersonForTesting>(person.PK));
			AssertSynchedDataIsEmptyAtRowLevel(person);

			person.CP_OC_Contact = ZGuid.Empty;
			person.CP_GS_NKStaff = staff.GS_Code;
			Factory.Save();

			AssertReadOnly(person, false, true, true, true, true, true, true);
			AssertPersonSychedWithStaff(person);
			AssertPersonSychedWithStaff(new BusinessObjectFactory().Load<CusInBondPersonForTesting>(person.PK));
			AssertSynchedDataIsEmptyAtRowLevel(person);
		}

		public void TestNoSync()
		{
			person.CP_FullName = contact.OC_ContactName;
			person.CP_DateOfBirth = contact.OC_Birthday;
			person.CP_Gender = contact.OC_Gender;
			person.CP_RN_NKNationality = contact.OC_RN_NKNationality;
			AddCertificate(person.Certificates, "TST", "123456789");

			AssertReadOnly(person, false, false, false, false, false, false, false);
			AssertEquals("CP_FullName set", contact.OC_ContactName, person.CP_FullName);
			AssertEquals("CP_DateOfBirth set", contact.OC_Birthday, person.CP_DateOfBirth);
			AssertEquals("CP_Gender set", contact.OC_Gender, person.CP_Gender);
			AssertEquals("CP_RN_NKNationality set", contact.OC_RN_NKNationality, person.CP_RN_NKNationality);
			AssertEquals("One certificate added", 1, person.Certificates.Count);

			Factory.Save();
			var personInNewFactory = new BusinessObjectFactory().Load<CusInBondPersonForTesting>(person.PK);
			AssertReadOnly(personInNewFactory, false, false, false, false, false, false, false);
			AssertEquals("CP_FullName set", contact.OC_ContactName, personInNewFactory.CP_FullName);
			AssertEquals("CP_DateOfBirth set", contact.OC_Birthday, personInNewFactory.CP_DateOfBirth);
			AssertEquals("CP_Gender set", contact.OC_Gender, personInNewFactory.CP_Gender);
			AssertEquals("CP_RN_NKNationality set", contact.OC_RN_NKNationality, personInNewFactory.CP_RN_NKNationality);
			AssertEquals("One certificate added", 1, personInNewFactory.Certificates.Count);
		}

		public void TestPersistentDetailsNotClearedWhenParentSetAccidentallyButClearedOnSave()
		{
			person.CP_FullName = staff.GS_FullName;
			person.CP_DateOfBirth = staff.GS_Birthdate;
			person.CP_Gender = staff.GS_Gender;
			person.CP_RN_NKNationality = staff.GS_RN_NKNationalityCode;
			AddCertificate(person.Certificates, "TST", "123456789");
			Factory.Save();

			person.CP_OC_Contact = contact.PK;
			person.CP_OC_Contact = ZGuid.Empty;
			person.CP_GS_NKStaff = staff.GS_Code;
			person.CP_GS_NKStaff = ZString.Empty;

			AssertEquals("CP_FullName overridden", staff.GS_FullName, person.CP_FullName);
			AssertEquals("CP_DateOfBirth overridden", staff.GS_Birthdate, person.CP_DateOfBirth);
			AssertEquals("CP_Gender overridden", staff.GS_Gender, person.CP_Gender);
			AssertEquals("CP_RN_NKNationality overridden", staff.GS_RN_NKNationalityCode, person.CP_RN_NKNationality);
			AssertEquals("One cetificate added", 1, person.Certificates.Count);

			person.CP_GS_NKStaff = staff.GS_Code;
			Factory.Save();
			AssertSynchedDataIsEmptyAtRowLevel(person);
		}

		public void TestReadOnly()
		{
			AssertReadOnly(person, false, false, false, false, false, false, false);

			person.CP_GS_NKStaff = staff.GS_Code;
			AssertReadOnly(person, false, true, true, true, true, true, true);

			person.CP_GS_NKStaff = ZString.Empty;
			AssertReadOnly(person, false, false, false, false, false, false, false);

			person.CP_OC_Contact = contact.PK;
			AssertReadOnly(person, true, false, true, true, true, true, true);

			person.CP_OC_Contact = ZGuid.Empty;
			AssertReadOnly(person, false, false, false, false, false, false, false);

			person.CP_FullName = staff.GS_FullName;
			person.CP_DateOfBirth = staff.GS_Birthdate;
			person.CP_Gender = staff.GS_Gender;
			person.CP_RN_NKNationality = staff.GS_RN_NKNationalityCode;
			AddCertificate(person.Certificates, "TST", "123456789");
			AssertReadOnly(person, false, false, false, false, false, false, false);

			person.CP_OC_Contact = contact.PK;
			Factory.Save();
			var personInNewFactory = new BusinessObjectFactory().Load<CusInBondPersonForTesting>(person.PK);
			AssertReadOnly(personInNewFactory, true, false, true, true, true, true, true);

			person.CP_OC_Contact = ZGuid.Empty;
			person.CP_Gender = Constants.Genders.Man;
			Factory.Save();
			personInNewFactory = new BusinessObjectFactory().Load<CusInBondPersonForTesting>(person.PK);
			AssertReadOnly(personInNewFactory, false, false, false, false, false, false, false);
		}

		public void TestCloneCertificates()
		{
			person.CP_FullName = staff.GS_FullName;
			person.CP_DateOfBirth = staff.GS_Birthdate;
			person.CP_Gender = staff.GS_Gender;
			person.CP_RN_NKNationality = staff.GS_RN_NKNationalityCode;
			AddCertificate(person.Certificates, "TST", "123456789");

			var clonedPerson = (CusInBondPerson)person.Clone();

			AssertEquals("CP_FullName", staff.GS_FullName, clonedPerson.CP_FullName);
			AssertEquals("CP_DateOfBirth", staff.GS_Birthdate, clonedPerson.CP_DateOfBirth);
			AssertEquals("CP_Gender", staff.GS_Gender, clonedPerson.CP_Gender);
			AssertEquals("CP_RN_NKNationality", staff.GS_RN_NKNationalityCode, clonedPerson.CP_RN_NKNationality);

			AssertEquals("One cetificate", 1, clonedPerson.Certificates.Count);
			var clonedCertificate = clonedPerson.Certificates[0];

			AssertEquals("XZ_Type", "TST", clonedCertificate.XZ_Type);
			AssertEquals("XZ_RefNumber", "123456789", clonedCertificate.XZ_RefNumber);
		}

		public void TestLoadOrCreate()
		{
			var cusInBondHeader = Factory.New<CusInBondHeaderForTesting>();
			var cusInBondPerson = CusInBondPerson.LoadOrCreate<CusInBondPersonForTesting>(cusInBondHeader, "LOC");
			CombineAssertions(() =>
			{
				AssertNotNull("Has been created", cusInBondPerson);
				AssertEquals("HasChanges", false, cusInBondPerson.HasChanges);
				AssertEquals("CP_BH_Header", cusInBondHeader.PK, cusInBondPerson.CP_BH_Header);
				AssertEquals("CP_Type", "LOC", cusInBondPerson.CP_Type);

				var newCusInBondPerson = CusInBondPerson.LoadOrCreate<CusInBondPersonForTesting>(cusInBondHeader, "LOC");
				AssertSame("Has been loaded", cusInBondPerson, newCusInBondPerson);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Ilya Statkevych";
			contact.OC_Birthday = new ZDateTime(1988, 01, 27);
			contact.OC_Gender = Constants.Genders.Man;
			contact.OC_RN_NKNationality = Constants.CountryCodes.Ukraine;

			contactCert1 = AddCertificate(contact.Certificates, CertificateTypePairList.Codes.PA1, "XX12345", new ZDateTime(2015, 03, 12), Constants.CountryCodes.Ukraine);
			contactCert2 = AddCertificate(contact.Certificates, CertificateTypePairList.Codes.CA1, "ABBA123456", new ZDateTime(2013, 02, 07), Constants.CountryCodes.Ukraine);
			contactCert3 = AddCertificate(contact.Certificates, "XXX", "1234567890");

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "CHRIS AOMAD";
			staff.GS_Birthdate = new ZDate(1965, 09, 19);
			staff.GS_Gender = Constants.Genders.Man;
			staff.GS_RN_NKNationalityCode = Constants.CountryCodes.UnitedStates;

			staffCert1 = AddCertificate(staff.Certificates, CertificateTypePairList.Codes.PA1, "15504141", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates);
			staffCert2 = AddCertificate(staff.Certificates, "XXX", "1234567890");
			staffCert3 = AddCertificate(staff.Certificates, CertificateTypePairList.Codes.CA1, "P100971204141", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates, USStatesList.Codes.Virginia);

			person = Factory.NewWithValidTestData<CusInBondPersonForTesting>();
			Factory.Save();
		}

		void AssertPersonSychedWithStaff(CusInBondPerson synchedPerson)
		{
			AssertEquals("CP_FullName", staff.GS_FullName, synchedPerson.CP_FullName);
			AssertEquals("CP_DateOfBirth", staff.GS_Birthdate, synchedPerson.CP_DateOfBirth);
			AssertEquals("CP_Gender", staff.GS_Gender, synchedPerson.CP_Gender);
			AssertEquals("CP_RN_NKNationality", staff.GS_RN_NKNationalityCode, synchedPerson.CP_RN_NKNationality);

			AssertEquals("Certificates.Count", 2, synchedPerson.Certificates.Count);

			AssertCertificate("Should contain passport", synchedPerson.Certificates, staffCert1, true);
			AssertCertificate("Should NOT contain not applicable certificate", synchedPerson.Certificates, staffCert2, false);
			AssertCertificate("Should contain driver's license", synchedPerson.Certificates, staffCert3, true);
		}

		void AssertPersonSychedWithContact(CusInBondPerson synchedPerson)
		{
			AssertEquals("CP_FullName", contact.OC_ContactName, synchedPerson.CP_FullName);
			AssertEquals("CP_DateOfBirth", contact.OC_Birthday, synchedPerson.CP_DateOfBirth);
			AssertEquals("CP_Gender", contact.OC_Gender, synchedPerson.CP_Gender);
			AssertEquals("CP_RN_NKNationality", contact.OC_RN_NKNationality, synchedPerson.CP_RN_NKNationality);

			AssertEquals("Certificates.Count", 2, synchedPerson.Certificates.Count);

			AssertCertificate("Should contain passport", synchedPerson.Certificates, contactCert1, true);
			AssertCertificate("Should contain driver's license", synchedPerson.Certificates, contactCert2, true);
			AssertCertificate("Should NOT contain not applicable certificate", synchedPerson.Certificates, contactCert3, false);
		}

		static void AssertCertificate(string message, IEnumerable<GenRegCertAccredMaintList> certificates, GenRegCertAccredMaintList certificate, bool contains)
		{
			var result = (from cert in certificates
						  where cert.XZ_Type == certificate.XZ_Type
								&& cert.XZ_RefNumber == certificate.XZ_RefNumber
								&& cert.XZ_ExpiryOrDueDate == certificate.XZ_ExpiryOrDueDate
								&& cert.XZ_RN_NKCountryOfIssuance == certificate.XZ_RN_NKCountryOfIssuance
								&& cert.XZ_StateOrProvinceOfIssuance == certificate.XZ_StateOrProvinceOfIssuance
						  select cert).FirstOrDefault();

			if (contains)
			{
				AssertNotNull(message, result);
			}
			else
			{
				AssertNull(message, result);
			}
		}

		static void AssertSynchedDataIsEmptyAtRowLevel(CusInBondPerson synchedPerson)
		{
			var row = ((IBusinessObjectInternals)synchedPerson).Row;
			AssertEquals("CP_FullName", string.Empty, row[CusInBondPerson.Schema.CP_FullName]);
			AssertEquals("CP_DateOfBirth", DBNull.Value, row[CusInBondPerson.Schema.CP_DateOfBirth]);
			AssertEquals("CP_Gender", string.Empty, row[CusInBondPerson.Schema.CP_Gender]);
			AssertEquals("CP_RN_NKNationality", string.Empty, row[CusInBondPerson.Schema.CP_RN_NKNationality]);
			AssertEquals("Certificates.Count", 0, new GenRegCertAccredMaintListCollection(synchedPerson).Count);
		}

		static void AssertReadOnly(CusInBondPerson person, bool staff, bool contact, bool nationality, bool certificates, bool gender, bool fullName, bool birthdate)
		{
			AssertEquals("CP_GS_NKStaff.ReadOnly", staff, person.CP_GS_NKStaffInfo.ReadOnly);
			AssertEquals("CP_OC_Contact.ReadOnly", contact, person.CP_OC_ContactInfo.ReadOnly);
			AssertEquals("CP_FullName.ReadOnly", fullName, person.CP_FullNameInfo.ReadOnly);
			AssertEquals("CP_DateOfBirth.ReadOnly", birthdate, person.CP_DateOfBirthInfo.ReadOnly);
			AssertEquals("CP_Gender.ReadOnly", gender, person.CP_GenderInfo.ReadOnly);
			AssertEquals("CP_RN_NKNationality.ReadOnly", nationality, person.CP_RN_NKNationalityInfo.ReadOnly);
			AssertEquals("Certificates.ReadOnly", certificates, person.Certificates.ReadOnly);
		}

		static GenRegCertAccredMaintList AddCertificate(GenRegCertAccredMaintListCollection certificates, string type, string number, ZDateTime? expiry = null, string country = null, string state = null)
		{
			var cert = certificates.AddNew();
			cert.XZ_Type = type;
			cert.XZ_RefNumber = number;
			cert.XZ_ExpiryOrDueDate = expiry.GetValueOrDefault();
			cert.XZ_RN_NKCountryOfIssuance = country;
			cert.XZ_StateOrProvinceOfIssuance = state;
			return cert;
		}

		OrgContact contact;
		GenRegCertAccredMaintList contactCert1;
		GenRegCertAccredMaintList contactCert2;
		GenRegCertAccredMaintList contactCert3;
		GlbStaff staff;
		GenRegCertAccredMaintList staffCert1;
		GenRegCertAccredMaintList staffCert2;
		GenRegCertAccredMaintList staffCert3;
		CusInBondPersonForTesting person;
	}

	class CusInBondPersonForTesting : CusInBondPerson
	{
		public CusInBondPersonForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Header")]
		public override ZGuid CP_BH_Header
		{
			get { return base.CP_BH_Header; }
			set { base.CP_BH_Header = value; }
		}

		public CusInBondHeaderForTesting Header => Factory.Load<CusInBondHeaderForTesting>(CP_BH_Header);

		public override ICodeDescriptionPairList GetCertificateTypeList()
		{
			var list = new CertificateTypePairList();
			list.AddPair("TST", "Test Certificate");
			return list;
		}

		public override ICodeDescriptionPairList GetActiveCertificateTypeList() => GetCertificateTypeList();
	}
}
