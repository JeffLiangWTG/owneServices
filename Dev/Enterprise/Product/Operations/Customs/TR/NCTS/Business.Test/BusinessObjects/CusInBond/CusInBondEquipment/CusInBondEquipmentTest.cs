using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(CusInBondEquipment))]
	class CusInBondEquipmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadOrCreateEquipment()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.Trailer1 = "AB1234CD";
			header.Trailer2 = "AB5678CD";

			Factory.Save();

			AssertEquals(header.Equipment1.BJ_RegistrationNumber, "AB1234CD");
			AssertEquals(header.Equipment2.BJ_RegistrationNumber, "AB5678CD");
		}
	}
}
