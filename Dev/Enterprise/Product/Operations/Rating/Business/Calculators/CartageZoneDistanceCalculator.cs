using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// This calculator is used when calculating rates based on transport zone sets.
	/// </summary>
	[CalculatorProperty(Items.ACIZoneData, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool4", RelatedTo = "UseACIZones")]
	public class CartageZoneDistanceCalculator : CartageCalculator
	{
		public CartageZoneDistanceCalculator(IRateLine master)
			: base(master)
		{
		}

		protected override void PerformPostItemCreationActions()
		{
			base.PerformPostItemCreationActions();

			//Bool4 is used to change whether it displays ACI Zones or Transport Zones
			Bool4Info.ValueChanged += delegate
			{
				ReloadCartageZones();
				ParentRateEntry.Factory.ClearCachedValue<ZBool>(Line.GetACICacheKey());
			};

			RateLineBizO.RefreshBinding();
		}

		public new const string Code = RatingCalculatorCodes.CartageZoneDistance;

		#region Properties

		public ZBool UseACIZones
		{
			get { return (ZBool)this[Items.ACIZoneData]; }
			set
			{
				this[Items.ACIZoneData] = value;
			}
		}

		#endregion

		#region Validation

		/// <summary>
		/// Returns the RateLineItems that belong to the same zone (ACI or DomesticZone) as the
		/// RateLineItem that was given as a parameter.
		/// </summary>
		protected override IEnumerable<RateLineItem> GetItemsForGridValidation(RateLineItem lineItem)
		{
			if (Line.UsesACIZones())
			{
				var tmF1Zone = lineItem.TM_F1Zone;

				return RateLineBizO.RateLineItems
					.Cast<RateLineItem>()
					.Where(x => x.TM_F1Zone == tmF1Zone);
			}
			else
			{
				var zonePK = lineItem.TM_TZ_DomesticZone;

				return RateLineBizO.RateLineItems
					.Cast<RateLineItem>()
					.Where(x => x.TM_TZ_DomesticZone == zonePK);
			}
		}

		protected override void ValidateTM_BreakCore(RateLineItem lineItem)
		{
			var zone = Line.UsesACIZones() ? CartageZones.FindZone(lineItem.TM_F1Zone) : CartageZones.FindZone(lineItem.TM_TZ_DomesticZone);
			if (zone != null)
			{
				new WeightBreakValidator(zone.ZoneRateLineItems, lineItem).CheckWeightBreak();
			}
		}

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			base.ValidateTM_Text(lineItem);
			if (lineItem.TM_Type == Items.ACIZoneData && Line.UsesACIZones())
			{
				var isValidOrigin = ParentRateEntry.IsOriginEntry()
					&& ParentRateEntry.Origin() != null
					&& ParentRateEntry.Origin().Country != null
					&& (ParentRateEntry.Origin().Country.Code == Constants.CountryCodes.UnitedStates || ParentRateEntry.Origin().Country.Code == Constants.CountryCodes.Canada);

				var isValidDestination = ParentRateEntry.IsDestinationEntry()
					&& ParentRateEntry.Destination() != null
					&& ParentRateEntry.Destination().Country != null
					&& (ParentRateEntry.Destination().Country.Code == Constants.CountryCodes.UnitedStates || ParentRateEntry.Destination().Country.Code == Constants.CountryCodes.Canada);

				if (!isValidOrigin && !isValidDestination)
				{
					lineItem.TM_TextInfo.AddError(Res.GetString("7767b402-ed59-4ab0-bb09-e697891caa66", "You can only specify the usage of ACI Zone Information for Origin rates from the United States or Canada, or Destination rates to the United States or Canada."));
				}
			}
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			if (UseACIZones)
			{
				CalculateUsingACIZoneInformation(calcOutput);
			}
			else
			{
				CalculateUsingNonACIZoneInformation(calcOutput);
			}
		}

		void CalculateUsingACIZoneInformation(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var aciZone = GetACIZone(parameters);
			if (aciZone != null)
			{
				var zone = GetZoneOrStandardZone(ZGuid.Empty, aciZone.F1_Zone);

				if (zone != null)
				{
					CalculateForItems(zone.ZoneRateLineItems.Cast<IRateLineItem>(), calcOutput);
					calcOutput.Set(calcOutput.PaymentBases.SetCartageZoneDescription(zone.ZoneName).ToList());
				}
				else
				{
					calcOutput.FailureMessage = Res.GetString("1589e60c-d369-431f-873b-44a2b1a1e6f6", "Zone {0} was not included in this rate.", aciZone.F1_Zone);
				}
			}
			else
			{
				calcOutput.FailureMessage = Res.GetString("dcca2068975eee4d-001e-4f69-9bfb-220dd2865b91", "no zone being found for the given details.");
			}
		}

		void CalculateUsingNonACIZoneInformation(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var zoneLog = new ZStringBuilder();
			var logger = parameters.Logger;
			var transportZone = GetTransportZone(parameters.Criteria, zoneLog, logger);
			var zone = FindAndLogCartageZone(transportZone, zoneLog, logger);

			if (zone != null)
			{
				var cartageZoneDescription = "";

				if (!zone.ZoneName.IsEmpty && transportZone != null && !transportZone.TransportProvider.TP_OH_RelatedParty.IsEmpty)
				{
					cartageZoneDescription += transportZone.TransportProvider.RelatedParty.OH_Code + " ";
				}

				cartageZoneDescription += zone.Description;
				CalculateForItems(zone.ZoneRateLineItems.Cast<IRateLineItem>(), calcOutput);
				calcOutput.Set(calcOutput.PaymentBases.SetCartageZoneDescription(cartageZoneDescription).ToList());
			}
			else
			{
				calcOutput.FailureMessage = Res.GetString("ca128670-b2e5-484c-924e-28139e9f5881", "no transport zone being found for the given details.");
			}
		}

		CartageZone GetZoneOrStandardZone(ZGuid transportZonePk, ZString aciZoneName)
		{
			CartageZone result = null;
			if (!transportZonePk.IsEmpty)
			{
				result = CartageZones.FindZone(transportZonePk);
			}
			else if (!aciZoneName.IsEmpty)
			{
				result = CartageZones.FindZone(aciZoneName);
			}

			if (result == null || result.ZoneRateLineItems.Count == 0)
			{
				result = HasStandardRate() ? CartageZones[0] : null;
			}

			return result;
		}

		RefDomesticCartageZone GetACIZone(AutoRatingCalculatorParameters parameters)
		{
			var (cartageAddress, documentaryAddress) = GetCartageAddressPair(parameters.Criteria);
			if (cartageAddress == null)
			{
				return null;
			}

			var portCode = cartageAddress.E2_PortCode;
			if (portCode.IsEmpty)
			{
				portCode = Line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Origin
					? parameters.Criteria.OriginCode
					: parameters.Criteria.DestinationCode;
			}

			// Though the below code refers to address's city, we don't use it as a mandatory field to lookup zones. Only postcode is mandatory.
			// An example is zoneA has postcode with empty city and an address with empty city can match that zone. See change in WI00026028.
			var addressToUse = cartageAddress;
			if (cartageAddress.E2_Postcode.IsEmpty && documentaryAddress != null)
			{
				addressToUse = documentaryAddress;
			}
			var postcode = addressToUse.E2_Postcode;
			if (portCode.IsEmpty || postcode.IsEmpty)
			{
				return null;
			}

			return RefDomesticCartageZone.GetZone(Line.Factory, postcode, portCode, addressToUse.E2_City);
		}

		#region Get Transport Zone (Non-ACI Zone)

		#region SuppressResourceStringsCheckRegion

		RateTransportZone GetTransportZone(RatingCriteria criteria, ZStringBuilder zoneLog, ILogger logger)
		{
			var (cartageAddress, documentaryAddress) = GetCartageAddressPair(criteria);
			var transportZoneSet = GetTransportZoneSet(criteria);

			if (transportZoneSet != null && !transportZoneSet.TP_IsActive)
			{
				logger.Warning(ZString.Format("{0} cannot be used as the transport zone set is not active. Please update the rates for {1}.", transportZoneSet.UniqueCode, Line.DisplayInfo()));
				transportZoneSet = null;
			}

			if (transportZoneSet == null)
			{
				return null;
			}

			var distanceHelper = new RatingDistanceCalculationHelper(criteria, Line);
			var distance = distanceHelper.GetDistance(zoneLog);
			var zone = RateTransportZoneHelper.GetZoneForDistance(transportZoneSet, cartageAddress?.CountryCode, distance);

			if (zone != null)
			{
				zoneLog.AppendLine(ZString.Format("- '{0}' matched by distance ({1})", zone.TZ_ZoneName, distance));
				return zone;
			}

			zoneLog.AppendLine(ZString.Format("- No transport zone could be matched by distance ({0})", distance));
			if (cartageAddress == null)
			{
				return null;
			}

			var checkResult = CheckAddressHasNonEmptyPostcodeOrCity(cartageAddress);
			var addressToUse = cartageAddress;
			// fallback to documentaryAddress when both postcode and city are empty
			if (checkResult == AddressCheckResult.None && documentaryAddress != null)
			{
				addressToUse = documentaryAddress;
			}

			// When country code is empty there will be no zone returned.
			// For now, just keep existing old behaviour with country code from address.
			zone = RateTransportZoneHelper.GetZoneForPostCodeOrCityTown(transportZoneSet, addressToUse.CountryCode, addressToUse.E2_Postcode, addressToUse.E2_City);
			var fallbackZoneMessage = zone != null
				? ZString.Format("- '{0}' transport zone matched by {1} fallback", zone.TZ_ZoneName, addressToUse.AddressCaption)
				: ZString.Format("- No transport zone could be matched by {0} fallback", addressToUse.AddressCaption);

			zoneLog.AppendLine(fallbackZoneMessage);

			return zone;
		}

		static AddressCheckResult CheckAddressHasNonEmptyPostcodeOrCity(IDocAddress address)
		{
			var result = AddressCheckResult.None;
			var postcode = address?.E2_Postcode ?? ZString.Empty;
			if (!postcode.IsEmpty)
			{
				result = AddressCheckResult.PostcodeOnly;
			}

			var city = address?.E2_City ?? ZString.Empty;
			if (!city.IsEmpty)
			{
				result |= AddressCheckResult.CityOnly;
			}

			return result;
		}

		[Flags]
		enum AddressCheckResult
		{
			None = 0,
			PostcodeOnly = 1,
			CityOnly = 2,
			PostcodeAndCity = PostcodeOnly | CityOnly
		}

		/// <summary>
		/// Typically there will be only one Transport Zone Set per CTZ Calculator.
		/// If an International Zone has countries, then we expect the most specific Transport Zone per Country.
		/// </summary>
		RateTransportProvider GetTransportZoneSet(RatingCriteria criteria)
		{
			RateTransportProvider transportZoneSet = null;

			var calculatorTransportZoneSets = GetTransportZoneSetsFromCalculatorCartageZones();
			if (calculatorTransportZoneSets.Length == 1)
			{
				transportZoneSet = calculatorTransportZoneSets[0];
			}
			else if (calculatorTransportZoneSets.Length > 1)
			{
				var isLocationAnInternationalZone = Line.GetCartageLocationForZones() is RefZoneHeader;
				if (!isLocationAnInternationalZone)
				{
					var message = "Contact the Rating Team. Calculator Zones should match only one Transport Zone Set unless the location is an International Zone.";
					throw new DeveloperNotificationException(message);
				}

				if (Line.ParentRateEntry.IsOriginEntry() && criteria.Origin != null)
				{
					transportZoneSet = calculatorTransportZoneSets.SingleOrDefault(zoneSet => criteria.Origin.Country.PK == zoneSet.Country.PK);
				}
				else if (Line.ParentRateEntry.IsDestinationEntry() && criteria.Destination != null)
				{
					transportZoneSet = calculatorTransportZoneSets.SingleOrDefault(zoneSet => criteria.Destination.Country.PK == zoneSet.Country.PK);
				}
			}

			return transportZoneSet;
		}

		#endregion

		RateTransportProvider[] GetTransportZoneSetsFromCalculatorCartageZones()
		{
			var transportZoneSets = Array.Empty<RateTransportProvider>();

			var listOfZones = CartageZones.Cast<CartageZone>();
			var zones = listOfZones.Select(zone => zone.ZonePK);
			var zonePKs = zones.Where(pk => !pk.IsEmpty);

			if (zonePKs.Any())
			{
				var subQuery = new ZDBOnlySubQuery(typeof(RateTransportZone), RateTransportZonesSchema.TZ_TP);
				subQuery.AddToFilter(RateTransportZonesSchema.PK, zonePKs);
				var query = new ZDBOnlyQuery(typeof(RateTransportProvider));
				query.AddSubQuery(subQuery, JoinCondition.And);

				transportZoneSets = Line.Factory.Load<RateTransportProvider>(query);
			}

			return transportZoneSets;
		}

		CartageZone FindAndLogCartageZone(RateTransportZone transportZone, ZStringBuilder zoneLog, ILogger logger)
		{
			CartageZone cartageZone = null;

			#region SuppressResourceStringsCheckRegion

			if (transportZone != null)
			{
				cartageZone = CartageZones.FindZone(transportZone.PK);

				if (cartageZone != null && cartageZone.ZoneRateLineItems.Count == 0)
				{
					zoneLog.AppendLine(ZString.Format("- '{0}' transport zone cannot be used as there are no rates set on the calculator", cartageZone.Description));
				}
			}

			if ((cartageZone == null || cartageZone.ZoneRateLineItems.Count == 0) && HasStandardRate())
			{
				cartageZone = CartageZones[0];
				zoneLog.AppendLine(ZString.Format("- '{0}' fall back used", cartageZone.Description));
			}

			//FreightAutoRater calls Calculate twice but criteria will always matches the same zone, so only log this the first time
			if (!Line.ViewAgentRates)
			{
				var logMessage = cartageZone == null
					? ZString.Format("No transport zone matched for RateLine {0}", Line.DisplayInfo())
					: ZString.Format("Matched '{0}' for RateLine {1}", cartageZone.Description, Line.DisplayInfo());

				if (!zoneLog.IsEmpty)
				{
					logMessage += System.Environment.NewLine;
					logMessage += zoneLog.ToString();
				}

				logger.Information(logMessage);
			}

			#endregion

			return cartageZone;
		}

		#endregion

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			if (HasZoneRates())
			{
				var result = new QuotationLineList();

				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));

				for (var i = 0; i < CartageZones.Count; i++)
				{
					if (CartageZones[i].ZoneRateLineItems.Count > 0)
					{
						string description = i == 0 ? (ZString)StandardRateText : CartageZones[i].ZoneName;
						result.AddRange(GetQuotationLinesForIterator(flags, CartageZones[i].ZoneRateLineItems.Cast<IRateLineItem>(), description));
					}
				}

				return result;
			}
			else
			{
				return base.GetQuotationLinesInternal(flags);
			}
		}

		static string StandardRateText => Res.GetString("cca96f42-51d8-41d7-9fb8-7ab4579a3f28", "Standard Rate");

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			if (HasZoneRates())
			{
				for (var i = 0; i < CartageZones.Count; i++)
				{
					if (CartageZones[i].ZoneRateLineItems.Count > 0)
					{
						var description = i == 0
							? (ZString)StandardRateText
							: CartageZones[i].ZoneName;
						var docLineAmount1 = GetDocLineAmountForIterator(CartageZones[i].ZoneRateLineItems.Cast<IRateLineItem>(), description);
						result = result + docLineAmount1;
					}
				}

				return result;
			}
			else
			{
				return base.GetDocLineAmount();
			}
		}

		#endregion

		#region Implementation

		bool HasStandardRate()
		{
			return (CartageZones[0].ZoneRateLineItems.Count > 0);
		}

		bool HasZoneRates() => CartageZones.Where(cartageZone => !cartageZone.ZoneName.IsEmpty).Cast<CartageZone>().Any(x => x.ZoneRateLineItems.Count > 0);

		#region Cartage Address For Zones (CNE/CNR Pickup or Delivery Address)

