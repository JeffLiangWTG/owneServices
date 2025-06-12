using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eServices.eHubDataModel.eHubTransactions;
using CargoWise.eHub.Portal.Controllers.Service;

namespace CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging
{
	public class CarrierHandlingAgentType : DefaultCarrierType
	{
		public CarrierHandlingAgentType(Controller controller, eHubTransactionsContext context)
			: base(controller, context)
		{
		}

		public override string PrefixName
		{
			get { return "1st Phase"; }
		}

		public override string Name
		{
			get { return "Carrier Handling Agent"; }
		}

		public override string ID
		{
			get { return OceanCarrierMessagingController.TypeNames.CarrierHandlingAgent; }
		}

		public override string MatchingGroupRuleName
		{
			get { return "CarrierHandlingAgent"; }
		}

		public override string[] JqGridColumns()
		{
			return new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.carrierAgent, Row.RowNames.eventBranch, Row.RowNames.port, Row.RowNames.shpType, Row.RowNames.docName, Row.RowNames.provider };
		}

		protected override string[] MandatoryColumns()
		{
			return new[] { Row.RowNames.client, Row.RowNames.carrierAgent, Row.RowNames.docName, Row.RowNames.provider };
		}

		protected override List<dynamic> GetJsonObject(IEnumerable<Row> vals)
		{
			return vals.Select(x => new { id = x.id, client = x.client, carrier = x.carrier, carrierAgent = x.carrierAgent, eventBranch = x.eventBranch, port = x.port, shpType = x.shpType, docName = x.docName, provider = x.provider }).ToList<dynamic>();
		}

		public override string GetColumnName(string column)
		{
			if (column == Row.RowNames.carrierAgent) return "Handling Agent C1C";
			return base.GetColumnName(column);
		}

		protected override List<string> GetExportCsvColumns(Row row)
		{
			return new List<string> { row.client, row.carrier, row.carrierAgent, row.eventBranch, row.port, row.shpType, row.docName, row.provider };
		}
	}
}
