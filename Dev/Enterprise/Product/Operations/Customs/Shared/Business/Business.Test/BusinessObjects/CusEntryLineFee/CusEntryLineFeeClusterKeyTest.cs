using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	sealed class CusEntryLineFeeClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		public void TestSetClusterKeyOnAncesterObject()
		{
			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("ClusterKey", 0, ClusterKeyEntityToTest.CF_ClusterKey);
				AssertEquals("Parent ClusterKey", 0, ClusterKeyEntityToTest.EntryLine.CL_ClusterKey);
				AssertEquals("Grand-Parent ClusterKey", 0, ClusterKeyEntityToTest.EntryLine.Header.CH_ClusterKey);
				AssertEquals("Great-Grand-Parent ClusterKey", 0, ClusterKeyEntityToTest.EntryLine.Header.Declaration.JE_ClusterKey);
			});

			Factory.Save();

			CombineAssertions("After 1st Save", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.CF_ClusterKey);
				AssertEquals("Parent ClusterKey", 1, ClusterKeyEntityToTest.EntryLine.CL_ClusterKey);
				AssertEquals("Grand-Parent ClusterKey", 1, ClusterKeyEntityToTest.EntryLine.Header.CH_ClusterKey);
				AssertEquals("Great-Grand-Parent ClusterKey", 1, ClusterKeyEntityToTest.EntryLine.Header.Declaration.JE_ClusterKey);
			});

			var oldDec = ClusterKeyEntityToTest.EntryLine.Header.Declaration;
			var newDec = Factory.New<BaseJobDeclaration>();
			ClusterKeyEntityToTest.EntryLine.Header.CH_JE = newDec.PK;
			Factory.Save();

			CombineAssertions("After changing an ancester FK to antoher Cluster Parent.", () =>
			{
				AssertEquals("Old Declaration ClusterKey", 1, oldDec.JE_ClusterKey);
				AssertEquals("New Declaration ClusterKey", 2, newDec.JE_ClusterKey);

				AssertEquals("ClusterKey", 2, ClusterKeyEntityToTest.CF_ClusterKey);
				AssertEquals("Parent ClusterKey", 2, ClusterKeyEntityToTest.EntryLine.CL_ClusterKey);
				AssertEquals("Grand-Parent ClusterKey", 2, ClusterKeyEntityToTest.EntryLine.Header.CH_ClusterKey);
				AssertEquals("Great-Grand-Parent ClusterKey", 2, ClusterKeyEntityToTest.EntryLine.Header.Declaration.JE_ClusterKey);
			});
		}

		new CusEntryLineFee ClusterKeyEntityToTest => (CusEntryLineFee)base.ClusterKeyEntityToTest;

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var entryLine = (CusEntryLine)NewParentObject();
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeAmount = 1;
			return fee;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			return entryHeader.MergedLines.AddNew();
		}
	}
}
