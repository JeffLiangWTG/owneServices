using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business.ExportAWB.ExportAWBHeader.Helpers;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.AWB;
using static Enterprise.Registry.Business.SupplyChainSecurityOrganisationToUse;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolExportAWBHeader : ExportAWBHeader
	{
		public ConsolExportAWBHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		#region Loader

		public static ConsolExportAWBHeader LoadOrCreate(ForwardingConsol consol)
		{
			return new AWBHeaderLoader<ConsolExportAWBHeader, ForwardingConsol>().LoadOrCreate(consol);
		}

		#endregion

		#region Related Objects

		public override IAWBParent Parent
		{
			get { return Consol; }
		}

		public override TypeOfAWB AWBType
		{
			get
			{
				TypeOfAWB result = TypeOfAWB.UndefinedMaster;

				if (Consol != null)
				{
					switch (Consol.JK_AgentType)
					{
						case Core.Constants.AgentType.Direct:
							result = TypeOfAWB.DirectMaster;
							break;

						case Core.Constants.AgentType.Agent:
						case Core.Constants.AgentType.AWBCoload:
							result = TypeOfAWB.AgentMaster;
							break;

						case Core.Constants.AgentType.CoLoad:
							result = TypeOfAWB.MasterHouse;
							break;
					}
				}

				return result;
			}
		}

		protected override AWBActions GetAWBActions() => new ConsolAWBActions(Consol, AWBActions.ActionsModeType.None);

		#region Consol

		public override ForwardingConsol Consol
		{
			get
			{
				if (fConsol != null && fConsol.IsDeleted)
				{
					return null;
				}

				if (fConsol == null)
				{
					fConsol = Factory.Load<ForwardingConsol>(EH_ParentID);
				}

				return fConsol;
			}
		}
		ForwardingConsol fConsol;

		protected override bool ShouldPopulate
		{
			get { return Consol != null && Consol.IsAir; }
		}

		#endregion

		#region Shipment

		protected CommonShipment DirectShipment
		{
			get
			{
				return IsDirectMAWB ? Consol.ShipmentsForTotalling.Cast<ForwardingShipment>().FirstOrDefault() : null;
			}
		}

		#endregion

		#region ULD Containers

		public override IEnumerable<CommonContainer> ULDContainers
		{
			get { return Consol != null ? Consol.Containers.Cast<CommonContainer>().Where(x => x.JC_ContainerMode == Core.Constants.ContainerModes.ULD) : Enumerable.Empty<CommonContainer>(); }
		}

		#endregion

		#region Rate Lines

		protected override void PopulateMandatoryRateLines(bool[] linesPopulated)
		{
			var undgNatureAndQuantityOfGoodsElementsList = ExcludedDangerousGoodsDetails.GetExcludedDangerousGoodDetailsElements(Consol);
			undgNatureAndQuantityOfGoodsElementsList.ForEach(
				elements =>
				{
					elements.ForEach(element =>
					{
						var dgDetailLines = ExcludedDangerousGoodsDetails.GetDetailLinesFromPackLineUNDGNatureAndQuantityOfGoodsElement(element);
						PopulateNatureAndQtyOfGoodsLineFromList(linesPopulated, dgDetailLines);
					});
				}
			);
		}

		void PopulateNatureAndQtyOfGoodsLineFromList(bool[] linesPopulated, IEnumerable<DetailLine> detailLinesToPopulate)
		{
			var rateLineNumber = Math.Max(LineNumberOfFirstEmptyRateLine, LineNumberOfFirstEmptyNatureAndQtyOfGoods);
			var dictionaries = new List<Dictionary<int, string>>();

			foreach (var line in detailLinesToPopulate)
			{
				var natureAndQtyOfGoodsLinesToPopulate = GetNatureAndQtyOfGoodsLinesToPopulate(rateLineNumber, line.Value, line.IsText, allowPartialPopulation: false);
				if (natureAndQtyOfGoodsLinesToPopulate == null)
				{
					return;
				}

				dictionaries.Add(natureAndQtyOfGoodsLinesToPopulate);
				rateLineNumber += natureAndQtyOfGoodsLinesToPopulate.Count;
			}

			foreach (var dictionary in dictionaries)
			{
				foreach (var natureAndQtyOfGoodsLinesToPopulate in dictionary)
				{
					PopulateLine(natureAndQtyOfGoodsLinesToPopulate.Key, natureAndQtyOfGoodsLinesToPopulate.Value, suspendSettingHasChanges: true);
					PopulateNatureAndQtyOfGoodsType(natureAndQtyOfGoodsLinesToPopulate.Key, NatureAndQtyOfGoodsTypes.GoodsDescription, suspendSettingHasChanges: true);
					linesPopulated[natureAndQtyOfGoodsLinesToPopulate.Key - 1] = true;
				}
			}
		}

		protected override void PopulateAdditionalRateLines(bool[] linesPopulated)
		{
			var builder = new AWBAdditionalRateLineBuilder(this, Consol?.DischargePort?.Country?.Code ?? ZString.Empty, linesPopulated);
			builder.BuildRatelines(AWBRateLines);

			if (IsMixedULDAndLoose)
			{
				var builderLoose = new AWBAdditionalLooseRatelineBuilder(this, LooseRateLineGrossWeight, RateLineWeightUnit, LooseRateLineChargeableWeightEstimate);
				builderLoose.BuildRatelines(AWBRateLines);
			}
		}

		protected override void RateLinesFinalRefinement()
		{
			base.RateLinesFinalRefinement();
			PopulateLithiumBatteryStatements();
		}

		protected override void PopulateInitialRateLines(bool[] linesPopulated)
		{
			if (IsMixedULDAndLoose)
			{
				PopulateULDMixedWithLooseRateLines(linesPopulated);
			}
			else
			{
				base.PopulateInitialRateLines(linesPopulated);
			}
		}

		void PopulateLithiumBatteryStatements()
		{
			var lithiumBatteryUNDGs = Consol?.Shipments?.OfType<ForwardingShipment>()
				.SelectMany(shipment => shipment.OuterPackLines).OfType<PackLine>()
				.SelectMany(p => p.UNDGs)
				.Where(undg => undg.DI_PackingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionII & undg.Substance != null && LithiumBatteryConstants.UNNOCodes.CodesList.Contains(undg.Substance.DG_UNNO));

			var lithiumBatteryPackTypes = lithiumBatteryUNDGs?
				.Select(GetUNDGPackingInstruction)
				.Where(s => !string.IsNullOrEmpty(s))
				.Distinct()
				.ToList();

			if (lithiumBatteryPackTypes?.Count > 0)
			{
				var rateLineNumber = Math.Max(LineNumberOfFirstEmptyRateLine, LineNumberOfFirstEmptyNatureAndQtyOfGoods);

				foreach (var packType in lithiumBatteryPackTypes)
				{
					if (AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)rateLineNumber] is ExportAWBRateLine rateLine)
					{
						using (rateLine.SuspendSettingHasChanges())
						{
							PopulateNatureAndQtyOfGoodsType(rateLineNumber, Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery);
							rateLine.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = packType;

							rateLineNumber += rateLine.NatureAndQtyOfGoodsLithiumBattery.WrappedDescriptions.Count;
						}
					}

					if (rateLineNumber > Constants.NumberOfRateLines)
					{
						break;
					}
				}

				if (lithiumBatteryPackTypes.Any(x => x == Core.Constants.AWB.LithiumBatteryTypes.Codes.PI965 || x == Core.Constants.AWB.LithiumBatteryTypes.Codes.PI968)
					&& AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)rateLineNumber] is ExportAWBRateLine rateline)
				{
					using (rateline.SuspendSettingHasChanges())
					{
						PopulateNatureAndQtyOfGoodsType(rateLineNumber, Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription);
						PopulateNatureAndQtyOfGoodsLine(rateLineNumber, Res.GetString("4bae9c2b-17d5-4e63-9d1e-759d85ff421d", "Cargo Aircraft Only"));
					}
				}
			}
		}

		string GetUNDGPackingInstruction(UNDGDataItem undgDataItem)
		{
			var packingInstruction = Consol.JK_Calc_IsCargoOnly ? undgDataItem.Substance.DG_CargoPackIns : undgDataItem.Substance.DG_PaxPackIns;

#if NETFRAMEWORK
			return LithiumBatteryConstants.RefPackingInstructionCodeDictionary.GetValueOrDefault(packingInstruction)
				?? string.Empty;
#else
			return System.Collections.Generic.CollectionExtensions.GetValueOrDefault(
				LithiumBatteryConstants.RefPackingInstructionCodeDictionary,
				packingInstruction
			) ?? string.Empty;
#endif
		}

		public override ZDecimal EH_TotalGrossWeight
		{
			get
			{
				if (ULDContainers.Any() && Math.Max(LineNumberOfFirstEmptyRateLine, LineNumberOfFirstEmptyNatureAndQtyOfGoods) >= Constants.NumberOfRateLines)
				{
					return CalculateTotalGrossWeightForULD();
				}

				return base.EH_TotalGrossWeight;
			}
		}

		ZDecimal CalculateTotalGrossWeightForULD()
		{
			ZDecimal totalContainerTareWeight = 0m;
			if (!FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.Value)
			{
				foreach (CommonContainer uldContainer in ULDContainers)
				{
					if (uldContainer.ContainerWeightUnit == RateLineGrossWeightUnit)
					{
						totalContainerTareWeight += uldContainer.JC_TareWeight;
					}
					else
					{
						totalContainerTareWeight += Core.Constants.Weight.Convert(uldContainer.JC_TareWeight, uldContainer.ContainerWeightUnit, RateLineGrossWeightUnit);
					}
				}
			}

			return totalContainerTareWeight + RateLineGrossWeight;
		}

		protected override Forwarding.AWB.Business.ExportAWBRateLineCollection GetNewAWBRateLines()
		{
			return new ConsolExportAWBRateLineCollection(this);
		}

		protected override ZString RateClass
		{
			get { return ULDContainers.Any() ? (ZString)Core.Constants.AWB.RateClass.UnitLoadDeviceBasicCharge : base.RateClass; }
		}

		#region Commodity Item Number

		protected override ZString CommodityItemNumber
		{
			get
			{
				if (Consol == null)
				{
					return ZString.Empty;
				}
				if (Consol.JK_ConsolMode == ContainerModes.ULD)
				{
					var packLineCommodities = CommodityItemNumberHelper.GetCommodityItemNumberFromULDContainerPackLines(ULDContainers);

					if (packLineCommodities != ZString.Empty)
					{
						return packLineCommodities;
					}

					var containerCommodities = CommodityItemNumberHelper.GetCommodityItemNumberFromULDContainers(ULDContainers);

					if (containerCommodities != ZString.Empty)
					{
						return containerCommodities;
					}

					return ZString.Empty;
				}
				else
				{
					return CommodityItemNumberHelper.GetCommodityItemNumberFromShipments(Consol);
				}
			}
		}

		#endregion

		protected override ZString RateLineNoPieces
		{
			get
			{
				ZInt result = ULDContainers.Any() ? (ZInt)ULDContainers.Count() : (ZInt)(Consol?.JK_TotalShipmentQuantity ?? ZDecimal.Zero);
				result = Math.Min(result, 9999);

				return result.ToString();
			}
		}

		protected override ZDecimal RateLineTotal
		{
			get
			{
				if (ShouldUseBillingSellRateForRateLineCharges())
				{
					var jobHeader = new JobHeader.Loader(Consol.DirectShipment).Load();
					var freightCharges = GetFreightCharges(jobHeader);

					return freightCharges.Sum((charge) => GetSellAmountWithCompanyCurrency(charge));
				}

				return base.RateLineTotal;
			}
		}

		ZDecimal GetSellAmountWithCompanyCurrency(JobCharge charge)
		{
			return charge.JR_RX_NKSellCurrency != "" && charge.JR_RX_NKSellCurrency == AWBCurrency
				? charge.JR_OSSellAmt
				: GetAmountBasedOnCurrencyObject(
					new JobHeader.Loader(Consol.DirectShipment).Load(),
					GlbCompany.CurrentCompany.LocalCurrency,
					new Money(charge.JR_LocalSellAmt, GlbCompany.CurrentCompany.LocalCurrency),
					charge.JR_OH_SellAccount,
					CostSell.Revenue
				);
		}

		bool ShouldUseBillingSellRateForRateLineCharges()
		{
			if (Consol == null)
			{
				return false;
			}

			var isConsolPrepaidOrCollect = Consol.JK_PrepaidCollect == PaymentType.Collect || Consol.JK_PrepaidCollect == PaymentType.Prepaid;
			var mawbSellingRateType = FreightDataRegistry.Instance.MAWBBillingSellRate.Value;

			var billingSellRateMatchesConsolPaymentType = (mawbSellingRateType == MAWBBillingSellRateModes.Both && isConsolPrepaidOrCollect)
				|| (mawbSellingRateType == MAWBBillingSellRateModes.CollectOnly && Consol.JK_PrepaidCollect == PaymentType.Collect)
				|| (mawbSellingRateType == MAWBBillingSellRateModes.PrepaidOnly && Consol.JK_PrepaidCollect == PaymentType.Prepaid);

			return Consol.IsDirect && Consol.DirectShipment != null && billingSellRateMatchesConsolPaymentType;
		}

		protected override ZDecimal RateLineRateChargeOrDiscount
		{
			get { return (Consol != null && !ShouldUseBillingSellRateForRateLineCharges()) ? Consol.JK_ConsolChargeableRate : (ZDecimal)0; }
		}

		#region Volume and Dimensions

		protected override string VolumeAndDimensionsPrintOption
		{
			get { return Consol?.JK_PrintOptionForPackagesOnAWB; }
		}

		protected override StringRegistryItem ExtraNatureAndQtyOfGoods
		{
			get { return FreightDataRegistry.Instance.MAWBNatureAndQtyOfGoodsExtraText; }
		}

		protected override void SetALLNatureAndQtyOfGoods()
		{
			StringCollectionX dimensionLines = GetAvailableDimensions();
			ZDecimal volume = Consol?.GetTotalShipmentVolumeForDoc(Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay) ?? ZDecimal.Zero;

			if (volume > 0M && dimensionLines.Count > 0 && LineNumberOfFirstEmptyNatureAndQtyOfGoods != Constants.NumberNatureAndDescriptionLines)
			{
				foreach (string dimensionLine in dimensionLines)
				{
					PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, dimensionLine);
				}

				PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, Constants.VOL + " " + volume.ToString(3) + " "
					+ (Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Actual ? Consol.JK_CorrectedConsolVolumeUnit : Consol.JK_TotalShipmentVolumeUnit));
			}
			else
			{
				SetDEFNatureAndQtyOfGoods();
			}
		}

		protected override void SetVOLNatureAndQtyOfGoods()
		{
			ZDecimal volume = Consol?.GetTotalShipmentVolumeForDoc(Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay) ?? ZDecimal.Zero;

			if (volume > 0M)
			{
				PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, Constants.VOL + " " + volume.ToString(3) + " "
					+ (Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Actual ? Consol.JK_CorrectedConsolVolumeUnit : Consol.JK_TotalShipmentVolumeUnit));
			}
			else
			{
				PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, Constants.NoDimensionsAvailable, true);
			}
		}

		protected override void SetPKSNatureAndQtyOfGoods()
		{
			StringCollectionX dimensionLines = GetAvailableDimensions();

			if (dimensionLines.Count > 0)
			{
				foreach (string dimensionLine in dimensionLines)
				{
					PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, dimensionLine);
				}
			}
			else
			{
				PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, Constants.NoDimensionsAvailable, true);
			}
		}

		protected override void SetDEFNatureAndQtyOfGoods()
		{
			StringCollectionX dimensionLines = GetAvailableDimensions();
			ZDecimal volume = Consol?.GetTotalShipmentVolumeForDoc(Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay) ?? ZDecimal.Zero;

			if (dimensionLines.Count > 0 && WillFitInFreeSpace(dimensionLines.Count))
			{
				foreach (string dimensionLine in dimensionLines)
				{
					PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, dimensionLine);
				}
			}
			else if (volume > 0M)
			{
				PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, Constants.VOL + " " + volume.ToString(3) + " "
					+ (Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Actual ? Consol.JK_CorrectedConsolVolumeUnit : Consol.JK_TotalShipmentVolumeUnit));
			}
			else
			{
				PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, Constants.NoDimensionsAvailable, true);
			}
		}

		protected override void SetNDANatureAndQtyOfGoods()
		{
			PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, Constants.NoDimensionsAvailable, true);
		}

		StringCollectionX GetAvailableDimensions()
		{
			var result = new StringCollectionX();

			if (Consol != null)
			{
				foreach (CommonShipment shipment in Consol.ShipmentsForTotalling.ToArray())
				{
					foreach (PackLine packLine in shipment.OuterPackLines.ToArray())
					{
						if (packLine.JL_Width != 0 && packLine.JL_Length != 0 && packLine.JL_Height != 0 && packLine.JL_PackageCount != 0)
						{
							result.Add(GetDimensionText(packLine));
						}
					}
				}
			}

			return result;
		}

		#endregion

