using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PackingGroupDeepCopyStrategyTest : TestCaseWithFactory
	{
		public void TestClone()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			packingGroup.CR_CargoStatus = "AAA";
			packingGroup.CR_HouseContainerNumber = 32;
			BaseCusContainer packingContainer = declaration.CusContainers.AddNew();
			packingContainer.CO_ContainerNumber = "OLCU1231231";
			packingGroup.CR_CO_Container = packingContainer.PK;
			BasePackage package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = "TT";
			package.CW_ShippingSymbol = "symbol";
			package.CW_MarksAndNos = "marks and numbers";
			Factory.Save();

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			AssertEquals(0, clonedDeclaration.Bills.Count);

			clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopy).Clone();
			AssertEquals(1, clonedDeclaration.Bills.Count);

			Bill clonedBill = clonedDeclaration.Bills[0];
			BasePackingGroup clonedPackingGroup = clonedBill.PackingGroups[0];

			AssertEquals("clonedPackingGroup.Container.CO_ContainerNumber", "OLCU1231231", clonedPackingGroup.Container.CO_ContainerNumber);
			AssertEquals("clonedPackingGroup.CR_CargoStatus", ZString.Empty, clonedPackingGroup.CR_CargoStatus);
			AssertEquals("clonedPackingGroup.CR_HouseContainerNumber", (short)32, clonedPackingGroup.CR_HouseContainerNumber);

			AssertEquals(1, clonedPackingGroup.Packages.Count);
			BasePackage clonedPackage = clonedPackingGroup.Packages[0];

			AssertEquals("clonedPackage.CW_PackQty", 10, clonedPackage.CW_PackQty);
			AssertEquals("clonedPackage.CW_PackType", "TT", package.CW_PackType);
			AssertEquals("clonedPackage.CW_ShippingSymbol", "symbol", package.CW_ShippingSymbol);
			AssertEquals("clonedPackage.CW_MarksAndNos", "marks and numbers", package.CW_MarksAndNos);
		}
	}
}
