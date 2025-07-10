using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefAccElectronicProcessingFee : IDataSetStorage
	{
		Guid EPF_PK { get; set; }
		string EPF_SystemCode { get; set; }
		string EPF_Category { get; set; }
		string EPF_Code { get; set; }
		string EPF_Description { get; set; }
		string EPF_Currency { get; set; }
		decimal EPF_Price { get; set; }
		DateTime EPF_ValidFrom { get; set; }
		string EPF_CountryCode { get; set; }
		string EPF_JobDirection { get; set; }
	}
}
