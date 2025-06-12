using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eHub.Portal.Models.View.MessageReference;
using Common.Logging;
using CsvHelper;
using Newtonsoft.Json;


namespace CargoWise.eHub.Portal.Controllers
{
	public class eHubMessageReferenceController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("eHubMessageReferenceLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index(string applicationCode)
		{
			var codes = ApplicationCodes;

			if (applicationCode == null || applicationCode.Trim().Equals(string.Empty))
			{
				if (codes.Any())
					return RedirectToAction("Index", "eHubMessageReference", new { applicationCode = codes.FirstOrDefault() });
				else
				{
					return View(new IndexModel
					{
						Selected = string.Empty,
						CodeList = codes
					});
				}
			}

			return View(new IndexModel
			{
				Selected = applicationCode,
				CodeList = codes
			});
		}

		[HttpGet]
		public JsonResult List(string applicationCode)
		{
			var sortIndex = Request["sidx"] ?? "CC_ID";
			var sortDirection = Request["sord"] ?? "ASC";
			string searchQuery = Request["filters"] ?? null;

			var vals = (from r in Context.eHubMessageReferenceRegistries
						where r.CR_ApplicationCode == applicationCode
						select new MessageReferenceRegistry
						{
							CR_PK = r.CR_PK,
							CR_CC_Client = r.eHubClient.CC_PK,
							CC_ID = r.eHubClient.CC_ID,
							CC_FriendlyName = r.eHubClient.CC_FriendlyName,
							CR_ApplicationCode = r.CR_ApplicationCode,
							CR_MessageReference = r.CR_MessageReference,
							CR_Password = r.CR_Password
						}).OrderBy(string.Format("{0} {1}", sortIndex, sortDirection));

			MultipleFilter filterQuery = new MultipleFilter();
			if (!string.IsNullOrEmpty(searchQuery))
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
				vals = ApplyMultipleValuesFilter(filterQuery, vals);

			vals = ApplySorting(sortIndex, sortDirection, vals);

			int rowNo = GetRows(vals);
			int total = GetTotal(vals, rowNo);
			int page = GetPage(total);

			return Json(new
			{
				page = page,
				total = total,
				records = vals.Count(),
				eHubMessageReferenceRegistry = vals.Skip((page - 1) * rowNo).Take(rowNo)
			}, JsonRequestBehavior.AllowGet);
		}

		private IQueryable<MessageReferenceRegistry> ApplyMultipleValuesFilter(MultipleFilter filterQuery, IQueryable<MessageReferenceRegistry> vals)
		{
			switch (filterQuery.groupOp)
			{
				case "AND":
					foreach (var rule in filterQuery.rules)
						vals = ApplySearching(rule.field, rule.data, rule.op, vals);
					break;
				case "OR":
					vals = ApplySearching(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, vals);
					for (int rule = 1; rule < filterQuery.rules.Count; rule++)
					{
						IQueryable<MessageReferenceRegistry> filterData = vals;
						filterData = ApplySearching(filterQuery.rules[rule].field, filterQuery.rules[rule].data, filterQuery.rules[rule].op, filterData);
						vals = vals.AsQueryable().Union(filterData);
					}
					break;
				default:
					break;
			}
			return vals;
		}
		static IQueryable<MessageReferenceRegistry> ApplySearching(string searchField, string searchString, string searchOper, IQueryable<MessageReferenceRegistry> messageReferences)
		{
			switch (searchField)
			{
				case "CC_ID":
					switch (searchOper)
					{
						case "eq":
							messageReferences = messageReferences.Where(r => r.CC_ID == searchString);
							break;
						case "bw":
							messageReferences = messageReferences.Where(r => r.CC_ID.StartsWith(searchString));
							break;
						case "ew":
							messageReferences = messageReferences.Where(r => r.CC_ID.EndsWith(searchString));
							break;
						case "cn":
							messageReferences = messageReferences.Where(r => r.CC_ID.Contains(searchString));
							break;
					}
					break;
				case "CC_FriendlyName":
					switch (searchOper)
					{
						case "eq":
							messageReferences = messageReferences.Where(r => r.CC_FriendlyName == searchString);
							break;
						case "bw":
							messageReferences = messageReferences.Where(r => r.CC_FriendlyName.StartsWith(searchString));
							break;
						case "ew":
							messageReferences = messageReferences.Where(r => r.CC_FriendlyName.EndsWith(searchString));
							break;
						case "cn":
							messageReferences = messageReferences.Where(r => r.CC_FriendlyName.Contains(searchString));
							break;
					}
					break;

				case "CR_MessageReference":
					switch (searchOper)
					{
						case "eq":
							messageReferences = messageReferences.Where(r => r.CR_MessageReference == searchString);
							break;
						case "bw":
							messageReferences = messageReferences.Where(r => r.CR_MessageReference.StartsWith(searchString));
							break;
						case "ew":
							messageReferences = messageReferences.Where(r => r.CR_MessageReference.EndsWith(searchString));
							break;
						case "cn":
							messageReferences = messageReferences.Where(r => r.CR_MessageReference.Contains(searchString));
							break;
					}
					break;
				case "CR_Password":
					switch (searchOper)
					{
						case "eq":
							messageReferences = messageReferences.Where(r => r.CR_Password == searchString);
							break;
						case "bw":
							messageReferences = messageReferences.Where(r => r.CR_Password.StartsWith(searchString));
							break;
						case "ew":
							messageReferences = messageReferences.Where(r => r.CR_Password.EndsWith(searchString));
							break;
						case "cn":
							messageReferences = messageReferences.Where(r => r.CR_Password.Contains(searchString));
							break;
					}
					break;
			}

			return messageReferences;
		}

		private static IQueryable<MessageReferenceRegistry> ApplySorting(string sortIndex, string sortDirection, IQueryable<MessageReferenceRegistry> messageReferenceRows)
		{
			var messageReference = messageReferenceRows.AsEnumerable();
			var propertyInfo = typeof(MessageReferenceRegistry).GetProperty(sortIndex);
			if (sortDirection.ToUpper() == "ASC")
			{
				messageReference = messageReference.OrderBy(x => propertyInfo.GetValue(x, null));
			}
			else
			{
				messageReference = messageReference.OrderByDescending(x => propertyInfo.GetValue(x, null));
			}

			return messageReference.AsQueryable();
		}


		protected void AddMessageReferenceRegistryLog(string oper, eHubMessageReferenceRegistry registry)
		{
			tempLogs.Add($"[{oper}] eHubMessageReferenceRegistry: CR_PK={registry.CR_PK}, CR_CC_Client={registry.CR_CC_Client}" +
				$", CR_ApplicationCode={registry.CR_ApplicationCode}, CR_MessageReference={registry.CR_MessageReference}, CR_Password={registry.CR_Password}");
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
		public JsonResult Edit(string applicationCode)
		{
			try
			{
				string operation = GetOperation();
				eHubMessageReferenceRegistry registry = operation.Equals(Operation.Add) ? CreateNewRegistry() : GetRegistry();

				if (operation.Equals(Operation.Delete))
					Context.eHubMessageReferenceRegistries.DeleteObject(registry);
				else
				{
					var messageReference = Request["CR_MessageReference"];
					if (messageReference == null || string.IsNullOrEmpty(messageReference = messageReference.Trim()))
						return Json(new { success = false, message = "Message Reference Required" });

					UpdateRegistryProperties(registry, applicationCode, messageReference);
				}

				Context.SaveChanges();
				AddMessageReferenceRegistryLog(operation, registry);
				SaveLogs();
				return Json(new { success = true, newid = registry.CR_PK });
			}
			catch (HttpException) { throw; }
			catch (Exception e)
			{
				string message;
				if (e.InnerException != null)
				{
					message = string.Format("Unexpected Exception: {0}", e.InnerException.Message);
				}
				else
				{
					message = string.Format("Unexpected Exception: {0}", e.Message);
				}
				return Json(new { success = false, message = message });
			}
		}

		[HttpGet]
		public ActionResult ClientList(bool includeNonProd)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clients;
			if (includeNonProd)
			{
				clients = from c in Context.eHubClients
						  orderby c.CC_ID
						  select new { c.CC_ID, c.CC_FriendlyName };
			}
			else
			{
				clients = from c in Context.eHubClients
						  join p in Context.ediProdClients on c.CC_PK equals p.CC_PK into joinedClients
						  from p in joinedClients.DefaultIfEmpty()
						  where p == null || p.LD_LicenceType == "PRD"
						  orderby c.CC_ID
						  select new { c.CC_ID, c.CC_FriendlyName };

			}

			if (filtered)
			{
				string ccid = Request["CC_ID"];
				string cccode = Request["OH_Code"];
				string ccname = Request["CC_FriendlyName"];
				if (!String.IsNullOrWhiteSpace(ccid))
					clients = clients.Where(c => c.CC_ID.StartsWith(ccid, StringComparison.OrdinalIgnoreCase));
				if (!String.IsNullOrWhiteSpace(ccname))
					clients = clients.Where(c => c.CC_FriendlyName.Contains(ccname));
			}

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

			int count = clients.Count();

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubClients = clients.Skip((page - 1) * rows).Take(rows).ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public FileContentResult ExportCSV(string applicationCode)
		{
			if (applicationCode == null || applicationCode.Trim().Equals(string.Empty))
				throw new HttpException(404, "Invalid Application Code");

			using (var wrt = new StringWriter())
			using (var csv = new CsvWriter(wrt))
			{
				new List<String> { "eHub Client ID, Name, Application Code, Message Reference, Password" }.ForEach(h => csv.WriteField(h));
				csv.NextRecord();
				var messageReferences = GetMessageReferences("CC_ID", "ASC", applicationCode);
				foreach (var reference in messageReferences)
				{
					csv.WriteField(reference.CC_ID);
					csv.WriteField(reference.CC_FriendlyName);
					csv.WriteField(reference.CR_ApplicationCode);
					csv.WriteField(reference.CR_MessageReference);
					csv.WriteField(reference.CR_Password);
					csv.NextRecord();
				}
				return File(Encoding.Default.GetBytes(wrt.ToString()), "text/csv", string.Format("eHubMessageRegistry_{0}.csv", applicationCode));
			}
		}

		[HttpPost]
		public ActionResult ImportCSV(string applicationCode)
		{
			try
			{
				var file = Request.Files["file"];

				if (file.ContentLength == 0 || !file.FileName.Substring(file.FileName.LastIndexOf('.')).Equals(".csv"))
				{
					TempData.Add("errorMessage", "Invalid File.");
					return RedirectToAction("Index", new { applicationCode = applicationCode });
				}

				List<MessageReferenceRegistry> registryList = ParseCsv();
				CleanRegistry(applicationCode);
				AddToRegistry(applicationCode, registryList);
				Context.SaveChanges();
				SaveLogs();
				return RedirectToAction("Index", new { applicationCode = applicationCode });
			}
			catch (Exception)
			{
				TempData.Add("errorMessage", "Error occurred.");
				return RedirectToAction("Index", new { applicationCode = applicationCode });
			}
		}

		private List<string> ApplicationCodes { get { return Context.eHubMessageReferenceRegistries.Select(r => r.CR_ApplicationCode).Distinct().ToList(); } }

		#region Import Methods

		private void CleanRegistry(string applicationCode)
		{
			var forDeletion = Context.eHubMessageReferenceRegistries.Where(r => r.CR_ApplicationCode.Equals(applicationCode));

			foreach (var registry in forDeletion)
			{
				Context.eHubMessageReferenceRegistries.DeleteObject(registry);
				AddMessageReferenceRegistryLog("del", registry);
			}
		}

		private void AddToRegistry(string applicationCode, List<MessageReferenceRegistry> registryList)
		{
			foreach (var registry in registryList)
			{
				if (registryList.IndexOf(registry) == 0)
					continue;

				var client = Context.eHubClients.Where(c => c.CC_ID.Equals(registry.CC_ID));
				if (!client.Any())
					continue;

				var reg = new eHubMessageReferenceRegistry
				{
					CR_PK = Guid.NewGuid(),
					CR_ApplicationCode = applicationCode.ToUpper(),
					eHubClient = client.First(),
					CR_MessageReference = registry.CR_MessageReference,
					CR_Password = registry.CR_Password
				};

				Context.eHubMessageReferenceRegistries.AddObject(reg);
				AddMessageReferenceRegistryLog("add", reg);
			}
		}

		private List<MessageReferenceRegistry> ParseCsv()
		{
			List<MessageReferenceRegistry> registryList = new List<MessageReferenceRegistry>();
			using (var reader = new StreamReader(Request.Files["file"].InputStream))
			using (var csv = new CsvParser(reader))
			{
				var values = csv.Read();
				while ((values = csv.Read()) != null)
				{
					registryList.Add(new MessageReferenceRegistry
					{
						CC_ID = SafeGet(values, 0),
						CC_FriendlyName = SafeGet(values, 1),
						CR_ApplicationCode = SafeGet(values, 2),
						CR_MessageReference = SafeGet(values, 3),
						CR_Password = SafeGet(values, 4)
					});
				}
			}
			return registryList;
		}

		private string SafeGet(string[] array, int index)
		{
			return (index < array.Length) ? array[index] : "";
		}

		#endregion

		#region Edit Methods

		private void UpdateRegistryProperties(eHubMessageReferenceRegistry registry, string applicationCode, string messageReference)
		{
			registry.eHubClient = GetClient();
			registry.CR_ApplicationCode = applicationCode.ToUpper();
			registry.CR_MessageReference = messageReference;
			registry.CR_Password = GetPassword();
		}

		private string GetPassword()
		{
			var password = Request["CR_Password"];
			if (password != null && string.IsNullOrEmpty(password.Trim()))
				password = null;
			return password;
		}

		private string GetOperation()
		{
			string operation = null;
			if ((operation = Request["oper"]) == null)
				throw new HttpException(500, "Invalid Operation");
			return operation;
		}

		private eHubClient GetClient()
		{
			string clientID = Request["CC_ID"];
			if (clientID == null)
				throw new HttpException(404, "eHub Client ID Not Indicated");
			var clients = Context.eHubClients.Where(c => c.CC_ID.Equals(clientID));
			if (!clients.Any())
				throw new HttpException(404, "eHub Client Not Found");
			return clients.First();
		}

		private eHubMessageReferenceRegistry GetRegistry()
		{
			try
			{
				if (Request["id"] == null)
					throw new HttpException(404, "eHub Message Registry ID Not Indicated");
				var id = new Guid(Request["id"]);
				var registries = Context.eHubMessageReferenceRegistries.Where(r => r.CR_PK.Equals(id));
				if (!registries.Any())
					throw new HttpException(404, "eHub Message Registry Not Found");
				return registries.First();
			}
			catch (FormatException)
			{
				throw new HttpException(404, "Invalid Registry ID Format");
			}
		}

		private eHubMessageReferenceRegistry CreateNewRegistry()
		{
			var registry = new eHubMessageReferenceRegistry();
			registry.CR_PK = Guid.NewGuid();
			Context.eHubMessageReferenceRegistries.AddObject(registry);
			return registry;
		}

		#endregion

		#region List Methods

		private int StringToInt(string stringValue, int defaultValue)
		{
			stringValue = stringValue != null ? stringValue.Trim() : defaultValue.ToString();
			int value = defaultValue;
			int.TryParse(stringValue, out value);
			return value;
		}

		private IQueryable<MessageReferenceRegistry> GetMessageReferences(String sortIndex, String sortOrder, String applicationCode, Group filters = null)
		{
			var messageReferences = (from r in Context.eHubMessageReferenceRegistries
									 where r.CR_ApplicationCode == applicationCode
									 select new MessageReferenceRegistry
									 {
										 CR_PK = r.CR_PK,
										 CR_CC_Client = r.eHubClient.CC_PK,
										 CC_ID = r.eHubClient.CC_ID,
										 CC_FriendlyName = r.eHubClient.CC_FriendlyName,
										 CR_ApplicationCode = r.CR_ApplicationCode,
										 CR_MessageReference = r.CR_MessageReference,
										 CR_Password = r.CR_Password
									 }).OrderBy(string.Format("{0} {1}", sortIndex, sortOrder));

			if (filters != null)
			{
				var stringFilters = filters.ToString();
				messageReferences = messageReferences.Where(stringFilters);
			}

			return messageReferences;
		}

		private string BuildFilter(Group filters)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("(");

			builder.Append(")");
			return builder.ToString();
		}



		#region Pagination

		private int GetPage(int total)
		{
			int page = StringToInt(Request["page"], 1);
			if (page < 1)
				page = 1;
			else if (page > total)
				page = total;
			return page;
		}

		private static int GetTotal(IQueryable<MessageReferenceRegistry> messageReferences, int rows)
		{
			var total = Convert.ToInt32(Math.Ceiling((double)messageReferences.Count() / (double)rows));
			if (total <= 0)
				total = 1;
			return total;
		}

		private int GetRows(IQueryable<MessageReferenceRegistry> messageReferences)
		{
			int rows = StringToInt(Request["rows"], messageReferences.Count());
			if (rows < 1)
				rows = 1;
			return rows;
		}

		#endregion


		#endregion

		public static class Operation
		{
			public static string Add { get { return "add"; } }
			public static string Delete { get { return "del"; } }
			public static string Edit { get { return "edit"; } }
		}

		public class Group
		{
			public string groupOp { get; set; }
			public Rule[] rules { get; set; }
			public Group[] groups { get; set; }

			private bool HasRule { get { return rules != null ? rules.Any() : false; } }
			private bool HasGroup { get { return groups != null ? groups.Any() : false; } }
			private string Separator { get { return string.Format(" {0} ", groupOp); } }

			public override string ToString()
			{
				List<string> rulesAndGroups = new List<string>();
				addNonEmpty(rulesAndGroups, rules, groups);
				var stringValue = string.Join(Separator, rulesAndGroups).Trim();
				return !string.IsNullOrEmpty(stringValue) ? string.Format("({0})", stringValue) : string.Empty;
			}

			public void addNonEmpty(List<string> list, params object[][] rulesOrGroups)
			{
				foreach (var iqueryable in rulesOrGroups)
				{
					if (iqueryable == null)
						continue;
					var stringValues = from o in iqueryable where !string.IsNullOrEmpty(o.ToString()) select o.ToString();
					list.AddRange(stringValues);
				}
			}

			public class Rule
			{
				public string field { get; set; }
				public string op { get; set; }
				public string data { get; set; }

				public override string ToString()
				{
					if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(op))
						return string.Empty;
					string searchFormat;
					switch (op)
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
							throw new Exception("Unsupported Operation: " + op);
					}

					return string.Format(searchFormat, field, data).Trim();
				}
			}
		}
	}
}

