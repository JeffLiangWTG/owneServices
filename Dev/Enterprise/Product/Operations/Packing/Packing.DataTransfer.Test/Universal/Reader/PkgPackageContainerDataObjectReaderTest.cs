using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Packing.DataTransfer.Testing
{
	public class PkgPackageContainerDataObjectReaderTest : PkgPackageDataObjectReaderTestCase
	{
		#region TestPkgPackageContainerDataObjectReader_FieldMappings

		public void TestPkgPackageContainerDataObjectReader_FieldMappings()
		{
			AssertPkgPackageContainerDataObjectReader_FieldMappings(shouldImportBookedDimensions: false);
		}

		public void TestPkgPackageContainerDataObjectReader_FieldMappings_ShouldAlsoImportBookedDimensions()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPackingSupportsImportingBookedDimensions);
			var parentJob = Factory.New<DummyWithPackingSupportsImportingBookedDimensions>();
			var pkgPackageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);
			AssertPkgPackageContainerDataObjectReader_FieldMappings(shouldImportBookedDimensions: true, pkgPackageJob);
		}

		void AssertPkgPackageContainerDataObjectReader_FieldMappings(bool shouldImportBookedDimensions, PkgPackageJob customPackageJob = null)
		{
			#region SetupPackingJob

			Data.CreatePackingData();
			Factory.SaveForTesting();

			var pkgPackageJob = customPackageJob ?? Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";

			var container = pkgPackageJob.Packages.AddNew("CNT");
			container.KP_PackageQty = 1;
			container.Container.K0_AirVentFlowRate = 7m;
			container.Container.K0_AirVentFlowRateUnit = "M2";
			container.Container.K0_ContainerMode = "AIR";
			container.KP_DunnageWeight = 8m;
			container.Container.K0_HumidityPercent = 9;
			container.Container.K0_IsControlledAtmosphere = true;
			container.Container.K0_IsDamaged = true;
			container.Container.K0_IsEmpty = true;
			container.Container.K0_IsSealOk = true;
			container.Container.K0_IsShipperOwned = true;
			container.Container.K0_Quality = "RIC";
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			container.Container.K0_RefrigGeneratorID = "REFRIG123";
			container.Container.K0_Seal1 = "SEAL-1";
			container.Container.K0_Seal2 = "SEAL-2";
			container.Container.K0_Seal3 = "SEAL-3";
			container.Container.K0_SetPointTemp = 10m;
			container.Container.K0_SetPointTempUnit = Constants.Temperature.Centigrade;
			container.Container.K0_Status = "ARV";
			container.KP_TareWeight = 11m;
			container.Container.K0_TempRecorderSerialNumber = "TEMPSER123";
			container.KP_DimensionUQ = "M";
			container.KP_Height = 1m;
			container.KP_Length = 2m;
			container.KP_PackageID = "CONT-1";
			container.KP_VolumeUQ = "M3";
			container.KP_Weight = 5m;
			container.KP_WeightUQ = "T";
			container.KP_Width = 6m;
			container.KP_TransportRef = "TRANSPORT REF";
			container.KP_GoodsDescription = "GOODS DESC";
			container.KP_HSCode = "HARMON CODE";
			container.KP_Volume = 12m;
			container.UNDGs.Add(Data.UndgDataItemEXP);
			container.UNDGs.Add(Data.UndgDataItemLOS);
			container.KP_RH_NKCommodityCode = "GEN";
			container.Container.K0_Seal1PartyType = Constants.ContainerSealParties.Codes.CarrierShippingLine;
			container.Container.K0_Seal2PartyType = Constants.ContainerSealParties.Codes.ConsignorShipper;
			container.Container.K0_Seal3PartyType = Constants.ContainerSealParties.Codes.Customs;

			var box = container.Packages.AddNew("BOX");

			#endregion

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var packingLineDO = pkgPackageJobDataObject.ContainerCollection[0];

			var dummy = Factory.New<DummyWithPacking>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);

			var reader = new PkgPackageContainerDataObjectReader(packingLineDO, Logger, Factory, packageJob.Packages);
			var containerRead = reader.ReadIntoBusinessObject();

			#region AssertPackingJob

			AssertNotNull(containerRead);
			AssertNotEquals(container, containerRead);

			CombineAssertions("containerRead", () =>
			{
				AssertEquals("containerRead.KP_DimensionUQ", "M", containerRead.KP_DimensionUQ);
				AssertEquals("containerRead.KP_Height", 1m, containerRead.KP_Height);
				AssertEquals("containerRead.KP_Length", 2m, containerRead.KP_Length);
				AssertEquals("containerRead.KP_PackageID", "CONT-1", containerRead.KP_PackageID);
				AssertEquals("containerRead.KP_PackageQty", 1, containerRead.KP_PackageQty);
				AssertEquals("containerRead.KP_Volume", 12m, containerRead.KP_Volume);
				AssertEquals("containerRead.KP_VolumeUQ", "M3", containerRead.KP_VolumeUQ);
				AssertEquals("containerRead.KP_Weight", 5m, containerRead.KP_Weight);
				AssertEquals("containerRead.KP_WeightUQ", "T", containerRead.KP_WeightUQ);
				AssertEquals("containerRead.KP_Width", 6m, containerRead.KP_Width);
				if (shouldImportBookedDimensions)
				{
					AssertEquals("containerRead.BookedDimensions.KPB_KP_Package", containerRead.PK, containerRead.BookedDimensions.KPB_KP_Package);
					AssertEquals("containerRead.BookedDimensions.KPB_DimensionUQ", "M", containerRead.BookedDimensions.KPB_DimensionUQ);
					AssertEquals("containerRead.BookedDimensions.KPB_Height", 1m, containerRead.BookedDimensions.KPB_Height);
					AssertEquals("containerRead.BookedDimensions.KPB_Length", 2m, containerRead.BookedDimensions.KPB_Length);
					AssertEquals("containerRead.BookedDimensions.KPB_PackageQty", 1, containerRead.BookedDimensions.KPB_PackageQty);
					AssertEquals("containerRead.BookedDimensions.KPB_Volume", 12m, containerRead.BookedDimensions.KPB_Volume);
					AssertEquals("containerRead.BookedDimensions.KPB_VolumeUQ", "M3", containerRead.BookedDimensions.KPB_VolumeUQ);
					AssertEquals("containerRead.BookedDimensions.KPB_Weight", 5m, containerRead.BookedDimensions.KPB_Weight);
					AssertEquals("containerRead.BookedDimensions.KPB_WeightUQ", "T", containerRead.BookedDimensions.KPB_WeightUQ);
					AssertEquals("containerRead.BookedDimensions.KPB_Width", 6m, containerRead.BookedDimensions.KPB_Width);
				}
				AssertEquals("containerRead.KP_TransportRef", "TRANSPORT REF", containerRead.KP_TransportRef);
				AssertEquals("containerRead.KP_GoodsDescription", "GOODS DESC", containerRead.KP_GoodsDescription);
				AssertEquals("containerRead.KP_HSCode", "HARMON CODE", containerRead.KP_HSCode);
				AssertEquals("containerRead.Container.K0_AirVentFlowRate", 7m, containerRead.Container.K0_AirVentFlowRate);
				AssertEquals("containerRead.Container.K0_AirVentFlowRateUnit", "M2", containerRead.Container.K0_AirVentFlowRateUnit);
				AssertEquals("containerRead.Container.K0_ContainerMode", "AIR", containerRead.Container.K0_ContainerMode);
				AssertEquals("containerRead.KP_DunnageWeight", 8m, containerRead.KP_DunnageWeight);
				AssertEquals("containerRead.Container.K0_HumidityPercent", (ZByte)9, containerRead.Container.K0_HumidityPercent);
				AssertEquals("containerRead.Container.K0_IsControlledAtmosphere", true, containerRead.Container.K0_IsControlledAtmosphere);
				AssertEquals("containerRead.Container.K0_IsDamaged", true, containerRead.Container.K0_IsDamaged);
				AssertEquals("containerRead.Container.K0_IsEmpty", true, containerRead.Container.K0_IsEmpty);
				AssertEquals("containerRead.Container.K0_IsSealOk", true, containerRead.Container.K0_IsSealOk);
				AssertEquals("containerRead.Container.K0_IsShipperOwned", true, containerRead.Container.K0_IsShipperOwned);
				AssertEquals("containerRead.Container.K0_Quality", "RIC", containerRead.Container.K0_Quality);
				AssertEquals("containerRead.Container.K0_RC_ContainerType", "20GP", containerRead.Container.ContainerType.RC_Code);
				AssertEquals("containerRead.Container.K0_RefrigGeneratorID", "REFRIG123", containerRead.Container.K0_RefrigGeneratorID);
				AssertEquals("containerRead.Container.K0_Seal1", "SEAL-1", containerRead.Container.K0_Seal1);
				AssertEquals("containerRead.Container.K0_Seal2", "SEAL-2", containerRead.Container.K0_Seal2);
				AssertEquals("containerRead.Container.K0_Seal3", "SEAL-3", containerRead.Container.K0_Seal3);
				AssertEquals("containerRead.Container.K0_SetPointTemp", 10m, containerRead.Container.K0_SetPointTemp);
				AssertEquals("containerRead.Container.K0_SetPointTempUnit", Constants.Temperature.Centigrade, containerRead.Container.K0_SetPointTempUnit);
				AssertEquals("containerRead.Container.K0_Status", "ARV", containerRead.Container.K0_Status);
				AssertEquals("containerRead.KP_TareWeight", 11m, containerRead.KP_TareWeight);
				AssertEquals("containerRead.Container.K0_TempRecorderSerialNumber", "TEMPSER123", containerRead.Container.K0_TempRecorderSerialNumber);
				AssertEquals("Container should Contain 2 Dangerous Goods", 2, containerRead.UNDGs.Count);
				AssertEquals("containerRead.KP_RH_NKCommodityCode", "GEN", containerRead.KP_RH_NKCommodityCode);
				AssertCollectionContains("EXP", containerRead.UNDGs.Select(u => u.Substance.DG_Code));
				AssertCollectionContains("LOS", containerRead.UNDGs.Select(u => u.Substance.DG_Code));
				AssertEquals("container.Container.K0_Seal1PartyType", Constants.ContainerSealParties.Codes.CarrierShippingLine, containerRead.Container.K0_Seal1PartyType);
				AssertEquals("container.Container.K0_Seal2PartyType", Constants.ContainerSealParties.Codes.ConsignorShipper, containerRead.Container.K0_Seal2PartyType);
				AssertEquals("container.Container.K0_Seal3PartyType", Constants.ContainerSealParties.Codes.Customs, containerRead.Container.K0_Seal3PartyType);

				AssertEquals("The Package Job reader will import the inner packages for the container", 0, containerRead.Packages.Count);

				if (!shouldImportBookedDimensions)
				{
					var bookedDimensions = Factory.Load<PkgPackageBookedDetail>(new ZQuery());
					AssertEquals("Given the dummy packing object does not implement IPackingParentSupportsImportingBookedDimensions then the import should not import any booked dimensions.", 0, bookedDimensions.Length);
				}
			});

			#endregion
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_ControllerAtmosphereIsFalseButHasPointTempUnit

		public void TestPkgPackageContainerDataObjectReader_ControllerAtmosphereIsFalseButHasPointTempUnit()
		{
			Data.CreatePackingData();
			var containerDataObject = new Container
			{
				ContainerType = new ContainerType { Code = "20GP" },
				IsControlledAtmosphere = false,
				SetPointTemp = decimal.Zero,
				SetPointTempUnit = Constants.Temperature.Centigrade
			};

			var container = new PkgPackageContainerDataObjectReader(containerDataObject, Logger, Factory, Data.PackageJob.Packages).ReadIntoBusinessObject();
			AssertEquals(false, container.Container.K0_IsControlledAtmosphere);
			AssertEquals(decimal.Zero, container.Container.K0_SetPointTemp);
			AssertEquals(true, container.Container.K0_SetPointTempUnit.IsEmpty);
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReaderWithoutContainerType

		public void TestPkgPackageContainerDataObjectReaderWithoutContainerType()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			AssertPackagesRead("CONTFCL", Constants.ContainerModes.FCL, "20GP", Constants.PkgUnit.Container);
			AssertPackagesRead("CONT123", Constants.ContainerModes.FCL, "", Constants.PkgUnit.Container, typeof(DataObjectReadFailureException), "Cannot import Container 'CONT123 - FCL' without a Container Type.");
			AssertPackagesRead("BBK123", Constants.ContainerModes.BreakBulk, "", Constants.PkgUnit.BreakBulk);
			AssertPackagesRead("BULK123", Constants.ContainerModes.Bulk, "", Constants.PkgUnit.Unit);
			AssertPackagesRead("MILK123", Constants.ContainerModes.Liquid, "", Constants.PkgUnit.Unit);
			AssertPackagesRead("VIN123", Constants.ContainerModes.RollOnRollOff, "", Constants.PkgUnit.Unit);
		}

		void AssertPackagesRead(ZString id, ZString mode, ZString type, ZString expectedPkgType, Type expectedEx = null, string expectedMsg = null)
		{
			var dummy = Factory.New<DummyWithPacking>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);

			var container = new Container();
			container.ContainerNumber = id;
			container.FCL_LCL_AIR = new ContainerMode() { Code = mode };
			if (!type.IsEmpty)
			{
				container.ContainerType = new ContainerType() { Code = type };
			}

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, packageJob.Packages);

			if (expectedEx != null)
			{
				AssertExceptionThrown(expectedEx, expectedMsg, () => reader.ReadIntoBusinessObject());
			}
			else
			{
				PkgPackage packageRead = null;
				AssertNoExceptionThrown(() => packageRead = reader.ReadIntoBusinessObject());

				AssertEquals(expectedPkgType, packageRead.KP_F3_NKPackType);

				if (expectedPkgType == Constants.PkgUnit.Container)
				{
					AssertEquals(mode, packageRead.Container.K0_ContainerMode);
					AssertEquals(type, packageRead.Container.ContainerType.RC_Code);
				}
				else
				{
					AssertNull(mode, packageRead.Container);
				}
			}
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_VolumeIsNotCalculatedFromDimsIfVolumeIsZero

		public void TestPkgPackageContainerDataObjectReader_VolumeIsNotCalculatedFromDimsIfVolumeIsZero()
		{
			Data.CreatePackingData();
			var containerDataObject = new Container
			{
				ContainerType = new ContainerType { Code = "20GP" },
				TotalLength = 5m,
				TotalWidth = 6m,
				TotalHeight = 3m,
				LengthUnit = new UnitOfLength { Code = "M" },
				VolumeUnit = new UnitOfVolume { Code = "CF" }
			};

			var container = new PkgPackageContainerDataObjectReader(containerDataObject, Logger, Factory, Data.PackageJob.Packages).ReadIntoBusinessObject();
			AssertEquals(0m, container.KP_Volume);
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_TargetContainer

		public void TestPkgPackageContainerDataObjectReader_TargetContainer()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var container1 = packageJob.Packages.AddNew("CNT", "CON1");
			container1.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			var container2 = packageJob.Packages.AddNew("CNT", "CON2");
			container2.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			var containerDataObject = new Container
			{
				ContainerNumber = "CON1",
				VolumeCapacity = 5m
			};

			var matchedContainer = new PkgPackageContainerDataObjectReader(containerDataObject, Logger, Factory, Data.PackageJob.Packages, targetContainer: container1).ReadIntoBusinessObject();
			AssertEquals("Should have matched container by target container", container1, matchedContainer);
			AssertEquals(5m, matchedContainer.KP_Volume);
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_TargetContainerIsNull

		public void TestPkgPackageContainerDataObjectReader_TargetContainerIsNull()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var container1 = packageJob.Packages.AddNew("CNT", "CON1");
			container1.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			var container2 = packageJob.Packages.AddNew("CNT", "CON2");
			container2.Container.K0_RC_ContainerType = Data.Container20GP.PK;
			var containerDataObject = new Container
			{
				ContainerNumber = "CON3", // if this was CON2 it would rename it as it's a duplicate
				ContainerCount = 1,
				ContainerType = new ContainerType() { Code = "20GP" },
				VolumeCapacity = 5m
			};

			var matchedContainer = new PkgPackageContainerDataObjectReader(containerDataObject, Logger, Factory, Data.PackageJob.Packages).ReadIntoBusinessObject();
			AssertEquals("Should not match on existing containers", false, new PkgPackage[] { container1, container2 }.Any(c => c.PK == matchedContainer.PK));
			AssertEquals("New package should be added.", "CON3", matchedContainer.KP_PackageID);
			AssertEquals("New package should be added.", 1, matchedContainer.KP_PackageQty);
			AssertEquals("New package should be added.", Data.Container20GP.PK, matchedContainer.Container.K0_RC_ContainerType);
			AssertEquals("New package should be added.", 5m, matchedContainer.KP_Volume);
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_KP_DimensionUQBlankThrowException

		public void TestPkgPackageContainerDataObjectReader_KP_DimensionUQBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_DimensionUQ] CHECK ((KP_Length = 0 AND KP_Width = 0 AND KP_Height = 0) OR KP_DimensionUQ <> '');
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "20GP" },
				TotalLength = 0m,
				TotalWidth = 0m,
				TotalHeight = 0m,
				LengthUnit = new UnitOfLength { Code = "" }
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			container.TotalLength = 45m;
			var expectedErrorMessage = @"A dimension unit of measurement is required if a dimension value is entered.
