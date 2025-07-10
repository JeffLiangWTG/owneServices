using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ
{
	sealed class ExportPreAdviceNotificationBuilder
	{
		public ExportPreAdviceNotificationBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			this.parameters = parameters;
			this.context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;

		public ExportPreAdviceNotification Build()
		{
			var exportPreAdviceNotification = new ExportPreAdviceNotification(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);
			var containersToShow = parameters?.Data as IReadOnlyCollection<ForwardingContainer> ?? System.Array.Empty<ForwardingContainer>();

			exportPreAdviceNotification.BookingConfirmationReference = consol.JK_BookingReference;
			PopulateGeneral(exportPreAdviceNotification);
			PopulateContainers(exportPreAdviceNotification, containersToShow);

			ValidateGeneral(exportPreAdviceNotification);
			ValidateContainers(exportPreAdviceNotification);
			exportPreAdviceNotification.ValidateAllIncludingChildren();
			return exportPreAdviceNotification;
		}

		#region Populations

		void PopulateGeneral(ExportPreAdviceNotification notification)
		{
			notification.Shipper = consol.IsDirect
				? AddressBuilder.Create(context, consol.DirectShipment?.ConsignorDocumentaryAddress)
				: AddressBuilder.Create(context, consol.SendingForwarderWithContact);

			var cusCode = consol.IsDirect
				? consol.DirectShipment?.Consignor?
					.ConfigOrg
					.CustomsCodes
					.Cast<OrgCusCode>()
					.FirstOrDefault(cusCode => cusCode.OK_CodeType == OrgCusCode.CodeTypes.PortSystemNumber)
				: consol.SendingForwarderAddress?
					.Header
					.CustomsCodes
					.OfType<OrgCusCode>()
					.FirstOrDefault(cusCode => cusCode.OK_CodeType == OrgCusCode.CodeTypes.PortSystemNumber);
			notification.ShipperCode = cusCode?.OK_CustomsRegNo ?? notification.Shipper?.CompanyName ?? ZString.Empty;

			notification.Carrier = AddressBuilder.Create(context, consol.ShippingLineAddress);
			notification.CarrierCode = consol.ShippingLineAddress?.GetRegistrationNumberWithFallbackToOrgHeader(OrgCusCode.CodeTypes.PortSystemNumber, Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
			notification.CarrierBookingReference = consol.JK_BookingReference;
			notification.FreightForwardersReference = consol.JK_UniqueConsignRef;

			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));
			var transports = consol.Transports.OfType<Freight.Business.Transport>().ToArray();
			var firstSeaLeg = transports.FirstOrDefault(x => x.TransportMode == TransportModes.Sea
				&& (x.LoadPort?.Country.Code.EqualsIgnoringCase(CountryCodes.NewZealand) ?? false));

			notification.Vessel = Vessel.Create(context, firstSeaLeg);
			notification.Voyage = firstSeaLeg?.JW_VoyageFlight ?? ZString.Empty;
			notification.PortOfLoad = Unloco.Create(context, firstSeaLeg?.LoadPort);
			notification.PortOfDischarge = Unloco.Create(context, firstSeaLeg?.DiscPort);

			var preCarriageLegMode = transports.OrderByDescending(x => x.JW_LegOrder)
				.FirstOrDefault(x => x.JW_LegOrder < firstSeaLeg?.JW_LegOrder)?.TransportMode ?? ZString.Empty;
			var modeLookup = new CodeDescriptionPairList();
			modeLookup.AddPair(ZString.Empty, ZString.Empty);
			modeLookup.AddPair(TransportModes.Road, nameof(TransportModes.Road));
			modeLookup.AddPair(TransportModes.Rail, nameof(TransportModes.Rail));
			notification.PreCarriageMode = new CodeDescription(modeLookup)
			{
				Code = preCarriageLegMode == TransportModes.Road || preCarriageLegMode == TransportModes.Rail
					? preCarriageLegMode : ZString.Empty
			};

			var origin = consol.LoadPort;
			if (consol.Shipments.Any())
			{
				var shipments = consol.Shipments.Cast<ForwardingShipment>().ToArray();
				var shipment = shipments.FirstOrDefault(x => !x.JS_RL_NKOrigin.IsEmpty);
				if (shipment != null && shipments.All(x => x.JS_RL_NKOrigin == shipment.JS_RL_NKOrigin))
				{
					origin = shipment.Origin;
				}
			}
			notification.Origin = Unloco.Create(context, origin);

			notification.DepartureCTOAddress = AddressBuilder.Create(context, consol.DepartureCTOAddress);
			var loadPortFacility = consol.DepartureCTOAddress?.Header.CustomsCodes.OfType<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.CodeTypes.PortSystemNumber);
			notification.LoadPortFacility = loadPortFacility?.OK_CustomsRegNo ?? ZString.Empty;
			notification.OperationalPort = Unloco.Create(context, consol.DepartureCTOAddress?.RelatedPortCode);
		}

		void PopulateContainers(ExportPreAdviceNotification notification, IReadOnlyCollection<ForwardingContainer> containers)
		{
			var containerBuilder = new ContainerBuilder();
			var containerCollection = new List<Container>();
			var packLineBuilder = new PackingLineBuilder();

			foreach (var containerBizObj in containers)
			{
				var packLines = containerBizObj.PackLines?.Cast<PackLine>().Select(p => packLineBuilder.Build(p)).ToArray();
				var container = containerBuilder.Build(containerBizObj, context, packLines);
				container.RefrigerationType = containerBizObj.IsChiller ? (NoResString)"Chilled" : containerBizObj.IsFreezer ? (NoResString)"Frozen" : ZString.Empty;
				container.EPANStatus = containerBizObj.JC_EPANStatus;

				var packLine = packLines?.OrderByDescending(x => x.Weight.Value)
					.FirstOrDefault(x => !x.HarmonizedCode.Code.IsEmpty);
				container.HSCode = packLine?.HarmonizedCode.Code ?? ZString.Empty;

				containerCollection.Add(container);
			}

			notification.Containers = containerCollection.OrderBy(container => container.Number).ToArray();
		}

		#endregion

		#region Validations

		void ValidateGeneral(ExportPreAdviceNotification notification)
		{
			notification.Shipper.AddPartyNameAndAddressValidation((NoResString)"Sending")
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			notification.ShipperCodeInfo.AddMessageErrorIfEmpty((NoResString)"Sending Party Code is required.");

			notification.Carrier.AddPartyNameAndAddressValidation((NoResString)"Carrier")
				.AddContactDetailsValidation()
				.AddAsciiCharactersValidation();

			notification.CarrierCodeInfo.AddMessageErrorIfEmpty((NoResString)"Carrier Code is required. Please provide in Organization > Config > Registration Numbers / Codes - type 'PSN', Country 'NZ'.");

			notification.CarrierBookingReferenceInfo.AddMessageErrorIfEmpty((NoResString)"Carrier Booking Reference is required.");
			notification.FreightForwardersReferenceInfo.AddMessageErrorIfEmpty((NoResString)"Freight Forwarders Reference is required.");
			notification.Vessel.NameInfo.AddMessageErrorIfEmpty((NoResString)"Vessel Name is required.");
			notification.VoyageInfo.AddMessageErrorIfEmpty((NoResString)"Voyage is required.");

			notification.PortOfLoad.AddRequiredValidation((NoResString)"Port Of Load")
				.AddAsciiCharactersValidation();

			notification.PortOfDischarge.AddRequiredValidation((NoResString)"Port Of Discharge")
				.AddAsciiCharactersValidation();

			notification.Origin.AddRequiredValidation((NoResString)"Origin")
				.AddAsciiCharactersValidation();

			notification.LoadPortFacilityInfo.AddMessageErrorIfEmpty((NoResString)"Load Port Facility is required.");

			notification.OperationalPort.AddRequiredValidation((NoResString)"Operational Port")
				.AddAsciiCharactersValidation();
		}

		void ValidateContainers(ExportPreAdviceNotification notification)
		{
			foreach (var container in notification.Containers)
			{
				container.NumberInfo.AddMessageErrorIfEmpty((NoResString)"Container number is required.");
				container.NumberInfo.AddMessageError(() => !container.Number.IsEmpty && !container.IsShipperOwned && !ContainerNumberValidation.IsValidContainerNumber(container.Number),
					(NoResString)"Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

				container.Type.ISOCodeInfo.AddMessageErrorIfEmpty((NoResString)"ISO code is required.");
				container.GoodsWeight.ValueInfo.AddMessageErrorIfEmpty((NoResString)"Net Weight is required.");
				((Measurement)container.GrossWeight).ValueInfo.AddMessageErrorIfEmpty((NoResString)"Gross Weight is required.");
				container.HSCodeInfo.AddMessageErrorIfEmpty((NoResString)"HS Code is required.");

				if (notification.OperationalPort.Code == "NZLYT")
				{
					container.VerifiedMethod.CodeInfo.AddMessageErrorIfEmpty((NoResString)"Verified Method is required.");
					container.VerifiedDateInfo.AddMessageErrorIfEmpty((NoResString)"Verified Date is required.");
					container.VerifiedByAddress.CompanyNameInfo.AddMessageErrorIfEmpty((NoResString)"Verified By is required.");
				}

				var isFlatRacksCode = new Regex(@"^.{2}[Pp].{1}$").IsMatch(container.Type.ISOCode);
				container.SealInfo.AddMessageError(() => !isFlatRacksCode && container.Seal.IsEmpty, (NoResString)"Seal is required.");
				((CodeDescription)container.SealPartyType).CodeInfo.AddMessageError(() => !isFlatRacksCode && container.SealPartyType.Code.IsEmpty, (NoResString)"Seal Party Code is required.");

				container.HSCodeInfo.AddWarning(() => ValidateHSCode(container),
					(NoResString)"Multiple HS Codes have been found on different Pack Lines in this Container, please validate if the correct one has been selected");
			}
		}

		bool ValidateHSCode(Container container)
		{
			if (container.PackingLines != null && container.PackingLines.Count > 1)
			{
				var code = ZString.Empty;
				foreach (var packLine in container.PackingLines)
				{
					if (!packLine.HarmonizedCode.Code.IsEmpty)
					{
						if (code.IsEmpty)
						{
							code = packLine.HarmonizedCode.Code;
						}
						else if (!code.EqualsIgnoringCase(packLine.HarmonizedCode.Code))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		#endregion
	}
}
