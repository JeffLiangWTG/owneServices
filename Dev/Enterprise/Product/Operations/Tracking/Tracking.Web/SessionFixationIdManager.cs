using System;
using System.Web;
using System.Web.SessionState;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.GUI.Login;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Tracking.Web
{
	[CodeAlive("This class is a custom sessionIDManagerType referenced in the web.config sessionState")]
	public class SessionFixationIdManager : ISessionIDManager
	{
		public SessionFixationIdManager() : this(new SessionIDManager()) { }

		public SessionFixationIdManager(ISessionIDManager manager)
		{
			idManager = manager ?? throw new ArgumentNullException(nameof(manager));
		}

		readonly ISessionIDManager idManager;

		public string CreateSessionID(HttpContext context)
		{
			return idManager.CreateSessionID(context);
		}

		public string GetSessionID(HttpContext context)
		{
			if (context.Request.IsAuthenticated)
			{
				return idManager.GetSessionID(context);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var token = LoginRouter.GetIdentityTokenFromRequest(context.Request);
				var isAuthenticating = ((ITokenizedAccessControl)new TokenizedAccessControl()).TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _);
				return isAuthenticating ? idManager.GetSessionID(context) : null;
			}
		}

		public void Initialize()
		{
			idManager.Initialize();
		}

		public bool InitializeRequest(HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue)
		{
			return idManager.InitializeRequest(context, suppressAutoDetectRedirect, out supportSessionIDReissue);
		}

		public void RemoveSessionID(HttpContext context)
		{
			idManager.RemoveSessionID(context);
		}

		public void SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded)
		{
			idManager.SaveSessionID(context, id, out redirected, out cookieAdded);
		}

		public bool Validate(string id)
		{
			return idManager.Validate(id);
		}
	}
}
