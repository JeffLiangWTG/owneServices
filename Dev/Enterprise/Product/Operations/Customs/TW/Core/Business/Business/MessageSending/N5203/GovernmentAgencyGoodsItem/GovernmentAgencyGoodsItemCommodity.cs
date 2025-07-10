using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class Commodity : ICommodity
	{
		public Commodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			this.EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
			this.FirstInvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		public virtual IEnumerable<IAdditionalDocument> AdditionalDocuments
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					foreach (var additionalDocument in GetAdditionalDocuments(invoiceLine))
					{
						yield return additionalDocument;
					}
				}
			}
		}

		protected IEnumerable<IAdditionalDocument> GetAdditionalDocuments(JobComInvoiceLine invoiceLine)
		{
			var permitDocuments = invoiceLine.PermitCusSupportingCollection.Cast<PermitCusSupporting>().
				Where(x => !x.CSI_ReferenceNumber.IsEmpty).GroupBy(x => new { x.CSI_ReferenceNumber, x.CSI_LineNo }).Select(x => x.First()).Select(x => new AdditionalDocumentWrapper(x.CSI_ReferenceNumber, x.CSI_LineNo));
			var exemptionOfControllingAgenciesDocuments = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.Cast<ExemptionOfControllingAgenciesCusSupporting>().
				Where(x => !x.CSI_ReferenceNumber.IsEmpty).GroupBy(x => new { x.CSI_ReferenceNumber }).Select(x => x.First()).Select(x => new AdditionalDocumentWrapper(x.CSI_ReferenceNumber, 0));
			return permitDocuments.Concat(exemptionOfControllingAgenciesDocuments).Take(5);
		}

		public ZString CommercialCategorizationID => FirstInvoiceLine.JI_Model.ExcludeNonValidXMLCharacters();

		public virtual ZString Description => EntryLine.CL_Calc_GoodsDescription.ExcludeNonValidXMLCharacters();

		public ZString GoodsGroupNameCode => ZString.Empty;

		public ZString Name
		{
			get
			{
				var brand = FirstInvoiceLine.JI_BrandName.ExcludeNonValidXMLCharacters();
				if (brand.IsEmpty)
				{
					brand = MessageConstants.NoBrand;
				}
				return brand;
			}
		}

		public ZString BarCode => ZString.Empty;

		public ZString ChineseDescription => ZString.Empty;

		public ZString EnglishDescription => ZString.Empty;

		public ZString CITESImportPermitID => ZString.Empty;

		public ZString FTATariffCode => ZString.Empty;

		public ZString SHTCImportPermitID => ZString.Empty;

		public ZString TariffCodeExtensionCode => ZString.Empty;

		public virtual IEnumerable<IClassification> Classifications => EntryLine.GetClassifications((hazMatCode, idTypeCode) => new ClassificationWrapper(hazMatCode, idTypeCode), false);

		public ICommodityRelatedPackaging CommodityRelatedPackaging => null;

		public IConstituent Constituent => new Constituent(FirstInvoiceLine);

		public ICommodityDutyTaxFee DutyTaxFee => null;

		public IGovernmentProcedure GovernmentProcedure => null;

		public IEnumerable<ZString> HandlingInstructionsCodes => null;

		public IInvoiceLine InvoiceLine => invoiceLineCached ?? (invoiceLineCached = InvoiceLineCore);
		IInvoiceLine invoiceLineCached;

		protected virtual IInvoiceLine InvoiceLineCore => new CommodityInvoiceLine(EntryLine);

		public IPreviousDocument PreviousDocument => null;

		public IEnumerable<ICommodityNumber> CommodityNumbers
		{
			get
			{
				var customsOwnerPartNo = FirstInvoiceLine.JI_CustomsOwnerPartNo;
				if (!customsOwnerPartNo.IsEmpty)
				{
					yield return new CommodityNumberWrapper(customsOwnerPartNo, MessageConstants.IdentificationTypeCodes.BP);
				}

				var customsSupplierPartNo = FirstInvoiceLine.JI_CustomsSupplierPartNo;
				if (!customsSupplierPartNo.IsEmpty)
				{
					yield return new CommodityNumberWrapper(customsSupplierPartNo, MessageConstants.IdentificationTypeCodes.SA);
				}
			}
		}

		public IEnumerable<IDutyOtherTaxFee> DutyOtherTaxFees
		{
			get
			{
				foreach (var lineFee in EntryLine.GetDutyOtherTaxFees())
				{
					var chargeType = SharedHelper.GetTypeCodeForDutyTaxFee(lineFee.CF_ChargeType, lineFee.CF_MethodOfPayment, ZBool.False);
					var rate = lineFee.CF_Rate;
					if (!chargeType.IsEmpty && !rate.IsEmpty)
					{
						yield return new DutyOtherTaxFeeWrapper(chargeType, UniversalReferenceConstants.MethodOfCalculation.Percentage, rate);
					}
				}
			}
		}

		public IDutyTaxFeeAmount DutyTaxFeeAmount => null;

		public IDutyTaxFeeQuantity DutyTaxFeeQuantity => null;

		public IFood Food => null;

		public IQuarantine Quarantine => null;

		public IVehicle Vehicle => null;

		public IWine Wine => null;

		public ZString CargoDescription => ZString.Empty;

		public ZString BondedNoteCode => EntryLine.CL_BondedGoodsCode;

		public virtual IEnumerable<ZString> VehicleIDs
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
				{
					foreach (var vehicleID in invoiceLine.GetVehicleIDs())
					{
						yield return vehicleID;
					}
				}
			}
		}

		IClassification ICommodity.Classification => null;

		public IInvoice Invoice => null;

		public ZString PrintingTariffCode => null;

		protected readonly CusEntryLine EntryLine;
		protected readonly JobComInvoiceLine FirstInvoiceLine;
	}
}
