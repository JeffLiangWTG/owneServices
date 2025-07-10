using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PackingHouseLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			CombineAssertions(() =>
			{
				var lookups = new PackingHouseLookups(packingHouse);
				var coll = (ZZRefCusCodeListCombinedCollection)lookups.CY_CodeList;
				NUnit.Framework.Assert.That(coll.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(x => x.FilterName == "Country").Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(coll.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(x => x.FilterName == "Tariff").Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));

				invoiceLine.JI_CountryOfOrigin = "AU";
				invoiceLine.JI_Tariff = "07061000005";
				coll = (ZZRefCusCodeListCombinedCollection)lookups.CY_CodeList;
				NUnit.Framework.Assert.That(coll.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(x => x.FilterName == "Country").Value, NUnit.Framework.Is.EqualTo("AU").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(coll.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().First(x => x.FilterName == "Tariff").Value, NUnit.Framework.Is.EqualTo("07061000005").Using(CustomComparers.TypeComparison));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			packingHouse = invoiceLine.PackingHouseCollection.AddNew();
		}

		JobComInvoiceLine invoiceLine;
		PackingHouse packingHouse;
	}
}
