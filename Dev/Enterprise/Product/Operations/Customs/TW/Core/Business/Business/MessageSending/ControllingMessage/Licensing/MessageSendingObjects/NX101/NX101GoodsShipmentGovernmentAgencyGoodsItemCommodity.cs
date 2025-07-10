using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity : ICommodity
	{
		public NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity(CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
			invoiceHeader = Argument.NotNull(invoiceLine.InvoiceHeader, "invoiceLine.InvoiceHeader");
			this.header = header;
			isCertificateType01 = header.TW1_CertificateType == CertificateTypeList.Codes.Code1;
		}

		readonly CusTWControllingMessageHeader header;
		readonly JobComInvoiceLine invoiceLine;
		readonly JobComInvoiceHeader invoiceHeader;
		readonly bool isCertificateType01;

		IEnumerable<IAdditionalDocument> ICommodity.AdditionalDocuments => null;

		ZString ICommodity.CommercialCategorizationID => isCertificateType01 ? ZString.Empty : invoiceLine.JI_Model;

		ZString ICommodity.Description => invoiceLine.NX101PermitGoodsDescription.Left(512);

		ZString ICommodity.GoodsGroupNameCode => ZString.Empty;

		ZString ICommodity.Name => isCertificateType01 ? ZString.Empty : invoiceLine.JI_BrandName;

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

		ICommodityRelatedPackaging ICommodity.CommodityRelatedPackaging => new CommodityRelatedPackagingWrapper(ZString.Empty, ZString.Empty, invoiceLine.JI_InnerPackDescription);

		IConstituent ICommodity.Constituent => isCertificateType01 ? default : new ConstituentWrapper(invoiceLine.JI_Compositions, ZString.Empty, ZString.Empty);

		ICommodityDutyTaxFee ICommodity.DutyTaxFee => null;

		IGovernmentProcedure ICommodity.GovernmentProcedure => null;

		IEnumerable<ZString> ICommodity.HandlingInstructionsCodes => null;

		IInvoiceLine ICommodity.InvoiceLine => header.IsCertificate15 ? new NX101InvoiceLine(invoiceHeader.JZ_RX_NKInvoice_Currency, invoiceLine.JI_PermitUnitPrice * invoiceLine.JI_PermitQty) : null;

		IInvoice ICommodity.Invoice
		{
			get
			{
				IInvoice result = null;
				var certificateType = header.TW1_CertificateType;
				if (certificateType == CertificateTypeList.Codes.Code9
					|| certificateType == CertificateTypeList.Codes.Code11
					|| certificateType == CertificateTypeList.Codes.Code12
					|| certificateType == CertificateTypeList.Codes.Code13
					|| certificateType == CertificateTypeList.Codes.Code14
					|| certificateType == CertificateTypeList.Codes.Code15)
				{
					var invoiceDate = invoiceHeader.JZ_InvoiceDate;
					if (invoiceDate.IsValid)
					{
						result = new InvoiceWrapper(invoiceHeader.JZ_InvoiceNumber, invoiceDate.Date);
					}
				}
				return result;
			}
		}

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
