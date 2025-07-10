using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGSubstanceVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var substance = new UNDGSubstance();
			substance.SetDefaultValues();
			substance.DG_PK = Guid.NewGuid();
			substance.DG_UNNO = "3019";
			substance.DG_Variant = "c";
			substance.DG_PSN = "ORGANOTIN PESTICIDE, LIQUID, TOXIC, FLAMMABLE";
			substance.DG_Standard = "IMO";
			result.Add(substance);
			result.Add(new UNDGAttribute
			{
				DA_PK = Guid.NewGuid(),
				DA_Type = "XXX",
				DA_Index = "1",
				DA_Language = "EN",
				DA_Descriptor = "Irritating to skin, eyes and mucous membranes.",
				DA_DG = substance.DG_PK
			});
			result.Add(new UNDGReference
			{
				DR_PK = Guid.NewGuid(),
				DR_Type = "YYY",
				DR_Code = "111",
				DR_RN_NKCountry = "SG",
				DR_Description = "Desc",
				DR_FlashPointLower = (decimal)1.1,
				DR_HasFlashPointLower = true,
				DR_FlashPointUpper = (decimal)2.2,
				DR_HasFlashPointUpper = true,
				DR_DG = substance.DG_PK
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is UNDGSubstance substance)
			{
				substance.DG_PSN = "XX";
			}
			if (data is UNDGAttribute attr)
			{
				attr.DA_Descriptor = "GG";
			}
			if (data is UNDGReference refer)
			{
				refer.DR_Description = "HH";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			var substance = new UNDGSubstance();
			substance.SetDefaultValues();
			substance.DG_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
			substance.DG_UNNO = "3333";
			substance.DG_Variant = "c";
			substance.DG_PSN = "BBBB";
			substance.DG_Standard = "IMO";
			return new object[] { substance };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is UNDGAttribute attr)
			{
				attr.DA_DG = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			if (data is UNDGReference reference)
			{
				reference.DR_DG = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
				return true;
			}
			return false;
		}
	}
}
