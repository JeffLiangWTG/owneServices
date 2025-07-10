using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefStlFieldMappingVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefStlFieldMapping
				{
					SFM_PK = Guid.NewGuid(),
					SFM_FeatureCode = "AAA",
					SFM_BillableCount = "A",
					SFM_Reference1 = "REF1",
					SFM_Reference2 = "REF2",
					SFM_Reference3 = "REF3",
					SFM_Reference4 = "REF4",
					SFM_Reference5 = "REF5",
					SFM_Category = "Category",
					SFM_PriceItemCode = "ItemCode",
					SFM_ServiceOccuredUTC = "OccuredUTC",
					SFM_ClientStaffCode = "StaffCode"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefStlFieldMapping)data).SFM_FeatureCode = "BBB";
			return true;
		}
	}
}
