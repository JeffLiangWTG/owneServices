using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class PackingSynchroniserTest : Customs.Business.Testing.PackingSynchroniserTest<JobDeclaration>
	{
		public override void TestSychroniseCore()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";//US syncs to MB as 12345678
			DecorateConsolToBeRelevant(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX123456";

			consol.Shipments.Add(shipment);

			declaration.CusContainers.AddNew().CO_ContainerNumber = container.JC_ContainerNum;

			PackLine containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 10;
			containerised.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			containerised.JL_JC = container.PK;
			containerised.JL_MarksAndNumbers = "MarksAndNumbers1";

			containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 15;
			containerised.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			containerised.JL_JC = container.PK;
			containerised.JL_MarksAndNumbers = "MarksAndNumbers2";

			PackLine nonContainerised = shipment.OuterPackLines.AddNew();
			nonContainerised.JL_PackageCount = 20;
			nonContainerised.JL_F3_NKPackType = Core.Constants.PkgUnit.Carton;
			nonContainerised.JL_JC = ZGuid.Empty;
			nonContainerised.JL_Calc_ContainerNumber = "";
			nonContainerised.JL_MarksAndNumbers = "MarksAndNumbers3";

			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("There should be three rows", 3, declaration.Packages.Count);

			Package package = declaration.Packages[0];
			AssertEquals(10, package.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Pallet), package.CW_PackType);
			AssertEquals(declaration.CusContainers[0], package.PackingGroup.Container);
			AssertEquals("", package.CW_MarksAndNos);

			package = declaration.Packages[1];
			AssertEquals(15, package.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Bag), package.CW_PackType);
			AssertEquals(declaration.CusContainers[0], package.PackingGroup.Container);
			AssertEquals("", package.CW_MarksAndNos);

			package = declaration.Packages[2];
			AssertEquals(20, package.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Carton), package.CW_PackType);
			AssertEquals(null, package.PackingGroup.Container);
			AssertEquals("", package.CW_MarksAndNos);
		}

		public void TestSynchronizePackingLineForImportJobWhenHouseBillIsEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX123456";

			shipment.JS_HouseBill = ZString.Empty;
			consol.Shipments.Add(shipment);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = container.JC_ContainerNum;

			var containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 10;
			containerised.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			containerised.JL_JC = container.PK;
			containerised.JL_MarksAndNumbers = "MarksAndNumbers1";

			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("There should be one rows", 1, declaration.Packages.Count);

			var package = declaration.Packages[0];
			AssertEquals(10, package.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Pallet), package.CW_PackType);
			AssertEquals(declaration.CusContainers[0], package.PackingGroup.Container);
			AssertEquals("", package.CW_MarksAndNos);
		}

		public void TestSynchronizePackingLineForExportJobWhenHouseBillIsEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";
			DecorateConsolToBeRelevant(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX123456";

			shipment.JS_HouseBill = ZString.Empty;
			consol.Shipments.Add(shipment);

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = container.JC_ContainerNum;

			var containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 10;
			containerised.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			containerised.JL_JC = container.PK;
			containerised.JL_MarksAndNumbers = "MarksAndNumbers1";

			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("There should be one rows", 1, declaration.Packages.Count);

			var package = declaration.Packages[0];
			AssertEquals(10, package.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Pallet), package.CW_PackType);
			AssertEquals(declaration.CusContainers[0], package.PackingGroup.Container);
			AssertEquals("", package.CW_MarksAndNos);
		}

		public void TestLikePackLinesAreRolledUpInto1Line()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			shipment.JS_HouseBill = "HB001";
			consol.Shipments.Add(shipment);

			#region container 1

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "HLMU1234561";
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = container1.JC_ContainerNum;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 10;
			packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline1.JL_JC = container1.PK;
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 10;
			packline2.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline2.JL_JC = container1.PK;
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 10;
			packline3.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline3.JL_JC = container1.PK;
			packline3.JL_MarksAndNumbers = "MarksAndNumbers3";

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 10;
			packline4.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline4.JL_JC = container1.PK;
			packline4.JL_MarksAndNumbers = "MarksAndNumbers4";

			var packline5 = shipment.OuterPackLines.AddNew();
			packline5.JL_PackageCount = 10;
			packline5.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline5.JL_JC = container1.PK;
			packline5.JL_MarksAndNumbers = "MarksAndNumbers5";

			var packline6 = shipment.OuterPackLines.AddNew();
			packline6.JL_PackageCount = 10;
			packline6.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline6.JL_JC = container1.PK;
			packline6.JL_MarksAndNumbers = "MarksAndNumbers6";

			var packline7 = shipment.OuterPackLines.AddNew();
			packline7.JL_PackageCount = 10;
			packline7.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline7.JL_JC = container1.PK;
			packline7.JL_MarksAndNumbers = "MarksAndNumbers7";

			var packline8 = shipment.OuterPackLines.AddNew();
			packline8.JL_PackageCount = 10;
			packline8.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline8.JL_JC = container1.PK;
			packline8.JL_MarksAndNumbers = "MarksAndNumbers8";

			var packline9 = shipment.OuterPackLines.AddNew();
			packline9.JL_PackageCount = 10;
			packline9.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline9.JL_JC = container1.PK;
			packline9.JL_MarksAndNumbers = "MarksAndNumbers9";

			var packline10 = shipment.OuterPackLines.AddNew();
			packline10.JL_PackageCount = 10;
			packline10.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline10.JL_JC = container1.PK;
			packline10.JL_MarksAndNumbers = "MarksAndNumbers10";

			#endregion

			#region container 2

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CSJU0293847";
			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = container2.JC_ContainerNum;

			var packline11 = shipment.OuterPackLines.AddNew();
			packline11.JL_PackageCount = 10;
			packline11.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			packline11.JL_JC = container2.PK;
			packline11.JL_MarksAndNumbers = "MarksAndNumbers11";

			var packline12 = shipment.OuterPackLines.AddNew();
			packline12.JL_PackageCount = 10;
			packline12.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			packline12.JL_JC = container2.PK;
			packline12.JL_MarksAndNumbers = "MarksAndNumbers12";

			var packline13 = shipment.OuterPackLines.AddNew();
			packline13.JL_PackageCount = 12;
			packline13.JL_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			packline13.JL_JC = container2.PK;
			packline13.JL_MarksAndNumbers = "MarksAndNumbers13";

			var packline14 = shipment.OuterPackLines.AddNew();
			packline14.JL_PackageCount = 10;
			packline14.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			packline14.JL_JC = container2.PK;
			packline14.JL_MarksAndNumbers = "MarksAndNumbers14";

			var packline15 = shipment.OuterPackLines.AddNew();
			packline15.JL_PackageCount = 10;
			packline15.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			packline15.JL_JC = container2.PK;
			packline15.JL_MarksAndNumbers = "MarksAndNumbers15";

			var packline16 = shipment.OuterPackLines.AddNew();
			packline16.JL_PackageCount = 144;
			packline16.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
			packline16.JL_JC = container2.PK;
			packline16.JL_MarksAndNumbers = "MarksAndNumbers16";

			#endregion

			#region container 3

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "MSKU0413847";
			var cusContainer3 = declaration.CusContainers.AddNew();
			cusContainer3.CO_ContainerNumber = container3.JC_ContainerNum;

			var packline17 = shipment.OuterPackLines.AddNew();
			packline17.JL_PackageCount = 25;
			packline17.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline17.JL_JC = container3.PK;
			packline17.JL_MarksAndNumbers = "MarksAndNumbers17";

			var packline18 = shipment.OuterPackLines.AddNew();
			packline18.JL_PackageCount = 1000;
			packline18.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			packline18.JL_JC = container3.PK;
			packline18.JL_MarksAndNumbers = "MarksAndNumbers18";

			#endregion

			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("The 18 forwarding package lines should should be rolled up into 6 declaration package rows - aggregated on container & pack type", 6, declaration.Packages.Count);

			var packagePallet = declaration.Packages[0];
			AssertEquals("This should have accumulated all 10 like packing type lines", 100, packagePallet.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Pallet), packagePallet.CW_PackType);

			var packageBag = declaration.Packages[1];
			AssertEquals("This should have accumulated all 4 like packing type (Bag) lines from Container 2", 40, packageBag.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Bag), packageBag.CW_PackType);

			var packageBottle = declaration.Packages[2];
			AssertEquals("This should be the 1 distinct Bottle packing type line in Container 2", 12, packageBottle.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Bottle), packageBottle.CW_PackType);

			var packageBox = declaration.Packages[3];
			AssertEquals("This should be the 1 distinct Box packing type line in container 2", 144, packageBox.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Box), packageBox.CW_PackType);

			var packagePalletContainer3 = declaration.Packages[4];
			AssertEquals("This should be the 1 distinct Pallet packing type line for the third container", 25, packagePalletContainer3.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Pallet), packagePalletContainer3.CW_PackType);

			var packageBagContainer3 = declaration.Packages[5];
			AssertEquals("This should be the 1 distinct Bag packing type line for the third container", 1000, packageBagContainer3.CW_PackQty);
			AssertEquals(PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Bag), packageBagContainer3.CW_PackType);

			var packlineNew = shipment.OuterPackLines.AddNew();
			packlineNew.JL_PackageCount = 12;
			packlineNew.JL_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			packlineNew.JL_JC = container2.PK;
			packlineNew.JL_MarksAndNumbers = "MarksAndNumbers13";

			var existingPackages = declaration.Packages.ToArray();
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("The existing package is updated with new quantity", 24, (declaration.Packages.Cast<Customs.Business.IPackingInformation>().Single(p => p.HouseBillContainer.Container.CO_ContainerNumber == "CSJU0293847" && p.HouseBillContainer.HouseBill.CU_BillNum == "HB001" && p.PackType == PackageTypeConverter.GetCustomsPackageType(Core.Constants.PkgUnit.Bottle)) as Package).CW_PackQty);
			AssertContainsExactElementsInAnyOrder("The existing package are not changed", existingPackages, declaration.Packages);
			AssertEquals("The new package was absorbed", 6, declaration.Packages.Count);

			packlineNew.Delete();
			packline13.Delete();
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Aggregated packlines can be deleted", 5, declaration.Packages.Count);
		}

		public void TestShipmentSynchroniser_WhenECIWriteOffError_ChangePackageTypeInPackLines_Ok()
		{
			// Functional test to confirm that when declaration has ECI WriteOff error,
			// to correct it if we modify Package type in shipment pack lines, it does not lead to exception.

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			shipment.JS_HouseBill = "HB001";
			consol.Shipments.Add(shipment);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			#region container

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "HLMU1234561";
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = container1.JC_ContainerNum;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 10;
			packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline1.JL_JC = container1.PK;
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 10;
			packline2.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline2.JL_JC = container1.PK;
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 10;
			packline3.JL_F3_NKPackType = Core.Constants.PkgUnit.Piece;
			packline3.JL_JC = container1.PK;
			packline3.JL_MarksAndNumbers = "MarksAndNumbers3";

			#endregion

			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals("The 3 forwarding package lines should be rolled up into 2 declaration package rows - aggregated on container & pack type", 2, declaration.Packages.Count);
			AssertHasRowError("The last package should have an error.", declaration.Packages[1], "You can only have 1 Package Type per House Bill/Container combination for a write off declaration.");

			// Correct to remove the error.
			packline3.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			AssertEquals("The 3 forwarding package lines should be rolled up into 1 declaration package rows - aggregated on container & pack type", 1, declaration.Packages.Count);
		}

		protected override void TestResultsWhenBillIsCreatedLater(JobDeclaration declaration)
		{
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
		}

		protected override JobDeclaration GetDeclarationPackingRelevant()
		{
			return Factory.New<JobDeclaration>();
		}

		protected override JobDeclaration GetDeclarationPackingNotRelevant()
		{
			return null;
		}

		protected override void DecorateConsolToBeRelevant(ForwardingConsol consol)
		{
			//because declaration above is export
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
		}
	}

	public class PackingSynchronisationTest : Customs.Business.Testing.PackingSynchronisationTest
	{
		protected override Customs.Business.BaseJobDeclaration GetDeclarationPackingRelevant()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
