using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.LocalCartage.Module
{
	internal class CartageOperationalActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Cartage; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.TransportJob;

		public override string SingularElementNoun
		{
			get { return Res.GetString("LocalCartage|CartageOperationalActionSupporter|SingularElementNoun", "Transport Job"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("LocalCartage|CartageOperationalActionSupporter|ElementNoun", "Transport Jobs"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}

		public override Type RootType
		{
			get { return typeof(CommonCartage); }
		}

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.TransportJobInvoicing; }
		}
	}
}
