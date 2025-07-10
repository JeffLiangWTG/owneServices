using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(TransitJobInvoicingSupporter<WhsItemDispatchLoadList>))]
	public class WhsItemWhsItemDispatchLoadListInvoicingSupporterTest : TransitJobInvoicingSupporterTest<WhsItemDispatchLoadList>
	{
		protected override IJobInvoicingSupporter GetInvoicingSupporter(WhsItemDispatchLoadList loadList) => new TransitJobInvoicingSupporter<WhsItemDispatchLoadList>(loadList);

		#region Implementation

		protected override JobInvoicingConsumerType ExpectedConsumerType => JobInvoicingConsumerTypes.TransitDispatchLoadList;

		protected override SecurityCheckpoint ExpectedAuditSecurityCheckpoint => Env.Security.WhsItemDispatchLoadListAuditBilling;

		protected override SecurityCheckpoint ExpectedJobInvoicingSecurityCheckpoint => Env.Security.WhsItemDispatchLoadListJobInvoicing;

		#endregion
	}
}
