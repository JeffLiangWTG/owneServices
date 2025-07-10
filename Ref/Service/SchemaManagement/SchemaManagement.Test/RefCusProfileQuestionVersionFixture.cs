using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusProfileQuestionVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var pk = Guid.NewGuid();
			var profileQuestion = new RefCusProfileQuestion
			{
				XQ2_PK = pk,
				XQ2_XXX_ProfileType = profileTypePK,
				XQ2_QuestionCode = "1001",
				XQ2_AnswerDataType = "NUMBER",
				XQ2_StartDate = new DateTime(2024, 1, 1),
				XQ2_EndDate = new DateTime(2079, 1, 1),
				XQ2_Name = "Name1",
				XQ2_Text = "Text1",
				XQ2_Note = "Note1",
				XQ2_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(profileQuestion);

			var answerList = new RefCusProfileQuestionAnswerList
			{
				XQ4_PK = Guid.NewGuid(),
				XQ4_XQ2_Question = profileQuestion.XQ2_PK,
				XQ4_Value = "Value1",
				XQ4_Description = "Description"
			};
			result.Add(answerList);

			var answerListLanguage = new RefCusProfileQuestionAnswerListLanguage
			{
				XAL_PK = Guid.NewGuid(),
				XAL_XQ4_QuestionAnswer = answerList.XQ4_PK,
				XAL_Description = "Description",
				XAL_ZX6_NKLanguage = "EN"
			};
			result.Add(answerListLanguage);

			var attribute = new RefCusProfileQuestionAttribute
			{
				XQ3_PK = Guid.NewGuid(),
				XQ3_XQ2_Question = profileQuestion.XQ2_PK,
				XQ3_Name = "Name11",
				XQ3_Value = "Value1"
			};
			result.Add(attribute);

			var questionLanguage = new RefCusProfileQuestionLanguage
			{
				XQL_PK = Guid.NewGuid(),
				XQL_XQ2_Question = profileQuestion.XQ2_PK,
				XQL_Name = "Name1",
				XQL_Text = "Text1",
				XQL_Note = "Note1",
				XQL_ZX6_NKLanguage = "EN"
			};
			result.Add(questionLanguage);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			var modifiedString = "Modified";
			if (data is RefCusProfileQuestion profileQuestion)
			{
				profileQuestion.XQ2_AnswerDataType = "STRING";
			}
			if (data is RefCusProfileQuestionAnswerList answerList)
			{
				answerList.XQ4_Value = modifiedString;
			}
			if (data is RefCusProfileQuestionAnswerListLanguage answerListLanguage)
			{
				answerListLanguage.XAL_Description = modifiedString;
			}
			if (data is RefCusProfileQuestionAttribute attribute)
			{
				attribute.XQ3_Value = modifiedString;
			}
			if (data is RefCusProfileQuestionLanguage questionLanguage)
			{
				questionLanguage.XQL_Note = modifiedString;
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusProfileQuestion
			{
				XQ2_PK = profileQuestionPK,
				XQ2_XXX_ProfileType = profileTypePK,
				XQ2_QuestionCode = "1002",
				XQ2_AnswerDataType = "NUMBER",
				XQ2_StartDate = new DateTime(2025, 1, 1),
				XQ2_EndDate = new DateTime(2079, 1, 1),
				XQ2_Name = "Name2",
				XQ2_Text = "Text2",
				XQ2_Note = "Note2",
				XQ2_ZZZ_NKDataGrouping = "ZA"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusProfileQuestionAnswerList answerList)
			{
				answerList.XQ4_XQ2_Question = profileQuestionPK;
				return true;
			}
			if (data is RefCusProfileQuestionAttribute attribute)
			{
				attribute.XQ3_XQ2_Question = profileQuestionPK;
				return true;
			}
			if (data is RefCusProfileQuestionLanguage questionLanguage)
			{
				questionLanguage.XQL_XQ2_Question = profileQuestionPK;
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefCusTariffTypes.Add(new RefCusTariffType
				{
					ZZI_PK = tariffTypePK,
					ZZI_TariffType = "1P1",
					ZZI_Description = "1P1",
					ZZI_ZZZ_NKDataGrouping = "ZA",
					ZZI_ZZ9_NKNomenclatureGroupType = "CDS"
				});
				context.RefCusProfileTypes.Add(new RefCusProfileType
				{
					XXX_PK = profileTypePK,
					XXX_ProfileType = "TST",
					XXX_Description = "Test",
					XXX_ZZI_TariffType = tariffTypePK,
					XXX_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Language = "EN",
					ZX6_Description = "English"
				});

				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(RefDbVersionControl)}");
			}
		}

		readonly Guid tariffTypePK = Guid.Parse("F96D4801-344D-487C-9A63-7C7C6187FDC5");
		readonly Guid profileTypePK = Guid.Parse("1FA9C40A-EF50-4360-A8A7-62680558419E");
		readonly Guid profileQuestionPK = Guid.Parse("9BABA88C-8ECE-4759-953B-E437F0252E12");
	}
}
