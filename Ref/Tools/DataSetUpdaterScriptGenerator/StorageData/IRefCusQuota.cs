using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusQuota : IDataSetStorage
	{
		Guid ZXQ_PK { get; set; }
		string ZXQ_OrderNumber { get; set; }
		decimal ZXQ_InitialAmount { get; set; }
		string ZXQ_UnitOfMeasure { get; set; }
		decimal ZXQ_Balance { get; set; }
		DateTime ZXQ_StartDate { get; set; }
		DateTime ZXQ_EndDate { get; set; }
		string ZXQ_ZZZ_NKDataGrouping { get; set; }
	}
}
