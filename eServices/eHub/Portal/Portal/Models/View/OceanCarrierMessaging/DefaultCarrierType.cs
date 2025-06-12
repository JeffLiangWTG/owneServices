using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Controllers.Service;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging
{
	public class DefaultCarrierType : AbstractOceanCarrierMessagingType
	{
		public DefaultCarrierType(Controller controller, eHubTransactionsContext context)
			: base(controller, context)
		{
		}

		public override string PrefixName
		{
			get { return "Default Phase"; }
		}

		public override string Name
		{
			get { return "Default Carrier"; }
		}

		public override string ID
		{
			get { return OceanCarrierMessagingController.TypeNames.DefaultCarrier; }
		}

		public override string MatchingGroupRuleName
		{
			get { return "DefaultCarrier"; }
		}

		public override string[] JqGridColumns()
		{

			return new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.eventBranch, Row.RowNames.port, Row.RowNames.shpType, Row.RowNames.docName, Row.RowNames.provider };
		}

		protected override string[] MandatoryColumns()
		{
			return new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.docName, Row.RowNames.provider };
		}

		protected override List<dynamic> GetJsonObject(IEnumerable<Row> vals)
		{
			return vals.Select(x => new { id = x.id, client = x.client, carrier = x.carrier, eventBranch = x.eventBranch, port = x.port, shpType = x.shpType, docName = x.docName, provider = x.provider, }).ToList<dynamic>();
		}
	
		internal override List<Row> ValuesDatabase()
		{
			int count = 0;

			var rule = Rule.GetForReading(Context.eHubClients.First(c => c.CC_ID == "SHIPPING_INSTRUCTION"));
			var subRules = GetGroupRule(rule).SubRules;
			var oceanMessagingData = subRules.Select(x =>
			{
				var citeria = (Condition)x;
				if (!string.IsNullOrEmpty(citeria.Expression))
				{
					var condition = citeria.Expression.Split(new[] { "&&" }, StringSplitOptions.None).Select(y => y.Trim().Split(new[] { "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries));
					var mclient = condition.FirstOrDefault(a => a[0] == "@SourceParty");
					var mcarrier = condition.FirstOrDefault(a => a[0] == "@SCAC");
					var mport = condition.FirstOrDefault(a => a[0] == "@Port");
					var mdocName = condition.FirstOrDefault(a => a[0] == "@DocumentName");
					var mshpType = condition.FirstOrDefault(a => a[0] == "@NVOCC");
					var meventBranch = condition.FirstOrDefault(a => a[0] == "@EventBranch");

					var row = new Row
					{
						id = ++count + "",
						client = mclient != null ? ExtractExpressionValue(mclient, Row.RowNames.client) : DEFAULTVALUE_ANY,
						carrier = mcarrier != null ? ExtractExpressionValue(mcarrier, Row.RowNames.carrier) : DEFAULTVALUE_ANY,
						port = mport != null ? ExtractExpressionValue(mport, Row.RowNames.port) : DEFAULTVALUE_ANY,
						provider = citeria.ServiceProvider.ProviderClient.CC_ID,
						docName = mdocName != null ? mdocName.Last() : DEFAULTVALUE_ANY,
						shpType = mshpType != null ? mshpType.Last() == "Y" ? NVOCC : VOCC : DEFAULTVALUE_ANY,
						eventBranch = meventBranch != null ? ExtractExpressionValue(meventBranch, Row.RowNames.eventBranch) : DEFAULTVALUE_ANY

					};

					if (!IsDefaultCarrier())
					{
						var mCarrierAgent = condition.FirstOrDefault(a => a[0] == "@" + ID);
						row.carrierAgent = mCarrierAgent != null ? mCarrierAgent.Last() : DEFAULTVALUE_ANY;
					}

					return row;
				}
				else
				{
					var row = new Row { id = ++count + "", client = DEFAULTVALUE_ANY, carrier = DEFAULTVALUE_ANY, carrierAgent = DEFAULTVALUE_ANY, port = DEFAULTVALUE_ANY, provider = citeria.ServiceProvider.ProviderClient.CC_ID, docName = DEFAULTVALUE_ANY, shpType = DEFAULTVALUE_ANY, eventBranch = DEFAULTVALUE_ANY };
					if (!IsDefaultCarrier())
					{
						row.carrierAgent = DEFAULTVALUE_ANY;
					}

					return row;
				}
			});

			var oceanMessagingList = oceanMessagingData.ToList();

			var defaultProvider = rule.DefaultServiceProvider;
			oceanMessagingList.Add(new Row { id = DEFAULT_ID, client = "Default", carrier = "Default", carrierAgent = "Default", port = "Default", eventBranch = "Default", provider = defaultProvider == null ? REJECT_SERVICE_PROVIDER : defaultProvider.ProviderClient.CC_ID, docName = "Default", shpType = "Default" });

			return oceanMessagingList;
		}

		protected override void AddValues(LinkedList<Row> existingData)
		{
			var id = Request["id"];
			var afterRowId = Request["afterRowId"];
			var client = Request["client"];
			var carrier = Request["carrier"];
			var provider = Request["provider"];
			var port = Request["port"];
			var docName = Request["docName"];
			var shpType = Request["shpType"];
			var eventBranch = Request["eventBranch"];

			if (id.Equals("_empty"))
				id = Request["nextid"];

			LinkedListNode<Row> afterRow = existingData.Find(existingData.FirstOrDefault(x => x.id.Equals(afterRowId)));
			if (String.IsNullOrWhiteSpace(carrier)) carrier = DEFAULTVALUE_ANY;
			if (String.IsNullOrWhiteSpace(port)) port = DEFAULTVALUE_ANY;
			if (String.IsNullOrWhiteSpace(docName)) docName = DEFAULTVALUE_ANY;
			if (String.IsNullOrWhiteSpace(shpType)) shpType = DEFAULTVALUE_ANY;
			if (String.IsNullOrWhiteSpace(eventBranch)) eventBranch = DEFAULTVALUE_ANY;

			var row = new Row
			{
				id = id,
				client = client,
				carrier = carrier,
				provider = provider,
				port = port,
				docName = docName,
				shpType = shpType,
				eventBranch = eventBranch
			};

			if (!IsDefaultCarrier())
			{
				var carrierAgent = Request["carrierAgent"];
				row.carrierAgent = string.IsNullOrWhiteSpace(carrierAgent) ? DEFAULTVALUE_ANY : carrierAgent;
			}

			if (afterRow != null)
				existingData.AddBefore(afterRow, row);
		}

		protected override void EditValues(LinkedList<Row> existingData)
		{
			var id = Request["id"];
			var existed = existingData.Any(x => x.id.Equals(id));
			if (!existed)
			{
				AddValues(existingData);
			}
			else
			{
				var currentRow = existingData.Single(x => x.id.Equals(id));
				currentRow.client = string.IsNullOrWhiteSpace(Request["client"]) ? DEFAULTVALUE_ANY : Request["client"];
				currentRow.carrier = string.IsNullOrWhiteSpace(Request["carrier"]) ? DEFAULTVALUE_ANY : Request["carrier"];
				currentRow.provider = Request["provider"];
				currentRow.port = string.IsNullOrWhiteSpace(Request["port"]) ? DEFAULTVALUE_ANY : Request["port"];
				currentRow.docName = string.IsNullOrWhiteSpace(Request["docName"]) ? DEFAULTVALUE_ANY : Request["docName"];
				currentRow.shpType = string.IsNullOrWhiteSpace(Request["shpType"]) ? DEFAULTVALUE_ANY : Request["shpType"];
				currentRow.eventBranch = string.IsNullOrWhiteSpace(Request["eventBranch"]) ? DEFAULTVALUE_ANY : Request["eventBranch"];

				if (!IsDefaultCarrier())
				{
					currentRow.carrierAgent = string.IsNullOrWhiteSpace(Request["carrierAgent"]) ? DEFAULTVALUE_ANY : Request["carrierAgent"];
				}
			}
		}

        internal override void SaveToDatabase(LinkedList<Row> data) 
		{
			var lastRowDefault = data.Last.Value;
			
			var rule = Rule.GetForEditing(Context, Context.eHubClients.Single(c => c.CC_ID == "SHIPPING_INSTRUCTION"));

			var newSubRules = new List<Criterion>();

			var count = 0;
			var node = data.First;
			while (node != null && node != data.Last)
			{
				node.Value.id = count++ + "";
				var value = node.Value;
				var expression = new List<string>();
				AddExpression(expression, BuildExpression("@SourceParty", value.client, Row.RowNames.client));
				AddExpression(expression, BuildExpression("@SCAC", value.carrier, Row.RowNames.carrier));
				AddExpression(expression, BuildExpression("@" + ID, value.carrierAgent, Row.RowNames.carrierAgent));
				AddExpression(expression, BuildExpression("@Port", value.port, Row.RowNames.port));
				AddExpression(expression, BuildExpression("@DocumentName", value.docName, Row.RowNames.docName));
				if (!value.shpType.Equals(DEFAULTVALUE_ANY))
				{
					AddExpression(expression, BuildExpression("@NVOCC", value.shpType == NVOCC ? "Y" : "N", Row.RowNames.shpType));
				}
				
				AddExpression(expression, BuildExpression("@EventBranch", value.eventBranch, Row.RowNames.eventBranch));
				
				var criterion = new Condition
				{
					Expression = string.Join(" && ", expression.ToArray()),
					ServiceProvider = rule.ServiceProviders.Single(p => p.ProviderClient.CC_ID == value.provider)
				};
				newSubRules.Add(criterion);
				node = node.Next;
			}

			OCMLogger.Debug($"Adding 'Insert' and 'Delete' to the context started.");
			GetGroupRule(rule).SubRules.Load(newSubRules, OCMLogger);
			rule.DefaultServiceProvider = lastRowDefault.provider.Equals(REJECT_SERVICE_PROVIDER) ? null : rule.ServiceProviders.Single(sp => sp.ProviderClient.CC_ID == lastRowDefault.provider);
			OCMLogger.Debug($"'Insert' and 'Delete' have been added to the context after calling RoutingRuleEngine and before calling 'SaveChanges'.");
			Context.SaveChanges();
			OCMLogger.Debug($"Changes have been saved to the database.");

		}

		protected override List<Row> FilterValues(IEnumerable<Row> vals)
		{
			string client = Request["client"];
			string carrier = Request["carrier"];
			string carrierAgent = Request["carrierAgent"];
			string provider = Request["provider"];
			string port = Request["port"];
			string docName = Request["docName"];
			string shpType = Request["shpType"];
			string eventBranch = Request["eventBranch"];
			if (!string.IsNullOrWhiteSpace(client))
				vals = vals.Where(c => c.client.ToUpper().Contains(client.ToUpper()));
			if (!string.IsNullOrWhiteSpace(carrier))
				vals = vals.Where(c => c.carrier.ToUpper().Contains(carrier.ToUpper()));
			if (!string.IsNullOrWhiteSpace(carrierAgent))
				vals = vals.Where(c => c.carrierAgent.ToUpper().Contains(carrierAgent.ToUpper()));
			if (!string.IsNullOrWhiteSpace(provider))
				vals = vals.Where(c => c.provider.ToUpper().Contains(provider.ToUpper()));
			if (!string.IsNullOrWhiteSpace(port))
				vals = vals.Where(c => c.port.ToUpper().Contains(port.ToUpper()));
			if (!string.IsNullOrWhiteSpace(docName))
				vals = vals.Where(c => c.docName.ToUpper().Contains(docName.ToUpper()));
			if (!string.IsNullOrWhiteSpace(shpType))
				vals = vals.Where(c => c.shpType.ToUpper().Contains(shpType.ToUpper()));
			if (!string.IsNullOrWhiteSpace(eventBranch))
				vals = vals.Where(c => c.eventBranch.ToUpper().Contains(eventBranch.ToUpper()));
			return vals.ToList();
		}

		protected override List<Row> SearchValues(string searchField, string searchString, string searchOper, IQueryable<Row> vals)
		{
			switch (searchField)
			{
				case "client":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.client.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.client.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.client.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.client.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
				case "carrier":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.carrier.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.carrier.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.carrier.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.carrier.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
				case "carrierAgent":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.carrierAgent.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.carrierAgent.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.carrierAgent.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.carrierAgent.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
				case "provider":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.provider.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.provider.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.provider.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.provider.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
				case "port":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.port.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.port.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.port.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.port.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
				case "docName":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.docName.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.docName.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.docName.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.docName.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
				case "eventBranch":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.eventBranch.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.eventBranch.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.eventBranch.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.eventBranch.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
				case "shpType":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.shpType.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.shpType.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.shpType.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
			}

			return vals.ToList();
		}

		protected override List<string> GetExportCsvColumns(Row row)
		{
			return new List<string> { row.client, row.carrier, row.eventBranch, row.port, row.shpType, row.docName, row.provider};
		}

		protected override LinkedList<Row> ImportCsvCore()
		{
			var data = new LinkedList<Row>();
			var row = new Row { id = DEFAULT_ID, client = "Default", carrier = "Default", eventBranch = "Default", port = "Default", shpType = "Default", docName = "Default", provider = "" };

			if (!IsDefaultCarrier())
			{
				row.carrierAgent = "Default";
			}

			var lastrow = new LinkedListNode<Row>(row);
			var httpPostedFileBase = Request.Files["uploadFile"];
			if (httpPostedFileBase != null)
				using (var rdr = new StreamReader(httpPostedFileBase.InputStream))
				using (var csv = new CsvHelper.CsvParser(rdr))
				{
					string[] fields;
					var fieldNames = csv.Read();

					ValidateCsvHeaders(fieldNames);

					int count = 1;
					while ((fields = csv.Read()) != null)
					{
						if (fields[0] == "Default")
						{
							lastrow.Value.provider = fields[2];
							continue;
						}
						var id = "" + count++;
						var newRow = IsDefaultCarrier() ?
							new Row { id = id, client = fields[0], carrier = fields[1], eventBranch = fields[2], port = fields[3], shpType = fields[4], docName = fields[5], provider = fields[6] } :
							new Row { id = id, client = fields[0], carrier = fields[1], carrierAgent = fields[2], eventBranch = fields[3], port = fields[4], shpType = fields[5], docName = fields[6], provider = fields[7] };

						data.AddLast(newRow);
					}
				}
			data.AddLast(lastrow);
			return data;
		}

		protected override void ValidateCsvDataAndReplaceWithValidData(LinkedList<Row> data)
		{
			var service = Context.eHubClients.First(s => s.CC_ID.Equals("SHIPPING_INSTRUCTION"));
			var provider = service.eHubServiceProviders_Service.Select(s => s.eHubClient_Provider.CC_ID);
			var clients = Context.eHubClients.Select(c => c.CC_ID).ToList();

			var node = data.First;
			var count = 1;

			while (node != null && node != data.Last) // ignore last row which is default
			{
				var value = node.Value;
				if (!value.client.Contains('*') && !clients.Contains(value.client))
				{
					value.client = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.carrier))
				{
					value.carrier = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.carrierAgent))
				{
					value.carrierAgent = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.port))
				{
					value.port = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.shpType))
				{
					value.shpType = DEFAULTVALUE_ANY;
				}
				else if (ShipmentTypes.All(x => x.shpType != value.shpType))
				{
					throw new InvalidOperationException($"The Shipment Type \"{value.shpType}\" is invalid in row {count} of the csv file.");
				}
				if (string.IsNullOrWhiteSpace(value.docName))
				{
					value.docName = DEFAULTVALUE_ANY;
				}
				else if (DocNames.All(x => x.docName != value.docName))
				{
					throw new InvalidOperationException($"The Document Name \"{value.docName}\" is invalid in row {count} of the csv file.");
				}
				if (string.IsNullOrWhiteSpace(value.eventBranch))
				{
					value.eventBranch = DEFAULTVALUE_ANY;
				}
				if (!provider.Contains(value.provider))
				{
					throw new InvalidOperationException($"The Service Provider \"{value.provider}\" is invalid in row {count} of the csv file.");
				}
				count += 1;
				node = node.Next;
			}

			if (node != null)
			{
				var lastValue = node.Value; // last node
				if (!provider.Contains(lastValue.provider))
				{
					lastValue.provider = REJECT_SERVICE_PROVIDER;
				}
			}
		}

		protected override void ValidateCsvHeaders(string[] fieldNames)
		{
			if (IsDefaultCarrier())
			{
				if (!Enumerable.SequenceEqual(fieldNames, new[] { "Client ID", "Carrier SCAC/C1C", "Event Branch", "Port/Country", "Shipment Type", "Document Name", "Service Provider" }))
				{
					throw new InvalidOperationException($"The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\"");
				}
			}
			if (IsCarrierBookingAgent())
			{
				if (!Enumerable.SequenceEqual(fieldNames, new[] { "Client ID", "Carrier SCAC/C1C", "Booking Agent C1C", "Event Branch", "Port/Country", "Shipment Type", "Document Name", "Service Provider" }))
				{
					throw new InvalidOperationException($"The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Booking Agent C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\"");
				}
			}
			if (IsCarrierHandlingAgent())
			{
				if (!Enumerable.SequenceEqual(fieldNames, new[] { "Client ID", "Carrier SCAC/C1C", "Handling Agent C1C", "Event Branch", "Port/Country", "Shipment Type", "Document Name", "Service Provider" }))
				{
					throw new InvalidOperationException($"The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Handling Agent C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\"");
				}
			}
		}

		protected bool IsDefaultCarrier()
		{
			return ID.Equals(OceanCarrierMessagingController.TypeNames.DefaultCarrier);
		}

		protected bool IsCarrierBookingAgent()
		{
			return ID.Equals(OceanCarrierMessagingController.TypeNames.CarrierBookingAgent);
		}

		protected bool IsCarrierHandlingAgent()
		{
			return ID.Equals(OceanCarrierMessagingController.TypeNames.CarrierHandlingAgent);
		}
	}
}
