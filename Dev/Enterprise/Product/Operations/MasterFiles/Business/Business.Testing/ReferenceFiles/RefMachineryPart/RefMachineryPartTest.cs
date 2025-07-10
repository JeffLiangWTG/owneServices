using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefMachineryPart))]
	class RefMachineryPartTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var machineryMake = Factory.New<RefMachineryMake>();
			machineryMake.RMM_Code = "DKN1";
			machineryMake.RMM_Description = "DAIKIN1";

			var componentCode = Factory.New<RefMRComponentCode>();
			componentCode.RCC_Group = "CEDEX";
			componentCode.RCC_Code = "AA1";
			componentCode.RCC_Description = "Component Code for testing1";
			componentCode.RCC_Machinery = true;
			componentCode.RCC_TankCleaning = true;

			var machineryPart = Factory.New<RefMachineryPart>();
			machineryPart.RMP_PartNumber = "PN001";
			machineryPart.RMP_Description = "Description Test";
			machineryPart.RMP_RMM_MachineryMake = machineryMake.PK;
			machineryPart.RMP_RCC_ComponentCode = componentCode.PK;

			return machineryPart;
		}
	}
}
