using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCarrierServiceLevelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPL_CarrierServiceLevelDescription()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCarrierServiceLevel svcLvl1 = org.MiscServ.CarrierServiceLevels.AddNew();

			svcLvl1.PL_Code = "ABC";
			AssertNoErrors(svcLvl1.PL_CodeInfo);

			svcLvl1.PL_Code = "";
			AssertHasErrors(svcLvl1.PL_CodeInfo);

			svcLvl1.PL_Code = "ABC";

			OrgCarrierServiceLevel svcLvl2 = org.MiscServ.CarrierServiceLevels.AddNew();
			svcLvl2.PL_Code = "DEF";

			AssertNoErrors(svcLvl1.PL_CodeInfo);
			AssertNoErrors(svcLvl2.PL_CodeInfo);

			svcLvl2.PL_Code = "ABC";
			AssertNoErrors(svcLvl1.PL_CodeInfo);
			AssertHasErrors(svcLvl2.PL_CodeInfo);

			svcLvl1.RunPreSaveValidation();
			AssertHasErrors(svcLvl1.PL_CodeInfo);
			AssertHasErrors(svcLvl2.PL_CodeInfo);
		}

		public void TestPL_CarrierServiceLevelDescriptionDescription()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCarrierServiceLevel svcLvl1 = org.MiscServ.CarrierServiceLevels.AddNew();

			svcLvl1.PL_CarrierServiceLevelDescription = "Zubin";
			AssertNoErrors(svcLvl1.PL_CarrierServiceLevelDescriptionInfo);

			svcLvl1.PL_CarrierServiceLevelDescription = "";
			AssertHasErrors(svcLvl1.PL_CarrierServiceLevelDescriptionInfo);

			svcLvl1.PL_CarrierServiceLevelDescription = "Zubin";

			OrgCarrierServiceLevel svcLvl2 = org.MiscServ.CarrierServiceLevels.AddNew();
			svcLvl2.PL_CarrierServiceLevelDescription = "Rakhsh";

			AssertNoErrors(svcLvl1.PL_CarrierServiceLevelDescriptionInfo);
			AssertNoErrors(svcLvl2.PL_CarrierServiceLevelDescriptionInfo);

			svcLvl2.PL_CarrierServiceLevelDescription = "Zubin";
			AssertNoErrors(svcLvl1.PL_CarrierServiceLevelDescriptionInfo);
			AssertHasErrors(svcLvl2.PL_CarrierServiceLevelDescriptionInfo);

			svcLvl1.RunPreSaveValidation();
			AssertHasErrors(svcLvl1.PL_CarrierServiceLevelDescriptionInfo);
			AssertHasErrors(svcLvl2.PL_CarrierServiceLevelDescriptionInfo);
		}

		public void TestPL_ProductCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCarrierServiceLevel svcLvl1 = org.MiscServ.CarrierServiceLevels.AddNew();

			svcLvl1.PL_Code = "AAA";
			svcLvl1.PL_CarrierServiceLevelDescription = "Desc1";
			svcLvl1.PL_ProductCode = "PRODUCT1";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);

			svcLvl1.PL_ProductCode = "";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);

			svcLvl1.PL_ProductCode = "PRODUCT1";

			OrgCarrierServiceLevel svcLvl2 = org.MiscServ.CarrierServiceLevels.AddNew();
			svcLvl2.PL_Code = "BBB";
			svcLvl2.PL_CarrierServiceLevelDescription = "Desc2";
			svcLvl2.PL_ProductCode = "PRODUCT2";

			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);
			AssertNoErrors(svcLvl2.PL_ProductCodeInfo);

			svcLvl2.PL_ProductCode = "PRODUCT1";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);
			AssertHasErrors(svcLvl2.PL_ProductCodeInfo);

			svcLvl1.RunPreSaveValidation();
			AssertHasErrors(svcLvl1.PL_ProductCodeInfo);
			AssertHasErrors(svcLvl2.PL_ProductCodeInfo);
		}

		public void TestPL_ProductCode_CommaSeparated()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCarrierServiceLevel svcLvl1 = org.MiscServ.CarrierServiceLevels.AddNew();

			svcLvl1.PL_Code = "AAA";
			svcLvl1.PL_CarrierServiceLevelDescription = "Desc1";
			svcLvl1.PL_ProductCode = "PR1";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);

			svcLvl1.PL_ProductCode = "";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);

			svcLvl1.PL_ProductCode = "PR1,PR2";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);

			svcLvl1.PL_ProductCode = "PR1 , PR2";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);

			svcLvl1.PL_ProductCode = "    PR1,	PR2";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);

			svcLvl1.PL_ProductCode = ",,PR1";
			AssertHasError(svcLvl1.PL_ProductCodeInfo, "The Product Code must contain 1 value, or a series of values each separated by a comma.");

			svcLvl1.PL_ProductCode = ",";
			AssertHasError(svcLvl1.PL_ProductCodeInfo, "The Product Code must contain 1 value, or a series of values each separated by a comma.");

			svcLvl1.PL_ProductCode = "PR1,";
			AssertHasError(svcLvl1.PL_ProductCodeInfo, "The Product Code must contain 1 value, or a series of values each separated by a comma.");

			svcLvl1.PL_ProductCode = "PR1,PR2";

			OrgCarrierServiceLevel svcLvl2 = org.MiscServ.CarrierServiceLevels.AddNew();
			svcLvl2.PL_Code = "BBB";
			svcLvl2.PL_CarrierServiceLevelDescription = "Desc2";
			svcLvl2.PL_ProductCode = "PR3";

			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);
			AssertNoErrors(svcLvl2.PL_ProductCodeInfo);

			svcLvl2.PL_ProductCode = "PR2";
			AssertNoErrors(svcLvl1.PL_ProductCodeInfo);
			AssertHasError(svcLvl2.PL_ProductCodeInfo, "The Product Code has been duplicated and must be unique.");
		}

		public void TestPL_UniversalServiceLevel()
		{
			var org = Factory.New<OrgHeader>();

			var level1 = org.MiscServ.CarrierServiceLevels.AddNew();
			level1.PL_Code = "STD";

			level1.PL_CarrierServiceCode = "";
			AssertNoErrors(level1.PL_CarrierServiceCodeInfo);

			level1.PL_CarrierServiceCode = "USTD";
			AssertNoErrors(level1.PL_ProductCodeInfo);

			var level2 = org.MiscServ.CarrierServiceLevels.AddNew();
			level2.PL_Code = "EXP";

			level2.PL_CarrierServiceCode = "";
			AssertNoErrors(level2.PL_CarrierServiceCodeInfo);

			level2.PL_CarrierServiceCode = "USTD";
			AssertHasError(level2.PL_CarrierServiceCodeInfo, "The Carrier Service Code has been duplicated and must be unique.");

			level2.PL_CarrierServiceCode = "UEXP";
			AssertNoErrors(level2.PL_CarrierServiceCodeInfo);
		}

		public void TestPL_UniversalServiceLevel_CommaSeparated()
		{
			var org = Factory.New<OrgHeader>();

			var level1 = org.MiscServ.CarrierServiceLevels.AddNew();
			level1.PL_Code = "STD";

			level1.PL_CarrierServiceCode = "";
			AssertNoErrors(level1.PL_CarrierServiceCodeInfo);

			level1.PL_CarrierServiceCode = "A,B";
			AssertNoErrors(level1.PL_CarrierServiceCodeInfo);

			level1.PL_CarrierServiceCode = "A, B";
			AssertNoErrors(level1.PL_CarrierServiceCodeInfo);

			level1.PL_CarrierServiceCode = "A ,B";
			AssertNoErrors(level1.PL_CarrierServiceCodeInfo);

			level1.PL_CarrierServiceCode = "      A,	B";
			AssertNoErrors(level1.PL_CarrierServiceCodeInfo);

			level1.PL_CarrierServiceCode = "";
			AssertNoErrors(level1.PL_CarrierServiceCodeInfo);

			level1.PL_CarrierServiceCode = ",,B";
			AssertHasError(level1.PL_CarrierServiceCodeInfo, "The Carrier Service Code must contain 1 value, or a series of values each separated by a comma.");

			level1.PL_CarrierServiceCode = ",";
			AssertHasError(level1.PL_CarrierServiceCodeInfo, "The Carrier Service Code must contain 1 value, or a series of values each separated by a comma.");

			level1.PL_CarrierServiceCode = "B,";
			AssertHasError(level1.PL_CarrierServiceCodeInfo, "The Carrier Service Code must contain 1 value, or a series of values each separated by a comma.");

			level1.PL_CarrierServiceCode = "A,B";

			var level2 = org.MiscServ.CarrierServiceLevels.AddNew();
			level2.PL_Code = "EXP";

			level2.PL_CarrierServiceCode = "";
			AssertNoErrors(level2.PL_CarrierServiceCodeInfo);

			level2.PL_CarrierServiceCode = "B";
			AssertHasError(level2.PL_CarrierServiceCodeInfo, "The Carrier Service Code has been duplicated and must be unique.");

			level2.PL_CarrierServiceCode = "UEXP";
			AssertNoErrors(level2.PL_CarrierServiceCodeInfo);
		}
	}
}
