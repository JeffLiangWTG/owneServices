using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class UNDGSubstanceCFRVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var substance = new UNDGSubstanceCFR()
			{
				CFR_PK = Guid.NewGuid(),
				CFR_PSN = "ORGANOTIN PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				CFR_SpecialProvisions = "",
				CFR_UNNO = "3019",
				CFR_Variant = "c",
				CFR_CargoAirRailLimit = 0,
				CFR_AppliesForAirTransport = false,
				CFR_AppliesForDomesticTransport = false,
				CFR_AppliesForInternationalTransport = false,
				CFR_AppliesForVesselTransport = false,
				CFR_BulkPackingInstructions = "",
				CFR_BulkPackingProvisions = "",
				CFR_CargoAirRailLimitUnit = "",
				CFR_CVL = "",
				CFR_EmergencyResponseGuide = "",
				CFR_ExceptedQuantity = "",
				CFR_GeneralStowage = "",
				CFR_IBCInstructions = "",
				CFR_IBCProvisions = "",
				CFR_CargoAirRailLimitType = "",
				CFR_IsFixedPSN = false,
				CFR_PAXAirRailLimitType = "",
				CFR_LimitedQuantityPermitted = false,
				CFR_MarinePollutant = "",
				CFR_PackingExceptions = "",
				CFR_PackingGroup = "",
				CFR_PackingInstructions = "",
				CFR_PackingProvisions = "",
				CFR_PassengerStowage = "",
				CFR_PAXAirRailLimit = 0,
				CFR_PAXAirRailLimitUnit = "",
				CFR_PoisonInhalationHazard = "",
				CFR_Prefix = "",
				CFR_PrimaryClass = "",
				CFR_ReportableQuantity = 0,
				CFR_ReportableQuantityUnit = "",
				CFR_RequiresTechnicalNameInParenthesis = true,
				CFR_SecondaryClass = "",
				CFR_State = "",
				CFR_StowageCategory = "",
				CFR_StowageCodes = "",
				CFR_StowageIMDGCodes = "",
				CFR_TankInstructions = "",
				CFR_TankProvisions = "",
				CFR_TechnicalName = "",
				CFR_TertiaryClass = "",
				CFR_TreatAs = "",
				CFR_Variation = new string('a', 150),
				CFR_LQMaxAmt = 0,
				CFR_LQMaxAmtUQ = "",
				CFR_IsActive = true,
				CFR_SecondaryPAXAirRailLimit = 0,
				CFR_SecondaryPAXAirRailLimitUnit = "",
				CFR_SecondaryCargoAirRailLimit = 0,
				CFR_SecondaryCargoAirRailLimitUnit = ""
			};
			result.Add(substance);
			result.Add(new UNDGAttributeZZ()
			{
				DAZ_ParentPK = substance.CFR_PK,
				DAZ_ParentCode = "CFR",
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
			if (data is UNDGSubstanceCFR substance)
			{
				substance.CFR_PSN = "CFR";
			}
			else if (data is UNDGAttributeZZ attribute)
			{
				attribute.DAZ_Descriptor = "attr";
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			return new object[] { new UNDGSubstanceCFR()
			{
				CFR_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6"),
				CFR_PSN = "BBBB",
				CFR_SpecialProvisions = "",
				CFR_UNNO = "1111",
				CFR_Variant = "c",
				CFR_CargoAirRailLimit = 0,
				CFR_AppliesForAirTransport = false,
				CFR_AppliesForDomesticTransport = false,
				CFR_AppliesForInternationalTransport = false,
				CFR_AppliesForVesselTransport = false,
				CFR_BulkPackingInstructions = "",
				CFR_BulkPackingProvisions = "",
				CFR_CargoAirRailLimitUnit = "",
				CFR_CVL = "",
				CFR_EmergencyResponseGuide = "",
				CFR_ExceptedQuantity = "",
				CFR_GeneralStowage = "",
				CFR_IBCInstructions = "",
				CFR_IBCProvisions = "",
				CFR_CargoAirRailLimitType = "",
				CFR_IsFixedPSN = false,
				CFR_PAXAirRailLimitType = "",
				CFR_LimitedQuantityPermitted = false,
				CFR_MarinePollutant = "",
				CFR_PackingExceptions = "",
				CFR_PackingGroup = "",
				CFR_PackingInstructions = "",
				CFR_PackingProvisions = "",
				CFR_PassengerStowage = "",
				CFR_PAXAirRailLimit = 0,
				CFR_PAXAirRailLimitUnit = "",
				CFR_PoisonInhalationHazard = "",
				CFR_Prefix = "",
				CFR_PrimaryClass = "",
				CFR_ReportableQuantity = 0,
				CFR_ReportableQuantityUnit = "",
				CFR_RequiresTechnicalNameInParenthesis = true,
				CFR_SecondaryClass = "",
				CFR_State = "",
				CFR_StowageCategory = "",
				CFR_StowageCodes = "",
				CFR_StowageIMDGCodes = "",
				CFR_TankInstructions = "",
				CFR_TankProvisions = "",
				CFR_TechnicalName = "",
				CFR_TertiaryClass = "",
				CFR_TreatAs = "",
				CFR_Variation = new string('a', 150),
				CFR_LQMaxAmt = 0,
				CFR_LQMaxAmtUQ = "",
				CFR_IsActive = true,
				CFR_SecondaryPAXAirRailLimit = 0,
				CFR_SecondaryPAXAirRailLimitUnit = "",
				CFR_SecondaryCargoAirRailLimit = 0,
				CFR_SecondaryCargoAirRailLimitUnit = ""
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
