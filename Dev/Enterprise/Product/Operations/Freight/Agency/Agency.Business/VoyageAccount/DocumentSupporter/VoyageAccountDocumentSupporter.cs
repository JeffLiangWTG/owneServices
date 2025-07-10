using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Agency.Business
{
	public class VoyageAccountDocumentSupporter : DocumentSupporter
	{
		public VoyageAccountDocumentSupporter(VoyageAccount account)
			: base(account) { }

		protected VoyageAccount VoyageAccount
		{
			get { return (VoyageAccount)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencyVoyageAccount; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, VoyageAccount);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			switch (dataContext)
			{
				case DataContext.AgencyVoyageAccount:
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(DataContext.AgencyVoyageAccount, Account) };

				default:
					return null;
			}
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.AgencyVoyageAccount, DataContext.GenericFreightJob };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			if (contactType == ContactType.ShippingLine)
			{
				return new OrgHeaderContact(Account.Header, null);
			}
			else
			{
				return base.GetContactOrganisation(menuName, contactType, direction);
			}
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != DataContext.GenericFreightJob
				&& dataContext != DataContext.AgencyVoyageAccount
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#region Implementation

		VoyageAccount Account
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (VoyageAccount)base.BusinessObject; }
		}

		#endregion
	}
}


