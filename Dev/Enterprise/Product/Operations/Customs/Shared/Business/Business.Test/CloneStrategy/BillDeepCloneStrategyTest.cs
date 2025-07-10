using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BillDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneForTemplateCopy()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "XXXX";
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_GUIPresentationRecord = true;

			BaseCusContainer packingContainer = declaration.CusContainers.AddNew();
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = packingContainer.PK;
			packingGroup.Packages.AddNew();

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(0, clonedDeclaration.Bills.Count);
			AssertEquals(0, clonedDeclaration.CusContainers.Count);
			AssertEquals(0, clonedDeclaration.PackingGroups.Count);
		}

		public void TestCloneForTemplateCopyWithoutShipment()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "XXXX";
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_GUIPresentationRecord = true;

			BaseCusContainer packingContainer = declaration.CusContainers.AddNew();
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = packingContainer.PK;
			packingGroup.Packages.AddNew();

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(0, clonedDeclaration.Bills.Count);
			AssertEquals(0, clonedDeclaration.CusContainers.Count);
			AssertEquals(0, clonedDeclaration.PackingGroups.Count);
		}

		public void TestCloneForCountryToCountry()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "XXXX";
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_GUIPresentationRecord = true;

			BaseCusContainer packingContainer = declaration.CusContainers.AddNew();
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = packingContainer.PK;
			packingGroup.Packages.AddNew();

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy).Clone();
			AssertEquals(1, clonedDeclaration.Bills.Count);

			Bill clonedBill = clonedDeclaration.Bills[0];
			AssertEquals("XXXX", clonedBill.CU_BillNum);
			AssertEquals(BillTypeList.Codes.HouseBill, clonedBill.CU_BillType);
			AssertEquals(true, clonedBill.CU_GUIPresentationRecord);

			AssertEquals(1, clonedBill.PackingGroups.Count);
			AssertEquals(1, clonedBill.PackingGroups[0].Packages.Count);
			AssertEquals(1, clonedDeclaration.CusContainers.Count);
			AssertEquals(clonedDeclaration.CusContainers[0].PK, clonedBill.PackingGroups[0].CR_CO_Container);
		}

		public void TestRemoveUnlinkedPackingGroupWithCusContainerDuringDeepClone()
		{
			var shipment = Factory.New<CommonShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "XXXX";
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_GUIPresentationRecord = true;

			var packingContainer = declaration.CusContainers.AddNew();
			var packingGroup1 = houseBill.PackingGroups.AddNew();
			packingGroup1.CR_CO_Container = packingContainer.PK;
			packingGroup1.Packages.AddNew();
			var packingGroup2 = houseBill.PackingGroups.AddNew();
			packingGroup2.Packages.AddNew();

			var clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopyWithinShipment).Clone();
			var clonedBill = clonedDeclaration.Bills[0];

			AssertEquals(1, clonedBill.PackingGroups.Count);
			AssertEquals(1, clonedBill.PackingGroups[0].Packages.Count);
			AssertEquals(1, clonedDeclaration.CusContainers.Count);
			AssertEquals(clonedDeclaration.CusContainers[0].PK, clonedBill.PackingGroups[0].CR_CO_Container);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopyWithinShipment).Clone();
			clonedBill = clonedDeclaration.Bills[0];
			AssertEquals(2, clonedBill.PackingGroups.Count);
			AssertEquals(1, clonedBill.PackingGroups[0].Packages.Count);
			AssertEquals(1, clonedBill.PackingGroups[1].Packages.Count);
			AssertEquals(1, clonedDeclaration.CusContainers.Count);
			AssertEquals(clonedDeclaration.CusContainers[0].PK, clonedBill.PackingGroups[0].CR_CO_Container);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			packingGroup1.CR_CO_Container = ZGuid.Empty;
			clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopyWithinShipment).Clone();
			clonedBill = clonedDeclaration.Bills[0];
			AssertEquals(2, clonedBill.PackingGroups.Count);
			AssertEquals(1, clonedBill.PackingGroups[0].Packages.Count);
			AssertEquals(1, clonedBill.PackingGroups[1].Packages.Count);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopyWithinShipment).Clone();
			clonedBill = clonedDeclaration.Bills[0];
			AssertEquals(0, clonedBill.PackingGroups.Count);

			declaration.ShouldCloneContainersEvenNotLinked = true;
			packingGroup1.CR_CO_Container = ZGuid.Empty;
			clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopyWithinShipment).Clone();
			clonedBill = clonedDeclaration.Bills[0];
			AssertEquals(2, clonedBill.PackingGroups.Count);
			AssertEquals(1, clonedBill.PackingGroups[0].Packages.Count);
			AssertEquals(1, clonedBill.PackingGroups[1].Packages.Count);
		}
	}
}
