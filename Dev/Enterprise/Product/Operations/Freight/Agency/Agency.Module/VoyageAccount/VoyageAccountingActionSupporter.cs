using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal class VoyageAccountingActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencyVoyageAccount; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AgencyVoyageAccount;

		public override Type RootType
		{
			get { return typeof(VoyageAccount); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("8ce8d095-98d6-4c25-bd58-38ac0d3fee68", "voyage account"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("3ed6dc72-4db3-4b15-8134-341902e9da9f", "voyage accounts"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}
	}
}


