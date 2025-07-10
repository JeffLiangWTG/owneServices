using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal class BillOfLadingActionSupporter : AgencyShipmentActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencyDocumentation; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.AgencyBillOfLading;

		public override string PluralElementNoun
		{
			get { return Res.GetString("a0ff3855-7c60-4633-ac0b-943ca044ad1d", "bills of lading"); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("cb68fe4b-f48f-4956-a715-5f26754327fe", "bill of lading"); }
		}

		public override Type RootType
		{
			get { return typeof(BillOfLading); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.Shipping);
		}

		#region IInvoicingSecurityCheckpointProvider Members

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.AgencyBillOfLadingJobInvoicing; }
		}

		#endregion
	}
}
