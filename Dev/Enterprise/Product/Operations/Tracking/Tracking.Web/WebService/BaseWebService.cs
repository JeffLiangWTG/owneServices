using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Web.WebService
{
	/// <summary>
	/// Base web service for WebTracker
	/// </summary>
	[WebService(Namespace = "http://www.cargowise.com/EnterpriseService/")]
	public abstract class BaseWebService : WebServiceWithFactory
	{
		#region Test method for clients to try

		[WebMethod(EnableSession = true, Description = "Test method to check whether login credentials work")]
		[SoapHeader("MessageHeader")]
		public string Hello()
		{
			return ExecuteAndDispose(() =>
			{
				try
				{
					OrgHeader currentOrg = CheckMessageHeaderAndLogin();
					return "You logged in as " + currentOrg.OH_FullName;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return "No login info provided";
				}
			});
		}

		#endregion

		#region Web application - related

		protected Global AppInstance
		{
			get { return (Global)Context.ApplicationInstance; }
		}

		#endregion

		#region Authorisation and SOAP

		public WebTrackerSOAPHeader MessageHeader;

		protected OrgHeader CheckMessageHeaderAndLogin()
		{
			if (MessageHeader == null)
			{
				throw new Exception("ERROR: Please supply login credentials");
			}

			OrgHeader currentOrg = Login() ?? throw new Exception("Login information is not valid");

			return currentOrg;
		}

		OrgHeader Login()
		{
			AppInstance.SetupEnvironment();

			string companyCode = MessageHeader.CompanyCode;
			string userName = MessageHeader.UserName ?? string.Empty;
			string password = MessageHeader.Password ?? string.Empty;

			return Login(companyCode, userName, password);
		}

		/// <summary>
		/// Authenticates user and returns company PK used for filtering
		/// </summary>
		/// <param name="company">Company Code for login company</param>
		/// <param name="user1">User's email</param>
		/// <param name="password">Password</param>
		/// <returns>PK of Current company if login information is correct, ZGuid.Invalid otherwise</returns>
		OrgHeader Login(string company, string user1, string password)
		{
			TrackingSiteUser user = (TrackingSiteUser)AppInstance.GetNewSiteUser();
			user.Login(company, user1, password);

			// Need to set up WebEnv SiteUser to be able to generate DocumentLink
			if (user.IsLoggedIn)
			{
				AppInstance.Session[ZEnterpriseGlobalBase.SiteUserSessionKey] = user;
			}

			return (user.IsLoggedIn) ? user.LoggedInOrganisation : null;
		}

		#endregion
	}
}
