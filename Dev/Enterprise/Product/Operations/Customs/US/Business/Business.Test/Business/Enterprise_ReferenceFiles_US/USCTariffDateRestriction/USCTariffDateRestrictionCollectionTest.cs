using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffDateRestrictionCollection))]
	sealed class USCTariffDateRestrictionCollectionTest : ActiveBusinessObjectCollectionTestCase<USCTariffDateRestrictionCollection>
	{
		public void TestAllRestrictionDateDescriptions()
		{
			Tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("1", 301, 630);
			Tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("2", 1201, 131);

			AssertEquals("Mar-01 to Jun-30, Dec-01 to Jan-31", Tariff.TariffDateRestrictions.GetAllRestrictionDateDescriptions(2008));
		}

		public void TestGetRestrictionDateForAParticularDate()
		{
			USCTariffDateRestriction restriction1 = Tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("1", 301, 630);
			USCTariffDateRestriction restriction2 = Tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("2", 1201, 131);

			AssertEquals(restriction1, Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2008, 3, 1)));
			AssertEquals(restriction1, Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2008, 6, 30)));
			AssertEquals(restriction2, Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2008, 12, 1)));
			AssertEquals(restriction2, Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2008, 1, 31)));
			AssertEquals(restriction2, Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2009, 1, 31)));

			AssertEquals(restriction1, Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2007, 3, 1)));
			AssertEquals(restriction1, Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2007, 6, 30)));
			AssertNull(Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2008, 2, 29)));
			AssertNull(Tariff.TariffDateRestrictions.GetRestrictionFor(new ZDateTime(2008, 7, 1)));
		}

		public void TestGetRestrictionDate()
		{
			USCTariffDateRestriction restriction = Tariff.TariffDateRestrictions.GetRestrictionDateFor("2", 11, 22);
			AssertNull(restriction);

			USCTariffDateRestriction restriction1 = Tariff.TariffDateRestrictions.AddNew();
			restriction1.UF_EntryDateRestrictionCode = "1";
			restriction1.UF_EntryDateRestrictionFrom = 11;
			restriction1.UF_EntryDateRestrictionTo = 22;

			USCTariffDateRestriction restriction2 = Tariff.TariffDateRestrictions.AddNew();
			restriction2.UF_EntryDateRestrictionCode = "2";
			restriction2.UF_EntryDateRestrictionFrom = 11;
			restriction2.UF_EntryDateRestrictionTo = 22;

			restriction = Tariff.TariffDateRestrictions.GetRestrictionDateFor("2", 11, 22);
			AssertNotNull(restriction);
			AssertEquals(restriction2, restriction);
		}

		public void TestCreateHavingRestrictionDates()
		{
			USCTariffDateRestriction restriction1 = Tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("1", 11, 22);
			AssertEquals("one element created", 1, Tariff.TariffDateRestrictions.Count);
			AssertEquals("1", restriction1.UF_EntryDateRestrictionCode);
			AssertEquals((short)11, restriction1.UF_EntryDateRestrictionFrom);
			AssertEquals((short)22, restriction1.UF_EntryDateRestrictionTo);

			USCTariffDateRestriction restriction2 = Tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("1", 11, 22);
			AssertEquals("still the first one", restriction1, restriction2);

			USCTariffDateRestriction restriction3 = Tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("2", 11, 22);
			AssertEquals("another element should have been created", 2, Tariff.TariffDateRestrictions.Count);
			AssertNotEquals(restriction2, restriction3);
			AssertEquals("2", restriction3.UF_EntryDateRestrictionCode);
			AssertEquals((short)11, restriction3.UF_EntryDateRestrictionFrom);
			AssertEquals((short)22, restriction3.UF_EntryDateRestrictionTo);
		}

		protected override USCTariffDateRestrictionCollection GetCollectionToTest()
		{
			return new USCTariffDateRestrictionCollection(Tariff);
		}

		USCTariff Tariff
		{
			get { return fTariff ?? (fTariff = Factory.New<USCTariff>()); }
		}
		USCTariff fTariff;
	}
}
