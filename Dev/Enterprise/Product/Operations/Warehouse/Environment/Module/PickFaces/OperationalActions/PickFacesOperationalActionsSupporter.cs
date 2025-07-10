using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Environment.Module
{
	public class PickFacesOperationalActionsSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.WhsPickFace;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsConfigPickFaces;

		public override Type RootType => typeof(WhsPickFaceView);

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Warehouse);
		}
	}
}
