using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class BaseJobDeclarationFetchStrategy : EnterpriseBusinessObjectFetchStrategy, IDocumentSupporterFetchStrategy
	{
		public BaseJobDeclarationFetchStrategy(BaseJobDeclaration declaration)
			: base(declaration)
		{
		}

		#region FetchForMerge

		public void FetchForMerge()
		{
			AddFetchAndMarkInFetchDetail(FetchForMergeCore, (x) => x.HasRunFetchForMerge, (x) =>
				{
					x.HasRunFetchForMerge = true;
				});
		}

		protected virtual void FetchForMergeCore()
		{
			if (!declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction)
			{
				foreach (CusEntryInstruction instruction in declaration.CustomsEntryInstructions)
				{
					AddMergeFetchHintsFor(instruction);
				}
			}

			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				AddMergeFetchHintsFor(entry);
			}

			foreach (BaseJobComInvoiceGroupHeader groupHeader in declaration.AllGroupHeaders)
			{
				AddMergeFetchHintsFor(groupHeader);
			}

			foreach (BaseJobComInvoiceHeader invoice in declaration.Invoices)
			{
				AddMergeFetchHintsFor(invoice);
			}

			foreach (BaseJobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				AddMergeFetchHintsFor(invoiceLine);
			}
			AddMergeFetchHintsAfterInvoiceLines();

			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					AddMergeFetchHintsFor(entryLine);
				}
			}
		}

		protected virtual void AddMergeFetchHintsAfterInvoiceLines()
		{
		}

		protected virtual void AddMergeFetchHintsFor(BaseJobComInvoiceGroupHeader groupHeader)
		{
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, groupHeader.PK);
		}

		protected virtual void AddMergeFetchHintsFor(BaseJobComInvoiceHeader invoice)
		{
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoice.PK);
		}

		protected virtual void AddMergeFetchHintsFor(CusEntryHeader entry)
		{
			Factory.AddFetchHint(CusEntryLineSchema.CL_CH, entry.PK);
			Factory.AddFetchHint(CusEntryHeaderChargesSchema.C1_CH, entry.PK);
		}

		protected virtual void AddMergeFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
		{
			Factory.AddFetchHint(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_ParentID, invoiceLine.PK);
			Factory.AddFetchHint(CusClassificationSchema.PK, invoiceLine.JI_CC);
			Factory.AddFetchHint(CusUnderbondDecSchema.BU_JI, invoiceLine.PK);
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoiceLine.PK);
		}

		protected virtual void AddMergeFetchHintsFor(CusEntryLine entryLine)
		{
			Factory.AddFetchHint(CusEntryLineFeeSchema.CF_CL, entryLine.PK);
			Factory.AddFetchHint(CusUnderbondDecSchema.BU_CL, entryLine.PK);
		}

		protected virtual void AddMergeFetchHintsFor(CusEntryInstruction instruction)
		{
		}

		#endregion

		#region Fetch For RefreshExRate

		public void FetchForRefreshExRate()
		{
			AddFetchAndMarkInFetchDetail(FetchForRefreshExRateCore, (x) => x.HasRunFetchForRefreshExRate, (x) =>
			{
				x.HasRunFetchForRefreshExRate = true;
			});
		}

		protected virtual void FetchForRefreshExRateCore()
		{
			foreach (BaseJobComInvoiceGroupHeader groupInvoice in declaration.AllGroupHeaders)
			{
				AddRefreshExRateFetchHintsFor(groupInvoice);
			}
			foreach (BaseJobComInvoiceHeader invoice in declaration.Invoices)
			{
				AddRefreshExRateFetchHintsFor(invoice);
			}
			foreach (BaseJobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				AddRefreshExRateFetchHintsFor(invoiceLine);
			}
		}

		protected virtual void AddRefreshExRateFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
		{
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoiceLine.PK);
		}

		protected virtual void AddRefreshExRateFetchHintsFor(BaseJobComInvoiceHeader invoice)
		{
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoice.PK);
		}

		protected virtual void AddRefreshExRateFetchHintsFor(BaseJobComInvoiceGroupHeader groupInvoice)
		{
			Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, groupInvoice.PK);
		}

		#endregion

		#region Document Supporter Fetch Hints

		protected ChildTypeSupported FetchHintDecider => fetchHintDecider ?? (fetchHintDecider = new ChildTypeSupported(declaration));
		ChildTypeSupported fetchHintDecider;

		protected bool SupportsAdditionalInvoiceLineEntryLineLink => AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(declaration.CountryCode);

		void IDocumentSupporterFetchStrategy.AddDocumentSupporterFetchHints()
		{
			AddDocumentSupporterFetchHintsForJobDeclaration();

			var externalFetchHintSupporter = (IExternalFetchHintSupporter)declaration.Factory;

			externalFetchHintSupporter.AddFetchHint(new FetchHint(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK));

			externalFetchHintSupporter.AddTableFetchHintCreator(CusContainerSchema.Instance, GetCusContainerRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusClassPartPivotSchema.Instance, GetCusClassPartPivotRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusDecHouseBillSchema.Instance, GetCusDecHouseBillRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusDecHouseContainerPivotSchema.Instance, GetCusDecHouseContainerPivotRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusEntryHeaderSchema.Instance, GetCusEntryHeaderRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusEntryLineSchema.Instance, GetCusEntryLineRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusEntryInstructionSchema.Instance, GetEntryInstructionRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(JobComInvoiceHeaderSchema.Instance, GetJobComInvoiceHeaderRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(JobComInvoiceLineSchema.Instance, GetJobComInvoiceLineRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(JobContainerSchema.Instance, GetJobContainerRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(LandedCostHeaderSchema.Instance, GetLandedCostHeaderRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(LandedCostHistorySchema.Instance, GetLandedCostHistoryRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(OrgAddressSchema.Instance, GetOrgAddressRelatedFetchHints, true);
			externalFetchHintSupporter.AddTableFetchHintCreator(OrgSupplierPartSchema.Instance, GetOrgSupplierPartRelatedFetchHints, true);

			if (SupportsAdditionalInvoiceLineEntryLineLink)
			{
				externalFetchHintSupporter.AddTableFetchHintCreator(CusUnderbondDecSchema.Instance, GetCusUnderbondDecRelatedFetchHints);
			}
		}

		protected virtual void AddDocumentSupporterFetchHintsForJobDeclaration()
		{
			var declarationPK = declaration.PK;
			var declarationClusterKey = declaration.JE_ClusterKey;
			var externalFetchHintSupporter = (IExternalFetchHintSupporter)declaration.Factory;
			var declarationRow = (IColumnIndexer)((IBusinessObjectInternals)declaration).Row;

			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusContainerSchema.CO_JE, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusEntryHeaderSchema.CH_JE, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(JobDocAddressSchema.E2_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(JobHeaderSchema.JH_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(StmALogSchema.SL_Parent, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(StmNoteSchema.ST_ParentID, declarationPK));

			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusContainerInvoiceLinePivotSchema.C2_ClusterKey, declarationClusterKey));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusHouseContPackInvoiceLinePivotSchema.CHC_ClusterKey, declarationClusterKey));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(JobComInvoiceLineTaxSchema.JLT_ClusterKey, declarationClusterKey));

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

			AddFetchHints(externalFetchHintSupporter, declarationRow, JobDeclarationSchema.JE_OA_ManufacturerAddress, OrgAddressSchema.PK, OrgCusCodeSchema.OK_OA_PremisesAddress);
			AddFetchHints(externalFetchHintSupporter, declarationRow, JobDeclarationSchema.JE_OA_SoldToPartyAddress, OrgAddressSchema.PK, OrgCusCodeSchema.OK_OA_PremisesAddress);
			AddFetchHints(externalFetchHintSupporter, declarationRow, JobDeclarationSchema.JE_OH_BuyingAgent, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OA_PremisesAddress);
			AddFetchHints(externalFetchHintSupporter, declarationRow, JobDeclarationSchema.JE_OH_Forwarder, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH);
			AddFetchHints(externalFetchHintSupporter, declarationRow, JobDeclarationSchema.JE_OH_Importer, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH);
			AddFetchHints(externalFetchHintSupporter, declarationRow, JobDeclarationSchema.JE_OH_SellingAgent, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OA_PremisesAddress);
			AddFetchHints(externalFetchHintSupporter, declarationRow, JobDeclarationSchema.JE_OH_ShippingLine, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH);
			AddFetchHints(externalFetchHintSupporter, declarationRow, JobDeclarationSchema.JE_OH_Supplier, OrgHeaderSchema.PK, OrgAddressSchema.OA_OH, OrgCusCodeSchema.OK_OH);
		}

		IEnumerable<IFetchHint> GetCusContainerRelatedFetchHints(IColumnIndexer cusContainerRow)
		{
			var containerPK = cusContainerRow.GetValue(CusContainerSchema.PK);
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
			yield return new FetchHint(StmALogSchema.SL_Parent, entryHeaderPK);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, entryHeaderPK);
		}

		protected virtual IEnumerable<IFetchHint> GetCusEntryLineRelatedFetchHints(IColumnIndexer row)
		{
			var entryLinePK = row.GetValue(CusEntryLineSchema.PK);
			yield return new FetchHint(CusEntryLineFeeSchema.CF_CL, entryLinePK);
			if (FetchHintDecider != null)
			{
				if (FetchHintDecider.EntryLineUseCusAddInfo)
				{
					yield return new FetchHint(CusAddInfoSchema.B7_ParentID, entryLinePK);
				}
				if (FetchHintDecider.EntryLineUseCusCodeData)
				{
					yield return new FetchHint(CusCodeDataSchema.CY_ParentID, entryLinePK);
				}
			}
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
			yield return new FetchHint(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_ClusterKey, declaration.JE_ClusterKey);
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
			if (SupportsAdditionalInvoiceLineEntryLineLink)
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

		protected virtual IEnumerable<IFetchHint> GetJobContainerRelatedFetchHints(IColumnIndexer row)
		{
			var containerPK = row.GetValue(JobContainerSchema.PK);
			yield return new FetchHint(JobServiceSchema.ES_ParentID, containerPK);
			yield return new FetchHint(JobContainerPackPivotSchema.J6_JC, containerPK);
			yield return new FetchHint(CusEntryNumSchema.CE_ParentID, containerPK);
		}

		protected virtual IEnumerable<IFetchHint> GetLandedCostHeaderRelatedFetchHints(IColumnIndexer row)
		{
			var costHeaderPK = row.GetValue(LandedCostHeaderSchema.PK);
			yield return new FetchHint(LandCostInputSchema.LI_LT, costHeaderPK);
			yield return new FetchHint(LandedCostHistorySchema.LH_LT, costHeaderPK);
		}

		protected virtual IEnumerable<IFetchHint> GetLandedCostHistoryRelatedFetchHints(IColumnIndexer row)
		{
			var costHistoryPK = row.GetValue(LandedCostHistorySchema.PK);
			yield return new FetchHint(LandedLineCostItemSchema.LZ_LH, costHistoryPK);
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

		protected virtual IEnumerable<IFetchHint> GetCusUnderbondDecRelatedFetchHints(IColumnIndexer row)
		{
			var fetchHint = GetFetchHintIfNotEmpty(CusEntryLineSchema.PK, row, CusUnderbondDecSchema.BU_CL);
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

		protected FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, IColumnIndexer row, SchemaColumn rowColumn)
		{
			return GetFetchHintIfNotEmpty(column, row.GetValue(rowColumn));
		}

		FetchHint GetFetchHintIfNotEmpty(SchemaColumn column, IZType value)
		{
			return !value.IsEmpty ? new FetchHint(column, value) : null;
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

		public class ChildTypeSupported
		{
			public ChildTypeSupported(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
				invoiceType = ((IBusinessObjectCollection)declaration.Invoices).TypeOfElements;
				billType = declaration.Bills.TypeOfElements;
				invoiceLineType = declaration.InvoiceLines.TypeOfElements;
				entryLineType = new CusEntryLineTypeDecider().GetTypeForCountryCode(declaration.CountryCode);
			}

			readonly Type billType;
			readonly Type invoiceType;
			readonly Type invoiceLineType;
			readonly Type entryLineType;
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

			public bool EntryLineUseCusAddInfo
			{
				get
				{
					if (!entryLineUseCusAddInfo.HasValue)
					{
						entryLineUseCusAddInfo = typeof(ICusAddInfoTypeSupporter).IsAssignableFrom(entryLineType);
					}
					return entryLineUseCusAddInfo.Value;
				}
			}
			bool? entryLineUseCusAddInfo;

			public bool EntryLineUseCusCodeData
			{
				get
				{
					if (!entryLineUseCusCodeData.HasValue)
					{
						entryLineUseCusCodeData = typeof(ICusCodeDataTypeSupporter).IsAssignableFrom(entryLineType);
					}
					return entryLineUseCusCodeData.Value;
				}
			}
			bool? entryLineUseCusCodeData;
		}

		#endregion

		#region Implementation

		BaseJobDeclaration declaration
		{
			get { return BusinessObject; }
		}

		RowFactory RowFactory
		{
			get { return ((IBusinessObjectFactoryInternals)Factory).RowFactory; }
		}

		Dictionary<BusinessObject, FetchDetail> FetchDetailDictionary
		{
			get
			{
				return Factory.GetCachedValue("BaseJobDeclarationFetchStrategy_FetchDetail", delegate
				{
					return new Dictionary<BusinessObject, FetchDetail>();
				});
			}
		}

		protected void AddFetchAndMarkInFetchDetail(Action addFetch, Func<FetchDetail, bool> hasRunFetch, Action<FetchDetail> markHasRunFetch)
		{
			var fetchDetail = GetFetchDetail();
			var rowFactory = RowFactory;
			if (fetchDetail == null || !hasRunFetch(fetchDetail) ||
				rowFactory != null && fetchDetail.QueryCacheClearCount < rowFactory.QueryCacheClearCount)
			{
				addFetch();
				if (fetchDetail == null)
				{
					fetchDetail = new FetchDetail();
					FetchDetailDictionary.Add(BusinessObject, fetchDetail);
				}
				markHasRunFetch(fetchDetail);

				if (rowFactory != null)
				{
					fetchDetail.QueryCacheClearCount = rowFactory.QueryCacheClearCount;
				}
			}
		}

		FetchDetail GetFetchDetail()
		{
			FetchDetail details;
			return FetchDetailDictionary.TryGetValue(BusinessObject, out details) ? details : null;
		}

		protected class FetchDetail
		{
			public bool HasRunFetchForMerge;
			public bool HasRunFetchForRefreshExRate;
			public int QueryCacheClearCount;
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(JobComInvoiceHeaderRefsSchema.J2_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(JobComInvoiceLineSchema.JI_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(CusEquipmentSchema.CEQ_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(CusDecHouseContainerPackSchema.CW_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(CusDecHouseBillSchema.CU_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(CusContainerSchema.CO_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(JobComInvLineComponentInventorySchema.JIV_ClusterKey, BusinessObject.JE_ClusterKey);

			if (BusinessObject.SupportDeclarationRefs)
			{
				Factory.AddFetchHint(JobDecRefsSchema.J3_ClusterKey, BusinessObject.JE_ClusterKey);
			}
			if (BusinessObject.SupportInvoiceLineRefs)
			{
				Factory.AddFetchHint(JobComInvLineRefsSchema.JG_ClusterKey, BusinessObject.JE_ClusterKey);
			}
			if (BusinessObject.SupportsJobComInvoiceLineTax)
			{
				Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_ClusterKey, BusinessObject.JE_ClusterKey);
			}
			if (BusinessObject.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
			{
				Factory.AddFetchHint(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_ClusterKey, BusinessObject.JE_ClusterKey);
			}
			if (BusinessObject.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				Factory.AddFetchHint(CusHouseContPackInvoiceLinePivotSchema.CHC_ClusterKey, BusinessObject.JE_ClusterKey);
			}
			if (AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(BusinessObject.CountryCode))
			{
				Factory.AddFetchHint(CusUnderbondDecSchema.BU_ClusterKey, BusinessObject.JE_ClusterKey);
			}
			Factory.AddFetchHint(CusContainerInvoiceLinePivotSchema.C2_ClusterKey, BusinessObject.JE_ClusterKey);

			Factory.AddFetchHint(CusContainerInvoiceLinePivotSchema.C2_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(CusEntryHeaderSchema.CH_ClusterKey, BusinessObject.JE_ClusterKey);
			Factory.AddFetchHint(typeof(GlbBranch), BusinessObject.PK);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, BusinessObject.PK);
			Factory.AddFetchHint(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobOrderHeaderSchema.JD_JE, declaration.PK);
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);

			if (!declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction || declaration.SupportMultipleWarehouseEntry)
			{
				Factory.AddFetchHint(CusEntryInstructionSchema.CEI_ClusterKey, BusinessObject.JE_ClusterKey);
			}

			AddCusAddInfoAndCusCodeDataFetchHintForCollection(declaration.Invoices);
			AddCusAddInfoAndCusCodeDataFetchHintForCollection(declaration.InvoiceLines);

			BusinessObject.HasFetchForLoadChildEditableObjectsBeenCalled = true;
		}

		protected void AddCusAddInfoAndCusCodeDataFetchHintForCollection(IBusinessObjectCollection collection)
		{
			var elementType = collection.TypeOfElements;
			var supportCusAddInfo = typeof(ICusAddInfoTypeSupporter).IsAssignableFrom(elementType);
			var supportCusCodeData = typeof(ICusCodeDataTypeSupporter).IsAssignableFrom(elementType);
			if (supportCusAddInfo || supportCusCodeData)
			{
				foreach (IBusiness bizObj in collection)
				{
					if (supportCusAddInfo)
					{
						Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, bizObj.Identifier);
					}
					if (supportCusCodeData)
					{
						Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, bizObj.Identifier);
					}
				}
			}
		}

		internal bool SupportCusAddInfo<T>(IBusinessObjectCollection collection)
		{
			return typeof(T).IsAssignableFrom(collection.TypeOfElements);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(typeof(OrgHeader), declaration.JE_OH_Importer);
			Factory.AddFetchHint(typeof(OrgHeader), declaration.JE_OH_Supplier);
			var invoices = declaration.Invoices; // cause Invoices to be part of Children for fetch validation to work
			var invoiceLines = declaration.InvoiceLines; // cause InvoiceLines to be part of Children for fetch validation to work

			foreach (Bill bill in declaration.Bills)
			{
				Factory.AddFetchHint(CusDecHouseBillSchema.PK, bill.CU_CU_ParentBill);
			}

			if (declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				foreach (var package in declaration.Packages)
				{
					Factory.AddFetchHint(typeof(InvoiceLinePackagePivot), CusHouseContPackInvoiceLinePivotSchema.CHC_CW, package.PK);
				}
				foreach (var invoiceLine in invoiceLines)
				{
					Factory.AddFetchHint(typeof(InvoiceLinePackagePivot), CusHouseContPackInvoiceLinePivotSchema.CHC_JI, invoiceLine.PK);
				}
			}

			if (declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
			{
				Factory.AddFetchHint(typeof(InvoiceHeaderPackagePivot), CusHouseContPackInvoiceHeaderPivotSchema.CHZ_ClusterKey, BusinessObject.JE_ClusterKey);
			}

			if (!declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction)
			{
				foreach (CusEntryInstruction instruction in declaration.CustomsEntryInstructions)
				{
					AddFetchForValidateForEntryInstruction(instruction);
				}
			}

			if (declaration.IsWHSUniversalXMLActive && declaration.IsBondedWhsQuantityRequiredForBondedWarehouse)
			{
				foreach (BaseJobComInvoiceLine invoiceLine in invoiceLines)
				{
					Factory.AddFetchHint(typeof(CusEntryLine), CusEntryLineSchema.PK, invoiceLine.JI_CL);
				}
			}
		}

		protected virtual void AddFetchForValidateForEntryInstruction(CusEntryInstruction instruction)
		{
			var query = Universal.RefCusProcedure.Loader.GetFilter(instruction.CountryCode, ZDateTime.Today);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, string.Empty);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, declaration.JE_MessageType);
			Factory.AddFetchHint(RefCusProcedureSchema.Instance, query);
		}

		protected sealed override void FetchForViewCore(TableColumn[] columns)
		{
			FetchForViewDeclaration(columns);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected virtual void FetchForViewDeclaration(TableColumn[] columns)
		{
			var cusEntryNumRequiredFetchForView = false;
			var cusEntryHeaderRequiredFetchForView = false;
			var cusEntryInstructionRequiredFetchForView = false;
			var docsAndCartageRequiredFetchForView = false;
			var processTasksRequiredFetchForView = false;
			var shipmentRequiredFetchForView = false;
			var orderNumbersRequiredFetchForView = false;
			var orgRequiredFetchForView = false;
			var stmALogRequiredFetchForView = false;
			var cusDecHouseBillRequiredFetchForView = false;
			var jobComInvoiceHeaderRequiredFetchForView = false;
			var stmNoteRequiredFetchForView = false;
			var jobConsolTransportRequiredFetchForView = false;
			var jobConShipLinkRequiredFetchForView = false;
			var reverseShipmentLinkRequiredFetchForView = false;
			var transportBookingRequiredFetchForView = false;
			var declarantRequiredFetchForView = false;

			foreach (var tableColumn in columns)
			{
				switch (tableColumn.ColumnName)
				{
					case BaseJobDeclaration.Schema.TotalOutstandingAmount:
					case BaseJobDeclaration.Schema.TotalInvoicedAmount:
					case BaseJobDeclaration.Schema.TotalBilledAmount:
						AddFetchHintForJob();
						shipmentRequiredFetchForView = BusinessObject.JE_JS.IsValid;
						var job = BusinessObject.Job;
						if (job != null)
						{
							var query = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, job.PK);
							query.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
							Factory.AddFetchHint(typeof(JobHeader), query);
						}
						break;
					case nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_Status):
					case nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_HoldReason):
					case nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_ProfitLossReasonCode):
					case nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_TotalProfitRevenueMargin):
					case "BillingBranch":
					case "BillingDepartment":
					case "BillingOperator":
					case "LocalClientCode":
					case "LocalClientName":
					case "LocalClientAddressAsString":
					case "LocalClientAddressShortCode":
					case "LocalClientAddress1":
					case "LocalClientAddress2":
					case "LocalClientCity":
					case "LocalClientState":
					case "LocalClientCountry":
						AddFetchHintForJob();
						shipmentRequiredFetchForView = BusinessObject.JE_JS.IsValid;
						break;
					default:
						if (IsJobComInvoiceHeaderBillRelatedColumn(tableColumn.ColumnName))
						{
							jobComInvoiceHeaderRequiredFetchForView = true;
						}

						if (IsCusDecHouseBillRelatedColumn(tableColumn.ColumnName))
						{
							cusDecHouseBillRequiredFetchForView = true;
						}

						if (!cusEntryHeaderRequiredFetchForView && IsCusEntryHeaderRelatedColumn(tableColumn.ColumnName))
						{
							cusEntryHeaderRequiredFetchForView = true;
						}

						if (IsCusEntryInstructionRelatedColumn(tableColumn.ColumnName))
						{
							cusEntryInstructionRequiredFetchForView = true;
						}

						if (IsCusEntryNumRelatedColumn(tableColumn.ColumnName))
						{
							cusEntryNumRequiredFetchForView = true;
						}

						if (IsProcessTasksRelatedColumn(tableColumn.ColumnName))
						{
							processTasksRequiredFetchForView = true;
						}

						if (declaration.JE_JS.IsValid && !shipmentRequiredFetchForView && IsJobShipmentRelatedColumn(tableColumn.ColumnName))
						{
							shipmentRequiredFetchForView = true;
						}

						if (IsJobOrderHeaderRelatedColumn(tableColumn.ColumnName))
						{
							orderNumbersRequiredFetchForView = true;
						}

						if (IsJobDocsAndCartageRelatedColumn(tableColumn.ColumnName))
						{
							docsAndCartageRequiredFetchForView = true;
						}

						if (!orgRequiredFetchForView && IsOrgRequiredRelatedColumn(tableColumn.ColumnName))
						{
							orgRequiredFetchForView = true;
						}

						if (IsStmALogRelatedColumn(tableColumn.ColumnName))
						{
							stmALogRequiredFetchForView = true;
						}

						if (IsStmNoteRelatedColumn(tableColumn.ColumnName))
						{
							stmNoteRequiredFetchForView = true;
						}

						if (IsJobConsolTransportRelatedColumn(tableColumn.ColumnName))
						{
							jobConsolTransportRequiredFetchForView = true;
						}

						if (tableColumn.ColumnName == BaseJobDeclaration.Schema.JE_MarksAndNumbers || tableColumn.ColumnName == BaseJobDeclaration.Schema.JE_MarksAndNumbersShort)
						{
							// this is potentially expensive, so use it cautiously
							reverseShipmentLinkRequiredFetchForView = true;
						}

						if (tableColumn.ColumnName == BaseJobDeclaration.Schema.JE_ETAOfDischarge || tableColumn.ColumnName == BaseJobDeclaration.Schema.JE_ETDOfLoading)
						{
							jobConShipLinkRequiredFetchForView = true;
						}

						if (tableColumn.ColumnName == BaseJobDeclaration.Schema.RelatedTransportBookingsJobNumbers)
						{
							transportBookingRequiredFetchForView = true;
						}

						if (tableColumn.ColumnName == BaseJobDeclaration.Schema.DeclarantCode || tableColumn.ColumnName == BaseJobDeclaration.Schema.DeclarantName)
						{
							declarantRequiredFetchForView = true;
						}

						break;
				}
			}

			if (jobComInvoiceHeaderRequiredFetchForView)
			{
				Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_ClusterKey, declaration.JE_ClusterKey);
			}

			if (cusDecHouseBillRequiredFetchForView)
			{
				Factory.AddFetchHint(CusDecHouseBillSchema.CU_ClusterKey, declaration.JE_ClusterKey);
			}

			if (cusEntryHeaderRequiredFetchForView)
			{
				Factory.AddFetchHint(CusEntryHeaderSchema.CH_ClusterKey, declaration.JE_ClusterKey);
			}

			if (cusEntryInstructionRequiredFetchForView)
			{
				Factory.AddFetchHint(CusEntryInstructionSchema.CEI_ClusterKey, declaration.JE_ClusterKey);
			}

			if (cusEntryNumRequiredFetchForView)
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, declaration.PK);
			}

			if (processTasksRequiredFetchForView)
			{
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(AccTransactionHeaderSchema.Instance, new InvoiceLoader(Factory).GetInvoicesForUniqueRefQuery(null, BusinessObject.JE_DeclarationReference));
				Factory.AddFetchHint(CusContainerSchema.CO_ClusterKey, BusinessObject.JE_ClusterKey);
				Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_ClusterKey, BusinessObject.JE_ClusterKey);
				Factory.AddFetchHint(JobHeaderSchema.JH_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(DtbBookingConsolidationSchema.KB_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(JobCartageSchema.JJ_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(CusEntryHeaderSchema.CH_ClusterKey, BusinessObject.JE_ClusterKey);
				Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, BusinessObject.PK);
			}

			if (shipmentRequiredFetchForView)
			{
				Factory.AddFetchHint(JobShipmentSchema.PK, declaration.JE_JS);
			}

			if (orderNumbersRequiredFetchForView)
			{
				Factory.AddFetchHint(JobOrderHeaderSchema.JD_JE, BusinessObject.PK);
				Factory.AddFetchHint(JobOrderHeaderSchema.JD_JS, declaration.JE_JS);
			}

			if (reverseShipmentLinkRequiredFetchForView)
			{
				// this is potentially expensive, so use it cautiously
				Factory.AddFetchHint(JobDeclarationSchema.JE_JS, declaration.JE_JS);
			}

			if (docsAndCartageRequiredFetchForView)
			{
				if (ShouldAddJobDocsAndTypeWhenNeeded)
				{
					Factory.AddFetchHint(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID, BusinessObject.PK);
				}
				else
				{
					Factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, BusinessObject.PK);
				}
				if (declaration.JE_JS.IsValid)
				{
					if (ShouldAddJobDocsAndTypeWhenNeeded)
					{
						Factory.AddFetchHint(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID, declaration.JE_JS);
					}
					else
					{
						Factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, declaration.JE_JS);
					}
				}
			}

			if (orgRequiredFetchForView)
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.JE_OH_Importer);
				Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.JE_OH_Supplier);
				Factory.AddFetchHint(OrgMiscServSchema.OM_OH, declaration.JE_OH_Supplier);
				Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.JE_OH_Forwarder);
				Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.JE_OH_ControllingAgent);
				Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.JE_OH_ControllingCustomer);
				Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.JE_OH_ExternalBroker);
			}

			if (stmALogRequiredFetchForView)
			{
				Factory.AddFetchHint(StmALogSchema.SL_Parent, BusinessObject.PK);
			}

			if (stmNoteRequiredFetchForView)
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.JE_JS);
			}

			if (jobConsolTransportRequiredFetchForView)
			{
				Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, BusinessObject.PK);
				Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, declaration.JE_JS);
			}

			if (jobConShipLinkRequiredFetchForView)
			{
				Factory.AddFetchHint(JobConShipLinkSchema.JN_JS, declaration.JE_JS);
			}

			if (transportBookingRequiredFetchForView)
			{
				Factory.AddFetchHint(DtbBookingConsolidationSchema.KB_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(DtbBookingConsolidationSchema.KB_ParentID, BusinessObject.JE_JS);
				Factory.AddFetchHint(JobShipmentSchema.PK, declaration.JE_JS);
			}

			if (declarantRequiredFetchForView)
			{
				Factory.AddFetchHint(OrgAddressSchema.PK, declaration.JE_OA_DeclarantAddress);
			}

			base.FetchForViewCore(columns);
		}

		void AddFetchHintForJob()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, BusinessObject.JE_JS.IsValid ? BusinessObject.JE_JS : BusinessObject.PK);
			query.AddToFilter(JobHeaderSchema.JH_GC, BusinessObject.Branch.Company.PK);

			Factory.AddFetchHint(typeof(JobHeader), query);
		}

		protected virtual bool IsJobComInvoiceHeaderBillRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsCusDecHouseBillRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.HouseBillsCommaSeparated
					|| columnName == BaseJobDeclaration.Schema.MasterBillsCommaSeparated
					|| columnName == BaseJobDeclaration.Schema.JE_BillsFilterBy
					|| columnName == BaseJobDeclaration.Schema.HouseBillIssuedDate;
		}

		protected virtual bool IsStmALogRelatedColumn(string columnName)
		{
			return declaration.StmALogProxyFieldsNames.Contains(columnName);
		}

		protected virtual bool IsStmNoteRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.JE_MarksAndNumbers
					|| columnName == BaseJobDeclaration.Schema.JE_MarksAndNumbersShort
					|| columnName == BaseJobDeclaration.Schema.JE_QuarantineMessagingRemarks;
		}

		protected virtual bool IsJobConsolTransportRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.JE_ETAOfDischarge
					|| columnName == BaseJobDeclaration.Schema.JE_ETDOfLoading;
		}

		protected virtual bool IsJobDocsAndCartageRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.JE_CartageCompleted
					|| columnName == BaseJobDeclaration.Schema.JE_DeliveryOrPickupLabourCharge
					|| columnName == BaseJobDeclaration.Schema.JE_DeliveryOrPickupLabourTime
					|| columnName == BaseJobDeclaration.Schema.JE_DeliveryOrPickupRequiredBy
					|| columnName == BaseJobDeclaration.Schema.JE_PickupOrDeliveryTruckWaitCharge
					|| columnName == BaseJobDeclaration.Schema.JE_PickupOrDeliveryTruckWaitTime
					|| columnName == BaseJobDeclaration.Schema.JE_EstimatedDeliveryOrPickup
					|| columnName == BaseJobDeclaration.Schema.JE_FCLDeliveryOrPickupEquipmentNeeded
					|| columnName == BaseJobDeclaration.Schema.DeliveryOrPickupCartageCoPK
					|| columnName == BaseJobDeclaration.Schema.JP_Calc_CartageAdvised
					|| columnName.Contains("DocsAndCartage");
		}

		protected virtual bool IsOrgRequiredRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.ImporterName
					|| columnName == BaseJobDeclaration.Schema.SupplierName
					|| columnName == BaseJobDeclaration.Schema.ForwarderName
					|| columnName == BaseJobDeclaration.Schema.JE_OH_ControllingAgent
					|| columnName == BaseJobDeclaration.Schema.JE_OH_ControllingCustomer
					|| columnName == BaseJobDeclaration.Schema.JE_OH_ExternalBroker;
		}

		protected virtual bool IsJobShipmentRelatedColumn(string columnName)
		{
			return
				columnName == BaseJobDeclaration.Schema.OrderNumbers ||
				columnName == BaseJobDeclaration.Schema.JE_CartageCompleted ||
				columnName == BaseJobDeclaration.Schema.JE_DeliveryOrPickupLabourCharge ||
				columnName == BaseJobDeclaration.Schema.JE_DeliveryOrPickupLabourTime ||
				columnName == BaseJobDeclaration.Schema.JE_DeliveryOrPickupRequiredBy ||
				columnName == BaseJobDeclaration.Schema.JE_PickupOrDeliveryTruckWaitCharge ||
				columnName == BaseJobDeclaration.Schema.JE_PickupOrDeliveryTruckWaitTime ||
				columnName == BaseJobDeclaration.Schema.JE_EstimatedDeliveryOrPickup ||
				columnName == BaseJobDeclaration.Schema.JE_ETAOfDischarge ||
				columnName == BaseJobDeclaration.Schema.JE_FCLDeliveryOrPickupEquipmentNeeded ||
				columnName == BaseJobDeclaration.Schema.DeliveryOrPickupCartageCoPK ||
				columnName == BaseJobDeclaration.Schema.JE_ETDOfLoading ||
				columnName == BaseJobDeclaration.Schema.JE_MarksAndNumbers ||
				columnName == BaseJobDeclaration.Schema.JE_MarksAndNumbersShort ||
				columnName == BaseJobDeclaration.Schema.JP_Calc_CartageAdvised;
		}

		protected virtual bool IsJobOrderHeaderRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.OrderNumbers;
		}

		protected virtual bool IsProcessTasksRelatedColumn(string columnName)
		{
			return columnName.Contains((NoResString)"Milestone");
		}

		protected virtual bool IsCusEntryHeaderRelatedColumn(string columnName)
		{
			bool related = columnName == BaseJobDeclaration.Schema.DeclarationNumber
							|| columnName == BaseJobDeclaration.Schema.EarliestCustomsEntryIssueDate
							|| columnName == BaseJobDeclaration.Schema.WarehouseTransactionStatusDescription
							|| columnName == BaseJobDeclaration.Schema.WarehouseTransactionStatus
							|| columnName == BaseJobDeclaration.Schema.EntryReleaseDate
							|| columnName == BaseJobDeclaration.Schema.PhaseStatus
							|| columnName == BaseJobDeclaration.Schema.PhaseStatusDescription;

			if (ShouldCombineEntryStatusFromHeaders)
			{
				related = related || columnName == BaseJobDeclaration.Schema.JE_EntryStatus
								|| columnName == BaseJobDeclaration.Schema.JE_EntryStatusDescription
								|| columnName == BaseJobDeclaration.Schema.JE_MessageStatus
								|| columnName == BaseJobDeclaration.Schema.JE_MessageStatusDescription;
			}

			return related;
		}

		bool ShouldCombineEntryStatusFromHeaders
		{
			get
			{
				if (!shouldCombineEntryStatusFromHeaders.HasValue)
				{
					shouldCombineEntryStatusFromHeaders = DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.PK);
				}
				return shouldCombineEntryStatusFromHeaders.Value;
			}
		}
		bool? shouldCombineEntryStatusFromHeaders;

		protected virtual bool IsCusEntryInstructionRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsCusEntryNumRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.DeclarationNumber || columnName == BaseJobDeclaration.Schema.EarliestCustomsEntryIssueDate;
		}

		protected virtual bool ShouldAddJobDocsAndTypeWhenNeeded
		{
			get { return false; }
		}

		protected new BaseJobDeclaration BusinessObject
		{
			get { return (BaseJobDeclaration)base.BusinessObject; }
		}

		#endregion

	}
}
