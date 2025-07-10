using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(TransitJobInvoicingSupporter<WhsItemDispatchTransportationUnit>))]
	public class WhsItemDispatchTransportationUnitInvoicingSupporterTest : TransitJobInvoicingSupporterTest<WhsItemDispatchTransportationUnit>
	{
		protected override IJobInvoicingSupporter GetInvoicingSupporter(WhsItemDispatchTransportationUnit transportationUnit) => new TransitJobInvoicingSupporter<WhsItemDispatchTransportationUnit>(transportationUnit);

		#region Implementation

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.TransitDispatchTransportationUnit;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.WhsItemDispatchTransportationUnitAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.WhsItemDispatchTransportationUnitJobInvoicing;

		#endregion
	}
}
