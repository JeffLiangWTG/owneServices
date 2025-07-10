using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business.ExportAWB.ExportAWBHeader.Helpers;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[UniversalCopyIgnoreElement("InvoiceJobHeader")]
	public class ShipmentExportAWBHeader : ExportAWBHeader
	{
		#region Constants

		public new sealed class Constants
		{
			public static class PrepaidCollect1CharCodes
			{
				public const string Both = "B";
			}

			public static class PrepaidCollect3CharCodes
			{
				public const string Both = "BTH";
			}

			Constants() { }
		}

		#endregion

		public ShipmentExportAWBHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			TempRateClasses = new ZString[ExportAWBHeader.Constants.NumberOfRateLines];
		}

		#region Loader

		public static ShipmentExportAWBHeader LoadOrCreate(ForwardingShipment shipment)
		{
			return new AWBHeaderLoader<ShipmentExportAWBHeader, ForwardingShipment>().LoadOrCreate(shipment);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EH_Table = ForwardingShipment.Schema.TableName;
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
				else if (Shipment != null)
				{
					if (!Shipment.IsAir)
					{
						result = SaveMode.Never;
					}
					else
					{
						if (Shipment.OverrideWaybillDefaultsHasChanges && (Shipment.JS_OverrideWaybillDefaults || IsInDatabase)
							|| ShouldSavedHeaderForRateLinesOverridden)
						{
							result = SaveMode.Forced;
						}
						else if (!Shipment.JS_OverrideWaybillDefaults)
						{
							result = SaveMode.Never;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region Lookups

		protected override CodeDescriptionPairList PrepaidCollectCoreList
		{
			get
			{
				CodeDescriptionPairList result = base.PrepaidCollectCoreList;
				result.AddPair(Constants.PrepaidCollect1CharCodes.Both, ResString.GetMultilingualString("b6fcc886-436a-4514-b846-7a4467664dac", "Both"));
				return result;
			}
		}

		#endregion

		#region Related Objects

		public override IAWBParent Parent
		{
			get { return Shipment; }
		}

		public override TypeOfAWB AWBType
		{
			get { return TypeOfAWB.House; }
		}

		public virtual ForwardingShipment Shipment
		{
			get
			{
				if (fShipment != null && fShipment.IsDeleted)
				{
					return null;
				}

				if (fShipment == null)
				{
					fShipment = Factory.Load<ForwardingShipment>(EH_ParentID);
				}

				return fShipment;
			}
		}
		ForwardingShipment fShipment;

		public ForwardingConsol ConsolForAWBLabel { get; set; }

		protected override bool ShouldPopulate
		{
			get { return Shipment != null && Shipment.IsAir && !Shipment.JS_OverrideWaybillDefaults; }
		}

		public override ForwardingConsol Consol
		{
			get
			{
				return Shipment != null
						? MovementLegComparer.FirstOrDefaultLegForTransportMode(Shipment.CandidateConsolsForDepartureArrival.Cast<ForwardingConsol>(), Core.Constants.TransportModes.Air)
						: null;
			}
		}

		public override IEnumerable<CommonContainer> ULDContainers
		{
			get
			{
				var uldContainers = new List<CommonContainer>();
				if (Shipment != null)
				{
					uldContainers.AddRange(Shipment.Containers.Cast<CommonContainer>().Where(x => x.JC_ContainerMode == Core.Constants.ContainerModes.ULD));
				}
				return uldContainers;
			}
		}

		protected override bool ShowExportStatementSetting(ExportStatementSetting mandatorySetting)
		{
			return mandatorySetting.UseOnHawb;
		}

		#region Other Charges

		protected override IEnumerable<OtherChargeTemplate> GetOtherChargeTemplates()
		{
			var charges = HouseBillCharges.GetCharges();

			string GetPrepaidCollect(JobCharge charge)
			{
				if (HouseBillCharges.JobHeaderAtOrigin?.LocalChargesPK == charge.JR_OH_SellAccount)
				{
					return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
				}

				if (HouseBillCharges.JobHeaderAtOrigin?.AgentCollectPK == charge.JR_OH_SellAccount)
				{
					return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
				}

				if (ShipmentINCOTerm != null)
				{
					string prepaidCollect = IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Origin, ShipmentINCOTerm.IncoTermCode);
					return !string.IsNullOrEmpty(prepaidCollect)
						? prepaidCollect == Enterprise.Core.Constants.PaymentType.Collect
							? ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect
							: ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid
						: ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
				}

				return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			}

			return charges.Select(charge => CreateOtherChargeTemplate(charge, GetPrepaidCollect(charge)));
		}

		OtherChargeTemplate CreateOtherChargeTemplate(JobCharge charge, ZString prepaidCollect)
		{
			var template = new OtherChargeTemplate(this);

			template.PrepaidCollect = string.IsNullOrEmpty(prepaidCollect) ? EH_OtherPPDCOL : prepaidCollect;

			template.AccChargeCode = charge.ChargeCode;
			template.ChargeAmountProvider = () => GetSellAmountInHAWBCurrency(charge);
			template.CostAmountProvider = () => GetCostAmountInHAWBCurrency(charge);
			template.ProfitAmountProvider = () => template.ChargeAmount - template.CostAmount;
			template.IATAChargeCodeProvider = () => charge.ChargeCode == null ? ZString.Empty
													: (Consol?.ShippingLine != null ? charge.ChargeCode.GetIATACodeWithFallback(Consol.ShippingLine.PK) : charge.ChargeCode.AC_IATA_ChargeCodeMap);
			template.TaxAmount = GetTaxAmountInHAWBCurrency(charge);
			template.CurrencyProvider = () => charge.JR_RX_NKSellCurrency;

			return template;
		}

		protected override bool GroupChargesByIATACode
		{
			get { return ExportAWBRegistry.Instance.HAWBGroupOtherChargesByIATACode.Value; }
		}

		protected JobCharge[] GetJobCharges()
		{
			return GetJobCharges(InvoiceJobHeader);
		}

		protected override JobCharge[] GetJobCharges(JobHeader invoiceJobHeader)
		{
			var result = new List<JobCharge>();
			result.AddRange(base.GetJobCharges(invoiceJobHeader));

			if (HouseBillCharges.PrintChargesBilledToLocalClientAtDestAsCollect)
			{
				result.Append(HouseBillCharges.GetChargesAtDestination(false));
			}

			return result.ToArray();
		}

		ZString GetChargeType(JobCharge charge, bool includeThirdParty)
		{
			if (InvoiceJobHeader != null && charge != null)
			{
				if (charge.JR_OH_SellAccount == InvoiceJobHeader.AgentCollectPK)
				{
					return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
				}

				if (charge.JR_OH_SellAccount == InvoiceJobHeader.LocalChargesPK || (includeThirdParty && !charge.JR_OH_SellAccount.IsEmpty))
				{
					return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
				}
			}

			return ZString.Empty;
		}

		protected override Forwarding.AWB.Business.ExportAWBOtherChargesCollection GetNewAWBOtherCharges()
		{
			return new ShipmentExportAWBOtherChargesCollection(this);
		}

		#endregion

		#region Accounting Info

		protected override CodeDescriptionPairListRegistryItem ExtraAccountingInfo
		{
			get { return FreightDataRegistry.Instance.HAWBAccountingInfoExtraText; }
		}

		#endregion

		#region Invoicing Job

		public JobHeader InvoiceJobHeader
		{
			get
			{
				JobHeader result = null;
				if (Shipment != null && !Shipment.IsDeleted)
				{
					result = HouseBillCharges.JobHeaderAtOrigin ?? new JobHeader.Loader(Shipment).Load();
				}
				return result;
			}
		}

		#endregion

		#region Rate Lines

		protected override ZString RateClass
		{
			get
			{
				ZString result = ZString.Empty;
				if (EH_WeightBTH)
				{
					result = Shipment.IsPrepaid ? ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid : (Shipment.IsCollect ? ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect : "");
				}
				else
				{
					var freightCharges = GetFreightCharges();
					var freightCharge = (freightCharges.Length == 1) ? freightCharges[0] : null;
					var useMinimumChargeClass = false;

					if (freightCharge != null)
					{
						var paymentBases = GetSellPaymentBases(freightCharge.PK);
						var min = nameof(RateInfo.RateInfoType.MIN);
						useMinimumChargeClass = paymentBases.Any(x => x.RateReference.EqualsIgnoringCase(min));
					}

					if (useMinimumChargeClass)
					{
						result = Core.Constants.AWB.RateClass.MinimumCharge;
					}
					else
					{
						result = base.RateClass;
					}
				}

				return result;
			}
		}

		#region Volume and Dimensions

		protected override bool AllowRecogniseAndUpdateNatureAndQtyOfGoodsTypeFromText
		{
			get { return false; }
		}

		protected override string VolumeAndDimensionsPrintOption
		{
			get { return Shipment?.DocsAndCartage?.JP_PrintOptionForPackagesOnAWB; }
		}

		protected override StringRegistryItem ExtraNatureAndQtyOfGoods
		{
			get { return FreightDataRegistry.Instance.HAWBNatureAndQtyOfGoodsExtraText; }
		}

		void SetVolumeAndDimensionsText(ZString text, bool isText = false)
		{
			if (!EH_AreRateLinesOverridden)
			{
				PopulateNatureAndQtyOfGoodsLine(LineNumberOfFirstEmptyNatureAndQtyOfGoods, text, isText);
			}
		}

		protected override void SetVolumeAndDimensions()
		{
			VolumeAndDimensionForFollowOnPage = ZString.Empty;
			base.SetVolumeAndDimensions();
		}

		void SetVolumeAndDimensionsLinesAndFollowOnPage(IEnumerable<string> dimensionsLines, ZDecimal volume, bool includeBothDimensionsAndVolume = false, bool includeDimensionsEvenIfTheyDontFit = false)
		{
			SetVolumeAndDimensionLines(dimensionsLines, volume, includeBothDimensionsAndVolume, includeDimensionsEvenIfTheyDontFit);
			SetVolumeAndDimensionsForFollowOnPage(dimensionsLines, volume, includeBothDimensionsAndVolume);
		}

		void SetVolumeAndDimensionLines(IEnumerable<string> dimensionLines, ZDecimal volume, bool includeBothDimensionsAndVolume, bool includeDimensionsEvenIfTheyDontFit)
		{
			var setVolumeAndDimensionsParams = new List<(string line, bool isText)>();

			if (dimensionLines.Any() || volume > 0M)
			{
				var dimensionsWillFit = WillFitInFreeSpace(dimensionLines.Count());
				if (dimensionLines.Any() && (includeDimensionsEvenIfTheyDontFit || dimensionsWillFit))
				{
					setVolumeAndDimensionsParams.AddRange(dimensionLines.Select(line => (line, false)));
				}

				if (volume > 0M && (includeBothDimensionsAndVolume || dimensionLines.IsNullOrEmpty() || !dimensionsWillFit))
				{
					var volumeLine = ExportAWBHeader.Constants.VOL + " " + volume.ToString(GetNumberOfDecimalsForUnit()) + " " + Shipment.JS_UnitOfVolume;
					setVolumeAndDimensionsParams.Add((volumeLine, false));
				}
			}
			else
			{
				setVolumeAndDimensionsParams.Add((ExportAWBHeader.Constants.NoDimensionsAvailable, true));
			}

			if (!DoVolumeAndDimensionLinesExistAlready(setVolumeAndDimensionsParams.Select(parameters => parameters.line)))
			{
				setVolumeAndDimensionsParams.ForEach(parameters => SetVolumeAndDimensionsText(parameters.line, parameters.isText));
			}
		}

		bool DoVolumeAndDimensionLinesExistAlready(IEnumerable<string> volumeAndDimensionLines)
		{
			var awbRateLines = AWBRateLines.Cast<ExportAWBRateLine>();

			foreach (var line in volumeAndDimensionLines)
			{
				if (!awbRateLines.Any(rateLine => rateLine.NatureAndQtyOfGoodsText.Text.Equals(line)))
				{
					return false;
				}
			}

			return true;
		}

		void SetVolumeAndDimensionsForFollowOnPage(IEnumerable<string> dimensionLines, ZDecimal volume, bool includeBothDimensionsAndVolume = false)
		{
			if (dimensionLines.Any() || volume > 0M)
			{
				var stringBuilder = new ZStringBuilder(dimensionLines);

				if (volume > 0M && (includeBothDimensionsAndVolume || dimensionLines.IsNullOrEmpty()))
				{
					var text = ExportAWBHeader.Constants.VOL + " " + volume.ToString(GetNumberOfDecimalsForUnit()) + " " + Shipment.JS_UnitOfVolume;
					stringBuilder.Append(text);
				}

				VolumeAndDimensionForFollowOnPage += stringBuilder.ToStringWithDelimiterBetweenAppends("\n");
			}
			else
			{
				VolumeAndDimensionForFollowOnPage += VolumeAndDimensionForFollowOnPage.IsEmpty
					? ExportAWBHeader.Constants.NoDimensionsAvailable
					: "\n" + ExportAWBHeader.Constants.NoDimensionsAvailable;
			}
		}

		public ZString VolumeAndDimensionForFollowOnPage { get; private set; }

		protected override void SetVOLNatureAndQtyOfGoods()
		{
			SetVolumeAndDimensionsLinesAndFollowOnPage(new List<string>(), Shipment?.GetVolumeForDoc(Env.Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay) ?? ZDecimal.Zero);
		}

		protected override void SetPKSNatureAndQtyOfGoods()
		{
			SetVolumeAndDimensionsLinesAndFollowOnPage(GetAvailableDimensions(), 0M, includeDimensionsEvenIfTheyDontFit: true);
		}

		protected override void SetDEFNatureAndQtyOfGoods()
		{
			SetVolumeAndDimensionsLinesAndFollowOnPage(GetAvailableDimensions(), Shipment?.GetVolumeForDoc(Env.Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay) ?? ZDecimal.Zero);
		}

		protected override void SetALLNatureAndQtyOfGoods()
		{
			SetVolumeAndDimensionsLinesAndFollowOnPage(GetAvailableDimensions(), Shipment?.GetVolumeForDoc(Env.Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay) ?? ZDecimal.Zero, includeBothDimensionsAndVolume: true);
		}

		protected override void SetNDANatureAndQtyOfGoods()
		{
			SetVolumeAndDimensionsText(ExportAWBHeader.Constants.NoDimensionsAvailable);
		}

		protected override void PopulateFollowOnAndDimensions()
		{
			if (Shipment?.IsAWBValuesOverriddenProperty ?? ZBool.False)
			{
				SetVolumeAndDimensions();
			}
		}

		int GetNumberOfDecimalsForUnit()
		{
			if (FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.Value)
			{
				int registrySetNumberOfDecimals = FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetNumberOfDecimals(Shipment.TransportMode, Shipment.JS_UnitOfVolume);
				if (registrySetNumberOfDecimals != -1)
				{
					return registrySetNumberOfDecimals;
				}
			}
			return 3;
		}

		IEnumerable<string> GetAvailableDimensions()
		{
			var result = new List<string>();

			if (Shipment != null)
			{
				foreach (PackLine packLine in Shipment.OuterPackLines)
				{
					if (packLine.JL_Width != 0 && packLine.JL_Length != 0 && packLine.JL_Height != 0 && packLine.JL_PackageCount != 0)
					{
						result.Add(GetDimensionText(packLine));
					}
				}
			}

			return result;
		}

		#endregion

		protected override ZString RateLineNoPieces
		{
			get
			{
				ZInt result = Math.Min(Shipment?.JS_OuterPacks ?? ZInt.Zero, 9999);
				return result.ToString();
			}
		}

		protected override ZDecimal RateLineRateChargeOrDiscount
		{
			get { return 0; }
		}

		protected override ZDecimal RateLineTotal
		{
			get
			{
				var freightCharges = GetFreightCharges();
				return freightCharges.Sum((c) => GetSellAmountInHAWBCurrency(c));
			}
		}

		internal ZDecimal GetSellAmountInHAWBCurrency(JobCharge charge)
		{
			return charge.JR_RX_NKSellCurrency != "" && charge.JR_RX_NKSellCurrency == AWBCurrency
				? charge.JR_OSSellAmt
				: GetAmountInHAWBCurrency(new Money(charge.JR_LocalSellAmt, GlbCompany.CurrentCompany.LocalCurrency),
					charge.JR_OH_SellAccount, CostSell.Revenue);
		}

		internal ZDecimal GetCostAmountInHAWBCurrency(JobCharge charge)
		{
			return charge.JR_RX_NKCostCurrency != "" && charge.JR_RX_NKCostCurrency == AWBCurrency
				? charge.JR_OSCostAmt
				: GetAmountInHAWBCurrency(new Money(charge.JR_LocalCostAmt, GlbCompany.CurrentCompany.LocalCurrency),
					charge.JR_OH_CostAccount, CostSell.Cost);
		}

		internal ZDecimal GetTaxAmountInHAWBCurrency(JobCharge charge)
		{
			return charge.JR_RX_NKSellCurrency != "" && charge.JR_RX_NKSellCurrency == AWBCurrency
				? charge.JR_OSSellGSTAmt_Calc
				: GetAmountInHAWBCurrency(new Money(Factory.Load<ICharge>(charge.PK)?.LocalSellGSTAmt ?? charge.JR_Calc_LocalSellTaxAmt, GlbCompany.CurrentCompany.LocalCurrency),
					charge.JR_OH_SellAccount, CostSell.Revenue);
		}

		bool ShouldSplitFreightChargesToPrepaidAndCollect(JobCharge[] prepaidFreightCharges, JobCharge[] collectFreightCharges)
		{
			return prepaidFreightCharges.Length > 0
					&& collectFreightCharges.Length > 0
					&& EH_TotalLineTotals == GetChargesTotalInHAWBCurrency(prepaidFreightCharges) + GetChargesTotalInHAWBCurrency(collectFreightCharges);
		}

		ZDecimal GetChargesTotalInHAWBCurrency(IEnumerable<JobCharge> charges)
		{
			return charges.Sum((c) => GetSellAmountInHAWBCurrency(c));
		}

		internal virtual ZDecimal GetAmountInHAWBCurrency(Money initialMonetaryAmount, ZGuid orgPK, CostSell costOrSell)
		{
			return GetAmountBasedOnCurrencyObject(InvoiceJobHeader, AWBCurrencyObject, initialMonetaryAmount, orgPK, costOrSell);
		}

		protected override ZString RateLineWeightUnit
		{
			get { return Core.Constants.Weight.IsImperial(Shipment?.JS_UnitOfWeight ?? ZString.Empty) ? "L" : "K"; }
		}

		protected override ZString RateLineGrossWeightUnit
		{
			get { return Core.Constants.Weight.IsImperial(Shipment?.JS_UnitOfWeight ?? ZString.Empty) ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms; }
		}

		protected override Forwarding.AWB.Business.ExportAWBRateLineCollection GetNewAWBRateLines()
		{
			return new ShipmentExportAWBRateLineCollection(this);
		}

		protected override ZString AsAgreed1st
		{
			get
			{
				if (IsImportToBrazil
					|| Shipment == null)
				{
					return Core.Constants.AWB.AsAgreedTypes.Codes.None;
				}

				var chargesApply = Shipment.JS_HBLAWBChargesDisplay;
				var (asAgreedFirstSet, _) = ChargesApplyHelper.GetAsAgreedCodesFromChargesApply(chargesApply);
				return asAgreedFirstSet;
			}
		}

		protected override ZString AsAgreed2nd
		{
			get
			{
				if (IsImportToBrazil
					|| Shipment == null)
				{
					return Core.Constants.AWB.AsAgreedTypes.Codes.None;
				}

				var chargesApply = Shipment.JS_HBLAWBChargesDisplay;
				var (_, asAgreedValuesSecondSet) = ChargesApplyHelper.GetAsAgreedCodesFromChargesApply(chargesApply);
				return asAgreedValuesSecondSet;
			}
		}

		IJobPaymentBasis[] GetSellPaymentBases(ZGuid freightChargePK)
		{
			var query = new ZQuery(JobPaymentBasisSchema.PBS_JR, freightChargePK);
			query.AddToFilter(JobPaymentBasisSchema.PBS_IsCost, false);
			return Factory.Load<JobPaymentBasis>(query);
		}

		public JobCharge[] GetFreightCharges()
		{
			return GetFreightCharges(InvoiceJobHeader);
		}

		JobCharge[] GetPrepaidFreightCharges(JobCharge[] jobCharges)
		{
			var freightCharges = GetFreightCharges(jobCharges);
			return freightCharges.Where((c) => GetChargeType(c, true) == ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid).ToArray();
		}

		JobCharge[] GetCollectFreightCharges(JobCharge[] jobCharges)
		{
			var freightCharges = GetFreightCharges(jobCharges);
			return freightCharges.Where((c) => GetChargeType(c, true) == ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect).ToArray();
		}

		protected override FreightTaxes CalculateTotalsForTaxFromFreightCharges()
		{
			var jobCharges = GetJobCharges();

			var prepaidFreightCharges = GetPrepaidFreightCharges(jobCharges);
			var collectFreightCharges = GetCollectFreightCharges(jobCharges);

			return new FreightTaxes(prepaidFreightCharges.Sum(x => GetTaxAmountInHAWBCurrency(x)), collectFreightCharges.Sum(x => GetTaxAmountInHAWBCurrency(x)));
		}

		#endregion

		#endregion

		protected override void PopulateSpecialHandlingItems()
		{
			if (IsAWBOverridden)
			{
				return;
			}

			var defaultCodes = new List<string>();

			if (Consol != null && SecurityStatusAWBVisibility && IsBorrowedMaster)
			{
				Consol.RefreshSecurityStatusCode();
				var securityCode = Consol.SecurityStatusCode;
				if (!securityCode.IsEmpty && securityCode != SecurityJobConsolAWBSpecialHandling.NotSecured)
				{
					defaultCodes.Add(securityCode);
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
		}

		protected override void PopulateShortGoodsDescriptionforFHL()
		{
			var description = Shipment == null || Shipment.JS_GoodsDescription.IsEmpty ? DetailedGoodsDescription : Shipment.JS_GoodsDescription;
			EH_ManifestDescriptionOfGoods = description.Substring(0, Math.Min(description.Length, ExportAWBHeaderSchema.EH_ManifestDescriptionOfGoods.MaxLength));
		}

		protected override AWBActions GetAWBActions() => new ShipmentAWBActions(Shipment, AWBActions.ActionsModeType.None);

		#region Properties

		#region Shipper

		public override ZString EH_ShipperName
		{
			get { return base.EH_ShipperName; }
			set
			{
				base.EH_ShipperName = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllShipperConsigneeFields();
			}
		}

		public override ZString EH_ShipperAddress
		{
			get { return base.EH_ShipperAddress; }
			set
			{
				base.EH_ShipperAddress = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllShipperConsigneeFields();
			}
		}

		public override ZString EH_ShipperPlace
		{
			get { return base.EH_ShipperPlace; }
			set
			{
				base.EH_ShipperPlace = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllShipperConsigneeFields();
			}
		}

		public override ZString EH_ShipperCountryCode
		{
			get { return base.EH_ShipperCountryCode; }
			set
			{
				base.EH_ShipperCountryCode = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllShipperConsigneeFields();
			}
		}

		public override ZString EH_ShipperContactCode
		{
			get { return base.EH_ShipperContactCode; }
			set
			{
				base.EH_ShipperContactCode = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllShipperConsigneeFields();
			}
		}

		public override ZString EH_ShipperContactDetail
		{
			get { return base.EH_ShipperContactDetail; }
			set
			{
				base.EH_ShipperContactDetail = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllShipperConsigneeFields();
			}
		}

		public override ZString EH_ShipperContactEmail
		{
			get
			{
				var addressToUse = (DefaultShipperAddressType == DefaultAddressTypes.Office && ShipperOfficeAddress != null)
					? ShipperOfficeAddress
					: (DefaultShipperAddressType == DefaultAddressTypes.Pickup && ShipperPickupAddress != null)
						? ShipperPickupAddress as IDocAddress
						: ShipperDocumentaryAddress;

				var email = ZString.Empty;
				if (addressToUse is JobDocAddress jobDocAddress && !string.IsNullOrWhiteSpace(jobDocAddress.E2_Email))
				{
					email = jobDocAddress.E2_Email;
				}
				else if (!string.IsNullOrWhiteSpace(addressToUse?.Organisation?.Email))
				{
					email = addressToUse.Organisation.Email;
				}
				return email;
			}
		}

		public override ZString EH_ShipperAddress2
		{
			get { return base.EH_ShipperAddress2; }
			set
			{
				base.EH_ShipperAddress2 = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllShipperConsigneeFields();
			}
		}

		public override ZString EH_ShipperAccount
		{
			get { return base.EH_ShipperAccount; }
			set
			{
				base.EH_ShipperAccount = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllShipperConsigneeFields();
			}
		}

		#endregion

		#region Consignee

		public override ZString EH_ConsigneeName
		{
			get { return base.EH_ConsigneeName; }
			set
			{
				base.EH_ConsigneeName = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllConsigneeFields();
			}
		}

		public override ZString EH_ConsigneeAddress
		{
			get { return base.EH_ConsigneeAddress; }
			set
			{
				base.EH_ConsigneeAddress = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllConsigneeFields();
			}
		}

		public override ZString EH_ConsigneePlace
		{
			get { return base.EH_ConsigneePlace; }
			set
			{
				base.EH_ConsigneePlace = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllConsigneeFields();
			}
		}

		public override ZString EH_ConsigneeCountryCode
		{
			get { return base.EH_ConsigneeCountryCode; }
			set
			{
				base.EH_ConsigneeCountryCode = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllConsigneeFields();
			}
		}

		public override ZString EH_ConsigneeContactCode
		{
			get { return base.EH_ConsigneeContactCode; }
			set
			{
				base.EH_ConsigneeContactCode = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllConsigneeFields();
			}
		}

		public override ZString EH_ConsigneeContactDetail
		{
			get { return base.EH_ConsigneeContactDetail; }
			set
			{
				base.EH_ConsigneeContactDetail = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllConsigneeFields();
			}
		}

		public override ZString EH_ConsigneeContactEmail
		{
			get
			{
				var addressToUse = (DefaultConsigneeAddressType == DefaultAddressTypes.Office && ConsigneeOfficeAddress != null)
					? ConsigneeOfficeAddress
					: (DefaultConsigneeAddressType == DefaultAddressTypes.Delivery && ConsigneeDeliveryAddress != null)
						? ConsigneeDeliveryAddress as IDocAddress
						: ConsigneeDocumentaryAddress;

				var email = ZString.Empty;
				if (addressToUse is JobDocAddress jobDocAddress && !string.IsNullOrWhiteSpace(jobDocAddress.E2_Email))
				{
					email = jobDocAddress.E2_Email;
				}
				else if (!string.IsNullOrWhiteSpace(addressToUse?.Organisation?.Email))
				{
					email = addressToUse.Organisation.Email;
				}
				return email;
			}
		}

		public override ZString EH_ConsigneeAddress2
		{
			get { return base.EH_ConsigneeAddress2; }
			set
			{
				base.EH_ConsigneeAddress2 = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllConsigneeFields();
			}
		}

		public override ZString EH_ConsigneeAccount
		{
			get { return base.EH_ConsigneeAccount; }
			set
			{
				base.EH_ConsigneeAccount = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateAllConsigneeFields();
			}
		}

		#endregion

		#region Notes

		public override ZString AWBRatelineOvertypedNotes
		{
			get
			{
				ZString result = ZString.Empty;
				if (Shipment != null && !Shipment.IsDeleted)
				{
					StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description);
					if (notes.Length > 0)
					{
						result = notes[0].ST_NoteDataAsText;
					}
				}
				return result;
			}
		}

		#endregion

		#region Issued By

		protected override BillIssuedBy GetNewIssuedBy()
		{
			if (ForwardingConfigurationRegistry.Instance.PrintForwarderBranchDetailsHAWBIssuedBySection.Value)
			{
				RefUNLOCO uNLOCO = null;

				if (FreightDataRegistry.Instance.IssuedByDetailsUseShipmentOrigin.Value)
				{
					uNLOCO = Shipment.Origin;
				}
				else
				{
					if (Consol != null)
					{
						foreach (Transport transport in Consol.Transports)
						{
							if (transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1)
							{
								uNLOCO = transport.LoadPort;
							}
						}
					}
				}

				return new BillIssuedBy(uNLOCO, true);
			}

			return new BillIssuedBy((OrgHeader)null, true);
		}

		#endregion

		#region Addrsss Overrides

		#region Consignee Address

		protected override OrgHeader Consignee
		{
			get { return Shipment != null ? Shipment.Consignee : null; }
		}

		protected override JobDocAddress ConsigneeDocumentaryAddress
		{
			get { return (Shipment != null && !Shipment.IsDeleted) ? Shipment.ConsigneeDocumentaryAddress : null; }
		}

		protected override DefaultAddressTypes DefaultConsigneeAddressType
		{
			get
			{
				DefaultAddressTypes result = DefaultAddressTypes.None;
				if (Shipment != null && Shipment.Consignee != null)
				{
					result = TranslateDefaultAddressType(Shipment.Consignee.MiscServ.OM_IMDocumentAddressPreference);
				}
				return result == DefaultAddressTypes.None ? base.DefaultConsigneeAddressType : result;
			}
		}

		protected override OrgAddress ConsigneeOfficeAddress
		{
			get { return Shipment != null && Shipment.Consignee != null ? Shipment.Consignee.Addresses.DefaultAddressOfType(OrgAddressType.Office, false) : null; }
		}

		protected override OrgAddress ConsigneeDeliveryAddress
		{
			get { return Shipment != null && Shipment.Consignee != null ? Shipment.Consignee.Addresses.DefaultAddressOfType(OrgAddressType.Delivery, false) : null; }
		}

		protected override ZString ConsigneeAccount
		{
			get { return Shipment != null && Shipment.Consignee != null ? Shipment.Consignee.OH_Code : ZString.Empty; }
		}

		protected override ZString DefaultConsigneeCompanyName
		{
			get { return Shipment != null && Shipment.Consignee != null ? Shipment.Consignee.OH_FullNameTruncated : ZString.Empty; }
		}

		protected override List<OrgAddress> GetConsigneeAddresses()
		{
			List<OrgAddress> result = new List<OrgAddress>();
			if (Shipment != null && Shipment.Consignee != null)
			{
				foreach (OrgAddress address in Shipment.Consignee.Addresses)
				{
					result.Add(address);
				}
			}
			return result;
		}

		#endregion

		#region Shipper Address

		protected override OrgHeader Shipper
		{
			get { return Shipment != null ? Shipment.Consignor : null; }
		}

		protected override JobDocAddress ShipperDocumentaryAddress
		{
			get { return (Shipment != null && !Shipment.IsDeleted) ? Shipment.ConsignorDocumentaryAddress : null; }
		}

		protected override DefaultAddressTypes DefaultShipperAddressType
		{
			get
			{
				DefaultAddressTypes result = DefaultAddressTypes.None;
				if (Shipper != null)
				{
					result = TranslateDefaultAddressType(Shipment.Consignor.MiscServ.OM_EXDocumentAddressPreference);
				}
				return result == DefaultAddressTypes.None ? base.DefaultShipperAddressType : result;
			}
		}

		protected override OrgAddress ShipperOfficeAddress
		{
			get { return Shipper != null ? Shipper.Addresses.DefaultAddressOfType(OrgAddressType.Office, false) : null; }
		}

		protected override OrgAddress ShipperPickupAddress
		{
			get { return Shipper != null ? Shipper.Addresses.DefaultAddressOfType(OrgAddressType.Pickup, false) : null; }
		}

		protected override ZString DefaultShipperCompanyName
		{
			get { return Shipper != null ? Shipper.OH_FullNameTruncated : ZString.Empty; }
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

		#endregion

		#region Also Notify Address

		internal protected override JobDocAddress NotifyPartyDocumentaryAddress
		{
			get { return Shipment != null ? Shipment.NotifyPartyDocumentaryAddress : null; }
		}

		protected override List<OrgAddress> GetAlsoNotifyAddresses()
		{
			List<OrgAddress> result = new List<OrgAddress>();
			if (Shipment != null && Shipment.NotifyParty != null)
			{
				foreach (OrgAddress address in Shipment.NotifyParty.Addresses)
				{
					result.Add(address);
				}
			}

			result.AddRange(GetConsigneeAddresses());

			return result;
		}

		#endregion

		#endregion

		#region Locations

		protected override ZString OriginCode
		{
			get
			{
				var result = IATAPortCodeFromUNLOCOCode(FirstAirLegOfSeaAirShipment?.LoadPort);

				if (result.IsEmpty)
				{
					result = IATAPortCodeFromUNLOCOCode(OriginLOCO);
				}

				if (result.IsEmpty)
				{
					result = IATAPortCodeFromUNLOCOCode(ConsolOriginLOCO);
				}

				return result;
			}
		}

		protected override ZString AirportOfDeparture
		{
			get
			{
				var result = GetPortNameFromUNLOCO(FirstAirLegOfSeaAirShipment?.LoadPort);

				if (result.IsEmpty)
				{
					result = GetPortNameFromUNLOCO(OriginLOCO);
				}

				if (result.IsEmpty)
				{
					result = GetPortNameFromUNLOCO(ConsolOriginLOCO);
				}

				return result.Left(35);
			}
		}

		Transport FirstAirLegOfSeaAirShipment => Shipment != null && Shipment.IsSeaAir
			? Shipment.TransportsIncludingRelated.FirstLegMatching(leg => leg.IsAir)
			: null;

		protected override ZString AWBDestinationCode
		{
			get
			{
				var result = IATAPortCodeFromUNLOCOCode(LastAirLegOfSeaAirShipment?.DiscPort);

				if (result.IsEmpty)
				{
					result = IATAPortCodeFromUNLOCOCode(DestinationLOCO);
				}

				return result;
			}
		}

		protected override ZString AWBDestinationText
		{
			get
			{
				var result = GetPortNameFromUNLOCO(LastAirLegOfSeaAirShipment?.DiscPort);

				if (result.IsEmpty)
				{
					result = GetPortNameFromUNLOCO(DestinationLOCO);
				}

				return result.Left(35);
			}
		}

		Transport LastAirLegOfSeaAirShipment
		{
			get
			{
				var chain = Shipment?.TransportsIncludingRelated.GetChain(FirstAirLegOfSeaAirShipment);
				if (chain == null)
				{
					return null;
				}

				Transport lastAirLeg = null;
				foreach (var transport in chain)
				{
					if (transport.IsAir)
					{
						lastAirLeg = transport;
					}
					else if (lastAirLeg != null)
					{
						break;
					}
				}
				return lastAirLeg;
			}
		}

		static ZString GetPortNameFromUNLOCO(RefUNLOCO unloco) => unloco?.RL_PortName ?? ZString.Empty;

		public override GlbBranch DeparturePortRelatedBranch
		{
			get
			{
				GlbBranch result = null;
				if (ConsolOriginLOCO != null)
				{
					result = GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, ConsolOriginLOCO, GlbCompany.CurrentCompany);
				}

				return result ?? GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, OriginLOCO, GlbCompany.CurrentCompany);
			}
		}

		protected override RefUNLOCO OriginLOCO
		{
			get { return (Shipment != null && !Shipment.IsDeleted) ? Shipment.Origin : null; }
		}

		protected override RefUNLOCO DestinationLOCO
		{
			get { return (Shipment != null && !Shipment.IsDeleted) ? Shipment.Destination : null; }
		}

		#endregion

		#region AWB Defaults

		protected override ZBool OverrideWaybillDefaults
		{
			get { return (Shipment != null && !Shipment.IsDeleted) ? Shipment.JS_OverrideWaybillDefaults : ZBool.True; }
		}

		#endregion

		#region Currency

		public override ZString EH_Currency
		{
			get { return base.EH_Currency; }
			set
			{
				base.EH_Currency = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateChargeDetails();
			}
		}

		#endregion

		#region EH_WeightPrepaidCollect

		public override ZString EH_WeightPrepaidCollect
		{
			get { return base.EH_WeightPrepaidCollect; }
			set
			{
				if (EH_WeightPrepaidCollect != value)
				{
					ZString oldValue = base.EH_WeightPrepaidCollect;
					base.EH_WeightPrepaidCollect = value;
					EH_RateClassLabelTextInfo.RefreshBinding();
					UpdateRateClasses(oldValue, value);
					((ShipmentExportAWBHeaderValidation)Validation).ValidateChargeDetails();
				}
			}
		}

		void UpdateRateClasses(ZString oldValue, ZString newValue)
		{
			if (oldValue != Constants.PrepaidCollect1CharCodes.Both)
			{
				if (newValue == Constants.PrepaidCollect1CharCodes.Both)
				{
					for (int i = 0; i < AWBRateLines.Count; i++)
					{
						if (!AWBRateLines[i].ER_RateClass.IsEmpty)
						{
							TempRateClasses[i] = AWBRateLines[i].ER_RateClass;
							AWBRateLines[i].ER_RateClass = Shipment.IsPrepaid ? ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid : (Shipment.IsCollect ? ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect : "");
						}
					}
				}
			}
			else
			{
				if (newValue != Constants.PrepaidCollect1CharCodes.Both)
				{
					for (int i = 0; i < AWBRateLines.Count; i++)
					{
						if (!AWBRateLines[i].ER_RateClass.IsEmpty)
						{
							AWBRateLines[i].ER_RateClass = (!TempRateClasses[i].IsEmpty) ? TempRateClasses[i] : RateClass;
						}
					}
				}
			}
		}

		readonly ZString[] TempRateClasses;

		protected override ZString ChargesCode
		{
			get { return EH_WeightPrepaidCollect + EH_OtherPrepaidCollect; }
		}

		public override ZString WeightVPPDCOL
		{
			get
			{
				if (ShipmentINCOTerm != null)
				{
					return GetAWBPrepaidCollect(IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, ShipmentINCOTerm.IncoTermCode));
				}

				if (Shipment != null && Shipment.IsDomestic())
				{
					return GetPPDCOLForDomestic(Shipment.JS_INCO);
				}

				return string.Empty;
			}
		}

		string GetAWBPrepaidCollect(string prepaidCollect)
			=> !string.IsNullOrEmpty(prepaidCollect)
				? prepaidCollect == Enterprise.Core.Constants.PaymentType.Collect
					? ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect
					: ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid
				: string.Empty;

		protected override ZString OtherPPDCOL
		{
			get
			{
				if (ShipmentINCOTerm != null)
				{
					return GetAWBPrepaidCollect(IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Origin, ShipmentINCOTerm.IncoTermCode));
				}

				if (Shipment != null && Shipment.IsDomestic())
				{
					return GetPPDCOLForDomestic(Shipment.JS_INCO);
				}

				return string.Empty;
			}
		}

		IncoTerm ShipmentINCOTerm
		{
			get
			{
				if (Shipment != null && !Shipment.IsDeleted)
				{
					IncoTerm result;
					IncoTermRegistry.TryGetValue(Shipment.JS_INCO, out result);
					return result;
				}
				return null;
			}
		}

		string GetPPDCOLForDomestic(string incoTerm)
		{
			switch (incoTerm)
			{
				case Core.Constants.DomesticPaymentTerms.Prepaid:
					return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
				case Core.Constants.DomesticPaymentTerms.Collect:
				case Core.Constants.DomesticPaymentTerms.CollectCOD:
				case Core.Constants.DomesticPaymentTerms.CollectThirdParty:
					return ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
				default:
					return string.Empty;
			}
		}

		#endregion

		#region EH_WeightPPD

		public override ZBool EH_WeightPPD
		{
			get { return base.EH_WeightPPD || EH_WeightBTH; }
		}

		#endregion

		#region EH_WeightCOL

		public override ZBool EH_WeightCOL
		{
			get { return base.EH_WeightCOL || EH_WeightBTH; }
		}

		#endregion

		#region EH_WeightBTH

		public ZBool EH_WeightBTH
		{
			get { return EH_WeightPrepaidCollect == Constants.PrepaidCollect1CharCodes.Both; }
		}

		#endregion

		#region EH_OtherPrepaidCollect

		public override ZString EH_OtherPrepaidCollect
		{
			get { return base.EH_OtherPrepaidCollect; }
			set
			{
				base.EH_OtherPrepaidCollect = value;
				((ShipmentExportAWBHeaderValidation)Validation).ValidateChargeDetails();
			}
		}

		#endregion

		#region EH_OtherPPD

		public override ZBool EH_OtherPPD
		{
			get { return base.EH_OtherPPD || EH_OtherBTH; }
		}

		#endregion

		#region EH_OtherCOL

		public override ZBool EH_OtherCOL
		{
			get { return base.EH_OtherCOL || EH_OtherBTH; }
		}

		#endregion

		#region EH_OtherBTH

		public ZBool EH_OtherBTH
		{
			get { return EH_OtherPrepaidCollect == Constants.PrepaidCollect1CharCodes.Both; }
		}

		#endregion

		#region EH_RateClassLabelText

		public override ZString EH_RateClassLabelText
		{
			get
			{
				return (EH_WeightVPPDCOL == Constants.PrepaidCollect3CharCodes.Both) ? new ZString(Res.GetString("ShipmentExportAWBHeader|RateClassLabelText", "Prepaid/Collect")) : base.EH_RateClassLabelText;
			}
		}

		#endregion

		#region AWBCurrency

		protected override ZString AWBCurrency
		{
			get
			{
				var currency = AWBCurrencyObject;
				return currency != null ? currency.RX_Code : ZString.Empty;
			}
		}

		RefCurrency AWBCurrencyObject
		{
			get
			{
				RefCurrency currency = null;

				var setCollectInvoiceCurrency = FreightDataRegistry.Instance.SetHAWBCurrencyToFirstCollectInvoiceCurrency.Value;
				var setPrepaidInvoiceCurrency = FreightDataRegistry.Instance.SetHAWBCurrencyToFirstPrepaidInvoiceCurrency.Value;

				if (setCollectInvoiceCurrency || setPrepaidInvoiceCurrency)
				{
					var freightChargeCode = (ZGuid)Env.Registry.FreightChargeCode;
					var header = InvoiceJobHeader;
					var client = header != null ? header.LocalCharges : null;

					if (client != null && !freightChargeCode.IsEmpty)
					{
						var jobCharges = GetJobCharges();
						var charges = Array.FindAll(jobCharges.ToArray(), (c) => c.JR_AC == freightChargeCode && !c.JR_OH_SellAccount.IsEmpty);

						if (setPrepaidInvoiceCurrency)
						{
							currency = GetCurrencyFromInvoice(charges, (c) => c.JR_OH_SellAccount == client.PK);
							if (currency == null)
							{
								currency = GetCurrencyFromCharges(charges, (c) => c.JR_OH_SellAccount == client.PK);
							}
						}

						if (currency == null && setCollectInvoiceCurrency)
						{
							currency = GetCurrencyFromInvoice(charges, (c) => c.JR_OH_SellAccount != client.PK);
							if (currency == null)
							{
								currency = GetCurrencyFromCharges(charges, (c) => c.JR_OH_SellAccount != client.PK);
							}
						}
					}
				}

				return currency ?? GlbCompany.CurrentCompany.LocalCurrency;
			}
		}

		RefCurrency GetCurrencyFromCharges(JobCharge[] charges, Predicate<JobCharge> predicate)
		{
			foreach (var charge in charges)
			{
				if (predicate(charge))
				{
					var newCharge = Factory.Load<ICharge>(charge.PK);

					return (newCharge == null || newCharge.IsBillInLocalCurrency) ? null : charge.SellCurrency;
				}
			}

			return null;
		}

		RefCurrency GetCurrencyFromInvoice(JobCharge[] charges, Predicate<JobCharge> predicate)
		{
			var lines = new List<AccTransactionLines>();
			foreach (var charge in charges)
			{
				if (predicate(charge) && charge.IsRevenuePosted)
				{
					lines.Add(charge.ARLine);
				}
			}

			if (lines.Count > 0)
			{
				lines.Sort(new Comparison<AccTransactionLines>((x, y) => x.AL_PostDate.CompareTo(y.AL_PostDate)));
				var invoice = lines[0].TransactionHeader;

				return invoice != null ? invoice.TransactionCurrency : null;
			}

			return null;
		}

		#endregion

		#region Customs and Insurance Value

		protected override ZDecimal CustomsValue
		{
			get
			{
				ZDecimal result = 0;

				var isDestinedToBangladesh = DestinationCountryCode.Equals(Core.Constants.CountryCodes.Bangladesh);
				var showGoodsValueOnHAWB = FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.Value || isDestinedToBangladesh;
				if (Shipment != null && showGoodsValueOnHAWB)
				{
					result = Shipment.JS_GoodsValue;
				}

				return result;
			}
		}

		protected override ZString CustomsValueCurrency
		{
			get { return Shipment != null ? Shipment.JS_RX_NKGoodsValueCurr : ZString.Empty; }
		}

		protected override ZDecimal InsuranceValue
		{
			get { return Shipment != null ? Shipment.JS_InsuranceValue : (ZDecimal)0M; }
		}

		protected override ZString InsuranceValueCurrency
		{
			get { return Shipment != null ? Shipment.JS_RX_NKInsuranceCurrency : ZString.Empty; }
		}

		#endregion

		#region Customs Entry Numbers

		protected override IEnumerable<EntryNumber> GetCustomsEntryNumbers()
		{
			bool allowLoadCusEntryNumbersForRelatedCountries = GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion
				|| Core.Constants.CountryCodes.IsUsaOrTerritory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				|| GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Australia
				|| GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.HongKong
				|| GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Kenya
				|| GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Switzerland;

			if (!allowLoadCusEntryNumbersForRelatedCountries || Shipment == null)
			{
				return Enumerable.Empty<EntryNumber>();
			}

			List<CusEntryNumber> entryNums;
			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Kenya)
			{
				entryNums = Shipment.CusEntryNumbers.Cast<CusEntryNumber>().ToList();
				var coLoadEntryNums = Shipment.CoLoadShipments.OfType<ForwardingShipment>().SelectMany(s => s.CusEntryNumbers.Cast<CusEntryNumber>()).ToList();
				entryNums.AddRange(coLoadEntryNums);
			}
			else
			{
				entryNums = Shipment.CusEntryNumbers.Cast<CusEntryNumber>().ToList();
			}

			return CreateCustomsEntryNumbers(entryNums);
		}

		protected override ZString CustomsEntryNumber
		{
			get
			{
				if (IsKenyaExport)
				{
					return string.Join(" ", CustomsEntryNumbers.Where(c => c.Type == CusEntryNumberTypes.Standard.ClearancePermitNumber && !c.Number.IsEmpty).Select(c => c.Number));
				}
				else if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.HongKong)
				{
					var entryNumbers = CustomsEntryNumbers.ToList();
					var entryNumberCount = entryNumbers.Count;
					return entryNumberCount == 0 || entryNumberCount > 4 ? string.Empty : $"{entryNumbers.First().Type}: " + ZString.Join(",", entryNumbers.Select(x => x.Number).ToArray());
				}

				return base.CustomsEntryNumber;
			}
		}

		bool IsKenyaExport =>
			GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Kenya
			&& OriginCountryCode == Core.Constants.CountryCodes.Kenya
			&& DestinationCountryCode != Core.Constants.CountryCodes.Kenya;

		#endregion

		#region Other Properties

		public override ZString UniqueReference
		{
			get { return (Shipment != null && !Shipment.IsDeleted) ? Shipment.JS_UniqueConsignRef : ZString.Empty; }
		}

		protected override ZString ReferenceNumber
		{
			get { return Shipment != null ? (NoResString)"HAWB No: " + Shipment.JS_HouseBill : ""; } // IATA Text
		}

		protected override ZString ConsolNumber
		{
			get { return (Consol != null) ? Consol.JK_UniqueConsignRef : ZString.Empty; }
		}

		protected override ZDecimal RateLineChargeableWeight
		{
			get { return Shipment?.GetChargeableForDoc(Env.Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay) ?? ZDecimal.Zero; }
		}

		protected override ZDecimal RateLineGrossWeight
		{
			get
			{
				if (Core.Constants.Weight.ContainsCode(Shipment?.JS_UnitOfWeight ?? ZString.Empty))
				{
					return Core.Constants.Weight.Convert(Shipment.GetWeightForDoc(Env.Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay), Shipment.JS_UnitOfWeight, RateLineGrossWeightUnit);
				}
				return 0m;
			}
		}

		public override ZString GoodsDescription
		{
			get
			{
				ZStringBuilder sb = new ZStringBuilder();

				if (Shipment != null)
				{
					if (!Shipment.DetailedGoodsDescriptionNoteText.IsEmpty)
					{
						sb.Append(Shipment.DetailedGoodsDescriptionNoteText);
					}
					else
					{
						sb.AppendIfNotEmpty(Shipment.JS_GoodsDescription);
					}

					sb.AppendIfNotEmpty(Shipment.JS_MarksAndNumbers);
				}
				return sb.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion

		#region EH_TotalWeightPPD

		public override ZDecimal EH_TotalWeightPPD
		{
			get
			{
				ZDecimal result = 0m;

				if (EH_WeightBTH)
				{
					foreach (ShipmentExportAWBRateLine rateLine in AWBRateLines)
					{
						if (rateLine.ER_RateClass == ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid)
						{
							result += rateLine.ER_Total;
						}
					}
				}
				else
				{
					var jobCharges = GetJobCharges();
					var prepaidFreightCharges = GetPrepaidFreightCharges(jobCharges);
					var collectFreightCharges = GetCollectFreightCharges(jobCharges);

					if (ShouldSplitFreightChargesToPrepaidAndCollect(prepaidFreightCharges, collectFreightCharges))
					{
						result = GetChargesTotalInHAWBCurrency(prepaidFreightCharges);
					}
					else
					{
						result = base.EH_TotalWeightPPD;
					}
				}

				return result;
			}
		}

		#endregion

		#region EH_TotalWeightCOL

		public override ZDecimal EH_TotalWeightCOL
		{
			get
			{
				ZDecimal result = 0m;

				if (EH_WeightBTH)
				{
					foreach (ShipmentExportAWBRateLine rateLine in AWBRateLines)
					{
						if (rateLine.ER_RateClass == ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect)
						{
							result += rateLine.ER_Total;
						}
					}
				}
				else
				{
					var jobCharges = GetJobCharges();
					var prepaidFreightCharges = GetPrepaidFreightCharges(jobCharges);
					var collectFreightCharges = GetCollectFreightCharges(jobCharges);

					if (ShouldSplitFreightChargesToPrepaidAndCollect(prepaidFreightCharges, collectFreightCharges))
					{
						result = GetChargesTotalInHAWBCurrency(collectFreightCharges);
					}
					else
					{
						result = base.EH_TotalWeightCOL;
					}
				}

				return result;
			}
		}

		#endregion

		#region EH_AgentApprovedExporterNumber

		public override ZString EH_AgentApprovedExporterNumber
		{
			get
			{
				if (Shipment != null && SupplyChainSecurityConfiguration.UseConsignorApprovalNumberAsAgentApprovedExporterNumber)
				{
					if (Shipment.ConsignorDocumentaryAddress.Address != null && SupplyChainSecurityConfiguration.IsEnabled)
					{
						if (Shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.CoLoadMaster
							&& Shipment.CoLoadShipments.Cast<CommonShipment>().Any(x => x.AviationSecurity.HasUnknownInspectionTypeCode))
						{
							return FreightDataRegistry.AviationSecurity_Unknown_Code;
						}

						var knownShipperDetails = Shipment.ConsignorDocumentaryAddress.Address.KnownShipper;
						if (knownShipperDetails != null
							&& knownShipperDetails.OV_EXApprovedOrMajorExporter != AviationSecuritySchemeMembershipEx.Codes.No
							&& ((!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(knownShipperDetails.OV_EXApprovedOrMajorExporter) && knownShipperDetails.OV_EXApprovalExpiryDate.IsEmpty)
								|| (knownShipperDetails.OV_EXApprovalExpiryDate.IsValid && knownShipperDetails.OV_EXApprovalExpiryDate >= Shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date)))
						{
							return knownShipperDetails.OV_EXApprovalNumber;
						}
					}

					return ZString.Empty;
				}

				return base.EH_AgentApprovedExporterNumber;
			}
		}

		#endregion

		#region Transports

		protected override ZString HandlingInformation
		{
			get
			{
				ZString shipment3rdFlightInfo = Get3rdFlightInformationString(Shipment != null ? Shipment.Transports : null);
				ZString consol3rdFlightInfo = Get3rdFlightInformationString(Consol != null ? Consol.Transports : null);
				var handlingInfoBuilder = new ZStringBuilder();

				if (!shipment3rdFlightInfo.IsEmpty)
				{
					handlingInfoBuilder.Append(shipment3rdFlightInfo);
				}
				else if (!consol3rdFlightInfo.IsEmpty)
				{
					handlingInfoBuilder.Append(consol3rdFlightInfo);
				}

				if (Shipment != null && !Shipment.IsDeleted)
				{
					Shipment.Notes.ForceReloadRelatedElementsOnNextAccess = true;

					var notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description, true);
					if (notes.Length > 0)
					{
						handlingInfoBuilder.Append(notes[0].ST_NoteDataAsText);
					}

					if (DestinationCountryCode == Core.Constants.CountryCodes.Argentina && !EH_ConsigneeTraderNo.IsEmpty)
					{
						handlingInfoBuilder.Append(ConsigneeTraderTypeWithNo);
					}

					handlingInfoBuilder.AppendIfNotEmpty(GetEgyptHandlingInformation());
					handlingInfoBuilder.AppendIfNotEmpty(Shipment.GetBrazilAWBHandlingInformation());
				}

				ZString result = handlingInfoBuilder.ToStringWithNewLineBetweenAppends();
				AddExtraText(FreightDataRegistry.Instance.HAWBHandlingInformationExtraText.Value, ref result, FreightDataRegistry.Instance.HAWBHandlingInformationExtraText, System.Environment.NewLine);

				return result;
			}
		}

		string GetEgyptHandlingInformation()
		{
			if (DestinationCountryCode == Core.Constants.CountryCodes.Egypt)
			{
				var acidNumbers = Shipment.Numbers
					.Cast<CusEntryNumber>()
					.Where(c => c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Egypt && c.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference)
					.Select(c => c.CE_EntryNum)
					.Distinct();

				if (acidNumbers.Any())
				{
					return Res.GetString("e114e766-994c-40de-b3df-c49a8aa1e08c", "ACID Number:{0}", string.Join(",", acidNumbers));
				}
			}

			return string.Empty;
		}

		#endregion

		#region Flights

		protected override ZString BillNumber
		{
			get { return (Shipment != null && !Shipment.IsDeleted) ? Shipment.JS_HouseBill : ZString.Empty; }
		}

		protected Transport ShipmentDepartureFlight2
		{
			get
			{
				if (Shipment != null && !Shipment.IsDeleted)
				{
					foreach (Transport transport in Shipment.Transports)
					{
						if (transport.JW_TransportMode == Core.Constants.TransportModes.Air && transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight2)
						{
							return transport;
						}
					}
				}

				return null;
			}
		}

		protected override Transport DepartureFlight2
		{
			get { return ShipmentDepartureFlight2 ?? base.DepartureFlight2; }
		}

		protected override Transport DepartureFlight3
		{
			get
			{
				if (Shipment != null && !Shipment.IsDeleted)
				{
					foreach (Transport transport in Shipment.Transports)
					{
						if (transport.JW_TransportMode == Core.Constants.TransportModes.Air && transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight3)
						{
							return transport;
						}
					}
				}

				return ShipmentDepartureFlight2 != null ? null : base.DepartureFlight3;
			}
		}

		protected override ZString OptionalShippingInformation1
		{
			get
			{
				ZString result = string.Empty;
				AddExtraText(FreightDataRegistry.Instance.HAWBOptionalShippingInfoOneExtraText.Value, ref result, FreightDataRegistry.Instance.HAWBOptionalShippingInfoOneExtraText);
				return result;
			}
		}

		protected override ZString OptionalShippingInformation2
		{
			get
			{
				ZString result = string.Empty;
				AddExtraText(FreightDataRegistry.Instance.HAWBOptionalShippingInfoTwoExtraText.Value, ref result, FreightDataRegistry.Instance.HAWBOptionalShippingInfoTwoExtraText);
				return result;
			}
		}

		#endregion

		#region PopulateIssueDate

		protected override void PopulateIssueDate()
		{
			if (Shipment != null && !Shipment.JS_HouseBillIssueDate.IsEmpty)
			{
				EH_AWBIssueDate = Shipment.JS_HouseBillIssueDate;
			}
			else
			{
				base.PopulateIssueDate();
			}
		}

		#endregion

		#region Advance Cargo Reporting Self-Filer

		protected override ZBool IsConsigneeAdvanceCargoReportingSelfFilerSet
		{
			get
			{
				return Shipment != null ? (Shipment.IsDirectShipment ? Shipment.IsAdvanceCargoReportingSelfFiler : Consol != null ? Consol.IsAdvanceCargoReportingSelfFiler : false) : false;
			}
		}

		#endregion

		#region Other Properties

		public override bool IsHAWB => true;

		public override bool IsIndirectHAWB => Consol != null && !Consol.IsDirect;

		protected override ZInt ShippingLoadAndCount
		{
			get
			{
				return SlacHelper.GetShippingLoadAndCount(Shipment, out _);
			}
		}

		protected override ZBool IsDomestic
		{
			get { return Shipment != null ? Shipment.IsDomestic() : ZBool.True; }
		}

		protected override ZString ExtraCarrierInfoLine2
		{
			get
			{
				ZString result = "";
				if (!string.IsNullOrEmpty(Env.Registry.Freight.AirWaybill.HAWBDefaultCarrierText))
				{
					ZString text = Env.Registry.Freight.AirWaybill.HAWBDefaultCarrierText + ": " + EH_AirlineName;
					result = text.SubstringSafe(0, 64);
				}
				return result;
			}
		}

		protected override ZString ExtraShipperInfoLine1
		{
			get
			{
				ZString text = Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText;
				return text.SubstringSafe(0, 64);
			}
		}

		protected override ZString ExtraShipperInfoLine2
		{
			get
			{
				ZString text = Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText;
				var securityStatusSuffix = ZString.Empty;

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

		public override bool SecurityStatusAWBVisibility
		{
			get { return SupplyChainSecurityConfiguration.IsEnabled && SupplyChainSecurityConfiguration.ShowSecurityStatusOnHAWB; }
		}

		const int ExtraShipperInfoLineLength = 64;

		public override ZString RegistrationNumber
		{
			get
			{
				return ConsigneeTraderTypeWithNo;
			}
		}

		public override ZString ExtraShipperData
		{
			get
			{
				var hongKong = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.HongKong);
				var resultBuilder = new ZStringBuilder();

				if (Shipment != null && Shipment.Consignor != null)
				{
					if (Shipment.Consignor.CountryCode == Core.Constants.CountryCodes.HongKong)
					{
						var customeCode = Shipment.Consignor.CustomsCodes.GetCustomsRegNo(OrgCusCode.HKCodeTypes.KnownConsignorNumber, hongKong);

						if (!customeCode.IsEmpty)
						{
							resultBuilder.Append("KC:"); // Customs Code
							resultBuilder.Append(customeCode);
						}
					}
				}

				resultBuilder.AppendIfNotEmpty(ShipperTraderTypeWithNo);

				return resultBuilder.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		bool ShipmentShowCommunityTransitStatusCode
		{
			get
			{
				if (Shipment != null)
				{
					var euProvider = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
					if ((Shipment.IsExport() || Shipment.IsDomestic())
						&& !Shipment.JS_RL_NKOrigin.IsEmpty
						&& euProvider.IsCountryEuOrCtCountry(Shipment.JS_RL_NKOrigin.Left(2)))
					{
						return true;
					}

					if ((GlbCompany.CurrentCompany?.Country?.IsPartOfEuropeanUnion ?? false)
						&& Shipment.IsCrossTrade()
						&& Shipment.TransportsIncludingRelated.Cast<Transport>()
							.Any(t => !t.JW_RL_NKLoadPort.IsEmpty && euProvider.IsCountryEuOrCtCountry(t.JW_RL_NKLoadPort.Left(2))))
					{
						return true;
					}
				}

				return false;
			}
		}

		public override ZString SpecialHandlingCode
		{
			get
			{
				var result = ZString.Empty;

				if (ShipmentShowCommunityTransitStatusCode)
				{
					if (CusEntryNumberTypes.EU.CommunityTransitStatusCodesList_Export.ContainsCode(Shipment.JS_CommunityTransitStatus))
					{
						result = Shipment.JS_CommunityTransitStatus;
					}
				}

				return result;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = (NoResString)"House Air Waybill"; // IATA Text

				if (Shipment != null)
				{
					result += (NoResString)" for " + Shipment.HumanReadableName; // IATA Text
				}

				return result;
			}
		}

		protected override ZString Get3CharPrepaidCollectCode(ZString shortCode)
		{
			ZString result = shortCode;
			switch (shortCode)
			{
				case ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid:
					result = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
					break;

				case ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect:
					result = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
					break;

				case ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both:
					result = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
					break;
			}

			return result;
		}

		protected override ExportAWBHeaderValidation GetNewValidation()
		{
			var validation = new ShipmentExportAWBHeaderValidation(this);
			foreach (var validator in Validators)
			{
				if (!IsDeleted && validator.IsApplicable() && validator is AutoExportAWBHeaderValidation extraValidation)
				{
					validation.Add(extraValidation);
				}
			}
			return validation;
		}

		protected override ZString[] ExportStatements => ExportStatementsCore((ExportStatementSetting _) => Shipment.ExportStatement).Where(value => !value.IsEmpty).ToArray();

		public override List<ExportAWBExportStatement> ExportStatements_CargoIMP =>
			ExportStatementsCore((ExportStatementSetting statementSetting) => new ExportAWBExportStatement() { Code = statementSetting.Code, Statement = Shipment.ExportStatement_CargoIMP });

		List<T> ExportStatementsCore<T>(Func<ExportStatementSetting, T> resultFunction)
		{
			var result = new List<T>();
			var statementSetting = Shipment?.ExportStatementSetting;
			if (statementSetting != null && statementSetting.UseOnHawb)
			{
				result.Add(resultFunction(statementSetting));
			}

			return result;
		}

		#endregion

		#endregion

		#region Calculation Log Analyzer

		protected override CalculationLogsAnalyzer GetCalculationLogsAnalyzer()
		{
			return new ShipmentCalculationLogsAnalyzer(this);
		}

		#endregion

		public override ZString HouseBill
		{
			get
			{
				ZString houseBill = (Shipment != null) ? Shipment.JS_HouseBill : ZString.Empty;

				if (houseBill.Length > 12 && houseBill.StartsWith(CommonShipment.PreAllocatedHouseBillPrefix, StringComparison.Ordinal))
				{
					return houseBill.SubstringSafe(3);
				}

				return houseBill;
			}
		}

		protected override ZString MessageForReplaceMacrosFailed => Res.GetString("1FE80E13-098E-450A-85BD-4AF6F060AD5A", "HAWB cannot be generated.");

		protected override IEnumerable<ZString> GetDGCodesFromParentBO()
		{
			return Shipment.GetDGUNNOValues();
		}

		protected override IEnumerable<ZString> GetDGUNNOValues()
		{
			return Shipment.GetDGUNNOValues();
		}

		public override ZString DetailedGoodsDescription
		{
			get { return Shipment?.DetailedGoodsDescriptionNoteText ?? ZString.Empty; }
		}

		protected override TaxCodeInformation GetTaxCodeInformationForBangladesh(List<TaxCodeInformation> taxInfos)
		{
			if (taxInfos.IsNullOrEmpty())
			{
				return null;
			}

			return Consol == null || IsIndirectHAWB
				? taxInfos.FirstOrDefault(t => t.Code == OrgCusCode.CodeTypes.VATCode)
				: null;
		}

		public override bool IsImportToBangladesh => IsImportToCountry(CountryCodes.Bangladesh);

		public override bool IsImportToBrazil => IsImportToCountry(CountryCodes.Brazil);

		public override bool IsImportToChina => IsImportToCountry(CountryCodes.China);

		public override bool IsBolivianNITRequired => Consol == null || IsIndirectHAWB;

		public override bool IsHondurasRTNRequired => IsImportToHonduras && (Consol == null || IsIndirectHAWB);

		protected override bool IsImportToCountry(ZString countryCode)
		{
			return Shipment != null && Shipment.IsImportTo(countryCode);
		}

		protected override bool IsExportFromCountry(ZString countryCode)
		{
			return Shipment != null && Shipment.IsExportFrom(countryCode);
		}

		public override bool IsTransitingThroughChina => IsTransitingThrough(Core.Constants.CountryCodes.China);

		protected override bool IsTransitingThrough(ZString countryCode)
		{
			return Shipment != null
					&& (!Shipment.JS_RL_NKOrigin.StartsWith(countryCode, StringComparison.Ordinal) && !Shipment.JS_RL_NKDestination.StartsWith(countryCode, StringComparison.Ordinal))
					&& (Shipment.TransportsIncludingRelated.IsAnyDischargeInCountry(countryCode) || Shipment.TransportsIncludingRelated.IsAnyLoadPortInCountry(countryCode));
		}

		#region Goods Declaration Reference Number

		public override ZString ShipmentNumberWithEmptyHSCode
		{
			get
			{
				if (Shipment == null || Shipment.Consols.IsNullOrEmpty() || Shipment.Consols.Cast<ForwardingConsol>().First().IsDirect)
				{
					return ZString.Empty;
				}

				return Shipment.IsAnyPacklineHSCodeEmpty ? Shipment.JS_UniqueConsignRef : ZString.Empty;
			}
		}

		protected override IEnumerable<GoodsDeclarationReferenceNumber> GetGoodsDeclarationReferenceNumbers()
		{
			if (Shipment == null)
			{
				yield break;
			}

			var goodsDeclarationReferenceNumber = CreateGoodsDeclarationReferenceNumber(Shipment);

			if (goodsDeclarationReferenceNumber != null)
			{
				yield return goodsDeclarationReferenceNumber;
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

		#endregion

		#region Movement Reference Numbers

		protected override IEnumerable<MovementReferenceNumber> GetMovementReferenceNumbers()
		{
			if (Shipment == null || !GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion)
			{
				yield break;
			}

			var number = CreateMovementReferenceNumber(Shipment);

			if (number != null)
			{
				yield return number;
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

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Italy
				&& shipment.Origin != null
				&& shipment.Origin.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Italy
				&& shipment.IsExport())
			{
				movementReferenceNumber.CommunityTransitStatusCode = shipment.JS_CommunityTransitStatus;
			}

			if (!shipment.JS_HouseBill.IsEmpty)
			{
				movementReferenceNumber.RelatedNumbers.Add(new EntryNumber
				{
					Type = RelatedMovementReferenceNumberType.Codes.HouseWaybillNumber,
					Number = shipment.JS_HouseBill
				});
			}

			if (Shipment.JS_PackingMode == Core.Constants.ContainerModes.ULD)
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

		#region HSCodeValidation

		public override bool HasInboundToICS2Zone
		{
			get
			{
				return Shipment?.TransportsIncludingRelated.IsAirImportOrTransitToICS2Zone ?? false;
			}
		}

		#endregion

		#region EORIValidation

		public override bool IsImportToICS2Zone
		{
			get
			{
				return Shipment != null && Shipment.TransportsIncludingRelated != null
					&& Shipment.TransportsIncludingRelated.IsAirImportOrTransitToICS2Zone && !IsOriginInIcs2Zone
					&& HasDestinationInIcs2Zone;
			}
		}

		public bool IsOriginInIcs2Zone
		{
			get
			{
				return OriginLOCO?.IsInIcs2Zone ?? false;
			}
		}

		#endregion

		protected override string SupportedTaxDocumentType => hawbDocumentType;

		#region Harmonized Code

		protected override StringCollectionX GetFormattedHarmonisedCodes()
		{
			var result = new StringCollectionX();

			var availableHSCodes = GetAvailableHarmonisedCodes();
			if (availableHSCodes.Count > 0)
			{
				var codes = string.Join(", ", availableHSCodes.ToArray());
				result.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)"HS Codes: {0}", codes)); // Harmonised Code Description
			}

			return result;
		}

		public override StringCollectionX GetAvailableHarmonisedCodes()
		{
			var result = new StringCollectionX();

			if (Shipment != null)
			{
				result.AddRange(HarmonisedCodeHelper.GetOuterPackLineHarmonisedCodes(Shipment, Shipment.JS_RL_NKOrigin.SubstringSafe(0, 2)));
			}

			return result;
		}

		public override List<ZString> GetShipmentReferencesWithoutHSCode()
		{
			var result = new List<ZString>();

			if (Shipment != null && Shipment.IsHarmonizedCodeMissing(Shipment.JS_RL_NKOrigin))
			{
				if (Shipment.IsHighVolumeLowValue)
				{
					result.Add((NoResString)"HVLV Item Line of " + Shipment.JS_UniqueConsignRef);
				}
				else
				{
					result.Add((NoResString)"Packline of " + Shipment.JS_UniqueConsignRef);
				}
			}

			return result;
		}

		public override bool HasExtraHSCodes
		{
			get
			{
				var hsCodeLines = AWBRateLines.Cast<ExportAWBRateLine>().Where(e => e.IsHSCodeLine);
				var hsCodeCount = 0;

				foreach (var line in hsCodeLines)
				{
					var codes = line.NatureAndQtyOfGoods.Text.Replace("HS Codes: ", "");
					if (codes.EndsWith(","))
					{
						codes = codes.Substring(0, codes.Length - 1);
					}

					hsCodeCount += codes.Split(',').Length;
				}

				return hsCodeCount < GetAvailableHarmonisedCodes().Count;
			}
		}

		#endregion

		#region Issuing Agent Name

		protected override ZString GetIssuingAgentName(int maxLength)
		{
			return GetUsernamePlusDgn(GlbStaff.CurrentUser, maxLength);
		}

		#endregion

		protected override ZString AWBAgentsSignature => ((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName).Left(ShippersSignatureMaxLength);

		protected HouseBillCharges HouseBillCharges => houseBillCharges ??= new (Shipment);
		HouseBillCharges houseBillCharges;

		public override string SwitzerlandDepartureFlightCode
		{
			get
			{
				return Shipment?.Consols
					.OfType<ForwardingConsol>()
					.Where(c => c.DepartureFlightOriginLoco != null
								&& c.DepartureFlightOriginLoco.Code.StartsWith(Core.Constants.CountryCodes.Switzerland)
								&& !c.Transports.MostInterestingTransport.JW_ETD.IsEmpty)
					.OrderByDescending(c => c.Transports.MostInterestingTransport.JW_ETD)
					.LastOrDefault()?.DepartureFlightOriginLoco?.Code ?? ZString.Empty;
			}
		}
	}

	internal static class ShipmentExtension
	{
		internal static IEnumerable<ZString> GetDGUNNOValues(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return Enumerable.Empty<ZString>();
			}

			var zStrings = shipment
				.OuterPackLines
				.Cast<ForwardingPackLine>()
				.SelectMany(packline => packline.UNDGs)
				.Where(dgLine => dgLine.Substance != null)
				.Select(dgLine => dgLine.Substance)
				.Select(substance => new ZString(substance.GetUnnoPrefix() + substance.DG_UNNO.Trim()));

			if (shipment.IsHighVolumeLowValue)
			{
				foreach (var item in shipment.HVLVItems)
				{
					zStrings = zStrings.Concat(item.DGUNNOValues());
				}
			}

			return zStrings.Distinct();
		}

		internal static IEnumerable<ZString> GetDGCodes(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return Enumerable.Empty<ZString>();
			}

			var zStrings = shipment
				.OuterPackLines
				.Cast<ForwardingPackLine>()
				.SelectMany(packline => packline.UNDGs)
				.Where(dgLine => dgLine.Substance != null)
				.Select(dgLine => dgLine.Substance)
				.Select(substance => new ZString(substance.GetUnnoPrefix() + substance.DG_Code.Trim()));

			if (shipment.IsHighVolumeLowValue)
			{
				foreach (var item in shipment.HVLVItems)
				{
					zStrings = zStrings.Concat(item.DGCodes);
				}
			}

			return zStrings.Distinct();
		}

		internal static bool IsHarmonizedCodeMissing(this ForwardingShipment shipment, ZString loadPort)
		{
			if (shipment.IsHighVolumeLowValue && shipment.HVLVConsignmentHeader != null)
			{
				var itemLines = shipment.Factory.Load<IHVLVItemLine>(new ZQuery(HVLVItemLineSchema.HVS_ClusterKey, shipment.HVLVConsignmentHeader.HCH_ClusterKey));
				return !itemLines.Any()
					|| itemLines.Any(p => p.HVS_DestinationTariff.IsEmpty);
			}

			var packLines = shipment.OuterPackLines.Cast<PackLine>().ToArray();
			return !packLines.Any()
				|| packLines.Any(p =>
				{
					var hsCode = p.HarmonisedCodes
						.FirstOrDefault(h => h.JLH_RN_NKCountry == loadPort.SubstringSafe(0, 2))?.JLH_Code
						?? p.JL_HarmonisedCode;
					return string.IsNullOrEmpty(hsCode);
				});
		}
	}
}
