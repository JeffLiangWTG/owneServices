using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal class BillContainersActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencyBillConatiner; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AgencyBillContainers;

		public override Type RootType
		{
			get { return typeof(BillOfLadingContainer); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("fd39fdc1-2920-4c8d-916f-bcf53c12d13f", "container"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("f5738ac1-df69-4156-9354-f4ee105786fd", "containers"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Shipping);
		}
	}
}


