using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvLineRefs))]
	sealed class JobComInvLineRefsClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var invLine = (BaseJobComInvoiceLine)NewParentObject();
			return invLine.InvoiceLineRefs.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var invLine = Factory.New<BaseJobComInvoiceLine>();
			invLine.JI_JZ = invoice.PK;
			return invLine;
		}
	}
}
