using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IUNDGSubstanceCFR : IDataSetStorage
	{
		Guid CFR_PK { get; set; }
		string CFR_UNNO { get; set; }
		string CFR_Variant { get; set; }
		string CFR_Prefix { get; set; }
		string CFR_CVL { get; set; }
		string CFR_PSN { get; set; }
		string CFR_Variation { get; set; }
		string CFR_PrimaryClass { get; set; }
		string CFR_SecondaryClass { get; set; }
		string CFR_TertiaryClass { get; set; }
		string CFR_MarinePollutant { get; set; }
		string CFR_ExceptedQuantity { get; set; }
		bool CFR_LimitedQuantityPermitted { get; set; }
		decimal CFR_ReportableQuantity { get; set; }
		string CFR_ReportableQuantityUnit { get; set; }
		string CFR_GeneralStowage { get; set; }
		string CFR_PassengerStowage { get; set; }
		string CFR_StowageCategory { get; set; }
		string CFR_StowageCodes { get; set; }
		string CFR_StowageIMDGCodes { get; set; }
		string CFR_BulkPackingInstructions { get; set; }
		string CFR_BulkPackingProvisions { get; set; }
		string CFR_IBCInstructions { get; set; }
		string CFR_IBCProvisions { get; set; }
		string CFR_PackingExceptions { get; set; }
		string CFR_PackingInstructions { get; set; }
		string CFR_PackingProvisions { get; set; }
		string CFR_PackingGroup { get; set; }
		string CFR_SpecialProvisions { get; set; }
		string CFR_TankInstructions { get; set; }
		string CFR_TankProvisions { get; set; }
		string CFR_PoisonInhalationHazard { get; set; }
		string CFR_State { get; set; }
		bool CFR_IsFixedPSN { get; set; }
		bool CFR_AppliesForAirTransport { get; set; }
		bool CFR_AppliesForDomesticTransport { get; set; }
		bool CFR_AppliesForInternationalTransport { get; set; }
		bool CFR_AppliesForVesselTransport { get; set; }
		bool CFR_RequiresTechnicalNameInParenthesis { get; set; }
		string CFR_EmergencyResponseGuide { get; set; }
		string CFR_TechnicalName { get; set; }
		string CFR_TreatAs { get; set; }
		decimal CFR_PAXAirRailLimit { get; set; }
		string CFR_PAXAirRailLimitUnit { get; set; }
		decimal CFR_CargoAirRailLimit { get; set; }
		string CFR_CargoAirRailLimitUnit { get; set; }
		bool CFR_IsActive { get; set; }
		decimal CFR_LQMaxAmt { get; set; }
		string CFR_LQMaxAmtUQ { get; set; }
		string CFR_PAXAirRailLimitType { get; set; }
		string CFR_CargoAirRailLimitType { get; set; }
		decimal CFR_SecondaryPAXAirRailLimit { get; set; }
		string CFR_SecondaryPAXAirRailLimitUnit { get; set; }
		decimal CFR_SecondaryCargoAirRailLimit { get; set; }
		string CFR_SecondaryCargoAirRailLimitUnit { get; set; }
		string CFR_MinCW1Version { get; set; }
		string CFR_MaxCW1Version { get; set; }
	}
}
