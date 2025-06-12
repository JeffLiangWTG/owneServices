extern alias DataModelShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CargoWise.eHub.Core.Logging.LoggerExtensions;
using Common.Logging;
using DataModelShared::System.Linq.Dynamic;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Newtonsoft.Json;

namespace CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging
{
	public abstract class AbstractOceanCarrierMessagingType : IOceanCarrierMessagingType
	{
		public abstract string PrefixName { get; }
		public abstract string Name { get; }
		public abstract string ID { get; }
		public abstract string MatchingGroupRuleName { get; }

		private readonly Controller controller;
		private readonly eHubTransactionsContext context;

		protected const int START_ID = 1;
		protected const string DEFAULTVALUE_ANY = "[ANY]";
		protected const string DEFAULT_ID = "default";
		protected const string NVOCC = "NVOCC";
		protected const string VOCC = "VOCC";
		protected const string REJECT_SERVICE_PROVIDER = "Reject";
		protected const string ExperssionFormatString = "[{0},{1},{2}]";
		internal static ILog CsvLogger = LogManager.GetLogger("OCMCsvLogger");
        internal static ILog OCMLogger = LogManager.GetLogger<IOceanCarrierMessagingType>();

        [Flags]
		public enum ExpressionOptions
		{
			None = 0,
			HandleEqual = 1,
			HandleWildcard = 2,
			HandleGroup = 4
		}

		protected static readonly IList<PartyToCopy> PartiesToCopy = new List<PartyToCopy>()
		{
			new PartyToCopy{ partyToCopy = "CarrierHandlingAgent" },
			new PartyToCopy{ partyToCopy = "CarrierBookingAgent" }
		};

		protected static readonly IList<DocumentName> DocNames = new List<DocumentName>()
		{
			new DocumentName{docName = DEFAULTVALUE_ANY},
			new DocumentName{docName = "Booking Request"},
			new DocumentName{docName = "eManifest"},
			new DocumentName{docName = "Shipping Instruction"},
			new DocumentName{docName = "Shipping Order"},
			new DocumentName{docName = "Verified Gross Container Weight"},
		};

		protected static readonly IList<SplitByOptions> SplitOptions = new List<SplitByOptions>()
		{
			new SplitByOptions{splitOption = "SubShipment", resultValue = "OCM_SUBSHIPMENT_SPLIT"},
			new SplitByOptions{splitOption = "Container",  resultValue = "OCM_CONTAINER_SPLIT"}
		};

		protected static readonly IList<ShipmentType> ShipmentTypes = new List<ShipmentType>()
		{
			new ShipmentType{shpType = DEFAULTVALUE_ANY , description = DEFAULTVALUE_ANY},
			new ShipmentType{shpType = NVOCC, description = "For Co-Loaders"},
			new ShipmentType{shpType = VOCC, description = "For Ocean Carriers"},
		};

		public HttpRequestBase Request { get { return controller.Request; } }
		public HttpResponseBase Response { get { return controller.Response; } }
		protected HttpSessionStateBase Session { get { return controller.Session; } }
        protected string StaffID { get { return controller.HttpContext.User.Identity.Name; } }
        protected eHubTransactionsContext Context { get { return context; } }

		protected AbstractOceanCarrierMessagingType(Controller controller, eHubTransactionsContext context)
		{
			this.controller = controller;
			this.context = context;
        }

        public abstract string[] JqGridColumns();
        internal abstract List<Row> ValuesDatabase();
		protected abstract void AddValues(LinkedList<Row> existingData);
		protected abstract void EditValues(LinkedList<Row> existingData);
		internal abstract void SaveToDatabase(LinkedList<Row> data);
		protected abstract List<dynamic> GetJsonObject(IEnumerable<Row> vals);
		protected abstract List<Row> FilterValues(IEnumerable<Row> vals);
		protected abstract List<Row> SearchValues(string searchField, string searchString, string searchOperation, IQueryable<Row> vals);
		protected abstract List<string> GetExportCsvColumns(Row row);
		protected abstract LinkedList<Row> ImportCsvCore();
		protected abstract void ValidateCsvHeaders(string[] headers);
		protected abstract void ValidateCsvDataAndReplaceWithValidData(LinkedList<Row> data);

