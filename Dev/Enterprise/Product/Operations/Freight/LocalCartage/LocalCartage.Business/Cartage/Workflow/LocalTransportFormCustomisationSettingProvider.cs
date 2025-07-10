using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class LocalTransportFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			var result = new List<string>();
			result.Add(JobCartageSchema.JJ_E3_NKJobType.Name);
			result.Add(JobCartageSchema.JJ_GB.Name);
			result.Add(JobCartageSchema.JJ_OH_ClientID.Name);
			return result.ToArray();
		}
	}
}