#if DEBUG
		internal
#endif
		(IDocAddress, IDocAddress) GetCartageAddressPair(RatingCriteria criteria)
		{
			if (criteria.RateTypeToUse == RateType.TransportBookings || criteria.RateTypeToUse == RateType.LocalTransport)
			{
				if (criteria.PickupAddress != null && criteria.DeliveryAddress != null)
				{
					var isDeliveryCTO = criteria.DeliveryAddress.E2_AddressType == DocAddressTypes.Codes.LocalCartageCTO;
					var isPickupCTO = criteria.PickupAddress.E2_AddressType == DocAddressTypes.Codes.LocalCartageCTO;
					var isDeliveryCFS = criteria.DeliveryAddress.E2_AddressType == DocAddressTypes.Codes.LocalCartageCFS;
					var isPickupCFS = criteria.PickupAddress.E2_AddressType == DocAddressTypes.Codes.LocalCartageCFS;

					// Cartage being delivered to a CTO or CFS should be treated as an export unless it's CTO->CTO or CTO/CFS->CFS
					if ((isDeliveryCTO && !isPickupCTO) || (isDeliveryCFS && !isPickupCTO && !isPickupCFS))
					{
						return (criteria.PickupAddress, criteria.ConsignorDocumentaryAddress);
					}

					return (criteria.DeliveryAddress, criteria.ConsigneeDocumentaryAddress);
				}
			}

			return GetCartageAddressPairByChargeGroup(criteria);
		}

		(IDocAddress, IDocAddress) GetCartageAddressPairByChargeGroup(RatingCriteria criteria)
		{
			switch (Line.ChargeCode.AC_ChargeGroup)
			{
				case ChargeCodeGroupList.Codes.WHSInwards:
				case ChargeCodeGroupList.Codes.OriginBrokerageOnly:
				case ChargeCodeGroupList.Codes.OriginBrokerage:
				case ChargeCodeGroupList.Codes.Origin:
					return (criteria.PickupAddress, criteria.ConsignorDocumentaryAddress);

				case ChargeCodeGroupList.Codes.WHSOutwards:
				case ChargeCodeGroupList.Codes.BrokerageOnly:
				case ChargeCodeGroupList.Codes.Brokerage:
				case ChargeCodeGroupList.Codes.Destination:
					return (criteria.DeliveryAddress, criteria.ConsigneeDocumentaryAddress);

				default:
					return (null, null);
			}
		}

		#endregion

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			clone.RateLineItems.RemoveAndDeleteAll();

			foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
			{
				if (item.RateOperatorIsNonPrintedFlag())
				{
					clone.RateLineItems.CloneItem(item);
				}
			}

			foreach (CartageZone zone in CartageZones)
			{
				if (zone.ZoneRateLineItems.Any())
				{
					if (UseACIZones)
					{
						CloneLineItemsUpdatingRateValuesWithZones(ctbCalc, clone, zone.ZoneRateLineItems, rateTypeToUpdate, zone.ZoneName);
					}
					else
					{
						CloneLineItemsUpdatingRateValuesWithZones(ctbCalc, clone, zone.ZoneRateLineItems, rateTypeToUpdate, zonePK: zone.ZonePK);
					}
				}
			}

			ReloadCartageZones();
		}

		#endregion

		#region DeleteOrphanedZones

		void DeleteOrphanedZones()
		{
			var lineItems = RateLineBizO.RateLineItems.Cast<RateLineItem>();
			// because CartageZones is such a tightly-bound property that has
			// side-effects ... it needs to be accessed on its own first
			var cartageZones = CartageZones;

			var invalidLineItems = UseACIZones
				? lineItems.Where(x => (!x.TM_F1Zone.IsEmpty && !cartageZones.ContainsZone(x.TM_F1Zone)) || !x.TM_TZ_DomesticZone.IsEmpty)
				: lineItems.Where(x => (!x.TM_TZ_DomesticZone.IsEmpty && !cartageZones.ContainsZone(x.TM_TZ_DomesticZone)) || !string.IsNullOrEmpty(x.TM_F1Zone));

			foreach (var item in invalidLineItems.ToList())
			{
				item.Delete();
			}
		}

		internal static void DeleteOrphanedZonesIfNeeded(RateEntry entry)
		{
			if (entry.IsDeleted || (entry.Parent?.IsDeleted ?? true))
			{
				return;
			}

			IEnumerable<RateLine> cartageZoneDistanceRateLines = null;

			if (entry.IsRateLinesLoaded &&
				(entry.TI_OriginLRCInfo.HasChanges
					|| entry.TI_DestinationLRCInfo.HasChanges
					|| entry.TI_OH_SupplierInfo.HasChanges
					|| entry.Parent.TH_OHInfo.HasChanges
					|| GetCartageZoneDistanceRateLines(entry).Any(rateLine => ShouldDeleteOrphanedZones(rateLine))))
			{
				foreach (var rateLine in GetCartageZoneDistanceRateLines(entry))
				{
					rateLine.GetCalculator<CartageZoneDistanceCalculator>()?.DeleteOrphanedZones();
				}
			}

			bool ShouldDeleteOrphanedZones(RateLine rateLine)
			{
				if (rateLine.RateLineItems.Any(rateLineItem => rateLineItem.HasChanges))
				{
					return true;
				}

				var cartageZones = ((CartageZoneDistanceCalculator)rateLine.Calculator).CartageZones;
				return rateLine.UsesACIZones()
					? rateLine.RateLineItems.Cast<RateLineItem>().Any(rateLineItem => cartageZones.FindZone(rateLineItem.TM_F1Zone) == null)
					: rateLine.RateLineItems.Cast<RateLineItem>().Any(rateLineItem => cartageZones.FindZone(rateLineItem.TM_TZ_DomesticZone) == null);
			}

			IEnumerable<RateLine> GetCartageZoneDistanceRateLines(RateEntry rateEntry)
				=> cartageZoneDistanceRateLines ??= rateEntry.RateLines.Cast<RateLine>().Where(rateLine => rateLine.RateCalculatorType == CalculatorType.CartageZoneDistance);
		}

		#endregion

	}

#if DEBUG
	[TestOnlyCalculator]
	public class CartageCalculatorWithOverride : CartageZoneDistanceCalculator
	{
		public CartageCalculatorWithOverride(IRateLine master)
			: base(master)
		{
		}

		public ZString DetermineDefaultEquipment_Exposed()
		{
			return base.DetermineDefaultEquipment();
		}

		public new const string Code = "XBT";
	}

#endif
}
