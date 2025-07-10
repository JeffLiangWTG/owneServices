using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerInvoiceLinePivot))]
	sealed class CusContainerInvoiceLinePivotClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var container = (BaseCusContainer)NewParentObject();
			var invoice = container.Declaration.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			return invLine.ContainersPivot.AddPivotFor(container);
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var container = dec.CusContainers.AddNew();
			return container;
		}
	}
}
