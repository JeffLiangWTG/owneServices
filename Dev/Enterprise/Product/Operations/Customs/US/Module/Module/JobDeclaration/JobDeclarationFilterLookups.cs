using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public CodeDescriptionPairList ReconIssueList => OtherReconIssueListCreator.CreateOtherReconIssueList(Factory);

		public ReconIssueCodeList IssueCodeList => Factory.GetCachedValue<ReconIssueCodeList>();

		public ZZRefCusCodeListCombinedCollection RegionalPorts => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
		public FDAStatusList FDAMsgStatusList => Factory.GetCachedValue<FDAStatusList>();

		public CodeDescriptionPairList ReleaseStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("USJobDeclarationFilterLookups|ReleaseStatusList", delegate
				{
					var result = new CRLReleaseStatusList();
					result.RemoveCode(CRLReleaseStatusList.Codes.HLD);
					return result;
				});
			}
		}

		public FDAEntryLevelDispositionCodeList FDAStatusList => Factory.GetCachedValue<FDAEntryLevelDispositionCodeList>();

		public CodeDescriptionPairList StatusDispositionList => ENSStatusDispositionCodeListLoader.GetENSStatusDispositionCodeList(Factory);

		public Customs.Business.YesNoList YesNoList => Factory.GetCachedValue<Customs.Business.YesNoList>();

		public JobApplicationCodeList JobApplicationCodeList => Factory.GetCachedValue<JobApplicationCodeList>();

		public JobHeaderStatusList JobHeaderStatusList => Factory.GetCachedValue<JobHeaderStatusList>();

		public BillIssuerOrganisationFindBoxCollection Carriers => new BillIssuerOrganisationFindBoxCollection(Factory);

		public EntryModeList EntryModes => Factory.GetCachedValue<EntryModeList>();

		public CargoReleaseTypeList CargoReleaseTypes => Factory.GetCachedValue<CargoReleaseTypeList>();

		internal CodeDescriptionPairList MessageStatus_Export_List
		{
			get
			{
				return Factory.GetCachedValue("ExportStatusListForFilter", delegate
				{
					var result = new AESDirectCustomsEntryStatus();

					result.RemoveCode(AESDirectCustomsEntryStatus.Codes.NotSent);
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList MessageStatusListForBLU => Factory.GetCachedValue<MessageStatusListBLU>();

		public CodeDescriptionPairList MessageStatusListForCRL
		{
			get
			{
				return Factory.GetCachedValue("CargoReleaseStatusListForFilter", delegate
				{
					var result = new MessageStatusListCRL();

					result.RemoveCode(MessageStatusListCRL.Codes.NotSent);
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList MessageStatusListForENS
		{
			get
			{
				return Factory.GetCachedValue("EntrySummaryStatusListForFilter", delegate
				{
					var result = new MessageStatusListENS();

					result.RemoveCode(MessageStatusListENS.Codes.NotSent);
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);
					result.AddPair(ImportMessageStatusList.Codes.EntrySummaryCanceled, ImportMessageStatusList.Descriptions.EntrySummaryCanceled);

					return result;
				});
			}
		}

		public CodeDescriptionPairList MessageStatusListForINB
		{
			get
			{
				return Factory.GetCachedValue("InBondStatusListForFilter", delegate
				{
					var result = new MessageStatusListIT();

					result.RemoveCode(MessageStatusListIT.Codes.NotSent);
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList FTZAdmissionStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("FTZAdmissionMessageStatusList", delegate
				{
					var result = new FTZAdmissionMessageStatusList();
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList FTZConcurrenceStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("FTZConcurrenceMessageStatusList", delegate
				{
					var result = new FTZConcurrenceMessageStatusList();
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList FTZDeliveryOfGoodsStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("FTZDeliveryOfGoodsMessageStatusList", delegate
				{
					var result = new FTZDeliveryOfGoodsMessageStatusList();
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList DISStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("DISStatusList", delegate
				{
					var result = new Common.US.DIS.StatusList();
					result.RemoveCode(Common.US.DIS.StatusList.Codes.MUL);
					return result;
				});
			}
		}

		public CodeDescriptionPairList FTZGoodsArrivalStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("FTZGoodsArrivalMessageStatusList", delegate
				{
					var result = new FTZGoodsArrivalMessageStatusList();
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList FTZPTTStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("FTZPTTMessageStatusList", delegate
				{
					var result = new FTZPTTMessageStatusList();
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList PGAStatus
		{
			get
			{
				return Factory.GetCachedValue("PGAStatus", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(DeclarationFilterConstants.FilterCodeAndDesc.ALLMayProceedManuallyClosed, DeclarationFilterConstants.FilterCodeAndDesc.ALLMayProceedManuallyClosed);
					result.AddPair(DeclarationFilterConstants.FilterCodeAndDesc.HasAnyPGAStatus, DeclarationFilterConstants.FilterCodeAndDesc.HasAnyPGAStatus);
					result.AddPair(DeclarationFilterConstants.FilterCodeAndDesc.HasNoPGAStatus, DeclarationFilterConstants.FilterCodeAndDesc.HasNoPGAStatus);
					result.AddPair(DeclarationFilterConstants.FilterCodeAndDesc.HasAnyManuallyClosed, DeclarationFilterConstants.FilterCodeAndDesc.HasAnyManuallyClosed);
					result.AddPair(DeclarationFilterConstants.FilterCodeAndDesc.HoldIntact, DeclarationFilterConstants.FilterCodeAndDesc.HoldIntact);
					result.AddPair(DeclarationFilterConstants.FilterCodeAndDesc.MayNotProceed, DeclarationFilterConstants.FilterCodeAndDesc.MayNotProceed);
					return result;
				});
			}
		}

		public List<ZZRefCusCodeListCombined> AESSeverityList => Factory.GetCachedValue("AESSeverityList", () =>
		{
			return ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESSeverityIndicator, ZDateTime.Today)
				.OrderBy(i => i.ZZD_Code == DeclarationFilterConstants.FilterCodeAndDesc.Space)
				.ThenBy(i => i.ZZD_Code)
				.ToList();
		});

		public ZZRefCusCodeListCombinedCollection AESResponseCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESResponseCode, ZDateTime.Today);

		public override CodeDescriptionPairList ApplicationCodeList() => Factory.GetCachedValue("USDeclarationApplicationCodeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(JobApplicationCodeList.Codes.ACE, JobApplicationCodeList.Descriptions.ACE);
			result.AddPair(JobApplicationCodeList.Codes.ACS, JobApplicationCodeList.Descriptions.ACS);
			result.AddRange(base.ApplicationCodeList());
			return result;
		});

		public override CodeDescriptionPairList EntryTypeList => Factory.GetCachedValue("EntryTypeListForModuleFilterBizObj", () => Business.EntryTypeList.GetACEList());

		public CodeDescriptionPairList TaxDeferIndicatorList => Factory.GetCachedValue<TaxDeferIndicatorList>();

		public OrgHeaderCollection LocationOfGoodsList => new BondedWarehouseCollection(Factory);

		public CodeDescriptionPairList ElectronicInvoiceStatusListForFilter
		{
			get
			{
				return Factory.GetCachedValue("ElectronicInvoiceStatusListForFilter", delegate
				{
					var result = new MessageStatusListEI();

					result.RemoveCode(MessageStatusListEI.Codes.Multiple);
					result.RemoveCode(MessageStatusListEI.Codes.NotSent);

					result.AddPair(MessageStatusListEI.Codes.Multiple, "Multiple Invoices with differing EI Status");
					result.AddPair(DeclarationFilterConstants.MessageStatus.NotSentForFilter, DeclarationFilterConstants.MessageStatus.NotSentForFilterDescription);

					return result;
				});
			}
		}

		public CodeDescriptionPairList StatementStatusList => Factory.GetCachedValue<StatementHeaderStatusList>();

		public CodeDescriptionPairList PaymentStatusList => Factory.GetCachedValue<PaymentStatusList>();

		public PaymentTypeList PaymentTypeList
		{
			get
			{
				return Factory.GetCachedValue("PaymentTypeList",
					delegate
					{
						var result = new PaymentTypeList();
						result.Sort();
						return result;
					}
				);
			}
		}

		public InbondCommonTypeList InbondCommonTypeList => Factory.GetCachedValue<InbondCommonTypeList>();

		protected new JobDeclarationFilterBusinessObject FilterBizObj => (JobDeclarationFilterBusinessObject)base.FilterBizObj;

		public Common.US.ISF.DispositionCodeList ISFBillStatusList
		{
			get
			{
				return Factory.GetCachedValue("DeclarationISFUSISF.DispositionCodeListWithMultiple", delegate
				{
					var result = new Common.US.ISF.DispositionCodeList();
					result.AddPair(Common.US.ISF.ISFStatusHelper.Multiple, "Multiple bills have different statuses.");
					return result;
				});
			}
		}

		public CodeDescriptionPairList SEBillStatusList
		{
			get
			{
				return UniversalReferenceDataHelper.GetDispositionCodeDescriptionList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode);
			}
		}

		public HLDOrEXMStatusList HLDOrEXMStatusList
		{
			get
			{
				return Factory.GetCachedValue<HLDOrEXMStatusList>();
			}
		}
		public CodeDescriptionPairList EntrySummaryActionsList
		{
			get
			{
				return Factory.GetCachedValue("EntrySummaryActionsList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(DeclarationFilterConstants.ALL, "All Records");
					result.AddPair(DeclarationFilterConstants.Incomplete, "All with Action Incomplete");
					return result;
				});
			}
		}

		public OrgHeaderCollection NotifyParties => new OrgHeaderCollection(Factory);

		public OrgHeaderCollection SoldToParties => new OrgHeaderCollection(Factory);

		public AESCommodityFilingOptionList FilingOptionList => Factory.GetCachedValue<AESCommodityFilingOptionList>();

		public CodeDescriptionPairList SPIList => SPICompleteList.GetCachedList(Factory);

		public CodeDescriptionPairList BondDispositionCodeList => Factory.GetCachedValue<BondDispositionCodeList>();

		public CodeDescriptionPairList InsuranceDispositionCodeList => Factory.GetCachedValue<InsuranceDispositionCodeList>();
	}
}
