using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefEquipmentGrade))]
	class RefEquipmentGradeTest : EnterpriseBusinessObjectTestCase
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
			var equipmentGrade = Factory.New<RefEquipmentGrade>();
			equipmentGrade.REG_Code = "AAA";
			equipmentGrade.REG_Description = "Equipment Grade for testing";
			return equipmentGrade;
		}
	}
}
