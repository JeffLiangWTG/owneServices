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
	public class SplitingWhiteListType : AbstractOceanCarrierMessagingType
	{
		public SplitingWhiteListType(Controller controller, eHubTransactionsContext context)
			: base(controller, context)
		{
		}

		public override string PrefixName
		{
			get { return string.Empty; }
		}

		public override string Name
		{
			get { return "Spliting White List"; }
		}

		public override string ID
		{
			get { return OceanCarrierMessagingController.TypeNames.SplitingWhiteList; }
		}

		public override string MatchingGroupRuleName
		{
			get { return "SplitingWhiteList"; }
		}
		public override string[] JqGridColumns()
		{
			return new[] { Row.RowNames.client, Row.RowNames.purpose, Row.RowNames.port, Row.RowNames.docName, Row.RowNames.shipNamespace, Row.RowNames.splitBy};
		}
		protected override string[] MandatoryColumns()
		{
			return new[] { Row.RowNames.client, Row.RowNames.splitBy};
		}

		protected override List<dynamic> GetJsonObject(IEnumerable<Row> vals)
		{
			return vals.Select(x => new { id = x.id, client = x.client, purpose = x.purpose, port = x.port, docName = x.docName, shipNamespace = x.shipNamespace, splitBy = x.splitBy}).ToList<dynamic>();
		}
		protected Group GetGroupRule()
		{
			return GetGroupRule(Rule.GetForEditing(Context, Context.eHubClients.Single(c => c.CC_ID == "SHIPPING_INSTRUCTION")), MatchingGroupRuleName)
			       ?? GetGroupRule(Rule.GetForReading(Context.eHubClients.First(c => c.CC_ID == "OCM_Splitting")), MatchingGroupRuleName);
		}
        internal override void SaveToDatabase(LinkedList<Row> data)
		{
			var newSubRules = new List<Criterion>();

			var count = 0;
			var node = data.First;
			
			while (node != null)
			{
				node.Value.id = count++ + "";
				var value = node.Value;
				var expression = new List<string>();
				AddExpression(expression, BuildExpression("@SourceParty", value.client, Row.RowNames.client));
				AddExpression(expression, BuildExpression("@Port", value.port, Row.RowNames.port));
				AddExpression(expression, BuildExpression("@DocumentName", value.docName, Row.RowNames.docName));
				AddExpression(expression, BuildExpression("@UShipmentNamespace", value.shipNamespace, Row.RowNames.shipNamespace));
				AddExpression(expression, BuildExpression("@Purpose", value.purpose, Row.RowNames.purpose));

				if (value.splitBy != null && !DEFAULTVALUE_ANY.Equals(value.splitBy))
				{
					var target = value.splitBy == "SubShipment" ? "@SubShipmentCount" : "@ContainerCount";
					AddExpression(expression, BuildExpression(target, "0", Row.RowNames.splitBy, "NotEqual"));
					AddExpression(expression, BuildExpression(target, "1", Row.RowNames.splitBy, "NotEqual"));
				}

				var recipientId = SplitOptions.First(x => x.splitOption == value.splitBy).recipientID;
				var resultValue = SplitOptions.First(x => x.splitOption == value.splitBy).resultValue;

				var criterion = new Condition
				{
					Expression = string.Join(" && ", expression.ToArray()),
					ResultValue = resultValue
				};

			
				newSubRules.Add(criterion);
				node = node.Next;
			}

			GetGroupRule().SubRules.Load(newSubRules, OCMLogger);
			Context.SaveChanges();
		}


        internal override List<Row> ValuesDatabase()
		{
			int count = 0;

			var subRules = GetGroupRule().SubRules;

			var oceanMessagingData = subRules.Select(x =>
			{
				var citeria = (Condition)x;
				if (!string.IsNullOrEmpty(citeria.Expression))
				{
					var condition = citeria.Expression.Split(new[] {"&&"}, StringSplitOptions.None)
						.Select(y => y.Trim().Split(new[] {"[", "]", ","}, StringSplitOptions.RemoveEmptyEntries));
					var mclient = condition.FirstOrDefault(a => a[0] == "@SourceParty");
					var mport = condition.FirstOrDefault(a => a[0] == "@Port");
					var mdocName = condition.FirstOrDefault(a => a[0] == "@DocumentName");
					var mshpType = condition.FirstOrDefault(a => a[0] == "@NVOCC");
					var mshpNameSpace = condition.FirstOrDefault(a => a[0] == "@UShipmentNamespace");
					var mpurpose = condition.FirstOrDefault(a => a[0] == "@Purpose");
					var msplitByValue = SplitOptions.First(y => y.resultValue == x.ResultValue).splitOption;


					return new Row
					{
						id = ++count + "",
						client = mclient != null ? ExtractExpressionValue(mclient, Row.RowNames.client) : DEFAULTVALUE_ANY,
						port = mport != null ? ExtractExpressionValue(mport, Row.RowNames.port) : DEFAULTVALUE_ANY,
						docName = mdocName != null ? mdocName.Last() : DEFAULTVALUE_ANY,
						shpType = mshpType != null ? mshpType.Last() == "Y" ? NVOCC : VOCC : DEFAULTVALUE_ANY,
						shipNamespace = mshpNameSpace != null ? mshpNameSpace.Last() : DEFAULTVALUE_ANY,
						purpose = mpurpose != null ? mpurpose.Last() : DEFAULTVALUE_ANY,
						splitBy = msplitByValue
					};
				}

				return new Row { id = ++count + "", client = DEFAULTVALUE_ANY, purpose = DEFAULTVALUE_ANY, port = DEFAULTVALUE_ANY, docName = DEFAULTVALUE_ANY, shipNamespace = DEFAULTVALUE_ANY, splitBy = DEFAULTVALUE_ANY };
			});

			var oceanMessagingList = oceanMessagingData.ToList();

			return oceanMessagingList;
		}
		protected override void MoveTo(LinkedList<Row> data, string id)
		{
			int newPosition = Int32.Parse(Request["newPosition"]) - 1; // index = position - 1, newpos must be a number.
			var count = 0;
			var currentSelectedPosition = -1;
			LinkedListNode<Row> previousRowInNewPosition = null, currentSelectedRow = null;
			var node = data.First;
			while (node != null) // change all id except last one
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

		protected override void AddValues(LinkedList<Row> existingData)
		{
			var id = Request["id"];
			var afterRowId = Request["afterRowId"];
			var client = Request["client"];
			var port = Request["port"];
			var docName = Request["docName"];
			var purpose = Request["purpose"];
			var splitBy = Request["splitBy"];
			var shipNamespace = Request["shipNamespace"];
			if (id.Equals("_empty"))
				id = Request["nextid"];

			if (string.IsNullOrWhiteSpace(port)) port = DEFAULTVALUE_ANY;
			if (string.IsNullOrWhiteSpace(docName)) docName = DEFAULTVALUE_ANY;
			if (string.IsNullOrWhiteSpace(purpose)) purpose = DEFAULTVALUE_ANY;
			if (string.IsNullOrWhiteSpace(splitBy)) splitBy = DEFAULTVALUE_ANY;
			if (string.IsNullOrWhiteSpace(shipNamespace)) shipNamespace = DEFAULTVALUE_ANY;

			var row = new Row
			{
				id = id,
				client = client,
				port = port,
				docName = docName,
				splitBy = splitBy,
				purpose = purpose,
				shipNamespace = shipNamespace,
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
				currentRow.client = string.IsNullOrWhiteSpace(Request["client"]) ? DEFAULTVALUE_ANY : Request["client"];
				currentRow.port = string.IsNullOrWhiteSpace(Request["port"]) ? DEFAULTVALUE_ANY : Request["port"];
				currentRow.docName = string.IsNullOrWhiteSpace(Request["docName"]) ? DEFAULTVALUE_ANY : Request["docName"];
				currentRow.splitBy = Request["splitBy"];
				currentRow.purpose = string.IsNullOrWhiteSpace(Request["purpose"]) ? DEFAULTVALUE_ANY : Request["purpose"];
				currentRow.shipNamespace = string.IsNullOrWhiteSpace(Request["shipNamespace"]) ? DEFAULTVALUE_ANY : Request["shipNamespace"];
			}
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

				case "purpose":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.purpose.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.purpose.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.purpose.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.purpose.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;

				case "splitBy":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.splitBy.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.splitBy.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.splitBy.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.splitBy.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;

				case "shipNamespace":
					switch (searchOper)
					{
						case "eq":
							vals = vals.Where(v => v.splitBy.ToUpper().Equals(searchString.ToUpper()));
							break;
						case "bw":
							vals = vals.Where(v => v.splitBy.ToUpper().StartsWith(searchString.ToUpper()));
							break;
						case "ew":
							vals = vals.Where(v => v.splitBy.ToUpper().EndsWith(searchString.ToUpper()));
							break;
						case "cn":
							vals = vals.Where(v => v.splitBy.ToUpper().Contains(searchString.ToUpper()));
							break;
					}
					break;
			}

			return vals.ToList();
		}
		protected override List<Row> FilterValues(IEnumerable<Row> vals)
		{
			string client = Request["client"];
			string port = Request["port"];
			string docName = Request["docName"];
			string shipNamespace = Request["shipNamespace"];
			string purpose = Request["purpose"];
			string splitBy = Request["splitBy"];
			if (!string.IsNullOrWhiteSpace(client))
				vals = vals.Where(c => c.client.ToUpper().Contains(client.ToUpper()));
			if (!string.IsNullOrWhiteSpace(port))
				vals = vals.Where(c => c.port.ToUpper().Contains(port.ToUpper()));
			if (!string.IsNullOrWhiteSpace(docName))
				vals = vals.Where(c => c.docName.ToUpper().Contains(docName.ToUpper()));
			if (!string.IsNullOrWhiteSpace(shipNamespace))
				vals = vals.Where(c => c.shipNamespace.ToUpper().Contains(shipNamespace.ToUpper()));
			if (!string.IsNullOrWhiteSpace(purpose))
				vals = vals.Where(c => c.purpose.ToUpper().Contains(purpose.ToUpper()));
			if (!string.IsNullOrWhiteSpace(splitBy))
				vals = vals.Where(c => c.splitBy.ToUpper().Contains(splitBy.ToUpper()));
			return vals.ToList();
		}

		protected override List<string> GetExportCsvColumns(Row row)
		{
			return new List<string> { row.client, row.purpose, row.port, row.docName, row.shipNamespace,  row.splitBy};
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
						data.AddLast(new Row { id = id, client = fields[0], purpose = fields[1], port = fields[2], docName = fields[3], shipNamespace = fields[4], splitBy = fields[5]});
					}
				}
			return data;
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
				if (string.IsNullOrWhiteSpace(value.purpose))
				{
					value.purpose = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.shipNamespace))
				{
					value.purpose = DEFAULTVALUE_ANY;
				}
				if (string.IsNullOrWhiteSpace(value.docName))
				{
					value.docName = DEFAULTVALUE_ANY;
				}
				else if (DocNames.All(x => x.docName != value.docName))
				{
					throw new InvalidOperationException($"The Document Name \"{value.docName}\" is invalid in row {count} of the csv file.");
				}
				if (SplitOptions.All(x => x.splitOption != value.splitBy))
				{
					throw new InvalidOperationException($"The Split By \"{value.splitBy}\" is invalid in row {count} of the csv file.");
				}
				node = node.Next;
				count += 1;
			}
		}

		protected override void ValidateCsvHeaders(string[] fieldNames)
		{

			if (!Enumerable.SequenceEqual(fieldNames, new[] { "Client ID", "Purpose", "Port/Country", "Document Name", "UShipment NameSpace", "Split By" }))
			{
				throw new InvalidOperationException($"The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\", \"Split By\"");
			}
		}
	}
}
