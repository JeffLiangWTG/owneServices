using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GoodsShipmentGovernmentAgencyGoodsItemCommodityOnlyMandatory : ICommodity
	{
		public NX101GoodsShipmentGovernmentAgencyGoodsItemCommodityOnlyMandatory(JobComInvoiceLine invoiceLine, ZString description)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
			this.description = description;
		}

		readonly JobComInvoiceLine invoiceLine;
		readonly ZString description;

		IEnumerable<IAdditionalDocument> ICommodity.AdditionalDocuments => null;

		ZString ICommodity.CommercialCategorizationID => ZString.Empty;

		ZString ICommodity.Description => description;

		ZString ICommodity.GoodsGroupNameCode => ZString.Empty;

		ZString ICommodity.Name => ZString.Empty;

		ZString ICommodity.BarCode => ZString.Empty;

		ZString ICommodity.ChineseDescription => ZString.Empty;

		ZString ICommodity.EnglishDescription => ZString.Empty;

		ZString ICommodity.CITESImportPermitID => ZString.Empty;

		ZString ICommodity.FTATariffCode => ZString.Empty;

		ZString ICommodity.SHTCImportPermitID => ZString.Empty;

		ZString ICommodity.TariffCodeExtensionCode => ZString.Empty;

		IEnumerable<IClassification> ICommodity.Classifications
		{
			get
			{
				var tariff = invoiceLine.JI_Tariff;
				var impTariff = invoiceLine.JI_IMPTariff;
				if (!tariff.IsEmpty)
				{
					yield return new ClassificationWrapper(tariff, MessageConstants.IdentificationTypeCodes.HS);
				}

				if (!impTariff.IsEmpty)
				{
					yield return new ClassificationWrapper(impTariff, MessageConstants.IdentificationTypeCodes.ZZZ);
				}
			}
		}

		IClassification ICommodity.Classification => null;

		ICommodityRelatedPackaging ICommodity.CommodityRelatedPackaging => null;

		IConstituent ICommodity.Constituent => null;

		ICommodityDutyTaxFee ICommodity.DutyTaxFee => null;

		IGovernmentProcedure ICommodity.GovernmentProcedure => null;

		IEnumerable<ZString> ICommodity.HandlingInstructionsCodes => null;

		IInvoiceLine ICommodity.InvoiceLine => null;

		IInvoice ICommodity.Invoice => null;

		IPreviousDocument ICommodity.PreviousDocument => null;

		IEnumerable<ICommodityNumber> ICommodity.CommodityNumbers => null;

		IEnumerable<IDutyOtherTaxFee> ICommodity.DutyOtherTaxFees => null;

		IDutyTaxFeeAmount ICommodity.DutyTaxFeeAmount => null;

		IDutyTaxFeeQuantity ICommodity.DutyTaxFeeQuantity => null;

		IFood ICommodity.Food => null;

		IQuarantine ICommodity.Quarantine => null;

		IVehicle ICommodity.Vehicle => null;

		IWine ICommodity.Wine => null;

		ZString ICommodity.CargoDescription => ZString.Empty;

		ZString ICommodity.BondedNoteCode => ZString.Empty;

		IEnumerable<ZString> ICommodity.VehicleIDs => null;

		ZString ICommodity.PrintingTariffCode => invoiceLine.JI_TariffPrintLength;
	}
}
