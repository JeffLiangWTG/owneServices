using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

#pragma warning disable IDE0001 // Prevent name simplification to AutoReconFlattenedDataLine
using Schema = Enterprise.Customs.US.Business.ReconFlattenedDataLine.Schema;
#pragma warning restore IDE0001 // Prevent name simplification to AutoReconFlattenedDataLine

namespace Enterprise.Customs.US.Business
{
	public static class ImportCollectionInfoImplForReconExtensions
	{
		public static void AddFlattenedDataPropertiesForLineExport(this ImportCollectionInfoImpl impl)
		{
			AddProperty(impl, Schema.JobNumber, "Job #");
			AddEntryUpdatableHeaderFields(impl);

			AddProperty(impl, Schema.LineNumber, "Rec Line #");
			AddProperty(impl, Schema.OrgLineNumber, "Orig. Line #");
			AddProperty(impl, Schema.IsChildLine, "Is Child Line");

			AddProperty(impl, Schema.ProductNumber, "Product Code");
			AddProperty(impl, Schema.Lookup, "Lookup");
			AddProperty(impl, Schema.OriginalFormattedTariff, "Orig. Tariff", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.OriginalFormattedSupTariff, "Orig. Prov/Prog. Tariff", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.ReconFormattedTariff, "Rec Tariff", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconFormattedSupTariff, "Rec Prov/Prog. Tariff", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.GoodsDescription, "Goods Desc", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.MessageMode, "Message Mode", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.CountryOfOrigin, "C/O");
			AddProperty(impl, Schema.OriginalSPI, "Orig. SPI", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconSPI, "Rec SPI", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconSecondarySPI, "Set Ind", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.OriginalCustomsValue, "Orig. CV");
			AddProperty(impl, Schema.Original98Value, "Orig. US/Orig. Value");

			AddProperty(impl, Schema.ReconCustomsValue, "Rec CV");
			AddProperty(impl, Schema.Recon98Value, "Rec US/Orig. Value");

			AddProperty(impl, Schema.OriginalFirstUQ, "Orig. 1st UQ");
			AddProperty(impl, Schema.OriginalSupFirstUQ, "Orig. Prov/Prog. 1st UQ");
			AddProperty(impl, Schema.OriginalFirstQty, "Orig. 1st Qty");
			AddProperty(impl, Schema.OriginalSupFirstQty, "Orig. Prov/Prog. 1st Qty");
			AddProperty(impl, Schema.ReconFirstQty, "Rec 1st Qty");
			AddProperty(impl, Schema.ReconSupFirstQty, "Rec Prov/Prog. 1st Qty");

			AddProperty(impl, Schema.OriginalSecondUQ, "Orig. 2nd UQ");
			AddProperty(impl, Schema.OriginalSupSecondUQ, "Orig. Prov/Prog. 2nd UQ");
			AddProperty(impl, Schema.OriginalSecondQty, "Orig. 2nd Qty");
			AddProperty(impl, Schema.OriginalSupSecondQty, "Orig. Prov/Prog. 2nd Qty");
			AddProperty(impl, Schema.ReconSecondQty, "Rec 2nd Qty");
			AddProperty(impl, Schema.ReconSupSecondQty, "Rec Prov/Prog. 2nd Qty");

			AddProperty(impl, Schema.OriginalThirdUQ, "Orig. 3rd UQ");
			AddProperty(impl, Schema.OriginalSupThirdUQ, "Orig. Prov/Prog. 3rd UQ");
			AddProperty(impl, Schema.OriginalThirdQty, "Orig. 3rd Qty");
			AddProperty(impl, Schema.OriginalSupThirdQty, "Orig. Prov/Prog. 3rd Qty");
			AddProperty(impl, Schema.ReconThirdQty, "Rec 3rd Qty");
			AddProperty(impl, Schema.ReconSupThirdQty, "Rec Prov/Prog. 3rd Qty");

			AddProperty(impl, Schema.OriginalCottonFeeExempt, "Orig. Cotton Fee Exempt", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconCottonFeeExempt, "Rec Cotton Fee Exempt", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.OriginalDutyOverride, "Override Orig. Duty?");
			AddProperty(impl, Schema.OriginalDuty, "Orig. Duty");
			AddProperty(impl, Schema.OriginalSupDutyOverride, "Override Orig. Prov/Prog. Duty?");
			AddProperty(impl, Schema.OriginalSupDuty, "Orig. Prov/Prog. Duty");

			AddProperty(impl, Schema.ReconDutyOverride, "Override Rec Duty?");
			AddProperty(impl, Schema.ReconDuty, "Rec Duty");
			AddProperty(impl, Schema.ReconSupDutyOverride, "Override Rec Prov/Prog. Duty?");
			AddProperty(impl, Schema.ReconSupDuty, "Rec Prov/Prog. Duty");

			AddProperty(impl, Schema.OriginalTaxApply, "Orig. Tax Apply?");
			AddProperty(impl, Schema.OriginalTaxCode, "Orig. Tax Code");
			AddProperty(impl, Schema.OriginalTaxAmount, "Orig. Tax Amount");
			AddProperty(impl, Schema.OriginalTaxRateType, "Orig. Tax Rate Type");
			AddProperty(impl, Schema.OriginalTaxRateS, "Orig. Tax Rate");
			AddProperty(impl, Schema.OriginalTaxRate, "Orig. Tax Rate Value");
			AddProperty(impl, Schema.OriginalTaxRateQuantity, "Orig. Tax Qty");
			AddProperty(impl, Schema.OriginalRateType, "Orig. Rate Type");

			AddProperty(impl, Schema.ReconTaxApply, "Rec Tax Apply?");
			AddProperty(impl, Schema.ReconTaxCode, "Rec Tax Code");
			AddProperty(impl, Schema.ReconTaxRateType, "Rec Tax Rate Type");
			AddProperty(impl, Schema.ReconTaxRateS, "Rec Tax Rate");
			AddProperty(impl, Schema.ReconTaxRate, "Rec Tax Rate Value");
			AddProperty(impl, Schema.ReconTaxRateQuantity, "Rec Tax Qty");
			AddProperty(impl, Schema.ReconRateType, "Rec Rate Type");

			AddProperty(impl, Schema.OverrideOriginalMPF, "Override Orig. MPF");
			AddProperty(impl, Schema.OriginalMPF, "Orig. MPF");
			AddProperty(impl, Schema.OverrideOriginalHMF, "Override Orig. HMF");
			AddProperty(impl, Schema.OriginalHMF, "Orig. HMF", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.OriginalOtherFeeCode, "Oth. Fee Code");
			AddProperty(impl, Schema.OverrideOriginalOtherFeeAmount, "Override Orig. Oth. Fee");
			AddProperty(impl, Schema.OriginalOtherFee, "Orig. Oth. Fee");

			AddProperty(impl, Schema.OverrideReconMPF, "Override Rec MPF");
			AddProperty(impl, Schema.ReconMPF, "Rec MPF");
			AddProperty(impl, Schema.OverrideReconHMF, "Override Rec HMF");
			AddProperty(impl, Schema.ReconHMF, "Rec HMF");
			AddProperty(impl, Schema.ReconOtherFeeCode, "Rec Oth. Fee Code");
			AddProperty(impl, Schema.OverrideReconOtherFeeAmount, "Override Rec Oth. Fee");
			AddProperty(impl, Schema.ReconOtherFee, "Rec Oth. Fee");

			AddProperty(impl, Schema.HTSChangedDueToValue, "HTS Changed Due to Value");
			AddProperty(impl, Schema.IsTextile, "Is Textile");
			AddProperty(impl, Schema.ReconReason, "Rec Reason");
		}

