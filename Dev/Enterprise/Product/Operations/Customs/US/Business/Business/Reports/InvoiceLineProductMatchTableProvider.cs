using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Reports
{
	public class InvoiceLineProductMatchTableProvider : ParameterisedTableProvider
	{
		public override bool HandlesSortInternally => true;

		internal int BatchSize { get; set; } = 100;
		internal int MaxDeclarationCount { get; set; } = 100_000;
		internal int MaxInvoiceLineCount { get; set; } = 1_000_000;

		ReportData reportData;

		internal static string TooManyDeclarationsMessage(int estimatedDeclarationCount, int maxDeclarationCount)
		{
			return Enterprise.Customs.US.Business.Res.GetString(
				"InvoiceLineProductMatchTableProvider.Messages.TooManyDeclarations",
				"Estimated number of declarations is {0:N0}. That is greater than allowed maximum of {1:N0}.",
				estimatedDeclarationCount, maxDeclarationCount);
		}

		internal static string TooManyInvoiceLinesMessage(int invoiceLineCount, int maxInvoiceLineCount)
		{
			return Enterprise.Customs.US.Business.Res.GetString(
				"InvoiceLineProductMatchTableProvider.Messages.TooManyInvoiceLines",
				"Found at least {0:N0} invoice lines. That is greater than allowed maximum of {1:N0}.",
				invoiceLineCount, maxInvoiceLineCount);
		}

		protected override DataTable GetDataTable()
		{
			reportData = new ReportData();

			var table = new InvoiceLineProductMatchDataSet.InvoiceLineProductMatchDataSetDataTable();

			var reader = new FilteredBusinessObjectReader(CreateDeclarationQuery(), typeof(JobDeclaration));
			reader.BatchSize = BatchSize;
			reader.FactoryProvider.Current.RefreshEnabled = false;

			var estimatedDeclarationCount = reader.ApproximateCount;
			if (estimatedDeclarationCount > MaxDeclarationCount)
			{
				throw new DataProviderException(TooManyDeclarationsMessage(estimatedDeclarationCount, MaxDeclarationCount));
			}

			var batch = reader.LoadNextBatchInANewFactory(null).Cast<JobDeclaration>().ToList();
			while (batch.Count > 0)
			{
				PreFetchChildren(batch);
				foreach (var declaration in batch)
				{
					ProcessDeclaration(declaration);
				}
				batch = reader.LoadNextBatchInANewFactory(batch.Last()).Cast<JobDeclaration>().ToList();
			}

			BuildReportLines(table);

			return table;
		}

		void ProcessDeclaration(JobDeclaration declaration)
		{
			foreach (var line in declaration.InvoiceLines.OfType<JobComInvoiceLine>().Where(l => !l.IsChildLine))
			{
				Process(line, line.Part, line.Pivot, 0);

				var childLines = line.ChildLines?.ToArray() ?? Array.Empty<JobComInvoiceLine>();
				var pivotChildLines = line.Pivot?.Children?.OfType<CusClassPartPivot>().OrderBy(l => l.CI_ChildListOrder).ToArray() ?? Array.Empty<CusClassPartPivot>();
				for (int i = 0; i < childLines.Length && i < pivotChildLines.Length; i++)
				{
					Process(childLines[i], line.Part, pivotChildLines[i], i + 1);
				}
			}
		}

		void BuildReportLines(InvoiceLineProductMatchDataSet.InvoiceLineProductMatchDataSetDataTable table)
		{
			foreach (var part in reportData.Parts.Values.OrderBy(p => p.PartNo).ThenBy(p => p.PartPK))
			{
				foreach (var pivot in part.Pivots.Values.OrderBy(p => p.ChildLineNo))
				{
					if (pivot.NonMatchingLines.Count > 0)
					{
						foreach (var line in pivot.NonMatchingLines.OrderBy(l => l.JE_DeclarationReference).ThenBy(l => l.JZ_InvoiceNumber).ThenBy(l => l.JI_LineNo))
						{
							AddReportLine(table, part, pivot, line);
						}
					}
				}
			}
		}

		void PreFetchChildren(List<JobDeclaration> declarations)
		{
			if (declarations.Count == 0)
			{
				return;
			}

			var factory = declarations[0].Factory;

			var orgPKs = new HashSet<ZGuid>();

			foreach (var declaration in declarations)
			{
				factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
				factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, declaration.PK);
				factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, declaration.PK);
				factory.AddFetchHint(CusUnderbondDecSchema.BU_ClusterKey, declaration.JE_ClusterKey);
				factory.AddFetchHint(typeof(CusAddInfo), new ZQuery(CusAddInfoSchema.B7_ParentID, declaration.PK).AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USWHSPackLine));

				if (declaration.JE_OH_Importer.IsValid)
				{
					orgPKs.Add(declaration.JE_OH_Importer);
				}
				if (declaration.JE_OH_Supplier.IsValid)
				{
					orgPKs.Add(declaration.JE_OH_Supplier);
				}
			}

			factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgPKs));
			factory.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, orgPKs));
			foreach (var orgPK in orgPKs)
			{
				factory.AddFetchHint(OrgMiscServSchema.OM_OH, orgPK);
			}

			var externalFetchHintSupporter = (IExternalFetchHintSupporter)factory;
			using (externalFetchHintSupporter.SetupCreator())
			{
				externalFetchHintSupporter.AddTableFetchHintCreator(JobComInvoiceHeaderSchema.Instance, GetJobComInvoiceHeaderRelatedFetchHints);
				externalFetchHintSupporter.AddTableFetchHintCreator(JobComInvoiceLineSchema.Instance, GetJobComInvoiceLineRelatedFetchHints);
				externalFetchHintSupporter.AddTableFetchHintCreator(OrgSupplierPartSchema.Instance, GetOrgSupplierPartRelatedFetchHints);
				externalFetchHintSupporter.AddTableFetchHintCreator(CusClassPartPivotSchema.Instance, GetCusClassPartPivotRelatedFetechHints);

				var partPKs = new HashSet<ZGuid>();
				foreach (var invoiceLine in declarations.SelectMany(d => d.InvoiceLines).OfType<JobComInvoiceLine>())
				{
					if (invoiceLine.JI_OP.IsValid)
					{
						partPKs.Add(invoiceLine.JI_OP);
					}
				}

				var pivotPKs = factory.Load<CusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_OP, partPKs)).Select(p => p.PK).ToArray();
				factory.Load<CusUSClassification>(new ZQuery(CusUSClassificationSchema.CD_ParentID, pivotPKs));
				factory.Load<CusLineTariffDetail>(new ZQuery(CusLineTariffDetailSchema.BZ_ParentID, pivotPKs));
			}
		}

		static IEnumerable<IFetchHint> GetJobComInvoiceHeaderRelatedFetchHints(IColumnIndexer jobComInvoiceHeaderRow)
		{
			var invoicePK = jobComInvoiceHeaderRow.GetValue(JobComInvoiceHeaderSchema.PK);
			yield return new ZQueryFetchHint(JobComInvoiceLineSchema.Instance, new ZQuery(JobComInvoiceLineSchema.JI_JZ, invoicePK));
		}

		static IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer jobComInvoiceLineRow)
		{
			var entryLinePk = jobComInvoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_CL);
			if (entryLinePk.IsValid)
			{
				yield return new FetchHint(CusEntryLineSchema.PK, entryLinePk);
			}

			var partPk = jobComInvoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_OP);
			if (partPk.IsValid)
			{
				yield return new FetchHint(OrgSupplierPartSchema.PK, partPk);
			}
			else
			{
				var partNo = jobComInvoiceLineRow.GetValue(JobComInvoiceLineSchema.JI_PartNo);
				if (!partNo.IsEmpty) // Empty barcodes will not be queried
				{
					yield return new FetchHint(OrgSupplierPartBarcodeSchema.PH_Barcode, partNo);
					yield return new ZQueryFetchHint(OrgSupplierPartSchema.Instance, GetPartBarcodeFilterQuery(partNo));
				}
			}

			ZDBOnlyQuery GetPartBarcodeFilterQuery(string barcode)
			{
				var queryPart = new ZDBOnlyQuery(typeof(OrgSupplierPart));

				var subQueryBarcode = new ZDBOnlySubQuery(typeof(OrgSupplierPartBarcode), OrgSupplierPartBarcodeSchema.PH_OP);
				subQueryBarcode.AddToFilter(OrgSupplierPartBarcodeSchema.PH_Barcode, SQLComparisonOperator.Equal, barcode);
				queryPart.AddSubQuery(subQueryBarcode, JoinCondition.And);

				return queryPart;
			}
		}

		static IEnumerable<IFetchHint> GetOrgSupplierPartRelatedFetchHints(IColumnIndexer orgSupplierPartRow)
		{
			var partPk = orgSupplierPartRow.GetValue(OrgSupplierPartSchema.PK);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, partPk);
			yield return new FetchHint(CusClassPartPivotSchema.CI_OP, partPk);
			yield return new FetchHint(OrgPartRelationSchema.OU_OP, partPk);
		}

		static IEnumerable<IFetchHint> GetCusClassPartPivotRelatedFetechHints(IColumnIndexer cusClassPartPivotRow)
		{
			var cusClassPartPivotPk = cusClassPartPivotRow.GetValue(CusClassPartPivotSchema.PK);
			yield return new FetchHint(CusAttributeFilterSchema.BG_CI, cusClassPartPivotPk);
		}

		void Process(JobComInvoiceLine line, OrgSupplierPart part, CusClassPartPivot pivot, int childLineNo)
		{
			if (part == null && line.JI_OP.IsEmpty && line.JI_PartNo.IsEmpty)
			{
				return;
			}

			if (MatchesFilter(line))
			{
				reportData.InvoiceLineCount++;
				if (reportData.InvoiceLineCount > MaxInvoiceLineCount)
				{
					throw new DataProviderException(TooManyInvoiceLinesMessage(reportData.InvoiceLineCount, MaxInvoiceLineCount));
				}
				var partDetails = reportData.GetOrAdd(part, line.JI_PartNo);
				var pivotDetails = partDetails?.GetOrAdd(pivot, childLineNo);
				pivotDetails?.Add(this, line);
			}
		}

		bool MatchesFilter(JobComInvoiceLine line)
		{
			if (ManufacturerPK.IsValid && line.Manufacturer?.PK != ManufacturerPK)
			{
				return false;
			}
			if (ProductCodePrefixes.Count > 0)
			{
				var productCode = line.JI_PartNo;
				if (!ProductCodePrefixes.Any(p => productCode.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
				{
					return false;
				}
			}
			return true;
		}

		string[] DeclarationBranches { get; set; }
		string[] Declarations { get; set; }
		ZDateTime DeclarationDateFrom { get; set; }
		ZDateTime DeclarationDateTo { get; set; }
		ZDateTime ImportDateFrom { get; set; }
		ZDateTime ImportDateTo { get; set; }
		ZGuid ImporterPK { get; set; }
		ZGuid SupplierPK { get; set; }
		ZGuid ImporterOfRecordPK { get; set; }
		ZGuid ManufacturerPK { get; set; }
		List<string> ProductCodePrefixes { get; } = new List<string>();
		bool CompareAddCvd { get; set; }
		bool CompareSPI { get; set; }
		bool ComparePGA { get; set; }
		bool CompareTariff { get; set; }

		protected override void SetupParameterValues(object[] values)
		{
			int idx = 0;
			DeclarationBranches = Split((string)values[idx++]);
			Declarations = Split((string)values[idx++]);
			DeclarationDateFrom = (ZDateTime)values[idx++];
			DeclarationDateTo = (ZDateTime)values[idx++];
			ImportDateFrom = (ZDateTime)values[idx++];
			ImportDateTo = (ZDateTime)values[idx++];
			ImporterPK = (Guid)values[idx++];
			SupplierPK = (Guid)values[idx++];
			ImporterOfRecordPK = (Guid)values[idx++];
			ManufacturerPK = (Guid)values[idx++];
			ProductCodePrefixes.Clear();
			AddIfNotEmpty(ProductCodePrefixes, (string)values[idx++]);
			AddIfNotEmpty(ProductCodePrefixes, (string)values[idx++]);
			AddIfNotEmpty(ProductCodePrefixes, (string)values[idx++]);
			AddIfNotEmpty(ProductCodePrefixes, (string)values[idx++]);
			AddIfNotEmpty(ProductCodePrefixes, (string)values[idx++]);
			string[] auditOptions = Split((string)values[idx++]);
			if (auditOptions.Length == 0)
			{
				CompareAddCvd = true;
				CompareSPI = true;
				ComparePGA = true;
				CompareTariff = true;
			}
			else
			{
				CompareAddCvd = auditOptions.Contains("ADD/CVD");
				CompareSPI = auditOptions.Contains("SPI");
				ComparePGA = auditOptions.Contains("PGA");
				CompareTariff = auditOptions.Contains("Tariff");
			}
		}

		protected override Parameter[] ExpectedParameters()
		{
			return new Parameter[]
			{
				new Parameter("Declaration Branch", typeof(string)),
				new Parameter("Declarations", typeof(string)),
				new Parameter("Job Registered On->DateFrom", typeof(ZDateTime)),
				new Parameter("Job Registered On->DateTo", typeof(ZDateTime)),
				new Parameter("Import Date->DateFrom", typeof(ZDateTime)),
				new Parameter("Import Date->DateTo", typeof(ZDateTime)),
				new Parameter("Importer", typeof(Guid)),
				new Parameter("Supplier", typeof(Guid)),
				new Parameter("Importer of Record", typeof(Guid)),
				new Parameter("Manufacturer", typeof(Guid)),
				new Parameter("Product Code Starts With 1", typeof(string)),
				new Parameter("Product Code Starts With 2", typeof(string)),
				new Parameter("Product Code Starts With 3", typeof(string)),
				new Parameter("Product Code Starts With 4", typeof(string)),
				new Parameter("Product Code Starts With 5", typeof(string)),
				new Parameter("Audit Options", typeof(string))
			};
		}

		ZQuery CreateDeclarationQuery()
		{
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);

			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			if (DeclarationBranches.Length > 0)
			{
				branchQuery.AddToFilter(GlbBranchSchema.GB_Code, DeclarationBranches);
			}

			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, new ZString[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.ImportByExternalBroker });
			declarationQuery.AddSubQuery(branchQuery, JoinCondition.And);

			if (Declarations.Length > 0)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, Declarations);
			}
			if (DeclarationDateFrom.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, DeclarationDateFrom);
			}
			if (DeclarationDateTo.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, DeclarationDateTo);
			}
			if (ImportDateFrom.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ImportDateFrom);
			}
			if (ImportDateTo.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ImportDateTo);
			}
			if (ImporterPK.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, ImporterPK);
			}
			if (SupplierPK.IsValid)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, SupplierPK);
			}
			if (ImporterOfRecordPK.IsValid)
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDeclarationSchema.JE_OA_DeclarantAddress);
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, SQLComparisonOperator.Equal, ImporterOfRecordPK);
				declarationQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
			}
			return declarationQuery;
		}

		void AddIfNotEmpty(List<string> list, string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				list.Add(value.Trim());
			}
		}

		static string[] Split(string listAsString)
		{
			if (string.IsNullOrWhiteSpace(listAsString))
			{
				return Array.Empty<string>();
			}
			return listAsString.Split(',').Select(i => i.Trim()).ToArray();
		}

		void AddReportLine(InvoiceLineProductMatchDataSet.InvoiceLineProductMatchDataSetDataTable table, PartDetails part, PivotDetails pivot, InvoiceLineDetails line)
		{
			var row = table.NewInvoiceLineProductMatchDataSetRow();
			table.AddInvoiceLineProductMatchDataSetRow(row);

			row.PartPK = part.PartPK.IsValid ? part.PartPK.ToGuid() : Guid.Empty;
			row.PartNo = part.PartNo;
			row.PivotPK = pivot.PivotPK.IsValid ? pivot.PivotPK.ToGuid() : Guid.Empty;
			row.ChildLineNo = pivot.ChildLineNo;
			row.MatchingLineCount = pivot.MatchingLines;
			row.NonMatchingLineCount = pivot.NonMatchingLines.Count;
			row.ProductTariff = pivot.Tariff;
			row.ProductSupTariff = pivot.SupTariff;
			row.ProductCVDApplicable = pivot.CVDApplicable;
			row.ProductCVDCaseNo = pivot.CVDCaseNo;
			row.ProductCVDDepositRateInd = pivot.CVDDepositRateInd;
			row.ProductCVDBonded = pivot.CVDBonded;
			row.ProductADDApplicable = pivot.ADDApplicable;
			row.ProductADDCaseNo = pivot.ADDCaseNo;
			row.ProductADDDepositRateInd = pivot.ADDDepositRateInd;
			row.ProductADDBonded = pivot.ADDBonded;
			row.ProductADDDeclarationID = pivot.ADDDeclarationID;
			row.ProductSPI = pivot.SPI;
			row.ProductPGAIndicators = pivot.PGAIndicators;

			row.AddCvdMatches = line.AddCvdMatches;
			row.SPIMatches = line.SPIMatches;
			row.PGAMatches = line.PGAMatches;
			row.TariffMatches = line.TariffMatches;

			row.JE_DeclarationReference = line.JE_DeclarationReference;
			row.JZ_InvoiceNumber = line.JZ_InvoiceNumber;
			row.JI_LineNo = line.JI_LineNo;

			if (!line.JE_SystemCreateTimeUtc.IsEmpty)
			{
				row.JE_SystemCreateTimeUtc = line.JE_SystemCreateTimeUtc.ToDateTime();
			}
			if (!line.ImportDate.IsEmpty)
			{
				row.ImportDate = line.ImportDate.ToDateTime();
			}
			row.EntryType = line.EntryType;
			row.EntryFilerCode = line.EntryFilerCode;
			row.EntryNumber = line.EntryNumber;
			row.EntryLineNumber = line.EntryLineNumber;
			row.BranchCode = line.BranchCode;
			row.BrokerCode = line.BrokerCode;
			row.BrokerName = line.BrokerName;
			row.ImporterCode = line.ImporterCode;
			row.ImporterName = line.ImporterName;

			row.InvoiceLineTariff = line.Tariff;
			row.InvoiceLineSupTariff = line.SupTariff;
			row.InvoiceLineCVDApplicable = line.CVDApplicable;
			row.InvoiceLineCVDCaseNo = line.CVDCaseNo;
			row.InvoiceLineCVDDepositRateInd = line.CVDDepositRateInd;
			row.InvoiceLineCVDBonded = line.CVDBonded;
			row.InvoiceLineADDApplicable = line.ADDApplicable;
			row.InvoiceLineADDCaseNo = line.ADDCaseNo;
			row.InvoiceLineADDDepositRateInd = line.ADDDepositRateInd;
			row.InvoiceLineADDBonded = line.ADDBonded;
			row.InvoiceLineADDDeclarationID = line.ADDDeclarationID;
			row.InvoiceLineSPI = line.SPI;
			row.InvoiceLinePGAIndicators = line.PGAIndicators;
		}

		class ReportData
		{
			public Dictionary<(ZGuid PartPK, ZString partNo), PartDetails> Parts { get; } = new Dictionary<(ZGuid PartPK, ZString partNo), PartDetails>();
			public int InvoiceLineCount { get; set; }

			public PartDetails GetOrAdd(OrgSupplierPart part, ZString partNo)
			{
				var partPK = part?.PK ?? ZGuid.Empty;
				partNo = part?.OP_PartNum ?? partNo;

				var key = (partPK, partNo);

				if (!Parts.TryGetValue(key, out var partDetails))
				{
					partDetails = new PartDetails(partPK, partNo);
					Parts.Add(key, partDetails);
				}

				return partDetails;
			}
		}

		class PartDetails
		{
			public PartDetails(ZGuid partPK, ZString partNo)
			{
				// note: we should extract all required information from business object, but sholud not hold reference to business object, so it can be garbage-collected along with the temporary factory it was loaded in
				PartPK = partPK;
				PartNo = partNo;
			}

			public ZGuid PartPK { get; }
			public ZString PartNo { get; }

			public Dictionary<ZGuid, PivotDetails> Pivots { get; } = new Dictionary<ZGuid, PivotDetails>();

			public PivotDetails GetOrAdd(CusClassPartPivot pivot, int childLineNo)
			{
				var pivotPK = pivot?.PK ?? ZGuid.Empty;
				if (!Pivots.TryGetValue(pivotPK, out var pivotDetails))
				{
					pivotDetails = new PivotDetails(pivot, childLineNo);
					Pivots.Add(pivotPK, pivotDetails);
				}
				return pivotDetails;
			}
		}

		class PivotDetails
		{
			public PivotDetails(CusClassPartPivot pivot, int childLineNo)
			{
				// note: we should extract all required information from business object, but sholud not hold reference to business object, so it can be garbage-collected along with the temporary factory it was loaded in
				PivotPK = pivot?.PK ?? ZGuid.Empty;
				ChildLineNo = childLineNo;

				Tariff = GetActualTariff(pivot);
				SupTariff = pivot?.CI_FormattedSupplementalTariff ?? ZString.Empty;

				CVDApplicable = pivot?.CD_CVDApplicable ?? ZBool.False;
				CVDCaseNo = pivot?.CD_CVDCaseNo ?? ZString.Empty;
				CVDDepositRateInd = pivot?.CD_CVDDepositRateInd ?? ZString.Empty;
				CVDBonded = pivot?.CD_CVDBonded ?? ZBool.False;

				ADDApplicable = pivot?.CD_ADDApplicable ?? ZBool.False;
				ADDCaseNo = pivot?.CD_ADDCaseNo ?? ZString.Empty;
				ADDDepositRateInd = pivot?.CD_ADDDepositRateInd ?? ZString.Empty;
				ADDBonded = pivot?.CD_ADDBonded ?? ZBool.False;
				ADDDeclarationID = pivot?.CD_ADDDecID ?? ZString.Empty;

				SPI = pivot?.CD_SPI ?? ZString.Empty;

				PGAIndicators = pivot == null
					? ZString.Empty
					: FormatPGAIndicators(
						("Lacey", pivot.CD_LaceyActIndicator, pivot.CD_LaceyActDisclaimReason),
						("FDA", pivot.CD_ACEFDAIndicator, pivot.CD_ACEFDADisclaimReason),
						("NHTSA", pivot.CD_NHTSAIndicator, pivot.CD_NHTSADisclaimReason),
						("ATF", pivot.CD_ATFIndicator, null),
						("ODS", pivot.CD_ODSIndicator, pivot.CD_ODSDisclaimReason),
						("TSCA", pivot.CD_TSCAClaimIndicator, pivot.CD_TSCADisclaimReason),
						("PST", pivot.CD_PSTIndicator, pivot.CD_PSTDisclaimReason),
						("OMC", pivot.CD_OMCIndicator, pivot.CD_OMCDisclaimReason),
						("VNE", pivot.CD_VNEIndicator, pivot.CD_VNEDisclaimReason),
						("AMS", pivot.CD_AMSIndicator, pivot.CD_AMSDisclaimReason),
						("NOP", pivot.CD_NOPIndicator, pivot.CD_NOPDisclaimReason),
						("TTB", pivot.CD_TTBIndicator, pivot.CD_TTBDisclaimReason),
						("CPSC", pivot.CD_CPSCIndicator, pivot.CD_CPSCDisclaimReason),
						("DEA", pivot.CD_DEAIndicator, pivot.CD_DEADisclaimReason),
						("APHIS", pivot.CD_APHISIndicator, pivot.CD_APHISDisclaimReason),
						("DDTC", pivot.CD_DDTCIndicator, null),
						("NMFS370", pivot.CD_NMFS370Indicator, pivot.CD_NMFS370DisclaimReason),
						("NMFSAMR", pivot.CD_NMFSAMRIndicator, pivot.CD_NMFSAMRDisclaimReason),
						("NMFSHMS", pivot.CD_NMFSHMSIndicator, pivot.CD_NMFSHMSDisclaimReason),
						("NMFSSIMP", pivot.CD_NMFSSIMPIndicator, null),
						("FWS", pivot.CD_FWSIndicator, pivot.CD_FWSDisclaimReason)
					);
			}

			ZString GetActualTariff(CusClassPartPivot pivot)
			{
				if (pivot == null)
				{
					return ZString.Empty;
				}
				var tariff = pivot.CI_FormattedTariffNum;
				if (tariff.IsEmpty && !pivot.CI_CC.IsEmpty)
				{
					tariff = pivot.Classification?.CC_FormattedTariffNum ?? ZString.Empty;
				}
				return tariff;
			}

			public ZGuid PivotPK { get; }
			public int ChildLineNo { get; }

			public int MatchingLines { get; private set; }

			public List<InvoiceLineDetails> NonMatchingLines { get; } = new List<InvoiceLineDetails>();

			public ZString Tariff { get; }
			public ZString SupTariff { get; }

			public ZBool CVDApplicable { get; }
			public ZString CVDCaseNo { get; }
			public ZString CVDDepositRateInd { get; }
			public ZBool CVDBonded { get; }

			public ZBool ADDApplicable { get; }
			public ZString ADDCaseNo { get; }
			public ZString ADDDepositRateInd { get; }
			public ZBool ADDBonded { get; }
			public ZString ADDDeclarationID { get; }

			public ZString SPI { get; }
			public ZString PGAIndicators { get; }

			public void Add(InvoiceLineProductMatchTableProvider provider, JobComInvoiceLine line)
			{
				var lineDetails = new InvoiceLineDetails(this, line);

				bool matchingLine =
					(!provider.CompareAddCvd || lineDetails.AddCvdMatches) &&
					(!provider.CompareSPI || lineDetails.SPIMatches) &&
					(!provider.ComparePGA || lineDetails.PGAMatches) &&
					(!provider.CompareTariff || lineDetails.TariffMatches);

				if (matchingLine)
				{
					MatchingLines++;
				}
				else
				{
					NonMatchingLines.Add(lineDetails);
				}
			}
		}

		class InvoiceLineDetails
		{
			public InvoiceLineDetails(PivotDetails pivotDetails, JobComInvoiceLine line)
			{
				var declaration = line.InvoiceHeader?.JobDeclaration;
				var broker = declaration?.CusAgent;
				var importer = declaration?.Importer;
				var ensEntryNumber = declaration?.ENSEntryNumber;

				// note: we should extract all required information from business object, but sholud not hold reference to business object, so it can be garbage-collected along with the temporary factory it was loaded in
				JE_DeclarationReference = line.Declaration?.JE_DeclarationReference ?? ZString.Empty;
				JZ_InvoiceNumber = line.InvoiceHeader?.JZ_InvoiceNumber ?? ZString.Empty;
				JI_LineNo = line.JI_LineNo;

				JE_SystemCreateTimeUtc = declaration?.JE_SystemCreateTimeUtc ?? ZDateTime.Empty;
				ImportDate = declaration?.JE_DateOfArrival ?? ZDateTime.Empty;
				EntryType = declaration?.US_EntryType ?? ZString.Empty;
				EntryFilerCode = declaration?.US_EntryFilerCode ?? ZString.Empty;
				EntryNumber = ensEntryNumber?.CE_EntryNum ?? ZString.Empty;
				EntryLineNumber = line.CusEntryLine?.CL_LineNumber.ToString("D4", CultureInfo.InvariantCulture) ?? ZString.Empty;
				BranchCode = declaration?.Branch.GB_Code ?? ZString.Empty;
				BrokerCode = broker?.GS_Code ?? ZString.Empty;
				BrokerName = broker?.GS_FullName ?? ZString.Empty;
				ImporterCode = importer?.OH_Code ?? ZString.Empty;
				ImporterName = importer?.OH_FullName ?? ZString.Empty;

				Tariff = line.JI_FormattedTariff;
				SupTariff = line.SupTariffFormatted;

				CVDApplicable = line.US_CVD_NA;
				CVDCaseNo = line.US_CVDCaseNo;
				CVDDepositRateInd = line.US_CVDDepositRateIndicator;
				CVDBonded = line.US_IsBondedCVD;

				ADDApplicable = line.US_ADD_NA;
				ADDCaseNo = line.US_ADDCaseNo;
				ADDDepositRateInd = line.US_ADDDepositRateIndicator;
				ADDBonded = line.US_IsBondedADD;
				ADDDeclarationID = line.US_ADDDecID;

				SPI = line.US_SPI;
				PGAIndicators = FormatPGAIndicators(
					("Lacey", line.US_LaceyIndicator, line.US_LaceyDisclaimReason),
					("FDA", line.US_FDAIndicator, line.US_FDADisclaimReason),
					("NHTSA", line.US_NHTSAIndicator, line.US_NHTDisclaimReason),
					("ATF", line.US_ATFInd, null),
					("ODS", line.US_ODSInd, line.US_ODSDisclaimReason),
					("TSCA", line.US_TSCAInd, line.US_TSCADisclaimReason),
					("PST", line.US_PSTIndicator, line.US_PSTDisclaimReason),
					("OMC", line.US_OMCInd, line.US_OMCDisclaimReason),
					("VNE", line.US_VNEInd, line.US_VNEDisclaimReason),
					("AMS", line.US_AMSInd, line.US_AMSDisclaimReason),
					("NOP", line.US_NOPInd, line.US_NOPDisclaimReason),
					("TTB", line.US_TTBInd, line.US_TTBDisclaimReason),
					("CPSC", line.US_CPSCInd, line.US_CPSCDisclaimReason),
					("DEA", line.US_DEAInd, line.US_DEADisclaimReason),
					("APHIS", line.US_APHISInd, line.US_APHISDisclaimReason),
					("DDTC", line.US_DDTCInd, null),
					("NMFS370", line.US_NMFS370Ind, line.US_NMFS370DisclaimReason),
					("NMFSAMR", line.US_NMFSAMRInd, line.US_NMFSAMRDisclaimReason),
					("NMFSHMS", line.US_NMFSHMSInd, line.US_NMFSHMSDisclaimReason),
					("NMFSSIMP", line.US_NMFSSIMPInd, null),
					("FWS", line.US_FWSInd, line.US_FWSDisclaimReason)
				);

				AddCvdMatches = MatchAddCvd(pivotDetails);
				SPIMatches = MatchSPI(pivotDetails);
				PGAMatches = MatchPGA(pivotDetails);
				TariffMatches = MatchTariff(pivotDetails);
			}

			public ZString JE_DeclarationReference { get; }
			public ZString JZ_InvoiceNumber { get; }
			public ZInt JI_LineNo { get; }

			public ZDateTime JE_SystemCreateTimeUtc { get; }
			public ZDateTime ImportDate { get; }
			public ZString EntryType { get; }
			public ZString EntryFilerCode { get; }
			public ZString EntryNumber { get; }
			public ZString EntryLineNumber { get; }
			public ZString BranchCode { get; }
			public ZString BrokerCode { get; }
			public ZString BrokerName { get; }
			public ZString ImporterCode { get; }
			public ZString ImporterName { get; }

			public ZBool AddCvdMatches { get; }
			public ZBool SPIMatches { get; }
			public ZBool PGAMatches { get; }
			public ZBool TariffMatches { get; }

			public ZString Tariff { get; }
			public ZString SupTariff { get; }

			public ZBool CVDApplicable { get; }
			public ZString CVDCaseNo { get; }
			public ZString CVDDepositRateInd { get; }
			public ZBool CVDBonded { get; }

			public ZBool ADDApplicable { get; }
			public ZString ADDCaseNo { get; }
			public ZString ADDDepositRateInd { get; }
			public ZBool ADDBonded { get; }
			public ZString ADDDeclarationID { get; }

			public ZString SPI { get; }
			public ZString PGAIndicators { get; }

			bool MatchAddCvd(PivotDetails pivotDetails)
			{
				return
					CVDApplicable == pivotDetails.CVDApplicable &&
					CVDCaseNo == pivotDetails.CVDCaseNo &&
					CVDDepositRateInd == pivotDetails.CVDDepositRateInd &&
					CVDBonded == pivotDetails.CVDBonded &&
					ADDApplicable == pivotDetails.ADDApplicable &&
					ADDCaseNo == pivotDetails.ADDCaseNo &&
					ADDDepositRateInd == pivotDetails.ADDDepositRateInd &&
					ADDBonded == pivotDetails.ADDBonded &&
					ADDDeclarationID == pivotDetails.ADDDeclarationID;
			}

			bool MatchSPI(PivotDetails pivotDetails)
			{
				return SPI == pivotDetails.SPI;
			}

			bool MatchPGA(PivotDetails pivotDetails)
			{
				return PGAIndicators == pivotDetails.PGAIndicators;
			}

			bool MatchTariff(PivotDetails pivotDetails)
			{
				return Tariff == pivotDetails.Tariff && SupTariff == pivotDetails.SupTariff;
			}
		}

		static ZString FormatPGAIndicators(params (ZString Name, ZString Indicator, ZString DisclaimReason)[] pgas)
		{
			var pgaStrings = new List<string>();
			foreach (var pga in pgas)
			{
				if (!pga.Indicator.IsEmpty || !pga.DisclaimReason.IsEmpty)
				{
					pgaStrings.Add(FormattableString.Invariant($"{pga.Name} - {pga.Indicator}{(pga.DisclaimReason.IsEmpty ? string.Empty : "/" + pga.DisclaimReason)}"));
				}
			}
			return string.Join(", ", pgaStrings.OrderBy(l => l));
		}
	}
}
