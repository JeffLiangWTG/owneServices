using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerDetentionInvoicingSupporter))]
	internal class ContainerDetentionInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			ContainerDetention containerDetention = Factory.NewWithValidTestData<ContainerDetention>();
			return containerDetention;
		}
	}
}
