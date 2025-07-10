using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageJobPackageHeaderPivot))]
	class PkgPackageJobPackageHeaderPivotTest : PackingBusinessObjectTestCase
	{
		#region TestFetchStrategy

		public void TestFetchStrategy()
		{
			AssertEquals(typeof(PkgPackageJobPackageHeaderPivotFetchStrategy), Factory.New<PkgPackageJobPackageHeaderPivot>().FetchStrategy.GetType());
		}

		#endregion

		#region TestIDocumentSupportable

		public void TestIDocumentSupportable()
		{
			var packageHeaderPivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			var documentSupportable = (IDocumentSupportable)packageHeaderPivot;
			AssertEquals(packageHeaderPivot, documentSupportable.DocumentSupporter.BusinessObject);
			AssertEquals(typeof(PkgPackageJobPackageHeaderPivotDocumentSupporter), documentSupportable.DocumentSupporter.GetType());
		}

		#endregion

		#region TestPackageJob

		public void TestPackageJob()
		{
			var pivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			AssertNull(pivot.PackageJob);

			var job = Factory.New<PkgPackageJob>();
			pivot.KPJ_KJ_PackageJob = job.PK;
			AssertEquals(job, pivot.PackageJob);

			pivot.KPJ_KJ_PackageJob = ZGuid.Empty;
			AssertNull(pivot.PackageJob);
		}

		public void TestPackageJob_Sequence()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var header = Factory.New<PkgPackageHeader>();
			header.KPH_PackageID = "LP1";
			var pivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			pivot.KPJ_KPH_PackageHeader = header.PK;
			AssertEquals((ZShort)0, pivot.KPJ_Sequence);

			pivot.KPJ_KJ_PackageJob = Data.PackageJob.PK;
			AssertEquals((ZShort)1, pivot.KPJ_Sequence);

			pivot.KPJ_KJ_PackageJob = Data.PackageJob.PK;
			AssertEquals((ZShort)1, pivot.KPJ_Sequence);

			var dummy2 = Factory.New<DummyWithPacking>();
			dummy2.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummy2);
			var header2 = Factory.New<PkgPackageHeader>();
			header2.KPH_PackageID = "LP2";
			var pivot2 = Helper.CreatePackageHeaderPivot(packageJob2, header2);
			AssertEquals((ZShort)1, pivot2.KPJ_Sequence);

			pivot.KPJ_KJ_PackageJob = packageJob2.PK;
			AssertEquals((ZShort)2, pivot.KPJ_Sequence);

			pivot.KPJ_KJ_PackageJob = ZGuid.Empty;
			AssertEquals((ZShort)0, pivot.KPJ_Sequence);
		}

		#endregion

		#region TestPackageHeader

		public void TestPackageHeader()
		{
			var pivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			AssertNotNull(pivot.PackageHeader);

			var header = Factory.New<PkgPackageHeader>();
			pivot.KPJ_KPH_PackageHeader = header.PK;
			AssertEquals(header, pivot.PackageHeader);

			pivot.KPJ_KPH_PackageHeader = ZGuid.Empty;
			AssertNotNull(pivot.PackageHeader);
			AssertNotEquals(header, pivot.PackageHeader);
		}

		public void TestPackageHeader_Sequence()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var pivot = Factory.New<PkgPackageJobPackageHeaderPivot>();
			pivot.KPJ_KJ_PackageJob = Data.PackageJob.PK;
			AssertEquals((ZShort)0, pivot.KPJ_Sequence);

			var header = Factory.New<PkgPackageHeader>();
			header.KPH_PackageID = "LP1";
			pivot.KPJ_KPH_PackageHeader = header.PK;
			AssertEquals((ZShort)1, pivot.KPJ_Sequence);

			pivot.KPJ_KPH_PackageHeader = ZGuid.Empty;
			AssertEquals((ZShort)0, pivot.KPJ_Sequence);
		}

		#endregion

		#region TestIPackageIDSequence

		public void TestIPackageIDSequence()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var header = Factory.New<PkgPackageHeader>();
			header.KPH_PackageID = "LP1";
			var pivot = Helper.CreatePackageHeaderPivot(Data.PackageJob, header);
			var packageIDSequence = (IPackageSequence)pivot;

			AssertEquals((ZShort)1, packageIDSequence.Sequence);
			AssertEquals(pivot.KPJ_KPH_PackageHeader, packageIDSequence.PackageHeaderFK);
			AssertEquals(pivot.KPJ_KJ_PackageJob, packageIDSequence.PackageJobFK);
			AssertEquals(true, packageIDSequence.RequiresSequencing);

			packageIDSequence.Sequence = 2;
			AssertEquals((ZShort)2, packageIDSequence.Sequence);
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var header = Factory.New<PkgPackageHeader>();
			header.KPH_PackageID = "LP1";
			var header2 = Factory.New<PkgPackageHeader>();
			header2.KPH_PackageID = "LP2";

			var pivot1 = Helper.CreatePackageHeaderPivot(Data.PackageJob, header);
			var pivot2 = Helper.CreatePackageHeaderPivot(Data.PackageJob, header2);

			AssertEquals((ZShort)1, pivot1.KPJ_Sequence);
			AssertEquals((ZShort)2, pivot2.KPJ_Sequence);

			pivot1.Delete();
			AssertEquals((ZShort)1, pivot2.KPJ_Sequence);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var job = factory.NewWithValidTestData<PkgPackageJob>();
			var header = factory.New<PkgPackageHeader>();
			header.KPH_PackageID = "LP1";
			factory.Save();
			var pivot = Helper.CreatePackageHeaderPivot(job, header);

			return pivot;
		}
	}
}
