using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEquipmentValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEQ_IdentificationNumber()
		{
			CombineAssertions("When EquipmentsRequired is true", () =>
			{
				var declaration = Factory.New<JobDeclarationForTestingEquipments>();
				var equipment = declaration.Equipments.AddNew();
				AssertEquals("Precondition", true, equipment.Declaration.EquipmentsRequired);
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(equipment.CEQ_IdentificationNumberInfo);
			});

			CombineAssertions("When EquipmentsRequired is false", () =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var equipment = declaration.Equipments.AddNew();
				AssertEquals("Precondition", false, equipment.Declaration.EquipmentsRequired);
				equipment.CEQ_IdentificationNumber = "E1";
				equipment.CEQ_IdentificationNumber = ZString.Empty;
				AssertNoMessageErrorContaining(equipment.CEQ_IdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}
}
