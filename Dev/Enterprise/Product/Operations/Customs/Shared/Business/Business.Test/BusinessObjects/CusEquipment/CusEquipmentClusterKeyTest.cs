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
	[TestedType(typeof(CusEquipment))]
	sealed class CusEquipmentClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var dec = (BaseJobDeclaration)NewParentObject();
			var equipment = Factory.New<CusEquipment>();
			equipment.CEQ_JE_Declaration = dec.PK;
			equipment.CEQ_IdentificationNumber = "1";
			return equipment;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<BaseJobDeclaration>();

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => actualParentClusterKeyFksOfCandidateChildren;

		readonly SchemaGuidColumn[] actualParentClusterKeyFksOfCandidateChildren = new SchemaGuidColumn[]
		{
			CusDecHouseContainerPivotSchema.CR_CU_HouseBill,
		};
	}
}
