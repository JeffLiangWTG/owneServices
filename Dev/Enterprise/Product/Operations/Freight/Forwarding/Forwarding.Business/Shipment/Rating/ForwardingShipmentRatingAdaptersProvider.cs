using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentRatingAdaptersProvider : ShipmentRatingAdaptersProvider<ForwardingShipment>
	{
		protected internal ForwardingShipmentRatingAdaptersProvider(ForwardingShipment parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(ForwardingShipment parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = new List<IAutoRating>();

			var jobInvoicingSupporter = parent.InvoicingSupporter;
			var consol = parent.Consols.Count == 1 ? parent.GetFirstOrCorrectConsol() : null;

			var enableRateSelectorForConsolLevelChargesOnShipment =
				consol != null
				&& !consol.IsGatewayBillingEnabled()
				&& !parent.Gateways.Any()
				&& consol.TopLevelShipments.Count == 1
				&& !consol.HasConsolCosts(GlbCompany.CurrentCompany)
				&& Core.Constants.ContainerModes.IsFCLType(consol.JK_ConsolMode) == Core.Constants.ContainerModes.IsFCLType(parent.PackingMode)
				&& !parent.IsConsolLeadShipment()
				&& options.BillingType == BillingType.Invoicing
				&& options.AutoratingProcess == CostSell.Cost;

			var standaloneShipmentOnly =
				options.AutoratingProcess == CostSell.Cost &&
				options.StandaloneShipmentOnly &&
				jobInvoicingSupporter != null &&
				jobInvoicingSupporter.IsStandaloneShipment;

			if (enableRateSelectorForConsolLevelChargesOnShipment)
			{
				result.Add(new ForwardingShipmentRateSelectorEnabledRatingAdapter(parent));
			}
			else if (standaloneShipmentOnly)
			{
				var ratingRoutes = new List<RouteSetRatingRoute>();

				if (RatingDataRegistry.Instance.MultiModalRatingCostShipment.Value)
				{
					ratingRoutes.AddRange(parent.TransportsIncludingRelated.RouteSets.Select(x => new RouteSetRatingRoute(x, parent)));
				}
				else
				{
					var mostInterestingRoute = parent.TransportsIncludingRelated.RouteSets
						.FirstOrDefault(x => parent.MostInterestingTransport?.RouteSetNumber == x.RouteSetNumber);

					if (mostInterestingRoute != null)
					{
						ratingRoutes.Add(new RouteSetRatingRoute(mostInterestingRoute, parent));
					}
				}

				if (ratingRoutes.Any())
				{
					result.AddRange(ratingRoutes.Select(x => new ForwardingShipmentRatingRouteAdapter(x)));
				}
			}
			else if (parent.PackingMode == Constants.ContainerModes.BuyersConsol)
			{
				result.AddRange(GetBuyersConsolRelatedAdapters(parent));
			}
			else if (parent.PackingMode == Constants.ContainerModes.ShippersConsol)
			{
				result.AddRange(GetShippersConsolRelatedAdapters(parent));
			}
			else
			{
				result.Add(parent.RatingAdapter);
			}

			var declaration = parent.DeclarationForDocuments as IRatingSupporterWithAdapter;
			var ratingAdapter = declaration?.RatingAdapter;

			if (ratingAdapter != null && !result.Contains(ratingAdapter))
			{
				result.Add(ratingAdapter);
			}

			return result;
		}

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(ForwardingShipment parent)
		{
			var result = new List<IJobInvoicingPlugIn>(base.GetAdditionalJobs(parent));
			var bookings = TransportBookingLoader.GetRelatedTransportBookingJobInvoicingPlugIn(parent);
			result.AddRange(bookings);
			return new ReadOnlyCollection<IJobInvoicingPlugIn>(result);
		}

		protected override IQuickCalculateRating GetForQuickCalculate(ForwardingShipment parent, IAutoRatingInteractor uiInteractor)
		{
			var adapter = (parent.IsBuyersConsolLead || parent.IsShippersConsolLead) && !parent.ApportionConsol()
				? new ConsolLeadShipmentRatingAdapter(parent, null)
				: parent.RatingAdapter;

			return new QuickCalculateRating(adapter);
		}

		#region BCN Adapter

		IEnumerable<IAutoRating> GetBuyersConsolRelatedAdapters(ForwardingShipment parent)
		{
			var result = new List<IAutoRating>();

			// The flag indicates if we need additionally autorate ORG charges. Normally, freight and destination charges
			// (specified in the registry) for BCN shipment are autorated as FCL and then the amount gets apportioned to shipments.
			// But ORG charges are calculated for each shipment individually based on a shipment measures.
			// However, the user may add ORG charges to the apportion charges as well. In this case we don't need to calculate ORG separately
			// as the they will be calculated as FCL and apportioned.
			var shouldAutorateOriginCharges = !Env.Registry.Rating.BuyersConsolApportionedCodes.Contains(ChargeCodeGroupList.Codes.Origin);

			if (parent.IsBuyersConsolLead)
			{
				// Lead shipment

				if (shouldAutorateOriginCharges)
				{
					// Origin charges must be calculated based on measures of this particular shipment, so a separate adapter created
					result.Add(CreateBcnAdapterForOriginCharges(parent));
				}

				if (parent.ApportionConsol())
				{
					// Apportion or ApportionInvoiceMaster invoicing style

					// BCN adapter calculates FCL charges based on total measures from all shipments and apportions it to this shipment if provided.
					// In this invoicing style we need charges to be apportion to this lead shipment.
					result.Add(new ConsolLeadShipmentRatingAdapter(parent, parent));
				}
				else
				{
					// Master invoicing style

					// We don't need to apportion charges in Master invoicing mode for lead shipment, so, null is passed as thisShipment.
					// I.e. non apportioned amount will be added to this lead shipment.
					result.Add(new ConsolLeadShipmentRatingAdapter(parent, null));

					if (shouldAutorateOriginCharges)
					{
						// In Master invoicing style we should calculate origin charges from each individual sub shipments as well which should appear on
						// this lead shipment
						foreach (ForwardingShipment subShipment in parent.CoLoadShipments)
						{
							result.Add(CreateBcnAdapterForOriginCharges(subShipment));
						}
					}
				}
			}
			else
			{
				// Sub Shipment

				if (parent.ApportionConsol())
				{
					// Apportion or ApportionInvoiceMaster invoicing style

					var leadShipment = parent.GetConsolLeadShipment();
					if (leadShipment != null)
					{
						if (shouldAutorateOriginCharges)
						{
							// Original charges must be calculated based on measures of this particular shipment, so a separate adapter created
							result.Add(CreateBcnAdapterForOriginCharges(parent));
						}

						// We need apportion FCL charges so, the lead shipment is passed as master one (to calculate total BCN amount)
						// and this sub shipment passed as thisShipment.
						result.Add(new ConsolLeadShipmentRatingAdapter(leadShipment, parent));
					}
					else
					{
						// We cannot autorated without the Lead Shipment. A special adapter will be created with
						// a specific status not allowing to run autorating.
						result.Add(CreateAdapterForSubShipmentWithoutLeadShipment(parent));
					}
				}
				else
				{
					// Master invoicing style

					// This style is not supported by BCN sub shipments. A special adapter will be created with
					// a specific status not allowing to run autorating.
					result.Add(CreateSubShipmentAdapterForMasterInvoicingStyle(parent));
				}
			}

			return result;
		}

		#endregion

		#region SCN Adapter

		IEnumerable<IAutoRating> GetShippersConsolRelatedAdapters(ForwardingShipment parent)
		{
			var result = new List<IAutoRating>();

			// The flag indicates if we need additionally autorate DST charges. Normally, freight and origin charges
			// (specified in the registry) for SCN shipment are autorated as FCL and then the amount gets apportioned to shipments.
			// But DST charges are calculated for each shipment individually based on a shipment measures.
			// However, the user may add DST charges to the apportion charges as well. In this case we don't need to calculate DST separately
			// as the they will be calculated as FCL and apportioned.
			var shouldAutorateDestinationCharges = !Env.Registry.Rating.ShippersConsolApportionedCodes.Contains(ChargeCodeGroupList.Codes.Destination);

			if (parent.IsShippersConsolLead)
			{
				// Lead shipment

				if (shouldAutorateDestinationCharges)
				{
					// Destination charges must be calculated based on measures of this particular shipment, so a separate adapter created
					result.Add(CreateScnAdapterForDestinationCharges(parent));
				}

				if (parent.ApportionConsol())
				{
					// Apportion or ApportionInvoiceMaster invoicing style

					// SCN adapter calculates FCL charges based on total measures from all shipments and apportions it to this shipment if provided.
					// In this invoicing style we need charges to be apportion to this lead shipment.
					result.Add(new ConsolLeadShipmentRatingAdapter(parent, parent));
				}
				else
				{
					// Master invoicing style

					// We don't need to apportion charges in Master invoicing mode for lead shipment, so, null is passed as thisShipment.
					// I.e. non apportioned amount will be added to this lead shipment.
					result.Add(new ConsolLeadShipmentRatingAdapter(parent, null));

					if (shouldAutorateDestinationCharges)
					{
						// In Master invoicing style we should calculate destination charges from each individual sub shipments as well which should appear on
						// this lead shipment
						foreach (ForwardingShipment subShipment in parent.CoLoadShipments)
						{
							result.Add(CreateScnAdapterForDestinationCharges(subShipment));
						}
					}
				}
			}
			else
			{
				// Sub Shipment

				if (parent.ApportionConsol())
				{
					// Apportion or ApportionInvoiceMaster invoicing style

					var leadShipment = parent.GetConsolLeadShipment();
					if (leadShipment != null)
					{
						if (shouldAutorateDestinationCharges)
						{
							// Destination charges must be calculated based on measures of this particular shipment, so a separate adapter created
							result.Add(CreateScnAdapterForDestinationCharges(parent));
						}

						// We need apportion FCL charges so, the lead shipment is passed as master one (to calculate total SCN amount)
						// and this sub shipment passed as thisShipment.
						result.Add(new ConsolLeadShipmentRatingAdapter(leadShipment, parent));
					}
					else
					{
						// We cannot autorated without the Lead Shipment. A special adapter will be created with
						// a specific status not allowing to run autorating.
						result.Add(CreateAdapterForSubShipmentWithoutLeadShipment(parent));
					}
				}
				else
				{
					// Master invoicing style

					// This style is not supported by SCN sub shipments. A special adapter will be created with
					// a specific status not allowing to run autorating.
					result.Add(CreateSubShipmentAdapterForMasterInvoicingStyle(parent));
				}
			}

			return result;
		}

		#endregion

		IAutoRating CreateSubShipmentAdapterForMasterInvoicingStyle(ForwardingShipment parent)
		{
			var mode = parent.PackingMode == Constants.ContainerModes.BuyersConsol ?
				Res.GetString("4b7153b4-b94e-47c8-b588-acf89d3e98c7", "Buyer") : Res.GetString("2c3b9a50-0d0a-42af-9be8-27c994f797b9", "Shipper");

			var message = Res.GetString(
				"606b602e-1308-47fc-b5f6-de7518193dc3",
				"This Shipment is part of a {0}'s Consol. Generally, you should Autorate and invoice from the {0}'s Consol Master", mode);

			var bcnLeadShipment = parent.GetConsolLeadShipment();
			if (bcnLeadShipment != null)
			{
				message += " " + Res.GetString("ad1caf16-1333-4350-b578-4ae4d3d1916f", "- Shipment {0}", bcnLeadShipment.JS_UniqueConsignRef);
			}

			var shipmentProxy = new AutoRatingProxy(parent.RatingAdapter);
			shipmentProxy.ValuesCanBeSet = true;
			shipmentProxy.StatusInformation = new AutoRatingStatusInfo(false, message);
			shipmentProxy.ValuesCanBeSet = false;

			return shipmentProxy;
		}

		IAutoRating CreateAdapterForSubShipmentWithoutLeadShipment(ForwardingShipment parent)
		{
			var mode = parent.PackingMode == Constants.ContainerModes.BuyersConsol ? (NoResString)"Buyer" : (NoResString)"Shipper";
			var message = Res.GetString(
				"7b516436-4b93-4e40-9d9a-fe76d4947031",
				"This Shipment is marked as a {0}'s Consol but there is no Lead specified. Please specify a Lead Shipment if you wish to apportion {0}'s Consol charges when Autorating.", mode);

			var proxy = new AutoRatingProxy(parent.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.StatusInformation = new AutoRatingStatusInfo(false, message);
			proxy.ValuesCanBeSet = false;

			return proxy;
		}

		IAutoRating CreateBcnAdapterForOriginCharges(ForwardingShipment parent)
		{
			var mode = FreightRatingHelper.CalculateFreightModeFromTransportMode(parent.TransportMode);
			mode |= FreightMode.NonContainerised; // we want to calculate Origin charges with LCL rates

			if (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.Value)
			{
				mode |= FreightMode.BCN;
			}

			var shipmentProxy = new AutoRatingProxy(parent.RatingAdapter);
			shipmentProxy.ValuesCanBeSet = true;
			shipmentProxy.StatusInformation = parent.GetStatusInformation();
			shipmentProxy.ChargeCodeGroups = new ChargeCodeGroupCollection();
			shipmentProxy.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Origin);
			shipmentProxy.FreightMode = mode;
			shipmentProxy.ValuesCanBeSet = false;

			return shipmentProxy;
		}

		IAutoRating CreateScnAdapterForDestinationCharges(ForwardingShipment parent)
		{
			var mode = FreightRatingHelper.CalculateFreightModeFromTransportMode(parent.TransportMode);
			mode |= FreightMode.SCN;

			var shipmentProxy = new AutoRatingProxy(parent.RatingAdapter);
			shipmentProxy.ValuesCanBeSet = true;
			shipmentProxy.StatusInformation = parent.GetStatusInformation();
			shipmentProxy.ChargeCodeGroups = new ChargeCodeGroupCollection();
			shipmentProxy.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Destination);
			shipmentProxy.FreightMode = mode;
			shipmentProxy.ValuesCanBeSet = false;

			return shipmentProxy;
		}
	}
}
