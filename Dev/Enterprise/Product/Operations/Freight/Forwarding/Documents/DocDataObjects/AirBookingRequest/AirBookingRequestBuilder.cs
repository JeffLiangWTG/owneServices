using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirBookingRequestValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AirBookingRequestBuilder
	{
		public AirBookingRequestBuilder(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			context = new CommonContext(consol.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingConsol consol;
		readonly IContext context;

		ZString AirlinePrefix => airlinePrefix ?? (airlinePrefix = consol.JK_MasterBillNum.SubstringSafe(0, 3)).Value;
		ZString? airlinePrefix;
		ZBool IsEmiratesAirline
		{
			get
			{
				return AirlinePrefix.EqualsIgnoringCase("176");
			}
		}

		AirlineConfig AirlineConfig => airlineConfig ?? (airlineConfig = AirBookingCarrierConfigurationManager.GetAirlineConfig(AirlinePrefix));
		AirlineConfig airlineConfig;

		public AirBookingRequest Build()
		{
			var bookingRequest = new AirBookingRequest(
				nameof(ForwardingConsol),
				consol.JK_UniqueConsignRef);

			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));

			PopulateHeader(bookingRequest);

			bookingRequest.FlightDetails = CreateFlightDetails();
			bookingRequest.Dimensions = CreateDimensions();
			bookingRequest.Ulds = CreateContainers();

			PopulateSpecialHandlingCodes(bookingRequest);
			PopulateGoodsDetails(bookingRequest);
			PopulateAdditionalInformation(bookingRequest);
			PopulateCarrierContractNumbers(bookingRequest);
			PopulateCarrierBookingReference(bookingRequest);
			PopulateTemperature(bookingRequest);

			AddValidations(bookingRequest);
			bookingRequest.ValidateAllIncludingChildren();

			return bookingRequest;
		}

		#region PopulateHeader

		void PopulateHeader(AirBookingRequest bookingRequest)
		{
			bookingRequest.EBookingApiUrlCode =
				!string.IsNullOrEmpty(FreightDataRegistry.Instance.CustomEBookingApiServiceUrl.Value) ? new ZString("CUSTOM") : FreightDataRegistry.Instance.EBookingServicesApiUrl.Value.SelectedEBookingApiUrlCode; // Constant

			bookingRequest.TermsAndConditions = GetTermsAndConditions();
			bookingRequest.MasterAirWaybillNumber = consol.JK_MasterBillNum.FormatAirMAWB();
			bookingRequest.BookingReferenceNumber = consol.JK_UniqueConsignRef;

			var carrier = consol.ShippingLineAddress?.Header;

			var airlinePrefix = carrier?.MiscServ.Airline?.RM_TwoCharacterCode ?? string.Empty;
			var airlineName = carrier?.MiscServ.Airline?.RM_AirlineName1 ?? string.Empty;

			var carrierCodeDescriptionList = new CodeDescriptionPairList();
			carrierCodeDescriptionList.AddPair(airlinePrefix, airlineName);

			bookingRequest.CASS = GetFormattedCASSNumber();

			bookingRequest.Carrier = new CodeDescription(carrierCodeDescriptionList)
			{
				Code = airlinePrefix,
				Description = airlineName
			};

			bookingRequest.Agent = Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName;

			if (bookingRequest.Agent.IsEmpty)
			{
				bookingRequest.Agent = GlbBranch.CurrentBranch?.OrgProxy?.OH_FullName ?? ZString.Empty;
			}

			var airTransports = consol
				.Transports
				.Cast<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Air)
				.ToArray();

			var origin = airTransports.FirstOrDefault();

			var originAirport = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = origin?.JW_RL_NKLoadPort ?? ZString.Empty
			};

			bookingRequest.OriginAirport = originAirport;

			var destination = airTransports.LastOrDefault();

			var destinationAirport = new Unloco(context.Factory, context.Unlocos, context.Countries)
			{
				Code = destination?.JW_RL_NKDiscPort ?? ZString.Empty
			};

			bookingRequest.DestinationAirport = destinationAirport;
		}

		ZString[] GetTermsAndConditions()
		{
			ZString[] CreateTermsAndConditionLines(string termsAndConditions)
			{
				const int termsAndConditionsMaxLineLength = 145;

				var lines = TextSplitter.Split(termsAndConditions, termsAndConditionsMaxLineLength);

				return lines
					.Select(s => new ZString(s))
					.ToArray();
			}

			StmNote GetTermsAndConditionsNote()
			{
				var refAirline = RefAirline.LoadFromAirlinePrefix(context.Factory, AirlinePrefix);

				return refAirline
					?.Notes
					.FindByDescription(PredefinedNoteTypes.Instance.TermsAndConditions.Description)
					.Where(note => !note.ST_NoteText.IsEmpty)
					.FirstOrDefault();
			}

			if (GetTermsAndConditionsNote() is StmNote termsAndConditionsNote)
			{
				return CreateTermsAndConditionLines(termsAndConditionsNote.ST_NoteText);
			}

			var airlineOptions = AirlineConfig?.Options;
			if (airlineOptions?.RequiredTermsAgreement ?? false)
			{
				var message = Res.GetString("e0223299-3ec5-414b-bc25-2ba56b141c6d", "By submitting or canceling this eBooking you confirm that you have read, understood and agree to all the terms and conditions for this Airline.");
				return CreateTermsAndConditionLines(message);
			}

			return Array.Empty<ZString>();
		}

		string GetFormattedCASSNumber()
		{
			var unformattedNumber = new ZString(Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode).KeepNumericCharacters();

			if (unformattedNumber.IsEmpty)
			{
				return string.Empty;
			}

			var cass = unformattedNumber.SubstringSafe(7, 4);
			return FormattableString.Invariant($"{unformattedNumber.Left(2)}-{unformattedNumber.SubstringSafe(2, 1)} {unformattedNumber.SubstringSafe(3, 4)}{(cass.IsEmpty ? "" : "/" + cass)}"); // formatting logic of the CASS number
		}

		#endregion

		#region PopulateGoodsDetails

		void PopulateTemperature(AirBookingRequest bookingRequest)
		{
			bookingRequest.RequiresTemperatureControl = consol.JK_RequiresTemperatureControl;
			bookingRequest.TemperatureMinimum = new Measurement
			{
				Value = consol.JK_RequiredTemperatureMinimum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = consol.JK_RequiredTemperatureUnit
				}
			};

			bookingRequest.TemperatureMaximum = new Measurement
			{
				Value = consol.JK_RequiredTemperatureMaximum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = consol.JK_RequiredTemperatureUnit
				}
			};
		}

		#endregion

		#region PopulateGoodsDetails

		void PopulateGoodsDetails(AirBookingRequest bookingRequest)
		{
			PopulateTotalPieces(bookingRequest);
			PopulateTotalWeight(bookingRequest);
			PopulateTotalVolume(bookingRequest);

			var productMustBeProvidedMessage = Res.GetString("74c8bd77-30fa-42c6-a8bc-33ed2ca7c516", "Product must be provided");
			var productShouldBeFromTheListMessage = Res.GetString("63a0384b-f41e-4b47-a8a9-ace2cc3436f1", "Product should be selected from the list");

			if (CanLoadCommodityFromRefData())
			{
				bookingRequest.ProductInfo.ValueChanged += (s, e) =>
				{
					bookingRequest.Commodity = ZString.Empty;
					PopulateCommoditiyCodesBasedOnSelectedProduct(bookingRequest, bookingRequest.Product);
				};
				bookingRequest.CommodityInfo.ValueChanged += (s, e) =>
					PopulateSpecialHandlingCodes(bookingRequest, bookingRequest.CommodityCollection, bookingRequest.Commodity);

				PopulateProductListWithRefData(bookingRequest, productMustBeProvidedMessage, productShouldBeFromTheListMessage);

				var selectedProduct = GetOverridenValue(nameof(bookingRequest.Product));

				if (string.IsNullOrEmpty(selectedProduct) && bookingRequest.ProductList.Count == 1)
				{
					bookingRequest.Product = bookingRequest.ProductList[0]?.Code;
					selectedProduct = bookingRequest.Product;
				}

				PopulateCommoditiyCodesBasedOnSelectedProduct(bookingRequest, selectedProduct);
				bookingRequest.CommodityIsVisible = true;
			}
			else
			{
				var goodsDescriptions = AirlineConfig?.Commodities;
				PopulateGoodsDescription(bookingRequest, goodsDescriptions);
				PopulateProductListWithAirlineConfiguration(bookingRequest, productMustBeProvidedMessage, productShouldBeFromTheListMessage);
				bookingRequest.CommodityIsVisible = (AirlineConfig?.Options?.CommodityCode ?? AirlineConfigOptionValue.NotSupported) != AirlineConfigOptionValue.NotSupported;
			}
		}

		void PopulateProductListWithAirlineConfiguration(AirBookingRequest bookingRequest, string productMustBeProvidedMessage, string productShouldBeFromTheListMessage)
		{
			var products = AirlineConfig?.Products;
			if (!products.IsNullOrEmpty())
			{
				var productList = CreateProductList(products);
				bookingRequest.ProductList = productList;
				bookingRequest.ProductInfo.AddMessageError(() => bookingRequest.Product.IsEmpty, productMustBeProvidedMessage);
				bookingRequest.ProductInfo.AddMessageError(() => !bookingRequest.ProductList.ContainsCode(bookingRequest.Product), productShouldBeFromTheListMessage);
			}
		}

		bool CanLoadCommodityFromRefData()
		{
			var commodityPivots = context.Factory.LoadTop1<RefAirlineProductCodeCommodityCodePivot>(
												new ZQuery(RefAirlineProductCodeCommodityCodePivotSchema.RPC_AirlineID, AirlinePrefix));

			var canLoadCommodityFromRefData = commodityPivots != null;
			return canLoadCommodityFromRefData;
		}

		void PopulateProductListWithRefData(AirBookingRequest bookingRequest, string productMustBeProvidedMessage, string productShouldBeFromTheListMessage)
		{
			var refAirlineProducts = context.Factory.Load<RefAirlineProductCode>(new ZQuery(RefAirlineProductCodeSchema.RAR_AirlineID, AirlinePrefix));
			bookingRequest.ProductDescriptionAndDetailsMap = new Dictionary<string, RefAirlineProductCode>();
			if (!refAirlineProducts.IsNullOrEmpty())
			{
				refAirlineProducts.ForEach(refProduct =>
				{
					bookingRequest.ProductDescriptionAndDetailsMap[refProduct.RAR_Description] = refProduct;
				});
				bookingRequest.ProductList = CreateProductList(PopulateAirlineConfigProductList(refAirlineProducts).ToList());
				bookingRequest.ProductInfo.AddMessageError(() => bookingRequest.Product.IsEmpty, productMustBeProvidedMessage);
				bookingRequest.ProductInfo.AddMessageError(() => !bookingRequest.ProductList.ContainsCode(bookingRequest.Product), productShouldBeFromTheListMessage);
			}
		}

		string GetOverridenValue(string propertyName)
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load(consol, ConsolDocumentDataStoreNames.AirBookingRequest);

			var xmlData = documentData?.ReadXml();
			if (xmlData?.GetEntity()?.Properties.TryGetValue(propertyName, out var result) ?? false)
			{
				return result?.Value?.ToString();
			}

			return null;
		}

		void PopulateCommoditiyCodesBasedOnSelectedProduct(AirBookingRequest bookingRequest, string selectedProduct)
		{
			if (string.IsNullOrEmpty(selectedProduct) || !bookingRequest.ProductDescriptionAndDetailsMap.ContainsKey(selectedProduct))
			{
				bookingRequest.CommodityCollection = null;
				return;
			}

			var commodityQuery = CreateCommodityQuery(bookingRequest.ProductDescriptionAndDetailsMap[selectedProduct].PK);
			var refAirlineCommodities = context.Factory.Load<RefAirlineCommodityCode>(commodityQuery);
			bookingRequest.CommodityCollection = null;
			if (!refAirlineCommodities.IsNullOrEmpty())
			{
				PopulateCommodities(bookingRequest, ConvertToAirlineConfigCommodityList(refAirlineCommodities).ToList());
			}

			var overridenCommodity = GetOverridenValue(nameof(bookingRequest.Commodity));

			if (!string.IsNullOrEmpty(overridenCommodity))
			{
				bookingRequest.Commodity = overridenCommodity;
			}
			else
			{
				var defaultCommodity = GetDefaultCommodity(bookingRequest, selectedProduct, bookingRequest.OriginAirport.Code, bookingRequest.DestinationAirport.Code);
				if (!defaultCommodity.IsEmpty)
				{
					bookingRequest.Commodity = defaultCommodity;
				}
			}
		}

		ZString GetDefaultCommodity(AirBookingRequest bookingRequest, string selectedProduct, ZString origin, ZString destination)
		{
			var refAirline = GetRefAirline();
			var productCode = GetProductCode(bookingRequest, selectedProduct);
			if (refAirline != null && !productCode.IsEmpty)
			{
				return FindBestMatchCommodityCode(refAirline.PK, productCode, origin, destination);
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString GetProductCode(AirBookingRequest bookingRequest, string selectedProduct)
		{
			if (bookingRequest.ProductDescriptionAndDetailsMap?.TryGetValue(selectedProduct, out var product) ?? false)
			{
				return product.RAR_Code;
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString FindBestMatchCommodityCode(ZGuid refAirlinePK, ZString product, ZString origin, ZString destination)
		{
			var originAnddestinationMatchQuery = new ZQuery();
			originAnddestinationMatchQuery.AddToFilter(GetMatchOriginAndDestinationSubQuery(origin, destination), JoinCondition.Or);
			originAnddestinationMatchQuery.AddToFilter(GetMatchOriginAndDestinationSubQuery(origin, ZString.Empty), JoinCondition.Or);
			originAnddestinationMatchQuery.AddToFilter(GetMatchOriginAndDestinationSubQuery(ZString.Empty, destination), JoinCondition.Or);
			originAnddestinationMatchQuery.AddToFilter(GetMatchOriginAndDestinationSubQuery(ZString.Empty, ZString.Empty), JoinCondition.Or);

			var query = new ZQuery();
			query.AddToFilter(originAnddestinationMatchQuery, JoinCondition.And);

			var airlineQuery = new ZQuery();
			airlineQuery.AddToFilter(RefAirlineDefaultCommodityCodeSchema.RDC_RM, refAirlinePK);

			query.AddToFilter(airlineQuery, JoinCondition.And);

			var productQuery = new ZQuery();
			productQuery.AddToFilter(RefAirlineDefaultCommodityCodeSchema.RDC_RAR_NKProductCode, product);

			query.AddToFilter(productQuery, JoinCondition.And);

			ZString orderByString = $"{RefAirlineDefaultCommodityCodeSchema.Constants.RDC_RL_NKOrigin} DESC, {RefAirlineDefaultCommodityCodeSchema.Constants.RDC_RL_NKDestination} DESC";
			query.OrderBy = orderByString;

			var result = context.Factory.LoadTop1<RefAirlineDefaultCommodityCode>(query);
			return result != null ? result.RDC_RAC_NKCommodityCode : ZString.Empty;
		}

		ZQuery GetMatchOriginAndDestinationSubQuery(ZString origin, ZString destination)
		{
			var query = new ZQuery();
			query.AddToFilter(RefAirlineDefaultCommodityCodeSchema.RDC_RL_NKOrigin, origin);
			query.AddToFilter(RefAirlineDefaultCommodityCodeSchema.RDC_RL_NKDestination, destination);
			return query;
		}

		RefAirline GetRefAirline()
		{
			var query = new ZQuery();
			query.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, AirlinePrefix);
			return context.Factory.LoadTop1<RefAirline>(query);
		}

		IEnumerable<AirlineConfigCommodity> ConvertToAirlineConfigCommodityList(RefAirlineCommodityCode[] refCommodities)
		{
			return refCommodities.Select(commodity =>
			{
				var specialHandlingCodesString = (string)commodity.RAC_SpecialHandlingCodes;
				var specialHandlingCodes = specialHandlingCodesString
					.Split(',')
					.Select(code => code.Trim())
					.Where(code => !code.IsNullOrEmpty())
					.ToArray();
				var newAirlineConfigCommodity = new AirlineConfigCommodity()
				{
					Code = commodity.RAC_Code,
					Description = commodity.RAC_Description,
					SpecialHandlingCodes = specialHandlingCodes
				};
				return newAirlineConfigCommodity;
			});
		}

		IEnumerable<AirlineConfigProduct> PopulateAirlineConfigProductList(RefAirlineProductCode[] refAirlineProducts)
		{
			return refAirlineProducts.Select(refProduct =>
			{
				var airlineConfigProduct = new AirlineConfigProduct();
				airlineConfigProduct.Code = refProduct.RAR_Code;
				airlineConfigProduct.Description = refProduct.RAR_Description;
				return airlineConfigProduct;
			});
		}

		void PopulateCommodities(AirBookingRequest bookingRequest, IReadOnlyCollection<AirlineConfigCommodity> commodities)
		{
			if (!commodities.IsNullOrEmpty())
			{
				if (bookingRequest.CommodityCollection == null)
				{
					bookingRequest.CommodityCollection = new AirlineConfigCommodityBusinessObjectCollection();
				}
				else
				{
					bookingRequest.CommodityCollection.RemoveAll();
				}

				foreach (var airlineConfigCommodity in commodities.OrderBy(c => c.Description))
				{
					bookingRequest.CommodityCollection.Add(new AirlineConfigCommodityBusinessObject(airlineConfigCommodity));
				}
			}
			else
			{
				bookingRequest.CommodityCollection = null;
			}
		}

		ZDBOnlyQuery CreateCommodityQuery(ZGuid refProductPk)
		{
			var subQueryProductCommodityPivot = new ZDBOnlySubQuery(typeof(RefAirlineProductCodeCommodityCodePivot), RefAirlineProductCodeCommodityCodePivotSchema.RPC_RAC);
			subQueryProductCommodityPivot.AddToFilter(new ZQuery(RefAirlineProductCodeCommodityCodePivotSchema.RPC_RAR, SQLComparisonOperator.Equal, refProductPk));

			var queryCommodity = new ZDBOnlyQuery(typeof(RefAirlineCommodityCode));
			queryCommodity.AddSubQuery(RefAirlineCommodityCodeSchema.PK, subQueryProductCommodityPivot, JoinCondition.And);
			return queryCommodity;
		}

		void PopulateGoodsDescription(AirBookingRequest bookingRequest, IReadOnlyCollection<AirlineConfigCommodity> commodities)
		{
			if (!commodities.IsNullOrEmpty())
			{
				if (bookingRequest.GoodsDescriptionCollection == null)
				{
					bookingRequest.GoodsDescriptionCollection = new AirlineConfigCommodityBusinessObjectCollection();
				}
				else
				{
					bookingRequest.GoodsDescriptionCollection.RemoveAll();
				}

				PopulateGoodsDescriptionCollection(bookingRequest, commodities);

				bookingRequest.GoodsDescriptionInfo.ValueChanged += (s, e) =>
					PopulateSpecialHandlingCodes(bookingRequest, bookingRequest.GoodsDescriptionCollection, bookingRequest.GoodsDescription);

				var overridenGoodDescription = GetOverridenValue(nameof(bookingRequest.GoodsDescription));
				if (!string.IsNullOrEmpty(overridenGoodDescription))
				{
					bookingRequest.GoodsDescription = overridenGoodDescription;
				}
			}
			else
			{
				bookingRequest.GoodsDescriptionCollection = null;
				switch (consol.Shipments.Count)
				{
					case 0:
						break;

					case 1:
						const int goodsDescriptionMaxLength = 15;

						bookingRequest.GoodsDescription = !consol.Shipments[0].JS_GoodsDescription.IsEmpty
							? consol.Shipments[0].JS_GoodsDescription.Left(goodsDescriptionMaxLength)
							: consol.Shipments[0].DetailedGoodsDescriptionNoteText.Left(goodsDescriptionMaxLength);

						break;

					default:
						bookingRequest.GoodsDescription = (NoResString)"Consolidation"; // programmatic constant
						break;
				}

				bookingRequest.GoodsDescriptionInfo.AddMaximumLengthValidation(Res.GetString("2797e678-ca2a-4f59-b2c6-3f7e84332375", "Goods description"), 15);
			}
		}

		CodeDescriptionPairList CreateProductList(IReadOnlyCollection<AirlineConfigProduct> products)
		{
			var result = new CodeDescriptionPairList();

			foreach (var airlineConfigProduct in products.OrderBy(product => product.Description))
			{
				result.Add(airlineConfigProduct);
			}

			return result;
		}

		void PopulateGoodsDescriptionCollection(AirBookingRequest bookingRequest, IReadOnlyCollection<AirlineConfigCommodity> commodities)
		{
			foreach (var airlineConfigCommodity in commodities.OrderBy(c => c.Description))
			{
				bookingRequest.GoodsDescriptionCollection.Add(new AirlineConfigCommodityBusinessObject(airlineConfigCommodity));
			}
		}

		bool ThereAreLooseOrULDContainersOnConsol(AirBookingRequest bookingRequest) =>
			bookingRequest.Ulds.Count > 0 || bookingRequest.Dimensions.Count > 0;

		void PopulateTotalPieces(AirBookingRequest bookingRequest)
		{
			var loosePieces = bookingRequest.Dimensions.Sum(d => d.Quantity);
			var uldPieces = bookingRequest.Ulds.Sum(u => u.ContainerCount);
			var totalPieces = loosePieces + uldPieces;

			bookingRequest.TotalPieces = totalPieces;
			bookingRequest.TotalPiecesInfo.AddWarning(() => bookingRequest.TotalPieces != totalPieces, Res.GetString("41795195-54dd-4442-b61a-8dfea75090d8", "Total Pieces does not equal the sum of Dimensions Pieces and/or ULD Counts."));
		}

		void PopulateTotalWeight(AirBookingRequest bookingRequest)
		{
			var weightUnit = Weight.Kilograms;

			if (PreAllocationHasValue())
			{
				weightUnit = consol.WeightVerificationUnit;
			}
			else
			{
				var weightUnits = bookingRequest
					.Dimensions
					.Select(d => d.Weight.Unit.Code)
					.Concat(bookingRequest.Ulds.Select(uld => uld.GrossWeight.Unit.Code))
					.Distinct()
					.Where(unit => Weight.ContainsCode(unit));
				weightUnit = weightUnits.Any() && weightUnits.All(x => Weight.IsImperial(x)) ? Weight.Pounds : Weight.Kilograms;
			}

			var looseWeight = bookingRequest.Dimensions.Sum(d =>
				Utilities.Round(Weight.Convert(d.Weight.Value, d.Weight.Unit.Code, weightUnit), 3));
			var uldWeight = bookingRequest.Ulds.Sum(u =>
				Utilities.Round(Weight.Convert(u.GrossWeight.Value, u.GrossWeight.Unit.Code, weightUnit), 3));
			var totalWeight = CalculateRounding(weightUnit, looseWeight + uldWeight, 1);

			bookingRequest.TotalWeight = new Measurement
			{
				Value = totalWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = weightUnit,
				}
			};
		}

		void PopulateTotalVolume(AirBookingRequest bookingRequest)
		{
			var lengthUnits = bookingRequest
					.Dimensions
					.Select(d => d.Length.Unit.Code)
					.Distinct()
					.Where(unit => Length.ContainsCode(unit))
					.ToArray();
			var useImperialUnits = lengthUnits.Length > 0 & lengthUnits.All(x => Length.IsImperial(x));
			var lengthUnit = useImperialUnits
				? Length.Feet
				: Length.Metres;
			var volumeUnit = useImperialUnits
				? Volume.CubicFeet
				: Volume.CubicMetres;

			var looseVolume = bookingRequest.Dimensions.Sum(dim => CalculateDimensionVolume(dim, lengthUnit, volumeUnit));
			var uldsVolume = consol.Containers.OfType<ForwardingContainer>().Sum(container => CalculateContainerVolume(container));
			var totalVolume = looseVolume + uldsVolume;

			if (PreAllocationHasValue() && volumeUnit != consol.VolumeVerificationUnit)
			{
				totalVolume = Volume.ConvertSafe(totalVolume, volumeUnit, consol.VolumeVerificationUnit, false);
				volumeUnit = consol.VolumeVerificationUnit;
			}

			bookingRequest.TotalVolume = new Measurement
			{
				Value = CalculateRounding(volumeUnit, totalVolume, 3),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = volumeUnit
				}
			};
		}

		ZDecimal CalculateRounding(ZString unit, ZDecimal value, ZInt defaultNumberOfDecimals)
		{
			var roundingMode = FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetRoundingMode(consol.JK_TransportMode, unit);
			var numberOfDecimals = FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.Value
			? (ZInt)FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetNumberOfDecimals(consol.JK_TransportMode, unit)
			: defaultNumberOfDecimals;
			if (numberOfDecimals < 0)
			{
				numberOfDecimals = defaultNumberOfDecimals;
			}
			return DefaultNumberOfDecimals.GetRoundedValue(value, roundingMode, numberOfDecimals);
		}

		ZDecimal CalculateDimensionVolume(PackingLine packingLine, string dimensionUQ, string volumeUnit)
		{
			if (packingLine == null)
			{
				return 0m;
			}

			// This is for pre-allocation which does have volume but not have actual length, width or height
			if (packingLine.Volume != null && packingLine.Volume.Value > 0)
			{
				return Volume.Convert(packingLine.Volume.Value, packingLine.Volume.Unit.Code, volumeUnit, false);
			}

			var packVolume = Length.Convert(packingLine.Length.Value, packingLine.Length.Unit.Code, dimensionUQ)
				* Length.Convert(packingLine.Width.Value, packingLine.Width.Unit.Code, dimensionUQ)
				* Length.Convert(packingLine.Height.Value, packingLine.Height.Unit.Code, dimensionUQ);

			return Utilities.Round(packVolume * packingLine.Quantity, 3);
		}

		ZDecimal CalculateContainerVolume(ForwardingContainer container)
		{
			return Utilities.Round(container.JC_Calc_ActualCapacity * container.JC_ContainerCount, 3);
		}
		#endregion

		#region PopulateAdditionalInformation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		void PopulateAdditionalInformation(AirBookingRequest bookingRequest)
		{
			var allNotes = consol.Notes.GetAllNotes();

			var specialInstructions = new ZStringBuilder();
			var dangerousGoodsHandlingInformation = new ZStringBuilder();
			var goodsHandlingInstructions = new ZStringBuilder();
			var bookingConfirmationNotes = new ZStringBuilder();

			const string bookingConfirmationNoteDescription = "Booking Confirmation Notes";

			foreach (var notes in allNotes.Cast<StmNote>())
			{
				if (notes.ST_Description == PredefinedNoteTypes.Instance.SpecialInstructions.Description)
				{
					specialInstructions.Append(notes.ST_NoteDataAsText);
				}
				else if (notes.ST_Description == PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description)
				{
					dangerousGoodsHandlingInformation.Append(notes.ST_NoteDataAsText);
				}
				else if (notes.ST_Description == PredefinedNoteTypes.Instance.HandlingInstructions.Description)
				{
					goodsHandlingInstructions.Append(notes.ST_NoteDataAsText);
				}
				else if (notes.ST_Description.EqualsIgnoringCase(bookingConfirmationNoteDescription))
				{
					bookingConfirmationNotes.Append(notes.ST_NoteDataAsText);
				}
			}

			bookingRequest.SpecialInstructions = specialInstructions.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
			bookingRequest.DangerousGoodsHandlingInformation = dangerousGoodsHandlingInformation.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
			bookingRequest.GoodsHandlingInstructions = goodsHandlingInstructions.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
			bookingRequest.BookingConfirmationNotes = bookingConfirmationNotes.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);

			if (bookingRequest.FlightDetails.Any(f => f.Status.Code == TransportStatus.Planned))
			{
				bookingRequest.FlightDetailsNote = Res.GetString("058f9b88-25a4-46df-8351-6056189b7bdd", "(Note: For ‘Planned’ status, flight details might not be sent if the airline returns offers based on available capacity at the time of query.)");
			}
		}

		#endregion

		#region PopulateCarrierContractNumbers

		void PopulateCarrierContractNumbers(AirBookingRequest bookingRequest)
		{
			var types = new CodeDescriptionPairList();
			types.AddPair(Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, (NoResString)"Carrier Contract Number"); // programmatic constant

			var referenceNumbers = new List<ReferenceNumber>();

			foreach (var number in consol.Numbers.Cast<CusEntryNumber>())
			{
				if (number.CE_EntryType == Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON)
				{
					var referenceNumber = ReferenceNumber.Create(context, number, types);
					referenceNumbers.Add(referenceNumber);
				}
			}

			bookingRequest.CarrierContractNumbers = referenceNumbers;
		}

		#endregion

		#region CreateFlightDetails

		FlightDetail[] CreateFlightDetails()
		{
			var flightDetails = new List<FlightDetail>();

			foreach (var transport in consol.Transports.Cast<Freight.Business.Transport>())
			{
				if (!transport.IsAir)
				{
					continue;
				}

				var flightDetail = new FlightDetail(transport.PK);

				flightDetail.TransportType = new CodeDescription(transport.JW_TransportType_List)
				{
					Code = transport.JW_TransportType
				};

				flightDetail.FlightNumber = transport.JW_VoyageFlight;

				flightDetail.PortOfLoading = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = transport.JW_RL_NKLoadPort
				};

				flightDetail.PortOfDischarge = new Unloco(context.Factory, context.Unlocos, context.Countries)
				{
					Code = transport.JW_RL_NKDiscPort
				};

				flightDetail.ETD = transport.JW_ETD;
				flightDetail.ETA = transport.JW_ETA;

				flightDetail.Status = new FlightStatus(transport.JW_Status_List)
				{
					Code = transport.JW_Status
				};

				var warnings = OnlineFlightMatchingValidationHelper.GetGenericWarningForMatchStatus(transport);
				flightDetail.FlightNumberInfo.AddWarning(() => warnings.Any(), string.Join(System.Environment.NewLine, warnings));

				flightDetails.Add(flightDetail);
			}

			return flightDetails.ToArray();
		}

		#endregion

		#region CreatePackLines

		bool PreAllocationHasValue()
		{
			return (!consol.JK_TotalShipmentCountCheck.IsEmpty)
				|| (!consol.JK_TotalShipmentActWeightCheck.IsEmpty)
				|| (!consol.JK_TotalShipmentActVolumeCheck.IsEmpty)
				|| (!consol.JK_MaximumAllowablePackageHeight.IsEmpty)
				|| (!consol.JK_MaximumAllowablePackageLength.IsEmpty)
				|| (!consol.JK_MaximumAllowablePackageWidth.IsEmpty);
		}

		/// <summary>
		/// Create dimensions for loose information:
		///	When PreAllocationHasValue then use PreAllocation value
		///	Otherwise use loose pack lines those are not inside container
		///	Therefore in Populate total pieces/weight/volume, no need to use PreAllocationHasValue condition again
		///	</summary>
		/// <returns></returns>
		PackingLine[] CreateDimensions()
		{
			if (PreAllocationHasValue())
			{
				return CreateDimensionFromPreAllocation();
			}
			else
			{
				var loosePackLines = consol
					.ShipmentsForTotalling.Cast<ForwardingShipment>()
					.SelectMany(shipment => shipment.OuterPackLines)
					.Cast<ForwardingPackLine>()
					.Where(p => p.GetContainer(consol) == null)
					.ToArray();

				var isWeightMetric = loosePackLines.Any(packline => !Weight.IsImperial(packline.JL_ActualWeightUQ));
				var packLines = new List<PackingLine>();

				foreach (var packLine in loosePackLines)
				{
					var invalidDimensionValueMessage = Res.GetString("263006bb-7f88-43c6-9f69-d56e09921d8e", "Invalid Dimension Value.");

					var unitOfMeasurement = new CodeDescription(context.DimensionUnits) { Code = packLine.JL_UnitOfDimension };
					var length = new Measurement { Value = packLine.JL_Length, Unit = unitOfMeasurement };
					var width = new Measurement { Value = packLine.JL_Width, Unit = unitOfMeasurement };
					var height = new Measurement { Value = packLine.JL_Height, Unit = unitOfMeasurement };

					length.ValueInfo.AddMessageError(() => length.Value <= 0, invalidDimensionValueMessage);
					width.ValueInfo.AddMessageError(() => width.Value <= 0, invalidDimensionValueMessage);
					height.ValueInfo.AddMessageError(() => height.Value <= 0, invalidDimensionValueMessage);

					var packingLineDataObject = new PackingLine(packLine.PK, packLine.Factory)
					{
						Quantity = packLine.JL_PackageCount,
						Length = length,
						Width = width,
						Height = height,
						Weight = new Measurement
						{
							Value = packLine.JL_ActualWeight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = packLine.JL_ActualWeightUQ
							}
						}
					};

					packLines.Add(packingLineDataObject);
				}

				return packLines.ToArray();
			}
		}

		PackingLine[] CreateDimensionFromPreAllocation()
		{
			var piecesCount = consol.JK_TotalShipmentCountCheck;

			var measurementUnit = new CodeDescription(context.DimensionUnits) { Code = consol.JK_MaximumAllowablePackageUnit };
			var length = new Measurement { Value = consol.JK_MaximumAllowablePackageLength, Unit = measurementUnit };
			var height = new Measurement { Value = consol.JK_MaximumAllowablePackageHeight, Unit = measurementUnit };
			var width = new Measurement { Value = consol.JK_MaximumAllowablePackageWidth, Unit = measurementUnit };

			var weight = new Measurement
			{
				Value = consol.JK_TotalShipmentActWeightCheck,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = consol.JK_TotalShipmentChargeableUnit
				}
			};

			var packLines = new List<PackingLine>();
			var packingLine = new PackingLine(new ZGuid(), consol.Factory)
			{
				Quantity = piecesCount,
				Length = length,
				Height = height,
				Width = width,
				Weight = weight,
				Volume = new Measurement
				{
					Value = consol.JK_TotalShipmentActVolumeCheck,
					Unit = new CodeDescription(context.VolumeUnits)
					{
						Code = consol.JK_TotalShipmentActOtherUnit
					}
				},
			};

			packLines.Add(packingLine);
			return packLines.ToArray();
		}

		#endregion

		#region CreateContainers

		ULD[] CreateContainers()
		{
			var ulds = new List<ULD>();

			foreach (var forwardingContainer in consol.Containers.Cast<ForwardingContainer>())
			{
				var weightUQ = Weight.IsImperial(forwardingContainer.JC_GrossWeightUQ) ? Weight.Pounds : Weight.Kilograms;

				var uld = new ULD(forwardingContainer.PK)
				{
					Number = forwardingContainer.JC_ContainerNum,
					ContainerCount = forwardingContainer.JC_ContainerCount,
					Type = new UldContainerType
					{
						Code = forwardingContainer.RefContainer == null ? ZString.Empty : forwardingContainer.RefContainer.RC_Code
					},
					GrossWeight = new Measurement
					{
						Value = Utilities.Round(Weight.Convert(forwardingContainer.JC_GrossWeight, forwardingContainer.JC_GrossWeightUQ, weightUQ), 3),
						Unit = new CodeDescription(context.WeightUnits)
						{
							Code = weightUQ
						}
					},
					TareWeight = new Measurement
					{
						Value = Utilities.Round(Weight.Convert(forwardingContainer.JC_TareWeight, forwardingContainer.JC_GrossWeightUQ, weightUQ), 3),
						Unit = new CodeDescription(context.WeightUnits)
						{
							Code = weightUQ
						}
					},
					GoodsWeight = new Measurement
					{
						Value = Utilities.Round(Weight.Convert(forwardingContainer.GoodsWeight, forwardingContainer.GoodsWeightUQ, weightUQ), 3),
						Unit = new CodeDescription(context.WeightUnits)
						{
							Code = weightUQ
						}
					},
					ContainerVolume = new Measurement
					{
						Value = forwardingContainer.JC_Calc_ActualCapacity * forwardingContainer.JC_ContainerCount,
						Unit = new CodeDescription(context.VolumeUnits)
						{
							Code = Volume.CubicMetres
						}
					},
					IsNonOperativeReefer = forwardingContainer.JC_IsNonOperativeReefer
				};

				ulds.Add(uld);
			}

			return ulds.ToArray();
		}

		#endregion

		#region PopulateSpecialHandlingCodes

		void PopulateSpecialHandlingCodes(AirBookingRequest bookingRequest)
		{
			PopulateSpecialHandlingCodes(bookingRequest, ConsolSpecialHandlingCodes);
		}

		void PopulateSpecialHandlingCodes(AirBookingRequest bookingRequest, AirlineConfigCommodityBusinessObjectCollection configCollection, string code)
		{
			var config = configCollection?.Cast<AirlineConfigCommodityBusinessObject>()
				.FirstOrDefault(cd => cd.Code.EqualsIgnoringCase(code));
			var airlineSpecialHandlingCodes = config?.SpecialHandlingCodes ?? Enumerable.Empty<string>();
			var allSpecialHandlingCodes = ConsolSpecialHandlingCodes.Concat(airlineSpecialHandlingCodes).Distinct();
			PopulateSpecialHandlingCodes(bookingRequest, allSpecialHandlingCodes);
		}

		void PopulateSpecialHandlingCodes(AirBookingRequest bookingRequest, IEnumerable<string> specialHandlingCodes)
		{
			bookingRequest.SpecialHandlingItems = GetSpecialHandlingItems(specialHandlingCodes);
			bookingRequest.SpecialHandling = string.Join(", ", bookingRequest.SpecialHandlingItems.Select(i => i.Code));
		}

		IReadOnlyCollection<ICodeDescription> GetSpecialHandlingItems(IEnumerable<string> codes)
		{
			var specialHandlingList = new AWBSpecialHandlingCodeDescriptionPairList();
			var specialHandlingItems = new List<ICodeDescription>();

			foreach (var specialHandlingCode in codes)
			{
				var codeDescription = new CodeDescription(specialHandlingList)
				{
					Code = specialHandlingCode
				};

				specialHandlingItems.Add(codeDescription);
			}

			return specialHandlingItems;
		}

		string[] ConsolSpecialHandlingCodes => consolSpecialHandlingCodes ?? (consolSpecialHandlingCodes = GetConsolSpecialHandlingCodes().ToArray());
		string[] consolSpecialHandlingCodes;

		IEnumerable<string> GetConsolSpecialHandlingCodes()
		{
			foreach (var specialHandling in consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>())
			{
				yield return specialHandling.JKH_Code;
			}

			if (!consol.SecurityStatusCode.IsEmpty && consol.SecurityStatusCode != SecurityJobConsolAWBSpecialHandling.NotSecured)
			{
				yield return consol.SecurityStatusCode;
			}
		}

		#endregion

		#region AddValidations

		void AddValidations(AirBookingRequest bookingRequest)
		{
			AddHeaderValidation(bookingRequest);
			AddGoodsDetailsValidation(bookingRequest);
			AddFlightValidation(bookingRequest);
			AddULDDetailsValidation(bookingRequest);
			AddDimensionsValidation(bookingRequest);
			AddAdditionalInformationValidation(bookingRequest);
		}

		void AddHeaderValidation(AirBookingRequest bookingRequest)
		{
			bookingRequest.CASSInfo.AddMessageErrorIfEmpty(Res.GetString("5abd1622-ef9a-415b-8ca8-0e8c5695363c", "IATA CASS number is required"));
			bookingRequest.CASSInfo.AddMessageError(ValidateCASSNumber, Res.GetString("5e44eaf5-35e9-42bf-9bd9-691df0c41e87", "Invalid Agent IATA Code - Must contain either 7 digits, or 11 digits when a 4 digit CASS Account Code suffix is included."));

			bool ValidateCASSNumber()
			{
				var length = bookingRequest.CASS.KeepNumericCharacters().Length;
				return length != 7 && length != 11;
			}

			bookingRequest.MasterAirWaybillNumberInfo.AddMessageErrorIfEmpty(Res.GetString("499eb2a4-d7dc-4a98-936b-748aafe5b9f1", "MAWB is required"));
			bookingRequest.MasterAirWaybillNumberInfo.AddMessageError(
				() => !Regex.IsMatch(bookingRequest.MasterAirWaybillNumber, @"^[0-9]{3}-?[0-9]{8}$"),
				Res.GetString("9d8df33e-61cd-46a6-a91d-ac43ebff4c12", "The MAWB number is invalid."));

			((Unloco)bookingRequest.OriginAirport).AddIATAValidation(Res.GetString("f7bcb38c-c93d-4e81-bb2a-ef92a735bb8b", "Origin airport"));
			((Unloco)bookingRequest.DestinationAirport).AddIATAValidation(Res.GetString("e2d23d6d-948b-43f0-943e-1907824e1c1b", "Destination airport"));

			bookingRequest.AgentInfo.AddMessageErrorIfEmpty(Res.GetString("1a6ddc80-6251-4827-a47f-d6e0bd924d06", "Agent is required"));
			bookingRequest.AgentInfo.AddMaximumLengthValidation(Res.GetString("2d26267d-4db3-4729-ab23-8fa539da574b", "Agent"), 200);
			bookingRequest.AgentInfo.AddAsciiCharactersValidation();

			((CodeDescription)bookingRequest.Carrier).CodeInfo.AddMessageError(() => consol.ShippingLineAddress?.Header == null, Res.GetString("99d499f7-b755-452b-850c-582b9ae43772", "Carrier is required"));
		}

		void AddGoodsDetailsValidation(AirBookingRequest bookingRequest)
		{
			bookingRequest.TotalPiecesInfo.AddMaximumLengthValidation(Res.GetString("d418155c-2c80-4fe3-8361-12e27a23e611", "Total Pieces"), 4);
			bookingRequest.TotalPiecesInfo.AddMessageError(() => bookingRequest.TotalPieces <= 0, Res.GetString("66a0ee8e-cb6f-4341-8731-ef65b45d0b81", "Total Pieces is required."));

			((Measurement)bookingRequest.TotalWeight).ValueInfo.AddMaximumLengthValidation(Res.GetString("1471d8b6-773a-4413-85aa-68e414897b0c", "Total Weight"), 7);
			((Measurement)bookingRequest.TotalWeight).ValueInfo.AddMessageError(() => bookingRequest.TotalWeight.Value <= 0, Res.GetString("62d4fc02-bd7a-45bd-bd19-287a3345590b", "Total Weight is required."));

			((Measurement)bookingRequest.TotalVolume).ValueInfo.AddMaximumLengthValidation(Res.GetString("450200bb-e55a-4f93-bffc-d8fd5a916506", "Total Volume"), 9);

			bookingRequest.GoodsDescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("290b56d0-079d-4cea-9394-26b7e4037934", "Goods description must be provided."));
			bookingRequest.GoodsDescriptionInfo.AddAsciiCharactersValidation();

			bookingRequest.CommodityInfo.AddAsciiCharactersValidation();
			bookingRequest.CommodityInfo.AddMaximumLengthValidation(Res.GetString("f1bb0d60-ed92-4ca2-a3ab-b3e4105e5e4d", "Commodity"), 7);

			bookingRequest.CommodityInfo.AddMessageError(() => CommodityCodeIsRequired() && bookingRequest.Commodity.IsEmpty, Res.GetString("4c48a08a-5ccd-4734-b460-04610146fa3c", "Commodity Code is required."));
			bookingRequest.CommodityInfo.AddMessageError(() => !(bookingRequest.CommodityCollection?.Cast<AirlineConfigCommodityBusinessObject>().Any(cm => cm.Code.EqualsIgnoringCase(bookingRequest.Commodity)) ?? true),
					Res.GetString("77e8056e-d6a9-42c6-887b-d9786fcab68b", "Commodity should be selected from the list"));
			bookingRequest.CommodityInfo.AddWarning(() => !bookingRequest.Commodity.IsEmpty && bookingRequest.Commodity == GetDefaultCommodity(bookingRequest, bookingRequest.Product, bookingRequest.OriginAirport.Code, bookingRequest.DestinationAirport.Code),
				Res.GetString("BD02341D-3714-48D3-B69B-16612E74D281", "Please note a default Commodity Code has been applied from airline {0} reference file. This code can be overridden.", AirlinePrefix));
		}

		bool CommodityCodeIsRequired()
		{
			return (AirlineConfig?.Options?.CommodityCode ?? AirlineConfigOptionValue.NotSupported) == AirlineConfigOptionValue.Required;
		}

		void AddFlightValidation(AirBookingRequest bookingRequest)
		{
			bookingRequest.ErrorPlaceHolderInfo.AddMessageError(() => !bookingRequest.FlightDetails.Any(), Res.GetString("a4b55600-52d2-4da2-a8be-9e8fb5edab9d", "At least one Flight Detail is required."));
			foreach (var flightDetail in bookingRequest.FlightDetails)
			{
				flightDetail.FlightNumberInfo.AddMaximumLengthValidation(Res.GetString("f712d565-2e19-4c21-8b45-595202410879", "Flight Number"), 15);
				flightDetail.FlightNumberInfo.AddMessageError(
					() => !Regex.IsMatch(flightDetail.FlightNumber, @"^[a-z0-9]{2}[0-9]{1,4}[a-z]?$", RegexOptions.IgnoreCase),
					Res.GetString("ea2d339d-5a0f-4089-81e5-40ebf73f0dad", "Flight Number must be entered and have the following format MMN(N)(N)(N)(A)"));

				flightDetail.PortOfLoading.AddIATAValidation(Res.GetString("e103eb7c-63a1-407f-b66d-d99f758833ab", "Departure airport"));
				flightDetail.PortOfDischarge.AddIATAValidation(Res.GetString("3975c368-6711-4912-8cea-3d9bd75bc47f", "Arrival airport"));

				flightDetail.ETDInfo.AddMessageError(() => flightDetail.ETD.IsEmpty, Res.GetString("76d0c08d-c68c-4c6f-8063-739dd4a39459", "Estimated date of departure is mandatory"));

				flightDetail.AllotmentIdInfo.AddMaximumLengthValidation(Res.GetString("da8bc6fa-2c85-43ba-98a2-8f504665c663", "Allotment ID"), 14);
				flightDetail.AllotmentIdInfo.AddMessageError(() => !flightDetail.AllotmentId.IsEmpty && !flightDetail.AllotmentId.IsLettersAndNumbersOnlyOrEmpty, Res.GetString("c5c90bbe-6f53-486b-997d-bd8d88a489f4", "Allotment ID must be alphanumeric characters."));
			}
		}

		void AddULDDetailsValidation(AirBookingRequest bookingRequest)
		{
			foreach (var uld in bookingRequest.Ulds)
			{
				uld.Type.CodeInfo.AddMessageError(
					() => !Regex.IsMatch(uld.Type.Code, @"^[a-z][a-z0-9]{2}$", RegexOptions.IgnoreCase),
					Res.GetString("d78d90d2-1a83-453b-ab51-f4d736eda68f", "Container type must be entered and have the following format AMM."));

				uld.NumberInfo.AddMessageError(
					() => uld.ContainerVolume.Value <= 0 || !uld.Number.IsEmpty && !CommonContainerValidation.ULDRegex.IsMatch(uld.Number),
					GetUldNumberInfoError(uld));

				uld.GrossWeight.ValueInfo.AddMessageError(
					() => uld.GrossWeight.Value <= 0,
					Res.GetString("8692df2c-8bec-4fcf-b3b9-89cb5c9c0d48", "ULD gross weight is mandatory, please review the container tab and enter the gross weight."));

				uld.GrossWeight.ValueInfo.AddMaximumLengthValidation(Res.GetString("e5e839e3-fb1d-49d3-a941-4393098c2166", "Gross Weight"), 7);
				uld.TareWeight.ValueInfo.AddMaximumLengthValidation(Res.GetString("a78287ee-c7c9-4692-a8e6-e9045991a234", "Tare Weight"), 7);
				uld.GoodsWeight.ValueInfo.AddMaximumLengthValidation(Res.GetString("5594db4f-f2e9-43d0-972a-a69c3f3d8e6b", "Goods Weight"), 7);
			}

			static string GetUldNumberInfoError(ULD uld)
			{
				var err = new StringBuilder();
				if (!uld.Number.IsEmpty && !CommonContainerValidation.ULDRegex.IsMatch(uld.Number))
				{
					err.AppendLine(Res.GetString("abd4e848-f2ea-4a8b-912c-7254b4cac598", "This is not a valid ULD number, expected format should be similar to 'AKE1234QF', 'PMC12345CX'."));
				}
				if (uld.ContainerVolume.Value <= 0)
				{
					err.AppendLine(Res.GetString("0ca7a06f-1b8f-4dfd-9ba8-39f457313a00", "ULD volume is mandatory, please review the container tab and enter the actual dimensions within the Measures sub tab."));
				}

				return err.ToString().TrimEnd("\r\n".ToCharArray());
			}
		}

		void AddDimensionsValidation(AirBookingRequest bookingRequest)
		{
			foreach (var dimension in bookingRequest.Dimensions)
			{
				dimension.Weight.ValueInfo.AddMaximumLengthValidation(Res.GetString("4ee611c4-e160-4e3b-a6fe-9d0e8026925b", "Total Weight"), 7);
				dimension.QuantityInfo.AddMaximumLengthValidation(Res.GetString("404c89ad-30ed-4a1a-9875-edbb76b2f574", "Pieces"), 4);
				dimension.QuantityInfo.AddMessageErrorIfEmpty(Res.GetString("1b0f5398-be99-4aab-4ef4-2f57469e00bd", "The value for the number of loose pieces cannot be zero."));
			}
		}

		void AddAdditionalInformationValidation(AirBookingRequest bookingRequest)
		{
			var lengthLimit = IsEmiratesAirline ? 200 : 65;

			bookingRequest.SpecialInstructionsInfo.AddAsciiCharactersValidation();
			bookingRequest.SpecialInstructionsInfo.AddMaximumLengthValidation(Res.GetString("bd8ace45-4b95-4b25-90d1-b308aa9afdc0", "Special Instructions"), lengthLimit);

			bookingRequest.DangerousGoodsHandlingInformationInfo.AddAsciiCharactersValidation();
			bookingRequest.DangerousGoodsHandlingInformationInfo.AddMaximumLengthValidation(Res.GetString("0173407f-0ddf-4690-a96c-828f9f444b7d", "Dangerous Goods Handling Information"), lengthLimit);

			bookingRequest.GoodsHandlingInstructionsInfo.AddAsciiCharactersValidation();
			bookingRequest.GoodsHandlingInstructionsInfo.AddMaximumLengthValidation(Res.GetString("fedb9dc7-7c99-42c4-a665-9b5701e08d23", "Goods Handling Instructions"), 65);
		}

		#endregion

		#region CarrierBookingReference

		void PopulateCarrierBookingReference(AirBookingRequest bookingRequest)
		{
			bookingRequest.CarrierBookingReference = consol.JK_BookingReference;
		}

		#endregion

	}
}
