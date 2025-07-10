using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffDateRestriction))]
	sealed class USCTariffDateRestrictionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsPassedDateWithinRestrictedDates()
		{
			USCTariffDateRestriction restriction1 = Factory.New<USCTariffDateRestriction>();
			restriction1.UF_EntryDateRestrictionCode = "1";
			restriction1.UF_EntryDateRestrictionFrom = 601;
			restriction1.UF_EntryDateRestrictionTo = 831;

			USCTariffDateRestriction restriction2 = Factory.New<USCTariffDateRestriction>();
			restriction2.UF_EntryDateRestrictionCode = "2";
			restriction2.UF_EntryDateRestrictionFrom = 1101;
			restriction2.UF_EntryDateRestrictionTo = 115;

			AssertEquals("ZDate(2008, 6, 1) for restriction1", true, restriction1.IsPassedDateWithinRestrictedDates(new ZDate(2008, 6, 1)));
			AssertEquals("ZDate(2008, 8, 31) for restriction1", true, restriction1.IsPassedDateWithinRestrictedDates(new ZDate(2008, 8, 31)));
			AssertEquals("ZDate(2008, 5, 31) for restriction1", false, restriction1.IsPassedDateWithinRestrictedDates(new ZDate(2008, 5, 31)));

			AssertEquals("ZDate(2008, 11, 1) for restriction2", true, restriction2.IsPassedDateWithinRestrictedDates(new ZDate(2008, 11, 1)));
			AssertEquals("ZDate(2009, 1, 15) for restriction2", true, restriction2.IsPassedDateWithinRestrictedDates(new ZDate(2009, 1, 15)));
			AssertEquals("ZDate(2007, 11, 1) for restriction2", true, restriction2.IsPassedDateWithinRestrictedDates(new ZDate(2007, 11, 1)));
			AssertEquals("ZDate(2008, 1, 15) for restriction2", true, restriction2.IsPassedDateWithinRestrictedDates(new ZDate(2008, 1, 15)));

			AssertEquals("ZDate(2008, 12, 1) for restriction2", true, restriction2.IsPassedDateWithinRestrictedDates(new ZDate(2008, 12, 1)));
			AssertEquals("ZDate(2007, 12, 15) for restriction2", true, restriction2.IsPassedDateWithinRestrictedDates(new ZDate(2007, 12, 15)));

			AssertEquals("ZDate(2008, 10, 31) for restriction2", false, restriction2.IsPassedDateWithinRestrictedDates(new ZDate(2008, 10, 31)));
			AssertEquals("ZDate(2008, 1, 16) for restriction2", false, restriction2.IsPassedDateWithinRestrictedDates(new ZDate(2008, 1, 16)));
		}

		public void TestAfterSavingEmptyRecordRowIsNotSavedButNotDeleted()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today;
			USCTariffDateRestriction tariffDateRestriction = Factory.New<USCTariffDateRestriction>();
			Factory.Save();
			AssertEquals(false, tariffDateRestriction.IsDeleted);
			AssertEquals(false, tariffDateRestriction.IsInDatabase);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AssertEquals(null, factory2.Load<USCTariffDateRestriction>(tariffDateRestriction.PK));

			tariffDateRestriction.UF_EntryDateRestrictionCode = "X";
			tariffDateRestriction.UF_EntryDateRestrictionFrom = 101;
			tariffDateRestriction.UF_EntryDateRestrictionTo = 1101;

			Factory.Save();
			AssertEquals(false, tariffDateRestriction.IsDeleted);
			AssertEquals(true, tariffDateRestriction.IsInDatabase);

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			AssertNotNull(factory3.Load<USCTariffDateRestriction>(tariffDateRestriction.PK));

			tariffDateRestriction.UF_EntryDateRestrictionCode = "";
			Factory.Save();
			AssertEquals(true, tariffDateRestriction.IsDeleted);
			AssertEquals(false, tariffDateRestriction.IsInDatabase);

			BusinessObjectFactory factory4 = new BusinessObjectFactory();
			AssertNull(factory4.Load<USCTariffDateRestriction>(tariffDateRestriction.PK));
		}

		public void TestEditedEmptyRecordIsDeleted()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today;
			USCTariffDateRestriction tariffDateRestriction = Factory.New<USCTariffDateRestriction>();
			tariffDateRestriction.UF_EntryDateRestrictionCode = "X";
			tariffDateRestriction.UF_EntryDateRestrictionFrom = 101;
			tariffDateRestriction.UF_EntryDateRestrictionTo = 1101;

			Factory.Save();
			AssertEquals(false, tariffDateRestriction.IsDeleted);
			AssertEquals(true, tariffDateRestriction.IsInDatabase);
			tariffDateRestriction.UF_EntryDateRestrictionTo = 0;
			Factory.Save();
			AssertEquals(true, tariffDateRestriction.IsDeleted);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AssertEquals(null, factory2.Load<USCTariffDateRestriction>(tariffDateRestriction.PK));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			USCTariffDateRestriction result = factory.New<USCTariffDateRestriction>();
			result.UF_EntryDateRestrictionCode = "X";
			result.UF_EntryDateRestrictionFrom = 101;
			result.UF_EntryDateRestrictionTo = 1101;

			return result;
		}
	}
}
