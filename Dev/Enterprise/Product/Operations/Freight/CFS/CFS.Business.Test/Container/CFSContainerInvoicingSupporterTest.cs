using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSContainerInvoicingSupporter))]
	public class CFSContainerInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			CFSContainer cfsContainer = Factory.NewWithValidTestData<CFSContainer>();
			return cfsContainer;
		}

		protected override bool ExcludeFromTestBecauseNoBillingTab
		{
			get
			{
				return true;
			}
		}
	}
}
