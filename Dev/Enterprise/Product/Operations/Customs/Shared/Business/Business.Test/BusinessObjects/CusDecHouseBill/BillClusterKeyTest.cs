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
	[TestedType(typeof(Bill))]
	sealed class BillClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var packingGroup = Factory.New<BasePackingGroup>();
			packingGroup.CR_CU_HouseBill = ((Bill)ClusterKeyEntityToTest).PK;
			return new IClusterKeyWorker[] { packingGroup };
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => actualParentClusterKeyFksOfCandidateChildren;

		readonly SchemaGuidColumn[] actualParentClusterKeyFksOfCandidateChildren = new SchemaGuidColumn[]
		{
			JobComInvoiceHeaderSchema.JZ_JE,
		};

		protected override IClusterKeyEntity NewClusterKeyEntity() => ((BaseJobDeclaration)NewParentObject()).Bills.AddNew();
		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<BaseJobDeclaration>();
	}
}