ID: ABC
Pack Type: CNT
Quantity: 1
Length: 45
Width: 0
Height: 0
Dimension Unit: ";

			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Length > 0 and LengthUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			container.TotalLength = 0m;
			container.TotalWidth = 40m;
			expectedErrorMessage = @"A dimension unit of measurement is required if a dimension value is entered.
ID: ABC
Pack Type: CNT
Quantity: 1
Length: 0
Width: 40
Height: 0
Dimension Unit: ";

			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Width > 0 and LengthUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			container.TotalWidth = 0m;
			container.TotalHeight = 40m;
			expectedErrorMessage = @"A dimension unit of measurement is required if a dimension value is entered.
ID: ABC
Pack Type: CNT
Quantity: 1
Length: 0
Width: 0
Height: 40
Dimension Unit: ";

			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Height > 0 and LengthUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), expectedErrorMessage, () => reader.ReadIntoBusinessObject());

			container.TotalHeight = 0m;
			container.LengthUnit = new UnitOfLength { Code = "M" };
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("All dimensions are zero and LengthUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_KP_HeightLessThanZeroThrowException

		public void TestPkgPackageContainerDataObjectReader_KP_HeightLessThanZeroThrowException()
		{
			// violate CONSTRAINT [Constraint_KP_Height] CHECK (KP_Height >= 0);
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "20GP" },
				TotalHeight = 0m
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Height = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.TotalHeight = 5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Height > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.TotalHeight = -5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Height < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package height must not be negative.
ID: ABC
Pack Type: CNT
Quantity: 1
Height: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_KP_LengthLessThanZeroThrowException

		public void TestPkgPackageContainerDataObjectReader_KP_LengthLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Length] CHECK (KP_Length >= 0);
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "20GP" },
				TotalLength = 0m
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Length = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.TotalLength = 5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Length > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.TotalLength = -5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Length < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package length must not be negative.
ID: ABC
Pack Type: CNT
Quantity: 1
Length: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_KP_VolumeLessThanZeroThrowException

		public void TestPkgPackageContainerDataObjectReader_KP_VolumeLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Volume] CHECK (KP_Volume >= 0);
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "20GP" },
				VolumeCapacity = 0m
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Volume = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.VolumeCapacity = 5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Volume > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.VolumeCapacity = -5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Volume < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package volume must not be negative.
ID: ABC
Pack Type: CNT
Quantity: 1
Volume: -5
Volume Unit: M3", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_KP_VolumeUqBlankThrowException

		public void TestPkgPackageContainerDataObjectReader_KP_VolumeUqBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_VolumeUQ] CHECK (KP_Volume = 0 OR KP_VolumeUQ <> '');
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "20GP" },
				VolumeCapacity = 0m,
				VolumeUnit = new UnitOfVolume { Code = "" }
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.",
				() => reader.ReadIntoBusinessObject());

			container.VolumeCapacity = 45m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Volume > 0 and VolumeUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), @"Volume Unit required if a package volume value is entered.
