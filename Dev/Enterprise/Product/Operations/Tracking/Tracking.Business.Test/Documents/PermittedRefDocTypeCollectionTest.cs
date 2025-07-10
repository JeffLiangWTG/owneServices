using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Documents.Testing
{
	[TestedType(typeof(PermittedRefDocTypeCollection))]
	sealed class PermittedRefDocTypeCollectionTest : ActiveBusinessObjectCollectionTestCase<PermittedRefDocTypeCollection>
	{
		public void TestConstructorDoesNotAllowSiteUserNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PermittedRefDocTypeCollection(Factory, null));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PermittedRefDocTypeCollection(Factory, new ZQuery(), null));
		}

		public void TestPermittedRefDocTypeCollectionMatchesFilter()
		{
			var docType1 = Factory.NewWithValidTestData<RefDocType>();
			var securityRights = new DocumentWebSecurityRights();
			var docWebSecurityRight = securityRights.GetSecurityRight(docType1);

			var docType2 = Factory.NewWithValidTestData<RefDocType>();

			var helper = new TestHelper(Factory);
			var orgRight = helper.TestOrg.SecurityRights.Cast<OrgSecurity>().First(r => r.OX_SecurityItemName == docWebSecurityRight.Code);
			AssertNotNull("organisation right should not be null", orgRight);

			var contactRight = helper.TestContact.SecurityRightsForBindingOnly[0];
			AssertNotNull("contact right should not be null", contactRight);
			contactRight.OZ_OX = orgRight.PK;
			contactRight.OZ_Granted = false;
			Factory.Save();

			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			Assert("test site user should not have the permission for the new doc type", !helper.TestSiteUser.AreSecurityRightsGranted(docWebSecurityRight));

			var permittedRefDocTypeCollection = new PermittedRefDocTypeCollection(Factory, new ZQuery(RefDocTypeSchema.PK, new ZGuid[] { docType1.PK, docType2.PK }), helper.TestSiteUser);
			AssertEquals("the collection should not contain the new doc type as site user has no permission for doc type 1", 1, permittedRefDocTypeCollection.Count);

			contactRight.OZ_Granted = true;
			Factory.Save();
			helper.TestSiteUser.Logout();
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			Assert("test site user should have the permission for the new doc type", helper.TestSiteUser.AreSecurityRightsGranted(docWebSecurityRight));
			AssertEquals("the collection should contain the new doc type as site user has the permission for doc type 1", 2, permittedRefDocTypeCollection.Count);
		}

		protected override PermittedRefDocTypeCollection GetCollectionToTest()
		{
			var helper = new TestHelper(Factory);
			return new PermittedRefDocTypeCollection(Factory, helper.TestSiteUser);
		}
	}
}
