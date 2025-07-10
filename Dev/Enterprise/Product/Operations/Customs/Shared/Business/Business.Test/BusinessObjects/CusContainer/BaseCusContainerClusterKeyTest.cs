using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusContainer))]
	sealed class BaseCusContainerClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var container = ClusterKeyEntityToTest;
			var invoice = ((BaseCusContainer)container).Declaration.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			var containerInvLinePivot = invLine.ContainersPivot.AddPivotFor((BaseCusContainer)container);
			return new IClusterKeyWorker[] { containerInvLinePivot };
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => actualParentClusterKeyFksOfCandidateChildren;

		readonly SchemaGuidColumn[] actualParentClusterKeyFksOfCandidateChildren = new SchemaGuidColumn[]
		{
			CusDecHouseContainerPivotSchema.CR_CU_HouseBill,
			JobComInvoiceLineSchema.JI_JZ,
			QuarantineColsDirectionSchema.QCD_QCH_ColsHeader,
		};

		protected override IClusterKeyEntity NewClusterKeyEntity() => ((BaseJobDeclaration)NewParentObject()).CusContainers.AddNew();

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			return dec;
		}
	}
}
