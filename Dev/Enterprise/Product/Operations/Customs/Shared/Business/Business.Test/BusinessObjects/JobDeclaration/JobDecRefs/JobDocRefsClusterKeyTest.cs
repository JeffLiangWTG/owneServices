using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDecRefs))]
	sealed class JobDocRefsClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		public void TestSetClusterKeyOnNewJobDecRef()
		{
			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("ClusterKey", 0, ClusterKeyEntityToTest.J3_ClusterKey);
				AssertEquals("Parent ClusterKey", 0, ClusterKeyEntityToTest.Declaration.JE_ClusterKey);
			});

			Factory.Save();

			CombineAssertions("After Save", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.J3_ClusterKey);
				AssertEquals("Parent ClusterKey", 1, ClusterKeyEntityToTest.Declaration.JE_ClusterKey);
			});
		}

		public void TestSetClusterKeyOnExistingJobDecRef()
		{
			var dec1 = ClusterKeyEntityToTest.Declaration;

			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("ClusterKey", 0, ClusterKeyEntityToTest.J3_ClusterKey);
				AssertEquals("Parent 1 ClusterKey", 0, dec1.JE_ClusterKey);
			});

			Factory.Save();

			CombineAssertions("After 1st Save", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.J3_ClusterKey);
				AssertEquals("Parent 1 ClusterKey", 1, dec1.JE_ClusterKey);
			});

			var dec2 = Factory.New<BaseJobDeclaration>();
			ClusterKeyEntityToTest.J3_JE = dec2.PK;

			CombineAssertions("After changing FK to antoher Cluster Parent, but before saving it.", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.J3_ClusterKey);
				AssertEquals("Parent 1 ClusterKey", 1, dec1.JE_ClusterKey);
				AssertEquals("Parent 2 ClusterKey", 0, dec2.JE_ClusterKey);
			});

			Factory.Save();

			CombineAssertions("After changing FK to another Cluster Parent and saving it.", () =>
			{
				AssertEquals("ClusterKey", 2, ClusterKeyEntityToTest.J3_ClusterKey);
				AssertEquals("Parent 1 ClusterKey", 1, dec1.JE_ClusterKey);
				AssertEquals("Parent 2 ClusterKey", 2, dec2.JE_ClusterKey);
			});
		}

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity() => ((BaseJobDeclaration)NewParentObject()).DeclarationRefs.AddNew();

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<BaseJobDeclaration>();

		new JobDecRefs ClusterKeyEntityToTest => (JobDecRefs)base.ClusterKeyEntityToTest;
	}
}
