using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefRefrigerantType))]
	class RefRefrigerantTypeTest : EnterpriseBusinessObjectTestCase
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
			var refrigerantType = Factory.New<RefRefrigerantType>();
			refrigerantType.RRT_Code = "R-12";
			refrigerantType.RRT_Description = "Dichlorodifluoromethane (CFC-12)";

			return refrigerantType;
		}
	}
}
