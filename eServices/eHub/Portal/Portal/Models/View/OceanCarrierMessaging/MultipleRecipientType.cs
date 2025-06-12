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
	public class MultipleRecipientType : AbstractOceanCarrierMessagingType
	{
		public MultipleRecipientType(Controller controller, eHubTransactionsContext context)
			: base(controller, context)
		{
		}

		public override string PrefixName
		{
			get { return string.Empty; }
		}

		public override string Name
		{
			get { return "Multiple Recipients Copying"; }
		}

		public override string ID
		{
			get { return OceanCarrierMessagingController.TypeNames.MultipleRecipients; }
		}

		public override string MatchingGroupRuleName
		{
			get { return "MultipleRecipientsCopying"; }
		}

		public override string[] JqGridColumns()
		{
			return new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.port, Row.RowNames.docName, Row.RowNames.shipNamespace, Row.RowNames.partyToCopy };
		}

		protected override string[] MandatoryColumns()
		{
			return new[] { Row.RowNames.client, Row.RowNames.partyToCopy };
		}


		private Group GetMultipleRecipientGroupRule()
		{
			return Rule.GetForEditing(Context, Context.eHubClients.Single(c => c.CC_ID == "OCM_MutipleRecipientsCopying")).FindGroupRule(MatchingGroupRuleName);
		}

		protected override void AddValues(LinkedList<Row> existingData)
		{
			var id = Request["id"];
			var afterRowId = Request["afterRowId"];
			var client = Request["client"];
			var carrier = Request["carrier"];
			var port = Request["port"];
			var docName = Request["docName"];
			var shpNamespace = Request["shipNamespace"];
			var partyToCopy = Request["partyToCopy"];

			if (id.Equals("_empty"))
				id = Request["nextid"];

			if (String.IsNullOrWhiteSpace(carrier)) carrier = DEFAULTVALUE_ANY;
			if (String.IsNullOrWhiteSpace(port)) port = DEFAULTVALUE_ANY;
			if (String.IsNullOrWhiteSpace(docName)) docName = DEFAULTVALUE_ANY;
			if (String.IsNullOrWhiteSpace(shpNamespace)) shpNamespace = DEFAULTVALUE_ANY;

			var row = new Row
			{
				id = id,
				client = client,
				carrier = carrier,
				port = port,
				docName = docName,
				shipNamespace = shpNamespace,
				partyToCopy = partyToCopy
			};

			AddRow(existingData, afterRowId, row);
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
				currentRow.client = Request["client"];
				currentRow.carrier = string.IsNullOrWhiteSpace(Request["carrier"]) ? DEFAULTVALUE_ANY : Request["carrier"];
				currentRow.port = string.IsNullOrWhiteSpace(Request["port"]) ? DEFAULTVALUE_ANY : Request["port"];
				currentRow.docName = string.IsNullOrWhiteSpace(Request["docName"]) ? DEFAULTVALUE_ANY : Request["docName"];
				currentRow.shipNamespace = string.IsNullOrWhiteSpace(Request["shipNamespace"]) ? DEFAULTVALUE_ANY : Request["shipNamespace"];
				currentRow.partyToCopy = Request["partyToCopy"];
			}
		}

		protected override List<Row> FilterValues(IEnumerable<Row> vals)
		{
			string carrier = Request["carrier"];
			string port = Request["port"];
			string shipNamespace = Request["shipNamespace"];
			string docName = Request["docName"];
			if (!string.IsNullOrWhiteSpace(carrier))
				vals = vals.Where(c => c.carrier.ToUpper().Contains(carrier.ToUpper()));
			if (!string.IsNullOrWhiteSpace(port))
				vals = vals.Where(c => c.port.ToUpper().Contains(port.ToUpper()));
			if (!string.IsNullOrWhiteSpace(docName))
				vals = vals.Where(c => c.docName.ToUpper().Contains(docName.ToUpper()));
			if (!string.IsNullOrWhiteSpace(shipNamespace))
				vals = vals.Where(c => c.shipNamespace.ToUpper().Contains(shipNamespace.ToUpper()));
			return vals.ToList();
		}

		protected override List<string> GetExportCsvColumns(Row row)
		{
			return new List<string> { row.client, row.carrier, row.port, row.docName, row.shipNamespace, row.partyToCopy };
		}

		protected override List<dynamic> GetJsonObject(IEnumerable<Row> vals)
		{
			return vals.Select(x => new { id = x.id, client = x.client, carrier = x.carrier, port = x.port, docName = x.docName, shipNamespace = x.shipNamespace, partyToCopy = x.partyToCopy }).ToList<dynamic>();
		}

		protected override LinkedList<Row> ImportCsvCore()
		{
			var data = new LinkedList<Row>();
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
						var id = "" + count++;
						var newRow = new Row { id = id, client = fields[0], carrier = fields[1], port = fields[2], docName = fields[3], shipNamespace = fields[4], partyToCopy = fields[5] };
						data.AddLast(newRow);
					}
				}
			return data;
		}

        internal override void SaveToDatabase(LinkedList<Row> data)
		{
			Group groupRule = GetMultipleRecipientGroupRule();

			var newSubRules = new List<Criterion>();

			var count = 0;
			var node = data.First;
			while (node != null)
			{
				node.Value.id = count++ + "";
				var value = node.Value;
				var expression = new List<string>();
				AddExpression(expression, BuildExpression("@SourceParty", value.client, Row.RowNames.client));
				AddExpression(expression, BuildExpression("@SCACUniShip", value.carrier, Row.RowNames.carrier));
				AddExpression(expression, BuildExpression("@Port", value.port, Row.RowNames.port));
				AddExpression(expression, BuildExpression("@UShipmentNamespace", value.shipNamespace, Row.RowNames.shipNamespace));
				AddExpression(expression, BuildExpression("@DocumentName", value.docName, Row.RowNames.docName));
				
				var criterion = new Condition
				{
					Expression = string.Join(" && ", expression.Where(x => !string.IsNullOrEmpty(x))),
					Value = value.partyToCopy
				};

				newSubRules.Add(criterion);
				node = node.Next;
			}
			
			groupRule.SubRules.Load(newSubRules, OCMLogger);
			Context.SaveChanges();
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

				case "shipNamespace":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.shipNamespace.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.shipNamespace.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.shipNamespace.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.shipNamespace.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;

				case "partyToCopy":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.partyToCopy.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.partyToCopy.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.partyToCopy.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.partyToCopy.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
			}

			return vals.ToList();
		}

		protected override void ValidateCsvDataAndReplaceWithValidData(LinkedList<Row> data)
		{
			var clients = Context.eHubClients.Select(c => c.CC_ID).ToList();

			var node = data.First;
			var count = 1;

			while (node != null)
			{
				var value = node.Value;
				if (!value.client.Contains('*') && !clients.Contains(value.client))
				{
					value.client = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.port))
				{
					value.port = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.docName))
				{
					value.docName = DEFAULTVALUE_ANY;
				}
				else if (DocNames.All(x => x.docName != value.docName))
				{
					throw new InvalidOperationException($"The Document Name \"{value.docName}\" is invalid in row {count} of the csv file.");
				}
				if (string.IsNullOrWhiteSpace(value.carrier))
				{
					value.carrier = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.shipNamespace))
				{
					value.shipNamespace = DEFAULTVALUE_ANY;
				}
				if (PartiesToCopy.All(x => x.partyToCopy != value.partyToCopy))
				{
					throw new InvalidOperationException($"The Party To Copy \"{value.partyToCopy}\" is invalid in row {count} of the csv file.");
				}
				node = node.Next;
				count += 1;
			}
		}

        internal override List<Row> ValuesDatabase()
		{
			int count = 0;

			var subRules = GetMultipleRecipientGroupRule().SubRules;

			var oceanMessagingData = subRules.Select(x =>
			{
				var citeria = (Condition)x;
				if (!string.IsNullOrEmpty(citeria.Expression))
				{
					var condition = citeria.Expression.Split(new[] { "&&" }, StringSplitOptions.None).Select(y => y.Trim().Split(new[] { "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries));
					var mclient = condition.FirstOrDefault(a => a[0] == "@SourceParty");
					var mport = condition.FirstOrDefault(a => a[0] == "@Port");
					var mdocName = condition.FirstOrDefault(a => a[0] == "@DocumentName");
					var mcarrier = condition.FirstOrDefault(a => a[0] == "@SCACUniShip");
					var mshpNameSpace = condition.FirstOrDefault(a => a[0] == "@UShipmentNamespace");
					var mpartyToCopy = x.Value;
					return new Row
					{
						id = ++count + "",
						client = mclient != null ? ExtractExpressionValue(mclient, Row.RowNames.client) : DEFAULTVALUE_ANY,
						port = mport != null ? ExtractExpressionValue(mport, Row.RowNames.port) : DEFAULTVALUE_ANY,
						docName = mdocName != null ? mdocName.Last() : DEFAULTVALUE_ANY,
						carrier = mcarrier != null ? ExtractExpressionValue(mcarrier, Row.RowNames.carrier) : DEFAULTVALUE_ANY,
						shipNamespace = mshpNameSpace != null ? mshpNameSpace.Last() : DEFAULTVALUE_ANY,
						partyToCopy = mpartyToCopy
					};
				}
				else
				{
					return new Row { id = ++count + "", client = DEFAULTVALUE_ANY, carrier = DEFAULTVALUE_ANY, port = DEFAULTVALUE_ANY, docName = DEFAULTVALUE_ANY, partyToCopy = DEFAULTVALUE_ANY, shipNamespace = DEFAULTVALUE_ANY };
				}
			});

			var oceanMessagingList = oceanMessagingData.ToList();

			return oceanMessagingList;
		}

		protected override void ValidateCsvHeaders(string[] fieldNames)
		{

			if (!Enumerable.SequenceEqual(fieldNames, new[] { "Client ID", "Carrier SCAC/C1C", "Port/Country", "Document Name", "UShipment NameSpace", "Party To Copy" }))
			{
				throw new InvalidOperationException($"The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\", \"Party To Copy\"");
			}
		}
	}
}
