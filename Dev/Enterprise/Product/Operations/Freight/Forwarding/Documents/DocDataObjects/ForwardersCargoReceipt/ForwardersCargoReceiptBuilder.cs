using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CarrierMessageValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal abstract class ForwardersCargoReceiptBuilder
	{
		public ForwardersCargoReceiptBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters)
		{
			this.shipment = shipment;
			this.parameters = parameters;
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		protected readonly ForwardingShipment shipment;
		protected readonly IDocDataObjectParameters parameters;
		protected readonly IContext context;

		protected T Build<T>(T wrapper) where T : ForwardersCargoReceipt
		{
			if (shipment == null || CurrentConsol == null)
			{
				return wrapper;
			}

			PopulateAddresses(wrapper);
			PopulatePorts(wrapper);
			PopulateShipment(wrapper);

			AddValidation(wrapper);

			wrapper.Delivered = GetLastDocumentDeliveredEvent(parameters.DocumentTitle) != null;

			return wrapper;
		}

		void PopulateShipment(ForwardersCargoReceipt wrapper)
		{
			wrapper.IncotermCodeDescription = new CodeDescription(shipment.Lookups.JS_INCO_List)
			{
				Code = Core.Constants.IncoTerms.GetMappedOfficialIncoterm(shipment.JS_INCO)
			};

			wrapper.IncotermDescription = wrapper.IncotermCodeDescription?.Description ?? ZString.Empty;
			wrapper.MarksAndNumbers = shipment.JS_MarksAndNumbers;
			if (CurrentConsol != null)
			{
				wrapper.Transports = DocDataObjects.Transports.Create(context, Transports);
				wrapper.TransportMode = CurrentConsol.JK_TransportMode;

				wrapper.SupplierBookingNumber = ZString.Join(", ", shipment.OuterPackLines
					.Cast<ForwardingPackLine>()
					.Select(packLine => packLine.GetContainer(CurrentConsol))
					.Where(container => container?.SupplierBooking != null)
					.Select(container => container.SupplierBooking.JSB_BookingId)
					.Distinct()
					.OrderBy(bookingId => bookingId)
					.ToArray());

				PopulateContainerRelated(wrapper);

				if (CurrentConsol.MostInterestingTransportForBinding.Count > 0)
				{
					var lastTransport = CurrentConsol.MostInterestingTransportForBinding.Cast<Freight.Business.Transport>().Last();
					if (CurrentConsol.JK_TransportMode == Core.Constants.TransportModes.Air)
					{
						wrapper.VesselName = lastTransport.JW_JX_JV_RegistrationNo;
						wrapper.VoyageFlightNumber = lastTransport.JW_VoyageFlightForBinding;
					}
					else if (CurrentConsol.JK_TransportMode == Core.Constants.TransportModes.Sea)
					{
						wrapper.VesselName = lastTransport.JW_VesselForBinding;
						wrapper.VoyageFlightNumber = lastTransport.JW_VoyageFlightForBinding;
					}
				}
			}
		}

		void AddValidation(ForwardersCargoReceipt wrapper)
		{
			AddStandardAddressValidation(wrapper);

			wrapper.IncotermDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("fa0455d6-7197-4996-94f7-22a1f8b2a423", "Incoterm is required."));
			wrapper.ContainerSummaryInfo.AddMessageErrorIfEmpty(Res.GetString("a9d9ba95-3281-4e38-be7d-5c9cba8c678a", "Container Numbers, Types, and Seals is required."));
			wrapper.VesselNameInfo.AddMessageErrorIfEmpty(Res.GetString("1743bc2b-4d53-45b5-82f2-49269fdf4e85", "Vessel is required."));
			wrapper.VoyageFlightNumberInfo.AddMessageErrorIfEmpty(Res.GetString("75bd16c3-41d8-429c-befa-982b9f753135", "Voyage is required."));
			wrapper.SupplierBookingNumberInfo.AddMessageErrorIfEmpty(Res.GetString("e61a47f8-1c97-4102-a7ab-1736de81304e", "Supplier Booking Number is required."));
			wrapper.Origin?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("9a5bee14-3706-43ca-b601-eb8c877539c4", "Origin is required."));
			wrapper.Origin?.Country?.CodeInfo?.AddMessageErrorIfEmpty(Res.GetString("e91d3c3b-9e21-4942-b726-f5f7842f9d19", "Origin Country/Region is required."));
			wrapper.PortOfLoading?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("f59b4ea1-0d50-4cce-86d9-10e8510077b7", "Load Port is required."));
			wrapper.PortOfDischarge?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("f57c0dd8-b13f-4aab-8def-8da9535a8636", "Discharge Port is required."));
			wrapper.CargoReceiptDateInfo.AddMessageErrorIfEmpty(Res.GetString("7e79f1f8-92e1-422b-8ad5-db0e677a5a37", "Cargo Receipt Date is required."));
		}

		void AddStandardAddressValidation(ForwardersCargoReceipt wrapper)
		{
			wrapper.Shipper
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Shipper", () => false);// programmatic constant

			wrapper.Buyer
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Buyer", () => false);// programmatic constant

			wrapper.Carrier
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Carrier", () => false);// programmatic constant

			wrapper.Destination
				.AddStandardAddressValidation()
				.AddPartyNameAndAddressValidation((NoResString)"Destination", () => false);// programmatic constant

			wrapper.Shipper?.CompanyNameInfo.AddMessageError(() => string.IsNullOrWhiteSpace(wrapper.Shipper.CompanyName)
				|| string.IsNullOrWhiteSpace(wrapper.Shipper.AddressLine1)
				|| string.IsNullOrWhiteSpace(wrapper.Shipper.Country?.Name)
				, (NoResString)"Shipper name and address information is required.");// programmatic constant

			wrapper.Buyer?.CompanyNameInfo.AddMessageError(() => string.IsNullOrWhiteSpace(wrapper.Buyer.CompanyName)
				|| string.IsNullOrWhiteSpace(wrapper.Buyer.AddressLine1)
				|| string.IsNullOrWhiteSpace(wrapper.Buyer.Country?.Name)
				, (NoResString)"Buyer name and address information is required.");// programmatic constant

			wrapper.Carrier?.CompanyNameInfo.AddMessageError(() => string.IsNullOrWhiteSpace(wrapper.Carrier.CompanyName)
				|| string.IsNullOrWhiteSpace(wrapper.Carrier.AddressLine1)
				|| string.IsNullOrWhiteSpace(wrapper.Carrier.Country?.Name)
				, (NoResString)"Carrier name and address information is required.");// programmatic constant

			wrapper.Destination?.CompanyNameInfo.AddMessageError(() => string.IsNullOrWhiteSpace(wrapper.Destination.City)
				|| string.IsNullOrWhiteSpace(wrapper.Destination.Country?.Name)
				, (NoResString)"Destination city and country is required.");// programmatic constant
		}

		void PopulateContainerRelated(ForwardersCargoReceipt fcr)
		{
			if (CurrentShipment.Containers.Count() == 1)
			{
				fcr.CargoReceiptDate = CurrentShipment.Containers.First().JC_FCLWharfGateIn;
				fcr.OnBoardDate = CurrentShipment.Containers.First().JC_FCLOnBoardVessel;
			}
			else if (CurrentShipment.Containers.Any())
			{
				fcr.CargoReceiptDate = CurrentShipment.Containers.OrderBy(container => container.JC_FCLWharfGateIn).Last().JC_FCLWharfGateIn;
				fcr.OnBoardDate = CurrentShipment.Containers.OrderBy(container => container.JC_FCLOnBoardVessel).Last().JC_FCLOnBoardVessel;
			}

			if (fcr.CargoReceiptDate.IsEmpty)
			{
				fcr.CargoReceiptDate = (CurrentConsol.Transports.FirstOrDefault() as Freight.Business.Transport)?.JW_TerminalReceivalCommences ?? ZDateTime.Empty;
			}

			if (fcr.OnBoardDate.IsEmpty)
			{
				fcr.OnBoardDate = (CurrentConsol.Transports.FirstOrDefault() as Freight.Business.Transport)?.JW_ETD ?? ZDateTime.Empty;
			}

			fcr.ContainerSummary = GetContainerSummary();
		}

		void PopulateAddresses(ForwardersCargoReceipt wrapper)
		{
			wrapper.Shipper = AddressBuilder.Create(context, Shipper).AddAsAgentInfoToCompanyName(Shipper);
			wrapper.Buyer = AddressBuilder.Create(context, Buyer);
			wrapper.ControllingCustomer = AddressBuilder.Create(context, ControllingCustomer);
			wrapper.NotifyParty = AddressBuilder.Create(context, NotifyParty);
			wrapper.NotifyParty2 = AddressBuilder.Create(context, NotifyParty2);
			wrapper.NotifyParty3 = AddressBuilder.Create(context, NotifyParty3);
			wrapper.Carrier = AddressBuilder.Create(context, shipment.DepartureConsol?.ShippingLineAddress, false);
		}

		void PopulatePorts(ForwardersCargoReceipt wrapper)
		{
			wrapper.PortOfLoading = Unloco.Create(context, Transports.FirstOrDefault()?.LoadPort).WithCustomNameProvider(GetDetailedPortName);

			if (CurrentConsol != null)
			{
				if (CurrentConsol.JK_TransportMode == Core.Constants.TransportModes.Air)
				{
					wrapper.PortOfDischarge = Unloco.Create(context, Transports.LastOrDefault()?.DiscPort).WithCustomNameProvider(GetDetailedPortName);
				}
				else if (CurrentConsol.JK_TransportMode == Core.Constants.TransportModes.Sea)
				{
					wrapper.PortOfDischarge = Unloco.Create(context, CurrentConsol.Transports.MostInterestingTransport?.DiscPort).WithCustomNameProvider(GetDetailedPortName);
				}
				else
				{
					wrapper.PortOfDischarge = Unloco.Create(context, (IUnloco)null).WithCustomNameProvider(GetDetailedPortName);
				}
			}

			wrapper.Origin = Unloco.Create(context, Origin).WithCustomNameProvider(GetDetailedPortName);

			if (!(shipment.ConsigneeDeliveryAddress?.IsEmpty ?? true))
			{
				wrapper.Destination = AddressBuilder.Create(context, shipment.ConsigneeDeliveryAddress);
			}
			else if (!(shipment.ConsigneeDocumentaryAddress?.IsEmpty ?? true))
			{
				wrapper.Destination = AddressBuilder.Create(context, shipment.ConsigneeDocumentaryAddress);
			}
			else
			{
				wrapper.Destination = AddressBuilder.Create(context, null);
			}
		}

		ZString GetContainerSummary()
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (ForwardingContainer container in CurrentConsol.Containers)
			{
				if (container.JC_ContainerNum.IsEmpty || !container.PackLines.Any())
				{
					continue;
				}

				result.Append(result.Length > 0 ? ";" : "");
				result.Append(container.JC_ContainerNum).Append(" - ").Append(container.RefContainer?.RC_Code ?? ZString.Empty);
				if (GetValidSealNums(container).Any())
				{
					result.Append("(").Append(ZString.Join(";", GetValidSealNums(container).ToArray())).Append(")");
				}
			}

			return result.ToString();
		}

		protected IReadOnlyCollection<Freight.Business.Transport> Transports => transports ?? (transports = GetTransports());
		IReadOnlyCollection<Freight.Business.Transport> transports;

		IReadOnlyCollection<Freight.Business.Transport> GetTransports()
		{
			CurrentConsol?.Transports.Sort(MovementLegComparer.PortsAndDatesBased(CurrentConsol.Transports));
			return CurrentConsol?
				.Transports?
				.OfType<Freight.Business.Transport>()
				.ToArray() ?? Enumerable.Empty<Freight.Business.Transport>().ToArray();
		}

		string GetDetailedPortName(IRefUNLOCO refUnloco)
		{
			return UnlocoExtensions.GetDetailedPortName(refUnloco);
		}

		IEnumerable<ZString> GetValidSealNums(ForwardingContainer container)
		{
			if (!container.JC_SealNum.IsEmpty)
			{
				yield return container.JC_SealNum;
			}

			if (!container.JC_AdditionalSealNum.IsEmpty)
			{
				yield return container.JC_AdditionalSealNum;
			}

			if (!container.JC_Additional2SealNum.IsEmpty)
			{
				yield return container.JC_Additional2SealNum;
			}
		}

		protected StmALog GetLastDocumentDeliveredEvent(ZString documentName)
		{
			if (shipment == null)
			{
				return null;
			}

			var query = new ZQuery(JobDocumentDataSchema.JDD_ParentID, SQLComparisonOperator.Equal, shipment.PK);
			query.AddToFilter(JobDocumentDataSchema.JDD_Name, SQLComparisonOperator.Equal, parameters.DataStoreName);

			var data = shipment.Factory.LoadTop1<IVisualizerDocumentData>(query);
			if (data == null)
			{
				return null;
			}

			var lastDocumentDeliveredEvent = (data as EnterpriseBusinessObject).Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.DocumentDeliveredCode && !log.SL_IsCancelled && log.SL_Reference.EndsWith(documentName))
				.OrderByDescending(log => log.SL_EventTime)
				.FirstOrDefault();

			return lastDocumentDeliveredEvent;
		}

		JobDocAddress Shipper => shipment?.ConsignorDocumentaryAddress;
		JobDocAddress Buyer => shipment?.ConsigneeDocumentaryAddress;
		JobDocAddress NotifyParty => shipment?.NotifyPartyDocumentaryAddress;
		JobDocAddress NotifyParty2 => shipment?.NotifyParty2DocumentaryAddress;
		JobDocAddress NotifyParty3 => shipment?.NotifyParty3DocumentaryAddress;
		JobDocAddress ControllingCustomer => shipment?.ControllingCustomerAddress;

		RefUNLOCO Origin => shipment.Origin;

		ForwardingShipment CurrentShipment => shipment;
		ForwardingConsol CurrentConsol => CurrentShipment?.DepartureConsol;
	}
}
