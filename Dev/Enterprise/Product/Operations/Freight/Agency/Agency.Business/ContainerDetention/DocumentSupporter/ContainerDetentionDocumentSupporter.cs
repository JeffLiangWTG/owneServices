using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class ContainerDetentionDocumentSupporter : DocumentSupporter
	{
		public ContainerDetentionDocumentSupporter(ContainerDetention detention)
			: base(detention) { }

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DetentionInvoice; }
		}

		public override ClientAndAgentBrandingBusinessObject GetAlternativeBranding()
		{
			return DocumentsDataRegistry.Instance.PrincipalDocumentBrand.FindBrandingForPrincipal(Detention.NC_OH_Principal);
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.Receivables)
			{
				return new OrgHeaderContact(Detention.Client, null);
			}
			else
			{
				return base.GetContactOrganisation(menuName, contact, direction);
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Detention) ?? (new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(DataContext.DetentionInvoice, Detention) });

			return result;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[]
			{
				DataContext.GenericFreightJob,
				DataContext.DetentionInvoice
			};
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != DataContext.GenericFreightJob
				&& dataContext != DataContext.DetentionInvoice
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#region Implementation

		ContainerDetention Detention
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ContainerDetention)BusinessObject; }
		}

		#endregion
	}
}


