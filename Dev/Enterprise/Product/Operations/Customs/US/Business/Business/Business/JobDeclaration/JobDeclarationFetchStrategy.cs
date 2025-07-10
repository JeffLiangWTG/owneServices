using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration BusinessObject
		{
			get { return (JobDeclaration)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(OrgAddress), BusinessObject.JE_OA_SoldToPartyAddress);
			Factory.AddFetchHint(typeof(CusInBondHeader), CusInBondHeaderSchema.BH_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.JE_OH_Importer);
			Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.JE_OH_Supplier);
			Factory.AddFetchHint(OrgAddressSchema.OA_OH, BusinessObject.JE_OA_ConsigneeAddress_ZAddress.OrgPK);
			Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.JE_OA_ConsigneeAddress);
			Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.JE_OH_Importer);
			if (BusinessObject.IOR is OrgHeader ior)
			{
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, ior.CountryData.PK);
			}

			Factory.AddFetchHint(ZZRefCusCodeListCombinedSchema.ZZD_Code, BusinessObject.US_SchDArrival);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
			if (BusinessObject.IsImport)
			{
				Factory.AddFetchHint(StmALogSchema.SL_Parent, BusinessObject.PK);
			}
			foreach (JobComInvoiceLine invoiceLine in BusinessObject.InvoiceLines)
			{
				Factory.AddFetchHint(JobComInvoiceLineSchema.JI_ParentID, invoiceLine.PK);
				Factory.AddFetchHint(JobComInvoiceLineSchema.PK, invoiceLine.US_JI_ParentProduct);
			}
			foreach (var bill in BusinessObject.Bills)
			{
				Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, bill.PK);
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, bill.PK);
			}
			if (BusinessObject.IOR is OrgHeader ior)
			{
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, ior.CountryData.PK);
			}
		}

		protected override bool IsCusEntryHeaderRelatedColumn(string columnName)
		{
			return base.IsCusEntryHeaderRelatedColumn(columnName) ||
				columnName == JobDeclaration.Schema.CargoReleaseStatus ||
				columnName == JobDeclaration.Schema.CargoReleaseStatusDesc ||
				columnName == JobDeclaration.Schema.SimplifiedEntryBillStatus ||
				columnName == JobDeclaration.Schema.SimplifiedEntryBillStatusDescription ||
				columnName == JobDeclaration.Schema.HLDOrEXMStatus ||
				columnName == JobDeclaration.Schema.EntrySummaryStatus ||
				columnName == JobDeclaration.Schema.EntrySummaryStatusDesc ||
				columnName == JobDeclaration.Schema.ExportStatus ||
				columnName == JobDeclaration.Schema.ExportStatusDesc ||
				columnName == JobDeclaration.Schema.EntrySubmittedDate ||
				columnName == JobDeclaration.Schema.TotalPayable ||
				columnName == JobDeclaration.Schema.TIBExpiryDate ||
				columnName == JobDeclaration.Schema.TIBNumOfExtensions ||
				columnName == "BGMReferences" ||
				columnName == JobDeclaration.Schema.US_AnticipatedLiquidationDate ||
				columnName == JobDeclaration.Schema.US_CollectionDate;
		}

		protected override bool IsCusDecHouseBillRelatedColumn(string columnName)
		{
			return base.IsCusDecHouseBillRelatedColumn(columnName) ||
				columnName == JobDeclaration.Schema.IsSplitShipment ||
				columnName == JobDeclaration.Schema.ISFBillStatus ||
				columnName == JobDeclaration.Schema.ISFBillStatusDescription;
		}

		protected override bool IsJobComInvoiceHeaderBillRelatedColumn(string columnName)
		{
			return base.IsJobComInvoiceHeaderBillRelatedColumn(columnName) ||
				columnName == JobDeclaration.Schema.ElectronicInvoiceStatus ||
				columnName == JobDeclaration.Schema.ElectronicInvoiceStatusDescription;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewDeclaration(TableColumn[] columns)
		{
			base.FetchForViewDeclaration(columns);
			var orgAddressRequiredFetchForView = false;
			var customsCodesRequiredFetchForView = false;
			var processTasksRequiredFetchForView = false;

			ZQuery headerQuery = new ZQuery(JobHeaderSchema.JH_ParentID, BusinessObject.JE_JS.IsValid ? BusinessObject.JE_JS : BusinessObject.PK);
			headerQuery.AddToFilter(JobHeaderSchema.JH_GC, BusinessObject.CompanyPK);
			Factory.AddFetchHint(typeof(JobHeader), headerQuery);

			if (BusinessObject.JE_JS.IsValid)
			{
				Factory.AddFetchHint(JobShipmentSchema.PK, BusinessObject.JE_JS);
			}

			foreach (TableColumn tableColumn in columns)
			{
				switch (tableColumn.ColumnName)
				{
					case JobDeclaration.Schema.CarrierSCAC:
						if (BusinessObject.JE_OH_ShippingLine.IsValid)
						{
							Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.JE_OH_ShippingLine);
							Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.JE_OH_ShippingLine);
						}
						break;
					case JobDeclaration.Schema.IORName:
					case JobDeclaration.Schema.UltimateConsigneeName:
						if (!orgAddressRequiredFetchForView)
						{
							if (BusinessObject.IOROrgPK.IsValid)
							{
								Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.IOROrgPK);
								Factory.AddFetchHint(OrgAddressSchema.OA_OH, BusinessObject.IOROrgPK);
							}
							orgAddressRequiredFetchForView = true;
						}
						break;
					case JobDeclaration.Schema.SPIAuditDate:
					case JobDeclaration.Schema.SPIAuditReference:
					case JobDeclaration.Schema.SPIAuditUser:
					case JobDeclaration.Schema.SPIAuditUserName:
					case JobDeclaration.Schema.FDAAuditDate:
					case JobDeclaration.Schema.FDAAuditReference:
					case JobDeclaration.Schema.FDAAuditUser:
					case JobDeclaration.Schema.FDAAuditUserName:
					case JobDeclaration.Schema.CWAuditDate:
					case JobDeclaration.Schema.CWAuditReference:
					case JobDeclaration.Schema.CWAuditUser:
					case JobDeclaration.Schema.CWAuditUserName:
					case JobDeclaration.Schema.TIBClosedDate:
					case JobDeclaration.Schema.TIBClosedReference:
					case JobDeclaration.Schema.TIBClosedUser:
					case JobDeclaration.Schema.TIBClosedUserName:
						Factory.AddFetchHint(StmALogSchema.SL_Parent, BusinessObject.JE_JS.IsValid ? BusinessObject.JE_JS : BusinessObject.PK);
						break;
					case JobDeclaration.Schema.LiquidationDate:
						Factory.AddFetchHint(CusLiquidationSchema.B8_JE, BusinessObject.PK);
						break;

					case JobDeclaration.Schema.DISStatus:
					case JobDeclaration.Schema.DISStatusDescription:
						Factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, BusinessObject.JE_JS.IsValid ? BusinessObject.JE_JS : BusinessObject.PK);
						Factory.AddFetchHint(JobRequiredDocumentSchema.EQ_ParentID, BusinessObject.DocsAndCartage.PK);
						break;

					case JobDeclaration.Schema.IncompleteDispositionsCode:
					case JobDeclaration.Schema.IncompleteDispositionsDescription:
						Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, BusinessObject.PK);
						var entrySummaryEntry = BusinessObject.ActiveEntryHeaders.EntrySummaryEntry;
						if (entrySummaryEntry != null)
						{
							Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, entrySummaryEntry.PK);
						}
						Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
						break;

					case JobDeclaration.Schema.ImporterEIN:
					case JobDeclaration.Schema.ImporterOfRecordEIN:
						if (!customsCodesRequiredFetchForView)
						{
							if (BusinessObject.JE_OH_Importer.IsValid)
							{
								Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.JE_OH_Importer);
								Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.JE_OH_Importer);
								Factory.AddFetchHint(OrgMiscServSchema.OM_OH, BusinessObject.JE_OH_Importer);
							}

							if (BusinessObject.IOROrgPK.IsValid)
							{
								Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.IOROrgPK);
								Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, BusinessObject.IOROrgPK);
								Factory.AddFetchHint(OrgMiscServSchema.OM_OH, BusinessObject.IOROrgPK);
							}

							customsCodesRequiredFetchForView = true;
						}
						break;
					case JobDeclaration.Schema.PGAStatus:
					case JobDeclaration.Schema.PGAStatusDesc:
						Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, BusinessObject.PK);
						Factory.AddFetchHint(CusDispositionSchema.CDI_ParentID, BusinessObject.PK);
						Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
						break;
					case JobDeclaration.Schema.HLDOrEXMStatus:
						Factory.AddFetchHint(CusDecHouseBillSchema.CU_ClusterKey, BusinessObject.JE_ClusterKey);
						break;
					default:
						if (!processTasksRequiredFetchForView && IsProcessTasksRelatedColumn(tableColumn.ColumnName))
						{
							processTasksRequiredFetchForView = true;
						}
						break;
				}
			}

			if (processTasksRequiredFetchForView)
			{
				Factory.AddFetchHint(CusDecHouseBillSchema.CU_ClusterKey, BusinessObject.JE_ClusterKey);
				Factory.AddFetchHint(CusInBondHeaderSchema.BH_ParentID, BusinessObject.PK);
			}
			base.FetchForViewDeclaration(columns);
		}

		protected override bool ShouldAddJobDocsAndTypeWhenNeeded
		{
			get { return true; }
		}

		protected override void FetchForMergeCore()
		{
			try
			{
				isImport = BusinessObject.IsImport;
				BusinessObject.LoadInvoiceLinesChildrenForPossiblyDeletionIfNeeded();
				BusinessObject.InvoiceLines.FetchStrategy.FetchForValidate();
				base.FetchForMergeCore();
			}
			finally
			{
				isImport = false;
			}
		}
		bool isImport;

		protected override void AddMergeFetchHintsAfterInvoiceLines()
		{
			base.AddMergeFetchHintsAfterInvoiceLines();
			foreach (JobComInvoiceLine invoiceLine in BusinessObject.InvoiceLines)
			{
				var importTariff = invoiceLine.ImportTariff;
				if (importTariff != null)
				{
					Factory.AddFetchHint(USCTariffDutyRateSchema.UD_UE, importTariff.PK);
				}
			}
		}

		protected override void AddMergeFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.FetchForLoadChildEditableObjectsIfNeeded();
			base.AddMergeFetchHintsFor(invoiceLine);
			if (isImport)
			{
				var usInvoiceLine = (JobComInvoiceLine)invoiceLine;
				AddFetchHintsForTariffs(invoiceLine.EffectiveDateForDutyRate, usInvoiceLine.JI_Tariff, usInvoiceLine.US_SupTariff);
				AddFetchHintsForTariffs(usInvoiceLine.FTZCurrentDutyDate, usInvoiceLine.US_FTZCurrentTariff);
			}
		}

		protected override void AddRefreshExRateFetchHintsFor(BaseJobComInvoiceLine invoiceLine)
		{
			base.AddRefreshExRateFetchHintsFor(invoiceLine);
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, invoiceLine.PK);
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			BusinessObject.FetchForLoadChildEditableObjectsIfNeeded();
			BusinessObject.Bills.ForEach(bill => bill.FetchForLoadChildEditableObjectsIfNeeded());
		}

		void AddFetchHintsForTariffs(ZDate effectiveDate, params ZString[] tariffNumbers)
		{
			foreach (var tariffNumber in tariffNumbers)
			{
				if (!tariffNumber.IsEmpty)
				{
					Factory.AddFetchHint(USCTariffSchema.Instance, new USCTariff.Loader(Factory).GetLoadBestMatchFilter(tariffNumber, effectiveDate));
					Factory.AddFetchHint(USCTariffRuleSchema.Instance, USCTariffRule.Loader.GetTariffRange(tariffNumber));
					Factory.AddFetchHint(TariffViewSchema.Instance, TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, tariffNumber, effectiveDate));
				}
			}
		}
	}
}
