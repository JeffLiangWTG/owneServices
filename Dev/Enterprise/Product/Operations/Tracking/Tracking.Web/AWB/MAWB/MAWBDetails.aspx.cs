using System;
using System.ComponentModel;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.Tracking.Web
{
	public partial class MAWBDetails : BasePageWithAuthorisation
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
			get { return isPersistDataSource; }
		}

		readonly bool isPersistDataSource = true;

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

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			ZtextlabelStatus.Bind(DataSource);
			DisabledSubmitButtons.Add(SaveMAWB.ClientID);
			DisabledSubmitButtons.Add(SendFWB.ClientID);
			DisabledSubmitButtons.Add(SendFWBFHL.ClientID);
			DisabledSubmitButtons.Add(ResendFWBFHL.ClientID);
		}

		protected void SaveMAWB_Click(object sender, EventArgs e)
		{
			SaveDataSourceFactory();
		}

		protected void SendFWB_Click(object sender, EventArgs e)
		{
			ShouldReSendFWB = true;
			SaveDataSourceFactory();
		}

		protected void SendFWBFHL_Click(object sender, EventArgs e)
		{
			ShouldSendFWBFHL = true;
			SaveDataSourceFactory();
		}

		protected void ResendFWBFHL_Click(object sender, EventArgs e)
		{
			ShouldReSendFWBFHL = true;
			SaveDataSourceFactory();
		}

		[DefaultValue(false)]
		public bool ShouldReSendFWB { get; set; }

		[DefaultValue(false)]
		public bool ShouldReSendFWBFHL { get; set; }

		[DefaultValue(false)]
		public bool ShouldSendFWBFHL { get; set; }

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			if (ShouldReSendFWB || ShouldReSendFWBFHL || ShouldSendFWBFHL)
			{
				var cleanFactory = new BusinessObjectFactory();
				var mAWBReloaded = cleanFactory.Load<TrackingMAWBHeader>(MAWB.PK);
				if (mAWBReloaded != null)
				{
					var messageManager = new MessageSendingManager(mAWBReloaded, SiteUser.LoggedInUser);
					var resultMessage = string.Empty;
					var messageResult = false;

					if (ShouldReSendFWB)
					{
						messageResult = messageManager.ReSendThisOnly(out resultMessage);
					}
					else if (ShouldReSendFWBFHL)
					{
						messageResult = messageManager.ReSendAll(out resultMessage);
					}
					else if (ShouldSendFWBFHL)
					{
						messageResult = messageManager.SendAll(out resultMessage);
					}

					Session[zPageCustomAlertMessageIndexer] = resultMessage;
				}
				else
				{
					try
					{
						((WebExceptionReporter)ExceptionReporter.Instance).ReportDeveloperException((NoResString)"MAWB Not Found - Send Failure", string.Format((NoResString)"MAWB database record could not be found. MAWB PK: {0}, Reference: {1}, WayBillNumber: {2}", MAWB.PK, MAWB.EH_ReferenceNumber, MAWB.EH_WayBillNumber), new Exception("Developer generated exception")); // Developer only
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
					Session[zPageCustomAlertMessageIndexer] = Res.GetString("999c922c-3038-4269-b5e9-27eefcbb7e1d", "There was a problem while sending. Please try sending again.");
				}
			}

			RedirectToMAWBDetailsPage();
		}

		protected void RedirectToMAWBDetailsPage()
		{
			Response.Redirect(String.Format((NoResString)"{0}?Ref={1}", AppInstance.MAWBDetailsPage, MAWB.PK)); // Redirect path
		}

		#endregion

		#region Services

		public void RegisterAWBService()
		{
			if (!this.AJAX.ScriptManager.Services.Contains(AWBWebService))
			{
				this.AJAX.ScriptManager.Services.Add(AWBWebService);
			}
		}

		protected ServiceReference AWBWebService
		{
			get
			{
				return awbWebService ?? (awbWebService = new ServiceReference(AppInstance.ApplicationRoot + "WebService/AWBService.asmx"));
			}
		}

		ServiceReference awbWebService;

		#endregion

		#region Page Setup

		void SetupPage()
		{
			ViewMessages.OnClientClick = String.Format((NoResString)"javascript:window.open('{0}', '{1}', '{2}');return(false);", String.Format((NoResString)"{0}?Ref={1}&ParentType={2}&{3}={4}", AppInstance.EDIMessagesPage, MAWB.PK, Server.UrlEncode(MAWB.GetType().AssemblyQualifiedName), TrackingConstants.QueryStringKeys.PopupKey, "Y"), (NoResString)"_blank", Global.EDIMessagePopupWindowStyle); // JavaScript Code

			GoToHAWB.OnClientClick = String.Format((NoResString)"javascript:window.location='{0}';return(false);", String.Format((NoResString)"{0}?Ref={1}", AppInstance.HAWBListPage, MAWB.PK)); // JavaScript Code

			SendFWB.Visible = MAWB.HasBeenSent;
			SendFWB.Enabled = SiteUser.CanAdminMAWB;

			SendFWBFHL.Enabled = SiteUser.CanSendMAWB;

			ResendFWBFHL.Visible = MAWB.HasBeenSent;
			ResendFWBFHL.Enabled = SiteUser.CanAdminMAWB;

			SaveMAWB.Enabled = SiteUser.CanEditMAWB;
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.MAWBDetails;
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
