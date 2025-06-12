using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Portal.Controllers
{
	public class BroadcastSubscriptionsController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("BroadcastSubscriptionsLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult Broadcasters()
		{
			var broadcasters = Context.eHubSubscriptionBroadcasters.Select(b => new
			{
				SB_PK = b.SB_PK,
				ST_ID = b.eHubSubscriptionType.ST_ID,
				ST_Name = b.eHubSubscriptionType.ST_Name,
				CC_ID_Sender = b.eHubClient_Sender.CC_ID,
				CC_ID_Recipient = b.eHubClient_Recipient == null ? null : b.eHubClient_Recipient.CC_ID
			}).OrderBy(b => b.ST_Name);
			return Json(new { eHubSubscriptionBroadcasters = broadcasters.ToList() }, JsonRequestBehavior.AllowGet);
		}

		public JsonResult BroadcasterInfo(Guid broadcaster)
		{
			return Json(new
			{
				eHubSubscriptionBroadcaster = Context.eHubSubscriptionBroadcasters.Select(b => new
				{
					SB_PK = b.SB_PK,
					ST_ID = b.eHubSubscriptionType.ST_ID,
					ST_Name = b.eHubSubscriptionType.ST_Name,
					CC_ID_Sender = b.eHubClient_Sender.CC_ID,
					CC_ID_Recipient = b.eHubClient_Recipient == null ? null : b.eHubClient_Recipient.CC_ID,
					SB_SubscriberSelectSql = b.SB_SubscriberSelectSql
				}).First(b => b.SB_PK == broadcaster)
			}, JsonRequestBehavior.AllowGet);
		}

		protected void AddSubscriptionBroadcasterLog(string oper, eHubSubscriptionBroadcaster broadcaster)
		{
			tempLogs.Add($"[{oper}] eHubSubscriptionBroadcaster: SB_PK={broadcaster.SB_PK}, SB_ST={broadcaster.SB_ST}, SB_CC_Sender={broadcaster.SB_CC_Sender}, SB_CC_Recipient={broadcaster.SB_CC_Recipient?.ToString() ?? "NULL"}, SB_SubscriberSelectSql={broadcaster.SB_SubscriberSelectSql ?? "NULL"}");
		}

		protected void AddSubscriptionTypeLog(string oper, eHubSubscriptionType type)
		{
			tempLogs.Add($"[{oper}] eHubSubscriptionType: ST_PK={type.ST_PK}, ST_ID={type.ST_ID}, ST_Name={type.ST_Name}, ST_ExpiryDays={type.ST_ExpiryDays}");
		}

		protected void AddSubscriptionValueLog(string oper, eHubSubscriptionValue val)
		{
			tempLogs.Add($"[{oper}] eHubSubscriptionValue: SV_PK={val.SV_PK}, SV_ST={val.SV_ST}, SV_CC_Sender={val.SV_CC_Sender}, SV_CC_Recipient={val.SV_CC_Recipient}, SV_Value={val.SV_Value}, SV_Reference={val.SV_Reference}" +
			$", SV_ReferenceType={val.SV_ReferenceType}, SV_SubscribedUTC={val.SV_SubscribedUTC.ToString("yyyy-MM-dd hh:mm:ss.fff")}, SV_ExpiryUTC={val.SV_ExpiryUTC?.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
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
		public void BroadcasterInfoEdit()
		{
			var sbPK = Request["SB_PK"];
			var stID = Request["ST_ID"];
			var stName = Request["ST_Name"];
			var ccIDSender = Request["CC_ID_Sender"];
			var ccIDRecipient = Request["CC_ID_Recipient"];

			eHubSubscriptionBroadcaster broadcaster;
			if (String.IsNullOrWhiteSpace(sbPK))
				broadcaster = new eHubSubscriptionBroadcaster { SB_PK = Guid.NewGuid() };
			else
			{
				var sbpkguid = Guid.Parse(sbPK);
				broadcaster = Context.eHubSubscriptionBroadcasters.First(t => t.SB_PK == sbpkguid);
			}

			switch (Request["oper"])
			{
				case "add":
					if (String.IsNullOrWhiteSpace(stID) || String.IsNullOrWhiteSpace(stName) || String.IsNullOrWhiteSpace(ccIDSender)) throw new ValidationException("ID, Name and Broadcaster are required.");
					if (Context.eHubSubscriptionTypes.Any(t => t.ST_ID == stID)) throw new ValidationException("ID is not unique.");
					broadcaster.eHubSubscriptionType = new eHubSubscriptionType { ST_PK = Guid.NewGuid(), ST_ID = stID, ST_Name = stName };
					broadcaster.eHubClient_Sender = Context.eHubClients.First(c => c.CC_ID == ccIDSender);
					broadcaster.SB_CC_Recipient = string.IsNullOrWhiteSpace(ccIDRecipient) ? (Guid?)null : Context.eHubClients.First(c => c.CC_ID == ccIDRecipient).CC_PK;
					Context.eHubSubscriptionTypes.AddObject(broadcaster.eHubSubscriptionType);
					Context.eHubSubscriptionBroadcasters.AddObject(broadcaster);
					AddSubscriptionTypeLog(Request["oper"], broadcaster.eHubSubscriptionType);
					break;
				case "edit":
					if (String.IsNullOrWhiteSpace(stID) || String.IsNullOrWhiteSpace(stName) || String.IsNullOrWhiteSpace(ccIDSender)) throw new ValidationException("ID, Name and Broadcaster are required.");
					broadcaster.eHubSubscriptionType.ST_ID = stID;
					broadcaster.eHubSubscriptionType.ST_Name = stName;
					AddSubscriptionTypeLog(Request["oper"], broadcaster.eHubSubscriptionType);
					broadcaster.eHubClient_Sender = Context.eHubClients.First(c => c.CC_ID == ccIDSender);
					broadcaster.SB_CC_Recipient = string.IsNullOrWhiteSpace(ccIDRecipient) ? (Guid?)null : Context.eHubClients.First(c => c.CC_ID == ccIDRecipient).CC_PK;
					break;
				case "del":
					broadcaster.eHubSubscriptionType.eHubSubscriptionValues.ToList().ForEach(v =>
					{
						AddSubscriptionValueLog(Request["oper"], v);
						Context.eHubSubscriptionValues.DeleteObject(v);
					});
					AddSubscriptionTypeLog(Request["oper"], broadcaster.eHubSubscriptionType);
					Context.eHubSubscriptionTypes.DeleteObject(broadcaster.eHubSubscriptionType);
					Context.eHubSubscriptionBroadcasters.DeleteObject(broadcaster);

					break;
				default:
					break;
			}
			Context.SaveChanges();
			AddSubscriptionBroadcasterLog(Request["oper"], broadcaster);
			SaveLogs();
		}

		public JsonResult Subscribers(Guid broadcaster)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchQuery = Request["filters"];
			MultipleFilter filterQuery = new MultipleFilter();
			if (!string.IsNullOrEmpty(searchQuery))
			{
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);
			}

			var broadcasterInstance = Context.eHubSubscriptionBroadcasters.First(b => b.SB_PK == broadcaster);
			var vals = Context.eHubSubscriptionValues.Where(v => v.SV_ST == broadcasterInstance.SB_ST);

			vals.ToList().ForEach(val =>
			{
				val.SV_Value = (String.IsNullOrEmpty(val.SV_Value)) ? "FALSE" : val.SV_Value.ToUpper();
			});

			Context.SaveChanges();

			if (filterQuery.rules != null && filterQuery.rules.Count > 0) {
				vals = ApplyMultipleValuesFilter(filterQuery, vals);
			}

			int count = vals.Count();

			vals = ApplyValuesSort(sidx, sord, vals);

			vals = vals.Skip((page - 1) * rows).Take(rows);

			var list = vals.ToList().Select(v => new
			{
				SV_PK = v.SV_PK,
				SV_CC_Provider = v.eHubClient_Provider.CC_ID,
				SV_CC_SubscriberID = v.eHubClient_Subscriber.CC_ID,
				SV_CC_SubscriberName = v.eHubClient_Subscriber.CC_FriendlyName,
				SV_SubscribedUTC = v.SV_SubscribedUTC.ToString("s"),
				SV_Value = v.SV_Value
			});

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubSubscriptionValues = list.ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		private IQueryable<eHubSubscriptionValue> ApplyMultipleValuesFilter(MultipleFilter filterQuery, IQueryable<eHubSubscriptionValue> vals)
		{
			switch (filterQuery.groupOp)
			{
				case "AND":
					foreach (var rule in filterQuery.rules)
						vals = ApplyValuesFilter(rule.field, rule.data, rule.op, vals);
					break;
				case "OR":
					var origin = vals;
					vals = ApplyValuesFilter(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, vals);
					for (int rule = 1; rule < filterQuery.rules.Count; rule++)
					{
						IQueryable<eHubSubscriptionValue> filterData = origin;
						filterData = ApplyValuesFilter(filterQuery.rules[rule].field, filterQuery.rules[rule].data, filterQuery.rules[rule].op, filterData);
						vals = vals.AsQueryable().Union(filterData);
					}
					break;
				default:
					break;
			}

			return vals;
		}

		static IQueryable<eHubSubscriptionValue> ApplyValuesFilter(string searchField, string searchString, string searchOper, IQueryable<eHubSubscriptionValue> vals)
		{
			switch (searchField)
			{
				case "eHubClient_Subscriber.CC_ID":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.eHubClient_Subscriber.CC_ID == searchString);
							break;
						case "bw":
							vals = vals.Where(v => v.eHubClient_Subscriber.CC_ID.StartsWith(searchString));
							break;
						case "ew":
							vals = vals.Where(v => v.eHubClient_Subscriber.CC_ID.EndsWith(searchString));
							break;
						case "cn":
							vals = vals.Where(v => v.eHubClient_Subscriber.CC_ID.Contains(searchString));
							break;
					}
					break;
				case "eHubClient_Subscriber.CC_FriendlyName":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.eHubClient_Provider.CC_ID == searchString);
							break;
						case "bw":
							vals = vals.Where(v => v.eHubClient_Provider.CC_ID.StartsWith(searchString));
							break;
						case "ew":
							vals = vals.Where(v => v.eHubClient_Provider.CC_ID.EndsWith(searchString));
							break;
						case "cn":
							vals = vals.Where(v => v.eHubClient_Provider.CC_ID.Contains(searchString));
							break;
					}
					break;
			}
			return vals;
		}

		static IQueryable<eHubSubscriptionValue> ApplyValuesSort(string sidx, string sord, IQueryable<eHubSubscriptionValue> vals)
		{
			switch (sidx + " " + sord)
			{
				case "eHubClient_Subscriber.CC_ID asc":
					vals = vals.OrderBy(v => v.eHubClient_Subscriber.CC_ID);
					break;
				case "eHubClient_Subscriber.CC_ID desc":
					vals = vals.OrderByDescending(v => v.eHubClient_Subscriber.CC_ID);
					break;
				case "eHubClient_Subscriber.CC_FriendlyName asc":
					vals = vals.OrderBy(v => v.eHubClient_Subscriber.CC_FriendlyName);
					break;
				case "eHubClient_Subscriber.CC_FriendlyName desc":
					vals = vals.OrderByDescending(v => v.eHubClient_Subscriber.CC_FriendlyName);
					break;
				case "SV_SubscribedUTC asc":
					vals = vals.OrderBy(v => v.SV_SubscribedUTC);
					break;
				case "SV_SubscribedUTC desc":
					vals = vals.OrderByDescending(v => v.SV_SubscribedUTC);
					break;
				case "SV_Value asc":
					vals = vals.OrderBy(v => v.SV_Value);
					break;
				case "SV_Value desc":
					vals = vals.OrderByDescending(v => v.SV_Value);
					break;
				default:
					vals = vals.OrderBy(v => v.eHubClient_Subscriber.CC_ID);
					break;
			}
			return vals;
		}

		[HttpPost]
		public JsonResult SubscriberEdit(Guid broadcaster)
		{
			try
			{
				var broadcasterInstance = Context.eHubSubscriptionBroadcasters.First(b => b.SB_PK == broadcaster);
				var svpk = Request["SV_PK"] ?? Request["id"];
				var providerID = Request["SV_CC_Provider"];
				var provider = Context.eHubClients.FirstOrDefault(c => c.CC_ID == providerID);
				var subscriberID = Request["SV_CC_SubscriberID"];
				var subscriber = Context.eHubClients.FirstOrDefault(c => c.CC_ID == subscriberID);
				var archivedValue = Request["SV_Value"];

				eHubSubscriptionValue val;
				if (String.IsNullOrWhiteSpace(svpk) || svpk == "_empty")
					val = new eHubSubscriptionValue();
				else
				{
					var svpkguid = Guid.Parse(svpk);
					val = Context.eHubSubscriptionValues.First(v => v.SV_PK == svpkguid);
				}

				switch (Request["oper"])
				{
					case "add":
						val.SV_PK = Guid.NewGuid();
						val.SV_ST = broadcasterInstance.SB_ST;
						val.SV_CC_Sender = broadcasterInstance.SB_CC_Sender;
						val.SV_CC_Recipient = subscriber.CC_PK;
						val.SV_Value = String.IsNullOrEmpty(archivedValue) ? "FALSE" : archivedValue.ToUpper();
						val.SV_SubscribedUTC = DateTime.UtcNow;
						Context.eHubSubscriptionValues.AddObject(val);
						break;
					case "edit":
						val.SV_Value = String.IsNullOrEmpty(archivedValue) ? val.SV_Value : archivedValue.ToUpper();
						val.SV_CC_Recipient = subscriber.CC_PK;
						val.SV_SubscribedUTC = DateTime.UtcNow;
						break;
					case "del":
						Context.eHubSubscriptionValues.DeleteObject(val);
						break;
					default:
						break;
				}
				Context.SaveChanges();
				AddSubscriptionValueLog(Request["oper"], val);
				SaveLogs();
				return Json(new { success = true, id = val.SV_PK }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		[HttpGet]
		public FileContentResult SubscribersExportCsv()
		{
			var broadcaster = new Guid(Request["broadcasterPK"]);
			var broadcasterInstance = Context.eHubSubscriptionBroadcasters.First(b => b.SB_PK == broadcaster);
			using (var wrt = new StringWriter())
			using (var csv = new CsvHelper.CsvWriter(wrt))
			{
				csv.WriteField("Subscriber");
				csv.WriteField("Archive Outbox");
				csv.NextRecord();
				Context.eHubSubscriptionValues.Where(v => v.SV_ST == broadcasterInstance.SB_ST).OrderBy(v => v.eHubClient_Subscriber.CC_ID).ToList().ForEach(v =>
				{
					csv.WriteField(v.eHubClient_Subscriber.CC_ID);
					csv.WriteField(v.SV_Value.ToUpper());
					csv.NextRecord();
				});

				var subtype = Context.eHubSubscriptionTypes.First(t => t.ST_PK == broadcasterInstance.SB_ST);
				string filename = String.Format("{0}_{1}.csv", subtype.ST_ID, subtype.ST_Name.Replace(" ", ""));
				return File(Encoding.Default.GetBytes(wrt.ToString()), "text/text", filename);
			}
		}

		[HttpPost]
		public JsonResult SubscribersImportCsv()
		{
			try
			{
				var broadcaster = new Guid(Request["broadcasterPK"]);
				var broadcasterInstance = Context.eHubSubscriptionBroadcasters.First(b => b.SB_PK == broadcaster);
				var option = Request["option"];

				var values = new Dictionary<string, string>();
				var archivedWarning = false;
				using (var rdr = new StreamReader(Request.Files["uploadFile"].InputStream))
				using (var csv = new CsvHelper.CsvParser(rdr))
				{
					var fields = csv.Read();
					while ((fields = csv.Read()) != null)
					{
						if (fields[1].ToUpper() != "TRUE" && fields[1].ToUpper() != "FALSE")
						{
							archivedWarning = true;
						}
						var archivedValue = (fields[1].ToUpper() == "TRUE") ? "TRUE" : "FALSE";
						values.Add(fields[0], archivedValue);
					}
				}

				var oldSubscriptionValues = Context.eHubSubscriptionValues
					.Where(v => v.SV_ST == broadcasterInstance.SB_ST)
					.ToDictionary(
						v => v.eHubClient_Subscriber.CC_ID,
						v => v
					);

				foreach (KeyValuePair<string, string> v in values)
				{
					if (!oldSubscriptionValues.ContainsKey(v.Key))
					{
						var subsriber_CC = Context.eHubClients.FirstOrDefault(c => c.CC_ID == v.Key);
						if (subsriber_CC == null)
						{
							throw new Exception(
								"File cannot be imported because there exists a row with invalid subscriber ID. First invalid subscriber ID would be '" +
								v.Key + "'");
						}

						var val = new eHubSubscriptionValue
						{
							SV_PK = Guid.NewGuid(),
							eHubClient_Provider = broadcasterInstance.eHubClient_Sender,
							eHubClient_Subscriber = subsriber_CC,
							SV_Value = v.Value,
							SV_ST = broadcasterInstance.SB_ST,
							SV_SubscribedUTC = DateTime.UtcNow,
						};
						Context.eHubSubscriptionValues.AddObject(val);
						AddSubscriptionValueLog("add", val);

					}
					else
					{
						var oldSubscriptionValue = oldSubscriptionValues[v.Key];

						if (oldSubscriptionValue.SV_Value != v.Value)
						{
							oldSubscriptionValue.SV_Value = v.Value;
							AddSubscriptionValueLog("edit", oldSubscriptionValue);
						}
					}
				};

				if (option == "replace")
				{
					oldSubscriptionValues
						.Where(o => !values.ContainsKey(o.Key))
						.Select(o => o.Value).ToList().ForEach(v =>
						{
							Context.eHubSubscriptionValues.DeleteObject(v);
							AddSubscriptionValueLog("del", v);
						});
				}

				Context.SaveChanges();
				SaveLogs();

				if (archivedWarning)
				{
					return Json(new
					{
						success = true,
						archivedWarning = true,
						message = "WARNING: Archived value must be 'TRUE' or 'FALSE'. Imported file has some invalid archived value(s). All of invalid values are converted to 'FALSE' for 'Archive Outbox'"
					}, JsonRequestBehavior.AllowGet);
				}
				else
				{
					return Json(new
					{
						success = true,
						archivedWarning = false,
						message = "Imported file successfully!!!"
					}, JsonRequestBehavior.AllowGet);
				}

			}
			catch (Exception e)
			{
				return Json(new
				{
					success = false,
					message = e.Message
				}, JsonRequestBehavior.AllowGet);
			}

		}

		public JsonResult Clients(bool includeNonProd, Guid? broadcaster)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);
			var eHubClients = Context.eHubClients.AsQueryable();
			if (broadcaster != null)
			{
				var broadcasterInstance = Context.eHubSubscriptionBroadcasters.First(v => v.SB_PK == broadcaster);
				var existingClients = Context.eHubSubscriptionValues
					.Where(v => (v.SV_CC_Sender == broadcasterInstance.SB_CC_Sender && v.SV_ST == broadcasterInstance.SB_ST))
					.Select(v => v.SV_CC_Recipient).ToList();

				var existingClientsSet = new HashSet<Guid>(existingClients);

				eHubClients = eHubClients.Where(c => !existingClientsSet.Contains(c.CC_PK));
			}

			IEnumerable<dynamic> clients;
			if (includeNonProd)
			{
				clients = eHubClients.Select(c => new
				{
					c.CC_ID,
					c.CC_FriendlyName
				});
			}
			else
			{
				clients = from c in eHubClients
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
	}
}
