using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	class CommonConsolidatedTransportBookingDocumentSupporter : DocumentSupporter
	{
		public CommonConsolidatedTransportBookingDocumentSupporter(CommonConsolidatedTransportBooking booking)
			: base(booking)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TransportBooking; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.ConsolidatedTransportBookingCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case Constants.DataContext.ConsolidatedTransportBooking:
					{
						DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(Constants.DataContext.ConsolidatedTransportBooking, ConsolidatedTransportBooking);
						return new DocumentWrapper[] { wrapper };
					}
				default:
					throw new ArgumentException("Invalid DataContext: " + dataContext);
			}
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Constants.DataContext.ConsolidatedTransportBooking
			};
		}

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.LocalTransport)
			{
				return ConsolidatedTransportBooking.TransportCo;
			}
			else
			{
				return base.GetOverriddenDeliveryDetails(menuName, contact, direction);
			}
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;

			if (contact == ContactType.LocalTransport)
			{
				if (ConsolidatedTransportBooking.TransportCo != null && ConsolidatedTransportBooking.TransportCo.Header != null)
				{
					result = new OrgHeaderContact(ConsolidatedTransportBooking.TransportCo.Header, ConsolidatedTransportBooking.TransportCo);
				}
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contact, direction);
			}

			return result;
		}

		CommonConsolidatedTransportBooking ConsolidatedTransportBooking
		{
			get { return (CommonConsolidatedTransportBooking)BusinessObject; }
		}
	}
}
