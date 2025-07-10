using System.Collections.Generic;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.GUI;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.LVS.Module
{
	public class LowValueEntriesBillActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			var result = new List<OperationalActionMethod>();

			if (typeof(USConsignmentCombined).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new ADDCVDAppliesActionMethod());
				result.Add(new ADDCVDDoesNotApplyActionMethod());
				result.Add(new DisclaimApplicablePGAsActionMethod());
			}

			return (result.Count > 0) ? result.ToArray() : null;
		}
	}
}
