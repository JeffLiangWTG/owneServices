using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IUNDGSubstanceRID : IDataSetStorage
	{
		Guid RID_PK { get; set; }
		bool RID_IsActive { get; set; }
		string RID_UNNO { get; set; }
		string RID_Variant { get; set; }
		string RID_PSN { get; set; }
		string RID_Class { get; set; }
		string RID_ClassificationCode { get; set; }
		string RID_PG { get; set; }
		string RID_Labels { get; set; }
		string RID_SpecialProvisions { get; set; }
		decimal RID_LQMaxAmt { get; set; }
		string RID_LQMaxAmtUQ { get; set; }
		decimal RID_LQ2MaxAmt { get; set; }
		string RID_LQ2MaxAmtUQ { get; set; }
		string RID_ExceptedQuantityCode { get; set; }
		string RID_PackIns { get; set; }
		string RID_IBCIns { get; set; }
		string RID_PackProv { get; set; }
		string RID_MixedPackProv { get; set; }
		string RID_BulkContainerTankIns { get; set; }
		string RID_BulkContainerTankProv { get; set; }
		string RID_TankCode { get; set; }
		string RID_TankSpecProv { get; set; }
		string RID_TransportCategory { get; set; }
		string RID_CarriagePackagesSpecialProv { get; set; }
		string RID_CarriageBulkSpecialProv { get; set; }
		string RID_ColisExpressCode { get; set; }
		string RID_HazardIDNumber { get; set; }
		string RID_CarriageLoadingSpecialProv { get; set; }
		string RID_MinCW1Version { get; set; }
		string RID_MaxCW1Version { get; set; }
	}
}
