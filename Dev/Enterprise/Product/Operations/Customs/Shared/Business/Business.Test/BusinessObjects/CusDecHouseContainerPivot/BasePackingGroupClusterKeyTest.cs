using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BasePackingGroup))]
	sealed class BasePackingGroupClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var basePackage = ((BasePackingGroup)ClusterKeyEntityToTest).Packages.AddNew();
			basePackage.CW_PackQty = 1;
			return new IClusterKeyWorker[] { basePackage };
		}

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var bill = (Bill)NewParentObject();
			return bill.PackingGroups.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			return dec.Bills.AddNew();
		}
	}
}