ID: ABC
Pack Type: CNT
Quantity: 1
Volume: 45
Volume Unit: ", () => reader.ReadIntoBusinessObject());

			container.VolumeCapacity = 0m;
			container.VolumeUnit = new UnitOfVolume { Code = "M3" };
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Volume is zero and VolumeUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_KP_WeightLessThanZeroThrowException

		public void TestPkgPackageContainerDataObjectReader_KP_WeightLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Weight] CHECK (KP_Weight >= 0);
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "20GP" },
				GrossWeight = 0m
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Weight = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.GrossWeight = 5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Weight > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.GrossWeight = -5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Weight < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package weight must not be negative.
ID: ABC
Pack Type: CNT
Quantity: 1
Weight: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_KP_WeightUqBlankThrowException

		public void TestPkgPackageContainerDataObjectReader_KP_WeightUqBlankThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_WeightUQ] CHECK (KP_Weight = 0 OR KP_WeightUQ <> '');
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "20GP" },
				GrossWeight = 0m,
				WeightUnit = new UnitOfWeight { Code = "" }
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Precondition: Initial reader state must be correct.", () => reader.ReadIntoBusinessObject());

			container.GrossWeight = 45m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Weight > 0 and WeightUnit is empty there should be an exception thrown", typeof(DataObjectReadFailureException), @"Weight Unit required if a package weight value is entered.
