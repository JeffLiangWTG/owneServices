using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal class ContainerManagerActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencyContainerMgr; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AgencyContainerManager;

		public override Type RootType
		{
			get { return typeof(RefContainerStock); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("1f113dad-cb5b-4b1d-89af-41233728bfbf", "container"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("e56a99c0-b9bf-4ec8-9cdf-9e47926e0ff6", "containers"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Shipping);
		}
	}
}


