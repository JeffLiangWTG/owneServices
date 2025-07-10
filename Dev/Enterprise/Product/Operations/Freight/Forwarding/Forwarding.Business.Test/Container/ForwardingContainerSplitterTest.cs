using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Testing
{
	sealed class ForwardingContainerSplitterTest : TestCaseWithFactory
	{
		#region General

		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ForwardingContainerSplitter(null));
			AssertNoExceptionThrown(() => new ForwardingContainerSplitter(Factory.New<ForwardingContainer>()));
		}

		public void TestFactory()
		{
			ForwardingContainer multiContainer = Factory.New<ForwardingContainer>();
			multiContainer.JC_ContainerCount = 3;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			AssertEquals(Factory, splitter.Factory);
		}

		public void TestDefaultSplitMethodProvider()
		{
			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(Factory.New<ForwardingContainer>());
			AssertEquals(ForwardingContainerSplitter.SplitMethod.None, splitter.SplitMethodProvider());
		}

		public void TestCallingSplitOnDetachedContainerDoesNotCauseAnException()
		{
			ForwardingContainer multiContainer = Factory.New<ForwardingContainer>();
			multiContainer.JC_ContainerCount = 3;

			AssertNull("Prerequisite", multiContainer.Consol);
			AssertNoExceptionThrown(() => (new ForwardingContainerSplitter(multiContainer)).Split());
		}

		public void TestRemovedContainerIsDeletedFromDatabase()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 600;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 600m;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.Litre;
			shipment.JS_OuterPacks = 7;

			ForwardingPackLine packLine = shipment.OuterPackLines[0];

			Factory.Save();

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer) { SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly };
			splitter.Split();

			Factory.Save();

			AssertNull(Factory.LoadTop1<ForwardingContainer>(new ZQuery(JobContainerSchema.PK, multiContainer.PK)));
			AssertNull(Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.PK, packLine.PK)));
		}

		#endregion

		#region Preconditions

		public void TestCheckIsMultiContainer()
		{
			ForwardingContainer multiContainer = Factory.New<ForwardingContainer>();
			multiContainer.JC_ContainerCount = -1;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();
			AssertEquals("Selected container is not a multi-container", splitResult);

			multiContainer.JC_ContainerCount = 0;
			splitResult = splitter.Split();
			AssertEquals("Selected container is not a multi-container", splitResult);

			multiContainer.JC_ContainerCount = 1;
			splitResult = splitter.Split();
			AssertEquals("Selected container is not a multi-container", splitResult);

			multiContainer.JC_ContainerCount = 33;
			splitResult = splitter.Split();
			AssertEquals(ZString.Empty, splitResult);
		}

		public void TestCheckHasProducts()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;

			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);

			shipment.OuterPackLines[0].Products.AddNew();

			AssertEquals("Prerequisite", 1, shipment.OuterPackLines[0].Products.Count);

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();
			AssertEquals("Container cannot have any products packed", splitResult);

			shipment.OuterPackLines[0].Products.DeleteAll();

			multiContainer.JC_ContainerCount = 33;
			splitResult = splitter.Split();
			AssertEquals(ZString.Empty, splitResult);
		}

		public void TestCheckDangnerousGoods_AllowSplitWhenNotPresent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;

			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();
			AssertEquals(ZString.Empty, splitResult);
			AssertEquals(2, consol.Containers.Count);
		}

		public void TestCheckDangnerousGoods_AllowSplitWhenBothDGVolumeAndWeightAreNotSet()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;
			shipment.OuterPackLines[0].JL_ActualVolume = 33m;
			shipment.OuterPackLines[0].JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			UNDGDataItem dg = shipment.OuterPackLines[0].UNDGs.AddNew();

			AssertEquals("Prerequisite", 0m, dg.DI_DGVolume);
			AssertEquals("Prerequisite", 0m, dg.DI_DGWeight);

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer) { SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly };
			ZString splitResult = splitter.Split();

			AssertEquals(ZString.Empty, splitResult);
		}

		public void TestCheckDangnerousGoods_DisallowIfMoreThanOne()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;
			shipment.OuterPackLines[0].JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			UNDGDataItem dg1 = shipment.OuterPackLines[0].UNDGs.AddNew();
			dg1.DI_DGVolume = 1.3m;
			dg1.DI_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			UNDGDataItem dg2 = shipment.OuterPackLines[0].UNDGs.AddNew();
			dg2.DI_DGVolume = 2m;
			dg2.DI_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();
			AssertEquals("The Split functionality is not allowed due to insufficient information on dangerous goods packline/s", splitResult);
		}

		public void TestCheckDangnerousGoods_DisallowIfPackLineVolumeIsNotValid()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;
			shipment.OuterPackLines[0].JL_ActualVolumeUQ = ZString.Empty;

			UNDGDataItem dg = shipment.OuterPackLines[0].UNDGs.AddNew();
			dg.DI_DGVolume = 1.3m;
			dg.DI_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);
			AssertEquals("Prerequisite", false, (new ZVolume(shipment.OuterPackLines[0].JL_ActualVolume, shipment.OuterPackLines[0].JL_ActualVolumeUQ)).IsValid);
			AssertEquals("Prerequisite", true, (new ZVolume(dg.DI_DGVolume, dg.DI_UnitOfVolume)).IsValid);

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();
			AssertEquals("The Split functionality is not allowed due to insufficient information on dangerous goods packline/s", splitResult);
		}

		public void TestCheckDangnerousGoods_DisallowIfDGVolumeIsNotValid()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;
			shipment.OuterPackLines[0].JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			UNDGDataItem dg = shipment.OuterPackLines[0].UNDGs.AddNew();
			dg.DI_DGVolume = 1.3m;
			dg.DI_UnitOfVolume = ZString.Empty;

			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);
			AssertEquals("Prerequisite", true, (new ZVolume(shipment.OuterPackLines[0].JL_ActualVolume, shipment.OuterPackLines[0].JL_ActualVolumeUQ)).IsValid);
			AssertEquals("Prerequisite", false, (new ZVolume(dg.DI_DGVolume, dg.DI_UnitOfVolume)).IsValid);

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();
			AssertEquals("The Split functionality is not allowed due to insufficient information on dangerous goods packline/s", splitResult);
		}

		public void TestCheckDangnerousGoods_VolumeAndWeighUnits()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;

			AssertEquals("Prerequisite", 1, shipment.OuterPackLines.Count);

			UNDGDataItem dg = shipment.OuterPackLines[0].UNDGs.AddNew();
			dg.DI_DGVolume = 2000m;
			dg.DI_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			dg.DI_DGWeight = 1000m;
			dg.DI_UnitOfWeight = Core.Constants.Weight.Pounds;

			shipment.OuterPackLines[0].JL_ActualVolume = 2m;
			shipment.OuterPackLines[0].JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			shipment.OuterPackLines[0].JL_ActualWeight = 1m;
			shipment.OuterPackLines[0].JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer) { SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly };
			ZString splitResult = splitter.Split();
			AssertEquals("The Split functionality is not allowed due to insufficient information on dangerous goods packline/s", splitResult);

			dg.DI_UnitOfVolume = Core.Constants.Volume.Litre;
			splitter = new ForwardingContainerSplitter(multiContainer) { SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly };
			splitResult = splitter.Split();
			AssertEquals("The Split functionality is not allowed due to insufficient information on dangerous goods packline/s", splitResult);

			dg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
			splitter = new ForwardingContainerSplitter(multiContainer) { SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly };
			splitResult = splitter.Split();
			AssertEquals(ZString.Empty, splitResult);
		}

		#endregion

		#region Split

		public void TestSplit_ContainerTareWeight()
		{
			AssertSplitContainerTareWeight(() => ForwardingContainerSplitter.SplitMethod.PackAllIntoFirstContainer);
			AssertSplitContainerTareWeight(() => ForwardingContainerSplitter.SplitMethod.DistributeEvenly);
		}

		void AssertSplitContainerTareWeight(Func<ForwardingContainerSplitter.SplitMethod> splitMethodProvider)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;
			multiContainer.JC_TareWeight = 3330;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 600;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 600m;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.Litre;
			shipment.JS_OuterPacks = 7;

			AssertEquals("Prerequiste", 1, shipment.OuterPackLines.Count);

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = splitMethodProvider;
			ZString splitResult = splitter.Split();

			AssertEquals(ZString.Empty, splitResult);
			AssertArrayEqualsByElements(new ZDecimal[] { 1110, 1110, 1110 }, consol.Containers.Cast<ForwardingContainer>().Select(c => c.JC_TareWeight).ToArray());
			AssertEquals(3, shipment.OuterPackLines.Count);
		}

		public void TestSplit_NoShipmentNoPackLines()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();

			AssertEquals(3, consol.Containers.Count);

			foreach (ForwardingContainer container in consol.Containers)
			{
				AssertEquals(1, (int)container.JC_ContainerCount);
				AssertEquals(0, container.PackLines.Count);
			}
		}

		public void TestSplit_Container_NotCopyContainerJobID_PackAllIntoFirstContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			var multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;
			multiContainer.JC_ContainerJobID = "D00001310";

			var splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.PackAllIntoFirstContainer;
			var splitResult = splitter.Split();

			AssertEquals(3, consol.Containers.Count);
			AssertEquals("D00001310", consol.Containers[0].JC_ContainerJobID);
			AssertEquals(string.Empty, consol.Containers[1].JC_ContainerJobID);
			AssertEquals(string.Empty, consol.Containers[2].JC_ContainerJobID);

			foreach (ForwardingContainer container in consol.Containers)
			{
				AssertEquals(1, (int)container.JC_ContainerCount);
				AssertEquals(0, container.PackLines.Count);
			}
		}

		public void TestSplit_Container_NotCopyContainerJobID_MultipleContainersDistributeEvenly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;
			multiContainer.JC_ContainerJobID = "D00001310";

			var splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			var splitResult = splitter.Split();

			AssertEquals(3, consol.Containers.Count);
			AssertEquals(string.Empty, consol.Containers[0].JC_ContainerJobID);
			AssertEquals(string.Empty, consol.Containers[1].JC_ContainerJobID);
			AssertEquals(string.Empty, consol.Containers[2].JC_ContainerJobID);

			foreach (ForwardingContainer container in consol.Containers)
			{
				AssertEquals(1, (int)container.JC_ContainerCount);
				AssertEquals(0, container.PackLines.Count);
			}
		}

		public void TestSplit_PackAllIntoFirstContainer()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 600;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment1.JS_ActualVolume = 600m;
			shipment1.JS_UnitOfVolume = Core.Constants.Volume.Litre;
			shipment1.JS_OuterPacks = 7;

			AssertEquals("Prerequiste", 1, shipment1.OuterPackLines.Count);

			shipment1.OuterPackLines[0].JL_Length = 0.1m;
			shipment1.OuterPackLines[0].JL_Width = 0.2m;
			shipment1.OuterPackLines[0].JL_Height = 0.3m;
			shipment1.OuterPackLines[0].JL_UnitOfDimension = Core.Constants.Length.Metres;

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 333.33m;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Ounces;
			shipment2.JS_ActualVolume = 6m;
			shipment2.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			shipment2.JS_OuterPacks = 33;

			AssertEquals("Prerequiste", 1, shipment2.OuterPackLines.Count);

			shipment2.OuterPackLines[0].JL_Length = 1m;
			shipment2.OuterPackLines[0].JL_Width = 2m;
			shipment2.OuterPackLines[0].JL_Height = 3m;
			shipment2.OuterPackLines[0].JL_UnitOfDimension = Core.Constants.Length.Feet;

			AssertContainsExactElementsInAnyOrder("Prerequiste", new[] { shipment1.OuterPackLines[0], shipment2.OuterPackLines[0] }, multiContainer.PackLines);

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer) { SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.PackAllIntoFirstContainer };
			ZString splitResult = splitter.Split();

			AssertEquals(ZString.Empty, splitResult);
			AssertEquals(3, consol.Containers.Count);

			AssertContainsExactElementsInAnyOrder(new[] { shipment1.OuterPackLines[0], shipment2.OuterPackLines[0] }, consol.Containers[0].PackLines);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1.OuterPackLines[1], shipment2.OuterPackLines[1] }, consol.Containers[1].PackLines);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1.OuterPackLines[2], shipment2.OuterPackLines[2] }, consol.Containers[2].PackLines);

			AssertEquals(3, shipment1.OuterPackLines.Count);

			AssertArrayEqualsByElements(new ZInt[] { 7, 0, 0 }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 600m, 0m, 0m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZString[] { "KG", "KG", "KG" }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeightUQ).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 42m, 0m, 0m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());
			AssertArrayEqualsByElements(new ZString[] { "L", "L", "L" }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolumeUQ).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 0.1m, 0.1m, 0.1m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Length).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 0.2m, 0.2m, 0.2m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Width).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 0.3m, 0.3m, 0.3m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Height).ToArray());
			AssertArrayEqualsByElements(new ZString[] { "M", "M", "M" }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_UnitOfDimension).ToArray());

			AssertEquals(3, shipment2.OuterPackLines.Count);

			AssertArrayEqualsByElements(new ZInt[] { 33, 0, 0 }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 333.33m, 0m, 0m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZString[] { "OZ", "OZ", "OZ" }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeightUQ).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 198m, 0m, 0m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());
			AssertArrayEqualsByElements(new ZString[] { "CF", "CF", "CF" }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolumeUQ).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 1m, 1m, 1m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Length).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 2m, 2m, 2m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Width).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 3m, 3m, 3m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Height).ToArray());
			AssertArrayEqualsByElements(new ZString[] { "FT", "FT", "FT" }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_UnitOfDimension).ToArray());
		}

		public void TestSplit_PackEvenly_Calculations()
		{
			var shipment = CreateShipmentAndSplit(6, 12m, 18m, 3);

			AssertEquals(3, shipment.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 2, 2, 2 }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 4m, 4m, 4m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 6m, 6m, 6m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());

			shipment = CreateShipmentAndSplit(1, 100m, 0m, 4);

			AssertEquals(4, shipment.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 1, 0, 0, 0 }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 100m, 0m, 0m, 0m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 0m, 0m, 0m, 0m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());

			shipment = CreateShipmentAndSplit(3, 12m, 22m, 3);

			AssertEquals(3, shipment.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 1, 1, 1 }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 4m, 4m, 4m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 7.333m, 7.333m, 7.333m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());

			shipment = CreateShipmentAndSplit(0, 12m, 22m, 3);

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 0 }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 12m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 22m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());

			shipment = CreateShipmentAndSplit(0, 0m, 22m, 3);

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 0 }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 0m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 22m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());
		}

		public void TestSplit_PackEvenly_Calculations_ZeroesDoNotCauseAnException()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment = CreateShipment(0, 0m, 0m);
			consol.Shipments.Add(shipment);

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			AssertEquals("Prerequisite", 0, shipment.OuterPackLines.Count);
			shipment.OuterPackLines.AddNew();

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();

			AssertEquals(3, consol.Containers.Count);
			AssertEquals(3, shipment.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 0, 0, 0 }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 0m, 0m, 0m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 0m, 0m, 0m }, shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());
		}

		public void TestSplit_PackEvenly_DimentionsDoNotOverrideVolume()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment1 = CreateShipment(6, 300m, 1m);
			consol.Shipments.Add(shipment1);

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			shipment1.OuterPackLines[0].JL_Length = 3m;
			shipment1.OuterPackLines[0].JL_Width = 4m;
			shipment1.OuterPackLines[0].JL_Height = 5m;

			AssertEquals("Prerequisite", 360m, shipment1.OuterPackLines[0].JL_ActualVolume);

			shipment1.OuterPackLines[0].JL_ActualVolume = 1;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();

			AssertEquals(3, consol.Containers.Count);
			AssertArrayEqualsByElements(new ZDecimal[] { 0.333m, 0.333m, 0.333m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());
		}

		public void TestSplit_PackEvenly_Packages()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment1 = CreateShipment(6, 300m, 600m);
			consol.Shipments.Add(shipment1);

			ForwardingShipment shipment2 = CreateShipment(2, 300m, 12m);
			consol.Shipments.Add(shipment2);

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			shipment1.OuterPackLines[0].JL_Length = 1m;
			shipment1.OuterPackLines[0].JL_Width = 10m;
			shipment1.OuterPackLines[0].JL_Height = 10m;

			shipment2.OuterPackLines[0].JL_Length = 1m;
			shipment2.OuterPackLines[0].JL_Width = 2m;
			shipment2.OuterPackLines[0].JL_Height = 3m;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();

			AssertEquals(3, consol.Containers.Count);
			AssertEquals(3, shipment1.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 2, 2, 2 }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 100m, 100m, 100m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 200m, 200m, 200m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 1m, 1m, 1m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Length).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 10m, 10m, 10m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Width).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 10m, 10m, 10m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Height).ToArray());

			AssertEquals(3, shipment2.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 1, 1, 0 }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 150m, 150m, 0m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 6m, 6m, 0m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 1m, 1m, 1m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Length).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 2m, 2m, 2m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Width).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 3m, 3m, 3m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_Height).ToArray());
		}

		public void TestSplit_PackEvenly_NoPackages()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = CreateShipment(0, 33m, 1.2m);
			consol.Shipments.Add(shipment1);

			var shipment2 = CreateShipment(0, 0m, 12.221m);
			consol.Shipments.Add(shipment2);

			var multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			var splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			var splitResult = splitter.Split();

			AssertEquals(3, consol.Containers.Count);
			AssertEquals(1, shipment1.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 0 }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 33m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 1.2m }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());

			AssertEquals(1, shipment2.OuterPackLines.Count);
			AssertArrayEqualsByElements(new ZInt[] { 0 }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_PackageCount).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 0m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualWeight).ToArray());
			AssertArrayEqualsByElements(new ZDecimal[] { 12.221m }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_ActualVolume).ToArray());
		}

		public void TestSplit_DangerousGoods()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualVolume = 4m;
			shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment1.JS_ActualWeight = 1000m;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment1.JS_OuterPacks = 2;

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			AssertEquals("Prerequisite", 1, shipment1.OuterPackLines.Count);

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "AAA";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			UNDGDataItem dg = shipment1.OuterPackLines[0].UNDGs.AddNew();
			dg.DI_DG = subs.PK;
			dg.LinkDefault(subs);
			dg.DI_DGVolume = 4000m;
			dg.DI_UnitOfVolume = Core.Constants.Volume.Litre;
			dg.DI_DGWeight = 1m;
			dg.DI_UnitOfWeight = Core.Constants.Weight.Tonnes;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer) { SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly };
			ZString splitResult = splitter.Split();

			AssertEquals(3, consol.Containers.Count);
			AssertEquals(3, shipment1.OuterPackLines.Count);

			AssertEquals(1, shipment1.OuterPackLines[0].JL_PackageCount);
			AssertEquals(2m, shipment1.OuterPackLines[0].JL_ActualVolume);
			AssertEquals(1, shipment1.OuterPackLines[0].UNDGs.Count);

			AssertEquals(0.5m, shipment1.OuterPackLines[0].UNDGs[0].DI_DGWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, shipment1.OuterPackLines[0].UNDGs[0].DI_UnitOfWeight);
			AssertEquals(2000m, shipment1.OuterPackLines[0].UNDGs[0].DI_DGVolume);
			AssertEquals(Core.Constants.Volume.Litre, shipment1.OuterPackLines[0].UNDGs[0].DI_UnitOfVolume);
			AssertEquals("AAA", shipment1.OuterPackLines[0].UNDGs[0].Substance.DG_Code);

			AssertEquals(0.5m, shipment1.OuterPackLines[1].UNDGs[0].DI_DGWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, shipment1.OuterPackLines[1].UNDGs[0].DI_UnitOfWeight);
			AssertEquals(2000m, shipment1.OuterPackLines[1].UNDGs[0].DI_DGVolume);
			AssertEquals(Core.Constants.Volume.Litre, shipment1.OuterPackLines[1].UNDGs[0].DI_UnitOfVolume);
			AssertEquals("AAA", shipment1.OuterPackLines[1].UNDGs[0].Substance.DG_Code);

			AssertEquals(0m, shipment1.OuterPackLines[2].UNDGs[0].DI_DGWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, shipment1.OuterPackLines[2].UNDGs[0].DI_UnitOfWeight);
			AssertEquals(0m, shipment1.OuterPackLines[2].UNDGs[0].DI_DGVolume);
			AssertEquals(Core.Constants.Volume.Litre, shipment1.OuterPackLines[2].UNDGs[0].DI_UnitOfVolume);
			AssertEquals("AAA", shipment1.OuterPackLines[2].UNDGs[0].Substance.DG_Code);
		}

		public void TestSpitContainersOriginAndDepartureConfirmations()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment1 = CreateShipment(6, 300m, 600m);
			consol.Shipments.Add(shipment1);

			ForwardingShipment shipment2 = CreateShipment(2, 300m, 12m);
			consol.Shipments.Add(shipment2);

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 3;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();

			AssertEquals(ZString.Empty, splitResult);
			AssertEquals(3, consol.Containers.Count);

			AssertNotNull(consol.Containers[0].OriginConfirm);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, consol.Containers[0].OriginConfirm.Shipments);
			AssertNotNull(consol.Containers[1].OriginConfirm);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, consol.Containers[1].OriginConfirm.Shipments);
			AssertNotNull(consol.Containers[2].OriginConfirm);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, consol.Containers[2].OriginConfirm.Shipments);

			AssertNotNull(consol.Containers[0].DestinationConfirm);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, consol.Containers[0].DestinationConfirm.Shipments);
			AssertNotNull(consol.Containers[1].DestinationConfirm);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, consol.Containers[1].DestinationConfirm.Shipments);
			AssertNotNull(consol.Containers[2].DestinationConfirm);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, consol.Containers[2].DestinationConfirm.Shipments);
		}

		public void TestTransitWareHouseFieldsClonedWhenSplit()
		{
			ZDateTime lastKnownTWStatusDateTime = ZDateTime.Now;

			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = CreateShipment(0, 0, 0);
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
			packLine1.JL_DepartureTransitWarehouseExcluded = false;
			packLine1.JL_OA_LastKnownTransitWarehouseAddress = orgAddress.PK;
			packLine1.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			packLine1.JL_LastKnownTransitWarehouseStatusDateTime = lastKnownTWStatusDateTime;
			consol.Shipments.Add(shipment1);

			var shipment2 = CreateShipment(0, 0, 0);
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
			packLine2.JL_DepartureTransitWarehouseExcluded = false;
			packLine2.JL_OA_LastKnownTransitWarehouseAddress = orgAddress.PK;
			packLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			packLine2.JL_LastKnownTransitWarehouseStatusDateTime = lastKnownTWStatusDateTime;
			consol.Shipments.Add(shipment2);

			var multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = 2;

			var splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			var splitResult = splitter.Split();

			AssertEquals(2, consol.Containers.Count);
			AssertEquals(1, shipment1.OuterPackLines.Count);
			AssertArrayEqualsByElements("Expecting Origin Transit Warehouse Status to be Confirmed", new ZString[] { FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_OriginTransitWarehouseStatus).ToArray());
			AssertArrayEqualsByElements("Expecting Departure Transit Warehouse Excluded to be False", new ZBool[] { false }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_DepartureTransitWarehouseExcluded).ToArray());
			AssertArrayEqualsByElements("Expecting Last Known Transit Warehouse Address to be copied", new ZGuid[] { orgAddress.PK }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_OA_LastKnownTransitWarehouseAddress).ToArray());
			AssertArrayEqualsByElements("Expecting Last Known Transit Warehouse Status to be Received", new ZString[] { FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_LastKnownTransitWarehouseStatus).ToArray());
			AssertArrayEqualsByElements("Expecting Last Known Transit Warehouse Status Date Time to be copied", new ZDateTime[] { lastKnownTWStatusDateTime }, shipment1.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_LastKnownTransitWarehouseStatusDateTime).ToArray());

			AssertEquals(1, shipment2.OuterPackLines.Count);
			AssertArrayEqualsByElements("Expecting Origin Transit Warehouse Status to be Confirmed", new ZString[] { FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_OriginTransitWarehouseStatus).ToArray());
			AssertArrayEqualsByElements("Expecting Departure Transit Warehouse Excluded to be False", new ZBool[] { false }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_DepartureTransitWarehouseExcluded).ToArray());
			AssertArrayEqualsByElements("Expecting Last Known Transit Warehouse Address to be copied", new ZGuid[] { orgAddress.PK }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_OA_LastKnownTransitWarehouseAddress).ToArray());
			AssertArrayEqualsByElements("Expecting Last Known Transit Warehouse Status to be Received", new ZString[] { FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_LastKnownTransitWarehouseStatus).ToArray());
			AssertArrayEqualsByElements("Expecting Last Known Transit Warehouse Status Date Time to be copied", new ZDateTime[] { lastKnownTWStatusDateTime }, shipment2.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_LastKnownTransitWarehouseStatusDateTime).ToArray());
		}

		#endregion

		#region Implementation

		ForwardingShipment CreateShipmentAndSplit(int packages, decimal weight, decimal volume, short noOfSplits)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingShipment shipment = CreateShipment(packages, weight, volume);
			consol.Shipments.Add(shipment);

			ForwardingContainer multiContainer = consol.Containers.AddNew();
			multiContainer.JC_ContainerCount = noOfSplits;

			ForwardingContainerSplitter splitter = new ForwardingContainerSplitter(multiContainer);
			splitter.SplitMethodProvider = () => ForwardingContainerSplitter.SplitMethod.DistributeEvenly;
			ZString splitResult = splitter.Split();

			AssertEquals(ZString.Empty, splitResult);

			return shipment;
		}

		ForwardingShipment CreateShipment(int packages, decimal weight, decimal volume)
		{
			return CreateShipment(packages, weight, Core.Constants.Weight.Kilograms, volume, Core.Constants.Volume.CubicMetres);
		}

		ForwardingShipment CreateShipment(int packages, decimal weight, string weightUnit, decimal volume, string volumeUnit)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OuterPacks = packages;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = weightUnit;
			shipment.JS_ActualVolume = volume;
			shipment.JS_UnitOfVolume = volumeUnit;

			return shipment;
		}

		#endregion
	}
}
