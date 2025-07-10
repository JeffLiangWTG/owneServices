using System.Web;
using CargoWise.Types;
using Enterprise.DocumentScanning.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(DocumentColumnProvider))]
	[HttpContextEnabledTest]
	sealed class DocumentColumnProviderTest : GridColumnProviderTest
	{
		#region TestCases

		public override void TestColumnKeys()
		{
			base.TestColumnKeys();

			LoginAsQuickViewUser();
			SetupNewProvider();
			base.TestColumnKeys();
		}

		public override void TestDefaultColumns()
		{
			base.TestDefaultColumns();

			LoginAsQuickViewUser();
			SetupNewProvider();
			base.TestDefaultColumns();
		}

		public override void TestRequiredColumns()
		{
			base.TestRequiredColumns();

			LoginAsQuickViewUser();
			SetupNewProvider();
			base.TestRequiredColumns();
		}

		public override void TestUniqueColumns()
		{
			base.TestUniqueColumns();

			LoginAsQuickViewUser();
			SetupNewProvider();
			base.TestUniqueColumns();
		}

		#endregion

		string ShipmentQuickViewUserLoginRequest
		{
			get
			{
				return @"javascript:return confirm('Error: You need to be logged in with a full account to see this information.\n\nPress Ok to Login now ? Cancel to continue');"; // Javascript HTML
			}
		}

		string RelatedDocumentViewerURLFormatString
		{
			get { return string.Format("{0}?Ref={{0}}&Doc={{1}}", eDocsRequestHandler.RequestHelper.BaseUrl); } // Its string formater
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZDateTimeColumn("Date", DocumentView.Schema.DateReceived)
			{
				ColumnKey = WebTracker.Grids.Documents.Date,
				DateTimeFormat = ZDateTimePickerFormat.Short
			}); // May be an identifier

			AddDefaultsColumn(new ZTextEditColumn("Description", DocumentView.Schema.Description)
			{
				ColumnKey = WebTracker.Grids.Documents.Description
			}); // May be an identifier

			AddDefaultsColumn(new ZTextEditColumn("Type", DocumentView.Schema.RT_Desc)
			{
				ColumnKey = WebTracker.Grids.Documents.Type
			}
				); // May be an identifier

			TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			if (siteUser != null && !siteUser.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(siteUser.IsShipmentQuickViewUser
												? new ZHyperLinkColumn("Link", "")
												{
													ColumnKey = WebTracker.Grids.Documents.View,
													DataNavigateUrlFormatString = HttpContext.Current.Request.Url.ToString(),
													DataNavigateUrlFields = System.Array.Empty<string>(),
													Text = "View",
													ClientClickHandler = ShipmentQuickViewUserLoginRequest
												}
												: new ZHyperLinkColumn("Link", "")
												{
													ColumnKey = WebTracker.Grids.Documents.View,
													DataNavigateUrlFormatString = RelatedDocumentViewerURLFormatString,
													DataNavigateUrlFields = new[] { DocumentView.Schema.ParentPK, DocumentView.Schema.StorageDocPK },
													Text = "View"
												});
			}
		}

		TrackingSiteUser LoggedSiteUser
		{
			get
			{
				return WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			}
		}

		void LoginAsQuickViewUser()
		{
			OrgContact contact = Factory.Load<OrgContact>(new ZGuid("f960e868-fef4-4cab-a1f5-3ace433c04e9"));
			AssertNotNull("Fixed Contact", contact);
			AssertEquals("Contact Name", "TONY MORAN - SALES", contact.OC_ContactName);
			contact.OC_WebAccessEnabled = true;
			var password = "test";
			contact.SetHashedPassword(password);

			Factory.Save();

			TrackingSiteUser user = new TrackingSiteUser();
			user.Login(contact.ParentOrg.OH_Code, contact.OC_Email, password);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);
			AssertNotNull("No tracking site user", LoggedSiteUser);
			AssertEquals("IsLogged", true, LoggedSiteUser.IsLoggedIn);
			AssertEquals("Not IsShipmentQuickViewUser", false, LoggedSiteUser.IsShipmentQuickViewUser);
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new DocumentColumnProvider();
		}
	}
}
