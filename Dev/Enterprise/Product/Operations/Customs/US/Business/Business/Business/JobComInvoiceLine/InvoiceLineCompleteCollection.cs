using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
			this.declaration = jobDeclaration;
		}
		readonly JobDeclaration declaration;

		public void RefreshTariff()
		{
			var invoiceLinesToRefresh = this.Select(x => x).ToList();
			foreach (JobComInvoiceLine invoiceLine in invoiceLinesToRefresh)
			{
				invoiceLine.RefreshTariffDetails();
			}
		}

		public void ClearCalculateException()
		{
			foreach (JobComInvoiceLine invoiceLine in this)
			{
				invoiceLine.ClearCalculateException();
			}
		}

		public IEnumerable<JobComInvoiceLine> GetInvoiceLinesWithTariffType(ZString tariffType)
		{
			return this.Cast<JobComInvoiceLine>().Where(x => x.US_TariffType == tariffType);
		}

		protected override void SetDefaultFromPreviousLine(BaseJobComInvoiceLine previousLine, BaseJobComInvoiceLine currentInvoiceLine)
		{
			ReconDeclaration reconDec = declaration.ReconDeclaration;
			if (reconDec != null && reconDec.SelectedOriginalEntry.IsValid)
			{
				JobComInvoiceHeader previousInvoice = (JobComInvoiceHeader)previousLine.InvoiceHeader;
				if (previousInvoice == null || previousInvoice.US_CH_ReconEntry != reconDec.SelectedOriginalEntry)
				{
					previousLine = GetPreviousReconLineMatchingSelectedOriginalEntry(reconDec.SelectedOriginalEntry);
				}
			}

			if (previousLine == null)
			{
				SetDefaultForFirstInvoiceLine(currentInvoiceLine);
			}
			else
			{
				SetDefaultFromPreviousLineCore(previousLine, currentInvoiceLine);
			}
		}

		protected override void SetDefaultForFirstInvoiceLine(BaseJobComInvoiceLine firstInvoiceLine)
		{
			if (declaration.IsRecon && declaration.ReconDeclaration.OriginalEntries.Count == 0)
			{
				declaration.ReconDeclaration.OriginalEntries.AddNew();
			}
			base.SetDefaultForFirstInvoiceLine(firstInvoiceLine);
		}

		BaseJobComInvoiceLine GetPreviousReconLineMatchingSelectedOriginalEntry(ZGuid selectedOriginalEntry)
		{
			BaseJobComInvoiceLine result = null;
			for (int i = Count - 1; i > -1; i--)
			{
				JobComInvoiceLine invoiceLine = this[i];
				JobComInvoiceHeader invoice = invoiceLine.InvoiceHeader;
				if (invoice != null && invoice.US_CH_ReconEntry == selectedOriginalEntry)
				{
					result = invoiceLine;
					break;
				}
			}

			return result;
		}

		void SetDefaultFromPreviousLineCore(BaseJobComInvoiceLine previousLine, BaseJobComInvoiceLine currentInvoiceLine)
		{
			base.SetDefaultFromPreviousLine(previousLine, currentInvoiceLine);
			JobComInvoiceLine uSPreviousLine = (JobComInvoiceLine)previousLine;
			JobComInvoiceLine uSCurrenctLine = (JobComInvoiceLine)currentInvoiceLine;

			if (declaration != null && uSPreviousLine != null && declaration.IsDrawback)
			{
				uSCurrenctLine.SetDrawbackDefaultFromPreviousLine(uSPreviousLine);
			}
		}

		public IEnumerable<ReconIssues> ReconIssues
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in this)
				{
					if (invoiceLine.Pivot != null)
					{
						yield return invoiceLine.Pivot.GetReconIssueCalculated();
					}

					if (invoiceLine.Declaration != null && invoiceLine.Declaration.Importer != null)
					{
						var manufacturer = invoiceLine.Manufacturer;
						if (manufacturer != null)
						{
							var link = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(manufacturer, invoiceLine.Declaration.Importer, invoiceLine.Declaration.FinalDestinationCountryCode) ?? OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(manufacturer, invoiceLine.Declaration.Importer, invoiceLine.Declaration.BranchCompanyCountryCode);

							if (link != null)
							{
								yield return link.GetReconIssueCalculated();
							}
						}
					}
				}
			}
		}

		public IEnumerable<ZBool> ReconNAFTAs
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in this)
				{
					if (invoiceLine.Pivot != null)
					{
						yield return invoiceLine.Pivot.CD_NAFTARecon;
					}
				}
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			var invoiceLine = bizO as JobComInvoiceLine;
			if (invoiceLine != null && invoiceLine.IsFDADeclared && !declaration.CanHavePGAFDA)
			{
				declaration.UpdateFDAMsgStatus(-1);
			}
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new InvoiceLineCompleteCollectionFetchStrategy(this);
		}

		class InvoiceLineCompleteCollectionFetchStrategy : Customs.Business.FetchStrategies.InvoiceLineCompleteCollectionFetchStrategy
		{
			public InvoiceLineCompleteCollectionFetchStrategy(InvoiceLineCompleteCollection collection)
				: base(collection)
			{
			}

			protected new InvoiceLineCompleteCollection Collection
			{
				get { return (InvoiceLineCompleteCollection)base.Collection; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				var declaration = Collection.declaration;
				var isRecon = false;
				var isFormalImport = false;
				var isExport = declaration.IsExport;
				var isImportACE = false;
				var isFTZAdmission = false;
				if (declaration == null || isExport || declaration.IsDrawback)
				{
					// no tariff related fetch 
				}
				else if (declaration.IsRecon)
				{
					isRecon = true;
				}
				else if (declaration.IsFTZAdmission)
				{
					isFTZAdmission = true;
					isImportACE = declaration.IsACE;
				}
				else
				{
					isFormalImport = true;
					isImportACE = declaration.IsACE;
				}
				var factory = Collection.Factory;
				var tariffs = (isRecon || isFormalImport || isFTZAdmission) ? new Dictionary<ZDate, HashSet<ZString>>() : null;
				var originalTariffs = isRecon ? new Dictionary<ZDate, HashSet<ZString>>() : null;
				var quotaDetails = isFormalImport ? new Dictionary<ZString, List<Tuple<ZString, ZString>>>() : null;
				var visaDetails = isFormalImport ? new List<Tuple<ZString, ZString, ZDateTime>>() : null;
				var countryOfOrigins = !isExport ? new HashSet<ZString>() : null;
				var requireCVDCaseNo = isImportACE ? new Dictionary<ZString, HashSet<ZString>>() : null;
				var requireADDCaseNo = isImportACE ? new Dictionary<ZString, HashSet<ZString>>() : null;
				foreach (JobComInvoiceLine invoiceLine in Collection)
				{
					factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, invoiceLine.PK);
					factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, invoiceLine.PK);
					factory.AddFetchHint(JobComInvoiceLineSchema.JI_ParentID, invoiceLine.PK);
					factory.AddFetchHint(JobComInvoiceLineSchema.PK, invoiceLine.US_JI_ParentProduct);
					if (countryOfOrigins != null)
					{
						var countryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
						if (!countryOfOrigin.IsEmpty && !countryOfOrigins.Contains(countryOfOrigin))
						{
							countryOfOrigins.Add(countryOfOrigin);
						}
					}
				}

				countryOfOrigins.ForEach((countryOfOrigin) => factory.AddFetchHint(USCCountrySchema.UC_Code, countryOfOrigin));
				var importLinesNotXSet = new List<JobComInvoiceLine>();

				foreach (JobComInvoiceLine invoiceLine in Collection)
				{
					var supTariff = invoiceLine.US_SupTariff;
					var tariff = invoiceLine.JI_Tariff;
					var effectiveDateForDutyRate = invoiceLine.EffectiveDateForDutyRate;
					if (tariffs != null)
					{
						if (!supTariff.IsEmpty || !tariff.IsEmpty)
						{
							AddTariffAndFetchHintIfNotEmpty(factory, tariffs, supTariff, effectiveDateForDutyRate);
							AddTariffAndFetchHintIfNotEmpty(factory, tariffs, tariff, effectiveDateForDutyRate);
							if (isImportACE && invoiceLine.US_SetInd != SecondarySpecProgIndicatorList.Codes.X)
							{
								importLinesNotXSet.Add(invoiceLine);
							}
						}
						if (originalTariffs != null)
						{
							var origTariff = invoiceLine.US_R_OrigTariff;
							if (!origTariff.IsEmpty && origTariff != tariff)
							{
								AddTariffAndFetchHintIfNotEmpty(factory, originalTariffs, origTariff, effectiveDateForDutyRate);
							}

							var orgSupTariff = invoiceLine.US_R_OrigSupTariff;
							if (!orgSupTariff.IsEmpty && orgSupTariff != supTariff)
							{
								AddTariffAndFetchHintIfNotEmpty(factory, originalTariffs, orgSupTariff, effectiveDateForDutyRate);
							}
						}
					}
					else
					{
						if (invoiceLine.UseScheduleB)
						{
							factory.AddFetchHint(TariffViewSchema.Instance, TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB, tariff, effectiveDateForDutyRate));
						}
						else
						{
							factory.AddFetchHint(TariffViewSchema.Instance, TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export, tariff, effectiveDateForDutyRate));
						}
					}

					if (quotaDetails != null)
					{
						GatherQuotaFetchDetail(quotaDetails, invoiceLine);
					}

					if (visaDetails != null)
					{
						GatherVisaFetchDetail(declaration, visaDetails, invoiceLine);
					}
					if (isFormalImport)
					{
						factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, invoiceLine.PK));
					}
				}

				if (tariffs != null && tariffs.Count > 0)
				{
					var htsPk = RefCusTariffType.Loader.Load(factory, Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem)?.PK ?? ZGuid.Empty;
					var relatedTariffQueries = new List<ZQuery>();
					var uscTariffLoader = new USCTariff.Loader(factory);
					var relatedTariffQuery = new ZQuery();
					var count = 0;
					foreach (var tariffData in tariffs)
					{
						var effectiveDateForDutyRate = tariffData.Key;
						foreach (var tariff in tariffData.Value)
						{
							factory.AddFetchHint(typeof(USCTariff), USCTariffSchema.UE_Tariff, tariff);
							factory.AddFetchHint(USCTariffRuleSchema.Instance, USCTariffRule.Loader.GetTariffRange(tariff));
							factory.AddFetchHint(TariffViewSchema.Instance, TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, tariff, effectiveDateForDutyRate));
							relatedTariffQuery.AddToFilter(TariffView.Loader.GetTariffRelationshipViewQuery(ZGuid.Empty, Array.Empty<ZString>(), tariff, htsPk), JoinCondition.Or);
							if (count == 100)
							{
								if (!relatedTariffQuery.IsEmpty)
								{
									relatedTariffQueries.Add(relatedTariffQuery);
									relatedTariffQuery = new ZQuery();
								}

								count = 0;
							}
							else
							{
								count++;
							}
						}
					}

					if (!relatedTariffQuery.IsEmpty)
					{
						relatedTariffQueries.Add(relatedTariffQuery);
					}

					if (relatedTariffQueries.Count > 0)
					{
						relatedTariffQueries.ForEach(query =>
							factory.Load<TariffRelationshipView>(query).ForEach(x =>
								factory.AddFetchHint(TariffViewSchema.PK, x.ZZH_ZZ1_LinkedTariffOrNationalCode)));
					}

					var tariffLoader = new TariffView.Loader(factory);
					relatedTariffQueries.Clear();
					relatedTariffQuery = new ZQuery();
					count = 0;
					foreach (var tariffData in tariffs)
					{
						var effectiveDateForDutyRate = tariffData.Key;
						foreach (var tariff in tariffData.Value)
						{
							if (uscTariffLoader.LoadBestMatch(tariff, effectiveDateForDutyRate) != null)
							{
								if (tariffLoader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, tariff, effectiveDateForDutyRate) is TariffView tariffView)
								{
									factory.AddFetchHint(TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode, tariffView.PK);
								}
								relatedTariffQuery.AddToFilter(tariffLoader.GetEffectiveTariffFilter(Core.Constants.CountryCodes.UnitedStates, ZString.Empty, ZString.Empty, effectiveDateForDutyRate, tariff, Constants.TariffTypes.HarmonizedSystem), JoinCondition.Or);
								if (count == 100)
								{
									if (!relatedTariffQuery.IsEmpty)
									{
										relatedTariffQueries.Add(relatedTariffQuery);
										relatedTariffQuery = new ZQuery();
									}

									count = 0;
								}
								else
								{
									count++;
								}
							}
						}
					}

					if (!relatedTariffQuery.IsEmpty)
					{
						relatedTariffQueries.Add(relatedTariffQuery);
					}

					if (relatedTariffQueries.Count > 0)
					{
						relatedTariffQueries.ForEach(query =>
							factory.Load<TariffView>(query).ForEach(x =>
								factory.AddFetchHint(TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode, x.PK)));
					}
				}

				if (quotaDetails != null && quotaDetails.Count > 0)
				{
					var quotaLoader = new USCQuota.Loader(factory);
					var presentationDate = ZDateTime.Today;
					var exportDate = declaration.JE_ExportDate;
					foreach (var pair in quotaDetails)
					{
						foreach (var quotaTariff in pair.Value)
						{
							factory.AddFetchHint(USCQuotaSchema.Instance, quotaLoader.GetBestMatchQueryFor(quotaTariff.Item1, quotaTariff.Item2, pair.Key, presentationDate, exportDate));
						}
					}
				}

				if (visaDetails != null && visaDetails.Count > 0)
				{
					var visaLoader = new USCVisa.Loader(factory);
					foreach (var visaDetail in visaDetails)
					{
						factory.AddFetchHint(USCVisaSchema.Instance, visaLoader.GetLoadFilter(visaDetail.Item1, visaDetail.Item2, visaDetail.Item3));
					}
				}
				GatherADD_CVDThatNeedCaseNoValidation(importLinesNotXSet, requireADDCaseNo, requireCVDCaseNo);
				AddCaseFetchHintsIfNeeded(factory, requireCVDCaseNo, ADD_CVDLiabilityChecker.ADD_CVD.CVD);
				AddCaseFetchHintsIfNeeded(factory, requireADDCaseNo, ADD_CVDLiabilityChecker.ADD_CVD.ADD);
			}

			static void AddCaseFetchHintsIfNeeded(BusinessObjectFactory factory, Dictionary<ZString, HashSet<ZString>> requireCaseNo, ADD_CVDLiabilityChecker.ADD_CVD add_CVD)
			{
				if (requireCaseNo != null)
				{
					var caseNumberPrefix = ADD_CVDLiabilityChecker.GetCaseNumberPrefix(add_CVD);
					var loader = new USCACCase.Loader(factory);
					foreach (var pair in requireCaseNo)
					{
						var countryOfOrigin = pair.Key;
						foreach (var tariffNumber in pair.Value)
						{
							loader.AddToNeedToLoadIfNeeded(tariffNumber, countryOfOrigin, caseNumberPrefix);
						}
					}
				}
			}

			static void GatherADD_CVDThatNeedCaseNoValidation(IEnumerable<JobComInvoiceLine> importLinesNotXSet, Dictionary<ZString, HashSet<ZString>> requireADDCaseNo, Dictionary<ZString, HashSet<ZString>> requireCVDCaseNo)
			{
				if (requireADDCaseNo != null || requireCVDCaseNo != null)
				{
					foreach (var invoiceLine in importLinesNotXSet)
					{
						var supTariff = invoiceLine.ImportSupTariff;
						var tariff = invoiceLine.ImportTariff;
						if (supTariff != null || tariff != null)
						{
							var validateCountryOfOrigin = invoiceLine.IsCountryOfOriginCanada ? (ZString)Core.Constants.CountryCodes.Canada : invoiceLine.US_UC_NKCountryOfOrigin;
							if (!validateCountryOfOrigin.IsEmpty)
							{
								if (!invoiceLine.US_CVD_NA && invoiceLine.US_CVDCaseNo.IsEmpty)
								{
									AddToCaseNoValidation(requireCVDCaseNo, supTariff, tariff, validateCountryOfOrigin);
								}
								if (!invoiceLine.US_ADD_NA && invoiceLine.US_ADDCaseNo.IsEmpty)
								{
									AddToCaseNoValidation(requireADDCaseNo, supTariff, tariff, validateCountryOfOrigin);
								}
							}
						}
					}
				}
			}

			static void AddToCaseNoValidation(Dictionary<ZString, HashSet<ZString>> requireCaseNo, USCTariff supTariff, USCTariff tariff, ZString validateCountryOfOrigin)
			{
				if (!requireCaseNo.TryGetValue(validateCountryOfOrigin, out var tariffsToValidateAgainst))
				{
					tariffsToValidateAgainst = new HashSet<ZString>();
					requireCaseNo.Add(validateCountryOfOrigin, tariffsToValidateAgainst);
				}
				if (tariff != null && !tariffsToValidateAgainst.Contains(tariff.UE_Tariff))
				{
					tariffsToValidateAgainst.Add(tariff.UE_Tariff);
				}
				if (supTariff != null && !tariffsToValidateAgainst.Contains(supTariff.UE_Tariff))
				{
					tariffsToValidateAgainst.Add(supTariff.UE_Tariff);
				}
			}

			void GatherVisaFetchDetail(JobDeclaration declaration, List<Tuple<ZString, ZString, ZDateTime>> visaDetails, JobComInvoiceLine invoiceLine)
			{
				var textileCategoryNo = invoiceLine.US_TextileCategoryNo_Effective;
				var countryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
				var exportDate = ZDateTime.Empty;
				if (!invoiceLine.US_DateOfExportFromCountryOfOrigin.IsEmpty)
				{
					exportDate = invoiceLine.US_DateOfExportFromCountryOfOrigin;
				}
				else
				{
					var invoice = invoiceLine.InvoiceHeader;
					if (invoice != null && !invoice.US_DateOfExport.IsEmpty)
					{
						exportDate = invoice.US_DateOfExport;
					}
					else
					{
						exportDate = declaration.JE_ExportDate;
					}
				}

				if (!textileCategoryNo.IsEmpty && !countryOfOrigin.IsEmpty && exportDate.IsValid)
				{
					visaDetails.Add(new Tuple<ZString, ZString, ZDateTime>(textileCategoryNo, countryOfOrigin, exportDate));
				}
			}

			void GatherQuotaFetchDetail(Dictionary<ZString, List<Tuple<ZString, ZString>>> quotaDetails, JobComInvoiceLine invoiceLine)
			{
				var countryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
				List<Tuple<ZString, ZString>> quotaTariffs;
				if (!quotaDetails.TryGetValue(countryOfOrigin, out quotaTariffs))
				{
					quotaTariffs = new List<Tuple<ZString, ZString>>();
					quotaDetails.Add(countryOfOrigin, quotaTariffs);
				}
				var tariffOne = ZString.Empty;
				var tariffTwo = ZString.Empty;
				if (!invoiceLine.HasEmptySupTariff)
				{
					tariffOne = invoiceLine.US_SupTariff;
					tariffTwo = invoiceLine.JI_Tariff;
				}
				else
				{
					tariffOne = invoiceLine.JI_Tariff;
					var theOnlySecondaryTariff = invoiceLine.SecondaryTariffLines.IsCountEqualTo(1) ? invoiceLine.SecondaryTariffLines.ElementAt(0) : null;
					tariffTwo = theOnlySecondaryTariff != null ? theOnlySecondaryTariff.JI_Tariff : ZString.Empty;
				}

				var quotaTariff = quotaTariffs.FirstOrDefault(x => x.Item1 == tariffOne && x.Item2 == tariffTwo);
				if (quotaTariff == null)
				{
					quotaTariffs.Add(new Tuple<ZString, ZString>(tariffOne, tariffTwo));
				}
			}

			void AddTariffAndFetchHintIfNotEmpty(BusinessObjectFactory factory, Dictionary<ZDate, HashSet<ZString>> tariffs, ZString tariff, ZDate effectiveDateForDutyRate)
			{
				if (!tariff.IsEmpty)
				{
					factory.AddFetchHint(typeof(USCTariffRule), USCTariffRule.Loader.GetTariffRange(tariff));
					if (!tariffs.TryGetValue(effectiveDateForDutyRate, out var list))
					{
						list = new HashSet<ZString>();
						list.Add(tariff);
						tariffs.Add(effectiveDateForDutyRate, list);
					}
					else if (!list.Contains(tariff))
					{
						list.Add(tariff);
					}
				}
			}
		}
	}
}
