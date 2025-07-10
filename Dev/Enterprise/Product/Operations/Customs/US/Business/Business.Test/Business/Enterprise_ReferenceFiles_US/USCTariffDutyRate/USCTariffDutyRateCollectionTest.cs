using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffDutyRateCollection))]
	sealed class USCTariffDutyRateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetRateForSPI()
		{
			USCTariffDutyRateCollection collection = (USCTariffDutyRateCollection)GetCollectionToTest();

			USCTariffDutyRate dutyRate1 = collection.AddNew();
			dutyRate1.UD_ISOCountryCode = "AU";

			USCTariffDutyRate dutyRate2 = collection.AddNew();
			dutyRate2.UD_ISOCountryCode = "MX";

			USCTariffDutyRate dutyRate3 = collection.AddNew();
			dutyRate3.UD_ISOCountryCode = "A ";

			AssertEquals(dutyRate1, collection.GetRateForSPI("AU"));
			AssertNull(collection.GetRateForSPI(" "));
		}

		public void TestIsQuantityRequired()
		{
			USCTariffDutyRateCollection collection = (USCTariffDutyRateCollection)GetCollectionToTest();
			USCTariffDutyRate dutyRate = collection.AddNew();
			AssertEquals(false, collection.RequiresFirstQuantity);
			AssertEquals(false, collection.RequiresSecondQuantity);
			AssertEquals(false, collection.RequiresThirdQuantity);

			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;
			AssertEquals(true, collection.RequiresFirstQuantity);
			AssertEquals(false, collection.RequiresSecondQuantity);
			AssertEquals(false, collection.RequiresThirdQuantity);

			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremSecondQuantity;
			AssertEquals(false, collection.RequiresFirstQuantity);
			AssertEquals(true, collection.RequiresSecondQuantity);
			AssertEquals(false, collection.RequiresThirdQuantity);

			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			AssertEquals(false, collection.RequiresFirstQuantity);
			AssertEquals(false, collection.RequiresSecondQuantity);
			AssertEquals(true, collection.RequiresThirdQuantity);
		}

		public void TestGetRateForTaxFeeClassCode()
		{
			USCTariffDutyRateCollection collection = (USCTariffDutyRateCollection)GetCollectionToTest();
			USCTariffDutyRate dutyRate = collection.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Tobacco;
			AssertNull(collection.GetRateForTaxFeeClassCode(Core.Constants.USCustoms.FeeCodes.Wines));
			AssertEquals(dutyRate, collection.GetRateForTaxFeeClassCode(Core.Constants.USCustoms.FeeCodes.Tobacco));
		}

		public void TestGetRateForElement()
		{
			USCTariffDutyRateCollection collection = (USCTariffDutyRateCollection)GetCollectionToTest();
			USCTariffDutyRate dutyRate = collection.AddNew();
			dutyRate.UD_DutyElement = "Z";
			AssertNull(collection.GetRateForElement("A"));
			AssertEquals(dutyRate, collection.GetRateForElement("Z"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			return new USCTariffDutyRateCollection(tariff, Factory);
		}
	}
}
