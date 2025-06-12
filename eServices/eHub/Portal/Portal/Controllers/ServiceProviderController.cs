using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Portal.Controllers
{
	public class ServiceProviderController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("ServiceProviderLogger");

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult CW1Clients()
		{
			return Json(GetCW1Clients(), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult ServiceProviderEdit()
		{
			try
			{
				var id = ValuesEditCore();
				return Json(new { success = true, id = id }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.InnerException != null ? ex.InnerException.Message : ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		public JsonResult GetColumnNames()
		{
			var services = ServiceList.Select(s => s.serviceID).ToList();
			services.Insert(0, "Provider");
			var jsonObject = new
			{
				names = services
			};
			return Json(jsonObject, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ContentResult GetProvidersWithServices(string providerFromUrl)
		{
			int page = Convert.ToInt32(Request["page"] ?? "1");
			int rows = Convert.ToInt32(Request["rows"] ?? "20");

			var services = ServiceList;
			var combined = new Dictionary<string, object>();
			var providers = ProviderList.GroupBy(p => p.providerID,
																		p => p.servicePK,
																		(key, g) => new { providerID = key, servicePKs = g.ToList() });

			foreach (var provider in providers)
			{
				dynamic obj = combined.ContainsKey(provider.providerID) ? combined[provider.providerID] : new ExpandoObject();
				AddProperty(obj, "Provider", provider.providerID);
				foreach (var service in services)
				{
					if (provider.servicePKs.Contains(service.servicePK))
					{
						AddProperty(obj, service.serviceID, 1);
					}
					else
					{
						AddProperty(obj, service.serviceID, 0);
					}
				}
				combined[provider.providerID] = obj;
			}

			var list = combined.Skip((page - 1) * rows).Take(rows).ToList();

			var count = combined.Count();
			var jsonObject = new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				Services = FilterValues(list.Select(c => c.Value).ToList(), providerFromUrl).ToList()
			};
			return Content(JsonConvert.SerializeObject(jsonObject), "application/json");
		}

		private object ValuesEditCore()
		{
			var ID = "";
			switch (Request["oper"])
			{
				case "add":
					AddValues(out ID);
					break;
				case "edit":
					EditValues(out ID);
					break;
				case "del":
					DeleteValues(Guid.Empty, out ID);
					break;
				default:
					break;
			}
			return ID;
		}

		private void AddValues(out string ID)
		{
			EditValues(out ID);
		}

		protected void AddServiceProviderLog(string oper, eHubServiceProvider service)
		{
			logger.Info(() => $"[{oper}] eHubServiceProvider: SP_PK={service.SP_PK}, SP_CC_Service={service.SP_CC_Service}, SP_CC_Provider={service.SP_CC_Provider}, SP_RR={service.SP_RR}");
		}

		private void DeleteValues(Guid servicePK, out string ID)
		{
			var providerID = Request["id"];
			var provider = Context.eHubClients.Where(p => p.CC_ID == providerID).FirstOrDefault();

			var providerPK = provider.CC_PK;
			ID = providerID;
			var services = new List<eHubServiceProvider>();
			if (servicePK != Guid.Empty)
			{
				services = Context.eHubServiceProviders.Where(s => s.SP_CC_Provider == providerPK && s.SP_CC_Service == servicePK).ToList();
			}
			else
			{
				services = Context.eHubServiceProviders.Where(s => s.SP_CC_Provider == providerPK).ToList();
			}
			foreach (var service in services)
			{
				if (service.SP_RR != null)
				{
					throw new Exception("Cannot delete a service which is currently used in a routing rule");
				}
				Context.eHubServiceProviders.DeleteObject(service);
			}
			Context.SaveChanges();
			foreach (var service in services)
			{
				AddServiceProviderLog("del", service);
			}
		}

		private void EditValues(out string ID)
		{
			var providerID = Request["Provider"];
			if (String.IsNullOrEmpty(providerID))
			{
				throw new Exception("Please select a client.");
			}
			var provider = Context.eHubClients.Where(p => p.CC_ID == providerID).FirstOrDefault();

			var providerPK = provider.CC_PK;
			ID = providerID;
			var servicesForProvider = GetServiceListForProvider(providerPK);
			foreach (var service in ServiceList)
			{
				object serviceForProvider = null;
				if (servicesForProvider != null)
				{
					serviceForProvider = servicesForProvider.Where(s => s.CC_ID == service.serviceID).FirstOrDefault();
				}

				if (Request[service.serviceID].ToLower() == "on" && serviceForProvider == null)
				{
					AddValue(providerPK, service.servicePK);
				}
				else if (Request[service.serviceID].ToLower() == "off" && serviceForProvider != null)
				{
					DeleteValues(service.servicePK, out ID);
				}
			}
		}

		private void AddValue(Guid providerPK, Guid servicePK)
		{
			var newProvidedService = new eHubServiceProvider();
			newProvidedService.SP_PK = Guid.NewGuid();
			newProvidedService.SP_CC_Provider = providerPK;
			newProvidedService.SP_CC_Service = servicePK;

			Context.eHubServiceProviders.AddObject(newProvidedService);
			Context.SaveChanges();
			AddServiceProviderLog("add", newProvidedService);
		}

		List<dynamic> FilterValues(IEnumerable<dynamic> vals, string providerFromUrl)
		{
			string searchQuery = Request["filters"];	
			MultipleFilter filterQuery = new MultipleFilter();
			if (!string.IsNullOrEmpty(searchQuery))
			{
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);
			}
			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
			{
				vals = ApplyMultipleSearchValuesFilter(filterQuery, vals);
			}
			string provider = Request["Provider"];
			if (!string.IsNullOrWhiteSpace(provider))
				vals = vals.Where(c => c.Provider.ToUpper().Contains(provider.ToUpper()));
			if (!string.IsNullOrWhiteSpace(providerFromUrl))
			{
				vals = vals.Where(c => c.Provider.ToUpper().Contains(providerFromUrl.ToUpper()));
				if (vals.Count() == 0)
				{
					dynamic obj = new ExpandoObject();
					AddProperty(obj, "Provider", providerFromUrl);
					foreach (var service in ServiceList)
					{
						AddProperty(obj, service.serviceID, 0);
					}
					var list = new List<ExpandoObject>();
					list.Add(obj);
					vals = list;
				}
			}
			foreach (var service in ServiceList)
			{
				string status = Request[service.serviceID];
				if (!string.IsNullOrWhiteSpace(status))
				{
					vals = vals.Where(c => GetProperty(c, service.serviceID).Equals(status)).ToArray();
				}
			}

			string sortField = Request["sidx"];
			string sortOrder = Request["sord"];
			if (!string.IsNullOrEmpty(sortField) && !string.IsNullOrEmpty(sortOrder))
			{
				vals = ApplySortOrderFilter(sortField, sortOrder, vals);
			}

			return vals.ToList<dynamic>();
		}

		private List<dynamic> ApplyMultipleSearchValuesFilter(MultipleFilter filterQuery, IEnumerable<dynamic> vals)
		{
			var resultVars = vals.AsQueryable();
			switch (filterQuery.groupOp)
			{
				case "AND":
					foreach (var rule in filterQuery.rules)
					{
						resultVars = SearchValues(rule.field, rule.data, rule.op, resultVars).AsQueryable();
					}
					break;
				case "OR":
					resultVars = SearchValues(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, resultVars).AsQueryable();
					for (int rule = 1; rule < filterQuery.rules.Count; rule++)
					{
						var filteredData = SearchValues(filterQuery.rules[rule].field, filterQuery.rules[rule].data, filterQuery.rules[rule].op, resultVars).AsQueryable();
						resultVars = resultVars.Union(filteredData);
					}
					break;
				default:
					break;
			}
			return resultVars.ToList();
		}

		private IEnumerable<dynamic> SearchValues(string searchField, string searchString, string searchOper, IEnumerable<dynamic> vals)
		{
			if (searchField.Equals("Provider"))
			{
				switch (searchOper)
				{
					case "eq":
						vals = vals.Where(v => v.Provider.ToUpper().Equals(searchString.ToUpper()));
						break;
					case "bw":
						vals = vals.Where(v => v.Provider.ToUpper().StartsWith(searchString.ToUpper()));
						break;
					case "ew":
						vals = vals.Where(v => v.Provider.ToUpper().EndsWith(searchString.ToUpper()));
						break;
					case "cn":
						vals = vals.Where(v => v.Provider.ToUpper().Contains(searchString.ToUpper()));
						break;
				}
			}
			else if (ServiceList.Where(c => c.serviceID.Equals(searchField)).ToArray().Length > 0)
			{
				vals = vals.Where(c => GetProperty(c, searchField).Equals(searchString)).ToArray();
			}

			return vals;
		}

		private IEnumerable<dynamic> ApplySortOrderFilter(string sortField, string sortOrder, IEnumerable<dynamic> list)
		{
			switch (sortOrder) {
				case "asc":
					return list.OrderBy(c => ((IDictionary<string, object>)c)[sortField]);
				case "desc":
					return list.OrderByDescending(c => ((IDictionary<string, object>)c)[sortField]);
				default:
					throw new ArgumentException($"Invalid sort order '{sortOrder}' provided. Expected 'asc' or 'desc'.", nameof(sortOrder));
			}
		}

		List<dynamic> GetServiceList()
		{
			var services = from s in Context.eHubClients
						   where s.CC_OwnerCategory.Equals("Service")
						   select new { serviceID = s.CC_ID, servicePK = s.CC_PK };
			return services.ToList<dynamic>();
		}

		List<dynamic> GetServiceProviderClientList()
		{
			var providers = from p in Context.eHubServiceProviders
							where p.eHubClient.CC_OwnerCategory != "Service" && p.eHubClient.CC_OwnerCategory != "ServiceProvider"
							select new { providerID = p.eHubClient.CC_ID, servicePK = p.SP_CC_Service, providerPK = p.SP_CC_Provider };

			return providers.ToList<dynamic>();
		}

		object GetCW1Clients()
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			var provider = from p in Context.eHubClients
						   where p.CC_OwnerCategory != "Service Provider" && p.CC_OwnerCategory != "Service" &&
						   !Context.eHubServiceProviders.Any(s => s.SP_CC_Provider == p.CC_PK)
						   select new { Provider_CC_ID = p.CC_ID, Provider_CC_PK = p.CC_PK };

			if (filtered)
			{
				string id = Request["Provider_CC_ID"];
				if (!String.IsNullOrWhiteSpace(id))
					provider = provider.Where(c => c.Provider_CC_ID.ToUpper().Contains(id.ToUpper()));
			}

			int count = provider.Count();

			switch (sidx + " " + sord)
			{
				case "ID desc":
					provider = provider.OrderByDescending(c => c.Provider_CC_ID);
					break;
				default:
					provider = provider.OrderBy(c => c.Provider_CC_ID);
					break;
			}

			return new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				serviceProvider = provider.Skip((page - 1) * rows).Take(rows).ToList()
			};
		}

		List<dynamic> GetServiceListForProvider(Guid providerPK)
		{
			var services = from s in Context.eHubServiceProviders
						   join e in Context.eHubClients on s.SP_CC_Service equals e.CC_PK
						   where s.SP_CC_Provider == providerPK
						   select new { CC_ID = e.CC_ID };

			return services == null ? null : services.ToList<dynamic>();
		}

		static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
		{
			var expandoDict = expando as IDictionary<string, object>;
			if (expandoDict.ContainsKey(propertyName))
				expandoDict[propertyName] = propertyValue;
			else
				expandoDict.Add(propertyName, propertyValue);
		}

		string GetProperty(ExpandoObject expando, string propertyName)
		{
			var expandoDict = expando as IDictionary<string, object>;
			return expandoDict[propertyName].ToString();
		}

		List<dynamic> ServiceList
		{
			get { return serviceList ?? (serviceList = GetServiceList()); }
		}

		List<dynamic> ProviderList
		{
			get { return providerList ?? (providerList = GetServiceProviderClientList()); }
		}

		List<dynamic> serviceList;
		List<dynamic> providerList;
	}
}
