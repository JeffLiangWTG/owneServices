using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader>
	{
		protected internal CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader, invoiceType: typeof(JobComInvoiceHeader))
		{
		}

		#region Set Organizations

		protected override void FillOrganizationsCore(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData, Dictionary<string, ValueSetter> delaySetters)
		{
			var usInvoice = (JobComInvoiceHeader)invoice;
			var invoiceRow = GetColumnIndexer(usInvoice);

			if (invoice.IsExport && invoice.IsAttachedToPersistentDeclaration)
			{
				var addressCollection = dataObject.OrganizationAddressCollection;
				FillOrganizationAddress(invoiceRow, DocAddressTypes.Codes.USPrincipalPartyInInterest, dataObject.Supplier);
				FillOrganizationAddress(invoiceRow, DocAddressTypes.Codes.UltimateConsignee, dataObject.Buyer);
				if (addressCollection != null)
				{
					FillOrganizationAddress(invoiceRow, DocAddressTypes.Codes.IntermediateConsignee, addressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.IntermediateConsignee)));
					FillOrganizationAddress(invoiceRow, DocAddressTypes.Codes.SupplierPickupDeliveryAddress, addressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.SupplierPickupDeliveryAddress)));
				}
			}
			else
			{
				ZGuid? supplierAddressPK = null;
				ZGuid? buyerPK = null;
				ZGuid? buyerAddressPK = null;

				if (dataObject.Supplier != null)
				{
					ZGuid? supplierPK = null;
					OrgAddress supplierAddress = null;

					var infoMatcher = helper.CommercialInfoMatcher;

					if (infoMatcher == null || !infoMatcher.TryGetMatchedSupplier(dataObject.Supplier, out supplierAddress))
					{
						supplierAddress = new OrganisationDataObjectReader(dataObject.Supplier, logger, factory).GetMatched(invoice, OrganisationTypes.Consignor);
					}
					if (supplierAddress == null)
					{
						supplierPK = ZGuid.Empty;
						supplierAddressPK = ZGuid.Empty;
					}
					else
					{
						supplierPK = supplierAddress.OA_OH;
						supplierAddressPK = supplierAddress.PK;
					}
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OH_Supplier, supplierPK, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OA_SupplierAddress, supplierAddressPK, delaySetters);
				}

				if (dataObject.Buyer != null)
				{
					var buyerAddress = new OrganisationDataObjectReader(dataObject.Buyer, logger, factory).GetMatched(usInvoice, OrganisationTypes.Consignee);
					if (buyerAddress == null)
					{
						buyerPK = ZGuid.Empty;
						buyerAddressPK = ZGuid.Empty;
					}
					else
					{
						buyerPK = buyerAddress.OA_OH;
						buyerAddressPK = buyerAddress.PK;
					}
					SetValueWithDelay(invoiceRow, JobComInvoiceHeaderSchema.JZ_OH_Buyer, () => usInvoice.IsExport ? buyerPK : GetImporter(usInvoice), delaySetters);
				}

				// Export Data
				if (invoice.IsExport)
				{
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OA_BuyerAddress, buyerAddressPK, delaySetters);
					FillIntermConsigneeAddress(dataObject, invoice, delaySetters);
					FillConsignee(dataObject, invoice, delaySetters);
				}

				// None Export Data
				if (invoice.IsImport)
				{
					FillManufacturer(dataObject, invoice, delaySetters);
					FillShipToParty(dataObject, invoice, delaySetters);
					FillSeller(dataObject, invoice, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OA_BuyerAddress, buyerAddressPK, delaySetters);
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_OA_ExporterAddress, GetExporterAddressPKForHeader(usInvoice), delaySetters);
					SetValue(usInvoice, JobComInvoiceHeader.Schema.JZ_OA_InvoicerDocAddress, GetInvoicerAddressPK(usInvoice), delaySetters);
					SetValue(usInvoice, JobComInvoiceHeader.Schema.JZ_OA_FDAShipperAddress, GetFDAShipperAddressPK(usInvoice), delaySetters);
					FillSoldToPartyAddress(dataObject, invoice, delaySetters);
					FillSellingAgent(dataObject, invoice, delaySetters);
					FillBuyerAgent(dataObject, invoice, delaySetters);
					FillConsigneeAddress(dataObject, invoice, delaySetters);
				}
			}
		}

		ZGuid? GetImporter(JobComInvoiceHeader invoice)
		{
			ZGuid? result = null;
			if (invoice.IsImport)
			{
				result = helper.GetOrganisationPK(this, dataObject, invoice, AddressTypes.Importer, OrganisationTypes.Consignee);
			}
			return result;
		}

		ZGuid? GetFDAShipperAddressPK(JobComInvoiceHeader invoice)
		{
			return helper.GetAddressPK(this, dataObject, invoice, Constants.AddressType.FDAShipper, OrganisationTypes.Consignor);
		}

		ZGuid? GetInvoicerAddressPK(JobComInvoiceHeader invoice)
		{
			return helper.GetAddressPK(this, dataObject, invoice, Constants.AddressType.Invoicer, OrganisationTypes.Consignor);
		}

		ZGuid? GetExporterAddressPKForHeader(JobComInvoiceHeader invoice)
		{
			ZGuid? result = null;
			var declaration = invoice.JobDeclaration;
			if (declaration != null)
			{
				if (!declaration.IsACE)
				{
					result = helper.GetAddressPK(this, dataObject, invoice, Constants.AddressType.Exporter, OrganisationTypes.Consignor);
				}
				else
				{
					result = helper.GetAddressPK(this, dataObject, invoice, Constants.AddressType.ForeignExporter, OrganisationTypes.Consignor)
						?? helper.GetAddressPK(this, dataObject, invoice, Constants.AddressType.Exporter, OrganisationTypes.Consignor);
				}
			}
			return result;
		}

		protected override void FillOrganizationsCore(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillOrganizationsCore(invoiceLineData, invoiceLine, delaySetters);

			if (invoiceLineData.OrganizationAddressCollection != null)
			{
				var usInvoiceLine = (JobComInvoiceLine)invoiceLine;
				var invoiceLineRow = GetColumnIndexer(usInvoiceLine);
				var invoice = helper.Load<JobComInvoiceHeader>(invoiceLineRow, JobComInvoiceLineSchema.JI_JZ);
				if (invoice != null)
				{
					if (delaySetters == null)
					{
						var addInfos = invoiceLineRow.GetAddInfos(JobComInvoiceLineSchema.JI_AddInfo);
						SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_AddInfo, AddInfoParser.Serialise(addInfos));
					}

					if (invoice.IsImport)
					{
						var declaration = invoice.JobDeclaration;
						if (declaration != null && declaration.IsACE)
						{
							SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_OA_ExporterAddress, GetExporterAddressPKForLine(invoiceLineData, usInvoiceLine), delaySetters);
						}
					}
				}
			}
		}

		ZGuid? GetExporterAddressPKForLine(IOrganizationAddressCollectionParent dataObject, JobComInvoiceLine invoiceLine)
		{
			var addressPK = helper.GetAddressPK(this, dataObject, invoiceLine, Constants.AddressType.ForeignExporter, OrganisationTypes.Consignor)
					?? helper.GetAddressPK(this, dataObject, invoiceLine, Constants.AddressType.Exporter, OrganisationTypes.Consignor);
			return addressPK;
		}

		protected override void FillConsigneeAddress(IOrganizationAddressCollectionParent invoiceLineData,
			BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			var usInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var invoiceLineRow = GetColumnIndexer(usInvoiceLine);
			var invoice = helper.Load<JobComInvoiceHeader>(invoiceLineRow, JobComInvoiceLineSchema.JI_JZ);
			if (invoice != null)
			{
				if (invoice.IsImport)
				{
					base.FillConsigneeAddress(invoiceLineData, invoiceLine, delaySetters);
				}
			}
		}

		#endregion

		protected override void ImportCountrySpecificRelatedData(BaseJobComInvoiceHeader invoiceBO, Dictionary<string, ValueSetter> delaySetters)
		{
			var invoice = invoiceBO as JobComInvoiceHeader;
			invoice.IsRetrievingInvoiceFromReleaseEntry = false;
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForInvoice(BaseJobComInvoiceHeader invoice)
		{
			return new AddInfoDataObjectReader<JobComInvoiceHeader>(logger, helper, JobComInvoiceHeaderSchema.JZ_AddInfo, USAddInfoSchema.Instance);
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForInvoiceLine(BaseJobComInvoiceLine invoiceLine, CommercialInvoiceLine invoiceLineData)
		{
			var addInfoColumnsToDbColumnsMapping = new Dictionary<string, SchemaColumn>();

			var secondQuantityUnit = invoiceLineData.CustomsSecondQuantityUnit;
			var thirdQuantityUnit = invoiceLineData.CustomsThirdQuantityUnit;
			var doseDataFilledByInvoiceLineData = invoiceLineData.CustomsSecondQuantity.HasValue
												  || (secondQuantityUnit != null && secondQuantityUnit.Code.HasValue)
												  || invoiceLineData.CustomsThirdQuantity.HasValue
												  || (thirdQuantityUnit != null && thirdQuantityUnit.Code.HasValue);
			if (!doseDataFilledByInvoiceLineData)
			{
				addInfoColumnsToDbColumnsMapping.Add(WarehouseCustomsAddInfoWrapper.AddInfoKeys.SecondQty, JobComInvoiceLineSchema.JI_CustomsSecondQuantity);
				addInfoColumnsToDbColumnsMapping.Add(WarehouseCustomsAddInfoWrapper.AddInfoKeys.SecondCustomsQuantityUnit, JobComInvoiceLineSchema.JI_CustomsSecondUnitQty);
				addInfoColumnsToDbColumnsMapping.Add(WarehouseCustomsAddInfoWrapper.AddInfoKeys.ThirdQty, JobComInvoiceLineSchema.JI_CustomsThirdQuantity);
				addInfoColumnsToDbColumnsMapping.Add(WarehouseCustomsAddInfoWrapper.AddInfoKeys.ThirdCustomsQuantityUnit, JobComInvoiceLineSchema.JI_CustomsThirdUnitQty);
			}
			return new AddInfoDataObjectReader<JobComInvoiceLine>(logger, helper, JobComInvoiceLineSchema.JI_AddInfo, USAddInfoSchema.Instance, null, addInfoColumnsToDbColumnsMapping);
		}

		protected override IEnumerable<ZString> GetSettingOrderForStandalone(BaseJobComInvoiceHeader invoice)
		{
			return SettingOrderDeterminer.GetSettingOrderForStandalone((JobComInvoiceHeader)invoice);
		}

		protected override IEnumerable<ZString> GetSettingOrderForNormal(BaseJobComInvoiceHeader invoice)
		{
			return SettingOrderDeterminer.GetSettingOrderForNormal((JobComInvoiceHeader)invoice);
		}

		protected override IEnumerable<ZString> GetSettingOrder(BaseJobComInvoiceLine invoiceLine)
		{
			return SettingOrderDeterminer.GetSettingOrder((JobComInvoiceLine)invoiceLine);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IDisposable GetInvoiceLineImportSettings(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, bool isDefaultingEnabled)
		{
			IDisposable result = null;
			if (isDefaultingEnabled && !invoiceLineData.PartNo.GetValueOrDefault().IsEmpty)
			{
				var addInfoCollection = invoiceLineData.AddInfoCollection ?? new List<AddInfo>();
				var addInfoGroupCollection = invoiceLineData.AddInfoGroupCollection ?? new List<AddInfoGroup>();
				result = new InvoiceLineImportSetting((JobComInvoiceLine)invoiceLine, addInfoCollection, addInfoGroupCollection);
			}
			return result;
		}

		class InvoiceLineImportSetting : IDisposable
		{
			public InvoiceLineImportSetting(JobComInvoiceLine invoiceLine, List<AddInfo> addInfoCollection, List<AddInfoGroup> addInfoGroupCollection)
			{
				this.invoiceLine = invoiceLine;
				this.addInfoCollection = addInfoCollection;
				this.addInfoGroupCollection = addInfoGroupCollection;

				invoiceLine.ShouldNotDefaultDOTForProductXMLImport = HasDOTData();
				invoiceLine.ShouldNotDefaultFDAForProductXMLImport = HasFDAData();
				invoiceLine.ShouldNotDefaultPGAForProductXMLImport = HasPGAData();
				invoiceLine.ShouldNotDefaultACEFDAForProductXMLImport = HasACEFDAData();
				invoiceLine.ShouldNotDefaultNHTSAForProductXMLImport = HasNHTSAData();
				invoiceLine.ShouldNotDefaultATFForProductXMLImport = HasATFData();
				invoiceLine.ShouldNotDefaultTTBForProductXMLImport = HasTTBData();
				invoiceLine.ShouldNotDefaultAMSForProductXMLImport = !invoiceLine.IsExport && HasAMSData();
				invoiceLine.ShouldNotDefaultNOPForProductXMLImport = !invoiceLine.IsExport && HasNOPData();
				invoiceLine.ShouldNotDefaultOMCForProductXMLImport = HasOMCData();
				invoiceLine.ShouldNotDefaultHFCForProductXMLImport = !invoiceLine.IsExport && HasHFCData();
				invoiceLine.ShouldNotDefaultODSTSCAForProductXMLImport = HasODSTSCAData();
				invoiceLine.ShouldNotDefaultPSTForProductXMLImport = !invoiceLine.IsExport && HasPSTData();
				invoiceLine.ShouldNotDefaultVNEForProductXMLImport = HasVNEData();
				invoiceLine.ShouldNotDefaultCPSCForProductXMLImport = HasCPSCData();
				invoiceLine.ShouldNotDefaultDEAForProductXMLImport = HasDEAData();
				invoiceLine.ShouldNotDefaultAPHISForProductXMLImport = HasAPHISData();
				invoiceLine.ShouldNotDefaultNMFS370ForProductXMLImport = HasNMFS370Data();
				invoiceLine.ShouldNotDefaultNMFSAMRForProductXMLImport = HasNMFSAMRData();
				invoiceLine.ShouldNotDefaultNMFSHMSForProductXMLImport = HasNMFSHMSData();
				invoiceLine.ShouldNotDefaultNMFSSIMPForProductXMLImport = HasNMFSSIMPData();
				invoiceLine.ShouldNotDefaultFWSForProductXMLImport = HasFWSData();
				invoiceLine.ShouldNotDefaultExportAMSForProductXMLImport = invoiceLine.IsExport && HasExportAMSData();
				invoiceLine.ShouldNotDefaultExportEPAForProductXMLImport = invoiceLine.IsExport && HasExportEPAData();
			}
			readonly JobComInvoiceLine invoiceLine;
			readonly List<AddInfo> addInfoCollection;
			readonly List<AddInfoGroup> addInfoGroupCollection;

			public bool HasCPSCData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_CPSCInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USCPSCHeader);
			}

			public bool HasDOTData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_DOTIndicator.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USDOT);
			}

			public bool HasOMCData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_OMCInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USOMCHeader);
			}

			public bool HasHFCData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_HFCInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USHFCHeader);
			}

			public bool HasFDAData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_FDAIndicator.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USFDA);
			}

			public bool HasPGAData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_LaceyIndicator.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USPGACommon);
			}

			public bool HasACEFDAData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_FDAIndicator.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USACEFDA);
			}

			public bool HasNHTSAData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_NHTSAIndicator.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USNHTSAHeader);
			}

			public bool HasAMSData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_AMSInd.Substring(3)).HasValue
					|| (addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USAMSLine) && addInfoGroupCollection.Any(x => AMSProgramList.IsAMSProgramButNotNOP(invoiceLine.Factory, x.AddInfoCollection.GetZStringValue("ProgramType").GetValueOrDefault())));
			}

			public bool HasNOPData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_NOPInd.Substring(3)).HasValue
					|| (addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USAMSLine) && addInfoGroupCollection.Any(x => !AMSProgramList.IsAMSProgramButNotNOP(invoiceLine.Factory, x.AddInfoCollection.GetZStringValue("ProgramType").GetValueOrDefault())));
			}

			public bool HasTTBData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_TTBInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USTTBLine);
			}

			public bool HasNMFS370Data()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_NMFS370Ind.Substring(3)).HasValue
						|| (addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USNMFSLine) && addInfoGroupCollection.Any(x => x.AddInfoCollection.GetZStringValue("ProgramType").GetValueOrDefault() == NMFSProgramCodeList.Codes._370));
			}

			public bool HasNMFSAMRData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_NMFSAMRInd.Substring(3)).HasValue
							|| (addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USNMFSLine) && addInfoGroupCollection.Any(x => x.AddInfoCollection.GetZStringValue("ProgramType").GetValueOrDefault() == NMFSProgramCodeList.Codes.AMR));
			}

			public bool HasNMFSHMSData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_NMFSHMSInd.Substring(3)).HasValue
					|| (addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USNMFSLine) && addInfoGroupCollection.Any(x => x.AddInfoCollection.GetZStringValue("ProgramType").GetValueOrDefault() == NMFSProgramCodeList.Codes.HMS));
			}

			public bool HasNMFSSIMPData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_NMFSSIMPInd.Substring(3)).HasValue
					|| (addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USNMFSLine) && addInfoGroupCollection.Any(x => x.AddInfoCollection.GetZStringValue("ProgramType").GetValueOrDefault() == "SIM"));
			}

			public bool HasATFData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_ATFInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USATF);
			}

			public bool HasODSTSCAData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_ODSInd.Substring(3)).HasValue || addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_TSCAInd.Substring(3)).HasValue || addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_TSCACertification.Substring(3)).HasValue || addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_TSCAODSCertIndividual.Substring(3)).HasValue;
			}

			public bool HasPSTData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_PSTIndicator.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USPesticide);
			}

			public bool HasVNEData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_VNEInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USPGAVehicle);
			}

			public bool HasDEAData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_DEAInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USDEAHeader);
			}

			public bool HasAPHISData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_APHISInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USAPHISHeader);
			}

			public bool HasFWSData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_FWSInd.Substring(3)).HasValue || addInfoGroupCollection.Any(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USFWSHeader);
			}

			public bool HasExportAMSData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_AMSInd.Substring(3)).HasValue || addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_ExportCertificateNo).HasValue;
			}

			public bool HasExportEPAData()
			{
				return addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_PSTIndicator.Substring(3)).HasValue || addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_EPAConsentNumber).HasValue || addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_HazWasteTrackingNo).HasValue
					|| addInfoCollection.GetZDecimalValue(JobComInvoiceLine.Schema.US_EPANetQty).HasValue || addInfoCollection.GetZStringValue(JobComInvoiceLine.Schema.US_EPANetQtyUQ.Substring(3)).HasValue;
			}

			public void Dispose()
			{
				invoiceLine.ShouldNotDefaultDOTForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultFDAForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultPGAForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultATFForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultACEFDAForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultNHTSAForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultTTBForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultODSTSCAForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultPSTForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultVNEForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultCPSCForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultDEAForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultAPHISForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultAMSForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultFWSForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultNMFS370ForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultNMFSAMRForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultNMFSHMSForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultNMFSSIMPForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultNOPForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultOMCForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultHFCForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultExportEPAForProductXMLImport = false;
				invoiceLine.ShouldNotDefaultExportAMSForProductXMLImport = false;
			}
		}

		protected override void AddFetchHintsForFillCommercialInvoiceLineBasedOnLineNoDetails(BaseJobDeclaration declaration, BaseJobComInvoiceHeader invoice, SortedDictionary<ZInt, CommercialInvoiceLineDataRelationship> lineNoDictionary)
		{
			var boFactory = factory.BOFactory;
			if (declaration?.IsDrawback ?? ZBool.False)
			{
				var useViewForDrawbackEntryLine = USCustomsDataRegistry.Instance.UseViewForDrawbackEntryLine.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (useViewForDrawbackEntryLine)
				{
					var parentInvoiceLineData = lineNoDictionary.Values.SelectMany(x => x.ParentInvoiceLineData);
					foreach (var commercialInvoiceLine in parentInvoiceLineData)
					{
						var importEntryNo = commercialInvoiceLine.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.DrawbackImportEntryNo);
						var drawbackImportDeclarationLine = commercialInvoiceLine.AddInfoCollection.GetZShortValue(Constants.AddInfoKeys.InvoiceLine.DrawbackImportDeclarationLine);
						if (importEntryNo.HasValue && drawbackImportDeclarationLine.HasValue)
						{
							DrawbackJobComInvoiceLineFetchHintHelper.AddFetchHintForUSImportEntryLineTable(boFactory, importEntryNo.Value, drawbackImportDeclarationLine.Value);
						}
					}

					((IBusinessObjectFactoryInternals)factory.BOFactory).RowFactory.ExecuteFetchHintsForTable(USImportEntryLineSchema.Constants.TableName);

					foreach (var commercialInvoiceLine in parentInvoiceLineData)
					{
						var drawbackEntryLine = GetViewDrawbackImportEntryLineData(commercialInvoiceLine);
						if (drawbackEntryLine != null)
						{
							var query = new ZQuery(USImportEntryLineFeeSchema.USF_CL, drawbackEntryLine.PK);
							boFactory.AddFetchHint(USImportEntryLineFeeSchema.Instance, query);
							query = new ZQuery(USImportEntryLineSchema.USE_CH, drawbackEntryLine.USE_CH);
							boFactory.AddFetchHint(typeof(USImportEntryLine), query);
						}
					}
				}
			}
		}

		USImportEntryLine GetViewDrawbackImportEntryLineData(CommercialInvoiceLine commercialInvoiceLine)
		{
			USImportEntryLine result = null;
			var importEntryNo = commercialInvoiceLine.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.DrawbackImportEntryNo);
			var drawbackImportDeclarationLine = commercialInvoiceLine.AddInfoCollection.GetZShortValue(Constants.AddInfoKeys.InvoiceLine.DrawbackImportDeclarationLine);
			if (importEntryNo.HasValue && drawbackImportDeclarationLine.HasValue)
			{
				result = DrawbackJobComInvoiceLineFetchHintHelper.GetViewDrawbackImportEntryLineData((x) => factory.BOFactory.Load<USImportEntryLine>(x), importEntryNo.Value, drawbackImportDeclarationLine.Value);
			}
			return result;
		}

		protected override void FillCountrySpecificDetails(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
		{
			base.FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);
			if (helper.IsSourceAndTargetCountrySame && invoiceLineData.AddInfoCollection != null)
			{
				var parentProductLineNoAddInfo = invoiceLineData.AddInfoCollection.GetZIntValue(Constants.AddInfoKeys.InvoiceLine.ParentProductLineNo, logger);
				if (parentProductLineNoAddInfo.HasValue)
				{
					var parentProductLineNo = parentProductLineNoAddInfo.Value;
					var lineNo = invoiceLineData.LineNo.GetValueOrDefault();
					if (parentProductLineNo > ZInt.Zero && parentProductLineNo == lineNo)
					{
						logger.LogBoth(Integration.LogType.Error, Res.GetString("BA57AD08-50C3-4227-9642-F6CFC2FCF06D", "Commercial Invoice Line cannot not have the same {0} and {1} ('{2}').", "LineNo", "ParentProductLineNo", parentProductLineNo));
					}
					else
					{
						if (parentInvoiceLine == null)
						{
							logger.Log(Integration.LogType.Warning, Res.GetString("ACF5E84F-9B47-4A0C-9C05-47AC5CBDA0D5", "Cannot not find {0} ('{1}') for Commercial Invoice Line with {2} ('{3}').", "ParentProductLineNo", parentProductLineNo, "LineNo", lineNo));
						}
						else
						{
							if (delaySetters == null)
							{
								var addInfos = invoiceLineRow.GetAddInfos(JobComInvoiceLineSchema.JI_AddInfo);
								addInfos.Update(JobComInvoiceLine.Schema.US_JI_ParentProduct.Substring(3), parentInvoiceLine.PK);
								SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_AddInfo, AddInfoParser.Serialise(addInfos));
							}
							else
							{
								SetValue(invoiceLineRow, USAddInfoSchema.US_JI_ParentProduct, parentInvoiceLine.PK, delaySetters, JobComInvoiceLineSchema.PK);
							}
						}
					}
				}
			}
			if (delaySetters == null)
			{
				var parentInvoiceLineRow = GetColumnIndexer(helper.Load<JobComInvoiceLine>(invoiceLineRow, JobComInvoiceLineSchema.JI_ParentID));
				if (parentInvoiceLineRow != null)
				{
					var addInfoManager = (IAddInfoManager)parentInvoiceLine;
					try
					{
						addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
						var addInfos = parentInvoiceLineRow.GetAddInfos(JobComInvoiceLineSchema.JI_AddInfo);
						addInfos.Update(JobComInvoiceLine.Schema.US_IsParent.Substring(3), ZBool.True);
						SetValue(parentInvoiceLineRow, JobComInvoiceLineSchema.JI_AddInfo, AddInfoParser.Serialise(addInfos));
					}
					finally
					{
						addInfoManager.UpdateAddInfoFromString(parentInvoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_AddInfo));
						addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
					}
				}
			}
		}

		protected override void FillCommercialInvoiceLineData(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, ZString invoiceNumber, BaseJobComInvoiceLine parentInvoiceLine, bool isStandalone, bool isDeclarationIntegrated, bool supportsChcPivotBetweenInvoiceLineAndPacking)
		{
			base.FillCommercialInvoiceLineData(invoiceLineData, invoiceLine, invoiceNumber, parentInvoiceLine, isStandalone, isDeclarationIntegrated, supportsChcPivotBetweenInvoiceLineAndPacking);
			if (invoiceLine is JobComInvoiceLine line && line.US_UC_NKCountryOfOrigin.IsEmpty && !line.IsExport)
			{
				var countryOfOrigin = invoiceLineData.CountryOfOrigin?.Code ?? ZString.Empty;
				if (!countryOfOrigin.IsEmpty)
				{
					line.US_UC_NKCountryOfOrigin = countryOfOrigin;
				}
			}
		}

		ZInt? GetParentProductLineNo(CommercialInvoiceLine invoiceLineData)
		{
			return invoiceLineData.AddInfoCollection != null ? invoiceLineData.AddInfoCollection.GetZIntValue(Constants.AddInfoKeys.InvoiceLine.ParentProductLineNo) : null;
		}

		protected override ZInt? GetParentLineNo(CommercialInvoiceLine invoiceLineData)
		{
			var result = base.GetParentLineNo(invoiceLineData);
			if (result.GetValueOrDefault() == ZInt.Zero)
			{
				result = GetParentProductLineNo(invoiceLineData);
			}
			return result;
		}

		protected override bool ImportEmptyHarmonisedCode => false;

		protected override IEnumerable<BaseJobComInvoiceLine> GetChildInvoiceLinesAddedBySystemCore(BaseJobComInvoiceLine parentLine)
		{
			var usParentLine = (JobComInvoiceLine)parentLine;
			return usParentLine.ChildLines.Concat(usParentLine.ProductRelatedLines);
		}

		void FillOrganizationAddress(IColumnIndexer invoiceRow, ZString addressType, OrganizationAddress orgAddressDataObject)
		{
			if (orgAddressDataObject != null)
			{
				var invoicePK = invoiceRow.GetValue(JobComInvoiceHeaderSchema.PK);
				var query = new ZQuery(JobDocAddressSchema.E2_AddressType, addressType);
				query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, 0);
				query.AddToFilter(JobDocAddressSchema.E2_ParentID, invoicePK);
				var docAddress = factory.LoadTop1<JobDocAddress>(query) ?? factory.New<JobDocAddress>();
				var docAddresRow = GetColumnIndexer(docAddress);
				if (!docAddress.IsInDatabase)
				{
					SetValue(docAddresRow, JobDocAddressSchema.E2_AddressType, addressType);
					SetValue(docAddresRow, JobDocAddressSchema.E2_AddressSequence, 0);
					SetValue(docAddresRow, JobDocAddressSchema.E2_ParentID, invoicePK);
					SetValue(docAddresRow, JobDocAddressSchema.E2_ParentTableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
				}
				var reader = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory);
				var orgAddress = reader.GetMatched(true);
				if (orgAddress == null)
				{
					SetValue(docAddresRow, JobDocAddressSchema.E2_OA_Address, ZGuid.Empty);
					SetValue(docAddresRow, JobDocAddressSchema.E2_AddressOverride, ZBool.True);
				}
				reader.PopulateJobDocAddress(orgAddress, docAddress);
			}
		}

		protected override IEnumerable<ZString> GetColumnNamesForSuspendSetting(CommercialInvoiceLine invoiceLineData)
		{
			return base.GetColumnNamesForSuspendSetting(invoiceLineData).Union(
				AddAddInfoColumnsToSuspendSetting(invoiceLineData.AddInfoCollection,
					JobComInvoiceLine.Schema.US_SupTariff,
					JobComInvoiceLine.Schema.US_DRWAdValoremRate,
					JobComInvoiceLine.Schema.US_DRWWeightedRatio,
					JobComInvoiceLine.Schema.US_DRWMPFWeightedRatio,
					JobComInvoiceLine.Schema.US_DRWLineDuty,
					JobComInvoiceLine.Schema.US_DRWDeclaredTax,
					JobComInvoiceLine.Schema.US_DRWDeclaredVFD,
					JobComInvoiceLine.Schema.US_DRWDeclaredHMF,
					JobComInvoiceLine.Schema.US_DRWDeclaredMPF,
					JobComInvoiceLine.Schema.US_DRWImportQuantity,
					JobComInvoiceLine.Schema.US_DRWImportUQ,
					JobComInvoiceLine.Schema.US_DRWExportQuantity,
					JobComInvoiceLine.Schema.US_DRWExportUQ,
					JobComInvoiceLine.Schema.US_DRWImportQuantity2,
					JobComInvoiceLine.Schema.US_DRWImportUQ2,
					JobComInvoiceLine.Schema.US_DRWImportQuantity3,
					JobComInvoiceLine.Schema.US_DRWImportUQ3,
					JobComInvoiceLine.Schema.US_DRWValuePerUQ,
					JobComInvoiceLine.Schema.US_DRWValuePerUQ2,
					JobComInvoiceLine.Schema.US_DRWValuePerUQ3,
					JobComInvoiceLine.Schema.US_DRWLineDutyRateDesc,
					JobComInvoiceLine.Schema.US_DRWCalcDutyWithAdValoremRate
					));
		}

		IEnumerable<ZString> AddAddInfoColumnsToSuspendSetting(List<AddInfo> addInfoCollection, params ZString[] addInfoColumnNames)
		{
			foreach (var addInfoColumnName in addInfoColumnNames)
			{
				if (addInfoCollection.GetZStringValue(addInfoColumnName.SubstringSafe(3)).HasValue)
				{
					yield return addInfoColumnName;
				}
			}
		}
	}
}
