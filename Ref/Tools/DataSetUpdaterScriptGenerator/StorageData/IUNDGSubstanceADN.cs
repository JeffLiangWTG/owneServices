using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IUNDGSubstanceADN : IDataSetStorage
	{
		Guid ADN_PK { get; set; }
		bool ADN_IsActive { get; set; }
		string ADN_UNNO { get; set; }
		string ADN_Variant { get; set; }
		string ADN_PSN { get; set; }
		string ADN_Class { get; set; }
		string ADN_ClassificationCode { get; set; }
		string ADN_PG { get; set; }
		string ADN_Labels { get; set; }
		string ADN_SpecialProvisions { get; set; }
		string ADN_ExceptedQuantityCode { get; set; }
		decimal ADN_LQMaxAmt { get; set; }
		string ADN_LQMaxAmtUQ { get; set; }
		decimal ADN_LQ2MaxAmt { get; set; }
		string ADN_LQ2MaxAmtUQ { get; set; }
		bool ADN_CarriagePermittedPacks { get; set; }
		bool ADN_CarriagePermittedBulk { get; set; }
		bool ADN_CarriagePermittedTanks { get; set; }
		string ADN_CarriagePermittedDetails { get; set; }
		bool ADN_EquipPPE { get; set; }
		bool ADN_EquipEscapeDevice { get; set; }
		bool ADN_EquipGasDetector { get; set; }
		bool ADN_EquipToximeter { get; set; }
		bool ADN_EquipBreathingApparatus { get; set; }
		string ADN_EquipmentDetails { get; set; }
		string ADN_Ventilation { get; set; }
		string ADN_LoadingSpecialProv { get; set; }
		string ADN_LoadingSpecialProvNote { get; set; }
		string ADN_UnloadingSpecialProv { get; set; }
		string ADN_UnloadingSpecialProvNote { get; set; }
		string ADN_OperationSpecialProv { get; set; }
		string ADN_OperationSpecialProvNote { get; set; }
		byte ADN_BlueCones { get; set; }
		string ADN_MinCW1Version { get; set; }
		string ADN_MaxCW1Version { get; set; }
	}
}
