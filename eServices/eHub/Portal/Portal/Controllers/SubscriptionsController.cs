using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
	public class SubscriptionsController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("SubscriptionsLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult Types()
		{
			var types = Context.eHubSubscriptionTypes.Where(t => t.eHubSubscriptionBroadcasters.Count == 0).Select(t => new
			{
				ST_PK = t.ST_PK,
				ST_ID = t.ST_ID,
				ST_Name = t.ST_Name,
				ST_ExpiryDays = t.ST_ExpiryDays
			}).OrderBy(t => t.ST_ID);
			return Json(new { eHubSubscriptionTypes = types.ToList() }, JsonRequestBehavior.AllowGet);
		}

		public JsonResult TypeInfo(Guid type)
		{
			return Json(new
			{
				eHubSubscriptionType = Context.eHubSubscriptionTypes.Select(t => new
				{
					ST_PK = t.ST_PK,
					ST_ID = t.ST_ID,
					ST_Name = t.ST_Name,
					ST_ExpiryDays = t.ST_ExpiryDays
				}).First(t => t.ST_PK == type)
			}, JsonRequestBehavior.AllowGet);
		}

		protected void AddSubscriptionTypeLog(string oper, eHubSubscriptionType type)
		{
			tempLogs.Add($"[{oper}] eHubSubscriptionType: ST_PK={type.ST_PK}, ST_ID={type.ST_ID}, ST_Name={type.ST_Name}, ST_ExpiryDays={type?.ST_ExpiryDays}");
		}

		protected void AddSubscriptionAutoSubscribeLog(string oper, eHubSubscriptionAutoSubscribe autosub)
		{
			tempLogs.Add($"[{oper}] eHubSubscriptionAutoSubscribe: SA_PK={autosub.SA_PK}, SA_ST={autosub.SA_ST}, SA_CC_Recipient={autosub.SA_CC_Recipient}, SA_DT={autosub.SA_DT}" +
				$", SA_ValueXpath={autosub.SA_ValueXpath}, SA_ValueProperty={autosub.SA_ValueProperty}, SA_ReferenceXpath={autosub.SA_ReferenceXpath}, SA_ReferenceProperty={autosub.SA_ReferenceProperty}");
		}

		protected void AddSubscriptionLookupLog(string oper, eHubSubscriptionLookup lookup)
		{
			tempLogs.Add($"[{oper}] eHubSubscriptionLookup: SL_PK={lookup.SL_PK}, SL_ST={lookup.SL_ST}, SL_DT={lookup.SL_DT}, SL_ValueXpath={lookup.SL_ValueXpath}, SL_ValueProperty={lookup.SL_ValueProperty}");
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
		public void TypeInfoEdit()
		{
			var stpk = Request["ST_PK"];
			var id = Request["ST_ID"];
			var name = Request["ST_Name"];
			var expiry = Request["ST_ExpiryDays"];

			eHubSubscriptionType type;
			if (String.IsNullOrWhiteSpace(stpk))
				type = new eHubSubscriptionType();
			else
			{
				var stpkguid = Guid.Parse(stpk);
				type = Context.eHubSubscriptionTypes.First(t => t.ST_PK == stpkguid);
			}

			switch (Request["oper"])
			{
				case "add":
					ValidateTypeInput(id, name, expiry);
					type.ST_PK = Guid.NewGuid();
					type.ST_ID = id;
					type.ST_Name = name;
					type.ST_ExpiryDays = String.IsNullOrWhiteSpace(expiry) ? (int?)null : int.Parse(expiry);
					Context.eHubSubscriptionTypes.AddObject(type);
					break;
				case "edit":
					ValidateTypeInput(id, name, expiry);
					type.ST_ID = id;
					type.ST_Name = name;
					type.ST_ExpiryDays = String.IsNullOrWhiteSpace(expiry) ? (int?)null : int.Parse(expiry);
					break;
				case "del":
					type.eHubSubscriptionAutoSubscribes.ToList().ForEach(a =>
					{
						Context.eHubSubscriptionAutoSubscribes.DeleteObject(a);
						AddSubscriptionAutoSubscribeLog(Request["oper"], a);
					});
					type.eHubSubscriptionLookups.ToList().ForEach(l =>
					{
						Context.eHubSubscriptionLookups.DeleteObject(l);
						AddSubscriptionLookupLog(Request["oper"], l);
					});
					type.eHubSubscriptionValues.ToList().ForEach(v =>
					{
						Context.eHubSubscriptionValues.DeleteObject(v);
						AddSubscriptionValueLog(Request["oper"], v);
					});
					Context.eHubSubscriptionTypes.DeleteObject(type);
					break;
				default:
					break;
			}
			Context.SaveChanges();
			AddSubscriptionTypeLog(Request["oper"], type);
			SaveLogs();
		}

		void ValidateTypeInput(string id, string name, string expirytxt)
		{
			if (String.IsNullOrWhiteSpace(id) || String.IsNullOrWhiteSpace(name))
				throw new ValidationException("ID and Name are required.");

			if (!String.IsNullOrWhiteSpace(expirytxt))
			{
				int expiryint;
				if (!int.TryParse(expirytxt, out expiryint) || expiryint <= 0)
					throw new ValidationException("Expiry Days must be specified as a positive integer.");
			}
		}

		public JsonResult AutoSubscribes(Guid? type)
		{
			var autos = Context.eHubSubscriptionAutoSubscribes.Where(a => a.SA_ST == type).ToList().Select(a => new
			{
				SA_PK = a.SA_PK,
				DT_Code = a.eHubMessageType.DT_Code,
				CC_ID_Recipient = a.eHubClient_Recipient != null ? a.eHubClient_Recipient.CC_ID : null,
				SA_ValueXpath = a.SA_ValueXpath,
				SA_ValueProperty = a.SA_ValueProperty,
				SA_ReferenceXpath = a.SA_ReferenceXpath,
				SA_ReferenceProperty = a.SA_ReferenceProperty
			}).OrderBy(a => a.DT_Code);
			return Json(new { eHubSubscriptionAutoSubscribes = autos.ToList() }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult AutoSubscribesEdit(Guid type)
		{
			try
			{
				var sapk = Request["SA_PK"] ?? Request["id"];
				var messageCode = Request["DT_Code"];
				var message = String.IsNullOrWhiteSpace(messageCode) ? Guid.Empty : Context.eHubMessageTypes.First(m => m.DT_Code == messageCode).DT_PK;
				var recipientID = Request["CC_ID_Recipient"];
				var recipient = String.IsNullOrWhiteSpace(recipientID) ? (Guid?)null : Context.eHubClients.First(c => c.CC_ID == recipientID).CC_PK;
				var valueXpath = Request["SA_ValueXpath"];
				var valueProp = Request["SA_ValueProperty"];
				var refXpath = Request["SA_ReferenceXpath"];
				var refProp = Request["SA_ReferenceProperty"];

				eHubSubscriptionAutoSubscribe autosub;
				if (String.IsNullOrWhiteSpace(sapk) || sapk == "_empty")
					autosub = new eHubSubscriptionAutoSubscribe();
				else
				{
					var sapkguid = Guid.Parse(sapk);
					autosub = Context.eHubSubscriptionAutoSubscribes.First(v => v.SA_PK == sapkguid);
				}

				switch (Request["oper"])
				{
					case "add":
						autosub.SA_PK = Guid.NewGuid();
						autosub.SA_DT = message;
						autosub.SA_CC_Recipient = recipient;
						autosub.SA_ValueXpath = String.IsNullOrWhiteSpace(valueXpath) ? null : valueXpath;
						autosub.SA_ValueProperty = String.IsNullOrWhiteSpace(valueProp) ? null : valueProp;
						autosub.SA_ReferenceXpath = String.IsNullOrWhiteSpace(refXpath) ? null : refXpath;
						autosub.SA_ReferenceProperty = String.IsNullOrWhiteSpace(refProp) ? null : refProp;
						autosub.SA_ST = type;
						Context.eHubSubscriptionAutoSubscribes.AddObject(autosub);
						break;
					case "edit":
						autosub.SA_DT = message;
						autosub.SA_CC_Recipient = recipient;
						autosub.SA_ValueXpath = String.IsNullOrWhiteSpace(valueXpath) ? null : valueXpath;
						autosub.SA_ValueProperty = String.IsNullOrWhiteSpace(valueProp) ? null : valueProp;
						autosub.SA_ReferenceXpath = String.IsNullOrWhiteSpace(refXpath) ? null : refXpath;
						autosub.SA_ReferenceProperty = String.IsNullOrWhiteSpace(refProp) ? null : refProp;
						break;
					case "del":
						Context.eHubSubscriptionAutoSubscribes.DeleteObject(autosub);
						break;
					default:
						break;
				}
				Context.SaveChanges();
				AddSubscriptionAutoSubscribeLog(Request["oper"], autosub);
				SaveLogs();

				return Json(new { success = true, id = autosub.SA_PK }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		public JsonResult Lookups(Guid? type)
		{
			var query = Context.eHubSubscriptionLookups.AsQueryable();

			if (type != null)
			{
				query = query.Where(l => l.SL_ST == type);
			}

			var lookups = query.Select(l => new
				{
					l.SL_PK,
					l.eHubMessageType.DT_Code,
					l.SL_ValueXpath,
					l.SL_ValueProperty
				}).OrderBy(l => l.DT_Code).ToList();

			return Json(new { eHubSubscriptionLookups = lookups }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult LookupsEdit(Guid type)
		{
			try
			{
				var slpk = Request["SL_PK"] ?? Request["id"];
				var messageCode = Request["DT_Code"];
				var message = String.IsNullOrWhiteSpace(messageCode) ? Guid.Empty : Context.eHubMessageTypes.First(m => m.DT_Code == messageCode).DT_PK;
				var valueXpath = Request["SL_ValueXpath"];
				var valueProp = Request["SL_ValueProperty"];

				eHubSubscriptionLookup lookup;
				if (String.IsNullOrWhiteSpace(slpk) || slpk == "_empty")
					lookup = new eHubSubscriptionLookup();
				else
				{
					var slpkguid = Guid.Parse(slpk);
					lookup = Context.eHubSubscriptionLookups.First(v => v.SL_PK == slpkguid);
				}

				switch (Request["oper"])
				{
					case "add":
						lookup.SL_PK = Guid.NewGuid();
						lookup.SL_DT = message;
						lookup.SL_ValueXpath = String.IsNullOrWhiteSpace(valueXpath) ? null : valueXpath;
						lookup.SL_ValueProperty = String.IsNullOrWhiteSpace(valueProp) ? null : valueProp;
						lookup.SL_ST = type;
						Context.eHubSubscriptionLookups.AddObject(lookup);
						break;
					case "edit":
						lookup.SL_DT = message;
						lookup.SL_ValueXpath = String.IsNullOrWhiteSpace(valueXpath) ? null : valueXpath;
						lookup.SL_ValueProperty = String.IsNullOrWhiteSpace(valueProp) ? null : valueProp;
						break;
					case "del":
						Context.eHubSubscriptionLookups.DeleteObject(lookup);
						break;
					default:
						break;
				}
				Context.SaveChanges();
				AddSubscriptionLookupLog(Request["oper"], lookup);
				SaveLogs();

				return Json(new { success = true, id = lookup.SL_PK }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		public JsonResult Values(Guid? type)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchQuery = Request["filters"];

			var vals = ReadOnlyContext.eHubSubscriptionValues.AsQueryable();

			if (type != null)
			{
				vals = vals.Where(v => v.SV_ST == type);
			}

			MultipleFilter filterQuery = new MultipleFilter();
			if (!string.IsNullOrEmpty(searchQuery))
			{
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);
			}

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
			{
				vals = ApplyMultipleValuesFilter(filterQuery, vals, type);
			}

			int count = vals.Count();

			vals = ApplyValuesSort(sidx, sord, vals);

			vals = vals.Skip((page - 1) * rows).Take(rows);

			var list = vals.ToList().Select(v => new
			{
				SV_PK = v.SV_PK,
				SV_ST = v.eHubSubscriptionType.ST_Name,
				SV_CC_Provider = v.eHubClient_Provider.CC_ID,
				SV_CC_Subscriber = v.eHubClient_Subscriber.CC_ID,
				SV_Value = v.SV_Value,
				SV_Reference = v.SV_Reference,
				SV_SubscribedUTC = v.SV_SubscribedUTC.ToString("s"),
				SV_ExpiryUTC = v.SV_ExpiryUTC.HasValue ? v.SV_ExpiryUTC.Value.ToString("s") : String.Empty,
				SV_ReferenceType = v.SV_ReferenceType,
				Link = CreateMessageDetailsLink(v.eHubClient_Provider.CC_ID, v.eHubClient_Subscriber.CC_ID, v.SV_SubscribedUTC)
			});

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rows),
				records = count,
				eHubSubscriptionValues = list.ToList()
			}, JsonRequestBehavior.AllowGet);
		}

		private IQueryable<eHubSubscriptionValue> ApplyMultipleValuesFilter(MultipleFilter filterQuery, IQueryable<eHubSubscriptionValue> vals, Guid? type)
		{
			switch (filterQuery.groupOp)
			{
				case "AND":
					foreach (var rule in filterQuery.rules)
						vals = ApplyValuesFilter(rule.field, rule.data, rule.op, vals);
					break;
				case "OR":
					vals = ApplyValuesFilter(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, vals);
					for (int rule = 1; rule < filterQuery.rules.Count; rule++)
					{
						IQueryable<eHubSubscriptionValue> filterData = ReadOnlyContext.eHubSubscriptionValues;
						if (type != null)
						{
							filterData = ReadOnlyContext.eHubSubscriptionValues.Where(v => v.SV_ST == type);
						}
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
				case "Subscription Type":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.eHubSubscriptionType.ST_Name == searchString);
							break;
						case "bw":
							vals = vals.Where(v => v.eHubSubscriptionType.ST_Name.StartsWith(searchString));
							break;
						case "ew":
							vals = vals.Where(v => v.eHubSubscriptionType.ST_Name.EndsWith(searchString));
							break;
						case "cn":
							vals = vals.Where(v => v.eHubSubscriptionType.ST_Name.Contains(searchString));
							break;
					}
					break;
				case "eHubClient_Provider.CC_ID":
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
				case "SV_Value":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.SV_Value == searchString);
							break;
						case "bw":
							vals = vals.Where(v => v.SV_Value.StartsWith(searchString));
							break;
						case "ew":
							vals = vals.Where(v => v.SV_Value.EndsWith(searchString));
							break;
						case "cn":
							vals = vals.Where(v => v.SV_Value.Contains(searchString));
							break;
					}
					break;
				case "SV_Reference":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.SV_Reference == searchString);
							break;
						case "bw":
							vals = vals.Where(v => v.SV_Reference.StartsWith(searchString));
							break;
						case "ew":
							vals = vals.Where(v => v.SV_Reference.EndsWith(searchString));
							break;
						case "cn":
							vals = vals.Where(v => v.SV_Reference.Contains(searchString));
							break;
					}
					break;
				case "SV_ReferenceType":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.SV_ReferenceType == searchString);
							break;
						case "bw":
							vals = vals.Where(v => v.SV_ReferenceType.StartsWith(searchString));
							break;
						case "ew":
							vals = vals.Where(v => v.SV_ReferenceType.EndsWith(searchString));
							break;
						case "cn":
							vals = vals.Where(v => v.SV_ReferenceType.Contains(searchString));
							break;
					}
					break;
				case "eHubClient_Subscribed.CC_ID":
					DateTime subscribedSearchDate = DateTime.Parse(searchString);
					subscribedSearchDate = DateTime.SpecifyKind(subscribedSearchDate, DateTimeKind.Utc);
					bool hasTimeComponentSubscribed = subscribedSearchDate.TimeOfDay.TotalSeconds > 0;
					switch (searchOper)
					{
						case "ge":
							vals = vals.Where(v => v.SV_SubscribedUTC >= subscribedSearchDate);
							break;
						case "gt":
							if (!hasTimeComponentSubscribed)
							{
								subscribedSearchDate = subscribedSearchDate.AddDays(1).AddTicks(-1);
							}
							vals = vals.Where(v => v.SV_SubscribedUTC > subscribedSearchDate);
							break;
						case "le":
							if (!hasTimeComponentSubscribed)
							{
								subscribedSearchDate = subscribedSearchDate.AddDays(1).AddTicks(-1);
							}
							vals = vals.Where(v => v.SV_SubscribedUTC <= subscribedSearchDate);
							break;
						case "lt":
							vals = vals.Where(v => v.SV_SubscribedUTC < subscribedSearchDate);
							break;
					}
					break;
				case "SV_ExpiryUTC":
					DateTime expiredSearchDate = DateTime.Parse(searchString);
					expiredSearchDate = DateTime.SpecifyKind(expiredSearchDate, DateTimeKind.Utc);
					bool hasTimeComponentExpired = expiredSearchDate.TimeOfDay.TotalSeconds > 0;
					switch (searchOper)
					{
						case "ge":
							vals = vals.Where(v => v.SV_ExpiryUTC >= expiredSearchDate);
							break;
						case "gt":
							if (!hasTimeComponentExpired)
							{
								expiredSearchDate = expiredSearchDate.AddDays(1).AddTicks(-1);
							}
							vals = vals.Where(v => v.SV_ExpiryUTC > expiredSearchDate);
							break;
						case "le":
							if (!hasTimeComponentExpired)
							{
								expiredSearchDate = expiredSearchDate.AddDays(1).AddTicks(-1);
							} 
							vals = vals.Where(v => v.SV_ExpiryUTC <= expiredSearchDate);
							break;
						case "lt":
							vals = vals.Where(v => v.SV_ExpiryUTC < expiredSearchDate);
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
				case "eHubClient_Provider.CC_ID asc":
					vals = vals.OrderBy(v => v.eHubClient_Provider.CC_ID).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.SV_Value);
					break;
				case "eHubClient_Provider.CC_ID desc":
					vals = vals.OrderByDescending(v => v.eHubClient_Provider.CC_ID).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.SV_Value);
					break;
				case "eHubClient_Subscriber.CC_ID asc":
					vals = vals.OrderBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID).ThenBy(v => v.SV_Value);
					break;
				case "eHubClient_Subscriber.CC_ID desc":
					vals = vals.OrderByDescending(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID).ThenBy(v => v.SV_Value);
					break;
				case "SV_ReferenceType asc":
					vals = vals.OrderBy(v => v.SV_ReferenceType).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID);
					break;
				case "SV_ReferenceType desc":
					vals = vals.OrderByDescending(v => v.SV_ReferenceType).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID);
					break;
				case "SV_Value asc":
					vals = vals.OrderBy(v => v.SV_Value).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID);
					break;
				case "SV_Value desc":
					vals = vals.OrderByDescending(v => v.SV_Value).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID);
					break;
				case "SV_Reference asc":
					vals = vals.OrderBy(v => v.SV_Reference).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID);
					break;
				case "SV_Reference desc":
					vals = vals.OrderByDescending(v => v.SV_Reference).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID);
					break;
				case "eHubClient_Subscribed.CC_ID asc":
					vals = vals.OrderBy(v => v.SV_SubscribedUTC);
					break;
				case "eHubClient_Subscribed.CC_ID desc":
					vals = vals.OrderByDescending(v => v.SV_SubscribedUTC);
					break;
				case "SV_ExpiryUTC asc":
					vals = vals.OrderBy(v => v.SV_ExpiryUTC);
					break;
				case "SV_ExpiryUTC desc":
					vals = vals.OrderByDescending(v => v.SV_ExpiryUTC);
					break;
				default:
					vals = vals.OrderBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.eHubClient_Provider.CC_ID).ThenBy(v => v.SV_Value);
					break;
			}
			return vals;
		}

		[HttpPost]
		public JsonResult ValuesEdit(Guid type)
		{
			try
			{
				var svpk = Request["SV_PK"] ?? Request["id"];
				var providerID = Request["SV_CC_Provider"];
				var provider = Context.eHubClients.FirstOrDefault(c => c.CC_ID == providerID);
				var subscriberID = Request["SV_CC_Subscriber"];
				var subscriber = Context.eHubClients.FirstOrDefault(c => c.CC_ID == subscriberID);
				var subscriptionType = Context.eHubSubscriptionTypes.First(t => t.ST_PK == type);

				string reference = string.Empty;
				if (subscriptionType.ST_ID.Equals("GBCCNS", StringComparison.CurrentCultureIgnoreCase) || subscriptionType.ST_ID.Equals("GBCMCP", StringComparison.CurrentCultureIgnoreCase))
				{
					reference = Request.Unvalidated["SV_Reference"];
				}
				else
				{
					reference = Request["SV_Reference"];
				}
				var value = Request["SV_Value"];
				var referenceType = Request["SV_ReferenceType"];
				var expiryDays = subscriptionType.ST_ExpiryDays;

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
						val.SV_ST = type;
						val.SV_CC_Sender = provider.CC_PK;
						val.SV_CC_Recipient = subscriber.CC_PK;
						val.SV_Value = value;
						val.SV_Reference = String.IsNullOrWhiteSpace(reference) ? null : reference;
						val.SV_ReferenceType = String.IsNullOrWhiteSpace(referenceType) ? null : referenceType;
						val.SV_SubscribedUTC = DateTime.UtcNow;
						val.SV_ExpiryUTC = expiryDays.HasValue ? DateTime.UtcNow.AddDays(expiryDays.Value) : (DateTime?)null;

						var preLazyLoadingEnabled = Context.LazyLoadingEnabled;
						Context.LazyLoadingEnabled = false;
						Context.eHubSubscriptionValues.AddObject(val);
						Context.LazyLoadingEnabled = preLazyLoadingEnabled;
						break;
					case "edit":
						val.SV_CC_Sender = provider.CC_PK;
						val.SV_CC_Recipient = subscriber.CC_PK;
						val.SV_Value = value;
						val.SV_Reference = String.IsNullOrWhiteSpace(reference) ? null : reference;
						val.SV_ReferenceType = String.IsNullOrWhiteSpace(referenceType) ? null : referenceType;
						val.SV_SubscribedUTC = DateTime.UtcNow;
						val.SV_ExpiryUTC = expiryDays.HasValue ? DateTime.UtcNow.AddDays(expiryDays.Value) : (DateTime?)null;
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
		public FileContentResult ValuesExportCsv()
		{
			var type = new Guid(Request["typePK"]);
			using (var wrt = new StringWriter())
			using (var csv = new CsvHelper.CsvWriter(wrt))
			{
				new List<string> { "Provider", "Subscriber", "Reference Type", "Value", "Reference", "Subscribed Time" }.ForEach(f => csv.WriteField(f));
				csv.NextRecord();
				Context.eHubSubscriptionValues.Where(v => v.SV_ST == type).OrderBy(v => v.eHubClient_Provider.CC_ID).ThenBy(v => v.eHubClient_Subscriber.CC_ID).ThenBy(v => v.SV_Value)
					.ToList().ForEach(v =>
					{
						v.SV_SubscribedUTC = DateTime.SpecifyKind(v.SV_SubscribedUTC, DateTimeKind.Utc);
						new List<string> { v.eHubClient_Provider.CC_ID, v.eHubClient_Subscriber.CC_ID, v.SV_ReferenceType, v.SV_Value, v.SV_Reference, v.SV_SubscribedUTC.ToString("O")}.ForEach(f => csv.WriteField(f));
						csv.NextRecord();
					});
				var subtype = Context.eHubSubscriptionTypes.First(t => t.ST_PK == type);
				string filename = String.Format("{0}_{1}.csv", subtype.ST_ID, subtype.ST_Name.Replace(" ", ""));
				return File(Encoding.Default.GetBytes(wrt.ToString()), "text/text", filename);
			}
		}

		internal string CreateMessageDetailsLink(string provider, string subscriber, DateTime subscribedUTC)
		{
			var from = subscribedUTC.AddSeconds(-10).ToString("yyyyMMddHHmmss");
			var to = subscribedUTC.AddSeconds(10).ToString("yyyyMMddHHmmss");
			
			var adminWebsite = ConfigurationManager.AppSettings["AdminWebsite"];
			return 
				$"{adminWebsite}/Messages?Role=Specific&DateFilterType=WithinDates&DateRangeType=UTC&IsIncludingArchiveStaging=False&SenderInbox={subscriber}&RecipientOutbox={provider}&From={from}&To={to}";

		}


		[HttpPost]
		public void ValuesImportCsv()
		{
			var typePK = new Guid(Request["typePK"]);
			var type = Context.eHubSubscriptionTypes.First(t => t.ST_PK == typePK);
			var option = Request["option"];
			var values = new List<eHubSubscriptionValue>();

			using (var rdr = new StreamReader(Request.Files["uploadFile"].InputStream))
			using (var csv = new CsvHelper.CsvParser(rdr))
			{
				var fields = csv.Read();
				while ((fields = csv.Read()) != null)
				{
					var senderID = fields[0];
					var recipientID = fields[1];
					values.Add(new eHubSubscriptionValue
					{
						SV_CC_Sender = Context.eHubClients.First(c => c.CC_ID == senderID).CC_PK,
						SV_CC_Recipient = Context.eHubClients.First(c => c.CC_ID == recipientID).CC_PK,
						SV_ReferenceType = String.IsNullOrWhiteSpace(fields[2]) ? null : fields[2],
						SV_Value = fields[3],
						SV_Reference = String.IsNullOrWhiteSpace(fields[4]) ? null : fields[4],
						SV_ST = type.ST_PK,
						SV_SubscribedUTC = DateTime.UtcNow,
						SV_ExpiryUTC = type.ST_ExpiryDays.HasValue ? DateTime.UtcNow.AddDays(type.ST_ExpiryDays.Value) : (DateTime?)null
					});
				}
			}

			var oldvals = Context.eHubSubscriptionValues.Where(v => v.SV_ST == type.ST_PK).ToList();

			oldvals.Join(values,
				o => new { o.SV_CC_Sender, o.SV_CC_Recipient, o.SV_Value, o.SV_ReferenceType, o.SV_Reference },
				n => new { n.SV_CC_Sender, n.SV_CC_Recipient, n.SV_Value, n.SV_ReferenceType, n.SV_Reference },
				(o, n) => new { o, n }
			).Where(j => j.o.SV_Reference != j.n.SV_Reference || j.o.SV_ReferenceType != j.n.SV_ReferenceType).ToList().ForEach(j =>
			{
				j.o.SV_Reference = j.n.SV_Reference;
				j.o.SV_ReferenceType = j.n.SV_ReferenceType;
				j.o.SV_SubscribedUTC = j.n.SV_SubscribedUTC;
				j.o.SV_ExpiryUTC = j.o.SV_ExpiryUTC;
			});

			values.Except(oldvals,
				new LambdaComparer<eHubSubscriptionValue>((n, o) =>
					n.SV_CC_Sender == o.SV_CC_Sender
					&& n.SV_CC_Recipient == o.SV_CC_Recipient
					&& n.SV_Value == o.SV_Value
					&& n.SV_ReferenceType == o.SV_ReferenceType
					&& n.SV_Reference == o.SV_Reference
				)).ToList().ForEach(v =>
			{
				v.SV_PK = Guid.NewGuid();
				var preLazyLoadingEnabled = Context.LazyLoadingEnabled;
				Context.LazyLoadingEnabled = false;
				Context.eHubSubscriptionValues.AddObject(v);
				Context.LazyLoadingEnabled = preLazyLoadingEnabled;
				AddSubscriptionValueLog("add", v);
			});


			if (option == "replace")
			{
				oldvals.Except(values,
					new LambdaComparer<eHubSubscriptionValue>((n, o) =>
						n.SV_CC_Sender == o.SV_CC_Sender
						&& n.SV_CC_Recipient == o.SV_CC_Recipient
						&& n.SV_Value == o.SV_Value
						&& n.SV_ReferenceType == o.SV_ReferenceType
						&& n.SV_Reference == o.SV_Reference
				)).ToList().ForEach(v =>
				{
					Context.eHubSubscriptionValues.DeleteObject(v);
					AddSubscriptionValueLog("del", v);
				});
			}

			Context.SaveChanges();
			SaveLogs();
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
				clients = Context.eHubClients.Select(c => new
				{
					c.CC_ID,
					c.CC_FriendlyName
				});
			}
			else
			{
				clients = from c in Context.eHubClients
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

		public JsonResult MessageTypes()
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);
			string filter;

			if (filtered)
				filter = String.Format("DT_Code.Contains(\"{0}\")", Request["DT_Code"]);
			else
				filter = "1 == 1";

			var msgs = Context.eHubMessageTypes.Select(m => new
			{
				DT_Code = m.DT_Code,
			}).Where(filter).OrderBy(sidx + " " + sord);

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)msgs.Count() / (double)rows),
				records = msgs.Count(),
				eHubMessageTypes = msgs.Skip((page - 1) * rows).Take(rows).ToList()
			}, JsonRequestBehavior.AllowGet);
		}
	}
}
