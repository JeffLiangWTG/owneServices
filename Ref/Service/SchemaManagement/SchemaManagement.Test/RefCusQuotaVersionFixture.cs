using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusQuotaVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
{
				new RefCusQuota()
				{
					ZXQ_PK = Guid.NewGuid(),
					ZXQ_OrderNumber = "AAA",
					ZXQ_InitialAmount = 1,
					ZXQ_UnitOfMeasure = "KGM",
					ZXQ_Balance = 1,
					ZXQ_StartDate = new DateTime(2000,1,1),
					ZXQ_EndDate = new DateTime(2010,1,1),
					ZXQ_ZZZ_NKDataGrouping = "ZA"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefCusQuota)data).ZXQ_UnitOfMeasure = "NEW";
			return true;
		}
	}
}
