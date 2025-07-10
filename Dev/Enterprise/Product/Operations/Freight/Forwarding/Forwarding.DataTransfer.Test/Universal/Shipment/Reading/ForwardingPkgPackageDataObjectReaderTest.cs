using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.DataTransfer.Universal.Testing;
using Enterprise.Registry.Business.Customs;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingPkgPackageDataObjectReaderTest : MatchAndRelabelOnPreviousPackageIDTest<ForwardingPackageJob>
	{
		protected override void SetUp()
		{
			base.SetUp();
			getValidImportedInnerPackages = packingLine => packingLine?.PackingLineCollection ?? Enumerable.Empty<PackingLine>();
			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.SaveForTesting();

			packageJob = Factory.New<ForwardingPackageJob>();
			packageJob.KJ_JobID = shipment.JobNumber;
			packageJob.KJ_ParentID = shipment.PK;
			packageJob.KJ_ParentTableCode = shipment.TablePrefix;

			transitWarehouseAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();
		}

		Func<PackingLine, IEnumerable<PackingLine>> getValidImportedInnerPackages;
		ForwardingShipment shipment;
		ForwardingPackageJob packageJob;
		OrgAddress transitWarehouseAddress;

		protected override ForwardingPackageJob PackageJob => packageJob;

		protected override DataObjectReader<IDataObject, ForwardingPackageJob> GetReader(DataObjectList<PackingLine> packingLineCollection)
		{
			var packagesWrapper = new PackagesWrapper(new UniversalShipment(), packingLineCollection);
			return new ForwardingPkgPackageJobDataObjectReader(packagesWrapper, Logger, Factory, shipment, packingLineCollection.Content == CollectionContent.Complete, transitWarehouseAddress, getValidImportedInnerPackages);
		}

		public void TestImportCompletePackages()
		{
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OA_LastKnownTransitWarehouseAddress = transitWarehouseAddress.PK;
			var pkgPackage = PackageJob.Packages.AddNew("PLT", "ABC");
			var pkgPackage2 = PackageJob.Packages.AddNew("PLT", "DEF");
			var pkgPackageOther = PackageJob.Packages.AddNew("PLT", "XXX");
			packLine.PkgPackageCollection.Add(pkgPackage);
			packLine.PkgPackageCollection.Add(pkgPackage2);
			packLine.PkgPackageCollection.Add(pkgPackageOther);
			Factory.SaveForTesting();
			var packingLine = CreatePackingLine("DEF", 1, "PLT", 150m, "ABC");
			var packingLine2 = CreatePackingLine("GHI", 1, "PLT", 200m, "DEF");
			var packingLineCollection = new DataObjectList<PackingLine>(new PackingLine[2] { packingLine, packingLine2 }) { Content = CollectionContent.Complete };
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals(2, PackageJob.Packages.Count);
			AssertEquals(2, packLine.PkgPackageCollection.Count);
			AssertNull("Package ID: ABC", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "ABC"));
			AssertNotNull("Package ID: DEF", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "DEF"));
			AssertNotNull("Package ID: GHI", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "GHI"));
			AssertNull("Package ID: XXX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "XXX"));
		}

		public void TestImportPartialPackages()
		{
			var packLine = shipment.OuterPackLines.AddNew();
			var pkgPackage = PackageJob.Packages.AddNew("PLT", "ABC");
			var pkgPackage2 = PackageJob.Packages.AddNew("PLT", "DEF");
			var pkgPackageOther = PackageJob.Packages.AddNew("PLT", "XXX");
			packLine.PkgPackageCollection.Add(pkgPackage);
			packLine.PkgPackageCollection.Add(pkgPackage2);
			packLine.PkgPackageCollection.Add(pkgPackageOther);
			Factory.SaveForTesting();
			var packingLine = CreatePackingLine("DEF", 1, "PLT", 150m, "ABC");
			var packingLine2 = CreatePackingLine("GHI", 1, "PLT", 200m, "DEF");
			var packingLineCollection = new DataObjectList<PackingLine>(new PackingLine[2] { packingLine, packingLine2 }) { Content = CollectionContent.Partial };
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals(3, PackageJob.Packages.Count);
			AssertEquals(3, packLine.PkgPackageCollection.Count);
			AssertNull(PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "ABC"));
			AssertNotNull(PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "DEF"));
			AssertNotNull(PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "GHI"));
			AssertNotNull(PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "XXX"));
		}

		public void TestThrowImportFailureIfThereIsNoPackLineAndNoRefNumInDataObject()
		{
			var imporetdHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU1",
			};

			var imporetdHU2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = null,
			};
			imporetdHU2.PackingLineID = null;

			AssertExceptionThrown<DataObjectReadFailureException>("Should not accept packing line elements with no reference number and no packline id",
				"There are packages lacking both packline Id and reference number",
				() => ForwardingPkgPackageJobDataObjectReader.ThrowImportFailureIfThereIsNoPacklineIdAndNoReferenceNumberForOnePackageInDataObject(new DataObjectList<PackingLine>() { imporetdHU1, imporetdHU2 }));
		}

		public void TestThrowImportFailureIfPacklinenumberIsNotUniqueForPackagesWithNoRefNumInDataObject()
		{
			var imporetdHU1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = "HU1",
			};
			imporetdHU1.PackingLineID = Guid.NewGuid().ToString();

			var imporetdHU2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = null,
			};
			imporetdHU2.PackingLineID = Guid.NewGuid().ToString();

			var imporetdHU3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = PackType.Freight.PLT },
				ReferenceNumber = null,
			};
			imporetdHU3.PackingLineID = imporetdHU2.PackingLineID;

			AssertExceptionThrown<DataObjectReadFailureException>("Should not accept packing line elements with no reference number and packline id",
				"Packline id is not unique among packages with no reference number",
				() => ForwardingPkgPackageJobDataObjectReader.ThrowImportFailureIfThereAreTwoPackagesWithNoReferenceNumberButSameRefNumberInDataObject(new DataObjectList<PackingLine>() { imporetdHU1, imporetdHU2, imporetdHU3 }));
		}

		[ExpectNoExceptions]
		public void TestPackedPackageShouldHaveSameTopLevelHandlingUnitAsItsHandlingUnit()
		{
			var pkgPackage1 = PackageJob.Packages.AddNew("PLT", "XXX");
			var pkgPackage1Inners = new[] { PackageJob.Packages.AddNew("PLT", "GHI") };
			foreach (var pkgPackage1Inner in pkgPackage1Inners)
			{
				pkgPackage1Inner.KP_KP_TopHandlingUnitPackage = pkgPackage1.PK;

				var divot = pkgPackage1.PackageHandlingUnitHandlingUnitDivots.AddNew();
				divot.KPD_KP_Package = pkgPackage1Inner.PK;
				divot.KPD_PackedTime = ZDateTimeOffset.Now;
				divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;
			}

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OA_LastKnownTransitWarehouseAddress = transitWarehouseAddress.PK;
			packLine.PkgPackageCollection.Add(pkgPackage1);

			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("YYY", 1, "PLT", 300m);
			var packingLine1Inners = new List<PackingLine> { CreatePackingLine("GHI", 1, "PLT", 100m) };
			packingLine1.SetPackingLineCollection(() => packingLine1Inners);
			var packingLine2 = CreatePackingLine("XXX", 1, "PLT", 300m);

			var packingLineCollection = new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2 }) { Content = CollectionContent.Complete };
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();
		}

		public void TestPopulatePackageByPackingLineID()
		{
			var packLine = shipment.OuterPackLines.AddNew();
			var pkgPackage = PackageJob.Packages.AddNew("PLT", 10);
			packLine.PkgPackageCollection.Add(pkgPackage);
			Factory.SaveForTesting();

			var packingLine = CreatePackingLine("", 5, "PLT", 150m);
			packingLine.PackingLineID = pkgPackage.KP_ExternalReference;
			var packingLineCollection = new DataObjectList<PackingLine>(new PackingLine[] { packingLine });
			var reader = GetReader(packingLineCollection);
			var newPackageJob = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Shipment should have only one package.", 1, newPackageJob.Packages.Count);
			AssertEquals("The only package's qty should be 5.", 5, newPackageJob.Packages.First().KP_PackageQty);
		}

		#region Overpack

		public void TestOverpack_ImportCompletePackages()
		{
			var pkgPackage1 = PackageJob.Packages.AddNew("PLT", "ABC");
			var pkgPackage2 = PackageJob.Packages.AddNew("PLT", "DEF");
			var pkgPackageInner1 = PackageJob.Packages.AddNew("PLT", "GHI");
			var pkgPackageInner2 = PackageJob.Packages.AddNew("PLT", "JKL");

			var pkgPackageOverpack = PackageJob.Packages.AddNew("PLT", "XXX");
			foreach (var pkgPackageInner in new[] { pkgPackageInner1, pkgPackageInner2 })
			{
				pkgPackageInner.KP_KP_TopHandlingUnitPackage = pkgPackageOverpack.PK;

				var divot = pkgPackageOverpack.PackageHandlingUnitHandlingUnitDivots.AddNew();
				divot.KPD_KP_Package = pkgPackageInner.PK;
				divot.KPD_PackedTime = ZDateTimeOffset.Now;
				divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;
			}

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OA_LastKnownTransitWarehouseAddress = transitWarehouseAddress.PK;
			packLine.PkgPackageCollection.Add(pkgPackage1);
			packLine.PkgPackageCollection.Add(pkgPackage2);
			packLine.PkgPackageCollection.Add(pkgPackageOverpack);

			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("DEF", 1, "PLT", 150m, "ABC");
			var packingLine2 = CreatePackingLine("MNO", 1, "PLT", 200m);
			var packingLine3 = CreatePackingLine("GHI", 1, "PLT", 200m);
			var packingLine4 = CreatePackingLine("XXX", 1, "PLT", 200m);
			var packingLineOverpack = CreatePackingLine("PQR", 1, "PLT", 300m, "DEF");
			var packingLineInner1 = CreatePackingLine("STU", 1, "PLT", 100m);
			var packingLineInner2 = CreatePackingLine("VWX", 1, "PLT", 100m);
			packingLineOverpack.SetPackingLineCollection(() => new List<PackingLine> { packingLineInner1, packingLineInner2 });

			var packingLineCollection = new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2, packingLine3, packingLine4, packingLineOverpack }) { Content = CollectionContent.Complete };
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(7, PackageJob.Packages.Count);
			AssertEquals(3, packLine.PkgPackageCollection.Count);
			AssertNull("Package ID: ABC", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "ABC"));
			AssertNotNull("Package ID: DEF", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "DEF"));
			AssertNull("Package ID: JKL", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "JKL"));
			AssertNotNull("Package ID: XXX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "XXX"));
			AssertNotNull("Package ID: MNO", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "MNO"));
			AssertNotNull("Package ID: PQR", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "PQR"));
			AssertNotNull("Package ID: STU", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "STU"));
			AssertNotNull("Package ID: VWX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "VWX"));

			var package = PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "GHI");
			CombineAssertions("Package ID: GHI", () =>
			{
				AssertNotNull(package);
				AssertEquals("KP_KP_TopHandlingUnitPackage", ZGuid.Empty, package.KP_KP_TopHandlingUnitPackage);
			});
		}

		public void TestOverpack_ImportPartialPackages()
		{
			var pkgPackage1 = PackageJob.Packages.AddNew("PLT", "ABC");
			var pkgPackage2 = PackageJob.Packages.AddNew("PLT", "DEF");
			var pkgPackageInner1 = PackageJob.Packages.AddNew("PLT", "GHI");
			var pkgPackageInner2 = PackageJob.Packages.AddNew("PLT", "JKL");

			var pkgPackageOverpack = PackageJob.Packages.AddNew("PLT", "XXX");
			foreach (var pkgPackageInner in new[] { pkgPackageInner1, pkgPackageInner2 })
			{
				pkgPackageInner.KP_KP_TopHandlingUnitPackage = pkgPackageOverpack.PK;

				var divot = pkgPackageOverpack.PackageHandlingUnitHandlingUnitDivots.AddNew();
				divot.KPD_KP_Package = pkgPackageInner.PK;
				divot.KPD_PackedTime = ZDateTimeOffset.Now;
				divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;
			}

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.PkgPackageCollection.Add(pkgPackage1);
			packLine.PkgPackageCollection.Add(pkgPackage2);
			packLine.PkgPackageCollection.Add(pkgPackageOverpack);

			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("DEF", 1, "PLT", 150m, "ABC");
			var packingLine2 = CreatePackingLine("MNO", 1, "PLT", 200m);
			var packingLineOverpack = CreatePackingLine("PQR", 1, "PLT", 300m, "DEF");
			var packingLineInner1 = CreatePackingLine("STU", 1, "PLT", 100m);
			var packingLineInner2 = CreatePackingLine("VWX", 1, "PLT", 100m);
			packingLineOverpack.SetPackingLineCollection(() => new List<PackingLine> { packingLineInner1, packingLineInner2 });

			var packingLineCollection = new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2, packingLineOverpack }) { Content = CollectionContent.Partial };
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(8, PackageJob.Packages.Count);
			AssertEquals(3, packLine.PkgPackageCollection.Count);
			AssertNull("Package ID: ABC", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "ABC"));
			AssertNotNull("Package ID: DEF", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "DEF"));
			AssertNotNull("Package ID: GHI", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "GHI"));
			AssertNotNull("Package ID: JKL", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "JKL"));
			AssertNotNull("Package ID: XXX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "XXX"));
			AssertNotNull("Package ID: MNO", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "MNO"));
			AssertNotNull("Package ID: PQR", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "PQR"));
			AssertNotNull("Package ID: STU", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "STU"));
			AssertNotNull("Package ID: VWX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "VWX"));
		}

		public void TestOverpack_ImportValidInnerPackages()
		{
			var pkgPackage1 = PackageJob.Packages.AddNew("PLT", "ABC");
			var pkgPackage2 = PackageJob.Packages.AddNew("PLT", "DEF");
			var pkgPackageInner1 = PackageJob.Packages.AddNew("PLT", "GHI");
			var pkgPackageInner2 = PackageJob.Packages.AddNew("PLT", "JKL");

			var pkgPackageOverpack = PackageJob.Packages.AddNew("PLT", "XXX");
			foreach (var pkgPackageInner in new[] { pkgPackageInner1, pkgPackageInner2 })
			{
				pkgPackageInner.KP_KP_TopHandlingUnitPackage = pkgPackageOverpack.PK;

				var divot = pkgPackageOverpack.PackageHandlingUnitHandlingUnitDivots.AddNew();
				divot.KPD_KP_Package = pkgPackageInner.PK;
				divot.KPD_PackedTime = ZDateTimeOffset.Now;
				divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;
			}

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OA_LastKnownTransitWarehouseAddress = transitWarehouseAddress.PK;
			packLine.PkgPackageCollection.Add(pkgPackage1);
			packLine.PkgPackageCollection.Add(pkgPackage2);
			packLine.PkgPackageCollection.Add(pkgPackageOverpack);

			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("DEF", 1, "PLT", 150m, "ABC");
			var packingLine2 = CreatePackingLine("MNO", 1, "PLT", 200m);
			var packingLineOverpack = CreatePackingLine("PQR", 1, "PLT", 300m, "DEF");
			var packingLineInner1 = CreatePackingLine("STU", 1, "PLT", 100m);
			var packingLineInner2 = CreatePackingLine("VWX", 1, "PLT", 100m);
			packingLineOverpack.SetPackingLineCollection(() => new List<PackingLine> { packingLineInner1, packingLineInner2 });

			var packingLineCollection = new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2, packingLineOverpack }) { Content = CollectionContent.Complete };
			getValidImportedInnerPackages = packingLine => new[] { packingLineInner1 };
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(4, PackageJob.Packages.Count);
			AssertEquals(2, packLine.PkgPackageCollection.Count);
			AssertNull("Package ID: ABC", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "ABC"));
			AssertNotNull("Package ID: DEF", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "DEF"));
			AssertNull("Package ID: GHI", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "GHI"));
			AssertNull("Package ID: JKL", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "JKL"));
			AssertNull("Package ID: XXX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "XXX"));
			AssertNotNull("Package ID: MNO", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "MNO"));
			AssertNotNull("Package ID: PQR", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "PQR"));
			AssertNotNull("Package ID: STU", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "STU"));
			AssertNull("Package ID: VWX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "VWX"));
		}

		public void TestOverpack_RemoveInnerPackagesFromPackLines()
		{
			var pkgPackage1 = PackageJob.Packages.AddNew("PLT", "ABC");
			var pkgPackage2 = PackageJob.Packages.AddNew("PLT", "DEF");
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.PkgPackageCollection.Add(pkgPackage1);
			packLine.PkgPackageCollection.Add(pkgPackage2);

			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("DEF", 1, "PLT", 150m);
			var packingLine2 = CreatePackingLine("MNO", 1, "PLT", 200m);
			var packingLineInner1 = CreatePackingLine("ABC", 1, "PLT", 100m);
			var packingLineInner2 = CreatePackingLine("VWX", 1, "PLT", 100m);
			var packingLineOverpack = CreatePackingLine("PQR", 1, "PLT", 300m);
			packingLineOverpack.SetPackingLineCollection(() => new List<PackingLine> { packingLineInner1, packingLineInner2 });
			var packingLineCollection = new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2, packingLineOverpack }) { Content = CollectionContent.Complete };
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(5, PackageJob.Packages.Count);
			AssertNotNull("Package ID: ABC", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "ABC"));
			AssertNotNull("Package ID: DEF", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "DEF"));
			AssertNotNull("Package ID: MNO", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "MNO"));
			AssertNotNull("Package ID: VWX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "VWX"));
			AssertNotNull("Package ID: PQR", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "PQR"));
			AssertEquals(1, packLine.PkgPackageCollection.Count);
			AssertNull("PackLine Package ID: ABC", packLine.PkgPackageCollection.FirstOrDefault(t => t.KP_PackageID == "ABC"));
			AssertNotNull("PackLine Package ID: DEF", packLine.PkgPackageCollection.FirstOrDefault(t => t.KP_PackageID == "DEF"));
		}

		public void TestOverpack_ImportCompletePackages_InnersWithoutPackageID()
		{
			var pkgPackage1 = PackageJob.Packages.AddNew("PLT", "ABC");
			var pkgPackage2 = PackageJob.Packages.AddNew("PLT", "DEF");
			var pkgPackageInner1 = PackageJob.Packages.AddNew("PLT", ZString.Empty);
			var pkgPackageInner2 = PackageJob.Packages.AddNew("PLT", ZString.Empty);

			var pkgPackageOverpack = PackageJob.Packages.AddNew("PLT", "XXX");
			foreach (var pkgPackageInner in new[] { pkgPackageInner1, pkgPackageInner2 })
			{
				pkgPackageInner.KP_KP_TopHandlingUnitPackage = pkgPackageOverpack.PK;

				var divot = pkgPackageOverpack.PackageHandlingUnitHandlingUnitDivots.AddNew();
				divot.KPD_KP_Package = pkgPackageInner.PK;
				divot.KPD_PackedTime = ZDateTimeOffset.Now;
				divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;
			}

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OA_LastKnownTransitWarehouseAddress = transitWarehouseAddress.PK;
			packLine.PkgPackageCollection.Add(pkgPackage1);
			packLine.PkgPackageCollection.Add(pkgPackage2);
			packLine.PkgPackageCollection.Add(pkgPackageOverpack);

			Factory.SaveForTesting();

			var packingLine1 = CreatePackingLine("DEF", 1, "PLT", 150m, "ABC");
			var packingLine2 = CreatePackingLine("MNO", 1, "PLT", 200m);
			var packingLine3 = CreatePackingLine("GHI", 1, "PLT", 200m);
			var packingLine4 = CreatePackingLine("XXX", 1, "PLT", 200m);
			var packingLineOverpack = CreatePackingLine("PQR", 1, "PLT", 300m, "DEF");
			var packingLineInner1 = CreatePackingLine(ZString.Empty, 1, "PLT", 100m);
			var packingLineInner2 = CreatePackingLine(ZString.Empty, 1, "PLT", 100m);
			packingLineOverpack.SetPackingLineCollection(() => new List<PackingLine> { packingLineInner1, packingLineInner2 });

			var packingLineCollection = new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2, packingLine3, packingLine4, packingLineOverpack }) { Content = CollectionContent.Complete };
			var reader = GetReader(packingLineCollection);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(7, PackageJob.Packages.Count);
			AssertEquals(3, packLine.PkgPackageCollection.Count);
			AssertNull("Package ID: ABC", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "ABC"));
			AssertNotNull("Package ID: DEF", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "DEF"));
			AssertNotNull("Package ID: XXX", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "XXX"));
			Assert("Package ID: XXX Inner 1", pkgPackageInner1.IsDeleted);
			Assert("Package ID: XXX Inner 2", pkgPackageInner2.IsDeleted);
			AssertNotNull("Package ID: MNO", PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "MNO"));

			var package = PackageJob.Packages.FirstOrDefault(t => t.KP_PackageID == "PQR");
			AssertNotNull("Package ID: PQR", package);
			AssertEquals("Package ID: PQR Inners", 2, package.PackageHandlingUnitHandlingUnitDivots.Count);
		}

		#endregion

		public void TestPopulatePackages_RemoveUnprocessedExistedPackges()
		{
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackLineId = "WUT2C400000857";
			var pkgPackage1 = PackageJob.Packages.AddNew("PLT", 10);
			pkgPackage1.KP_ExternalReference = "WUT2C400000857";
			packLine1.PkgPackageCollection.Add(pkgPackage1);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackLineId = "WUT2C400000858";
			var pkgPackage2 = PackageJob.Packages.AddNew("PLT", 30);
			pkgPackage2.KP_ExternalReference = "WUT2C400000859";
			pkgPackage2.KP_PreviousPackLineID = "WUT2C400000858";
			packLine2.PkgPackageCollection.Add(pkgPackage2);
			Factory.SaveForTesting();

			var xmlPackage1 = CreatePackingLine("", 5, "PLT", 150m);
			xmlPackage1.PreviousPackingLineID = "WUT2C400000857";
			xmlPackage1.PackingLineID = "WUT2C400000860";
			var xmlPackage2 = CreatePackingLine("", 5, "PLT", 150m);
			xmlPackage2.PreviousPackingLineID = "WUT2C400000858";
			xmlPackage2.PackingLineID = "WUT2C400000861";
			var xmlPackageCollection = new DataObjectList<PackingLine>(new PackingLine[] { xmlPackage1, xmlPackage2 }) { Content = CollectionContent.Complete };
			var reader = GetReader(xmlPackageCollection);
			var newPackageJob = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Shipment should have only 2 packages.", 2, newPackageJob.Packages.Count);
			var newPackageIds = new HashSet<ZGuid>();
			foreach (var package in newPackageJob.Packages)
			{
				newPackageIds.Add(package.PK);
			}
			Assert("pkgPackage1 has been removed.", !newPackageIds.Contains(pkgPackage1.PK));
			Assert("pkgPackage2 has been removed.", !newPackageIds.Contains(pkgPackage2.PK));
		}

		static PackingLine CreatePackingLine(string referenceNumber, long packQty, string packType, decimal weight, string previousPackageID = null)
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = referenceNumber,
				PackQty = packQty,
				PackType = new PackageType
				{
					Code = packType
				},
				Weight = weight
			};

			if (previousPackageID != null)
			{
				packingLine.SetAddInfoCollection(() => new List<AddInfo>
				{
					new AddInfo
					{
						Key = "PreviousPackageID",
						Value = previousPackageID
					}
				});
			}

			return packingLine;
		}
	}
}
