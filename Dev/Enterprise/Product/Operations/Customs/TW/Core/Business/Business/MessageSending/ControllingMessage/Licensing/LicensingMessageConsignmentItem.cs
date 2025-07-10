using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageConsignmentItem : IConsignmentItem, ICommodity, IOrigin
	{
		JobComInvoiceLine FirstInvoiceLine { get; }

		public LicensingMessageConsignmentItem(JobComInvoiceLine invoiceLine)
		{
			FirstInvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		ZString IConsignmentItem.Split => default;

		ICommodity IConsignmentItem.Commodity => this;

		IGoodsMeasure IConsignmentItem.GoodsMeasure => default;

		IPackaging IConsignmentItem.Packaging => default;

		IEnumerable<ITransportContractDocument> IConsignmentItem.TransportContractDocuments => default;

		IOrigin IConsignmentItem.Origin => this;

		ZString IConsignmentItem.AssociatedGovernmentProcedureCode => default;

		#region IOrigin members
		ZString IOrigin.CountryCode => FirstInvoiceLine.JI_CountryOfOrigin;

		IAdditionalDocument IOrigin.AdditionalDocument => default;
		#endregion

		#region ICommodity members
		IEnumerable<IAdditionalDocument> ICommodity.AdditionalDocuments => default;

		ZString ICommodity.CommercialCategorizationID => default;

		ZString ICommodity.Description => FirstInvoiceLine.CusEntryLine?.CL_Calc_GoodsDescription ?? ZString.Empty;

		ZString ICommodity.GoodsGroupNameCode => FirstInvoiceLine.JI_GoodsType;

		ZString ICommodity.Name => FirstInvoiceLine.JI_BrandName;

		ZString ICommodity.BarCode => default;

		ZString ICommodity.ChineseDescription => default;

		ZString ICommodity.EnglishDescription => default;

		ZString ICommodity.CITESImportPermitID => default;

		ZString ICommodity.FTATariffCode => default;

		ZString ICommodity.SHTCImportPermitID => default;

		ZString ICommodity.TariffCodeExtensionCode => default;

		IEnumerable<IClassification> ICommodity.Classifications => default;

		IClassification ICommodity.Classification => default;

		ICommodityRelatedPackaging ICommodity.CommodityRelatedPackaging => CommodityRelatedPackagingWrapper.GetCommodityRelatedPackaging(FirstInvoiceLine);

		IConstituent ICommodity.Constituent => default;

		ICommodityDutyTaxFee ICommodity.DutyTaxFee => default;

		IGovernmentProcedure ICommodity.GovernmentProcedure => default;

		IEnumerable<ZString> ICommodity.HandlingInstructionsCodes => default;

		IInvoiceLine ICommodity.InvoiceLine => default;

		IInvoice ICommodity.Invoice => default;

		IPreviousDocument ICommodity.PreviousDocument => default;

		IEnumerable<ICommodityNumber> ICommodity.CommodityNumbers => default;

		IEnumerable<IDutyOtherTaxFee> ICommodity.DutyOtherTaxFees => default;

		IDutyTaxFeeAmount ICommodity.DutyTaxFeeAmount => default;

		IDutyTaxFeeQuantity ICommodity.DutyTaxFeeQuantity => default;

		IFood ICommodity.Food => default;

		IQuarantine ICommodity.Quarantine => default;

		IVehicle ICommodity.Vehicle => default;

		IWine ICommodity.Wine => default;

		ZString ICommodity.CargoDescription => default;

		ZString ICommodity.BondedNoteCode => default;

		IEnumerable<ZString> ICommodity.VehicleIDs => default;

		ZString ICommodity.PrintingTariffCode => default;
		#endregion
	}
}
