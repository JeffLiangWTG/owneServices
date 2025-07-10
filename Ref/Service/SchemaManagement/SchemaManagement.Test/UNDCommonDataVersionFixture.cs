using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGCommonDataVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[] { new UNDGCommonData {
				DC_PK = Guid.NewGuid(),
				DC_Language = "EN",
				DC_Type = "SP1",
				DC_Index = "1",
				DC_Descriptor = "GG"
			} };
		}

		protected override bool UpdateData(object data)
		{
			((UNDGCommonData)data).DC_Descriptor = "SP2";
			return true;
		}
	}
}
