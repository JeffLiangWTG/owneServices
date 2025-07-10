using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGSubstanceRIDVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var substance = new UNDGSubstanceRID()
			{
				RID_PK = Guid.NewGuid(),
				RID_BulkContainerTankIns = "",
				RID_BulkContainerTankProv = "",
				RID_CarriageBulkSpecialProv = "",
				RID_CarriagePackagesSpecialProv = "",
				RID_Class = "",
				RID_ClassificationCode = "",
				RID_ColisExpressCode = "",
				RID_ExceptedQuantityCode = "",
				RID_HazardIDNumber = "",
				RID_IBCIns = "",
				RID_IsActive = true,
				RID_Labels = "",
				RID_LQ2MaxAmt = 0,
				RID_LQ2MaxAmtUQ = "",
				RID_LQMaxAmt = 0,
				RID_LQMaxAmtUQ = "",
				RID_MixedPackProv = "",
				RID_PackIns = "",
				RID_PackProv = "",
				RID_PG = "",
				RID_PSN = "ORGANOTIN PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				RID_SpecialProvisions = "",
				RID_TankCode = "",
				RID_TankSpecProv = "",
				RID_TransportCategory = "",
				RID_UNNO = "3019",
				RID_Variant = "c",
				RID_CarriageLoadingSpecialProv = ""
			};
			result.Add(substance);
			result.Add(new UNDGAttributeZZ()
			{
				DAZ_ParentPK = substance.RID_PK,
				DAZ_ParentCode = "RID",
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
			if (data is UNDGSubstanceRID substance)
			{
				substance.RID_PSN = "XX";
			}
			if (data is UNDGAttributeZZ attr)
			{
				attr.DAZ_Descriptor = "XX";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new UNDGSubstanceRID()
			{
				RID_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				RID_BulkContainerTankIns = "",
				RID_BulkContainerTankProv = "",
				RID_CarriageBulkSpecialProv = "",
				RID_CarriagePackagesSpecialProv = "",
				RID_Class = "",
				RID_ClassificationCode = "",
				RID_ColisExpressCode = "",
				RID_ExceptedQuantityCode = "",
				RID_HazardIDNumber = "",
				RID_IBCIns = "",
				RID_IsActive = true,
				RID_Labels = "",
				RID_LQ2MaxAmt = 0,
				RID_LQ2MaxAmtUQ = "",
				RID_LQMaxAmt = 0,
				RID_LQMaxAmtUQ = "",
				RID_MixedPackProv = "",
				RID_PackIns = "",
				RID_PackProv = "",
				RID_PG = "",
				RID_PSN = "BBBB",
				RID_SpecialProvisions = "",
				RID_TankCode = "",
				RID_TankSpecProv = "",
				RID_TransportCategory = "",
				RID_UNNO = "1111",
				RID_Variant = "c",
				RID_CarriageLoadingSpecialProv = ""
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
