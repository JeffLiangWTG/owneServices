using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public class RateLinesLookups : AutoRateLinesLookups
	{
		public RateLinesLookups(AutoRateLines parent)
			: base(parent)
		{
			RateLine = (IRateLine)parent;
			Factory = parent.Factory;
		}

		public RateLinesLookups(IRateLine parent, BusinessObjectFactory factory) : base(null)
		{
			RateLine = parent;
			Factory = factory;
		}

		protected override BusinessObjectFactory Factory { get; }

		IRateLine RateLine { get; }

		#region Zones

		#region SuppressResourceStringsCheckRegion

		public CodeDescriptionPairList Zones
		{
			get
			{
				var zones = new CodeDescriptionPairList();
				var relevantLocation = RateLine.GetCartageLocationForZones();
				if (relevantLocation != null)
				{
					if (RateLine.Uses(CalculatorType.CartageZoneDistance) && RateLine.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones)
					{
						zones = Factory.GetCachedValue("RateLinesLookups.ACI." + relevantLocation.Code, () => GetACIZones(relevantLocation.Code));
					}
					else
					{
						var zoneOwner = GetTransportZoneSetOwner(RateLine.ParentRateEntry);
						var locationCode = relevantLocation is OrgAddress address ? address.PK.ToString() : relevantLocation.Code.ToString();
						var rateKey = string.Join(".", new string[]
						{
							"RateLinesLookups.Transport",
							zoneOwner?.OH_Code ?? "Generic",
							locationCode,
							RateLine.ParentRateEntry.TI_Mode
						}.Where(value => !string.IsNullOrEmpty(value))); // Codes should not be translated.

						zones = Factory.GetCachedValue(rateKey, () => GetActiveTransportZonesNamesList(zoneOwner, relevantLocation));
					}
				}

				return zones;
			}
		}

		CodeDescriptionPairList GetACIZones(ZString relevantPort)
		{
			var result = new CodeDescriptionPairList();

			var filter = new ZQuery();
			filter.AddToFilter(RefDomesticCartageZoneSchema.F1_RL_NKLoco, SQLComparisonOperator.Like, relevantPort + "%");
			filter.AddToFilter(RefDomesticCartageZoneSchema.F1_Zone, SQLComparisonOperator.NotEqual, "");

			var zones = Factory.Load<RefDomesticCartageZone>(filter);
			var zonesNamesNoRepeats = zones
				.Select(z => z.F1_Zone)
				.Distinct();

			foreach (var zone in zonesNamesNoRepeats)
			{
				result.AddPair(zone, zone);
			}

			return result;
		}

		IEnumerable<RateTransportProvider> GetActiveTransportZoneSets(OrgHeader orgHeader, ILocation relevantLocation)
		{
			var transportZoneSet = RateTransportZoneHelper.GetTransportZoneSets(relevantLocation, orgHeader, RatingConstants.RatingZoneTypes.Rating, RateLine.ParentRateEntry.TI_Mode, Factory);
			var ratingHeaderParent = RateLine.ParentRateEntry.ParentRatingHeader.Header;

			if (orgHeader != ratingHeaderParent && !transportZoneSet.Any(p => p != null && p.TP_OH_RelatedParty == orgHeader.PK))
			{
				transportZoneSet = GetActiveTransportZoneSets(ratingHeaderParent, relevantLocation);
			}

			return transportZoneSet;
		}

		CodeDescriptionPairList GetActiveTransportZonesNamesList(OrgHeader orgHeader, ILocation relevantLocation)
		{
			var transportZoneSets = GetActiveTransportZoneSets(orgHeader, relevantLocation);
			var result = new CodeDescriptionPairList();

			foreach (var zoneSet in transportZoneSets.Where(z => z != null))
			{
				foreach (var zone in zoneSet.Zones)
				{
					if (zone.TZ_IsActive)
					{
						result.AddPair(zone.PK, zone.TZ_ZoneName, zone.TZ_ZoneName);
					}
				}
			}

			return result;
		}

		static OrgHeader GetTransportZoneSetOwner(IRateEntry entry)
		{
			OrgHeader result = null;
			if (entry.Supplier != null && !entry.IsCosting())
			{
				result = entry.Supplier;
			}

			return result ?? entry.ParentRatingHeader.Header;
		}

		#endregion

		#endregion

		#region Charge Codes

		public override AccChargeCodeCollection ChargeCodes
		{
			get
			{
				var cacheKeyForRateLinesChargeCodes = GetFactoryCacheKeyForRateLinesChargeCodes();

				if (RateLine.RateCalculatorType != CalculatorType.FreightInclusive)
				{
					return Factory.GetCachedValue(cacheKeyForRateLinesChargeCodes, () => GetChargeCodes(false));
				}

				// let's not cache charge codes for FRT calculator for now - considering the complexity of logic to have them
				// another WI should carefully create a correct cache key
				var chargeCodes = GetChargeCodes(false);
				return chargeCodes;
			}
		}

		public AccChargeCodeCollection GetChargeCodes(bool isFromCalculator)
		{
			var (filter, chargeGroupWarning) = GetChargeCodesFilter(isFromCalculator);

			AccChargeCodeCollection result;
			var entry = RateLine.ParentRateEntry;
			var company = entry.Company();
			if (company == null || entry != null && entry.IsPublished)
			{
				result = new GlobalChargeCodesCollection(Factory, filter, Guid.Empty);
			}
			else
			{
				result = new AccChargeCodeCollection(Factory, filter, company.PK.ToGuid());
			}

			result.SetOverrideNotificationWhenAdditionalFilterNotMet(chargeGroupWarning);
			SetDefaultChargeGroupFilterForChargeCodesCollection(result);

			return result;
		}

		(ZQuery, string) GetChargeCodesFilter(bool isFromCalculator)
		{
			var entry = RateLine.ParentRateEntry;

			var validGroups = GetChargeGroupsByParentEntryMode(entry);
			var invalidTypes = GetInvalidChargeCodeTypes(entry);

			var filter = new ZQuery(AccChargeCodeSchema.AC_RateCalculator, SQLComparisonOperator.NotEqual, ZString.Empty);
			filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.NotEqual, invalidTypes);
			filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
			if (validGroups.Any())
			{
				filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_ChargeGroup, validGroups);
			}

			var chargeGroupWarning = Res.GetString("a9652b34-5bed-4dd7-87ad-98cf4d3c3cbd", @"For a charge code to be valid for this rate line it must
 - be active
 - have a Rate Calculator
 - have charge group of {0}
 - not have charge type of {1}",
				string.Join(", ", validGroups),
				new ZStringBuilder(invalidTypes).ToStringWithDelimiterBetweenAppends(", "));

			const string whitespace = " ";

			if (entry != null && RateLine.RateCalculatorType == CalculatorType.FreightInclusive)
			{
				if (isFromCalculator)
				{
					filter.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, RateLine.TL_AC);
				}
				else
				{
					var excludedChargeCodes = RateLine.ParentRateEntry.ChildRateLines
						.Where(l => l.PK.IsValid && l.PK != RateLine.PK && l.RateCalculatorType == CalculatorType.FreightInclusive)
						.SelectMany(x => x.ChildRateLineItems)
						.Where(i => i.TM_Type == FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType && !i.TM_AC.IsEmpty)
						.Select(i => i.TM_AC)
						.Distinct();

					foreach (var excludedChargeCode in excludedChargeCodes)
					{
						filter.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, excludedChargeCode);
					}

					chargeGroupWarning += System.Environment.NewLine + whitespace + Res.GetString("d9a9399e-b673-46fe-bffe-22c4b0440bff", "- not be part of any charge using Freight Inclusive Calculator");
				}
			}

			var company = entry.Company();
			if (company == null)
			{
				chargeGroupWarning += System.Environment.NewLine + whitespace + Res.GetString("a2141f9e-eff8-4c90-b4af-9fc007db51f7", "- be a Global charge");
			}

			return (filter, chargeGroupWarning);
		}

		#region FactoryCacheKey

		#region SuppressResourceStringsCheckRegion

		string GetFactoryCacheKeyForRateLinesChargeCodes()
		{
			var entry = RateLine.ParentRateEntry;

			if (entry == null)
			{
				return Invariant($"RateLinesLookups.ChargeCodes.{RateLine.IsBulkRateUpdateActionLine()}");
			}

			return Invariant($"RateLinesLookups.ChargeCodes.{entry.PK}.{entry.TI_RateCategory}.{entry.Company()?.PK.ToString() ?? "null"}.{entry.IsPublished}.{RateLine.IsBulkRateUpdateActionLine()}");
		}

		#endregion

		#endregion

		#region Valid Charge Code Group List

		public bool IsChargeGroupApplicableToRateLine(string groupToCheck)
		{
			return GetChargeGroupsByParentEntryMode(RateLine.ParentRateEntry).Any(chargeGroup => chargeGroup == groupToCheck);
		}

		string[] GetChargeGroupsByParentEntryMode(IRateEntry entry)
		{
			if (entry == null)
			{
				return Array.Empty<string>();
			}

			if (entry.IsForwarding() && RateLine.IsBulkRateUpdateActionLine())
			{
				return new[]
					{
							ChargeCodeGroupList.Codes.Origin,
							ChargeCodeGroupList.Codes.OriginBrokerage,
							ChargeCodeGroupList.Codes.OriginBrokerageOnly,
							ChargeCodeGroupList.Codes.Loading,
							ChargeCodeGroupList.Codes.Freight,
							ChargeCodeGroupList.Codes.Insurance,
							ChargeCodeGroupList.Codes.Unloading,
							ChargeCodeGroupList.Codes.Destination,
							ChargeCodeGroupList.Codes.Brokerage,
							ChargeCodeGroupList.Codes.BrokerageOnly,
							ChargeCodeGroupList.Codes.CustomsDuty,
						};
			}

			if (entry.IsForwarding())
			{
				return RatingConstants.GetForwardingChargeCodeGroupsByRateCategory()[entry.TI_RateCategory];
			}

			if (entry.IsCustoms())
			{
				return RatingConstants.GetCutomsChargeCodeGroupsByRateCategory()[entry.TI_RateCategory];
			}

			if (entry.IsShipping())
			{
				if (entry.IsOriginEntry())
				{
					return new[]
					{
						ChargeCodeGroupList.Codes.Origin,
						ChargeCodeGroupList.Codes.Loading,
					};
				}

				if (entry.IsDestinationEntry())
				{
					return new[]
					{
						ChargeCodeGroupList.Codes.Destination,
						ChargeCodeGroupList.Codes.Unloading
					};
				}

				return new[]
				{
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance
				};
			}

			if (entry.IsShippingExportDetention())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.Origin
				};
			}

			if (entry.IsShippingImportDetention())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.Destination
				};
			}

			if (entry.TI_RateCategory == RatingConstants.RateCategory.CST)
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.ContainerStorage
				};
			}

			if (entry.IsCFS())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.CFSLoadList,
					ChargeCodeGroupList.Codes.CFSShipment
				};
			}

			if (entry.IsWHS())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.WHSInwards,
					ChargeCodeGroupList.Codes.WHSOutwards,
					ChargeCodeGroupList.Codes.WHSStorage,
					ChargeCodeGroupList.Codes.WHSAdHocServiceJob
				};
			}

			if (entry.IsTRW())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.TRWReceive,
					ChargeCodeGroupList.Codes.TRWDispatch,
					ChargeCodeGroupList.Codes.TRWDispatchLoadList
				};
			}

			if (entry.IsTWU())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit,
					ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit
				};
			}

			if (entry.IsDomesticTransport())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.TransportBooking
				};
			}

			if (entry.IsPortTransport())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.Transport
				};
			}

			if (entry.IsContainerYard())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.YardGateIn,
					ChargeCodeGroupList.Codes.YardGateOut,
					ChargeCodeGroupList.Codes.YardStorage,
					ChargeCodeGroupList.Codes.MNRWorkOrderHeader,
					ChargeCodeGroupList.Codes.LabourHourRate
				};
			}

			if (entry.IsContainerYardTPU())
			{
				return new[]
				{
					ChargeCodeGroupList.Codes.YardTransportationUnitGateIn,
					ChargeCodeGroupList.Codes.YardTransportationUnitGateOut,
				};
			}

			return Array.Empty<string>();
		}

		#endregion

		#region Invalid Charge Code Types

		static IEnumerable<string> GetInvalidChargeCodeTypes(IRateEntry entry)
		{
			var result = new List<string> { ChargeType.NonAccrual };
			if (entry != null)
			{
				if (entry.IsCosting())
				{
					result.Add(ChargeType.Revenue);
				}

				if (entry.ParentRatingHeader != null && entry.ParentRatingHeader.IsQuote())
				{
					result.Add(ChargeType.Comment);
				}
			}

			return result;
		}

		#endregion

		#region SetDefaultChargeGroupFilterForChargeCodesCollection

		/// <summary>
		/// For a given Charge Code Collection, used in a findbox, this method will set the default 
		/// filter for the Charge Group field based on whether this Rate Line is a Freight, Origin, Destination
		/// or CFS line.
		/// </summary>
		/// <param name="collection">The Charge Code Collection on which this filter default should be set.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")] // just different cases + ifelse
		void SetDefaultChargeGroupFilterForChargeCodesCollection(AccChargeCodeCollection collection)
		{
			var entry = RateLine.ParentRateEntry;
			if (entry != null && !RateLine.IsBulkRateUpdateActionLine())
			{
				var chargeGroupDefault = ZString.Empty;

				switch (entry.TI_RateCategory)
				{
					case RatingConstants.RateCategory.AIR:
					case RatingConstants.RateCategory.FCL:
					case RatingConstants.RateCategory.LCL:
					case RatingConstants.RateCategory.CAI:
					case RatingConstants.RateCategory.CFC:
					case RatingConstants.RateCategory.CLC:
						chargeGroupDefault = ChargeCodeGroupList.Codes.Freight;
						break;

					case RatingConstants.RateCategory.ORG:
					case RatingConstants.RateCategory.COR:
						chargeGroupDefault = AccChargeCodeLookups.OriginAndLoadingGroupFilterCode;
						break;

					case RatingConstants.RateCategory.DST:
					case RatingConstants.RateCategory.CDS:
						chargeGroupDefault = AccChargeCodeLookups.DestinationAndUnloadingGroupFilterCode;
						break;

					case RatingConstants.RateCategory.CST:
						chargeGroupDefault = ChargeCodeGroupList.Codes.ContainerStorage;
						break;

					case RatingConstants.RateCategory.TRN:
						chargeGroupDefault = ChargeCodeGroupList.Codes.Transport;
						break;

					case RatingConstants.RateCategory.TBC:
						chargeGroupDefault = ChargeCodeGroupList.Codes.TransportBooking;
						break;
				}

				if (entry.IsCFS())
				{
					chargeGroupDefault = AccChargeCodeLookups.CFSGroupFilterCode;
				}
				else if (entry.IsTRW())
				{
					chargeGroupDefault = AccChargeCodeLookups.TRWGroupFilterCode;
				}
				else if (entry.IsTWU())
				{
					chargeGroupDefault = AccChargeCodeLookups.TWUGroupFilterCode;
				}
				else if (entry.IsWHS())
				{
					chargeGroupDefault = AccChargeCodeLookups.WHSGroupFilterCode;
				}
				else if (entry.IsContainerYard())
				{
					chargeGroupDefault = AccChargeCodeLookups.CYDGroupFilterCode;
				}
				else if (entry.IsContainerYardTPU())
				{
					chargeGroupDefault = AccChargeCodeLookups.CYUGroupFilterCode;
				}

				if (!chargeGroupDefault.IsEmpty)
				{
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Charge Group", "Property", chargeGroupDefault)); // Filter related.
				}
			}
		}

		#endregion

		#endregion

		#region Rating Conditions

		public static string GetRateLineConditionsDescription(IRateLine line)
		{
			return GetRateLineConditions(line.Factory, line.ParentRateEntry).GetDescriptionFromCode(line.TL_Condition);
		}

		public CodeDescriptionPairList RateLineConditions
		{
			get { return GetRateLineConditions(Factory, RateLine.ParentRateEntry); }
		}

		static CodeDescriptionPairList GetRateLineConditions(BusinessObjectFactory factory, IRateEntry rateEntry)
		{
			return factory.GetCachedValue(GetKey(rateEntry), () => GetRateLineConditionsList(rateEntry));
		}

		static string GetKey(IRateEntry entry)
		{
			if (entry != null)
			{
				return "RateLinesLookups.RateLineConditions" + entry.RateType();
			}

			return "";
		}

		static CodeDescriptionPairList GetRateLineConditionsList(IRateEntry entry)
		{
			return entry != null
				? BuildConditionsList(entry.ParentRatingHeader.RateTypeSafe(), entry.RateType())
				: new CodeDescriptionPairList();
		}

		public static CodeDescriptionPairList BuildConditionsList(string ratingHeaderType, RateType rateType)
		{
			var list = new CodeDescriptionPairList();

			if ((rateType & RateType.Forwarding) != 0)
			{
				list.Add(new CodeDescriptionPair(MasterFiles.Business.RateLineConditions.OwnBrokerage, MasterFiles.Business.RateLineConditions.Descriptions.OwnBrokerage));
				list.Add(new CodeDescriptionPair(MasterFiles.Business.RateLineConditions.HandOver, MasterFiles.Business.RateLineConditions.Descriptions.HandOver));
				list.Add(new CodeDescriptionPair(MasterFiles.Business.RateLineConditions.ForwardingAndBrokerage, MasterFiles.Business.RateLineConditions.Descriptions.ForwardingAndBrokerage));
				list.Add(new CodeDescriptionPair(MasterFiles.Business.RateLineConditions.OwnCFS, MasterFiles.Business.RateLineConditions.Descriptions.OwnCFS));
				list.Add(new CodeDescriptionPair(MasterFiles.Business.RateLineConditions.OwnControllingAgent, MasterFiles.Business.RateLineConditions.Descriptions.OwnControllingAgent));

				if (ratingHeaderType != RatingConstants.RatingHeaderTypes.IntercompanyTariff)
				{
					list.Add(new CodeDescriptionPair(MasterFiles.Business.RateLineConditions.OwnGateway, MasterFiles.Business.RateLineConditions.Descriptions.OwnGateway));
				}
			}

			var dangerousGoodsRateTypes = RateType.Forwarding | RateType.Shipping | RateType.TransportBookings | RateType.LocalTransport;
			if ((rateType & dangerousGoodsRateTypes) != 0)
			{
				list.Add(new CodeDescriptionPair(MasterFiles.Business.RateLineConditions.DangerousGoods, MasterFiles.Business.RateLineConditions.Descriptions.DangerousGoods));
			}

			list.Add(new CodeDescriptionPair(MasterFiles.Business.RateLineConditions.UserDefined, MasterFiles.Business.RateLineConditions.Descriptions.UserDefined));

			return list;
		}

		#endregion

		#region Container Ownerships

		public ReadOnlyCodeDescriptionPairList ContainerOwnerships => GetContainerOwnerships(RateLine);

		public static string GetContainerOwnershipsDescription(IRateLine line) =>
			GetContainerOwnerships(line).GetDescriptionFromCode(line.TL_ContainerOwnership);

		static ReadOnlyCodeDescriptionPairList GetContainerOwnerships(IRateLine line) =>
			line.TL_WeightVolume == QuantityUnit.CN
				? line.Factory.GetCachedValue("RateLinesLookups.ContainerOwnerships", BuildContainerOwnershipList)
				: new CodeDescriptionPairList();

		public static CodeDescriptionPairList BuildContainerOwnershipList()
		{
			return new CodeDescriptionPairList
			{
				new CodeDescriptionPair(ContainerOwnership.Codes.CarrierOwned, ContainerOwnership.Descriptions.CarrierOwned),
				new CodeDescriptionPair(ContainerOwnership.Codes.ShipperOwned, ContainerOwnership.Descriptions.ShipperOwned)
			};
		}

		#endregion

		#region Rate Calculator List

		public CodeDescriptionPairList RateCalculators
		{
			get
			{
				var entry = RateLine?.ParentRateEntry;
				if (entry != null && entry.IsContainerYard())
				{
					return Factory.GetCachedValue("RateLinesLookups.YardRateCalculators", () => new CodeDescriptionPairList(OLookUpEditType.RateCalculators));
				}
				else
				{
					return Factory.GetCachedValue("RateLinesLookups.RateCalculators", GetCalculatorsList);
				}
			}
		}

		CodeDescriptionPairList GetCalculatorsList()
		{
			var rateCalculatorList = new CodeDescriptionPairList(OLookUpEditType.RateCalculators);
			rateCalculatorList.RemoveCode(RatingCalculatorCodes.CombinedWithIncrement);
			return rateCalculatorList;
		}

		#endregion

		#region Units List

		public CodeDescriptionPairList WeightVolumes
		{
			get { return GetUnits(PluralState.Plural); }
		}

		internal CodeDescriptionPairList GetUnits(PluralState pluralState)
		{
			return UnitHelper.GetUnits(RateLine.ParentRateEntry, RateLine.Country().RN_Code, Factory, pluralState, RateLine.Uses(CalculatorType.Equalization));
		}

		public RefPackTypeCollection PackageTypes
		{
			get { return UnitHelper.GetCachedPackageTypes(Factory); }
		}

		#endregion

		#region Service Levels

		public RefServiceLevelCollection ServiceLevels
		{
			get { return Factory.GetCachedValue("RefServiceLevelCollection", delegate { return new RefServiceLevelCollection(Factory); }); }
		}

		#endregion

		#region Commodity Codes

		public RefCommodityCodeCollection CommodityCodes
		{
			get { return Factory.GetCachedValue("RefCommodityCodeCollection", delegate { return new RefCommodityCodeCollection(Factory); }); }
		}

		#endregion

		#region Suppliers

		public OrgHeaderCollection Suppliers
		{
			get { return Factory.GetCachedValue("OrgHeaderCollection", delegate { return new OrgHeaderCollection(Factory); }); }
		}

		#endregion

		#region Consignees

		public ConsigneeCollection Consignees
		{
			get { return Factory.GetCachedValue("ConsigneeCollection", delegate { return new ConsigneeCollection(Factory); }); }
		}

		#endregion

		#region Consignors

		public ConsignorCollection Consignors
		{
			get { return Factory.GetCachedValue("ConsignorCollection", delegate { return new ConsignorCollection(Factory); }); }
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollection", delegate { return new LocationCollection(Factory); }); }
		}

		#endregion

		#region Equipment Types

		public CodeDescriptionPairList EquipmentTypes
		{
			get
			{
				if (RateLine?.ParentRateEntry?.IsFCL() ?? false)
				{
					return Factory.GetCachedValue<CodeDescriptionPairList>("FCLEquipmentNeededList", () => new FCLEquipmentNeededList(false));
				}
				else if (RateLine?.ParentRateEntry != null && (RateLine.ParentRateEntry.IsLCL() || RateLine.ParentRateEntry.IsAir()))
				{
					return Factory.GetCachedValue<CodeDescriptionPairList>("LCLAIREquipmentNeededList", () => new LCLAIREquipmentNeededList(false));
				}
				else if ((RateLine?.ParentRateEntry?.IsBCN() ?? false) || (RateLine?.ParentRateEntry?.IsSCN() ?? false))
				{
					return Factory.GetCachedValue("ConsolEquipmentNeededList", () => GetConsolLeadEquipmentNeededList());
				}
				else
				{
					return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.CustomType);
				}
			}
		}

		CodeDescriptionPairList GetConsolLeadEquipmentNeededList()
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(Factory.GetCachedValue<CodeDescriptionPairList>("FCLEquipmentNeededList", () => new FCLEquipmentNeededList(false)));
			list.AddRangeOverwriteIfExists(Factory.GetCachedValue<CodeDescriptionPairList>("LCLAIREquipmentNeededList", () => new LCLAIREquipmentNeededList(false)));
			list.Sort();
			return list;
		}

		#endregion

		#region Message Type/SubType

		public CodeDescriptionPairList MessageTypeList
		{
			get
			{
				var companyCode = RateLine.ParentRateEntry?.Company()?.GC_Code ?? GlbCompany.CurrentCompany.GC_Code;
				var countryCode = RateLine.ParentRateEntry?.Country().RN_Code.ToString() ?? string.Empty;
				return Factory.GetCachedValue($"RateLineMessageTypeList_{companyCode}_{countryCode}", () => (CodeDescriptionPairList)MessageTypeAndSubTypeListHelper.MessageTypeList(Factory, companyCode, countryCode)); // Factory Cache Key
			}
		}

		public CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				var companyCode = RateLine.ParentRateEntry?.Company()?.GC_Code ?? GlbCompany.CurrentCompany.GC_Code;
				var countryCode = RateLine.ParentRateEntry?.Country().RN_Code.ToString() ?? string.Empty;
				var messageType = RateLine.FindRateLineItem(AgencyCalculator.Items.MessageType)?.TM_Text ?? string.Empty;
				return Factory.GetCachedValue($"RateLineMessageSubTypeList_{companyCode}_{countryCode}_{messageType}", () => (CodeDescriptionPairList)MessageTypeAndSubTypeListHelper.MessageSubTypeList(Factory, companyCode, countryCode, messageType)); // Factory Cache Key
			}
		}

		#endregion

		#region Unit Multiples

		public CodeDescriptionPairList UnitMultiples
		{
			get { return Factory.GetCachedValue("RateLinesLookups.UnitMultiples", () => new UnitHelper().GetUnitMultiple(Factory)); }
		}

		#endregion

		#region Rounding

		public CodeDescriptionPairList Roundings
		{
			get
			{
				if (RateLine.ParentRateEntry != null)
				{
					var rateCategory = RateLine.ParentRateEntry.TI_RateCategory;
					return Factory.GetCachedValue("RateLinesLookups.Roundings" + rateCategory, () => GetCachedRoundings(Factory, rateCategory));
				}

				return new CodeDescriptionPairList();
			}
		}

		public static CodeDescriptionPairList GetCachedRoundings(BusinessObjectFactory factory, string rateCategory = null)
			=> factory.GetCachedValue("RateLinesLookups.Roundings" + rateCategory, () => BuildRoundings(rateCategory));

		static CodeDescriptionPairList BuildRoundings(string rateCategory)
		{
			var roundings = new CodeDescriptionPairList();
			var baseList = new RatingRoundingTypeList();

			var defaultRounding = DataRegistryRating.Instance.DefaultRounding.GetDefaultRounding(rateCategory);
			var defaultRoundingType = defaultRounding.RoundingType;
			var defaultRoundingDescription = baseList.GetDescriptionFromCode(defaultRoundingType);

			roundings.AddPair(RatingRoundingTypes.DefaultFromRegistry, Res.GetString("c865fbd9-cbfb-4c5f-b226-f30a69c87036", "Default from registry - {0}", defaultRoundingDescription));
			roundings.AddRange(baseList);

			if (!string.IsNullOrEmpty(rateCategory))
			{
				var rateType = RatingConstants.RateCategory.GetRateType(rateCategory);

				if (rateType != RateType.Forwarding &&
					(rateType & RateType.TransportBookings) == 0)
				{
					roundings.RemoveCode(RatingRoundingTypes.Chargeable);
				}
			}

			return roundings;
		}

		#endregion

		#region Product Numbers

		public OrgSupplierPartCollection ProductNumbers
		{
			get
			{
				if (RateLine.ParentRateEntry?.ParentRatingHeader != null)
				{
					return Factory.GetCachedValue("OrgSupplierPartCollection", () => new OrgSupplierPartCollection(Factory, null, RateLine.ParentRateEntry.ParentRatingHeader.Header, false));
				}
				else
				{
					return Factory.GetCachedValue("OrgSupplierPartCollectionEmpty", () => new OrgSupplierPartCollection(Factory));
				}
			}
		}

		#endregion

		#region Fee Charges

		public CodeDescriptionPairList FeeChargeTypes
			=> GetFeeChargeTypeList();

		static public CodeDescriptionPairList GetFeeChargeTypeList()
		{
			var types = new CodeDescriptionPairList();
			var registryTypes = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value.FeeChargeTypes;

			foreach (FeeChargeType type in registryTypes)
			{
				types.AddPair(type.Code, type.Description);
			}

			return types;
		}

		public CodeDescriptionPairList FeeChargeLevels
		{
			get
			{
				var parentFeeChargeType = RateLine.GetFeeChargeType();
				if (parentFeeChargeType != null)
				{
					var levels = new CodeDescriptionPairList();
					foreach (FeeChargeLevel level in parentFeeChargeType.FeeChargeLevels)
					{
						levels.AddPair(level.Code, level.Description);
					}

					return levels;
				}

				return new CodeDescriptionPairList();
			}
		}

		#endregion

		#region Basic Operators

		CodeDescriptionPairList BasicOperators
		{
			get
			{
				return Factory.GetCachedValue("BasicOperators", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Calculator.Items.Operator.MIN, Res.GetString("8f22c2e1-5fd2-47ee-857e-08b90a9ac968", "Minimum"));
					result.AddPair(Calculator.Items.Operator.MAX, Res.GetString("6ae85ce5-2d9a-4dc0-9d80-52133ef8d6b2", "Maximum"));
					result.AddPair(Calculator.Items.Operator.BAS, RatingDataRegistry.Instance.BaseRateText.Value);
					return result;
				});
			}
		}

		#endregion

		#region Weight Breaks

		public CodeDescriptionPairList WeightBreaks
		{
			get
			{
				if (RateLine.UsesCompanyTariffOrCostBasedCalculator())
				{
					return Factory.GetCachedValue("WeightBreaks_CompTariffOrCost", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Calculator.Items.Operator.Minus, Res.GetString("e8bac3bc-b809-4baa-9e1c-d343c9f88b48", "Less Than"));
						result.AddPair(Calculator.Items.Operator.Plus, Res.GetString("af30a5e7-4b40-486d-81f4-aad7155295a5", "Greater Than"));
						return result;
					});
				}
				else if (RateLine.Uses(CalculatorType.HighestRate))
				{
					return Factory.GetCachedValue("WeightBreaks_HighestRate", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Calculator.Items.Operator.MIN, Res.GetString("8f22c2e1-5fd2-47ee-857e-08b90a9ac968", "Minimum"));
						result.AddPair(Calculator.Items.Operator.UNT, Res.GetString("e088d899-c976-40c7-aa67-0008b07d2ecc", "Per Unit of Quantity"));
						return result;
					});
				}
				else if (RateLine.Uses(CalculatorType.ValueRange))
				{
					return Factory.GetCachedValue("WeightBreaks_ValueRange", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Calculator.Items.Operator.MIN, Res.GetString("8f22c2e1-5fd2-47ee-857e-08b90a9ac968", "Minimum"));
						result.AddPair(Calculator.Items.Operator.Minus, Res.GetString("e8bac3bc-b809-4baa-9e1c-d343c9f88b48", "Less Than"));
						result.AddPair(Calculator.Items.Operator.Plus, Res.GetString("af30a5e7-4b40-486d-81f4-aad7155295a5", "Greater Than"));
						return result;
					});
				}
				else if (RateLine.Uses(CalculatorType.CombinedWithIncrement))
				{
					return Factory.GetCachedValue("WeightBreaks_CombinedWithIncrement", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Calculator.Items.Operator.Minus, Res.GetString("e8bac3bc-b809-4baa-9e1c-d343c9f88b48", "Less Than"));
						result.AddPair(Calculator.Items.Operator.Plus, Res.GetString("af30a5e7-4b40-486d-81f4-aad7155295a5", "Greater Than"));
						return result;
					});
				}
				else if (RateLine.IsCalculatorInitialized && RateLine.Calculator.UseInclusiveBreaks)
				{
					return Factory.GetCachedValue("WeightBreaks_InclusiveBreaks", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(BasicOperators);
						result.AddPair(Calculator.Items.Operator.UNT, Res.GetString("e088d899-c976-40c7-aa67-0008b07d2ecc", "Per Unit of Quantity"));
						result.AddPair(Calculator.Items.Operator.Minus, Res.GetString("f62a4052-e468-4748-b716-3d4573738bab", "Less Than Or Equal To"));
						result.AddPair(Calculator.Items.Operator.Plus, Res.GetString("af30a5e7-4b40-486d-81f4-aad7155295a5", "Greater Than"));
						return result;
					});
				}
				else
				{
					return Factory.GetCachedValue("WeightBreaks_General", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(BasicOperators);
						result.AddPair(Calculator.Items.Operator.UNT, Res.GetString("e088d899-c976-40c7-aa67-0008b07d2ecc", "Per Unit of Quantity"));
						result.AddPair(Calculator.Items.Operator.Minus, Res.GetString("e8bac3bc-b809-4baa-9e1c-d343c9f88b48", "Less Than"));
						result.AddPair(Calculator.Items.Operator.Plus, Res.GetString("f3951d9b-0f81-40a6-a58c-dc5990f13331", "Greater Than Or Equal To"));
						return result;
					});
				}
			}
		}

		#endregion

		#region Breaks Per

		public CodeDescriptionPairList BreaksPer
		{
			get
			{
				return Factory.GetCachedValue("BreaksPer", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Calculator.Items.BreaksPerContainerTypeOrClass, Res.GetString("530bf598-0477-4b63-a76e-f61011de6872", "Container Type/Class"));
					result.AddPair(Calculator.Items.BreaksPerContainer, Res.GetString("9231526a-5207-4fb6-a09b-7e384d30c3b3", "Container"));
					return result;
				});
			}
		}

		#endregion

		public CodeDescriptionPairList UnitFactors
		{
			get
			{
				var rateEntry = RateLine.ParentRateEntry;
				if (rateEntry == null)
				{
					return new CodeDescriptionPairList();
				}

				return Factory.GetCachedValue
				(
					$"RateLinesLookups.UnitFactors-{RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.Value}-" +
					$"{rateEntry.ParentRatingHeader.RateTypeSafe() ?? string.Empty}-" +
					$"{rateEntry.TI_RateCategory}-{rateEntry.TI_Mode}-{rateEntry.TI_RC}-" +
					$"{RateLine.TL_WeightVolume}-{RateLine.TL_RateCalculator}",
					() => GetUnitFactors(RateLine)
				);
			}
		}

		static CodeDescriptionPairList GetUnitFactors(IRateLine rateLine)
		{
			var rateEntry = rateLine.ParentRateEntry;
			if (rateEntry != null)
			{
				return BuildUnitFactors(rateEntry.ParentRatingHeader.RateTypeSafe(), rateLine.RateType(), rateLine, rateEntry.TI_Mode);
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}

		/// <summary>
		/// Build unit factor choices
		/// </summary>
		/// <param name="calculator">null to return values for all calculators, or else just for given one</param>
		/// <param name="rateEntry">null to return values for all rateentries, or else just for given one</param>
		public static CodeDescriptionPairList BuildUnitFactors(string ratingHeaderType, RateType rateType, IRateLine rateLine = null, string rateMode = null)
		{
			var list = new CodeDescriptionPairList();

			if (ratingHeaderType == RatingConstants.RatingHeaderTypes.IntercompanyTariff)
			{
				list.Add(new CodeDescriptionPair(UnitFactorList.Codes.SAM, UnitFactorList.Descriptions.SAM));
			}
			else
			{
				if (ratingHeaderType == RatingConstants.RatingHeaderTypes.ClientRate ||
					ratingHeaderType == RatingConstants.RatingHeaderTypes.Tariff ||
					ratingHeaderType == RatingConstants.RatingHeaderTypes.Quote)
				{
					switch (rateMode)
					{
						case RateMode.SCN:
							list.Add(new CodeDescriptionPair(UnitFactorList.Codes.SCN, UnitFactorList.Descriptions.SCN));
							break;
						case RateMode.BCN:
							list.Add(new CodeDescriptionPair(UnitFactorList.Codes.BCN, UnitFactorList.Descriptions.BCN));
							break;
						default:
							list.Add(new CodeDescriptionPair(UnitFactorList.Codes.BCN, UnitFactorList.Descriptions.BCN));
							list.Add(new CodeDescriptionPair(UnitFactorList.Codes.SCN, UnitFactorList.Descriptions.SCN));
							break;
					}
				}

				if (HasRatingHeaderSupportForIndividualContainer(ratingHeaderType)
					&& HasModuleSupportForIndividualContainer(rateType)
					&& (rateLine == null || (IsRateEntryContainerized(rateLine.ParentRateEntry) && IsLineUnitSupportedForContainerCount(rateLine.TL_WeightVolume))))
				{
					list.Add(new CodeDescriptionPair(UnitFactorList.Codes.CTN, UnitFactorList.Descriptions.CTN));
				}

				if ((rateType & RateType.Warehouse) != 0)
				{
					var calculator = rateLine?.Calculator;
					if (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.Value && (calculator == null || calculator.SupportsPacksWeightUnitFactor))
					{
						list.Add(new CodeDescriptionPair(UnitFactorList.Codes.PacksWeight, UnitFactorList.Descriptions.PacksWeight));
					}

					if (calculator == null || calculator.SupportsProductLineUnitFactor)
					{
						list.Add(new CodeDescriptionPair(UnitFactorList.Codes.ProductLine, UnitFactorList.Descriptions.ProductLine));
					}

					if (calculator == null || calculator.SupportsPackageLineUnitFactor)
					{
						list.Add(new CodeDescriptionPair(UnitFactorList.Codes.PackageLine, UnitFactorList.Descriptions.PackageLine));
					}

					if (rateLine != null && rateLine.TL_RateCalculator == WarehousePackCalculator.Code && rateLine.TL_WeightVolume == QuantityUnit.PK)
					{
						list.Add(new CodeDescriptionPair(UnitFactorList.Codes.LoadedPackagesOnly, UnitFactorList.Descriptions.LoadedPackagesOnly));
					}
				}
			}

			// Inner Pack
			if (ratingHeaderType == RatingConstants.RatingHeaderTypes.ClientRate ||
				ratingHeaderType == RatingConstants.RatingHeaderTypes.Tariff ||
				ratingHeaderType == RatingConstants.RatingHeaderTypes.Quote)
			{
				// Only forwarding jobs may have Inner Pack Lines
				if ((rateType & RateType.Forwarding) != 0)
				{
					// Calculation per Inner Pack is allowed only if the rate doesn't have a specific container as inner pack lines
					// don't depend on a specific container like outer pack lines.
					if (rateLine == null || rateLine.ParentRateEntry.TI_RC.IsEmpty && rateLine.RequiresWeightVolume())
					{
						list.AddPair(UnitFactorList.Codes.InnerPack, UnitFactorList.Descriptions.InnerPack);
					}
				}
			}

			return list;
		}

		static bool HasRatingHeaderSupportForIndividualContainer(string ratingHeaderType)
			=> ratingHeaderType == RatingConstants.RatingHeaderTypes.ClientRate
			|| ratingHeaderType == RatingConstants.RatingHeaderTypes.Tariff
			|| ratingHeaderType == RatingConstants.RatingHeaderTypes.Quote;

		static bool HasModuleSupportForIndividualContainer(RateType rateType)
			=> rateType == RateType.Forwarding
			|| rateType == RateType.Customs
			|| rateType == RateType.TransportBookings;

		static bool IsRateEntryContainerized(IRateEntry rateEntry)
			=> rateEntry == null || (!rateEntry.TI_RC.IsEmpty && (rateEntry.IsFCL() || rateEntry.IsBCN() || rateEntry.IsSCN() || rateEntry.IsULD()));

		static bool IsLineUnitSupportedForContainerCount(string lineUnit)
			=> lineUnit == QuantityUnit.CN || lineUnit == QuantityUnit.TU;
	}

	[ModuleID(ModuleId.AccGlobalChargeCode)]
	public class GlobalChargeCodesCollection : AccChargeCodeCollection
	{
		public GlobalChargeCodesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlobalChargeCodesCollection(BusinessObjectFactory factory, ZQuery sqlFilter, Guid companyPK) : base(factory, sqlFilter, companyPK)
		{
		}
	}
}

