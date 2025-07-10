using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusCodeTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var refCusCodeType = new RefCusCodeType
			{
				ZZK_PK = Guid.NewGuid(),
				ZZK_CodeType = "MAN",
				ZZK_Description = "Manifest Country",
				ZZK_MaxLength = 0,
				ZZK_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(refCusCodeType);
			var refCusCodeTypeLanguage = new RefCusCodeTypeLanguage
			{
				ZXI_PK = Guid.NewGuid(),
				ZXI_ZX6_NKLanguage = "EN",
				ZXI_ZZK_CodeType = refCusCodeType.ZZK_PK,
				ZXI_Description = "English",
			};
			result.Add(refCusCodeTypeLanguage);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusCodeType codeType)
			{
				codeType.ZZK_Description = "XX";
				return true;
			}
			if (data is RefCusCodeTypeLanguage codeTypeLanguage)
			{
				codeTypeLanguage.ZXI_Description = "XX";
				return true;
			}
			return false;
		}

		protected override IEnumerable<object> GetData(object[] dataSet, DataPreparationStep step)
		{
			var result = base.GetData(dataSet, step);
			if (step == DataPreparationStep.Delete)
			{
				// Cannot delete RefCusCodeListAttributeName but have to flag its RefDbVerionControl as deleted.
				// This is for backward compatible with client data contract ver 24 or lower
				result = result.Where(x => x.GetType() != typeof(RefCusCodeListAttributeName));
			}
			return result;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var refLanguageType = new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				};
				context.RefLanguageTypes.Add(refLanguageType);
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusCodeType
			{
				ZZK_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZK_CodeType = "BBB",
				ZZK_Description = "BBBBB",
				ZZK_MaxLength = 0,
				ZZK_ZZZ_NKDataGrouping = "ZA"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusCodeTypeLanguage typeLanguage)
			{
				typeLanguage.ZXI_ZZK_CodeType = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}
	}
}
