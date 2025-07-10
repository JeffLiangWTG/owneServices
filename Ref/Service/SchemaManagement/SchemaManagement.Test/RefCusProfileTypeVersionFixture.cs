using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusProfileTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var profileType = new RefCusProfileType
			{
				XXX_PK = Guid.NewGuid(),
				XXX_ProfileType = "A",
				XXX_ZZI_TariffType = Guid.Parse("B8911435-0FFF-40B0-B24E-E2281C8DE515"),
				XXX_Description = "A Description",
				XXX_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(profileType);
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusProfileType profileType)
			{
				profileType.XXX_Description = "B Description";
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
					ZZI_PK = Guid.Parse("B8911435-0FFF-40B0-B24E-E2281C8DE515"),
					ZZI_TariffType = "1P1",
					ZZI_Description = "1P1",
					ZZI_ZZZ_NKDataGrouping = "ZA",
					ZZI_ZZ9_NKNomenclatureGroupType = "CDS"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(RefDbVersionControl)}");
			}
		}
	}
}
