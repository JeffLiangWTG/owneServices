using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefShippingLineMessagingRequirementTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefShippingLineMessagingRequirementType
				{
					RST_PK = Guid.NewGuid(),
					RST_Code = "AAA",
					RST_Description = "Description"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefShippingLineMessagingRequirementType)data).RST_Description = "Description 2";
			return true;
		}
	}
}