#endregion

		#region Mixed ULD and Loose Rate Lines

		bool IsMixedULDAndLoose
		{
			get => IsULD && ULDContainers.Any() && Consol.UnAllocatedPackLines.Any();
		}

		void PopulateULDMixedWithLooseRateLines(bool[] linesPopulated)
		{
			var rateLine = AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)1];
			using (rateLine.SuspendSettingHasChanges())
			{
				rateLine.Clear();
				rateLine.ER_NoOfPiecesOrRCP = RateLineNoPieces;
				rateLine.ER_RateClass = RateClass;
				rateLine.ER_CommodityItemNumber = CommodityItemNumber;
				rateLine.ER_WeightInLBsOrKGs = RateLineWeightUnit;
				rateLine.ER_GrossWeight = RateLineGrossWeight - LooseRateLineGrossWeight;
				rateLine.ER_ChargeableWeight = RateLineChargeableWeight - LooseRateLineChargeableWeightEstimate;
			}

			linesPopulated[0] = true;
		}

		IEnumerable<PackLine> LoosePackLines
		{
			get => Consol.UnAllocatedPackLines.Cast<PackLine>();
		}

		ZDecimal LooseRateLineGrossWeight
		{
			get => TotalCalculation.GetTotalWeight(LoosePackLines, PackLine.Schema.JL_ActualWeight, PackLine.Schema.JL_ActualWeightUQ, Consol.JK_TotalShipmentWeightUnit);
		}

		ZDecimal LooseRateLineChargeableWeightEstimate
		{
			get
			{
				var shipmentPackageCounts = Consol.Shipments
					.Cast<CommonShipment>()
					.ToDictionary
					(shipment => shipment,
					shipment => shipment.OuterPackLines.Cast<PackLine>().Sum(packLine => packLine.JL_PackageCount));

				return LoosePackLines.Sum(packLine =>
				{
					var shipmentPackageCount = shipmentPackageCounts[packLine.Shipment];
					if (shipmentPackageCount > 0)
					{
						return ((ZDecimal)packLine.JL_PackageCount / shipmentPackageCount) * packLine.Shipment.JS_ActualChargeable;
					}
					else
					{
						return 0;
					}
				});
			}
		}

		#endregion

		#region Accounting Information

		protected override Forwarding.AWB.Business.ExportAWBAccountingInformationCollection GetNewAWBAccountingInformations()
		{
			return new ConsolExportAWBAccountingInformationCollection(this, Factory);
		}

		protected override CodeDescriptionPairListRegistryItem ExtraAccountingInfo
		{
			get { return FreightDataRegistry.Instance.MAWBAccountingInfoExtraText; }
		}

		#endregion

		#region Other Charges

		protected override IEnumerable<OtherChargeTemplate> GetOtherChargeTemplates()
		{
			var otherChargeTemplates = new List<OtherChargeTemplate>();

			if (Consol == null)
			{
				return otherChargeTemplates;
			}

			var chargesAndCosts = GetChargesAndCosts(Consol);

			foreach (var chargeOrCost in chargesAndCosts)
			{
				var chargeCode = Factory.Load<AccChargeCode>(chargeOrCost.ChargeCodePk);
				var iataChargeCodeOverride = chargeCode == null ? ZString.Empty
											: (Consol?.ShippingLine != null ? chargeCode.GetIATACodeWithFallback(Consol.ShippingLine.PK) : chargeCode.AC_IATA_ChargeCodeMap);
				if (!iataChargeCodeOverride.IsEmpty)
				{
					var awbCurrency = AWBCurrency;
					var template = new OtherChargeTemplate(this);

					template.PrepaidCollect = GetPrepaidCollectFromConsol();
					template.AccChargeCode = chargeCode;

					decimal localToAWBExRate = Consol.GetExchangeRateFromFreightCostsOrSchedule(awbCurrency);
					decimal localToOSExRate = Consol.GetExchangeRateFromFreightCostsOrSchedule(chargeOrCost.CurrencyCode);

					template.ChargeAmountProvider = () => chargeOrCost.GetChargeAmountFromCost(localToAWBExRate, awbCurrency);
					template.CostAmountProvider = () => template.ChargeAmount;
					template.IATAChargeCodeProvider = () => iataChargeCodeOverride;
					template.TaxAmount = chargeOrCost.GetTaxAmountFromCost(localToAWBExRate, localToOSExRate, awbCurrency);
					template.ProfitAmountProvider = () => chargeOrCost.GetProfitAmount(awbCurrency, localToAWBExRate, template.CostAmount);
					otherChargeTemplates.Add(template);
				}
			}

			return otherChargeTemplates;
		}

		/// <summary>
		/// A JobConsolCost or a JobCharge business object.
		/// </summary>
		abstract class CostBizo
		{
			protected readonly BusinessObject bizo;

			protected CostBizo(BusinessObject bizo)
			{
				Argument.NotNull(bizo, nameof(bizo));
				this.bizo = bizo;
			}

			internal abstract ZGuid ChargeCodePk { get; }
			internal abstract ZString CurrencyCode { get; }
			internal abstract ZDecimal LocalCostAmount { get; }
			internal abstract ZDecimal OSCostTaxAmount { get; }
			internal abstract ZDecimal OSCostTaxAmountCalc { get; }

			internal ZDecimal GetChargeAmountFromCost(decimal awbExchangeRate, string awbCurrency)
			{
				return awbExchangeRate == 1m
					? LocalCostAmount
					: (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(LocalCostAmount, awbExchangeRate, awbCurrency);
			}

			internal ZDecimal GetTaxAmountFromCost(decimal awbExchangeRate, decimal osExchangeRate, string awbCurrency)
			{
				var taxAmount = OSCostTaxAmountCalc;
				decimal overallExchangeRate = osExchangeRate != 0 ? (1 / osExchangeRate) * awbExchangeRate : 1;
				return overallExchangeRate == 1m
					? taxAmount
					: (ZDecimal)Env.CurrentCompany.ExchangeRate.ForeignToForeign(taxAmount, osExchangeRate, awbExchangeRate, awbCurrency);
			}

			internal abstract ZDecimal GetProfitAmount(string awbCurrency, decimal awbExchangeRate, decimal costAmount);
		}

		class ConsolCostBizo : CostBizo
		{
			public ConsolCostBizo(IJobConsolCost cost)
				: base((BusinessObject)cost)
			{
				consolCost = cost;
			}

			readonly IJobConsolCost consolCost;

			internal override ZGuid ChargeCodePk => (ZGuid)bizo[JobConsolCostSchema.E6_AC_ChargeCode];
			internal override ZString CurrencyCode => (ZString)bizo[JobConsolCostSchema.E6_RX_NKCurrency];
			internal override ZDecimal LocalCostAmount => (ZDecimal)bizo[JobConsolCostSchema.E6_LocalCostAmount];
			internal override ZDecimal OSCostTaxAmount => (ZDecimal)bizo[JobConsolCostSchema.E6_OSGSTAmount];
			internal override ZDecimal OSCostTaxAmountCalc => consolCost.E6_OSGSTAmount_Calc;

			internal override ZDecimal GetProfitAmount(string awbCurrency, decimal awbExchangeRate, decimal costAmount)
			{
				ZDecimal result = -costAmount;

				foreach (var charge in GetApportionedCharges())
				{
					ZDecimal sell = charge.JR_RX_NKSellCurrency != "" && charge.JR_RX_NKSellCurrency == awbCurrency ? charge.JR_OSSellAmt :
						awbExchangeRate == 1m ? charge.JR_LocalSellAmt : (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(charge.JR_LocalSellAmt, awbExchangeRate, awbCurrency);

					result += sell;
				}

				return result;
			}

			JobCharge[] GetApportionedCharges()
			{
				var query = new ZQuery(JobChargeSchema.JR_E6, (ZGuid)bizo[JobConsolCostSchema.PK]);
				var profitShareChargeCode = ObjectFactory.Get<IAccounting>().ProfitShareChargeCode;

				if (!profitShareChargeCode.IsEmpty)
				{
					query.AddToFilter(JobChargeSchema.JR_AC, SQLComparisonOperator.NotEqual, profitShareChargeCode);
				}

				return bizo.Factory.Load<JobCharge>(query);
			}
		}

		class ChargeBizo : CostBizo
		{
			readonly JobCharge charge;

			public ChargeBizo(JobCharge charge)
				: base(charge)
			{
				Argument.NotNull(charge, nameof(charge));
				this.charge = charge;
			}

			internal override ZGuid ChargeCodePk => (ZGuid)bizo[JobChargeSchema.JR_AC];
			internal override ZString CurrencyCode => (ZString)bizo[JobChargeSchema.JR_RX_NKCostCurrency];
			internal override ZDecimal LocalCostAmount => (ZDecimal)bizo[JobChargeSchema.JR_LocalCostAmt];
			internal override ZDecimal OSCostTaxAmount => (ZDecimal)bizo[JobChargeSchema.JR_OSCostGSTAmt];
			internal override ZDecimal OSCostTaxAmountCalc => charge.JR_OSCostGSTAmt_Calc;
			internal override ZDecimal GetProfitAmount(string awbCurrency, decimal awbExchangeRate, decimal costAmount) => 0;
		}

		IList<CostBizo> GetChargesAndCosts(ForwardingConsol consol)
		{
			var result = new List<CostBizo>();
			if (Consol.IsGateway())
			{
				var job = consol.Job;
				if (job != null)
				{
					var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
					foreach (var charge in charges)
					{
						result.Add(new ChargeBizo(charge));
					}
				}
			}

			var costQuery = new ZQuery(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
			costQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, Consol.PK);
			costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, JobConsolSchema.Constants.Prefix);
			costQuery.FetchOnlyFromLocalCache = !Consol.IsInDatabase;
			var costs = Factory.Load<IJobConsolCost>(costQuery);
			foreach (var cost in costs)
			{
				result.Add(new ConsolCostBizo(cost));
			}
			return result;
		}

		ZString GetPrepaidCollectFromConsol()
		{
			ZString result = ZString.Empty;

			if (Consol.JK_PrepaidCollect == Core.Constants.PaymentType.Prepaid)
			{
				result = Constants.PrepaidCollect3CharCodes.Prepaid;
			}
			else if (Consol.JK_PrepaidCollect == Core.Constants.PaymentType.Collect)
			{
				result = Constants.PrepaidCollect3CharCodes.Collect;
			}

			return result;
		}

		protected override bool GroupChargesByIATACode
		{
			get { return ExportAWBRegistry.Instance.MAWBGroupOtherChargesByIATACode.Value; }
		}

		protected override Forwarding.AWB.Business.ExportAWBOtherChargesCollection GetNewAWBOtherCharges()
		{
			return new ConsolExportAWBOtherChargesCollection(this);
		}

		#endregion

		#region Freight Charges

		protected override FreightTaxes CalculateTotalsForTaxFromFreightCharges()
		{
			var freightChargeCodePk = Env.Registry.FreightChargeCode;
			var freightCharges = GetChargesAndCosts(Consol)
				.Where(x => x.ChargeCodePk == freightChargeCodePk);
			decimal prepaidTax = 0;
			decimal collecTax = 0;
			if (freightCharges.Any())
			{
				decimal freightTaxAmount = 0;
				var prepaidCollect = GetPrepaidCollectFromConsol();
				var awbCurrency = AWBCurrency;
				var localToAWBExRate = Consol.GetExchangeRateFromFreightCostsOrSchedule(awbCurrency);

				foreach (var chargeOrCost in freightCharges)
				{
					var localToOSExRate = Consol.GetExchangeRateFromFreightCostsOrSchedule(chargeOrCost.CurrencyCode);

					freightTaxAmount += chargeOrCost.GetTaxAmountFromCost(localToAWBExRate, localToOSExRate, awbCurrency);
				}

				prepaidTax = prepaidCollect == Constants.PrepaidCollect3CharCodes.Prepaid ? freightTaxAmount : 0;
				collecTax = prepaidCollect == Constants.PrepaidCollect3CharCodes.Collect ? freightTaxAmount : 0;
			}

			return new FreightTaxes(prepaidTax, collecTax);
		}

		#endregion

		#region Security Declaration

		protected override void PopulateSecurityDeclaration()
		{
			var consol = Consol;

			if (consol == null)
			{
				return;
			}

			if (!consol.JK_OverrideSecurityDeclarationDefaults)
			{
				PopulatePartySecurityStatus(consol);
				EH_AdditionalSecurityInformation = base.AdditionalSecurityInformation;
				EH_ScheduledArrivalDate = ZDateTimeOffset.Empty;
				EH_AdditionalSecurityInformationStatement = ZString.Empty;
				EH_AdditionalScreeningMethods = ZString.Empty;
			}

			ClearSecurityStatusLinesCachedValues();
			PopulateSecurityStatusLines(consol);
			Validation.ValidateAll();
		}

		void ClearSecurityStatusLinesCachedValues()
		{
			foreach (ExportAWBSecurityStatusLine securityStatusLines in ExportAWBSecurityStatusLines)
			{
				securityStatusLines.Shipments.Clear();
				securityStatusLines.Organization = null;
			}
		}

		void PopulatePartySecurityStatus(ForwardingConsol consol)
		{
			var agentApproval = SupplyChainSecurityConfiguration.GetAgentApproval(consol);
			if (agentApproval == null)
			{
				EH_AgentApprovalCategory = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				EH_RN_NKAgentApprovalCountryCode = GlbCompany.CurrentCompany.Country.Code;
				EH_AgentApprovalNumber = string.Empty;
			}
			else
			{
				PopulatePartySecurityStatus(agentApproval);
			}

			EH_SecurityStatusIssueDate = !EH_SecurityStatusIssueDate.IsEmpty ? EH_SecurityStatusIssueDate : EH_AWBIssueDate;
			EH_SecurityStatusIssuedBy = GlbStaff.CurrentUser.GS_FullName.Left(EH_SecurityStatusIssuedByInfo.MaxLength);
			EH_GS_NKSecurityStatusIssuedByCode = GlbStaff.CurrentUser.GS_Code;
		}

		void PopulatePartySecurityStatus(OrgCountryData data)
		{
			EH_AgentApprovalCategory = Lookups.AgentApprovalCategoryList.ContainsCode(data.OV_EXApprovedOrMajorExporter)
				? data.OV_EXApprovedOrMajorExporter
				: (ZString)AviationSecuritySchemeMembership.Codes.RegulatedAgent;

			EH_AgentApprovalExpiryDate = data.OV_EXApprovalExpiryDate;

			if (SupplyChainSecurityConfiguration.ApprovalCodeNumberIsHiddenOnSecurityDeclaration(data.OV_EXApprovedOrMajorExporter))
			{
				EH_AgentApprovalNumber = ZString.Empty;
			}
			else if (data.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent
				&& data.OV_EXApprovalNumber.StartsWith(AviationSecuritySchemeMembership.Codes.RegulatedAgent, StringComparison.Ordinal))
			{
				EH_AgentApprovalNumber = SupplyChainSecurityConfiguration.ShowPrefixOnAgentApprovalNumber
					? data.OV_EXApprovalNumber
					: data.OV_EXApprovalNumber.Substring(AviationSecuritySchemeMembership.Codes.RegulatedAgent.Length);
			}
			else
			{
				EH_AgentApprovalNumber = data.OV_EXApprovalNumber;
			}

			EH_RN_NKAgentApprovalCountryCode = data.OV_RN_NKIssuingAuthorityCountry;
		}

		void PopulateSecurityStatusLines(ForwardingConsol consol)
		{
			var shipmentsCollection = consol.IsDirect
				? consol.ShipmentsForTotalling.Cast<ForwardingShipment>()
				: consol.Shipments.Cast<ForwardingShipment>();

			var shipments = shipmentsCollection
				.Where(ForwardingShipmentExtensions.IsFHLShipment)
				.ToArray();

			using (ExportAWBSecurityStatusLines.SuspendSettingHasChanges())
			{
				if (SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(Consol) ||
					SupplyChainSecurityConfiguration.IsTranshipment(consol))
				{
					PopulateExportAWBSecurityStatusLines(SecurityStatusLineType.KnownConsignor,
						ExtractPermitDetails(shipments),
						FindMatchingPermit,
						UpdateSecurityStatusLineFromPermit,
						!consol.JK_OverrideSecurityDeclarationDefaults);
				}

				PopulateExportAWBSecurityStatusLines(SecurityStatusLineType.ScreeningMethod,
					ExtractScreeningMethods(shipments),
					(line, data) => data.FirstOrDefault(d => d == line.EAS_ScreeningMethod),
					(value, line) => line.EAS_ScreeningMethod = value,
					true);

				PopulateExportAWBSecurityStatusLines(SecurityStatusLineType.ExceptionCode,
					ExtractExemptionCodes(shipments),
					(line, data) => data.FirstOrDefault(d => d == line.EAS_ExemptionGround),
					(value, line) => line.EAS_ExemptionGround = value,
					true);

				if (!HasOtherScreeningMethod)
				{
					EH_AdditionalScreeningMethods = ZString.Empty;
				}
			}
		}

		PermitDetails FindMatchingPermit(ExportAWBSecurityStatusLine line, IEnumerable<PermitDetails> permits)
		{
			var matchingPermit = permits.FirstOrDefault(permit =>
					permit.Type == line.EAS_ApprovalCategory
					&& permit.ApprovalCountry == line.EAS_RN_NKCountryCode
					&& permit.Number == line.EAS_ApprovalNumber
					&& permit.ExpiryDate == line.EAS_ApprovalExpiryDate);

			if (matchingPermit != null)
			{
				line.Organization = matchingPermit.Organization;

				line.Shipments.Clear();
				matchingPermit.Shipments.ForEach(shipment => line.Shipments.Add(shipment));
			}

			return matchingPermit;
		}

		void UpdateSecurityStatusLineFromPermit(PermitDetails permit, ExportAWBSecurityStatusLine line)
		{
			line.EAS_ApprovalCategory = permit.Type;
			line.EAS_ApprovalNumber = permit.Number;
			line.EAS_RN_NKCountryCode = permit.ApprovalCountry;
			line.EAS_ApprovalExpiryDate = permit.ExpiryDate;
			line.Organization = permit.Organization;

			line.Shipments.Clear();
			permit.Shipments.ForEach(shipment => line.Shipments.Add(shipment));
		}

		void PopulateExportAWBSecurityStatusLines<T>(SecurityStatusLineType type,
			IEnumerable<T> data,
			Func<ExportAWBSecurityStatusLine, IEnumerable<T>, T> matcher,
			Action<T, ExportAWBSecurityStatusLine> updater,
			bool allowChangeExistingCollection)
		{
			var toPopulate = new List<T>(data);

			var availableLines = ExportAWBSecurityStatusLines
				.Cast<ExportAWBSecurityStatusLine>()
				.Where(line => !line.IsDeleted && line.Type == type)
				.ToArray();

			var comparer = EqualityComparer<T>.Default;

			var unmatched = new List<ExportAWBSecurityStatusLine>();

			foreach (var line in availableLines)
			{
				var t = matcher(line, toPopulate);

				if (comparer.Equals(t, default(T)))
				{
					unmatched.Add(line);
				}
				else
				{
					toPopulate.Remove(t);
				}
			}

			if (!allowChangeExistingCollection)
			{
				return;
			}

			foreach (var value in toPopulate)
			{
				var line = unmatched.FirstOrDefault();

				if (line == null)
				{
					line = ExportAWBSecurityStatusLines.AddNew();
				}
				else
				{
					unmatched.Remove(line);
				}

				using (line.SuspendSettingHasChanges())
				{
					updater(value, line);
				}
			}

			foreach (var line in unmatched.ToArray())
			{
				line.Delete();
			}
		}

		internal IEnumerable<PermitDetails> ExtractPermitDetails(IEnumerable<ForwardingShipment> shipments)
		{
			var permitDetails = new Dictionary<PermitKey, PermitDetails>();

			if (!SupplyChainSecurityConfiguration.ShouldDefaultPermitToSecurityDeclaration)
			{
				return permitDetails.Values;
			}

			var approvalCategoryList = new AviationSecuritySchemeMembership();
			foreach (var shipment in shipments)
			{
				if (shipment.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved)
				{
					continue;
				}

				var permitAddress = GetPermitAddress(shipment);
				PermitKey key;

				var consignorSecurity = GetApproval(permitAddress);
				if (consignorSecurity != null)
				{
					ZString number;
					if (consignorSecurity.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent
						&& consignorSecurity.OV_EXApprovalNumber.StartsWith(AviationSecuritySchemeMembership.Codes.RegulatedAgent, StringComparison.Ordinal))
					{
						number = consignorSecurity.OV_EXApprovalNumber.Substring(AviationSecuritySchemeMembership.Codes.RegulatedAgent.Length);
					}
					else
					{
						number = consignorSecurity.OV_EXApprovalNumber;
					}

					key = new PermitKey(approvalCategoryList.ContainsCode(consignorSecurity.OV_EXApprovedOrMajorExporter)
							? consignorSecurity.OV_EXApprovedOrMajorExporter
							: (ZString)AviationSecuritySchemeMembership.Codes.KnownConsignor,
							number,
							consignorSecurity.OV_RN_NKIssuingAuthorityCountry,
							consignorSecurity.OV_EXApprovalExpiryDate,
							permitAddress != null ? permitAddress.Header.PK : ZGuid.Empty);
				}
				else
				{
					key = new PermitKey(AviationSecuritySchemeMembership.Codes.KnownConsignor,
						ZString.Empty,
						ZString.Empty,
						ZDate.Empty,
						permitAddress != null ? permitAddress.Header.PK : ZGuid.Empty);
				}

				if (permitDetails.ContainsKey(key))
				{
					permitDetails[key].Shipments.Add(shipment);
				}
				else
				{
					permitDetails.Add(key,
						new PermitDetails
						{
							Type = key.Type,
							Number = key.Number,
							ApprovalCountry = key.ApprovalCountry,
							ExpiryDate = key.ExpiryDate,
							Shipments = { shipment },
							Organization = permitAddress != null ? permitAddress.Header : null
						});
				}
			}

			return permitDetails.Values;
		}

		OrgAddress GetPermitAddress(ForwardingShipment shipment)
		{
			if (SupplyChainSecurityConfiguration.OrganisationsToUse.ContainsKey(SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom)
				&& shipment.ConsignorPickupAddress.HasRealAddress)
			{
				var pickupAddressOrgToUse = SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom];
				if (pickupAddressOrgToUse.ValidationCode == ValidationCodes.Yes || pickupAddressOrgToUse.ValidationCode == ValidationCodes.Warning)
				{
					var approval = shipment.ConsignorPickupAddress.Address.KnownShipper;
					if (approval != null
						&& SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(approval.OV_EXApprovedOrMajorExporter)
						&& (approval.OV_EXApprovalExpiryDate.IsEmpty || approval.OV_EXApprovalExpiryDate >= ZDate.Today))
					{
						return shipment.ConsignorPickupAddress.Address;
					}
				}
			}

			if (shipment.ConsignorDocumentaryAddress.HasRealAddress)
			{
				return shipment.ConsignorDocumentaryAddress.Address;
			}

			return null;
		}

		OrgCountryData GetApproval(OrgAddress address)
		{
			if (address == null)
			{
				return null;
			}

			if (SupplyChainSecurityConfiguration.IsAddressLevelScheme)
			{
				OrgCountryData knownShipperRecord = null;

				var countryCode = SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode;

				var relevantOrgHeader = address.Header;

				var orgAddressKnownShipperDetails = GetAddressKnownShipperDetails(address, countryCode);
				if (orgAddressKnownShipperDetails.Length == 1)
				{
					knownShipperRecord = orgAddressKnownShipperDetails[0];
				}
				else if (relevantOrgHeader != null)
				{
					knownShipperRecord = relevantOrgHeader.Addresses
						.Cast<OrgAddress>()
						.Where(x => x.PK != address.PK)
						.Select(x => GetAddressKnownShipperDetails(x, countryCode))
						.FirstOrDefault(x => x.Length == 1 && SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval(x[0].OV_EXApprovedOrMajorExporter))
						?.FirstOrDefault();
				}

				return knownShipperRecord;
			}
			else
			{
				return address.Header?.CountryDataCollectionForThisCompany.Cast<OrgCountryData>().FirstOrDefault();
			}
		}

		OrgCountryData[] GetAddressKnownShipperDetails(OrgAddress address, ZString country)
		{
			if (address == null)
			{
				return Array.Empty<OrgCountryData>();
			}

			var query = new ZQuery(OrgCountryDataSchema.OV_OA_ApprovedLocation, address.PK);
			query.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, country);
			return address.Factory.Load<OrgCountryData>(query);
		}

		CodeDescriptionPairList ExemptionCodesList
		{
			get { return ShipmentInspectionTypeLists.GetCountrySpecificExemptionCodes(GlbCompany.CurrentCompany.Country.Code); }
		}

		IEnumerable<ZString> ExtractScreeningMethods(IEnumerable<ForwardingShipment> shipments)
		{
			var result = new HashSet<ZString>();
			var screeningMethodList = new CodeDescriptionPairList();

			SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection
				.Cast<ShipmentInspectionType>()
				.Where(item => !ExemptionCodesList.ContainsCode(item.Code))
				.ForEach(item => screeningMethodList.AddPair(item.Code, item.Description));

			if (SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(Consol))
			{
				foreach (var inspectionCode in shipments.SelectMany(s => s.PackLineInspectionList)
					.Concat(shipments.SelectMany(s => s.PackLineAdditionalInspectionList))
					.Where(i => i == FreightDataRegistry.AviationSecurity_Unknown_Code || screeningMethodList.ContainsCode(i)))
				{
					result.Add(inspectionCode);
				}

				foreach (var shipment in shipments
						.Where(s => s.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Screened
						&& (s.JS_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code || screeningMethodList.ContainsCode(s.JS_InspectionTypeCode))))
				{
					result.Add(shipment.JS_InspectionTypeCode);
				}
			}
			else
			{
				foreach (var shipment in shipments)
				{
					if (shipment.JS_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code
						|| screeningMethodList.ContainsCode(shipment.JS_InspectionTypeCode))
					{
						result.Add(shipment.JS_InspectionTypeCode);
					}
				}
			}

			foreach (var shipment in shipments)
			{
				if (shipment.AviationSecurity.IsHighRiskShipment
					&& (shipment.JS_AdditionalInspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code || screeningMethodList.ContainsCode(shipment.JS_AdditionalInspectionTypeCode)))
				{
					result.Add(shipment.JS_AdditionalInspectionTypeCode);
				}
			}

			return result;
		}

		IEnumerable<ZString> ExtractExemptionCodes(IEnumerable<ForwardingShipment> shipments)
		{
			var result = new HashSet<ZString>();

			if (SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(Consol))
			{
				foreach (var inspectionCode in shipments.SelectMany(s => s.PackLineInspectionList)
					.Concat(shipments.SelectMany(s => s.PackLineAdditionalInspectionList))
					.Where(i => ExemptionCodesList.ContainsCode(i)))
				{
					result.Add(ShipmentInspectionType.GetIATAExemptionCode(inspectionCode) ?? inspectionCode);
				}

				foreach (var shipment in shipments
						.Where(s => ExemptionCodesList.ContainsCode(s.JS_InspectionTypeCode)))
				{
					result.Add(ShipmentInspectionType.GetIATAExemptionCode(shipment.JS_InspectionTypeCode) ?? shipment.JS_InspectionTypeCode);
				}
			}
			else
			{
				foreach (var shipment in shipments)
				{
					var isOldShipmentWithEXM = (shipment.JS_SystemCreateTimeUtc < FreightDataRegistry.Instance.EXMExemptionCodeRemovalDate.Value && shipment.JS_InspectionTypeCode == "EXM");

					if (ExemptionCodesList.ContainsCode(shipment.JS_InspectionTypeCode) || isOldShipmentWithEXM)
					{
						result.Add(ShipmentInspectionType.GetIATAExemptionCode(shipment.JS_InspectionTypeCode) ?? shipment.JS_InspectionTypeCode);
					}
				}
			}

			foreach (var shipment in shipments)
			{
				if (shipment.AviationSecurity.IsHighRiskShipment && ExemptionCodesList.ContainsCode(shipment.JS_AdditionalInspectionTypeCode))
				{
					result.Add(ShipmentInspectionType.GetIATAExemptionCode(shipment.JS_AdditionalInspectionTypeCode) ?? shipment.JS_AdditionalInspectionTypeCode);
				}
			}

			return result;
		}

		internal struct PermitKey
		{
			public PermitKey(ZString type, ZString number, ZString approvalCountry, ZDateTime expiryDate, ZGuid orgPK)
			{
				this.Type = type;
				this.Number = number;
				this.ApprovalCountry = approvalCountry;
				this.ExpiryDate = expiryDate;
				this.OrgPK = orgPK;
			}

			public ZString Type { get; }

			public ZString Number { get; }
			public ZString ApprovalCountry { get; }

			public ZDateTime ExpiryDate { get; }

			public ZGuid OrgPK { get; }

			public override bool Equals(object obj)
			{
				return Equals((PermitKey)obj);
			}

			public bool Equals(PermitKey other)
			{
				return Type == other.Type && Number == other.Number && ApprovalCountry == other.ApprovalCountry && ExpiryDate == other.ExpiryDate && OrgPK == other.OrgPK;
			}

			public override int GetHashCode()
			{
				return new Tuple<ZString, ZString, ZString, ZDateTime, ZGuid>(Type, Number, ApprovalCountry, ExpiryDate, OrgPK).GetHashCode();
			}
		}

		internal sealed class PermitDetails
		{
			public ZString Type { get; set; }

			public ZString Number { get; set; }

			public ZString ApprovalCountry { get; set; }

			public ZDateTime ExpiryDate { get; set; }

			public OrgHeader Organization { get; set; }

			public ICollection<ForwardingShipment> Shipments
			{
				get
				{
					if (shipments == null)
					{
						shipments = new List<ForwardingShipment>();
					}

					return shipments;
				}
			}

			List<ForwardingShipment> shipments;
		}

		#endregion

