using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusProfileQuestionPathwayVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var pk = Guid.NewGuid();
			var pathway = new RefCusProfileQuestionPathway
			{
				XQP_PK = pk,
				XQP_XQ2_QuestionParent = profileQuestionPK1,
				XQP_XQ2_QuestionChild = profileQuestionPK2,
				XQP_Description = "Desc",
				XQP_StartDate = new DateTime(2024, 1, 1),
				XQP_EndDate = new DateTime(2079, 6, 6),
				XQP_AllowMultipleAnswers = true,
				XQP_IsAnswerMandatory = true
			};
			result.Add(pathway);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusProfileQuestionPathway pathway)
			{
				pathway.XQP_Description = "Modified";
			}
			return true;
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
				context.RefCusProfileQuestions.Add(new RefCusProfileQuestion
				{
					XQ2_PK = profileQuestionPK1,
					XQ2_XXX_ProfileType = profileTypePK,
					XQ2_QuestionCode = "1001",
					XQ2_AnswerDataType = "NUMBER",
					XQ2_StartDate = new DateTime(2024, 1, 1),
					XQ2_EndDate = new DateTime(2079, 1, 1),
					XQ2_Name = "Name1",
					XQ2_Text = "Text1",
					XQ2_Note = "Note1",
					XQ2_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefCusProfileQuestions.Add(new RefCusProfileQuestion
				{
					XQ2_PK = profileQuestionPK2,
					XQ2_XXX_ProfileType = profileTypePK,
					XQ2_QuestionCode = "1002",
					XQ2_AnswerDataType = "STRING",
					XQ2_StartDate = new DateTime(2024, 1, 1),
					XQ2_EndDate = new DateTime(2079, 1, 1),
					XQ2_Name = "Name2",
					XQ2_Text = "Text2",
					XQ2_Note = "Note2",
					XQ2_ZZZ_NKDataGrouping = "ZA"
				});

				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(RefDbVersionControl)}");
			}
		}

		readonly Guid tariffTypePK = Guid.Parse("FA554B4A-6CB6-4126-8055-1B9EB7DF0939");
		readonly Guid profileTypePK = Guid.Parse("FF981ACE-3A66-49FF-B117-EAB469FB06A5");
		readonly Guid profileQuestionPK1 = Guid.Parse("D5B43A16-6850-4547-8E7E-E368BD6865DE");
		readonly Guid profileQuestionPK2 = Guid.Parse("DD56FEE8-74E2-4E61-A7A1-A31E41168BFA");
	}
}
