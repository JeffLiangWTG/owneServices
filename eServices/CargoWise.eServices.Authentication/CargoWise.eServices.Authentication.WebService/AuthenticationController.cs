using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using log4net;
using log4net.Repository.Hierarchy;

namespace CargoWise.eServices.Authentication.WebService
{
	[CacheResponse]
	public class AuthenticationController : ApiController
	{
	   [HttpPost]
		public HttpResponseMessage ValidateSystemIDAndPassword([FromBody]string[] param)
		{
            Logger.Debug($"[SystemID: {param[0]}] [ValidateSystemIDAndPassword: Start with request from IP Address/es: {GetIPAddresses()}]");
			if (DatabaseHelper.ValidateSystemIDAndPassword(param[0], param[1]))
			{
				return new HttpResponseMessage(HttpStatusCode.OK);
			}
			return new HttpResponseMessage(HttpStatusCode.Unauthorized)
			{
				ReasonPhrase = "Validation failed"
			};
		}

		[HttpPost]
		public HttpResponseMessage ValidateCodeAndPassword([FromBody]string[] param)
		{
		    Logger.Debug($"[Code: {param[0]}-{param[1]}] [ValidateCodeAndPassword: Start with request from IP Address/es: {GetIPAddresses()}]");
            if (DatabaseHelper.ValidateCodeAndPassword(param[0], param[1], param[2]))
			{
				return new HttpResponseMessage(HttpStatusCode.OK);
			}
			return new HttpResponseMessage(HttpStatusCode.Unauthorized)
			{
				ReasonPhrase = "Validation failed"
			};
		}

		[HttpPost]
		public HttpResponseMessage CheckSystemIDExistence([FromBody]string systemID)
		{
		    Logger.Debug($"[SystemID: {systemID}] [CheckSystemIDExistence: Start with request from IP Address/es: {GetIPAddresses()}]");
            if (DatabaseHelper.CheckSystemIDExistence(systemID))
			{
				return new HttpResponseMessage(HttpStatusCode.Found);
			}
			return new HttpResponseMessage(HttpStatusCode.Unauthorized)
			{
				ReasonPhrase = "Could not find the system."
			};
		}

		[HttpPost]
		public HttpResponseMessage CheckCodeExistence([FromBody]string[] param)
		{
		    Logger.Debug($"[Code: {param[0]}-{param[1]}] [CheckCodeExistence: Start with request from IP Address/es: {GetIPAddresses()}]");
            if (DatabaseHelper.CheckCodeExistence(param[0], param[1]))
			{
				return new HttpResponseMessage(HttpStatusCode.Found);
			}
			return new HttpResponseMessage(HttpStatusCode.Unauthorized)
			{
				ReasonPhrase = "Could not find the system."
			};
		}

        [HttpGet]
        [SkipCacheResponse]
        public bool Ping()
        {
            return true;
        }

        public virtual IDatabaseHelper DatabaseHelper => databaseHelper ?? (databaseHelper = new DatabaseHelper());

	    DatabaseHelper databaseHelper;
	    static readonly ILog Logger = LogManager.GetLogger(typeof(AuthenticationController));

        static string GetIPAddresses()
	    {
	        var httpContext = HttpContext.Current;

            if (httpContext != null)
	        {
	            string ipAddresses = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
	            if (!string.IsNullOrEmpty(ipAddresses) && !ipAddresses.Equals("unknown", StringComparison.OrdinalIgnoreCase))
	            {
	                return ipAddresses;
	            }

	            return httpContext.Request.ServerVariables["REMOTE_ADDR"];
	        }

	        return "<UNKNOWN>";
	    }
    }
}