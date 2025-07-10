
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryPayInfo))]
	sealed class CusEntryPayInfoClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var entryHeader = (CusEntryHeader)NewParentObject();
			return entryHeader.EntryPayInfos.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = dec.PK;
			return entryHeader;
		}
	}
}
