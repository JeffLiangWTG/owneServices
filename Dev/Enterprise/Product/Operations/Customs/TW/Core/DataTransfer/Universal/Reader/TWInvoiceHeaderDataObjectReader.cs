using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.TW.DataTransfer.Constants;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader>
	{
		public TWInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override void FillCommercialInfo(IEnumerable<CommercialCharge> commercialInvoiceChargeCollection, ICommonNonApportionedChargeProvider<BaseInvoiceCharge> provider, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			CommercialChargeDataObjectReader<BaseInvoiceCharge>.FillCommercialInfo(commercialInvoiceChargeCollection, provider, logger, factory,
				(charge, logger, provider, factory) => new CommercialChargeDataObjectReader(charge, logger, provider, factory));
		}

		protected override void FillCustomizedFields(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
			base.FillCustomizedFields(invoiceLineData, invoiceLine);
			if (invoiceLine is JobComInvoiceLine twInvoiceLine)
			{
				FillInvoiceLineAddInfoCollection(invoiceLineData, twInvoiceLine);
				FillCustomsReferenceCollection(invoiceLineData, twInvoiceLine);
				FillInvoiceLineDeclarationGoodsDescription(invoiceLineData, twInvoiceLine);
			}
		}

		void FillCustomsReferenceCollection(CommercialInvoiceLine invoiceLineData, JobComInvoiceLine twInvoiceLine)
		{
			FillChassisNumbers(invoiceLineData, twInvoiceLine);
			FillAssignedNumbers(invoiceLineData, twInvoiceLine);
		}

		void FillChassisNumbers(CommercialInvoiceLine invoiceLineData, JobComInvoiceLine twInvoiceLine)
		{
			var carChassisNumbers = invoiceLineData.CustomsReferenceCollection?.Where(reference => (reference.Type?.Code ?? ZString.Empty) == AddInfoGroupTypeCodes.CarChassisNumberType);
			if (carChassisNumbers?.Any() ?? ZBool.False)
			{
				var chassisJobComInvLineRefsCollection = twInvoiceLine.ChassisJobComInvLineRefsCollection;
				foreach (var carChassis in carChassisNumbers)
				{
					var number = carChassis.Reference.GetValueOrDefault();
					if (!number.IsEmpty)
					{
						var referenceRow = GetColumnIndexer(chassisJobComInvLineRefsCollection.AddNew());
						SetValue(referenceRow, JobComInvLineRefsSchema.JG_ReferenceNumber, number);
					}
				}
			}
		}

		void FillAssignedNumbers(CommercialInvoiceLine invoiceLineData, JobComInvoiceLine twInvoiceLine)
		{
			var assignedJobComInvLineRefs = invoiceLineData.CustomsReferenceCollection?.Where(reference => (reference.Type?.Code ?? ZString.Empty) == CusSupportingInfoTypeList.Codes.AssignedNumber);
			if (assignedJobComInvLineRefs?.Any() ?? ZBool.False)
			{
				var assignedJobComInvLineRefsCollection = twInvoiceLine.AssignedJobComInvLineRefsCollection;
				foreach (var assignedJobComInvLineRef in assignedJobComInvLineRefs)
				{
					var number = assignedJobComInvLineRef.Reference.GetValueOrDefault();
					if (!number.IsEmpty)
					{
						var referenceRow = GetColumnIndexer(assignedJobComInvLineRefsCollection.AddNew());
						SetValue(referenceRow, JobComInvLineRefsSchema.JG_ReferenceNumber, number);
					}
				}
			}
		}

		void FillInvoiceLineAddInfoCollection(CommercialInvoiceLine invoiceLineData, JobComInvoiceLine twInvoiceLine)
		{
			var addInfoCollection = invoiceLineData.AddInfoCollection;
			if (addInfoCollection != null)
			{
				var previousPermitNumber = addInfoCollection.GetZStringValue(Constants.AddInfoKeys.ControllingMessage.PreviousPermitNumber, logger).GetValueOrDefault();
				if (!previousPermitNumber.IsEmpty)
				{
					var referenceRow = GetColumnIndexer(twInvoiceLine.PrePermitNoCusSupporting);
					SetValue(referenceRow, CusSupportingInfoSchema.CSI_ReferenceNumber, previousPermitNumber);
				}
			}
		}

		void FillInvoiceLineDeclarationGoodsDescription(CommercialInvoiceLine invoiceLineData, JobComInvoiceLine twInvoiceLine)
		{
			var detailedDescription = invoiceLineData.DetailedDescription.GetValueOrDefault();
			if (!detailedDescription.IsEmpty)
			{
				twInvoiceLine.JI_DeclarationGoodsDescription = detailedDescription;
			}
		}

		protected override void FillMarksAndNumbers(BaseJobComInvoiceHeader invoice, Dictionary<string, ValueSetter> delaySetters)
		{
			var twInvoice = (JobComInvoiceHeader)invoice;
			var invoiceRow = GetColumnIndexer(twInvoice);
			var markAndNumbers = dataObject.MarksAndNumbers.GetValueOrDefault();
			SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_MarksAndNumbers, markAndNumbers.Left(JobComInvoiceHeader.Schema.JZ_MarksAndNumbersMaxLength), delaySetters);
			twInvoice.TW_MarksAndNumbersOverFlow = markAndNumbers.SubstringSafe(JobComInvoiceHeader.Schema.JZ_MarksAndNumbersMaxLength);
		}

		protected override BaseJobComInvoiceHeader GetNewInvoice()
		{
			var invoice = (JobComInvoiceHeader)base.GetNewInvoice();
			invoice.JustAddedByDataObjectReader = true;
			return invoice;
		}

		protected override void FillOrganizationsCore(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillOrganizationsCore(invoice, commercialInvoiceHeaderRelatedData, delaySetters);

			if (invoice is JobComInvoiceHeader twInvoice)
			{
				FillDocumentaryAddressIfNeeded(twInvoice, dataObject.Supplier, DocAddressTypes.Codes.SupplierDocumentaryAddress, nameof(DocAddressTypes.Codes.SupplierDocumentaryAddress), OrganisationTypes.Consignor);
				FillDocumentaryAddressIfNeeded(twInvoice, dataObject.Buyer, DocAddressTypes.Codes.BuyerDocumentaryAddress, nameof(DocAddressTypes.Codes.BuyerDocumentaryAddress), OrganisationTypes.Consignee);
			}
		}

		void FillDocumentaryAddressIfNeeded(JobComInvoiceHeader twInvoice, OrganizationAddress address, ZString docAddressType, ZString docAddressTypeName, OrganisationTypes orgCategory)
		{
			if (address != null && !(dataObject.OrganizationAddressCollection?.Any(x => x.AddressType.GetValueOrDefault().Equals(docAddressTypeName)) ?? false))
			{
				var infoMatcher = helper.CommercialInfoMatcher;
				if (infoMatcher == null || !infoMatcher.TryGetMatchedSupplier(address, out var orgAddress))
				{
					orgAddress = new OrganisationDataObjectReader(address, logger, factory).GetMatched(twInvoice, orgCategory);
				}

				if (orgAddress != null && orgAddress.PK.IsValid)
				{
					var query = new ZQuery(JobDocAddressSchema.E2_AddressType, docAddressType);
					query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, 0);
					query.AddToFilter(JobDocAddressSchema.E2_ParentID, twInvoice.PK);
					query.FetchOnlyFromLocalCache = true;

					var docAddress = factory.LoadTop1<JobDocAddress>(query);
					if (docAddress == null)
					{
						docAddress = factory.New<JobDocAddress>();
						docAddress.E2_ParentID = twInvoice.PK;
						docAddress.E2_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
						docAddress.E2_AddressType = docAddressType;
						docAddress.E2_AddressSequence = 0;
					}

					var docAddresRow = GetColumnIndexer(docAddress);
					SetValue(docAddresRow, JobDocAddressSchema.E2_OA_Address, orgAddress.PK);
					SetValue(docAddresRow, JobDocAddressSchema.E2_AddressOverride, ZBool.False);
				}
			}
		}

		protected override Customs.DataTransfer.Universal.CustomsSupportingInformationCollectionDataObjectReader CreateNewCustomsSupportingInformationCollectionDataObjectReader()
		{
			return new CustomsSupportingInformationCollectionDataObjectReader(logger, helper);
		}
	}
}
