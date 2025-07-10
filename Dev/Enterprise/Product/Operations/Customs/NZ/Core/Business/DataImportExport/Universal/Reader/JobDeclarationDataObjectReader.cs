using System;
using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class JobDeclarationDataObjectReader : JobDeclarationDataObjectReader<JobDeclaration, Bill, CusContainer, JobComInvoiceGroupHeader>
	{
		internal JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null)
			: base(declarationDataObject, logger, factory, shipment)
		{
		}

		protected new UniversalDataObjectReaderHelper Helper
		{
			get { return (UniversalDataObjectReaderHelper)base.Helper; }
		}

		protected override void AddFetchHintsRelatedToCommerialInvoiceLineTariff(JobDeclaration declaration, List<ZString> harmonisedCodes)
		{
			base.AddFetchHintsRelatedToCommerialInvoiceLineTariff(declaration, harmonisedCodes);
			foreach (var harmonisedCode in harmonisedCodes)
			{
				var fetchHint = UniversalTariffHelper.GetTariffFetchHint(factory.BOFactory, harmonisedCode);
				factory.BOFactory.AddFetchHint(fetchHint.table, fetchHint.query);
			}
		}

		protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}

		protected override void FillOrganizationsCore(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillOrganizationsCore(organizationAddressCollection, declaration, delaySetters);
			if (delaySetters == null)
			{
				var declarationRow = GetColumnIndexer(declaration);
				SetValue(declarationRow, JobDeclarationSchema.JE_OH_NotifyParty, GetDeliveryNotificationPartyPK(declaration));
			}
			else
			{
				FillNZOrganization((SchemaGuidColumn column, Func<JobDeclaration, ZGuid?> getOrganisationPK, Func<SchemaColumn, string> getColumnName) =>
				{
					SetValueWithDelay(declaration, column, () => getOrganisationPK(declaration), delaySetters, JobDeclarationSchema.PK, getColumnName);
				});
			}
		}

		protected override AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			return new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
		}

		protected override CustomsContainerDataObjectReader<JobDeclaration, CusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CustomsContainerDataObjectReader<JobDeclaration, CusContainer>(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		void FillNZOrganization(Action<SchemaGuidColumn, Func<JobDeclaration, ZGuid?>, Func<SchemaColumn, string>> setValue)
		{
			setValue(JobDeclarationSchema.JE_OH_NotifyParty, GetDeliveryNotificationPartyPK, (x) => JobDeclaration.Schema.JE_OH_NotifyParty);
		}

		ZGuid? GetDeliveryNotificationPartyPK(JobDeclaration declaration)
		{
			return Helper.GetOrganisationPK(this, dataObject, declaration, Constants.AddressType.DeliveryNotificationParty, OrganisationTypes.None);
		}
	}
}
