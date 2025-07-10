using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSContainerDocumentSupporter : ContainerDocumentSupporter
	{
		public CFSContainerDocumentSupporter(CFSContainer cfsContainer)
			: base(cfsContainer)
		{
		}

		protected CFSContainer ContainerRego
		{
			get { return (CFSContainer)BusinessObject; }
		}

		#region Overrides

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			ZString result = ZString.Empty;
			switch (filterType)
			{
				case MenuTemplateFilterType.IMP:
					if (ContainerRego.ContainsImportShipments)
					{
						result = "PRINT";
					}

					break;

				case MenuTemplateFilterType.OFW:
					if (ContainerRego.ContainsOnForwardingShipments)
					{
						result = "PRINT";
					}

					break;

				case MenuTemplateFilterType.TRN:
					if (ContainerRego.ContainsTranshipments)
					{
						result = "PRINT";
					}

					break;
			}
			return result;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CFSContainerRegistrationCustomiseDocuments; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
			{
				Core.Constants.DataContext.PackUnpackContainerRego,
				Core.Constants.DataContext.CartageAdvice,
				Core.Constants.DataContext.ERA,
				Core.Constants.DataContext.RequestForService,
				Core.Constants.DataContext.Service,
				Core.Constants.DataContext.GenericFreightJob,
				Core.Constants.DataContext.GenericFreightJobServices
			};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CFSContainerRego; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.GenericFreightJobServices)
			{
				return ContainerRego.GetServiceWrappersForDocBuilder(((IHaveServices)ContainerRego).ServiceParent, Core.Constants.DataContext.GenericFreightJob);
			}

			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, ContainerRego);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			if (dataContext == Core.Constants.DataContext.PackUnpackContainerRego ||
				dataContext == Core.Constants.DataContext.CartageAdvice ||
				dataContext == Core.Constants.DataContext.ERA ||
				dataContext == Core.Constants.DataContext.Service)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.PackUnpackContainerRego, ContainerRego) };
			}
			else if (dataContext == Core.Constants.DataContext.RequestForService)
			{
				return ContainerRego.GetServiceWrappers(Core.Constants.DataContext.PackUnpackContainerRego);
			}

			return null;
		}

		public override string TransportMode
		{
			get { return ContainerRego.TransportMode; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType type, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (type == ContactType.LocalTransport)
			{
				if (ContainerRego.CFSArrival != null && ContainerRego.CFSArrival.TransportCo != null)
				{
					result = new OrgHeaderContact(ContainerRego.CFSArrival.TransportCo, ContainerRego.CFSArrival.TransportProvider);
				}
				else if (ContainerRego.Consol != null)
				{
					result = new OrgHeaderContact(ContainerRego.Consol.CartageCo, ContainerRego.Consol.CartageCoAddress);
				}
			}
			else if (type == ContactType.ImportFreightAgent)
			{
				result = new OrgHeaderContact(ContainerRego.CFSClient, null);
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Constants.DataContext.GenericFreightJob
				&& dataContext != Constants.DataContext.GenericFreightJobServices
				&& dataContext != Constants.DataContext.PackUnpackContainerRego
				&& dataContext != Constants.DataContext.CartageAdvice
				&& dataContext != Constants.DataContext.ERA
				&& dataContext != Constants.DataContext.Service
				&& dataContext != Constants.DataContext.RequestForService
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion
	}
}
