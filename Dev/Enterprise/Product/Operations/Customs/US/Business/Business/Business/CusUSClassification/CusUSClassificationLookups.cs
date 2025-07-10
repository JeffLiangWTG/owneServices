using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusUSClassificationLookups : AutoCusUSClassificationLookups
	{
		public CusUSClassificationLookups(AutoCusUSClassification parent)
			: base(parent)
		{
		}

		new CusUSClassification Parent
		{
			get { return (CusUSClassification)base.Parent; }
		}

		CusClassPartPivot Pivot
		{
			get { return Parent.Parent; }
		}

		public ReconIssueCodeList OtherReconIssueList
		{
			get { return OtherReconIssueListCreator.CreateOtherReconIssueList(Factory); }
		}

		public CodeDescriptionPairList TaxCodeList
		{
			get { return USCTariff.GetTaxCodeList(Pivot.ImportTariff, Factory); }
		}

		public CodeDescriptionPairList TaxRateList
		{
			get
			{
				var pivot = Pivot;

				return pivot != null
					? USCTariff.GetTaxRateList(pivot.ImportTariff, pivot.CD_TaxCode, pivot.CD_TaxRateType, Factory)
					: Factory.GetCachedValue<CodeDescriptionPairList>();
			}
		}

		public CodeDescriptionPairList CBMATaxRateList => Business.CBMATaxRateList.GetList(Factory, Pivot.CD_TaxCode, Pivot.CD_TaxRateDesc);

		public DepositRateIndicatorList AntidumpingDutyDepositRates
		{
			get { return new DepositRateIndicatorList(AntidumpingUSCACCaseRate); }
		}

		public DepositRateIndicatorList CountervailingDutyDepositRates
		{
			get { return new DepositRateIndicatorList(CountervailingUSCACCaseRate); }
		}

		public USCACCaseRate AntidumpingUSCACCaseRate
		{
			get
			{
				return Pivot.GetUSCACCaseRate(Pivot.AntidumpingDutyCase);
			}
		}

		public USCACCaseRate CountervailingUSCACCaseRate
		{
			get
			{
				return Pivot.GetUSCACCaseRate(Pivot.CountervailingDutyCase);
			}
		}

		public CodeDescriptionPairList SPIList
		{
			get
			{
				SPILine spiLine = SPILine.New(Pivot);
				if (spiLine == null)
				{
					return new CodeDescriptionPairList();
				}
				else
				{
					return Factory.GetCachedValue(spiLine.GetKeyForSPIList(),
						delegate
						{
							return SPICompleteList.GetRelevantListFor(spiLine);
						}
					);
				}
			}
		}

		public USCCountryCollection USCountryList
		{
			get { return new USCCountryCollection(Factory); }
		}

		public CodeDescriptionPairList US_YesNoList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(Factory); }
		}

		public CodeDescriptionPairList ProductClaimList
		{
			get { return Factory.GetCachedValue<SecondarySpecProgIndicatorList>(); }
		}

		public CodeDescriptionPairList CD_RulingTypeList
		{
			get { return Factory.GetCachedValue<PIRPRulingTypeList>(); }
		}

		public CodeDescriptionPairList US_OGAIndicatorList
		{
			get { return Factory.GetCachedValue<OGAIndicatorList>(); }
		}

		public CodeDescriptionPairList PGADisclaimReasonList
		{
			get { return Factory.GetCachedValue<PGADisclaimReasonList>(); }
		}

		public CodeDescriptionPairList US_OGAIndicatorWithoutDisclaimerList
		{
			get { return OGAIndicatorList.GetWithoutDisclaim(Factory); }
		}

		public TSCAIndicatorList CD_TSCAIndicatorList
		{
			get { return Factory.GetCachedValue<TSCAIndicatorList>(); }
		}

		public CodeDescriptionPairList CD_TSCAODSCertIndividualList
		{
			get
			{
				return Factory.GetCachedValue("TSCAODSCertIndividualList",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(PartyTypeList.Codes.CustomsBroker, PartyTypeList.Descriptions.CustomsBroker);
						result.AddPair(PartyTypeList.Codes.Importer, PartyTypeList.Descriptions.Importer);
						return result;
					}
					);
			}
		}

		public CodeDescriptionPairList TaxApplyList
		{
			get { return Factory.GetCachedValue<TaxApplyList>(); }
		}

		public CodeDescriptionPairList CD_SelectedRateTypeList
		{
			get { return Factory.GetCachedValue<RateTypeList>(); }
		}

		public virtual RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public AESOriginIndicatorList CD_OriginIndicatorList
		{
			get { return Factory.GetCachedValue<AESOriginIndicatorList>(); }
		}

		public ExportInformationCodeList CD_ExportCode_List
		{
			get { return Factory.GetCachedValue<ExportInformationCodeList>(); }
		}

		public CodeDescriptionPairList CD_ITARExemptionNoCodes
		{
			get { return CusITARENCodeConstants.GetITARExemptionNumberCodeList(Factory); }
		}

		public CodeDescriptionPairList DDTCExemptionCodes
		{
			get
			{
				if (Pivot.CI_ChildType == ClassificationTypeList.Codes.HTE)
				{
					return CusITARENCodeConstants.GetITARExemptionNumberCodeList(Factory);
				}
				else
				{
					return Factory.GetCachedValue<ACEDDTCExemptionCodes>();
				}
			}
		}

		public DDTCLicenseTypeCodes DDTCLicenseTypeCodes
		{
			get { return Factory.GetCachedValue<DDTCLicenseTypeCodes>(); }
		}

		public USMLCategoryCodes CD_DDTCUSMLCategoryCodes
		{
			get { return Factory.GetCachedValue<USMLCategoryCodes>(); }
		}

		public DDTCUnitOfMeasureList CD_DDTCUnitOfMeasureList
		{
			get { return Factory.GetCachedValue<DDTCUnitOfMeasureList>(); }
		}

		public CodeDescriptionPairList CD_ZoneStatusList
		{
			get { return Factory.GetCachedValue<ZoneStatusList>(); }
		}

		public CodeDescriptionPairList CD_ADDCVDNonReimbursementList
		{
			get { return ADDCVDNonReimbursementList.GetCachedList(Factory); }
		}

		[System.Xml.Serialization.XmlIgnore]
		public USAESLicenseCodeCollection USAESLicenseCodes
		{
			get
			{
				if (uSAESLicenseCodes == null)
				{
					uSAESLicenseCodes = new USAESLicenseCodeCollection(Factory);
				}
				return uSAESLicenseCodes;
			}
		}
		USAESLicenseCodeCollection uSAESLicenseCodes;
	}
}
