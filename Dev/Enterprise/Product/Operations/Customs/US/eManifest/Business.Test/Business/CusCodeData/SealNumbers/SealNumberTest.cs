using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(SealNumber))]
	sealed class SealNumberTest : Customs.Business.Testing.CusCodeDataTest<SealNumber>
	{
		public void TestSetDefaultValues()
		{
			var number = Factory.New<SealNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.SealNumber, number.CY_Type);
			AssertEquals(CusCodeDataTypeList.Codes.SealNumber, number.CY_Code);
		}

		public void TestParent()
		{
			var equipment = Factory.New<Equipment>();
			var number = Factory.New<SealNumber>();
			number.CY_ParentID = equipment.PK;
			number.CY_ParentTableCode = equipment.TablePrefix;
			AssertEquals(equipment, number.Parent);
			number = equipment.SealNumbers.AddNew();
			AssertEquals(equipment.TablePrefix, number.CY_ParentTableCode);
			AssertEquals(equipment.PK, number.CY_ParentID);
			AssertEquals(equipment, number.Parent);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var trip = factory.New<Trip>();
			var equipment = trip.Equipment.AddNew();
			return equipment.SealNumbers.AddNew();
		}
	}
}
