using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using CoreConsts = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Rating.Business
{
	public static class RatingConstants
	{
		#region PaymentTerms
		public static class PaymentTerms
		{
			public const string Prepaid = "PPD";
			public const string Collect = "CCX";
		}
		#endregion

		#region Frequency Units

		public static class FrequencyUnits
		{
			public const string Daily = "DAILY";
			public const string Days = "DAYS";
			public const string Week = "WEEK";
			public const string Fortnight = "FORTNIGHT";
			public const string Monthly = "MONTHLY";
		}

		#endregion

		#region Transit Times

		public static class TransitTimes
		{
			public const string Overnight = "OVN";
			public const string SameDay = "SMD";

			static Dictionary<string, int> SpecialTransitTimes => new Dictionary<string, int>
			{
				{ SameDay, 0 },
				{ Overnight, 1 },
			};

			public static bool CheckIsSpecialValue(string transitTime)
			{
				return SpecialTransitTimes.ContainsKey(transitTime);
			}

			public static int ConvertToInt(string transitTimeString)
			{
				if (SpecialTransitTimes.TryGetValue(transitTimeString, out var result))
				{
					return result;
				}

				if (int.TryParse(transitTimeString, out result))
				{
					return result;
				}

				return -1;
			}
		}

		#endregion

		#region Units

		[CodeAlive("Code is required")]
		public class Units : QuantityUnit
		{
		}

		#endregion

		#region Rate Category Types

		public static class RateCategory
		{
			public const string SummaryRatesCategory = "SMR";
			public const string RatesServiceCategory = "WiseRates";

			public static readonly ReadOnlyCollection<string> RateCategories = new ReadOnlyCollection<string>(
				new[]
				{
					AIR, FCL, LCL, ORG, DST,
					SCO, SNC, SOR, SDE, SED, SID, PAC, UNP, CST, WHS, TRW, TWU, TRN, TBC,
					CAI, CFC, CLC, COR, CDS,
					CYD, CYU, CYM
				});

			[RateType(RateType.Forwarding), IsFreight]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string AIR = "AIR";
			[RateType(RateType.Forwarding), IsFreight]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string FCL = "FCL";
			[RateType(RateType.Forwarding), IsFreight]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string LCL = "LCL";
			[RateType(RateType.Forwarding), IsOrigin]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string ORG = "ORG";
			[RateType(RateType.Forwarding), IsDestination]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string DST = "DST";

			[RateType(RateType.Customs), IsFreight]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string CAI = "CAI";
			[RateType(RateType.Customs), IsFreight]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string CFC = "CFC";
			[RateType(RateType.Customs), IsFreight]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string CLC = "CLC";
			[RateType(RateType.Customs), IsOrigin]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string COR = "COR";
			[RateType(RateType.Customs), IsDestination]
			[DataContext(DataContext.Rating, DataContext.Quotation)]
			public const string CDS = "CDS";

			[RateType(RateType.Shipping), IsOrigin]
			[DataContext(DataContext.ShippingRating)]
			public const string SOR = "SOR";
			[RateType(RateType.Shipping), IsDestination]
			[DataContext(DataContext.ShippingRating)]
			public const string SDE = "SDE";
			[RateType(RateType.Shipping), IsFreight]
			[DataContext(DataContext.ShippingRating)]
			public const string SCO = "SCO";
			[RateType(RateType.Shipping), IsFreight]
			[DataContext(DataContext.ShippingRating)]
			public const string SNC = "SNC";

			[RateType(RateType.ShippingExportDetention)]
			[DataContext(DataContext.ShippingDetentionRating), IsOrigin]
			public const string SED = "SED";
			[RateType(RateType.ShippingImportDetention)]
			[DataContext(DataContext.ShippingDetentionRating), IsDestination]
			public const string SID = "SID";

			[RateType(RateType.CFS), IsOrigin]
			[DataContext(DataContext.CFSRating)]
			public const string PAC = "PAC";
			[RateType(RateType.CFS), IsDestination]
			[DataContext(DataContext.CFSRating)]
			public const string UNP = "UNP";
			[RateType(RateType.CFS)]
			[DataContext(DataContext.None)]
			public const string CST = "CST";

			[RateType(RateType.Warehouse)]
			[DataContext(DataContext.WarehouseRating)]
			public const string WHS = "WHS";
			[RateType(RateType.TransitWarehouse)]
			[DataContext(DataContext.WarehouseRating)]
			public const string TRW = "TRW";
			[RateType(RateType.TransitWarehouseTransportationUnit)]
			[DataContext(DataContext.WarehouseRating)]
			public const string TWU = "TWU";

			[RateType(RateType.LocalTransport), IsOrigin]
			[DataContext(DataContext.TransportRating)]
			public const string TRN = "TRN";
			[RateType(RateType.TransportBookings), IsOrigin]
			[DataContext(DataContext.TransportRating)]
			public const string TBC = "TBC";

			[RateType(RateType.ContainerYard)]
			[DataContext(DataContext.ContainerYardRating)]
			public const string CYD = "CYD";
			[RateType(RateType.ContainerYard)]
			[DataContext(DataContext.ContainerYardRating)]
			public const string CYM = "CYM";
			[RateType(RateType.ContainerYardTransportationUnit)]
			[DataContext(DataContext.ContainerYardRating)]
			public const string CYU = "CYU";

			public const string ALL = "ALL";

			static IEnumerable<string> categoriesWithCarrierContractNumberLookup => new string[] {
#if !WINZOR
				RatingConstants.RateCategory.AIR,
				RatingConstants.RateCategory.FCL,
				RatingConstants.RateCategory.LCL,
				RatingConstants.RateCategory.ORG,
				RatingConstants.RateCategory.DST,
				RatingConstants.RateCategory.CAI,
				RatingConstants.RateCategory.CFC,
				RatingConstants.RateCategory.CLC,
				RatingConstants.RateCategory.COR,
				RatingConstants.RateCategory.CDS,
#endif
				};

			public static bool SupportsTransportZones(string category)
			{
				return category == TBC;
			}

			public static bool SupportsContractNumberLookup(string category)
			{
				return categoriesWithCarrierContractNumberLookup.Contains(category);
			}

			public static bool IsValidEntryCategory(string category)
			{
				return RateCategories.Contains(category);
			}

			public static bool IsValidFilterCategory(string category)
			{
				return IsValidEntryCategory(category) || category == SummaryRatesCategory;
			}

			public static RateType GetRateType(string rateCategory)
			{
				return GetRateTypeInfo(rateCategory).RateType;
			}

			public static string[] GetRateCategories(RateType rateType, RateCategoryGroup rateCategoryGroup)
			{
				var result = new List<string>();
				foreach (var keyValue in RateCategoryToRateType)
				{
					if ((keyValue.Value.RateType & rateType) != 0)
					{
						bool saticfiesFilter;
						if (keyValue.Value.IsFreight)
						{
							saticfiesFilter = (rateCategoryGroup & RateCategoryGroup.Freight) != 0;
						}
						else if (keyValue.Value.IsOrigin)
						{
							saticfiesFilter = (rateCategoryGroup & RateCategoryGroup.Origin) != 0;
						}
						else if (keyValue.Value.IsDestination)
						{
							saticfiesFilter = (rateCategoryGroup & RateCategoryGroup.Destination) != 0;
						}
						else
						{
							saticfiesFilter = (rateCategoryGroup & RateCategoryGroup.OtherSupplementary) != 0;
						}

						if (saticfiesFilter)
						{
							result.Add(keyValue.Key);
						}
					}
				}

				return result.ToArray();
			}

			public static DataContext[] GetDataContexts(string rateCategory)
			{
				return GetRateTypeInfo(rateCategory).DataContexts;
			}

			public static bool IsFreight(string rateCategory)
			{
				return GetRateTypeInfo(rateCategory).IsFreight;
			}

			public static bool IsOrigin(string rateCategory)
			{
				return GetRateTypeInfo(rateCategory).IsOrigin;
			}

			public static bool IsDestination(string rateCategory)
			{
				return GetRateTypeInfo(rateCategory).IsDestination;
			}

			static RateTypeInfo GetRateTypeInfo(string rateCategory)
			{
				RateTypeInfo rateTypeInfo;
				if (RateCategoryToRateType.TryGetValue(rateCategory, out rateTypeInfo))
				{
					return rateTypeInfo;
				}
				else
				{
					return new RateTypeInfo();
				}
			}

			static Dictionary<string, RateTypeInfo> RateCategoryToRateType
			{
				get
				{
					if (rateCategoryToRateType == null)
					{
						rateCategoryToRateType = new Dictionary<string, RateTypeInfo>();
						foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
						{
							var fieldInfo = typeof(RateCategory).GetField(rateCategory, BindingFlags.Public | BindingFlags.Static);
							rateCategoryToRateType.Add((string)fieldInfo.GetValue(null), new RateTypeInfo(fieldInfo));
						}
					}

					return rateCategoryToRateType;
				}
			}

			struct RateTypeInfo
			{
				public RateTypeInfo(FieldInfo fieldInfo)
				{
					RateType = ((RateTypeAttribute)(fieldInfo.GetCustomAttributes(typeof(RateTypeAttribute), false)[0])).RateType;
					IsFreight = fieldInfo.IsDefined(typeof(IsFreightAttribute), false);
					IsOrigin = fieldInfo.IsDefined(typeof(IsOriginAttribute), false);
					IsDestination = fieldInfo.IsDefined(typeof(IsDestinationAttribute), false);
					DataContexts = ((DataContextAttribute)(fieldInfo.GetCustomAttributes(typeof(DataContextAttribute), false)[0])).DataContexts;
				}

				public readonly RateType RateType;
				public readonly bool IsFreight;
				public readonly bool IsOrigin;
				public readonly bool IsDestination;
				public readonly DataContext[] DataContexts;
			}

			[ThreadStatic]
			static Dictionary<string, RateTypeInfo> rateCategoryToRateType;
		}

		public static class RateMode
		{
			internal static string Generalize(string rateMode, string rateCategory)
			{
				if (string.IsNullOrEmpty(rateCategory))
				{
					throw new ArgumentException("Need to know rate category to generalize rate mode");
				}

				return Generalize(rateMode, RatingConstants.RateCategory.IsFreight(rateCategory));
			}

			internal static string Generalize(string rateMode, bool isFreight)
			{
				if (string.IsNullOrEmpty(rateMode))
				{
					throw new ArgumentException("Cannot generalize empty rate mode");
				}

				if (isFreight && rateMode != Core.Constants.RateMode.ALL)
				{
					return Core.Constants.RateMode.ALL;
				}

				switch (rateMode)
				{
					case Core.Constants.RateMode.ALL:
						return string.Empty;

					case Core.Constants.RateMode.AIR:
					case Core.Constants.RateMode.SEA:
					case Core.Constants.RateMode.ROA:
					case Core.Constants.RateMode.RAI:
						return Core.Constants.RateMode.ALL;

					case Core.Constants.RateMode.ULD:
					case Core.Constants.RateMode.LSE:
						return Core.Constants.RateMode.AIR;

					case Core.Constants.RateMode.FCL:
					case Core.Constants.RateMode.GRP:
					case Core.Constants.RateMode.LCL:
						return Core.Constants.RateMode.SEA;

					case Core.Constants.RateMode.FRO:
					case Core.Constants.RateMode.LRO:
					case Core.Constants.RateMode.FTL:
						return Core.Constants.RateMode.ROA;

					case Core.Constants.RateMode.FRA:
					case Core.Constants.RateMode.LRA:
					case Core.Constants.RateMode.FWL:
						return Core.Constants.RateMode.RAI;

					case Core.Constants.RateMode.OBC:
					case Core.Constants.RateMode.UNA:
						return Core.Constants.RateMode.COU;

					default:
						return Core.Constants.RateMode.ALL;
				}
			}

			internal static bool IsFirstModeMoreSpecific(ZString mode1, ZString mode2, ZString rateCategory)
			{
				if (!string.IsNullOrEmpty(mode1) && !string.IsNullOrEmpty(mode2) && !string.IsNullOrEmpty(rateCategory) && mode1 != mode2)
				{
					var generalized = Generalize(mode1, rateCategory);

					while (!string.IsNullOrEmpty(generalized))
					{
						if (generalized == mode2)
						{
							return true;
						}

						generalized = Generalize(generalized, rateCategory);
					}
				}

				return false;
			}

			[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
			public static string GetTransportModes(string rateMode)
			{
				switch (rateMode)
				{
					case Core.Constants.RateMode.ALL:
						return CoreConsts.TransportModes.All;

					case Core.Constants.RateMode.AIR:
					case Core.Constants.RateMode.ULD:
					case Core.Constants.RateMode.LSE:
						return CoreConsts.TransportModes.Air;

					case Core.Constants.RateMode.SEA:
					case Core.Constants.RateMode.FCL:
					case Core.Constants.RateMode.GRP:
					case Core.Constants.RateMode.LCL:
						return CoreConsts.TransportModes.Sea;

					case Core.Constants.RateMode.ROA:
					case Core.Constants.RateMode.FRO:
					case Core.Constants.RateMode.LRO:
					case Core.Constants.RateMode.FTL:
						return CoreConsts.TransportModes.Road;

					case Core.Constants.RateMode.RAI:
					case Core.Constants.RateMode.FRA:
					case Core.Constants.RateMode.LRA:
					case Core.Constants.RateMode.FWL:
						return CoreConsts.TransportModes.Rail;

					case Core.Constants.RateMode.MAI:
						return CoreConsts.TransportModes.Mail;

					case Core.Constants.RateMode.COU:
						return CoreConsts.TransportModes.Courier;

					default:
						throw new InvalidOperationException(FormattableString.Invariant($"Unexpected rateMode of {rateMode}"));
				}
			}
		}

		public static class TransportMode               //ToDo: those ore Rates Service transport modes - should originate from there
		{
			public const string AIR = nameof(AIR);
			public const string SEA = nameof(SEA);
		}

		#endregion

		#region Rating Header Types

		public static class RatingHeaderTypes
		{
			public const string ClientRate = "SAL";
			public const string Costing = "COS";
			public const string Tariff = "GLB";
			public const string Quote = "QTE";
			public const string IntercompanyTariff = "ICT";
			public const string WiseCost = "WCT";
		}

		#endregion

		#region Document Template Types

		public static class DocTemplateTypes
		{
			public const string CoverPage = "CVR";
			public const string TrailingPage = "TRA";
			public const string StandardPricingPage = "PST";
			public const string OneOffPricingPage = "POF";
			public const string OneOffMultiCarriersPricingPage = "POM";
			public const string TableFormatPricingPage = "PTF";

			public static bool IsPricingPage(ZString templateType)
			{
				return templateType == StandardPricingPage ||
					templateType == OneOffPricingPage ||
					templateType == OneOffMultiCarriersPricingPage ||
					templateType == TableFormatPricingPage;
			}
		}

		#endregion

		#region Mode Converters

		public static ZString GetTransportModeFromMode(ZString value)
		{
			switch (value)
			{
				case "":
					return "";
				case Core.Constants.RateMode.AIR:
				case Core.Constants.RateMode.LSE:
				case Core.Constants.RateMode.ULD:
					return Core.Constants.TransportModes.Air;

				case Core.Constants.RateMode.SEA:
				case Core.Constants.RateMode.LCL:
				case Core.Constants.RateMode.FCL:
					return Core.Constants.TransportModes.Sea;

				case Core.Constants.RateMode.ROA:
				case Core.Constants.RateMode.LRO:
				case Core.Constants.RateMode.FRO:
				case Core.Constants.RateMode.FTL:
					return Core.Constants.TransportModes.Road;

				case Core.Constants.RateMode.RAI:
				case Core.Constants.RateMode.LRA:
				case Core.Constants.RateMode.FRA:
				case Core.Constants.RateMode.FWL:
					return Core.Constants.TransportModes.Rail;

				case Core.Constants.RateMode.COU:
					return Core.Constants.TransportModes.Courier;

				default:
					return Core.Constants.TransportModes.Other;
			}
		}

		public static ZString GetOneOffQuoteContainerModeFromMode(ZString value)
		{
			if (value == Core.Constants.RateMode.SEA ||
				value == Core.Constants.RateMode.RAI ||
				value == Core.Constants.RateMode.ROA ||
				value == Core.Constants.RateMode.LRO ||
				value == Core.Constants.RateMode.FWL ||
				value == Core.Constants.RateMode.COU)
			{
				return value;
			}
			return GetContainerModeFromMode(value);
		}

		public static ZString GetContainerModeFromMode(ZString value)
		{
			switch (value)
			{
				case "":
					return "";
				case Core.Constants.RateMode.AIR:
				case Core.Constants.RateMode.LSE:
					return Core.Constants.ContainerModes.Loose;

				case Core.Constants.RateMode.ULD:
					return Core.Constants.ContainerModes.ULD;

				case Core.Constants.RateMode.LCL:
				case Core.Constants.RateMode.ROA:
				case Core.Constants.RateMode.LRO:
				case Core.Constants.RateMode.RAI:
				case Core.Constants.RateMode.LRA:
				case Core.Constants.RateMode.FWL:
					return Core.Constants.ContainerModes.LCL;

				case Core.Constants.RateMode.FTL:
					return Core.Constants.ContainerModes.FTL;

				case Core.Constants.RateMode.SEA:
				case Core.Constants.RateMode.FCL:
				case Core.Constants.RateMode.FRO:
				case Core.Constants.RateMode.FRA:
					return Core.Constants.ContainerModes.FCL;

				case Core.Constants.RateMode.COU:
					return Core.Constants.ContainerModes.OnBoardCourier;

				default:
					return Core.Constants.ContainerModes.Other;
			}
		}

		#endregion

		#region RatingZoneTypes

		public static class RatingZoneTypes
		{
			public const string All = "ALL";
			public const string Rating = "RAT";
			public const string Operations = "OPS";
			public const string Reporting = "RPT";

			public static class Descriptions
			{
				public static string All { get { return Res.GetString("78AFA101-EA20-4B68-8EBF-B2C80CE2F6F2", "All Types"); } }
				public static string Rating { get { return Res.GetString("217CADAF-C228-490B-AE52-EA1B72B1C433", "Rating Only"); } }
				public static string Operations { get { return Res.GetString("B4DF967A-42C2-4CA7-B772-747AD098B4DA", "Operations Only"); } }
				public static string Reporting { get { return Res.GetString("9C794642-E6D3-44D6-90B8-BD6006246B5B", "Reporting Only"); } }
			}
		}

		#endregion

		#region RateCategoryToChargeCodeGroup

		public static Dictionary<RateType, JobInvoicingConsumerType[]> JobTypesByRateType => new Dictionary<RateType, JobInvoicingConsumerType[]>
		{
			{
				RateType.Forwarding, new[]
				{
					JobInvoicingConsumerTypes.Shipment,
					JobInvoicingConsumerTypes.QuotedBooking,
					JobInvoicingConsumerTypes.Consol,
					JobInvoicingConsumerTypes.ForwardingConsol,
					JobInvoicingConsumerTypes.GatewayConsol,
					JobInvoicingConsumerTypes.Brokerage,
					JobInvoicingConsumerTypes.MasterAWB,
					JobInvoicingConsumerTypes.OneOffQuotation,
					JobInvoicingConsumerTypes.eManifest
				}
			},
			{
				RateType.Customs, new[]
				{
					JobInvoicingConsumerTypes.Shipment,
					JobInvoicingConsumerTypes.QuotedBooking,
					JobInvoicingConsumerTypes.Consol,
					JobInvoicingConsumerTypes.ForwardingConsol,
					JobInvoicingConsumerTypes.GatewayConsol,
					JobInvoicingConsumerTypes.Brokerage,
					JobInvoicingConsumerTypes.MasterAWB,
					JobInvoicingConsumerTypes.OneOffQuotation,
					JobInvoicingConsumerTypes.eManifest
				}
			},
			{
				RateType.CFS, new[]
				{
					JobInvoicingConsumerTypes.CFSShipment,
					JobInvoicingConsumerTypes.CFSLoadList,
					JobInvoicingConsumerTypes.FCLStorage,
				}
			},
			{
				RateType.Warehouse, new[]
				{
					JobInvoicingConsumerTypes.WarehouseInwards,
					JobInvoicingConsumerTypes.WarehouseOutwards,
					JobInvoicingConsumerTypes.WarehouseStorage,
					JobInvoicingConsumerTypes.WarehouseStocktake,
					JobInvoicingConsumerTypes.WarehouseAdHocServiceJob,
					JobInvoicingConsumerTypes.WarehouseVASOrder,
				}
			},
			{
				RateType.TransitWarehouse, new[]
				{
					JobInvoicingConsumerTypes.TransitReceive,
					JobInvoicingConsumerTypes.TransitDispatch,
					JobInvoicingConsumerTypes.TransitDispatchLoadList,
				}
			},
			{
				RateType.TransitWarehouseTransportationUnit, new[]
				{
					JobInvoicingConsumerTypes.TransitReceiveTransportationUnit,
					JobInvoicingConsumerTypes.TransitDispatchTransportationUnit,
				}
			},
			{
				RateType.TransportBookings, new[]
				{
					JobInvoicingConsumerTypes.TransportBooking,
					JobInvoicingConsumerTypes.TransportBookingConsignment,
					JobInvoicingConsumerTypes.TransportConsignment,
				}
			},
			{
				RateType.Shipping, new[]
				{
					JobInvoicingConsumerTypes.AgencyBillOfLading,
					JobInvoicingConsumerTypes.AgencyBooking,
				}
			},
			{
				RateType.ShippingImportDetention, new[]
				{
					JobInvoicingConsumerTypes.AgencyDetentionInvoice,
				}
			},
			{
				RateType.ShippingExportDetention, new[]
				{
					JobInvoicingConsumerTypes.AgencyDetentionInvoice,
				}
			},
			{
				RateType.LocalTransport, new[]
				{
					JobInvoicingConsumerTypes.LocalCartage,
				}
			},
			{
				RateType.ContainerYard, new[]
				{
					JobInvoicingConsumerTypes.CYDReceiveAdvice,
					JobInvoicingConsumerTypes.CYDReleaseAdvice,
					JobInvoicingConsumerTypes.CYDTransportationUnit,
					JobInvoicingConsumerTypes.MNRWorkOrderHeader,
					JobInvoicingConsumerTypes.CYDPeriodicInvoicing,
				}
			},
			{
				RateType.ContainerYardTransportationUnit, new[]
				{
					JobInvoicingConsumerTypes.CYDTransportationUnit,
				}
			},
		};

		public static Dictionary<string, string[]> GetForwardingChargeCodeGroupsByRateCategory() => new Dictionary<string, string[]>
		{
			{
				RateCategory.AIR, new[]
				{
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance
				}
			},
			{
				RateCategory.FCL, new[]
				{
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance
				}
			},
			{
				RateCategory.LCL, new[]
				{
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance
				}
			},
			{
				RateCategory.ORG, new[]
				{
					ChargeCodeGroupList.Codes.Origin,
					ChargeCodeGroupList.Codes.Loading,
					ChargeCodeGroupList.Codes.OriginBrokerage,
					ChargeCodeGroupList.Codes.OriginBrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty,
				}
			},
			{
				RateCategory.DST, new[]
				{
					ChargeCodeGroupList.Codes.Destination,
					ChargeCodeGroupList.Codes.Unloading,
					ChargeCodeGroupList.Codes.Brokerage,
					ChargeCodeGroupList.Codes.BrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty
				}
			},
		};

		public static Dictionary<string, string[]> GetCutomsChargeCodeGroupsByRateCategory() => new Dictionary<string, string[]>
		{
			{
				RateCategory.CAI, new[]
				{
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance
				}
			},
			{
				RateCategory.CFC, new[]
				{
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance
				}
			},
			{
				RateCategory.CLC, new[]
				{
					ChargeCodeGroupList.Codes.Freight,
					ChargeCodeGroupList.Codes.Insurance
				}
			},
			{
				RateCategory.COR, new[]
				{
					ChargeCodeGroupList.Codes.Origin,
					ChargeCodeGroupList.Codes.Loading,
					ChargeCodeGroupList.Codes.OriginBrokerage,
					ChargeCodeGroupList.Codes.OriginBrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty,
				}
			},
			{
				RateCategory.CDS, new[]
				{
					ChargeCodeGroupList.Codes.Destination,
					ChargeCodeGroupList.Codes.Unloading,
					ChargeCodeGroupList.Codes.Brokerage,
					ChargeCodeGroupList.Codes.BrokerageOnly,
					ChargeCodeGroupList.Codes.CustomsDuty
				}
			},
		};

		public static string InferForwardingRateCategory(string ccGroup, string transportMode = "", bool containerized = false)
		{
			switch (ccGroup)
			{
				case ChargeCodeGroupList.Codes.Freight:
				case ChargeCodeGroupList.Codes.Insurance:
					switch (transportMode)
					{
						case TransportMode.AIR:
							return RateCategory.AIR;
						case TransportMode.SEA:
							return containerized ? RateCategory.FCL : RateCategory.LCL;
						default:
							return string.Empty;
					}

				case ChargeCodeGroupList.Codes.CustomsDuty:
					return string.Empty;
			}

			return GetForwardingChargeCodeGroupsByRateCategory().SingleOrDefault(x => x.Value.Contains(ccGroup)).Key;
		}

		#endregion

		#region RatingFormActionsMenu

		public static class RatingFormActionsMenu
		{
			public static MultilingualString ImportForwardAirCosts
			{
				get { return ResString.GetMultilingualString("8401a234-26d3-4038-bd78-bfa274fe66f1", "Import Forward Air Costs"); }
			}

			public static MultilingualString ImportIATATACT
			{
				get { return ResString.GetMultilingualString("7edb4f88-9561-4276-ba06-8077e4b15976", "Import IATA Tact Rate File"); }
			}

			public static MultilingualString PublishAllCostsAsGlobalCosts
			{
				get { return ResString.GetMultilingualString("2a7e76e7-f5b2-4e52-8702-5abea3fde16f", "Publish all costs as global costs"); }
			}

			public static MultilingualString PublishAllRatesAsGlobalRates
			{
				get { return ResString.GetMultilingualString("b49a7165-4a29-4be2-9b57-1b1ae1009bff", "Publish all rates as global rates"); }
			}
		}

		#endregion

		#region RateEntryContextItems

		public static class RateEntryContextMenu
		{
			public static MultilingualString Duplicate
			{
				get { return ResString.GetMultilingualString("ac6df8a7-6911-4725-862d-1eeaef2ae6ff", "Duplicate"); }
			}

			public static MultilingualString ChangeValidityDates
			{
				get { return ResString.GetMultilingualString("d3e30c10-72de-4d95-9ceb-d734a00748ea", "Change Validity Dates"); }
			}

			public static MultilingualString PublishSelectedAsGlobalCosts
			{
				get { return ResString.GetMultilingualString("daed9f36-aad6-4764-8923-19e5ef512885", "Publish Selected as Global Costs(s)"); }
			}

			public static MultilingualString PublishSelectedAsGlobalRates
			{
				get { return ResString.GetMultilingualString("d728a4c6-0dda-41a5-bc99-dddaf4f1fe8b", "Publish Selected as Global Rates(s)"); }
			}

			public static MultilingualString QuoteRelatedRatesFromCurrentTab
			{
				get { return ResString.GetMultilingualString("823035ff-1bba-4166-9460-51022b89b157", "Quote Related Rate(s) (From Current Tab)"); }
			}

			public static MultilingualString QuoteSelectedRatesFromAllTabs
			{
				get { return ResString.GetMultilingualString("412c6b6f-1464-4905-9f24-83153d2694a0", "Quote Selected Rate(s) (Across All Tabs)"); }
			}

			public static MultilingualString QuoteSelectedRatesFromCurrentTab
			{
				get { return ResString.GetMultilingualString("645c61ed-08e3-4b2c-bd4b-ca4283dbfec5", "Quote Selected Rate(s) (From Current Tab)"); }
			}
		}

		#endregion

		#region WiseRatesViewContextItems

		public static class WiseRatesViewContextMenu
		{
			public static MultilingualString ShowCommodityGroups => ResString.GetMultilingualString("ACA9887E-9A63-452F-81A3-C49D42807812", "Show Commodity Groups");
			public static MultilingualString AssignGlobalChargeCode => ResString.GetMultilingualString("CAE1757E-493D-4E01-B4F4-3B6164F76271", "Assign Universal Charge Code to Global Charge Code");
			public static MultilingualString AssignLocalChargeCode => ResString.GetMultilingualString("BD9560CB-0BAD-4559-B708-23918BA6F134", "Assign Universal Charge Code to Charge Code");
			public static MultilingualString AssignCarrierCode(string code) => ResString.GetMultilingualString("5771fedb-c432-431d-917e-00f3e1312e16", "Assign SCAC/C1C '{0}' to Carrier", code);
			public static MultilingualString AssignIATACode(string code) => ResString.GetMultilingualString("58344D54-F60E-41B9-B640-EBF93A40E965", "Assign IATA Code '{0}' to Carrier", code);
			public static MultilingualString AssignIATACode() => ResString.GetMultilingualString("70B5F296-5C9F-4C77-B801-DCE883F0618A", "Assign IATA Code to Carrier");
			public static MultilingualString CreateAirlineAndAssignIATACode(string code) => ResString.GetMultilingualString("1D90817F-C2CD-4C23-AE69-951889E7EF65", "Create Airline and assign to Carrier for IATA Code '{0}'", code);
			public static MultilingualString CreateAirlineAndAssignIATACode() => ResString.GetMultilingualString("10C5F0DA-6B63-4593-9F13-19C58D81BF6F", "Create Airline and assign to Carrier for IATA Code");

			public static MultilingualString AssignCarrierCode() => ResString.GetMultilingualString("343125b8-1030-48d3-8405-808d841f7c62", "Assign SCAC/C1C to Carrier");
			public static MultilingualString AssignCarrierServiceLevel => ResString.GetMultilingualString("63656acb-e1be-4890-8163-11515e1ffad1", "Assign Carrier Service Level(s) to Carrier");
			public static MultilingualString AssignSeaContainer(string isoType)
				=> string.IsNullOrWhiteSpace(isoType)
					? AssignContainer
					: ResString.GetMultilingualString("221a8512-9ba2-4d4c-b367-d89bce4841c5", "Create SEA Container and assign ISO Type '{0}'", isoType);
			public static MultilingualString CreateAirContainer(string containerCode)
				=> string.IsNullOrEmpty(containerCode)
					? AssignContainer
					: ResString.GetMultilingualString("2b58a87f-c22d-457f-ab2e-39f2650630b0", "Create a new '{0}' AIR Container", containerCode);
			static MultilingualString AssignContainer => ResString.GetMultilingualString("395b54ce-41c1-4368-930a-c8f3b2afabbd", "Assign Container");
		}

		#endregion

		public static string RateNotePrefix
		{
			get { return Res.GetString("8efcbd00-1635-473a-a665-16fec850288f", "RATE NOTE:"); }
		}

		public static char BulletPoint
		{
			get { return (char)8226; }
		}
		public static string NoResults
		{
			get { return Res.GetString("UniversalChargeCodeModule|NoResultsFound", "No results found."); }
		}
	}
}

