using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbCompanyFilterBusinessObject))]
	sealed class GlbCompanyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEmptyFilter()
		{
			GlbCompanyFilterBusinessObject filterObject = new GlbCompanyFilterBusinessObject();

			GlbCompanyCollection collection = new GlbCompanyCollection(Factory, filterObject.Filter);

			AssertEquals("Collection should contain Company1.", true, collection.Contains(company1));
			AssertEquals("Collection should contain Company2.", true, collection.Contains(company2));
		}

		#region Implementation

		RefCurrency currency1, currency2;
		RefCountry country1, country2;
		GlbCompany company1, company2;

		protected override void SetUp()
		{
			base.SetUp();
			currency1 = Factory.NewWithValidTestData<RefCurrency>();
			currency2 = Factory.NewWithValidTestData<RefCurrency>();
			country1 = Factory.New<RefCountry>();
			country1.RN_Code = "AA";
			country1.RN_RX_NKLocalCurrency = currency1.RX_Code;

			country2 = Factory.New<RefCountry>();
			country2.RN_Code = "ZZ";
			country2.RN_RX_NKLocalCurrency = currency2.RX_Code;

			company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = country1.Code;
			company1.GC_RX_NKLocalCurrency = currency1.RX_Code;
			company1.GC_Code = "BOB";
			company1.GC_Name = "XXX";
			company1.GC_City = "Alexandria";
			company1.GC_State = "NSW";
			company1.GC_WebAddress = "site.ua";

			company2 = Factory.New<GlbCompany>();
			company2.GC_RN_NKCountryCode = country2.Code;
			company2.GC_RX_NKLocalCurrency = currency2.RX_Code;
			company2.GC_Code = "WOW";
			company2.GC_Name = "MOO";
			company2.GC_City = "Babylon";
			company2.GC_State = "Babylonian";
			company2.GC_WebAddress = "google.com";
			company2.GC_IsActive = false;

			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbCompanyFilterBusinessObject();
		}

		#endregion
	}
}
