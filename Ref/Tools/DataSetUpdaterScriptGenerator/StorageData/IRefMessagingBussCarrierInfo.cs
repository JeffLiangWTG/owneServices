using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefMessagingBussCarrierInfo : IDataSetStorage
	{
		Guid ZMC_PK { get; set; }
		string ZMC_CarrierCode { get; set; }
		string ZMC_CarrierName { get; set; }
		string ZMC_CountryCode { get; set; }
		Guid ZMC_ZMP_PackageInfo { get; set; }
	}
}
