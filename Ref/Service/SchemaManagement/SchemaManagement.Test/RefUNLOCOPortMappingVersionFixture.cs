using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefUNLOCOPortMappingVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>
			{
				new RefUNLOCOPortMapping()
				{
					RLM_PK = Guid.NewGuid(),
					RLM_RL_NKCode = "AUALX",
					RLM_RL_NKCodeRelated = "AUBOT"
				}
			};
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefUNLOCOPortMapping portMapping)
			{
				portMapping.RLM_RL_NKCode = "AUSYD";
			}
			return true;
		}
	}
}
