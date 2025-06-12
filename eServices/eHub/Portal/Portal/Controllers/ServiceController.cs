using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.Extensions;
using CargoWise.eHub.Portal.Models.View;
using Common.Logging;

namespace CargoWise.eHub.Portal.Controllers
{
	public class ServiceController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("AirlineMessagingLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index(Guid? id)
		{
			var serviceView = new ServiceView();

			if (id != null && id.HasValue)
			{
				serviceView.Client = Context.GetClient(id.Value);
				ViewBag.PageTitle = serviceView.Client.CC_FriendlyName;
				serviceView.AirServiceActive = IsAirServiceActive(serviceView.Client);
			}

			return View(serviceView);
		}

		public ActionResult Details(Guid id)
		{
			var serviceView = GetServiceView(id);
			return View("Details", serviceView);
		}

		public ActionResult Delete(Guid id)
		{
			var serviceView = GetServiceView(id);
			if (!serviceView.AirServiceActive) return View("NotFound");
			return View("Delete", serviceView);
		}

		protected void AddAirConnectionPerBranchLog(string oper, eHubAirConnectionPerBranch airConnectionPerBranch)
		{
			tempLogs.Add($"[{oper}] eHubAirConnectionPerBranch: AB_CC_Client={airConnectionPerBranch.AB_CC_Client}, AB_CC_AirServiceProvider={airConnectionPerBranch.AB_CC_AirServiceProvider}, AB_IssuingCarrierAgentIATACode={airConnectionPerBranch.AB_IssuingCarrierAgentIATACode}" +
			$", AB_PIMA={airConnectionPerBranch.AB_PIMA}, AB_PASSWORD={airConnectionPerBranch.AB_PASSWORD}");
		}

		protected void AddAirConnectionLog(string oper, eHubAirConnection airConnection)
		{
			tempLogs.Add($"[{oper}] eHubAirConnection: AC_CC_Client={airConnection.AC_CC_Client}, AC_CC_AirServiceProvider={airConnection.AC_CC_AirServiceProvider}, AC_PIMA={airConnection.AC_PIMA}, AC_PASSWORD={airConnection.AC_PASSWORD}");
		}

		protected void AddAirDefaultServiceProviderLog(string oper, eHubAirDefaultServiceProvider routing)
		{
			tempLogs.Add($"[{oper}] eHubAirDefaultServiceProvider: AD_CC_Client={routing.AD_CC_Client}, AD_DT_MessageType={routing.AD_DT_MessageType}, AD_CC_AirServiceProvider={routing.AD_CC_AirServiceProvider}");
		}

		protected void AddAirServiceProviderMappingLog(string oper, eHubAirServiceProviderMapping airlineMapping)
		{
			tempLogs.Add($"[{oper}] eHubAirServiceProviderMapping: AM_CC_Client={airlineMapping.AM_CC_Client}, AM_CC_Airline={airlineMapping.AM_CC_Airline}, AM_DT_MessageType={airlineMapping.AM_DT_MessageType}, AM_CC_AirServiceProvider={airlineMapping.AM_CC_AirServiceProvider}");
		}

		protected void AddClientLog(string oper, eHubClient client)
		{
			tempLogs.Add($"[{oper}] eHubClient: CC_PK={client.CC_PK}, CC_ID={client.CC_ID}, CC_FriendlyName={client.CC_FriendlyName}, CC_Odyssey_OH={client.CC_Odyssey_OH}, CC_DistributionZone={client.CC_DistributionZone}" +
			$", CC_EmailAddress={client.CC_EmailAddress}, CC_Password={client.CC_Password}, CC_IsAirServiceProvider={client.CC_IsAirServiceProvider}, CC_AirlineCode={client.CC_AirlineCode}, CC_AirServiceProvider={client.CC_AirServiceProvider}" +
			$", CC_AirlinePrefix={client.CC_AirlinePrefix}, CC_USCustomsRecipient={client.CC_USCustomsRecipient}, CC_AS2_Code={client.CC_AS2_Code}, CC_SCAC_Code={client.CC_SCAC_Code}, CC_OwnerCategory={client.CC_OwnerCategory}" +
			$", CC_SystemCategory={client.CC_SystemCategory}, CC_RR={client.CC_RR}, CC_RequireStatusResponse={client.CC_RequireStatusResponse}, CC_NotificationForInboxRecipient={client.CC_NotificationForInboxRecipient}");
		}

		protected void SaveLogs()
		{
			foreach (var tempLog in tempLogs)
			{
				logger.Info(() => tempLog);
			}
			tempLogs.Clear();
		}

		[HttpPost]
		[ActionName("Delete")]
		public ActionResult DeletePost(Guid id)
		{
			var client = Context.GetClient(id);
			if (client == null) return View("NotFound");

			ViewBag.PageTitle = client.CC_FriendlyName;

			foreach (var o in client.eHubAirConnections.SelectMany(o => o.eHubAirConnectionPerBranches).ToList())
			{
				Context.eHubAirConnectionPerBranches.DeleteObject(o);
				AddAirConnectionPerBranchLog("del", o);
			}
			foreach (var o in client.eHubAirConnections.ToList())
			{
				Context.eHubAirConnections.DeleteObject(o);
				AddAirConnectionLog("del", o);
			}
			client.CC_AirServiceProvider = null;
			AddClientLog("edit", client);
			foreach (var o in client.eHubAirDefaultServiceProviders.ToList())
			{
				Context.eHubAirDefaultServiceProviders.DeleteObject(o);
				AddAirDefaultServiceProviderLog("del", o);
			}
			foreach (var o in client.eHubAirServiceProviderMappings.ToList())
			{
				Context.eHubAirServiceProviderMappings.DeleteObject(o);
				AddAirServiceProviderMappingLog("del", o);
			}

			try
			{
				Context.SaveChanges();
				SaveLogs();
				return View("Deleted", Context.GetClient(id));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", ex.InnerException.Message);
				var serviceView = GetServiceView(id);
				return View("Delete", serviceView);
			}
		}

		ServiceView GetServiceView(Guid clientId)
		{
			var serviceView = new ServiceView();
			serviceView.Client = Context.GetClient(clientId);
			ViewBag.PageTitle = serviceView.Client.CC_FriendlyName;
			serviceView.AirServiceActive = IsAirServiceActive(serviceView.Client);
			return serviceView;
		}

		private static bool IsAirServiceActive(eHubClient client)
		{
			bool result = false;
			result = client.CC_AirServiceProvider.HasValue;
			result = result || client.eHubAirConnections.Any();
			result = result || client.eHubAirDefaultServiceProviders.Any();
			result = result || client.eHubAirServiceProviderMappings.Any();
			return result;
		}
	}
}

