using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusAUNexdocECMCodeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefCusAUNexdocECMCode()
				{
					ZY5_PK = Guid.NewGuid(),
					ZY5_CommodityCode = "A",
					ZY5_PackTypeCode = "PT",
					ZY5_PreservationCode = "PC",
					ZY5_ProductTypeCode = "PT",
					ZY5_SupplementaryCode = "SC",
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefCusAUNexdocECMCode)data).ZY5_ProductTypeCode = "PT2";
			return true;
		}
	}
}
