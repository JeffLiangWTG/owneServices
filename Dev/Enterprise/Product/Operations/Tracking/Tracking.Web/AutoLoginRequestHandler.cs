using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.SessionState;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Web.Base;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	public class AutoLoginRequestHandler : IHttpHandler, IRequiresSessionState
	{
		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			var wrapper = new HttpContextWrapper(context);

			ProcessRequest(wrapper);
		}

		public void ProcessRequest(HttpContextBase context)
		{
			if (!IsRedirectedByJavascriptForExcelHack(context))
			{
				DoJavascriptRedirectionForExcelHack(context);
			}
			else
			{
				var webController = new ZWebController(context);
				var queryStringData = context.Request.QueryString[TrackingConstants.AutoLogin.SecureQueryStringDataKey];
				try
				{
					var queryString = new SecureQueryString(queryStringData);
					var contactPK = new ZGuid(queryString[TrackingConstants.AutoLogin.ContactPKKey]);
					var requireLogin = false;
					if (!bool.TryParse(queryString[TrackingConstants.AutoLogin.RequireLoginKey], out requireLogin))
					{
						requireLogin = false;
					}
					var businessContext = TrackingConstants.BusinessContext.NoBusinessContext;
					var businessContextKey = queryString[TrackingConstants.AutoLogin.BusinessContextKey];
					if (!string.IsNullOrEmpty(businessContextKey))
					{
						businessContext = (TrackingConstants.BusinessContext)Enum.Parse(typeof(TrackingConstants.BusinessContext), queryString[TrackingConstants.AutoLogin.BusinessContextKey], true);
					}
					if (!contactPK.IsEmpty ||
						(contactPK.IsEmpty &&
						(businessContext != TrackingConstants.BusinessContext.Transaction &&
							businessContext != TrackingConstants.BusinessContext.eDoc &&
							businessContext != TrackingConstants.BusinessContext.Quotations &&
							businessContext != TrackingConstants.BusinessContext.QuotationClientReplyAccept &&
							businessContext != TrackingConstants.BusinessContext.QuotationClientReplyNotAccept &&
							businessContext != TrackingConstants.BusinessContext.HouseBill &&
							businessContext != TrackingConstants.BusinessContext.FreightLabel)))
					{
						if (contactPK.IsEmpty && businessContext == TrackingConstants.BusinessContext.NoBusinessContext)
						{
							requireLogin = true;
						}
						using (Db.DisposableActionForDbConnection())
						{
							if (!string.IsNullOrEmpty(queryString[TrackingConstants.AutoLogin.BusinessContextPKKey]))
							{
								var businessContextPK = new ZGuid(queryString[TrackingConstants.AutoLogin.BusinessContextPKKey]);
								var additionalRefsString = queryString[TrackingConstants.AutoLogin.BusinessContextAdditionalRefsKey];
								var businessContextAdditionalRefs = !string.IsNullOrEmpty(additionalRefsString) ? new ZString(queryString[TrackingConstants.AutoLogin.BusinessContextAdditionalRefsKey]) : ZString.Empty;

								webController.RedirectToTrackingPage(contactPK, businessContext, businessContextPK, StringToZGuids(businessContextAdditionalRefs), requireLogin, queryStringData);
							}
							else if (!string.IsNullOrEmpty(queryString[TrackingConstants.AutoLogin.BusinessContextNKKey]))
							{
								var businessContextNK = new ZString(queryString[TrackingConstants.AutoLogin.BusinessContextNKKey]);
								webController.RedirectToTrackingPage(contactPK, businessContext, businessContextNK, requireLogin, queryStringData);
							}
							else
							{
								webController.RedirectToTrackingPage(contactPK, businessContext, ZString.Empty, requireLogin, queryStringData);
							}
						}
					}
				}
				catch (QueryStringException)
				{
				}
			}
		}

		ZGuid[] StringToZGuids(ZString refsString)
		{
			List<ZGuid> result = new List<ZGuid>();

			if (!refsString.IsEmpty)
			{
				ZString[] refs = refsString.Split(',');

				foreach (ZString guidStr in refs)
				{
					result.Add(new ZGuid(guidStr));
				}
			}

			return result.ToArray();
		}

		bool IsRedirectedByJavascriptForExcelHack(HttpContextBase context)
		{
			return (context.Request.QueryString["ClientRedirection"] != null);
		}

		/// <summary>
		/// This is required because Excel / Winword keeps its own browser handler before actually opening a new browser window.
		/// This means that the newly opened browser window would have a different session ID to the excel one and redirection wouldn't work as the authorization cookie is stored against the session.
		/// This is a funky workround to navigate the URL from the existing session through Javascript.
		/// </summary>
		/// <param name="context"></param>
		void DoJavascriptRedirectionForExcelHack(HttpContextBase context)
		{
			var appInstance = (Global)context.ApplicationInstance;
			var queryString = string.Format(CultureInfo.InvariantCulture, "{0}{1}ClientRedirection=TRUE", context.Request.Url.Query, (string.IsNullOrEmpty(context.Request.Url.Query)) ? "?" : "&"); // Non-semantic text
			context.Response.AddHeader("Content-Type", "text/html"); // Response header key value
			context.Response.AddHeader((NoResString)"Refresh", (NoResString)"0;url=" + string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}{1}", appInstance.AutoLoginRequestHandler, queryString)); // Response header key value
			context.Response.End();
		}
	}
}
