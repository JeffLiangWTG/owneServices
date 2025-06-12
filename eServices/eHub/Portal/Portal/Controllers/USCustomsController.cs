using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Portal.Controllers
{
	public class USCustomsController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("USCustomsLogger");
		protected List<string> listLogs = new List<string>();

		#region Index
		public ActionResult Index()
		{
			return View();
		}

		public JsonResult Registry()
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchFilter = Request["filters"];
			List<string> searchFormatList = null;
			string search = null;

			MultipleFilter filterQuery = new MultipleFilter();
			if (!string.IsNullOrEmpty(searchFilter))
			{
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchFilter);
			}

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
			{
				searchFormatList = GetMultipleValuesFilter(filterQuery);
			}

			var reg = (from c in Context.eHubClients
					   where (c.eHubUSCustomsRegistry.Count > 0)
					   select new 
					   {
						   CC_ID = c.CC_ID, 
						   CC_FriendlyName = c.CC_FriendlyName,
						   CC_USCustomsRecipient = c.CC_USCustomsRecipient,
						   USE = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "USE").Select(r => new RegistryItem { ER_Value = r.ER_Value, ER_IsProduction = r.ER_IsProduction }),
						   USI = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "USI" && r.ER_Name == "Entry Filer Code").Select(r => new RegistryItem { ER_Value = r.ER_Value, ER_IsProduction = r.ER_IsProduction }),
						   ISF = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "USI" && r.ER_Name == "ISF User Data").Select(r => new RegistryItem { ER_Value = r.ER_Value, ER_IsProduction = r.ER_IsProduction }),
						   AMS = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "AMS").Select(r => new RegistryItem { ER_Value = r.ER_Value, ER_IsProduction = r.ER_IsProduction }),
						   AMA = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "AMA").Select(r => new RegistryItem { ER_Value = r.ER_Value, ER_IsProduction = r.ER_IsProduction }),
						   MAN = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "MAN" && r.ER_Name == "Client Network ID").Select(r => new RegistryItem { ER_Value = r.ER_Value, ER_IsProduction = r.ER_IsProduction }),
						   UEM = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "UEM" && r.ER_Name == "Carrier Code").Select(r => new RegistryItem { ER_Value = r.ER_Value, ER_IsProduction = r.ER_IsProduction }),
						   USD = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "USD" && r.ER_Name == "Entry Filer Code").Select(r => new RegistryItem { ER_Value = r.ER_Value, ER_IsProduction = r.ER_IsProduction }).FirstOrDefault(),
						   USE_ER_VALUE = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "USE").Select(e => e.ER_Value).FirstOrDefault(),
						   USI_ER_VALUE = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "USI" && r.ER_Name == "Entry Filer Code").Select(e => e.ER_Value).FirstOrDefault(),
						   ISF_ER_VALUE = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "USI" && r.ER_Name == "ISF User Data").Select(e => e.ER_Value).FirstOrDefault(),
						   AMS_ER_VALUE = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "AMS").Select(e => e.ER_Value).FirstOrDefault(),
						   AMA_ER_VALUE = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "AMA").Select(e => e.ER_Value).FirstOrDefault(),
						   MAN_ER_VALUE = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "MAN" && r.ER_Name == "Client Network ID").Select(e => e.ER_Value).FirstOrDefault(),
						   UEM_ER_VALUE = c.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == "UEM" && r.ER_Name == "Carrier Code").Select(e => e.ER_Value).FirstOrDefault()
					   }).OrderBy(FilterFieldToString(sidx) + " " + sord);

			if (searchFormatList != null)
			{
				string queryOperator = "&&";
				if (searchFilter.Contains("OR")){
					queryOperator = "||";
				}

				int queryCount = 0;

				List<Tuple<string, string, string>> extractedValues = getFieldandString(searchFilter);
				Func<string, Tuple<string, string, string>, string> totalSearch = (currentSearch, searchValues) =>
				{
					StringBuilder resultBuilder = new StringBuilder(currentSearch);
					resultBuilder.Append(string.Format(searchFormatList[queryCount], FilterFieldToString(searchValues.Item1), searchValues.Item3));
					resultBuilder.Append($" {queryOperator} ");
					queryCount++;
					return resultBuilder.ToString();
				};

				search = extractedValues.Aggregate("", totalSearch);
				search = search.TrimEnd($" {queryOperator} ".ToCharArray());
				reg = reg.Where(search);
			}

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)reg.Count() / (double)rows),
				records = reg.Count(),
				eHubUSCustomsRegistry = reg.Skip((page - 1) * rows).Take(rows).ToList().Select(x => new USCustomsRegistryFormattedData
				{
					id = x.CC_ID,
					CC_ID = x.CC_ID,
					CC_FriendlyName = x.CC_FriendlyName,
					CC_USCustomsRecipient = x.CC_USCustomsRecipient,
					USE = string.Join(",", x.USE.Select(y => y.ER_Value)),
					USE_IsProduction = x.USE.Select(y => y.ER_IsProduction).FirstOrDefault(),
					USI = string.Join(",", x.USI.Select(y => y.ER_Value)),
					USI_IsProduction = x.USI.Select(y => y.ER_IsProduction).FirstOrDefault(),
					ISF = string.Join(",", x.ISF.Select(y => y.ER_Value)),
					ISF_IsProduction = x.ISF.Select(y => y.ER_IsProduction).FirstOrDefault(),
					AMS = string.Join(",", x.AMS.Select(y => y.ER_Value)),
					AMS_IsProduction = x.AMS.Select(y => y.ER_IsProduction).FirstOrDefault(),
					AMA = string.Join(",", x.AMA.Select(y => y.ER_Value)),
					AMA_IsProduction = x.AMA.Select(y => y.ER_IsProduction).FirstOrDefault(),
					MAN = string.Join(",", x.MAN.Select(y => y.ER_Value)),
					MAN_IsProduction = x.MAN.Select(y => y.ER_IsProduction).FirstOrDefault(),
					UEM = string.Join(",", x.UEM.Select(y => y.ER_Value)),
					UEM_IsProduction = x.UEM.Select(y => y.ER_IsProduction).FirstOrDefault(),
					USD = x.USD
				})
			}, JsonRequestBehavior.AllowGet);
		}

		private string FilterFieldToString(string field)
		{
			switch (field)
			{
				case "USE":
					return field + "_ER_VALUE";
				case "USI":
					return field + "_ER_VALUE";
				case "ISF":
					return field + "_ER_VALUE";
				case "AMS":
					return field + "_ER_VALUE";
				case "AMA":
					return field + "_ER_VALUE";
				case "MAN":
					return field + "_ER_VALUE";
				case "UEM":
					return field + "_ER_VALUE";
				default:
					return field;
			}
		}

		private List<Tuple<string,string,string>> getFieldandString(string searchFilter)
		{
			List<Tuple<string, string, string>> extractedValues = new List<Tuple<string, string, string>>();

			JObject jsonObject = JObject.Parse(searchFilter);

			foreach (var rule in jsonObject["rules"])
			{
				string field = rule["field"].ToString();
				string op = rule["op"].ToString();
				string data = rule["data"].ToString();
				extractedValues.Add(Tuple.Create(field, op, data));
			}

			return extractedValues;
		}


		private List<string> GetMultipleValuesFilter(MultipleFilter filterQuery)
		{
			List<string> vals = new List<string>();
			string val;
			foreach (var rule in filterQuery.rules)
			{
				val = ApplyValuesFilter(rule.op);
				vals.Add(val);
			}
			return vals;
		}

		static string ApplyValuesFilter(string searchOper)
		{
			string searchFormat;
			switch (searchOper)
			{
				case "eq":
					searchFormat = "{0} == \"{1}\"";
					break;
				case "bw":
					searchFormat = "{0}.StartsWith(\"{1}\")";
					break;
				case "ew":
					searchFormat = "{0}.EndsWith(\"{1}\")";
					break;
				case "cn":
					searchFormat = "{0}.Contains(\"{1}\")";
					break;
				default:
					searchFormat = null;
					break;
			}
			return searchFormat;
		}

		public virtual bool DoesClientHavePRDLicence(string eHubId)
			=> Context.ediProdClients.FirstOrDefault(c => c.CC_ID == eHubId)?.LD_LicenceType == "PRD";

		protected void USCustomseHubClientLog(string oper, eHubClient rego)
		{
			listLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubClient: CC_ID={rego.CC_ID}, CC_FriendlyName={rego.CC_FriendlyName}, CC_USCustomsRecipient={rego.CC_USCustomsRecipient}, CC_Odyssey_OH={rego.CC_Odyssey_OH}, CC_DistributionZone={rego.CC_DistributionZone}" +
				$", CC_Password={rego.CC_Password}, CC_IsAirServiceProvider={rego.CC_IsAirServiceProvider}, CC_AirlineCode={rego.CC_AirlineCode}, CC_AirServiceProvider={rego.CC_AirServiceProvider}, CC_AirlinePrefix={rego.CC_AirlinePrefix}" +
				$", CC_EmailAddress={rego.CC_EmailAddress}, CC_AS2_Code={rego.CC_AS2_Code}, CC_SCAC_Code={rego.CC_SCAC_Code}, CC_OwnerCategory={rego.CC_OwnerCategory}, CC_SystemCategory={rego.CC_SystemCategory}, CC_RR={rego.CC_RR}" +
				$", CC_RequireStatusResponse={rego.CC_RequireStatusResponse}, CC_NotificationForInboxRecipient={rego.CC_NotificationForInboxRecipient}");

			var groupedEntries = rego.eHubUSCustomsRegistry.GroupBy(r => new { r.ER_ApplicationCode, r.ER_Name });
			foreach (var registryGroup in groupedEntries)
			{
				var firstRegistry = registryGroup.FirstOrDefault();
				listLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubUSCustomsRegistry: ER_CC_Client={firstRegistry.ER_CC_Client}, ER_ApplicationCode={firstRegistry.ER_ApplicationCode}, ER_Name={firstRegistry.ER_Name}, ER_IsProduction={firstRegistry.ER_IsProduction}");

				var valueBuilder = new StringBuilder($"[{oper}] ER_Value(s)=");
				foreach (var item in registryGroup.Select((value, index) => new { value, index }))
				{
					if (item.index > 0)
					{
						valueBuilder.Append(",");
					}
					valueBuilder.Append($"{item.value.ER_Value}");
				}
				listLogs.Add(valueBuilder.ToString());
			}
		}

		protected void SaveLogs()
		{
			foreach (var listLog in listLogs)
			{
				logger.Info(listLog);
			}
			listLogs.Clear();
		}

		public JsonResult Clients(bool includeNonProd)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clients;
			if (includeNonProd)
			{
				clients = Context.eHubClients.Where(c => c.eHubUSCustomsRegistry.Count() == 0 && c.CC_OwnerCategory == "Client" && c.CC_SystemCategory == "Enterprise").Select(c => new
				{
					CC_ID = c.CC_ID,
					CC_FriendlyName = c.CC_FriendlyName
				});
			}
			else
			{
				clients = from c in Context.eHubClients
						  where c.eHubUSCustomsRegistry.Count() == 0 &&
								c.CC_OwnerCategory == "Client" &&
								c.CC_SystemCategory == "Enterprise"
						  join p in Context.ediProdClients on c.CC_PK equals p.CC_PK into joinedClients
						  from p in joinedClients.DefaultIfEmpty()
						  where p == null || p.LD_LicenceType == "PRD"
						  select new { c.CC_ID, c.CC_FriendlyName };

			}

			if (filtered)
			{
				string ccid = Request["CC_ID"];
				string ccname = Request["CC_FriendlyName"];
				if (!String.IsNullOrWhiteSpace(ccid))
					clients = clients.Where(c => c.CC_ID.StartsWith(ccid, StringComparison.OrdinalIgnoreCase));
				if (!String.IsNullOrWhiteSpace(ccname))
					clients = clients.Where(c => c.CC_FriendlyName.Contains(ccname));
			}

			int count = clients.Count();

			switch (sidx + " " + sord)
			{
				case "CC_ID asc":
					clients = clients.OrderBy(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
					break;
				case "CC_ID desc":
					clients = clients.OrderByDescending(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
					break;
				case "CC_FriendlyName asc":
					clients = clients.OrderBy(c => c.CC_FriendlyName).ThenBy(c => c.CC_ID);
					break;
				case "CC_FriendlyName desc":
					clients = clients.OrderByDescending(c => c.CC_FriendlyName).ThenBy(c => c.CC_ID);
					break;
				default:
					clients = clients.OrderBy(c => c.CC_ID).ThenBy(c => c.CC_FriendlyName);
					break;
			}

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubClients = clients.Skip((page - 1) * rows).Take(rows).ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult RegistryEdit()
		{
			try
			{
				var clientID = Request["CC_ID"] ?? Request["id"];
				var client = Context.eHubClients.Where(c => c.CC_ID == clientID).FirstOrDefault();
				var operValue = Request["oper"];
				switch (operValue)
				{
					case "add":
						UpdateEntries(client, bool.Parse(Request["USE_IsProduction"]));
						break;
					case "edit":
						listLogs.Add($"[{operValue}] eHubClient (Previous Values)");
						var previousID = Request["id"];
						if (previousID != clientID)
						{
							var previousClient = Context.eHubClients.Single(c => c.CC_ID == previousID);
							USCustomseHubClientLog(operValue, previousClient);
							previousClient.CC_USCustomsRecipient = null;
							previousClient.eHubUSCustomsRegistry.ToList().ForEach(r =>
							{
								Context.eHubUSCustomsRegistry.DeleteObject(r);
								AddUSCustomsLog(operValue, r);
							});
						}
						else
						{
							USCustomseHubClientLog(operValue, client);
						}
						UpdateEntries(client, DoesClientHavePRDLicence(clientID));
						listLogs.Add($"[{operValue}] eHubClient (New Values)");
						break;
					case "del":
						client.CC_USCustomsRecipient = null;
						client.eHubUSCustomsRegistry.ToList().ForEach(r =>
						{
							Context.eHubUSCustomsRegistry.DeleteObject(r);
							AddUSCustomsLog(operValue, r);
						});
						break;
					default:
						break;
				}
				Context.SaveChanges();
				USCustomseHubClientLog(operValue, client);
				SaveLogs();
				return Json(new { success = true, newid = client.CC_PK }, JsonRequestBehavior.AllowGet);
			}
			catch (ApplicationException ex)
			{
				return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
			}
		}

		void UpdateEntries(eHubClient client, bool isPrdLicence)
		{
			client.CC_USCustomsRecipient = bool.Parse(Request["CC_USCustomsRecipient"]);
			UpdateEntry(client, "USE", "USE", "Entry Filer Code", isPrdLicence);
			UpdateEntry(client, "USI", "USI", "Entry Filer Code", isPrdLicence);
			UpdateEntry(client, "ISF", "USI", "ISF User Data", isPrdLicence);
			UpdateEntry(client, "AMS", "AMS", "Entry Filer Code", isPrdLicence);
			UpdateEntry(client, "AMA", "AMA", "Participant Originator Code", isPrdLicence);
			UpdateEntry(client, "MAN", "MAN", "Client Network ID", isPrdLicence);
			UpdateEntry(client, "UEM", "UEM", "Carrier Code", isPrdLicence);
			UpdateEntryDIS(client, isPrdLicence);
			if (client.eHubUSCustomsRegistry.Count() == 0)
				throw new ApplicationException("At least one code must be specified.");
		}

		private void UpdateEntry(eHubClient client, string type, string code, string name, bool isPrdLicence)
		{
			var values = Request[type];
			var regs = client.eHubUSCustomsRegistry.Where(r => r.ER_ApplicationCode == code && r.ER_Name == name);

			if (string.IsNullOrWhiteSpace(values))
			{
				foreach (var reg in regs.ToList())
				{
					Context.eHubUSCustomsRegistry.DeleteObject(reg);
				}
			}
			else
			{
				var valuesList = values.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
				foreach (var reg in regs.ToList())
				{
					if (valuesList.Contains(reg.ER_Value))
					{
						valuesList.Remove(reg.ER_Value);
						reg.ER_IsProduction = isPrdLicence;
					}
					else
					{
						Context.eHubUSCustomsRegistry.DeleteObject(reg);
					}
				}
				foreach (var value in valuesList)
				{
					var reg = new eHubUSCustomsRegistry()
					{
						ER_PK = Guid.NewGuid(),
						ER_ApplicationCode = code,
						ER_Name = name,
						ER_Value = value,
						ER_IsProduction = isPrdLicence
					};
					client.eHubUSCustomsRegistry.Add(reg);
				}
			}
		}

		private void UpdateEntryDIS(eHubClient client, bool isProd)
		{
			var code = "USD";
			var name = "Entry Filer Code";
			var isDISAllowed = !string.IsNullOrEmpty(Request["USE"]) || !string.IsNullOrEmpty(Request["USI"]) || !string.IsNullOrEmpty(Request["ISF"]);
			var reg = client.eHubUSCustomsRegistry.FirstOrDefault(r => r.ER_ApplicationCode == code && r.ER_Name == name);
			if (reg != null)
			{
				if (isDISAllowed)
				{
					reg.ER_Value = string.Empty;
					reg.ER_IsProduction = isProd;
				}
				else
				{
					Context.eHubUSCustomsRegistry.DeleteObject(reg);
				}
			}
			else if (isDISAllowed)
			{
				reg = new eHubUSCustomsRegistry()
				{
					ER_PK = Guid.NewGuid(),
					ER_ApplicationCode = code,
					ER_Name = name,
					ER_Value = string.Empty,
					ER_IsProduction = isProd
				};
				client.eHubUSCustomsRegistry.Add(reg);
			}
		}

		protected void AddUSCustomsLog(string oper, eHubUSCustomsRegistry reg)
		{
			listLogs.Add($"[{HttpContext.User.Identity.Name}] [{oper}] eHubUSCustomsRegistry: ER_CC_Client={reg.ER_CC_Client}, ER_ApplicationCode={reg.ER_ApplicationCode}, ER_Name={reg.ER_Name}, ER_Value={reg.ER_Value}, ER_IsProduction={reg.ER_IsProduction}");
		}

		#endregion
	}
}