		public static void AddFlattenedDataPropertiesForLineUpdate(this ImportCollectionInfoImpl impl)
		{
			AddEntryUpdatableHeaderFields(impl);

			AddProperty(impl, Schema.LineNumber, "Rec Line #");
			AddProperty(impl, Schema.OrgLineNumber, "Orig. Line #");
			AddProperty(impl, Schema.IsChildLine, "Is Child Line");

			AddProperty(impl, Schema.ProductNumber, "Product Code");
			AddProperty(impl, Schema.Lookup, "Lookup");

			AddProperty(impl, Schema.OriginalFormattedTariff, "Orig. Tariff", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.OriginalFormattedSupTariff, "Orig. Prov/Prog. Tariff", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconFormattedTariff, "Rec Tariff", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconFormattedSupTariff, "Rec Prov/Prog. Tariff", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.GoodsDescription, "Goods Desc", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.MessageMode, "Message Mode", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.CountryOfOrigin, "C/O");
			AddProperty(impl, Schema.OriginalSPI, "Orig. SPI", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconSPI, "Rec SPI", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconSecondarySPI, "Set Ind", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.OriginalCustomsValue, "Orig. CV");
			AddProperty(impl, Schema.Original98Value, "Orig. US/Orig. Value");

			AddProperty(impl, Schema.ReconCustomsValue, "Rec CV");
			AddProperty(impl, Schema.Recon98Value, "Rec US/Orig. Value");

			AddProperty(impl, Schema.OriginalFirstQty, "Orig. 1st Qty");
			AddProperty(impl, Schema.OriginalSupFirstQty, "Orig. Prov/Prog. 1st Qty");
			AddProperty(impl, Schema.ReconFirstQty, "Rec 1st Qty");
			AddProperty(impl, Schema.ReconSupFirstQty, "Rec Prov/Prog. 1st Qty");

			AddProperty(impl, Schema.OriginalSecondQty, "Orig. 2nd Qty");
			AddProperty(impl, Schema.OriginalSupSecondQty, "Orig. Prov/Prog. 2nd Qty");
			AddProperty(impl, Schema.ReconSecondQty, "Rec 2nd Qty");
			AddProperty(impl, Schema.ReconSupSecondQty, "Rec Prov/Prog. 2nd Qty");

			AddProperty(impl, Schema.OriginalThirdQty, "Orig. 3rd Qty");
			AddProperty(impl, Schema.OriginalSupThirdQty, "Orig. Prov/Prog. 3rd Qty");
			AddProperty(impl, Schema.ReconThirdQty, "Rec 3rd Qty");
			AddProperty(impl, Schema.ReconSupThirdQty, "Rec Prov/Prog. 3rd Qty");

			AddProperty(impl, Schema.OriginalCottonFeeExempt, "Orig. Cotton Fee Exempt", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconCottonFeeExempt, "Rec Cotton Fee Exempt", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.OriginalDutyOverride, "Override Orig. Duty?");
			AddProperty(impl, Schema.OriginalDuty, "Orig. Duty");

			AddProperty(impl, Schema.OriginalSupDutyOverride, "Override Orig. Prov/Prog. Duty?");
			AddProperty(impl, Schema.OriginalSupDuty, "Orig. Prov/Prog. Duty");

			AddProperty(impl, Schema.ReconDutyOverride, "Override Rec Duty?");
			AddProperty(impl, Schema.ReconDuty, "Rec Duty");

			AddProperty(impl, Schema.ReconSupDutyOverride, "Override Rec Prov/Prog. Duty?");
			AddProperty(impl, Schema.ReconSupDuty, "Rec Prov/Prog. Duty");

			AddProperty(impl, Schema.OriginalTaxApply, "Orig. Tax Apply?");
			AddProperty(impl, Schema.OriginalTaxCode, "Orig. Tax Code");
			AddProperty(impl, Schema.OriginalTaxAmount, "Orig. Tax Amount");
			AddProperty(impl, Schema.OriginalTaxRateType, "Orig. Tax Rate Type");
			AddProperty(impl, Schema.OriginalTaxRateS, "Orig. Tax Rate");
			AddProperty(impl, Schema.OriginalTaxRate, "Orig. Tax Rate Value");
			AddProperty(impl, Schema.OriginalTaxRateQuantity, "Orig. Tax Qty");
			AddProperty(impl, Schema.OriginalRateType, "Orig. Rate Type");

			AddProperty(impl, Schema.ReconTaxApply, "Rec Tax Apply?");
			AddProperty(impl, Schema.ReconTaxCode, "Rec Tax Code");
			AddProperty(impl, Schema.ReconTaxRateType, "Rec Tax Rate Type");
			AddProperty(impl, Schema.ReconTaxRateS, "Rec Tax Rate");
			AddProperty(impl, Schema.ReconTaxRate, "Rec Tax Rate Value");
			AddProperty(impl, Schema.ReconTaxRateQuantity, "Rec Tax Qty");
			AddProperty(impl, Schema.ReconRateType, "Rec Rate Type");

			AddProperty(impl, Schema.OverrideOriginalMPF, "Override Orig. MPF");
			AddProperty(impl, Schema.OriginalMPF, "Orig. MPF");
			AddProperty(impl, Schema.OverrideOriginalHMF, "Override Orig. HMF");
			AddProperty(impl, Schema.OriginalHMF, "Orig. HMF", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.OriginalOtherFeeCode, "Orig. Fee Code");
			AddProperty(impl, Schema.OverrideOriginalOtherFeeAmount, "Override Orig. Oth. Fee");
			AddProperty(impl, Schema.OriginalOtherFee, "Orig. Oth. Fee");

			AddProperty(impl, Schema.OverrideReconMPF, "Override Rec MPF");
			AddProperty(impl, Schema.ReconMPF, "Rec MPF");
			AddProperty(impl, Schema.OverrideReconHMF, "Override Rec HMF");
			AddProperty(impl, Schema.ReconHMF, "Rec HMF");
			AddProperty(impl, Schema.ReconOtherFeeCode, "Rec Oth. Fee Code");
			AddProperty(impl, Schema.OverrideReconOtherFeeAmount, "Override Rec Oth. Fee");
			AddProperty(impl, Schema.ReconOtherFee, "Rec Oth. Fee");

			AddProperty(impl, Schema.HTSChangedDueToValue, "HTS Changed Due to Value");
			AddProperty(impl, Schema.IsTextile, "Is Textile");
			AddProperty(impl, Schema.ReconReason, "Rec Reason");
		}

