using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusBondDetailValidationTest : TestCaseWithFactory
	{
		public void TestValidateUS_BondType()
		{
			bondData.PW_BondType = "~";
			AssertHasError(bondData.PW_BondTypeInfo, "Enter a valid selection.");
			bondData.PW_BondType = bondData.Lookups.BondTypeList[0].Code;
			AssertNoError(bondData.PW_BondTypeInfo, "Enter a valid selection.");
		}

		public void TestValidateUS_UR_BondFiledPort()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			newFactory.Save();

			bondData.PW_BondFiledPort = "2222";
			AssertHasError(bondData.PW_BondFiledPortInfo, "Enter a valid selection.");
			bondData.PW_BondFiledPort = "3901";
			AssertNoError(bondData.PW_BondFiledPortInfo, "Enter a valid selection.");
		}

		public void TestCheckPW_BondExpiryDate()
		{
			bondData.PW_BondEffectiveDate = ZDateTime.Today;
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(-2);
			AssertHasError(bondData.PW_BondExpiryDateInfo, CusBondDetailValidation.ExpiryDateLessThenEffectiveDate);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(4);
			AssertNoError(bondData.PW_BondExpiryDateInfo, CusBondDetailValidation.ExpiryDateLessThenEffectiveDate);
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddYears(-12);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddYears(-11);
			AssertNoErrors(bondData.PW_BondExpiryDateInfo);
		}

		public void TestCheckPW_BondEffectiveDate()
		{
			AssertNoErrors(bondData.PW_BondEffectiveDateInfo);
			bondData.Validation.ValidatePW_BondEffectiveDate();
			AssertHasErrors(bondData.PW_BondEffectiveDateInfo);
			bondData.PW_BondExpiryDate = ZDateTime.Today;
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(5);
			AssertHasError(bondData.PW_BondExpiryDateInfo, CusBondDetailValidation.ExpiryDateLessThenEffectiveDate);
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-5);
			AssertNoError(bondData.PW_BondExpiryDateInfo, CusBondDetailValidation.ExpiryDateLessThenEffectiveDate);
			bondData.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bondData.PW_BondEffectiveDate = ZDateTime.Empty;
			AssertNoErrors(bondData.PW_BondEffectiveDateInfo);
			bondData.PW_BondEffectiveDate = ZDateTime.Today.AddYears(-12);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddYears(-11);
			AssertNoErrors(bondData.PW_BondEffectiveDateInfo);
		}

		public void TestCheckPW_BondAmount()
		{
			bondData.PW_BondAmount = -23.1m;
			AssertHasErrors(bondData.PW_BondAmountInfo);
			bondData.PW_BondAmount = 23.1m;
			AssertNoErrors(bondData.PW_BondAmountInfo);
		}

		public void TestCheckPW_ActivityCode()
		{
			bondData.PW_ActivityCode = ZString.Empty;
			AssertHasError(bondData.PW_ActivityCodeInfo, CusBondDetailValidation.ActivityCodeShouldBeEntered);
			bondData.PW_ActivityCode = "521";
			AssertNoError(bondData.PW_ActivityCodeInfo, CusBondDetailValidation.ActivityCodeShouldBeEntered);
		}

		OrgHeader organisation;
		CusBondDetail bondData;
		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			bondData = Factory.New<CusBondDetail>();
			bondData.Parent = organisation;
		}
	}
}
