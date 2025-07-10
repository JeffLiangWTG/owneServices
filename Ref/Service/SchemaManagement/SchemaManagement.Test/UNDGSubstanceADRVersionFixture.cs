using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGSubstanceADRVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var substance = new UNDGSubstanceADR();
			substance.SetDefaultValues();
			substance.ADR_PK = Guid.NewGuid();
			substance.ADR_UNNO = "3019";
			substance.ADR_Variant = "c";
			substance.ADR_PSN = "ORGANOTIN PESTICIDE, LIQUID, TOXIC, FLAMMABLE";
			result.Add(substance);
			result.Add(new UNDGAttributeZZ()
			{
				DAZ_ParentPK = substance.ADR_PK,
				DAZ_ParentCode = "ADR",
				DAZ_Descriptor = "AA",
				DAZ_Language = "EN",
				DAZ_Index = "A",
				DAZ_PK = Guid.NewGuid(),
				DAZ_Type = "A"
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is UNDGSubstanceADR substance)
			{
				substance.ADR_PSN = "XX";
			}
			if (data is UNDGAttributeZZ attr)
			{
				attr.DAZ_Descriptor = "GG";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			var substance = new UNDGSubstanceADR();
			substance.SetDefaultValues();
			substance.ADR_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
			substance.ADR_UNNO = "1111";
			substance.ADR_Variant = "c";
			substance.ADR_PSN = "BBBB";
			return new object[] { substance };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is UNDGAttributeZZ attr)
			{
				attr.DAZ_ParentPK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}
	}
}
