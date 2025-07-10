using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class Commodity : ICommodity,
		IClassification,
		IConstituent,
		ICommodityDutyTaxFee,
		IDutyTaxFeeAmount,
		IInvoiceLine
	{
		public Commodity(AsycudaPackedItem packedItem)
		{
			PackedItem = Argument.NotNull(packedItem, nameof(packedItem));
		}

		AsycudaPackedItem PackedItem { get; }
		const string BuyerCommodityType = "BA";
		const string SupplierCommodityType = "SA";

		IEnumerable<IAdditionalDocument> ICommodity.AdditionalDocuments => default;

		ZString ICommodity.CommercialCategorizationID => PackedItem.API_Model;

		ZString ICommodity.Description => PackedItem.API_GoodsDescription;

		ZString ICommodity.GoodsGroupNameCode => default;

		ZString ICommodity.Name => PackedItem.API_Brand;

		ZString ICommodity.BarCode => default;

		ZString ICommodity.ChineseDescription => default;

		ZString ICommodity.EnglishDescription => default;

		ZString ICommodity.CITESImportPermitID => default;

		ZString ICommodity.FTATariffCode => default;

		ZString ICommodity.SHTCImportPermitID => default;

		ZString ICommodity.TariffCodeExtensionCode => default;

		IEnumerable<IClassification> ICommodity.Classifications => default;

		IClassification ICommodity.Classification => this;

		ICommodityRelatedPackaging ICommodity.CommodityRelatedPackaging => default;

		IConstituent ICommodity.Constituent => this;

		ICommodityDutyTaxFee ICommodity.DutyTaxFee => this;

		IGovernmentProcedure ICommodity.GovernmentProcedure => new GovernmentProcedureWrapper(PackedItem.ModeOfStatisticsOrDutyTreatment);

		IEnumerable<ZString> ICommodity.HandlingInstructionsCodes => default;

		IInvoiceLine ICommodity.InvoiceLine => this;

		IInvoice ICommodity.Invoice => default;

		IPreviousDocument ICommodity.PreviousDocument => default;

		IEnumerable<ICommodityNumber> ICommodity.CommodityNumbers
		{
			get
			{
				if (!PackedItem.IsAir)
				{
					var buyerCommodity = PackedItem.API_CustomsBuyerPartNo;
					if (!buyerCommodity.IsEmpty)
					{
						yield return new CommodityNumberWrapper(buyerCommodity, BuyerCommodityType);
					}

					var supplierCommodity = PackedItem.API_CustomsSupplierPartNo;
					if (!supplierCommodity.IsEmpty)
					{
						yield return new CommodityNumberWrapper(supplierCommodity, SupplierCommodityType);
					}
				}
			}
		}

		IEnumerable<IDutyOtherTaxFee> ICommodity.DutyOtherTaxFees => Enumerable.Empty<DutyOtherTaxFeeWrapper>();

		IDutyTaxFeeAmount ICommodity.DutyTaxFeeAmount => this;

		IDutyTaxFeeQuantity ICommodity.DutyTaxFeeQuantity => new DutyTaxFeeQuantity(PackedItem);

		IFood ICommodity.Food => default;

		IQuarantine ICommodity.Quarantine => default;

		IVehicle ICommodity.Vehicle => default;

		IWine ICommodity.Wine => default;

		ZString ICommodity.CargoDescription => default;

		ZString ICommodity.BondedNoteCode => default;

		IEnumerable<ZString> ICommodity.VehicleIDs => default;

		ZString ICommodity.PrintingTariffCode => default;

		#region IClassification

		ZString IClassification.ID => PackedItem.API_Tariff;

		ZString IClassification.IdentificationTypeCode => default;

		#endregion

		#region IConstituent

		ZString IConstituent.ElementDescription => PackedItem.API_Remarks;

		ZString IConstituent.LevelID => default;

		ZString IConstituent.Thickness => default;

		#endregion

		#region ICommodityDutyTaxFee members

		ZDecimal ICommodityDutyTaxFee.AdValoremTaxBaseAmount => PackedItem.API_CustomsValue;

		ZString ICommodityDutyTaxFee.DutyRegimeCode => ZString.Empty;

		ZDecimal ICommodityDutyTaxFee.SpecificTaxBaseQuantity => PackedItem.API_CustomsQty2;

		ZDecimal ICommodityDutyTaxFee.PercentageNumeric => ZDecimal.Zero;

		#endregion

		#region IDutyTaxFeeAmount

		ZDecimal IDutyTaxFeeAmount.PercentageNumeric => default;

		ZDecimal IDutyTaxFeeAmount.TaxRateNumeric => ZDecimal.ParseSafe(PackedItem.AdValoremRateFormulaDerivedFrom, ZDecimal.Zero);

		#endregion

		#region IInvoiceLine members
		ZString IInvoiceLine.ChargesTypeCode => default;

		ZString IInvoiceLine.CurrencyTypeCode => default;

		ZDecimal IInvoiceLine.UnitPriceAmount => default;

		ZDecimal IInvoiceLine.ItemChargeAmount => PackedItem.API_CustomsValue;

		ZDecimal IInvoiceLine.SubTotalAmount => default;
		#endregion
	}
}
