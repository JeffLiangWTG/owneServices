using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefExchangeRateCollection))]
	sealed class RefExchangeRateCollectionTest : ActiveBusinessObjectCollectionTestCase<RefExchangeRateCollection>
	{
		[ExpectNoExceptions]
		public void TestRemoveAndDeleteSuceedsWithSufficientAccess()
		{
			RefExchangeRateCollection collection = new RefExchangeRateCollection(Factory);
			RefExchangeRate exchangeRate = collection.AddNew();
			exchangeRate.RE_ExRateType = "CUS";
			Env.Security.CustomsExchangeRateUpdate.IsAllowed = true;
			collection.DeleteAll();
		}

		public void TestLoad()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var pk = ZGuid.NewZGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO RefDatabase_RefExchangeRateZZ (ZZN_PK, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_RX_NKExCurrency, ZZN_RN_NKCountry, ZZN_AsPublished)
VALUES ('{pk}', 'CUS', '2017-01-11 00:00:00', '2079-06-06 23:59:00', 10, 'USD', '{countryCode}', '')");

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_RN_NKCountryCode = countryCode;
			Factory.Save();

			var pkForCurrentCompany = (Guid)Db.Connection.ExecuteScalar($@"SELECT uuid = CONVERT(uniqueidentifier, HASHBYTES('SHA2_256', CONCAT(ZZN_PK, '{GlbCompany.CurrentCompany.GC_Code}'))) FROM RefDatabase_RefExchangeRateZZ
WHERE ZZN_PK = '{pk}'");

			var pkForAnotherCompany = (Guid)Db.Connection.ExecuteScalar($@"SELECT uuid = CONVERT(uniqueidentifier, HASHBYTES('SHA2_256', CONCAT(ZZN_PK, '{anotherCompany.GC_Code}'))) FROM RefDatabase_RefExchangeRateZZ
WHERE ZZN_PK = '{pk}'");

			CombineAssertions(() =>
			{
				var collection = new RefExchangeRateCollection(Factory, new ZQuery(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK));
				var loader = collection as IBusinessObjectLoader;
				AssertNotNull("loader isn't null", loader);

				RefExchangeRate exRateFromLoader = null;
				AssertNoExceptionThrown("No Exception", () => exRateFromLoader = (RefExchangeRate)loader.Load(Factory, pkForCurrentCompany));
				AssertEquals("Can load for current company", GlbCompany.CurrentCompany.PK, exRateFromLoader.RE_GC);

				AssertNoExceptionThrown("No Exception", () => exRateFromLoader = (RefExchangeRate)loader.Load(Factory, pkForAnotherCompany));
				AssertNull("Cannot load from another company", exRateFromLoader);
			});
		}
	}
}
