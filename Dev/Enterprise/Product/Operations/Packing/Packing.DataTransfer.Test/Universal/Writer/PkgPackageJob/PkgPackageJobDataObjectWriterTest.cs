using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.DataTransfer.Testing
{
	public class PkgPackageJobDataObjectWriterTest : PackingTestCaseWithFactory
	{
		#region TestGetDataObjectRequiresParentJob

		public void TestGetDataObjectRequiresParentJob()
		{
			var pkgPackageJob = Factory.New<PkgPackageJob>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob));
		}

		#endregion

		#region TestGetDataObject_PkgPackageJob

		public void TestGetDataObject_PkgPackageJob()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);

			AssertNotNull("Precondition", pkgPackageJobDataObject);
			AssertNotNull("DataContext should be created", pkgPackageJobDataObject.DataContext);
		}

		#endregion

		#region TestGetDataObject_PkgPackages

		#region TestGetDataObject_PkgPackages_Containers

		public void TestGetDataObject_PkgPackages_Containers()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = pkgPackageJob.Packages.AddNew("CNT");
			container.KP_PackageQty = 3;
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
			container.Container.K0_RC_ContainerType = containerType20GP.PK;
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
			container.KP_RH_NKCommodityCode = "GEN";
			container.KP_IsCheckedWeighedCubed = true;
			container.KP_IsPillaged = true;
			container.KP_IsHeatTreated = true;
			container.KP_IsFumigated = true;
			container.KP_RequiresTemperatureControl = true;
			container.KP_RequiredTemperatureMinimum = -10m;
			container.KP_RequiredTemperatureMaximum = 5m;
			container.KP_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			container.KP_MarksAndNumbers = "1234321";
			container.Container.K0_Seal1PartyType = Constants.ContainerSealParties.Codes.CarrierShippingLine;
			container.Container.K0_Seal2PartyType = Constants.ContainerSealParties.Codes.ConsignorShipper;
			container.Container.K0_Seal3PartyType = Constants.ContainerSealParties.Codes.Customs;

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			AssertNull("No PackingLine should be Created.", pkgPackageJobDataObject.PackingLineCollection);
			AssertEquals("Container Packages should be stored in ContainerCollection", 1, pkgPackageJobDataObject.ContainerCollection.Count);
			AssertEquals(CollectionContent.Complete, pkgPackageJobDataObject.ContainerCollection.Content);

			var containerDataObject = pkgPackageJobDataObject.ContainerCollection[0];
			AssertEquals("container.KP_DimensionUQ", "M", containerDataObject.LengthUnit.Code);
			AssertEquals("container.KP_Height", 1m, containerDataObject.TotalHeight);
			AssertEquals("container.KP_Length", 2m, containerDataObject.TotalLength);
			AssertEquals("container.KP_PackageID", "CONT-1", containerDataObject.ContainerNumber);
			AssertEquals("container.KP_PackageQty", 3, containerDataObject.ContainerCount);
			AssertEquals("container.KP_Volume", 12m, containerDataObject.VolumeCapacity);
			AssertEquals("container.KP_VolumeUQ", "M3", containerDataObject.VolumeUnit.Code);
			AssertEquals("container.KP_Weight", 5m, containerDataObject.GrossWeight);
			AssertEquals("container.KP_WeightUQ", "T", containerDataObject.WeightUnit.Code);
			AssertEquals("container.KP_Width", 6m, containerDataObject.TotalWidth);
			AssertEquals("container.KP_TransportRef", "TRANSPORT REF", containerDataObject.TransportReference);
			AssertEquals("container.KP_GoodsDescription", "GOODS DESC", containerDataObject.GoodsDescription);
			AssertEquals("container.KP_HSCode", "HARMON CODE", containerDataObject.HarmonisedCode);
			AssertEquals("container.Commodity.Code", "GEN", containerDataObject.Commodity.Code);
			AssertEquals("container.Commodity.Description", "General", containerDataObject.Commodity.Description);
			AssertEquals("container.Container.K0_AirVentFlowRate", 7m, containerDataObject.AirVentFlow);
			AssertEquals("container.Container.K0_AirVentFlowRateUnit", "M2", containerDataObject.AirVentFlowRateUnit.Code);
			AssertEquals("container.Container.K0_ContainerMode", "AIR", containerDataObject.FCL_LCL_AIR.Code);
			AssertEquals("container.KP_DunnageWeight", 8m, containerDataObject.DunnageWeight);
			AssertEquals("container.Container.K0_HumidityPercent", (ZByte)9, containerDataObject.HumidityPercent);
			AssertEquals("container.Container.K0_IsControlledAtmosphere", true, containerDataObject.IsControlledAtmosphere);
			AssertEquals("container.Container.K0_IsDamaged", true, containerDataObject.IsDamaged);
			AssertEquals("container.Container.K0_IsEmpty", true, containerDataObject.IsEmptyContainer);
			AssertEquals("container.Container.K0_IsSealOk", true, containerDataObject.IsSealOk);
			AssertEquals("container.Container.K0_IsShipperOwned", true, containerDataObject.IsShipperOwned);
			AssertEquals("container.Container.K0_Quality", "RIC", containerDataObject.ContainerQuality.Code);
			AssertEquals("container.Container.K0_RC_ContainerType", containerType20GP.RC_Code, containerDataObject.ContainerType.Code);
			AssertEquals("container.Container.K0_RefrigGeneratorID", "REFRIG123", containerDataObject.RefrigGeneratorID);
			AssertEquals("container.Container.K0_Seal1", "SEAL-1", containerDataObject.Seal);
			AssertEquals("container.Container.K0_Seal2", "SEAL-2", containerDataObject.SecondSeal);
			AssertEquals("container.Container.K0_Seal3", "SEAL-3", containerDataObject.ThirdSeal);
			AssertEquals("container.Container.K0_SetPointTemp", 10m, containerDataObject.SetPointTemp);
			AssertEquals("container.Container.K0_SetPointTempUnit", Constants.Temperature.Centigrade, containerDataObject.SetPointTempUnit);
			AssertEquals("container.Container.K0_Status", "ARV", containerDataObject.ContainerStatus.Code);
			AssertEquals("container.KP_TareWeight", 11m, containerDataObject.TareWeight);
			AssertEquals("container.Container.K0_TempRecorderSerialNumber", "TEMPSER123", containerDataObject.TempRecorderSerialNo);
			AssertEquals("container.Container.K0_Seal1PartyType", Constants.ContainerSealParties.Codes.CarrierShippingLine, containerDataObject.SealPartyType.Code);
			AssertEquals("container.Container.K0_Seal2PartyType", Constants.ContainerSealParties.Codes.ConsignorShipper, containerDataObject.SecondSealPartyType.Code);
			AssertEquals("container.Container.K0_Seal3PartyType", Constants.ContainerSealParties.Codes.Customs, containerDataObject.ThirdSealPartyType.Code);

			AssertEquals("Container should be assigned a Link, so it could be identified by other BizOs", 0, containerDataObject.Link);

			AssertEquals("package.KP_IsCheckedWeighedCubed", true, containerDataObject.IsCheckedWeighedCubed);
			AssertEquals("package.KP_Pillaged", true, containerDataObject.Pillaged);
			AssertEquals("package.KP_IsFumigated", true, containerDataObject.Fumigated);
			AssertEquals("package.KP_IsHeatTreated", true, containerDataObject.HeatTreated);
			AssertEquals("package.KP_RequiresTemperatureControl", true, containerDataObject.RequiresTemperatureControl);
			AssertEquals("package.KP_RequiredTemperatureMinimum", -10m, containerDataObject.RequiredTemperatureMinimum);
			AssertEquals("package.KP_RequiredTemperatureMaximum", 5m, containerDataObject.RequiredTemperatureMaximum);
			AssertEquals("package.KP_RequiredTemperatureUnit", "C", containerDataObject.RequiredTemperatureUnit.Code);
			AssertEquals("package.KP_RequiredTemperatureUnit", "Centigrade", containerDataObject.RequiredTemperatureUnit.Description);
			AssertEquals("package.KP_MarksAndNumbersVersion", "1234321", containerDataObject.MarksAndNos);
		}

		#endregion

		#region TestGetDataObject_PkgPackages_ContainerAsChild

		[ExpectExceptionMessage(typeof(ArgumentException), "Container package should never be a child package.")]
		public void TestGetDataObject_PkgPackages_ContainerAsChild()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var box = pkgPackageJob.Packages.AddNew("BOX");
			var container = box.Packages.AddNew("CNT");

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
		}

		#endregion

		#region TestGetDataObject_PkgPackages_ContainerWithChildPackages

		public void TestGetDataObject_PkgPackages_ContainerWithChildPackages()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var container = pkgPackageJob.Packages.AddNew("CNT");
			var box = container.Packages.AddNew("BOX");
			var bag = box.Packages.AddNew("BAG");

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			AssertEquals("One Container should be created", 1, pkgPackageJobDataObject.ContainerCollection.Count);
			AssertEquals("One Top Level Packing Line should be created", 1, pkgPackageJobDataObject.PackingLineCollection.Count);
			AssertEquals("One Child Packing Line should be created", 1, pkgPackageJobDataObject.PackingLineCollection[0].PackingLineCollection.Count);

			var containerDataObject = pkgPackageJobDataObject.ContainerCollection[0];
			var boxDataObject = pkgPackageJobDataObject.PackingLineCollection[0];
			var bagDataObject = boxDataObject.PackingLineCollection[0];

			AssertEquals("Container should be assigned a Link, so it could be identified by other BizOs", 0, containerDataObject.Link);

			AssertEquals("Package is a container child, so should have link to related container.", 0, boxDataObject.ContainerLink);
			AssertEquals("Package should be assigned a Link, so it could be identified by other BizOs", 0, boxDataObject.Link);

			AssertNull("Package is a container's grand child, so no link to related container should be created.", bagDataObject.ContainerLink);
			AssertEquals("Package should be assigned a Link, so it could be identified by other BizOs", 1, bagDataObject.Link);
		}

		#endregion

		#region TestGetDataObject_PkgPackages_Packages

		public void TestGetDataObject_PkgPackages_Packages()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var package = pkgPackageJob.Packages.AddNew("BOX");

			var nmfc = Factory.New<RefNMFC>();
			nmfc.FN_ItemNo = "654321";
			nmfc.FN_Class = "92.5";
			nmfc.FN_Description = "Test37";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "TST";
			commodity.RH_Description = "Test";
			commodity.RH_FN_NKNMFC = nmfc.FN_Code;

			Factory.Save();

			package.KP_DimensionUQ = "M";
			package.KP_Height = 1m;
			package.KP_Length = 2m;
			package.KP_PackageID = "PACKAGE123";
			package.KP_PackageQty = 3;
			package.KP_VolumeUQ = "M3";
			package.KP_TareWeight = 7m;
			package.KP_DunnageWeight = 2.3m;
			package.KP_Weight = 5m;
			package.KP_WeightUQ = "T";
			package.KP_Width = 6m;
			package.KP_MarksAndNumbers = "MARK123";
			package.KP_TransportRef = "TRANSPORT REF";
			package.KP_GoodsDescription = "GOODS DESC";
			package.KP_HSCode = "HARMON CODE";
			package.KP_ExternalReference = "PackLineID";
			package.KP_PreviousPackLineID = "PreviousPackLineID";
			package.KP_Volume = 12m;
			package.KP_RH_NKCommodityCode = "TST";
			package.KP_RequiresTemperatureControl = true;
			package.KP_RequiredTemperatureMinimum = -10;
			package.KP_RequiredTemperatureMaximum = -5;
			package.KP_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			package.KP_IsFumigated = true;
			package.KP_IsNonStackable = true;
			package.KP_IsTopLoadOnly = true;
			package.KP_IsHeatTreated = true;
			package.KP_IsISPMPallet = true;
			package.KP_IsPillaged = true;
			package.KP_IsDamaged = true;
			package.KP_IsCheckedWeighedCubed = true;

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			CombineAssertions(() =>
			{
				AssertNull("No ContainerLines should be Created.", pkgPackageJobDataObject.ContainerCollection);
				AssertEquals("Non Container Packages should be stored in PackingLineCollection", 1, pkgPackageJobDataObject.PackingLineCollection.Count);
			});

			var packageDataObject = pkgPackageJobDataObject.PackingLineCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("package.KP_DimensionUQ", "M", packageDataObject.LengthUnit.Code);
				AssertEquals("package.KP_Height", 1m, packageDataObject.Height);
				AssertEquals("package.KP_Length", 2m, packageDataObject.Length);
				AssertEquals("package.KP_PackageID", "PACKAGE123", packageDataObject.ReferenceNumber);
				AssertEquals("package.KP_PackageQty", 3L, packageDataObject.PackQty);
				AssertEquals("package.KP_F3_NKPackType", "BOX", packageDataObject.PackType.Code);
				AssertEquals("package.KP_Volume", 12m, packageDataObject.Volume);
				AssertEquals("package.KP_VolumeUQ", "M3", packageDataObject.VolumeUnit.Code);
				AssertEquals("package.KP_Weight", 5m, packageDataObject.Weight);
				AssertEquals("package.KP_WeightUQ", "T", packageDataObject.WeightUnit.Code);
				AssertEquals("package.KP_Width", 6m, packageDataObject.Width);
				AssertEquals("package.KP_TransportRef", "TRANSPORT REF", packageDataObject.TransportReference);
				AssertEquals("package.MarksAndNos", "MARK123", packageDataObject.MarksAndNos);
				AssertEquals("package.GoodsDescription", "GOODS DESC", packageDataObject.GoodsDescription);
				AssertEquals("package.HarmonisedCode", "HARMON CODE", packageDataObject.HarmonisedCode);
				AssertEquals("package.PackingLineID", "PackLineID", packageDataObject.PackingLineID);
				AssertEquals("package.PreviousPackingLineID", "PreviousPackLineID", packageDataObject.PreviousPackingLineID);
				AssertEquals("package.Commodity.Code", "TST", packageDataObject.Commodity.Code);
				AssertEquals("package.Commodity.Description", "Test", packageDataObject.Commodity.Description);
				AssertEquals("package.NMFC.Class", "92.5", packageDataObject.NMFC.Class);
				AssertEquals("package.NMFC.Code", "654321|92.5", packageDataObject.NMFC.Code);
				AssertEquals("package.NMFC.Description", "Test37", packageDataObject.NMFC.Description);
				AssertEquals("package.NMFC.ItemNo", "654321", packageDataObject.NMFC.ItemNo);
				AssertEquals("package.KP_TareWeight", 7m, packageDataObject.TareWeight);
				AssertEquals("package.KP_DunnageWeight", 2.3m, packageDataObject.DunnageWeight);
				AssertEquals("package.KP_IsFumigated", true, packageDataObject.Fumigated);
				AssertEquals("package.KP_IsNonStackable", true, packageDataObject.NonStackable);
				AssertEquals("package.KP_IsTopLoadOnly", true, packageDataObject.TopLoadOnly);
				AssertEquals("package.KP_IsHeatTreated", true, packageDataObject.HeatTreated);
				AssertEquals("package.KP_IsISPMPallet", true, packageDataObject.ISPMPallet);
				AssertEquals("package.KP_IsPillaged", true, packageDataObject.Pillaged);
				AssertEquals("package.KP_IsDamaged", true, packageDataObject.IsDamaged);
				AssertEquals("package.KP_IsCheckedWeighedCubed", true, packageDataObject.IsCheckedWeighedCubed);
				// temperatures
				AssertEquals("package.RequiresTemperatureControl", true, packageDataObject.RequiresTemperatureControl);
				AssertEquals("package.RequiredTemperatureMinimum", -10m, packageDataObject.RequiredTemperatureMinimum);
				AssertEquals("package.RequiredTemperatureMaximum", -5m, packageDataObject.RequiredTemperatureMaximum);
				AssertEquals("package.RequiredTemperatureUnit", "C", packageDataObject.RequiredTemperatureUnit.Code);
				AssertEquals("package.RequiredTemperatureUnit", "Centigrade", packageDataObject.RequiredTemperatureUnit.Description);

				AssertEquals("Package should be assigned a Link, so it could be identified by other BizOs", 0, packageDataObject.Link);
				AssertNull("Package has no container parent, so Link to it should not exist.", packageDataObject.ContainerLink);
			});
		}

		#endregion

		#region TestGetDataObject_PkgPackages_PackageWithChild

		public void TestGetDataObject_PkgPackages_PackageWithChildPackages()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var box = pkgPackageJob.Packages.AddNew("BOX");
			var bag = box.Packages.AddNew("BAG");

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			AssertNull("No Containers should be created", pkgPackageJobDataObject.ContainerCollection);
			AssertEquals("One Top Level Packing Line should be created", 1, pkgPackageJobDataObject.PackingLineCollection.Count);
			AssertEquals("One Child Packing Line should be created", 1, pkgPackageJobDataObject.PackingLineCollection[0].PackingLineCollection.Count);

			var boxDataObject = pkgPackageJobDataObject.PackingLineCollection[0];
			var bagDataObject = boxDataObject.PackingLineCollection[0];

			AssertNull("Package is not a container child, so no Link to related container should be created.", boxDataObject.ContainerLink);
			AssertEquals("Package should be assigned a Link, so it could be identified by other BizOs", 0, boxDataObject.Link);

			AssertNull("Package is not a container child, so no Link to related container should be created.", bagDataObject.ContainerLink);
			AssertEquals("Package should be assigned a Link, so it could be identified by other BizOs", 1, bagDataObject.Link);
		}

		#endregion

		#region TestGetDataObject_PkgPackages_PackageWithPackableItems

		public void TestGetDataObject_PkgPackages_PackageWithPackableItems()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);

			var dummyParent = Factory.New<DummyWithPacking>();
			var pkgPackageJob = Factory.New<PkgPackageJob>();
			pkgPackageJob.KJ_JobID = "PJ00000001";
			pkgPackageJob.KJ_ParentTableCode = dummyParent.TablePrefix;
			pkgPackageJob.KJ_ParentID = dummyParent.PK;

			var box = pkgPackageJob.Packages.AddNew("BOX");
			var container1 = pkgPackageJob.Packages.AddNew("CNT");
			container1.Container.K0_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var container2 = pkgPackageJob.Packages.AddNew("CNT");
			container2.Container.K0_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var packableItemParent1 = dummyParent.Lines.AddNew();
			packableItemParent1.Description = "HELLO";
			packableItemParent1.DescriptionSupplement = "Greeting";
			packableItemParent1.ZD1_Code = "WOP";
			packableItemParent1.TotalQtyUQ = "UNT";

			var packableItemParent2 = dummyParent.Lines.AddNew();
			packableItemParent2.Description = "COLOUR";
			packableItemParent2.DescriptionSupplement = "Colourful Mind";
			packableItemParent2.ZD1_Code = "LEZ";
			packableItemParent2.AutoPackPackageType = "CTN";
			packableItemParent2.TotalQty = 723.1m;
			packableItemParent2.TotalQtyUQ = "PLT";
			packableItemParent2.WeightPerUnit = 6m;
			packableItemParent2.WeightUQ = "G";

			((IActiveBusinessObjectCollection)dummyParent.Lines).ApplySort(new InstantiationTimeComparer());

			container2.Pack(packableItemParent2, 1.1m);
			container2.Pack(packableItemParent1, 2m);
			container2.Pack(packableItemParent1, 1m);

			box.Pack(packableItemParent1, 3.4m);

			// simulate case where release line no longer exists
			var emptyDivot = box.PackedItemDivots.AddNew();
			emptyDivot.KI_PackedQty = 3.4m;

			Shipment pkgPackageJobData = null;
			AssertNoExceptionThrown("Empty Divot should not cause exception",
				() => pkgPackageJobData = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob));
			AssertNotNull(pkgPackageJobData);
			AssertEquals("Two Containers should be created", 2, pkgPackageJobData.ContainerCollection.Count);
			AssertEquals("One Top Level Packing Line should be created and one in container", 2, pkgPackageJobData.PackingLineCollection.Count);
			AssertEquals("One Child Packed Item should be created", 1, pkgPackageJobData.PackingLineCollection[0].PackedItemCollection.Count);
			AssertNotNull(pkgPackageJobData.CommercialInfo);

			var containers = pkgPackageJobData.ContainerCollection.OrderBy(c => c.Link).ToList();
			var containerData = containers[1];
			var packingLines = pkgPackageJobData.PackingLineCollection.OrderByDescending(c => c.Link).ToList();
			var boxData = packingLines[0];
			var containerPiece = packingLines[1];
			AssertEquals(2, containerPiece.PackedItemCollection.Count);

			var packedItem1OnContainer1 = containerPiece.PackedItemCollection.OrderBy(c => c.PackedQuantity).ToList()[0];
			var packedItem2OnContainer1 = containerPiece.PackedItemCollection.OrderBy(c => c.PackedQuantity).ToList()[1];
			var packedItemOnPackageData = boxData.PackedItemCollection[0];
			var packedItemsData1 = pkgPackageJobData.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection[0];
			var packedItemsData2 = pkgPackageJobData.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection[1];

			AssertNull("Package is not a container child, so no Link to related container should be created.", boxData.ContainerLink);
			AssertEquals("Package should be assigned a Link, so it could be identified by other BizOs", 0, boxData.Link);

			CombineAssertions(delegate
			{
				AssertEquals("containerData.Link", 1, containerData.Link);
				AssertEquals("containerPiece.PackType.Code", "PCE", containerPiece.PackType.Code);
				AssertEquals("containerPiece.PackType.Description", "Piece", containerPiece.PackType.Description);
				AssertEquals("containerPiece.ContainerLink", 1, containerPiece.ContainerLink);
				AssertEquals("containerPiece.PackQty", 4L, containerPiece.PackQty);

				AssertEquals("packedItem1OnContainer1.CommercialInvoiceLineLink", 1, packedItem1OnContainer1.CommercialInvoiceLineLink);
				AssertEquals("packedItem1OnContainer1.Description", "COLOUR - Colourful Mind", packedItem1OnContainer1.Description);
				AssertEquals("packedItem1OnContainer1.PackedQuantity", 1.1m, packedItem1OnContainer1.PackedQuantity);
				AssertEquals("packedItem1OnContainer1.UnitOfQuantity.Code", "PLT", packedItem1OnContainer1.UnitOfQuantity.Code);

				AssertEquals("packedItem2OnContainer1.CommercialInvoiceLineLink", 0, packedItem2OnContainer1.CommercialInvoiceLineLink);
				AssertEquals("packedItem2OnContainer1.Description", "HELLO - Greeting", packedItem2OnContainer1.Description);
				AssertEquals("packedItem2OnContainer1.PackedQuantity", 3m, packedItem2OnContainer1.PackedQuantity);
				AssertEquals("packedItem2OnContainer1.UnitOfQuantity.Code", "PLT", packedItem1OnContainer1.UnitOfQuantity.Code);

				AssertEquals("packedItemOnPackageData.Description", "HELLO - Greeting", packedItemOnPackageData.Description);
				AssertEquals("packedItemOnPackageData.PackedQuantity", 3.4m, packedItemOnPackageData.PackedQuantity);
				AssertEquals("packedItemOnPackageData.UnitOfQuantity.Code", "UNT", packedItemOnPackageData.UnitOfQuantity.Code);
				AssertEquals("packedItemOnPackageData.CommercialInvoiceLineLink", 0, packedItemOnPackageData.CommercialInvoiceLineLink);

				AssertEquals("packedItemsData1.Description", "HELLO", packedItemsData1.Description);
				AssertEquals("packedItemsData1.LineNo", 0, packedItemsData1.LineNo);
				AssertEquals("packedItemsData1.Link", 0, packedItemsData1.Link);
				AssertEquals("packedItemsData1.InvoiceQuantity", 100m, packedItemsData1.InvoiceQuantity);
				AssertEquals("packedItemsData1.InvoiceQuantityUnit.Code", "UNT", packedItemsData1.InvoiceQuantityUnit.Code);
				AssertEquals("packedItemsData1.InvoiceQuantityUnit.Description", "Unit", packedItemsData1.InvoiceQuantityUnit.Description);
				AssertEquals("packedItemsData1.Weight", 200m, packedItemsData1.Weight);
				AssertEquals("packedItemsData1.WeightUnit.Code", "KG", packedItemsData1.WeightUnit.Code);
				AssertEquals("packedItemsData1.WeightUnit.Description", "Kilograms", packedItemsData1.WeightUnit.Description);

				var customField1 = packedItemsData1.CustomizedFieldCollection[0];
				AssertEquals("customField1.DataType", DataType.String, customField1.DataType);
				AssertEquals("customField1.Key", "ZD1 Code", customField1.Key);
				AssertEquals("customField1.Value", "WOP", customField1.Value);

				AssertEquals("packedItemsData2.Description", "COLOUR", packedItemsData2.Description);
				AssertEquals("packedItemsData2.LineNo", 1, packedItemsData2.LineNo);
				AssertEquals("packedItemsData2.Link", 1, packedItemsData2.Link);
				AssertEquals("packedItemsData2.InvoiceQuantity", 723.1m, packedItemsData2.InvoiceQuantity);
				AssertEquals("packedItemsData2.InvoiceQuantityUnit.Code", "PLT", packedItemsData2.InvoiceQuantityUnit.Code);
				AssertEquals("packedItemsData2.InvoiceQuantityUnit.Description", "Pallet", packedItemsData2.InvoiceQuantityUnit.Description);
				AssertEquals("packedItemsData2.Weight", 4338.6m, packedItemsData2.Weight);
				AssertEquals("packedItemsData2.WeightUnit.Code", "G", packedItemsData2.WeightUnit.Code);
				AssertEquals("packedItemsData2.WeightUnit.Description", "Grams", packedItemsData2.WeightUnit.Description);

				var customField2 = packedItemsData2.CustomizedFieldCollection[0];
				AssertEquals("customField2.DataType", DataType.String, customField2.DataType);
				AssertEquals("customField2.Key", "ZD1 Code", customField2.Key);
				AssertEquals("customField2.Value", "LEZ", customField2.Value);
			});
		}

		#endregion

		#region TestGetDataObject_PkgPackages_UNGDs

		public void TestGetDataObject_PkgPackages_UNGDs()
		{
			Data.CreatePackingData();
			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var container = pkgPackageJob.Packages.AddNew("CNT");
			CreateUNDG(container, "5555");
			CreateUNDG(container, "6666");

			var box = container.Packages.AddNew("BOX");
			CreateUNDG(box, "7777");

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var containerDataObject = pkgPackageJobDataObject.ContainerCollection[0];
			AssertEquals("Container should Contain 2 Dangerous Goods", 2, containerDataObject.UNDGCollection.Count);
			AssertCollectionContain(containerDataObject.UNDGCollection, "5555");
			AssertCollectionContain(containerDataObject.UNDGCollection, "6666");

			var boxDataObject = pkgPackageJobDataObject.PackingLineCollection[0];
			AssertEquals("Box should Contain 1 Dangerous Good", 1, boxDataObject.UNDGCollection.Count);
			AssertCollectionContain(boxDataObject.UNDGCollection, "7777");
		}

		void CreateUNDG(PkgPackage package, ZString code)
		{
			var undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = code;

			var undgDataItem = Factory.New<UNDGDataItem>();
			undgDataItem.DI_DG = undgSubstance.PK;

			package.UNDGs.Add(undgDataItem);
		}

		void AssertCollectionContain(List<UNDG> allUNDGs, ZString undgCodeToFind)
		{
			foreach (UNDG undg in allUNDGs)
			{
				if (undg.UNDGCode.Value == undgCodeToFind)
				{
					return;
				}
			}
			Fail(string.Format("UNDG with code {0} could not be found.", undgCodeToFind));
		}

		#endregion

		#region TestGetDataObject_PkgPackages_LoosePackageID

		public void TestGetDataObject_PkgPackages_LoosePackageID()
		{
			Data.CreatePackingData();
			Data.Dummy.IsLoosePackageIDsSupported = true;

			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var loosePackageID = pkgPackageJob.LoosePackageIDs.AddNew();
			loosePackageID.KPH_PackageID = "123";
			var package = pkgPackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "321");

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var packingLineCollection = pkgPackageJobDataObject.PackingLineCollection;
			AssertEquals("All packages has been exported to packing line", 2, packingLineCollection.Count);

			var loosePackageIDDataObject = packingLineCollection.Single(p => p.ReferenceNumber.Value == "123");
			AssertNull("Only the reference number must be populated", loosePackageIDDataObject.PackType);
			AssertNull("Only the reference number must be populated", loosePackageIDDataObject.PackQty);

			var packageDataObject = packingLineCollection.Single(p => p.ReferenceNumber.Value == "321");
			AssertEquals(Constants.PkgUnit.Pallet, packageDataObject.PackType.Code);
			AssertEquals(1L, packageDataObject.PackQty);
		}

		#endregion

		#region TestGetDataObject_PkgPackages_LoosePackage_ParentJobNotSupportLoosePackage

		public void TestGetDataObject_PkgPackages_LoosePackage_ParentJobNotSupportLoosePackage()
		{
			Data.CreatePackingData();

			var pkgPackageJob = Data.PackageJob;
			pkgPackageJob.KJ_JobID = "PJ00000001";
			var loosePackage = pkgPackageJob.LoosePackageIDs.AddNew();
			loosePackage.KPH_PackageID = "123";
			var package = pkgPackageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "321");

			AssertEquals("Precondition", false, Data.Dummy.IsLoosePackageIDsSupported);

			var pkgPackageJobDataObject = new PkgPackageJobDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, pkgPackageJob))).GetDataObject(pkgPackageJob);
			var packingLineCollection = pkgPackageJobDataObject.PackingLineCollection;
			AssertEquals("Only non-loose package can be exported", 1, packingLineCollection.Count);

			var packageDataObject = packingLineCollection.Single();
			AssertEquals("321", packageDataObject.ReferenceNumber.Value);
			AssertEquals(Constants.PkgUnit.Pallet, packageDataObject.PackType.Code);
			AssertEquals(1L, packageDataObject.PackQty);
		}

		#endregion

		#endregion
	}
}
