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
	public class ClientPIMAController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("AirlineMessagingLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index(Guid id, string env = "prod")
		{
			var clientPIMAView = GetClientPIMAViewList(id);
			clientPIMAView.Env = env;
			return View("Index", clientPIMAView);
		}

		public ActionResult Edit(Guid id, string env = "prod")
		{
			var clientPIMAView = GetClientPIMAViewList(id, true);
			clientPIMAView.Env = env;
			return View("Edit", clientPIMAView);
		}

		[HttpPost]
		public ActionResult Edit(Guid id, FormCollection formValues)
		{
			return View("Edit", GetClientPIMAViewList(id, true));
		}

		protected void AddAirConnectionPerBranchLog(string oper, eHubAirConnectionPerBranch airConnectionPerBranch)
		{
			tempLogs.Add($"[{oper}] eHubAirConnectionPerBranch: AB_CC_Client={airConnectionPerBranch.AB_CC_Client}, AB_CC_AirServiceProvider={airConnectionPerBranch.AB_CC_AirServiceProvider}, AB_IssuingCarrierAgentIATACode={airConnectionPerBranch.AB_IssuingCarrierAgentIATACode}" +
			$", AB_PIMA={airConnectionPerBranch.AB_PIMA}, AB_PASSWORD={airConnectionPerBranch.AB_PASSWORD}");
		}

		protected void AddAirConnectionLog(string oper, eHubAirConnection airConnection)
		{
			tempLogs.Add($"[{oper}] eHubAirConnection: AB_CC_Client={airConnection.AC_CC_Client}, AB_CC_AirServiceProvider={airConnection.AC_CC_AirServiceProvider}, AC_PIMA={airConnection.AC_PIMA}, AB_PASSWORD={airConnection.AC_PASSWORD}");
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
		public ActionResult Update(Guid id, Dictionary<string, List<ServiceProviderPIMAView>> data)
		{
			Dictionary<string, object> result = new Dictionary<string, object>();
			try
			{
				data = data ?? new Dictionary<string, List<ServiceProviderPIMAView>>();
				var airConnectionList = Context.GetAirConnectionQuery(id).ToList();
				var airConnectionsPerBranchList = Context.GetAirConnectionPerBranchQuery(id, airConnectionList).ToList();
				foreach (KeyValuePair<string, List<ServiceProviderPIMAView>> entry in data)
				{
					Guid serviceProviderId;
					if (Guid.TryParse(entry.Key, out serviceProviderId))
					{
						foreach (ServiceProviderPIMAView spPimaView in entry.Value)
						{
							if (spPimaView.IATA.Equals("Default", StringComparison.InvariantCultureIgnoreCase))
							{
								var airConnection = (from ac in airConnectionList
													 where ac.AC_CC_AirServiceProvider == serviceProviderId
													 select ac).FirstOrDefault();

								if (string.IsNullOrEmpty(spPimaView.PIMA))
								{
									if (airConnection != null)
									{
										Context.eHubAirConnections.DeleteObject(airConnection);
										AddAirConnectionLog("del", airConnection);
									}
								}
								else
								{
									CreateOrEditAirConnection(id, serviceProviderId, airConnection, spPimaView, airConnectionList);
								}
							}
							else
							{
								var airConnectionPerBranch = (from acpb in airConnectionsPerBranchList
															  where acpb.AB_CC_AirServiceProvider == serviceProviderId &&
																	acpb.AB_IssuingCarrierAgentIATACode == spPimaView.IATA
															  select acpb).FirstOrDefault();

								CreateOrEditAirConnectionPerBranch(id, serviceProviderId, airConnectionPerBranch, spPimaView.IATA, spPimaView, airConnectionsPerBranchList);
							}
						}

					}
					else
					{
						continue;
					}
				}
				CleanUpAirConnections(airConnectionList, airConnectionsPerBranchList);
				Context.SaveChanges();
				SaveLogs();
				result.Add("success", true);
			}
			catch (Exception e)
			{
				result.Add("success", false);
				result.Add("message", e.StackTrace.ToString());
			}
			return Json(result);
		}

		#region Implementation

		void CreateOrEditAirConnection(Guid clientId, Guid spId, eHubAirConnection airConnection, ServiceProviderPIMAView pimaView, List<eHubAirConnection> airConnectionList)
		{
			if (airConnection == null)
			{
				// Add new eHubAirConnection
				airConnection = new eHubAirConnection();
				airConnection.AC_CC_AirServiceProvider = spId;
				airConnection.AC_CC_Client = clientId;
				airConnection.AC_PASSWORD = pimaView.Password;
				airConnection.AC_PIMA = pimaView.PIMA;
				Context.eHubAirConnections.AddObject(airConnection);
				AddAirConnectionLog("add", airConnection);
			}
			else
			{
				// Edit eHubAirConnection
				airConnection.AC_PASSWORD = pimaView.Password;
				airConnection.AC_PIMA = pimaView.PIMA;
				AddAirConnectionLog("edit", airConnection);
				airConnectionList.Remove(airConnection);
			}
		}

		void CreateOrEditAirConnectionPerBranch(Guid clientId, Guid spId, eHubAirConnectionPerBranch airConnectionPerBranch, string IATA, ServiceProviderPIMAView pimaView, List<eHubAirConnectionPerBranch> airConnectionsPerBranchList)
		{

			if (airConnectionPerBranch == null)
			{
				// Add new eHubAirConnectionPerBranch
				airConnectionPerBranch = new eHubAirConnectionPerBranch();
				airConnectionPerBranch.AB_CC_AirServiceProvider = spId;
				airConnectionPerBranch.AB_CC_Client = clientId;
				airConnectionPerBranch.AB_IssuingCarrierAgentIATACode = pimaView.IATA;
				airConnectionPerBranch.AB_PASSWORD = pimaView.Password;
				airConnectionPerBranch.AB_PIMA = pimaView.PIMA;
				Context.eHubAirConnectionPerBranches.AddObject(airConnectionPerBranch);
				AddAirConnectionPerBranchLog("add", airConnectionPerBranch);
			}
			else
			{
				// Edit eHubAirConnectionPerBranch
				airConnectionPerBranch.AB_PASSWORD = pimaView.Password;
				airConnectionPerBranch.AB_PIMA = pimaView.PIMA;
				AddAirConnectionPerBranchLog("edit", airConnectionPerBranch);
				airConnectionsPerBranchList.Remove(airConnectionPerBranch);
			}
		}

		private void CleanUpAirConnections(List<eHubAirConnection> airConnectionList, List<eHubAirConnectionPerBranch> airConnectionsPerBranchList)
		{

			foreach (var ac in airConnectionList)
			{
				Context.eHubAirConnections.DeleteObject(ac);
				AddAirConnectionLog("del", ac);
			}

			foreach (var acpb in airConnectionsPerBranchList)
			{
				Context.eHubAirConnectionPerBranches.DeleteObject(acpb);
				AddAirConnectionPerBranchLog("del", acpb);
			}
		}

		private void AddServiceProvider(ClientPIMAView clientPimaView, ServiceProviderPIMAView serviceProviderPimaView, bool isTest)
		{
			if (isTest)
			{
				clientPimaView.ServiceProviderPIMAViewTest.Add(serviceProviderPimaView);
			}
			else
			{
				clientPimaView.ServiceProviderPIMAView.Add(serviceProviderPimaView);
			}
		}

		ClientPIMAView GetClientPIMAViewList(Guid clientId, bool IncludeEmptyProvider = false)
		{
			var servicePoviderList = Context.GetAllAirServiceProvider().Where(sp => !sp.CC_ID.ToUpper().Contains("DISABLED")).OrderBy(sp => sp.CC_ID);
			var airConnectionList = Context.GetAirConnectionQuery(clientId).ToList();

			var clientPIMAView = new ClientPIMAView();
			clientPIMAView.Client = Context.GetClient(clientId);
			clientPIMAView.ServiceProviderPIMAView = new List<ServiceProviderPIMAView>();
			clientPIMAView.ServiceProviderPIMAViewTest = new List<ServiceProviderPIMAView>();

			foreach (var serviceProvider in servicePoviderList)
			{
				var serviceProviderPIMAView = new ServiceProviderPIMAView();
				var isTest = serviceProvider.CC_ID.ToUpper().Contains("TEST");

				serviceProviderPIMAView.ServiceProviderId = serviceProvider.CC_PK;
				serviceProviderPIMAView.ServiceProviderName = serviceProvider.CC_FriendlyName;

				var airConnection = FindAirConnection(airConnectionList, serviceProvider.CC_PK);
				serviceProviderPIMAView.IATA = "Default";
				serviceProviderPIMAView.IsDefault = true;
				serviceProviderPIMAView.IATAHtmlAttributes = new { @readonly = "readonly" };
				serviceProviderPIMAView.Button = "<div class=\"addButton\" id=\"" + serviceProvider.CC_PK + "\"></div>";

				if (airConnection != null)
				{
					serviceProviderPIMAView.PIMA = airConnection.AC_PIMA;
					serviceProviderPIMAView.Password = airConnection.AC_PASSWORD;
					AddServiceProvider(clientPIMAView, serviceProviderPIMAView, isTest);
				}
				else
				{
					if (IncludeEmptyProvider)
					{
						AddServiceProvider(clientPIMAView, serviceProviderPIMAView, isTest);
					}
				}

				// Get Data From AirConnectionPerBranch
				var airConnectionsPerBranch = Context.GetAirConnectionPerBranchQuery(clientId, serviceProvider.CC_PK);
				foreach (var acPerBranch in airConnectionsPerBranch)
				{
					var pimaView = new ServiceProviderPIMAView();
					pimaView.ServiceProviderId = serviceProvider.CC_PK;
					pimaView.ServiceProviderName = serviceProvider.CC_FriendlyName;
					pimaView.IATA = acPerBranch.AB_IssuingCarrierAgentIATACode;
					pimaView.PIMA = acPerBranch.AB_PIMA;
					pimaView.Password = acPerBranch.AB_PASSWORD;
					pimaView.Button = "<div class=\"deleteButton\" id=\"" + serviceProvider.CC_PK + "\"></div>";
					pimaView.IsDefault = false;

					AddServiceProvider(clientPIMAView, pimaView, isTest);
				}
			}

			return clientPIMAView;
		}

		eHubAirConnection FindAirConnection(List<Models.eHubTransactions.eHubAirConnection> airConnectionList, Guid serviceProviderId)
		{
			return (from ac in airConnectionList where ac.AC_CC_AirServiceProvider == serviceProviderId select ac).FirstOrDefault();
		}

		#endregion
	}
}
