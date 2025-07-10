using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusCodeOrAttributeTransportMode : IDataSetStorage
	{
		Guid ZZU_PK { get; set; }
		string ZZU_TransportMode { get; set; }
		Nullable<Guid> ZZU_ZZD_CodeList { get; set; }
		Nullable<Guid> ZZU_ZZE_Attribute { get; set; }
	}
}
