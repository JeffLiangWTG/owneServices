using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationDocumentSupporter : DocumentSupporter
	{
		public DtbBookingConsolidationDocumentSupporter(DtbBookingConsolidation bookingConsolidation)
			: base(bookingConsolidation)
		{
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());
			result.Add(new DtbBookingConsolidationCartageAdviceDocumentEventsHandler() { DocumentSupporter = this });
			return result.ToArray();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbConsolidation; }
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get
			{
				// tested in IDtbBookingParentTestCase.TestDocumentSupporter_SupportedChildBusinessContexts
				return new[]
				{
					BusinessContext.DtbBooking,
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
					BusinessContext.TransitDspConsignmnt
				};
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.DtbBookingConsolidationCustomiseDocuments; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
					Constants.DataContext.DtbConsolidation,
					Constants.DataContext.GenericFreightJob
			};
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (commandBeingRun != null && dataContext == Constants.DataContext.GenericFreightJob)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, TransportBookingConsolidation);
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result;
			switch (businessContext)
			{
				case BusinessContext.DtbBooking:
					result = GetBookingsToPrintDocumentSupporters();
					break;
				default:
					result = GetParentDocumentSupportable();
					break;
			}

			return result;
		}

		IDocumentSupportable[] GetParentDocumentSupportable() // tested in IDtbBookingParentTestCase.TestDocumentSupporter_GetChildCollection
		{
			IDocumentSupportable validParentDocumentSupportable = null;
			var loader = TransportBookingConsolidation.Parent;
			if (loader != null)
			{
				var iParent = loader.ParentWithWorkflow as IDocumentSupportable;
				if (iParent != null)
				{
					var iConsolidation = (IDocumentSupportable)TransportBookingConsolidation;
					var parentDocumentSupporter = iParent.DocumentSupporter;
					var consolidationDocumentSupporter = iConsolidation.DocumentSupporter;
					if (consolidationDocumentSupporter.SupportedChildBusinessContexts.Contains(parentDocumentSupporter.BusinessContext))
					{
						validParentDocumentSupportable = iParent;
					}
				}
			}

			return validParentDocumentSupportable != null ? new[] { validParentDocumentSupportable } : Array.Empty<IDocumentSupportable>();
		}

		IDocumentSupportable[] GetBookingsToPrintDocumentSupporters()
		{
			IDocumentSupportable[] result;

			var activeBookings = TransportBookingConsolidation.ActiveBookings;
			if (activeBookings.Count == 0)
			{
				result = Array.Empty<IDocumentSupportable>();
			}
			else if (activeBookings.Count == 1)
			{
				result = new IDocumentSupportable[] { activeBookings[0] };
			}
			else
			{
				var bookingsToPrintArgs = new DtbBookingsToPrintEventArgs(new DocumentDtbBookingCollection(activeBookings));

				TransportBookingConsolidation.RunOnSelectDtbBookingsToPrint(this, bookingsToPrintArgs);

				if (bookingsToPrintArgs.ContinueToPrint)
				{
					result = bookingsToPrintArgs.BookingsToSelectFrom
						.Cast<DocumentDtbBooking>()
						.Where(b => b.IncludeInDelivery)
						.Select(b => (IDocumentSupportable)b.Booking)
						.ToArray();
				}
				else
				{
					result = Array.Empty<IDocumentSupportable>();
				}
			}

			SelectedBookingsToPrint = result;

			return result;
		}

		public IDocumentSupportable[] Bookings
		{
			get
			{
				if (SelectedBookingsToPrint.Length > 0)
				{
					return SelectedBookingsToPrint;
				}
				else
				{
					return (IDocumentSupportable[])TransportBookingConsolidation.ActiveBookings;
				}
			}
		}

		IDocumentSupportable[] SelectedBookingsToPrint { get; set; } = Array.Empty<IDocumentSupportable>();

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return TransportBookingConsolidation.Bookings.Count > 0 ? DtbBookingDocumentSupporter.GetDocumentDeliveryContact(contactType, TransportBookingConsolidation.Bookings[0]) : null;
		}

		public DtbBookingConsolidation TransportBookingConsolidation
		{
			get { return (DtbBookingConsolidation)BusinessObject; }
		}
	}
}
