using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Portal.Controllers
{
	public class CodeMappingController : ControllerBase
	{
		const char wildcard = '*';
		const string blankFormat = "#BLANK#";
		const string passThroughFormat_Out = "#INPUTFIELD{0}#";
		const string passThroughFormat_In = "^#INPUTFIELD(?<key>[1-5])#$";
		public ILog logger = LogManager.GetLogger("CodeMappingLogger");
		protected List<string> tempLogs = new List<string>();

		public ActionResult Index()
		{
			return View();
		}

		public JsonResult Senders()
		{
			var senders = from t in Context.eHubTransformationSets
						  select new
						  {
							  CC_PK = t.TS_CC_Sender ?? (Guid?)Guid.Empty,
							  CC_ID = t.TS_CC_Sender.HasValue ? t.eHubClient_Sender.CC_ID : "*",
							  CC_FriendlyName = t.TS_CC_Sender.HasValue ? t.eHubClient_Sender.CC_FriendlyName : "Multiple senders"
						  };

			return Json(new { eHubClients = senders.Distinct().OrderBy(c => c.CC_ID).ToList() }, JsonRequestBehavior.AllowGet);
		}

		public JsonResult Recipients(Guid sender)
		{
			var recipients = from t in Context.eHubTransformationSets
							 where (t.TS_CC_Sender ?? (Guid?)Guid.Empty) == sender
							 select new
							 {
								 CC_PK = t.TS_CC_Recipient ?? (Guid?)Guid.Empty,
								 CC_ID = t.TS_CC_Recipient.HasValue ? t.eHubClient_Recipient.CC_ID : "*",
								 CC_FriendlyName = t.TS_CC_Recipient.HasValue ? t.eHubClient_Recipient.CC_FriendlyName : "Multiple Recipients"
							 };

			return Json(new { eHubClients = recipients.Distinct().OrderBy(c => c.CC_ID).ToList() }, JsonRequestBehavior.AllowGet);
		}

		public JsonResult TransformationSets(Guid sender, Guid recipient)
		{
			var transformationSets = from t in Context.eHubTransformationSets
									 where (t.TS_CC_Sender ?? (Guid?)Guid.Empty) == sender
										&& (t.TS_CC_Recipient ?? (Guid?)Guid.Empty) == recipient
									 orderby t.TS_Name
									 select new
									 {
										 t.TS_PK,
										 t.TS_Name
									 };

			return Json(new { eHubTransformationSets = transformationSets.ToList() }, JsonRequestBehavior.AllowGet);
		}

		public JsonResult CodeSets(Guid sender, Guid recipient, Guid? transformation)
		{
			var codesets = from cs in Context.eHubCodeSets
						   where (transformation == null && cs.CS_CC_Sender == sender && cs.CS_CC_Recipient == recipient & cs.CS_TS == null)
							  || (transformation != null && cs.CS_TS == transformation)
						   select new
						   {
							   cs.CS_PK,
							   cs.CS_Name,
						   };

			return Json(new { eHubCodeSets = codesets.OrderBy(cs => cs.CS_Name).ToList() }, JsonRequestBehavior.AllowGet);
		}

		public JsonResult CodeSetDetails(Guid codeset)
		{
			var codesetObj = Context.eHubCodeSets.First(c => c.CS_PK == codeset);
			string[] codesetKeys = new string[] { codesetObj.CS_Key1Name, codesetObj.CS_Key2Name, codesetObj.CS_Key3Name, codesetObj.CS_Key4Name, codesetObj.CS_Key5Name };

			var result = new
			{
				codesetName = codesetObj.CS_Name,
				codesetKeys = codesetKeys.Where(k => k != null).Select((k, i) => new { id = i, pos = i + 1, keyName = k }).ToList(),
				codesetResults = codesetObj.eHubCodeSetResults.OrderBy(r => r.CR_Order).Select((r, i) => new { id = i, key = r.CR_PK, resultName = r.CR_Name }).ToList(),
			};

			return Json(result, JsonRequestBehavior.AllowGet);
		}

		protected void AddCodeSetLog(string oper, eHubCodeSet codeset)
		{
			tempLogs.Add($"[{oper}] eHubCodeSet: CS_PK={codeset.CS_PK}, CS_Name={codeset.CS_Name}, CS_TS={codeset.CS_TS}, CS_CC_Sender={codeset.CS_CC_Sender}, CS_CC_Recipient={codeset.CS_CC_Recipient}" +
				$", CS_Key1Name={codeset.CS_Key1Name}, CS_Key2Name={codeset.CS_Key2Name}, CS_Key3Name={codeset.CS_Key3Name}, CS_Key4Name={codeset.CS_Key4Name}, CS_Key5Name={codeset.CS_Key5Name}");
		}

		protected void AddCodeMapKeyLog(string oper, eHubCodeMapKey codemapKey)
		{
			tempLogs.Add($"[{oper}] eHubCodeMapKey: CK_PK={codemapKey.CK_PK}, CK_CS={codemapKey.CK_CS}, CK_Order={codemapKey.CK_Order}" +
				$", CK_Key1Value={codemapKey.CK_Key1Value}, CK_Key2Value={codemapKey.CK_Key2Value}, CK_Key3Value={codemapKey.CK_Key3Value}, CK_Key4Value={codemapKey.CK_Key4Value}, CK_Key5Value={codemapKey.CK_Key5Value}");
		}

		protected void AddCodeSetResultLog(string oper, eHubCodeSetResult codesetResult)
		{
			tempLogs.Add($"[{oper}] eHubCodeSetResult: CR_PK={codesetResult.CR_PK}, CR_CS={codesetResult.CR_CS}, CR_Order={codesetResult.CR_Order}, CR_Name={codesetResult.CR_Name}");
		}

		protected void AddCodeMapValueLog(string oper, eHubCodeMapValue codemapValue)
		{
			tempLogs.Add($"[{oper}] eHubCodeMapValue: CV_CK={codemapValue.CV_CK}, CV_CR={codemapValue.CV_CR}, CV_OutputCode={codemapValue.CV_OutputCode}, CV_PassThroughKey={codemapValue.CV_PassThroughKey}");
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
		public JsonResult SaveCodeSet()
		{
			eHubCodeSet codeset;

			var codesetKeys = (from k in JObject.Parse(Request.Form["codesetKeys"])["codesetKeys"]
							   select new List<object>
							   {
								   String.IsNullOrWhiteSpace(k.Value<string>("pos")) ? null as int? : Int32.Parse(k.Value<string>("pos")),
								   k.Value<string>("keyName")
							   }).ToList();
			Guid resultPK;
			var codesetResults = (from r in JObject.Parse(Request.Form["codesetResults"])["codesetResults"]
								  select new List<object>
								  {
									  Guid.TryParse(r.Value<string>("key"), out resultPK) ? resultPK : null as Guid?,
									  r.Value<string>("resultName")
								  }).ToList();

			if (Request.Form["action"] == "add")
			{
				Guid senderPK = new Guid(Request.Form["sender"]);
				Guid recipientPK = new Guid(Request.Form["recipient"]);
				codeset = new eHubCodeSet
				{
					CS_PK = Guid.NewGuid(),
					CS_CC_Sender = senderPK == Guid.Empty ? recipientPK : senderPK,
					CS_CC_Recipient = recipientPK == Guid.Empty ? senderPK : recipientPK,
					CS_TS = Guid.Parse(Request.Form["transformation"]),
					CS_Name = Request.Form["codesetName"],
				};
				Context.eHubCodeSets.AddObject(codeset);
				UpdateNewCodeSet(codeset, codesetKeys, codesetResults);
				Context.SaveChanges();
				AddCodeSetLog("add", codeset);
				SaveLogs();
			}
			else
			{
				try
				{
					using (TransactionScope trans = new TransactionScope(TransactionScopeOption.Required))
					{
						Guid codesetPK = Guid.Parse(Request.Form["codeset"]);
						codeset = Context.eHubCodeSets.First(cs => cs.CS_PK == codesetPK);
						codeset.CS_Name = Request.Form["codesetName"];
						UpdateExistingCodeSetKeys(codeset, codesetKeys);
						UpdateExistingCodeSetResults(codeset, codesetResults);
						Context.SaveChanges();
						AddCodeSetLog(Request.Form["action"], codeset);
						SaveLogs();
						var data = GetCodeMapData(codesetPK);
						var keys = GetCompositeKeys(data, 0);
						var duplicates = reduceToDuplicates(keys);
						if (duplicates.Any())
						{
							var exceptionMessage = new StringBuilder("Specified key field deletions cannot be applied because it would result in duplicate rows for existing maps.<br>");
							foreach (var dup in duplicates)
							{
								exceptionMessage.Append($"{string.Join(",", dup.Key)} x {dup.Count()}<br>");
							}
							throw new Exception(exceptionMessage.ToString());
						}
						trans.Complete();
					}
				}
				catch (Exception ex)
				{
					return Json(new { exception = ex.Message }, JsonRequestBehavior.AllowGet);
				}
			}

			return Json(new { codeset = codeset.CS_PK }, JsonRequestBehavior.AllowGet);
		}

		void UpdateNewCodeSet(eHubCodeSet codeset, List<List<object>> codesetKeys, List<List<object>> codesetResults)
		{
			var keys = (from k in codesetKeys select new { originalPosition = (int?)k[0], keyName = (string)k[1] }).ToList();
			var results = (from r in codesetResults select new { resultPK = (Guid?)r[0], resultName = (string)r[1] }).ToList();

			codeset.CS_Key1Name = keys.Count > 0 ? keys[0].keyName : null;
			codeset.CS_Key2Name = keys.Count > 1 ? keys[1].keyName : null;
			codeset.CS_Key3Name = keys.Count > 2 ? keys[2].keyName : null;
			codeset.CS_Key4Name = keys.Count > 3 ? keys[3].keyName : null;
			codeset.CS_Key5Name = keys.Count > 4 ? keys[4].keyName : null;

			eHubCodeMapKey codemapKey = new eHubCodeMapKey
			{
				CK_PK = Guid.NewGuid(),
				CK_CS = codeset.CS_PK,
				CK_Order = 1,
				CK_Key1Value = keys.Count > 0 ? "%" : null,
				CK_Key2Value = keys.Count > 1 ? "%" : null,
				CK_Key3Value = keys.Count > 2 ? "%" : null,
				CK_Key4Value = keys.Count > 3 ? "%" : null,
				CK_Key5Value = keys.Count > 4 ? "%" : null,
			};
			Context.eHubCodeMapKeys.AddObject(codemapKey);
			AddCodeMapKeyLog("add", codemapKey);

			for (int i = 0; i < results.Count; i++)
			{
				eHubCodeSetResult codesetResult = new eHubCodeSetResult
				{
					CR_PK = Guid.NewGuid(),
					CR_CS = codeset.CS_PK,
					CR_Name = results[i].resultName,
					CR_Order = i + 1,
				};
				Context.eHubCodeSetResults.AddObject(codesetResult);
				AddCodeSetResultLog("add", codesetResult);

				eHubCodeMapValue codemapValue = new eHubCodeMapValue
				{
					CV_CK = codemapKey.CK_PK,
					CV_CR = codesetResult.CR_PK,
					CV_OutputCode = string.Empty,
					CV_PassThroughKey = null,
				};
				Context.eHubCodeMapValues.AddObject(codemapValue);
				AddCodeMapValueLog("add", codemapValue);
			}
		}

		void UpdateExistingCodeSetKeys(eHubCodeSet codeset, List<List<object>> codesetKeys)
		{
			var keys = (from k in codesetKeys select new { originalPosition = (int?)k[0], keyName = (string)k[1] }).ToList();

			string[] oldKeyNames = new string[] { codeset.CS_Key1Name, codeset.CS_Key2Name, codeset.CS_Key3Name, codeset.CS_Key4Name, codeset.CS_Key5Name };
			string[] newKeyNames = new string[5];

			bool changeMapKeys = keys.Select((k, i) => new { oldPos = k.originalPosition, newPos = i + 1 }).Any(p => p.newPos != p.oldPos)
									|| oldKeyNames.Count(o => o != null) > codesetKeys.Count;

			codeset.CS_Key1Name = keys.Count > 0 ? keys[0].keyName : null;
			codeset.CS_Key2Name = keys.Count > 1 ? keys[1].keyName : null;
			codeset.CS_Key3Name = keys.Count > 2 ? keys[2].keyName : null;
			codeset.CS_Key4Name = keys.Count > 3 ? keys[3].keyName : null;
			codeset.CS_Key5Name = keys.Count > 4 ? keys[4].keyName : null;

			if (changeMapKeys)
			{
				foreach (var mapKey in codeset.eHubCodeMapKeys)
				{
					string[] oldKeyValues = new string[] { mapKey.CK_Key1Value, mapKey.CK_Key2Value, mapKey.CK_Key3Value, mapKey.CK_Key4Value, mapKey.CK_Key5Value };
					string[] newKeyValues = new string[5];
					bool defaultKey = (mapKey.CK_Order == codeset.eHubCodeMapKeys.Count);
					for (int i = 0; i < 5; i++)
					{
						if (i >= codesetKeys.Count)
						{
							newKeyValues[i] = null;
						}
						else
						{
							if (defaultKey)
							{
								newKeyValues[i] = "%";
							}
							else
							{
								if (keys[i].originalPosition.HasValue)
								{
									newKeyValues[i] = oldKeyValues[keys[i].originalPosition.Value - 1];
								}
								else
								{
									newKeyValues[i] = "%";
								}
							}
						}
					}
					mapKey.CK_Key1Value = newKeyValues[0];
					mapKey.CK_Key2Value = newKeyValues[1];
					mapKey.CK_Key3Value = newKeyValues[2];
					mapKey.CK_Key4Value = newKeyValues[3];
					mapKey.CK_Key5Value = newKeyValues[4];
					AddCodeMapKeyLog("edit", mapKey);
				}
			}
		}

		void UpdateExistingCodeSetResults(eHubCodeSet codeset, List<List<object>> codesetResults)
		{
			var results = (from r in codesetResults select new { resultPK = (Guid?)r[0], resultName = (string)r[1] }).ToList();

			foreach (var result in codeset.eHubCodeSetResults.ToList())
			{
				var newResult = results.FirstOrDefault(r => r.resultPK == result.CR_PK);
				if (newResult == null)
				{
					foreach (var mapvalue in result.eHubCodeMapValues.ToList())
					{
						Context.eHubCodeMapValues.DeleteObject(mapvalue);
						AddCodeMapValueLog("del", mapvalue);
					}
					Context.eHubCodeSetResults.DeleteObject(result);
					AddCodeSetResultLog("del", result);
				}
				else
				{
					result.CR_Name = newResult.resultName;
					result.CR_Order = results.IndexOf(newResult) + 1;
					AddCodeSetResultLog("edit", result);
				}
			}

			foreach (var result in results.Where(r => r.resultPK == null))
			{
				eHubCodeSetResult newResult = new eHubCodeSetResult()
				{
					CR_PK = Guid.NewGuid(),
					CR_CS = codeset.CS_PK,
					CR_Order = results.IndexOf(result) + 1,
					CR_Name = result.resultName,
				};
				Context.eHubCodeSetResults.AddObject(newResult);
				AddCodeSetResultLog("add", newResult);
				foreach (var key in codeset.eHubCodeMapKeys)
				{
					eHubCodeMapValue value = new eHubCodeMapValue()
					{
						CV_CK = key.CK_PK,
						CV_CR = newResult.CR_PK,
						CV_OutputCode = String.Empty,
					};
					Context.eHubCodeMapValues.AddObject(value);
					AddCodeMapValueLog("add", value);
				}
			}
		}

		[HttpPost]
		public void DeleteCodeSet(Guid codeset)
		{
			var cv = (from v in Context.eHubCodeMapValues
					  where v.eHubCodeMapKey.CK_CS == codeset
					  select v).ToList();
			foreach (var val in cv)
			{
				Context.eHubCodeMapValues.DeleteObject(val);
				AddCodeMapValueLog("del", val);
			}

			var ck = (from k in Context.eHubCodeMapKeys
					  where k.CK_CS == codeset
					  select k).ToList();
			foreach (var key in ck)
			{
				Context.eHubCodeMapKeys.DeleteObject(key);
				AddCodeMapKeyLog("del", key);
			}

			var cr = (from r in Context.eHubCodeSetResults
					  where r.CR_CS == codeset
					  select r).ToList();
			foreach (var result in cr)
			{
				Context.eHubCodeSetResults.DeleteObject(result);
				AddCodeSetResultLog("del", result);
			}

			var cs = (from c in Context.eHubCodeSets
					  where c.CS_PK == codeset
					  select c).First();
			Context.eHubCodeSets.DeleteObject(cs);
			AddCodeSetLog("del", cs);

			Context.SaveChanges();
			SaveLogs();
		}

		[HttpPost]
		public JsonResult AssignCodeSet(Guid codeset, Guid? transformation, string name)
		{
			bool success = false;
			bool duplicate = false;
			string error = String.Empty;

			try
			{
				var cur = (from c in Context.eHubCodeSets
						   where c.CS_PK == codeset
						   select c).First();

				if (transformation == null &&
					(cur.CS_CC_Sender != cur.eHubTransformationSet.TS_CC_Sender ||
					 cur.CS_CC_Recipient != cur.eHubTransformationSet.TS_CC_Recipient))
				{
					error = "Cannot create unassigned code sets for test clients. These clients are assumed to be test clients becuase they are allocated to this interface for the selected code set but the code set is configured to be referenced in the actual transformation processes with different client codes than these.";
				}
				else
				{
					var dup = from c in Context.eHubCodeSets
							  where c.CS_Name == (name == String.Empty ? cur.CS_Name : name)
								 && ((transformation == null && c.CS_TS == null)
									 || (c.CS_TS == transformation))
								 && c.CS_CC_Sender == cur.CS_CC_Sender
								 && c.CS_CC_Recipient == cur.CS_CC_Recipient
							  select c.CS_PK;

					if (dup.Count() > 0)
					{
						duplicate = true;
					}
					else
					{
						if (name != String.Empty)
							cur.CS_Name = name;
						cur.CS_TS = transformation;
						Context.SaveChanges();
						AddCodeSetLog("edit", cur);
						SaveLogs();
						success = true;
					}
				}
			}
			catch (Exception ex)
			{
				error = "A system error has occurred. Please contact support.";
				LogError(ex.ToString());
			}

			return Json(new
			{
				success = success,
				duplicate = duplicate,
				error = error
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult CopyCodeSet(Guid codeset, Guid? transformation, string name)
		{
			bool success = false;
			bool duplicate = false;
			string error = String.Empty;

			try
			{
				var cur = (from c in Context.eHubCodeSets
						   where c.CS_PK == codeset
						   select c).First();

				if (transformation == null &&
					(cur.CS_CC_Sender != cur.eHubTransformationSet.TS_CC_Sender ||
					 cur.CS_CC_Recipient != cur.eHubTransformationSet.TS_CC_Recipient))
				{
					error = "Cannot create unassigned code sets for test clients. These clients are assumed to be test clients because they are allocated to this interface for the selected code set but the code set is configured to be referenced in the actual transformation processes with different client codes than these.";
				}
				else
				{
					var dup = from c in Context.eHubCodeSets
							  where c.CS_Name == (name == String.Empty ? cur.CS_Name : name)
								 && ((transformation == null && c.CS_TS == null)
									 || (c.CS_TS == transformation))
								 && c.CS_CC_Sender == cur.CS_CC_Sender
								 && c.CS_CC_Recipient == cur.CS_CC_Recipient
							  select c.CS_PK;

					if (dup.Count() > 0)
					{
						duplicate = true;
					}
					else
					{
						var newCs = new eHubCodeSet
						{
							CS_PK = Guid.NewGuid(),
							CS_Name = (name == String.Empty) ? cur.CS_Name : name,
							CS_TS = transformation,
							CS_CC_Sender = cur.CS_CC_Sender,
							CS_CC_Recipient = cur.CS_CC_Recipient,
							CS_Key1Name = cur.CS_Key1Name,
							CS_Key2Name = cur.CS_Key2Name,
							CS_Key3Name = cur.CS_Key3Name,
							CS_Key4Name = cur.CS_Key4Name,
							CS_Key5Name = cur.CS_Key5Name,
						};
						Context.eHubCodeSets.AddObject(newCs);
						AddCodeSetLog("add", newCs);

						foreach (var cr in cur.eHubCodeSetResults)
						{
							var newCr = new eHubCodeSetResult
							{
								CR_PK = Guid.NewGuid(),
								CR_CS = newCs.CS_PK,
								CR_Order = cr.CR_Order,
								CR_Name = cr.CR_Name,
							};
							Context.eHubCodeSetResults.AddObject(newCr);
							newCs.eHubCodeSetResults.Add(newCr);
							AddCodeSetResultLog("add", newCr);
						}
						Context.SaveChanges();
						SaveLogs();

						foreach (var ck in cur.eHubCodeMapKeys)
						{
							var newCk = new eHubCodeMapKey
							{
								CK_PK = Guid.NewGuid(),
								CK_CS = newCs.CS_PK,
								CK_Order = ck.CK_Order,
								CK_Key1Value = ck.CK_Key1Value,
								CK_Key2Value = ck.CK_Key2Value,
								CK_Key3Value = ck.CK_Key3Value,
								CK_Key4Value = ck.CK_Key4Value,
								CK_Key5Value = ck.CK_Key5Value,
							};
							Context.eHubCodeMapKeys.AddObject(newCk);
							newCs.eHubCodeMapKeys.Add(newCk);
							AddCodeMapKeyLog("add", newCk);

							foreach (var cv in ck.eHubCodeMapValues)
							{
								var newCv = new eHubCodeMapValue
								{
									CV_CK = newCk.CK_PK,
									CV_CR = newCs.eHubCodeSetResults.First(r => r.CR_Name == cv.eHubCodeSetResult.CR_Name).CR_PK,
									CV_OutputCode = cv.CV_OutputCode,
									CV_PassThroughKey = cv.CV_PassThroughKey,
								};
								Context.eHubCodeMapValues.AddObject(newCv);
								newCk.eHubCodeMapValues.Add(newCv);
								AddCodeMapValueLog("add", newCv);
							}
						}

						Context.SaveChanges();
						SaveLogs();
						success = true;
					}
				}
			}
			catch
			{
				success = false;
				error = "A system error has occurred. Please contact support.";
				//LogError(ex.ToString());
			}

			return Json(new
			{
				success = success,
				duplicate = duplicate,
				error = error
			},
			JsonRequestBehavior.AllowGet);
		}

		public JsonResult CodeMaps(Guid codeset)
		{
			var keyNames = GetCodeSetKeyNames(codeset);
			var results = GetCodeSetResultNames(codeset);
			var names = keyNames.Concat(results).ToList();
			var data = GetCodeMapData(codeset);
			var keys = GetCompositeKeys(data, 0);
			var duplicates = reduceToDuplicates(keys);
			var hasDefaultRow = HasDefaultRow(codeset);
			List<object> rows = new List<object>();

			foreach (var map in data.Select((e, i) => new { values = e, id = i }))
			{
				Dictionary<string, string> m = new Dictionary<string, string>();
				m.Add("id", map.id.ToString());
				for (int i = 0; i < map.values.Count; i++)
				{
					m.Add(names[i], map.values[i]);
				}
				rows.Add(m);
			}

			return Json(new
			{
				keys = keyNames,
				results = results,
				rows = rows,
				duplicates = duplicates,
				hasDefaultRow
			}
			, JsonRequestBehavior.AllowGet);
		}

		List<string> GetCodeSetKeyNames(Guid codeset)
		{
			var cs = (from s in Context.eHubCodeSets
					  where s.CS_PK == codeset
					  select s).First();

			List<string> keys = new List<string>();
			if (cs.CS_Key1Name != null)
			{
				keys.Add(cs.CS_Key1Name);
				if (cs.CS_Key2Name != null)
				{
					keys.Add(cs.CS_Key2Name);
					if (cs.CS_Key3Name != null)
					{
						keys.Add(cs.CS_Key3Name);
						if (cs.CS_Key4Name != null)
						{
							keys.Add(cs.CS_Key4Name);
							if (cs.CS_Key5Name != null)
							{
								keys.Add(cs.CS_Key5Name);
							}
						}
					}
				}
			}
			return keys;
		}

		List<string> GetCodeSetResultNames(Guid codeset)
		{
			return (from r in Context.eHubCodeSetResults
					where r.CR_CS == codeset
					orderby r.CR_Order
					select r.CR_Name).ToList();
		}

		bool HasDefaultRow(Guid codeset)
		{
			return (from k in Context.eHubCodeMapKeys
					where k.CK_CS == codeset && (k.CK_Key1Value == "%" || k.CK_Key1Value == null)
											 && (k.CK_Key2Value == "%" || k.CK_Key2Value == null)
											 && (k.CK_Key3Value == "%" || k.CK_Key3Value == null)
											 && (k.CK_Key4Value == "%" || k.CK_Key4Value == null)
											 && (k.CK_Key5Value == "%" || k.CK_Key5Value == null)
					select k.CK_PK).Any();
		}

		List<List<string>> GetCodeMapData(Guid codeset)
		{
			var cv = from k in Context.eHubCodeMapKeys
					 join v in Context.eHubCodeMapValues on k.CK_PK equals v.CV_CK
					 join r in Context.eHubCodeSetResults on v.CV_CR equals r.CR_PK
					 where k.CK_CS == codeset
					 group new
					 {
						 r.CR_Name,
						 r.CR_Order,
						 Code = v.CV_OutputCode,
						 PassThrough = v.CV_PassThroughKey
					 }
					 by new
					 {
						 k.CK_Order,
						 k.CK_Key1Value,
						 k.CK_Key2Value,
						 k.CK_Key3Value,
						 k.CK_Key4Value,
						 k.CK_Key5Value,
					 } into m
					 orderby m.Key.CK_Order
					 select new
					 {
						 Key1 = m.Key.CK_Key1Value,
						 Key2 = m.Key.CK_Key2Value,
						 Key3 = m.Key.CK_Key3Value,
						 Key4 = m.Key.CK_Key4Value,
						 Key5 = m.Key.CK_Key5Value,
						 Results = m.OrderBy(r => r.CR_Order)
					 };

			List<List<string>> data = new List<List<string>>();
			foreach (var v in cv)
			{
				List<string> m = (new string[] { v.Key1, v.Key2, v.Key3, v.Key4, v.Key5 })
					.TakeWhile(k => k != null)
					.Select(k => k == String.Empty ? blankFormat : k.Replace('%', wildcard))
					.ToList();
				m.AddRange(v.Results.Select(r => r.Code == String.Empty ? blankFormat : r.Code ?? String.Format(passThroughFormat_Out, r.PassThrough)));
				data.Add(m);
			}

			return data;
		}

		public FileContentResult DownloadCsv(Guid codeset)
		{
			// Build the CSV data
			StringBuilder csv = new StringBuilder();

			List<string> headers = GetCodeSetKeyNames(codeset);
			headers.AddRange(GetCodeSetResultNames(codeset));
			for (int i = 0; i < headers.Count; i++)
			{
				if (i > 0)
				{
					csv.Append(',');
				}
				AppendCsvField(csv, headers[i]);
			}
			csv.AppendLine();

			var data = GetCodeMapData(codeset);

			foreach (var map in data)
			{
				for (int i = 0; i < map.Count; i++)
				{
					if (i > 0)
					{
						csv.Append(',');
					}
					AppendCsvField(csv, map[i]);
				}
				csv.AppendLine();
			}

			string filename = (from cs in Context.eHubCodeSets
							   where cs.CS_PK == codeset
							   select cs.eHubClient_Sender.CC_FriendlyName + " - " +
										cs.eHubClient_Recipient.CC_FriendlyName + " - " +
										(cs.eHubTransformationSet == null ? "Default" : cs.eHubTransformationSet.TS_Name) + " - " +
										cs.CS_Name + ".csv").First();

			return File(Encoding.Default.GetBytes(csv.ToString()), "text/text", filename);
		}

		void AppendCsvField(StringBuilder sb, string field)
		{
			if (field.IndexOfAny(new char[] { '"', ',' }) >= 0)
				sb.Append('"').Append(field).Append('"');
			else
				sb.Append(field);
		}

		class CodeMapKeyModel
		{
			int Length;
			public string[] Keys;
			public CodeMapKeyModel(List<string> key, int len)
			{
				Length = len;
				Keys = new string[len];
				for (var i = 0; i < len; i++)
				{
					Keys[i] = key[i];
				}
			}

			public override bool Equals(object obj)
			{
				var compare = obj as CodeMapKeyModel;
				if (compare == null || Length != compare.Length)
					return false;
				for (int i = 0; i < Length; i++)
					if (Keys[i] != compare.Keys[i])
					{
						return false;
					}
				return true;
			}

			public override int GetHashCode()
			{
				var hash = 0;
				for (var i = 0; i < Length; i++)
					hash ^= Keys[i].GetHashCode();
				return hash;
			}
		}

		List<List<string>> DelimitedDuplicate(List<List<string>> csv, int codeMapKeyLength)
		{
			var groups = csv.GroupBy(i => new CodeMapKeyModel(i, codeMapKeyLength));
			var result = new List<List<string>>();
			foreach (var group in groups)
			{
				if (!group.Any())
				{
					continue;
				}

				if (group.Count() == 1)
				{
					result.Add(group.First());
				}
				else
				{
					List<string> delimitedResult = group.Key.Keys.ToList();
					// setup StringBuilder for Delimited Values
					List<StringBuilder> delimitedValues = new List<StringBuilder>();
					for (int i = codeMapKeyLength; i < group.First().Count; i++)
					{
						delimitedValues.Add(new StringBuilder());
					}

					// combine all value
					foreach (var oneRecord in group)
					{
						for (int i = codeMapKeyLength; i < oneRecord.Count; i++)
						{
							delimitedValues[i - codeMapKeyLength].Append($"|{oneRecord[i]}");
						}
					}

					//add to result
					delimitedResult.AddRange(delimitedValues.Select(t => t.ToString(1, t.Length - 1)));
					result.Add(delimitedResult);
				}
			}
			return result;
		}

		IEnumerable<IGrouping<IEnumerable<string>, IEnumerable<string>>> reduceToDuplicates(IEnumerable<IEnumerable<string>> e)
		{
			return e.GroupBy(i => i, new LambdaComparer<IEnumerable<string>>((a, b) => a.SequenceEqual(b))).Where(g => g.Count() > 1);
		}

		[HttpPost]
		public JsonResult SaveCodeMaps()
		{
			Guid codesetPK = new Guid(Request.Form["codeset"]);
			var keys = GetCodeSetKeyNames(codesetPK);
			var results = (from r in Context.eHubCodeSetResults
						   where r.CR_CS == codesetPK
						   orderby r.CR_Order
						   select r.CR_PK).ToList();
			var nullArgs = new string[5];
			var rawMaps = JsonConvert.DeserializeObject<List<List<string>>>(Request.Form["codeMapsData"]);
			var rawMapKeys = GetCompositeKeys(rawMaps, 0);
			var newMaps = (from m in rawMaps
						   select m.Select(c => c == blankFormat ? String.Empty : c.Replace(wildcard, '%'))
						  )
						  .Select(m => new
						  {
							  keys = nullArgs.Zip(m.Take(keys.Count).Concat(nullArgs), (a, b) => b ?? a).ToArray(),
							  values = m.Skip(keys.Count).ToArray()
						  }
						  ).ToList();
			Regex passThroughFormat = new Regex(passThroughFormat_In);
			try
			{
				using (TransactionScope trans = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 10, 0)))
				{
					// Clear old key/values that are not included in the new dataset
					Context.eHubCodeMapKeys.Where(k => k.CK_CS == codesetPK).ToList().ForEach(k =>
					{
						if (!newMaps.Exists(m => m.keys[0] == k.CK_Key1Value && m.keys[1] == k.CK_Key2Value &&
							m.keys[2] == k.CK_Key3Value && m.keys[3] == k.CK_Key4Value && m.keys[4] == k.CK_Key5Value))
						{
							k.eHubCodeMapValues.ToList().ForEach(v =>
							{
								Context.eHubCodeMapValues.DeleteObject(v);
								AddCodeMapValueLog("del", v);
							});
							Context.eHubCodeMapKeys.DeleteObject(k);
							AddCodeMapKeyLog("del", k);
						}
					});
					Context.SaveChanges();

					// Clear and insert new key/values
					int i = 1;
					var mapKeys = Context.eHubCodeMapKeys.Where(k => k.CK_CS == codesetPK).ToList();
					foreach (var csKeyString in newMaps.Select(cs => $"{cs.keys[0]}{cs.keys[1]}{cs.keys[2]}{cs.keys[3]}{cs.keys[4]}").Distinct())
					{
						var keysToDelete = new List<eHubCodeMapKey>();
						var keysToAdd = new List<eHubCodeMapKey>();
						var valuesToDelete = new List<eHubCodeMapValue>();
						var valuesToAdd = new List<eHubCodeMapValue>();

						foreach (var keyToDelete in mapKeys.Where(k => csKeyString == ($"{k.CK_Key1Value}{k.CK_Key2Value}{k.CK_Key3Value}{k.CK_Key4Value}{k.CK_Key5Value}")))
						{
							valuesToDelete.AddRange(keyToDelete.eHubCodeMapValues);
							keysToDelete.Add(keyToDelete);
						}

						foreach (var m in newMaps.Where(cs => csKeyString == ($"{cs.keys[0]}{cs.keys[1]}{cs.keys[2]}{cs.keys[3]}{cs.keys[4]}")))
						{
							Guid ckpk = Guid.NewGuid();
							var newCk = new eHubCodeMapKey
							{
								CK_PK = ckpk,
								CK_CS = codesetPK,
								CK_Order = (m.keys[0] == "%" || m.keys[0] == null)
											&& (m.keys[1] == "%" || m.keys[1] == null)
											&& (m.keys[2] == "%" || m.keys[2] == null)
											&& (m.keys[3] == "%" || m.keys[3] == null)
											&& (m.keys[4] == "%" || m.keys[4] == null) ? newMaps.Count : i++,
								CK_Key1Value = m.keys[0],
								CK_Key2Value = m.keys[1],
								CK_Key3Value = m.keys[2],
								CK_Key4Value = m.keys[3],
								CK_Key5Value = m.keys[4],
							};
							keysToAdd.Add(newCk);

							m.values.Zip(results, (v, r) => new { v, r }).ToList().ForEach(vr =>
							{
								Match match = passThroughFormat.Match(vr.v);
								var newCv = new eHubCodeMapValue
								{
									CV_CK = ckpk,
									CV_CR = vr.r,
									CV_OutputCode = match.Success ? null : vr.v,
									CV_PassThroughKey = match.Success ? Int32.Parse(match.Groups["key"].Value) as int? : null
								};
								valuesToAdd.Add(newCv);

							});
						}

						if (CodeMappingHelper.ShouldUpdateDatabase(keysToDelete, keysToAdd, valuesToDelete, valuesToAdd))
						{
							valuesToDelete.ForEach(v =>
							{
								Context.eHubCodeMapValues.DeleteObject(v);
								AddCodeMapValueLog("del", v);
							});
							keysToDelete.ForEach(k =>
							{
								Context.eHubCodeMapKeys.DeleteObject(k);
								AddCodeMapKeyLog("del", k);
							});
							keysToAdd.ForEach(k =>
							{
								Context.eHubCodeMapKeys.AddObject(k);
								AddCodeMapKeyLog("add", k);
							});
							valuesToAdd.ForEach(v =>
							{
								Context.eHubCodeMapValues.AddObject(v);
								AddCodeMapValueLog("add", v);
							});
						}
					}
					Context.SaveChanges();
					trans.Complete();
					SaveLogs();
				}
				var duplicates = reduceToDuplicates(rawMapKeys);
				return Json(new
				{
					duplicates = duplicates
				}, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				if (ex.GetType().Name == "UpdateException" && ex.InnerException != null)
				{
					return Json(new
					{
						exception = "Save Failed.<br>" + ex.GetType() + ":<br>" + ex.InnerException.Message
					}, JsonRequestBehavior.AllowGet);
				}
				return Json(new
				{
					exception = "Save Failed.<br>" + ex.GetType() + ":<br>" + ex.Message
				}, JsonRequestBehavior.AllowGet);
			}
		}

		[HttpPost]
		public void ImportCsvFile()
		{
			try
			{
				Guid codesetPK = new Guid(Request.Form["codeset"]);
				bool merge = (Request.Form["merge"] == "merge");
				List<List<string>> csv = ParseCsvData(Request.Files["uploadFile"].InputStream);
				//var csvCopied = GetCompositeKeys(csv, 1);
				var keys = GetCodeSetKeyNames(codesetPK);
				var results = GetCodeSetResultNames(codesetPK);
				var names = keys.Concat(results).ToList();
				List<List<string>> output = new List<List<string>>();
				List<object> rows = new List<object>();
				bool newDefault = false;
				var warning = string.Empty;
				// Check that header labels match code set fields
				if (!csv[0].SequenceEqual(names, StringComparer.InvariantCultureIgnoreCase))
				{
					throw new Exception("File cannot be imported because the header row does not match the code set field names.");
				}

				// Check that data exists.
				if (csv.Count < 2)
				{
					throw new Exception("File does not contain any data to import.");
				}

				// Remove header row
				csv.RemoveAt(0);

				for (int i = 0; i < csv.Count; i++)
				{
					for (int j = 0; j < csv[i].Count; j++)
					{
						if (csv[i][j] == String.Empty)
						{
							csv[i][j] = blankFormat;
						}
					}
				}

				if (keys.Count == 0)
				{
					if (csv.Count > 1)
					{
						throw new ApplicationException("File cannot be imported because it contains multiple rows but this output-only code set can only have one.");
					}
					output = csv;
				}
				else
				{
					// Check that rows contain correct number of keys.
					if (csv.Min(r => r.Count) < keys.Count)
					{
						throw new ArgumentException("File cannot be imported because one or more rows do not contain enough values.");
					}

					// Check for duplicates
					var delimitedCsv = DelimitedDuplicate(csv, keys.Count);

					if (delimitedCsv.Count < csv.Count)
					{
						csv = delimitedCsv;
						warning = "Duplicated composite keys are imported. Those records will be merged into a single row with '|' delimiter. Please make sure that the interface supports delimiter or contact eServices support! Are you sure to make this change?";
					}

					// Check that default is last if present.
					int defaultPos = csv.IndexOf(csv.FirstOrDefault(r => r.Take(keys.Count).All(k => k == wildcard.ToString())));
					if (defaultPos >= 0)
					{
						newDefault = true;
						if (defaultPos != csv.Count - 1)
						{
							throw new ArgumentException("File cannot be imported because default row is present but is not last.");
						}
					}

					// Create the new maps for output
					if (!merge)
					{
						if (defaultPos == -1)
						{
							throw new ArgumentException("File cannot be imported because default row is not present but is required for override.");
						}
						output = csv;
					}
					else
					{
						var old = JsonConvert.DeserializeObject<List<List<string>>>(Request.Form["codeMapsData"]);

						var oldUnchanged = old
							.Except(
								csv,
								new LambdaComparer<List<string>>(
									(a, b) => a.Take(keys.Count)
										.SequenceEqual(
											b.Take(keys.Count)
										)
								)
							);

						if (newDefault)
						{
							output.AddRange(csv.Take(csv.Count - 1));
							output.AddRange(oldUnchanged);
							output.Add(csv.Last());
						}
						else
						{
							output = csv;
							output.AddRange(oldUnchanged);
						}
					}
				}

				// Convert rows to dictionaries
				foreach (var row in output)
				{
					rows.Add(FormatRowData(names, row));
				}

				// Serialize output as JSON text.
				Response.Write(JsonConvert.SerializeObject(new
				{
					keys = keys,
					results = results,
					rows = rows,
					warning = warning
				}, Formatting.None));
			}
			catch (ArgumentException ex)
			{
				Response.Write(ex.Message);
				return;
			}
		}

		private static List<List<string>> GetCompositeKeys(List<List<string>> dataList, int startIndex)
		{
			var compositeKeys = new List<List<string>>();
			for (var index = startIndex; index < dataList.Count; index++)
			{
				var compositeKeyItem = new List<string>(dataList[index]);
				compositeKeys.Add(compositeKeyItem);
			}
			foreach (var rowList in compositeKeys)
			{
				rowList.RemoveAt(rowList.Count - 1);
			}
			return compositeKeys;
		}

		Dictionary<string, string> FormatRowData(List<string> names, List<string> row)
		{
			Dictionary<string, string> m = new Dictionary<string, string>();

			for (int i = 0; i < row.Count && i < names.Count; i++)
			{
				m.Add(names[i], row[i]);
			}

			for (int i = row.Count; i < names.Count; i++)
			{
				m.Add(names[i], String.Empty);
			}

			return m;
		}

		public List<List<string>> ParseCsvData(string csv)
		{
			return ParseCsvData(new MemoryStream(Encoding.Default.GetBytes(csv)));
		}

		public List<List<string>> ParseCsvData(Stream csv)
		{
			List<List<string>> table = new List<List<string>>();

			StringBuilder field = new StringBuilder();
			Regex endQuote = new Regex(@"^(?<extra>\""\s*)(,|$)");

			using (StreamReader rdr = new StreamReader(csv))
			{
				while (!rdr.EndOfStream)
				{
					List<string> row = new List<string>();
					bool quoted = false;
					string line = rdr.ReadLine();

					for (int i = 0; i <= line.Length; i++)
					{
						if (i == line.Length)
						{
							row.Add(String.Empty);
							break;
						}

						field.Clear();

						if (quoted = line[i] == '"')
							i++;

						for (; i < line.Length; i++)
						{
							if (line[i] == (quoted ? '"' : ','))
							{
								if (quoted)
								{
									Match match = endQuote.Match(line.Substring(i));
									if (match.Success)
									{
										i += match.Groups["extra"].Length;
										break;
									}
								}
								else
									break;
							}

							field.Append(line[i]);
						}
						row.Add(field.ToString().Trim());
					}

					table.Add(row);
				}
			}

			return table;
		}
	}
}
