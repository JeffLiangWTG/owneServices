using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class DeclarationDataObjectWriter : TopLevelDataObjectWriter<BaseJobDeclaration, Shipment>, IMergeDataObjectWriter, IContainerParentDataObjectWriter, IDeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected internal ZBool ShouldPopulateAttachedDocumentCollection { get; set; } = ZBool.False;

		void IMergeDataObjectWriter.MergeData(IDataObject dataObject, BusinessObject bussinesBO)
		{
			var shipmentData = dataObject as Shipment;
			var declarationBO = bussinesBO as BaseJobDeclaration;
			if (shipmentData != null && declarationBO != null)
			{
				var dataContext = shipmentData.DataContext;
				if (dataContext != null)
				{
					dataContext.AddDataSource(DataContextType.CustomsDeclaration, declarationBO.JE_DeclarationReference);
				}
				var keepExistingData = !eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.Value;
				var primaryDataSource = writeManager.ContentFilterManager?.EDIMessageContentFilter?.GetUniversalShipmentPrimaryDataSource();
				if (!string.IsNullOrEmpty(primaryDataSource))
				{
					switch (primaryDataSource)
					{
						case EDIMessageContentPrimaryDataSource.Codes.Brokerage:
							keepExistingData = false;
							break;
						case EDIMessageContentPrimaryDataSource.Codes.Shipment:
							keepExistingData = true;
							break;
					}
				}
				PopulateDataObjectCore(declarationBO, shipmentData, keepExistingData, null);
			}
		}

		protected override BaseJobDeclaration GetTypedBusinessObject(BusinessObject sourceBO)
		{
			var declarationProvider = sourceBO as Integration.IForwardingShipmentDeclarationProvider; 
			if (declarationProvider != null)
			{
				var declaration = (BaseJobDeclaration)declarationProvider.GetDeclaration();
				return declaration ?? throw new InvalidOperationException("ForwardingShipment sourceBO must have a declaration");
			}

			return (BaseJobDeclaration)sourceBO;
		}

		protected sealed override void AddTableFetchHintCreators(IExternalFetchHintSupporter externalFetchHintSupporter, BaseJobDeclaration sourceBO)
		{
			AddTableFetchHintCreatorsCore(externalFetchHintSupporter, sourceBO);
		}

		internal void AddTableFetchHintCreatorsCore(IExternalFetchHintSupporter externalFetchHintSupporter, BaseJobDeclaration sourceBO)
		{
			try
			{
				if (helper == null || helper.countryCode != sourceBO.CountryCode)
				{
					helper = CreateNewUniversalDataObjectWriterHelper(sourceBO);
				}
				DeclarationRowForFetchHint = DataObjectReader.GetColumnIndexerFromRow(sourceBO);
				FetchHintDecider = new ChildTypeSupported(sourceBO);
				AddTableFetchHintCreators(externalFetchHintSupporter);
				externalFetchHintSupporter.AddRelatedTableHints(CusDecHouseBillSchema.Constants.TableName, sourceBO.Bills.OfType<INeedRow>().Select(x => (IColumnIndexer)x.Row).ToArray());
				externalFetchHintSupporter.AddRelatedTableHints(JobComInvoiceHeaderSchema.Constants.TableName, sourceBO.Invoices.OfType<INeedRow>().Select(x => (IColumnIndexer)x.Row).ToArray());
				externalFetchHintSupporter.AddRelatedTableHints(JobComInvoiceLineSchema.Constants.TableName, sourceBO.InvoiceLines.OfType<INeedRow>().Select(x => (IColumnIndexer)x.Row).ToArray());
				externalFetchHintSupporter.AddRelatedTableHints(CusEntryHeaderSchema.Constants.TableName, sourceBO.CustomsEntryHeaders.OfType<INeedRow>().Select(x => (IColumnIndexer)x.Row).ToArray());
				externalFetchHintSupporter.AddRelatedTableHints(CusEntryLineSchema.Constants.TableName, sourceBO.CustomsEntryHeaders.OfType<CusEntryHeader>().SelectMany(header => header.AllEntryLines).OfType<INeedRow>().Select(x => (IColumnIndexer)x.Row).ToArray());
			}
			finally
			{
				DeclarationRowForFetchHint = null;
			}
		}

		public class ChildTypeSupported
		{
			public ChildTypeSupported(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				invoiceType = ((IBusinessObjectCollection)declaration.Invoices).TypeOfElements;
				billType = declaration.Bills.TypeOfElements;
				invoiceLineType = declaration.InvoiceLines.TypeOfElements;
			}
			readonly Type billType;
			readonly Type invoiceType;

			readonly Type invoiceLineType;
			readonly BaseJobDeclaration declaration;

			public bool DeclarationUseCusAddInfo
			{
				get
				{
					if (!declarationUseCusAddInfo.HasValue)
					{
						declarationUseCusAddInfo = typeof(ICusAddInfoTypeSupporter).IsAssignableFrom(declaration.GetType());
					}
					return declarationUseCusAddInfo.Value;
				}
			}
			bool? declarationUseCusAddInfo;

			public bool DeclarationUseCusCodeData
			{
				get
				{
					if (!declarationUseCusCodeData.HasValue)
					{
						declarationUseCusCodeData = typeof(ICusCodeDataTypeSupporter).IsAssignableFrom(declaration.GetType());
					}
					return declarationUseCusCodeData.Value;
				}
			}
			bool? declarationUseCusCodeData;

			public bool DeclarationUseCusSupportingInfo
			{
				get
				{
					if (!declarationUseCusSupportingInfo.HasValue)
					{
						declarationUseCusSupportingInfo = typeof(ICusSupportingInfoTypeSupporter).IsAssignableFrom(declaration.GetType());
					}
					return declarationUseCusSupportingInfo.Value;
				}
			}
			bool? declarationUseCusSupportingInfo;

			public bool BillUseCusAddInfo
			{
				get
				{
					if (!billUseCusAddInfo.HasValue)
					{
						billUseCusAddInfo = typeof(ICusAddInfoTypeSupporter).IsAssignableFrom(billType);
					}
					return billUseCusAddInfo.Value;
				}
			}
			bool? billUseCusAddInfo;

			public bool BillUseCusCodeData
			{
				get
				{
					if (!billUseCusCodeData.HasValue)
					{
						billUseCusCodeData = typeof(ICusCodeDataTypeSupporter).IsAssignableFrom(billType);
					}
					return billUseCusCodeData.Value;
				}
			}
			bool? billUseCusCodeData;

			public bool InvoiceUseCusAddInfo
			{
				get
				{
					if (!invoiceUseCusAddInfo.HasValue)
					{
						invoiceUseCusAddInfo = typeof(ICusAddInfoTypeSupporter).IsAssignableFrom(invoiceType);
					}
					return invoiceUseCusAddInfo.Value;
				}
			}
			bool? invoiceUseCusAddInfo;

			public bool InvoiceUseCusCodeData
			{
				get
				{
					if (!invoiceUseCusCodeData.HasValue)
					{
						invoiceUseCusCodeData = typeof(ICusCodeDataTypeSupporter).IsAssignableFrom(invoiceType);
					}
					return invoiceUseCusCodeData.Value;
				}
			}
			bool? invoiceUseCusCodeData;

			public bool InvoiceUseCusSupportingInfo
			{
				get
				{
					if (!invoiceUseCusSupportingInfo.HasValue)
					{
						invoiceUseCusSupportingInfo = typeof(ICusSupportingInfoTypeSupporter).IsAssignableFrom(invoiceType);
					}
					return invoiceUseCusSupportingInfo.Value;
				}
			}
			bool? invoiceUseCusSupportingInfo;

			public bool InvoiceLineUseCusAddInfo
			{
				get
				{
					if (!invoiceLineUseCusAddInfo.HasValue)
					{
						invoiceLineUseCusAddInfo = typeof(ICusAddInfoTypeSupporter).IsAssignableFrom(invoiceLineType);
					}
					return invoiceLineUseCusAddInfo.Value;
				}
			}
			bool? invoiceLineUseCusAddInfo;

			public bool InvoiceLineUseCusCodeData
			{
				get
				{
					if (!invoiceLineUseCusCodeData.HasValue)
					{
						invoiceLineUseCusCodeData = typeof(ICusCodeDataTypeSupporter).IsAssignableFrom(invoiceLineType);
					}
					return invoiceLineUseCusCodeData.Value;
				}
			}
			bool? invoiceLineUseCusCodeData;

			public bool InvoiceLineUseCusSupportingInfo
			{
				get
				{
					if (!invoiceLineUseCusSupportingInfo.HasValue)
					{
						invoiceLineUseCusSupportingInfo = typeof(ICusSupportingInfoTypeSupporter).IsAssignableFrom(invoiceLineType);
					}
					return invoiceLineUseCusSupportingInfo.Value;
				}
			}
			bool? invoiceLineUseCusSupportingInfo;
		}

		protected UniversalDataObjectWriterHelper helper
		{
			get;
			private set;
		}

		protected IColumnIndexer DeclarationRowForFetchHint
		{
			get;
			private set;
		}

		protected ChildTypeSupported FetchHintDecider
		{
			get;
			private set;
		}

		protected virtual void AddTableFetchHintCreators(IExternalFetchHintSupporter externalFetchHintSupporter)
		{
			var declarationPK = DeclarationRowForFetchHint.GetValue(JobDeclarationSchema.PK);
			externalFetchHintSupporter.AddFetchHint(new FetchHint(StmNoteSchema.ST_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusContainerSchema.CO_JE, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(JobDocAddressSchema.E2_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusHouseContPackInvoiceLinePivotSchema.CHC_JE, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE, declarationPK));
			if (FetchHintDecider.DeclarationUseCusAddInfo)
			{
				externalFetchHintSupporter.AddFetchHint(new FetchHint(CusAddInfoSchema.B7_ParentID, declarationPK));
			}
			if (FetchHintDecider.DeclarationUseCusCodeData)
			{
				externalFetchHintSupporter.AddFetchHint(new FetchHint(CusCodeDataSchema.CY_ParentID, declarationPK));
			}
			if (FetchHintDecider.DeclarationUseCusSupportingInfo)
			{
				externalFetchHintSupporter.AddFetchHint(new FetchHint(CusSupportingInfoSchema.CSI_ParentID, declarationPK));
			}
			AddFetchHints(externalFetchHintSupporter, DeclarationRowForFetchHint, JobDeclarationSchema.JE_OH_Importer, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH);
			AddFetchHints(externalFetchHintSupporter, DeclarationRowForFetchHint, JobDeclarationSchema.JE_OH_Forwarder, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH);
			AddFetchHints(externalFetchHintSupporter, DeclarationRowForFetchHint, JobDeclarationSchema.JE_OH_ShippingLine, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH);
			AddFetchHints(externalFetchHintSupporter, DeclarationRowForFetchHint, JobDeclarationSchema.JE_OH_Supplier, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH);
			AddFetchHints(externalFetchHintSupporter, DeclarationRowForFetchHint, JobDeclarationSchema.JE_OA_ManufacturerAddress, OrgAddressSchema.PK, OrgCusCodeSchema.OK_OA_PremisesAddress);
			AddFetchHints(externalFetchHintSupporter, DeclarationRowForFetchHint, JobDeclarationSchema.JE_OA_SoldToPartyAddress, OrgAddressSchema.PK, OrgCusCodeSchema.OK_OA_PremisesAddress);
			AddFetchHints(externalFetchHintSupporter, DeclarationRowForFetchHint, JobDeclarationSchema.JE_OH_SellingAgent, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OA_PremisesAddress);
			AddFetchHints(externalFetchHintSupporter, DeclarationRowForFetchHint, JobDeclarationSchema.JE_OH_BuyingAgent, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OA_PremisesAddress);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusDecHouseBillSchema.Instance, GetCusDecHouseBillRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusContainerSchema.Instance, GetCusContainerRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusDecHouseContainerPivotSchema.Instance, GetCusDecHouseContainerPivotRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusEntryInstructionSchema.Instance, GetEntryInstructionRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(JobComInvoiceHeaderSchema.Instance, GetJobComInvoiceHeaderRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(JobComInvoiceLineSchema.Instance, GetJobComInvoiceLineRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(OrgAddressSchema.Instance, GetOrgAddressRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(OrgSupplierPartSchema.Instance, GetOrgSupplierPartRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusClassPartPivotSchema.Instance, GetCusClassPartPivotRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusEntryHeaderSchema.Instance, GetCusEntryHeaderRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusEntryLineSchema.Instance, GetCusEntryLineRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(JobContainerSchema.Instance, GetJobContainerRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(LandedCostHeaderSchema.Instance, GetLandedCostHeaderRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(LandedCostHistorySchema.Instance, GetLandedCostHistoryRelatedFetchHints);
			if (helper.SupportAdditionalInvoiceLineEntryLineLink)
			{
				externalFetchHintSupporter.AddTableFetchHintCreator(CusUnderbondDecSchema.Instance, GetCusUnderbondDecRelatedFetchHints);
			}
		}

		protected virtual IEnumerable<IFetchHint> GetCusUnderbondDecRelatedFetchHints(IColumnIndexer row)
		{
			var fetchHint = GetFetchHintIfNotEmpty(CusEntryLineSchema.PK, row, CusUnderbondDecSchema.BU_CL);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
		}

		protected virtual IEnumerable<IFetchHint> GetLandedCostHistoryRelatedFetchHints(IColumnIndexer row)
		{
			var costHistoryPK = row.GetValue(LandedCostHistorySchema.PK);
			yield return new FetchHint(LandedLineCostItemSchema.LZ_LH, costHistoryPK);
		}

		protected virtual IEnumerable<IFetchHint> GetLandedCostHeaderRelatedFetchHints(IColumnIndexer row)
		{
			var costHeaderPK = row.GetValue(LandedCostHeaderSchema.PK);
			yield return new FetchHint(LandCostInputSchema.LI_LT, costHeaderPK);
			yield return new FetchHint(LandedCostHistorySchema.LH_LT, costHeaderPK);
		}

		protected virtual IEnumerable<IFetchHint> GetJobContainerRelatedFetchHints(IColumnIndexer row)
		{
			var containerPK = row.GetValue(JobContainerSchema.PK);
			yield return new FetchHint(JobServiceSchema.ES_ParentID, containerPK);
			yield return new FetchHint(JobContainerPackPivotSchema.J6_JC, containerPK);
			yield return new FetchHint(CusEntryNumSchema.CE_ParentID, containerPK);
		}

		protected virtual IEnumerable<IFetchHint> GetCusDecHouseBillRelatedFetchHints(IColumnIndexer row)
		{
			var billPK = row.GetValue(CusDecHouseBillSchema.PK);
			yield return new FetchHint(JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill, billPK);
			yield return new FetchHint(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, billPK);
			if (FetchHintDecider != null)
			{
				if (FetchHintDecider.BillUseCusAddInfo)
				{
					yield return new FetchHint(CusAddInfoSchema.B7_ParentID, billPK);
				}
				if (FetchHintDecider.BillUseCusCodeData)
				{
					yield return new FetchHint(CusCodeDataSchema.CY_ParentID, billPK);
				}
			}
		}

		IEnumerable<IFetchHint> GetCusContainerRelatedFetchHints(IColumnIndexer cusContainerRow)
		{
			var containerPK = cusContainerRow.GetValue(CusContainerSchema.PK);
			yield return new FetchHint(CusContainerInvoiceLinePivotSchema.C2_CO, containerPK);
			yield return new FetchHint(CusDecHouseContainerPivotSchema.CR_CO_Container, containerPK);
			var fetchHint = GetFetchHintIfNotEmpty(JobContainerSchema.PK, cusContainerRow, CusContainerSchema.CO_JC);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
			fetchHint = GetFetchHintIfNotEmpty(ProcessTasksSchema.P9_ParentID, cusContainerRow, CusContainerSchema.CO_JC);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
			fetchHint = GetFetchHintIfNotEmpty(RefContainerSchema.PK, cusContainerRow, CusContainerSchema.CO_RC);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
		}

		protected virtual IEnumerable<IFetchHint> GetCusDecHouseContainerPivotRelatedFetchHints(IColumnIndexer row)
		{
			var packageGroupPK = row.GetValue(CusDecHouseContainerPivotSchema.PK);
			yield return new FetchHint(CusDecHouseContainerPackSchema.CW_CR_HouseContainer, packageGroupPK);
		}

		protected virtual IEnumerable<IFetchHint> GetCusEntryHeaderRelatedFetchHints(IColumnIndexer row)
		{
			var entryHeaderPK = row.GetValue(CusEntryHeaderSchema.PK);
			yield return new FetchHint(CusEntryLineSchema.CL_CH, entryHeaderPK);
			yield return new FetchHint(CusEntryHeaderChargesSchema.C1_CH, entryHeaderPK);
			yield return new FetchHint(CusEntryNumSchema.CE_ParentID, entryHeaderPK);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, entryHeaderPK);
		}

		protected virtual IEnumerable<IFetchHint> GetCusEntryLineRelatedFetchHints(IColumnIndexer row)
		{
			var entryLinePK = row.GetValue(CusEntryLineSchema.PK);
			yield return new FetchHint(CusEntryLineFeeSchema.CF_CL, entryLinePK);
		}

		protected virtual IEnumerable<IFetchHint> GetEntryInstructionRelatedFetchHints(IColumnIndexer row)
		{
			return Enumerable.Empty<IFetchHint>();
		}

		protected virtual IEnumerable<IFetchHint> GetJobComInvoiceHeaderRelatedFetchHints(IColumnIndexer row)
		{
			var invoicePK = row.GetValue(JobComInvoiceHeaderSchema.PK);
			yield return new FetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoicePK);
			yield return new FetchHint(JobComInvoiceLineSchema.JI_JZ, invoicePK);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, invoicePK);
			yield return new FetchHint(GenPivotSchema.XX_Relation1ID, invoicePK);
			yield return new FetchHint(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JZ, invoicePK);
			yield return new FetchHint(JobDocAddressSchema.E2_ParentID, invoicePK);
			var fetchHint = GetFetchHintIfNotEmpty(CusDecHouseBillSchema.PK, row, JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
			foreach (var fetchHint1 in GetFetchHintsIfNotEmpty(row, JobComInvoiceHeaderSchema.JZ_OH_Supplier, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH))
			{
				yield return fetchHint1;
			}
			foreach (var fetchHint1 in GetFetchHintsIfNotEmpty(row, JobComInvoiceHeaderSchema.JZ_OH_Buyer, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH))
			{
				yield return fetchHint1;
			}
			if (FetchHintDecider != null)
			{
				if (FetchHintDecider.InvoiceUseCusAddInfo)
				{
					yield return new FetchHint(CusAddInfoSchema.B7_ParentID, invoicePK);
				}
				if (FetchHintDecider.InvoiceUseCusCodeData)
				{
					yield return new FetchHint(CusCodeDataSchema.CY_ParentID, invoicePK);
				}
				if (FetchHintDecider.InvoiceUseCusSupportingInfo)
				{
					yield return new FetchHint(CusSupportingInfoSchema.CSI_ParentID, invoicePK);
				}
			}
		}

		protected virtual IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoiceLinePK);
			yield return new FetchHint(UNDGDataItemSchema.DI_ParentID, invoiceLinePK);
			yield return new FetchHint(CusContainerInvoiceLinePivotSchema.C2_JI, invoiceLinePK);
			yield return new FetchHint(CusHouseContPackInvoiceLinePivotSchema.CHC_JI, invoiceLinePK);
			yield return new FetchHint(JobComInvoiceLineTaxSchema.JLT_JI, invoiceLinePK);
			yield return new FetchHint(JobComInvLineComponentInventorySchema.JIV_JI, invoiceLinePK);
			var fetchHint = GetFetchHintIfNotEmpty(CusClassificationSchema.PK, row, JobComInvoiceLineSchema.JI_CC);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
			fetchHint = GetFetchHintIfNotEmpty(OrgSupplierPartSchema.PK, row, JobComInvoiceLineSchema.JI_OP);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
			fetchHint = GetFetchHintIfNotEmpty(OrgSupplierPartSchema.OP_PartNum, row, JobComInvoiceLineSchema.JI_PartNo);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
			fetchHint = GetFetchHintIfNotEmpty(CusEntryLineSchema.PK, row, JobComInvoiceLineSchema.JI_CL);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
			if (helper.SupportAdditionalInvoiceLineEntryLineLink)
			{
				yield return new FetchHint(CusUnderbondDecSchema.BU_JI, invoiceLinePK);
			}
			if (FetchHintDecider != null)
			{
				if (FetchHintDecider.InvoiceLineUseCusAddInfo)
				{
					yield return new FetchHint(CusAddInfoSchema.B7_ParentID, invoiceLinePK);
				}
				if (FetchHintDecider.InvoiceLineUseCusCodeData)
				{
					yield return new FetchHint(CusCodeDataSchema.CY_ParentID, invoiceLinePK);
				}
				if (FetchHintDecider.InvoiceLineUseCusSupportingInfo)
				{
					yield return new FetchHint(CusSupportingInfoSchema.CSI_ParentID, invoiceLinePK);
				}
			}
		}

		protected virtual IEnumerable<IFetchHint> GetOrgAddressRelatedFetchHints(IColumnIndexer row)
		{
			var orgAddressPK = row.GetValue(OrgAddressSchema.PK);
			yield return new FetchHint(OrgTranslatedAddressSchema.OTA_OA, orgAddressPK);
			yield return new FetchHint(OrgAddressCapabilitySchema.PZ_OA, orgAddressPK);
		}

		protected virtual IEnumerable<IFetchHint> GetOrgSupplierPartRelatedFetchHints(IColumnIndexer row)
		{
			var partPK = row.GetValue(OrgSupplierPartSchema.PK);
			yield return new FetchHint(OrgPartRelationSchema.OU_OP, partPK);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, partPK);
			yield return new FetchHint(CusClassPartPivotSchema.CI_OP, partPK);
		}

		protected virtual IEnumerable<IFetchHint> GetCusClassPartPivotRelatedFetchHints(IColumnIndexer row)
		{
			var pivotPK = row.GetValue(CusClassPartPivotSchema.PK);
			yield return new FetchHint(CusUSClassificationSchema.CD_ParentID, pivotPK);
			var fetchHint = GetFetchHintIfNotEmpty(CusClassificationSchema.PK, row, CusClassPartPivotSchema.CI_CC);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
			var parentPK = row.GetValue(CusClassPartPivotSchema.CI_CI_Parent);
			if (parentPK.IsValid && parentPK != pivotPK)
			{
				yield return new FetchHint(CusClassPartPivotSchema.PK, parentPK);
			}
			fetchHint = GetFetchHintIfNotEmpty(OrgHeaderSchema.PK, row, CusClassPartPivotSchema.CI_OH);
			if (fetchHint != null)
			{
				yield return fetchHint;
			}
		}

		protected void AddFetchHints(IExternalFetchHintSupporter supporter, IColumnIndexer row, SchemaColumn rowColumn, params SchemaColumn[] columns)
		{
			foreach (var fetchHint in GetFetchHintsIfNotEmpty(row, rowColumn, columns))
			{
				supporter.AddFetchHint(fetchHint);
			}
		}

		protected IEnumerable<FetchHint> GetFetchHintsIfNotEmpty(IColumnIndexer row, SchemaColumn rowColumn, params SchemaColumn[] columns)
		{
			var value = row.GetValue(rowColumn);
			if (!value.IsEmpty)
			{
				foreach (var column in columns)
				{
					yield return new FetchHint(column, value);
				}
			}
		}

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, Dictionary<ZString, ZString> addInfos, SchemaStringColumn addInfoColumn)
		{
			return GetFetchHintIfNotEmpty(column, addInfos.GetValue(addInfoColumn));
		}

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, Dictionary<ZString, ZString> addInfos, SchemaDecimalColumn addInfoColumn)
		{
			return GetFetchHintIfNotEmpty(column, addInfos.GetValue(addInfoColumn));
		}

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, Dictionary<ZString, ZString> addInfos, SchemaBoolColumn addInfoColumn)
		{
			return GetFetchHintIfNotEmpty(column, addInfos.GetValue(addInfoColumn));
		}

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, Dictionary<ZString, ZString> addInfos, SchemaDateTimeColumn addInfoColumn)
		{
			return GetFetchHintIfNotEmpty(column, addInfos.GetValue(addInfoColumn));
		}

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, Dictionary<ZString, ZString> addInfos, SchemaIntColumn addInfoColumn)
		{
			return GetFetchHintIfNotEmpty(column, addInfos.GetValue(addInfoColumn));
		}

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, Dictionary<ZString, ZString> addInfos, SchemaShortColumn addInfoColumn)
		{
			return GetFetchHintIfNotEmpty(column, addInfos.GetValue(addInfoColumn));
		}

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, Dictionary<ZString, ZString> addInfos, SchemaGuidColumn addInfoColumn)
		{
			return GetFetchHintIfNotEmpty(column, addInfos.GetValue(addInfoColumn));
		}

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, IColumnIndexer row, SchemaColumn rowColumn)
		{
			return GetFetchHintIfNotEmpty(column, row.GetValue(rowColumn));
		}

		FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, IZType value)
		{
			FetchHint result = null;
			if (!value.IsEmpty)
			{
				result = new FetchHint(column, value);
			}
			return result;
		}

		internal void PopulateDataObject(CusEntryHeader entryBO, Shipment declarationData)
		{
			PopulateDataObjectCore(entryBO.Declaration, declarationData, false, entryBO);
		}

		protected sealed override void PopulateDataObject(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			PopulateDataObjectCore(declarationBO, declarationData, false, null);
		}

		protected virtual UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			return new UniversalDataObjectWriterHelper(declarationBO.Factory, declarationBO.CountryCode);
		}

		protected ILandedCostDataWriter landedCostDataWriter
		{
			get;
			private set;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Someone else can do that.")]
		void PopulateDataObjectCore(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData, CusEntryHeader relatedEntry)
		{
			landedCostDataWriter = null;
			if (helper == null || helper.countryCode != declarationBO.CountryCode)
			{
				helper = CreateNewUniversalDataObjectWriterHelper(declarationBO);
			}

			bool isTopLevelContextCustomsDeclaration = IsTopLevelContextCustomsDeclaration(declarationBO);

			declarationData.SetDateCollection(() => declarationData.DateCollection ?? new List<Date>());
			PopulateShipmentData(declarationBO, declarationData, keepExistingData);
			var landedCostHeader = ((ILandedCostHeader)declarationBO).IsLCSupported ? declarationBO.LandedCostHeaderForDocuments as Integration.LandedCosting.ILandedCostHeader : null;
			if (landedCostHeader != null)
			{
				landedCostDataWriter = ObjectFactory.New<ILandedCostDataWriter>(writeManager, landedCostHeader);
			}
			if (ContainerOverride == null)
			{
				PopulateContainerData(declarationBO, declarationData, keepExistingData);
			}
			if (isTopLevelContextCustomsDeclaration)
			{
				PopulatePackageData(declarationBO, declarationData);
			}
			var entryInstructions = declarationBO.CustomsEntryInstructionProvider?.CustomsEntryInstructions;
			if (entryInstructions != null)
			{
				declarationData.SetEntryInstructionCollection(() => ProcessCollection(entryInstructions, GetNewCustomsEntryInstructionDataObjectWriter(), true));
			}
			if (relatedEntry != null)
			{
				helper.SetupEntryRelatedCommercialInfo(relatedEntry);
			}
			PopulateCommercialInfo(declarationBO, declarationData, landedCostHeader, relatedEntry);

			if (isTopLevelContextCustomsDeclaration && !keepExistingData)
			{
				declarationData.SetTransportLegCollection(() =>
				{
					var shipmentBO = declarationBO.Shipment;
					var transportLegs = shipmentBO == null ? declarationBO.TransportsIncludingRelated : shipmentBO.TransportsIncludingRelated;
					transportLegs.Sort(MovementLegComparer.PortsAndDatesBased(transportLegs));
					return ProcessCollection(transportLegs, new TransportLegDataObjectWriter(writeManager), CollectionContent.Complete, true);
				});

				var notes = declarationBO.IsPluggedIntoShipment && declarationBO.Shipment != null ? declarationBO.Shipment.Notes : declarationBO.Notes;
				var noteToPopulate = notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				declarationData.SetNoteCollection(() => ProcessCollection(noteToPopulate, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
			}
			declarationData.SetOrganizationAddressCollection(() => declarationData.OrganizationAddressCollection.MergeCollection(ProcessCollection(declarationBO.DocAddresses, GetNewJobDocAddressDataObjectWriter()), keepExistingData, UniversalDataObjectWriterHelper.IsOrganizationAddressTypeMatched));
			PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);
			PopulateDates(declarationBO, declarationData, keepExistingData);

			PopulateCusEntryNumberData(declarationBO, declarationData, keepExistingData);
			declarationData.SetAdditionalReferenceCollection(() => declarationData.AdditionalReferenceCollection.MergeCollectionByCandidateKey(ProcessCollection(declarationBO.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager)), keepExistingData));
			declarationData.SetAddInfoCollection(() =>
			{
				var addInfos = CreateDeclarationAddInfo(declarationBO, declarationData);
				PopulateDeclarationAddInfoFromRealFieldCore(declarationBO, addInfos);
				return addInfos;
			});
			PopulateCusAddInfoData(declarationBO, declarationData);
			declarationData.SetCustomsReferenceCollection(() => CustomsReferenceCollectionCreator.CreateCollection(helper, declarationBO, writeManager));
			PopulateCusSupportingInfoData(declarationBO, declarationData);

			if (isTopLevelContextCustomsDeclaration && !keepExistingData)
			{
				PopulateLocalProcessingFromDocsAndCartage(declarationData, declarationBO);
				PopulateDocsAndCartageCustomAttributesToCustomizedFields(declarationData, declarationBO);
			}

			if (!keepExistingData)
			{
				PopulateProFormaInvoiceDataToCustomizedFields(declarationData, declarationBO);
			}

			if (declarationBO.IsPluggedIntoShipment)
			{
				var customizedFieldContainer = declarationData as ICustomizedFieldContainer;
				if (customizedFieldContainer != null)
				{
					PopulateWorkflowCustomFields(declarationBO, customizedFieldContainer);
				}
			}

			var oldSortInformation = declarationBO.ActiveEntryHeaders.SortInformation;
			try
			{
				declarationBO.ActiveEntryHeaders.Sort(CusEntryHeader.Schema.CH_CH_PrimeEntry);
				var entries = GetEntryHeadersToPopulate(declarationBO);
				declarationData.SetEntryHeaderCollection(() => ProcessCollection(entries, GetNewCustomsEntryHeaderDataObjectWriter(), true));
			}
			finally
			{
				if (oldSortInformation != null)
				{
					declarationBO.ActiveEntryHeaders.Sort(oldSortInformation);
				}
			}

			if (declarationData.DateCollection?.Count == 0)
			{
				declarationData.SetDateCollection(() => null);
			}
			MergeCountrySpecificRelatedData(isTopLevelContextCustomsDeclaration, declarationBO, declarationData);

			declarationData.SetRelatedShipmentCollection(() => declarationBO.JE_JS.IsEmpty ? ProcessCollection(declarationBO.AttachedOrders, new OrderDataObjectWriter(writeManager)) : declarationData.RelatedShipmentCollection);

			declarationData.CustomsProfileIdentifier = new ValueTypePair
			{
				Type = new ZString(GetCustomsProfileType(declarationBO).ToString()),
				Value = declarationBO.JE_CustomsProfile
			};

			if (ShouldPopulateAttachedDocumentCollection)
			{
				PopulateAttachedDocumentCollection(declarationData, declarationBO);
			}

			PopulateCountrySpecificData(declarationData, declarationBO);
		}

		protected override void PopulateWorkflowCustomFields(BaseJobDeclaration sourceBO, ref Shipment dataObject)
		{
			if (!sourceBO.IsPluggedIntoShipment)
			{
				base.PopulateWorkflowCustomFields(sourceBO, dataObject);
			}
		}

		protected override IEnumerable<IPropertyValue> GetAllCustomPropertiesWithDefaultValue(BaseJobDeclaration sourceBO)
		{
			if (sourceBO.IsPluggedIntoShipment)
			{
				return GetUserDefinedValues(sourceBO);
			}
			return base.GetAllCustomPropertiesWithDefaultValue(sourceBO);
		}

		protected virtual void PopulateAttachedDocumentCollection(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			var eDocs = GeteDocsToPopulate(declarationBO);
			declarationData.SetAttachedDocumentCollection(() => eDocs.Select(x => new AttachedDocument
			{
				FileName = x.FileName,
				ImageData = (SubStreamableStream)new MemoryStream(x.ImageData),
				Type = new DocumentType
				{
					Code = x.DocType,
					Description = x.Description
				}
			}).ToList());
		}

		protected virtual IEnumerable<IeDoc> GeteDocsToPopulate(BaseJobDeclaration declarationBO)
		{
			return Enumerable.Empty<IeDoc>();
		}

		static CustomsProfileType GetCustomsProfileType(BaseJobDeclaration declarationBO)
		{
			CustomsProfileType result;
			var countryCode = declarationBO.CountryCode;
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.UnitedKingdom:
					result = CustomsProfileType.AgentCode;
					break;
				case Core.Constants.CountryCodes.Italy:
					result = CustomsProfileType.Node;
					break;
				default:
					result = CustomsProfileType.UserName;
					break;
			}
			return result;
		}

		protected virtual IEnumerable<CusEntryHeader> GetEntryHeadersToPopulate(BaseJobDeclaration dedDeclarationBO)
		{
			return (entryHeaderPKsToPopulate == null
					? dedDeclarationBO.ActiveEntryHeaders.Cast<CusEntryHeader>()
					: dedDeclarationBO.ActiveEntryHeaders.Where(x => entryHeaderPKsToPopulate.Contains(x.PK)));
		}

		IEnumerable<ZGuid> entryHeaderPKsToPopulate;

		internal void SetEntryHeaderPKsToPopulate(IEnumerable<ZGuid> entryHeaderPKs)
		{
			entryHeaderPKsToPopulate = entryHeaderPKs;
		}

		protected virtual void PopulateCountrySpecificData(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
		}

		void PopulateLocalProcessingFromDocsAndCartage(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			var docsBO = declarationBO.DocsAndCartage;
			if (docsBO != null)
			{
				var writer = new LocalProcessingDataObjectWriter(writeManager);
				declarationData.LocalProcessing = writer.GetDataObject(docsBO);
			}
		}

		void PopulateDocsAndCartageCustomAttributesToCustomizedFields(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			var docsBO = declarationBO.DocsAndCartage;
			if (docsBO != null)
			{
				declarationData.SetCustomizedFieldCollection(() =>
				{
					var customFieldsWritingHelper = new CustomFieldsDataObjectWritingHelper<JobDocsAndCartage>(docsBO, new JobDocsAndCartageCustomFieldsDescriptor());
					var docsBOCustomFields = customFieldsWritingHelper.GetCustomizedFieldValues().ToArray();

					if (docsBOCustomFields.Any())
					{
						var customizedFieldCollection = declarationData.CustomizedFieldCollection ?? new List<CustomizedField>();
						customizedFieldCollection.AddRange(docsBOCustomFields);
						return customizedFieldCollection.Count > 0 ? customizedFieldCollection : null;
					}
					return declarationData.CustomizedFieldCollection;
				});
			}
		}

		void PopulateProFormaInvoiceDataToCustomizedFields(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			declarationData.SetCustomizedFieldCollection(() =>
			{
				var formaInvoiceList = PopulateProFormaInvoiceData(declarationBO);
				if (formaInvoiceList != null)
				{
					var customizedFieldCollection = declarationData.CustomizedFieldCollection ?? new List<CustomizedField>();
					customizedFieldCollection.AddRange(formaInvoiceList);
					return customizedFieldCollection.Count > 0 ? customizedFieldCollection : null;
				}
				return declarationData.CustomizedFieldCollection;
			});
		}

		IEnumerable<CustomizedField> PopulateProFormaInvoiceData(BaseJobDeclaration declarationBO)
		{
			var supplier = declarationBO.Supplier;
			if (supplier == null)
			{ return null; }
			var list = new List<CustomizedField>();

			if (!supplier.ExportersBankName.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.ExportersBankName, supplier.ExportersBankName));
			}

			if (!supplier.ExportersBankAccount.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.ExportersBankAccount, supplier.ExportersBankAccount));
			}

			if (!supplier.ExportersSwiftCode.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.ExportersSwiftCode, supplier.ExportersSwiftCode));
			}

			if (!supplier.MethodOfPayment.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.MethodOfPayment, supplier.MethodOfPayment));
			}

			if (!supplier.AdditionalInformation.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.AdditionalInformation, supplier.AdditionalInformation));
			}

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.HongKong)
			{
				if (supplier.CountryDataCollectionForThisCompany != null)
				{
					var org = supplier.CountryDataCollectionForThisCompany.Cast<OrgCountryData>().FirstOrDefault(x => !x.OV_EXApprovalNumber.IsEmpty);
					if (org != null)
					{
						list.Add(GetCustomizedField(SDFields.ApprovalNumber, org.OV_EXApprovalNumber));
					}
				}
			}

			var docNotelist = PopulateDocNoteData(declarationBO);
			if (docNotelist.Any())
			{
				list.AddRange(docNotelist);
			}

			return list;
		}

		IEnumerable<CustomizedField> PopulateDocNoteData(BaseJobDeclaration declarationBO)
		{
			var list = new List<CustomizedField>();
			var docNote = declarationBO.DocNote;

			ZString letterOfCreditNumber = docNote.GetSystemDefinedFieldValue(SDFields.LetterOfCreditNumber);
			if (!letterOfCreditNumber.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.LetterOfCreditNumber, letterOfCreditNumber));
			}

			ZString letterOfCreditDate = docNote.GetSystemDefinedFieldValue(SDFields.LetterOfCreditDate);
			if (!letterOfCreditDate.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.LetterOfCreditDate, letterOfCreditDate));
			}

			ZString insuranceNumber = docNote.GetSystemDefinedFieldValue(SDFields.InsurancePolicyNumber);
			if (!insuranceNumber.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.InsurancePolicyNumber, insuranceNumber));
			}

			ZString insuredValue = docNote.GetSystemDefinedFieldValue(SDFields.InsuredValue);
			if (!insuredValue.IsEmpty)
			{
				list.Add(GetCustomizedField(SDFields.InsuredValue, insuredValue));
			}

			return list;
		}

		CustomizedField GetCustomizedField(ZString key, ZString value)
		{
			return new CustomizedField() { Key = key, DataType = DataType.String, Value = value };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SDField Code")]
		public static class SDFields
		{
			public const string ApprovalNumber = "ApprovalNumber";
			public const string ExportersBankName = "ExportersBankName";
			public const string ExportersBankAccount = "ExportersBankAccount";
			public const string ExportersSwiftCode = "ExportersSwiftCode";
			public const string MethodOfPayment = "MethodOfPayment";
			public const string AdditionalInformation = "Additional Information";
			public const string InsurancePolicyNumber = "Insurance Policy Number";
			public const string InsuredValue = "Insured Value (Include Currency)";
			public const string LetterOfCreditNumber = "Letter of Credit Number";
			public const string LetterOfCreditDate = "Letter of Credit Date";
		}

		protected virtual CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriter()
		{
			return new CustomsEntryInstructionDataObjectWriter(writeManager, helper);
		}

		protected virtual void MergeCountrySpecificRelatedData(ZBool isTopLevelContextCustomsDeclaration, BaseJobDeclaration declarationBO, Shipment declarationData)
		{
		}

		protected virtual CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}

		protected virtual JobDocAddressDataObjectWriter GetNewJobDocAddressDataObjectWriter()
		{
			return new JobDocAddressDataObjectWriter(writeManager);
		}

		protected void PopulateCusSupportingInfoData(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			if (declarationBO is ICusSupportingInfoTypeSupporter)
			{
				PopulateCusSupportingInfoDataCore(declarationBO, declarationData);
			}
		}

		protected virtual void PopulateCusSupportingInfoDataCore(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.SetCustomsSupportingInformationCollection(() => CustomsSupportingInformationCollectionCreator.CreateCollection(helper, declarationBO, writeManager));
		}

		protected virtual void PopulateCusAddInfoData(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.SetAddInfoGroupCollection(() => AddInfoGroupCollectionCreator.CreateCollection(helper, declarationBO, writeManager));
		}

		protected virtual List<AddInfo> CreateDeclarationAddInfo(BaseJobDeclaration declarationBO, Shipment declarationData) => AddInfoCollectionCreator.CreateCollection(declarationBO, JobDeclarationSchema.JE_AddInfo);

		protected virtual void PopulateDeclarationAddInfoFromRealFieldCore(BaseJobDeclaration declarationBO, List<AddInfo> addInfos)
		{
			if (!declarationBO.JE_TransportModeInland.IsEmpty)
			{
				addInfos.AddIfMissing(Constants.AddInfoKeys.Declaration.InlandModeOfTransport, declarationBO.JE_TransportModeInland);
			}
			if (declarationBO.SupportUseOwnerRefAsQuarantineRefUsage)
			{
				addInfos.AddIfMissing(Constants.AddInfoKeys.Declaration.UseOwnerRefAsQuarantineRef, declarationBO.JE_UseOwnerRefAsQuarantineRef ? YesNoList.Codes.Yes : YesNoList.Codes.No);
			}
		}

		protected virtual void PopulateDates(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			declarationData.SetDateCollection(() =>
			{
				var list = new List<Date>();
				list.Add(DateType.Departure, ZBool.True, declarationBO.JE_DateAtOrigin);
				list.Add(DateType.LoadingDate, ZBool.False, declarationBO.JE_ExportDate);
				list.Add(DateType.FirstArrivalInCountry, ZBool.False, declarationBO.JE_DateOfFirstArrival);
				list.Add(DateType.DischargeDate, ZBool.False, declarationBO.JE_DateOfArrival);
				list.Add(DateType.Arrival, ZBool.True, declarationBO.JE_DateAtFinalDestination);
				list.Add(DateType.EntrySubmitted, ZBool.False, declarationBO.JE_EntrySubmittedDate);
				list.Add(DateType.EntryAuthorisation, ZBool.False, declarationBO.JE_EntryAuthorisationDate);
				list.Add(DateType.WarehouseRelease, ZBool.False, declarationBO.JE_WarehouseReleaseDate);
				list.Add(DateType.EntryDate, ZBool.False, declarationBO.JE_EntryDate);
				return declarationData.DateCollection.MergeCollectionByCandidateKey(list, keepExistingData);
			});
		}

		protected virtual void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData) // TODO Change this to DataRow
		{
			var existingCollection = declarationData.OrganizationAddressCollection;
			declarationData.SetOrganizationAddressCollection(() => null);

			bool isTopLevelContextCustomsDeclaration = IsTopLevelContextCustomsDeclaration(declarationBO);

			if (isTopLevelContextCustomsDeclaration && !keepExistingData)
			{
				var job = declarationBO.Job;
				if (job != null)
				{
					declarationData.AddOrgAddress(writeManager, job.LocalChargesAddr, AddressTypes.SendersLocalClient);
				}
			}
			var supplier = declarationBO.Supplier;
			if (supplier != null && supplier != declarationBO.SupplierDocumentaryAddress.Organisation)
			{
				declarationData.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			}
			var importer = declarationBO.Importer;
			if (importer != null && importer != declarationBO.ImporterDocumentaryAddress.Organisation)
			{
				declarationData.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			}
			var carrierCode = declarationBO.JE_CarrierCode;
			if (!carrierCode.IsEmpty)
			{
				var declarationCountry = declarationBO.Country;
				var carrier = OrgHeader.FindByOrgCusCode(declarationBO.Factory, OrgCusCode.CodeTypes.CarrierCode, carrierCode, declarationCountry?.RN_Code ?? ZString.Empty);
				if (carrier != null)
				{
					declarationData.AddOrgAddress(writeManager, carrier, DocAddressType.Carrier);
				}
				else
				{
					var carrierCodes = declarationBO.Lookups.CarrierCodeCollection as IFindBoxListProvider;
					var carrierName = carrierCodes?.DescriptionFromCode(carrierCode);
					declarationData.AddDummyOrganizationForCodeOnly(writeManager.WriterStrategy, DocAddressType.Carrier, carrierName, declarationCountry, OrgCusCode.CodeTypes.CarrierCode, carrierCode);
				}
			}

			declarationData.AddOrgAddress(writeManager, declarationBO.Forwarder, AddressTypes.Forwarder);
			declarationData.AddOrgAddress(writeManager, declarationBO.ShippingLine, AddressTypes.ShippingLine);
			declarationData.AddOrgAddress(writeManager, declarationBO.DeclarantAddress, AddressTypes.Declarant);

			PopulateManufacturer(declarationBO, declarationData);
			PopulateShipToParty(declarationBO, declarationData);
			PopulateSeller(declarationBO, declarationData);
			PopulateSoldToPartyAddress(declarationBO, declarationData);
			PopulateSellingAgent(declarationBO, declarationData);
			PopulateBuyingAgent(declarationBO, declarationData);
			PopulateExporter(declarationBO, declarationData);
			PopulateConsigneeAddress(declarationBO, declarationData);
			PopulateConsignee(declarationBO, declarationData);
			PopulateBuyer(declarationBO, declarationData);
			PopulateRepresentative(declarationBO, declarationData);
			PopulateControllingAgent(declarationBO, declarationData);
			PopulateControllingCustomer(declarationBO, declarationData);
			PopulateExternalBroker(declarationBO, declarationData);

			if (isTopLevelContextCustomsDeclaration && !keepExistingData)
			{
				if (declarationBO.IsExport)
				{
					declarationData.AddOrgAddress(writeManager, declarationBO.DeliveryOrPickupCartageCoAddr, AddressTypes.PickupLocalCartage);
				}
				else
				{
					declarationData.AddOrgAddress(writeManager, declarationBO.DeliveryOrPickupCartageCoAddr, AddressTypes.DeliveryLocalCartage);
				}
			}
			declarationData.SetOrganizationAddressCollection(() => existingCollection.MergeCollection(declarationData.OrganizationAddressCollection, keepExistingData, UniversalDataObjectWriterHelper.IsOrganizationAddressTypeMatched));
		}

		protected virtual void PopulateManufacturer(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.ManufacturerAddress, DocAddressType.Manufacturer);
		}

		protected virtual void PopulateShipToParty(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.ShipToPartyAddress, DocAddressType.ShipToParty);
		}

		protected virtual void PopulateSeller(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.SellerAddress, Constants.AddressTypes.Seller);
		}

		protected virtual void PopulateSoldToPartyAddress(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.SoldToPartyAddress, Constants.AddressTypes.SoldToParty);
		}

		protected virtual void PopulateBuyingAgent(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.BuyingAgent, Constants.AddressTypes.BuyingAgent);
		}

		protected virtual void PopulateSellingAgent(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.SellingAgent, Constants.AddressTypes.SellingAgent);
		}

		protected virtual void PopulateExporter(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.Exporter, DocAddressType.Exporter);
		}

		protected virtual void PopulateConsigneeAddress(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.ConsigneeAddress, Constants.AddressTypes.UltimateConsignee);
		}

		protected virtual void PopulateConsignee(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.IntermConsignee, Constants.AddressTypes.IntermediateConsignee);
		}

		protected virtual void PopulateBuyer(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.Buyer, DocAddressType.BuyerDocumentaryAddress);
		}

		protected virtual void PopulateRepresentative(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.Representative, DocAddressType.Representative);
		}

		protected virtual void PopulateControllingAgent(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.ControllingAgent, DocAddressType.ControllingAgent);
		}

		protected virtual void PopulateControllingCustomer(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.ControllingCustomer, DocAddressType.ControllingCustomer);
		}

		protected virtual void PopulateExternalBroker(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			declarationData.AddOrgAddress(writeManager, declarationBO.ExternalBroker, DocAddressType.ExternalBroker);
		}

		void PopulateCommercialInfo(BaseJobDeclaration declarationBO, Shipment declarationData, Integration.LandedCosting.ILandedCostHeader landedCostHeader, CusEntryHeader relatedEntry)
		{
			var topGroupInvoice = declarationBO.TopGroupInvoice;
			if (topGroupInvoice != null)
			{
				declarationData.CommercialInfo = CreateCommercialInfo(topGroupInvoice, relatedEntry);
				if (landedCostHeader != null)
				{
					declarationData.CommercialInfo.DateOfLandedCostProcessing = landedCostHeader.LT_DateOfProcessing;
				}
			}
		}

		UniversalCustoms.CommercialInfo CreateCommercialInfo(BaseJobComInvoiceGroupHeader topGroupInvoice, CusEntryHeader relatedEntry)
		{
			var commercialInfo = new UniversalCustoms.CommercialInfo()
			{
				Name = topGroupInvoice.JZ_InvoiceNumber,
				CommercialChargeCollection = CommercialChargeCollectionCreator.CreateCollection(helper, topGroupInvoice.PK),
				CustomsReferenceCollection = CustomsReferenceCollectionCreator.CreateCollection(helper, topGroupInvoice, writeManager, Constants.DataContext.InvoiceGroup),
				AddInfoCollection = GetInvoiceGroupAddInfoCollection(topGroupInvoice),
				AddInfoGroupCollection = GetInvoiceGroupAddInfoGroupCollection(topGroupInvoice)
			};

			commercialInfo.CommercialInvoiceCollection = GetCommercialInvoiceCollection(topGroupInvoice.JobComInvoiceHeaders.Cast<BaseJobComInvoiceHeader>().Where(x => helper.IsRelatedToEntry(x)).ToArray(), GetNewCommercialInvoiceHeaderDataObjectWriter(relatedEntry));

			if (landedCostDataWriter != null)
			{
				commercialInfo.TransportLogisticsCostCollection = landedCostDataWriter.PopulateTransportLogisticsCostCollection(topGroupInvoice);
			}
			var invoiceGroupHeaderBOs = topGroupInvoice.JobComInvoiceGroupHeaders.Cast<BaseJobComInvoiceGroupHeader>().Where(x => helper.IsRelatedToEntry(x)).ToArray();
			if (invoiceGroupHeaderBOs.Length > 0)
			{
				var groupCollection = new List<UniversalCustoms.CommercialInfo>();
				foreach (BaseJobComInvoiceGroupHeader groupHeader in invoiceGroupHeaderBOs)
				{
					groupCollection.Add(CreateCommercialInfo(groupHeader, relatedEntry));
				}
				commercialInfo.SubGroupCollection = groupCollection;
			}
			return commercialInfo;
		}

		protected virtual List<AddInfo> GetInvoiceGroupAddInfoCollection(BaseJobComInvoiceGroupHeader invoiceGroup)
		{
			return AddInfoCollectionCreator.CreateCollection(invoiceGroup, JobComInvoiceHeaderSchema.JZ_AddInfo);
		}

		protected virtual List<UniversalCustoms.AddInfoGroup> GetInvoiceGroupAddInfoGroupCollection(BaseJobComInvoiceGroupHeader invoiceGroup)
		{
			return AddInfoGroupCollectionCreator.CreateCollection(helper, invoiceGroup, writeManager, dataContext: Constants.DataContext.InvoiceGroup);
		}

		protected virtual DataObjectList<UniversalCustoms.CommercialInvoiceHeader> GetCommercialInvoiceCollection(BaseJobComInvoiceHeader[] invoiceBOs, CommercialInvoiceHeaderDataObjectWriter commercialInvoiceHeaderDataObjectWriter)
		{
			return ProcessCollection(invoiceBOs, commercialInvoiceHeaderDataObjectWriter, CollectionContent.Complete, true);
		}

		protected virtual CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(CusEntryHeader relatedEntry)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		void PopulateCusEntryNumberData(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			declarationData.SetEntryNumberCollection(() =>
			{
				var query = new ZQuery(CusEntryNumSchema.CE_ParentID, declarationBO.PK);
				query.AddToFilter(CusEntryNumSchema.CE_Category, SQLComparisonOperator.NotEqual, CusEntryNumber.Categories.AdditionalReferenceNumber);
				query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
				var entryNumbers = helper.Load<CusEntryNumber>(query);
				var entryNumberCollection = new List<EntryNumber>();

				if (entryNumbers.Length > 0)
				{
					var entryNumberTypeList = CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(declarationBO.Factory, declarationBO.CountryCode, declarationBO.IsImport);
					foreach (CusEntryNumber entryNumberBO in entryNumbers)
					{
						var entryNumberData = new EntryNumber()
						{
							CountryOfIssue = ListHelper.GetWithName<Country>(entryNumberBO.CE_RN_NKCountryCode, entryNumberBO.Lookups.Countries),
							//					EntryLineReference = entryNumberBO.CE_EntryLineReference,// speak to Ben before adding this back
							IssueDate = entryNumberBO.CE_IssueDate,
							Number = entryNumberBO.CE_EntryNum,
							Type = ListHelper.GetWithDescription<EntryType>(entryNumberBO.CE_EntryType, entryNumberTypeList),
							EntryIsSystemGenerated = entryNumberBO.CE_EntryIsSystemGenerated
						};

						entryNumberCollection.Add(entryNumberData);
					}
				}

				entryNumberCollection.AddRange(PopulateCusDisposition(declarationBO));
				if (entryNumberCollection.Count > 0)
				{
					return declarationData.EntryNumberCollection.MergeCollectionByCandidateKey(entryNumberCollection, keepExistingData);
				}
				else
				{
					return declarationData.EntryNumberCollection;
				}
			});
		}

		protected virtual List<EntryNumber> PopulateCusDisposition(BaseJobDeclaration declarationBO)
		{
			return new List<EntryNumber>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		void PopulateContainerData(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			declarationData.SetContainerCollection(() =>
			{
				var containerCollection = !keepExistingData || declarationData.ContainerCollection == null ? new DataObjectList<Container>() : declarationData.ContainerCollection;
				var declarationContainers = GetContainersToWrite(declarationBO);
				var shipmentContainers = GetContainersToWrite(declarationBO.Shipment);
				var hasDeclarationContainers = declarationContainers.Any();
				var hasShipmentContainers = shipmentContainers != null && shipmentContainers.Any();
				if (hasDeclarationContainers || hasShipmentContainers)
				{
					if (!keepExistingData)
					{
						containerCollection.Content = ContainerOverride == null ? CollectionContent.Complete : CollectionContent.Partial;
					}
					var containerObjectWriter = new ContainerDataObjectWriter(helper.BindToLists, writeManager);
					var containerCustomLabelsProvider = helper.GetCusContainerCustomLabelsProvider(declarationBO);
					if (hasDeclarationContainers)
					{
						foreach (var containerBO in declarationContainers)
						{
							PopulateContainerDataFromDeclaration(keepExistingData, containerCollection, containerObjectWriter, containerCustomLabelsProvider, containerBO, declarationBO);
						}
					}
					else if (hasShipmentContainers)
					{
						foreach (var containerBO in shipmentContainers)
						{
							PopulateContainerDataFromShipment(keepExistingData, containerCollection, containerObjectWriter, containerBO);
						}
					}
				}
				return containerCollection;
			});
		}

		void PopulateContainerDataFromDeclaration(bool keepExistingData, DataObjectList<Container> containerCollection, ContainerDataObjectWriter containerObjectWriter, ICustomLabelsProvider containerCustomLabelsProvider, BaseCusContainer containerBO, BaseJobDeclaration declarationBO)
		{
			Container containerData = null;
			if (keepExistingData)
			{
				containerData = containerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == containerBO.CO_ContainerNumber);
			}

			if (containerData == null)
			{
				var jobContainer = containerBO.JobContainer;
				containerData = jobContainer == null ? new Container(writeManager.WriterStrategy) : containerObjectWriter.GetDataObject(jobContainer);

				containerData.ContainerNumber = containerBO.CO_ContainerNumber;
				containerData.Seal = containerBO.CO_Seal;
				containerData.SecondSeal = containerBO.CO_SecondSeal;
				containerData.FCL_LCL_AIR = ListHelper.GetWithDescription<ContainerMode>(containerBO.CO_FCL_LCL_AIR, containerBO.Lookups.CO_FCL_LCL_NCT_List);
				containerData.GoodsWeight = containerBO.CO_Weight;
				containerData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(containerBO.CO_WeightUQ, containerBO.Lookups.WeightUnits);
				if (containerBO.UseContainerSize)
				{
					containerData.CustomsContainerSize = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(containerBO.CO_ContainerSize, containerBO.Lookups.ContainerSizeList);
				}
				containerData.MessageStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(containerBO.CO_MessageStatus, containerBO.Lookups.MessageStatusList);
				containerData.SetAddInfoGroupCollection(() => AddInfoGroupCollectionCreator.CreateCollection(helper, containerBO, writeManager));
				containerData.SetCustomsReferenceCollection(() => CustomsReferenceCollectionCreator.CreateCollection(helper, containerBO, writeManager));

				PopulateContainerAddInfo(containerData, containerBO);
				PopulateCountrySpecificContainerValue(containerData, containerBO, declarationBO);

				var refContainer = containerBO.Container;
				PopulateContainerVolumn(containerData, refContainer);

				CustomLabelsCustomizedFieldDataObjectWriter.Write(CusContainerSchema.Instance, containerBO, containerData, containerCustomLabelsProvider);
			}
			containerData.SetTransportLogisticsCostCollection(() => landedCostDataWriter?.PopulateTransportLogisticsCostCollection(containerBO));
			if (!containerCollection.Contains(containerData))
			{
				containerCollection.Add(containerData);
			}
		}

		protected virtual void PopulateCountrySpecificContainerValue(Container containerData, BaseCusContainer containerBO, BaseJobDeclaration declarationBO)
		{
		}

		protected virtual void PopulateContainerAddInfo(Container containerData, BaseCusContainer containerBO)
		{
			containerData.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(containerBO, CusContainerSchema.CO_AddInfo));
		}

		void PopulateContainerDataFromShipment(bool keepExistingData, DataObjectList<Container> containerCollection, ContainerDataObjectWriter containerObjectWriter, CommonContainer containerBO)
		{
			Container containerData = null;
			if (keepExistingData)
			{
				containerData = containerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == containerBO.JC_ContainerNum);
			}

			if (containerData == null)
			{
				containerData = containerObjectWriter.GetDataObject(containerBO);

				var refContainer = containerBO.Container;
				PopulateContainerVolumn(containerData, refContainer);
			}
			containerData.SetTransportLogisticsCostCollection(() => landedCostDataWriter?.PopulateTransportLogisticsCostCollection(containerBO));
			if (!containerCollection.Contains(containerData))
			{
				containerCollection.Add(containerData);
			}
		}

		void PopulateContainerVolumn(Container containerData, RefContainer refContainer)
		{
			if (refContainer != null)
			{
				containerData.ContainerType = ContainerType.New(refContainer);
				if (containerData.TotalHeight.GetValueOrDefault().IsEmpty)
				{
					containerData.TotalHeight = refContainer.RC_Height;
				}
				if (containerData.TotalWidth.GetValueOrDefault().IsEmpty)
				{
					containerData.TotalWidth = refContainer.RC_Width;
				}
				if (containerData.TotalLength.GetValueOrDefault().IsEmpty)
				{
					containerData.TotalLength = refContainer.RC_Length;
				}
			}
		}

		IEnumerable<CommonContainer> GetContainersToWrite(ForwardingShipment shipment)
		{
			return shipment?.Containers;
		}

		IEnumerable<BaseCusContainer> GetContainersToWrite(BaseJobDeclaration declaration)
		{
			return ContainerOverride != null ? new[] { ContainerOverride } : declaration.CusContainers.Cast<BaseCusContainer>();
		}

		BaseCusContainer ContainerOverride { get; set; }

		void PopulatePackageData(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			var supportsChcPivotBetweenInvoiceLineAndPacking = declarationBO.SupportsChcPivotBetweenInvoiceLineAndPacking;
			if (declarationBO.IsPackingInformationRelevant)
			{
				PopulatePackageDataFromPackage(declarationBO, declarationData, supportsChcPivotBetweenInvoiceLineAndPacking);
			}
			else
			{
				PopulateDummyPackageDataForContainerLinkIfNeeded(declarationBO, declarationData, supportsChcPivotBetweenInvoiceLineAndPacking);
			}
		}

		void PopulateDummyPackageDataForContainerLinkIfNeeded(BaseJobDeclaration declarationBO, Shipment declarationData, bool supportsChcPivotBetweenInvoiceLineAndPacking)
		{
			declarationData.SetPackingLineCollection(() =>
			{
				var packingLineCollection = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
				foreach (var containerBO in declarationBO.CusContainers.OfType<BaseCusContainer>())
				{
					if (containerBO.InvoiceLinePivotCollection.Count > 0)
					{
						var packingLineData = new PackingLine(writeManager.WriterStrategy);
						packingLineData.ContainerNumber = containerBO.CO_ContainerNumber;
						PopulatePackedItemData(null, containerBO.InvoiceLinePivotCollection, packingLineData, supportsChcPivotBetweenInvoiceLineAndPacking);
						packingLineCollection.Add(packingLineData);
					}
				}
				if (packingLineCollection.Count > 0)
				{
					return packingLineCollection;
				}
				else
				{
					return null;
				}
			});
		}

		void PopulatePackageDataFromPackage(BaseJobDeclaration declarationBO, Shipment declarationData, bool supportsChcPivotBetweenInvoiceLineAndPacking)
		{
			var packingGroups = declarationBO.PackingGroups;
			if (packingGroups.Count > 0)
			{
				declarationData.SetPackingLineCollection(() =>
				{
					var packingLineCollection = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };

					foreach (BasePackingGroup packingGroup in packingGroups)
					{
						var packages = packingGroup.Packages.Cast<BasePackage>().Where(x => x.CW_CW_Parent.IsEmpty);
						if (packages.Any())
						{
							foreach (BasePackage packageBO in packages)
							{
								packingLineCollection.Add(ExportOnePackage(packageBO, packingGroup, supportsChcPivotBetweenInvoiceLineAndPacking, declarationBO.SupportsParentPackage));
							}
						}
						else
						{
							var packingLineData = new PackingLine(writeManager.WriterStrategy);
							PopulatePackingLineData(packingGroup, null, packingLineData, supportsChcPivotBetweenInvoiceLineAndPacking);
							packingLineCollection.Add(packingLineData);
						}
					}
					return packingLineCollection;
				});
			}
		}

		PackingLine ExportOnePackage(BasePackage packageBO, BasePackingGroup packingGroup, bool supportsChcPivotBetweenInvoiceLineAndPacking, bool supportsParentPackage)
		{
			var packingLineData = new PackingLine(writeManager.WriterStrategy)
			{
				PackType = ListHelper.GetWithDescription<PackageType>(packageBO.CW_PackType, packageBO.PackTypeList),
				PackQty = new ZLong(packageBO.CW_PackQty),
				InBondPackQty = packageBO.CW_InBondPackQty,
				CustomsOuterPacks = packageBO.CW_OuterPacks,
				MarksAndNos = packageBO.CW_MarksAndNos,
				ShippingSymbol = packageBO.CW_ShippingSymbol,
				Link = helper.GetPackingLineLink(packageBO)
			};

			PopulateUNDG(packageBO, packingLineData, writeManager);
			PopulatePackingLineData(packingGroup, packageBO, packingLineData, supportsChcPivotBetweenInvoiceLineAndPacking);
			PopulatePackageAddInfoGroupCollection(packageBO, packingLineData);
			packingLineData.SetPackingLineCollection(() => ExportChildrenPackages(packageBO, packingGroup, supportsChcPivotBetweenInvoiceLineAndPacking, supportsParentPackage));

			return packingLineData;
		}

		List<PackingLine> ExportChildrenPackages(BasePackage parentPackageBO, BasePackingGroup packingGroup, bool supportsChcPivotBetweenInvoiceLineAndPacking, bool supportsParentPackage)
		{
			List<PackingLine> result = null;
			if (supportsParentPackage)
			{
				result = new List<PackingLine>();
				foreach (BasePackage child in parentPackageBO.Children)
				{
					result.Add(ExportOnePackage(child, packingGroup, supportsChcPivotBetweenInvoiceLineAndPacking, supportsParentPackage));
				}
			}
			return result;
		}

		protected virtual void PopulateUNDG(BasePackage packageBO, PackingLine packingLineData, IDataWritingManager manager)
		{
		}

		void PopulatePackingLineData(BasePackingGroup packingGroup, BasePackage packageBO, PackingLine packingLineData, bool supportsChcPivotBetweenInvoiceLineAndPacking)
		{
			var containerBO = packingGroup.Container;
			if (containerBO != null)
			{
				packingLineData.ContainerNumber = containerBO.CO_ContainerNumber;
			}
			PopulatePackedItemData(supportsChcPivotBetweenInvoiceLineAndPacking ? packageBO?.InvoiceLinePivotCollection : null, containerBO?.InvoiceLinePivotCollection, packingLineData, supportsChcPivotBetweenInvoiceLineAndPacking);

			var bill = packingGroup.Bill;
			if (bill != null)
			{
				packingLineData.BillNumber = bill.CU_BillNum;
				packingLineData.BillType = GetWayBillType(bill.CU_BillType);
			}
			if (packageBO != null)
			{
				packingLineData.Link = helper.GetPackingLineLink(packageBO);
			}
			//TODO: CR_CargoStatus
			//TODO: CR_HouseContainerNumber
		}

		WayBillType GetWayBillType(ZString billType)
		{
			return helper.GetWayBillType(billType);
		}

		void PopulatePackedItemData(BasePackagePivotsCollection<InvoiceLinePackagePivot> invoiceLinePackagePivotCollection, CusContainerInvoiceLinePivotCollection containerInvoiceLinePivotCollection, PackingLine packingLine, bool supportsChcPivotBetweenInvoiceLineAndPacking)
		{
			packingLine.SetPackedItemCollection(() =>
			{
				var list = new List<PackedItem>();
				if (supportsChcPivotBetweenInvoiceLineAndPacking)
				{
					var usedInvoiceLinePks = new List<ZGuid>();
					if (invoiceLinePackagePivotCollection != null && invoiceLinePackagePivotCollection.Count > 0)
					{
						foreach (InvoiceLinePackagePivot invoiceLinePackagePivot in invoiceLinePackagePivotCollection)
						{
							var invoiceLine = invoiceLinePackagePivot.InvoiceLine;
							var packedItem = new PackedItem()
							{
								CommercialInvoiceLineLink = helper.GetCommercialInvoiceLineLink(invoiceLine),
								PackedQuantity = (ZDecimal)invoiceLinePackagePivot.CHC_NumberOfPacks
							};
							list.Add(packedItem);
							var invoiceLinePK = invoiceLinePackagePivot.CHC_JI;
							if (invoiceLinePK.IsValid && !usedInvoiceLinePks.Contains(invoiceLinePK))
							{
								usedInvoiceLinePks.Add(invoiceLinePK);
							}
						}
					}
					if (containerInvoiceLinePivotCollection != null && containerInvoiceLinePivotCollection.Count > 0)
					{
						foreach (var pivot in containerInvoiceLinePivotCollection.OfType<CusContainerInvoiceLinePivot>().Where(x => !usedInvoiceLinePks.Contains(x.C2_JI)))
						{
							var invoiceLine = pivot.InvoiceLine;
							var packedItem = new PackedItem()
							{
								CommercialInvoiceLineLink = helper.GetCommercialInvoiceLineLink(invoiceLine)
							};
							list.Add(packedItem);
						}
					}
				}
				else if (containerInvoiceLinePivotCollection != null && containerInvoiceLinePivotCollection.Count > 0)
				{
					foreach (var pivot in containerInvoiceLinePivotCollection.OfType<CusContainerInvoiceLinePivot>())
					{
						var invoiceLine = pivot.InvoiceLine;
						var packedItem = new PackedItem()
						{
							CommercialInvoiceLineLink = helper.GetCommercialInvoiceLineLink(invoiceLine),
							PackedQuantity = (ZDecimal)pivot.C2_PackQty,
							GrossWeight = pivot.C2_GrossWeight,
							GrossWeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms, Description = Core.Constants.Weight.GetDescription(Core.Constants.Weight.Kilograms, Core.Constants.PluralState.NonPlural) },
							NetWeight = pivot.C2_NetWeight,
							GoodsValue = pivot.C2_SplitValue
						};
						packedItem.NetWeightUnit = packedItem.GrossWeightUnit;
						list.Add(packedItem);
					}
				}
				return list;
			});
		}

		protected virtual void PopulatePackageAddInfoGroupCollection(BasePackage packageBO, PackingLine packingLineData)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "Designer maintains this method.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void PopulateShipmentData(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			declarationData.MessageType = PopulateValue(declarationData.MessageType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_MessageType, declarationBO.Lookups.MessageTypeList));
			if (!declarationBO.JE_ApplicationCode.IsEmpty)
			{
				declarationData.MessagingApplicationCode = ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_ApplicationCode, declarationBO.Lookups.ApplicationCodeList);
			}
			declarationData.MessageSubType = PopulateValue(declarationData.MessageSubType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_MessageSubType, declarationBO.Lookups.MessageSubTypeList));
			declarationData.TransportMode = PopulateValue(declarationData.TransportMode, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_TransportMode, declarationBO.Lookups.TransportTypeList));
			declarationData.CustomsContainerMode = PopulateValue(declarationData.CustomsContainerMode, keepExistingData, () => ListHelper.GetWithDescription<ContainerMode>(declarationBO.JE_ContainerMode, declarationBO.Lookups.CargoIdTypeList));
			declarationData.ServiceLevel = PopulateValue(declarationData.ServiceLevel, keepExistingData, () => ListHelper.GetWithDescription<ServiceLevel>(declarationBO.JE_RS_NKServiceLevel, declarationBO.Lookups.ServiceLevels));
			declarationData.CustomsOffice = PopulateValue(declarationData.CustomsOffice, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair10Char>(declarationBO.JE_CustomsOffice, declarationBO.Lookups.CustomsOfficeList));
			declarationData.IsPersonalEffects = PopulateValue(declarationData.IsPersonalEffects, keepExistingData, () => declarationBO.JE_IsPersonalEffects);
			//TODO - JE_OverrideFreightDefaults
			declarationData.EFTMode = PopulateValue(declarationData.EFTMode, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_EFTMode, declarationBO.Lookups.EFTModeList));
			declarationData.MergeBy = PopulateValue(declarationData.MergeBy, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_MergeBy, declarationBO.Lookups.MergeByList));
			declarationData.OperationalStatus = PopulateValue(declarationData.OperationalStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_OperationalStatus, declarationBO.Lookups.OperationalStatusList));
			declarationData.MessageStatus = PopulateValue(declarationData.MessageStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_MessageStatus, declarationBO.Lookups.MessageStatusList));
			declarationData.EntryStatus = PopulateValue(declarationData.EntryStatus, keepExistingData, () => ListHelper.GetWithDescription<EntryStatus>(declarationBO.JE_EntryStatus, declarationBO.Lookups.EntryStatusList));
			declarationData.ConsolidatedCargoStatus = PopulateValue(declarationData.ConsolidatedCargoStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_ConsolidatedCargoStatus, declarationBO.Lookups.ConsolidatedCargoStatusList));
			PopulateNoOfPacksAndPackType(declarationBO, declarationData, keepExistingData);
			declarationData.TotalNoOfPieces = PopulateValue(declarationData.TotalNoOfPieces, keepExistingData, () => declarationBO.JE_TotalNoOfPieces);
			declarationData.TotalNoOfPiecesLanded = PopulateValue(declarationData.TotalNoOfPiecesLanded, keepExistingData, () => declarationBO.JE_LandedPieces);
			declarationData.WarehouseReleaseStatus = PopulateValue(declarationData.WarehouseReleaseStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.WarehouseTransactionStatus, declarationBO.Lookups.WarehouseTransactionStatusList));

			var refUNLOCOList = declarationBO.Factory.GetRefUNLOCOList();
			declarationData.PortOfOrigin = PopulateValue(declarationData.PortOfOrigin, keepExistingData, () => ListHelper.GetWithName(declarationBO.JE_RL_NKOrigin, refUNLOCOList));
			declarationData.PortOfLoading = PopulateValue(declarationData.PortOfLoading, keepExistingData, () => ListHelper.GetWithName(declarationBO.JE_RL_NKPortOfLoading, refUNLOCOList));
			declarationData.PortOfFirstArrival = PopulateValue(declarationData.PortOfFirstArrival, keepExistingData, () => ListHelper.GetWithName(declarationBO.JE_RL_NKPortOfFirstArrival, refUNLOCOList));
			declarationData.PortOfDischarge = PopulateValue(declarationData.PortOfDischarge, keepExistingData, () => ListHelper.GetWithName(declarationBO.JE_RL_NKPortOfArrival, refUNLOCOList));
			declarationData.PortOfDestination = PopulateValue(declarationData.PortOfDestination, keepExistingData, () => ListHelper.GetWithName(declarationBO.JE_RL_NKFinalDestination, refUNLOCOList));
			declarationData.GoodsDestination = PopulateValue(declarationData.GoodsDestination, keepExistingData, () => declarationBO.JE_GoodsDestination);
			declarationData.VesselName = PopulateValue(declarationData.VesselName, keepExistingData, () => declarationBO.JE_VesselName);
			declarationData.LloydsIMO = PopulateValue(declarationData.LloydsIMO, keepExistingData, () => !declarationBO.JE_LloydsIMO.IsEmpty ? declarationBO.JE_LloydsIMO : declarationBO.Vessel != null ? declarationBO.Vessel.RV_LloydsNumber : ZString.Empty);
			declarationData.VoyageFlightNo = PopulateValue(declarationData.VoyageFlightNo, keepExistingData, () => declarationBO.JE_VoyageFlightNo);
			PopulateLocationAtClearanceForWriter(declarationBO, declarationData, keepExistingData);
			declarationData.SubLocationAtClearance = PopulateValue(declarationData.SubLocationAtClearance, keepExistingData, () => GetSubLocationAtClearance(declarationBO.JE_SubLocationOfGoods, declarationBO.Lookups.SubLocationOfGoodsCollection));
			declarationData.GoodsOrigin = PopulateValue(declarationData.GoodsOrigin, keepExistingData, () =>
			{
				var goodsOrigin = declarationBO.Lookups.GoodsOrigin;
				if (goodsOrigin is IFindBoxListProvider findBoxListProvider)
				{
					return ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_GoodsOrigin, findBoxListProvider);
				}
				else
				{
					return ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_GoodsOrigin, (ICodeDescriptionPairList)goodsOrigin);
				}
			});
			declarationData.GoodsDescription = PopulateValue(declarationData.GoodsDescription, keepExistingData, () => declarationBO.JE_GoodsDescription);
			declarationData.ContainerCount = PopulateValue(declarationData.ContainerCount, keepExistingData, () => declarationBO.JE_ContainerCount);
			declarationData.ExportGoodsType = PopulateValue(declarationData.ExportGoodsType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_ExportGoodsType, declarationBO.Lookups.JE_ExportGoodsType_List));
			declarationData.AgentsReference = PopulateValue(declarationData.AgentsReference, keepExistingData, () => declarationBO.JE_AgentsReference);
			declarationData.OwnerRef = PopulateValue(declarationData.OwnerRef, keepExistingData, () => declarationBO.JE_OwnerRef);
			declarationData.Folio = PopulateValue(declarationData.Folio, keepExistingData, () => declarationBO.JE_Folio);
			declarationData.PaymentMethod = PopulateValue(declarationData.PaymentMethod, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_PaymentMethod, declarationBO.Lookups.PaymentPartyList));
			declarationData.PaidBy = PopulateValue(declarationData.PaidBy, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_PaidBy, declarationBO.Lookups.PaidByList));
			declarationData.DefermentAccountNumber = PopulateValue(declarationData.DefermentAccountNumber, keepExistingData, () => declarationBO.JE_DefermentAccountNumber);
			declarationData.TotalWeight = PopulateValue(declarationData.TotalWeight, keepExistingData, () => declarationBO.JE_TotalWeight);
			declarationData.TotalWeightUnit = PopulateValue(declarationData.TotalWeightUnit, keepExistingData, () => ListHelper.GetWithDescription<UnitOfWeight>(declarationBO.JE_TotalWeightUnit, declarationBO.Lookups.WeightUnitList));
			declarationData.TotalVolume = PopulateValue(declarationData.TotalVolume, keepExistingData, () => declarationBO.JE_TotalVolume);
			declarationData.TotalVolumeUnit = PopulateValue(declarationData.TotalVolumeUnit, keepExistingData, () => ListHelper.GetWithDescription<UnitOfVolume>(declarationBO.JE_TotalVolumeUnit, declarationBO.Lookups.VolumeUnitList));
			declarationData.ShipmentIncoTerm = PopulateValue(declarationData.ShipmentIncoTerm, keepExistingData, () => ListHelper.GetWithDescription<UniversalXml.IncoTerm>(declarationBO.JE_ShipmentIncoTerm, declarationBO.Lookups.IncoTermList));
			declarationData.AdditionalTerms = PopulateValue(declarationData.AdditionalTerms, keepExistingData, () => declarationBO.JE_ShipmentIncoTermPlace);
			declarationData.TransportNationality = PopulateValue(declarationData.TransportNationality, keepExistingData, () => ListHelper.GetWithName<Country>(declarationBO.JE_RN_NKTransportNationality, declarationBO.Lookups.TransportNationalities));
			declarationData.DeclarantType = PopulateValue(declarationData.DeclarantType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_DeclarantType, declarationBO.Lookups.DeclarantTypeList));
			//TODO - JE_LandedCostByWeight
			//TODO - JE_LandedCostByVolume
			//TODO - JE_LandedCostByUnits
			//TODO - JE_LandedCostByCost
			declarationData.CustomsBroker = PopulateValue(declarationData.CustomsBroker, keepExistingData, () => Staff.New(declarationBO.CusAgent));
			declarationData.Branch = PopulateValue(declarationData.Branch, keepExistingData, () => Branch.New(helper.Load<GlbBranch>(declarationBO.JE_GB)));
			declarationData.TotalNoOfPacksDecimal = PopulateValue(declarationData.TotalNoOfPacksDecimal, keepExistingData, () => declarationBO.JE_TotalNoOfPacksDecimal);
			//TODO - JE_AutoWeightApportion
			declarationData.ScreeningStatus = PopulateValue(declarationData.ScreeningStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_ScreeningStatus, declarationBO.Lookups.ScreeningStatusesList));
			declarationData.CustomsValuationPort = PopulateValue(declarationData.CustomsValuationPort, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(declarationBO.JE_IATALoadPort, declarationBO.Lookups.IATALoadPorts));
			PopulateAdditionalBillData(declarationBO, declarationData, keepExistingData);
			declarationData.UniqueConsignmentReference = PopulateValue(declarationData.UniqueConsignmentReference, keepExistingData, () => declarationBO.JE_UCR);
		}

		protected virtual void PopulateLocationAtClearanceForWriter(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			declarationData.LocationAtClearance = PopulateValue(declarationData.LocationAtClearance, keepExistingData, () => GetLocationAtClearance(GetLocationOfGoods(declarationBO), declarationBO.Lookups.LocationOfGoodsCollection));
		}

		protected virtual ZString GetLocationOfGoods(BaseJobDeclaration declarationBO) => declarationBO.JE_LocationOfGoods;

		protected virtual CodeDescriptionPair35Char GetLocationAtClearance(ZString locationOfGoods, IBusinessObjectCollection locationOfGoodsCollection)
		{
			var collection = locationOfGoodsCollection as IFindBoxListProvider;
			var result = new CodeDescriptionPair35Char()
			{
				Code = locationOfGoods.Left(35),
				Description = locationOfGoods.Length > 35 ? locationOfGoods : (ZString?)collection?.DescriptionFromCode(locationOfGoods)
			};
			return result;
		}

		protected virtual CodeDescriptionPair35Char GetSubLocationAtClearance(ZString subLocationOfGoods, IBusinessObjectCollection subLocationOfGoodsCollection)
		{
			var collection = subLocationOfGoodsCollection as IFindBoxListProvider;
			var result = new CodeDescriptionPair35Char
			{
				Code = subLocationOfGoods.Left(35),
				Description = subLocationOfGoods.Length > 35 ? subLocationOfGoods : (ZString?)collection?.DescriptionFromCode(subLocationOfGoods)
			};
			return result;
		}

		protected virtual void PopulateNoOfPacksAndPackType(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			declarationData.OuterPacks = PopulateValue(declarationData.OuterPacks, keepExistingData, () => declarationBO.JE_TotalNoOfPacks);
			declarationData.OuterPacksPackageType = PopulateValue(declarationData.OuterPacksPackageType, keepExistingData, () => ListHelper.GetWithDescription<PackageType>(declarationBO.JE_TotalNoOfPacksPackType, declarationBO.Lookups.JE_TotalNoOfPacksPackType_List));
		}

		void PopulateAdditionalBillData(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			var wayBillType = ZString.Empty;
			var wayBillNumber = declarationBO.JE_HouseBill;
			if (wayBillNumber.IsEmpty)
			{
				wayBillNumber = declarationBO.JE_MasterBill;
				if (!wayBillNumber.IsEmpty)
				{
					wayBillType = BillTypeList.Codes.MasterBill;
				}
			}
			else
			{
				wayBillType = BillTypeList.Codes.HouseBill;
			}
			var declarationPK = declarationBO.PK;
			var declarationClusterKey = declarationBO.JE_ClusterKey;

			declarationData.SetAdditionalBillCollection(() =>
			{
				var query = new ZQuery(CusDecHouseBillSchema.CU_ClusterKey, declarationClusterKey);
				query.AddToFilter(CusDecHouseBillSchema.CU_JE, declarationPK);
				var billBOs = helper.Load<Bill>(query);
				if (billBOs.Length > 0)
				{
					var additionalBills = new List<AdditionalBill>(billBOs.Length);
					Bill mainBill = null;
					foreach (var billBO in billBOs)
					{
						var billNumber = billBO.CU_BillNum;
						var billType = billBO.CU_BillType;
						if (mainBill == null && !wayBillNumber.IsEmpty && wayBillNumber == billNumber && billType == wayBillType)
						{
							mainBill = billBO;
						}
						var additionalBill = CreateAdditionalBill(billBO, helper);
						PopulateAdditionalInfoForAdditionalBill(billBO, additionalBill);
						additionalBills.Add(additionalBill);
					}

					if (mainBill != null && declarationBO.IsBillIssueDateVisible)
					{
						var billIssuedDate = Date.New(DateType.BillIssued, ZBool.False, mainBill.CU_IssueDate);

						var existingBillIssuedDate = declarationData.DateCollection.GetMatchedByCandidateKey(billIssuedDate);
						if (existingBillIssuedDate != null && !keepExistingData)
						{
							declarationData.DateCollection.Remove(existingBillIssuedDate);
						}

						if (existingBillIssuedDate == null || !keepExistingData)
						{
							declarationData.DateCollection.Add(billIssuedDate);
						}
					}

					if (additionalBills.Count > 0)
					{
						return additionalBills;
					}
				}
				return null;
			});

			if (!keepExistingData && !wayBillNumber.IsEmpty)
			{
				PopulateWayBillDetail(declarationBO, declarationData, wayBillType, wayBillNumber);
			}
		}

		protected virtual void PopulateAdditionalInfoForAdditionalBill(Bill billBO, AdditionalBill additionalBill)
		{
		}

		protected virtual void PopulateWayBillDetail(BaseJobDeclaration declarationBO, Shipment declarationData, ZString wayBillType, ZString wayBillNumber)
		{
			declarationData.WayBillNumber = wayBillNumber;
			declarationData.WayBillType = GetWayBillType(wayBillType);
			if (wayBillType == BillTypeList.Codes.HouseBill)
			{
				declarationData.SetAddInfoCollection(() => declarationData.AddInfoCollection.AddSafe(AddInfo.New(Constants.AddInfoKeys.Declaration.MasterWayBillNumber, declarationBO.JE_MasterBill)));
			}
		}

		AdditionalBill CreateAdditionalBill(Bill billBO, UniversalDataObjectWriterHelper billLookups)
		{
			var additionalBill = new AdditionalBill(writeManager.WriterStrategy)
			{
				BillNumber = billBO.CU_BillNum,
				BillType = GetWayBillType(billBO.CU_BillType),
				IssueDate = billBO.CU_IssueDate,
				NoOfPacks = billBO.CU_NoOfPacks,
				PackType = ListHelper.GetWithDescription<PackageType>(billBO.CU_PackType, billBO.Lookups.NoOfPacksPackType_List),
				MessageStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(billBO.CU_Status, billBO.Lookups.MessageStatusList),
			};
			additionalBill.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(billBO.CU_AddInfo));
			additionalBill.SetAddInfoGroupCollection(() => AddInfoGroupCollectionCreator.CreateCollection(billLookups, billBO, writeManager));
			additionalBill.SetCustomsReferenceCollection(() => CustomsReferenceCollectionCreator.CreateCollection(billLookups, billBO, writeManager));

			AddAdditionalParentBillDetails(billBO, additionalBill);
			return additionalBill;
		}

		protected virtual void AddAdditionalParentBillDetails(Bill billBO, AdditionalBill additionalBill)
		{
			ZString? parentBillNumber = null;
			ZString? parentMasterBillNumber = null;
			var parentBill = billBO.ParentBill;
			if (parentBill != null)
			{
				parentBillNumber = parentBill.CU_BillNum;
				var parentParentBill = parentBill.ParentBill;
				if (parentParentBill != null)
				{
					parentMasterBillNumber = parentParentBill.CU_BillNum;
				}
			}
			additionalBill.ParentBillNumber = parentBillNumber;
			if (parentMasterBillNumber.HasValue)
			{
				additionalBill.SetAddInfoCollection(() => additionalBill.AddInfoCollection.AddSafe(new AddInfo() { Key = Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber, Value = parentMasterBillNumber }));
			}
		}

		protected sealed override IEnumerable<IPropertyValue> GetUserDefinedValues(BaseJobDeclaration sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}

		protected sealed override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected sealed override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CustomsDeclaration;
		}

		#region IContainerParentDataObjectWriter

		void IContainerParentDataObjectWriter.PopulateContainer(BusinessObject sourceBO, ICommonContainer containerBO, ITopLevelDataObject dataObject)
		{
			var container = containerBO as CommonContainer;
			if (container != null)
			{
				ContainerOverride = sourceBO.Factory.LoadTop1<BaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, container.PK));
			}

			var declarationBO = sourceBO as BaseJobDeclaration;
			var shipmentData = dataObject as Shipment;
			if (declarationBO != null && shipmentData != null)
			{
				AddTableFetchHintCreators(declarationBO.Factory, declarationBO);
				PopulateContainerData(declarationBO, shipmentData, false);
			}
		}

		ITopLevelDataObject IContainerParentDataObjectWriter.GetContainerParentDataObject(BusinessObject sourceBO)
		{
			return GetDataObject(sourceBO as BaseJobDeclaration);
		}

		bool IsTopLevelContextCustomsDeclaration(BaseJobDeclaration declaration)
		{
			return !declaration.IsPluggedIntoShipment
				|| declaration.IsPluggedIntoShipment && writeManager.ContentFilterManager?.EDIMessageContentFilter?.GetUniversalShipmentPrimaryDataSource() == EDIMessageContentPrimaryDataSource.Codes.Brokerage
				|| writeManager.FilteredDataContextType.HasValue && writeManager.FilteredDataContextType == DataContextType.CustomsDeclaration;
		}

		#endregion
	}
}
