using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusProfileType : IDataSetStorage
	{
		Guid XXX_PK { get; set; }
		string XXX_ProfileType { get; set; }
		Guid XXX_ZZI_TariffType { get; set; }
		string XXX_Description { get; set; }
		string XXX_ZZZ_NKDataGrouping { get; set; }

		IEnumerable<IRefCusProfile> RefCusProfiles { get; }
		IEnumerable<IRefCusProfileQuestion> RefCusProfileQuestions { get; }
	}
}
