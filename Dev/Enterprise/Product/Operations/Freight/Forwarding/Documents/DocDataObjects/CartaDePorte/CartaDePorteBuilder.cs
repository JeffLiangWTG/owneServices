using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CPT
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class will be used in next workflow")]
	sealed class CartaDePorteBuilder
	{
		public CartaDePorteBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.parameters = parameters;
			this.context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;

		public CartaDePorte Build()
		{
			var customBusinessObject = shipment is ICustomFieldProvider customFieldProvider ? customFieldProvider.GetCustomBusinessObject() : null;
			var cartaDePorte = new CartaDePorte(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef, customBusinessObject);
			cartaDePorte.NumberOfCopies = shipment.JS_NoCopyBills;
			cartaDePorte.NumberOfOriginals = shipment.JS_NoOriginalBills;
			cartaDePorte.HouseBillNumber = shipment.JS_HouseBill;
			cartaDePorte.ShipmentNumber = shipment.JS_UniqueConsignRef;
			cartaDePorte.IsOriginal = string.Compare(parameters?.DocumentTitle, "ORIGINAL", StringComparison.OrdinalIgnoreCase) == 0;
			cartaDePorte.DocumentType = cartaDePorte.IsOriginal ? "ORIGINAL" : "COPIA";
			cartaDePorte.CommercialInvoiceNumber = ZString.Empty;
			cartaDePorte.ShippersReference = shipment.JS_BookingReference;
			cartaDePorte.DateOfIssue = ZDateTime.Now;

			cartaDePorte.BillOfLading = DepartureConsol?.JK_MasterBillNum ?? ZString.Empty;

			cartaDePorte.Logo = new HouseBillLogo(shipment, cartaDePorte.IsOriginal);
			cartaDePorte.TermsAndConditions = new HouseBillTermsAndConditions(shipment);

			cartaDePorte.OtherReference = shipment.JS_BookingReference.ToUpper() + ", " + shipment.JS_OrderReferences.ToUpper();
			cartaDePorte.Driver = ZString.Empty;
			cartaDePorte.VehicleType = ZString.Empty;
			cartaDePorte.VehicleRegNoTruck = ZString.Empty;
			cartaDePorte.VehicleRegNoWagon = ZString.Empty;

			cartaDePorte.TotalWeight = new Measurement
			{
				Value = shipment.JS_ActualWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = shipment.JS_UnitOfWeight
				}
			};

			cartaDePorte.TotalVolume = new Measurement
			{
				Value = shipment.JS_ActualVolume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = shipment.JS_UnitOfVolume
				}
			};

			PopulateAddresses(cartaDePorte);
			PopulateLocations(cartaDePorte);
			PopulateOrderReferences(cartaDePorte);
			PopulateOrders(cartaDePorte);
			PopulateCharges(cartaDePorte);
			PopulateContainers(cartaDePorte);
			PopulateLoosePackingLines(cartaDePorte);
			PopulateNotes(cartaDePorte);

			AddValidation(cartaDePorte);

			return cartaDePorte;
		}

		void AddValidation(CartaDePorte cartaDePorte)
		{
			cartaDePorte.ValidateAllIncludingChildren();
		}

		#region Addresses

		void PopulateAddresses(CartaDePorte cartaDePorte)
		{
			cartaDePorte.ForwardingAgent = AddressBuilder.Create(context, GlbBranch.CurrentBranch.OrgProxy?.MainAddress);

			cartaDePorte.Issuer = AddressBuilder.CreateForCurrentUser(context);
			cartaDePorte.Issuer.CompanyName = GlbBranch.CurrentBranch.GB_BranchName;
			cartaDePorte.Issuer.City = GlbBranch.CurrentBranch.City;

			cartaDePorte.Consignor = AddressBuilder.Create(context, shipment.ConsignorDocumentaryAddress);
			cartaDePorte.Consignee = AddressBuilder.Create(context, shipment.ConsigneeDocumentaryAddress);
			cartaDePorte.NotifyParty = AddressBuilder.Create(context, shipment.NotifyPartyDocumentaryAddress);
			cartaDePorte.ExportBroker = AddressBuilder.Create(context, shipment.ExportBroker?.MainAddress);

			cartaDePorte.SendingForwarder = AddressBuilder.Create(context, DepartureConsol?.SendingForwarderAddress);
		}

		#endregion

		#region Locations

		void PopulateLocations(CartaDePorte cartaDePorte)
		{
			cartaDePorte.PortOfOrigin = Unloco.Create(context, shipment.Origin);
			cartaDePorte.PortOfDestination = Unloco.Create(context, shipment.Destination);
			cartaDePorte.PortOfLoading = Unloco.Create(context, DepartureConsol?.LoadPort ?? shipment.LoadPort);
			cartaDePorte.PortOfDischarge = Unloco.Create(context, DepartureConsol?.DischargePort ?? shipment.DischargePort);
		}

		#endregion

		#region Orders

		void PopulateOrderReferences(CartaDePorte cartaDePorte)
		{
			cartaDePorte.OrderReferences = shipment
				.DocsAndCartage
				.OrderItems
				.OfType<OrderItem>()
				.Select(item => item.JT_OrderReference.ToString())
				.ToArray();
		}

		void PopulateOrders(CartaDePorte cartaDePorte)
		{
			cartaDePorte.Orders = shipment
				.AttachedOrders
				.Select(order => CreateOrderDataObject(order))
				.ToArray();

			if (cartaDePorte.Orders.Count > 0)
			{
				cartaDePorte.OtherReference = cartaDePorte.OtherReference + ", " + String.Join(", ", cartaDePorte.Orders.Select(o => o.OrderNumber));
			}
		}

		OrderDataObject CreateOrderDataObject(Order order)
		{
			return new OrderDataObject(order.PK)
			{
				OrderNumber = order.JD_OrderNumber,
				OrderDate = order.JD_OrderDate,
				OrderLines = order?
				.OrderLines
				.Select(orderLine => CreateOrderLineDataObject(orderLine))
				.ToArray()
			};
		}

		OrderLineDataObject CreateOrderLineDataObject(OrderLine orderLine)
		{
			return new OrderLineDataObject(orderLine.PK)
			{
				LineNumber = orderLine.JO_LineNo,
				Description = orderLine.JO_Description,
				OuterPacks = orderLine.JO_OuterPacks,
				InnerPacks = orderLine.JO_InnerPacks,
				TotalInnerPacks = orderLine.JO_TotalInnerPacks,
				QuantityOrdered = new Measurement
				{
					Value = orderLine.JO_Quantity,
					Unit = new CodeDescription(orderLine.JO_F3_NKPackType_List)
					{
						Code = orderLine.JO_F3_NKPackType
					}
				},
				QuantityInvoiced = new Measurement
				{
					Value = orderLine.JO_QtyInvoiced,
					Unit = new CodeDescription(orderLine.JO_F3_NKPackType_List)
					{
						Code = orderLine.JO_F3_NKPackType
					}
				},
				QuantityReceived = new Measurement
				{
					Value = orderLine.JO_QtyReceived,
					Unit = new CodeDescription(orderLine.JO_F3_NKPackType_List)
					{
						Code = orderLine.JO_F3_NKPackType
					}
				},
				QuantityRemaining = new Measurement
				{
					Value = orderLine.JO_QuantityRemaining,
					Unit = new CodeDescription(orderLine.JO_F3_NKPackType_List)
					{
						Code = orderLine.JO_F3_NKPackType
					}
				},
				ItemPrice = new Money
				{
					Amount = orderLine.JO_ItemPrice,
					Currency = new CodeDescription(Lookups.Currencies)
					{
						Code = orderLine.Order.JD_Calc_Currency.Currency
					}
				},
				TotalLinePrice = new Money
				{
					Amount = orderLine.JO_LinePrice,
					Currency = new CodeDescription(Lookups.Currencies)
					{
						Code = orderLine.Order.JD_Calc_Currency.Currency
					}
				},
				RequiredDate = orderLine.JO_LineDropDate,
				Status = new CodeDescription(orderLine.JO_LineStatus_List)
				{
					Code = orderLine.JO_LineStatus
				},
				Product = new CodeDescription(orderLine.JO_Partno_List)
				{
					Code = orderLine.JO_Partno
				},
				OrderNumber = orderLine.Order.JD_OrderNumberAndSplit
			};
		}

		#endregion

		#region Charges

		void PopulateCharges(CartaDePorte cartaDePorte)
		{
			cartaDePorte.Charges = new ChargesCollection(shipment, Lookups, cartaDePorte.IsOriginal, false);

			cartaDePorte.TotalCollect = cartaDePorte.Charges.Where(c => !c.IsPrepaid).Sum(c => c.LocalSell.Amount);
			cartaDePorte.TotalPrepaid = cartaDePorte.Charges.Where(c => c.IsPrepaid).Sum(c => c.LocalSell.Amount);
		}

		#endregion

		#region Goods Details (containers, packinglines, dangerous goods)

		void PopulateContainers(CartaDePorte cartaDePorte)
		{
			var consol = MovementLegComparer.FirstOrDefaultLegForTransportMode(shipment.Consols.Cast<CommonConsol>(), shipment.TransportMode);

			var containerBizObjs = consol
				?.Containers
				.OfType<ForwardingContainer>()
				.OrderBy(container => container.JC_ContainerNum)
				.ToArray();

			var containers = containerBizObjs?.Length > 0
				? CreateContainers(containerBizObjs)
				: Array.Empty<Container>();

			cartaDePorte.Containers = containers;

			var allPackingLines = containers
				.SelectMany(c => c.PackingLines)
				.ToArray();

			cartaDePorte.TotalPackCount = allPackingLines.Sum(p => p.Quantity);

			if (!allPackingLines.TryGetPackTypeCode(out var packTypeCode))
			{
				packTypeCode = shipment.JS_F3_NKPackType;
			}

			cartaDePorte.TotalPackType = new CodeDescription(shipment.Lookups.PackTypes)
			{
				Code = packTypeCode
			};
		}

		Container[] CreateContainers(ForwardingContainer[] containerBizObjs)
		{
			var containers = new List<Container>();

			var containerBuilder = new ContainerBuilder();
			var packlineBuilder = new PackingLineBuilder();

			var shipmentPks = GetPksFromShipmentAndAllSubShipmentsWithoutChildren();

			foreach (var containerBizObj in containerBizObjs)
			{
				ForwardingPackLine[] packingLineBizObjs;

				packingLineBizObjs = containerBizObj
					.PackLines
					.OfType<ForwardingPackLine>()
					.Where(p => p.JL_JS.In(shipmentPks))
					.ToArray();

				if (packingLineBizObjs.Length == 0)
				{
					continue;
				}

				var containerID = $"{containerBizObj.PK}-{shipment.JS_UniqueConsignRef}"; // Non-translatable Identifier
				var container = containerBuilder.Build(containerBizObj, context, containerID: containerID);

				var packingLines = packingLineBizObjs
					.Select(packLine => packlineBuilder.Build(packLine))
					.ToArray();

				container.PackingLines = packingLines;

				container.GoodsWeight = CreateContainerGoodsWeight(packingLines);
				container.Volume = CreateContainerVolume(packingLines);
				container.PackCount = packingLines.Sum(p => p.Quantity);
				container.PackType = new CodeDescription(shipment.Lookups.PackTypes)
				{
					Code = packingLines.GetPackTypeCode()
				};
				container.IsEmpty = false;

				containers.Add(container);
			}

			return containers.ToArray();
		}

		Measurement CreateContainerGoodsWeight(PackingLine[] packingLines)
		{
			var unitOfWeight = packingLines.HaveSameUnitOfWeight()
				? (packingLines[0].Weight?.Unit?.Code.ToString() ?? Constants.Weight.Kilograms)
				: Constants.Weight.Kilograms;

			var weight = packingLines.Sum(p => Constants.Weight.Convert(p.Weight.Value, p.Weight.Unit.Code, unitOfWeight));

			return new Measurement
			{
				Value = weight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};
		}

		Measurement CreateContainerVolume(PackingLine[] packingLines)
		{
			var unitOfVolume = packingLines.HaveSameUnitOfVolume()
				? (packingLines[0].Volume?.Unit?.Code.ToString() ?? Constants.Volume.CubicMetres)
				: Constants.Volume.CubicMetres;

			var volume = packingLines.Sum(p => Constants.Volume.Convert(p.Volume.Value, p.Volume.Unit.Code, unitOfVolume));

			return new Measurement
			{
				Value = volume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};
		}

		void PopulateLoosePackingLines(CartaDePorte cartaDePorte)
		{
			var packLineBuilder = new PackingLineBuilder();

			HashSet<ZGuid> shipmentPks = GetPksFromShipmentAndAllSubShipmentsWithoutChildren();

			var loosePackingLines = shipment
				.OuterPackLines
				.OfType<ForwardingPackLine>()
				.Where(p => !p.Containers.Any() && p.JL_JS.In(shipmentPks))
				.Select(packLine => packLineBuilder.Build(packLine))
				.ToArray();

			cartaDePorte.LoosePackingLines = loosePackingLines;
		}

		HashSet<ZGuid> GetPksFromShipmentAndAllSubShipmentsWithoutChildren()
		{
			HashSet<ZGuid> shipmentPks = new HashSet<ZGuid>();
			shipmentPks.Add(shipment.PK);

			foreach (var subShipmentPK in shipment.GetPksFromAllSubShipmentsWithoutChildren())
			{
				shipmentPks.Add(subShipmentPK);
			}

			return shipmentPks;
		}

		#endregion

		#region Notes

		void PopulateNotes(CartaDePorte cartaDePorte)
		{
			cartaDePorte.Notes = shipment
				.Notes
				.GetAllNotes()
				.Cast<StmNote>()
				.Select(note => new Note()
				{
					Text = note.ST_NoteDataAsText,
					Description = note.ST_Description
				})
				.ToArray();
		}

		#endregion

		#region Implementation

		ForwardingConsol DepartureConsol => departureConsol ?? (departureConsol = shipment.DepartureConsol);
		ForwardingConsol departureConsol;

		HouseBillLookups Lookups => lookups ?? (lookups = new HouseBillLookups(shipment.Factory));
		HouseBillLookups lookups;
		#endregion
	}
}
