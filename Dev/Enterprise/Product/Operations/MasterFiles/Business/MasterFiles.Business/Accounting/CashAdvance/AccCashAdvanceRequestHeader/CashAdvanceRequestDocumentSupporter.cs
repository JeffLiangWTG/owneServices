using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class CashAdvanceRequestDocumentSupporter : DocumentSupporter
	{
		public CashAdvanceRequestDocumentSupporter(AccCashAdvanceRequestHeader cashAdvanceRequestHeader)
			: base(cashAdvanceRequestHeader)
		{
		}

		protected AccCashAdvanceRequestHeader CashAdvanceRequestHeader
		{
			get { return (AccCashAdvanceRequestHeader)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.CashAdvanceRequest, Core.Constants.DataContext.GenericFreightJob };
		}

		public override CargoWise.Definitions.BusinessContext BusinessContext
		{
			get { return CargoWise.Definitions.BusinessContext.CashAdvanceRequest; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, CashAdvanceRequestHeader);
			}
			else if (dataContext == Core.Constants.DataContext.CashAdvanceRequest)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CashAdvanceRequest, CashAdvanceRequestHeader) };
			}
			return null;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(CashAdvanceRequestHeader.Organization, null);
		}

		#endregion
	}
}
