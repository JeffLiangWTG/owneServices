using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Res = Enterprise.Freight.Agency.Documents.DataObjects.Res;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class AgencyHouseBillBuilder
	{
		public AgencyHouseBillBuilder(BillOfLading billOfLading, IDocDataObjectParameters parameters)
		{
			this.billOfLading = Argument.NotNull(billOfLading, nameof(billOfLading));
			this.parameters = parameters;
			this.context = new CommonContext(billOfLading.Factory.GetCachedReadOnlyFactory());
		}
		readonly BillOfLading billOfLading;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;

		public AgencyHouseBill Build()
		{
			var customBusinessObject = billOfLading is ICustomFieldProvider customFieldProvider ? customFieldProvider.GetCustomBusinessObject() : null;
			var agencyHouseBill = new AgencyHouseBill(nameof(BillOfLading), billOfLading.JS_UniqueConsignRef, customBusinessObject);

			PopulateGeneralInfo(agencyHouseBill);
			PopulateAddresses(agencyHouseBill);
			PopulateLocations(agencyHouseBill);
			PopulateDates(agencyHouseBill);
			PopulateGoodsDetails(agencyHouseBill);
			PopulateCharges(agencyHouseBill);

			AddValidation(agencyHouseBill);

			return agencyHouseBill;
		}

		void AddValidation(AgencyHouseBill agencyHouseBill)
		{
			AddGeneralInfoValidation(agencyHouseBill);
			AddAddressesValidation(agencyHouseBill);
			AddLocationsValidation(agencyHouseBill);
			AddGoodsDetailsValidation(agencyHouseBill);

			agencyHouseBill.ValidateAllIncludingChildren();
		}

		#region GeneralInfo

		void PopulateGeneralInfo(AgencyHouseBill agencyHouseBill)
		{
			agencyHouseBill.IsOriginal = string.Compare(parameters?.DocumentTitle, "ORIGINAL", StringComparison.OrdinalIgnoreCase) == 0;
			agencyHouseBill.ExcessValueDeclaration = (NoResString)"Refer to clause on reverse side";

			agencyHouseBill.ContainerMode = new CodeDescription(billOfLading.Lookups.JS_PackingMode_List)
			{
				Code = billOfLading.JS_PackingMode
			};
			agencyHouseBill.ReleaseType = new CodeDescription(Lookups.ReleaseTypes)
			{
				Code = billOfLading.JS_ReleaseType
			};

			agencyHouseBill.OceanBillNumber = billOfLading.JS_HouseBill;
			agencyHouseBill.NumberOfOriginals = billOfLading.JS_NoOriginalBills;

			agencyHouseBill.Logo = new AgencyHouseBillLogo(billOfLading);
			agencyHouseBill.TermsAndConditions = new AgencyHouseBillTermsAndConditions(billOfLading);
			agencyHouseBill.Clause = billOfLading.Principal == null ? string.Empty : (AgencyRegistry.Instance.BillOfLadingClause(billOfLading.Principal).Value ?? string.Empty);

			agencyHouseBill.Transports = Transports.Create(context, SeaTransportsInPortOrder);
			agencyHouseBill.Vessel = agencyHouseBill.Transports.Main?.Vessel?.Name ?? ZString.Empty;
			agencyHouseBill.Voyage = agencyHouseBill.Transports.Main?.VoyageFlightNumber ?? ZString.Empty;

			agencyHouseBill.ShippedOnBoard = new CodeDescription(billOfLading.Lookups.JS_ShippedOnBoard_List)
			{
				Code = billOfLading.JS_ShippedOnBoard
			};
			agencyHouseBill.ShippedOnBoardDate = billOfLading.JS_ShippedOnBoardDate;

			agencyHouseBill.PaymentTerm = new CodeDescription(billOfLading.Lookups.JS_INCO_List)
			{
				Code = billOfLading.JS_INCO
			};

			agencyHouseBill.OuterPacks = billOfLading.JS_OuterPacks;
			agencyHouseBill.OuterPacksPackType = billOfLading.JS_F3_NKPackType;
		}

		#endregion

		#region Addresses

		void PopulateAddresses(AgencyHouseBill agencyHouseBill)
		{
			agencyHouseBill.Shipper = AddressBuilder.Create(context, billOfLading.ConsignorDocumentaryAddress);

			var consignee = AddressBuilder.Create(context, billOfLading.ConsigneeDocumentaryAddress);
			consignee.AddToOrderSupport();
			agencyHouseBill.Consignee = consignee;

			var notifyParty = AddressBuilder.Create(context, billOfLading.NotifyPartyDocumentaryAddress);
			notifyParty.AddSameAsConsigneeSupport();
			agencyHouseBill.NotifyParty = notifyParty;
		}

		#endregion

		#region Locations

		void PopulateLocations(AgencyHouseBill agencyHouseBill)
		{
			agencyHouseBill.PortOfDestination = Unloco.Create(context, billOfLading.Destination);
			agencyHouseBill.PortOfLoading = Unloco.Create(context, MainSeaLeg?.LoadPort);
			agencyHouseBill.PortOfDischarge = Unloco.Create(context, MainSeaLeg?.DiscPort);
			agencyHouseBill.FreightPayableAt = Unloco.Create(context,
				billOfLading.JS_INCO == DomesticPaymentTerms.Prepaid ? billOfLading.Origin : (billOfLading.JS_INCO == DomesticPaymentTerms.Collect ? billOfLading.Destination : null));

			agencyHouseBill.PlaceOfIssue = Unloco.Create(context,
				billOfLading.JS_RL_NKHouseBillIssuePlace.IsEmpty && !billOfLading.JS_HouseBillIssueDate.IsEmpty ? MainSeaLeg?.LoadPort : billOfLading.HouseBillIssuePlace);
			agencyHouseBill.PlaceOfReceipt = Unloco.Create(context, billOfLading.PlaceOfReceipt ?? billOfLading.Origin);
			agencyHouseBill.PlaceOfDelivery = Unloco.Create(context, billOfLading.PlaceOfDischarge ?? billOfLading.Destination);
		}

		#endregion

		#region Dates

		void PopulateDates(AgencyHouseBill agencyHouseBill)
		{
			agencyHouseBill.DateOfIssue = billOfLading.JS_HouseBillIssueDate;
			agencyHouseBill.DepartureDate = billOfLading.JS_E_DEP;
			agencyHouseBill.ArrivalDate = billOfLading.JS_E_ARV;
		}

		#endregion

		#region GoodsDetails

		void PopulateGoodsDetails(AgencyHouseBill agencyHouseBill)
		{
			if (agencyHouseBill.ContainerMode.Code == Core.Constants.ContainerModes.FCL)
			{
				PopulateContainers(agencyHouseBill);
				PopulateLoosePackingLines(agencyHouseBill);
			}
			else
			{
				PopulateTopLevelPackingLines(agencyHouseBill);
			}

			PopulateTotalWeightAndVolumeAndNumberOfPackages(agencyHouseBill);
			PopulateMarksAndNumbersAndGoodsDescription(agencyHouseBill);
		}

		#region Containers

		void PopulateContainers(AgencyHouseBill agencyHouseBill)
		{
			var containerBizObjs = billOfLading.RealContainers.OfType<AgencyShipmentContainer>()
				.OrderBy(container => container.JC_ContainerNum).ToArray();

			agencyHouseBill.Containers = CreateContainers(containerBizObjs);
		}

		Container[] CreateContainers(AgencyShipmentContainer[] containerBizObjs)
		{
			var containers = new List<Container>();

			var containerBuilder = new ContainerBuilder();
			var packlineBuilder = new PackingLineBuilder();

			foreach (var containerBizObj in containerBizObjs)
			{
				var packingLineBizObjs = containerBizObj.PackLines.OfType<AgencyShipmentPackLine>().ToArray();

				var containerID = $"{containerBizObj.PK}-{billOfLading.JS_UniqueConsignRef}";
				var container = containerBuilder.Build(containerBizObj, context, containerID: containerID);

				var packingLines = packingLineBizObjs
					.Select(packLine => packlineBuilder.Build(packLine))
					.OrderBy(packingLine => packingLine.PackingOrder)
					.ThenBy(packingLine => packingLine.PackingLineID).ToArray();

				container.PackingLines = packingLines;

				container.GoodsWeight = new Measurement
				{
					Value = packingLines.Sum(p => Weight.Convert(p.Weight.Value, p.Weight.Unit.Code, Weight.Kilograms)),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Weight.Kilograms
					}
				};

				container.Volume = new Measurement
				{
					Value = packingLines.Sum(p => Volume.Convert(p.Volume.Value, p.Volume.Unit.Code, Volume.CubicMetres)),
					Unit = new CodeDescription(context.VolumeUnits)
					{
						Code = Volume.CubicMetres
					}
				};

				container.PackCount = packingLines.Sum(p => p.Quantity);
				container.PackType = new CodeDescription(billOfLading.Lookups.PackTypes)
				{
					Code = packingLines.GetPackTypeCode()
				};
				container.IsEmpty = false;

				containers.Add(container);
			}

			return containers.ToArray();
		}

		#endregion

		#region LoosePackingLines

		void PopulateLoosePackingLines(AgencyHouseBill agencyHouseBill)
		{
			var packLineBuilder = new PackingLineBuilder();

			var loosePackingLines = billOfLading
				.OuterPackLines
				.OfType<BillOfLadingPackLine>()
				.Where(p => !p.Containers.Any())
				.Select(packLine => packLineBuilder.Build(packLine))
				.ToArray();

			agencyHouseBill.LoosePackingLines = loosePackingLines;
		}

		#endregion

		#region TopLevelPackingLines

		void PopulateTopLevelPackingLines(AgencyHouseBill houseBill)
		{
			var packLineBuilder = new PackingLineBuilder();

			var topLevelPackingLines = billOfLading
				.ShippingContainers
				.OfType<AgencyShipmentContainer>()
				.Select(container => packLineBuilder.Build(container))
				.ToArray();

			houseBill.TopLevelPackingLines = topLevelPackingLines;
		}

		#endregion

		#region TotalWeightAndVolumeAndNumberOfPackages

		void PopulateTotalWeightAndVolumeAndNumberOfPackages(AgencyHouseBill agencyHouseBill)
		{
			var totalWeight = agencyHouseBill.ContainerMode.Code == Core.Constants.ContainerModes.FCL
				? agencyHouseBill.Containers.Sum(c => c.GoodsWeight.Value) + agencyHouseBill.LoosePackingLines.Sum(p => Weight.Convert(p.Weight.Value, p.Weight.Unit.Code, Weight.Kilograms))
				: agencyHouseBill.TopLevelPackingLines.Sum(p => Weight.Convert(p.Weight.Value, p.Weight.Unit.Code, Weight.Kilograms));
			agencyHouseBill.TotalWeight = new Measurement
			{
				Value = totalWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Weight.Kilograms
				}
			};

			var totalVolume = agencyHouseBill.ContainerMode.Code == Core.Constants.ContainerModes.FCL
				? agencyHouseBill.Containers.Sum(c => c.Volume.Value) + agencyHouseBill.LoosePackingLines.Sum(p => Volume.Convert(p.Volume.Value, p.Volume.Unit.Code, Volume.CubicMetres))
				: agencyHouseBill.TopLevelPackingLines.Sum(p => Volume.Convert(p.Volume.Value, p.Volume.Unit.Code, Volume.CubicMetres));
			agencyHouseBill.TotalVolume = new Measurement
			{
				Value = totalVolume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = Volume.CubicMetres
				}
			};

			agencyHouseBill.TotalNumberOfPackages = agencyHouseBill.ContainerMode.Code == Core.Constants.ContainerModes.FCL
				? agencyHouseBill.Containers.Sum(c => c.ContainerCount) + agencyHouseBill.LoosePackingLines.Sum(p => p.Quantity)
				: agencyHouseBill.TopLevelPackingLines.Sum(p => p.Quantity);
		}

		#endregion

		#region MarksAndNumbersAndGoodsDescription

		void PopulateMarksAndNumbersAndGoodsDescription(AgencyHouseBill agencyHouseBill)
		{
			var marksAndNumbersNoteText = string.Join(System.Environment.NewLine,
				billOfLading.Notes.GetAllNotes().Cast<StmNote>().Where(note => note.ST_Description == PredefinedNoteTypes.Instance.MarksAndNumbers.Code).Select(note => note.ST_NoteText));
			agencyHouseBill.MarksAndNumbers = string.IsNullOrEmpty(marksAndNumbersNoteText) ? billOfLading.JS_MarksAndNumbers : marksAndNumbersNoteText;

			if (agencyHouseBill.MarksAndNumbers.IsEmpty && agencyHouseBill.ContainerMode.Code == Core.Constants.ContainerModes.FCL)
			{
				var packingLinesMarksAndNumbers = agencyHouseBill.Containers.SelectMany(container => container.PackingLines.Where(packLine => !packLine.MarksAndNumbers.IsEmpty)).Select(packLine => packLine.MarksAndNumbers)
					.Union(agencyHouseBill.LoosePackingLines.Where(loosePackingLine => !loosePackingLine.MarksAndNumbers.IsEmpty).Select(loosePackingLine => loosePackingLine.MarksAndNumbers));

				agencyHouseBill.MarksAndNumbers = string.Join(System.Environment.NewLine, packingLinesMarksAndNumbers);
			}
			agencyHouseBill.MarksAndNumbers = agencyHouseBill.MarksAndNumbers.SubstringSafe(0, 31981);

			var detailedGoodsDescriptionNoteText = new ZString(string.Join(System.Environment.NewLine,
				billOfLading.Notes.GetAllNotes().Cast<StmNote>().Where(note => note.ST_Description == PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code).Select(note => note.ST_NoteText)));
			agencyHouseBill.GoodsDescription = (string.IsNullOrEmpty(detailedGoodsDescriptionNoteText) ? billOfLading.JS_GoodsDescription : detailedGoodsDescriptionNoteText).SubstringSafe(0, 31981);
		}

		#endregion

		#endregion

		#region Charges

		void PopulateCharges(AgencyHouseBill agencyHouseBill)
		{
			agencyHouseBill.Charges = new ChargesCollection(billOfLading, Lookups, agencyHouseBill.IsOriginal);
		}

		#endregion

		#region Validation

		#region GeneralInfoValidation

		void AddGeneralInfoValidation(AgencyHouseBill agencyHouseBill)
		{
			agencyHouseBill.OceanBillNumberInfo.AddMessageErrorIfEmpty(Res.GetString("be51f283-c068-4678-a7e0-8afc531bc727", "Ocean Bill Number is required."));
			(agencyHouseBill.ReleaseType as CodeDescription)?.DescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("42651fb7-1b4a-4d94-97d3-d30805b200d3", "Release Type is required."));

			agencyHouseBill.VesselInfo.AddMessageErrorIfEmpty(Res.GetString("2917c7af-a359-44a6-8e17-2d4eef4850da", "Vessel is required."));
			agencyHouseBill.VoyageInfo.AddMessageErrorIfEmpty(Res.GetString("f12be17c-214c-41ec-af99-8c890d1d8469", "Voyage is required."));

			agencyHouseBill.NumberOfOriginalsInfo.AddMessageErrorIfEmpty(Res.GetString("928f83b4-31e3-4c48-9e13-6e7faa2f80c6", "No of Originals is required."));
		}

		#endregion

		#region AddressesValidation

		void AddAddressesValidation(AgencyHouseBill agencyHouseBill)
		{
			AddConsigneeAddressValidation(agencyHouseBill);
			AddNotifyPartyAddressValidation(agencyHouseBill);
			AddShipperAddressValidation(agencyHouseBill);
		}

		void AddConsigneeAddressValidation(AgencyHouseBill agencyHouseBill)
		{
			var consignee = agencyHouseBill.Consignee as Address;
			var notifyParty = agencyHouseBill.NotifyParty as Address;

			consignee.AddressFormattedInfo.AddMessageError(() => (consignee.IsToOrder() || (string.IsNullOrWhiteSpace(consignee.CompanyName) || string.IsNullOrWhiteSpace(consignee.AddressLine1) || string.IsNullOrWhiteSpace(consignee.Country?.Name)))
				&& (string.IsNullOrWhiteSpace(notifyParty.CompanyName) || notifyParty.IsSameAsConsignee())
				, Res.GetString("d824c14e-50e9-4d63-9bb6-fa82088b14fe", "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE."));
			consignee.AddAddressFormattedValidationDependencies();
			consignee.AddValidationDependencies(consignee.AddressFormattedInfo, notifyParty.CompanyNameInfo);
		}

		void AddNotifyPartyAddressValidation(AgencyHouseBill agencyHouseBill)
		{
			var notifyParty = agencyHouseBill.NotifyParty as Address;
			var consignee = agencyHouseBill.Consignee as Address;

			notifyParty.AddSameAsConsigneeSupport();

			notifyParty.AddressFormattedInfo.AddMessageError(() => (notifyParty.IsSameAsConsignee() || (string.IsNullOrWhiteSpace(notifyParty.CompanyName) || string.IsNullOrWhiteSpace(notifyParty.AddressLine1) || string.IsNullOrWhiteSpace(notifyParty.Country?.Name)))
				&& (string.IsNullOrWhiteSpace(consignee.CompanyName) || consignee.IsToOrder())
				, Res.GetString("7df3d181-a03d-420c-90c6-d6a6ec6217f7", "Notify Party name and address information is required, when Consignee is empty or TO ORDER."));
			notifyParty.AddAddressFormattedValidationDependencies();
			notifyParty.AddValidationDependencies(notifyParty.AddressFormattedInfo, consignee.CompanyNameInfo);
		}

		void AddShipperAddressValidation(AgencyHouseBill agencyHouseBill)
		{
			var shipper = agencyHouseBill.Shipper as Address;

			shipper.AddressFormattedInfo.AddMessageError(() => shipper.IsPartyNameAndAddressEmpty(), Res.GetString("e322ef08-346e-47c1-be9b-fd90d7d3fb14", "Shipper party name and address information is required."));
			shipper.AddAddressFormattedValidationDependencies();
		}

		#endregion

		#region LocationsValidation

		void AddLocationsValidation(AgencyHouseBill agencyHouseBill)
		{
			(agencyHouseBill.PortOfLoading as Unloco)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("12ebaa82-57ce-4c02-b53e-e48253902723", "Port of Lading is required."));
			(agencyHouseBill.PortOfDischarge as Unloco)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("9f021814-c501-4074-8bc0-157b53bd983b", "Port of Discharge is required."));
		}

		#endregion

		#region GoodsDetailsValidation

		void AddGoodsDetailsValidation(AgencyHouseBill agencyHouseBill)
		{
			agencyHouseBill.ErrorPlaceHolderInfo.AddMessageError(() => agencyHouseBill.MarksAndNumbers.IsEmpty, Res.GetString("92d892d2-d130-4819-aba9-10fd2562bfa4", "Marks and Numbers is required."));
			agencyHouseBill.ErrorPlaceHolderInfo.AddMessageError(() => agencyHouseBill.GoodsDescription.IsEmpty, Res.GetString("e4277b07-59d7-4aea-9f22-993d1737d35b", "Description of Goods is required."));

			(agencyHouseBill.TotalWeight as Measurement)?.ValueInfo.AddMessageErrorIfEmpty(Res.GetString("138e26c2-11ae-4fab-84f8-0a99ced8b07b", "Total Weight is required."));
			agencyHouseBill.ErrorPlaceHolderInfo.AddMessageError(() => agencyHouseBill.TotalWeight.Value.IsEmpty, Res.GetString("f92ee62d-ef4b-454d-ad15-b729ecd8d6fd", "Total Weight is required."));

			(agencyHouseBill.TotalVolume as Measurement)?.ValueInfo.AddMessageErrorIfEmpty(Res.GetString("6824fcd4-1fc8-42bb-aeb4-b306a03c6c1d", "Total Volume is required."));
			agencyHouseBill.ErrorPlaceHolderInfo.AddMessageError(() => agencyHouseBill.TotalVolume.Value.IsEmpty, Res.GetString("5ab3c5be-7524-4566-90c0-e983819ad6a7", "Total Volume is required."));
		}

		#endregion

		#endregion

		#region Implementation

		AgencyHouseBillLookups Lookups => lookups ?? (lookups = new AgencyHouseBillLookups(billOfLading.Factory));
		AgencyHouseBillLookups lookups;

		IReadOnlyCollection<Freight.Business.Transport> SeaTransportsInPortOrder
		{
			get
			{
				if (seaTransportsInPortOrder == null)
				{
					var sortedTransports = billOfLading
						.TransportsIncludingRelated
						.OfType<Freight.Business.Transport>()
						.Where(transport => transport.JW_TransportMode == TransportModes.Sea)
						.ToArray();
					MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
					seaTransportsInPortOrder = sortedTransports;
				}

				return seaTransportsInPortOrder;
			}
		}
		IReadOnlyCollection<Freight.Business.Transport> seaTransportsInPortOrder;

		Freight.Business.Transport MainSeaLeg
		{
			get
			{
				if (mainSeaLeg == null)
				{
					mainSeaLeg = SeaTransportsInPortOrder.FirstOrDefault(t => t.JW_TransportType == TransportPlanningType.MainVessel);
				}

				return mainSeaLeg;
			}
		}
		Freight.Business.Transport mainSeaLeg;

		#endregion
	}
}