		protected virtual Group GetGroupRule(Rule shippingInstructionRule)
		{
			return GetGroupRule(shippingInstructionRule, MatchingGroupRuleName);
		}

		protected Group GetGroupRule(Rule shippingInstructionRule, string groupName)
		{
			OCMLogger.Info(() => $"Finding group rules for [{groupName}]");
			Group groupRules = shippingInstructionRule.FindGroupRule(groupName);
			OCMLogger.Info(() => $"Successfully found group rules for [{groupName}]");
			return groupRules;
		}

		protected void AddExpression(List<string> expressionList, string expression)
		{
			if (!string.IsNullOrEmpty(expression)) expressionList.Add(expression);
		}

		#region public method

		public virtual string GetColumnName(string column)
		{
			if (column == Row.RowNames.id) return "id";
			if (column == Row.RowNames.client) return "Client ID";
			if (column == Row.RowNames.carrier) return "Carrier SCAC/C1C";
			if (column == Row.RowNames.carrierAgent) return "Carrier Agent";
			if (column == Row.RowNames.port) return "Port/Country";
			if (column == Row.RowNames.shpType) return "Shipment Type";
			if (column == Row.RowNames.docName) return "Document Name";
			if (column == Row.RowNames.provider) return "Service Provider";
			if (column == Row.RowNames.purpose) return "Purpose";
			if (column == Row.RowNames.splitBy) return "Split By";
			if (column == Row.RowNames.shipNamespace) return "UShipment NameSpace";
			if (column == Row.RowNames.partyToCopy) return "Party To Copy";
			if (column == Row.RowNames.eventBranch) return "Event Branch";
			return "";
		}

		protected virtual ExpressionOptions GetExpressionOption(string columnName)
		{
			if (columnName == Row.RowNames.client) return ExpressionOptions.HandleEqual | ExpressionOptions.HandleWildcard;
			if (columnName == Row.RowNames.carrier) return ExpressionOptions.HandleEqual | ExpressionOptions.HandleGroup | ExpressionOptions.HandleWildcard;
			if (columnName == Row.RowNames.carrierAgent) return ExpressionOptions.HandleEqual;
			if (columnName == Row.RowNames.port) return ExpressionOptions.HandleEqual | ExpressionOptions.HandleGroup | ExpressionOptions.HandleWildcard;
			if (columnName == Row.RowNames.shpType) return ExpressionOptions.HandleEqual;
			if (columnName == Row.RowNames.docName) return ExpressionOptions.HandleEqual;
			if (columnName == Row.RowNames.provider) return ExpressionOptions.HandleEqual;
			if (columnName == Row.RowNames.purpose) return ExpressionOptions.HandleEqual;
			if (columnName == Row.RowNames.splitBy) return ExpressionOptions.HandleEqual;
			if (columnName == Row.RowNames.shipNamespace) return ExpressionOptions.HandleEqual;
			if (columnName == Row.RowNames.partyToCopy) return ExpressionOptions.HandleEqual;
			if (columnName == Row.RowNames.eventBranch)
				return ExpressionOptions.HandleEqual;
			return ExpressionOptions.None;
		}

		protected virtual string[] MandatoryColumns()
		{
			return null;
		}

		public object ValuesEdit()
		{
			try
			{
				var cache = GetOceanMessagingCache();
				switch (Request["oper"])
				{
					case "add":
						ValidateMandatoryFields();
						ValidateExpressionValues();
						AddValues(cache);
						break;
					case "edit":
						ValidateMandatoryFields();
						ValidateExpressionValues();
						EditValues(cache);
						break;
					case "del":
						DeleteValues(cache);
						break;
				}
				CurrentSessionCache = cache;
				return new { success = true, id = Request["id"] };
			}
			catch (Exception ex)
			{
				return new {success = false, message = ex.Message};
			}
		}


		protected void DeleteValues(LinkedList<Row> existingData)
		{
			var id = Request["id"];
			var row = existingData.Single(x => x.id.Equals(id));
			existingData.Remove(row);
		}

