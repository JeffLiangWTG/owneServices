using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefCusNomenclatureGroupTypeVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[] { new RefCusNomenclatureGroupType {
				ZZ9_PK = Guid.NewGuid(),
				ZZ9_GroupType = "A",
				ZZ9_Description = "ADESC",
			} };
		}

		protected override bool UpdateData(object data)
		{
			((RefCusNomenclatureGroupType)data).ZZ9_Description = "AA";
			return true;
		}
	}
}
