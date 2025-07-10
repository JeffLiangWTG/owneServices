using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusAUNexdocECMCode : IDataSetStorage
	{
		Guid ZY5_PK { get; set; }
		string ZY5_CommodityCode { get; set; }
		string ZY5_PreservationCode { get; set; }
		string ZY5_ProductTypeCode { get; set; }
		string ZY5_PackTypeCode { get; set; }
		string ZY5_SupplementaryCode { get; set; }
	}
}
