using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDataRepo.Ent.Client.DataStorage;

public partial interface IZZRefCusCodeListAttributeCombined : IDataSetStorage
{
	Guid ZZE_PK { get; set; }
	Guid ZZE_ZZD_CodeList { get; set; }
	string ZZE_ZXE_NKName { get; set; }
	string ZZE_Value { get; set; }
	DateTime ZZE_StartDate { get; set; }
	DateTime ZZE_EndDate { get; set; }
	bool? ZZE_IsAir { get; set; }
	bool? ZZE_IsSea { get; set; }
	bool? ZZE_IsFix { get; set; }
	bool? ZZE_IsRai { get; set; }
	bool? ZZE_IsRoa { get; set; }
	bool? ZZE_IsMai { get; set; }
	bool? ZZE_IsInw { get; set; }
}
