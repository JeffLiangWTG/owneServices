using System;
using System.Collections.Generic;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.Service;

namespace Enterprise.ProductionRules.Business
{
	//This class is not in use for displaying the custom Fields on the Glow rule engine website for WI00700496,
	//it will be implemented in WI00808404 where The rule engine execution will be happening within CW1
	public class CW1CustomFieldLoader : CustomFieldLoader
	{
		protected override IReadOnlyDictionary<string, CustomFieldDetail> GetCustomFieldsFromDatabaseCore(string[] workflowProcessTypes)
		{
			throw new NotImplementedException();
		}
	}
}
