using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBExportStatement : IAWBExportStatementDetailsProvider
	{
		public ZString Code { get; set; }

		public ZString Statement { get; set; }
	}
}
