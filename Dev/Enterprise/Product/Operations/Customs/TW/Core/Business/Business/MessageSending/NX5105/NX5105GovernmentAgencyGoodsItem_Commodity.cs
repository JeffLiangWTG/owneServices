using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105GovernmentAgencyGoodsItem_Commodity : ICommodity
	{
		public NX5105GovernmentAgencyGoodsItem_Commodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			EntryLine = Argument.NotNull(entryLine, "entryLine");
			InvoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
		}

		public CusEntryLine EntryLine { get; }

		protected JobComInvoiceLine InvoiceLine { get; }

		IEnumerable<IAdditionalDocument> ICommodity.AdditionalDocuments
		{
			get
			{
				var referenceAlreadyUsed = new List<ZString>();
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					var permitList = new List<ZString>();
					var environmentalProtectionTariffCode = invoiceLine.JI_Calc_EnvironmentalProtectionCode;
					if (!environmentalProtectionTariffCode.IsEmpty)
					{
						permitList.Add(environmentalProtectionTariffCode);
					}
					var invoiceLinePermitList = invoiceLine.AssignedJobComInvLineRefsCollection.Cast<AssignedJobComInvLineRefs>().Select(x => x.JG_ReferenceNumber).ToList();
					if (invoiceLinePermitList.Any())
					{
						permitList.AddRange(invoiceLinePermitList);
					}
					foreach (var referenceNumber in permitList.Take(10))
					{
						if (!referenceAlreadyUsed.Contains(referenceNumber))
						{
							referenceAlreadyUsed.Add(referenceNumber);
							yield return new AdditionalDocumentWrapper(referenceNumber);
						}
					}
				}
			}
		}

		ZString ICommodity.CommercialCategorizationID
		{
			get
			{
				var model = EntryLine.CL_Model.ExcludeNonValidXMLCharacters();
				if (model.IsEmpty && InvoiceLine.IsL1Declaration)
				{
					model = Constants.NIL;
				}
				return model;
			}
		}

		public virtual ZString Description => EntryLine.CL_Calc_GoodsDescription.ExcludeNonValidXMLCharacters();

		ZString ICommodity.Name
		{
			get
			{
				var brandName = EntryLine.CL_BrandName.ExcludeNonValidXMLCharacters();
				if (brandName.IsEmpty && InvoiceLine.IsL1Declaration)
				{
					brandName = Constants.NIL;
				}
				return brandName;
			}
		}

		ZString ICommodity.ChineseDescription => GetChineseDescriptionCore();

		ZString ICommodity.EnglishDescription => GetEnglishDescriptionCore();

		protected virtual ZString GetEnglishDescriptionCore()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetChineseDescriptionCore()
		{
			return ZString.Empty;
		}

		ZString ICommodity.CITESImportPermitID => InvoiceLine.CitesPermit;

		ZString ICommodity.FTATariffCode => (InvoiceLine.JI_PrimaryPreference == Constants.PreferenceCodes.Preference2 || InvoiceLine.JI_PrimaryPreference == Constants.PreferenceCodes.ProvisionalPreference2) ? Constants.PackType : "";

		ZString ICommodity.SHTCImportPermitID => InvoiceLine.HighTechLicense;

		IEnumerable<IClassification> ICommodity.Classifications => EntryLine.GetClassifications((hazMatCode, idTypeCode) => new ClassificationWrapper(hazMatCode, idTypeCode), true);

		#region Constituent
		IConstituent ICommodity.Constituent => ConstituentCore;

		protected virtual IConstituent ConstituentCore
		{
			get
			{
				var compositions = InvoiceLine.JI_Compositions;
				if (compositions.IsEmpty && InvoiceLine.IsL1Declaration)
				{
					compositions = Constants.NIL;
				}
				return !compositions.IsEmpty ? new ConstituentWrapper(compositions.ExcludeNonValidXMLCharacters(), ZString.Empty, ZString.Empty) : null;
			}
		}
		#endregion

		public ICommodityDutyTaxFee DutyTaxFee => dutyTaxFee ?? (dutyTaxFee = GetDutyTaxFeeCore());
		ICommodityDutyTaxFee dutyTaxFee;

		protected virtual ICommodityDutyTaxFee GetDutyTaxFeeCore() => new NX5105Commodity_DutyTaxFee(EntryLine, InvoiceLine);

		IGovernmentProcedure ICommodity.GovernmentProcedure => new GovernmentProcedureWrapper(EntryLine.CL_Procedure);

		IInvoiceLine ICommodity.InvoiceLine => GetInvoiceLine();

		protected virtual IInvoiceLine GetInvoiceLine() => new NX5105Commodity_InvoiceLine(EntryLine);

		IPreviousDocument ICommodity.PreviousDocument
		{
			get
			{
				IPreviousDocument previousDocument = null;
				if (!InvoiceLine.PreviousPermitNo.IsEmpty)
				{
					previousDocument = new PreviousDocumentWrapper(InvoiceLine.PreviousPermitNo);
				}
				return previousDocument;
			}
		}

		IEnumerable<ICommodityNumber> ICommodity.CommodityNumbers
		{
			get
			{
				var buyerCommodityNumber = EntryLine.CL_CustomsOwnerPartNo;
				var supplyerCommodityNumber = EntryLine.CL_SupplierPartNumber;
				if (!buyerCommodityNumber.IsEmpty)
				{
					yield return new CommodityNumberWrapper(buyerCommodityNumber, MessageConstants.IdentificationTypeCodes.BP);
				}
				if (!supplyerCommodityNumber.IsEmpty)
				{
					yield return new CommodityNumberWrapper(supplyerCommodityNumber, MessageConstants.IdentificationTypeCodes.SA);
				}
			}
		}

		IVehicle ICommodity.Vehicle
		{
			get
			{
				var vehicle = new NX5105Commodity_Vehicle(EntryLine);
				return vehicle.Empty ? null : vehicle;
			}
		}

		IWine ICommodity.Wine => WineCore;
		protected virtual IWine WineCore => !InvoiceLine.JI_AlcoholPercentage.IsEmpty ? new NX5105CommodityWine(InvoiceLine) : null;

		#region GoodsGroupNameCode
		ZString ICommodity.GoodsGroupNameCode => GoodsGroupNameCodeCore;

		protected virtual ZString GoodsGroupNameCodeCore => ZString.Empty;
		#endregion

		#region BarCode
		ZString ICommodity.BarCode => BarCodeCore;

		protected virtual ZString BarCodeCore => ZString.Empty;
		#endregion

		#region TariffCodeExtensionCode
		ZString ICommodity.TariffCodeExtensionCode => TariffCodeExtensionCodeCore;

		protected virtual ZString TariffCodeExtensionCodeCore => ZString.Empty;
		#endregion

		#region CommodityRelatedPackaging
		ICommodityRelatedPackaging ICommodity.CommodityRelatedPackaging => CommodityRelatedPackagingCore;

		protected virtual ICommodityRelatedPackaging CommodityRelatedPackagingCore => null;
		#endregion

		#region HandlingInstructionsCodes
		IEnumerable<ZString> ICommodity.HandlingInstructionsCodes => HandlingInstructionsCodesCore;

		protected virtual IEnumerable<ZString> HandlingInstructionsCodesCore => null;
		#endregion

		IEnumerable<IDutyOtherTaxFee> ICommodity.DutyOtherTaxFees
		{
			get
			{
				var rateCodeList = SharedHelper.GetRateCodesByType(EntryLine.Factory, UniversalReferenceConstants.RefCusRateTypes.CommodityTaxes);
				foreach (var lineFee in EntryLine.GetDutyOtherTaxFees())
				{
					var chargeType = SharedHelper.GetTypeCodeForDutyTaxFee(lineFee.CF_ChargeType, lineFee.CF_MethodOfPayment, ZBool.True);
					var rate = lineFee.CF_Rate;
					if (!chargeType.IsEmpty && !rate.IsEmpty)
					{
						yield return new DutyOtherTaxFeeWrapper(chargeType, lineFee.CF_MethodOfCalculation, rate);
					}
				}
			}
		}

		IDutyTaxFeeAmount ICommodity.DutyTaxFeeAmount
		{
			get
			{
				var fee = GetEntryLineFeeByChargeType(UniversalReferenceConstants.RefCusRateCodes.DTA);
				DutyTaxFeeAmountWrapper result = null;
				if (fee != null)
				{
					result = new DutyTaxFeeAmountWrapper(fee.CF_Rate);
				}
				else
				{
					var randomLine = EntryLine.RandomLine;
					if (randomLine?.UniversalTariff?.GetApplicableRates(randomLine.DutyRateSelectionCriteria).FirstOrDefault(x => x.RateCode == UniversalReferenceConstants.RefCusRateCodes.DTA) is Universal.RateView firstRate)
					{
						var unitAndRate = firstRate.ZZ2_RateFormulaDerivedFrom.Split(new char[] { '/' });
						result = new DutyTaxFeeAmountWrapper(ZDecimal.ParseSafe(unitAndRate[0], ZDecimal.Zero));
					}
				}

				return result;
			}
		}

		IDutyTaxFeeQuantity ICommodity.DutyTaxFeeQuantity
		{
			get
			{
				var fee = GetEntryLineFeeByChargeType(UniversalReferenceConstants.RefCusRateCodes.DTS);
				DutyTaxFeeQuantityWrapper result = null;
				if (fee != null)
				{
					result = new DutyTaxFeeQuantityWrapper(fee.CF_MethodOfCalculation, fee.CF_Rate);
				}
				else
				{
					var randomLine = EntryLine.RandomLine;
					if (randomLine?.UniversalTariff?.GetApplicableRates(randomLine.DutyRateSelectionCriteria).FirstOrDefault(x => x.RateCode == UniversalReferenceConstants.RefCusRateCodes.DTS) is Universal.RateView firstRate)
					{
						var unitAndRate = firstRate.ZZ2_RateFormulaDerivedFrom.Split(new char[] { '/' });
						result = new DutyTaxFeeQuantityWrapper(unitAndRate.Length > 1 ? unitAndRate[1] : ZString.Empty, ZDecimal.ParseSafe(unitAndRate[0], ZDecimal.Zero));
					}
				}

				return result;
			}
		}

		CusEntryLineFee GetEntryLineFeeByChargeType(string chargeType) => EntryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == chargeType);

		#region Quarantine
		IQuarantine ICommodity.Quarantine => GetQuarantineCore();

		protected virtual IQuarantine GetQuarantineCore() => null;
		#endregion

		#region Food
		IFood ICommodity.Food => GetFoodCore();
		protected virtual IFood GetFoodCore() => null;
		#endregion

		#region Not Applicable

		ZString ICommodity.CargoDescription => ZString.Empty;

		ZString ICommodity.BondedNoteCode => ZString.Empty;

		IEnumerable<ZString> ICommodity.VehicleIDs => null;

		IClassification ICommodity.Classification => null;

		public IInvoice Invoice => null;

		public ZString PrintingTariffCode => null;

		#endregion

	}
}
