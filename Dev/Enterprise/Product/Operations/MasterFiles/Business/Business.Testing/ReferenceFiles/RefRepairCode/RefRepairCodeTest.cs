using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefRepairCode))]
	class RefRepairCodeTest : EnterpriseBusinessObjectTestCase
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
			var repairCode = Factory.New<RefRepairCode>();
			repairCode.RRC_Group = "CEDEX";
			repairCode.RRC_Code = "AA";
			repairCode.RRC_Description = "Repair Code for testing";
			repairCode.RRC_ServiceType = "RPR";

			return repairCode;
		}
	}
}
