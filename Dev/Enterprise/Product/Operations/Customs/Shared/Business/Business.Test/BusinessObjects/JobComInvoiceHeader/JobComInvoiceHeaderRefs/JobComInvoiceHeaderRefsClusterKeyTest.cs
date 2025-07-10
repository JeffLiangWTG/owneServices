using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderRefs))]
	sealed class JobComInvoiceHeaderRefsClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var invoice = (BaseJobComInvoiceHeader)NewParentObject();
			return invoice.InvoiceHeaderRefs.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			return invoice;
		}
	}
}
