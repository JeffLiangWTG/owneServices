using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingDeclarationCollection))]
	[HttpContextEnabledTest]
	sealed class TrackingDeclarationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingDeclarationCollection>
	{
		[ExpectNoExceptions]
		public void TestLoadWhenUserNotLoggedIn_ShouldNotThrowException()
		{
			var helper = new ZWebTestHelper(Factory);
			var testOrg = helper.TestOrg;
			testOrg.OH_Code = "XXXYYYZZZ";
			testOrg.OH_IsForwarder = true;
			OrgContact testContact = helper.TestContact;
			testContact.OC_Email = "test@testcompany.com";
			testContact.SetHashedPassword("testpassword");
			testContact.OC_WebAccessEnabled = true;

			var declarationSupplierExport = GetNewTrackingDeclaration();
			declarationSupplierExport.Declaration.JE_OH_Supplier = testOrg.PK;
			declarationSupplierExport.Declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

			Factory.Save();

			WebEnv.AppInstance.SiteUser.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			var trackingDeclarationCollection = new TrackingDeclarationCollection(Factory);
			var filter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>();
			WebEnv.AppInstance.SiteUser.Logout();
			trackingDeclarationCollection.Load(filter);
		}

		TrackingDeclaration GetNewTrackingDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			return new TrackingDeclaration(declaration);
		}

		#region Implementation

		protected override TrackingDeclarationCollection GetCollectionToTest()
		{
			return new TrackingDeclarationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TrackingDeclaration(Factory.New<BaseJobDeclaration>());
		}

		#endregion
	}
}
