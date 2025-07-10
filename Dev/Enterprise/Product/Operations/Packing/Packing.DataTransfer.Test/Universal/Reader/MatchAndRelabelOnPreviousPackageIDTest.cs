using System.Collections.Generic;
using System.Linq;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	public abstract class MatchAndRelabelOnPreviousPackageIDTest<T> : TestCaseWithFactoryAndMessagingHelpers where T : PkgPackageJob
	{
		protected abstract T PackageJob { get; }

		protected abstract DataObjectReader<IDataObject, T> GetReader(DataObjectList<PackingLine> packingLineCollection);

		#region Test Cases

		public void TestMatchAndRelabelOnPreviousPackageID_RenameTwice_UsesOriginalPackageID()
		{
			var package1 = PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			var package2 = PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "DEF");
			Factory.SaveForTesting();

			var packageData1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "DEF", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData1.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "ABC" }
			});

			var packageData2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "GHI", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 200m };
			packageData2.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "DEF" }
			});

			var packingLineCollection = new DataObjectList<PackingLine>(new PackingLine[] { packageData1, packageData2 });
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();

			// note: this only works due to the deferred trigger on PkgPackageHeader
			Factory.SaveForTesting();

			var defPackage = PackageJob.Packages.First(t => t.KP_PackageID == "DEF");
			var ghiPackage = PackageJob.Packages.First(t => t.KP_PackageID == "GHI");
			AssertEquals("correct weight", 150m, defPackage.KP_Weight);
			AssertEquals("correct weight", 200m, ghiPackage.KP_Weight);
		}

		public void TestMatchAndRelabelOnPreviousPackageID_MultipleReferenceNumbers_ShouldReject()
		{
			var package1 = PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			Factory.SaveForTesting();

			var packageData1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "DEF", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData1.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "ABC" }
			});

			var packageData2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "DEF", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 200m };

			var packingLineCollection = new DataObjectList<PackingLine>(new PackingLine[] { packageData1, packageData2 });
			var reader = GetReader(packingLineCollection);

			AssertExceptionThrown<DataObjectReadFailureException>("correct exception", "There are Packing Line elements with the same Reference Number value. See Reference Number/s: DEF", () => reader.ReadIntoBusinessObject());
		}

		public void TestMatchAndRelabelOnPreviousPackageID_RenameToDuplicate_ShouldPass()
		{
			var package1 = PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			var package2 = PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "DEF");
			Factory.SaveForTesting();

			var packageData = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "DEF", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "ABC" }
			});

			var packingLineCollection = new DataObjectList<PackingLine>(new PackingLine[] { packageData });
			var reader = GetReader(packingLineCollection);

			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
			Factory.SaveForTesting();

			var abcPackage = PackageJob.Packages.First(t => t.KP_PackageID == "ABC");
			var defPackage = PackageJob.Packages.First(t => t.KP_PackageID == "DEF");
			AssertEquals("correct weight", 0m, abcPackage.KP_Weight);
			AssertEquals("correct weight", 150m, defPackage.KP_Weight);
		}

		public void TestMatchAndRelabelOnPreviousPackageID_MultiplePreviousPackageIDs_ShouldNotReject()
		{
			var package1 = PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "ABC");
			Factory.SaveForTesting();

			var packageData1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "DEF", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData1.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "ABC" }
			});
			var packageData2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "GHI", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData2.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "ABC" }
			});

			var packingLineCollection = new DataObjectList<PackingLine>(new PackingLine[] { packageData1, packageData2 });
			var reader = GetReader(packingLineCollection);

			AssertExceptionThrown<DataObjectReadFailureException>("correct exception", "There are Packing Line elements with the same Previous Package ID value. See Previous Package ID/s: ABC", () => reader.ReadIntoBusinessObject());
		}

		public void TestMatchAndRelabelOnPreviousPackageID_MultiplePreviousPackageIDs_AfterRelabelled_ShouldPass()
		{
			var package1 = PackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "XOC-1");
			Factory.SaveForTesting();

			var packageData1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "XOC-1", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData1.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "XOC-9" }
			});
			var packageData2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "XOC-2", PackQty = 1, PackType = new PackageType { Code = "PLT" }, Weight = 150m };
			packageData2.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo() { Key = "PreviousPackageID", Value = "XOC-9" }
			});

			var packingLineCollection = new DataObjectList<PackingLine>(new PackingLine[] { packageData1, packageData2 });
			var reader = GetReader(packingLineCollection);

			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region Logger

		protected TestErrorLogger Logger
		{
			get { return logger ?? (logger = new TestErrorLogger()); }
		}

		TestErrorLogger logger;

		#endregion
	}
}
