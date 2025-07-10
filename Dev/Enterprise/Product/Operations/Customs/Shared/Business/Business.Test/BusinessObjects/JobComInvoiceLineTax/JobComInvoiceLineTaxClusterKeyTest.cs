using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	sealed class JobComInvoiceLineTaxClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var invLine = (BaseJobComInvoiceLine)NewParentObject();
			var invLineTax = Factory.New<JobComInvoiceLineTax>();
			invLineTax.JLT_JI = invLine.PK;
			invLineTax.JLT_Type = "X";
			return invLineTax;
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
