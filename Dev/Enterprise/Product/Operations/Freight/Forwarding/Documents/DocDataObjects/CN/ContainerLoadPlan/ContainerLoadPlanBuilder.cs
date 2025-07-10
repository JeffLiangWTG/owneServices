using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ContainerLoadPlanBuilder
	{
		public ContainerLoadPlanBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			Argument.NotNull(consol, nameof(consol));
			Argument.NotNull(parameters, nameof(parameters));

			this.consol = consol;
			this.parameters = parameters;
			this.context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IDocDataObjectParameters parameters;
		readonly CommonContext context;

		public ContainerLoadPlan Build()
		{
			var loadPlan = new ContainerLoadPlan(consol.JK_UniqueConsignRef, nameof(ForwardingConsol), DataContext.ContainerLoadPlan);

			loadPlan.ShipmentType = new CodeDescription(consol.JK_AgentType_List ?? new CodeDescriptionPairList())
			{
				Code = consol.JK_AgentType
			};

			var fclModes = new[] {
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.Groupage,
				Core.Constants.ContainerModes.BuyersConsol,
				Core.Constants.ContainerModes.ShippersConsol,
				Core.Constants.ContainerModes.Other
			};

			loadPlan.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List ?? new CodeDescriptionPairList())
			{
				Code = fclModes.Contains<string>(consol.JK_ConsolMode)
					? (ZString)Core.Constants.ContainerModes.FCL
					: consol.JK_ConsolMode
			};

			PopulateAddresses(loadPlan);
			PopulateRouting(loadPlan);
			PopulateContainers(loadPlan);

			loadPlan.ValidateAllIncludingChildren();

			return loadPlan;
		}

		#region Addresses

		void PopulateAddresses(ContainerLoadPlan loadPlan)
		{
			loadPlan.SendingAgent = AddressBuilder.Create(context, consol.SendingForwarderAddress)
				.AddAsAgentInfoToCompanyName(consol.SendingForwarderAddress)
				.AddPartyNameAndAddressValidation((NoResString)"Sending Agent"); // non-translatable validation message
			loadPlan.CurrentUser = AddressBuilder.CreateForCurrentUser(context);

			var carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			var hasScacNumber = carrier.HasRegistrationNumber(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode);
			carrier.CompanyNameInfo.AddMessageError(() => !hasScacNumber, (NoResString)"Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC."); // non-translatable validation message
			loadPlan.Carrier = carrier;

			loadPlan.DepartureCFSAddress = AddressBuilder.Create(context, consol.PackDepotAddress);
		}

		#endregion

		#region Routing

		void PopulateRouting(ContainerLoadPlan loadPlan)
		{
			var ningBoPorts = new string[] { "CNNGB", "CNNBO", "CNNBG" };
			var firstSeaLeg = consol.Transports?.FirstTransportWithTransportMode(Core.Constants.TransportModes.Sea);

			loadPlan.PortOfLoading = Unloco.Create(context, firstSeaLeg?.LoadPort)
				.AddRequiredValidation((NoResString)"Port of Load"); // non-translatable validation message

			loadPlan.PortOfDischarge = Unloco.Create(context, firstSeaLeg?.DiscPort)
				.AddRequiredValidation((NoResString)"Port of Discharge"); // non-translatable validation message

			loadPlan.TradeFlag = ningBoPorts.Contains(loadPlan.PortOfLoading?.Code.ToString(), StringComparer.OrdinalIgnoreCase) && loadPlan.PortOfLoading.Country?.Code == loadPlan.PortOfDischarge?.Country?.Code ? "N" : "W";

			loadPlan.OperationalPort = consol.GetOperationalPort(context);
			loadPlan.PortOfTranship = Unloco.Create(context, firstSeaLeg?.DiscPort);
			loadPlan.PlaceOfDelivery = Unloco.Create(context, consol.DischargePort);

			loadPlan.Vessel = Vessel.Create(context, firstSeaLeg);
			loadPlan.Vessel.NameInfo.AddMessageErrorIfEmpty((NoResString)"Vessel Name is mandatory."); // non-translatable validation message
			loadPlan.Vessel.LloydsIMOInfo.AddMessageErrorIfEmpty((NoResString)"Vessel's Lloyds/IMO is mandatory."); // non-translatable validation message
			loadPlan.VoyageFlightNumber = firstSeaLeg?.JW_VoyageFlight ?? ZString.Empty;
			loadPlan.VoyageFlightNumberInfo.AddMessageErrorIfEmpty((NoResString)"Voyage is mandatory."); // non-translatable validation message
			loadPlan.TransitBerthCodeInfo.AddMessageError(() => !(loadPlan.TransitBerthCode.ToString().ToCharArray().All(x => ZString.AlphanumericCharacters.Contains(x)) && loadPlan.TransitBerthCode.Length == 5), (NoResString)"Transit Berth Code is mandatory and should only contain 5 alphanumeric characters.");  // non-translatable validation message
		}

		#endregion

		#region Containers

		void PopulateContainers(ContainerLoadPlan loadPlan)
		{
			var containerBizObjs = GetContainers();

			var containers = new List<ContainerLoadPlanContainer>();

			foreach (var containerBizObj in containerBizObjs)
			{
				var container = CreateContainerLoadPlanContainer(loadPlan, containerBizObj);
				containers.Add(container);
			}

			loadPlan.Containers = containers;
		}

		ContainerLoadPlanContainer CreateContainerLoadPlanContainer(ContainerLoadPlan loadPlan, ForwardingContainer containerBizObj)
		{
			var container = new ContainerLoadPlanContainer(loadPlan, context, containerBizObj.PK);
			container.Number = containerBizObj.JC_ContainerNum;
			container.Seal = containerBizObj.JC_SealNum;
			container.SealInfo.AddMessageError(() => !string.IsNullOrWhiteSpace(container.Number) && string.IsNullOrWhiteSpace(container.Seal), (NoResString)"Seal number is required when container number is entered."); // non-translatable validation message

			container.SecondSeal = containerBizObj.JC_AdditionalSealNum;
			container.ThirdSeal = containerBizObj.JC_Additional2SealNum;
			container.Groups = GetSOGroupings(containerBizObj);
			container.Groups.ForEach(g => AddSOGroupingValidation(g));

			container.Mode = new CodeDescription(containerBizObj.JC_ContainerMode_List ?? new CodeDescriptionPairList())
			{
				Code = containerBizObj.JC_ContainerMode
			};

			container.PackDate = containerBizObj.JC_PackDate;

			var containerType = new ContainerType(context.ContainerTypes)
			{
				Code = containerBizObj.RefContainer?.RC_Code ?? ZString.Empty
			};

			containerType.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Container Type is required - Consol > Container > Container Type"); // non-translatable validation message
			container.ContainerType = containerType;

			container.SetTemperature = new Measurement
			{
				Value = containerBizObj.JC_SetPointTemp,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = containerBizObj.JC_SetPointTempUnit
				}
			};

			container.IsNonOperativeReefer = containerBizObj.JC_IsNonOperativeReefer;
			container.SetTemperature.ValueInfo.AddMessageError(() => (container.ContainerType.Type?.Code ?? ZString.Empty) == Core.Constants.ContainerTypes.Refrigerated && string.IsNullOrWhiteSpace(container.SetTemperature?.Unit?.Code) && !container.IsNonOperativeReefer, (NoResString)"Temperature is required when container is a reefer."); // non-translatable validation message

			var unitOfWeight = Core.Constants.Weight.Kilograms;
			var grossWeightInKg = Core.Constants.Weight.Convert(containerBizObj.JC_GrossWeight, containerBizObj.JC_GrossWeightUQ, unitOfWeight);

			container.GrossWeight = new Measurement
			{
				Value = grossWeightInKg,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			var tareWeightInKg = Core.Constants.Weight.Convert(containerBizObj.JC_TareWeight, containerBizObj.JC_GrossWeightUQ, unitOfWeight);

			container.TareWeight = new Measurement
			{
				Value = tareWeightInKg,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			container.ContainerErrorInfo.AddMessageError(() => !container.Groups.Any(), (NoResString)"Packs need to be allocated to container."); // non-translatable validation message

			return container;
		}

		void AddSOGroupingValidation(ContainerLoadPlanSOGrouping group)
		{
			group.SONumberInfo.AddMessageErrorIfEmpty((NoResString)"At least one Shipping Order/Shi Lian Dan (Carrier Booking Reference) number is required. \r\nEnter at pack level - Shipment > Packing > Shipping Order/Shi Lian Dan, at Shipment level - Shipment > Additional Details > References > Type = SLD."); // non-translatable validation message
			group.QuantityInfo.AddMessageError(() => group.Quantity == 0, (NoResString)"Pack count cannot be zero."); // non-translatable validation message
			group.Weight.ValueInfo.AddMessageError(() => (group.Weight?.Value ?? 0) == 0, (NoResString)"Pack weight cannot be zero."); // non-translatable validation message
			group.Volume.ValueInfo.AddMessageError(() => (group.Volume?.Value ?? 0) == 0, (NoResString)"Pack volume cannot be zero."); // non-translatable validation message
			group.GoodsDescriptionInfo.AddMessageErrorIfEmpty((NoResString)"Goods Description is mandatory."); // non-translatable validation message
			group.MarksAndNumbersInfo.AddMessageErrorIfEmpty((NoResString)"Marks are mandatory."); // non-translatable validation message

			foreach (var dg in group.DangerousGoods)
			{
				group.DangerousGoodsDescriptionInfo.AddMessageError(() => string.IsNullOrWhiteSpace(dg.Contact.FullName), (NoResString)"Contact Name is required for dangerous goods."); // non-translatable validation message
				group.DangerousGoodsDescriptionInfo.AddMessageError(() => string.IsNullOrWhiteSpace(dg.Contact.Phone), (NoResString)"Contact Phone is required for dangerous goods."); // non-translatable validation message

				group.AddValidationDependencies(group.DangerousGoodsDescriptionInfo, dg.Contact.FullNameInfo, dg.Contact.PhoneInfo);
			}
		}

		static List<ContainerLoadPlanSOGrouping> GetSOGroupings(ForwardingContainer containerBizObj)
		{
			var groups = new List<ContainerLoadPlanSOGrouping>();
			var groupBuilder = new ContainerLoadPlanSOGroupingBuilder();
			var packlineGroups = containerBizObj
				.PackLines
				.Cast<PackLine>()
				.GroupBy(x => Helpers.GetBookingNumberWithFallback(x));

			groups.AddRange(packlineGroups.Select(x => groupBuilder.Build(x.Key, x)));

			return groups;
		}

		IReadOnlyCollection<ForwardingContainer> GetContainers()
		{
			if (parameters?.Data is IReadOnlyCollection<ForwardingContainer> containers)
			{
				return containers;
			}

			return Array.Empty<ForwardingContainer>();
		}

		#endregion
	}
}
