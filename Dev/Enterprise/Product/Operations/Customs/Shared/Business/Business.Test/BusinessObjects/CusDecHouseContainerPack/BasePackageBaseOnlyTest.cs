using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BasePackageBaseOnlyTest : TestCaseWithFactory
	{
		public void TestContainerNumberForEquipment()
		{
			var declaration = Factory.New<JobDeclarationForTestingEquipments>();
			declaration.DisableDefaultPackingInformation = true;
			var bill = declaration.Bills.AddNew();
			var equipment = Factory.New<CusEquipment>();
			equipment.CEQ_JE_Declaration = declaration.PK;
			equipment.CEQ_IdentificationNumber = "E1";
			var packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = bill.PK;
			var package = packingGroup.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = "E1";
			AssertEquals("getter", "E1", package.CW_ContainerNoOrEquipmentNo);
			AssertSame("linked to equipment", equipment, package.PackingGroup.Equipment);
		}

		public void TestTypeOfBillsOnDeclaration_List()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var package = declaration.Packages.AddNew();
			AssertType<BillCollection<Bill, BaseJobDeclaration>>(package.BillsOnDeclaration_List);
		}

		public void TestCW_ContainerNoOrEquipmentNo_List()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(BasePackage), nameof(BasePackage.CW_ContainerNoOrEquipmentNo), false, attr => attr.ListDataSourceMember == nameof(BasePackage.ContainersAndEquipmentsOnDeclaration_List));
		}

		public void TestContainersAndEquipmentsOnDeclaration_List()
		{
			var declaration = Factory.New<JobDeclarationForTestingEquipments>();
			declaration.DisableDefaultPackingInformation = true;
			var bill = declaration.Bills.AddNew();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "D1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "A1";
			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "C1";
			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "B1";
			var packingGroup = declaration.PackingGroups.AddNew();
			var package = packingGroup.Packages.AddNew();
			packingGroup.CR_CU_HouseBill = bill.PK;
			var list = package.ContainersAndEquipmentsOnDeclaration_List;
			AssertEquals(4, list.Count);
			AssertBusinessObjectElement((BusinessObjectElement)list[0], container2, "A1", "A1");
			AssertBusinessObjectElement((BusinessObjectElement)list[1], equipment2, "B1", "Equipment:B1");
			AssertBusinessObjectElement((BusinessObjectElement)list[2], equipment1, "C1", "Equipment:C1");
			AssertBusinessObjectElement((BusinessObjectElement)list[3], container1, "D1", "D1");
		}

		void AssertBusinessObjectElement(BusinessObjectElement element, BusinessObject expectedBizObj, string expectedCode, string expectedDescription)
		{
			AssertEquals("BizObject", expectedBizObj, element.BizObject);
			AssertEquals("Code", expectedCode, element.Code);
			AssertEquals("Description", expectedDescription, element.Description);
		}

		public void TestIsSavedByFactory()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var package = declaration.Packages.AddNew();

			CombineAssertions(() =>
			{
				Assert("can be saved properly", package.IsSavedByFactory);
				declaration.MakeNonPersistent();
				Assert("should stop being saved if declaration not persistent", !package.IsSavedByFactory);
				package.Delete();
				Assert("can be saved properly for delete", package.IsSavedByFactory);
			});
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<BasePackage>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var package = declaration.Packages.AddNew();
				AssertEquals("From Declaration", "NZ", (package as ITypeDeciderContext).Country);
			});
		}
	}
}
