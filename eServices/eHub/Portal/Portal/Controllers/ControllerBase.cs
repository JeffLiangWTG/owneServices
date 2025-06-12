using System;
using System.Web;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Controllers
{
	[CustomAuthorize]
	public class ControllerBase : Controller
	{
		IeHubTransactionsContext context;
		IeHubTransactionsContext readOnlyContext;

		public ControllerBase()
		{
		}

		public IeHubTransactionsContext Context
		{
			get
			{
				return context ?? (context = new eHubTransactionsEntities());
			}
			set
			{
				context = value;
			}
		}

		public IeHubTransactionsContext ReadOnlyContext
		{
			get
			{
				return readOnlyContext ?? (readOnlyContext = new eHubTransactionsEntities("name=eHubTransactionsReadOnlyEntities"));
			}
			set
			{
				readOnlyContext = value;
			}
		}

		public static ActionResult ModalRedirect(ActionResult result)
		{
			if (result is ViewResult)
			{
				var output = (ViewResult)result;
				output.ViewName = output.ViewName + "Modal";
				return output;
			}

			if (result is RedirectToRouteResult)
			{
				var output = (RedirectToRouteResult)result;
				output.RouteValues["action"] = output.RouteValues["action"] + "Modal";
				return output;
			}

			return result;
		}

		public void ValidateModel<TModel>(TModel model) where TModel : class
		{
			this.TryUpdateModel<TModel>(model);
		}

		protected override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
			filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
			filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
			filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			filterContext.HttpContext.Response.Cache.SetNoStore();
			base.OnResultExecuted(filterContext);
		}

		protected void LogError(string message)
		{
			//EventLog.WriteEntry("eHub Portal Web Site", message, EventLogEntryType.Error);
		}
	}
}
