using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageContainer))]
	public class PkgPackageContainerTest : PackingBusinessObjectTestCase
	{
		#region TestGetNewValidation

		public void TestGetNewValidation()
		{
			Data.CreatePackingData();
			var container = Data.PackageJob.Packages.AddNew("CNT").Container;
			AssertEquals(typeof(PkgPackageContainerValidationForUnfinalisedPackageJob), container.Validation.GetType());

			Data.PackageJob.KJ_IsFinalized = true;
			AssertEquals(typeof(PkgPackageContainerValidation), container.Validation.GetType());
		}

		#endregion

		#region Related Entities

		#region TestPackage

		public void TestPackage()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertNull(container.Package);

			var package = Factory.New<PkgPackage>();
			container.K0_KP_Package = package.PK;
			AssertEquals(package, container.Package);
		}

		#endregion

		#endregion

		#region TestAllPackageProxyProperties

		public void TestAllPackageProxyProperties()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("CNT");
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Kilograms;

			var container = package.Container;
			AssertEquals("PackageWeight should be 10", 10m, container.PackageWeight);
			AssertEquals("PackageWeightUQ should be " + Constants.Weight.Kilograms, Constants.Weight.Kilograms, container.PackageWeightUQ);
		}

		#endregion

		#region Properties

		// persistent

		#region TestK0_RC_ContainerType

		public void TestK0_RC_ContainerType_SetsDefaultDetails()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var container = package.Container;
			package.KP_DimensionUQ = Constants.Length.Metres;
			package.KP_WeightUQ = Constants.Weight.Pounds;

			AssertEquals("Precondition", 0m, package.KP_Length);
			AssertEquals("Precondition", 0m, package.KP_Width);
			AssertEquals("Precondition", 0m, package.KP_Height);
			AssertEquals("Precondition", 0m, package.KP_TareWeight);

			container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals("Should have set the Package Length to the RefContainer Length (converted to the Package UQ).", 6.096m, package.KP_Length);
			AssertEquals("Should have set the Package Width to the RefContainer Width (converted to the Package UQ).", 2.438m, package.KP_Width);
			AssertEquals("Should have set the Package Height to the RefContainer Height (converted to the Package UQ).", 2.591m, package.KP_Height);
			AssertEquals("Should have set the package TareWeight to the RefContainer's TareWeight (converted to the Package UQ).", 5026.540m, package.KP_TareWeight);

			// Change the package UQs to match the RefContainer UQs

			package.KP_DimensionUQ = Constants.Length.Feet;
			package.KP_WeightUQ = Constants.Weight.Kilograms;

			container.K0_RC_ContainerType = ZGuid.Empty;
			AssertEquals("Removing the RefContainer should clear the Package Dimensions.", 0m, package.KP_Length);
			AssertEquals("Removing the RefContainer should clear the Package Dimensions", 0m, package.KP_Width);
			AssertEquals("Removing the RefContainer should clear the Package Dimensions", 0m, package.KP_Height);
			AssertEquals("Removing the RefContainer should clear the TareWeight", 0m, package.KP_TareWeight);

			container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals("Should have set the Package Length to the RefContainer Length.", 20m, package.KP_Length);
			AssertEquals("Should have set the Package Width to the RefContainer Width.", 8m, package.KP_Width);
			AssertEquals("Should have set the Package Height to the RefContainer Height.", 8.5m, package.KP_Height);
			AssertEquals("Should have set the Container TareWeight to the RefContainer's TareWeight.", 2280m, package.KP_TareWeight);
		}

		public void TestK0_RC_ContainerType_SetsDefaultDetailsWhenPackageQtySetFirst()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var container = package.Container;
			package.KP_DimensionUQ = Constants.Length.Feet;
			package.KP_WeightUQ = Constants.Weight.Kilograms;

			AssertEquals("Precondition", 0m, package.KP_Length);
			AssertEquals("Precondition", 0m, package.KP_Width);
			AssertEquals("Precondition", 0m, package.KP_Height);
			AssertEquals("Precondition", 0m, package.KP_TareWeight);

			package.KP_PackageQty = 2;
			container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals("Should have set the Package Length to the RefContainer Length (converted to the Package UQ).", 20m, package.KP_Length);
			AssertEquals("Should have set the Package Width to the RefContainer Width (converted to the Package UQ).", 8m, package.KP_Width);
			AssertEquals("Should have set the Package Height to the RefContainer Height (converted to the Package UQ).", 8.5m, package.KP_Height);
			AssertEquals("Should have set the package TareWeight to the RefContainer's TareWeight (converted to the Package UQ).", 4560m, package.KP_TareWeight);
		}

		#endregion

		#region TestSealParty

		public void TestK0_Seal1PartyType_ListAttribute()
		{
			var container = Factory.New<PkgPackageContainer>();
			var attribute = container.K0_Seal1PartyTypeInfo.GetAttribute<ListAttribute>();

			AssertEquals(attribute.ListDataSourceMember, "Lookups.SealParty_List");
		}

		public void TestK0_Seal2PartyType_ListAttribute()
		{
			var container = Factory.New<PkgPackageContainer>();
			var attribute = container.K0_Seal2PartyTypeInfo.GetAttribute<ListAttribute>();

			AssertEquals(attribute.ListDataSourceMember, "Lookups.SealParty_List");
		}

		public void TestK0_Seal3PartyType_ListAttribute()
		{
			var container = Factory.New<PkgPackageContainer>();
			var attribute = container.K0_Seal3PartyTypeInfo.GetAttribute<ListAttribute>();

			AssertEquals(attribute.ListDataSourceMember, "Lookups.SealParty_List");
		}

		#endregion

		#region TestTemperatureDetailsReadOnly

		public void TestTemperatureDetailsReadOnly()
		{
			var container = Factory.New<PkgPackageContainer>();

			var temperatureProperties = new ZPropertyInfo[]
			{
				container.K0_AirVentFlowRateInfo,
				container.K0_AirVentFlowRateUnitInfo,
				container.K0_HumidityPercentInfo,
				container.K0_RefrigGeneratorIDInfo,
				container.K0_SetPointTempInfo,
				container.K0_SetPointTempUnitInfo,
				container.K0_TempRecorderSerialNumberInfo
			};

			container.K0_IsControlledAtmosphere = false;
			Array.ForEach(temperatureProperties, info => AssertEquals(true, info.ReadOnly));

			container.K0_IsControlledAtmosphere = true;
			Array.ForEach(temperatureProperties, info => AssertEquals(false, info.ReadOnly));
		}

		#endregion

		// calculated

		#region TestRefContainerLength

		public void TestRefContainerLength()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			package.KP_DimensionUQ = "";
			AssertEquals(0m, package.Container.RefContainerLength);

			package.Container.K0_RC_ContainerType = Data.Container20GP.PK; // length is 20ft
			AssertEquals("No Package UQ, should default to the RefContainer UQ (ft).", 20m, package.Container.RefContainerLength);

			package.KP_DimensionUQ = Constants.Length.Metres;
			AssertEquals("Package UQ is Metres, RefContainer dimension should be in the same UQ.", 6.096m, package.Container.RefContainerLength);
		}

		#endregion

		#region TestRefContainerWidth

		public void TestRefContainerWidth()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			package.KP_DimensionUQ = "";
			AssertEquals(0m, package.Container.RefContainerWidth);

			package.Container.K0_RC_ContainerType = Data.Container20GP.PK; // width is 8ft
			AssertEquals("No Package UQ, should default to the RefContainer UQ (ft).", 8m, package.Container.RefContainerWidth);

			package.KP_DimensionUQ = Constants.Length.Metres;
			AssertEquals("Package UQ is Metres, RefContainer dimension should be in the same UQ.", 2.4384m, package.Container.RefContainerWidth);
		}

		#endregion

		#region TestRefContainerHeight

		public void TestRefContainerHeight()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			package.KP_DimensionUQ = "";
			AssertEquals(0m, package.Container.RefContainerHeight);

			package.Container.K0_RC_ContainerType = Data.Container20GP.PK; // height is 8.5ft
			AssertEquals("No Package UQ, should default to the RefContainer UQ (ft).", 8.5m, package.Container.RefContainerHeight);

			package.KP_DimensionUQ = Constants.Length.Metres;
			AssertEquals("Package UQ is Metres, RefContainer dimension should be in the same UQ.", 2.5908m, package.Container.RefContainerHeight);
		}

		#endregion

		#region TestOverhangLength

		public void TestOverhangLength()
		{
			AssertOverhang(PkgPackageSchema.KP_Length, RefContainerSchema.RC_Length, PkgPackageContainer.Schema.OverhangLength);
		}

		#endregion

		#region TestOverhangWidth

		public void TestOverhangWidth()
		{
			AssertOverhang(PkgPackageSchema.KP_Width, RefContainerSchema.RC_Width, PkgPackageContainer.Schema.OverhangWidth);
		}

		#endregion

		#region TestOverhangHeight

		public void TestOverhangHeight()
		{
			AssertOverhang(PkgPackageSchema.KP_Height, RefContainerSchema.RC_Height, PkgPackageContainer.Schema.OverhangHeight);
		}

		#endregion

		#region TestGoodsWeight

		public void TestGoodsWeight()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "CNT";

			var container = package.Container;
			CombineAssertions(() =>
			{
				AssertEquals(0m, container.GoodsWeight);
				package.GoodsWeight = 20;
				AssertEquals("GoodsWeight in package should equal to GoodsWeight in package", 20m, container.GoodsWeight);
				container.GoodsWeight = 15;
				AssertEquals("Changing the GoodsWeight in container should change the GoodsWeight in package.", 15m, container.GoodsWeight);
			});
		}

		#endregion

		#region TestTemperatureSummary

		public void TestTemperatureSummary()
		{
			var container = Factory.New<PkgPackageContainer>();
			container.K0_SetPointTemp = 40;
			container.K0_SetPointTempUnit = "F";
			AssertEquals("40 °F", container.TemperatureSummary);
		}

		#endregion

		#endregion

		#region Flags

		#region TestHasAllDefaultValues

		public void TestHasAllDefaultValues()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew("CNT");
			var container = package.Container;
			AssertEquals(true, container.HasAllDefaultValues);

			container.K0_RC_ContainerType = Data.Container20GP.PK;
			AssertEquals(false, container.HasAllDefaultValues);

			container.K0_RC_ContainerType = ZGuid.Empty;
			AssertEquals(true, container.HasAllDefaultValues);

			container.K0_IsSealOk = true;
			AssertEquals(false, container.HasAllDefaultValues);
		}

		#endregion

		#region TestIsNotTemperatureControlled

		public void TestIsNotTemperatureControlled()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertEquals("Default should be no temperature control.", true, container.IsNotTemperatureControlled);

			// turn on temperature control via the K0 property
			container.K0_IsControlledAtmosphere = true;
			container.K0_AirVentFlowRate = 7m;
			container.K0_AirVentFlowRateUnit = "2L";
			container.K0_HumidityPercent = 21;
			container.K0_RefrigGeneratorID = "R #123";
			container.K0_SetPointTemp = 3m;
			container.K0_SetPointTempUnit = Constants.Temperature.Centigrade;
			container.K0_TempRecorderSerialNumber = "S #123";
			AssertEquals("Precondition", 7m, container.K0_AirVentFlowRate);
			AssertEquals("Precondition", "2L", container.K0_AirVentFlowRateUnit);
			AssertEquals("Precondition", (byte)21, container.K0_HumidityPercent);
			AssertEquals("Precondition", "R #123", container.K0_RefrigGeneratorID);
			AssertEquals("Precondition", 3m, container.K0_SetPointTemp);
			AssertEquals("Precondition", Constants.Temperature.Centigrade, container.K0_SetPointTempUnit);
			AssertEquals("Precondition", "S #123", container.K0_TempRecorderSerialNumber);
			AssertEquals(false, container.IsNotTemperatureControlled);

			// turn off temperature control via the flag
			container.IsNotTemperatureControlled = true;
			AssertEquals(false, container.K0_IsControlledAtmosphere);
			AssertEquals(0m, container.K0_AirVentFlowRate);
			AssertEquals("", container.K0_AirVentFlowRateUnit);
			AssertEquals((byte)0, container.K0_HumidityPercent);
			AssertEquals("", container.K0_RefrigGeneratorID);
			AssertEquals(0m, container.K0_SetPointTemp);
			AssertEquals("", container.K0_SetPointTempUnit);
			AssertEquals("", container.K0_TempRecorderSerialNumber);
		}

		#endregion

		#region TestIsChiller

		// Copied from ITemperatureSettingsExtensionsTest due to the different project.
		// Please undo copy when this test class is also in the test project.
		public void TestIsChiller()
		{
			ITemperatureSettings temperatureSettings = Factory.New<PkgPackageContainer>();
			AssertEquals(false, temperatureSettings.IsChiller());

			// set temperature control to true

			temperatureSettings.IsTemperatureControlled = true;
			AssertEquals(true, temperatureSettings.IsChiller());

			// set a frozen temperature

			temperatureSettings.TemperatureMin = -1;
			AssertEquals("Temperature was below 0C, should not be a Chiller.", false, temperatureSettings.IsChiller());

			// test with Farhenheit

			temperatureSettings.TemperatureUnit = Constants.Temperature.Fahrenheit;
			temperatureSettings.TemperatureMin = 31;
			AssertEquals(false, temperatureSettings.IsChiller());

			temperatureSettings.TemperatureMin = 32; // == 0 degrees celcius
			AssertEquals(true, temperatureSettings.IsChiller());

			// set IsChiller directly

			temperatureSettings.SetIsChiller(true);
			AssertEquals(true, temperatureSettings.IsChiller());
			AssertEquals("Setting IsChiller should set the Temperature to 41F (5C).", 41m, temperatureSettings.TemperatureMin);
			AssertEquals("Setting IsChiller should set the Temperature to 41F (5C).", 41m, temperatureSettings.TemperatureMax);
			AssertEquals("Setting IsChiller should not change the Temperature Unit.", Constants.Temperature.Fahrenheit, temperatureSettings.TemperatureUnit);

			temperatureSettings.TemperatureUnit = "";
			temperatureSettings.SetIsChiller(true);
			AssertEquals("Setting IsChiller should default the Temperature Unit to C.", Constants.Temperature.Centigrade, temperatureSettings.TemperatureUnit);

			temperatureSettings.SetIsChiller(false);
			AssertEquals(false, temperatureSettings.IsChiller());
		}

		#endregion

		#region TestIsFreezer

		// Copied from ITemperatureSettingsExtensionsTest due to the different project.
		// Please undo copy when this test class is also in the test project.
		public void TestIsFreezer()
		{
			ITemperatureSettings temperatureSettings = Factory.New<PkgPackageContainer>();
			AssertEquals(false, temperatureSettings.IsFreezer());

			// set temperature control to true

			temperatureSettings.IsTemperatureControlled = true;
			AssertEquals(false, temperatureSettings.IsFreezer());

			// set a frozen temperature

			temperatureSettings.TemperatureMin = -1;
			AssertEquals("Temperature was below 0C, should be Frozen.", true, temperatureSettings.IsFreezer());

			// set a non-frozen temperature

			temperatureSettings.TemperatureMin = 0;
			AssertEquals("Temperature was 0C or higher, should not be Frozen.", false, temperatureSettings.IsFreezer());

			// test with Farhenheit

			temperatureSettings.TemperatureUnit = Constants.Temperature.Fahrenheit;
			temperatureSettings.TemperatureMin = 32;
			AssertEquals(false, temperatureSettings.IsFreezer());

			temperatureSettings.TemperatureMin = 31; // below 0 degrees celcius
			AssertEquals(true, temperatureSettings.IsFreezer());

			// set IsFreezer directly

			temperatureSettings.SetIsFreezer(true);
			AssertEquals(true, temperatureSettings.IsFreezer());
			AssertEquals("Setting IsFreezer should set the Temperature to 23F (-5C).", 23m, temperatureSettings.TemperatureMin);
			AssertEquals("Setting IsFreezer should set the Temperature to 23F (-5C).", 23m, temperatureSettings.TemperatureMax);
			AssertEquals("Setting IsFreezer should not change the Temperature Unit.", Constants.Temperature.Fahrenheit, temperatureSettings.TemperatureUnit);

			temperatureSettings.TemperatureUnit = "";
			temperatureSettings.SetIsFreezer(true);
			AssertEquals("Setting IsFreezer should default the Temperature Unit to C.", Constants.Temperature.Centigrade, temperatureSettings.TemperatureUnit);

			temperatureSettings.SetIsFreezer(false);
			AssertEquals(false, temperatureSettings.IsFreezer());
		}

		#endregion

		#endregion

		#region TestITemperatureSettings

		public void TestITemperatureSettings()
		{
			var container = Factory.New<PkgPackageContainer>();
			var containerTemperatureSettings = (ITemperatureSettings)container;

			containerTemperatureSettings.IsTemperatureControlled = true;
			AssertEquals(true, container.K0_IsControlledAtmosphere);
			container.K0_IsControlledAtmosphere = false;
			AssertEquals(false, containerTemperatureSettings.IsTemperatureControlled);

			containerTemperatureSettings.TemperatureMin = 7m;
			AssertEquals(7m, container.K0_SetPointTemp);
			container.K0_SetPointTemp = 14m;
			AssertEquals(14m, containerTemperatureSettings.TemperatureMin);

			containerTemperatureSettings.TemperatureMax = 6m;
			AssertEquals(6m, container.K0_SetPointTemp);
			container.K0_SetPointTemp = 5m;
			AssertEquals(5m, containerTemperatureSettings.TemperatureMax);

			containerTemperatureSettings.TemperatureUnit = Constants.Temperature.Centigrade;
			AssertEquals(Constants.Temperature.Centigrade, container.K0_SetPointTempUnit);
			container.K0_SetPointTempUnit = Constants.Temperature.Fahrenheit;
			AssertEquals(Constants.Temperature.Fahrenheit, containerTemperatureSettings.TemperatureUnit);
		}

		#endregion

		#region TestIPackingHasChanges

		public void TestIPackingHasChanges_SetCriticalChangesVersionID_AddContainerToPackageWithoutChangingPackType()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			var package = job.Packages.AddNew("PLT");
			Factory.Save();
			job.KJ_CriticalChangesVersionID = ZGuid.Empty;

			var container = Factory.New<PkgPackageContainer>();
			container.K0_KP_Package = package.PK;
			container.K0_RC_ContainerType = Data.Container20GP.PK;
			package.RegisterEditableChildObject(container);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}

		public void TestIPackingHasChanges_SetCriticalChangesVersionID_DeleteContainerInPackageWithoutChangingPackType()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			var package = job.Packages.AddNew("PLT");
			var container = Factory.New<PkgPackageContainer>();
			container.K0_KP_Package = package.PK;
			container.K0_RC_ContainerType = Data.Container20GP.PK;
			package.RegisterEditableChildObject(container);
			Factory.Save();
			job.KJ_CriticalChangesVersionID = ZGuid.Empty;

			package.UnRegisterEditableChildObject(container);
			container.Delete();
			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}

		public void TestIPackingHasChanges_SetCriticalChangesVersionID_OnContainerModeModified()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			var package = job.Packages.AddNew("PLT");
			var container = Factory.New<PkgPackageContainer>();
			container.K0_KP_Package = package.PK;
			container.K0_RC_ContainerType = Data.Container20GP.PK;
			package.RegisterEditableChildObject(container);
			Factory.Save();
			job.KJ_CriticalChangesVersionID = ZGuid.Empty;

			container.K0_ContainerMode = "FCL";
			AssertNoExceptionThrown(() => Factory.Save());
			AssertNotEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "CNT";
			return package.Container;
		}

		void AssertOverhang(SchemaDecimalColumn packageDimensionColumn, SchemaDecimalColumn refContainerDimensionColumn, string overhangDimensionColumnName)
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("CNT");
			var container = package.Container;

			AssertNull("Precondition - RefContainer was not null.", container.ContainerType);
			AssertEquals(0m, container[overhangDimensionColumnName]);

			package[packageDimensionColumn] = 24m;
			AssertEquals("RefContainer is null, there should be no overhang.", 0m, container[overhangDimensionColumnName]);

			Data.Container20GP[refContainerDimensionColumn] = 65.617m; // 65.6167979feet == 20 metres
			container.K0_RC_ContainerType = Data.Container20GP.PK; // this will default the package DIMS
			package[packageDimensionColumn] = 24m;
			AssertEquals(3.999938m, container[overhangDimensionColumnName]);
		}

		#endregion
	}
}
