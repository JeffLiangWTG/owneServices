using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefMessagingBussPackageVersion : IDataSetStorage
	{
		Guid ZMV_PK { get; set; }
		string ZMV_Version { get; set; }
		Guid ZMV_ZMP_PackageInfo { get; set; }
	}
}