ID: ABC
Pack Type: CNT
Quantity: 1
Weight: 45
Weight Unit: ", () => reader.ReadIntoBusinessObject());

			container.GrossWeight = 0m;
			container.WeightUnit = new UnitOfWeight { Code = "KG" };
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Weight is zero and WeightUnit has a value no exception should be thrown", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_KP_WidthLessThanZeroThrowException

		public void TestPkgPackageContainerDataObjectReader_KP_WidthLessThanZeroThrowException()
		{
			// ADD CONSTRAINT [Constraint_KP_Width] CHECK (KP_Width >= 0);
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "20GP" },
				TotalWidth = 0m
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Width = 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.TotalWidth = 5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertNoExceptionThrown("Width > 0: No exception required.", () => reader.ReadIntoBusinessObject());

			container.TotalWidth = -5m;
			reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			AssertExceptionThrown("Width < 0: An exception should be thrown", typeof(DataObjectReadFailureException), @"Package width must not be negative.
ID: ABC
Pack Type: CNT
Quantity: 1
Width: -5", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_ContainerWeightCalculation

		public void TestPkgPackageContainerDataObjectReader_ContainerWeightCalculation()
		{
			Data.CreatePackingData();

			var sourceContainer = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "40GP" },
				GrossWeight = 10000m,
				GoodsWeight = 1000m,
				TareWeight = 0m
			};

			var reader1 = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var readContainer1 = reader1.ReadIntoBusinessObject();

			AssertEquals("0 Tare weight should be re-calculated", 9000m, readContainer1.KP_TareWeight);
			AssertEquals("Goods weight should match difference between weight and tare weight, which matches the value in the data object", 1000m, readContainer1.Container.GoodsWeight);
			AssertEquals("Gross weight should match the value in the data object", 10000m, readContainer1.KP_Weight);

			sourceContainer.TareWeight = -5m;

			var reader2 = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var readContainer2 = reader2.ReadIntoBusinessObject();

			AssertEquals("Tare weight less than 0 should be re-calculated", 9000m, readContainer2.KP_TareWeight);
			AssertEquals("Goods weight should match difference between weight and tare weight, which matches the GoodsWeight value in the data object", 1000m, readContainer2.Container.GoodsWeight);
			AssertEquals("Gross weight should match the value in the data object", 10000m, readContainer2.KP_Weight);

			sourceContainer.TareWeight = 500m;

			var reader3 = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var readContainer3 = reader3.ReadIntoBusinessObject();

			AssertEquals("Tare weight greater than 0 should not be re-calculated", 500m, readContainer3.KP_TareWeight);
			AssertEquals("Goods weight should match difference between weight and tare weight, which does not match the GoodsWeight value in the data object", 9500m, readContainer3.Container.GoodsWeight);
			AssertEquals("Gross weight should match the value in the data object", 10000m, readContainer3.KP_Weight);

			sourceContainer.TareWeight = null;

			var reader4 = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var readContainer4 = reader4.ReadIntoBusinessObject();

			AssertEquals("Null Tare weight should be re-calculated", 9000m, readContainer4.KP_TareWeight);
			AssertEquals("Goods weight should match difference between weight and tare weight, which matches the GoodsWeight value in the data object", 1000m, readContainer4.Container.GoodsWeight);
			AssertEquals("Gross weight should match the value in the data object", 10000m, readContainer4.KP_Weight);

			sourceContainer.TareWeight = -5m;
			sourceContainer.GoodsWeight = 20_000m;

			var reader5 = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var readContainer5 = reader5.ReadIntoBusinessObject();

			AssertEquals("When re-calculating tare weight, it should be set to 0 if it would otherwise be set to a negative value", 0m, readContainer5.KP_TareWeight);
			AssertEquals("Goods weight should match difference between weight and tare weight, which does not match the GoodsWeight value in the data object", 10_000m, readContainer5.Container.GoodsWeight);
			AssertEquals("Gross weight should match the value in the data object, even if the GrossWeight value in the data object is less than the GoodsWeight value in the data object", 10_000m, readContainer5.KP_Weight);

			AssertNoExceptionThrown("No business object should break database constraints", () => { Factory.SaveForTesting(); });

			sourceContainer.TareWeight = null;
			sourceContainer.GrossWeight = null;
			sourceContainer.GoodsWeight = null;

			var reader6 = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var readContainer6 = reader6.ReadIntoBusinessObject();

			AssertEquals("Tare weight should not be modified from original defaults when tare weight and gross and goods weights have not been supplied", 3830m, readContainer6.KP_TareWeight);
			AssertEquals("Goods weight should match difference between weight and tare weight", 0m, readContainer6.Container.GoodsWeight);
			AssertEquals("Gross weight should not be modified from original defaults", 3830m, readContainer6.KP_Weight);
		}

		public void TestPkgPackageContainerDataObjectReader_DunnageWeightNegative_KP_DunnageWeightSetToZero()
		{
			Data.CreatePackingData();

			var container = new Container
			{
				ContainerType = new ContainerType { Code = "20GP" },
				DunnageWeight = -5m
			};

			var reader = new PkgPackageContainerDataObjectReader(container, Logger, Factory, Data.PackageJob.Packages);
			var readContainer = reader.ReadIntoBusinessObject();

			AssertEquals("Dunnage Weight should be set to 0 if it'd otherwise be negative", 0m, readContainer.KP_DunnageWeight);
			AssertNoExceptionThrown("Business object should not break database constraints", () => { Factory.SaveForTesting(); });
		}

		#endregion

		#region TestPkgPackageContainerDataObjectReader_UNDGImportDoesNotDuplicate

		public void TestPkgPackageContainerDataObjectReader_UNDGImportDoesNotDuplicate()
		{
			Data.CreatePackingData();
			Factory.SaveForTesting();

			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var container = pkgPackageJob.Packages.AddNew(Constants.PkgUnit.Container, "CON1");
			container.Container.K0_RC_ContainerType = Data.Container20GP.PK;

			container.UNDGs.Add(Data.UndgDataItemLOS);
			container.UNDGs.Add(Data.UndgDataItemEXP);

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var packingLineDO = pkgPackageJobDataObject.ContainerCollection[0];

			var reader1 = new PkgPackageContainerDataObjectReader(packingLineDO, Logger, Factory, pkgPackageJob.Packages, targetContainer: container);
			var package = reader1.ReadIntoBusinessObject();

			Factory.SaveForTesting();
			Factory.SaveAtEndOfImport(logger);

			AssertEquals("Package should contain 2 Dangerous Goods", 2, package.UNDGs.Count);
			AssertEquals("Container should contain 2 Dangerous Goods", 2, container.UNDGs.Count);

			var reader2 = new PkgPackageContainerDataObjectReader(packingLineDO, Logger, Factory, pkgPackageJob.Packages, targetContainer: container);
			package = reader2.ReadIntoBusinessObject();

			Factory.SaveForTesting();
			Factory.SaveAtEndOfImport(logger);

			AssertEquals("Package should contain 2 Dangerous Goods after importing again", 2, package.UNDGs.Count);
			AssertEquals("Container should contain 2 Dangerous Goods after importing again", 2, container.UNDGs.Count);
		}

		#endregion

		public void TestIsCheckedWeighedCubed_WhenIsCheckedWeighedCubed_IsTrue_ShouldKP_IsCheckedWeighedCubed_IsTrue()
		{
			Data.CreatePackingData();

			var sourceContainer = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "40GP" },
				GrossWeight = 10000m,
				GoodsWeight = 1000m,
				TareWeight = 0m,
				IsCheckedWeighedCubed = true,
			};

			var packageContainerDataObjectReader = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var package = packageContainerDataObjectReader.ReadIntoBusinessObject();

			AssertEquals(true, package.KP_IsCheckedWeighedCubed);
		}

		public void TestPillaged_WhenPillaged_IsTrue_ShouldKP_IsPillaged_IsTrue()
		{
			Data.CreatePackingData();

			var sourceContainer = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "40GP" },
				GrossWeight = 10000m,
				GoodsWeight = 1000m,
				TareWeight = 0m,
				Pillaged = true,
			};

			var packageContainerDataObjectReader = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var package = packageContainerDataObjectReader.ReadIntoBusinessObject();

			AssertEquals(true, package.KP_IsPillaged);
		}

		public void TestFumigated_WhenFumigated_IsTrue_ShouldKP_Fumigated_IsTrue()
		{
			Data.CreatePackingData();

			var sourceContainer = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "40GP" },
				GrossWeight = 10000m,
				GoodsWeight = 1000m,
				TareWeight = 0m,
				Fumigated = true,
			};

			var packageContainerDataObjectReader = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var package = packageContainerDataObjectReader.ReadIntoBusinessObject();

			AssertEquals(true, package.KP_IsFumigated);
		}

		public void TestHeatTreated_WhenHeatTreated_IsTrue_ShouldKP_HeatTreated_IsTrue()
		{
			Data.CreatePackingData();

			var sourceContainer = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "40GP" },
				GrossWeight = 10000m,
				GoodsWeight = 1000m,
				TareWeight = 0m,
				HeatTreated = true,
			};

			var packageContainerDataObjectReader = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var package = packageContainerDataObjectReader.ReadIntoBusinessObject();

			AssertEquals(true, package.KP_IsHeatTreated);
		}

		public void TestRequiresTemperature()
		{
			Data.CreatePackingData();

			var sourceContainer = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "40GP" },
				GrossWeight = 10000m,
				GoodsWeight = 1000m,
				TareWeight = 0m,
				RequiresTemperatureControl = true,
				RequiredTemperatureMinimum = -10m,
				RequiredTemperatureMaximum = 5m,
				RequiredTemperatureUnit = new CodeDescriptionPair1Char() { Code = "C", Description = "Centigrade" },
			};

			var packageContainerDataObjectReader = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var package = packageContainerDataObjectReader.ReadIntoBusinessObject();

			AssertEquals(true, package.KP_RequiresTemperatureControl);
			AssertEquals(-10m, package.KP_RequiredTemperatureMinimum);
			AssertEquals(5m, package.KP_RequiredTemperatureMaximum);
			AssertEquals(Constants.Temperature.Centigrade, package.KP_RequiredTemperatureUnit);
		}

		public void TestMarksAndNumbers()
		{
			Data.CreatePackingData();

			var sourceContainer = new Container
			{
				ContainerNumber = "ABC",
				ContainerType = new ContainerType { Code = "40GP" },
				GrossWeight = 10000m,
				GoodsWeight = 1000m,
				TareWeight = 0m,
				MarksAndNos = "123321",
			};

			var packageContainerDataObjectReader = new PkgPackageContainerDataObjectReader(sourceContainer, Logger, Factory, Data.PackageJob.Packages);
			var package = packageContainerDataObjectReader.ReadIntoBusinessObject();

			AssertEquals("123321", package.KP_MarksAndNumbers);
		}

		#region Logger

		TestErrorLogger Logger => logger ?? (logger = new TestErrorLogger());
		TestErrorLogger logger;

		#endregion
	}
}
