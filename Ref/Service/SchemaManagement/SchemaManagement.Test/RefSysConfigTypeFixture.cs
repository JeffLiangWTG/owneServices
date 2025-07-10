using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefSysConfigTypeFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var configType = new RefSysConfigType
			{
				ZRT_PK = Guid.NewGuid(),
				ZRT_ConfigCode = "TEST",
				ZRT_Description = "TST",
				ZRT_LongDescription = "TST",
			};
			result.Add(configType);
			result.Add(new RefSysConfig
			{
				ZRC_PK = Guid.NewGuid(),
				ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode,
				ZRC_BitValue = false,
				ZRC_BinaryValue = null,
				ZRC_DecimalValue = 0,
				ZRC_StringValue = "ZZ",
				ZRC_StartDate = new DateTime(2020, 1, 1),
				ZRC_EndDate = new DateTime(2020,12,31)
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefSysConfigType configType)
			{
				configType.ZRT_LongDescription = "TestDescription";
			}
			if (data is RefSysConfig config)
			{
				config.ZRC_StringValue = "XX";
			}
			return true;
		}
	}
}
