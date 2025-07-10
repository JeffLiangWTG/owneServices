using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetRefPackTypeCodeDescriptionPairsTest : WhsSecureServiceTestCase
	{
		#region TestGetRefPackTypeCodeDescriptionPairsWithStandardUnits

		public void TestGetRefPackTypeCodeDescriptionPairsWithStandardUnits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			AssertEquals("Pre-Condition:", SharedConstants.Languages.English, Res.CurrentLanguage);

			var webService1 = GetNewWebService();
			var defaultResponse = webService1.GetRefPackTypeCodeDescriptionPairs();
			AssertSuccessfulResponse(defaultResponse, webService1);

			var defaultUnits = new RefPackTypeCollection(Helper.Factory).GetAsCodeDescriptionPairWithStandardUnits();
			foreach (CodeDescriptionPairInfo pair in defaultResponse.CodeDescriptionPairs)
			{
				AssertEquals(pair.Code, true, defaultUnits.ContainsCode(pair.Code));
			}

			// A Chinese user login which update the current language.
			var chineseSpeaker = Helper.CreateGlbStaff("AAA", "AAA");
			chineseSpeaker.GS_IsDevice = true;
			chineseSpeaker.GS_WorkingLanguage = SharedConstants.Languages.ChineseSimplified;
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, chineseSpeaker);
			var response = webService2.GetRefPackTypeCodeDescriptionPairs();
			AssertSuccessfulResponse(response, webService2);

			AssertEquals("Current language should be updated:", Core.SharedConstants.Languages.ChineseSimplified, Res.CurrentLanguage);

			var units = new RefPackTypeCollection(Helper.Factory).GetAsCodeDescriptionPairWithStandardUnits();
			foreach (CodeDescriptionPairInfo pair in response.CodeDescriptionPairs)
			{
				AssertEquals(pair.Code, true, units.ContainsCode(pair.Code));
			}
		}

		public void TestGetRefPackTypeCodeDescriptionPairsWithStandardUnits_AlternateLanguage_Android()
		{
			TestGetRefPackTypeCodeDescriptionPairsWithStandardUnits_AlternateLanguageCore(true);
		}

		public void TestGetRefPackTypeCodeDescriptionPairsWithStandardUnits_AlternateLanguage_WinCE()
		{
			TestGetRefPackTypeCodeDescriptionPairsWithStandardUnits_AlternateLanguageCore(false);
		}

		void TestGetRefPackTypeCodeDescriptionPairsWithStandardUnits_AlternateLanguageCore(bool isAndroidDevice)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			AssertEquals("Pre-Condition:", SharedConstants.Languages.English, Res.CurrentLanguage);

			var webService1 = GetNewWebService();
			var defaultResponse = webService1.GetRefPackTypeCodeDescriptionPairs();
			AssertSuccessfulResponse(defaultResponse, webService1);

			var defaultUnits = new RefPackTypeCollection(Helper.Factory).GetAsCodeDescriptionPairWithStandardUnits();
			foreach (CodeDescriptionPairInfo pair in defaultResponse.CodeDescriptionPairs)
			{
				AssertEquals(pair.Code, true, defaultUnits.ContainsCode(pair.Code));
			}

			// A Spanish user login which update the current language.
			var spanishSpeaker = Helper.CreateGlbStaff("AAA", "AAA");
			spanishSpeaker.GS_IsDevice = true;
			spanishSpeaker.GS_WorkingLanguage = SharedConstants.Languages.Spanish;
			Helper.Factory.Save();

			var deprecationDate = ZDateTime.Now.AddYears(1).ToDateTime();
			using (WarehouseDataRegistry.Instance.WinCEApplicationUsageEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deprecationDate))
			{
				var webService2 = GetNewWebService(data.Whs1, spanishSpeaker);
				webService2.SecurityHeader.IsAndroidDevice = isAndroidDevice;
				if (!isAndroidDevice)
				{
					webService2.SecurityHeader.DeviceVersion = new DataService().SystemVersion();
				}

				var response = webService2.GetRefPackTypeCodeDescriptionPairs();
				AssertSuccessfulResponse(response, webService2);

				AssertEquals("Current language should be updated:", isAndroidDevice ? SharedConstants.Languages.Spanish : SharedConstants.Languages.English, Res.CurrentLanguage);

				var units = new RefPackTypeCollection(Helper.Factory).GetAsCodeDescriptionPairWithStandardUnits();
				foreach (var pair in response.CodeDescriptionPairs)
				{
					AssertEquals(pair.Code, true, units.ContainsCode(pair.Code));
				}
			}
		}

		#endregion

		#region TestGetRefPackTypeCodeDescriptionPair

		public void TestGetRefPackTypeCodeDescriptionPair()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			AssertEquals("Pre-Condition:", SharedConstants.Languages.English, Res.CurrentLanguage);
			var list = GetRefPackTypeCodeDescriptionPairsHelper.GetAsCodeDescriptionPair(isAndroidDevice: false, Helper.Factory);
			AssertEquals("The code and description should not be the same for EnglishAmerican.", false, list.Any(p => p.Code == p.Description));

			// A Chinese user login which update the current language.
			var chineseSpeaker = Helper.CreateGlbStaff("AAA", "AAA");
			chineseSpeaker.GS_IsDevice = true;
			chineseSpeaker.GS_WorkingLanguage = SharedConstants.Languages.ChineseSimplified;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, chineseSpeaker);
			var response = webService.GetRefPackTypeCodeDescriptionPairs(); // call this only for switching current language.
			AssertSuccessfulResponse(response, webService);

			AssertEquals("Current language should be updated:", SharedConstants.Languages.ChineseSimplified, Res.CurrentLanguage);

			list = GetRefPackTypeCodeDescriptionPairsHelper.GetAsCodeDescriptionPair(isAndroidDevice: false, Helper.Factory);
			AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Helper.Factory).Select(p => p.F3_DescriptionMultilingual.ToString()), list.Select(p => p.Description));
		}

		public void TestGetRefPackTypeCodeDescriptionPair_AlternateLanguageCore_Android()
		{
			TestGetRefPackTypeCodeDescriptionPair_AlternateLanguageCore(true);
		}

		public void TestGetRefPackTypeCodeDescriptionPair_AlternateLanguageCore_WinCE()
		{
			TestGetRefPackTypeCodeDescriptionPair_AlternateLanguageCore(false);
		}

		void TestGetRefPackTypeCodeDescriptionPair_AlternateLanguageCore(bool isAndroidDevice)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			AssertEquals("Pre-Condition:", SharedConstants.Languages.English, Res.CurrentLanguage);
			var list = GetRefPackTypeCodeDescriptionPairsHelper.GetAsCodeDescriptionPair(isAndroidDevice: isAndroidDevice, Helper.Factory);
			AssertEquals("The code and description should not be the same for EnglishAmerican.", false, list.Any(p => p.Code == p.Description));

			// A Spanish user login which update the current language.
			var spanishSpeaker = Helper.CreateGlbStaff("AAA", "AAA");
			spanishSpeaker.GS_IsDevice = true;
			spanishSpeaker.GS_WorkingLanguage = SharedConstants.Languages.Spanish;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, spanishSpeaker);
			webService.SecurityHeader.IsAndroidDevice = isAndroidDevice;
			if (!isAndroidDevice)
			{
				webService.SecurityHeader.DeviceVersion = new DataService().SystemVersion();
			}

			var deprecationDate = ZDateTime.Now.AddYears(1).ToDateTime();
			using (WarehouseDataRegistry.Instance.WinCEApplicationUsageEnabledUntil.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deprecationDate))
			{
				var response = webService.GetRefPackTypeCodeDescriptionPairs(); // call this only for switching current language.
				AssertSuccessfulResponse(response, webService);

				AssertEquals("Current language should be updated:", isAndroidDevice ? SharedConstants.Languages.Spanish : SharedConstants.Languages.English, Res.CurrentLanguage);

				list = GetRefPackTypeCodeDescriptionPairsHelper.GetAsCodeDescriptionPair(isAndroidDevice: isAndroidDevice, Helper.Factory);

				AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Helper.Factory).Select(p => p.F3_DescriptionMultilingual.ToString()), list.Select(p => p.Description));
			}
		}

		#endregion
	}
}
