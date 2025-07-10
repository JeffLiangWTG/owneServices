using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationStatusHelperTest : TestCaseWithFactory
	{
		public void TestShouldExcludeMessageStatusFromModuleGrid()
		{
			CombineAssertions("ShouldExcludeMessageStatusFromModuleGrid", () =>
			{
				AssertEquals("Switzerland", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Switzerland, ZGuid.Empty));
				AssertEquals("Netherlands", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Netherlands, ZGuid.Empty));
				AssertEquals("UnitedArabEmirates", true, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.UnitedArabEmirates, ZGuid.Empty));
				AssertEquals("Belgium", true, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Belgium, ZGuid.Empty));
				AssertEquals("Germany", true, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Germany, ZGuid.Empty));
				AssertEquals("China", true, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.China, ZGuid.Empty));
				AssertEquals("SouthAfrica", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.SouthAfrica, ZGuid.Empty));
				AssertEquals("Australia", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Australia, ZGuid.Empty));
				AssertEquals("Singapore", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Singapore, ZGuid.Empty));
				AssertEquals("UnitedKingdom", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.UnitedKingdom, ZGuid.Empty));
				AssertEquals("_TemplateCountryName_", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes._TemplateCountryName_, ZGuid.Empty));
				AssertEquals("Eritrea", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Eritrea, ZGuid.Empty));
				AssertEquals("UnitedKingdom", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Latvia, ZGuid.Empty));
				AssertEquals("Latvia", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.UnitedStates, ZGuid.Empty));
				AssertEquals("Taiwan", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Taiwan, ZGuid.Empty));
				AssertEquals("Italy", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Italy, ZGuid.Empty));
				AssertEquals("Spain", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Spain, ZGuid.Empty));
				AssertEquals("Ireland", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Ireland, ZGuid.Empty));
				AssertEquals("Japan", false, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Japan, ZGuid.Empty));
			});

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("IsInterfaceEnabledCompany", false, IntegratedCountryHelper.IsInterfaceEnabledCompany(GlbCompany.CurrentCompany.PK, Core.Constants.CountryCodes.Germany));
				AssertEquals("Germany", true, DeclarationStatusHelper.ShouldExcludeMessageStatusFromModuleGrid(Core.Constants.CountryCodes.Germany, ZGuid.Empty));
			}
		}

		[AsycudaCustomsCountries(Core.Constants.CountryCodes.Congo)]
		public void TestShouldCombineEntryStatusFromHeaders()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Switzerland", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Switzerland, ZGuid.Empty));
				AssertEquals("Ireland", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Ireland, ZGuid.Empty));
				AssertEquals("Netherlands", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Netherlands, ZGuid.Empty));
				AssertEquals("UnitedArabEmirates", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.UnitedArabEmirates, ZGuid.Empty));
				AssertEquals("Belgium", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Belgium, ZGuid.Empty));
				AssertEquals("Germany", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Germany, ZGuid.Empty));
				AssertEquals("Italy", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Italy, ZGuid.Empty));
				AssertEquals("China", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.China, ZGuid.Empty));
				AssertEquals("SouthAfrica", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.SouthAfrica, ZGuid.Empty));
				AssertEquals("Australia", false, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Australia, ZGuid.Empty));
				AssertEquals("UnitedStates", false, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.UnitedStates, ZGuid.Empty));
				AssertEquals("Congo", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Congo, ZGuid.Empty));
				AssertEquals("UnitedKingdom", false, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.UnitedKingdom, ZGuid.Empty));
				AssertEquals("Singapore", false, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Singapore, ZGuid.Empty));
				AssertEquals("Eritrea", false, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Eritrea, ZGuid.Empty));
				AssertEquals("UnitedKingdom", false, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Latvia, ZGuid.Empty));
				AssertEquals("Latvia", false, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.UnitedStates, ZGuid.Empty));
			});

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("IsInterfaceEnabledCompany", false, IntegratedCountryHelper.IsInterfaceEnabledCompany(GlbCompany.CurrentCompany.PK, Core.Constants.CountryCodes.Germany));
				AssertEquals("Germany", true, DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(Core.Constants.CountryCodes.Germany, ZGuid.Empty));
			}
		}

		public void TestShouldCombineMessageStatusFromHeaders()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Switzerland", true, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Switzerland));
				AssertEquals("Ireland", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Ireland));
				AssertEquals("Netherlands", true, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Netherlands));
				AssertEquals("UnitedArabEmirates", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.UnitedArabEmirates));
				AssertEquals("Belgium", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Belgium));
				AssertEquals("Germany", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Germany));
				AssertEquals("China", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.China));
				AssertEquals("SouthAfrica", true, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.SouthAfrica));
				AssertEquals("Australia", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Australia));
				AssertEquals("UnitedStates", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.UnitedStates));
				AssertEquals("Taiwan", true, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Taiwan));
				AssertEquals("Japan", true, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Japan));
				AssertEquals("UnitedKingdom", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.UnitedKingdom));
				AssertEquals("Singapore", false, DeclarationStatusHelper.ShouldCombineMessageStatusFromHeaders(Core.Constants.CountryCodes.Singapore));
			});
		}
	}
}
