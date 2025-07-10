using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	public partial class HAWBDetails : BasePageWithAuthorisation
	{
		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return TrackingMAWBHeader.FromPKFilteredBySiteUser(Factory, GetGuidFromParameter("Ref"), SiteUser);
		}

		public TrackingMAWBHeader HAWB
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

			bool isDataSourceValid = (HAWB != null);

			NotificationFlags.DisplayAll = false;
			NotificationFlags.DisplayErrors = true;
			NotificationFlags.DisplayMessageErrors = true;

			AuthorisedContent.Visible = isDataSourceValid;
			NotFoundError.Visible = !isDataSourceValid;

			if (IsShownInPopup)
			{
				DefaultBody.Attributes["class"] = (NoResString)"popup"; // HTML Code
			}

			if (!isDataSourceValid)
			{
				ShipmentNotFoundLabel.Text = Res.GetString("d6657875-a318-477c-8e62-c21d70b28af8", "HAWB was not found in the database or you don't have rights to view it.");
			}
			else
			{
				SetupPage();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			ZtextlabelStatus.Bind(DataSource);
			DisabledSubmitButtons.Add(SaveHAWB.ClientID);
			DisabledSubmitButtons.Add(SendFHL.ClientID);
		}

		protected override bool ShowCloseWindowInPopup
		{
			get { return false; }
		}

		[DefaultValue(false)]
		public bool ShouldReSendFHL { get; set; }

		protected void SaveHAWB_Click(object sender, EventArgs e)
		{
			SaveDataSourceFactory();
		}

		protected override void OnDataSourceFactorySaved()
		{
			if (ShouldReSendFHL)
			{
				var messageManager = new MessageSendingManager(HAWB, SiteUser.LoggedInUser);
				var resultMessage = string.Empty;
				var messageStatus = messageManager.ReSendThisOnly(out resultMessage);

				ZClientScript.RegisterStartupScript(GetType(),
					MessageScriptKey,
					String.Format(MessageScript, resultMessage, messageStatus && IsShownInPopup ? (NoResString)"true" : (NoResString)"false")); // Javascript segment
			}
			else if (IsShownInPopup)
			{
				ZClientScript.RegisterStartupScript(GetType(), RedirectScriptKey, RedirectScript);
			}
		}

		const string RedirectScriptKey = "RedirectScript";

		string RedirectScript
		{
			get
			{
				return (NoResString)@"<SCRIPT TYPE=""text/javascript"">
					try{
						if (window.opener && !window.opener.closed){
							window.opener.ShowReloadButton();
						}
					} catch(err) { }
					window.close();
				</SCRIPT>"; // Javascript segment
			}
		}

		const string MessageScriptKey = "MessageScript";

		string MessageScript
		{
			get
			{
				return (NoResString)@"<SCRIPT TYPE=""text/javascript"">
					alert('{0}');
					if ({1})
					{{
						window.close();
					}}
				</SCRIPT>"; // Javascript segment
			}
		}

		protected void SendFHL_Click(object sender, EventArgs e)
		{
			ShouldReSendFHL = true;
			SaveDataSourceFactory();
		}

		#endregion

		#region Page Setup

		void SetupPage()
		{
			ViewMessages.OnClientClick = String.Format((NoResString)"javascript:window.open('{0}', '{1}', '{2}');return(false);", String.Format((NoResString)"{0}?Ref={1}&ParentType={2}&{3}={4}", AppInstance.EDIMessagesPage, HAWB.PK, Server.UrlEncode(HAWB.GetType().AssemblyQualifiedName), TrackingConstants.QueryStringKeys.PopupKey, "Y"), (NoResString)"_blank", Global.EDIMessagePopupWindowStyle); // JavaScript Code

			ParentBill.Text = String.Format((NoResString)"MAWB # {0}", HAWB.ParentBill.EH_WayBillNumber); // JavaScript Code

			if (IsShownInPopup)
			{
				ParentBill.Enabled = false;
			}
			else
			{
				ParentBill.OnClientClick = String.Format((NoResString)"javascript:window.location='{0}';return(false);", String.Format((NoResString)"{0}?Ref={1}", AppInstance.MAWBDetailsPage, HAWB.EH_EH_Parent)); // JavaScript Code
			}

			SaveHAWB.Enabled = SiteUser.CanEditHAWB;

			SendFHL.Enabled = SiteUser.CanAdminHAWB;
			SendFHL.Visible = HAWB.HasBeenSent;
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.HAWBDetails;
		}

		#endregion

		#region Authorized

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewHAWB; }
		}

		#endregion
	}
}
