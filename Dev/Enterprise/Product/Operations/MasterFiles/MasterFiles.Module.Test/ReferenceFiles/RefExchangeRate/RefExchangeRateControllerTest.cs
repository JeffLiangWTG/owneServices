using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefExchangeRateController))]
	sealed class RefExchangeRateControllerTest : ZControllerBasherTest
	{
		public void TestLoadBusinessEntity()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var pk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO RefDatabase_RefExchangeRateZZ (ZZN_PK, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_RX_NKExCurrency, ZZN_RN_NKCountry, ZZN_AsPublished)
VALUES ('{pk}', 'CUS', '2017-01-11 00:00:00', '2079-06-06 23:59:00', 10, 'USD', '{countryCode}', '')");

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_RN_NKCountryCode = countryCode;
			Factory.Save();

			var pkForCurrentCompany = (Guid)Db.Connection.ExecuteScalar($@"SELECT uuid = CONVERT(uniqueidentifier, HASHBYTES('SHA2_256', CONCAT(ZZN_PK, '{GlbCompany.CurrentCompany.GC_Code}'))) FROM RefDatabase_RefExchangeRateZZ
WHERE ZZN_PK = '{pk}'");

			CombineAssertions(() =>
			{
				var refExchangeRate = Factory.LoadTop1<RefExchangeRate>(RefExchangeRate.Loader.GetFilterByPK(pkForCurrentCompany));
				using (var form = (RefExchangeRateForm)new RefExchangeRateController().ShowEditForm(refExchangeRate))
				{
					var entity = (RefExchangeRate)form.BusinessEntity;
					AssertEquals("Should bind to the BO of current company", GlbCompany.CurrentCompany.PK, entity.RE_GC);
				}
			});
		}

		public void TestLoad()
		{
			CombineAssertions(() =>
			{
				TestConnection.ExecuteNonQuery($@"
INSERT INTO RefDatabase_RefExchangeRateZZ (ZZN_PK, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_RX_NKExCurrency, ZZN_RN_NKCountry, ZZN_AsPublished)
VALUES ('{Guid.NewGuid()}', 'CUS', '2017-01-11 00:00:00', '2079-06-06 23:59:00', 10, 'USD', 'ES', '')");

				TestConnection.ExecuteNonQuery($@"
INSERT INTO RefDatabase_RefExchangeRateZZ (ZZN_PK, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_RX_NKExCurrency, ZZN_RN_NKCountry, ZZN_AsPublished)
VALUES ('{Guid.NewGuid()}', 'CUS', '2017-01-11 00:00:00', '2079-06-06 23:59:00', 10, 'USD', 'AU', '')");

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
				{
					var controller = new RefExchangeRateControllerForTest(Factory);

					var pkForCurrentCompany = (Guid)Db.Connection.ExecuteScalar($@"SELECT top 1 uuid = CONVERT(uniqueidentifier, HASHBYTES('SHA2_256', CONCAT(ZZN_PK, '{GlbCompany.CurrentCompany.GC_Code}'))) FROM RefDatabase_RefExchangeRateZZ
WHERE ZZN_RN_NKCountry = 'ES'");

					controller.ModuleResultsPKCollectionExposed = new ZPKCollection(new List<ZGuid>() { pkForCurrentCompany });

					var loader = controller as IBusinessObjectLoader;
					AssertNotNull(loader);

					BusinessObject exRateFromLoader = null;
					AssertNoExceptionThrown(() => exRateFromLoader = loader.Load(Factory, pkForCurrentCompany));
					AssertNotNull(exRateFromLoader);
				}
			});
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			RefExchangeRate testObject = Factory.NewWithValidTestData<RefExchangeRate>();
			testObject.RE_GC = GlbCompany.CurrentCompany.PK;
			testObject.RE_ExpiryDate = ZDateTime.BrettsBirthday;
			testObject.RE_StartDate = ZDateTime.BrettsBirthday;
			testObject.RE_RX_NKExCurrency = "USD";
			testObject.RE_ExRateType = "CUS";
			Factory.Save();
			AssertEquals(true, testObject.IsInDatabase);
			return testObject;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.ExchangeRate;
	}
}
