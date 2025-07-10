using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefMRComponentCode))]
	class RefMRComponentCodeTest : EnterpriseBusinessObjectTestCase
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
			var componentCode = Factory.New<RefMRComponentCode>();
			componentCode.RCC_Group = "CEDEX";
			componentCode.RCC_Code = "AA";
			componentCode.RCC_Description = "Component Code for testing";
			componentCode.RCC_Machinery = true;
			componentCode.RCC_TankCleaning = true;
			return componentCode;
		}
	}
}
