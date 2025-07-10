using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusProfileVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var pk = Guid.NewGuid();
			var profile = new RefCusProfile
			{
				XX0_PK = pk,
				XX0_XXX_ProfileType = profileTypePK,
				XX0_TariffCode = "1001",
				XX0_StartDate = new DateTime(2024, 1, 1),
				XX0_EndDate = new DateTime(2079, 1, 1),
				XX0_QuestionCode = "AAA",
				XX0_ZZZ_NKDataGrouping = "ZA",
				XX0_AllowMultipleAnswers = true,
				XX0_IsAnswerMandatory = true
			};
			result.Add(profile);

			var profileAttribute = new RefCusProfileAttribute
			{
				XXY_PK = Guid.NewGuid(),
				XXY_XX0_Profile = pk,
				XXY_Name = "Name1",
				XXY_Value = "Value1"
			};
			result.Add(profileAttribute);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusProfile profile)
			{
				profile.XX0_TariffCode = "1002";
			}
			if (data is RefCusProfileAttribute profileAttribute)
			{
				profileAttribute.XXY_Value = "Value2";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusProfile
			{
				XX0_PK = profilePK,
				XX0_XXX_ProfileType = profileTypePK,
				XX0_TariffCode = "1001",
				XX0_QuestionCode = "AAA",
				XX0_StartDate = new DateTime(1900, 01, 01),
				XX0_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				XX0_ZZZ_NKDataGrouping = "ZA"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			var profileAttribute = data as RefCusProfileAttribute;
			if (profileAttribute != null)
			{
				profileAttribute.XXY_XX0_Profile = profilePK;
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

				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(RefDbVersionControl)}");
			}
		}

		readonly Guid tariffTypePK = Guid.Parse("F96D4801-344D-487C-9A63-7C7C6187FDC5");
		readonly Guid profileTypePK = Guid.Parse("1FA9C40A-EF50-4360-A8A7-62680558419E");
		readonly Guid profilePK = Guid.Parse("9BABA88C-8ECE-4759-953B-E437F0252E12");
	}
}