		protected void AddRow(LinkedList<Row> existingData, string afterRowId, Row row)
		{
			LinkedListNode<Row> afterRow = existingData.Find(existingData.FirstOrDefault(x => x.id.Equals(afterRowId)));

			if (afterRow != null)
			{
				existingData.AddBefore(afterRow, row);
			}
			else
			{
				existingData.AddLast(row);
			}
		}

		public object Save()
		{
			return new {message = SaveCore()};
		}

		public object GetClients(bool includeNonProd)
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]) - 1; // Because we add [ANY] row in every page, the row count = [ANY] row + other rows. (15 = 1*[ANY] + 14*Other) => rows = 14
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			IEnumerable<dynamic> clients;
			if(includeNonProd)
			{
				clients = context.eHubClients.Select(c => new
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
				if (!string.IsNullOrWhiteSpace(ccid))
					clients = clients.Where(c => c.CC_ID.StartsWith(ccid, StringComparison.OrdinalIgnoreCase));
				if (!string.IsNullOrWhiteSpace(ccname))
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

			var list = clients.Skip((page - 1) * rows).Take(rows).ToList();
			list.Insert(0, new { CC_ID = DEFAULTVALUE_ANY, CC_FriendlyName = DEFAULTVALUE_ANY });

			return new
			{
				page = page,
				total = Math.Ceiling(count / (double)rows),
				records = count,
				eHubClients = list
			};
		}

		public object GetServiceProviders()
		{
			int page = Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			string selectedID = Request["selectedID"];
			if (selectedID == DEFAULT_ID) rows--; // Because we add REJECT row in every page, the row count = REJECT row + other rows. (15 = 1*[ANY] + 14*Other) => rows = 14
			string sidx = Request["sidx"];
			string sord = Request["sord"];
			bool filtered = Boolean.Parse(Request["_search"]);

			var service = context.eHubClients.First(s => s.CC_ID.Equals("SHIPPING_INSTRUCTION"));

			var provider = service.eHubServiceProviders_Service.Select(s => new { ID = s.eHubClient_Provider.CC_ID });

			if (filtered)
			{
				string id = Request["ID"];
				if (!string.IsNullOrWhiteSpace(id))
					provider = provider.Where(c => c.ID.ToUpper().Contains(id.ToUpper()));
			}

			int count = provider.Count();

			switch (sidx + " " + sord)
			{
				case "ID desc":
					provider = provider.OrderByDescending(c => c.ID);
					break;
				default:
					provider = provider.OrderBy(c => c.ID);
					break;
			}

			var list = provider.Skip((page - 1) * rows).Take(rows).ToList();
			if (selectedID == DEFAULT_ID) list.Insert(0, new { ID = REJECT_SERVICE_PROVIDER });

			return new
			{
				page = page,
				total = Math.Ceiling(count / (double)rows),
				records = count,
				serviceProvider = list
			};
		}

		public object GetDocumentNames()
		{
			return new
			{
				records = DocNames.Count,
				docNames = DocNames.Select(x => new {docName = x.docName}).ToList()
			};
		}

		public object GetPartiesToCopy()
		{
			return new
			{
				records = PartiesToCopy.Count,
				partiesToCopy = PartiesToCopy.Select(x => new { partyToCopy = x.partyToCopy }).ToList()
			};
		}

		public object GetSplitByOptions()
		{
			return new
			{
				records = SplitOptions.Count,
				splitByOptions = SplitOptions.Select(x => new { splitOption = x.splitOption }).ToList()
			};
		}

		public object GetShipmentTypes()
		{
			return new
			{
				records = ShipmentTypes.Count,
				shipmentTypes = ShipmentTypes.Select(x => new {shpType = x.shpType, description = x.description}).ToList()
			};
		}

		public object Values()
		{
			int page = string.IsNullOrEmpty(Request["page"]) ? 1 : Convert.ToInt32(Request["page"]);
			int rows = Convert.ToInt32(Request["rows"]);
			var list = GetValues();
			int count = list.Count();

			var total = (int)Math.Ceiling(count / (double)rows);
			if (page > total) page = total;
			var vals = list.Skip((page - 1) * rows).Take(rows).ToList();

			return new
			{
				page = page,
				total = total,
				records = count,
				oceanMessagingValues = vals
			};
		}

		public void ClearCache()
		{
			CurrentSessionCache = null;
		}

		public object MoveRow()
		{
			var data = GetOceanMessagingCache();
			var id = Request["id"];
			switch (Request["option"])
			{
				case "up":
					MoveUp(data, data.Find(data.FirstOrDefault(x => x.id.Equals(id))));
					break;
				case "down":
					MoveDown(data, data.Find(data.FirstOrDefault(x => x.id.Equals(id))));
					break;
				case "set":
					MoveTo(data, id);
					break;
			}
			CurrentSessionCache = data;

			var currentPosition = data.Select((n, i) => new { rowid = n.id, index = i }).First(x => x.rowid.Equals(id)).index;
			var pageNumber = (int)Math.Ceiling((currentPosition + 1) / float.Parse(Request["rowLimit"]));
			return new { page = pageNumber };
		}

		public void ReformatSessionRowID()
		{
			var sessionCache = CurrentSessionCache;
			if (sessionCache != null)
			{
				var count = START_ID;
				var node = sessionCache.First;
				while (node != sessionCache.Last && node != null) // change all id except last one
				{
					node.Value.id = count++ + "";
					node = node.Next;
				}
			}
			CurrentSessionCache = sessionCache;
		}

		public byte[] GetValuesExportCsv()
		{
			var cache = GetOceanMessagingCache();
			using (var wrt = new StringWriter())
			using (var csv = new CsvHelper.CsvWriter(wrt))
			{
				GetExportCsvHeaders().ForEach(f => csv.WriteField(f));
				csv.NextRecord();

				var node = cache.First;
				while (node != null)
				{
					var value = node.Value;
					GetExportCsvColumns(value).ForEach(f => csv.WriteField(f));
					csv.NextRecord();
					node = node.Next;
				}
				return Encoding.Default.GetBytes(wrt.ToString());
			}
		}

		List<string> GetExportCsvHeaders()
		{
			return JqGridColumns().Select(x => GetColumnName(x)).ToList();
		}

		public string GetExportFileName()
		{
			return ID + ".csv";
		}

		public object ImportCsv()
		{
			try
			{
				var data = ImportCsvCore();
				ValidateCsvDataAndReplaceWithValidData(data);
				CurrentSessionCache = data;

				CsvLogger.Debug(() => $"[{Name}] imported routing rules csv: \r\n\t{FormatLoggingValuesAsCSV(data)}");
				return new { success = true, message = "CSV file successfully imported." };
			}
			catch (Exception ex)
			{
				return new { success = false, message = ex.Message };
			}
		}

        #endregion

        void ValidateMandatoryFields()
		{
			var mandatoryFields = MandatoryColumns();
			if (mandatoryFields != null && mandatoryFields.Length != 0)
			{
				var emptyMandatoryFields = mandatoryFields.Where(x => String.IsNullOrWhiteSpace(Request[x])).Select(GetColumnName).ToArray();
				if (emptyMandatoryFields.Length > 0)
					throw new InvalidOperationException(string.Join(" and ", emptyMandatoryFields) + " cannot be empty.");
			}
		}

		void ValidateExpressionValues()
		{
			var fields = JqGridColumns();
			if (fields != null)
			{
				foreach (var field in fields)
				{
					ValidateValueForExpression(Request[field], GetExpressionOption(field), GetColumnName(field));
				}
			}
		}

        internal LinkedList<Row> CurrentSessionCache
		{
			get { return Session["OCM_" + ID] as LinkedList<Row>; }
			set { Session["OCM_" + ID] = value; }
		}

		LinkedList<Row> GetOceanMessagingCache()
		{
            return CurrentSessionCache ?? GetOceanMessagingfromDB();

        }

        LinkedList<Row> GetOceanMessagingfromDB()
        {
            return new LinkedList<Row>(ValuesDatabase());
        }

		internal virtual string SaveCore()
		{
			if (CurrentSessionCache != null)
			{
				LinkedList<Row> backupData = null;
				if (CsvLogger.IsDebugEnabled)
				{
					backupData = GetOceanMessagingfromDB();
				}
				var cache = GetOceanMessagingCache();
				try
				{
					OCMLogger.Info(() => $"[{StaffID}][{Name}] Saving routing rules data.");
					SaveToDatabase(cache);
				}
				catch (Exception ex)
				{
					OCMLogger.Error($"[{StaffID}] Fail to save all data into database.", ex);
					return "Fail to save all data into database. Reason: " + ex.Message;
				}
				if (CsvLogger.IsDebugEnabled)
				{
					CsvLoggerDebug(GetOceanMessagingfromDB(), backupData);
				}
				ClearCache();
				OCMLogger.Info(() => $"[{StaffID}][{Name}] Saved routing rules data.");
				return "Success tranfering all the data into database";
			}
			return "No changes to be applied.";
		}


		internal virtual void CsvLoggerDebug(LinkedList<Row> rowData, LinkedList<Row> previousRowData)
        {
            CsvLogger.Debug(() => $@"[{Name}] {StaffID} saved routing rules: 
    {FormatLoggingValuesAsCSV(rowData)}
    --------------------------------------------------------------------------------------------------
    --------------------------------------------------------------------------------------------------
[{Name}] Previous State:
    {FormatLoggingValuesAsCSV(previousRowData)}");
        }

		List<dynamic> GetValues()
		{
			var vals = GetOceanMessagingCache();
			string searchQuery = Request["filters"];
			bool filtered = Boolean.Parse(Request["_search"]);
			MultipleFilter filterQuery = new MultipleFilter();
			if (!string.IsNullOrEmpty(searchQuery))
			{
				filterQuery = JsonConvert.DeserializeObject<MultipleFilter>(searchQuery);
			}

			List<Row> list = vals.ToList();
			if (filterQuery.rules != null && filterQuery.rules.Count > 0)
			{
				list = ApplyMultipleValuesFilter(filterQuery, vals);
			}
			else if (filtered)
			{
				list = FilterValues(vals);
			}

			var defaultVal = vals.FirstOrDefault(x => x.id.Equals(DEFAULT_ID));
			if (defaultVal != null && !list.Any(x => x.id.Equals(DEFAULT_ID)))
			{
				list.Add(defaultVal);
			}

			string sortField = Request["sidx"];
			string sortOrder = Request["sord"];
			if (!string.IsNullOrEmpty(sortField) && !string.IsNullOrEmpty(sortOrder)) {
				list = ApplySortOrderFilter(sortField, sortOrder, list);
			}

			return GetJsonObject(list);
		}

		private List<Row> ApplyMultipleValuesFilter(MultipleFilter filterQuery, LinkedList<Row> vals)
		{
			var resultVars = vals.AsQueryable();
			switch (filterQuery.groupOp)
			{
				case "AND":
					foreach (var rule in filterQuery.rules)
						resultVars = SearchValues(rule.field, rule.data, rule.op, resultVars).AsQueryable();
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

        private List<Row> ApplySortOrderFilter(string sortField, string sortOrder, List<Row> list)
        {
            var queryableList = list.AsQueryable();
            return DynamicQueryable.OrderBy(queryableList, sortField + " " + sortOrder).ToList();
        }

		void MoveUp(LinkedList<Row> data, LinkedListNode<Row> currentRow)
		{
			var previous = currentRow?.Previous;
			if (previous != null)
			{
				data.Remove(currentRow);
				data.AddBefore(previous, currentRow);
			}
		}

		void MoveDown(LinkedList<Row> data, LinkedListNode<Row> currentRow)
		{
			var next = currentRow?.Next;
			if (next != null)
			{
				data.Remove(currentRow);
				data.AddAfter(next, currentRow);
			}
		}

		protected virtual void MoveTo(LinkedList<Row> data, string id)
		{
			int newPosition = Int32.Parse(Request["newPosition"]) - 1; // index = position - 1, newpos must be a number.
			var count = 0;
			var currentSelectedPosition = -1;
			LinkedListNode<Row> previousRowInNewPosition = null, currentSelectedRow = null;
			var node = data.First;
			while (node != data.Last && node != null) // change all id except last one
			{
				if (node.Value.id.Equals(id))
				{
					currentSelectedPosition = count;
					currentSelectedRow = node;
				}
				if (count == newPosition)
				{
					previousRowInNewPosition = node;
				}
				if (newPosition == currentSelectedPosition)
					return;
				if (previousRowInNewPosition != null && currentSelectedRow != null)
					break;
				count++;
				node = node.Next;
			}

			if (currentSelectedPosition != -1 && previousRowInNewPosition != null && currentSelectedRow != null)
			{
				data.Remove(currentSelectedRow);
				if (newPosition < currentSelectedPosition)
					data.AddBefore(previousRowInNewPosition, currentSelectedRow);
				else
					data.AddAfter(previousRowInNewPosition, currentSelectedRow);
			}
		}

		internal string BuildExpression(string target, string inputValue, string columnName, string operation = "Equal")
		{
			ExpressionOptions buildOption = GetExpressionOption(columnName);

			if (target == null || inputValue == null || DEFAULTVALUE_ANY.Equals(inputValue) || buildOption == ExpressionOptions.None)
			{
				return null;
			}

			ValidateValueForExpression(inputValue, buildOption, GetColumnName(columnName));

			string returnExpression = null;

			if (buildOption.HasFlag(ExpressionOptions.HandleGroup) && inputValue.Contains(","))
			{
				returnExpression = string.Format(ExperssionFormatString, target, "IsMatch", string.Format("^({0})$", inputValue.Replace(",", "|")));
			}
			else if (buildOption.HasFlag(ExpressionOptions.HandleWildcard) && inputValue.Contains("*"))
			{
				returnExpression = string.Format(ExperssionFormatString, target, "IsMatch", string.Format("^{0}$", inputValue.Replace("*", ".*")));
			}
			else if (buildOption.HasFlag(ExpressionOptions.HandleEqual))
			{
				returnExpression = string.Format(ExperssionFormatString, target, operation, inputValue);
			}

			return returnExpression;
		}

		void ValidateValueForExpression(string inputValue, ExpressionOptions buildOption, string columnName)
		{
			if (string.IsNullOrEmpty(inputValue)) return;

			if (inputValue.Contains("*") && !buildOption.HasFlag(ExpressionOptions.HandleWildcard))
			{
				throw new InvalidOperationException(string.Format("Cannot handle wild card for {0} column", columnName));
			}

			if (inputValue.Contains(",") && !buildOption.HasFlag(ExpressionOptions.HandleGroup))
			{
				throw new InvalidOperationException(string.Format("Cannot handle group of values for {0} column", columnName));
			}
		}

		internal string ExtractExpressionValue(string[] expressionValues, string columnName)
		{
			ExpressionOptions buildOption = GetExpressionOption(columnName);

			if ("IsMatch".Equals(expressionValues[1]) && (buildOption.HasFlag(ExpressionOptions.HandleGroup) || buildOption.HasFlag(ExpressionOptions.HandleWildcard)))
			{
				return ConvertValue(expressionValues.Last());
			}

			return expressionValues.Last();
		}

		protected string ConvertValue(string inputValue)
		{
			return inputValue?.TrimStart('^', '(').TrimEnd('$', ')').Replace(".*", "*").Replace("|", ",");
		}

        protected string FormatLoggingValuesAsCSV(LinkedList<Row> rowData)
        {
			string[] orderedFieldNames = { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.provider, Row.RowNames.port, Row.RowNames.docName, Row.RowNames.shpType, Row.RowNames.eventBranch };
            return string.Join("\r\n\t", rowData.Select(row => string.Join(", ", row.GetType().GetFields().Where(field => JqGridColumns().Contains(field.Name))
				.OrderBy(field => Array.IndexOf(orderedFieldNames, field.Name))
                .Select(field => string.Format("{0}='{1}'", field.Name, field.GetValue(row))))));
        }

        protected class DocumentName
		{
			internal string docName;
		}

		protected class PartyToCopy
		{
			internal string partyToCopy;
		}

		protected class SplitByOptions
		{
			internal string splitOption;
			internal string recipientID;
			internal string resultValue;
		}

		protected class ShipmentType
		{
			internal string shpType;
			internal string description;
		}
    }
}
