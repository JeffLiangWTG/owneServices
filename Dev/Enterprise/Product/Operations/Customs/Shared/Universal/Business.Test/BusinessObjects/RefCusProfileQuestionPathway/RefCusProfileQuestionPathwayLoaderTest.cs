using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileQuestionPathway.Loader))]
	class RefCusProfileQuestionPathwayLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusProfileQuestionPathway.Loader(Factory);
		}

		public void TestLoad()
		{
			var today = ZDateTime.Today;
			Helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			Factory.Save();
			var tariffType1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN", "NCM");
			Factory.Save();

			var profileType1 = Helper.CreateRefCusProfileType(tariffType1.PK, Core.Constants.CountryCodes.Brazil, "NCM", "BR NCM");
			Factory.Save();

			var question1 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 1", "ATT1", "Text 1", today.AddDays(-5), today.AddDays(5));
			var question2 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 2", "ATT2", "Text 2", today.AddDays(-5), today.AddDays(5));
			var question3 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 3", "ATT3", "Text 3", today.AddDays(-5), today.AddDays(5));
			var question4 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 4", "ATT4", "Text 4", today.AddDays(-5), today.AddDays(5));
			var question5 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 5", "ATT5", "Text 5", today.AddDays(-5), today.AddDays(5));
			var question6 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 6", "ATT6", "Text 6", today.AddDays(-5), today.AddDays(5));
			var question7 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 7", "ATT7", "Text 7", today.AddDays(-5), today.AddDays(5));
			var question8 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 8", "ATT8", "Text 8", today.AddDays(-5), today.AddDays(5));
			var question9 = Helper.CreateRefCusProfileQuestion(profileType1, "Test 9", "ATT9", "Text 9", today.AddDays(-5), today.AddDays(5));
			Factory.Save();

			var pathwayQuestion1 = Helper.CreateRefCusProfileQuestionPathway(question2, question3, "ATT2->ATT3", today.AddDays(-2), today.AddDays(5), "");
			var pathwayQuestion2 = Helper.CreateRefCusProfileQuestionPathway(question2, question4, "ATT2->ATT4", today.AddDays(-2), today.AddDays(5), "");
			var pathwayQuestion3 = Helper.CreateRefCusProfileQuestionPathway(question2, question5, "ATT2->ATT5", today.AddDays(-2), today.AddDays(5), "");
			var pathwayQuestion4 = Helper.CreateRefCusProfileQuestionPathway(question4, question1, "ATT4->ATT1", today.AddDays(-2), today.AddDays(5), "");
			var pathwayQuestion5 = Helper.CreateRefCusProfileQuestionPathway(question4, question6, "ATT4->ATT6", today.AddDays(-2), today.AddDays(5), "");
			var pathwayQuestion6 = Helper.CreateRefCusProfileQuestionPathway(question6, question7, "ATT6->ATT7", today.AddDays(-2), today.AddDays(5), "");
			var pathwayQuestion7 = Helper.CreateRefCusProfileQuestionPathway(question1, question2, "ATT1->ATT2", today.AddDays(-5), today.AddDays(-3), "");
			var pathwayQuestion8 = Helper.CreateRefCusProfileQuestionPathway(question4, question7, "ATT4->ATT7", today.AddDays(-5), today.AddDays(-3), "");
			var pathwayQuestion9 = Helper.CreateRefCusProfileQuestionPathway(question8, question9, "ATT8->ATT9", today.AddDays(-2), today.AddDays(5), "");
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loader = new RefCusProfileQuestionPathway.Loader(factory);

			CombineAssertions(() =>
			{
				AssertLoad(Array.Empty<RefCusProfileQuestionPathway>(), new[] { question1 }, today, true);
				AssertLoad(Array.Empty<RefCusProfileQuestionPathway>(), new[] { question7 }, today, true);
				AssertLoad(new[] { pathwayQuestion7 }, new[] { question1 }, today.AddDays(-4), true);
				AssertLoad(new[] { pathwayQuestion8 }, new[] { question4 }, today.AddDays(-4), true);
				AssertLoad(new[] { pathwayQuestion4, pathwayQuestion5, pathwayQuestion6 }, new[] { question4 }, today, true);
				AssertLoad(new[] { pathwayQuestion1, pathwayQuestion2, pathwayQuestion3, pathwayQuestion4, pathwayQuestion5, pathwayQuestion6 }, new[] { question2 }, today, true);
				AssertLoad(new[] { pathwayQuestion1, pathwayQuestion2, pathwayQuestion3, pathwayQuestion4, pathwayQuestion5, pathwayQuestion6, pathwayQuestion9 }, new[] { question2, question8 }, today, true);

				AssertLoad(Array.Empty<RefCusProfileQuestionPathway>(), new[] { question1 }, today, false);
				AssertLoad(Array.Empty<RefCusProfileQuestionPathway>(), new[] { question7 }, today, false);
				AssertLoad(new[] { pathwayQuestion7 }, new[] { question1 }, today.AddDays(-4), false);
				AssertLoad(new[] { pathwayQuestion8 }, new[] { question4 }, today.AddDays(-4), false);
				AssertLoad(new[] { pathwayQuestion4, pathwayQuestion5 }, new[] { question4 }, today, false);
				AssertLoad(new[] { pathwayQuestion1, pathwayQuestion2, pathwayQuestion3 }, new[] { question2 }, today, false);
				AssertLoad(new[] { pathwayQuestion1, pathwayQuestion2, pathwayQuestion3, pathwayQuestion9 }, new[] { question2, question8 }, today, false);
			});

			int GetHintCount() => factory.TableSelects.Single(x => x.TableName == RefCusProfileQuestionPathwaySchema.Constants.TableName).Value;
			var hintCount = GetHintCount();

			loader.Load(new[] { question1.PK, question2.PK, question4.PK, question7.PK, question8.PK }, today, true);
			AssertEquals("No more db hint when all RefCusProfileQuestionPathway cached", hintCount, GetHintCount());

			loader.Load(new[] { question1.PK, question2.PK, question4.PK, question7.PK, question8.PK }, today, false);
			AssertEquals("No more db hint when all RefCusProfileQuestionPathway cached", hintCount, GetHintCount());

			void AssertLoad(RefCusProfileQuestionPathway[] expected, RefCusProfileQuestion[] parentQuestions, ZDateTime date, bool recursive)
			{
				AssertContainsExactElementsInAnyOrder($"Load for {string.Join(",", parentQuestions.Select(x => x.XQ2_Code))}, date:{date}, recursive: {recursive}",
					expected.Select(x => x.XQP_Description), loader.Load(parentQuestions.Select(x => x.PK).ToArray(), date, recursive).Select(x => x.XQP_Description));
			}
		}

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
