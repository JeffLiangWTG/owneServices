using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusRulingVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var ruling = new RefCusRuling
			{
				ZZX_PK = Guid.NewGuid(),
				ZZX_RulingNumber = "Ruling Number",
				ZZX_RN_NKCountryCode = "NZ",
				ZZX_RulingType = "RT",
				ZZX_Description = "D",
				ZZX_StartDate = new DateTime(1900, 01, 01),
				ZZX_EndDate = new DateTime(2079, 06, 06, 23, 59, 0)
			};
			result.Add(ruling);
			result.Add(new RefCusRulingConfig
			{
				ZZY_PK = Guid.NewGuid(),
				ZZY_ZZX_CusRuling = ruling.ZZX_PK,
				ZZY_Category = "GST",
				ZZY_Type = "T",
				ZZY_Rate = 1,
				ZZY_Value = "V"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefCusRuling ruling)
			{
				ruling.ZZX_Description = "D2";
			}
			if (data is RefCusRulingConfig rulingConfig)
			{
				rulingConfig.ZZY_Value = "V2";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new RefCusRuling
			{
				ZZX_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ZZX_RulingNumber = "Ruling Number1",
				ZZX_RN_NKCountryCode = "NZ",
				ZZX_RulingType = "RT",
				ZZX_Description = "D",
				ZZX_StartDate = new DateTime(1900, 01, 01),
				ZZX_EndDate = new DateTime(2079, 06, 06, 23, 59, 0)
			} };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefCusRulingConfig rulingConfig)
			{
				rulingConfig.ZZY_ZZX_CusRuling = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}
	}
}
