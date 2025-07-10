using System;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	public partial class HAWBList : BasePageWithAuthorisation
	{
		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return TrackingMAWBHeader.FromPKFilteredBySiteUser(Factory, GetGuidFromParameter("Ref"), SiteUser);
		}

		public TrackingMAWBHeader MAWB
		{
			get { return DataSource as TrackingMAWBHeader; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion

		#region Events

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			bool isDataSourceValid = (MAWB != null);

			AuthorisedContent.Visible = isDataSourceValid;
			NotFoundError.Visible = !isDataSourceValid;

			if (!isDataSourceValid)
			{
				ShipmentNotFoundLabel.Text = Res.GetString("ecc1e1a8-ee9e-4ffd-bf77-e40255dda7c9", "MAWB was not found in the database or you don't have rights to view it.");
			}
			else
			{
				SetupPage();
			}
			if (IsAsyncPostBack)
			{
				SuspendValidationErrorMessageForCurrentLoad();
			}
		}

		protected override void RenderPageSpecificScripts()
		{
			base.RenderPageSpecificScripts();
			RenderReloadHAWBsScript();
		}

		#region Reload HAWBs Script

		const string ReloadHAWBsScriptKey = "ReloadHAWBsScriptKey";

		void RenderReloadHAWBsScript()
		{
			if (!ZClientScript.IsClientScriptBlockRegistered(GetType(), ReloadHAWBsScriptKey))
			{
				string javaScript = @"<SCRIPT>
                                        function ReloadHAWBs()
                                        {{
											__doPostBack('{0}','');
                                        }}

										function ShowReloadButton()
										{{
												$('{1}').style.display = 'inline';
										}}
                                        </SCRIPT>";
				ZClientScript.RegisterClientScriptBlock(GetType(), ReloadHAWBsScriptKey, string.Format(javaScript, RelatedHAWBsGrid.ClientID, ReloadGridSection.ClientID));
			}
		}

		#endregion

		#endregion

		#region Page Setup

		void SetupPage()
		{
			ParentBill.Text = String.Format((NoResString)"MAWB # {0}", MAWB.EH_WayBillNumber); // JavaScript Code
			ParentBill.OnClientClick = String.Format((NoResString)"javascript:window.location='{0}';return(false);", String.Format((NoResString)"{0}?Ref={1}", AppInstance.MAWBDetailsPage, MAWB.PK)); // JavaScript Code

			ReloadGrid.OnClientClick = String.Format((NoResString)"javascript:ReloadHAWBs();$('{0}').style.display='none';return(false);", ReloadGridSection.ClientID); // JavaScript Code
		}

		protected override void SetupGrids()
		{
			base.SetupGrids();

			RelatedHAWBsGrid.ColumnProvider = new TrackingHAWBHeaderColumnProvider(true);
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.HAWBList;
		}

		#endregion

		#region Authorized

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewMAWB; }
		}

		#endregion
	}
}
