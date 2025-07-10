using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get { return Parent; }
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		public CodeDescriptionPairList RejectedMerchandiseReasonList
		{
			get { return Factory.GetCachedValue<DrawbackRejectedMerchandiseReasonList>(); }
		}

		public override CodeDescriptionPairList TransportTypeList
		{
			get
			{
				CodeDescriptionPairList result1;
				if (Declaration.IsInBondOnly)
				{
					result1 = Factory.GetCachedValue("USInBondTransportTypeList",
						() =>
						{
							var result = new CodeDescriptionPairList();
							result.AddPair(Business.TransportTypeList.Codes.Sea, Business.TransportTypeList.Descriptions.Sea);
							result.AddPair(Business.TransportTypeList.Codes.Rail, Business.TransportTypeList.Descriptions.Rail);
							result.AddPair(Business.TransportTypeList.Codes.Truck, Business.TransportTypeList.Descriptions.Truck);
							result.AddPair(Business.TransportTypeList.Codes.Air, Business.TransportTypeList.Descriptions.Air);
							return result;
						});
				}
				else
				{
					result1 = Factory.GetCachedValue<TransportTypeList>();
				}

				return result1;
			}
		}

		protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
		{
			yield return JobMessageTypeList.Codes.Recon;
			yield return JobMessageTypeList.Codes.Drawback;
		}

		public override CodeDescriptionPairList JE_TotalNoOfPacksPackType_List
		{
			get { return Factory.GetCachedValue<ShippingOrPackingingUnitList>(); }
		}

		public override CodeDescriptionPairList CargoIdTypeList
		{
			get { return Factory.GetCachedValue<ContainerModeList>(); }
		}

		public override CodeDescriptionPairList EntryStatusList
		{
			get
			{
				if (Declaration.IsExport)
				{
					return Factory.GetCachedValue<AESDirectCustomsEntryStatus>();
				}
				else if (Declaration.IsDrawback)
				{
					return Factory.GetCachedValue<DrawbackSummaryStatusList>();
				}
				else if (Declaration.IsProtest)
				{
					return Factory.GetCachedValue<ProtestStatusCodesList>();
				}
				else
				{
					return Factory.GetCachedValue<ImportEntryStatusList>();
				}
			}
		}

		protected override ZQuery OriginPortFilter()
		{
			if (Declaration.IsExport && Declaration.IsUSTerritoryTreatedAsDomesticState)
			{
				ZQuery result = PortQuery("", PortLocation.All);
				result.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, new[] { Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.VirginIslands });
				return result;
			}

			return base.OriginPortFilter();
		}

		protected override ZQuery DischargePortFilter()
		{
			if (Declaration.IsExport && Declaration.IsUSTerritoryTreatedAsDomesticState)
			{
				return PortQuery("", PortLocation.All);
			}
			else
			{
				return base.DischargePortFilter();
			}
		}

		protected override ZQuery DestinationPortFilter()
		{
			if (Declaration.IsExport && Declaration.IsUSTerritoryTreatedAsDomesticState)
			{
				return PortQuery("", PortLocation.All);
			}
			else
			{
				return base.DestinationPortFilter();
			}
		}

		protected override ZQuery FinalDestinationPortFilter()
		{
			if (Declaration.IsExport && Declaration.IsUSTerritoryTreatedAsDomesticState)
			{
				var result = PortQuery("", PortLocation.All);
				result.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.VirginIslands });
				return result;
			}

			return base.FinalDestinationPortFilter();
		}

		public override RefUNLOCOCollection PortOfLoadings
		{
			get
			{
				if (Declaration.IsExport && Declaration.IsUSTerritoryTreatedAsDomesticState)
				{
					return Origins;
				}
				else
				{
					return base.PortOfLoadings;
				}
			}
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get
			{
				if (Declaration.IsExport)
				{
					return Factory.GetCachedValue<AESDirectCustomsEntryStatus>();
				}
				else if (Declaration.IsDrawback)
				{
					return Factory.GetCachedValue<DrawbackSummaryStatusList>();
				}
				else if (Declaration.IsRecon)
				{
					return Factory.GetCachedValue<ReconMessageStatusList>();
				}
				else if (Declaration.IsProtest)
				{
					return Factory.GetCachedValue<ProtestMessageStatusList>();
				}
				else if (Declaration.IsFTZAdmission)
				{
					return Factory.GetCachedValue<FTZMessageStatusList>();
				}
				else
				{
					return Factory.GetCachedValue<ImportMessageStatusList>();
				}
			}
		}

		public CodeDescriptionPairList BLUStatusList
		{
			get { return Factory.GetCachedValue<MessageStatusListBLU>(); }
		}

		public CodeDescriptionPairList FDAMsgStatusList
		{
			get { return Factory.GetCachedValue<FDAStatusList>(); }
		}

		public CodeDescriptionPairList ElectronicInvoiceStatusList
		{
			get { return Factory.GetCachedValue<MessageStatusListEI>(); }
		}

		public CodeDescriptionPairList USStatesList
		{
			get { return Factory.GetCachedValue<USStatesList>(); }
		}

		public USCarrierCombinedCollection USCarrierList
		{
			get { return new USCarrierCombinedCollection(Factory); }
		}

		public JobDeclarationCollection JobDeclarationList
		{
			get { return new JobDeclarationCollection(Factory, Declaration.Company.PK); }
		}

		public CodeDescriptionPairList ITStatusList
		{
			get { return Factory.GetCachedValue<MessageStatusListIT>(); }
		}

		public CusStatementHeaderCollection StatementList
		{
			get { return new CusStatementHeaderCollection(Factory); }
		}

		public CodeDescriptionPairList StatementStatusList
		{
			get { return Factory.GetCachedValue<StatementHeaderStatusList>(); }
		}

		public CodeDescriptionPairList PaymentStatusList
		{
			get { return Factory.GetCachedValue<PaymentStatusList>(); }
		}

		public override CodeDescriptionPairList ApplicationCodeList
		{
			get
			{
				var submissionType = Declaration.GetInterfaceSubmissionType();
				return Factory.GetCachedValue<CodeDescriptionPairList>(Declaration.JE_MessageType + submissionType, delegate
				{
					if (Declaration.IsExport || Declaration.IsFTZAdmission)
					{
						return Factory.GetCachedValue<DeclarationApplicationCodeList>();
					}
					var applicationCodeList = new JobApplicationCodeList();
					if (Declaration.IsFormalImport && !(submissionType.IsEmpty || submissionType == DeclarationApplicationCodeList.Codes.Builtin))
					{
						applicationCodeList.AddPair(DeclarationApplicationCodeList.Codes.Interfaced, DeclarationApplicationCodeList.Descriptions.Interfaced);
					}
					return applicationCodeList;
				});
			}
		}

		public CodeDescriptionPairList ZoneIDList
		{
			get
			{
				// Get zone IDs of fountains of type "FTZ" from the IOR, or if there's no IOR or the IOR has no such fountains, 
				// then try getting FTW fountains' zoneIDs from the Applicant. 
				CodeDescriptionPairList zonesCdpl = null;
				var iorFountains = Declaration.IOR?.OrgFountains; //.Where(stmNum=>stmNum.IsFTZNonWarehouseType);
				if (iorFountains != null && iorFountains.Count > 0)
				{
					zonesCdpl = iorFountains.GetFTZPrefixList(OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber, OrganisationViewStmNums.Schema.SN_ZoneIDPrefixMaxLength);
				}
				if (zonesCdpl == null)
				{
					var ftzFountain = Declaration.WarehouseDocAddress?.Address?.Header?.OrgFountains;
					if (ftzFountain != null && ftzFountain.Count > 0)
					{
						zonesCdpl = ftzFountain.GetFTZPrefixList(OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse, OrganisationViewStmNums.Schema.SN_ZoneIDPrefixMaxLength);
					}
				}
				if (zonesCdpl == null)
				{
					zonesCdpl = new CodeDescriptionPairList();
				}
				return zonesCdpl;
			}
		}

		public CodeDescriptionPairList DispositionCodeList
		{
			get { return PGADispositionCodeList.GetPGADispositionCodeList(Factory); }
		}

		public OrganisationsFindBoxCollection Organizations
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		public CommercialRulingCodeList CommercialRulingCodeList
		{
			get
			{
				return Factory.GetCachedValue("CommercialRulingCodeList", delegate
				{
					var result = new CommercialRulingCodeList();
					result.Sort();
					return result;
				});
			}
		}

		public CRLReleaseStatusList ReleaseStatusList
		{
			get { return Factory.GetCachedValue<CRLReleaseStatusList>(); }
		}

		public CodeDescriptionPairList SPNIDTypeList
		{
			get
			{
				return Factory.GetCachedValue(Declaration.JE_MessageType + "SPNIDTypeList", delegate
				{
					var result = new CodeDescriptionPairList();

					if (Declaration.IsImport)
					{
						result.AddPair(StandAlonePriorNoticeIDTypeList.Codes.BLN, StandAlonePriorNoticeIDTypeList.Descriptions.BLN);

						if (Declaration.IsFTZAdmission)
						{
							result.AddPair(StandAlonePriorNoticeIDTypeList.Codes.FTZ, StandAlonePriorNoticeIDTypeList.Descriptions.FTZ);
						}
						else
						{
							result.AddPair(StandAlonePriorNoticeIDTypeList.Codes.ENT, StandAlonePriorNoticeIDTypeList.Descriptions.ENT);
						}
					}

					return result;
				});
			}
		}

		public CodeDescriptionPairList CargoManifestStatusDispositionList
		{
			get { return UniversalReferenceDataHelper.GetDispositionCodeDescriptionList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode); }
		}

		public override OrgHeaderCollection BuyingAgents
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public override OrgHeaderCollection SellingAgents
		{
			get { return new ConsignorCollection(Factory); }
		}
	}
}
