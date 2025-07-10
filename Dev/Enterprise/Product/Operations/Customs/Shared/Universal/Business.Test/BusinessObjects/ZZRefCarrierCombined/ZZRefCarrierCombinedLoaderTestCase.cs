using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCarrierCombined.Loader))]
	public class ZZRefCarrierCombinedLoaderTestCase : LoaderTestCase
	{
		public void TestLoadCarriersWithAttributes()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "111";
			carrier1.ZZ4_CountryOrGrouping = "XX";
			carrier1.ZZ4_Description = "One";
			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "222";
			carrier2.ZZ4_CountryOrGrouping = "XX";
			carrier2.ZZ4_Description = "Two";
			var carrier3 = Factory.New<ZZRefCarrierCombined>();
			carrier3.ZZ4_Code = "333";
			carrier3.ZZ4_CountryOrGrouping = "XX";
			carrier3.ZZ4_Description = "Three";
			var attrib1A = carrier1.Attributes.AddNew();
			attrib1A.ZZG_Name = "A";
			attrib1A.ZZG_Value = "aa";
			var attrib1B = carrier1.Attributes.AddNew();
			attrib1B.ZZG_Name = "B";
			attrib1B.ZZG_Value = "bb";
			var attrib2A = carrier2.Attributes.AddNew();
			attrib2A.ZZG_Name = "A";
			attrib2A.ZZG_Value = "aa";
			Factory.Save();
			var loader = (ZZRefCarrierCombined.Loader)GetNewLoaderToTest();
			var results = loader.LoadCarriersWithAttributes("XX", new ZString[] { "A" });
			AssertEquals(2, results.Length);
			Assert((carrier1.PK == results[0].PK && carrier2.PK == results[1].PK) || (carrier1.PK == results[1].PK && carrier2.PK == results[0].PK));
			results = loader.LoadCarriersWithAttributes("XX", new ZString[] { "A", "B" });
			AssertEquals(1, results.Length);
			Assert(carrier1.PK == results[0].PK);
			results = loader.LoadCarriersWithAttributes("YY", new ZString[] { "A", "B" });
			AssertEquals(0, results.Length);
			results = loader.LoadCarriersWithAttributes("XX", new ZString[] { "B" });
			AssertEquals(1, results.Length);
			Assert(carrier1.PK == results[0].PK);
			results = loader.LoadCarriersWithAttributes("XX", new ZString[] { "C" });
			AssertEquals(0, results.Length);
			results = loader.LoadCarriersWithAttributes("XX", new ZString[] { "b" });
			AssertEquals(1, results.Length);
			Assert(carrier1.PK == results[0].PK);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new ZZRefCarrierCombined.Loader(Factory);
		}
	}
}
