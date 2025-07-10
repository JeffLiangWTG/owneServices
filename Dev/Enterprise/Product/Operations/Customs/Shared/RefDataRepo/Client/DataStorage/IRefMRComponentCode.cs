namespace CargoWise.RefDataRepo.Ent.Client.DataStorage
{
	using System;
	using CargoWise.RefDbRepo.Client.Common;
	public partial interface IRefMRComponentCode : IDataSetStorage
	{
		Guid RCC_PK { get; set; }
		bool RCC_IsActive { get; set; }
		string RCC_Code { get; set; }
		string RCC_Description { get; set; }
		string RCC_Group { get; set; }
		bool RCC_Machinery { get; set; }
		bool RCC_Structural { get; set; }
		bool RCC_TankCleaning { get; set; }
		bool RCC_TankRepair { get; set; }
		DateTime RCC_SystemCreateTimeUtc { get; set; }
		string RCC_SystemCreateUser { get; set; }
		DateTime RCC_SystemLastEditTimeUtc { get; set; }
		string RCC_SystemLastEditUser { get; set; }
	}
}
