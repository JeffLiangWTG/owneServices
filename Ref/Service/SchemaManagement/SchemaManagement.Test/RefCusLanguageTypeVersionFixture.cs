using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusLanguageTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[] { new RefLanguageType
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "EN",
				ZX6_Description = "English"
			} };
		}

		protected override bool UpdateData(object data)
		{
			((RefLanguageType)data).ZX6_Description = "YY";
			return true;
		}
	}
}
