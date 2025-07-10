using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingDeclarationStaticGeneratorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			CreateTestJobDeclaration();
			Factory.Save();
		}

		void CreateTestJobDeclaration()
		{
			TestJobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			TestJobDeclaration.JE_DeclarationReference = "B123456789";
			TestJobDeclaration.JE_OH_Supplier = TestOrg.PK;
		}
		BaseJobDeclaration TestJobDeclaration;

		TrackingSiteUser TestSiteUser
		{
			get
			{
				if (fTestSiteUser == null)
				{
					fTestSiteUser = new TrackingSiteUser();
					fTestSiteUser.Login(TestOrg.OH_Code, TestContact.OC_Email, Password);
				}

				return fTestSiteUser;
			}
		}
		TrackingSiteUser fTestSiteUser;

		OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
				}
				return fTestOrg;
			}
		}
		OrgHeader fTestOrg;

		OrgContact TestContact
		{
			get
			{
				if (fTestContact == null)
				{
					fTestContact = TestOrg.Contacts.AddNew();
					fTestContact.OC_Email = "test@cargowise.com";
					fTestContact.OC_WebAccessEnabled = true;
					fTestContact.SetHashedPassword(Password);
				}
				return fTestContact;
			}
		}
		OrgContact fTestContact;
		const string Password = "test";

		#region TestFromNumber
		[HttpContextEnabledTest]
		public void TestFromNumber()
		{
			TestContact.Factory.Save();
			TrackingDeclaration testDeclarationFromNumber = TrackingDeclaration.FromPKFilteredBySiteUser(Factory, TestJobDeclaration.PK, TestSiteUser);
			AssertEquals(TestJobDeclaration, testDeclarationFromNumber.Declaration);
			AssertEquals(TestContact.PK, testDeclarationFromNumber.LoggedInContact.PK);
			AssertEquals(TestOrg.PK, testDeclarationFromNumber.LoggedInOrganisation.PK);
		}

		public void TestFromNumberIncorrectOrg()
		{
			TrackingDeclaration testDeclarationFromNumber = TrackingDeclaration.FromPKFilteredBySiteUser(Factory, TestJobDeclaration.PK, null);
			AssertNull(testDeclarationFromNumber);
		}

		#endregion TestFromNumber
	}
}
