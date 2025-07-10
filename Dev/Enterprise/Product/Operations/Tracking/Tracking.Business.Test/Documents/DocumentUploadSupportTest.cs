using System.Linq;
using System.Web;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(DocumentUploadSupport))]
	sealed class DocumentUploadSupportTest : NonPersistentBusinessObjectTestCase
	{
		[HttpContextEnabledTest]
		public void TestDocTypes()
		{
			var documentUploadSupport = new DocumentUploadSupport(Factory);
			var docTypesCount = documentUploadSupport.DocTypes.Count;

			var helper = new TestHelper(Factory);
			var orgRightForDocType = helper.TestOrg.SecurityRights.Cast<OrgSecurity>().First(r => r.SecurityKey.Contains("RefDocType"));
			AssertNotNull("organisation right should not be null", orgRightForDocType);

			var contactRight = helper.TestContact.SecurityRightsForBindingOnly[0];
			AssertNotNull("contact right should not be null", contactRight);
			contactRight.OZ_OX = orgRightForDocType.PK;
			contactRight.OZ_Granted = false;
			Factory.Save();

			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			HttpContext.Current.Session["SiteUser"] = helper.TestSiteUser;
			AssertEquals("the doc types count shoud reduce 1 because the user has no permission for one of the doc types", docTypesCount - 1, documentUploadSupport.DocTypes.Count);
		}

		public void TestDocType_MaxLength()
		{
			var documentUploadSupport = new DocumentUploadSupport(Factory);
			var maxDocType = new string('A', AutoStorageDocs.Schema.SC_DocTypeMaxLength);
			documentUploadSupport.DocType = maxDocType;

			AssertEquals(maxDocType, documentUploadSupport.DocType);
			AssertEquals(AutoStorageDocs.Schema.SC_DocTypeMaxLength, documentUploadSupport.DocTypeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}
	}
}
