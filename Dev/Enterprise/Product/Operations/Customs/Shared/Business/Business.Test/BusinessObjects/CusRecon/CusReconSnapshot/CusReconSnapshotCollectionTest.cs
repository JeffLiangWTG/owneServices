using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusReconSnapshotCollection))]
	class CusReconSnapshotCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconSnapshotCollection>
	{
		public void TestConstructor_CusReconEntryMaster()
		{
			var master = Factory.New<CusReconEntry>();
			var collection = new CusReconSnapshotCollection(master);
			AssertEquals($"CRS_CRE_Entry = '{master.PK}'", collection.CompleteFilter.ParameterisedText.LiteralTextSql);
		}

		public void TestConstructor_CusReconEntryLineMaster()
		{
			var master = Factory.New<CusReconEntryLine>();
			var collection = new CusReconSnapshotCollection(master);
			AssertEquals($"CRS_CRL_Line = '{master.PK}'", collection.CompleteFilter.ParameterisedText.LiteralTextSql);
		}

		public void TestDoNotAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		protected override CusReconSnapshotCollection GetCollectionToTest()
		{
			var master = Factory.New<CusReconEntry>();
			return new CusReconSnapshotCollection(master);
		}
	}
}
