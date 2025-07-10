using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWInvoiceHeaderDataObjectWriter : CommercialInvoiceHeaderDataObjectWriter
	{
		public TWInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, CusEntryHeader relatedEntry = null) : base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override CommercialInvoiceHeader PopulateDataObject(BaseJobComInvoiceHeader invoiceBO)
		{
			var commercialInvoice = base.PopulateDataObject(invoiceBO);
			var invoiceHeader = invoiceBO as JobComInvoiceHeader;
			if (invoiceHeader != null)
			{
				var marksAndNumbers = invoiceHeader.TW_MarksAndNumbers;
				if (!marksAndNumbers.IsEmpty)
				{
					commercialInvoice.MarksAndNumbers = marksAndNumbers;
				}
			}
			return commercialInvoice;
		}

		protected override void PopulateCommercialInvoiceLineFields(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
			var invoiceLineTW = invoiceLine as JobComInvoiceLine;
			var goodsDescription = invoiceLineTW?.JI_DeclarationGoodsDescription ?? ZString.Empty;
			if (!goodsDescription.IsEmpty)
			{
				invoiceLineData.DetailedDescription = goodsDescription;
			}
		}

		protected override List<CustomsSupportingInformation> GetInvoiceLineCustomsSupportingInformationCollectionCore(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = new List<CustomsSupportingInformation>();
			var baseResult = base.GetInvoiceLineCustomsSupportingInformationCollectionCore(invoiceLineBO);
			if (baseResult != null)
			{
				result.AddRange(baseResult);
			}

			var invoiceLine = invoiceLineBO as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				var lookups = invoiceLine.Lookups;
				var partyIdentifier = invoiceLine.PartyIdentifier;
				var information = new CustomsSupportingInformation
				{
					Category = foodDrugType,
					Type = new CodeDescriptionPair6Char { Code = partyIdentifier, Description = lookups.PartyIdentifierCodeList.GetDescriptionFromCode(partyIdentifier) }
				};
				AddReferenceNumbers(invoiceLine.CertificateNo, invoiceLine.AuthorizedPerson, information);
				result.Add(information);

				var typeApprovalPartyIdentifier = invoiceLine.TypeApprovalPartyIdentifier;
				information = new CustomsSupportingInformation
				{
					Category = typeApprovalType,
					Type = new CodeDescriptionPair6Char { Code = typeApprovalPartyIdentifier, Description = lookups.PartyIdentifierCodeList.GetDescriptionFromCode(typeApprovalPartyIdentifier) },
					Description = lookups.ExemptionCodeList.GetDescriptionFromCode(invoiceLine.ExemptionCode)
				};
				AddReferenceNumbers(invoiceLine.TypeApprovalCertificateNo, invoiceLine.TypeApprovalAuthorizedParty, information);
				result.Add(information);
			}
			return result;
		}

		static void AddReferenceNumbers(ZString referenceNumber, ZString referenceNumber2, CustomsSupportingInformation information)
		{
			if (information.ReferenceNumberCollection == null)
			{
				information.ReferenceNumberCollection = new List<Reference>();
			}
			information.ReferenceNumberCollection.Add(new Reference { Type = new EntryType { Code = "RN1" }, ReferenceNumber = referenceNumber });
			information.ReferenceNumberCollection.Add(new Reference { Type = new EntryType { Code = "RN2" }, ReferenceNumber = referenceNumber2 });
		}

		protected override List<AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = new List<AddInfo>();
			var baseResult = base.GetInvoiceLineAddInfoCollection(invoiceLineBO);
			if (baseResult != null)
			{
				result.AddRange(baseResult);
			}
			if (invoiceLineBO is JobComInvoiceLine invoiceLine)
			{
				result.Add(new AddInfo { Key = Constants.AddInfoKeys.ControllingMessage.PreviousPermitNumber, Value = invoiceLine.PreviousPermitNo });
				result.Add(new AddInfo { Key = Constants.AddInfoKeys.InvoiceLine.NX101ShippingMarks, Value = invoiceLine.NX101ShippingMarks });
			}
			return result;
		}

		readonly CodeDescriptionPair foodDrugType = new CodeDescriptionPair() { Code = CusSupportingInfoTypeList.Codes.MedicalInstrumentPartyIdentifier, Description = "FoodDrugType" };
		readonly CodeDescriptionPair typeApprovalType = new CodeDescriptionPair() { Code = CusSupportingInfoTypeList.Codes.TypeApprovalCertificateNumber, Description = "TypeApprovalType" };
	}
}
