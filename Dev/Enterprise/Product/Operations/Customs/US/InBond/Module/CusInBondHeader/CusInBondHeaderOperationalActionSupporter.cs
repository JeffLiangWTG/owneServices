using System;
using CargoWise.Definitions;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.InBond.Module
{
	public class CusInBondHeaderOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(CusInBondHeader);

		public override BusinessContext BusinessContext => BusinessContext.CusInBondHeader;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.USInBond;

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.USInBond);
		}
	}
}
