using System.Web;
using Enterprise.DocumentScanning.Web;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class DocumentColumnProvider : GridColumnProvider
	{
		public DocumentColumnProvider()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript HTML")]
		public const string ShipmentQuickViewUserLoginRequest = @"javascript:return confirm('Error: You need to be logged in with a full account to see this information.\n\nPress Ok to Login now ? Cancel to continue');";

		protected string RelatedDocumentViewerURLFormatString
		{
			get { return string.Format((NoResString)"{0}?Ref={{0}}&Doc={{1}}", eDocsRequestHandler.RequestHelper.BaseUrl); } // Its string formater
		}

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("54febee3-fed1-4b22-968b-f616adf5e2b9", "Date"), DocumentView.Schema.DateReceived)
			{
				ColumnKey = WebTracker.Grids.Documents.Date,
				DateTimeFormat = ZDateTimePickerFormat.Short
			}); // May be an identifier

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("606cd771-f32c-4e31-af9d-80f796cb7ccf", "Description"), DocumentView.Schema.Description)
			{
				ColumnKey = WebTracker.Grids.Documents.Description
			}); // May be an identifier

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("77f21913-56c1-425a-a863-0230bbe28c62", "Type"), DocumentView.Schema.RT_Desc)
			{
				ColumnKey = WebTracker.Grids.Documents.Type
			}
				); // May be an identifier

			TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			if (siteUser != null && !siteUser.IsShipmentQuickViewUser)
			{
				AddToDictionaryAsDefault(siteUser.IsShipmentQuickViewUser
												? new ZHyperLinkColumn(Res.GetString("8ff49adc-4100-4802-bdb0-09f694eb953d", "Link"), "")
												{
													ColumnKey = WebTracker.Grids.Documents.View,
													DataNavigateUrlFormatString = HttpContext.Current.Request.Url.ToString(),
													DataNavigateUrlFields = System.Array.Empty<string>(),
													Text = Res.GetString("0d1dc61c-2252-4ef7-bd4b-35e0deb8c2ec", "View"),
													ClientClickHandler = ShipmentQuickViewUserLoginRequest
												}
												: new ZHyperLinkColumn(Res.GetString("8ff49adc-4100-4802-bdb0-09f694eb953d", "Link"), "")
												{
													ColumnKey = WebTracker.Grids.Documents.View,
													DataNavigateUrlFormatString = RelatedDocumentViewerURLFormatString,
													DataNavigateUrlFields = new[] { DocumentView.Schema.ParentPK, DocumentView.Schema.StorageDocPK },
													Text = Res.GetString("0d1dc61c-2252-4ef7-bd4b-35e0deb8c2ec", "View")
												});
			}
		}
	}
}
