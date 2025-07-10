using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity :
		ICommodity,
		IClassification,
		IConstituent,
		IInvoiceLine,
		IQuarantine,
		IAnimal,
		ICommodityRelatedPackaging,
		ICommodityDutyTaxFee,
		IWine,
		IFood,
		IGovernmentProcedure
	{
		protected CusTWControllingMessageHeader Header { get; }
		JobComInvoiceLine InvoiceLine { get; }
		CusEntryLine EntryLine => InvoiceLine.CusEntryLine;

		public LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			Header = header;
		}

		IEnumerable<IAdditionalDocument> ICommodity.AdditionalDocuments => GetAdditionalDocuments(InvoiceLine);

		protected virtual IEnumerable<IAdditionalDocument> GetAdditionalDocuments(JobComInvoiceLine invoiceLine)
		{
			var permitDocuments = invoiceLine.PermitCusSupportingCollection.Cast<PermitCusSupporting>().
				Where(x => !x.CSI_LineNo.IsEmpty).GroupBy(x => new { x.CSI_LineNo }).Select(x => x.First()).Select(x => new AdditionalDocumentWrapper(ZString.Empty, x.CSI_LineNo));
			return permitDocuments;
		}

		ZString ICommodity.CommercialCategorizationID => InvoiceLine.JI_Model;

		ZString ICommodity.Description => EntryLine?.CL_Calc_GoodsDescription ?? ZString.Empty;

		ZString ICommodity.GoodsGroupNameCode => InvoiceLine.JI_GoodsType;

		ZString ICommodity.Name => InvoiceLine.JI_BrandName;

		ZString ICommodity.BarCode => InvoiceLine.JI_BarCode;

		ZString ICommodity.ChineseDescription => InvoiceLine.JI_NDescription;

		ZString ICommodity.EnglishDescription => InvoiceLine.JI_Description;

		ZString ICommodity.CITESImportPermitID => null;

		ZString ICommodity.FTATariffCode => null;

		ZString ICommodity.SHTCImportPermitID => null;

		ZString ICommodity.TariffCodeExtensionCode => InvoiceLine.JI_TariffExtensionCode;

		IEnumerable<IClassification> ICommodity.Classifications => null;

		IClassification ICommodity.Classification => this;

		ICommodityRelatedPackaging ICommodity.CommodityRelatedPackaging => this;

		IConstituent ICommodity.Constituent => this;

		ICommodityDutyTaxFee ICommodity.DutyTaxFee => this;

		IGovernmentProcedure ICommodity.GovernmentProcedure => this;

		IEnumerable<ZString> ICommodity.HandlingInstructionsCodes => InvoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.Select(x => x.JG_ReferenceNumber);

		IInvoiceLine ICommodity.InvoiceLine => this;

		IInvoice ICommodity.Invoice => null;

		IPreviousDocument ICommodity.PreviousDocument => new PreviousDocumentWrapper(InvoiceLine.PreviousPermitNo);

		IEnumerable<ICommodityNumber> ICommodity.CommodityNumbers => null;

		IEnumerable<IDutyOtherTaxFee> ICommodity.DutyOtherTaxFees => null;

		IDutyTaxFeeAmount ICommodity.DutyTaxFeeAmount => null;

		IDutyTaxFeeQuantity ICommodity.DutyTaxFeeQuantity => null;

		IFood ICommodity.Food => this;

		IQuarantine ICommodity.Quarantine => this;

		IVehicle ICommodity.Vehicle => null;

		IWine ICommodity.Wine => this;

		ZString ICommodity.CargoDescription => null;

		ZString ICommodity.BondedNoteCode => null;

		IEnumerable<ZString> ICommodity.VehicleIDs => null;

		ZString ICommodity.PrintingTariffCode => null;

		#region
		ZString IGovernmentProcedure.TransportTypeCode => default;

		ZString IGovernmentProcedure.CurrentCode => GetCurrentCodeCore();

		protected virtual ZString GetCurrentCodeCore() => InvoiceLine.JI_Procedure;

		ZString IGovernmentProcedure.Description => default;
		#endregion

		#region IClassification

		ZString IClassification.ID => InvoiceLine.JI_Tariff;

		ZString IClassification.IdentificationTypeCode => ZString.Empty;

		#endregion

		#region IQuarantine members
		ZString IQuarantine.ObjectFeature => InvoiceLine.JI_QuarantineFeatures;

		ZString IQuarantine.Treatment => InvoiceLine.JI_QuarantineTreatment;

		IAnimal IQuarantine.Animal => this;

		IEnumerable<IAdditionalDocument> IQuarantine.AdditionalDocument => InvoiceLine.SlaughterDateCollection.Cast<SlaughterDate>().Select(x => new AdditionalDocumentWrapper(x.CY_Date));

		IEnumerable<IAdditionalInformation> IQuarantine.AdditionalInformation => InvoiceLine.PackingHouseCollection.Cast<PackingHouse>().Select(x => new AdditionalInformationWrapper(x.CY_Code));

		IEnumerable<IPackaging> IQuarantine.Packing => InvoiceLine.PackingDateCollection.Cast<PackingDate>().Select(x => new PackingWarpper(x.CY_Date));

		#region IAnimal members
		ZInt IAnimal.AgeMonthNumeric => InvoiceLine.JI_AnimalAgeMonth;

		ZInt IAnimal.AgeYearNumeric => InvoiceLine.JI_AnimalAgeYear;

		ZInt IAnimal.FemaleQuantity => InvoiceLine.JI_AnimalFemaleQty;

		ZInt IAnimal.MaleQuantity => InvoiceLine.JI_AnimalMaleQty;

		ZString IAnimal.MicrochipID => InvoiceLine.JI_MicrochipID;

		ZString IAnimal.Vaccination => InvoiceLine.JI_VaccinationTypeDate;
		#endregion
		#endregion

		#region IFood members
		ZDecimal? IFood.PHValueNumeric => InvoiceLine.JI_PHValueNumeric;

		ZDecimal? IFood.SterilizationValueNumeric => InvoiceLine.JI_SterilizationValueNumeric;

		IEnumerable<IFoodConstituent> IFood.Constituents => InvoiceLine.FoodDataCollection.Select(x => new FoodConstituentWrapper(x.CY_Data, x.Content));
		#endregion

		#region IConstituent
		ZString IConstituent.ElementDescription => InvoiceLine.JI_Compositions;

		ZString IConstituent.LevelID => InvoiceLine.JI_ProductGrade;

		ZString IConstituent.Thickness => InvoiceLine.JI_ProductThickness;
		#endregion

		#region IInvoiceLine
		ZString IInvoiceLine.ChargesTypeCode
		{
			get
			{
				var incoTerms = Calc_InvoiceHeaders.Select(x => x.JZ_IncoTerm).ToHashSet();
				return incoTerms.Count == 1 ? incoTerms.Single() : (ZString)Core.Constants.IncoTerms.FreeOnBoard;
			}
		}

		ZString IInvoiceLine.CurrencyTypeCode
		{
			get
			{
				var currencies = Calc_InvoiceHeaders.Select(x => x.JZ_RX_NKInvoice_Currency).ToHashSet();
				return currencies.Count == 1 ? currencies.Single() : (ZString)Core.Constants.CurrencyCodes.Taiwan;
			}
		}

		ISet<JobComInvoiceHeader> Calc_InvoiceHeaders => Header.Factory.GetValue(ref invoiceHeadersCached, () => Header.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().Select(x => x.Invoiceline.InvoiceHeader).WhereNotNull().ToHashSet());
		CachedProperty<ISet<JobComInvoiceHeader>> invoiceHeadersCached;

		ZDecimal IInvoiceLine.UnitPriceAmount => GetUnitPriceAmountCore();

		protected virtual ZDecimal GetUnitPriceAmountCore() => InvoiceLine.JI_EnteredUnitPrice;

		ZDecimal IInvoiceLine.ItemChargeAmount => GetItemChargeAmountCore();

		protected virtual ZDecimal GetItemChargeAmountCore() => InvoiceLine.JI_Calc_FOB_InLocalCurrency;

		ZDecimal IInvoiceLine.SubTotalAmount => InvoiceLine.JI_LinePrice;
		#endregion

		#region ICommodityRelatedPackaging members
		ZString ICommodityRelatedPackaging.PackingMethodDescription => InvoiceLine.JI_InnerPackType;

		ZString ICommodityRelatedPackaging.MaterialCode => InvoiceLine.JI_InnerPackingMaterial;

		ZString ICommodityRelatedPackaging.Specification => InvoiceLine.JI_InnerPackDescription;
		#endregion

		#region ICommodityDutyTaxFee members
		ZDecimal ICommodityDutyTaxFee.AdValoremTaxBaseAmount => GetAdValoremTaxBaseAmountCore();

		protected virtual ZDecimal GetAdValoremTaxBaseAmountCore() => EntryLine?.CL_CustomsValue ?? ZDecimal.Zero;

		ZString ICommodityDutyTaxFee.DutyRegimeCode => default;

		ZDecimal ICommodityDutyTaxFee.SpecificTaxBaseQuantity => default;

		ZDecimal ICommodityDutyTaxFee.PercentageNumeric => default;
		#endregion

		#region IWine
		ZInt IWine.AgeNumeric => InvoiceLine.JI_AlcoholAge;

		ZDecimal IWine.AlcoholContentNumeric => InvoiceLine.JI_AlcoholPercentage;

		ZDateTime IWine.BottledDate => InvoiceLine.JI_BottledDate;

		ZDecimal IWine.CoverLotNumberAmount => InvoiceLine.JI_AlteredLotNoAmt;

		ZString IWine.GeographicRegion => InvoiceLine.JI_AlcoholCountryRegion;

		ZDecimal IWine.OriginalNonLotNumberAmount => InvoiceLine.JI_NoOriginalLotNoAmt;

		ZDateTime IWine.ProductBestBeforeDateTime => InvoiceLine.JI_ExpirationDate;

		ZDateTime IWine.ProductExpiryDateTime => InvoiceLine.JI_AlcoholEndOfShelfLife;

		ZDecimal IWine.RemoveLotNumberAmount => InvoiceLine.JI_RemovedLotNoAmt;

		ZInt IWine.YearNumeric => InvoiceLine.JI_AlcoholYear;
		#endregion
	}
}
