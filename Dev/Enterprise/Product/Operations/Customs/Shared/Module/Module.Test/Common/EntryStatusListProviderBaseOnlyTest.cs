using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Module.Testing
{
	sealed class EntryStatusListProviderBaseOnlyTest : EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.HongKong, new string[] { "SUB", "ACK" }, new string[] { "WTO", "ROK", "CEO" });
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.HongKong, new string[] { "SUB", "ACK" }, new string[] { "WTO", "ROK", "CEO" }, "", true);
		}

		public void TestEntryBRStatusLists()
		{
			var countryCode = Core.Constants.CountryCodes.Brazil;
			var cusCodeHelper = new UniversalReferenceTestDataHelper(Factory);
			cusCodeHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsStatus, "Entry Status List Type");
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "E10", "Registered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "E11", "Declaration submitted for dispatch", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "E15", "ACD in process", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "E20", "Released without customs conference", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var expectedEntryStatusList = new[] { "E10", "E11", "E15", "E20", "SUB", "ACK" };
			RunStatusCodeListForVariousCountriesTester(countryCode, expectedEntryStatusList, new string[] { "WTO", "ROK", "CEO" });
			RunStatusCodeListForVariousCountriesTester(countryCode, expectedEntryStatusList, new string[] { "WTO", "ROK", "CEO" }, "", true);
		}

		public void TestCustomsStatusForInterface()
		{
			var countryCode = Core.Constants.CountryCodes.Brazil;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var cusCodeHelper = new UniversalReferenceTestDataHelper(Factory);
				cusCodeHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsStatusForInterface, "Entry Status For Interface List Type");
				cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatusForInterface, "TST", "TEST DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				RunStatusCodeListForVariousCountriesTester(countryCode, new[] { "TST", "SUB", "ACK" }, new string[] { "WTO", "ROK", "CEO" });

				customsInterface.RecipientID = string.Empty;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					RunStatusCodeListForVariousCountriesTester(countryCode, new[] { "SUB", "ACK" }, new string[] { "TST", "WTO", "ROK", "CEO" });
				}

				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					RunStatusCodeListForVariousCountriesTester(countryCode, Array.Empty<string>(), new string[] { "TST", "WTO", "ROK", "CEO" });
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;

			setLocalCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setLocalCountryCustomsInterface.Dispose();
		}

		LocalCountryCustomsInterface customsInterface;
		IDisposable setLocalCountryCustomsInterface;
	}
}
