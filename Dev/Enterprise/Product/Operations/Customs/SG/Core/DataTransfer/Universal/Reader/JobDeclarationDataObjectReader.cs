using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal
{
	public class JobDeclarationDataObjectReader : JobDeclarationDataObjectReader<JobDeclaration, Bill, CusContainer, Customs.Business.BaseJobComInvoiceGroupHeader>
	{
		internal JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null)
			: base(declarationDataObject, logger, factory, shipment)
		{
		}

		protected override CustomsContainerDataObjectReader<JobDeclaration, CusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CustomsContainerDataObjectReader<JobDeclaration, CusContainer>(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		protected override AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			return new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
		}

		protected override CommercialInvoiceHeaderDataObjectReader<Customs.Business.BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(Customs.Business.BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}

		protected override void FillCountrySpecificDetails(JobDeclaration declaration, Shipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillCountrySpecificDetails(declaration, dataObject, delaySetters);
			FillTradersRemarks(declaration);
		}

		void FillTradersRemarks(JobDeclaration declaration)
		{
			declaration.TradersRemarks.RemoveAndDeleteAll();
			var tradersRemarks = dataObject.CustomsReferenceCollection?.Where(x => x.Type.Code.GetValueOrDefault() == CusSupportingInfoTypeList.Codes.TradersRemarks).OrderBy(x => x.Order).ToArray();
			if (tradersRemarks != null && tradersRemarks.Any())
			{
				foreach (var remark in tradersRemarks)
				{
					var newRemarkRow = GetColumnIndexer(declaration.TradersRemarks.AddNew());
					SetValue(newRemarkRow, CusSupportingInfoSchema.CSI_Description, remark.Reference);
				}
			}
		}

		protected override void AddFetchHintsRelatedToCommerialInvoiceLineTariff(JobDeclaration declaration, List<ZString> harmonisedCodes)
		{
			base.AddFetchHintsRelatedToCommerialInvoiceLineTariff(declaration, harmonisedCodes);
			var tariffCodes = harmonisedCodes.Select(code => code.Replace(".", ""));
			var fetchHintFactory = factory.BOFactory;
			foreach (var tariffCode in tariffCodes)
			{
				fetchHintFactory.AddFetchHint(typeof(TariffView), TariffView.Loader.GetEffectiveTariffFilter(fetchHintFactory, Core.Constants.CountryCodes.Singapore, Customs.Universal.Constants.TariffTypes.HarmonizedSystem, tariffCode, declaration.DateForDutyRate));
			}
		}

		protected override OrgAddress FillImporter(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var importer = base.FillImporter(organizationAddressCollection, declaration, delaySetters);
			if (importer != null && IsImport)
			{
				SetOrganisationPK(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OH_Consignee, importer, delaySetters);
			}
			return importer;
		}

		protected override OrgAddress FillSupplier(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var supplier = base.FillSupplier(organizationAddressCollection, declaration, delaySetters);
			if (supplier != null && IsExport)
			{
				SetOrganisationPK(GetColumnIndexer(declaration), JobDeclarationSchema.JE_OH_Exporter, supplier, delaySetters);
			}
			return supplier;
		}

		ZString? messageType;
		ZString MessageType
		{
			get
			{
				if (!messageType.HasValue)
				{
					messageType = dataObject.MessageType?.GetCodeAsUpperCase() ?? ZString.Empty;
				}
				return messageType.Value;
			}
		}

		bool IsImport => MessageType == MessageTypeCodeList.Codes.INP;

		bool IsExport => MessageType == MessageTypeCodeList.Codes.OUT;
	}
}
