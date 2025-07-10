using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	partial class DataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetMatchedByNameEmailAndOrgPhone()
		{
			var name = "Justin Chen";
			var email = "justin.chen@wisetechglobal.com";
			var phone = "123";
			var anotherPhone = "456";
			var anotherEmail = "another@another.com";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
			anotherOrg.MainAddress.OA_Phone = anotherPhone;

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = name;
			contact.OC_Email = email;
			contact.OC_Phone = phone;

			var anotherContact = Factory.New<OrgContact>();
			anotherContact.OC_OH = anotherOrg.PK;
			anotherContact.OC_ContactName = name;
			anotherContact.OC_Email = anotherEmail;
			Factory.SaveForTesting();

			var organizationContact = new OrganizationContact { FullName = name, Email = email, Phone = phone };
			var reader = new OrganizationContactDataObjectReader(organizationContact, logger, Factory);
			AssertEquals(contact, reader.GetMatched());

			var anotherOrganizationContact = new OrganizationContact { FullName = name, Email = anotherEmail, Phone = anotherPhone };
			var anotherReader = new OrganizationContactDataObjectReader(anotherOrganizationContact, logger, Factory);
			AssertEquals(anotherContact, anotherReader.GetMatched());
		}

		public void TestOrganizationContactDataObjectReaderGetMatched()
		{
			AssertNotNull(ContactBobPhonePizzaHut);
			AssertNotNull(ContactWendyPhonePizzaHut);
			AssertNotNull(ContactBobPhoneDominos);

			var organizationContact = new OrganizationContact();
			var reader = new OrganizationContactDataObjectReader(organizationContact, logger, Factory);
			AssertNull(reader.GetMatched());
			organizationContact.FullName = BobName;
			organizationContact.Phone = PizzaHutNumber;
			AssertEquals(ContactBobPhonePizzaHut, reader.GetMatched());

			organizationContact.FullName = WendyName;
			AssertEquals(ContactWendyPhonePizzaHut, reader.GetMatched());

			organizationContact.Phone = DominosNumber;
			AssertNull(reader.GetMatched());

			organizationContact.FullName = BobName;
			AssertEquals(contactBobPhoneDominos, reader.GetMatched());
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		OrgContact ContactBobPhonePizzaHut
		{
			get
			{
				if (contactBobPhonePizzaHut == null)
				{
					contactBobPhonePizzaHut = Factory.New<OrgContact>();
					contactBobPhonePizzaHut.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					contactBobPhonePizzaHut.OC_ContactName = BobName;
					contactBobPhonePizzaHut.OC_Phone = PizzaHutNumber;
					Factory.SaveForTesting();
				}
				return contactBobPhonePizzaHut;
			}
		}
		OrgContact contactBobPhonePizzaHut;

		const string BobName = "BOB THE BUILDER";
		const string WendyName = "WENDY THE DESTROYER";
		const string PizzaHutNumber = "1300 PIZZA HUT";
		const string DominosNumber = "1300 DOMINOS";

		OrgContact ContactWendyPhonePizzaHut
		{
			get
			{
				if (contactWendyPhonePizzaHut == null)
				{
					contactWendyPhonePizzaHut = Factory.New<OrgContact>();
					contactWendyPhonePizzaHut.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					contactWendyPhonePizzaHut.OC_ContactName = WendyName;
					contactWendyPhonePizzaHut.OC_Phone = PizzaHutNumber;
					Factory.SaveForTesting();
				}
				return contactWendyPhonePizzaHut;
			}
		}
		OrgContact contactWendyPhonePizzaHut;

		OrgContact ContactBobPhoneDominos
		{
			get
			{
				if (contactBobPhoneDominos == null)
				{
					contactBobPhoneDominos = Factory.New<OrgContact>();
					contactBobPhoneDominos.OC_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_OH_OrgProxy)).PK;
					contactBobPhoneDominos.OC_ContactName = BobName;
					contactBobPhoneDominos.OC_Phone = DominosNumber;
					Factory.SaveForTesting();
				}
				return contactBobPhoneDominos;
			}
		}
		OrgContact contactBobPhoneDominos;
	}
}

