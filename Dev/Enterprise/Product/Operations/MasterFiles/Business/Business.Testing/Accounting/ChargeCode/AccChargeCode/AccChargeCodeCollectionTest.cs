using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCodeCollection))]
	class AccChargeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccChargeCodeCollection(Factory);
		}

		public void TestCollection()
		{
			AccChargeCodeCollection collection = new AccChargeCodeCollection(Factory, new ZQuery());

			AssertEquals("Collection should be emtpy", 0, collection.Count);

			AccChargeCode chargeCode = Factory.Load<AccChargeCode>(TestCaseHelper.GetFirstPKFromTable(AccChargeCode.Schema.TableName));
			collection.Add(chargeCode);

			AssertEquals("First Element", chargeCode, collection[0]);
		}

		public void TestStaticFilterByCurrentCompany()
		{
			AccChargeCode chargeCodeForCurrentCompany = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCodeForNonCurrentCompany = Factory.NewWithValidTestData<AccChargeCode>();
			GlbCompany nonCurrentCompany = Factory.New<GlbCompany>();
			GlbCompany currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);

			SetupCompany(nonCurrentCompany);
			SetupChargeCode(chargeCodeForCurrentCompany, currentCompany, "~NNN");
			SetupChargeCode(chargeCodeForNonCurrentCompany, nonCurrentCompany, "~CCC");

			Factory.Save();

			AccChargeCodeCollection chargeCodeCollection = new AccChargeCodeCollection(Factory, new ZQuery());
			Assert("AC_Code for Non Current Company should not be in collection", !chargeCodeCollection.ContainsCode(chargeCodeForNonCurrentCompany.AC_Code));
			Assert("AC_Code for Current Company should be in collection", chargeCodeCollection.ContainsCode(chargeCodeForCurrentCompany.AC_Code));
			chargeCodeCollection.Load();
			Assert("Charge Code for Non Current Company should not be in collection", !chargeCodeCollection.Contains(chargeCodeForNonCurrentCompany.PK));
			Assert("Charge Code for Current Company should be in collection", chargeCodeCollection.Contains(chargeCodeForCurrentCompany.PK));

			chargeCodeCollection = new AccChargeCodeCollection(Factory);
			Assert("AC_Code for Non Current Company should not be in collection", !chargeCodeCollection.ContainsCode(chargeCodeForNonCurrentCompany.AC_Code));
			Assert("AC_Code for Current Company should be in collection", chargeCodeCollection.ContainsCode(chargeCodeForCurrentCompany.AC_Code));
			chargeCodeCollection.Load();
			Assert("Charge Code for Non Current Company should not be in collection", !chargeCodeCollection.Contains(chargeCodeForNonCurrentCompany.PK));
			Assert("Charge Code for Current Company should be in collection", chargeCodeCollection.Contains(chargeCodeForCurrentCompany.PK));

			chargeCodeCollection = new AccChargeCodeCollection(Factory);
			chargeCodeCollection.Load(new ZQuery());
			Assert("Charge Code for Non Current Company should not be in collection", !chargeCodeCollection.Contains(chargeCodeForNonCurrentCompany.PK));
			Assert("Charge Code for Current Company should be in collection", chargeCodeCollection.Contains(chargeCodeForCurrentCompany.PK));
		}

		public void TestStaticFilterByCurrentCompanyIncludeGlobal()
		{
			var chargeCodeGlobal = Factory.NewWithValidTestData<AccChargeCode>();
			var nonCurrentCompany = Factory.New<GlbCompany>();
			SetupCompany(nonCurrentCompany);
			chargeCodeGlobal.AC_GC = ZGuid.Empty;
			chargeCodeGlobal.AC_Code = "CODE";
			chargeCodeGlobal.AC_Desc = "code desc";
			chargeCodeGlobal.AC_ChargeType = "REV";
			chargeCodeGlobal.AC_IsActive = true;
			Factory.Save();

			AccChargeCodeCollection chargeCodeCollection = new AccChargeCodeCollection(Factory, new ZQuery(), nonCurrentCompany.PK.ToGuid(), true);
			Assert("AC_Code should be in collection", chargeCodeCollection.ContainsCode(chargeCodeGlobal.AC_Code));

			chargeCodeCollection.Load();
			Assert("Charge Code should be in collection", chargeCodeCollection.Contains(chargeCodeGlobal.PK));
		}

		#region Implementation

		void SetupCompany(GlbCompany company)
		{
			company.GC_StartDate = Env.Time.CurrentLocalDateTime;
			company.GC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			company.GC_RX_NKLocalCurrency = Env.CurrentCompany.LocalCurrency.Code;
		}

		void SetupChargeCode(AccChargeCode chargeCode, GlbCompany company, ZString aC_Code)
		{
			chargeCode.AC_GC = company.PK;
			chargeCode.AC_Code = aC_Code;
		}

		#endregion
	}
}
