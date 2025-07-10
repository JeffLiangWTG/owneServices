using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(TransitJobInvoicingSupporter<WhsItemReceiveTransportationUnit>))]
	public class WhsItemReceiveTransportationUnitInvoicingSupporterTest : TransitJobInvoicingSupporterTest<WhsItemReceiveTransportationUnit>
	{
		protected override IJobInvoicingSupporter GetInvoicingSupporter(WhsItemReceiveTransportationUnit transportationUnit) => new TransitJobInvoicingSupporter<WhsItemReceiveTransportationUnit>(transportationUnit);

		#region Implementation

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.TransitReceiveTransportationUnit;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.WhsItemReceiveTransportationUnitAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.WhsItemReceiveTransportationUnitJobInvoicing;

		#endregion
	}
}
