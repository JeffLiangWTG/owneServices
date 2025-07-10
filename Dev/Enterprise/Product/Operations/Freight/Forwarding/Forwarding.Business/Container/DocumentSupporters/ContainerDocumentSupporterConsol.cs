using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class ContainerDocumentSupporterConsol : ContainerDocumentSupporter
	{
		public ContainerDocumentSupporterConsol(ForwardingContainer container)
			: base(container)
		{
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] {
				DataContext.CartageAdvice,
				DataContext.Service,
				DataContext.GenericFreightJob,
				DataContext.GenericFreightJobServices,
				DataContext.ForwardingConsol };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == DataContext.GenericFreightJobServices)
			{
				return Container.GetServiceWrappersForDocBuilder(Container.Consol, DataContext.GenericFreightJob);
			}
			else if (dataContext == DataContext.GenericFreightJob)
			{
				if (Container.Consol != null)
				{
					CommonConsol consol = (CommonConsol)Factory.Load<Enterprise.Integration.Forwarding.IForwardingConsol>(Container.Consol.PK);
					return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJobByContainerIfFCL, consol, Container);
				}
				return System.Array.Empty<DocumentWrapper>();
			}
			else if (dataContext == DataContext.ForwardingConsol)
			{
				if (Container.Consol != null)
				{
					DocumentCommonConsol docConsol = new DocumentCommonConsol(Container.Consol, Core.Constants.DataContext.ForwardingConsol);
					docConsol.ContainerToPrint = Container;

					DocumentWrapper consolWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingConsol, docConsol);
					return new DocumentWrapper[] { consolWrapper };
				}

				return System.Array.Empty<DocumentWrapper>();
			}
			else
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.Container, Container) };
			}
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.LocalTransport || contact == ContactType.ShippingLine)
			{
				var parentConsol = Container.Consol;
				if (parentConsol != null)
				{
					result = Container.Consol.DocumentSupporter.GetContactOrganisation(menuName, contact, direction);

					if (result == null)
					{
						result = GetFallBackContactOrgHeaderFromShipment(parentConsol, direction);
					}
				}
			}

			return result;
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ForwardingContainer; }
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case Core.Constants.DataContext.ForwardingConsol:
				case Core.Constants.DataContext.GenericFreightJob:
				case Core.Constants.DataContext.GenericFreightJobServices:
					{
						if (Container.Consol == null)
						{
							return Res.GetString("6471bbef-7ab4-408e-8896-37e21cbd540a",
								"This container is not associated with a consolidation.");
						}

						if (dataContextValue.DataContext == Core.Constants.DataContext.GenericFreightJobServices && !Container.Services.Any())
						{
							return Res.GetString("9a9ad08d-0df4-41f1-91ef-3f818dd6683a",
								"This container does not have any services.");
						}

						break;
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.CartageAdvice
				&& dataContext != Core.Constants.DataContext.Service
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#region Get Fall Back Org Contact From Shipment

		OrgHeaderContact GetFallBackContactOrgHeaderFromShipment(CommonConsol parentConsol, DocumentDirection direction)
		{
			var docsAndCartages = GetDocsAndCartageForConsol(parentConsol);
			if (!docsAndCartages.Any())
			{
				return null;
			}

			OrgHeader org = null;

			if (direction == DocumentDirection.ARV)
			{
				var possibleOrg = docsAndCartages.LastOrDefault().DeliveryCartageCo;

				if (parentConsol.IsFCL
					|| ((parentConsol.IsGroupage || parentConsol.IsBuyersConsol)
						&& docsAndCartages.All(x => x.DeliveryCartageCo == possibleOrg)))
				{
					org = possibleOrg;
				}
			}

			if (direction == DocumentDirection.DEP)
			{
				var possibleOrg = docsAndCartages.LastOrDefault().PickupCartageCo;

				if (parentConsol.IsFCL
					|| ((parentConsol.IsGroupage || parentConsol.IsBuyersConsol)
						&& docsAndCartages.All(x => x.PickupCartageCo == possibleOrg)))
				{
					org = possibleOrg;
				}
			}

			return org != null ? new OrgHeaderContact(org, null) : null;
		}

		IEnumerable<JobDocsAndCartage> GetDocsAndCartageForConsol(CommonConsol consol)
		{
			foreach (CommonShipment shipment in consol.Shipments)
			{
				if (shipment.Containers.Contains(Container))
				{
					yield return shipment.DocsAndCartage;
				}
			}
		}

		#endregion

		#region Branding

		public override IOrgHeader GetBrandedOrganisation(IContactType contactType, DocumentDirection direction)
		{
			IOrgHeader result = null;
			ContactType concreteContactType = contactType as ContactType;

			if (Container.Consol != null && concreteContactType != null && concreteContactType.BrandingType == ContactBrandingType.Agent)
			{
				result = (direction == DocumentDirection.DEP) ? Container.Consol.SendingForwarder : Container.Consol.ReceivingForwarder;
			}

			return result;
		}

		#endregion
	}
}
