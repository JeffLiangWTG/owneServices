using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusCodeListVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var codeList = new RefCusCodeList
			{
				ZZD_PK = Guid.NewGuid(),
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "PE",
				ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
				ZZD_StartDate = new DateTime(1900, 01, 01),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA"
			};
			result.Add(codeList);
			result.Add(new RefCusCodeListLanguage
			{
				ZXA_PK = Guid.NewGuid(),
				ZXA_ZZD_CodeList = codeList.ZZD_PK,
				ZXA_ZX6_NKLanguage = "EN",
				ZXA_Description = "English"
			});
			var codeListAttribute = new RefCusCodeListAttribute
			{
				ZZE_PK = Guid.NewGuid(),
				ZZE_ZXE_NKName = "Code",
				ZZE_Value = "15",
				ZZE_ZZD_CodeList = codeList.ZZD_PK
			};
			result.Add(codeListAttribute);

			result.Add(new RefCusCodeOrAttributeTransportMode
			{
				ZZU_PK = Guid.NewGuid(),
				ZZU_TransportMode = "AIR",
				ZZU_ZZD_CodeList = codeList.ZZD_PK,
				ZZU_DataSetPK = codeList.ZZD_PK,
				ZZU_DataSetCode = "ZZD"
			});
			result.Add(new RefCusCodeOrAttributeTransportMode
			{
				ZZU_PK = Guid.NewGuid(),
				ZZU_TransportMode = "SEA",
				ZZU_ZZE_Attribute = codeListAttribute.ZZE_PK,
				ZZU_DataSetPK = codeListAttribute.ZZE_PK,
				ZZU_DataSetCode = "ZZE"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusCodeList codeList)
			{
				codeList.ZZD_Description = "XX";
			}
			if (data is RefCusCodeListLanguage language)
			{
				language.ZXA_Description = "XX";
			}
			if (data is RefCusCodeListAttribute attr)
			{
				attr.ZZE_Value = "GG";
			}
			if (data is RefCusCodeOrAttributeTransportMode transMode)
			{
				if (transMode.ZZU_ZZD_CodeList != null)
				{
					transMode.ZZU_TransportMode = "MAI";
				}
				else if (transMode.ZZU_ZZE_Attribute != null)
				{
					transMode.ZZU_TransportMode = "FIX";
				}
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusCodeList
			{
				ZZD_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZD_ZZK_NKCodeType = "PKG",
				ZZD_Code = "BB",
				ZZD_Description = "BBB",
				ZZD_StartDate = new DateTime(1900, 01, 01),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
				ZZD_ZZZ_NKDataGrouping = "ZA"
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			var language = data as RefCusCodeListLanguage;
			if (language != null)
			{
				language.ZXA_ZZD_CodeList = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefCusCodeTypes.Add(new RefCusCodeType
				{
					ZZK_PK = Guid.NewGuid(),
					ZZK_CodeType = "PKG",
					ZZK_Description = "Package",
					ZZK_MaxLength = 0,
					ZZK_ZZZ_NKDataGrouping = "ZA"
				});
				context.RefLanguageTypes.Add(new RefLanguageType
				{
					ZX6_PK = Guid.NewGuid(),
					ZX6_Description = "English",
					ZX6_Language = "EN"
				});
				context.SaveChanges();
				context.RefCusCodeListAttributeNames.Add(new RefCusCodeListAttributeName
				{
					ZXE_PK = Guid.NewGuid(),
					ZXE_Name = "Code",
					ZXE_ZZK_NKCodeType = "PKG",
					ZXE_ZZZ_NKDataGrouping = "ZA",
					ZXE_Description = "AA",
					ZXE_IsMandatory = false,
					ZXE_AllowDuplicates = false,
					ZXE_IsValueMandatory = false,
					ZXE_ValueDataType = string.Empty,
					ZXE_MinLengthOrValue = 0,
					ZXE_MaxLengthOrValue = 0,
					ZXE_DecimalPlaces = 0,
					ZXE_ColumnCaption = ""
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
