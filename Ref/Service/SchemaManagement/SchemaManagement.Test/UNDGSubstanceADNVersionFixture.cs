using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGSubstanceADNVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var substance = new UNDGSubstanceADN()
			{
				ADN_PK = Guid.NewGuid(),
				ADN_BlueCones = 1,
				ADN_CarriagePermittedBulk = false,
				ADN_Class = "",
				ADN_ClassificationCode = "",
				ADN_ExceptedQuantityCode = "",
				ADN_Labels = "",
				ADN_LQ2MaxAmt = 0,
				ADN_LQ2MaxAmtUQ = "",
				ADN_LQMaxAmt = 0,
				ADN_LQMaxAmtUQ = "",
				ADN_PG = "",
				ADN_PSN = "ORGANOTIN PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				ADN_SpecialProvisions = "",
				ADN_UNNO = "3019",
				ADN_Variant = "c",
				ADN_CarriagePermittedDetails = "",
				ADN_CarriagePermittedPacks = false,
				ADN_CarriagePermittedTanks = false,
				ADN_EquipBreathingApparatus = false,
				ADN_EquipEscapeDevice = false,
				ADN_EquipGasDetector = false,
				ADN_EquipmentDetails = "",
				ADN_EquipPPE = false,
				ADN_EquipToximeter = false,
				ADN_LoadingSpecialProv = "",
				ADN_LoadingSpecialProvNote = "",
				ADN_OperationSpecialProv = "",
				ADN_OperationSpecialProvNote = "",
				ADN_UnloadingSpecialProv = "",
				ADN_UnloadingSpecialProvNote = "",
				ADN_Ventilation = "",
				ADN_IsActive = true
			};
			result.Add(substance);
			result.Add(new UNDGAttributeZZ()
			{
				DAZ_ParentPK = substance.ADN_PK,
				DAZ_ParentCode = "ADN",
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
			if (data is UNDGSubstanceADN substance)
			{
				substance.ADN_PSN = "XX";
			}
			if (data is UNDGAttributeZZ attr)
			{
				attr.DAZ_Descriptor = "XX";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new UNDGSubstanceADN()
			{
				ADN_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				ADN_BlueCones = 1,
				ADN_CarriagePermittedBulk = false,
				ADN_Class = "",
				ADN_ClassificationCode = "",
				ADN_ExceptedQuantityCode = "",
				ADN_Labels = "",
				ADN_LQ2MaxAmt = 0,
				ADN_LQ2MaxAmtUQ = "",
				ADN_LQMaxAmt = 0,
				ADN_LQMaxAmtUQ = "",
				ADN_PG = "",
				ADN_PSN = "BBB",
				ADN_SpecialProvisions = "",
				ADN_UNNO = "111",
				ADN_Variant = "c",
				ADN_CarriagePermittedDetails = "",
				ADN_CarriagePermittedPacks = false,
				ADN_CarriagePermittedTanks = false,
				ADN_EquipBreathingApparatus = false,
				ADN_EquipEscapeDevice = false,
				ADN_EquipGasDetector = false,
				ADN_EquipmentDetails = "",
				ADN_EquipPPE = false,
				ADN_EquipToximeter = false,
				ADN_LoadingSpecialProv = "",
				ADN_LoadingSpecialProvNote = "",
				ADN_OperationSpecialProv = "",
				ADN_OperationSpecialProvNote = "",
				ADN_UnloadingSpecialProv = "",
				ADN_UnloadingSpecialProvNote = "",
				ADN_Ventilation = "",
				ADN_IsActive = true
			} };
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
