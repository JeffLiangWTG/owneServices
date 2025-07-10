using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TariffToChange))]
	sealed class TariffToChangeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFormattedProperties()
		{
			USTariffBulkChange changer = new USTariffBulkChange(Factory);
			TariffToChange tariffToChange = new TariffToChange(changer);
			tariffToChange.OldTariffNum = "4823908000";
			tariffToChange.NewTariffNum = "4823908011";

			AssertEquals("4823.90.8000", tariffToChange.FormattedOldTariff);
			AssertEquals("4823.90.8011", tariffToChange.FormattedNewTariff);
			AssertEquals("4823908000", tariffToChange.OldTariffNum);
			AssertEquals("4823908011", tariffToChange.NewTariffNum);
		}

		public void TestValidateOldTariffNum()
		{
			USTariffBulkChange changer = new USTariffBulkChange(Factory);
			TariffToChange tariffToChange = changer.Tariffs.AddNew();
			tariffToChange.ValidateOldTariffNum();
			AssertHasError(tariffToChange.OldTariffNumInfo, TariffToChange.TariffIsEmpty);

			USCTariff trf = Factory.New<USCTariff>();
			trf.UE_Tariff = "4823908003";
			trf.UE_DateFrom = ZDateTime.Today.AddDays(-2);
			trf.UE_DateTo = ZDateTime.Today.AddDays(2);
			Factory.Save();

			tariffToChange.OldTariffNum = trf.UE_Tariff;
			AssertNoError(tariffToChange.OldTariffNumInfo, TariffToChange.TariffIsEmpty);
			AssertNoWarning(tariffToChange.OldTariffNumInfo, ListValidation.InvalidCodeMessage);

			tariffToChange.OldTariffNum = "48238";
			AssertHasError(tariffToChange.OldTariffNumInfo, TariffValidator.InvalidLength);

			tariffToChange.OldTariffNum = "99265";
			AssertHasError(tariffToChange.OldTariffNumInfo, TariffValidator.InvalidLength);

			tariffToChange.OldTariffNum = "99265123";
			AssertNoError(tariffToChange.OldTariffNumInfo, TariffValidator.InvalidLength);

			tariffToChange.OldTariffNum = "0000000001";
			AssertHasWarning(tariffToChange.OldTariffNumInfo, ListValidation.InvalidCodeMessage);

			tariffToChange.OldTariffNum = "4823908000";
			AssertNoError(tariffToChange.OldTariffNumInfo, TariffValidator.InvalidLength);

			TariffToChange tariffToChange2 = changer.Tariffs.AddNew();
			tariffToChange2.OldTariffNum = "4823908000";
			AssertHasError(tariffToChange2.OldTariffNumInfo, TariffToChange.TariffExists);

			tariffToChange2.OldTariffNum = "4823908004";
			AssertNoError(tariffToChange2.OldTariffNumInfo, TariffToChange.TariffExists);
		}

		public void TestValidateNewTariffNum()
		{
			USTariffBulkChange changer = new USTariffBulkChange(Factory);
			TariffToChange tariffToChange = new TariffToChange(changer);
			tariffToChange.ValidateNewTariffNum();
			AssertHasError(tariffToChange.NewTariffNumInfo, TariffToChange.TariffIsEmpty);

			tariffToChange.NewTariffNum = "4823908003";
			AssertNoError(tariffToChange.NewTariffNumInfo, TariffToChange.TariffIsEmpty);

			USCTariff trf = Factory.New<USCTariff>();
			trf.UE_Tariff = "4823908003";
			trf.UE_DateFrom = ZDateTime.Today.AddDays(-2);
			trf.UE_DateTo = ZDateTime.Today.AddDays(2);
			Factory.Save();

			tariffToChange.NewTariffNum = trf.UE_Tariff;
			AssertNoWarning(tariffToChange.NewTariffNumInfo, ListValidation.InvalidCodeMessage);

			tariffToChange.NewTariffNum = "48238";
			AssertHasError(tariffToChange.NewTariffNumInfo, TariffValidator.InvalidLength);

			tariffToChange.NewTariffNum = "4823908000";
			AssertNoError(tariffToChange.NewTariffNumInfo, TariffValidator.InvalidLength);

			tariffToChange.NewTariffNum = "0000000001";
			AssertHasWarning(tariffToChange.NewTariffNumInfo, ListValidation.InvalidCodeMessage);

			tariffToChange.NewTariffNum = "99265";
			AssertHasError(tariffToChange.NewTariffNumInfo, TariffValidator.InvalidLength);

			tariffToChange.NewTariffNum = "99265123";
			AssertNoError(tariffToChange.NewTariffNumInfo, TariffValidator.InvalidLength);

			tariffToChange.OldTariffNum = "99265123";
			tariffToChange.ValidateNewTariffNum();
			AssertHasError(tariffToChange.NewTariffNumInfo, TariffToChange.UpdateToTheSameTariffNotAllowed);

			tariffToChange.NewTariffNum = "99265124";
			AssertNoError(tariffToChange.NewTariffNumInfo, TariffToChange.UpdateToTheSameTariffNotAllowed);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			USTariffBulkChange changer = new USTariffBulkChange(Factory);
			return new TariffToChange(changer);
		}
	}
}