#endregion

		#region PopulateCore

		protected override void PopulateCore(bool populateAll)
		{
			ConsolAWBFetchHelper.AddFetchHintsForConsol(Consol);

			base.PopulateCore(populateAll);
		}

		#endregion

		#region Special Handling Items

		protected override void PopulateSpecialHandlingItems()
		{
			if (IsAWBOverridden)
			{
				return;
			}

			var defaultCodes = new List<string>();

			if (Consol != null)
			{
				Consol.RefreshSecurityStatusCode();
				var securityCode = Consol.SecurityStatusCode;
				if (!securityCode.IsEmpty && securityCode != SecurityJobConsolAWBSpecialHandling.NotSecured)
				{
					defaultCodes.Add(securityCode);
				}

				foreach (var specialHandlingItem in Consol.AWBSpecialHandlingItems.Cast<JobConsolAWBSpecialHandling>())
				{
					defaultCodes.Add(specialHandlingItem.JKH_Code);
				}
			}

			using (AWBSpecialHandlingItems.SuspendSettingHasChanges())
			{
				if (IsExportingData)
				{
					for (var index = AWBSpecialHandlingItems.Count - 1; index >= 0; index--)
					{
						var specialHandlingItem = AWBSpecialHandlingItems[index];
						using (specialHandlingItem.SuspendSettingHasChanges())
						{
							AWBSpecialHandlingItems.RemoveAndDelete(specialHandlingItem);
						}
					}
				}
				else
				{
					AWBSpecialHandlingItems.DeleteAll();
				}
				foreach (var defaultCode in defaultCodes)
				{
					var specialHandlingItem = AWBSpecialHandlingItems.AddNew();

					using (specialHandlingItem.SuspendSettingHasChanges())
					{
						specialHandlingItem.EP_SpecialHandling = defaultCode;
					}
				}
			}
		}

		#endregion

		#region Goods Declaration Reference Number

		public override ZString ShipmentNumberWithEmptyHSCode => Consol.Shipments.Cast<ForwardingShipment>().FirstOrDefault(x => x.IsAnyPacklineHSCodeEmpty)?.JS_UniqueConsignRef ?? ZString.Empty;

		protected override IEnumerable<GoodsDeclarationReferenceNumber> GetGoodsDeclarationReferenceNumbers()
		{
			if (Consol == null)
			{
				yield break;
			}

			foreach (ForwardingShipment shipment in Consol.Shipments)
			{
				var goodsDeclarationReferenceNumber = CreateGoodsDeclarationReferenceNumber(shipment);

				if (goodsDeclarationReferenceNumber != null)
				{
					yield return goodsDeclarationReferenceNumber;
				}
			}

			var authorizedSenderNumber = CreateAuthorizedSenderNumber(Consol);
			if (authorizedSenderNumber != null)
			{
				yield return authorizedSenderNumber;
			}
		}

		GoodsDeclarationReferenceNumber CreateGoodsDeclarationReferenceNumber(ForwardingShipment shipment)
		{
			var goodsDeclarationReferenceNumber = new GoodsDeclarationReferenceNumber();

			goodsDeclarationReferenceNumber.Numbers = GetAllGoodsDeclarationReferenceNumbersWithoutLeadingTypeCode(shipment);
			if (goodsDeclarationReferenceNumber.Numbers.IsNullOrEmpty())
			{
				return null;
			}

			goodsDeclarationReferenceNumber.MovementCode = GetNonEUMovementCode(shipment, Core.Constants.CountryCodes.Switzerland);
			goodsDeclarationReferenceNumber.CountryOfIssue = goodsDeclarationReferenceNumber.MovementCode == MovementReferenceCode.Codes.CustomsExport
				? shipment.Origin.RL_RN_NKCountryCode
				: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			return goodsDeclarationReferenceNumber;
		}

		List<ZString> GetAllGoodsDeclarationReferenceNumbersWithoutLeadingTypeCode(ForwardingShipment shipment)
		{
			var goodsDeclarationReferenceNumbers = shipment.CusEntryNumbers.Where(x => x.CE_EntryType == CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber).ToList();
			var result = new List<ZString>();

			foreach (var goodsDeclarationReferenceNumber in goodsDeclarationReferenceNumbers)
			{
				if (!goodsDeclarationReferenceNumber.CE_EntryNum.IsEmpty)
				{
					if (goodsDeclarationReferenceNumber.CE_EntryNum.StartsWith(CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber_4DIGITS, StringComparison.Ordinal))
					{
						result.Add(goodsDeclarationReferenceNumber.CE_EntryNum.SubstringSafe(CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber_4DIGITS.Length));
					}
					else if (goodsDeclarationReferenceNumber.CE_EntryNum.StartsWith(CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber, StringComparison.Ordinal))
					{
						result.Add(goodsDeclarationReferenceNumber.CE_EntryNum.SubstringSafe(CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber.Length));
					}
					else
					{
						result.Add(goodsDeclarationReferenceNumber.CE_EntryNum);
					}
				}
			}

			return result;
		}

		GoodsDeclarationReferenceNumber CreateAuthorizedSenderNumber(ForwardingConsol consol)
		{
			var authorizedSenderNumber = SupplyChainSecurityConfiguration.GetAuthorizedSenderNumber(consol);
			if (authorizedSenderNumber.IsEmpty)
			{
				return null;
			}

			var goodsDeclarationReferenceNumber = new GoodsDeclarationReferenceNumber();
			goodsDeclarationReferenceNumber.Numbers = new List<ZString> { authorizedSenderNumber };
			goodsDeclarationReferenceNumber.MovementCode = MovementReferenceCode.Codes.CustomsExport;
			goodsDeclarationReferenceNumber.CountryOfIssue = consol.JK_RL_NKLoadPort.SubstringSafe(0, 2);

			return goodsDeclarationReferenceNumber;
		}

		#endregion

		#region Movement Reference Numbers

		protected override IEnumerable<MovementReferenceNumber> GetMovementReferenceNumbers()
		{
			if (Consol == null || !GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion)
			{
				yield break;
			}

			foreach (ForwardingShipment shipment in Consol.Shipments)
			{
				var number = CreateMovementReferenceNumber(shipment);

				if (number != null)
				{
					yield return number;
				}
			}
		}

		MovementReferenceNumber CreateMovementReferenceNumber(ForwardingShipment shipment)
		{
			var movementReferenceNumbers = GetAllMovementReferenceNumbersWithoutLeadingTypeCode(shipment);
			if (movementReferenceNumbers == null || movementReferenceNumbers.Count == 0)
			{
				return null;
			}

			var movementReferenceNumber = new MovementReferenceNumber();

			movementReferenceNumber.Numbers.AddRange(movementReferenceNumbers);
			movementReferenceNumber.MovementCode = GetMovementCode(shipment);
			movementReferenceNumber.CountryOfIssue = movementReferenceNumber.MovementCode == MovementReferenceCode.Codes.CustomsExport
				? shipment.Origin.RL_RN_NKCountryCode
				: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			if (!Consol.IsDirect && !shipment.JS_HouseBill.IsEmpty)
			{
				movementReferenceNumber.RelatedNumbers.Add(new EntryNumber
				{
					Type = RelatedMovementReferenceNumberType.Codes.HouseWaybillNumber,
					Number = shipment.JS_HouseBill
				});
			}

			if (Consol.JK_ConsolMode == Core.Constants.ContainerModes.ULD)
			{
				foreach (ForwardingContainer container in shipment.Containers)
				{
					if (!container.JC_ContainerNum.IsEmpty)
					{
						movementReferenceNumber.RelatedNumbers.Add(new EntryNumber
						{
							Type = RelatedMovementReferenceNumberType.Codes.ULDIdentifier,
							Number = container.JC_ContainerNum
						});
					}
				}
			}

			return movementReferenceNumber;
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EH_Table = ForwardingConsol.Schema.TableName;
		}

		#endregion

		#region Saving

		public override SaveMode FactorySaveMode
		{
			get
			{
				SaveMode result = SaveMode.Normal;

				if (ForceSavingByFactory)
				{
					result = SaveMode.Forced;
				}
				else if (Consol != null)
				{
					if (!Consol.IsAir)
					{
						result = SaveMode.Never;
					}
					else
					{
						if (Consol.OverrideWaybillDefaultsHasChanges && (Consol.JK_OverrideWaybillDefaults || IsInDatabase)
							|| Consol.OverrideSecurityDeclarationHasChanges && (Consol.JK_OverrideSecurityDeclarationDefaults || IsInDatabase)
							|| ShouldSavedHeaderForRateLinesOverridden)
						{
							result = SaveMode.Forced;
						}
						else if (!Consol.JK_OverrideWaybillDefaults && !Consol.JK_OverrideSecurityDeclarationDefaults)
						{
							result = SaveMode.Never;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region Addresses

		#region Consignee Addresses

		protected OrgHeader ReceivingForwarder
		{
			get { return (Consol != null) ? Consol.ReceivingForwarder : null; }
		}

		protected override OrgHeader Consignee
		{
			get
			{
				return DirectShipment != null && DirectShipment.Consignee != null
					? DirectShipment.Consignee
					: ReceivingForwarder;
			}
		}

		protected override JobDocAddress ConsigneeDocumentaryAddress
		{
			get { return DirectShipment != null ? DirectShipment.ConsigneeDocumentaryAddress : ReceivingForwarderDocAddress; }
		}

		JobDocAddress ReceivingForwarderDocAddress
		{
			get
			{
				if (fReceivingForwarderDocAddress == null ||
					fReceivingForwarderDocAddress.IsDeleted ||
					ReceivingForwarder != null && ReceivingForwarder.PK != fReceivingForwarderDocAddress.OrganisationPK ||
					ReceivingForwarder == null && fReceivingForwarderDocAddress.OrganisationPK.IsValid)
				{
					if (ReceivingForwarder != null)
					{
						fReceivingForwarderDocAddress = DocAddresses.FindOrCreateWithDocAddressType(Consol.JK_OA_ReceivingForwarderAddress, DocAddressType.ConsigneeDocumentaryAddress);
					}
					else
					{
						fReceivingForwarderDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
					}

					fReceivingForwarderDocAddress.MakeNonPersistent();
				}

				return fReceivingForwarderDocAddress;
			}
		}
		JobDocAddress fReceivingForwarderDocAddress;

		protected override DefaultAddressTypes DefaultConsigneeAddressType
		{
			get
			{
				var result = Consignee != null
					? TranslateDefaultAddressType(Consignee.MiscServ.OM_IMDocumentAddressPreference)
					: DefaultAddressTypes.None;

				return result == DefaultAddressTypes.None
					? base.DefaultConsigneeAddressType
					: result;
			}
		}

		protected override OrgAddress ConsigneeOfficeAddress
		{
			get
			{
				return Consignee != null
						? Consignee.Addresses.DefaultAddressOfType(OrgAddressType.Office)
						: null;
			}
		}

		protected override OrgAddress ConsigneeDeliveryAddress
		{
			get
			{
				return Consignee != null
					? Consignee.Addresses.DefaultAddressOfType(OrgAddressType.Delivery)
					: null;
			}
		}

		protected override ZString ConsigneeAccount
		{
			get
			{
				if (Consol == null)
				{
					return ZString.Empty;
				}

				var handler = GetACASCountryHandler();
				if (handler.ShouldApplyACAS())
				{
					return base.ConsigneeAccount;
				}

				return Consol.IsCoLoad && ReceivingForwarder != null ? ReceivingForwarder.OH_Code : ZString.Empty;
			}
		}

		protected override ZString DefaultConsigneeCompanyName
		{
			get
			{
				return Consignee != null
					? Consignee.OH_FullNameTruncated
					: ZString.Empty;
			}
		}

		protected override List<OrgAddress> GetConsigneeAddresses()
		{
			return Consignee != null
				? Consignee.Addresses.Cast<OrgAddress>().ToList()
				: new List<OrgAddress>();
		}

		#endregion

		#region Shipper Addresses

		protected OrgHeader SendingForwarder
		{
			get { return (Consol != null) ? Consol.SendingForwarder : null; }
		}

		protected override JobDocAddress ShipperDocumentaryAddress
		{
			get { return DirectShipment != null ? DirectShipment.ConsignorDocumentaryAddress : SendingForwarderDocAddress; }
		}

		#region SendingForwarderDocAddress

		JobDocAddress SendingForwarderDocAddress
		{
			get
			{
				if (fSendingForwarderDocAddress == null ||
					fSendingForwarderDocAddress.IsDeleted ||
					SendingForwarder != null && SendingForwarder.PK != fSendingForwarderDocAddress.OrganisationPK ||
					SendingForwarder == null && fSendingForwarderDocAddress.OrganisationPK.IsValid)
				{
					if (SendingForwarder != null)
					{
						fSendingForwarderDocAddress = DocAddresses.FindOrCreateWithDocAddressType(Consol.JK_OA_SendingForwarderAddress, DocAddressType.ConsignorDocumentaryAddress);
					}
					else
					{
						fSendingForwarderDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
					}

					fSendingForwarderDocAddress.MakeNonPersistent();
				}

				return fSendingForwarderDocAddress;
			}
		}
		JobDocAddress fSendingForwarderDocAddress;

		#endregion

		protected override OrgHeader Shipper
		{
			get { return DirectShipment != null ? DirectShipment.Consignor : SendingForwarder; }
		}

		protected override DefaultAddressTypes DefaultShipperAddressType
		{
			get
			{
				DefaultAddressTypes result = DefaultAddressTypes.None;
				if (DirectShipment != null)
				{
					if (DirectShipment.Consignor != null)
					{
						result = TranslateDefaultAddressType(DirectShipment.Consignor.MiscServ.OM_EXDocumentAddressPreference);
					}
				}
				else if (SendingForwarder != null)
				{
					result = TranslateDefaultAddressType(SendingForwarder.MiscServ.OM_EXDocumentAddressPreference);
				}

				return result == DefaultAddressTypes.None ? base.DefaultShipperAddressType : result;
			}
		}

		protected override OrgAddress ShipperOfficeAddress
		{
			get
			{
				if (Shipper != null)
				{
					return Shipper.Addresses.DefaultAddressOfType(OrgAddressType.Office, false);
				}

				return null;
			}
		}

		protected override OrgAddress ShipperPickupAddress
		{
			get
			{
				if (Shipper != null)
				{
					return Shipper.Addresses.DefaultAddressOfType(OrgAddressType.Pickup, false);
				}

				return null;
			}
		}

		protected override ZString DefaultShipperCompanyName
		{
			get
			{
				if (Shipper != null)
				{
					return Shipper.OH_FullNameTruncated;
				}

				return null;
			}
		}

		protected override List<OrgAddress> GetShipperAddresses()
		{
			List<OrgAddress> result = new List<OrgAddress>();

			if (Shipper != null)
			{
				foreach (OrgAddress address in Shipper.Addresses)
				{
					result.Add(address);
				}
			}

			return result;
		}

		protected override ZString ShipperAccount
		{
			get
			{
				if (Consol == null)
				{
					return ZString.Empty;
				}

				var handler = GetACASCountryHandler();
				return handler.ShouldApplyACAS() || Consol.IsCoLoad ? base.ShipperAccount : ZString.Empty;
			}
		}

		protected override void PopulateShipperContactDetails(IDocAddress address)
		{
			if (address.E2_AddressOverride)
			{
				base.PopulateShipperContactDetails(address);
				return;
			}

			if (IsDirectMAWB && DirectShipment != null)
			{
				var orgContact = DirectShipment.ConsignorDocumentaryAddress?.Contact;

				EH_ShipperContactName = orgContact?.OC_ContactName.Left(EH_ShipperContactNameInfo.MaxLength) ?? ZString.Empty;

				if (orgContact != null)
				{
					var orgContactPhone = orgContact.PhoneFallbackToOrganisation;
					var orgContactFax = orgContact.FaxFallbackToOrganisation;
					(EH_ShipperContactCode, EH_ShipperContactDetail) = SetContactCodeAndDetails(orgContactPhone, orgContactFax);
				}
				else if (DirectShipment?.ConsignorDocumentaryAddress != null)
				{
					var consignorDocAddressPhone = DirectShipment.ConsignorDocumentaryAddress.E2_Phone;
					var consignorDocAddressFax = DirectShipment.ConsignorDocumentaryAddress.E2_Fax;
					(EH_ShipperContactCode, EH_ShipperContactDetail) = SetContactCodeAndDetails(consignorDocAddressPhone, consignorDocAddressFax);
				}
			}
			else
			{
				EH_ShipperContactName = Consol?.SendingForwarderWithContact?.OrgContact?.OC_ContactName.Left(EH_ShipperContactNameInfo.MaxLength) ?? ZString.Empty;
				var consignorPhone = Consol?.SendingForwarderWithContact?.ContactDetails_PhoneCore;
				var consignorFax = Consol?.SendingForwarderWithContact?.ContactDetails_FaxCore;
				(EH_ShipperContactCode, EH_ShipperContactDetail) = SetContactCodeAndDetails(consignorPhone, consignorFax);
			}
		}

		protected override void PopulateConsigneeContactDetails(IDocAddress address)
		{
			if (address.E2_AddressOverride)
			{
				base.PopulateConsigneeContactDetails(address);
				return;
			}

			if (IsDirectMAWB && DirectShipment != null)
			{
				var orgContact = DirectShipment.ConsigneeDocumentaryAddress?.Contact;

				EH_ConsigneeContactName = orgContact?.OC_ContactName.Left(EH_ConsigneeContactNameInfo.MaxLength) ?? ZString.Empty;

				if (orgContact != null)
				{
					var orgContactPhone = orgContact.PhoneFallbackToOrganisation;
					var orgContactFax = orgContact.FaxFallbackToOrganisation;
					(EH_ConsigneeContactCode, EH_ConsigneeContactDetail) = SetContactCodeAndDetails(orgContactPhone, orgContactFax);
				}
				else if (DirectShipment?.ConsigneeDocumentaryAddress != null)
				{
					var consigneeDocAddressPhone = DirectShipment.ConsigneeDocumentaryAddress.E2_Phone;
					var consigneeDocAddressFax = DirectShipment.ConsigneeDocumentaryAddress.E2_Fax;
					(EH_ConsigneeContactCode, EH_ConsigneeContactDetail) = SetContactCodeAndDetails(consigneeDocAddressPhone, consigneeDocAddressFax);
				}
			}
			else
			{
				EH_ConsigneeContactName = Consol?.ReceivingForwarderWithContact?.OrgContact?.OC_ContactName.Left(EH_ConsigneeContactNameInfo.MaxLength) ?? ZString.Empty;

				var consigneePhone = Consol?.ReceivingForwarderWithContact?.ContactDetails_PhoneCore;
				var consigneeFax = Consol?.ReceivingForwarderWithContact?.ContactDetails_FaxCore;
				(EH_ConsigneeContactCode, EH_ConsigneeContactDetail) = SetContactCodeAndDetails(consigneePhone, consigneeFax);
			}
		}

		#endregion

		#region Notify Address

		internal protected override JobDocAddress NotifyPartyDocumentaryAddress
		{
			get
			{
				JobDocAddress result = null;

				if (DirectShipment != null)
				{
					result = DirectShipment.NotifyPartyDocumentaryAddress;
				}
				else if (Consol != null)
				{
					result = Consol.NotifyPartyDocumentaryAddress;
				}

				return result;
			}
		}

		protected override List<OrgAddress> GetAlsoNotifyAddresses()
		{
			List<OrgAddress> result = new List<OrgAddress>();

			if (DirectShipment != null)
			{
				if (DirectShipment.NotifyParty != null)
				{
					result.AddRange(DirectShipment.NotifyParty.Addresses.Cast<OrgAddress>());
				}
			}
			else if (Consol != null)
			{
				if (Consol.NotifyParty != null)
				{
					result.AddRange(Consol.NotifyParty.Addresses.Cast<OrgAddress>());
				}

				if (Consol.NotifyParty2 != null)
				{
					result.AddRange(Consol.NotifyParty2.Addresses.Cast<OrgAddress>());
				}

				if (Consol.NotifyParty3 != null)
				{
					result.AddRange(Consol.NotifyParty3.Addresses.Cast<OrgAddress>());
				}
			}

			result.AddRange(GetConsigneeAddresses());

			return result;
		}

		#endregion

		#endregion

		#region Properties

		public override bool IsMAWB => true;

		public override bool IsDirectMAWB => Consol != null && Consol.IsDirect;

		public override ZString EH_ShipperAddress
		{
			get { return base.EH_ShipperAddress; }
			set
			{
				base.EH_ShipperAddress = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ShipperAddress2();
				}
			}
		}

		public override ZString EH_ShipperAddress2
		{
			get { return base.EH_ShipperAddress2; }
			set
			{
				base.EH_ShipperAddress2 = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ShipperAddress();
				}
			}
		}

		public override ZString EH_ShipperContactEmail
		{
			get
			{
				var email = ZString.Empty;
				if (IsDirectMAWB && DirectShipment != null)
				{
					var orgContact = DirectShipment.ConsignorDocumentaryAddress?.Contact;
					if (orgContact != null)
					{
						email = orgContact.EmailFallbackToOrganisation.IsValid ? orgContact.EmailFallbackToOrganisation : ZString.Empty;
					}
					else if (DirectShipment?.ConsignorDocumentaryAddress != null)
					{
						email = DirectShipment.ConsignorDocumentaryAddress.E2_Email.IsValid ? DirectShipment.ConsignorDocumentaryAddress.E2_Email : ZString.Empty;
					}
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(Consol?.SendingForwarderWithContact?.ContactDetails_Email))
					{
						email = Consol.SendingForwarderWithContact.ContactDetails_Email;
					}
					else if (!string.IsNullOrWhiteSpace(Consol?.SendingForwarderWithContact?.Email))
					{
						email = Consol.SendingForwarderWithContact.Email;
					}
					else if (Consol?.SendingForwarderWithContact?.OrgAddress is IContactBase contactBase && !string.IsNullOrWhiteSpace(contactBase.Email))
					{
						email = contactBase.Email;
					}
				}
				return email;
			}
		}

		public override ZString EH_ConsigneeAddress
		{
			get { return base.EH_ConsigneeAddress; }
			set
			{
				base.EH_ConsigneeAddress = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ConsigneeAddress2();
				}
			}
		}

		public override ZString EH_ConsigneeAddress2
		{
			get { return base.EH_ConsigneeAddress2; }
			set
			{
				base.EH_ConsigneeAddress2 = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateEH_ConsigneeAddress();
				}
			}
		}

		public override ZString EH_ConsigneeContactEmail
		{
			get
			{
				var email = ZString.Empty;
				if (IsDirectMAWB && DirectShipment != null)
				{
					var orgContact = DirectShipment.ConsigneeDocumentaryAddress?.Contact;
					if (orgContact != null)
					{
						email = orgContact.EmailFallbackToOrganisation.IsValid ? orgContact.EmailFallbackToOrganisation : ZString.Empty;
					}
					else if (DirectShipment?.ConsigneeDocumentaryAddress != null)
					{
						email = DirectShipment.ConsigneeDocumentaryAddress.E2_Email.IsValid ? DirectShipment.ConsigneeDocumentaryAddress.E2_Email : ZString.Empty;
					}
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(Consol?.ReceivingForwarderWithContact?.ContactDetails_Email))
					{
						email = Consol.ReceivingForwarderWithContact.ContactDetails_Email;
					}
					else if (!string.IsNullOrWhiteSpace(Consol?.ReceivingForwarderWithContact?.Email))
					{
						email = Consol.ReceivingForwarderWithContact.Email;
					}
					else if (Consol?.ReceivingForwarderWithContact?.OrgAddress is IContactBase contactBase && !string.IsNullOrWhiteSpace(contactBase.Email))
					{
						email = contactBase.Email;
					}
				}
				return email;
			}
		}

		#region Also Notify

		void ValidateMandatoryAlsoNotifyFields()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateMandatoryAlsoNotifyFields();
			}
		}

		public override ZString EH_AlsoNotifyName
		{
			get { return base.EH_AlsoNotifyName; }
			set
			{
				base.EH_AlsoNotifyName = value;
				ValidateMandatoryAlsoNotifyFields();
			}
		}

		public override ZString EH_AlsoNotifyAddress
		{
			get { return base.EH_AlsoNotifyAddress; }
			set
			{
				base.EH_AlsoNotifyAddress = value;
				ValidateMandatoryAlsoNotifyFields();
			}
		}

		public override ZString EH_AlsoNotifyPlace
		{
			get { return base.EH_AlsoNotifyPlace; }
			set
			{
				base.EH_AlsoNotifyPlace = value;
				ValidateMandatoryAlsoNotifyFields();
			}
		}

		public override ZString EH_AlsoNotifyCountryCode
		{
			get { return base.EH_AlsoNotifyCountryCode; }
			set
			{
				base.EH_AlsoNotifyCountryCode = value;
				ValidateMandatoryAlsoNotifyFields();
			}
		}

		public override ZString EH_AlsoNotifyState
		{
			get { return base.EH_AlsoNotifyState; }
			set
			{
				base.EH_AlsoNotifyState = value;
				ValidateMandatoryAlsoNotifyFields();
			}
		}

		public override ZString EH_AlsoNotifyPostCode
		{
			get { return base.EH_AlsoNotifyPostCode; }
			set
			{
				base.EH_AlsoNotifyPostCode = value;
				ValidateMandatoryAlsoNotifyFields();
			}
		}

		public override ZString EH_AlsoNotifyContactCode
		{
			get { return base.EH_AlsoNotifyContactCode; }
			set
			{
				base.EH_AlsoNotifyContactCode = value;
				ValidateMandatoryAlsoNotifyFields();
			}
		}

		public override ZString EH_AlsoNotifyContactDetail
		{
			get { return base.EH_AlsoNotifyContactDetail; }
			set
			{
				base.EH_AlsoNotifyContactDetail = value;
				ValidateMandatoryAlsoNotifyFields();
			}
		}

		#endregion

		#region Location Related

		protected override ZString OriginCode
		{
			get { return IATAPortCodeFromUNLOCOCode(OriginLOCO); }
		}

		protected override ZString AirportOfDeparture
		{
			get { return OriginLOCO != null ? OriginLOCO.RL_PortName.Left(35) : ZString.Empty; }
		}

		public override GlbBranch DeparturePortRelatedBranch
		{
			get { return OriginLOCO != null ? GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, OriginLOCO, GlbCompany.CurrentCompany) : null; }
		}

		protected override RefUNLOCO OriginLOCO
		{
			get { return ConsolOriginLOCO; }
		}

		protected override RefUNLOCO DestinationLOCO
		{
			get { return (Consol != null) ? Consol.DischargePort : null; }
		}

		protected RefUNLOCO LoadPortLOCO => Consol?.LoadPort;

		public RefCountry LoadPortCountry => LoadPortLOCO?.Country;

		protected override ZString AWBDestinationText
		{
			get { return ConsolDestinationLOCO != null ? ConsolDestinationLOCO.RL_PortName.Left(35) : ZString.Empty; }
		}

		protected override ZString AWBDestinationCode
		{
			get { return IATAPortCodeFromUNLOCOCode(ConsolDestinationLOCO); }
		}

		#endregion

		#region Advance Cargo Reporting Self-Filer

		protected override ZBool IsConsigneeAdvanceCargoReportingSelfFilerSet
		{
			get
			{
				return DirectShipment != null && DirectShipment.Consignee != null ? DirectShipment.IsAdvanceCargoReportingSelfFiler : Consol.IsAdvanceCargoReportingSelfFiler;
			}
		}

		#endregion

		protected override ZBool OverrideWaybillDefaults
		{
			get { return (Consol != null) ? Consol.JK_OverrideWaybillDefaults : ZBool.True; }
		}

		protected override ZBool OverrideSecurityDeclarationDefaults
		{
			get { return (Consol != null) ? Consol.JK_OverrideSecurityDeclarationDefaults : ZBool.True; }
		}

		protected override ZString ChargesCode
		{
			get { return (Consol != null) ? Consol.JK_PrepaidCollect.Left(2) : ZString.Empty; }
		}

		public override ZString WeightVPPDCOL
		{
			get
			{
				string code = ChargesCode.Left(2);
				return code.IsNullOrEmpty() ? "" : (code == "PP" ? Constants.PrepaidCollect3CharCodes.Prepaid : Constants.PrepaidCollect3CharCodes.Collect);
			}
		}

		protected override ZString OtherPPDCOL
		{
			get { return WeightVPPDCOL; }
		}

		protected override ZDecimal CustomsValue
		{
			get
			{
				ZDecimal result = 0;

				bool isDestinedForBangladesh = DestinationCountryCode.Equals(Core.Constants.CountryCodes.Bangladesh);
				bool showGoodsValueOnDirectMawb = FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.Value || isDestinedForBangladesh;
				if (IsDirectMAWB && Consol.Shipments.Count == 1 && showGoodsValueOnDirectMawb)
				{
					result = Consol.Shipments[0].JS_GoodsValue;
				}

				return result;
			}
		}

		protected override ZString CustomsValueCurrency
		{
			get
			{
				ZString result = "";

				if (IsDirectMAWB && Consol.Shipments.Count == 1)
				{
					result = Consol.Shipments[0].JS_RX_NKGoodsValueCurr;
				}

				return result;
			}
		}

		protected override ZDecimal InsuranceValue
		{
			get
			{
				ZDecimal result = 0;

				if (IsDirectMAWB && Consol.Shipments.Count == 1)
				{
					result = Consol.Shipments[0].JS_InsuranceValue;
				}

				return result;
			}
		}

		protected override ZString InsuranceValueCurrency
		{
			get
			{
				ZString result = "";

				if (IsDirectMAWB && Consol.Shipments.Count == 1)
				{
					result = Consol.Shipments[0].JS_RX_NKInsuranceCurrency;
				}

				return result;
			}
		}

		protected override ZString ConsolNumber
		{
			get { return (Consol != null) ? Consol.JK_UniqueConsignRef : ZString.Empty; }
		}

		protected override ZString AWBCurrency
		{
			get { return Consol != null && Consol.AWBCurrency != null ? Consol.AWBCurrency.RX_Code : ZString.Empty; }
		}

		#region Agent IATA Details

		protected override ZString AgentName
		{
			get { return IsBorrowedMaster ? BorrowedFrom.OH_FullNameTruncated.ToUpper() : base.AgentName; }
		}

		protected override ZString AgentPlace
		{
			get
			{
				if (IsBorrowedMaster)
				{
					return BorrowedFrom.UNLOCO != null ? BorrowedFrom.UNLOCO.RL_PortName.Left(50).ToUpper() : ZString.Empty;
				}
				else
				{
					return base.AgentPlace;
				}
			}
		}

		protected override ZString AgentIATACode
		{
			get { return IsBorrowedMaster ? BorrowedFrom.MiscServ.OM_FWIATACode.Trim().Left(14) : base.AgentIATACode; }
		}

		#region AgentAccountNo

		protected override ZString AgentAccountNo
		{
			get
			{
				ZString result = "";

				if (IsBorrowedMaster)
				{
					result = BorrowedFrom.MiscServ.OM_FWIATAAccountNumber.Trim().Left(14);
				}
				else
				{
					result = AirlineBranchAccountNumber;
					if (result.IsEmpty)
					{
						result = AgentAccountNumberFromIssuingCarrier;
					}
				}

				return result.IsEmpty ? base.AgentAccountNo : result;
			}
		}

		ZString AirlineBranchAccountNumber
		{
			get
			{
				if (Consol?.ShippingLine != null && GlbBranch.CurrentBranch != null)
				{
					var orgAirlineBranchAccount = Consol.ShippingLine.OrgAirlineBranchAccounts
						.FirstOrDefault(o => o.OAA_GB_Branch == GlbBranch.CurrentBranch.PK);
					if (orgAirlineBranchAccount != null)
					{
						return orgAirlineBranchAccount.OAA_APAirlineAccountNumber;
					}
				}

				return ZString.Empty;
			}
		}

		ZString AgentAccountNumberFromIssuingCarrier
		{
			get
			{
				if (PreviousAirlinePrefix != EH_AirlinePrefix) //cache - only do when the Airline prefix changes
				{
					if (!EH_AirlinePrefix.IsEmpty)
					{
						DynamicBusinessObjectCollection dynamicBusinessObjectCollection = new DynamicBusinessObjectCollection(Factory);
						dynamicBusinessObjectCollection.Load(AgentAccountNumberCommandText, AgentAccountNumberParameterCollection);

						if (dynamicBusinessObjectCollection.Count == 1)
						{
							DynamicBusinessObject dynamicBusinessObject = dynamicBusinessObjectCollection[0];
							fAgentAccountNumberFromIssuingCarrier = (ZString)dynamicBusinessObject[OrgCompanyDataSchema.Constants.OB_APAirlineAccountNumber];
						}
					}
					PreviousAirlinePrefix = EH_AirlinePrefix;
				}

				return fAgentAccountNumberFromIssuingCarrier;
			}
		}
		ZString PreviousAirlinePrefix;
		ZString fAgentAccountNumberFromIssuingCarrier;

		string AgentAccountNumberCommandText
		{
			get
			{
				return
					"select " + OrgCompanyDataSchema.Constants.OB_APAirlineAccountNumber +
					"  from " + OrgCompanyDataSchema.Constants.SqlSchemaName + "." + OrgCompanyDataSchema.Constants.TableName +
					"	 where " + OrgCompanyDataSchema.Constants.OB_GC + " = @CurrentCompany" +
					"	 and " + OrgCompanyDataSchema.Constants.OB_OH + " = (select top 1 " + OrgMiscServSchema.Constants.OM_OH +
					"                                                       from " + OrgMiscServSchema.Constants.SqlSchemaName + "." + OrgMiscServSchema.Constants.TableName +
					"                                                       where " + OrgMiscServSchema.Constants.OM_RM_Airline + " = (select top 1 " + RefAirlineSchema.Constants.PK +
					"                                                                                                                 from " + RefAirlineSchema.Constants.SqlSchemaName + "." + RefAirlineSchema.Constants.TableName +
					"                                                                                                                 where " + RefAirlineSchema.Constants.RM_EagleAddedAirlinePrefixOrAccountingCode + " = @AirlinePrefix" +
					"                                                                                                                 and " + RefAirlineSchema.Constants.RM_IsActive + " = 1))";
			}
		}

		ZSqlParameterCollection AgentAccountNumberParameterCollection
		{
			get
			{
				ZSqlParameterCollection result = new ZSqlParameterCollection();
				result.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, OrgCompanyDataSchema.OB_GC);
				result.Add("@AirlinePrefix", EH_AirlinePrefix, RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode);
				return result;
			}
		}

		#endregion

		#endregion

		#region Customs Entry Numbers

		protected override IEnumerable<EntryNumber> GetCustomsEntryNumbers()
		{
			var result = Array.Empty<EntryNumber>();

			if (Consol != null)
			{
				result = CreateCustomsEntryNumbers(Consol.CusEntryNums.Cast<CusEntryNumber>()).ToArray();

				if (!result.Any() && Consol.Shipments.Count == 1)
				{
					result = CreateCustomsEntryNumbers(Consol.Shipments[0].CusEntryNumbers.Cast<CusEntryNumber>()).ToArray();
				}
			}
			return result;
		}

		#endregion

		protected override ZString ReferenceNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol != null)
				{
					if (Consol.IsAgentOrDirect || Consol.IsAWBCoload)
					{
						result = AirlinePrefix + "-" + SerialNumber;
					}
					else if (Consol.IsCoLoad)
					{
						result = (NoResString)"MASTER HAWB:" + Consol.JK_UniqueConsignRef; // IATA Text
					}
				}
				return result.Trim();
			}
		}

		protected ZBool IsAgentOrDirect
		{
			get { return Consol != null && (Consol.IsAgentOrDirect || Consol.IsAWBCoload); }
		}

		public bool AsAgreedAllowed
		{
			get { return ((IsAgentOrDirect && Env.Registry.Freight.AirWaybill.AllowAsAgreed) || !IsAgentOrDirect); }
		}

		protected override TaxCodeInformation GetTaxCodeInformationForBangladesh(List<TaxCodeInformation> taxInfos)
		{
			if (!IsImportToBangladesh || taxInfos.IsNullOrEmpty())
			{
				return null;
			}

			return Consol.IsDirect
				? taxInfos.SingleOrDefault(t => t.Code == OrgCusCode.CodeTypes.VATCode)
				: taxInfos.SingleOrDefault(t => t.Code == OrgCusCode.BangladeshCodeTypes.AIN);
		}

		public override bool IsImportToBangladesh => IsImportToCountry(CountryCodes.Bangladesh);

		public override bool IsImportToBrazil => IsImportToCountry(CountryCodes.Brazil);

		public override bool IsImportToChina => IsImportToCountry(CountryCodes.China);

		public override bool IsBolivianNITRequired => IsDirectMAWB;

		public override bool IsHondurasRTNRequired => IsImportToHonduras && IsDirectMAWB;

		public override bool IsIndianCARNRequired
		{
			get
			{
				if (IsDirectMAWB)
				{
					return DirectShipment?.Consignee?.MiscServ.OM_IMAdvanceCargoReportingSelfFiler ?? false;
				}

				return Consol?.ReceivingForwarder?.MiscServ.OM_FWAdvanceCargoReportingSelfFiler ?? false;
			}
		}

		#region Canada

		public override bool IsImportToCanada => IsImportToCountry(CountryCodes.Canada);

		public override bool IsExportFromCanada => IsExportFromCountry(CountryCodes.Canada);

		#endregion

		protected override bool IsImportToCountry(ZString countryCode)
		{
			return Consol != null && Consol.IsImportTo(countryCode);
		}

		protected override bool IsExportFromCountry(ZString countryCode)
		{
			return Consol != null && Consol.IsExportFrom(countryCode);
		}

		public override bool IsTransitingThroughChina => IsTransitingThrough(CountryCodes.China);

		protected override bool IsTransitingThrough(ZString countryCode)
		{
			return Consol != null
					&& (!Consol.JK_RL_NKLoadPort.StartsWith(countryCode, StringComparison.Ordinal) && !Consol.JK_RL_NKDischargePort.StartsWith(countryCode, StringComparison.Ordinal))
					&& (Consol.Transports.IsAnyLoadInCountry(countryCode) || Consol.Transports.IsAnyDischargeInCountry(countryCode));
		}

		public override bool IsImportToICS2Zone
		{
			get
			{
				return Consol?.Transports != null && Consol.Transports.IsAirImportOrTransitToICS2Zone && HasDestinationInIcs2Zone && !IsOriginInIcs2Zone;
			}
		}

		#region HSCodeValidation

		public override bool HasInboundToICS2Zone
		{
			get
			{
				return Consol?.Transports.IsAirImportOrTransitToICS2Zone ?? false;
			}
		}

		#endregion

		public bool IsOriginInIcs2Zone
		{
			get
			{
				return LoadPortLOCO?.IsInIcs2Zone ?? false;
			}
		}

		public bool IsToOrderConsignee
		{
			get
			{
				var identifiers = new List<string> { (NoResString)"TO ORDER", (NoResString)"TO ORDER OF", (NoResString)"TO THE ORDER", (NoResString)"TO THE ORDER OF" }; // non-translatable validation message
				return identifiers.Contains(EH_ConsigneeName.ToUpperInvariant());
			}
		}

		protected override BillIssuedBy GetNewIssuedBy()
		{
			if (Consol != null)
			{
				return (Consol.IsAgentOrDirect || Consol.IsAWBCoload)
						? new BillIssuedByAirline(RefAirline.LoadFromAirlinePrefix(Factory, AirlinePrefix))
						: new BillIssuedBy(SendingForwarder);
			}

			return new BillIssuedBy((OrgHeader)null);
		}

		protected override bool ShowExportStatementSetting(ExportStatementSetting mandatorySetting)
		{
			return mandatorySetting.UseOnConsolidationMawb || mandatorySetting.UseOnDirectIATAMawb;
		}

		protected override ZDecimal RateLineChargeableWeight
		{
			get
			{
				ZDecimal result = 0;

				if (Consol != null)
				{
					result = Consol.JK_ConsolChargeable.IsEmpty ? Consol.GetTotalShipmentChargeableForDoc(Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay) : Consol.JK_ConsolChargeable;
				}

				return result;
			}
		}

		public ZDecimal ConsolChargeableWeight
		{
			get
			{
				return RateLineChargeableWeight;
			}
		}

		protected override ZDecimal RateLineGrossWeight
		{
			get
			{
				if (Consol == null)
				{
					return 0m;
				}
				var sourceWeightUnit = Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Actual ? Consol.JK_CorrectedConsolWeightUnit : Consol.JK_TotalShipmentWeightUnit;
				var targetWeightUnit = Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Actual ?
					(Consol.JK_CorrectedConsolWeightUnit == Weight.Pounds ? Weight.Pounds : Weight.Kilograms) :
					(Consol.JK_TotalShipmentWeightUnit == Weight.Pounds ? Weight.Pounds : Weight.Kilograms);
				return Weight.Convert(Consol.GetTotalShipmentWeightForDoc(Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay), sourceWeightUnit, targetWeightUnit);
			}
		}

		public override ZString GoodsDescription
		{
			get
			{
				ZString result = "";
				if (DirectShipment != null)
				{
					result = DirectShipment.DetailedGoodsDescriptionNoteText;
					if (result.IsEmpty)
					{
						result = DirectShipment.JS_GoodsDescription;
					}

					result += "\n";
				}
				else if (Consol != null && !Consol.IsDirect)
				{
					result = Core.Constants.AWB.NatureAndQtyOfGoodsDetails.ConsolAsPerList + "\n";
				}

				result = AddRadioactiveSubstanceInExceptedQuantities(result);

				if (!HAWBNumbers.IsEmpty)
				{
					result += HAWBNumbers + "\n";
				}

				return result;
			}
		}

		public ZString AddRadioactiveSubstanceInExceptedQuantities(ZString goodsDescription)
		{
			if (Consol == null)
			{
				return goodsDescription;
			}

			var shipmentsHasRadioactiveSubstanceInExceptedQuantities = Consol.Shipments.OfType<ForwardingShipment>()
					.Where(DoesShipmentHasRadioactiveSubstanceInExceptedQuantities);
				foreach (var shipment in shipmentsHasRadioactiveSubstanceInExceptedQuantities)
				{
					if (!Consol.IsDirect)
					{
						goodsDescription = AddShipmentAddressDetailsForRadioactiveSubstanceMessage(goodsDescription, shipment);
					}

					goodsDescription = AddRadioactiveUndgMessage(goodsDescription, shipment);
				}

			return goodsDescription;
		}

		 public ZString AddShipmentAddressDetailsForRadioactiveSubstanceMessage(ZString goodsDescription, ForwardingShipment shipment)
		 {
			var consignorAddress = shipment.ConsignorDocumentaryAddress;
			var consigneeAddress = shipment.ConsigneeDocumentaryAddress;
			goodsDescription += BuildOrgAddressMessage(shipment.Consignor, consignorAddress, (NoResString)"Shipper") + " ";
			goodsDescription += BuildOrgAddressMessage(shipment.Consignee, consigneeAddress, (NoResString)"Consignee") + "\n";

			return goodsDescription;
		 }

		public ZString AddRadioactiveUndgMessage(ZString goodsDescription, ForwardingShipment shipment)
		{
			foreach (var packLine in shipment.OuterPackLines.OfType<PackLine>())
			{
				var defaultPackCount = packLine.JL_PackageCount;
				var defaultPackType =
					packLine.JL_F3_NKPackType_List.GetDescriptionFromCode(packLine.JL_F3_NKPackType);
				var packLineUNDGsWithRadioactiveSubstance = packLine.UNDGs.Where(undg =>
					ForwardingUNDGDataItemExtensions.IsRadioactiveInExceptedQuantities(undg.Substance)
					|| (undg.Substance != null && undg.Substance.DG_UNNO == ShippersDeclarationUNDGExclusions.UNNOCodes.UN1845));
				foreach (var undg in packLineUNDGsWithRadioactiveSubstance)
				{
					goodsDescription += BuildRadioactiveUndgMessage(undg, defaultPackCount, defaultPackType) + "\n";
				}
			}

			return goodsDescription;
		}

		protected override ZString RateLineWeightUnit
		{
			get
			{
				if (Consol == null)
				{
					return "K";
				}

				return Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay == Core.WeightAndVolumeDisplayTypes.Codes.Actual ?
					(Consol.JK_CorrectedConsolWeightUnit == Weight.Pounds ? "L" : "K") :
					(Consol.JK_TotalShipmentWeightUnit == Weight.Pounds ? "L" : "K");
			}
		}

		protected override ZString RateLineGrossWeightUnit
		{
			get { return (Consol != null && Consol.JK_TotalShipmentWeightUnit == Core.Constants.Weight.Pounds) ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms; }
		}

		public override ZString AWBRatelineOvertypedNotes
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol != null)
				{
					StmNote[] notes = Consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description);
					if (notes.Length > 0)
					{
						result = notes[0].ST_NoteDataAsText;
					}
				}
				return result;
			}
		}

		protected override ZString AsAgreed1st
		{
			get
			{
				if (IsAgentOrDirect && IsImportToBrazil)
				{
					return Core.Constants.AWB.AsAgreedTypes.Codes.None;
				}

				if (Consol != null)
				{
					var chargesApply = Consol.JK_MBLAWBChargesDisplay;
					var (asAgreedFirstSet, _) = ChargesApplyHelper.GetAsAgreedCodesFromChargesApply(chargesApply);
					return asAgreedFirstSet;
				}

				return Core.Constants.AWB.AsAgreedTypes.Codes.None;
			}
		}

		protected override ZString AsAgreed2nd
		{
			get
			{
				if (IsAgentOrDirect && IsImportToBrazil)
				{
					return Core.Constants.AWB.AsAgreedTypes.Codes.None;
				}

				if (Consol != null)
				{
					var chargesApply = Consol.JK_MBLAWBChargesDisplay;
					var (_, asAgreedSecondSet) = ChargesApplyHelper.GetAsAgreedCodesFromChargesApply(chargesApply);
					return asAgreedSecondSet;
				}

				return Core.Constants.AWB.AsAgreedTypes.Codes.None;
			}
		}

		protected override ZString HandlingInformation
		{
			get
			{
				ZString flight3Info = Get3rdFlightInformationString(Consol != null ? Consol.Transports : null);
				var handlingInfoBuilder = new ZStringBuilder();
				var noteBuilder = new ZStringBuilder();

				if (Consol != null)
				{
					if (!flight3Info.IsEmpty)
					{
						handlingInfoBuilder.Append(flight3Info);
					}

					var dangerousGoodsHandlingInformation = GetDangerousGoodsHandlingInformation();

					if (!string.IsNullOrEmpty(dangerousGoodsHandlingInformation))
					{
						handlingInfoBuilder.Append(dangerousGoodsHandlingInformation);
					}

					StmNote[] notes;

					if (Consol.IsDirect)
					{
						notes = Consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
					}
					else
					{
						Consol.Notes.ForceReloadRelatedElementsOnNextAccess = true;
						notes = Consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description, true);
					}

					if (notes.Length > 0)
					{
						noteBuilder.Append(notes[0].ST_NoteDataAsText);
					}

					if (Consol.IsDirect && Consol.Shipments.Count == 1)
					{
						Consol.Shipments[0].Notes.ForceReloadRelatedElementsOnNextAccess = true;
						var shipmentNotes = Consol.Shipments[0].Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description, true);

						if (shipmentNotes.Length > 0)
						{
							noteBuilder.Append(shipmentNotes[0].ST_NoteDataAsText);
						}
					}
					noteBuilder.AppendIfNotEmpty(GetExclusiveUseHandlingInformation());

					if (!noteBuilder.IsEmpty)
					{
						handlingInfoBuilder.Append(noteBuilder.ToStringWithDelimiterBetweenAppends(" "));
					}

					if (DestinationCountryCode == CountryCodes.Argentina && !EH_ConsigneeTraderNo.IsEmpty)
					{
						handlingInfoBuilder.Append(ConsigneeTraderTypeWithNo);
					}
					handlingInfoBuilder.AppendIfNotEmpty(GetEgyptHandlingInformation());
					handlingInfoBuilder.AppendIfNotEmpty(Consol.GetBrazilAWBHandlingInformation());
				}

				ZString result = handlingInfoBuilder.ToStringWithNewLineBetweenAppends();
				AddExtraText(FreightDataRegistry.Instance.MAWBHandlingInformationExtraText.Value, ref result, FreightDataRegistry.Instance.MAWBHandlingInformationExtraText, System.Environment.NewLine);

				return result;
			}
		}

		string BuildOrgAddressMessage(OrgHeader org, JobDocAddress address, string orgTypeName)
		{
			var addressMessageList = new List<string>();
			if (org != null && !org.OH_FullName.IsEmpty)
			{
				addressMessageList.Add(org.OH_FullName);
			}
			if (address != null)
			{
				addressMessageList.Add(address.Address1);
				addressMessageList.Add(address.Address2);
				addressMessageList.Add(address.City);
				addressMessageList.Add(address.Country?.RN_Desc);
			}
			addressMessageList.RemoveAll(string.IsNullOrEmpty);
			return addressMessageList.Count > 0 ? $"{orgTypeName}: {string.Join(",", addressMessageList)}" : string.Empty;
		}

		string BuildRadioactiveUndgMessage(UNDGDataItem undg, ZInt defaultPackCount, string defaultPackType)
		{
			var packCount = undg.DI_PackageCount != 0 ? undg.DI_PackageCount : defaultPackCount;
			var packType = undg.DI_PackageCount != 0 ? undg.Lookups.PackTypes.GetDescriptionFromCode(undg.DI_F3_NKPackType) : defaultPackType;
			var radioactiveUndgMessageList = new List<string>();
			var undgUnno = (undg.Substance == null) ? ZString.Empty
				: undg.Substance.DG_UNNO.ToUpper();
			radioactiveUndgMessageList.Add("UN" + undgUnno + "," + undg.ProperShippingName);
			radioactiveUndgMessageList.Add(undg.Lookups.ApprovalCertificateTypeList.GetDescriptionFromCode(undg.DI_ApprovalCertificateType));
			radioactiveUndgMessageList.Add(undg.DI_ApprovalCertificateIDMark);
			radioactiveUndgMessageList.Add(packCount + " " + packType);

			return string.Join("\n", radioactiveUndgMessageList.Where(x => !string.IsNullOrEmpty(x)));
		}

		string GetExclusiveUseHandlingInformation()
		{
			var containsExclusiveUseUNDGDataItems = Consol
				.Shipments
				.Cast<ForwardingShipment>()
				.SelectMany(shipment => shipment.OuterPackLines)
				.Cast<ForwardingPackLine>()
				.SelectMany(packline => packline.UNDGs)
				.Cast<UNDGDataItem>()
				.Any(undgDataItem => undgDataItem.DI_IsExclusiveUse);

			if (containsExclusiveUseUNDGDataItems)
			{
				return (NoResString)"Exclusive Use";
			}

			return string.Empty;
		}

		bool DoesShipmentHasRadioactiveSubstanceInExceptedQuantities(ForwardingShipment forwardingShipment)
		{
			var packLineUNDGs = forwardingShipment.OuterPackLines.OfType<PackLine>().Distinct().SelectMany(packLine => packLine.UNDGs).ToList();

			foreach (var undg in packLineUNDGs)
			{
				if (ForwardingUNDGDataItemExtensions.IsRadioactiveInExceptedQuantities(undg.Substance))
				{
					return true;
				}
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Multiplier symbol")]
		string GetDangerousGoodsHandlingInformation()
		{
			var allPackLines = Consol.Shipments.OfType<ForwardingShipment>()
				.SelectMany(shipment => shipment.OuterPackLines)
				.OfType<PackLine>()
				.Distinct()
				.ToList();

			if (Consol.Shipments.OfType<ForwardingShipment>().Any(DoesShipmentHasRadioactiveSubstanceInExceptedQuantities))
			{
				return string.Empty;
			}

			var dangerousPackLinesToDeclare = allPackLines
				.Where(p => p.UNDGs.Sum(undg => undg.DI_PackageCount) != 0)
				.Where(p => ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(p) || ShippersDeclarationUNDGExclusions.RequireLithiumBatteriesUNDGsDeclaration(p))
				.ToList();

			if (dangerousPackLinesToDeclare.Count > 0)
			{
				var dgInfoBuilder = new ZStringBuilder();

				if (ContainsDGNonDGMixCrossAllPackLines(allPackLines, dangerousPackLinesToDeclare))
				{
					dgInfoBuilder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} x {1}",
						ShippersDeclarationUNDGExclusions.GetCountOfDangerousPacks(dangerousPackLinesToDeclare).ToString(CultureInfo.InvariantCulture),
						ShippersDeclarationUNDGExclusions.GetTypeOfDangerousPacks()));
				}

				dgInfoBuilder.Append("Dangerous Goods as per associated Shipper's Declaration");

				if (RequireCargoAircraftOnlyInHandlingInformation(dangerousPackLinesToDeclare))
				{
					dgInfoBuilder.Append("– Cargo Aircraft Only");
				}

				return dgInfoBuilder.ToStringWithDelimiterBetweenAppends(" ");
			}

			return string.Empty;
		}

		bool ContainsDGNonDGMixCrossAllPackLines(IEnumerable<PackLine> packlines, IEnumerable<PackLine> dangerousPackLines)
		{
			return packlines.Count() > dangerousPackLines.Count();
		}

		bool RequireCargoAircraftOnlyInHandlingInformation(IEnumerable<PackLine> packlines)
		{
			var undgs = packlines.SelectMany(packLine => packLine.UNDGs.Where(undg => undg.Substance != null));
			var showCargoAirCraftOnly = false;
			foreach (var undg in undgs)
			{
				showCargoAirCraftOnly = ForwardingConsol.RequireCargoAircraftOnly(undg);
				if (showCargoAirCraftOnly)
				{
					break;
				}
			}

			return showCargoAirCraftOnly;
		}

		string GetEgyptHandlingInformation()
		{
			if (DestinationCountryCode == CountryCodes.Egypt)
			{
				var acidNumbers = Consol
					.Numbers
					.Cast<CusEntryNumber>()
					.Where(c => c.CE_RN_NKCountryCode == CountryCodes.Egypt && c.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference)
					.Select(c => c.CE_EntryNum)
					.Distinct();

				if (acidNumbers.Any())
				{
					return (NoResString)"ACID Number:" + string.Join((NoResString)",", acidNumbers);
				}
			}

			return string.Empty;
		}

		protected override ZString OptionalShippingInformation1
		{
			get
			{
				ZString result = string.Empty;
				AddExtraText(FreightDataRegistry.Instance.MAWBOptionalShippingInfoOneExtraText.Value, ref result, FreightDataRegistry.Instance.MAWBOptionalShippingInfoOneExtraText);
				return result;
			}
		}

		protected override ZString OptionalShippingInformation2
		{
			get
			{
				ZString result = string.Empty;
				AddExtraText(FreightDataRegistry.Instance.MAWBOptionalShippingInfoTwoExtraText.Value, ref result, FreightDataRegistry.Instance.MAWBOptionalShippingInfoTwoExtraText);
				return result;
			}
		}

		protected override ZString BillNumber
		{
			get { return EH_AirlinePrefix + EH_AWBSerialNo; }
		}

		protected override ZInt ShippingLoadAndCount
		{
			get
			{
				if (Consol == null)
				{
					return ZInt.Zero;
				}

				if (IsULD)
				{
					var config = AWBRateLineHelper.GetSummarizeULDSLACConfig(Consol.DischargePort?.Country.Code ?? ZString.Empty);
					var shouldUseInners = config != null && (bool)config.UseShipmentInners;

					return shouldUseInners ?
						(ZInt)Consol.ShipmentsForTotalling.Cast<ForwardingShipment>().Sum(shipment => shipment.JS_TotalPackageCount) :
						(ZInt)Consol.ShipmentsForTotalling.Cast<ForwardingShipment>().Sum(shipment => shipment.JS_OuterPacks);
				}

				return SlacHelper.GetShippingLoadAndCount(Consol);
			}
		}

		public override bool ShouldPopulateSlacLine(CommonContainer uldContainer)
		{
			return uldContainer.JC_Calc_TotalPackages > 0;
		}

		public override bool ShouldSuppressSlacLines()
		{
			var config = GetSummarizeULDSLACConfig();

			if (MaximumULDContainerCountForSLACExceeded)
			{
				return true;
			}

			return config != null && (bool)config.TotalSLAC;
		}

		SummarizeULDSLACConfig GetSummarizeULDSLACConfig()
		{
			var configCollection = FreightDataRegistry.Instance.SummarizeULDSLACs.Value;

			var countryCode = Consol?.DischargePort?.Country?.Code;
			if (string.IsNullOrEmpty(countryCode))
			{
				return null;
			}

			return configCollection?.Cast<SummarizeULDSLACConfig>().FirstOrDefault(c => c.DestinationCountry.EqualsIgnoringCase(countryCode));
		}

		bool MaximumULDContainerCountForSLACExceeded
		{
			get { return ULDContainers.Count() > 5; }
		}

		bool IsULD
		{
			get { return Consol != null && Consol.JK_ConsolMode == Core.Constants.ContainerModes.ULD; }
		}

		protected override ZBool IsDomestic
		{
			get { return Consol != null ? Consol.IsDomestic() : ZBool.True; }
		}

		protected override ZString ExtraCarrierInfoLine2
		{
			get
			{
				ZString result = "";
				if (!Env.Registry.Freight.AirWaybill.MAWBDefaultCarrierText.IsNullOrEmpty())
				{
					ZString text = Env.Registry.Freight.AirWaybill.MAWBDefaultCarrierText + ": " + EH_AirlineName;
					result = text.SubstringSafe(0, 64);
				}
				return result;
			}
		}

		const int ExtraShipperInfoLineLength = 64;

		protected override ZString ExtraShipperInfoLine1
		{
			get
			{
				ZString text = Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText;
				return text.SubstringSafe(0, ExtraShipperInfoLineLength);
			}
		}

		protected override ZString ExtraShipperInfoLine2
		{
			get
			{
				ZString text = Env.Registry.Freight.AirWaybill.MAWBDefaultShipperText;
				ZString securityStatusSuffix = ZString.Empty;

				if (SecurityStatusAWBVisibility && IsBorrowedMaster)
				{
					var securityStatus = EH_SecurityStatus == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft
						? (ZString)FreightDataRegistry.AviationSecurity_Unknown_Code
						: EH_SecurityStatus;

					securityStatusSuffix = " " + securityStatus;
				}

				text = text.SubstringSafe(ExtraShipperInfoLineLength, ExtraShipperInfoLineLength - securityStatusSuffix.Length) + securityStatusSuffix;
				return text.Trim();
			}
		}

		public override ZString RegistrationNumber
		{
			get
			{
				return ConsigneeTraderTypeWithNo;
			}
		}

		public override ZString ExtraShipperData
		{
			get { return ShipperTraderTypeWithNo; }
		}

		bool ConsolShowCommunityTransitStatusCode
		{
			get
			{
				if (Consol != null)
				{
					var euProvider = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
					if ((Consol.IsExport() || Consol.IsDomestic())
						&& Consol.LoadPort != null
						&& Consol.LoadPort.Country != null
						&& euProvider.IsCountryEuOrCtCountry(Consol.LoadPort.Country.RN_Code))
					{
						return true;
					}

					if ((GlbCompany.CurrentCompany?.Country?.IsPartOfEuropeanUnion ?? false)
						&& Consol.IsCrossTrade()
						&& Consol.Transports.Cast<Transport>()
							.Any(t => !t.JW_RL_NKLoadPort.IsEmpty && euProvider.IsCountryEuOrCtCountry(t.JW_RL_NKLoadPort.Left(2))))
					{
						return true;
					}
				}

				return false;
			}
		}

		#region EH_AgentApprovedExporterNumber

		public override ZString EH_AgentApprovedExporterNumber
		{
			get
			{
				if (!SupplyChainSecurityConfiguration.AWBRANumber.IsEmpty && SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(Consol))
				{
					return SupplyChainSecurityConfiguration.AWBRANumber;
				}

				if (!SecurityStatusAWBVisibility)
				{
					return ZString.Empty;
				}

				if (IsBorrowedMaster && BorrowedFrom != null )
				{
					if (!OwnAgentApprovedExporterNumber.IsEmpty)
					{
						return EH_SecurityStatus == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft ?
							GetRegulatedAgentOfKnownShipper() : ZString.Empty;
					}
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.HongKong && EH_SecurityStatus == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft)
					{
						return BorrowedFrom.MainAddress?.KnownShipper?.OV_EXApprovalNumber ?? ZString.Empty;
					}
				}

				return new[] { AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft }
					.Contains(EH_SecurityStatus.ToString()) ? OwnAgentApprovedExporterNumber : ZString.Empty;
			}
		}
	
		ZString GetRegulatedAgentOfKnownShipper()
		{
			foreach (OrgAddress address in BorrowedFrom.Addresses)
			{
				if (address?.KnownShipper != null && address.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent)
				{
					return address.KnownShipper.OV_EXApprovalNumber;
				}
			}

			return ZString.Empty;
		}

		ZString OwnAgentApprovedExporterNumber
		{
			get
			{
				if (GlbBranch.CurrentBranch.OrgProxy != null
					&& GlbBranch.CurrentBranch.OrgProxy.MainAddress != null
					&& GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipper != null
					&& GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent)
				{
					return GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipper.OV_EXApprovalNumber;
				}
				else if (GlbCompany.CurrentCompany.OrgProxy != null
					&& GlbCompany.CurrentCompany.OrgProxy.MainAddress != null
					&& GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipper != null
					&& GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent)
				{
					return GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipper.OV_EXApprovalNumber;
				}

				return ZString.Empty;
			}
		}

		#endregion

		public override ZString SpecialHandlingCode
		{
			get
			{
				var result = ZString.Empty;

				if (ConsolShowCommunityTransitStatusCode)
				{
					// AWB box 21a must show most severe of its hawbs' community transit status
					result = Consol.Shipments.Cast<ForwardingShipment>()
						.Select(shipment => shipment.JS_CommunityTransitStatus)
						.Where(cts => !cts.IsEmpty && CusEntryNumberTypes.EU.CommunityTransitStatusCodesList_Export.ContainsCode(cts))
						.OrderBy(cts => cts, new CommunityTransitStatusComparer())
						.FirstOrDefault();
				}

				return result;
			}
		}

		#region Printing Neutral MAWB

		public override ZBool IsPrintingFinalNeutralMAWB { get; set; }

		public override ZBool IsReprintingNeutralMAWB
		{
			get { return Consol != null && Consol.IsNeutralMAWBPrinted; }
		}

		public override ZBool IsPrintingDraftNeutralMAWB
		{
			get
			{
				return (Consol != null && Consol.JK_IsNeutralMaster) &&
						(!IsReprintingNeutralMAWB && !IsPrintingFinalNeutralMAWB);
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = (NoResString)"Master Air Waybill"; // IATA Text

				if (Consol != null)
				{
					result += (NoResString)" for " + Consol.HumanReadableName; // IATA Text
				}

				return result;
			}
		}

		public ZString HAWBNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol != null)
				{
					foreach (CommonShipment shipment in Consol.Shipments)
					{
						if (shipment.ConsigneeDocumentaryAddress != null && shipment.JS_HouseBill != "")
						{
							foreach (Guid countryPK in Enterprise.Registry.Business.FreightDataRegistry.Instance.PrintHawbNumbersInBodyOfMawb.Value)
							{
								var country = Factory.Load<RefCountry>(countryPK);
								if (country != null && shipment.ConsigneeDocumentaryAddress.Country != null && shipment.ConsigneeDocumentaryAddress.Country.RN_Code == country.Code)
								{
									result += shipment.JS_HouseBill + ", ";
									break;
								}
							}
						}
					}

					if (!result.TrimEndIncludingWhiteSpace(',').IsEmpty)
					{
						result = "HAWBS: " + result.TrimEndIncludingWhiteSpace(','); // IATA Text
					}
				}

				return result;
			}
		}

		protected override ZString[] ExportStatements => ExportStatementsCore((ExportStatementSetting _, CommonShipment shipment) => shipment.ExportStatement).Where(value => !value.IsEmpty).ToArray();

		public override List<ExportAWBExportStatement> ExportStatements_CargoIMP =>
			ExportStatementsCore((ExportStatementSetting statementSetting, CommonShipment shipment) => new ExportAWBExportStatement() { Code = statementSetting.Code, Statement = shipment.ExportStatement_CargoIMP });

		List<T> ExportStatementsCore<T>(Func<ExportStatementSetting, CommonShipment, T> resultFunction)
		{
			var result = new List<T>();
			if (Consol != null)
			{
				foreach (CommonShipment shipment in Consol.Shipments)
				{
					if (shipment.IsExport() && shipment.IsAir)
					{
						var statementSetting = shipment.ExportStatementSetting;
						if ((statementSetting != null) && ((DirectShipment == null) ? statementSetting.UseOnConsolidationMawb : statementSetting.UseOnDirectIATAMawb))
						{
							var value = resultFunction(statementSetting, shipment);

							if (!result.Contains(value))
							{
								result.Add(value);
							}
						}
					}
				}
			}
			return result;
		}

		public override bool SecurityStatusAWBVisibility
		{
			get { return SupplyChainSecurityConfiguration.IsEnabled && SupplyChainSecurityConfiguration.ShowSecurityStatusOnMAWB; }
		}

		#region EH_KnownConsignorCode

		public override ZString EH_KnownConsignorCode => SupplyChainSecurityConfiguration.GetMAWBKnownConsignorCode(Consol);

		#endregion

		#region EH_AgentApprovalNumber

		protected override bool EH_AgentApprovalNumber_ReadOnly
		{
			get { return base.EH_AgentApprovalNumber_ReadOnly || !SupplyChainSecurityConfiguration.ExportAWBAgentApprovalNumberCanBeOverridden; }
		}

		#endregion

		#endregion

		#region Validation

		public new ConsolExportAWBHeaderValidation Validation
		{
			get { return (ConsolExportAWBHeaderValidation)base.Validation; }
		}

		protected override ExportAWBHeaderValidation GetNewValidation()
		{
			var validation = new ConsolExportAWBHeaderValidation(this);

			if (!suspendCargoSecurityValidation)
			{
				validation.Add(CargoSecurityValidation);
			}

			foreach (var validator in Validators)
			{
				if (!IsDeleted && validator.IsApplicable() && validator is AutoExportAWBHeaderValidation extraValidation)
				{
					validation.Add(extraValidation);
				}
			}

			return validation;
		}

		bool suspendCargoSecurityValidation;

		internal CargoSecurityExportAWBHeaderValidation CargoSecurityValidation
		{
			get { return new CargoSecurityExportAWBHeaderValidation(this); }
		}

		public IDisposable TemporarilySuspendCargoSecurityValidation()
		{
			suspendCargoSecurityValidation = true;
			UnRegisterEditableChildObject(ExportAWBSecurityStatusLines);

			return new DisposableAction(() =>
			{
				suspendCargoSecurityValidation = false;
				RegisterEditableChildObject(ExportAWBSecurityStatusLines);
				MarkAsNeedingValidation();
			});
		}

		#endregion

		#region Calculation Log Analyzer

		protected override CalculationLogsAnalyzer GetCalculationLogsAnalyzer()
		{
			return new ConsolCalculationLogsAnalyzer(this);
		}

		#endregion

		protected override ZString AWBAgentsSignature
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.HongKong && BorrowedMaster != null && BorrowedMaster.From != null
					&& new[] { AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
						AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft,
						AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly }.Contains(Consol.SecurityStatusCode.ToString()))
				{
					return BorrowedFrom.OH_FullNameTruncated.ToUpper().Left(AgentsSignatureMaxLength);
				}

				return ((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName).Left(AgentsSignatureMaxLength);
			}
		}

		protected override ZString MessageForReplaceMacrosFailed => Res.GetString("EFE6007F-B1CC-41E6-9A27-428216A1BB48", "MAWB cannot be generated.");

		protected override void SetSLACLineToReadOnly(bool readOnly)
		{
			if (IsULD)
			{
				((ExportAWBRateLine)SLACLine).SetReadOnlyIncludingChildren(readOnly);
			}
			else
			{
				base.SetSLACLineToReadOnly(readOnly);
			}
		}

		protected override IEnumerable<ZString> GetDGCodesFromParentBO()
		{
			return (Consol == null) ? Enumerable.Empty<ZString>()
				: GetConsolDGUNNOValues();
		}

		protected override IEnumerable<ZString> GetDGUNNOValues()
		{
			if (Consol == null)
			{
				return Enumerable.Empty<ZString>();
			}

			return GetConsolDGUNNOValues();
		}

		IEnumerable<ZString> GetConsolDGUNNOValues()
		{
			return Consol
				.ShipmentsForTotalling
				.Cast<ForwardingShipment>()
				.SelectMany(shipment => shipment.GetDGUNNOValues())
				.Distinct();
		}

		public override int ShippersSignatureMaxLength
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.Australia ? 20 : base.ShippersSignatureMaxLength; }
		}

		protected override string SupportedTaxDocumentType => mawbDocumentType;

		public override string SwitzerlandDepartureFlightCode
		{
			get
			{
				var code = Consol?.DepartureFlightOriginLoco?.Code ?? ZString.Empty;
				return code.StartsWith(Core.Constants.CountryCodes.Switzerland) ? code : ZString.Empty;
			}
		}

		#region Harmonized Code

		protected override void PopulateHarmonizedCodeType(int rateLineNumber)
		{
			PopulateNatureAndQtyOfGoodsType(rateLineNumber, Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode);
		}

		public override StringCollectionX GetAvailableHarmonisedCodes()
		{
			var result = new StringCollectionX();

			if (Consol != null)
			{
				foreach (CommonShipment shipment in Consol.ShipmentsForTotalling.ToArray())
				{
					var hsCodes = HarmonisedCodeHelper.GetOuterPackLineHarmonisedCodes(shipment, Consol.JK_RL_NKLoadPort.SubstringSafe(0, 2));
					foreach (var hsCode in hsCodes)
					{
						var hsCodeWithLabel = (NoResString)"HS Code: " + hsCode; // Harmonised Code Description
						if (!result.Contains(hsCodeWithLabel))
						{
							result.Add(hsCodeWithLabel);
						}
					}
				}
			}

			return result;
		}

		public override List<ZString> GetShipmentReferencesWithoutHSCode()
		{
			var result = new List<ZString>();

			if (Consol != null)
			{
				foreach (var shipment in Consol.ShipmentsForTotalling.Cast<ForwardingShipment>().Where(s => s.IsHarmonizedCodeMissing(Consol.JK_RL_NKLoadPort)))
				{
					if (shipment.IsHighVolumeLowValue)
					{
						result.Add((NoResString)"HVLV Item Line of " + shipment.JS_UniqueConsignRef);
					}
					else
					{
						result.Add((NoResString)"Packline of " + shipment.JS_UniqueConsignRef);
					}
				}
			}

			return result;
		}

		public override bool HasExtraHSCodes
		{
			get
			{
				var hSCodesCount = AWBRateLines.Cast<ExportAWBRateLine>().Count(e => e.IsHSCodeLine);
				return hSCodesCount < GetAvailableHarmonisedCodes().Count;
			}
		}

		#endregion

		#region DestinationShipperComment

		public override ZString DestinationShipperComment
		{
			get
			{
				if (!EH_ShipperCountryCode.IsEmpty && !EH_ShipperTraderNoType.IsEmpty)
				{
					var regulatingCountryCode = DestinationCountryCode;
					var taxInfos = RequiredTaxNumbers.GetTaxInfoFromRefTable(EH_ShipperCountryCode,
						null, regulatingCountry: regulatingCountryCode);

					var awbTaxInfo = taxInfos.FirstOrDefault(t =>
						t.DocumentType == mawbDocumentType
						&& t.ShortLabel == EH_ShipperTraderNoType);
					return awbTaxInfo?.Comments ?? ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		#endregion
	}
}