		public static void AddFlattenedDataPropertiesForEntryExport(this ImportCollectionInfoImpl impl)
		{
			AddProperty(impl, Schema.JobNumber, "Job #");
			AddEntryUpdatableHeaderFields(impl);
			AddProperty(impl, Schema.GoodsDescription, "Goods Desc", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.HasNoLineDetails, "Has No Line Details");
			AddProperty(impl, Schema.MessageMode, "Message Mode", ZCharacterCasing.Upper);
			AddEntryChargeFields(impl);
		}

		public static void AddFlattenedDataPropertiesForEntryUpdate(this ImportCollectionInfoImpl impl)
		{
			AddEntryUpdatableHeaderFields(impl);
			AddProperty(impl, Schema.GoodsDescription, "Goods Desc", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.HasNoLineDetails, "Has No Line Details");
			AddProperty(impl, Schema.MessageMode, "Message Mode", ZCharacterCasing.Upper);
			AddEntryChargeFields(impl);
		}

		#region Common Entry Level Fields

		static void AddEntryUpdatableHeaderFields(this ImportCollectionInfoImpl impl)
		{
			AddProperty(impl, Schema.EntryNumber, "Entry #", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.OwnerReferenceNumber, "Owner Ref");
			AddProperty(impl, Schema.EntryPort, "Entry Port", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ImportationDate, "Import Date");
			AddProperty(impl, Schema.PaymentDate, "Payment Due Date");
			AddProperty(impl, Schema.EntryDate, "Entry Date");
			if (ZZCustomsFunctionality.USFTAReconIndIsValid)
			{
				AddProperty(impl, Schema.FTAReconFiled, "FTA Recon Filed");
			}
		}

