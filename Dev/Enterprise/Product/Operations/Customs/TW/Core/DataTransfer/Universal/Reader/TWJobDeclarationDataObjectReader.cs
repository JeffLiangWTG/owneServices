using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWJobDeclarationDataObjectReader : JobDeclarationDataObjectReader<JobDeclaration, Bill, CusContainer, JobComInvoiceGroupHeader>
	{
		public TWJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null) : base(declarationDataObject, logger, factory, shipment)
		{
		}

		protected override CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, JobDeclaration declaration)
		{
			return new TWEntryInstructionDataObjectReader(entryInstructionDataObject, logger, Helper, factory, declaration);
		}

		protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new TWInvoiceHeaderDataObjectReader(groupHeader, invoiceData, logger, Helper, dataObject, landedCostDataReader);
		}

		protected override void FillOrganizationsCore(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillOrganizationsCore(organizationAddressCollection, declaration, delaySetters);
			var declarationRow = GetColumnIndexer(declaration);

			FillOrganizations(organizationAddressCollection, declaration, delaySetters, declarationRow, Constants.AddressTypes.NotifyParty, JobDeclarationSchema.JE_OH_NotifyParty, OrganisationTypes.None);

			if (declaration is JobDeclaration decl)
			{
				var bondedFactoryOrganizationAddresses = organizationAddressCollection.FindAll(x => x.AddressType.GetValueOrDefault() == Constants.AddressTypes.PreviousBondedFactory && !x.AddressOverride.GetValueOrDefault());
				if (bondedFactoryOrganizationAddresses.Any())
				{
					FillBondedFactories(bondedFactoryOrganizationAddresses, decl, delaySetters);
				}

				FillSupplierTranslatedDocumentaryAddress(organizationAddressCollection, decl, delaySetters);
				FillImporterTranslatedDocumentaryAddress(organizationAddressCollection, decl, delaySetters);
			}
		}

		void FillOrganizations(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters, IColumnIndexer declarationRow, ZString addressType, SchemaGuidColumn addressColumn, OrganisationTypes organisationType)
		{
			var organizationAddress = organizationAddressCollection.FirstOrDefault(addressType);
			if (organizationAddress != null)
			{
				var organizationAddressReader = new OrganisationDataObjectReader(organizationAddress, logger, factory);
				SetOrganisationPK(declarationRow, addressColumn, organizationAddressReader.GetMatched(declaration, organisationType), delaySetters);
			}
		}

		void FillBondedFactories(List<OrganizationAddress> bondedFactoryOrganizationAddresses, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var bondedFactories = declaration.BondedFactories;
			var bondedFactoriesEnumerable = bondedFactories.Cast<BondedFactory>();
			foreach (var address in bondedFactoryOrganizationAddresses)
			{
				var organizationAddressReader = new OrganisationDataObjectReader(address, logger, factory);
				var orgAddress = organizationAddressReader?.GetMatched();
				if (orgAddress != null && !bondedFactoriesEnumerable.Any(x => x.OrganisationPK == orgAddress.OA_OH && x.E2_OA_Address == orgAddress.PK))
				{
					var bondedFactoryRow = GetColumnIndexer(bondedFactories.AddNew());
					SetValue(bondedFactoryRow, JobDocAddressSchema.E2_OA_Address, orgAddress.PK, delaySetters);
				}
			}
		}

		void FillSupplierTranslatedDocumentaryAddress(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var localAddress = supplierDocumentaryAddress?.LocalAddress;
			if (localAddress != null && supplierDocumentaryAddress.E2_AddressOverride)
			{
				var supplierTranslatedDocumentaryAddress = organizationAddressCollection.FirstOrDefault(nameof(DocAddressType.SupplierTranslatedDocumentaryAddress));
				SetValuesForTranslatedDocumentaryAddress(localAddress, supplierTranslatedDocumentaryAddress, delaySetters);
			}
		}

		void FillImporterTranslatedDocumentaryAddress(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			var localAddress = importerDocumentaryAddress?.LocalAddress;
			if (localAddress != null && importerDocumentaryAddress.E2_AddressOverride)
			{
				var importerTranslatedDocumentaryAddress = organizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ImporterTranslatedDocumentaryAddress));
				SetValuesForTranslatedDocumentaryAddress(localAddress, importerTranslatedDocumentaryAddress, delaySetters);
			}
		}

		void SetValuesForTranslatedDocumentaryAddress(JobDocAddress localAddress, OrganizationAddress orgAddress, Dictionary<string, ValueSetter> delaySetters)
		{
			if (orgAddress != null)
			{
				SetValue(localAddress, JobDocAddressSchema.E2_CompanyName, orgAddress.CompanyName, delaySetters);
				SetValue(localAddress, JobDocAddressSchema.E2_Address1, orgAddress.Address1, delaySetters);
				SetValue(localAddress, JobDocAddressSchema.E2_Address2, orgAddress.Address2, delaySetters);
				SetValue(localAddress, JobDocAddressSchema.E2_AdditionalAddressInformation, orgAddress.AdditionalAddressInformation, delaySetters);
				SetValue(localAddress, JobDocAddressSchema.E2_RN_NKCountryCode, orgAddress.Country.GetNullableCodeAsUpperCase(), delaySetters);
				SetValue(localAddress, JobDocAddressSchema.E2_City, orgAddress.City, delaySetters);
				SetValue(localAddress, JobDocAddressSchema.E2_Postcode, orgAddress.Postcode, delaySetters);
				SetValue(localAddress, JobDocAddressSchema.E2_State, (ZString?)orgAddress.State, delaySetters);
			}
		}

		protected override void AddFetchHintsRelatedToCommerialInvoiceLinePart(JobDeclaration declaration, IEnumerable<MasterFiles.Business.OrgSupplierPart> parts)
		{
			if (declaration is JobDeclaration jobDeclaration && jobDeclaration.TrademarkDocManagerInfo?.MasterFactory is BusinessObjectFactory docFactory)
			{
				foreach (var part in parts)
				{
					docFactory.AddFetchHint(StorageMainSchema.SM_ParentFK, part.PK);
				}
			}
		}

		protected override void ImportCountrySpecificRelatedData(JobDeclaration declaration)
		{
			if (declaration is JobDeclaration jobDeclaration && jobDeclaration.CusEntryInstruction is CusEntryInstruction entryInstruction)
			{
				if (dataObject != null && dataObject.EntryNumberCollection != null)
				{
					var entryNumberObj = dataObject.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == CusEntryNumberTypes.Standard.UniqueConsignementReference);
					if (entryNumberObj != null)
					{
						entryInstruction.UCROverride = !entryNumberObj.EntryIsSystemGenerated.Value;
						entryInstruction.UCRNumber = entryNumberObj.Number.Value;
					}
				}
			}
		}

		protected override CustomsContainerDataObjectReader<JobDeclaration, CusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CustomsContainerDataObjectReader<JobDeclaration, CusContainer>(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		protected override AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			return new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
		}
	}
}
