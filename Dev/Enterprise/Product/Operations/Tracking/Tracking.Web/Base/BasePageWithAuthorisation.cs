using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public abstract class BasePageWithAuthorisation : BasePage
	{
		#region Constants

		protected static string UnauthorisedAccessMessage
		{
			get { return Res.GetString("a354ac4f-d582-4d42-969a-303942adb0b9", "You are not authorized to view this page. Please contact your system administrator to request access rights."); }
		}

		#endregion

		#region Controls

		protected internal ZTextLabel UnauthorisedLabel;
		protected internal System.Web.UI.HtmlControls.HtmlGenericControl UnauthorisedDiv;
		protected internal System.Web.UI.HtmlControls.HtmlGenericControl AuthorisedContent;

		#endregion

		#region Events

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DataSource == null)
			{
				var autoLoginQueryStringData = Session?[Global.AutoLoginQueryStringDataIndexer];
				if (autoLoginQueryStringData != null)
				{
					FormsAuthentication.SignOut();
					SiteUser?.Logout();

					var encodedData = Server?.UrlEncode(autoLoginQueryStringData as string);
					var targetUrl = $"{AppInstance?.AutoLoginRequestHandler}?{TrackingConstants.AutoLogin.SecureQueryStringDataKey}={encodedData}"; // Non-semantic text
					HttpContext.Current.Response?.Redirect(targetUrl);
				}
			}

			SetupAuthorisedContentInternal();
			SetupModulePage();
		}

		protected internal void OnUnloadInternal(EventArgs e) => OnUnload(e);

		protected override void OnUnload(EventArgs e)
		{
			using (Db.DisposableActionForDbConnection())
			{
				base.OnUnload(e);

				var session = Session;
				if (session != null)
				{
					session[Global.AutoLoginQueryStringDataIndexer] = null;
				}

				var siteUser = SiteUser;
				if (siteUser?.IsShipmentQuickViewUser == true)
				{
					FormsAuthentication.SignOut();
					siteUser.Logout();
					session?.Abandon();
				}
			}
		}

		protected internal void SetupModulePage()
		{
			if (this is IModulePage)
			{
				NotificationFlags.DisplayWarnings = false;
			}
		}

		protected internal void SetupAuthorisedContentInternal()
		{
			SwitchAuthorised(CanAccessAuthorisedContent);
			SetupAuthorisedContent(CanAccessAuthorisedContent);
		}

		void SwitchAuthorised(bool on)
		{
			if (on)
			{
				UnauthorisedDiv.Visible = false;
				UnauthorisedLabel.Text = "";
				AuthorisedContent.Visible = true;
				if (!IsPostBack)
				{
					WriteLicenceUsageLog();
				}
			}
			else
			{
				UnauthorisedDiv.Visible = true;
				UnauthorisedLabel.Text = UnauthorisedAccessMessage;
				AuthorisedContent.Visible = false;
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			HideUnaccessibleControls();
		}

		void HideUnaccessibleControls()
		{
			IAccessControllablePage page = this as IAccessControllablePage;
			if (page != null && page.dataSource != null && page.dataSource.SuppressionItem != null)
			{
				string[] captionsToHide = page.dataSource.SuppressionItem.CaptionsToHide(page.dataSource);

				foreach (string caption in captionsToHide)
				{
					if (caption == AccessControlRegistryItem.EverythingIsSuppressed)
					{
						SwitchAuthorised(false);
						break;
					}

					foreach (Control control in page.GetControls(caption))
					{
						control.Visible = false;
					}
				}
			}
		}

		#endregion

		#region Saving

		protected void SaveCancelWithRefreshOnFailure(string refreshUrl)
		{
			if (SaveDataSourceFactory())
			{
				LogCancel(DataSource);
			}
			else
			{
				ShowClientSideAlert(Res.GetString("0D691D81-8820-4020-8C02-CB4D68EBCC9E", "There was a problem while saving your changes. Please try again."));
				HttpContext.Current.Response.AddHeader((NoResString)"Refresh", string.Format((NoResString)"0; url={0}", refreshUrl)); // http header
			}
		}

		#endregion

		#region Logging

		protected void LogCancel(BusinessObject bizO)
		{
			if (bizO != null)
			{
				var eventLogHelper = new EventLogHelper();
				eventLogHelper.CreateLogForCancel(SiteUser, bizO);
			}
		}

		protected override void LogModuleChange()
		{
			if (CanAccessAuthorisedContent)
			{
				base.LogModuleChange();
			}
		}

		protected override void OnBeforeDataSourceFactorySaved()
		{
			base.OnBeforeDataSourceFactorySaved();

			if (DataSource != null)
			{
				DataSourceIsANewBusinessObject = !DataSource.IsInDatabase;
			}
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();

			if (DataSource != null && DataSource.IsInDatabase && DataSourceIsANewBusinessObject.HasValue)
			{
				if (DataSourceIsANewBusinessObject.Value)
				{
					new EventLogHelper().CreateLogForAdd(SiteUser, DataSource);
				}
				else
				{
					new EventLogHelper().CreateLogForEdit(SiteUser, DataSource);
				}

				DataSourceIsANewBusinessObject = null;
			}
		}

		bool? DataSourceIsANewBusinessObject;

		#endregion

		#region Licence Usage Logging

		protected virtual internal IReadOnlyCollection<ILicenceCheckpoint> LicenceCheckPoints
		{
			get
			{
				return AppInstance.WebAccessManager.LicenceCheckpoints(PageRelativePath);
			}
		}

		void WriteLicenceUsageLog()
		{
			ILicenceUsageLogWriter writer = WebEnv.AppInstance as ILicenceUsageLogWriter;
			if (writer != null)
			{
				writer.WriteLicenceUsageLog(LicenceCheckPoints.ToArray());
			}
		}

		#endregion

		#region Documents

		protected void GetDocument(ZString documentName, ZString documentPath)
		{
			GetDocument((IDocumentSupportable)DataSource, documentName, documentPath);
		}

		protected void GetDocument(IDocumentSupportable documentOwner, ZString documentName, ZString documentPath)
		{
			if (documentOwner == null)
			{
				throw new ArgumentException("documentOwner cannot be null", nameof(documentOwner));
			}

			DocumentCommand documentCommand = DocumentCommand.GetDocumentCommand(Factory, documentOwner, documentName, documentPath, ZString.Empty);

			if (documentCommand != null)
			{
				EventLogHelper eventLogHelper = new EventLogHelper();
				eventLogHelper.CreateLogForDocumentPrinted(SiteUser, documentName);
				string handlerUrl = DocumentRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid[] { Page.DataSourceIndexer, documentCommand.PK });
				HttpContext.Current.Response.Redirect(handlerUrl);
			}
			else
			{
				AppInstance.ReportError(Res.GetString("e9a7dd4f-f176-4ac4-8ff1-7c0f20426d3f", "Document not found"), Res.GetString("9d9afcfc-59b8-498f-8726-da0fa0a8bd33", "Document cannot be generated because system has not found the {0}>{1} Document.", documentPath, documentName));
			}
		}

		#endregion

		#region Implementation

		protected virtual void SetupAuthorisedContent(bool isAuthorised)
		{
		}

		protected abstract bool CanAccessAuthorisedContent { get; }

		#endregion

		#region Declaration Status Control

		protected string GetStatusControlPath(string countryCode, string direction)
		{
			return string.Format((NoResString)"{0}Declaration/Controls/Status/{1}/{2}/StatusControl.ascx", AppInstance.ApplicationRoot, countryCode, direction); // Formatting virtual url
		}

		#endregion
	}
}
