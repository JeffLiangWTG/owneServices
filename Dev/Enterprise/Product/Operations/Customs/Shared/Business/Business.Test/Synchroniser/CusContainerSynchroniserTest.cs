using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusContainerSynchroniserTest : SynchroniserTestCase
	{
		public void TestCusContainerSynchroniser()
		{
			var consol = CreateAirConsol();
			CommonContainer container = consol.Containers.AddNew();

			var customsContainer = Factory.New<BaseCusContainer>();

			var synchroniser = new CusContainerSynchroniser(customsContainer, container);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			container.JC_ContainerNum = TestContainerNumber;
			AssertEquals("CusContainer CO_ContainerNumber", TestContainerNumber, customsContainer.CO_ContainerNumber);

			container.JC_SealNum = TestSealNumber + "ab";
			AssertEquals("CusContainer CO_Seal", TestSealNumber + "AB", customsContainer.CO_Seal);

			container.JC_AdditionalSealNum = TestSealNumber2 + "ab";
			AssertEquals("CusContainer CO_SecondSeal", TestSealNumber2 + "AB", customsContainer.CO_SecondSeal);

			container.JC_IsControlledAtmosphere = true;

			var fortyFootGpContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, TestContainerTypeNK);
			container.JC_RC = fortyFootGpContainer.PK;

			AssertEquals("CusContainer CO_RC", fortyFootGpContainer.PK, customsContainer.CO_RC);

			container.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("CusContainer CO_FCL_LCL_AIR", Enterprise.Core.Constants.ContainerModes.FCL, customsContainer.CO_FCL_LCL_AIR);

			container.JC_AdditionalSealNum = TestSealNumber2 + "as";
			AssertEquals("CusContainer CO_SecondSeal", TestSealNumber2 + "AS", customsContainer.CO_SecondSeal);
		}

		public void TestCusContainerWhenConsolContainerIsOfTypeGRP()
		{
			var consol = CreateFCLConsol();
			CommonContainer container = consol.Containers.AddNew();

			var customsContainer = Factory.New<BaseCusContainer>();

			var synchroniser = new CusContainerSynchroniser(customsContainer, container);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			container.JC_ContainerNum = TestContainerNumber;
			AssertEquals("CusContainer CO_ContainerNumber", TestContainerNumber, customsContainer.CO_ContainerNumber);

			container.JC_SealNum = TestSealNumber;
			AssertEquals("CusContainer CO_Seal", TestSealNumber, customsContainer.CO_Seal);

			var fortyFootGpContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, TestContainerTypeNK);
			container.JC_RC = fortyFootGpContainer.PK;

			AssertEquals("CusContainer CO_RC", fortyFootGpContainer.PK, customsContainer.CO_RC);

			container.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.Groupage;
			AssertEquals("CusContainer CO_FCL_LCL_AIR", Enterprise.Core.Constants.ContainerModes.LCL, customsContainer.CO_FCL_LCL_AIR);
		}

		public void TestSourceContainerFieldsNotRevertToDefaultWhenIsCopyingIsTrue()
		{
			var consol = CreateFCLConsol();
			consol.JK_ConsolMode = "BCN";

			var sourceContainer = consol.Containers.AddNew();
			sourceContainer.JC_JK = consol.PK;

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Length = TestRC_Length;
			refContainer.RC_Height = TestRC_Height;
			refContainer.RC_Width = TestRC_Width;
			refContainer.RC_ContainerType = Constants.ContainerTypes.Refrigerated;
			refContainer.RC_TareWeight = TestRC_TareWeight;

			sourceContainer.JC_RC = refContainer.PK;

			AssertEquals(TestRC_Length, sourceContainer.JC_TotalLength);
			AssertEquals(TestRC_Height, sourceContainer.JC_TotalHeight);
			AssertEquals(TestRC_Width, sourceContainer.JC_TotalWidth);
			AssertEquals(TestRC_TareWeight, sourceContainer.JC_TareWeight);
			AssertEquals(true, sourceContainer.JC_IsControlledAtmosphere);

			sourceContainer.JC_TotalLength = TestJC_TotalLength;
			sourceContainer.JC_TotalHeight = TestJC_TotalHeight;
			sourceContainer.JC_TotalWidth = TestJC_TotalWidth;
			sourceContainer.JC_ContainerCount = 1;
			sourceContainer.JC_GrossWeightUQ = "kg";
			sourceContainer.JC_TareWeight = TestJC_TareWeight;
			sourceContainer.JC_IsControlledAtmosphere = false;

			var destination = Factory.New<BaseCusContainer>();
			destination.CO_ContainerNumber = sourceContainer.JC_ContainerNum;
			destination.CO_JC = sourceContainer.PK;

			var synchroniser = new CusContainerSynchroniser(destination, sourceContainer);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals(TestJC_TotalLength, sourceContainer.JC_TotalLength);
			AssertEquals(TestJC_TotalHeight, sourceContainer.JC_TotalHeight);
			AssertEquals(TestJC_TotalWidth, sourceContainer.JC_TotalWidth);
			AssertEquals(TestJC_TareWeight, sourceContainer.JC_TareWeight);
			AssertEquals(false, sourceContainer.JC_IsControlledAtmosphere);
		}

		public void TestTotalGoodsWeightFromPackLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			declaration.JE_JS = shipment.PK;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = shipment.JS_HouseBill;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.Shipments.Add(shipment);

			var forwardingContainer1 = consol.Containers.AddNew();
			forwardingContainer1.JC_ContainerNum = "C1";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_JC = forwardingContainer1.PK;
			packLine1.JL_ActualWeight = 10m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			var forwardingContainer2 = consol.Containers.AddNew();
			forwardingContainer2.JC_ContainerNum = "C2";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = forwardingContainer2.PK;
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 1m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;

			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise();

			AssertEquals("2 containers", 2, declaration.CusContainers.Count);

			var container1 = declaration.CusContainers.Find("C1");
			AssertNotNull(container1);
			var container2 = declaration.CusContainers.Find("C2");
			AssertNotNull(container2);

			AssertEquals(10m, container1.CO_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, container1.CO_WeightUQ);

			AssertEquals(1m, container2.CO_Weight);
			AssertEquals(Core.Constants.Weight.Tonnes, container2.CO_WeightUQ);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = forwardingContainer2.PK;
			packLine3.JL_PackageCount = 2;
			packLine3.JL_ActualWeight = 500m;
			packLine3.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;//two different units

			AssertEquals("converted to a KG", 1500m, container2.CO_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, container2.CO_WeightUQ);

			Factory.Save();

			forwardingContainer2.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			packLine3.JL_ActualWeight = 999999.999m;
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("CO_Weight remains unchanged", 1500m, container2.CO_Weight);
		}

		public void TestTotalGoodsWeightFromConsol()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var shipmentMaster = Factory.New<ForwardingShipment>();
			shipmentMaster.JS_HouseBill = "HBL1";
			declaration.JE_JS = shipmentMaster.PK;
			shipmentMaster.JS_ShipmentType = "BCN";
			shipmentMaster.JS_PackingMode = "BCN";
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = shipmentMaster.JS_HouseBill;

			var shipmentStd1 = Factory.New<ForwardingShipment>();
			shipmentStd1.JS_JS_ColoadMasterShipment = shipmentMaster.PK;
			var shipmentStd2 = Factory.New<ForwardingShipment>();
			shipmentStd2.JS_JS_ColoadMasterShipment = shipmentMaster.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_ConsolMode = "BCN";
			consol.Shipments.Add(shipmentMaster);
			consol.Shipments.Add(shipmentStd1);
			consol.Shipments.Add(shipmentStd2);

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "C2";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "C3";

			var packLine1 = shipmentMaster.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_JC = container1.PK;
			packLine1.JL_ActualWeight = 10m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			var packLine2 = shipmentStd1.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Decitons;

			var packLine3 = shipmentStd2.OuterPackLines.AddNew();
			packLine3.JL_JC = container3.PK;
			packLine3.JL_PackageCount = 2;
			packLine3.JL_ActualWeight = 30m;
			packLine3.JL_ActualWeightUQ = Core.Constants.Weight.Grams;

			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			declaration.ShipmentSynchroniser.Synchronise();

			var decContainer1 = declaration.CusContainers.Find("C1");
			AssertNotNull(decContainer1);
			var decContainer2 = declaration.CusContainers.Find("C2");
			AssertNotNull(decContainer2);
			var decContainer3 = declaration.CusContainers.Find("C3");
			AssertNotNull(decContainer3);

			AssertEquals("Goods Weight1", 10m, decContainer1.CO_Weight);
			AssertEquals("Goods WeightUQ1", Core.Constants.Weight.Kilograms, decContainer1.CO_WeightUQ);
			AssertEquals("Goods Weight2", 20m, decContainer2.CO_Weight);
			AssertEquals("Goods WeightUQ2", Core.Constants.Weight.Decitons, decContainer2.CO_WeightUQ);
			AssertEquals("Goods Weight3", 30m, decContainer3.CO_Weight);
			AssertEquals("Goods WeightUQ3", Core.Constants.Weight.Grams, decContainer3.CO_WeightUQ);
		}
	}
}
