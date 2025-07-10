using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefMessagingBussPackageInfo : IDataSetStorage
	{
		Guid ZMP_PK { get; set; }
		string ZMP_PackageName { get; set; }

		IEnumerable<IRefMessagingBussCarrierInfo> RefMessagingBussCarrierInfoes { get; }
		IEnumerable<IRefMessagingBussPackageVersion> RefMessagingBussPackageVersions { get; }
	}
}
