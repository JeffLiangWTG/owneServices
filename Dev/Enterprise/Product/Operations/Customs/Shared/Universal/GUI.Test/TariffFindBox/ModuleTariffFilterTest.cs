using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(ModuleTariffFilter))]
	class ModuleTariffFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFormatedText()
		{
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			helper.TariffFormatter = new TariffFormatterRemovePeriods();
			var filter = new ModuleTariffFilter("3.1..", TariffViewSchema.ZZ1_TariffCode, helper.TariffFormatter);
			filter.Property = "3.1..";
			AssertEquals("31", filter.Property);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "1P1", null, null);
			helper.TariffFormatter = new TariffFormatterRemovePeriods();
			return new ModuleTariffFilter(Constants.RefCusTariffFilters.TariffCode, TariffViewSchema.ZZ1_TariffCode, helper?.TariffFormatter);
		}
	}

	class TariffFormatterRemovePeriods : ITariffFormatter
	{
		public ZString DisplayFormat(ZString unformattedTariff) => unformattedTariff.Replace(".", "");

		public ZString Format(ZString unformattedTariff) => unformattedTariff.Replace(".", "");

		public ZString UniversalFormat(ZString unformattedTariff) => Format(unformattedTariff);
	}
}
