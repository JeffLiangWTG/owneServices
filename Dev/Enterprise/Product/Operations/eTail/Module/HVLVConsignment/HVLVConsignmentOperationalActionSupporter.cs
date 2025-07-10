using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.eTail.Module
{
	public class HVLVConsignmentOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(HVLVConsignment);

		public override BusinessContext BusinessContext => BusinessContext.HVLVConsignment;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.HVLVConsignment;

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			list.Add(ActionMethodProviderIDs.HVLV);
			base.PopulateMethods(list);
		}
	}
}
