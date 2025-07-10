using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProcedure.Loader))]
	class RefCusProceduresLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusProcedure.Loader(Factory);
		}

		public void TestConcessions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A#", "B$", "C%", "12A,12B", "Descr", "EXP");
			AssertArrayEqualsByElements(new ZString[] { "12A", "12B" }, procedure.Concessions.ToArray());
			procedure.ZZ6_Concession = "4";
			AssertArrayEqualsByElements(new ZString[] { "4" }, procedure.Concessions.ToArray());
			procedure.ZZ6_Concession = "12A, 12B";
			AssertArrayEqualsByElements(new ZString[] { "12A", "12B" }, procedure.Concessions.ToArray());
		}

		public void TestShipmentTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A#", "B$", "C%", "12A,12B", "Descr", "EXP,IMP");
			AssertArrayEqualsByElements(new ZString[] { "EXP", "IMP" }, procedure.ShipmentTypes.ToArray());
			procedure.ZZ6_ShipmentType = "EXP";
			AssertArrayEqualsByElements(new ZString[] { "EXP" }, procedure.ShipmentTypes.ToArray());
			procedure.ZZ6_ShipmentType = "EXP, EX";
			AssertArrayEqualsByElements(new ZString[] { "EXP", "EX" }, procedure.ShipmentTypes.ToArray());
		}

		public void TestGroups()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A#", "B$", "C%", "12A,12B", "Descr", "EXP");
			procedure.ZZ6_Group = "4";
			AssertArrayEqualsByElements(new ZString[] { "4" }, procedure.Groups.ToArray());
			procedure.ZZ6_Group = "1,2";
			AssertArrayEqualsByElements(new ZString[] { "1", "2" }, procedure.Groups.ToArray());
			procedure.ZZ6_Group = "11, 2";
			AssertArrayEqualsByElements(new ZString[] { "11", "2" }, procedure.Groups.ToArray());
		}

		public void TestLoadTop1FromCodeAndCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "11", "", "", "Clearance of goods for Home Use, and free circulation", "");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "12", "00", "", "Home Use' and payment of VAT, on goods received from the BLNS states.", "");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "13", "", "", "IncludeGroup", "", group: "Group");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "14", "", "F51", "IncludeConcession", "");
			Factory.Save();
			var loader = new RefCusProcedure.Loader(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("Test 1", null, loader.LoadTop1FromCodeAndCountry(ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Today));
				AssertEquals("Test 2", null, loader.LoadTop1FromCodeAndCountry(ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Australia, ZDateTime.Today));
				AssertEquals("Test 3", null, loader.LoadTop1FromCodeAndCountry(ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today));
				AssertEquals("Test 4", "Clearance of goods for Home Use, and free circulation", loader.LoadTop1FromCodeAndCountry("11", ZString.Empty, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today).ZZ6_Description);
				AssertEquals("Test 5", "Home Use' and payment of VAT, on goods received from the BLNS states.", loader.LoadTop1FromCodeAndCountry("12", "00", Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today).ZZ6_Description);
				AssertEquals("Test 6", null, loader.LoadTop1FromCodeAndCountry(ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today));
				AssertEquals("Test 7", "IncludeGroup", loader.LoadTop1FromCodeAndCountry("13", ZString.Empty, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, "Group").ZZ6_Description);
				AssertEquals("Test 8", null, loader.LoadTop1FromCodeAndCountry("14", ZString.Empty, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, null, "F61"));
				AssertEquals("Test 9", "IncludeConcession", loader.LoadTop1FromCodeAndCountry("14", ZString.Empty, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today).ZZ6_Description);
				AssertEquals("Test 10", "IncludeConcession", loader.LoadTop1FromCodeAndCountry("14", ZString.Empty, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, null, "F51").ZZ6_Description);
			}

			);
		}

		public void TestLoadFromProcedureAndPreviousProcedureAndConcession()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Singapore, "", "420", "", "3000", "SEASTORE (OUT APS)", "OUT", group: "APS");
			var procedure1Attribute1 = helper.CreateRefCusProcedureAttribute(procedure1.PK, Universal.AttributeNames.Codes.ISSEASTORE, "Y");
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Singapore, "", "440", "", "2000", "CWC (OUT DRT)", "OUT", group: "DRT");
			var procedure3 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Singapore, "", "440", "", "1000", "AEO (OUT DRT)", "OUT", group: "DRT");
			var procedure4 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Singapore, "", "440", "", "6000", "CNB (OUT DRT)", "OUT", group: "DRT");
			Factory.Save();
			var loader = new RefCusProcedure.Loader(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("Test 1", null, loader.LoadFromProcedureAndPreviousProcedureAndConcession(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Today));
				AssertEquals("Test 2", null, loader.LoadFromProcedureAndPreviousProcedureAndConcession(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Australia, ZDateTime.Today));
				AssertEquals("Test 3", null, loader.LoadFromProcedureAndPreviousProcedureAndConcession(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Singapore, ZDateTime.Today));
				AssertEquals("Test 4", "SEASTORE (OUT APS)", loader.LoadFromProcedureAndPreviousProcedureAndConcession("420", ZString.Empty, "3000", ZString.Empty, Core.Constants.CountryCodes.Singapore, ZDateTime.Today).ZZ6_Description);
				AssertEquals("Test 5", "AEO (OUT DRT)", loader.LoadFromProcedureAndPreviousProcedureAndConcession("440", ZString.Empty, "1000", ZString.Empty, Core.Constants.CountryCodes.Singapore, ZDateTime.Today).ZZ6_Description);
				AssertEquals("Test 6", null, loader.LoadFromProcedureAndPreviousProcedureAndConcession(ZString.Empty, ZString.Empty, "3000", ZString.Empty, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today));
				AssertEquals("Test 7", "CNB (OUT DRT)", loader.LoadFromProcedureAndPreviousProcedureAndConcession("440", ZString.Empty, "6000", ZString.Empty, Core.Constants.CountryCodes.Singapore, ZDateTime.Today).ZZ6_Description);
			}

			);
		}

		public void TestLoadForShipmentTypeAndCountry_AndTestLoadForCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure01GbImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "11", "11", "111", "One Import", "IMP");
			var procedure02GbExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "22", "22", "222", "Two Export", "EXP");
			var procedure03ItImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "CAT", "33", "33", "333", "Three Import", "IMP");
			var procedure04ItExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "CAT", "44", "44", "444", "Four Export", "EXP");
			var procedure05GbImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "55", "55", "555", "Five Import", "IMP");
			var procedure06GbExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "66", "77", "666", "Six Export", "EXP");
			var loader = new RefCusProcedure.Loader(Factory);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { procedure01GbImp, procedure05GbImp }, loader.LoadForShipmentTypeAndZzzDataGrouping("IMP", Core.Constants.CountryCodes.UnitedKingdom));
				AssertContainsExactElementsInAnyOrder(new[] { procedure06GbExp, procedure02GbExp }, loader.LoadForShipmentTypeAndZzzDataGrouping("EXP", Core.Constants.CountryCodes.UnitedKingdom));
				AssertContainsExactElementsInAnyOrder(new[] { procedure03ItImp }, loader.LoadForShipmentTypeAndZzzDataGrouping("IMP", Core.Constants.CountryCodes.Italy));
				AssertEquals(0, loader.LoadForShipmentTypeAndZzzDataGrouping("EXP", Core.Constants.CountryCodes.Germany).Length);
			}

			);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { procedure01GbImp, procedure02GbExp, procedure05GbImp, procedure06GbExp }, loader.LoadForZzzDataGrouping(Core.Constants.CountryCodes.UnitedKingdom));
				AssertContainsExactElementsInAnyOrder(new[] { procedure03ItImp, procedure04ItExp }, loader.LoadForZzzDataGrouping(Core.Constants.CountryCodes.Italy));
				AssertEquals(0, loader.LoadForZzzDataGrouping(Core.Constants.CountryCodes.Germany).Length);
			}

			);
		}

		public void TestLoadDistinctGroupCodesAndLoadForShipmentTypeAndCountryAndGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure01GbImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "11", "11", "111", "One Import", "IMP", group: "GR1,GR11");
			var procedure02GbExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "22", "22", "222", "Two Export", "EXP", group: "GR2;GR22;GR222");
			var procedure03ItImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "CAT", "33", "33", "333", "Three Import", "IMP", group: "GR3");
			var procedure04ItExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "CAT", "44", "44", "444", "Four Export", "EXP");
			var procedure05GbImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "55", "55", "555", "Five Import", "IMP", group: "GR5,GR1");
			var procedure06GbExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "66", "77", "666", "Six Export", "EXP", group: "GR6");
			var loader = new RefCusProcedure.Loader(Factory);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "GR1", "GR11", "GR5" }, loader.LoadDistinctGroupCodes("IMP", Core.Constants.CountryCodes.UnitedKingdom));
				AssertContainsExactElementsInAnyOrder(new[] { "GR6", "GR2", "GR22", "GR222" }, loader.LoadDistinctGroupCodes("EXP", Core.Constants.CountryCodes.UnitedKingdom));
				AssertContainsExactElementsInAnyOrder(new[] { "GR3" }, loader.LoadDistinctGroupCodes("IMP", Core.Constants.CountryCodes.Italy));
				AssertEquals(0, loader.LoadDistinctGroupCodes("EXP", Core.Constants.CountryCodes.Germany).Count());
				AssertContainsExactElementsInAnyOrder(new[] { procedure01GbImp, procedure05GbImp }, loader.LoadForShipmentTypeAndZzzDataGroupingAndGroup("IMP", Core.Constants.CountryCodes.UnitedKingdom, "GR1"));
			}

			);
		}

		public void TestLoadDistinctGroupCodesWithSetDateTime()
		{
			var date = new ZDateTime(2024,10,12,10,22,00);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedureWithDefaultDate(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "11", "11", "111", "One Import", "IMP", date, date.AddDays(+10), group: "GR1,GR11");
			helper.CreateRefCusProcedureWithDefaultDate(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "22", "22", "222", "two Import", "IMP", date.AddDays(-5), date.AddDays(-2), group: "GR2,GR22");
			helper.CreateRefCusProcedureWithDefaultDate(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "33", "33", "333", "Three Import", "IMP", date.AddDays(4), date.AddDays(+10), group: "GR5,GR1");
			var loader = new RefCusProcedure.Loader(Factory);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "GR1", "GR11" }, loader.LoadDistinctGroupCodes("IMP", Core.Constants.CountryCodes.UnitedKingdom, date));
				AssertContainsExactElementsInAnyOrder(new[] { "GR1", "GR11", "GR5" }, loader.LoadDistinctGroupCodes("IMP", Core.Constants.CountryCodes.UnitedKingdom, date.AddDays(5)));
			}
			);
		}

		public void TestLoadDistinctGroupCodesForDatagrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure01GbImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "11", "11", "111", "One Import", "IMP", group: "GR1,GR11");
			var procedure02GbExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "22", "22", "222", "Two Export", "EXP", group: "GR2;GR22;GR222");
			var procedure03ItImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "CAT", "33", "33", "333", "Three Import", "IMP", group: "GR3");
			var procedure04ItExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "CAT", "44", "44", "444", "Four Export", "EXP");
			var procedure05GbImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "55", "55", "555", "Five Import", "IMP", group: "GR5,GR1");
			var procedure06GbExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "66", "77", "666", "Six Export", "EXP", group: "GR6");
			var loader = new RefCusProcedure.Loader(Factory);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "GR1", "GR11", "GR5", "GR6", "GR2", "GR22", "GR222" }, loader.LoadDistinctGroupCodesForDatagrouping(Core.Constants.CountryCodes.UnitedKingdom));
				AssertContainsExactElementsInAnyOrder(new[] { "GR3" }, loader.LoadDistinctGroupCodesForDatagrouping(Core.Constants.CountryCodes.Italy));
				AssertEquals(0, loader.LoadDistinctGroupCodesForDatagrouping(Core.Constants.CountryCodes.Germany).Count());
			}

			);
		}

		public void TestLoadForShipmentTypeAndZzzDataGroupingAndGroupCaseSensitive()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure01DeExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "CAT", "11", "11", "111", "One Export", "EXP", group: "gr1");
			var procedure02DeExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "CAT", "22", "22", "222", "Two Export", "EXP", group: "gr1");
			var loader = new RefCusProcedure.Loader(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { procedure01DeExp, procedure02DeExp }, loader.LoadForShipmentTypeAndZzzDataGroupingAndGroup("EXP", Core.Constants.CountryCodes.Germany, "GR1"));
		}

		public void TestLoadDistinctGroupCodesShouldBeTrimmed()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "CAT", "22", "01", "F61", "Vorübergehende Ausfuhr zu anderen als unter Code 21 genannten Zwecken · Überführung von Waren in den zollrechtlich freien Verkehr mit gleichzeitiger Wiederversendung · Sonstige: Bevorratung", "EXP", group: "wP, nA,nK,nN");
			var loader = new RefCusProcedure.Loader(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { "wP", "nA", "nK", "nN" }, loader.LoadDistinctGroupCodes("EXP", Core.Constants.CountryCodes.Germany));
		}

		public void TestLoadForProcedureCodesAndGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CDS", "CAT", "11", "11", "111", "Import", "IMP");
			helper.CreateRefCusProcedure("CDS", "CAT", "11", "12", "222", "Import", "IMP");
			helper.CreateRefCusProcedure("XXX", "CAT", "12", "11", "333", "Import", "IMP");
			helper.CreateRefCusProcedure("CDS", "CAT", "11", "11", "444", "Export", "EXP");
			var loader = new RefCusProcedure.Loader(Factory);
			var testResult = loader.LoadForProcedureCodesAndGrouping("IMP", "11", "11", "CDS");
			AssertEquals(1, testResult.Length);
			Assert(testResult.Any(result => result.ZZ6_Concession == "111"));
		}

		public void TestReleasedGuarantee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("CDS", "CAT", "11", "11", "111", "Import", "IMP");
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			Assert(procedure.IsGuaranteeReleased());
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.No;
			Assert(!procedure.IsGuaranteeReleased());
		}

		public void TestConsumedGuarantee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("CDS", "CAT", "11", "11", "111", "Import", "IMP");
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			Assert(procedure.IsGuaranteeConsumed());
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.No;
			Assert(!procedure.IsGuaranteeConsumed());
		}

		public void TestIsTransit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("CDS", "CAT", "11", "11", "111", "Import", "IMP");
			procedure.ZZ6_IsTransit = YesNoList.Codes.Yes;
			Assert(procedure.IsTransit());
			procedure.ZZ6_IsTransit = YesNoList.Codes.No;
			Assert(!procedure.IsTransit());
		}
	}
}
