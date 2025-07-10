using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Packing.Business.Testing
{
	public class PackageSequenceCalculatorTest : PackingTestCaseWithFactory
	{
		public void TestPackageSequeceCalculatorConstructor_PackageJobIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PackageSequenceCalculator(null));
		}

		public void TestSequence_Overflow()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.Outer;

			var packageHeader = Factory.New<PkgPackageHeader>();
			packageHeader.KPH_PackageID = "P1";
			var package = Data.PackageJob.Packages.AddNew();
			package.KP_Sequence = short.MaxValue;
			package.KP_KPH_PackageHeader = packageHeader.PK;

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			AssertEquals(short.MaxValue, packageSequenceCalculator.CachedMaxSequence);

			var overflowPackage = Data.PackageJob.Packages.AddNew("BOX", "P1");
			AssertEquals($"Current Package Job {Data.PackageJob.PK} has too many outer packages, cannot fix the sequence correctly.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertEquals("The sequence of overflow package should be max value", short.MaxValue, overflowPackage.KP_Sequence);
		}

		public void TestSequence_PackageSequenceIsNull()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX", "P1");
			AssertEquals((ZShort)1, package.KP_Sequence);

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			packageSequenceCalculator.Sequence(null);

			AssertEquals("Sequence of Package should not be changed", (ZShort)1, package.KP_Sequence);
		}

		public void TestSequence_PackageNotInPackageJob()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX", "P1");

			var dummy2 = Factory.New<DummyWithPacking>();
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummy2);
			var package2 = packageJob2.Packages.AddNew();
			AssertEquals("Precondition", (ZShort)1, package2.KP_Sequence);

			// reset sequences to 0 for testing
			package2.KP_Sequence = 0;

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			packageSequenceCalculator.Sequence(package2);

			AssertEquals((ZShort)0, package2.KP_Sequence);
		}

		public void TestSequence_SequenceTypeIsConsolidated()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.Consolidated;

			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("BOX", "");
			var package3 = Data.PackageJob.Packages.AddNew("BOX", "P2");

			// reset sequences to 0 for testing
			package1.KP_Sequence = 0;
			package2.KP_Sequence = 0;
			package3.KP_Sequence = 0;

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			packageSequenceCalculator.Sequence(package1);
			packageSequenceCalculator.Sequence(package2);
			packageSequenceCalculator.Sequence(package3);

			AssertEquals("should not set sequence when sequence type is consolidated", (ZShort)0, package1.KP_Sequence);
			AssertEquals("should not set sequence when sequence type is consolidated", (ZShort)0, package2.KP_Sequence);
			AssertEquals("should not set sequence when sequence type is consolidated", (ZShort)0, package3.KP_Sequence);
		}

		public void TestSequence_SequenceTypeIsStandard()
		{
			Data.CreatePackingData();

			AssertEquals(PackageSequenceType.Standard, Data.PackageJob.ParentJob.PackageSequenceType);

			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("BOX", "");
			var package3 = Data.PackageJob.Packages.AddNew("BOX", "P2");

			// reset sequences to 0 for testing
			package1.KP_Sequence = 0;
			package2.KP_Sequence = 0;
			package3.KP_Sequence = 0;

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			packageSequenceCalculator.Sequence(package1);
			packageSequenceCalculator.Sequence(package2);
			packageSequenceCalculator.Sequence(package3);

			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)3, package3.KP_Sequence);
		}

		public void TestSequence_SequenceTypeIsOuter()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.Outer;

			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("BOX", "");
			var package3 = Data.PackageJob.Packages.AddNew("BOX", "P2");

			// reset sequences to 0 for testing
			package1.KP_Sequence = 0;
			package2.KP_Sequence = 0;
			package3.KP_Sequence = 0;

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			packageSequenceCalculator.Sequence(package1);
			packageSequenceCalculator.Sequence(package2);
			packageSequenceCalculator.Sequence(package3);

			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)0, package2.KP_Sequence);
			AssertEquals((ZShort)2, package3.KP_Sequence);
		}

		public void TestSequence_SequenceTypeIsOuterAndLooseID()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("BOX", "");

			var looseIDHeader1 = Factory.New<PkgPackageHeader>();
			looseIDHeader1.KPH_PackageID = "LP1";
			var looseID1 = Helper.CreatePackageHeaderPivot(Data.PackageJob, looseIDHeader1);

			var package3 = Data.PackageJob.Packages.AddNew("BOX", "P2");

			var looseIDHeader2 = Factory.New<PkgPackageHeader>();
			looseIDHeader2.KPH_PackageID = "LP2";
			var looseID2 = Helper.CreatePackageHeaderPivot(Data.PackageJob, looseIDHeader2);

			// reset sequences to 0 for testing
			package1.KP_Sequence = 0;
			package2.KP_Sequence = 0;
			package3.KP_Sequence = 0;
			looseID1.KPJ_Sequence = 0;
			looseID2.KPJ_Sequence = 0;

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			packageSequenceCalculator.Sequence(package1);
			packageSequenceCalculator.Sequence(package2);
			packageSequenceCalculator.Sequence(looseID1);
			packageSequenceCalculator.Sequence(package3);
			packageSequenceCalculator.Sequence(looseID2);

			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)0, package2.KP_Sequence);
			AssertEquals((ZShort)3, package3.KP_Sequence);
			AssertEquals((ZShort)2, looseID1.KPJ_Sequence);
			AssertEquals((ZShort)4, looseID2.KPJ_Sequence);
		}

		public void TestShiftSequence_SequenceIsZero()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("BOX", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("BOX", "P3");
			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)3, package3.KP_Sequence);

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			AssertEquals((ZShort)3, packageSequenceCalculator.CachedMaxSequence);

			// reset sequence to 0 for mimic the package be deleted
			package1.KP_Sequence = 0;

			packageSequenceCalculator.ShiftSequence(0);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)3, package3.KP_Sequence);
		}

		public void TestShiftSequence_SequenceIsLast()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("BOX", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("BOX", "P3");
			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)3, package3.KP_Sequence);

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			AssertEquals((ZShort)3, packageSequenceCalculator.CachedMaxSequence);

			// reset sequence to 0 for mimic the package be deleted
			package3.KP_Sequence = 0;

			packageSequenceCalculator.ShiftSequence(3);
			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
		}

		public void TestShiftSequence()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew("BOX", "P1");
			var package2 = Data.PackageJob.Packages.AddNew("BOX", "P2");
			var package3 = Data.PackageJob.Packages.AddNew("BOX", "P3");
			AssertEquals((ZShort)1, package1.KP_Sequence);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)3, package3.KP_Sequence);

			var packageSequenceCalculator = new PackageSequenceCalculator(Data.PackageJob);
			AssertEquals((ZShort)3, packageSequenceCalculator.CachedMaxSequence);

			// reset sequence to 0 for mimic the package be deleted
			package1.KP_Sequence = 0;

			packageSequenceCalculator.ShiftSequence(1);
			AssertEquals((ZShort)2, package2.KP_Sequence);
			AssertEquals((ZShort)1, package3.KP_Sequence);
		}
	}
}
