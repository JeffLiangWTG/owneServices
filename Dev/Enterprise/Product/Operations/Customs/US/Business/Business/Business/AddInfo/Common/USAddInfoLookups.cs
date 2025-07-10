//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class USAddInfoLookups : AutoUSAddInfoLookups
	{
		public USAddInfoLookups(AutoUSAddInfo parent)
			: base(parent)
		{
		}

		public BusinessObjectCollection LoadingSchDList
		{
			get
			{
				BusinessObjectCollection result;
				var parent = Parent;
				if (parent.Declaration is JobDeclaration declaration && declaration.US_SchDLoadingTypeIsDropEdit)
				{
					result = declaration.PortOfLadingMappings;
				}
				else
				{
					result = parent.IsExport ? RegionDistrictPorts : ForeignPorts;
				}
				return result;
			}
		}

		public CodeDescriptionPairList US_OGAIndicatorList
		{
			get
			{
				return Factory.GetCachedValue("US_OGAIndicatorList" + (Parent.IsExport ? "Export" : "Import"), delegate
				{
					var result = new OGAIndicatorList();

					if (Parent.IsExport)
					{
						result.RemoveCode(OGAIndicatorList.Codes.Disclaimed);
					}

					return result;
				});
			}
		}

		public OGAIndicatorList US_OGAIndicatorListWithDisclaimerList
		{
			get { return Factory.GetCachedValue<OGAIndicatorList>(); }
		}

		public CodeDescriptionPairList US_OGAIndicatorWithoutDisclaimerList
		{
			get { return OGAIndicatorList.GetWithoutDisclaim(Factory); }
		}

		public CodeDescriptionPairList US_ADDCVDNonReimbursementList
		{
			get { return ADDCVDNonReimbursementList.GetCachedList(Factory); }
		}

		public CodeDescriptionPairList US_YesOnlyList
		{
			get { return YesNoDefaultList.GetCachedYesOnlyList(Factory); }
		}

		public CodeDescriptionPairList US_YesNoList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(Factory); }
		}

		protected CodeDescriptionPairList GetTaxCodeList(USCTariff importTariff)
		{
			return USCTariff.GetTaxCodeList(importTariff, Factory);
		}

		public CodeDescriptionPairList TaxApplyList
		{
			get
			{
				return Factory.GetCachedValue("USTaxApplyList",
					delegate
					{
						var result = new CodeDescriptionPairList();

						result.AddPair(Business.TaxApplyList.Codes.Yes, Business.TaxApplyList.Descriptions.Yes);
						result.AddPair(Business.TaxApplyList.Codes.No, Business.TaxApplyList.Descriptions.No);
						result.AddPair(Business.TaxApplyList.Codes.Override, Business.TaxApplyList.Descriptions.Override);

						return result;
					}
				);
			}
		}

		public YesNoDefaultList US_YesNoDefaultList
		{
			get { return Factory.GetCachedValue<YesNoDefaultList>(); }
		}

		public RelatedPartyList US_RelatedOrgList
		{
			get { return Factory.GetCachedValue<RelatedPartyList>(); }
		}

		public CodeDescriptionPairList US_InbondType_List
		{
			get
			{
				if (Parent.IsExport)
				{
					return Factory.GetCachedValue<InbondTypeList>();
				}
				else
				{
					return Factory.GetCachedValue<InbondCommonTypeList>();
				}
			}
		}

		public ExportInformationCodeList US_ExportCode_List
		{
			get { return Factory.GetCachedValue<ExportInformationCodeList>(); }
		}

		public LimitedReportingExportInformationCodeList LimitedReportingExportCodeList
		{
			get { return Factory.GetCachedValue<LimitedReportingExportInformationCodeList>(); }
		}

		public VehicleIDTypeList US_VehicleIDType_List
		{
			get { return Factory.GetCachedValue<VehicleIDTypeList>(); }
		}

		public AESOriginIndicatorList US_AESOriginIndicator_List
		{
			get { return Factory.GetCachedValue<AESOriginIndicatorList>(); }
		}

		public ZZRefCusCodeListCombinedCollection US_FDAProductNumberList
		{
			get
			{
				var countryCode = Core.Constants.CountryCodes.UnitedStates;
				var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
				var effectiveDate = ZDateTime.Today;

				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, countryCode, listType, effectiveDate);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(countryCode), false));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(listType), false));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", effectiveDate));
				return collection;
			}
		}

		public BusinessObjectCollection RegionDistrictPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombinedCollection ForeignPorts
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(
					Factory,
					Core.Constants.CountryCodes.UnitedStates,
					new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port },
					ZDateTime.Today,
					new[]
					{
						new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, ForeignPortTypeList.Codes.Common)
					});
			}
		}

		public ZZRefCusCodeListCombinedCollection ForeignPortsForAES
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(
					Factory,
					Core.Constants.CountryCodes.UnitedStates,
					new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port },
					ZDateTime.Today,
					new[]
					{
					new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal,
					new ZString[] { ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.AES }
					)
					});
			}
		}

		public USCForeignAndRegionPortCollection USDischargePortsForUSTerritory
		{
			get { return new USCForeignAndRegionPortCollection(Factory); }
		}

		public ACEDDTCExemptionCodes DDTCExemptionCodes
		{
			get { return Factory.GetCachedValue<ACEDDTCExemptionCodes>(); }
		}

		public DDTCLicenseTypeCodes DDTCLicenseTypeCodes
		{
			get { return Factory.GetCachedValue<DDTCLicenseTypeCodes>(); }
		}

		public CodeDescriptionPairList USStateList
		{
			get { return Factory.GetCachedUSStateList(); }
		}

		public CodeDescriptionPairList USStatesForVehiclesList
		{
			get { return Factory.GetCachedUSStateListForVehicles(); }
		}

		public CodeDescriptionPairList US_UnitOfMeasureList
		{
			get
			{
				if (Parent.IsExport)
				{
					return Factory.GetCachedValue<AESUnitOfMeasureList>();
				}
				else
				{
					return Factory.GetCachedValue<ABIUnitOfMeasureList>();
				}
			}
		}

		public RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public ConsignorCollection Consignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		public BrokerCollection Brokers
		{
			get { return new BrokerCollection(Factory); }
		}

		public ForwarderCollection Forwarders
		{
			get { return new ForwarderCollection(Factory); }
		}

		public OrganisationsFindBoxCollection TransferorsAndTransferees
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public OrgContactCollection ContactList
		{
			get { return new OrgContactCollection(Factory); }
		}

		public FDACarrierTypeList FDACANTypes
		{
			get { return Factory.GetCachedValue<FDACarrierTypeList>(); }
		}

		public FDAStatusList FDAStatusList
		{
			get { return Factory.GetCachedValue<FDAStatusList>(); }
		}

		public OrganisationsFindBoxCollection BillIssuers
		{
			get { return new BillIssuerOrganisationFindBoxCollection(Factory); }
		}

		public TransportModeCodes ExportingTransportTypeList
		{
			get
			{
				return Factory.GetCachedValue("ExportingTransportTypeList",
					delegate
					{
						var list = new TransportModeCodes();
						list.Sort();
						return list;
					}
				);
			}
		}

		public CodeDescriptionPairList US_EntryTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				var declaration = Parent.Declaration;

				if (declaration != null)
				{
					result = Factory.GetCachedValue("EntryTypeList" + declaration.JE_MessageType + declaration.JE_ApplicationCode + declaration.Is7552 +
																				declaration.IsACEStandalonePNWithoutENSAndCRL + declaration.IsStandalonePNTypeOfBLN,
						delegate
						{
							CodeDescriptionPairList list;

							if (declaration.IsACE)
							{
								list = EntryTypeList.GetACEList();
								if (declaration.IsACEStandalonePNWithoutENSAndCRL && declaration.IsStandalonePNTypeOfBLN)
								{
									list.AddPair(EntryTypeList.Codes.ImmediateTransportation, EntryTypeList.Descriptions.ImmediateTransportation);
									list.AddPair(EntryTypeList.Codes.TransportationExportation, EntryTypeList.Descriptions.TransportationExportation);
								}
								list.Sort();
							}
							else if (declaration.IsDrawback)
							{
								if (declaration.IsACEDrawback)
								{
									list = ACEDrawbackProvisionsList.GetDrawbackProvisionList(Factory);
								}
								else if (declaration.Is7552)
								{
									list = EntryTypeList.GetDeliveryCertificateForDrawbackPurposeEntryTypeList();
									list.Sort();
								}
								else
								{
									list = EntryTypeList.GetDrawbackSummaryEntryTypeList();
									list.Sort();
								}
							}
							else
							{
								var entryTypeList = new EntryTypeList();
								entryTypeList.RemoveInBondEntryTypes();
								entryTypeList.RemoveDrawbackSummaryEntryTypes();
								entryTypeList.RemoveCode(EntryTypeList.Codes.WarehouseFTZ);
								entryTypeList.RemoveLiquidationEntryTypes();
								entryTypeList.RemoveCode(EntryTypeList.Codes.LowValue);
								list = entryTypeList;
								list.Sort();
							}
							return list;
						}
					);
				}

				return result;
			}
		}

		public CodeDescriptionPairList US_CylindricalRectangularList
		{
			get { return Factory.GetCachedValue<CylindricalRectangularList>(); }
		}

		public CodeDescriptionPairList US_TaxDeferIndicatorList
		{
			get { return Factory.GetCachedValue<TaxDeferIndicatorList>(); }
		}

		public RefUNLOCOCollection Ports
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public RefCountryCollection ExportCountries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public BillIssuerOrganisationFindBoxCollection Carriers
		{
			get { return new BillIssuerOrganisationFindBoxCollection(Factory); }
		}

		public CodeDescriptionPairList US_CargoStorageCodeList
		{
			get { return Factory.GetCachedValue<CargoStorageCodeList>(); }
		}

		public USCCountryCollection USCountryList
		{
			get { return new USCCountryCollection(Factory); }
		}

		public CodeDescriptionPairList US_ZoneStatusList
		{
			get
			{
				var declaration = Parent.Declaration;
				return Factory.GetCachedValue("ZoneStatusList" + (declaration != null ? declaration.US_EntryType : ZString.Empty),
					delegate
					{
						var result = new ZoneStatusList();

						if (declaration != null && declaration.IsConsumptionFTZ)
						{
							result.RemoveCode(ZoneStatusList.Codes.ZoneRestricted);
						}

						return result;
					});
			}
		}

		public CodeDescriptionPairList US_SpecialProgramList
		{
			get { return SpecialProgramList.GetCachedList(Factory); }
		}

		public CodeDescriptionPairList US_PrimarySPIList
		{
			get { return Factory.GetCachedValue<PrimarySpecProgramIndicatorList>(); }
		}

		public CodeDescriptionPairList ProductClaimList
		{
			get
			{
				var declaration = Parent.Declaration;

				return Factory.GetCachedValue<CodeDescriptionPairList>("ProductClaimList" + (declaration != null ? declaration.JE_ApplicationCode : ZString.Empty),
					delegate
					{
						var result = new SecondarySpecProgIndicatorList();

						if (declaration != null)
						{
							if (declaration.IsACE)
							{
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.S);
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.V);
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.X);
							}
							else
							{
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.C);
							}

							if (declaration.IsRecon)
							{
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.F);
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.G);
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.H);
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.M);
								result.RemoveCode(SecondarySpecProgIndicatorList.Codes.S);
							}
						}

						return result;
					});
			}
		}

		public CodeDescriptionPairList US_SelectedRateTypeList
		{
			get { return Factory.GetCachedValue<RateTypeList>(); }
		}

		public CodeDescriptionPairList US_SWPMList
		{
			get { return Factory.GetCachedValue<SWPMList>(); }
		}

		public CodeDescriptionPairList US_PIRPRulingTypeList
		{
			get { return Factory.GetCachedValue<PIRPRulingTypeList>(); }
		}

		public ImportMessageStatusList MessageStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}

		public TSCAIndicatorList US_TSCAIndicatorList
		{
			get { return Factory.GetCachedValue<TSCAIndicatorList>(); }
		}

		public CodeDescriptionPairList US_MissingDocumentList
		{
			get { return Factory.GetCachedValue<MissingDocumentList>(); }
		}

		public ReconOriginalEntryHeaderCollection ReconOriginalEntries
		{
			get
			{
				ReconOriginalEntryHeaderCollection result = null;

				IDeclarationProvider declarationProvider = Parent.Parent as IDeclarationProvider;

				if (declarationProvider != null && declarationProvider.Declaration != null)
				{
					JobDeclaration jobDeclaration = (JobDeclaration)declarationProvider.Declaration;

					if (jobDeclaration.ReconDeclaration != null)
					{
						result = jobDeclaration.ReconDeclaration.OriginalEntries;
					}
				}

				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection FIRMSList
		{
			get
			{
				return UniversalReferenceDataHelper.GetCachedRefCusCodeListCombinedCollection(Factory,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode,
					Parent.Declaration != null && Parent.Declaration.IsFTZAdmission,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType,
					FacilityTypeList.Codes.ForeignTradeZone_02);
			}
		}

		public DDTCUnitOfMeasureList US_DDTCUnitOfMeasureList
		{
			get { return Factory.GetCachedValue<DDTCUnitOfMeasureList>(); }
		}

		public CodeDescriptionPairList US_DDTCITARExemptionCodes
		{
			get { return CusITARENCodeConstants.GetITARExemptionNumberCodeList(Factory); }
		}

		public CodeDescriptionPairList US_USMLCategoryCodes
		{
			get { return USMLCategoryCodes.GetCachedPostECRList(Factory); }
		}

		public SPICompleteList SPICompleteList => SPICompleteList.GetCachedList(Factory);

		public SEBCalculationList SEBCalcCodes
		{
			get { return Factory.GetCachedValue<SEBCalculationList>(); }
		}

		public CodeDescriptionPairList US_DRWCDUseCodeList
		{
			get { return Factory.GetCachedValue<CDCMDUseValueCodeList>(); }
		}

		protected CodeDescriptionPairList GetTaxRateList(USCTariff importTariff, ZString taxCode, ZString rateType)
		{
			return USCTariff.GetTaxRateList(importTariff, taxCode, rateType, Factory);
		}

		public CodeDescriptionPairList AMSDisclaimProgramList
		{
			get
			{
				return Factory.GetCachedValue("AMSDisclaimProgramList",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(AMSProgramList.Codes.MO8, AMSProgramList.Descriptions.MO8);
						return result;
					}
					);
			}
		}

		public CodeDescriptionPairList PSTDisclaimProgramList
		{
			get { return Factory.GetCachedValue<PSTProductTypeList>(); }
		}

		public CodeDescriptionPairList US_TSCAODSCertIndividualList
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

		new AddInfo Parent => (AddInfo)base.Parent;
	}
}
