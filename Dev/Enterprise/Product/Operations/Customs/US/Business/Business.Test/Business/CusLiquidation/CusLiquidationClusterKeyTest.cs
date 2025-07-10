using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusLiquidation))]
	sealed class CusLiquidationClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		public void TestSetClusterKeyAfterAttachingAndDetachingFromDeclaration()
		{
			var liquidation = Factory.New<CusLiquidation>();
			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("ClusterKey", 0, liquidation.B8_ClusterKey);
			});
			Factory.Save();
			CombineAssertions("After 1st Save", () =>
			{
				AssertEquals("ClusterKey", 0, liquidation.B8_ClusterKey);
			});
			var dec1 = Factory.New<JobDeclaration>();
			liquidation.B8_JE = dec1.PK;
			Factory.Save();
			CombineAssertions("After setting FK to Cluster Parent (attaching).", () =>
			{
				AssertEquals("ClusterKey", 1, liquidation.B8_ClusterKey);
				AssertEquals("Parent FK", dec1.PK, liquidation.B8_JE);
				AssertEquals("Parent ClusterKey", 1, dec1.JE_ClusterKey);
			});
			var dec2 = Factory.New<JobDeclaration>();
			liquidation.B8_JE = dec2.PK;
			Factory.Save();
			CombineAssertions("After setting FK to another Cluster Parent (changing attachment).", () =>
			{
				AssertEquals("ClusterKey", 2, liquidation.B8_ClusterKey);
				AssertEquals("Parent FK", dec2.PK, liquidation.B8_JE);
				AssertEquals("Parent ClusterKey", 2, dec2.JE_ClusterKey);
				AssertEquals("Dec1 (no longer the parent) ClusterKey", 1, dec1.JE_ClusterKey);
			});
			liquidation.B8_JE = ZGuid.Empty;
			Factory.Save();
			CombineAssertions("After setting FK to empty (detaching).", () =>
			{
				AssertEquals("ClusterKey", 0, liquidation.B8_ClusterKey);
				AssertEquals("Parent FK", ZGuid.Empty, liquidation.B8_JE);
				AssertEquals("Dec1 (no longer the parent) ClusterKey", 1, dec1.JE_ClusterKey);
				AssertEquals("Dec2 (no longer the parent) ClusterKey", 2, dec2.JE_ClusterKey);
			});
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var dec = (JobDeclaration)NewParentObject();
			return dec.Liquidations.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<JobDeclaration>();
	}
}
