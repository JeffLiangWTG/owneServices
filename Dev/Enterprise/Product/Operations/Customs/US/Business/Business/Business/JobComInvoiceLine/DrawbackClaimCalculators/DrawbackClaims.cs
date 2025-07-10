using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackClaims
	{
		public DrawbackClaims(JobComInvoiceLine drawbackJobComInvoiceLine)
		{
			this.drawbackLine = drawbackJobComInvoiceLine;
			DutyClaim = new DrawbackDutyClaim(this);

			HMFClaim = new DrawbackHMFClaim(this);
			IRTaxClaim = new DrawbackIRTaxClaim(this);
			MPFClaim = new DrawbackMPFClaim(this);
			OtherFeesClaim = new DrawbackOtherFeesClaim(this);
		}
		readonly JobComInvoiceLine drawbackLine;

		public DrawbackClaims(DrawbackOtherFee aceDrawbackOtherFee)
		{
			this.drawbackLine = aceDrawbackOtherFee.Parent;
			this.aceDrawbackOtherFee = aceDrawbackOtherFee;
			ACEOtherFeeClaim = new ACEDrawbackOtherFeeClaim(this);
		}
		readonly DrawbackOtherFee aceDrawbackOtherFee;

		public JobComInvoiceLine DrawbackLine
		{
			get { return drawbackLine; }
		}

		public DrawbackOtherFee ACEDrawbackOtherFee
		{
			get { return aceDrawbackOtherFee; }
		}

		public IDrawbackEntryLine DeclaredLine
		{
			get { return cachedDeclaredLine == null ? GetDeclaredLine() : cachedDeclaredLine.Value; }
		}

		IDrawbackEntryLine GetDeclaredLine() => !DrawbackLine.IsBOMLineExpanded ? DrawbackLine.DrawbackImportEntryLine : null;
		IDisposable TemporaryCacheDeclaredLine() => new DisposableAction(() => cachedDeclaredLine = new CachedValue<IDrawbackEntryLine>(GetDeclaredLine), () => cachedDeclaredLine = null);
		CachedValue<IDrawbackEntryLine> cachedDeclaredLine;

		#region IsOverriden

		ZBool IsOverriden
		{
			get { return DrawbackLine.US_DRWClaimAmountOverriden_New; }
		}

		#endregion

		bool UseCaptureData => !DrawbackLine.US_DRWOldData;

		#region Quantity & UQs

		public ZDecimal ImportQuantity
		{
			get { return DrawbackLine.US_DRWImportQuantity; }
			set { DrawbackLine.US_DRWImportQuantity = value; }
		}

		public ZString ImportUQ
		{
			get { return DrawbackLine.US_DRWImportUQ; }
			set { DrawbackLine.US_DRWImportUQ = value; }
		}

		public ZDecimal ImportQuantity2
		{
			get { return DrawbackLine.US_DRWImportQuantity2; }
			set { DrawbackLine.US_DRWImportQuantity2 = value; }
		}

		public ZString ImportUQ2
		{
			get { return DrawbackLine.US_DRWImportUQ2; }
			set { DrawbackLine.US_DRWImportUQ2 = value; }
		}

		public ZDecimal ImportQuantity3
		{
			get { return DrawbackLine.US_DRWImportQuantity3; }
			set { DrawbackLine.US_DRWImportQuantity3 = value; }
		}

		public ZString ImportUQ3
		{
			get { return DrawbackLine.US_DRWImportUQ3; }
			set { DrawbackLine.US_DRWImportUQ3 = value; }
		}

		public ZDecimal ExportQuantity
		{
			get { return DrawbackLine.US_DRWExportQuantity; }
			set { DrawbackLine.US_DRWExportQuantity = value; }
		}

		public ZString ExportUQ
		{
			get { return DrawbackLine.US_DRWExportUQ; }
			set { DrawbackLine.US_DRWExportUQ = value; }
		}

		public ZDecimal UsedQuantity
		{
			get { return DrawbackLine.US_DRWQuantityUsed > 0 ? DrawbackLine.US_DRWQuantityUsed : DrawbackLine.US_DRWExportQuantity; }
			set { DrawbackLine.US_DRWQuantityUsed = value; }
		}

		public ZDecimal AllowableQTY
		{
			get { return DrawbackLine.US_DRWAllowQty; }
			set { DrawbackLine.US_DRWAllowQty = value; }
		}

		public ZDecimal AllowableQTY2
		{
			get { return DrawbackLine.US_DRWAllowQty2; }
			set { DrawbackLine.US_DRWAllowQty2 = value; }
		}

		public ZDecimal AllowableQTY3
		{
			get { return DrawbackLine.US_DRWAllowQty3; }
			set { DrawbackLine.US_DRWAllowQty3 = value; }
		}

		public ZDecimal GoodsValuePerUnit
		{
			get
			{
				return GetOverrideData(ref goodsValuePerUnitCached, () => DrawbackLine.US_DRWValuePerUQ, GetDefaultGoodsValuePerUnit);
			}
			set { DrawbackLine.US_DRWValuePerUQ = value; }
		}
		CachedProperty<ZDecimal> goodsValuePerUnitCached;

		public ZDecimal GoodsValuePerUnit2
		{
			get
			{
				return GetOverrideData(ref goodsValuePerUnit2Cached, () => DrawbackLine.US_DRWValuePerUQ2, GetDefaultGoodsValuePerUnit2);
			}
			set { DrawbackLine.US_DRWValuePerUQ2 = value; }
		}
		CachedProperty<ZDecimal> goodsValuePerUnit2Cached;

		public ZDecimal GoodsValuePerUnit3
		{
			get
			{
				return GetOverrideData(ref goodsValuePerUnit3Cached, () => DrawbackLine.US_DRWValuePerUQ3, GetDefaultGoodsValuePerUnit3);
			}
			set { DrawbackLine.US_DRWValuePerUQ3 = value; }
		}
		CachedProperty<ZDecimal> goodsValuePerUnit3Cached;

		public ZDecimal SubstitutedValuePerUnit
		{
			get { return IsOverriden ? DrawbackLine.US_DRWSubstituted : ZDecimal.Zero; }
			set { DrawbackLine.US_DRWSubstituted = value; }
		}

		public ZDecimal SubstitutedValuePerUnit2
		{
			get { return IsOverriden ? DrawbackLine.US_DRWSubstituted2 : ZDecimal.Zero; }
			set { DrawbackLine.US_DRWSubstituted2 = value; }
		}

		public ZDecimal SubstitutedValuePerUnit3
		{
			get { return IsOverriden ? DrawbackLine.US_DRWSubstituted3 : ZDecimal.Zero; }
			set { DrawbackLine.US_DRWSubstituted3 = value; }
		}

		#endregion
		public void DefaultOverrideData()
		{
			using (TemporaryCacheDeclaredLine())
			{
				if (ACEDrawbackOtherFee == null)
				{
					HMFClaim.DefaultWeightedRatio();
					MPFClaim.DefaultWeightedRatio();
					DrawbackLine.US_DRWLineDuty = DeclaredLine?.TotalDutyIncludingSecondaryLines ?? ZDecimal.Zero;
					DutyClaim.DefaultOverrideData();
					DutyClaim.DefaultDutyRateDesc();
					IRTaxClaim.DefaultOverrideData();
					HMFClaim.DefaultOverrideData();
					MPFClaim.DefaultOverrideData();
					OtherFeesClaim.DefaultOverrideData();
					DefaultGoodsValuePerUnit2();
					DefaultGoodsValuePerUnit3();
					DrawbackLine.DrawbackOtherFees.Cast<DrawbackOtherFee>().ForEach(x => x.Claims.DefaultOverrideData());
				}
				else
				{
					ACEOtherFeeClaim.DefaultOverrideData();
				}
			}
		}

		public void Default_99ClaimedDutyAndCalculatedAmount()
		{
			if (ACEDrawbackOtherFee == null)
			{
				DutyClaim.Default_99ClaimedDutyAndCalculatedAmount();
				IRTaxClaim.Default_99ClaimedDutyAndCalculatedAmount();
				MPFClaim.Default_99ClaimedDutyAndCalculatedAmount();
				HMFClaim.Default_99ClaimedDutyAndCalculatedAmount();
			}
			else
			{
				ACEOtherFeeClaim.Default_99ClaimedDutyAndCalculatedAmount();
			}
		}

		public void DefaultGoodsValuePerUnit()
		{
			var value = GetDefaultGoodsValuePerUnit();
			if (DrawbackLine.US_DRWValuePerUQ != value)
			{
				DrawbackLine.US_DRWValuePerUQ = value;
			}
		}

		ZDecimal GetDefaultGoodsValuePerUnit() => DrawbackLine.DutyPerUnit.Truncate(4);

		public void DefaultGoodsValuePerUnit2()
		{
			var value = GetDefaultGoodsValuePerUnit2();
			if (DrawbackLine.US_DRWValuePerUQ2 != value)
			{
				DrawbackLine.US_DRWValuePerUQ2 = value;
			}
		}
		ZDecimal GetDefaultGoodsValuePerUnit2() => GetRoundedRatio(DrawbackLine.DRWImportQuantity2, DrawbackLine.US_DRWDeclaredVFD);

		public void DefaultGoodsValuePerUnit3()
		{
			var value = GetDefaultGoodsValuePerUnit3();
			if (DrawbackLine.US_DRWValuePerUQ3 != value)
			{
				DrawbackLine.US_DRWValuePerUQ3 = value;
			}
		}
		ZDecimal GetDefaultGoodsValuePerUnit3() => GetRoundedRatio(DrawbackLine.DRWImportQuantity3, DrawbackLine.US_DRWDeclaredVFD);

		ZDecimal GetRoundedRatio(ZDecimal quantity, ZDecimal declaredVFD) => quantity > ZDecimal.Zero ? ((ZDecimal)(declaredVFD / quantity)).Truncate(4) : ZDecimal.Zero;

		public readonly DrawbackDutyClaim DutyClaim;
		public readonly DrawbackHMFClaim HMFClaim;
		public readonly DrawbackIRTaxClaim IRTaxClaim;
		public readonly DrawbackMPFClaim MPFClaim;
		public readonly DrawbackOtherFeesClaim OtherFeesClaim;
		public readonly ACEDrawbackOtherFeeClaim ACEOtherFeeClaim;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected TData GetOverrideDataGreaterThanOrEqualDefault<TData>(ref CachedProperty<TData> cachedProperty, Func<TData> getOverrideData, Func<TData> getOriginalData)
			where TData : INumericZType
		{
			return DrawbackLine.Factory.GetValue(ref cachedProperty, () => GetOverrideData(getOverrideData, getOriginalData, (x) => x.CompareTo(x.Default) >= 0));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected TData GetOverrideDataGreaterThanDefault<TData>(ref CachedProperty<TData> cachedProperty, Func<TData> getOverrideData, Func<TData> getOriginalData, bool shouldCheckOverrideData = false)
			where TData : INumericZType
		{
			return DrawbackLine.Factory.GetValue(ref cachedProperty, () => GetOverrideData(getOverrideData, getOriginalData, (x) => x.CompareTo(x.Default) > 0, shouldCheckOverrideData));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected TData GetOverrideData<TData>(ref CachedProperty<TData> cachedProperty, Func<TData> getOverrideData, Func<TData> getOriginalData)
			where TData : INumericZType
		{
			return DrawbackLine.Factory.GetValue(ref cachedProperty, () => GetOverrideData(getOverrideData, getOriginalData, (x) => x.CompareTo(x.Default) == -1));
		}

		protected TData GetOverrideDataGreaterThanDefault<TData>(Func<TData> getOverrideData, Func<TData> getOriginalData, bool shouldCheckOverrideData = false)
			where TData : INumericZType
		{
			return GetOverrideData(getOverrideData, getOriginalData, (x) => x.CompareTo(x.Default) > 0, shouldCheckOverrideData);
		}

		protected TData GetOverrideDataGreaterThanOrEqualDefault<TData>(Func<TData> getOverrideData, Func<TData> getOriginalData)
			where TData : INumericZType
		{
			return GetOverrideData(getOverrideData, getOriginalData, (x) => x.CompareTo(x.Default) >= 0);
		}

		protected TData GetOverrideData<TData>(Func<TData> getOverrideData, Func<TData> getOriginalData)
			where TData : INumericZType
		{
			return GetOverrideData(getOverrideData, getOriginalData, (x) => x.CompareTo(x.Default) == -1);
		}

		TData GetOverrideData<TData>(Func<TData> getOverrideData, Func<TData> getOriginalData, Func<TData, bool> isOverridaValid, bool shouldCheckOverrideData = false)
			where TData : INumericZType
		{
			var result = default(TData);
			if (UseCaptureData)
			{
				result = getOverrideData();
				if (shouldCheckOverrideData && !isOverridaValid(result))
				{
					result = getOriginalData();
				}
			}
			else
			{
				if (IsOverriden)
				{
					result = getOverrideData();
					if (!isOverridaValid(result))
					{
						result = getOriginalData();
					}
				}
				else
				{
					result = getOriginalData();
				}
			}

			return result;
		}

		#region Individual claim classes

		public abstract class IndividualClaim : ICalculationExhibitsSupporter
		{
			public IndividualClaim(DrawbackClaims claims)
			{
				this.claims = claims;
			}
			readonly DrawbackClaims claims;

			protected DrawbackClaims Claims
			{
				get { return claims; }
			}

			protected JobDeclaration Drawback
			{
				get { return Claims.DrawbackLine.Declaration; }
			}

			public void DefaultOverrideData()
			{
				DefaultDeclaredAmount();
				Default_99ClaimedDutyAndCalculatedAmount();
			}

			public void Default_99ClaimedDutyAndCalculatedAmount()
			{
				var claimedDuty = GetDefault99ClaimedDuty();
				if (IsOverriden99ClaimedDutyEnabled && claimedDuty != Overriden99ClaimedDuty)
				{
					Overriden99ClaimedDuty = claimedDuty;
				}
				var calculatedAmount = GetDefaultCalculatedAmount();
				if (calculatedAmount != OverridenCalculatedAmount)
				{
					OverridenCalculatedAmount = calculatedAmount;
				}
			}

			public void DefaultWeightedRatio()
			{
				var value = GetDefaultWeightedRatio();
				if (value != OverridenWeightedRatio)
				{
					OverridenWeightedRatio = value;
				}
			}
			ZDecimal GetDefaultWeightedRatio() => IsClaimable ? GetWeightedRatio() : ZDecimal.Zero;

			public void DefaultDeclaredAmount()
			{
				var value = GetDefaultDeclaredAmount();
				if (value != OverridenDeclaredAmount)
				{
					OverridenDeclaredAmount = value;
				}
			}
			ZDecimal GetDefaultDeclaredAmount() => IsClaimable ? GetDeclaredAmount() : ZDecimal.Zero;

			public ZBool IsClaimable => Claims.DrawbackLine.Factory.GetValue(ref isClaimableCached, () => IsClaimableCore);
			CachedProperty<ZBool> isClaimableCached;

			protected virtual ZBool IsClaimableCore
			{
				get { return true; }
			}

			#region Declared Amount

			public ZDecimal DeclaredAmount
			{
				get => Claims.GetOverrideDataGreaterThanOrEqualDefault(ref declaredAmountCached, () => OverridenDeclaredAmount, GetDefaultDeclaredAmount);
				set => OverridenDeclaredAmount = value;
			}
			CachedProperty<ZDecimal> declaredAmountCached;

			protected abstract ZDecimal OverridenDeclaredAmount { get; set; }
			protected abstract ZDecimal GetDeclaredAmount();

			#endregion

			#region Line Amount

			public ZDecimal LineAmount => Claims.DrawbackLine.Factory.GetValue(ref lineAmountCached, GetLineAmount);
			CachedProperty<ZDecimal> lineAmountCached;

			protected virtual ZDecimal GetLineAmount()
			{
				return WeightedRatio.IsEmpty ? DeclaredAmount : new ZDecimal(DeclaredAmount * WeightedRatio).Round(2);
			}

			#endregion

			#region Weighted Ratio

			public ZDecimal WeightedRatio
			{
				get => Claims.GetOverrideDataGreaterThanOrEqualDefault(ref weightedRatioCached, () => OverridenWeightedRatio, GetDefaultWeightedRatio);
				set { OverridenWeightedRatio = value; }
			}
			CachedProperty<ZDecimal> weightedRatioCached;

			protected virtual ZDecimal OverridenWeightedRatio
			{
				get { return Claims.DrawbackLine.US_DRWWeightedRatio; }
				set { Claims.DrawbackLine.US_DRWWeightedRatio = value; }
			}

			protected virtual ZDecimal GetWeightedRatio()
			{
				var drawbackImportEntryLine = Claims.DeclaredLine;
				var result = ZDecimal.Zero;
				var totalEnteredValueForEntry = drawbackImportEntryLine?.TotalEnteredValueForEntry ?? ZDecimal.Zero;
				if (totalEnteredValueForEntry > 0)
				{
					result = drawbackImportEntryLine.TotalCustomsValueIncludingSecondaryLines / totalEnteredValueForEntry;
				}
				return result;
			}

			#endregion

			#region Per Unit

			public ZDecimal PerUnit => Claims.DrawbackLine.Factory.GetValue(ref perUnitCached, GetPerUnit);
			CachedProperty<ZDecimal> perUnitCached;

			protected virtual ZDecimal GetPerUnit()
			{
				var importQuantity = Claims.ImportQuantity;
				return UseSubstitutedValuePerUnitIfAvailable(importQuantity != 0 ? (DeclaredAmount / importQuantity) : 0m);
			}

			protected ZDecimal UseSubstitutedValuePerUnitIfAvailable(ZDecimal perUnit)
			{
				var result = perUnit;
				if (result > 0m && Claims.SubstitutedValuePerUnit > 0m && Claims.GoodsValuePerUnit > 0)
				{
					result *= Claims.SubstitutedValuePerUnit / Claims.GoodsValuePerUnit;
				}
				return result;
			}

			#endregion

			#region Claimed Duty

			public ZDecimal ClaimedDuty => Claims.DrawbackLine.Factory.GetValue(ref claimedDutyCached, () => GetClaimedDuty().Round(5));
			CachedProperty<ZDecimal> claimedDutyCached;

			protected virtual ZDecimal GetClaimedDuty()
			{
				return PerUnit * Claims.UsedQuantity;
			}

			#endregion

			#region 99% Duty

			public ZDecimal _99ClaimedDuty
			{
				get => Claims.GetOverrideDataGreaterThanDefault(ref var_99ClaimedDutyCached, () => Overriden99ClaimedDuty, GetDefault99ClaimedDuty, true);
				set { Overriden99ClaimedDuty = value; }
			}
			CachedProperty<ZDecimal> var_99ClaimedDutyCached;

			protected virtual ZDecimal GetDefault99ClaimedDuty()
			{
				return new ZDecimal(ClaimedDuty * 0.99m).Truncate(2);
			}

			protected abstract ZDecimal Overriden99ClaimedDuty { get; set; }

			protected virtual bool IsOverriden99ClaimedDutyEnabled => true;

			#endregion

			#region Calculated Amount

			public ZDecimal CalculatedAmount
			{
				get => Claims.GetOverrideDataGreaterThanDefault(ref calculatedAmountCached, () => OverridenCalculatedAmount, GetDefaultCalculatedAmount);
				set { OverridenCalculatedAmount = value; }
			}
			CachedProperty<ZDecimal> calculatedAmountCached;

			protected virtual ZDecimal GetDefaultCalculatedAmount()
			{
				return _99ClaimedDuty;
			}

			protected abstract ZDecimal OverridenCalculatedAmount { get; set; }

			#endregion

			#region Adjusted Claim Amount
			public ZDecimal AdjClaimAmount
			{
				get
				{
					var result = ZDecimal.Zero;
					if (Claims.IsOverriden && OverridenAdjClaimAmount >= 0)
					{
						result = OverridenAdjClaimAmount;
					}
					return result;
				}
				set { OverridenAdjClaimAmount = value; }
			}

			protected abstract ZDecimal OverridenAdjClaimAmount { get; set; }
			#endregion

			protected ZDecimal GetActualAmount(ZDecimal amount)
			{
				return amount == -1 ? ZDecimal.Zero : amount;
			}

			#region ICalculationExhibitsSupporter Members

			ZString ICalculationExhibitsSupporter.LineDutyRateDesc
			{
				get { return Claims.DrawbackLine.LineDutyRateDesc; }
			}

			ZString ICalculationExhibitsSupporter.ImportEntryOrCMDNo
			{
				get { return Claims.DrawbackLine.FormattedDrawbackEntryNoOrCMDNumber; }
			}

			ZString ICalculationExhibitsSupporter.InvoiceNo
			{
				get { return Claims.DrawbackLine.ImpDeclInvoiceNumber; }
			}

			ZString ICalculationExhibitsSupporter.PartNo
			{
				get { return Claims.DrawbackLine.PartNo; }
			}

			ZDecimal ICalculationExhibitsSupporter.ImportQuantity
			{
				get { return Claims.ImportQuantity; }
			}

			ZDecimal ICalculationExhibitsSupporter.TotalLineValue
			{
				get
				{
					return Claims.DeclaredLine?.CL_CustomsValue ?? Claims.DrawbackLine.US_DRWDeclaredVFD;
				}
			}

			ZDecimal ICalculationExhibitsSupporter.IndividualValue
			{
				get
				{
					return Claims.DrawbackLine.Factory.GetValue(ref individualValueCached, () =>
					{
						var result = ZDecimal.Zero;
						var supporter = (ICalculationExhibitsSupporter)this;
						if (supporter.ImportQuantity > 0)
						{
							result = supporter.TotalLineValue / supporter.ImportQuantity;
						}

						return result;
					});
				}
			}
			CachedProperty<ZDecimal> individualValueCached;

			ZDecimal ICalculationExhibitsSupporter.LineAmount
			{
				get { return LineAmount; }
			}

			ZDecimal ICalculationExhibitsSupporter.LineAmountEligible
			{
				get { return LineAmount * 0.99m; }
			}

			ZDecimal ICalculationExhibitsSupporter.CalculatedAmountForQtyUsed
			{
				get
				{
					var supporter = (ICalculationExhibitsSupporter)this;
					return supporter.PerUnit * supporter.ExportQuantity;
				}
			}

			ZDecimal ICalculationExhibitsSupporter.WeightedRatio
			{
				get { return WeightedRatio; }
			}

			ZDecimal ICalculationExhibitsSupporter.ExportQuantity
			{
				get { return Claims.UsedQuantity; }
			}

			ZString ICalculationExhibitsSupporter.Description
			{
				get
				{
					var result = Claims.DrawbackLine.JI_Description;
					if (ShouldIncludeInvoiceNoAndPartNoToDescription)
					{
						var invoiceNumber = ((ICalculationExhibitsSupporter)this).InvoiceNo;
						result += !invoiceNumber.IsEmpty ? (!result.IsEmpty ? ", Invoice No.: " : "Invoice No.: ") + invoiceNumber : "";

						var partNo = ((ICalculationExhibitsSupporter)this).PartNo;
						result += !partNo.IsEmpty ? (!result.IsEmpty ? ", Part No.: " : "Part No.: ") + partNo : "";
					}
					return result;
				}
			}

			protected virtual bool ShouldIncludeInvoiceNoAndPartNoToDescription
			{
				get { return false; }
			}

			ZDecimal ICalculationExhibitsSupporter.PerUnit
			{
				get { return PerUnit; }
			}

			ZDecimal ICalculationExhibitsSupporter.AmountPaid
			{
				get { return ClaimedDuty; }
			}

			ZDecimal ICalculationExhibitsSupporter.AmountClaimed
			{
				get { return _99ClaimedDuty; }
			}

			#endregion
		}

		#region DrawbackDutyClaim class

		public class DrawbackDutyClaim : IndividualClaim, IDutyCalculationExhibitsSupporter
		{
			public DrawbackDutyClaim(DrawbackClaims claims)
				: base(claims)
			{
			}

			#region Implementation

			protected override ZDecimal OverridenDeclaredAmount
			{
				get { return Claims.DrawbackLine.US_DRWDeclaredVFD; }
				set { Claims.DrawbackLine.US_DRWDeclaredVFD = value; }
			}

			protected override ZDecimal GetDeclaredAmount()
			{
				return Claims.DeclaredLine?.TotalCustomsValueIncludingSecondaryLinesFromInvoiceLines ?? (Claims.IsOverriden ? GetActualAmount(OverridenDeclaredAmount) : ZDecimal.Zero);
			}

			protected override ZDecimal GetClaimedDuty()
			{
				return ExportValue * DutyRate;
			}

			protected override ZDecimal Overriden99ClaimedDuty
			{
				get { return Claims.DrawbackLine.USI_DRW99ClaimedDuty; }
				set { Claims.DrawbackLine.USI_DRW99ClaimedDuty = value; }
			}

			protected override ZDecimal OverridenCalculatedAmount
			{
				get { return Claims.DrawbackLine.US_DRWCalcDuty; }
				set { Claims.DrawbackLine.US_DRWCalcDuty = value; }
			}

			protected override ZDecimal OverridenAdjClaimAmount
			{
				get { return Claims.DrawbackLine.US_DRWAdjClaimDuty; }
				set { Claims.DrawbackLine.US_DRWAdjClaimDuty = value; }
			}

			#endregion

			#region Export Value

			public ZDecimal ExportValue
			{
				get => Claims.DrawbackLine.Factory.GetValue(ref exportValueCached, () =>
				{
					ZDecimal perUnit;
					if (Claims.SubstitutedValuePerUnit > 0m)
					{
						perUnit = Claims.SubstitutedValuePerUnit;
					}
					else
					{
						perUnit = ValuePerUnitIncludingSecondaryLines;
					}
					return new ZDecimal(Claims.UsedQuantity * perUnit).Round(5);
				});
			}
			CachedProperty<ZDecimal> exportValueCached;

			#endregion

			#region Duty Rate

			public ZDecimal DutyRate
			{
				get
				{
					return Claims.DrawbackLine.US_DRWCalcDutyWithAdValoremRate ? (Claims.DrawbackLine.US_DRWAdValoremRate / 100m) : DeclaredAmount != 0 ? (LineDuty / DeclaredAmount) : 0m;
				}
			}

			#endregion

			#region _99ClaimedDuty

			protected override ZDecimal GetDefault99ClaimedDuty()
			{
				return Claims.DrawbackLine.Factory.GetValue(ref default99ClaimedDuty, () =>
				{
					var result = ClaimedDuty;
					var drawbackNAFTAs = Claims.DrawbackLine.DrawbackNAFTAs.Cast<DrawbackNAFTA>().Where(x => x.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty > 0);
					if (drawbackNAFTAs.Any())
					{
						result = Math.Min(result, drawbackNAFTAs.Min(x => x.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty));
					}
					return new ZDecimal(result * 0.99m).Truncate(2);
				});
			}
			CachedProperty<ZDecimal> default99ClaimedDuty;

			#endregion

			#region CalculatedAmount

			protected override ZDecimal GetDefaultCalculatedAmount()
			{
				return new ZDecimal(ClaimedDuty * 0.99m).Truncate(2);
			}

			#endregion

			#region LineDuty

			public ZDecimal LineDuty
			{
				get => Claims.DrawbackLine.Factory.GetValue(ref lineDutyCached, () =>
					{
						var result = ZDecimal.Zero;

						if (Claims.IsOverriden && Claims.DrawbackLine.US_DRWCalcDutyWithAdValoremRate)
						{
							var totalLineValue = Claims.DrawbackLine.DeclaredVFD;

							result = GetLocalAmount(Claims.DrawbackLine.US_DRWAdValoremRate / 100 * totalLineValue).Round(2).Amount;
						}
						else
						{
							result = Claims.GetOverrideDataGreaterThanDefault(() => GetActualAmount(Claims.DrawbackLine.US_DRWLineDuty), () => Claims.DeclaredLine?.TotalDutyIncludingSecondaryLines ?? ZDecimal.Zero, true);
						}
						return result;
					});
				set
				{
					Claims.DrawbackLine.US_DRWLineDuty = value;
				}
			}
			CachedProperty<ZDecimal> lineDutyCached;

			Money GetLocalAmount(ZDecimal amount)
			{
				if (amount.IsEmpty)
				{
					return Money.Empty;
				}
				else
				{
					return new Money(amount, JobDeclaration.GetLocalCurrency());
				}
			}

			#endregion

			#region DutyRateDesc

			public ZString LineDutyRateDesc
			{
				get => Claims.DrawbackLine.Factory.GetValue(ref lineDutyRateDescCached, () =>
					{
						var result = ZString.Empty;
						if (Claims.UseCaptureData)
						{
							result = Claims.DrawbackLine.US_DRWLineDutyRateDesc;
						}
						else
						{
							if (Claims.IsOverriden)
							{
								result = Claims.DrawbackLine.US_DRWLineDutyRateDesc;
							}
							if (result.IsEmpty)
							{
								result = GetDefaultDutyRateDesc();
							}
						}
						return result;
					});
				set { Claims.DrawbackLine.US_DRWLineDutyRateDesc = value; }
			}
			CachedProperty<ZString> lineDutyRateDescCached;

			public void DefaultDutyRateDesc()
			{
				LineDutyRateDesc = GetDefaultDutyRateDesc();
			}

			ZString GetDefaultDutyRateDesc()
			{
				var result = ZString.Empty;
				if (Claims.IsOverriden && Claims.DrawbackLine.US_DRWCalcDutyWithAdValoremRate)
				{
					var rate = Claims.DrawbackLine.US_DRWAdValoremRate;
					result = rate == ZDecimal.Zero
						? DutyResult.DutyFreeString
						: rate.Round(4).ToString() + "%";
				}
				else
				{
					var declaredLine = Claims.DeclaredLine;
					if (declaredLine != null)
					{
						var parentLine = declaredLine.ParentLine;
						if (parentLine != null)
						{
							var sBuilder = new ZStringBuilder();
							var parentLineDutyRateDescription = parentLine.DutyRateDescription;
							if (!parentLineDutyRateDescription.IsEmpty && !parentLineDutyRateDescription.EqualsIgnoringCase(DutyResult.DutyFreeString))
							{
								sBuilder.Append(parentLine.DutyRateDescription);
							}

							foreach (var childLine in parentLine.ChildSecondaryEntryLines)
							{
								var childLineDutyRateDescription = childLine.DutyRateDescription;
								if (!childLineDutyRateDescription.IsEmpty && !childLineDutyRateDescription.EqualsIgnoringCase(DutyResult.DutyFreeString))
								{
									if (!sBuilder.IsEmpty)
									{
										sBuilder.Append("+");
									}

									sBuilder.Append(childLineDutyRateDescription);
								}
							}

							result = sBuilder.ToString();
						}
						else
						{
							result = declaredLine.DutyRateDescription;
						}
					}

					if (result.IsEmpty)
					{
						var dutyCalculator = new AppendixFDutyCalculator(Claims.DrawbackLine, Claims.DrawbackLine.Factory);
						var dutyResult = dutyCalculator.DutyResult;
						result = dutyResult != null ? dutyResult.RateString : ZString.Empty;
						result = result.IsEmpty ? (ZString)DutyResult.DutyFreeString : result;
					}
				}
				return result.Left(Claims.DrawbackLine.US_DRWLineDutyRateDescInfo.MaxLength);
			}
			#endregion

			protected override ZDecimal GetPerUnit() => Claims.DrawbackLine.Factory.GetValue(ref perUnitCoreCached, () =>
			{
				var result = ZDecimal.Zero;
				var importQuantity = Claims.ImportQuantity;
				if (importQuantity > 0)
				{
					var drawbackImportEntryLine = Claims.DeclaredLine;
					var entryLineCustomsValue = drawbackImportEntryLine != null ? CustomsValueDeciderForInvoiceLine.GetCustomsValue(drawbackImportEntryLine) : Claims.DrawbackLine.DeclaredVFD;
					result = new ZDecimal(entryLineCustomsValue / importQuantity).Round(DecimalPlaces);
				}
				return result;
			});
			CachedProperty<ZDecimal> perUnitCoreCached;

			public ZDecimal ValuePerUnitIncludingSecondaryLines => Claims.DrawbackLine.Factory.GetValue(ref valuePerUnitIncludingSecondaryLinesCached, () =>
			{
				var result = ZDecimal.Zero;
				var importQuantity = Claims.ImportQuantity;
				if (importQuantity > 0)
				{
					result = new ZDecimal(DeclaredAmount / importQuantity).Round(DecimalPlaces);
				}
				return result;
			});
			CachedProperty<ZDecimal> valuePerUnitIncludingSecondaryLinesCached;

			int DecimalPlaces
			{
				get
				{
					var isACE = Claims.DrawbackLine.Declaration?.IsACE ?? false;
					return isACE ? 4 : 5;
				}
			}

			#region IDutyCalculationExhibitsSupporter Members

			ZDecimal IDutyCalculationExhibitsSupporter.ExportValue
			{
				get { return ExportValue; }
			}

			ZDecimal IDutyCalculationExhibitsSupporter.DutyRate
			{
				get { return DutyRate; }
			}

			#endregion
		}

		#endregion

		#region DrawbackHMFClaim class

		public class DrawbackHMFClaim : IndividualClaim
		{
			public DrawbackHMFClaim(DrawbackClaims claims)
				: base(claims)
			{
			}

			protected override ZDecimal OverridenDeclaredAmount
			{
				get { return Claims.DrawbackLine.US_DRWDeclaredHMF; }
				set { Claims.DrawbackLine.US_DRWDeclaredHMF = value; }
			}

			protected override ZDecimal GetDeclaredAmount()
			{
				var isOverriden = Claims.IsOverriden;
				var drawbackImportEntryLine = Claims.DeclaredLine;
				return drawbackImportEntryLine != null ? (isOverriden && WeightedRatio.IsEmpty
						? drawbackImportEntryLine.TotalHMFIncludingSecondaryLines
						: drawbackImportEntryLine.HMFAmountForEntry)
					: isOverriden ? GetActualAmount(OverridenDeclaredAmount) : ZDecimal.Zero;
			}

			protected override ZBool IsClaimableCore
			{
				get
				{
					var result = ZBool.False;
					if (Drawback?.IsHMF_MPFClaimable ?? false)
					{
						var declaredLine = Claims.DeclaredLine;
						result = declaredLine == null
							? Claims.IsOverriden && OverridenDeclaredAmount > ZDecimal.Zero
							: declaredLine.TotalHMFIncludingSecondaryLines > 0;
					}
					else if (Claims.IsOverriden)
					{
						result = OverridenDeclaredAmount > ZDecimal.Zero;
					}

					return result;
				}
			}

			protected override bool ShouldIncludeInvoiceNoAndPartNoToDescription
			{
				get { return true; }
			}

			protected override ZDecimal GetPerUnit()
			{
				var result = ZDecimal.Zero;
				var supporter = (ICalculationExhibitsSupporter)this;
				if (supporter.ImportQuantity > 0)
				{
					if (Claims.DrawbackLine.Is7552)
					{
						result = supporter.LineAmount / supporter.ImportQuantity;
					}
					else
					{
						result = supporter.LineAmountEligible / supporter.ImportQuantity;
					}
				}

				return UseSubstitutedValuePerUnitIfAvailable(result);
			}

			protected override ZDecimal GetDefault99ClaimedDuty()
			{
				return ClaimedDuty.Round(2);
			}

			protected override ZDecimal Overriden99ClaimedDuty
			{
				get { return Claims.DrawbackLine.USI_DRW99ClaimedHMF; }
				set { Claims.DrawbackLine.USI_DRW99ClaimedHMF = value; }
			}

			protected override ZDecimal OverridenCalculatedAmount
			{
				get { return Claims.DrawbackLine.US_DRWCalcHMF; }
				set { Claims.DrawbackLine.US_DRWCalcHMF = value; }
			}

			protected override ZDecimal OverridenAdjClaimAmount
			{
				get { return Claims.DrawbackLine.US_DRWAdjClaimHMF; }
				set { Claims.DrawbackLine.US_DRWAdjClaimHMF = value; }
			}
		}

		#endregion

		#region DrawbackIRTaxClaim class

		public class DrawbackIRTaxClaim : IndividualClaim
		{
			public DrawbackIRTaxClaim(DrawbackClaims claims)
				: base(claims)
			{
			}

			protected override ZDecimal OverridenDeclaredAmount
			{
				get { return Claims.DrawbackLine.US_DRWDeclaredTax; }
				set { Claims.DrawbackLine.US_DRWDeclaredTax = value; }
			}

			protected override ZDecimal GetDeclaredAmount()
			{
				return Claims.DeclaredLine?.TotalTaxIncludingSecondaryLines ?? (Claims.IsOverriden ? GetActualAmount(OverridenDeclaredAmount) : ZDecimal.Zero);
			}

			protected override ZDecimal GetDefault99ClaimedDuty()
			{
				var declaration = Drawback;
				bool isFullRateApplied = false;

				if (declaration != null)
				{
					if (declaration.IsACEDrawback)
					{
						isFullRateApplied = ACEDrawbackProvisionsList.Is1313D(declaration.US_EntryType) || ACEDrawbackProvisionsList.Is5062(declaration.US_EntryType);
					}
					else
					{
						var drawbackSection = declaration.US_DRWSection.ToUpper().Replace(" ", "");
						isFullRateApplied = drawbackSection.Contains("1313(D)") || drawbackSection.Contains("5062");
					}
				}

				return isFullRateApplied ? ClaimedDuty.Round(2) : base.GetDefault99ClaimedDuty();
			}

			protected override ZDecimal Overriden99ClaimedDuty
			{
				get { return Claims.DrawbackLine.USI_DRW99ClaimedTax; }
				set { Claims.DrawbackLine.USI_DRW99ClaimedTax = value; }
			}

			protected override ZDecimal OverridenCalculatedAmount
			{
				get { return Claims.DrawbackLine.US_DRWCalcTax; }
				set { Claims.DrawbackLine.US_DRWCalcTax = value; }
			}

			protected override ZDecimal OverridenAdjClaimAmount
			{
				get { return Claims.DrawbackLine.US_DRWAdjClaimTax; }
				set { Claims.DrawbackLine.US_DRWAdjClaimTax = value; }
			}
		}

		#endregion

		#region DrawbackMPFClaim class

		public class DrawbackMPFClaim : IndividualClaim
		{
			public DrawbackMPFClaim(DrawbackClaims claims)
				: base(claims)
			{
			}

			protected override ZDecimal OverridenDeclaredAmount
			{
				get { return Claims.DrawbackLine.US_DRWDeclaredMPF; }
				set { Claims.DrawbackLine.US_DRWDeclaredMPF = value; }
			}

			protected override ZDecimal GetDeclaredAmount()
			{
				var isOverriden = Claims.IsOverriden;
				var drawbackImportEntryLine = Claims.DeclaredLine;
				return drawbackImportEntryLine != null ? (isOverriden && WeightedRatio.IsEmpty
						? drawbackImportEntryLine.TotalPayableMPFIncludingSecondaryLines
						: drawbackImportEntryLine.MPFAmountForEntry)
					: isOverriden ? GetActualAmount(OverridenDeclaredAmount) : ZDecimal.Zero;
			}

			protected override ZBool IsClaimableCore
			{
				get
				{
					var result = ZBool.False;
					if (Drawback?.IsHMF_MPFClaimable ?? false)
					{
						var declaredLine = Claims.DeclaredLine;
						result = declaredLine == null
							? Claims.IsOverriden && OverridenDeclaredAmount > ZDecimal.Zero
							: declaredLine.TotalPayableMPFIncludingSecondaryLines > 0;
					}
					else if (Claims.IsOverriden)
					{
						result = OverridenDeclaredAmount > ZDecimal.Zero;
					}

					return result;
				}
			}

			protected override bool ShouldIncludeInvoiceNoAndPartNoToDescription
			{
				get { return true; }
			}

			protected override ZDecimal GetPerUnit()
			{
				var result = ZDecimal.Zero;
				var supporter = (ICalculationExhibitsSupporter)this;
				if (supporter.ImportQuantity > 0)
				{
					if (Claims.DrawbackLine.Is7552)
					{
						result = supporter.LineAmount / supporter.ImportQuantity;
					}
					else
					{
						result = supporter.LineAmountEligible / supporter.ImportQuantity;
					}
				}

				return UseSubstitutedValuePerUnitIfAvailable(result);
			}

			protected override ZDecimal GetDefault99ClaimedDuty()
			{
				return ClaimedDuty.Round(2);
			}

			protected override ZDecimal GetWeightedRatio()
			{
				var drawbackImportEntryLine = Claims.DeclaredLine;
				var result = ZDecimal.Zero;
				var allDrawbackImportEntryLines = drawbackImportEntryLine?.AllDrawbackEntryLines ?? Array.Empty<IDrawbackEntryLine>();
				if (allDrawbackImportEntryLines.Length > 0 && drawbackImportEntryLine.TotalPayableMPFIncludingSecondaryLines > 0)
				{
					var totalCustomsValuesOfLinesWithMPF = allDrawbackImportEntryLines.Where(x => x.US_HasMPF).Sum(x => x.CL_CustomsValue);
					result = totalCustomsValuesOfLinesWithMPF > 0 ? drawbackImportEntryLine.TotalCustomsValueIncludingSecondaryLines / totalCustomsValuesOfLinesWithMPF : 0m;
				}
				return result;
			}

			protected override ZDecimal OverridenWeightedRatio
			{
				get { return Claims.DrawbackLine.US_DRWMPFWeightedRatio > 0 ? Claims.DrawbackLine.US_DRWMPFWeightedRatio : base.OverridenWeightedRatio; }
				set { Claims.DrawbackLine.US_DRWMPFWeightedRatio = value; }
			}

			protected override ZDecimal GetLineAmount()
			{
				var result = ZDecimal.Zero;
				if (Claims.UseCaptureData || Claims.IsOverriden || (Claims.DeclaredLine is IDrawbackEntryLine drawbackImportEntryLine && drawbackImportEntryLine.TotalPayableMPFIncludingSecondaryLines > 0))
				{
					var declaredAmount = DeclaredAmount;
					var weightedRatio = WeightedRatio;
					result = weightedRatio.IsEmpty ? declaredAmount : new ZDecimal(declaredAmount * weightedRatio).Round(2);
				}
				return result;
			}

			protected override ZDecimal Overriden99ClaimedDuty
			{
				get { return Claims.DrawbackLine.USI_DRW99ClaimedMPF; }
				set { Claims.DrawbackLine.USI_DRW99ClaimedMPF = value; }
			}

			protected override ZDecimal OverridenCalculatedAmount
			{
				get { return Claims.DrawbackLine.US_DRWCalcMPF; }
				set { Claims.DrawbackLine.US_DRWCalcMPF = value; }
			}

			protected override ZDecimal OverridenAdjClaimAmount
			{
				get { return Claims.DrawbackLine.US_DRWAdjClaimMPF; }
				set { Claims.DrawbackLine.US_DRWAdjClaimMPF = value; }
			}
		}

		#endregion

		#region DrawbackOtherFeesClaim class

		public class DrawbackOtherFeesClaim : IndividualClaim
		{
			public DrawbackOtherFeesClaim(DrawbackClaims claims)
				: base(claims)
			{
			}

			protected override ZDecimal OverridenDeclaredAmount
			{
				get { return Claims.DrawbackLine.US_DRWDeclaredOtherFees; }
				set { Claims.DrawbackLine.US_DRWDeclaredOtherFees = value; }
			}

			protected override ZDecimal GetDeclaredAmount()
			{
				var drawbackImportEntryLine = Claims.DeclaredLine;
				var result = ZDecimal.Zero;
				if (Drawback is JobDeclaration declaration && !declaration.US_AcceleratedClaimInd && drawbackImportEntryLine != null)
				{
					result = drawbackImportEntryLine.TotalFeeAmountIncludingSecondaryLines
						- (drawbackImportEntryLine.TotalTaxIncludingSecondaryLines + drawbackImportEntryLine.TotalMPFIncludingSecondaryLines
						+ drawbackImportEntryLine.TotalHMFIncludingSecondaryLines + drawbackImportEntryLine.TotalDutyIncludingSecondaryLines);
					if (result < 0)
					{
						result = ZDecimal.Zero;
					}
				}
				else if (Claims.IsOverriden)
				{
					result = GetActualAmount(OverridenDeclaredAmount);
				}

				return result;
			}

			protected override bool IsOverriden99ClaimedDutyEnabled => false;

			protected override ZDecimal Overriden99ClaimedDuty
			{
				get { return GetDefault99ClaimedDuty(); }
				set { }
			}

			protected override ZDecimal OverridenCalculatedAmount
			{
				get { return ZDecimal.Zero; }
				set { }
			}

			protected override ZDecimal OverridenAdjClaimAmount
			{
				get { return ZDecimal.Zero; }
				set { }
			}
		}

		#endregion

		#region ACEDrawbackOtherFeeClaim class

		public class ACEDrawbackOtherFeeClaim : IndividualClaim
		{
			public ACEDrawbackOtherFeeClaim(DrawbackClaims claims)
				: base(claims)
			{
			}

			protected override ZDecimal OverridenDeclaredAmount
			{
				get { return Claims.ACEDrawbackOtherFee?.US_FeeAmount ?? ZDecimal.Zero; }
				set
				{
					if (Claims.ACEDrawbackOtherFee is DrawbackOtherFee drawbackOtherFee)
					{
						drawbackOtherFee.US_FeeAmount = value;
					}
				}
			}

			protected override ZDecimal GetDeclaredAmount()
			{
				var result = ZDecimal.Zero;
				var drawbackImportEntryLine = Claims.DeclaredLine;
				if (Drawback is JobDeclaration declaration && !declaration.US_AcceleratedClaimInd && drawbackImportEntryLine != null)
				{
					var feeTypeCode = Claims.ACEDrawbackOtherFee?.US_FeeType ?? ZString.Empty;
					if (!feeTypeCode.IsEmpty)
					{
						if (feeTypeCode == DrawbackOtherFeeTypesList.Codes.OtherFee)
						{
							result += GetOtherFeeAmountFromEntryLine(drawbackImportEntryLine);

							foreach (var childLine in drawbackImportEntryLine.ChildSecondaryEntryLines)
							{
								result += GetOtherFeeAmountFromEntryLine(childLine);
							}
						}
						else
						{
							result += drawbackImportEntryLine.GetFeeAmount(feeTypeCode);

							foreach (var childLine in drawbackImportEntryLine.ChildSecondaryEntryLines)
							{
								result += childLine.GetFeeAmount(feeTypeCode);
							}
						}
					}
				}
				else if (Claims.IsOverriden)
				{
					result = OverridenDeclaredAmount;
				}

				return result;
			}

			ZDecimal GetOtherFeeAmountFromEntryLine(IDrawbackEntryLine entryLine)
			{
				var result = ZDecimal.Zero;
				var feeCusCodeDataList = CusFeeCodeConstants.GetAccountingClassFeeCodeList(entryLine.Factory);
				var otherFeeTypesList = entryLine.Factory.GetCachedValue<DrawbackOtherFeeTypesList>();
				var otherFees = entryLine.Fees.Where(fee => feeCusCodeDataList.ContainsCode(fee.Code) && !CusFeeCodeConstants.OtherFeeCodesToExcludeForACEDrawback(fee.Code) && !otherFeeTypesList.ContainsCode(fee.Code));
				result = otherFees.Sum(fee => fee.Amount);
				return result;
			}

			protected override ZDecimal Overriden99ClaimedDuty
			{
				get { return Claims.ACEDrawbackOtherFee?.US_99ClaimAmount ?? ZDecimal.Zero; }
				set
				{
					if (Claims.ACEDrawbackOtherFee is DrawbackOtherFee drawbackOtherFee)
					{
						drawbackOtherFee.US_99ClaimAmount = value;
					}
				}
			}

			protected override ZDecimal OverridenCalculatedAmount
			{
				get { return Claims.ACEDrawbackOtherFee?.US_CalculatedAmount ?? ZDecimal.Zero; }
				set
				{
					if (Claims.ACEDrawbackOtherFee is DrawbackOtherFee drawbackOtherFee)
					{
						drawbackOtherFee.US_CalculatedAmount = value;
					}
				}
			}

			protected override ZDecimal OverridenAdjClaimAmount
			{
				get { return Claims.ACEDrawbackOtherFee?.US_AdjClaimAmount ?? ZDecimal.Zero; }
				set
				{
					if (Claims.ACEDrawbackOtherFee is DrawbackOtherFee drawbackOtherFee)
					{
						drawbackOtherFee.US_AdjClaimAmount = value;
					}
				}
			}
		}

		#endregion
		#endregion
	}
}
