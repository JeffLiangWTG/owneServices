using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using USEntryDateElectionCodeList = Enterprise.Customs.US.Business.EntryDateElectionCodeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobDeclarationLookups : USAddInfoLookups
	{
		public AddInfoJobDeclarationLookups(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		public new AddInfoJobDeclaration Parent
		{
			get { return (AddInfoJobDeclaration)base.Parent; }
		}

		public JobDeclaration Declaration
		{
			get { return (JobDeclaration)Parent.Parent; }
		}

		public ZZRefCusCodeListCombinedCollection ImportEstablishments
		{
			get { return new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFSISEstablishmentNumbers, ZDate.Today); }
		}

		public CodeDescriptionPairList US_BondDesignationCodeList
		{
			get
			{
				var declaration = Parent.Declaration;
				return Factory.GetCachedValue("BondDesignationCodeList" + declaration.JE_MessageType + declaration.JE_ApplicationCode, delegate
				{
					var result = new BondDesignationCodeList();
					result.RemoveCode(BondDesignationCodeList.Codes.TerminateContinuousBond);
					if (declaration.IsACEDrawback)
					{
						result.RemoveCode(BondDesignationCodeList.Codes.SubstitutionBond);
						result.RemoveCode(BondDesignationCodeList.Codes.SupersedingBond);
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList US_BondDispositionCodeList
		{
			get { return Factory.GetCachedValue<BondDispositionCodeList>(); }
		}

		public CodeDescriptionPairList US_BondTypeList
		{
			get { return Factory.GetCachedValue<BondTypeList>(); }
		}

		public CodeDescriptionPairList AdditionBondTypesList
		{
			get
			{
				return Factory.GetCachedValue("AdditionBondTypesList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(BondTypeList.Codes.SingleTransactionBond, BondTypeList.Descriptions.SingleTransactionBond);
					return result;
				});
			}
		}

		public CodeDescriptionPairList CargoReleaseTypeList
		{
			get
			{
				var declaration = Parent.Declaration;

				return Factory.GetCachedValue("CargoReleaseTypes for " + declaration.JE_ApplicationCode + declaration.US_EnableCRL,
					delegate
					{
						var result = new CodeDescriptionPairList();

						if (declaration.IsACE)
						{
							if (declaration.US_EnableCRL)
							{
								result.AddPair(Enterprise.Customs.US.Business.CargoReleaseTypeList.Codes.SE, Enterprise.Customs.US.Business.CargoReleaseTypeList.Descriptions.SE);
							}
							else
							{
								result.AddPair(Enterprise.Customs.US.Business.CargoReleaseTypeList.Codes.ACE, Enterprise.Customs.US.Business.CargoReleaseTypeList.Descriptions.ACE);
								result.AddPair(Enterprise.Customs.US.Business.CargoReleaseTypeList.Codes.ACS, Enterprise.Customs.US.Business.CargoReleaseTypeList.Descriptions.ACS);
							}
						}
						else
						{
							result.AddPair(Enterprise.Customs.US.Business.CargoReleaseTypeList.Codes.CR, Enterprise.Customs.US.Business.CargoReleaseTypeList.Descriptions.CR);
							result.AddPair(Enterprise.Customs.US.Business.CargoReleaseTypeList.Codes.BCR, Enterprise.Customs.US.Business.CargoReleaseTypeList.Descriptions.BCR);
						}
						return result;
					});
			}
		}

		public EntryTypeList EntryTypeCodeList
		{
			get { return Factory.GetCachedValue<EntryTypeList>(); }
		}

		public ReconIssueCodeList OtherReconIssueList
		{
			get { return OtherReconIssueListCreator.CreateOtherReconIssueList(Factory); }
		}

		public ConsolidatedInformalList US_ConsolidatedInformalList
		{
			get
			{
				return Factory.GetCachedValue(Parent.Declaration.JE_ApplicationCode, delegate
				{
					var result = new ConsolidatedInformalList();

					if (Parent.Declaration.IsACE)
					{
						result.RemoveCode(ConsolidatedInformalList.Codes.Consolidated);
					}

					return result;
				});
			}
		}

		public MonthList US_MonthList
		{
			get { return Factory.GetCachedValue<MonthList>(); }
		}

		public PaymentTypeList US_PaymentTypeList
		{
			get { return Factory.GetCachedValue<PaymentTypeList>(); }
		}

		public CodeDescriptionPairList US_NAFTACountryCodeList
		{
			get
			{
				return Factory.GetCachedValue("US_NAFTACountryCodeList", // 'US_NAFTACountryCodeList' is not a database field
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.CountryCodes.Canada, "Canada");
						result.AddPair(Core.Constants.CountryCodes.Mexico, "Mexico");
						return result;
					}
				);
			}
		}

		public CodeDescriptionPairList US_ClaimPortCodeList
		{
			get
			{
				return Factory.GetCachedValue("US_ClaimPortCodeList", GetUSClaimPortCodeList); // 'US_ClaimPortCodeList' is not a database field
			}
		}

		public static CodeDescriptionPairList GetUSClaimPortCodeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("1001", "New York");
			result.AddPair("3901", "Chicago");
			result.AddPair("5301", "Houston");
			result.AddPair("2809", "San Francisco");
			return result;
		}

		public CodeDescriptionPairList ACEDrawbackProcessingPortCodeList
		{
			get
			{
				return Factory.GetCachedValue("ACEDrawbackProcessingPortCodeList", // 'US_ClaimPortCodeList' is not a database field
				delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(GetUSClaimPortCodeList());
					result.AddPair("9900", "National Finance Center");
					return result;
				});
			}
		}

		public CodeDescriptionPairList US_TeamNoForDrawbackCodeList
		{
			get
			{
				return Factory.GetCachedValue("US_TeamNoForDrawbackCodeList", GetUSTeamNoForDrawbackCodeList); // 'US_TeamNoForDrawbackCodeList' is not a database field
			}
		}

		public static CodeDescriptionPairList GetUSTeamNoForDrawbackCodeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("2DB", "1001 - New York");
			result.AddPair("3DR", "3901 - Chicago");
			result.AddPair("6D0", "5301 - Houston");
			result.AddPair("7D7", "2809 - San Francisco");
			return result;
		}

		public Dictionary<ZString, ZString> ValidClaimPortTeamNos
		{
			get
			{
				return Factory.GetCachedValue("ValidClaimPortTeamNos", // 'ValidClaimPortTeamNos' is not a database field
					delegate
					{
						Dictionary<ZString, ZString> result = new Dictionary<ZString, ZString>();
						result.Add("1001", "2DB");
						result.Add("3901", "3DR");
						result.Add("5301", "6D0");
						result.Add("2809", "7D7");
						return result;
					}
				);
			}
		}

		public CodeDescriptionPairList US_DRWFilingMethodCodeList
		{
			get { return Factory.GetCachedValue<DrawbackMethodOfFilingList>(); }
		}

		public CodeDescriptionPairList US_DRWPurposeCodeList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("US_DRWPurposeCodeList", () => new DrawbackDeclarationPurposeList());
			}
		}

		public IBusinessObjectCollection DestinationSchDList
		{
			get
			{
				IBusinessObjectCollection result;
				var declaration = Parent.Declaration;
				if (declaration.IsImport)
				{
					result = RegionDistrictPorts;
				}
				else if (declaration.IsExport)
				{
					result = ForeignPortsForAES;
				}
				else
				{
					result = ForeignPorts;
				}

				return result;
			}
		}

		public IBusinessObjectCollection PortOfEntrySchDList
		{
			get { return RegionDistrictPorts; }
		}

		public WarehouseClientCollection LocationOfGoodsList
		{
			get { return new WarehouseClientCollection(Factory); }
		}

		public BusinessObjectCollection OriginSchDList
		{
			get { return Parent.Declaration.IsExport ? RegionDistrictPorts : ForeignPorts; }
		}

		public BusinessObjectCollection DischargeSchDList
		{
			get
			{
				BusinessObjectCollection result;
				var declaration = Parent.Declaration;
				if (declaration.US_SchDArrivalTypeIsDropEdit)
				{
					result = declaration.PortOfArrivalRefLocoMappings;
				}
				else if (declaration.IsImport)
				{
					result = RegionDistrictPorts;
				}
				else if (declaration.IsExport)
				{
					result = declaration.IsUSTerritoryTreatedAsDomesticState ? USDischargePortsForUSTerritory : ForeignPortsForAES;
				}
				else
				{
					result = ForeignPorts;
				}

				return result;
			}
		}

		public BusinessObjectCollection SchDExportList
		{
			get
			{
				var declaration = Parent.Declaration;
				BusinessObjectCollection result;
				if (declaration.IsExport)
				{
					result = declaration.US_SchDExportTypeIsDropEdit ? declaration.PortOfExportRefLocoMappings : ExportRegionDistrictPorts;
				}
				else
				{
					result = declaration.US_SchDLoadingTypeIsDropEdit ? declaration.PortOfLadingMappings : ForeignPorts;
				}

				return result;
			}
		}

		public BusinessObjectCollection SchKList
		{
			get { return Parent.Declaration.IsExport ? ForeignPortsForAES : (BusinessObjectCollection)ForeignPorts; }
		}

		public CodeDescriptionPairList SpecialKList
		{
			get { return Factory.GetCachedValue<SpecialKCodes>(); }
		}

		public IBusinessObjectCollection FinalDestinations
		{
			get
			{
				IBusinessObjectCollection result;
				var declaration = Parent.Declaration;
				if (declaration.IsImport)
				{
					result = RegionDistrictPorts;
				}
				else if (declaration.IsExport)
				{
					result = ForeignPortsForAES;
				}
				else
				{
					result = ForeignPorts;
				}

				return result;
			}
		}

		public override RefUNLOCOCollection PortOfExports
		{
			get { return USUnLocoPorts; }
		}

		RefUNLOCOCollection USUnLocoPorts
		{
			get { return new RefUNLOCOCollection(Factory, new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, new[] { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.VirginIslands })); }
		}

		public MissingDocumentList MissingDocumentList
		{
			get { return new MissingDocumentList(); }
		}

		public CodeDescriptionPairList EntryDateElectionCodeList
		{
			get
			{
				var declaration = Parent.Declaration;

				return Factory.GetCachedValue("EntryDateElectionCodeList" + declaration.US_EntryType + declaration.JE_ApplicationCode + (declaration.IsACECargoCertificationMode ? "ACE" : ""), // 'EntryDateElectionCodeList' cached
					delegate
					{
						var result = new CodeDescriptionPairList();
						if (declaration.IsACE && declaration.IsConsumptionFTZ)
						{
							if (declaration.IsACECargoCertificationMode)
							{
								result.AddPair(USEntryDateElectionCodeList.Codes.NonWeeklyEstimateFilingDate, USEntryDateElectionCodeList.Descriptions.NonWeeklyEstimateFilingDate);
							}
							result.AddPair(USEntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate, USEntryDateElectionCodeList.Descriptions.WeeklyEstimateFilingDate);
						}
						else
						{
							result.AddPair(USEntryDateElectionCodeList.Codes.ArrivalDate, USEntryDateElectionCodeList.Descriptions.ArrivalDate);
							result.AddPair(USEntryDateElectionCodeList.Codes.PresentationDate, USEntryDateElectionCodeList.Descriptions.PresentationDate);
						}
						return result;
					}
				);
			}
		}

		public OrgHeaderCollection ImporterOfRecordList
		{
			get { return importerOfRecordList ?? (importerOfRecordList = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection importerOfRecordList;

		public TariffTypeList US_TariffTypeList
		{
			get { return Factory.GetCachedValue<TariffTypeList>(); }
		}

		public EntryModeList EntryModes
		{
			get
			{
				return Parent.Factory.GetCachedValue("EntryModes" + Parent.Declaration.US_EntryDate.ToShortDateString(), delegate
				{
					return EntryModeList.GetRelevantListFor(Parent.Declaration.US_EntryDate);
				});
			}
		}

		public AESCommodityFilingOptionList US_CommodityFilingOptions
		{
			get { return Factory.GetCachedValue<AESCommodityFilingOptionList>(); }
		}

		public CRLReleaseStatusList ReleaseStatusList
		{
			get { return Factory.GetCachedValue<CRLReleaseStatusList>(); }
		}

		public BondWaiverReasonCodeList BondWaiverReasonCodes
		{
			get
			{
				return Factory.GetCachedValue("BondWaiverReasonCode for " + Parent.Declaration.US_BondType,
					delegate
					{
						BondWaiverReasonCodeList result = new BondWaiverReasonCodeList();

						if (Parent.Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
						{
							result.RemoveCode(BondWaiverReasonCodeList.Codes._995);
							result.RemoveCode(BondWaiverReasonCodeList.Codes._996);
							result.RemoveCode(BondWaiverReasonCodeList.Codes._997);
						}

						return result;
					});
			}
		}

		public ZZRefCusCodeListCombinedCollection ExportRegionDistrictPorts
		{
			get
			{
				return Factory.GetCachedValue("ExportRegionDistrictPorts", () =>
				{
					var exportRegionDistrictPorts = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
					exportRegionDistrictPorts.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(Core.Constants.CountryCodes.UnitedStates)));
					exportRegionDistrictPorts.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName,
						"Property", new ZString(RefCusCodeListAttributeTypes.Codes.ROLE)));
					exportRegionDistrictPorts.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", new ZString("EXP")));
					return exportRegionDistrictPorts;
				});
			}
		}

		public CodeDescriptionPairList US_FTZAdmissionTypeList
		{
			get { return Factory.GetCachedValue<FTZAdmissionTypeCodeList>(); }
		}

		public CodeDescriptionPairList FTZDeliveryCodeList
		{
			get { return Factory.GetCachedValue<FTZDeliveryCodeList>(); }
		}

		public CodeDescriptionPairList US_SplitShipmentReleaseCodeList
		{
			get { return Factory.GetCachedValue<SplitShipmentReleaseCodeList>(); }
		}

		public CodeDescriptionPairList PriorNoticeModeCodeList
		{
			get { return Factory.GetCachedValue<PriorNoticeModeCodeList>(); }
		}

		public CodeDescriptionPairList DestructionResultCodesList
		{
			get { return Factory.GetCachedValue<DrawbackDestructionResultCodes>(); }
		}

		public CodeDescriptionPairList InsuranceAgentList => GlbExternalPasswordHelper.GetInsuranceAgents(Factory);

		public CodeDescriptionPairList InsuranceDispositionList => Factory.GetCachedValue<InsuranceDispositionCodeList>();

		[System.Xml.Serialization.XmlIgnore]
		public USAESLicenseCodeCollection US_LicenseType_List
		{
			get
			{
				if (uS_LicenseType_List == null)
				{
					uS_LicenseType_List = new USAESLicenseCodeCollection(Factory);
				}

				uS_LicenseType_List.FilterBusinessObjectDefaults.RemoveAll();
				uS_LicenseType_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", Declaration.GetEffectiveDateForECR()));
				return uS_LicenseType_List;
			}
		}
		USAESLicenseCodeCollection uS_LicenseType_List;

		public USAESECCNNumberCollection US_ECCNList
		{
			get
			{
				return Factory.GetCachedValue($"ECCNList|{Declaration.US_LicenseType}|{ZDateTime.Today.ToISO8601ShortDateString()}", delegate
				{
					var uS_ECCNList = new USAESECCNNumberCollection(Parent.Factory, Declaration.US_LicenseType);
					uS_ECCNList.FilterBusinessObjectDefaults.RemoveAll();
					uS_ECCNList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", ZDateTime.Today));
					return uS_ECCNList;
				});
			}
		}
	}
}
