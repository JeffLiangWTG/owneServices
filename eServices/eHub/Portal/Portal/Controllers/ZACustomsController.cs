using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Portal.Controllers
{
	public class ZACustomsController : ControllerBase
	{
		public ILog logger = LogManager.GetLogger("ZACustomsLogger");
		protected List<string> listLogs = new List<string>();
		public const string ZACustomsCode = "ZAC";
		public readonly Guid ZACEHubCodeSetPK = new Guid("ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736");

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult Registrations()
		{
			int page = Convert.ToInt32(Request["page"]);
			int rowNo = Convert.ToInt32(Request["rows"]);
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			string searchQuery = Request["filters"];

			MultipleFilter filterQuery = new MultipleFilter();

			if (!string.IsNullOrEmpty(searchQuery))
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);
			var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_ID == ZACustomsCode);
			var partialRegistrations = Context.eHubClientRegistrations.Where(r => r.CX_RT == regTypeInstance.RT_PK);
			partialRegistrations = PartialColumnsSelect(partialRegistrations, filterQuery);

			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
				partialRegistrations = ApplyMultipleValuesFilter(filterQuery, partialRegistrations);

			var regos = Context.eHubClientRegistrations
				.Join(partialRegistrations,
						r => r.CX_PK,
						pr => pr.CX_PK,
						(r, pr) => r)
				.Include(r => r.eHubClient);

			int count = regos.Count();

			regos = ApplyValuesSort(sidx, sord, regos);

			regos = regos.Skip((page - 1) * rowNo).Take(rowNo);

			var rows = LeftJoinCodeMapping(regos);

			return Json(new
			{
				page = page,
				total = Math.Ceiling((double)count / (double)rowNo),
				records = count,
				eHubClientRegistrations = rows
			}, JsonRequestBehavior.AllowGet);
		}

		private IQueryable<eHubClientRegistration> PartialColumnsSelect(IQueryable<eHubClientRegistration> registrations, MultipleFilter filterQuery)
		{
			registrations = registrations
						.Select(registration => new eHubClientRegistrationDTO
						{
							CX_PK = registration.CX_PK,
							CX_RT = registration.CX_RT,
							eHubClient = registration.eHubClient,
							CX_Code = registration.CX_Code,
							CX_Attr1 = registration.CX_Attr1,
							CX_Qualifier = registration.CX_Qualifier,
						});

			return registrations;
		}

		private IQueryable<eHubClientRegistration> ApplyMultipleValuesFilter(MultipleFilter filterQuery, IQueryable<eHubClientRegistration> registrations)
		{
			switch (filterQuery.groupOp)
			{
				case "AND":

					foreach (var rule in filterQuery.rules)
						registrations = ApplyValuesFilter(rule.field, rule.data, rule.op, registrations);
					break;

				case "OR":
					var origin = registrations;
					registrations = ApplyValuesFilter(filterQuery.rules[0].field, filterQuery.rules[0].data, filterQuery.rules[0].op, registrations);
					for (int rule = 1; rule < filterQuery.rules.Count; rule++)
					{
						IQueryable<eHubClientRegistration> filterData = origin;
						filterData = ApplyValuesFilter(filterQuery.rules[rule].field, filterQuery.rules[rule].data, filterQuery.rules[rule].op, filterData);
						registrations = registrations.AsQueryable().Union(filterData).Distinct(new RegoComparer());
					}
					break;

				default:
					break;
			}

			return registrations;
		}

		private class RegoComparer : IEqualityComparer<eHubClientRegistration>
		{
			public bool Equals(eHubClientRegistration a, eHubClientRegistration b) => a.CX_PK.Equals(b.CX_PK);
			public int GetHashCode(eHubClientRegistration a) => a.CX_PK.GetHashCode();
		}

		protected void ZACustomseHubLog(string oper, eHubClientRegistration rego, eHubCodeMapKey key)
		{
			listLogs.Add($"[{oper}] eHubClientRegistration: CX_PK={rego.CX_PK}, CX_Qualifier={rego.CX_Qualifier}, CX_Code={rego.CX_Code}, CX_Attr1={rego.CX_Attr1}, CX_Password1={rego.CX_Password1}, CX_Flag1={rego.CX_Flag1}, CX_Flag2={rego.CX_Flag2}" +
				$", CX_CC={rego.CX_CC}, CX_RT={rego.CX_RT}");

			listLogs.Add($"[{oper}] eHubCodeMapKey: CK_PK={key.CK_PK}, CK_Order={key.CK_Order}, CK_Key1Value={key.CK_Key1Value}, CK_Key2Value={key.CK_Key2Value}, CK_Key3Value={key.CK_Key3Value}, CK_Key4Value={key.CK_Key4Value}, CK_Key5Value={key.CK_Key5Value}");

			if (key.eHubCodeMapValues != null)
			{
				foreach (var codeMapValue in key.eHubCodeMapValues)
				{
					listLogs.Add($"[{oper}] eHubCodeMapValue: CV_CK={codeMapValue.CV_CK}, CR_Name={codeMapValue.eHubCodeSetResult?.CR_Name ?? ""}, CV_OutputCode={codeMapValue.CV_OutputCode}, CV_CR={codeMapValue.CV_CR}, CV_PassThroughKey={codeMapValue.CV_PassThroughKey}");
				}
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

		static IQueryable<eHubClientRegistration> ApplyValuesFilter(string searchField, string searchString, string searchOper, IQueryable<eHubClientRegistration> regos)
		{
			switch (searchField)
			{
				case "CX_CC_ID":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.eHubClient.CC_ID == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.eHubClient.CC_ID.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.eHubClient.CC_ID.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.eHubClient.CC_ID.Contains(searchString));
							break;
					}
					break;
				case "CX_Code":
					switch (searchOper)
					{
						case "eq":
							regos = regos.Where(r => r.CX_Code == searchString);
							break;
						case "bw":
							regos = regos.Where(r => r.CX_Code.StartsWith(searchString));
							break;
						case "ew":
							regos = regos.Where(r => r.CX_Code.EndsWith(searchString));
							break;
						case "cn":
							regos = regos.Where(r => r.CX_Code.Contains(searchString));
							break;
					}
					break;
			}
			return regos;
		}

		static IQueryable<eHubClientRegistration> ApplyValuesSort(string sidx, string sord, IQueryable<eHubClientRegistration> regos)
		{
			switch (sidx + " " + sord)
			{
				case "CX_CC_ID asc":
					regos = regos.OrderBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Code);
					break;
				case "CX_CC_ID desc":
					regos = regos.OrderByDescending(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Code);
					break;
				case "CX_Code asc":
					regos = regos.OrderBy(r => r.CX_Code).ThenBy(r => r.eHubClient.CC_ID);
					break;
				case "CX_Code desc":
					regos = regos.OrderByDescending(r => r.CX_Code).ThenBy(r => r.eHubClient.CC_ID);
					break;
				default:
					regos = regos.OrderBy(r => r.eHubClient.CC_ID).ThenBy(r => r.CX_Code);
					break;
			}
			return regos;
		}

		Dictionary<Guid, string> codesetResultNames;
		Dictionary<Guid, string> CodesetResultNames { get { return codesetResultNames ?? (codesetResultNames = Context.eHubCodeSetResults.Where(x => x.CR_CS == ZACEHubCodeSetPK).ToDictionary(x => x.CR_PK, x => x.CR_Name)); } }

		Dictionary<string, Guid> codesetResultPKs;
		Dictionary<string, Guid> CodesetResultPKs { get { return codesetResultPKs ?? (codesetResultPKs = Context.eHubCodeSetResults.Where(x => x.CR_CS == ZACEHubCodeSetPK).ToDictionary(x => x.CR_Name, x => x.CR_PK)); } }

		List<object> LeftJoinCodeMapping(IQueryable<eHubClientRegistration> regos)
		{
			List<object> rows = new List<object>();

			foreach (var map in regos)
			{
				var codeMapKey = Context.eHubCodeMapKeys.FirstOrDefault(x => x.CK_CS == ZACEHubCodeSetPK && x.CK_Key1Value == map.CX_Code);

				Dictionary<string, string> m = new Dictionary<string, string>();
				m.Add("CX_PK", map.CX_PK.ToString());
				m.Add("CX_CC_ID", map.eHubClient.CC_ID);
				m.Add("CX_Code", map.CX_Code);
				if (codeMapKey != null)
				{
					foreach (var eHubCodeMapValue in codeMapKey.eHubCodeMapValues)
					{
						m.Add(CodesetResultNames[eHubCodeMapValue.CV_CR], eHubCodeMapValue.CV_OutputCode);
					}
				}
				rows.Add(m);
			}
			return rows;
		}

		[HttpPost]
		public JsonResult RegistrationEdit()
		{
			try
			{
				var regTypeInstance = Context.eHubRegistrationTypes.First(r => r.RT_ID == ZACustomsCode);
				var oper = Request["oper"];
				var cxpk = Request["CX_PK"] ?? Request["id"];
				var clientID = Request["CX_CC_ID"];
				var code = Request["CX_Code"];
				var senderID = Request["SenderID"];
				var senderSubID = Request["SenderSubID"];
				var tradingPartnerID = Request["TradingPartnerID"];
				var AACID = Request["AACID"];

				eHubClientRegistration rego;
				eHubCodeMapKey key;
				Dictionary<string, eHubCodeMapValue> values = new Dictionary<string, eHubCodeMapValue>();

				Guid cxpkguid;
				Guid.TryParse(cxpk, out cxpkguid);

				var existingCode = Context.eHubClientRegistrations.FirstOrDefault(x => x.CX_Code == code && (cxpkguid == Guid.Empty || x.CX_PK != cxpkguid));
				if (existingCode != null)
					throw new ArgumentException(string.Format("\"{0}\" Agent Id already exists. Please choose a different Agent Id.", code));

				if (String.IsNullOrWhiteSpace(cxpk) || cxpk == "_empty")
				{
					rego = new eHubClientRegistration();
					key = new eHubCodeMapKey();
				}
				else
				{
					rego = regTypeInstance.eHubClientRegistrations.First(r => r.CX_PK == cxpkguid);
					key = Context.eHubCodeMapKeys.FirstOrDefault(x => x.CK_CS == ZACEHubCodeSetPK && x.CK_Key1Value == rego.CX_Code);
					if (key != null)
					{
						var codeMapValues = key.eHubCodeMapValues;
						values.Add("SenderID", codeMapValues.First(x => x.CV_CR == CodesetResultPKs["SenderID"]));
						values.Add("SenderSubID", codeMapValues.First(x => x.CV_CR == CodesetResultPKs["SenderSubID"]));
						values.Add("TradingPartnerID", codeMapValues.First(x => x.CV_CR == CodesetResultPKs["TradingPartnerID"]));
						values.Add("AACID", codeMapValues.First(x => x.CV_CR == CodesetResultPKs["AACID"]));
					}
				}

				switch (oper)
				{
					case "add":
						rego.CX_PK = Guid.NewGuid();
						rego.eHubRegistrationType = regTypeInstance;
						rego.eHubClient = Context.eHubClients.First(c => c.CC_ID == clientID);
						rego.CX_Code = code;
						rego.CX_Qualifier = code;
						Context.eHubClientRegistrations.AddObject(rego);
						var maxCK_Order = Context.eHubCodeMapKeys.Where(x => x.CK_CS == ZACEHubCodeSetPK).Max(y => y.CK_Order);
						var defaultFallbackKey = Context.eHubCodeMapKeys.First(x => x.CK_CS == ZACEHubCodeSetPK && x.CK_Order == maxCK_Order);
						key.CK_PK = Guid.NewGuid();
						key.CK_CS = ZACEHubCodeSetPK;
						key.CK_Order = defaultFallbackKey.CK_Order;
						key.CK_Key1Value = code;
						defaultFallbackKey.CK_Order++;
						Context.eHubCodeMapKeys.AddObject(key);
						key.eHubCodeMapValues.Add(new eHubCodeMapValue() { CV_CK = key.CK_PK, CV_CR = CodesetResultPKs["SenderID"], CV_OutputCode = senderID });
						key.eHubCodeMapValues.Add(new eHubCodeMapValue() { CV_CK = key.CK_PK, CV_CR = CodesetResultPKs["SenderSubID"], CV_OutputCode = senderSubID });
						key.eHubCodeMapValues.Add(new eHubCodeMapValue() { CV_CK = key.CK_PK, CV_CR = CodesetResultPKs["TradingPartnerID"], CV_OutputCode = tradingPartnerID });
						key.eHubCodeMapValues.Add(new eHubCodeMapValue() { CV_CK = key.CK_PK, CV_CR = CodesetResultPKs["AACID"], CV_OutputCode = AACID });
						break;
					case "edit":
						listLogs.Add($"[{oper}] eHubClient (Previous Values)");
						ZACustomseHubLog(oper, rego, key);

						rego.eHubClient = Context.eHubClients.First(c => c.CC_ID == clientID);
						rego.CX_Code = code;
						rego.CX_Qualifier = code;
						if (key != null)
						{
							key.CK_Key1Value = code;
							values["SenderID"].CV_OutputCode = senderID;
							values["SenderSubID"].CV_OutputCode = senderSubID;
							values["TradingPartnerID"].CV_OutputCode = tradingPartnerID;
							values["AACID"].CV_OutputCode = AACID;
						}

						listLogs.Add($"[{oper}] eHubClient (New Values)");
						break;
					case "del":
						Context.eHubClientRegistrations.DeleteObject(rego);
						if (key != null)
							Context.eHubCodeMapKeys.DeleteObject(key);
						break;
					default:
						break;
				}
				Context.SaveChanges();

				ZACustomseHubLog(oper, rego, key);
				SaveLogs();

				return Json(new { success = true, id = rego.CX_PK }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				var message = ex.InnerException != null ? ex.Message + " " + ex.InnerException.Message : ex.Message;
				return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
			}
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
		private class eHubClientRegistrationDTO : eHubClientRegistration
		{
			public override eHubClient eHubClient { get; set; }
		}
	}
}
