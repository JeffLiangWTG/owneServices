using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;
using static Enterprise.Integration.DocumentWrappers;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingDocumentSupporter : CommonShipmentDocumentSupporter
	{
		public QuotedBookingDocumentSupporter(QuotedBooking quotedBooking)
			: base(quotedBooking.Booking)
		{
			this.quotedBooking = quotedBooking;
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);

			bookingDeliveryCancelled = false;
		}

		protected QuotedBooking QuotedBooking
		{
			get { return quotedBooking; }
		}
		readonly QuotedBooking quotedBooking;

		#region GetContainers

		ContainerNonDependentCollection GetContainers()
		{
			ContainerNonDependentCollection result = new ContainerNonDependentCollection(Factory);
			foreach (ForwardingContainer container in quotedBooking.QuotedBookingContainers)
			{
				result.Add(container);
			}
			return result;
		}

		#endregion

		#region GetSelectedContainerWrappers

		DocumentWrapper[] GetSelectedContainerWrappers(ForwardingContainer[] containersToSelectFrom, CommonShipment shipment)
		{
			DocumentWrapper[] result = Array.Empty<DocumentWrapper>();

			BusinessObjectFactory selectionFactory = new BusinessObjectFactory();
			ContainerToSelectFromForPrintingCollection containersToSelectFrom_SelectionFactory = new ContainerToSelectFromForPrintingCollection(selectionFactory);

			foreach (ForwardingContainer currentContainer in containersToSelectFrom)
			{
				ContainerToSelectFromForPrinting containerToSelect_SelectionFactory = new ContainerToSelectFromForPrinting(currentContainer);
				containersToSelectFrom_SelectionFactory.Add(containerToSelect_SelectionFactory);
			}

			ContainersToPrintOptions containersToPrintOptions = QueryProvider.GetContainersToPrint(containersToSelectFrom_SelectionFactory, false);

			if (containersToPrintOptions != null && containersToPrintOptions.ContainersToPrint != null && containersToPrintOptions.ContainersToPrint.Length > 0)
			{
				result = new DocumentWrapper[containersToPrintOptions.ContainersToPrint.Length];

				for (int i = 0; i < containersToPrintOptions.ContainersToPrint.Length; i++)
				{
					ForwardingContainer containerInCurrentFactory = Factory.Load<ForwardingContainer>(containersToPrintOptions.ContainersToPrint[i].PK);
					DocumentWrapper containerWrapper = DocumentWrapperFactory.CreateContainerWrapperWithShipment(containerInCurrentFactory, shipment);
					result.SetValue(containerWrapper, i);
				}
			}

			return result;
		}

		#endregion

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.QuotedBooking; }
		}

		#endregion

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			StmMenuItem concreteCommandBeingRun = commandBeingRun as StmMenuItem;

			switch (dataContext)
			{
				case Constants.DataContext.CartageAdvice:
					if (QuotedBooking.Booking.JS_PackingMode == Core.Constants.ContainerModes.FCL)
					{
						result = GetSelectedContainerWrappers((ForwardingContainer[])GetContainers().ToArray(typeof(ForwardingContainer)), QuotedBooking.Booking);
					}
					else
					{
						result = new[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, QuotedBooking.Booking) };
					}
					break;

				case Constants.DataContext.RequestForService:
					result = Shipment.DocsAndCartage.GetServiceWrappers(Constants.DataContext.Shipment);
					break;

				case Core.Constants.DataContext.Service:
					result = new[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, QuotedBooking.Booking) };
					break;

				case Core.Constants.DataContext.FreightLabels:
					DocumentShipment docShipmentBizObject = new DocumentShipment(QuotedBooking.Booking, dataContext);

					bool canPrintFreightLabels = false;
					bool bypassDialog = QuotedBooking.Booking.IsDomestic() && (concreteCommandBeingRun == null || concreteCommandBeingRun.GetDocumentDirection() == DocumentDirection.ANY);

					if (bypassDialog && QuotedBooking.Booking.JS_OuterPacks > 0)
					{
						docShipmentBizObject.NumberOfLabelsToPrint = QuotedBooking.Booking.JS_OuterPacks;
						canPrintFreightLabels = true;
					}
					else if ((docShipmentBizObject = QueryProvider.GetDocumentOptions(docShipmentBizObject)) != null)
					{
						canPrintFreightLabels = true;
					}

					if (canPrintFreightLabels)
					{
						result = new[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.Shipment, docShipmentBizObject) };
					}
					break;

				case Constants.DataContext.GenericFreightJobByPackages:
					result = GetGenericWrapperForPacks(dataContext, false);
					break;

				case Core.Constants.DataContext.Shipment:
					result = new[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.QuotedBooking, QuotedBooking) };
					break;

				case Core.Constants.DataContext.GenericFreightJob:
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, QuotedBooking);
					break;

				case Core.Constants.DataContext.GenericFreightJobServices:
					result = QuotedBooking.Booking.DocsAndCartage.GetServiceWrappersForDocBuilder(QuotedBooking, Core.Constants.DataContext.GenericFreightJob);
					break;

				default:
					result = new[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.QuotedBooking, QuotedBooking) };
					break;
			}

			if (result != null)
			{
				foreach (var overridable in result.OfType<IBusinessObjectForCustomFieldsOverridable>())
				{
					overridable.OverrideBusinessObjectForCustomFields(QuotedBooking);
				}
			}

			return result;
		}

		#endregion

		#region GetGenericWrappers

		protected override DocumentWrapper[] GetGenericWrappers()
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, QuotedBooking);
		}

		#endregion

		#region GetSupportedDataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
					{
						Core.Constants.DataContext.Shipment,
						Core.Constants.DataContext.CartageAdvice,
						Core.Constants.DataContext.RequestForService,
						Core.Constants.DataContext.Service,
						Core.Constants.DataContext.FreightLabels,
						Core.Constants.DataContext.QuotedBooking,
						Core.Constants.DataContext.Quotation,
						Core.Constants.DataContext.GenericFreightJob,
						Core.Constants.DataContext.GenericFreightJobServices,
						Core.Constants.DataContext.GenericFreightJobByPackages
					};
		}

		#endregion

		#region Child Business Contexts

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.DtbBooking }; }
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommandBeingRun)
		{
			IDocumentSupportable[] result = null;

			switch (businessContext)
			{
				case BusinessContext.DtbBooking:
					if (!QuotedBooking.CanCreateTransportBooking)
					{
						bookingDeliveryCancelled = false;
						return Array.Empty<IDocumentSupportable>();
					}
					var transportBookings = TransportBookingLoader.GetBookingsToDeliver(QuotedBooking, menuToBeRun);
					bookingDeliveryCancelled = !transportBookings.Any();
					result = transportBookings.Cast<IDocumentSupportable>().ToArray();
					break;

				case BusinessContext.Quotation:
					result = new[] { QuotedBooking.Quote };
					break;

				default:
					result = base.GetChildCollection(menuToBeRun, businessContext, childCommandBeingRun);
					break;
			}

			return result;
		}

		#endregion

		#region GetBODocDataProvidersNotFoundMessage

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case Constants.DataContext.CartageAdvice:
					{
						if (QuotedBooking.Booking.JS_PackingMode == Core.Constants.ContainerModes.FCL)
						{
							var containers = GetContainers();
							if (!containers.Any())
							{
								return Res.GetString("bd1e1d40-9cdb-4484-8c9e-a2a849b8556c", "This booking does not have any containers.");
							}
						}

						break;
					}
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		#endregion

		#region ShowReasonForNotPrintingCore

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return !bookingDeliveryCancelled
				&& dataContext != Core.Constants.DataContext.FreightLabels
				&& dataContext != Core.Constants.DataContext.GenericFreightJob
				&& dataContext != Core.Constants.DataContext.GenericFreightJobServices
				&& dataContext != Core.Constants.DataContext.GenericFreightJobByPackages
				&& dataContext != Constants.DataContext.RequestForService
				&& dataContext != Core.Constants.DataContext.Service
				&& dataContext != Core.Constants.DataContext.Shipment
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		bool bookingDeliveryCancelled;

		#endregion

		#region GetFilterValue

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CNT:
					return QuotedBooking.Booking.JS_PackingMode;
				default:
					return base.GetFilterValue(filterName);
			}
		}

		#endregion

		protected override BusinessObject DeliveryObject => quotedBooking;

		protected override ZString GetCartageAdviceDocumentDataStateMessage()
		{
			return !quotedBooking.CanCreateTransportBooking ? (ZString)Res.GetString("89c6c1ae-fc18-448f-8233-46981bde0363", "You cannot create Cartage Advice for a Booking which has been converted to a Shipment. Run the Cartage Advice document directly from the converted Shipment instead.") : ZString.Empty;
		}
	}
}
