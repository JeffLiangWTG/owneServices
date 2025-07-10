using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileQuestion.Loader))]
	class RefCusProfileQuestionLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusProfileQuestion.Loader(Factory);
		}

		public void TestLoad()
		{
			var today = ZDateTime.Today;
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Angola);
			Factory.Save();
			var tariffType1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN", "NCM");
			var tariffType2 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Angola, "HSN", "NCM");
			Factory.Save();
			var tariff1 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, tariffType1.PK, "12345678", today.AddDays(-5), today.AddDays(5));
			var tariff2 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, tariffType1.PK, "87654321", today.AddDays(-5), today.AddDays(5));
			var tariff3 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Angola, tariffType2.PK, "12345678", today.AddDays(-5), today.AddDays(5));
			var tariff4 = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Angola, tariffType2.PK, "87654321", today.AddDays(-5), today.AddDays(5));

			var profileType1 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Brazil, "NCM", "BR NCM");
			var profileType2 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Brazil, "NVE", "BR NVE");
			var profileType3 = Helper.CreateRefCusProfileType(tariffType2.PK, Core.Constants.CountryCodes.Angola, "NCM", "AO NCM");
			Factory.Save();

			var profile1 = Helper.CreateRefCusProfile(profileType1, "12345678", "ATT1", today.AddDays(-5), today.AddDays(5), new[] { ("A1", "1"), ("A2", "2") });
			var profile2 = Helper.CreateRefCusProfile(profileType1, "123456", "ATT2", today.AddDays(-5), today.AddDays(5), new[] { ("A1", "1"), ("A2", "X") });
			var profile3 = Helper.CreateRefCusProfile(profileType1, "", "ATT3", today.AddDays(-5), today.AddDays(5), new[] { ("A1", "X"), ("A2", "2") });
			var profile4 = Helper.CreateRefCusProfile(profileType1, "1234", "ATT4", today.AddDays(-5), today.AddDays(5));
			var profile5 = Helper.CreateRefCusProfile(profileType1, "87654321", "ATT5", today.AddDays(-5), today.AddDays(5));
			var profile6 = Helper.CreateRefCusProfile(profileType2, "12345678", "ATT1", today.AddDays(-5), today.AddDays(5));
			var profile7 = Helper.CreateRefCusProfile(profileType3, "12345678", "ATT7", today.AddDays(-5), today.AddDays(5));
			var profile8 = Helper.CreateRefCusProfile(profileType3, "12345678", "", today.AddDays(-5), today.AddDays(5));
			var profile9 = Helper.CreateRefCusProfile(profileType1, "12345678", "ATT8", today.AddDays(-5), today.AddDays(5), new[] { ("A3", "3"), ("A4", "4") });

			var question1 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 1", "ATT1", "Text 1", today.AddDays(-5), today.AddDays(5), attributes: new[] { ("B1", "1"), ("B2", "2") });
			var question2 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 2", "ATT2", "Text 2", today.AddDays(-5), today.AddDays(5), attributes: new[] { ("B1", "1"), ("B2", "X") });
			var question3 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 3", "ATT3", "Text 3", today.AddDays(-2), today.AddDays(5), attributes: new[] { ("B1", "X"), ("B2", "2") });
			var question4 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 4", "ATT4", "Text 4", today.AddDays(-5), today.AddDays(-2));
			var question5 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 5", "ATT5", "Text 5", today.AddDays(-5), today.AddDays(5));
			var question6 = Helper.CreateRefCusProfileQuestion(profileType2, "Test 6", "ATT1", "Text 6", today.AddDays(-5), today.AddDays(5));
			var question7 = Helper.CreateRefCusProfileQuestion(profileType3, "Test 7", "ATT7", "Text 7", today.AddDays(-5), today.AddDays(5));
			var question8 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 8", "ATT8", "Text 8", today.AddDays(10), today.AddDays(20), attributes: new[] { ("C1", "1"), ("C2", "2") });

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loader = new RefCusProfileQuestion.Loader(factory);
			AssertEquals(0, loader.Load(ZString.Empty, tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today).Length);
			AssertEquals(0, loader.Load("NCM", ZGuid.Empty, "12345678", Core.Constants.CountryCodes.Brazil, today).Length);
			AssertEquals(0, loader.Load("NCM", tariffType1.PK, ZString.Empty, Core.Constants.CountryCodes.Brazil, today).Length);
			AssertEquals(0, loader.Load("NCM", tariffType1.PK, "12345678", ZString.Empty, today).Length);
			AssertEquals(3, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, ZDate.Empty).Length);

			AssertContainsExactElementsInAnyOrder(new[] { question1.PK, question2.PK, question3.PK }, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question1.PK, question2.PK, question4.PK }, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today.AddDays(-3)).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question1.PK }, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today, false, new[] { ("A1", new[] { "1" }), ("A2", new[] { "2" }) }, new[] { ("B1", "1"), ("B2", "2") }).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question8.PK }, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today, true, new[] { ("A3", new[] { "3" }), ("A4", new[] { "4" }) }, new[] { ("C1", "1"), ("C2", "2") }).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question1.PK, question2.PK }, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today, questionAttributes: new[] { ("B1", "1") }).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question1.PK, question3.PK }, loader.Load("NCM", tariffType1.PK, "12345678", Core.Constants.CountryCodes.Brazil, today, questionAttributes: new[] { ("B2", "2") }).Select(x => x.PK));

			AssertContainsExactElementsInAnyOrder(new[] { question1.PK }, loader.Load(new[] { profile1 }, Core.Constants.CountryCodes.Brazil, today).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question5.PK }, loader.Load(new[] { profile5 }, Core.Constants.CountryCodes.Brazil, today).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question6.PK }, loader.Load(new[] { profile6 }, Core.Constants.CountryCodes.Brazil, today).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question7.PK }, loader.Load(new[] { profile7 }, Core.Constants.CountryCodes.Angola, today).Select(x => x.PK));
			AssertEquals(0, loader.Load(new[] { profile8 }, Core.Constants.CountryCodes.Angola, today).Length);

			int GetHintCount() => factory.TableSelects.Single(x => x.TableName == RefCusProfileQuestionSchema.Constants.TableName).Value;
			var hintCount = GetHintCount();
			AssertContainsExactElementsInAnyOrder(new[] { question5.PK, question6.PK, question1.PK }, loader.Load(new[] { profile5, profile6, profile1 }, Core.Constants.CountryCodes.Brazil, today).Select(x => x.PK));
			AssertEquals("No more db hint when all RefCusProfileQuestion cached", hintCount, GetHintCount());
		}

		public void TestLoadByProfileTypeAndDataGrouping()
		{
			var today = ZDateTime.Today;
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Mexico);
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Angola);
			Factory.Save();

			var tariffType1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Mexico, "IDL");
			var tariffType2 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Angola, "HSN", "NCM");
			Factory.Save();

			var profileType1 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Mexico, "AI", "OPERACIONES DE COMERCIO EXTERIOR CON AMPARO");
			var profileType2 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Mexico, "AC", "ALMACÉN GENERAL DE DEPÓSITO CERTIFICADO");
			var profileType3 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Mexico, "B2", "BIENES DEL ARTÍCULO 2 DE LA LEY DEL IEPS.");
			var profileType4 = Helper.CreateRefCusProfileType(tariffType2.PK, Core.Constants.CountryCodes.Angola, "NCM", "AO NCM");
			Factory.Save();

			var question1 = Helper.CreateRefCusProfileQuestion(profileType1, "Complemento 1", "CO1", "Número de registro como Almacén General de Depósito certificado", today.AddDays(-5), today.AddDays(5));
			var question2 = Helper.CreateRefCusProfileQuestion(profileType2, "Complemento 1", "CO1", "Número de registro como Almacén General de Depósito certificado", today.AddDays(-5), today.AddDays(5));
			var question3 = Helper.CreateRefCusProfileQuestion(profileType3, "Complemento 1", "CO1", "Número de registro como Almacén General de Depósito certificado", today.AddDays(-5), today.AddDays(5));
			var question4 = Helper.CreateRefCusProfileQuestion(profileType2, "Complemento 2", "CO2", "La opción que aplique de acuerdo a los bienes señalados en el artículo 2, fracción I de la Ley del IEPS, de acuerdo a lo siguiente:", today.AddDays(-5), today.AddDays(5));
			var question5 = Helper.CreateRefCusProfileQuestion(profileType3, "Complemento 3", "CO3", "La cuota aplicable de conformidad con el Artículo 2, fracción I, incisos D) y/o H) de la Ley del IEPS.", today.AddDays(-5), today.AddDays(5));
			var question6 = Helper.CreateRefCusProfileQuestion(profileType4, "Test 7", "ATT7", "Text 7", today.AddDays(-5), today.AddDays(5));
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loader = new RefCusProfileQuestion.Loader(factory);
			AssertEquals(1, loader.Load(profileType1.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, today).Length);
			AssertEquals(2, loader.Load(profileType2.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, today).Length);
			AssertEquals(2, loader.Load(profileType3.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, today).Length);
			AssertEquals(1, loader.Load(profileType4.XXX_ProfileType, Core.Constants.CountryCodes.Angola, today).Length);
			AssertEquals(2, loader.Load(profileType3.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, ZDateTime.Empty).Length);
			AssertEquals(0, loader.Load(profileType3.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, today.AddDays(100)).Length); 
			AssertEquals(0, loader.Load(profileType3.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, today.AddDays(-100)).Length);
			AssertEquals(0, loader.Load(ZString.Empty, Core.Constants.CountryCodes.Mexico, today).Length);
			AssertEquals(0, loader.Load(profileType3.XXX_ProfileType, ZString.Empty, today).Length);

			AssertContainsExactElementsInAnyOrder(new[] { question1.PK }, loader.Load(profileType1.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, today).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question2.PK, question4.PK }, loader.Load(profileType2.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, today).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question3.PK, question5.PK }, loader.Load(profileType3.XXX_ProfileType, Core.Constants.CountryCodes.Mexico, today).Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { question6.PK }, loader.Load(profileType4.XXX_ProfileType, Core.Constants.CountryCodes.Angola, today).Select(x => x.PK));
		}

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
