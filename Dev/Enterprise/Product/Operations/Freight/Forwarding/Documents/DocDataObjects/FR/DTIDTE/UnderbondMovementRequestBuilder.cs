using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class UnderbondMovementRequestBuilder
	{
		public UnderbondMovementRequestBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			Argument.NotNull(consol, nameof(consol));
			Argument.NotNull(parameters, nameof(parameters));

			this.consol = consol;
			this.parameters = parameters;
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;
		readonly IDocDataObjectParameters parameters;

		public UnderbondMovementRequest Build()
		{
			var documentName = ConsolDocumentDataStoreNames.DTI.Equals(parameters.DataStoreName, StringComparison.OrdinalIgnoreCase) ?
				FrenchPortsConstants.DocumentNames.DTI :
				FrenchPortsConstants.DocumentNames.DTE;

			var request = new UnderbondMovementRequest(nameof(ForwardingConsol), consol.JK_UniqueConsignRef);
			request.ConsolNumber = consol.JK_UniqueConsignRef;
			request.BillOfLading = consol.JK_MasterBillNum;
			request.DTI = ConsolDocumentDataStoreNames.DTI.Equals(parameters.DataStoreName, StringComparison.OrdinalIgnoreCase);

			var fclModes = new[] {
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.ShippersConsol,
				Core.Constants.ContainerModes.Other
			};

			request.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List ?? new CodeDescriptionPairList())
			{
				Code = fclModes.Contains<string>(consol.JK_ConsolMode)
					? (ZString)Core.Constants.ContainerModes.FCL
					: consol.JK_ConsolMode
			};

			request.ShipmentType = new CodeDescription(consol.JK_AgentType_List ?? new CodeDescriptionPairList())
			{
				Code = consol.JK_AgentType
			};

			PopulateAddresses(request);

			PopulateTransportDetails(request);

			PopulcateAdditionalReferences(request);

			PopulcateContainers(request);

			PopulcatePortDues(request);

			AddValidation(request);

			request.ValidateAllIncludingChildren();

			return request;
		}

		void PopulateAddresses(UnderbondMovementRequest request)
		{
			if (consol.ShippingLineAddress != null)
			{
				request.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress, false);
				request.CarrierCI5 = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
				request.CarrierSON = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
				request.CarrierCCC = consol.ShippingLineAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.CodeTypes.CarrierCode);
			}

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			if (proxyMainAddress != null)
			{
				request.SendingParty = AddressBuilder.CreateForCurrentUser(context);
				request.SendingPartyCI5 = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
				request.SendingPartySON = proxyMainAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			}

			if (request.DTI)
			{
				if (consol.ArrivalUnpackCFSTransportAddress != null)
				{
					request.Transporter = AddressBuilder.Create(context, consol.ArrivalUnpackCFSTransportAddress);
					request.TransporterCI5 = consol.ArrivalUnpackCFSTransportAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
					request.TransporterSON = consol.ArrivalUnpackCFSTransportAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
				}
			}
			else
			{
				if (consol.DeparturePackCFSTransportAddress != null)
				{
					request.Transporter = AddressBuilder.Create(context, consol.DeparturePackCFSTransportAddress);
					request.TransporterCI5 = consol.DeparturePackCFSTransportAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
					request.TransporterSON = consol.DeparturePackCFSTransportAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
				}
			}

			if (consol.SendingForwarderAddress != null)
			{
				request.SendingForwarder = AddressBuilder.Create(context, consol.SendingForwarderAddress, false);
				request.SendingForwarderCI5 = consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
				request.SendingForwarderSON = consol.SendingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			}

			if (consol.ReceivingForwarderAddress != null)
			{
				request.ReceivingForwarder = AddressBuilder.Create(context, consol.ReceivingForwarderAddress, false);
				request.ReceivingForwarderCI5 = consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CI5);
				request.ReceivingForwarderSON = consol.ReceivingForwarderAddress.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.SON);
			}
		}

		void PopulateTransportDetails(UnderbondMovementRequest request)
		{
			request.TransportMode = "RTE";

			if (request.DTI)
			{
				var departureSeaLeg = SeaTransportsInLegOrder.FirstOrDefault();
				var arrivalFRSeaLeg = SeaTransportsInLegOrder.LastOrDefault(transport => FranceAndDependentCountriesCodes.Any(countryCode => transport.DiscPort.Country.Code == countryCode));

				request.VesselName = arrivalFRSeaLeg?.JW_Vessel ?? ZString.Empty;
				request.PortOfOrigin = Unloco.Create(context, departureSeaLeg?.LoadPort);
				request.PortOfDestination = Unloco.Create(context, arrivalFRSeaLeg?.DiscPort);
				request.OperationalPort = Unloco.Create(context, consol.ArrivalCTOAddress?.RelatedPortCode ?? SeaTransportsInLegOrder.LastOrDefault()?.DiscPort);

				request.VoyageFlightNo = arrivalFRSeaLeg?.JW_VoyageFlight ?? ZString.Empty;
			}
			else
			{
				var departureFRSeaLeg = SeaTransportsInLegOrder.FirstOrDefault(transport => FranceAndDependentCountriesCodes.Any(countryCode => transport.LoadPort.Country.Code == countryCode));
				var arrivalSeaLeg = SeaTransportsInLegOrder.LastOrDefault();

				request.VesselName = departureFRSeaLeg?.JW_Vessel ?? ZString.Empty;
				request.PortOfOrigin = Unloco.Create(context, departureFRSeaLeg?.LoadPort);
				request.PortOfDestination = Unloco.Create(context, arrivalSeaLeg?.DiscPort);
				request.OperationalPort = Unloco.Create(context, consol.DepartureCTOAddress?.RelatedPortCode ?? SeaTransportsInLegOrder.FirstOrDefault()?.LoadPort);

				request.VoyageFlightNo = departureFRSeaLeg?.JW_VoyageFlight ?? ZString.Empty;
			}

			request.PortOfTranshipment = Unloco.Create(context, Helpers.GetFranceTransshipmentPort(SeaTransportsInLegOrder, !request.DTI));

			var port = GetRequestPort(request);
			request.PortAreaFrom = new CodeDescription(() => consol.Factory.GetCachedValue(ZString.Format("FR_AreaList|Port_{0}", port.Code) // cache key
			, () => AreaList.GetAreaList(port.Code)));

			request.PortLocationFrom = new CodeDescription(() => consol.Factory.GetCachedValue(ZString.Format("FR_LocationList|Port_{0}|Area_{1}", port.Code, request.PortAreaFrom.Code) // cache key
										, () => LocationList.GetLocationList(port.Code, request.PortAreaFrom.Code)));

			request.PortAreaTo = new CodeDescription(() => consol.Factory.GetCachedValue(ZString.Format("FR_AreaList|Port_{0}", port.Code) // cache key
							, () => AreaList.GetAreaList(port.Code)));

			request.PortLocationTo = new CodeDescription(() => consol.Factory.GetCachedValue(ZString.Format("FR_LocationList|Port_{0}|Area_{1}", port.Code, request.PortAreaTo.Code) // cache key
										, () => LocationList.GetLocationList(port.Code, request.PortAreaTo.Code)));
		}

		ZString[] FranceAndDependentCountriesCodes
		{
			get
			{
				return consol.Factory.GetCachedValue("FranceAndDependentCountriesCodes", () =>
				new ZString[]
				{
					Core.Constants.CountryCodes.France,
					Core.Constants.CountryCodes.FrenchGuyana,
					Core.Constants.CountryCodes.FrenchPolynesia,
					Core.Constants.CountryCodes.Guadeloupe,
					Core.Constants.CountryCodes.Martinique,
					Core.Constants.CountryCodes.Mayotte,
					Core.Constants.CountryCodes.NewCaledonia,
					Core.Constants.CountryCodes.SaintBarthelemy,
					Core.Constants.CountryCodes.SaintMartin,
					Core.Constants.CountryCodes.StPierreEtMiquelon,
					Core.Constants.CountryCodes.WallisAndFutunaIslands
				});
			}
		}

		IUnloco GetRequestPort(UnderbondMovementRequest request)
		{
			return request.DTI ? request.PortOfDestination : request.PortOfOrigin;
		}

		void AddValidation(UnderbondMovementRequest request)
		{
			((CodeDescription)request.PortLocationTo).CodeInfo.AddPortLocationValidation(() => GetRequestPort(request)?.Code ?? ZString.Empty);
			((CodeDescription)request.PortAreaTo).CodeInfo.AddPortAreaValidation(() => GetRequestPort(request)?.Code ?? ZString.Empty);

			((CodeDescription)request.PortLocationFrom).CodeInfo.AddPortLocationValidation(() => GetRequestPort(request)?.Code ?? ZString.Empty);
			((CodeDescription)request.PortAreaFrom).CodeInfo.AddPortAreaValidation(() => GetRequestPort(request)?.Code ?? ZString.Empty);

			((CodeDescription)request.PortDuesCurrency).CodeInfo.AddInvalidCodeValidation();

			request.ReasonNoteInfo.AddMessageError(() => request.ReasonNote.Length > 35, Res.GetString("61D7B577-357E-4FAB-86A2-6FC2CBFB200A", "A maximum of 35 characters may be sent."));

			if (request.Containers.Any())
			{
				foreach (var container in request.Containers)
				{
					container.NumberInfo.AddMessageErrorIfEmpty(Res.GetString("9FBE0A36-ED07-4B6B-B209-41D8DAAC10C6", "Container Number is required."));
				}
			}
		}

		void PopulcateAdditionalReferences(UnderbondMovementRequest request)
		{
			request.CarrierBookingReference = consol.JK_BookingReference;
			request.BOL = consol.JK_MasterBillNum;
		}

		void PopulcateContainers(UnderbondMovementRequest request)
		{
			var containerBuilder = new ContainerBuilder();
			var containers = consol.Containers.OfType<ForwardingContainer>()
				.Select(container => BuildContainer(request, container))
				.ToArray();

			request.Containers = containers;
		}

		UnderbondMovementRequestContainer BuildContainer(UnderbondMovementRequest request, ForwardingContainer containerBO)
		{
			var requestContainer = new UnderbondMovementRequestContainer();
			requestContainer.Number = containerBO.JC_ContainerNum;
			requestContainer.IsEmptyContainer = containerBO.JC_IsEmptyContainer;
			requestContainer.ContainerMode = new ContainerMode { Code = containerBO.JC_ContainerMode };
			requestContainer.ECTICTNumber = request.DTI ? containerBO.JC_ImportDepotCustomsReference : containerBO.JC_ExportDepotCustomsReference;
			requestContainer.ContainerType = new ContainerType(context.ContainerTypes as IFindBoxListProvider)
			{
				Code = containerBO.RefContainer?.RC_Code ?? ZString.Empty,
				ISOCode = containerBO.RefContainer?.RC_ISOType ?? ZString.Empty,
				Type = new CodeDescription(containerBO?.RefContainer?.Lookups?.ContainerTypes ?? new CodeDescriptionPairList())
				{
					Code = containerBO.RefContainer?.RC_ContainerType ?? ZString.Empty
				}
			};
			requestContainer.IsNonOperativeReefer = containerBO.JC_IsNonOperativeReefer;

			return requestContainer;
		}

		void PopulcatePortDues(UnderbondMovementRequest request)
		{
			if (request.DTI)
			{
				request.PortDuesPort = Unloco.Create(context, consol.DischargePort);
				request.PortDuesCurrency = new CodeDescription(consol.RefCurrency_List) { Code = consol.DischargePort.GetCountryFromCode(consol.DischargePort.RL_RN_NKCountryCode).RN_RX_NKLocalCurrency };
			}
			else
			{
				request.PortDuesPort = Unloco.Create(context, consol.LoadPort);
				request.PortDuesCurrency = new CodeDescription(consol.RefCurrency_List) { Code = consol.LoadPort.GetCountryFromCode(consol.LoadPort.RL_RN_NKCountryCode).RN_RX_NKLocalCurrency };
			}
		}

		IReadOnlyCollection<Freight.Business.Transport> SeaTransportsInLegOrder => seaTransportsInLegOrder ?? (seaTransportsInLegOrder = GetSeaTransports());
		IReadOnlyCollection<Freight.Business.Transport> seaTransportsInLegOrder;

		IReadOnlyCollection<Freight.Business.Transport> GetSeaTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol?.Transports));
			return consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea)
				.ToArray();
		}
	}
}
