using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IUNDGSubstanceADR : IDataSetStorage
	{
		Guid ADR_PK { get; set; }
		bool ADR_IsActive { get; set; }
		string ADR_UNNO { get; set; }
		string ADR_Variant { get; set; }
		string ADR_PSN { get; set; }
		string ADR_Class { get; set; }
		string ADR_ClassificationCode { get; set; }
		string ADR_PG { get; set; }
		string ADR_Labels { get; set; }
		string ADR_SpecialProvisions { get; set; }
		decimal ADR_LQMaxAmt { get; set; }
		string ADR_LQMaxAmtUQ { get; set; }
		decimal ADR_LQ2MaxAmt { get; set; }
		string ADR_LQ2MaxAmtUQ { get; set; }
		string ADR_ExceptedQuantityCode { get; set; }
		string ADR_PackIns { get; set; }
		string ADR_PackProv { get; set; }
		string ADR_MixedPackingProv { get; set; }
		string ADR_BulkTankIns { get; set; }
		string ADR_BulkTankSpecProv { get; set; }
		string ADR_ADRTankCode { get; set; }
		string ADR_ADRTankSpecProv { get; set; }
		string ADR_TankVehicle { get; set; }
		string ADR_TransportCategory { get; set; }
		string ADR_PackingSpecialProv { get; set; }
		string ADR_BulkSpecialProv { get; set; }
		string ADR_LoadingSpecialProv { get; set; }
		string ADR_OperationSpecialProv { get; set; }
		string ADR_HazardIDNumber { get; set; }
		string ADR_MinCW1Version { get; set; }
		string ADR_MaxCW1Version { get; set; }
	}
}
