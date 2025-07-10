using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingDocumentSupporter : DocumentSupporter
	{
		public DtbBookingDocumentSupporter(DtbBooking booking)
			: base(booking)
		{
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());
			result.Add(new DtbBookingCartageAdviceDocumentEventsHandler() { DocumentSupporter = this });
			return result.ToArray();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbBooking; }
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get
			{
				// tested in IDtbBookingParentTestCase.TestDocumentSupporter_SupportedChildBusinessContexts
				return new[]
				{
					BusinessContext.DtbConsolidation,
					BusinessContext.Shipment,
					BusinessContext.Customs,
					BusinessContext.AgencyDocumentation, // Agency
					BusinessContext.QuotedBooking,
					BusinessContext.WhsInwards, // WhsReceive
					BusinessContext.WhsOrder,
					BusinessContext.Consol,
					BusinessContext.HVLVBookingHeader,
					BusinessContext.HVLVConsignment,
					BusinessContext.HVLVOriginLoadList,
					BusinessContext.TransitDspConsignmnt,
					BusinessContext.LTConsignment
				};
			}
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result;
			switch (businessContext)
			{
				case BusinessContext.DtbBooking:
					result = new IDocumentSupportable[] { Booking };
					break;
				case BusinessContext.DtbConsolidation:
					result = new IDocumentSupportable[] { Booking.ConsolidationSingleJob };
					break;
				case BusinessContext.LTConsignment:
					result = new IDocumentSupportable[] { (IDocumentSupportable)Booking.LandTransportConsignment };
					break;
				default:
					result = GetParentDocumentSupportable();
					break;
			}

			return result.Length == 0 && businessContext == BusinessContext ? new[] { Booking } : result;
		}

		IDocumentSupportable[] GetParentDocumentSupportable() // tested in IDtbBookingParentTestCase.TestDocumentSupporter_GetChildCollection
		{
			IDocumentSupportable validParentDocumentSupportable = null;
			var consolidation = Booking.ConsolidationSingleJob;
			if (consolidation != null)
			{
				var loader = consolidation.Parent;
				if (loader != null)
				{
					var iParent = loader.ParentWithWorkflow as IDocumentSupportable;
					if (iParent != null)
					{
						var iBooking = (IDocumentSupportable)Booking;
						var parentDocumentSupporter = iParent.DocumentSupporter;
						var bookingDocumentSupporter = iBooking.DocumentSupporter;
						if (bookingDocumentSupporter.SupportedChildBusinessContexts.Contains(parentDocumentSupporter.BusinessContext))
						{
							validParentDocumentSupportable = iParent;
						}
					}
				}
			}

			return validParentDocumentSupportable != null ? new[] { validParentDocumentSupportable } : Array.Empty<IDocumentSupportable>();
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.DtbBookingCustomiseDocuments; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return GetDocumentDeliveryContact(contactType, Booking);
		}

		public static IDocumentDeliveryContact GetDocumentDeliveryContact(IContactType contactType, DtbBooking booking)
		{
			OrgHeaderContact orgHeaderContact = null;
			if (contactType == ContactType.LocalTransport)
			{
				orgHeaderContact = new OrgHeaderContact(booking.Address.Organisation, booking.Address.Address);
			}
			else if (contactType == ContactType.Sales)
			{
				var job = booking.Job;
				var localClient = job != null ? job.LocalCharges : null;
				if (localClient != null)
				{
					orgHeaderContact = new OrgHeaderContact(localClient, null);
				}
			}

			return orgHeaderContact;
		}

		public DtbBooking Booking
		{
			get { return (DtbBooking)BusinessObject; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Booking);
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob, Constants.DataContext.GenericFreightJobServices, Constants.DataContext.DtbBooking };
		}
	}
}
