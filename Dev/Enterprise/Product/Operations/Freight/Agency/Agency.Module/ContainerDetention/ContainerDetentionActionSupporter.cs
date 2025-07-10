using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal sealed class ContainerDetentionActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DetentionInvoice; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AgencyContainerDetention;

		public override Type RootType
		{
			get { return typeof(ContainerDetention); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("{78BD32D4-17A0-4e83-974F-5DD2949A8A48}", "detention job"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("{0B924B63-5523-4912-B50A-F24F95D86B28}", "detention jobs"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Accounting);
		}

		#region IInvoicingSecurityCheckpointProvider Members

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.AgencyContainerDetentionJobInvoicing; }
		}

		#endregion
	}
}


