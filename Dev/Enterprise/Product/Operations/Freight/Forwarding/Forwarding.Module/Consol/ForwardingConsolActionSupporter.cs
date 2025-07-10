using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.Module
{
	class ForwardingConsolActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Consol; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.MaintainConsol;

		public override string PluralElementNoun
		{
			get { return Res.GetString("Forwarding|ConsolActionSupporter|PluralElementNoun", "Consols"); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("Forwarding|ConsolActionSupporter|SingularElementNoun", "Consol"); }
		}

		public override Type RootType
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.AWB);
			list.Add(ActionMethodProviderIDs.Accounting);
			list.Add(ActionMethodProviderIDs.PRAMessage);
			list.Add(ActionMethodProviderIDs.SendAdvancedAirCargoReport);
			list.Add(ActionMethodProviderIDs.DtbBookingParent);
		}

		#region IInvoicingSecurityCheckpointProvider Members

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.MaintainConsolJobInvoicing; }
		}

		#endregion
	}
}
