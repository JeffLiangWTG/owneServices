using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusProcedureFilterStripBusinessObject))]
	public class ZZRefCusProcedureFilterStripBusinessObjectTest : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		public void TestProcedureCodeList()
		{
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.ProcedureCode, "01", "DE Procedure 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "01", "IE ProhibitedGoodsExport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "01", "IE Advanced Procedure 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.ProcedureCode, "01", "EU Procedure 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.ProcedureCode, "10", "EU Procedure 10", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProcedureCode, "01", "IE Procedure 01 b", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var procedureCodeList = GetNew().Lookups.ProcedureCodeList();
				AssertEquals("IE Procedure 01 b", procedureCodeList.GetDescriptionFromCode("01"));
				AssertEquals("EU Procedure 10", procedureCodeList.GetDescriptionFromCode("10"));
			}
		}

		public void TestProcedureCodeListMultilingualDescription()
		{
			const string languageCodeGermany = "DE";
			helper.CreateOrGetLanguage(languageCodeGermany, "Deutsch");
			var procedure = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProcedureCode, "01", "Ecology process", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListLanguage(procedure, languageCodeGermany, "Ökologie Verfahrens");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				GlbStaff.CurrentUser.GS_WorkingLanguage = "EN";
				var procedureCodeList = GetNew().Lookups.ProcedureCodeList();
				var procedureCode = procedureCodeList[0];
				AssertEquals("Description by English", "Ecology process", procedureCode.Description);

				GlbStaff.CurrentUser.GS_WorkingLanguage = languageCodeGermany;
				Factory.ClearCachedValue<CodeDescriptionPairList>($"BaseZZRefCusProcedureFilterLookups|{Core.Constants.CountryCodes.Ireland}|{ZDateTime.Today.ToShortDateString()}|{nameof(ZZRefCusProcedureFilterLookups.ProcedureCodeList)}");
				procedureCode = GetNew().Lookups.ProcedureCodeList()[0];
				AssertEquals("Description by Germany", "Ökologie Verfahrens", procedureCode.Description);
			}
		}

		public void TestPreviousProcedureCodeList()
		{
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.ProcedureCode, "21", "DE Procedure 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "21", "IE ProhibitedGoodsExport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "21", "IE Advanced Procedure 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.ProcedureCode, "23", "EU Procedure 23", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProcedureCode, "21", "IE Procedure 21 b", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var previousProcedureCodeList = GetNew().Lookups.PreviousProcedureCodeList();
				AssertEquals("IE Procedure 21 b", previousProcedureCodeList.GetDescriptionFromCode("21"));
				AssertEquals("EU Procedure 23", previousProcedureCodeList.GetDescriptionFromCode("23"));
			}
		}

		public void TestConcessionCodeList()
		{
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, C.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "F21", "DE AdvancedProcedureCode F21", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProhibitedGoodsExport, "F21", "IE ProhibitedGoodsExport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.ProcedureCode, "F21", "IE Procedure F21", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(C.RefDataGrouping.Codes.EuropeanUnionEUN, C.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "E53", "EU AdvancedProcedureCode E53", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, C.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "F21", "IE AdvancedProcedureCode F21 b", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var concessionCodeList = GetNew().Lookups.ConcessionCodeList();
				AssertEquals("IE AdvancedProcedureCode F21 b", concessionCodeList.GetDescriptionFromCode("F21"));
				AssertEquals("EU AdvancedProcedureCode E53", concessionCodeList.GetDescriptionFromCode("E53"));
			}
		}

		UniversalReferenceTestDataHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(C.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", euDataGrouping);
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.ProcedureCode, "EU Procedure Code");
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.AdvancedProcedureCode, "IE Advanced Procedure Code");
			helper.CreateRefCusProcedure(
				Core.Constants.CountryCodes.Ireland, ZString.Empty,
				"01", "21", "F21", "Free circulation with onward dispatch -- Exemption from import duties of products of sea-fishing and other products taken from the territorial sea of a country or territory outside the customs territory of the Union by vessels solely registered or recorded in a member state and flying the flag of that state",
				"IMP"
			);
			helper.CreateRefCusProcedure(
				Core.Constants.CountryCodes.Ireland, ZString.Empty,
				"10", "23", "E53", "Permanent export. - Temporary export for return in the unaltered state. - Agricultural products refund, small quantities, no export certificate",
				"EXP"
			);
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => GetNew();

		ZZRefCusProcedureFilterStripBusinessObject GetNew() => new ZZRefCusProcedureFilterStripBusinessObject(Factory);
	}
}
