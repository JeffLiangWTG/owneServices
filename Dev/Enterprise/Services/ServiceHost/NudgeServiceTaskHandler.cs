using System;
using System.Threading.Tasks;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Services.ServiceHost
{
	public class NudgeServiceTaskHandler : IHttpHandler
	{
		/// <summary>
		/// You will need to configure this handler in the Web.config file of your 
		/// web and register it with IIS before being able to use it. For more information
		/// see the following link: http://go.microsoft.com/?linkid=8101007
		/// </summary>
		#region IHttpHandler Members

		public bool IsReusable
		{
			// Return false in case your Managed Handler cannot be reused for another request.
			// Usually this would be false in case you have some state information preserved per request.
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var request = context.Request;
			var response = context.Response;
			var serviceTaskCode = request.Params["code"];
			var key = request.Params["key"];
			int statusCode;
			string statusDesc;

			if (ValidateNudgeRequest(serviceTaskCode, key, out statusCode, out statusDesc))
			{
				Task.Run(() => ProcessNudgeRequest(serviceTaskCode));
			}
			response.StatusCode = statusCode;
			response.StatusDescription = statusDesc;
		}
		#endregion

		static bool ValidateNudgeRequest(string serviceTaskCode, string key, out int statusCode, out string statusDesc)
		{
			if (string.IsNullOrEmpty(serviceTaskCode) || string.IsNullOrEmpty(key))
			{
				statusCode = 400;
				statusDesc = (NoResString)"Invalid parameter(s)"; // string constant
				return false;
			}
			else if (!Constants.AuthKey.Equals(key))
			{
				statusCode = 500;
				statusDesc = (NoResString)"Incorrect key"; // string constant
				return false;
			}
			else
			{
				statusCode = 200;
				statusDesc = "";
				return true;
			}
		}

		internal static void ProcessNudgeRequest(string serviceTaskCode)
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(serviceTaskCode);
				}
				catch (Exception exception)
				{
					ErrorReporter.ReportOnce($"Error during nudging in {typeof(NudgeServiceTaskHandler).FullName}", exception);
				}
			}
		}
	}
}
