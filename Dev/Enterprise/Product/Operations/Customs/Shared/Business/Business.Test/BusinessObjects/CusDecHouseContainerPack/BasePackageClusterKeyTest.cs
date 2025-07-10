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
	[TestedType(typeof(BasePackage))]
	sealed class BasePackageClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => actualParentClusterKeyFksOfCandidateChildren;

		readonly SchemaGuidColumn[] actualParentClusterKeyFksOfCandidateChildren = new SchemaGuidColumn[]
		{
			CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE,
			CusHouseContPackInvoiceLinePivotSchema.CHC_JE,
		};

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var packingGroup = (BasePackingGroup)NewParentObject();
			var package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 1;
			return package;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var bill = dec.Bills.AddNew();
			return bill.PackingGroups.AddNew();
		}
	}
}
