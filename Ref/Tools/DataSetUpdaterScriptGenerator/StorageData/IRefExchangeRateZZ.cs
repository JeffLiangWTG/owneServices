using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefExchangeRateZZ : IDataSetStorage
	{
		Guid ZZN_PK { get; set; }
		string ZZN_ExRateType { get; set; }
		DateTime ZZN_StartDate { get; set; }
		DateTime ZZN_EndDate { get; set; }
		decimal ZZN_Rate { get; set; }
		string ZZN_RX_NKExCurrency { get; set; }
		string ZZN_RN_NKCountry { get; set; }
		string ZZN_AsPublished { get; set; }
	}
}
