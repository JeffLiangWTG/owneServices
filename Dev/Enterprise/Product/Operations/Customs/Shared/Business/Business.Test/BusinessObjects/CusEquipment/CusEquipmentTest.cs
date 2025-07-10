using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEquipment))]
	sealed class CusEquipmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainersAndEquipmentsOnDeclaration_ListWhenCEQ_IdentificationNumberChanged()
		{
			var declaration = Factory.New<JobDeclarationEquipmentsForTesting>();
			declaration.ContainersRequiredReturns = true;
			declaration.EquipmentsRequiredReturns = false;

			var containers = declaration.CusContainers;
			var container1 = containers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = containers.AddNew();
			container2.CO_ContainerNumber = "C2";
			var equipments = declaration.Equipments;
			var equipment = equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "E1";

			equipment.CEQ_IdentificationNumber = "E2";
			AssertEquals("When ContainersRequired is true and EquipmentsRequired is false", "C1, C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			declaration.EquipmentsRequiredReturns = true;
			equipment.CEQ_IdentificationNumber = "E1";
			AssertEquals("When ContainersRequired and EquipmentsRequired are both true", "C1, C2, E1", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);
		}

		public void TestTypeDecider()
		{
			AssertType<CusEquipmentTypeDecider>(CusEquipment.TypeDecider);
		}

		public void TestDeleteRemovesReferencesAndRebuildsPackingGroups()
		{
			var testDec = Factory.New<JobDeclarationEquipmentsForTesting>();
			testDec.SupportEquipmentsReturns = true;
			var equipment = testDec.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "ABC";

			var bill1 = testDec.Bills.AddNew();
			bill1.CU_BillNum = "Bill123";

			var pack1 = testDec.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_PackType = "A";
			pack1.CW_HouseBill = bill1.CU_BillUniqueCode;

			var pack2 = testDec.Packages.AddNew();
			pack2.CW_PackQty = 3;
			pack2.CW_PackType = "B";
			pack2.CW_HouseBill = bill1.CU_BillUniqueCode;
			pack2.CW_ContainerNoOrEquipmentNo = "ABC";

			var equipmentGroup = testDec.PackingGroups.Cast<BasePackingGroup>().FirstOrDefault(x => x.CR_CEQ_Equipment == equipment.PK);
			AssertNotNull("Pre-Req - Equipment linked to packing group", equipmentGroup);

			equipment.Delete();

			AssertEquals("Packing groups consolidated", 1, testDec.PackingGroups.Count);
			AssertEquals("Still 2 Packages", 2, testDec.PackingGroups[0].Packages.Count);

			equipmentGroup = testDec.PackingGroups.Cast<BasePackingGroup>().FirstOrDefault(x => x.CR_CEQ_Equipment == equipment.PK);
			AssertNull("No Equipment linked to packing group", equipmentGroup);
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
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusEquipment>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var equipment = declaration.Equipments.AddNew();
				AssertEquals("From Declaration", "NZ", (equipment as ITypeDeciderContext).Country);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<CusEquipment>();

		sealed class JobDeclarationEquipmentsForTesting : JobDeclarationForTestingEquipments
		{
			public JobDeclarationEquipmentsForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool ContainersRequiredReturns { get; set; }
			public bool EquipmentsRequiredReturns { get; set; }
			public bool SupportEquipmentsReturns { get; set; }

			public override ZBool ContainersRequired => ContainersRequiredReturns;
			public override bool EquipmentsRequired => EquipmentsRequiredReturns;
			protected override bool SupportEquipmentsCore => SupportEquipmentsReturns;
		}
	}
}
