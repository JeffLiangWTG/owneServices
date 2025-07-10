using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public sealed class MasterFilesOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		#region NewMethods

		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			var result = new List<OperationalActionMethod>();
			if (typeof(OrgSupplierPart).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new MarkProductsAsBarcodedMethod());
				result.Add(new UnmarkProductsAsBarcodedMethod());
				result.Add(new AssignCartonGroupMethod());
				result.Add(new AssignWhsPutawayGroupMethod());
				result.Add(new UpdateExpiryNotificationPeriodMethod());
				result.Add(new UpdateDynamicPickFaceAreaMethod());
				result.Add(new AuditClassificationLinesMethod());
			}

			return (result.Count > 0) ? result.ToArray() : null;
		}

		#endregion
	}
}
