using System;
using CargoWise.Definitions;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Module.OperationalActions
{
	public sealed class BaseJobDeclarationOperationalActionSupporter : OperationalActionSupporter, IInvoicingSecurityCheckpointProvider
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Customs; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.CustomsDeclarationEnquiry;

		public override Type RootType
		{
			get { return typeof(BaseJobDeclaration); }
		}

		public override string SingularElementNoun
		{
			get { return Res.GetString("38af1b1e-7ad0-477f-8d27-33ede9e4b93a", "declaration"); }
		}

		public override string PluralElementNoun
		{
			get { return Res.GetString("b512fa2f-0db2-46e0-9a7e-75424facadea", "declarations"); }
		}

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.GbJobDeclaration);
			list.Add(ActionMethodProviderIDs.GbPickupDropOff);
			list.Add(ActionMethodProviderIDs.USJobDeclaration);
			list.Add(ActionMethodProviderIDs.CAJobDeclaration);
			list.Add(ActionMethodProviderIDs.Accounting);
			list.Add(ActionMethodProviderIDs.JobDeclaration);
			list.Add(ActionMethodProviderIDs.FrJobDeclaration);
			list.Add(ActionMethodProviderIDs.TWJobDeclaration);
			list.Add(ActionMethodProviderIDs.EUJobDeclaration);
			// list.Add(Enterprise.Services.OperationalActions.Support.ActionMethodProviderIDs.YOURCOUNTRYHERE); // Ask Brian or Daniel for help
		}

		protected override bool SupportsBulkUpdatesCore
		{
			get { return true; }
		}

		#region IInvoicingSecurityCheckpointProvider Members

		SecurityCheckpoint IInvoicingSecurityCheckpointProvider.InvoicingCheckpoint
		{
			get { return Env.Security.CustomsDeclarationJobInvoicing; }
		}

		#endregion
	}
}
