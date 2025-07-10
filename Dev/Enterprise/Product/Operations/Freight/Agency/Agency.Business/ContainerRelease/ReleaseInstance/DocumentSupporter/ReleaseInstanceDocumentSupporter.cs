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
	public class ReleaseInstanceDocumentSupporter : DocumentSupporter
	{
		public ReleaseInstanceDocumentSupporter(ReleaseInstance instance)
			: base(instance.Shipment)
		{
			this.instance = instance;
		}

		#region GetAlternativeBranding

		public override ClientAndAgentBrandingBusinessObject GetAlternativeBranding()
		{
			return DocumentsDataRegistry.Instance.PrincipalDocumentBrand.FindBrandingForPrincipal(instance.Shipment.JS_OH_DeliveryAgent);
		}

		#endregion

		#region Implementation

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ContainerRelease; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.ContainerRelease };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == DataContext.ContainerRelease)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(dataContext, instance) };
			}
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, instance);
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.ExportFreightAgent)
			{
				if (instance.Shipment.BookingPartyDocumentaryAddress.Organisation != null)
				{
					return new OrgHeaderContact(instance.Shipment.BookingPartyDocumentaryAddress.Organisation, null);
				}
				else
				{
					return null;
				}
			}
			else if (contact == ContactType.Depot)
			{
				if (instance.ContainerYard != null)
				{
					return new OrgHeaderContact(instance.ContainerYard.Header, null);
				}
				else
				{
					return null;
				}
			}
			else
			{
				return base.GetContactOrganisation(menuName, contact, direction);
			}
		}
		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.ExportFreightAgent)
			{
				if (instance.Shipment.BookingPartyDocumentaryAddress.E2_AddressOverride)
				{
					return instance.Shipment.BookingPartyDocumentaryAddress;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return base.GetOverriddenDeliveryDetails(menuName, contact, direction);
			}
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != DataContext.GenericFreightJob
					&& dataContext != DataContext.ContainerRelease
					&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion

		readonly ReleaseInstance instance;
	}
}


