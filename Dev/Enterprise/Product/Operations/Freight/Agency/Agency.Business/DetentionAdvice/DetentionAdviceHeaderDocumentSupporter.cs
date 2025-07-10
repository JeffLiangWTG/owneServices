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
	internal class DetentionAdviceHeaderDocumentSupporter : DocumentSupporter
	{
		public DetentionAdviceHeaderDocumentSupporter(DetentionAdviceHeader advice)
			: base(advice) { }

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencyDtnAdvice; }
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.AgencyDetentionAdvice, DataContext.GenericFreightJob };
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == DataContext.AgencyDetentionAdvice)
			{
				return new DocumentWrapper[]
				{
					DocumentWrapperFactory.CreateWrapper(DataContext.AgencyDetentionAdvice, Parent)
				};
			}
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Parent);
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.ImportFreightAgent)
			{
				return new OrgHeaderContact(Parent.Client, null);
			}
			else
			{
				return base.GetContactOrganisation(menuName, contact, direction);
			}
		}

		#region Implementation

		DetentionAdviceHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DetentionAdviceHeader)base.BusinessObject; }
		}

		#endregion
	}
}


