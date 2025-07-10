using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using GetAttributesDelegate = Enterprise.Customs.Business.PartAttribListCreator.GetAttributesDelegate;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceLineLookups : AutoJobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(AutoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public BaseJobComInvoiceLine InvoiceLine => (BaseJobComInvoiceLine)Parent;

		#region Collection

		public ICollection CountryOfOrigins => CountryOfOriginsCore();

		protected virtual ICollection CountryOfOriginsCore() => new RefCountryCollection(Factory);

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public virtual ConsignorCollection SupplierList => new ConsignorCollection(Factory);

		public RefCurrencyCollection CurrencyList => new RefCurrencyCollection(Factory);

		public ICollection CountryList => GetNewCountriesList();

		protected virtual ICollection GetNewCountriesList()
		{
			return new RefCountryCollection(Factory);
		}

		public IBaseCusGoodsCatalogCollection<BaseCusGoodsCatalog> GoodsCatalogList
			=> new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory, GlbCompany.CurrentCompany.PK);

		public virtual IBaseClassificationCollection<BaseCusClassification> ClassificationList
			=> new BaseClassificationCollection<BaseCusClassification>(Factory, new ZQuery(CusClassificationSchema.CC_RN_NKCountryCode, GetCustomsCountryCode()));

		public OrgSupplierPartCollection PartsList
		{
			get
			{
				var declaration = InvoiceLine.Declaration;
				var result = GetNewPartCollection();
				if (result != null && declaration != null)
				{
					if (!InvoiceLine.JI_PartNo.IsEmpty)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product Code", "Property", InvoiceLine.JI_PartNo));
					}
					if (!InvoiceLine.SupplierPK_Effective.IsEmpty)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property2", InvoiceLine.SupplierPK_Effective));
					}
					if (!InvoiceLine.ImporterPK_Effective.IsEmpty)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", InvoiceLine.ImporterPK_Effective));
					}
				}
				SetOrAdjustPartsListProperties(result);
				return result;
			}
		}

		protected virtual void SetOrAdjustPartsListProperties(OrgSupplierPartCollection partsList)
		{
		}

		protected virtual OrgSupplierPartCollection GetNewPartCollection()
		{
			return InvoiceLine.Declaration != null ? new OrgSupplierPartCollection(Factory, InvoiceLine, InvoiceLine.InvoiceHeader?.IsExport ?? ZBool.False) : new OrgSupplierPartCollection(Factory);
		}

		public virtual OrgHeaderCollection ShipToParties => new ConsigneeCollection(Factory);

		public virtual OrgHeaderCollection SoldToParties => new ConsigneeCollection(Factory);

		public virtual OrgHeaderCollection Consignees => new ConsigneeCollection(Factory);

		public ICodeDescriptionPairList RefCountryStates => Factory.GetStateList(InvoiceLine.JI_CountryOfOrigin, false);

		public virtual ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => InvoiceLine.Declaration?.CustomsEntryInstructionProvider?.CustomsEntryInstructions;

		#endregion

		#region SellerConsignor

		public virtual OrgHeaderCollection SellerConsignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		#endregion

		#region List

		public virtual ICodeDescriptionPairList Procedures => new RefCusProcedureCollection(Factory, GetCustomsCountryCode(), InvoiceLine?.Declaration?.DateOfValuation ?? ZDateTime.Today);

		public CodeDescriptionPairList PartAttrib1List => GetPartAttribList(x => x.Attributes1);

		public CodeDescriptionPairList PartAttrib2List => GetPartAttribList(x => x.Attributes2);

		public CodeDescriptionPairList PartAttrib3List => GetPartAttribList(x => x.Attributes3);

		public CodeDescriptionPairList SerialList => new CodeDescriptionPairList();

		CodeDescriptionPairList GetPartAttribList(GetAttributesDelegate getAttributes)
		{
			CodeDescriptionPairList result = null;
			var classificationType = GetClassificationType();
			if (!classificationType.IsEmpty && InvoiceLine.Part is OrgSupplierPart part)
			{
				result = part.GetPartAttribList(InvoiceLine.ImporterPK_Effective, InvoiceLine.SupplierPK_Effective, classificationType, getAttributes, GetCustomsCountryCode());
			}
			return result ?? new CodeDescriptionPairList();
		}

		protected virtual ZString GetClassificationType()
		{
			var result = ZString.Empty;
			if (InvoiceLine.IsImport)
			{
				result = InvoiceLine.GetClassificationTypeProvider().HTICode;
			}
			else if (InvoiceLine.IsExport)
			{
				result = InvoiceLine.GetClassificationTypeProvider().HTECode;
			}
			return result;
		}

		public virtual CodeDescriptionPairList BondedWhsUnitQtyList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public virtual CodeDescriptionPairList HazardousMaterialCodeQualifierList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public virtual CodeDescriptionPairList InvoiceUQList => RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);

		public virtual CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public virtual CodeDescriptionPairList VolumeUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		public virtual CodeDescriptionPairList CustomsUQList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public CodeDescriptionPairList ContainerModeList => InvoiceLine?.Declaration?.Lookups?.CargoIdTypeList;

		public CodeDescriptionPairList SortedInvoiceList => InvoiceLine?.Declaration?.Lookups?.SortedInvoiceList;

		public CodeDescriptionPairList CustomsProcedureCodes => InvoiceLine?.Declaration?.CustomsEntryInstructionProvider?.SortedEntryInstructionList;

		public virtual ICodeDescriptionPairList RelatedIndicatorList => Factory.GetCachedValue<RelatedIndicatorList>();

		public virtual CodeDescriptionPairList ValuationCodeList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public virtual CodeDescriptionPairList TaxOrFeeCodeList
		{
			get
			{
				var invoiceLine = InvoiceLine;
				if (!invoiceLine.UseUniversalTariff)
				{
					return new CodeDescriptionPairList();
				}
				else
				{
					var dataGroupingCode = invoiceLine.GetDefaultDataGroupingCode();
					var effectiveDate = invoiceLine.EffectiveAssessmentDate;
					var type = TaxOrFeeType;
					var tariff = invoiceLine.JI_Tariff;
					return Factory.GetCachedValue(
						Invariant($"TaxOrFeeCodeList_{dataGroupingCode}_{type}_{tariff}_{effectiveDate.ToShortDateString()}"),
						() =>
						{
							var result = new CodeDescriptionPairList(RefCusTaxOrFee.Loader.GetList(Factory, dataGroupingCode, effectiveDate, type));
							if (invoiceLine.UniversalTariff is TariffView universalTariff)
							{
								var tariffTaxOrFeeCodeList = universalTariff.GetEffectiveVATApplicabilities(effectiveDate)
									.Select(x => x.ZX5_ZZF_NKTaxOrFeeCode.ToString()).Distinct().ToHashSet();
								var universalTariffTaxOrFeeCode = universalTariff.ZZ1_ZZF_NKTaxOrFeeCode;
								if (!universalTariffTaxOrFeeCode.IsEmpty && tariffTaxOrFeeCodeList.Contains(universalTariffTaxOrFeeCode))
								{
									tariffTaxOrFeeCodeList.Add(universalTariffTaxOrFeeCode);
								}

								if (tariffTaxOrFeeCodeList.Count > 0)
								{
									var filteredResult = new CodeDescriptionPairList();
									foreach (ICodeDescription pair in result)
									{
										if (tariffTaxOrFeeCodeList.Contains(pair.Code))
										{
											filteredResult.Add(pair);
										}
									}

									if (filteredResult.Count > 0)
									{
										result = filteredResult;
									}
								}
							}

							return result;
						});
				}
			}
		}

		public virtual ZString TaxOrFeeType => ZString.Empty;

		public virtual ICodeDescriptionPairList PrimaryPreferenceList => UniversalReferenceDataHelper.GetPreferenceList(Factory,
																											InvoiceLine.UseUniversalTariff,
																											InvoiceLine.JI_Tariff,
																											CountryOfOriginForPrimaryPreferenceList,
																											InvoiceLine.UniversalTariff,
																											InvoiceLine.AllApplicableRatesSelectionCriteria,
																											GetDefaultTariffDataGroupingCode());

		public CodeDescriptionPairList OrderNumbersList => UniversalReferenceDataHelper.GetDynamicOrderNumberList(InvoiceLine.UniversalTariff, InvoiceLine.AllApplicableRatesSelectionCriteria);

		public virtual CodeDescriptionPairList AdditionalCodesList => UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(InvoiceLine.UniversalTariff, CachedListOfAdditionalCodeDescriptions, new IZZRateSelectionCriteria[] { InvoiceLine.AllApplicableRatesSelectionCriteria }, InvoiceLine.ConditionSelectionCriterias, ConditionTypesToExcludeFromAdditionalCodesList, InvoiceLine.VATSelectionCriteria, InvoiceLine.TariffAdditionalCodeSelectionCriteria);

		protected virtual ZString[] ConditionTypesToExcludeFromAdditionalCodesList => Array.Empty<ZString>();

		public CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions
											=> RefCusCodeListTypes.GetCachedList(Factory,
																				GetDefaultDataGroupingCode(),
																				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes,
																				ZDateTime.Now,
																				languageCode: TranslationHelper.GetCurrentLanguageCode());

		#endregion

		protected virtual ZDateTime GetDateOfValuation() => InvoiceLine.Declaration?.DateOfValuation ?? ZDateTime.Today;

		protected ZString GetDefaultDataGroupingCode() => InvoiceLine.InvoiceHeader?.GetDefaultDataGroupingCode() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected ZString GetDefaultCusProcedureDataGroupingCode() => InvoiceLine.InvoiceHeader?.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure) ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected virtual ZString GetDefaultTariffDataGroupingCode() => InvoiceLine.InvoiceHeader?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected virtual ZString CountryOfOriginForPrimaryPreferenceList => InvoiceLine.JI_CountryOfOrigin;

		protected ZString GetCustomsCountryCode() => InvoiceLine.CustomsCountryCode;

		public OrgHeaderCollection Exporters => InvoiceLine.Declaration?.Lookups.Exporters ?? new OrgHeaderCollection(Factory);

		public OrgHeaderCollection Growers => new OrgHeaderCollection(Factory);

		public OrgHeaderCollection Producers => new OrgHeaderCollection(Factory);
	}
}
