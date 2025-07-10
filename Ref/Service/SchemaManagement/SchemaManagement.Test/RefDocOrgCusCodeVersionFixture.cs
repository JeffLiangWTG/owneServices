using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	class RefDocOrgCusCodeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefDocOrgCusCode()
				{
					DOC_PK = Guid.NewGuid(),
					DOC_CodeType = "AA",
					DOC_DocumentType = "AWB",
					DOC_ShortLabel = "CC",
					DOC_LongLabel = "CC",
					DOC_Description = "Description",
					DOC_Notes = "EE",
					DOC_Priority = 1,
					DOC_RN_NKCodeCountry = "AU",
					DOC_RN_NKRegulatingCountry = "NZ",
					DOC_Direction = "BTH"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefDocOrgCusCode)data).DOC_Priority = 2;
			return true;
		}
	}
}
