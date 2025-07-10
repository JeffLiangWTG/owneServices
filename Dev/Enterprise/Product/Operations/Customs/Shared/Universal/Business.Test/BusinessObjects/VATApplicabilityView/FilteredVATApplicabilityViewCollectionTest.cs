using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(FilteredVATApplicabilityViewCollection))]
	class FilteredVATApplicabilityViewCollectionTest : ActiveBusinessObjectCollectionTestCase<FilteredVATApplicabilityViewCollection>
	{
		public void TestEnableEffectiveData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			var rateCode = helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var vatApplicability1 = helper.CreateVATApplicabilityView(tariff, Core.Constants.CountryCodes.Eritrea, "RT1", startDate: new ZDateTime(2011, 1, 1), endDate: new ZDateTime(2012, 12, 31));
			var vatApplicability2 = helper.CreateVATApplicabilityView(tariff, Core.Constants.CountryCodes.SouthAfrica, "RT2", startDate: new ZDateTime(2012, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			var vatApplicability3 = helper.CreateVATApplicabilityView(tariff, "DG1", "RT3", startDate: new ZDateTime(2012, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			Factory.Save();
			var f = new BusinessObjectFactory();
			var tariffReloaded = f.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredCollection = new FilteredVATApplicabilityViewCollection(tariffReloaded, true);
			var collection = new VATApplicabilityViewCollection(tariffReloaded);
			AssertEquals("filteredCollection", 3, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability1", filteredCollection.FindByPK(vatApplicability1.PK));
			AssertNotNull("filteredCollection.vatApplicability2", filteredCollection.FindByPK(vatApplicability2.PK));
			AssertNotNull("filteredCollection.vatApplicability3", filteredCollection.FindByPK(vatApplicability3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.vatApplicability1", collection.FindByPK(vatApplicability1.PK));
			AssertNotNull("collection.vatApplicability2", collection.FindByPK(vatApplicability2.PK));
			AssertNotNull("collection.vatApplicability3", collection.FindByPK(vatApplicability3.PK));
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability1", filteredCollection.FindByPK(vatApplicability1.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.vatApplicability1", collection.FindByPK(vatApplicability1.PK));
			AssertNotNull("collection.vatApplicability2", collection.FindByPK(vatApplicability2.PK));
			AssertNotNull("collection.vatApplicability3", collection.FindByPK(vatApplicability3.PK));
			wrapper.EffectiveDataGrouping = "DG1";
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability3", filteredCollection.FindByPK(vatApplicability3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.vatApplicability1", collection.FindByPK(vatApplicability1.PK));
			AssertNotNull("collection.vatApplicability2", collection.FindByPK(vatApplicability2.PK));
			AssertNotNull("collection.vatApplicability3", collection.FindByPK(vatApplicability3.PK));
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability1", filteredCollection.FindByPK(vatApplicability1.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.vatApplicability1", collection.FindByPK(vatApplicability1.PK));
			AssertNotNull("collection.vatApplicability2", collection.FindByPK(vatApplicability2.PK));
			AssertNotNull("collection.vatApplicability3", collection.FindByPK(vatApplicability3.PK));
			wrapper.EffectiveDate = new ZDate(2013, 1, 1);
			AssertEquals("filteredCollection", 2, filteredCollection.Count);
			AssertNotNull("filteredCollection.vatApplicability2", filteredCollection.FindByPK(vatApplicability2.PK));
			AssertNotNull("filteredCollection.vatApplicability3", filteredCollection.FindByPK(vatApplicability3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.vatApplicability1", collection.FindByPK(vatApplicability1.PK));
			AssertNotNull("collection.vatApplicability2", collection.FindByPK(vatApplicability2.PK));
			AssertNotNull("collection.vatApplicability3", collection.FindByPK(vatApplicability3.PK));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var vat = Tariff.FilteredVATApplicabilities.AddNew();
			vat.ZX5_ZZ1_ParentTariffOrNationalCode = Tariff.PK;
			vat.ZX5_StartDate = new ZDateTime(2010, 12, 10);
			vat.ZX5_EndDate = new ZDateTime(2079, 06, 06);
			vat.ZX5_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			return vat;
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
		RefCusTariffType TariffType
		{
			get
			{
				if (tariffType == null)
				{
					tariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
					Factory.Save();
				}

				return tariffType;
			}
		}

		RefCusTariffType tariffType;
		TariffView Tariff => tariff ?? (tariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false));
		TariffView tariff;
		protected override FilteredVATApplicabilityViewCollection GetCollectionToTest()
		{
			return new FilteredVATApplicabilityViewCollection(Tariff, true);
		}
	}
}
