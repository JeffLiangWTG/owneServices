using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCarrierCodeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var code = new RefCarrierCode
			{
				ZZ4_PK = Guid.NewGuid(),
				ZZ4_Code = "AA",
				ZZ4_Description = "AAAA",
				ZZ4_IsAir = true,
				ZZ4_IsRail = false,
				ZZ4_IsRoad = false,
				ZZ4_IsSea = false,
				ZZ4_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(code);
			result.Add(new RefCarrierCodeAttribute
			{
				ZZG_PK = Guid.NewGuid(),
				ZZG_ZZ4_CarrierCode = code.ZZ4_PK,
				ZZG_Name = "AAA",
				ZZG_Value = "A"
			});
			result.Add(new RefCarrierVesselPivot
			{
				ZZQ_PK = Guid.NewGuid(),
				ZZQ_ZZ4 = code.ZZ4_PK,
				ZZQ_ZZO = Guid.Parse("096C03A2-B2AF-4DA4-823F-DEEFB6257C87")
			});

			result.Add(new RefCarrierCodeLanguage
			{
				ZCL_PK = Guid.NewGuid(),
				ZCL_ZX6_NKLanguage = "AA",
				ZCL_ZZ4_CarrierCode = code.ZZ4_PK,
				ZCL_Description = "ABCDEFGHIJKLMN"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCarrierCode code)
			{
				code.ZZ4_Description = "D2";
				return true;
			}
			if (data is RefCarrierCodeAttribute attr)
			{
				attr.ZZG_Value = "V2";
				return true;
			}
			return false;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCarrierCode
			{
				ZZ4_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZ4_Code = "BB",
				ZZ4_Description = "BBBB",
				ZZ4_IsAir = true,
				ZZ4_IsRail = false,
				ZZ4_IsRoad = false,
				ZZ4_IsSea = false,
				ZZ4_ZZZ_NKDataGrouping = "ZA"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCarrierCodeAttribute attr)
			{
				attr.ZZG_ZZ4_CarrierCode = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefCarrierVesselPivot pivot)
			{
				pivot.ZZQ_ZZ4 = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is RefCarrierCodeLanguage language)
			{
				language.ZCL_ZZ4_CarrierCode = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefVesselZZs.Add(new RefVesselZZ
				{
					ZZO_PK = Guid.Parse("096C03A2-B2AF-4DA4-823F-DEEFB6257C87"),
					ZZO_Code = "AAA",
					ZZO_LloydsNumber = "A",
					ZZO_RadioCallSign = "A",
					ZZO_VesselType = "AA",
					ZZO_ZZZ_NKDataGrouping = "ZA",
					ZZO_RN_NKCountryOfReg = "ZA"
				});

				context.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Language = "AA",
					ZX6_Description = "AALanguage",
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
