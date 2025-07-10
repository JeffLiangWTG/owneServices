using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGSubstanceJTTVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var substance = new UNDGSubstanceJTT()
			{
				JTT_PK = Guid.NewGuid(),
				JTT_IsActive = true,
				JTT_UNNO = "3019",
				JTT_Variant = "c",
				JTT_PSN = new string('T', 280),
				JTT_Class = "",
				JTT_ClassificationCode = "",
				JTT_PG = "",
				JTT_Labels = "",
				JTT_SpecialProvisions = "",
				JTT_LQMaxAmt = 0,
				JTT_LQMaxAmtUQ = "",
				JTT_LQ2MaxAmt = 0,
				JTT_LQ2MaxAmtUQ = "",
				JTT_ExceptedQuantityCode = "",
				JTT_PackIns = "",
				JTT_PackProv = "",
				JTT_MixedPackingProv = "",
				JTT_BulkTankIns = "",
				JTT_BulkTankSpecProv = "",
				JTT_TankCode = "",
				JTT_TankSpecProv = "",
				JTT_TankVehicle = "",
				JTT_TransportCategory = "",
				JTT_PackingSpecialProv = "",
				JTT_BulkSpecialProv = "",
				JTT_LoadingSpecialProv = "",
				JTT_OperationSpecialProv = "",
				JTT_HazardIDNumber = "",
			};
			result.Add(substance);
			result.Add(new UNDGAttributeZZ()
			{
				DAZ_ParentPK = substance.JTT_PK,
				DAZ_ParentCode = "JTT",
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
			if (data is UNDGSubstanceJTT substance)
			{
				substance.JTT_PSN = "XX";
			}
			if (data is UNDGAttributeZZ attr)
			{
				attr.DAZ_Descriptor = "GG";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new UNDGSubstanceJTT()
			{
				JTT_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				JTT_IsActive = true,
				JTT_UNNO = "1111",
				JTT_Variant = "c",
				JTT_PSN = new string('B', 290),
				JTT_Class = "",
				JTT_ClassificationCode = "",
				JTT_PG = "",
				JTT_Labels = "",
				JTT_SpecialProvisions = "",
				JTT_LQMaxAmt = 0,
				JTT_LQMaxAmtUQ = "",
				JTT_LQ2MaxAmt = 0,
				JTT_LQ2MaxAmtUQ = "",
				JTT_ExceptedQuantityCode = "",
				JTT_PackIns = "",
				JTT_PackProv = "",
				JTT_MixedPackingProv = "",
				JTT_BulkTankIns = "",
				JTT_BulkTankSpecProv = "",
				JTT_TankCode = "",
				JTT_TankSpecProv = "",
				JTT_TankVehicle = "",
				JTT_TransportCategory = "",
				JTT_PackingSpecialProv = "",
				JTT_BulkSpecialProv = "",
				JTT_LoadingSpecialProv = "",
				JTT_OperationSpecialProv = "",
				JTT_HazardIDNumber = "",
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
