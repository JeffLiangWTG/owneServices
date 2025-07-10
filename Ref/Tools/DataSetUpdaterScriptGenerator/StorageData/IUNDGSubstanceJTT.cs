using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IUNDGSubstanceJTT : IDataSetStorage
	{
		Guid JTT_PK { get; set; }
		bool JTT_IsActive { get; set; }
		string JTT_UNNO { get; set; }
		string JTT_Variant { get; set; }
		string JTT_PSN { get; set; }
		string JTT_Class { get; set; }
		string JTT_ClassificationCode { get; set; }
		string JTT_PG { get; set; }
		string JTT_Labels { get; set; }
		string JTT_SpecialProvisions { get; set; }
		decimal JTT_LQMaxAmt { get; set; }
		string JTT_LQMaxAmtUQ { get; set; }
		decimal JTT_LQ2MaxAmt { get; set; }
		string JTT_LQ2MaxAmtUQ { get; set; }
		string JTT_ExceptedQuantityCode { get; set; }
		string JTT_PackIns { get; set; }
		string JTT_PackProv { get; set; }
		string JTT_MixedPackingProv { get; set; }
		string JTT_BulkTankIns { get; set; }
		string JTT_BulkTankSpecProv { get; set; }
		string JTT_TankCode { get; set; }
		string JTT_TankSpecProv { get; set; }
		string JTT_TankVehicle { get; set; }
		string JTT_TransportCategory { get; set; }
		string JTT_PackingSpecialProv { get; set; }
		string JTT_BulkSpecialProv { get; set; }
		string JTT_LoadingSpecialProv { get; set; }
		string JTT_OperationSpecialProv { get; set; }
		string JTT_HazardIDNumber { get; set; }
		string JTT_MinCW1Version { get; set; }
		string JTT_MaxCW1Version { get; set; }
	}
}