		static void AddEntryChargeFields(this ImportCollectionInfoImpl impl)
		{
			AddProperty(impl, Schema.OriginalDuty, "Orig. Duty");
			AddProperty(impl, Schema.ReconDuty, "Rec Duty");

			AddProperty(impl, Schema.OriginalMPF, "Orig. MPF");
			AddProperty(impl, Schema.ReconMPF, "Rec MPF");

			AddProperty(impl, Schema.OriginalHMF, "Orig. HMF", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconHMF, "Rec HMF");

			AddProperty(impl, Schema.OriginalAVO, "Orig. AVO");
			AddProperty(impl, Schema.ReconAVO, "Rec AVO");

			AddProperty(impl, Schema.OriginalBeef, "Orig. Beef");
			AddProperty(impl, Schema.ReconBeef, "Rec Beef");

			AddProperty(impl, Schema.OriginalBlueberry, "Orig. Blueberry");
			AddProperty(impl, Schema.ReconBlueberry, "Rec Blueberry");

			AddProperty(impl, Schema.OriginalCotton, "Orig. Cotton", ZCharacterCasing.Upper);
			AddProperty(impl, Schema.ReconCotton, "Rec Cotton", ZCharacterCasing.Upper);

			AddProperty(impl, Schema.OriginalDairy, "Orig. Dairy");
			AddProperty(impl, Schema.ReconDairy, "Rec Dairy");

			AddProperty(impl, Schema.OriginalDistilledSpirits, "Orig. DistilledSpirits");
			AddProperty(impl, Schema.ReconDistilledSpirits, "Rec DistilledSpirits");

			AddProperty(impl, Schema.OriginalMailFee, "Orig. MailFee");
			AddProperty(impl, Schema.ReconMailFee, "Rec MailFee");

			AddProperty(impl, Schema.OriginalFreshLimes, "Orig. FreshLimes");
			AddProperty(impl, Schema.ReconFreshLimes, "Rec FreshLimes");

			AddProperty(impl, Schema.OriginalHoney, "Orig. Honey");
			AddProperty(impl, Schema.ReconHoney, "Rec Honey");

			AddProperty(impl, Schema.OriginalMango, "Orig. Mango");
			AddProperty(impl, Schema.ReconMango, "Rec Mango");

			AddProperty(impl, Schema.OriginalMerchandiseInformal, "Orig. MerchandiseInformal");
			AddProperty(impl, Schema.ReconMerchandiseInformal, "Rec MerchandiseInformal");

			AddProperty(impl, Schema.OriginalMerchandiseSurcharge, "Orig. MerchandiseSurcharge");
			AddProperty(impl, Schema.ReconMerchandiseSurcharge, "Rec MerchandiseSurcharge");

			AddProperty(impl, Schema.OriginalMushroom, "Orig. Mushroom");
			AddProperty(impl, Schema.ReconMushroom, "Rec Mushroom");

			AddProperty(impl, Schema.OriginalOtherAgencies, "Orig. OtherAgencies");
			AddProperty(impl, Schema.ReconOtherAgencies, "Rec OtherAgencies");

			AddProperty(impl, Schema.OriginalRaspberry, "Orig. Raspberry");
			AddProperty(impl, Schema.ReconRaspberry, "Rec Raspberry");

			AddProperty(impl, Schema.OriginalPork, "Orig. Pork");
			AddProperty(impl, Schema.ReconPork, "Rec Pork");

			AddProperty(impl, Schema.OriginalPotato, "Orig. Potato");
			AddProperty(impl, Schema.ReconPotato, "Rec Potato");

			AddProperty(impl, Schema.OriginalSoftwoodLumber, "Orig. Softwood Lumber");
			AddProperty(impl, Schema.ReconSoftwoodLumber, "Rec Softwood Lumber");

			AddProperty(impl, Schema.OriginalTobacco, "Orig. Tobacco");
			AddProperty(impl, Schema.ReconTobacco, "Rec Tobacco");

			AddProperty(impl, Schema.OriginalWatermelon, "Orig. Watermelon");
			AddProperty(impl, Schema.ReconWatermelon, "Rec Watermelon");

			AddProperty(impl, Schema.OriginalWines, "Orig. Wines");
			AddProperty(impl, Schema.ReconWines, "Rec Wines");
			AddProperty(impl, Schema.MonthlyFiling, "Monthly Filing");
			AddProperty(impl, Schema.ChangedLinesOnly, "Changed Lines Only");
			AddProperty(impl, Schema.MPC, "MPC");
			AddProperty(impl, Schema.OriginalCustomsValue, "Original Customs Value");
		}

		#endregion

		static void AddProperty(ImportCollectionInfoImpl impl, string fieldName, string headerText, ZCharacterCasing casing = ZCharacterCasing.Normal)
		{
			impl.Add(new ImportPropertyInfoImpl<ReconFlattenedDataLine>(fieldName) { HeaderText = headerText, CharacterCasing = casing });
		}
	}
}
