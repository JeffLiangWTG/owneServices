using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists.Testing
{
	class CargoTypeListTest : CodeDescriptionEnumListTestCase
	{
		public void TestGetDefaultCargoType()
		{
			CargoTypeList list = new CargoTypeList();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("No Containers", CargoTypeList.Codes.Fak, CargoTypeList.GetDefaultCargoType(declaration.CusContainers));

			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = ZString.Empty;
			AssertEquals("FAK if no mode set on Container", CargoTypeList.Codes.Fak, CargoTypeList.GetDefaultCargoType(declaration.CusContainers));

			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.LCL;
			AssertEquals("One LCL Container", CargoTypeList.Codes.Lcl, CargoTypeList.GetDefaultCargoType(declaration.CusContainers));

			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("One Empty Container", CargoTypeList.Codes.Empty, CargoTypeList.GetDefaultCargoType(declaration.CusContainers));

			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			AssertEquals("Two Empty Containers", CargoTypeList.Codes.Empty, CargoTypeList.GetDefaultCargoType(declaration.CusContainers));

			container2.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("Second Container now FCL", CargoTypeList.Codes.Fak, CargoTypeList.GetDefaultCargoType(declaration.CusContainers));

			container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("Both Containers now FCL", CargoTypeList.Codes.Fcl, CargoTypeList.GetDefaultCargoType(declaration.CusContainers));

			declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			AssertEquals("3 Containers now FCL", CargoTypeList.Codes.Fcl, CargoTypeList.GetDefaultCargoType(declaration.CusContainers));
		}

		protected override CodeDescriptionPairList GetNewList()
		{
			return new CargoTypeList();
		}
	}
}
