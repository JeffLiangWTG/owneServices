using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using EntryChargeTypeList = Enterprise.Registry.Business.Customs.US.EntryChargeTypeList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using static Enterprise.Integration.Customs.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[TestExcludeWorkflowProviderHasTestCase]
	[System.CodeDom.Compiler.GeneratedCode("CargoWise.EntityFramework", "1.0"), IsCancelledProperty(JobDeclarationSchema.Constants.JE_IsCancelled), PreventDelete(true)]
	[CodeProperty(JobDeclaration.Schema.JE_DeclarationReference), DescriptionProperty(JobDeclaration.Schema.Description)]
	[UserDefinedValues]
	public class ReconDeclaration : NonPersistentBusinessObjectWithLogsAndNotes,
		IReconDeclaration,
		IObsoleteValidation,
		IInvoicesProvider,
		ICustomLabelsConfigOrgProvider,
		IDutyDataLineHeaderProvider,
		IPrelimStatementDetailsDefault,
		IClientBranchDesignationDefault,
		IMessageAttachee,
		INeedDataSet,
		ICustomsJobInfoProvider,
		IServiceLocator,
		IStatementDeleteTransaction,
		IBondDetailsDefault,
		IWorkflowProvider,
		IEDocsProvider,
		IDocsAndCartageParent,
		IEDocsPluginHostDecider,
		IAllocateNumberSupporter,
		IReconOriginalChargeParent,
		ICancellable,
		IStatementLineDeclaration,
		IRatingSupporterWithAdapter,
		ICustomFieldProvider,
		IWrapPersistentBizO
	{
		#region Constants
		public class Constants
		{
			public const string DutyAccountingClassCode = "001";
			public const string InterestAccountingClassCode = "044";
		}

		#endregion
		public class Schema
		{
			public const string MessageStatus = "MessageStatus";
			public const string MessageStatusDescription = "MessageStatusDescription";
			public const string PreparerName = "PreparerName";
			public const string ReconEntryNumber = "ReconEntryNumber";
			public const string ReconEntryNumberWithEntryFilerCode = "ReconEntryNumberWithEntryFilerCode";
			public const string US_PaymentDate = USAddInfoSchema.Constants.US_PaymentDate;
			public const string TotalOriginalDuty = "TotalOriginalDuty";
			public const string TotalReconDuty = "TotalReconDuty";
			public const string TotalDutyDifference = "TotalDutyDifference";
			public const string TotalOriginalFee = "TotalOriginalFee";
			public const string TotalReconFee = "TotalReconFee";
			public const string TotalFeeDifference = "TotalFeeDifference";
			public const string TotalOriginalTax = "TotalOriginalTax";
			public const string TotalReconTax = "TotalReconTax";
			public const string TotalTaxDifference = "TotalTaxDifference";
			public const string InterestPaymentAmount = "InterestPaymentAmount";
			public const string US_DocProvidedDate = USAddInfoSchema.Constants.US_DocProvidedDate;
			public const string US_ClaimDate = USAddInfoSchema.Constants.US_ClaimDate;
			public const string US_ClaimID = USAddInfoSchema.Constants.US_ClaimID;
			public const string US_AnticipatedLiquidationDate = JobDeclaration.Schema.US_AnticipatedLiquidationDate;
			public const string US_LiquidationDate = JobDeclaration.Schema.LiquidationDate;

			public const string TableName = JobDeclaration.Schema.TableName;
		}

		public static ReconDeclaration Get(JobDeclaration reconWrappedJobDeclaration)
		{
			return reconWrappedJobDeclaration.ReconDeclaration ?? new ReconDeclaration(reconWrappedJobDeclaration);
		}

		public ReconDeclaration(JobDeclaration reconWrappedJobDeclaration)
			: base(reconWrappedJobDeclaration.Factory, ((INeedRow)reconWrappedJobDeclaration).Row)
		{
			this.ReconWrappedJobDeclaration = reconWrappedJobDeclaration;
			reconWrappedJobDeclaration.ReconDeclaration = this;
			RegisterEditableChildObject(reconWrappedJobDeclaration);

			//note: This can't be in SetDefaultValues because being backed by a persistent BizO means you have a non-Empty PK from the get go
			if (!reconWrappedJobDeclaration.IsInDatabase)
			{
				using (SuspendSettingHasChanges())
				{
					CusEntryHeader entry = reconWrappedJobDeclaration.ActiveEntryHeaders.AddNew();

					using (entry.SuspendSettingHasChanges())
					{
						entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
					}

					reconWrappedJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
					SetDefaults();
				}
			}
		}

		public readonly JobDeclaration ReconWrappedJobDeclaration;

		BusinessObject IWrapPersistentBizO.Parent => ReconWrappedJobDeclaration;

		public ZDateTime LiquidationDate
		{
			get { return ReconWrappedJobDeclaration.LiquidationDate; }
		}

		public ErrorsRecordCollection ENSStatusNotifications
		{
			get { return ReconWrappedJobDeclaration.ENSStatusNotifications; }
		}

		public ZString ENSStatusNotificationsReqFurtherActions
		{
			get { return ReconWrappedJobDeclaration.ENSStatusNotificationsReqFurtherActions; }
		}

		/// <summary>
		/// Factory to load underlying import entry details
		/// </summary>
		public BusinessObjectFactory ReadFactory
		{
			get
			{
				if (readFactory == null)
				{
					readFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				}
				return readFactory;
			}
		}
		BusinessObjectFactory readFactory;

		public void ReleaseReadFactory()
		{
			readFactory = null;
		}

		#region Methods

		public void CalculateDutyFeesForChangedEntries()
		{
			ReconDutyFeeCalculationManager.CalculateOnlyChangedEntries();
		}

		public void CalculateDutyFeesForAllEntries()
		{
			ReconDutyFeeCalculationManager.CalculateAll();
		}

		ReconDutyFeeCalculationManager ReconDutyFeeCalculationManager
		{
			get { return reconDutyFeeCalculationManager ?? (reconDutyFeeCalculationManager = new ReconDutyFeeCalculationManager(this)); }
		}
		ReconDutyFeeCalculationManager reconDutyFeeCalculationManager;

		public void CalculateCustomsValues()
		{
			ReconCustomsValueCalculationManager.CalculateCustomsValuesForAllEntryLines();
		}

		public ReconCustomsValueCalculationManager ReconCustomsValueCalculationManager
		{
			get { return reconCustomsValueCalculationManager ?? (reconCustomsValueCalculationManager = new ReconCustomsValueCalculationManager(this)); }
		}
		ReconCustomsValueCalculationManager reconCustomsValueCalculationManager;

		#endregion

		#region Properties

		public ZGuid JE_PK
		{
			get { return ReconWrappedJobDeclaration.PK; }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.BranchList))]
		public ZGuid JE_GB
		{
			get { return ReconWrappedJobDeclaration.JE_GB; }
			set
			{
				ReconWrappedJobDeclaration.JE_GB = value;
			}
		}

		void SetDefaults()
		{
			if (!GlbStaff.CurrentUser.GS_IsSystemAccount && !USCustomsDataRegistry.Instance.EntryDeclarant.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
			{
				JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			}
			US_ImportEntrySource = ReconciliationImportEntrySourceList.Codes.FiftyStates;
		}

		public GlbBranch Branch
		{
			get { return Factory.Load<GlbBranch>(JE_GB); }
		}

		public ZPropertyInfo JE_GBInfo
		{
			get { return ReconWrappedJobDeclaration == null ? null : GetWrappedZPropertyInfo(JobDeclarationSchema.Constants.JE_GB, x => ReconWrappedJobDeclaration.JE_GBInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.ApplicationCodeList))]
		public ZString JE_ApplicationCode
		{
			get { return ReconWrappedJobDeclaration.JE_ApplicationCode; }
			set
			{
				var oldValue = JE_ApplicationCode;
				if (oldValue != value)
				{
					ReconWrappedJobDeclaration.JE_ApplicationCode = value;
					ClearDataForApplicationCodeChange();
					JE_ApplicationCodeInfo.RefreshBinding();
				}
			}
		}

		void ClearDataForApplicationCodeChange()
		{
			if (IsACE)
			{
				AggregateRefundedFees.RemoveAndDeleteAll();
				US_TeamNo = ZString.Empty;
				foreach (ReconOriginalEntryHeader origEntry in OriginalEntries)
				{
					origEntry.RefundedFees.RemoveAndDeleteAll();
				}
			}
			else
			{
				foreach (ReconOriginalEntryHeader origEntry in OriginalEntries)
				{
					origEntry.US_PriorDisclosure = false;
					origEntry.US_NAFTAClaimStat = false;
					origEntry.US_ProtestStat = false;
					origEntry.US_ProtestID = ZString.Empty;
					origEntry.US_PendingActionID = ZString.Empty;
					origEntry.US_PendingActionIDType = ZString.Empty;
				}
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					invoiceLine.ReconRefundedFees.RemoveAndDeleteAll();
					invoiceLine.US_R_HTSChanged4ValueInd = false;
					invoiceLine.US_R_ReconReasonText = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo JE_ApplicationCodeInfo
		{
			get { return ReconWrappedJobDeclaration == null ? null : GetWrappedZPropertyInfo(JobDeclarationSchema.Constants.JE_ApplicationCode, x => ReconWrappedJobDeclaration.JE_ApplicationCodeInfo); }
		}

		ZBool IsRemoteLocationFiling
		{
			get
			{
				var isRemoteLocationFilingSent = false;
				var lastReceivedMessage = ReconEntry?.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.ReconciliationEntrySummaryResponse, EDIMessage.Direction.Receive);
				if (lastReceivedMessage is CBPEDIMessage receivedMessage && receivedMessage.MessageBlock.B is AABIOutputB bBlock)
				{
					isRemoteLocationFilingSent = bBlock.RemotelyFiledIndicator == "1";
				}

				return isRemoteLocationFilingSent;
			}
		}

		public ZBool IsACE => JE_ApplicationCode == JobApplicationCodeList.Codes.ACE;

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.CusAgents))]
		public ZString JE_GS_NKCusAgent
		{
			get { return ReconWrappedJobDeclaration.JE_GS_NKCusAgent; }
			set { ReconWrappedJobDeclaration.JE_GS_NKCusAgent = value; }
		}

		public GlbStaff CusAgent
		{
			get { return ReconWrappedJobDeclaration == null ? null : ReconWrappedJobDeclaration.CusAgent; }
		}

		#region BrokerPhone
		public ZString BrokerPhone => ReconWrappedJobDeclaration.Branch.GB_Phone.KeepNumericCharacters();
		#endregion

		public ZString PreparerName
		{
			get { return CusAgent != null ? CusAgent.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo PreparerNameInfo
		{
			get { return GetZPropertyInfo(Schema.PreparerName); }
		}

		public ZPropertyInfo JE_GS_NKCusAgentInfo
		{
			get { return ReconWrappedJobDeclaration == null ? null : GetWrappedZPropertyInfo(JobDeclarationSchema.Constants.JE_GS_NKCusAgent, x => ReconWrappedJobDeclaration.JE_GS_NKCusAgentInfo); }
		}

		public ZString US_EntryFilerCode
		{
			get { return AddInfo.US_EntryFilerCode; }
			set { AddInfo.US_EntryFilerCode = value; }
		}

		public ZPropertyInfo US_EntryFilerCodeInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_EntryFilerCode, x => AddInfo.US_EntryFilerCodeInfo); }
		}

		public ZString US_ENSAction
		{
			get { return AddInfo.US_ENSAction; }
			set { AddInfo.US_ENSAction = value; }
		}

		public ZPropertyInfo US_ENSActionInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_ENSAction, x => AddInfo.US_ENSActionInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.ImportEntrySourceList))]
		public ZString US_ImportEntrySource
		{
			get { return AddInfo.US_ImportEntrySource; }
			set { AddInfo.US_ImportEntrySource = value; }
		}

		public ZPropertyInfo US_ImportEntrySourceInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_ImportEntrySource, x => AddInfo.US_ImportEntrySourceInfo); }
		}

		public ZString ImportEntrySourceDescription
		{
			get { return Lookups.ImportEntrySourceList.GetDescriptionFromCode(US_ImportEntrySource); }
		}

		public bool IsPaymentTypeValid
		{
			get { return Lookups.US_PaymentTypeList.ContainsCode(US_PaymentType); }
		}

		bool IsManualPayment
		{
			get { return US_PaymentType.IsEmpty || US_PaymentType == PaymentTypeList.Codes.IndividualBasis; }
		}

		public bool CanSendOriginal
		{
			get { return !HasBeenLodgedAtCustoms; }
		}

		public bool CanSendWithdrawal
		{
			get { return HasBeenLodgedAtCustoms; }
		}

		public bool HasBeenLodgedAtCustoms
		{
			get
			{
				var entry = ReconEntry.GetEntry();
				return entry.HasBeenLodgedAtCustoms;
			}
		}

		public bool IsWaitingForRespones
		{
			get
			{
				var entry = ReconEntry.GetEntry();
				return entry.IsWaitingForResponse;
			}
		}

		#region Message Status

		[MaxLength(CusEntryHeader.Schema.CH_StatusMaxLength)]
		public ZString MessageStatus
		{
			get { return ReconEntry.CH_Status; }
			set
			{
				ReconEntry.CH_Status = value;
				ReconWrappedJobDeclaration.JE_MessageStatus = value;
				MessageStatusInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MessageStatusInfo
		{
			get { return GetZPropertyInfo(Schema.MessageStatus); }
		}

		public ZString MessageStatusDescription
		{
			get
			{
				string result = Factory.GetCachedValue<ReconMessageStatusList>().GetDescriptionFromCode(MessageStatus);
				if (result == null)
				{
					result = "";
				}
				return result;
			}
		}

		public ZPropertyInfo MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.MessageStatusDescription); }
		}

		#endregion

		#region Customs Status

		public ZString CustomsStatus
		{
			get { return ReconWrappedJobDeclaration.JE_EntryStatus; }
		}

		public ZString CustomsStatusDescription
		{
			get
			{
				string result = Factory.GetCachedValue<ReconMessageStatusList>().GetDescriptionFromCode(CustomsStatus);
				return result == null ? "" : result;
			}
		}

		#endregion

		public ZDate ReconPaymentDate
		{
			get
			{
				ZDate result = ZDate.Today;

				if (US_PreliminaryStatementPrintDate.IsValid)
				{
					result = US_PreliminaryStatementPrintDate.Date;
				}
				else if (US_EstimatedEntryDate.IsValid)
				{
					result = US_EstimatedEntryDate.Date;
				}

				return result;
			}
		}

		/// <summary>
		/// Just the digit without entry filer code
		/// </summary>
		public ZString ReconEntryNumber
		{
			get { return ReconEntry.EntryNumber; }
		}

		public ZPropertyInfo ReconEntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ReconEntryNumber); }
		}

		public ZString ReconEntryNumberWithEntryFilerCode
		{
			get { return ReconEntryNumber.IsEmpty ? string.Empty : US_EntryFilerCode + ReconEntryNumber; }
		}

		public ZPropertyInfo ReconEntryNumberWithEntryFilerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ReconEntryNumberWithEntryFilerCode); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.SchDPortList))]
		public ZString US_SchDEntry
		{
			get { return AddInfo.US_SchDEntry; }
			set
			{
				AddInfo.US_SchDEntry = value;

				string defaultValue = ReconTeamPortMapper.DefaultReconTeam(US_SchDEntry);
				if (defaultValue != "")
				{
					US_TeamNo = defaultValue;
				}
			}
		}

		public ZPropertyInfo US_SchDEntryInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_SchDEntry, x => AddInfo.US_SchDEntryInfo); }
		}

		public ZString FilingPortDescription
		{
			get
			{
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_SchDEntry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				return port?.ZZD_Description ?? ZString.Empty;
			}
		}

		#region Importer Of Record

		[RelatedBusinessObject("ImporterOfRecord")]
		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.ImporterList))]
		public ZGuid IOROrgPK
		{
			get { return ReconWrappedJobDeclaration.IOROrgPK; }
			set
			{
				ZGuid oldValue = IOROrgPK;
				ReconWrappedJobDeclaration.IOROrgPK = value;
				if (!IsCopying && oldValue != IOROrgPK)
				{
					DefaultDataFromIORIfNeeded();
					DefaultNotifyParty();
				}
			}
		}

		public ZPropertyInfo IOROrgPKInfo
		{
			get
			{
				return ReconWrappedJobDeclaration == null ? null : GetWrappedZPropertyInfo(JobDeclaration.Schema.IOROrgPK, x => ReconWrappedJobDeclaration.IOROrgPKInfo);
			}
		}

		public OrgHeader ImporterOfRecord
		{
			get
			{
				var address = Factory.Load<OrgAddress>(ReconWrappedJobDeclaration.JE_OA_DeclarantAddress);
				return address == null ? null : address.Header;
			}
		}

		public ZString ImporterOfRecordNumber
		{
			get
			{
				var ior = ImporterOfRecord;
				return ior != null ? OrgHeaderWrapper.GetCustomsRelatedCode(ior, OrgMatchedCustomsRegNoType.EIN) : ZString.Empty;
			}
		}

		public ZString ImporterOfRecordNumber4Print => OrgHeaderWrapper.GetCustomsCode(ImporterOfRecord, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });

		public ZString ImporterOfRecordPhone
		{
			get { return ImporterOfRecordCustomsAddress != null ? ImporterOfRecordCustomsAddress.OA_Phone : ZString.Empty; }
		}

		public ZString ImporterOfRecordFax
		{
			get { return ImporterOfRecordCustomsAddress != null ? ImporterOfRecordCustomsAddress.OA_Fax : ZString.Empty; }
		}

		public ZString ImporterOfRecordEmail
		{
			get { return ImporterOfRecordCustomsAddress != null ? ImporterOfRecordCustomsAddress.OA_Email : ZString.Empty; }
		}

		OrgAddress ImporterOfRecordCustomsAddress
		{
			get { return ImporterOfRecord.GetCustomsAddressDetailsFallingBackToMainAddress(); }
		}

		#endregion

		#region Importer

		public JobDocAddress ImporterAddress
		{
			get
			{
				if (importerAddress == null || importerAddress.IsDeleted)
				{
					importerAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ImporterDocumentaryAddress);
					importerAddress.OverrideRequirement = new JobDocAddressRequirement() { CanOverride = false };
					importerAddress.E2_OA_AddressInfo.ValueChanged += new EventHandler(ImporterAddressChanged);
					importerAddress.DefaultContactType = ContactType.Consignee;
				}
				return importerAddress;
			}
		}
		JobDocAddress importerAddress;

		void ImporterAddressChanged(object sender, EventArgs e)
		{
			var importerOfRecordOld = IOROrgPK;
			if (ReconWrappedJobDeclaration != null && importerAddress.Organisation != null)
			{
				ReconWrappedJobDeclaration.JE_OH_Importer = importerAddress.Organisation.PK;
			}
			DefaultIORIfNeeded();
			if (importerOfRecordOld != IOROrgPK)
			{
				DefaultDataFromIORIfNeeded();
			}

			IOROrgPKInfo.RefreshBinding();
		}

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = ReconWrappedJobDeclaration.DocAddresses;
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		[RelatedBusinessObject("Importer")]
		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.ImporterList))]
		public ZGuid JE_OH_Importer
		{
			get { return ImporterAddress.OrganisationPK; }
			set
			{
				ZGuid oldValue = JE_OH_Importer;
				ImporterAddress.OrganisationPK = value;
				ReconWrappedJobDeclaration.JE_OH_Importer = value;

				if (!IsCopying && oldValue != JE_OH_Importer)
				{
					DefaultIORIfNeeded();
				}
			}
		}

		public ZPropertyInfo JE_OH_ImporterInfo
		{
			get { return ReconWrappedJobDeclaration == null ? null : GetWrappedZPropertyInfo(JobDeclarationSchema.Constants.JE_OH_Importer, x => ImporterAddress.OrganisationPKInfo); }
		}

		public OrgHeader Importer
		{
			get { return ReconWrappedJobDeclaration.Importer; }
		}

		public ZString ImporterPhone
		{
			get
			{
				var result = ZString.Empty;
				if (ImporterAddress != null && ImporterAddress.Address != null)
				{
					result = ImporterAddress.Address.OA_Phone;
					if (result.IsEmpty)
					{
						result = !Importer.MainAddress.OA_Phone.IsEmpty ? Importer.MainAddress.OA_Phone : ImporterAddress.E2_Phone;
					}
				}

				return result;
			}
		}

		public ZString ImporterFax
		{
			get
			{
				var result = ZString.Empty;
				if (ImporterAddress != null && ImporterAddress.Address != null)
				{
					result = ImporterAddress.Address.OA_Fax;
					if (result.IsEmpty)
					{
						result = !Importer.MainAddress.OA_Fax.IsEmpty ? Importer.MainAddress.OA_Fax : ImporterAddress.E2_Fax;
					}
				}
				return result;
			}
		}

		public ZString ImporterEmail
		{
			get
			{
				var result = ZString.Empty;
				if (ImporterAddress != null && ImporterAddress.Address != null)
				{
					result = ImporterAddress.Address.OA_Email;
					if (result.IsEmpty)
					{
						result = !Importer.MainAddress.OA_Email.IsEmpty ? Importer.MainAddress.OA_Email : ImporterAddress.E2_Email;
					}
				}
				return result;
			}
		}

		#endregion

		#region 4811 Party
		[RelatedBusinessObject("NotifyParty")]
		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.ImporterList))]
		public ZGuid JE_OH_NotifyParty
		{
			get { return ReconWrappedJobDeclaration.JE_OH_NotifyParty; }
			set { ReconWrappedJobDeclaration.JE_OH_NotifyParty = value; }
		}

		public ZPropertyInfo JE_OH_NotifyPartyInfo => ReconWrappedJobDeclaration.JE_OH_NotifyPartyInfo;

		public OrgHeader NotifyParty => Factory.Load<OrgHeader>(JE_OH_NotifyParty);

		public ZString NotifyPartyID => NotifyParty != null ? OrgHeaderWrapper.GetCustomsRelatedCode(NotifyParty, OrgMatchedCustomsRegNoType.EIN) : ZString.Empty;

		void DefaultNotifyParty()
		{
			var notifyParty = IORWrapper != null ? IORWrapper.NotifyParty : null;
			if (notifyParty != null)
			{
				JE_OH_NotifyParty = notifyParty.PK;
			}
		}

		#endregion

		internal RegistrationNumberResult GetRegistrationNumberResult(JobDocAddress docAddress)
		{
			return new RegistrationNumberResult(docAddress.Factory, true,
				delegate
				{
					RegistrationNumber result = new RegistrationNumber();
					if (docAddress != null && !docAddress.IsDeleted)
					{
						OrgHeader org = Factory.Load<OrgHeader>(docAddress.OrganisationPK);

						if (org != null)
						{
							OrgCusCode cusCode = OrgHeaderWrapper.GetCustomsRelatedOrgCusCode(org, OrgMatchedCustomsRegNoType.EIN);
							if (cusCode != null)
							{
								result.Number = cusCode.OK_CustomsRegNo;
								result.NumberType = cusCode.OK_CodeType;
							}
						}
					}
					return result;
				});
		}

		#region SummaryDocRecipientAddress
		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.Organisations))]
		public JobDocAddress SummaryDocRecipientAddress
		{
			get
			{
				if (fSummaryDocRecipientAddress == null || fSummaryDocRecipientAddress.IsDeleted)
				{
					fSummaryDocRecipientAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
					fSummaryDocRecipientAddress.Requirement.GetRegistrationNumberResult = GetRegistrationNumberResult;
					fSummaryDocRecipientAddress.Requirement.LookupsGovRegNumTypes = Lookups.GetGovNumTypeList;
				}
				return fSummaryDocRecipientAddress;
			}
		}
		JobDocAddress fSummaryDocRecipientAddress;

		public ZString DocRecipientID => SummaryDocRecipientAddress.E2_GovRegNum;
		public ZDateTime US_DocProvidedDate
		{
			get { return AddInfo.US_DocProvidedDate; }
			set { AddInfo.US_DocProvidedDate = value; }
		}
		public ZPropertyInfo US_DocProvidedDateInfo => GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_DocProvidedDate, x => AddInfo.US_DocProvidedDateInfo);
		#endregion

		#region Claimant
		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.Organisations))]
		public JobDocAddress ClaimantAddress
		{
			get
			{
				if (fClaimantAddress == null || fClaimantAddress.IsDeleted)
				{
					fClaimantAddress = DocAddresses.FindOrCreateWithRequirement(ClaimantDocAddressRequirement);
					fClaimantAddress.Requirement.GetRegistrationNumberResult = GetRegistrationNumberResult;
					fClaimantAddress.Requirement.LookupsGovRegNumTypes = Lookups.GetGovNumTypeList;
				}
				return fClaimantAddress;
			}
		}
		JobDocAddress fClaimantAddress;

		JobDocAddressRequirement ClaimantDocAddressRequirement
		{
			get
			{
				if (fClaimantDocAddressRequirement == null)
				{
					fClaimantDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ClaimantAddress);
					ReconWrappedJobDeclaration.DocAddressManager.AddRequirement(fClaimantDocAddressRequirement);
				}
				return fClaimantDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fClaimantDocAddressRequirement;

		public ZString ClaimantID => ClaimantAddress.E2_GovRegNum;

		public ZDateTime US_ClaimDate
		{
			get { return AddInfo.US_ClaimDate; }
			set { AddInfo.US_ClaimDate = value; }
		}
		public ZPropertyInfo US_ClaimDateInfo => GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_ClaimDate, x => AddInfo.US_ClaimDateInfo);

		public ZString US_ClaimID
		{
			get { return AddInfo.US_ClaimID; }
			set { AddInfo.US_ClaimID = value; }
		}
		public ZPropertyInfo US_ClaimIDInfo => GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_ClaimID, x => AddInfo.US_ClaimIDInfo);

		#endregion

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.SuretyCodeList))]
		public ZString US_SuretyCode
		{
			get { return AddInfo.US_SuretyCode; }
			set { AddInfo.US_SuretyCode = value; }
		}

		public ZPropertyInfo US_SuretyCodeInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_SuretyCode, x => AddInfo.US_SuretyCodeInfo); }
		}

		public ZDateTime US_PaymentDate
		{
			get { return AddInfo.US_PaymentDate; }
			set { AddInfo.US_PaymentDate = value; }
		}

		public ZPropertyInfo US_PaymentDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PaymentDate, x => AddInfo.US_PaymentDateInfo); }
		}

		/// <summary>
		/// Estimated Reconciliation Entry Summary Date
		/// </summary>
		public ZDateTime US_EstimatedEntryDate
		{
			get { return AddInfo.US_EstimatedEntryDate; }
			set
			{
				AddInfo.US_EstimatedEntryDate = value;
				DefaultPreliminaryStatementPrintDateIfNeeded();
			}
		}

		public ZPropertyInfo US_EstimatedEntryDateInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_EstimatedEntryDate, x => AddInfo.US_EstimatedEntryDateInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.US_TeamNoList))]
		public ZString US_TeamNo
		{
			get { return AddInfo.US_TeamNo; }
			set { AddInfo.US_TeamNo = value; }
		}

		public ZPropertyInfo US_TeamNoInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_TeamNo, x => AddInfo.US_TeamNoInfo); }
		}

		public ZString TeamNoDescription
		{
			get { return Lookups.US_TeamNoList.GetDescriptionFromCode(US_TeamNo); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.US_IssueCodeList))]
		public ZString US_IssueCode
		{
			get { return AddInfo.US_IssueCode; }
			set
			{
				AddInfo.US_IssueCode = value;
				OriginalEntries.MarkAsNeedingValidation();
			}
		}

		public ZString US_IssueCodeDescription
		{
			get { return Lookups.US_IssueCodeList.GetDescriptionFromCode(US_IssueCode); }
		}

		public ZPropertyInfo US_IssueCodeInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_IssueCode, x => AddInfo.US_IssueCodeInfo); }
		}

		public bool IsNoChangeAggregate
		{
			get { return US_IsAggregate && US_R_IsNoChangeAgg; }
		}

		public ZBool US_IsAggregate
		{
			get { return AddInfo.US_IsAggregate; }
			set
			{
				AddInfo.US_IsAggregate = value;

				if (US_IsAggregate)
				{
					foreach (ReconOriginalEntryHeader originalEntry in OriginalEntries)
					{
						originalEntry.RefundedFees.RemoveAndDeleteAll();
					}
				}
				else
				{
					US_R_IsNoChangeAgg = false;
					US_R_Waive = false;
					AggregateRefundedFees.RemoveAndDeleteAll();
				}
			}
		}

		public ZPropertyInfo US_IsAggregateInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_IsAggregate, x => AddInfo.US_IsAggregateInfo); }
		}

		public ZBool US_R_Waive
		{
			get { return AddInfo.US_R_Waive; }
			set { AddInfo.US_R_Waive = value; }
		}

		public ZPropertyInfo US_R_WaiveInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_R_Waive, x => AddInfo.US_R_WaiveInfo); }
		}

		public ZBool US_R_IsNoChangeAgg
		{
			get { return AddInfo.US_R_IsNoChangeAgg; }
			set
			{
				var oldValue = AddInfo.US_R_IsNoChangeAgg;

				if (oldValue != value)
				{
					AddInfo.US_R_IsNoChangeAgg = value;

					foreach (var originalEntry in OriginalEntries.Cast<ReconOriginalEntryHeader>())
					{
						if (US_R_IsNoChangeAgg)
						{
							originalEntry.AddAggregateFeesIfNecessary();
						}

						originalEntry.GetWrappedEntry()?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public ZPropertyInfo US_R_IsNoChangeAggInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_R_IsNoChangeAgg, x => AddInfo.US_R_IsNoChangeAggInfo); }
		}

		public ZString JE_DeclarationReference
		{
			get { return ReconWrappedJobDeclaration.JE_DeclarationReference; }
		}

		public ZPropertyInfo JE_DeclarationReferenceInfo
		{
			get { return ReconWrappedJobDeclaration == null ? null : GetWrappedZPropertyInfo(JobDeclarationSchema.Constants.JE_DeclarationReference, x => ReconWrappedJobDeclaration.JE_DeclarationReferenceInfo); }
		}

		public ZString Description
		{
			get { return ReconWrappedJobDeclaration.Description; }
		}

		public ZString US_Comment
		{
			get { return AddInfo.US_Comment; }
			set { AddInfo.US_Comment = value; }
		}

		public ZPropertyInfo US_CommentInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_Comment, x => AddInfo.US_CommentInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.US_PaymentTypeList))]
		public ZString US_PaymentType
		{
			get { return AddInfo.US_PaymentType; }
			set
			{
				AddInfo.US_PaymentType = value;
				new ClientBranchDesignationDefaulter().Default(this);
				DefaultPreliminaryStatementPrintDateIfNeeded();
				BrokerToPayIndicator = new PaymentDetailsDefaulter().GetDefaultBrokerToPayIndicatorBasedOnPaymentType(US_PaymentType);
			}
		}

		void DefaultPreliminaryStatementPrintDateIfNeeded()
		{
			if (IsPaymentTypeValid)
			{
				new PrelimStatementDetailsDefaulter().Default(this);
			}
			else
			{
				US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			}
		}

		public ZPropertyInfo US_PaymentTypeInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_PaymentType, x => AddInfo.US_PaymentTypeInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.US_YesNoList))]
		public ZString BrokerToPayIndicator
		{
			get { return ReconWrappedJobDeclaration.BrokerToPayIndicator; }
			set { ReconWrappedJobDeclaration.BrokerToPayIndicator = value; }
		}

		public ZPropertyInfo BrokerToPayIndicatorInfo
		{
			get { return ReconWrappedJobDeclaration == null ? null : GetWrappedZPropertyInfo(JobDeclaration.Schema.BrokerToPayIndicator, x => ReconWrappedJobDeclaration.BrokerToPayIndicatorInfo); }
		}

		public ZString PaymentTypeDescription
		{
			get { return Lookups.US_PaymentTypeList.GetDescriptionFromCode(US_PaymentType); }
		}

		public ZDateTime US_PreliminaryStatementPrintDate
		{
			get { return AddInfo.US_PreliminaryStatementPrintDate; }
			set { AddInfo.US_PreliminaryStatementPrintDate = value; }
		}

		public ZPropertyInfo US_PreliminaryStatementPrintDateInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_PreliminaryStatementPrintDate, x => AddInfo.US_PreliminaryStatementPrintDateInfo); }
		}

		public ZString US_ClientBranchDesignation
		{
			get { return AddInfo.US_ClientBranchDesignation; }
			set { AddInfo.US_ClientBranchDesignation = value; }
		}

		public ZPropertyInfo US_ClientBranchDesignationInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_ClientBranchDesignation, x => AddInfo.US_ClientBranchDesignationInfo); }
		}

		//TODO Joo Investigate if interest amount can be persisted against original entries like in non-aggregated.
		public ZDecimal US_R_AggregateInterest
		{
			get { return AddInfo.US_R_AggregateInterest; }
			set { AddInfo.US_R_AggregateInterest = value; }
		}

		public ZPropertyInfo US_R_AggregateInterestInfo
		{
			get { return GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_R_AggregateInterest, x => AddInfo.US_R_AggregateInterestInfo); }
		}

		public void ResumeApportionment()
		{
			ReconWrappedJobDeclaration.ResumeApportionment();
		}

		public bool ApportionmentDirty
		{
			get { return ReconWrappedJobDeclaration.ApportionmentDirty; }
		}

		public ZString DISStatus
		{
			get { return ReconWrappedJobDeclaration.DISStatus; }
		}

		public ZString DISStatusDescription
		{
			get { return ReconWrappedJobDeclaration.DISStatusDescription; }
		}

		#region Anticipated Liquidation Entry Properties

		public ZDateTime US_AnticipatedLiquidationDate
		{
			get { return ReconEntry != null ? ReconEntry.US_AnticipatedLiquidationDate : ZDateTime.Empty; }
		}

		public ZDecimal US_AnticipatedLiquidatedDuty
		{
			get { return ReconEntry != null ? ReconEntry.US_AnticipatedLiquidatedDuty : ZDecimal.Zero; }
		}

		public ZDateTime US_CollectionDate
		{
			get { return ReconEntry != null ? ReconEntry.US_CollectionDate : ZDateTime.Empty; }
		}

		#endregion

		#region Calculated Totals

		public ZDecimal TotalOriginalDuty
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Duty); }
		}

		public ZPropertyInfo TotalOriginalDutyInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOriginalDuty); }
		}

		public ZDecimal TotalReconDuty
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Duty); }
		}

		public ZPropertyInfo TotalReconDutyInfo
		{
			get { return GetZPropertyInfo(Schema.TotalReconDuty); }
		}

		public ZDecimal TotalDutyDifference
		{
			get { return TotalReconDuty - TotalOriginalDuty; }
		}

		public ZPropertyInfo TotalDutyDifferenceInfo
		{
			get { return GetZPropertyInfo(Schema.TotalDutyDifference); }
		}

		public ZDecimal TotalOriginalFee
		{
			get { return OriginalEntries.GetOriginalChargeAmount(EntryChargeTypeList.GetFeeCodes()); }
		}

		public ZPropertyInfo TotalOriginalFeeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOriginalFee); }
		}

		public ZDecimal TotalReconFee
		{
			get { return OriginalEntries.GetReconChargeAmount(EntryChargeTypeList.GetFeeCodes()); }
		}

		public ZPropertyInfo TotalReconFeeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalReconFee); }
		}

		public ZDecimal TotalFeeDifference
		{
			get { return TotalReconFee - TotalOriginalFee; }
		}

		public ZPropertyInfo TotalFeeDifferenceInfo
		{
			get { return GetZPropertyInfo(Schema.TotalFeeDifference); }
		}

		public ZDecimal TotalOriginalTax
		{
			get { return OriginalEntries.GetOriginalChargeAmount(CusFeeCodeConstants.GetTaxCodes()); }
		}

		public ZPropertyInfo TotalOriginalTaxInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOriginalTax); }
		}

		public ZDecimal TotalReconTax
		{
			get { return OriginalEntries.GetReconChargeAmount(CusFeeCodeConstants.GetTaxCodes()); }
		}

		public ZPropertyInfo TotalReconTaxInfo
		{
			get { return GetZPropertyInfo(Schema.TotalReconTax); }
		}

		public ZDecimal TotalTaxDifference
		{
			get { return TotalReconTax - TotalOriginalTax; }
		}

		public ZPropertyInfo TotalTaxDifferenceInfo
		{
			get { return GetZPropertyInfo(Schema.TotalTaxDifference); }
		}

		public ZDecimal InterestPaymentAmount
		{
			get { return US_IsAggregate ? US_R_AggregateInterest : OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest); }
		}

		public ZPropertyInfo InterestPaymentAmountInfo
		{
			get { return GetZPropertyInfo(Schema.InterestPaymentAmount); }
		}

		public ZDecimal TotalOriginalMPF
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
		}

		public ZDecimal TotalReconMPF
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing); }
		}

		public ZDecimal TotalMPFDifference
		{
			get { return TotalReconMPF - TotalOriginalMPF; }
		}

		public ZDecimal TotalOriginalSorghum
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Sorghum); }
		}

		public ZDecimal TotalReconSorghum
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Sorghum); }
		}

		public ZDecimal TotalSorghumDifference
		{
			get { return TotalReconSorghum - TotalOriginalSorghum; }
		}

		public ZDecimal TotalOriginalOtherAgencies
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies); }
		}

		public ZDecimal TotalReconOtherAgencies
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.OtherAgencies); }
		}

		public ZDecimal TotalOtherAgenciesDifference
		{
			get { return TotalReconOtherAgencies - TotalOriginalOtherAgencies; }
		}

		public ZDecimal TotalOriginalHMF
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		public ZDecimal TotalReconHMF
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		public ZDecimal TotalHMFDifference
		{
			get { return TotalReconHMF - TotalOriginalHMF; }
		}

		public ZDecimal TotalOriginalAvocado
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Avocado); }
		}

		public ZDecimal TotalReconAvocado
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Avocado); }
		}

		public ZDecimal TotalAvocadoDifference
		{
			get { return TotalReconAvocado - TotalOriginalAvocado; }
		}

		public ZDecimal TotalOriginalBeef
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Beef); }
		}

		public ZDecimal TotalReconBeef
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Beef); }
		}

		public ZDecimal TotalBeefDifference
		{
			get { return TotalReconBeef - TotalOriginalBeef; }
		}

		public ZDecimal TotalOriginalBlueberry
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Blueberry); }
		}

		public ZDecimal TotalReconBlueberry
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Blueberry); }
		}

		public ZDecimal TotalBlueberryDifference
		{
			get { return TotalReconBlueberry - TotalOriginalBlueberry; }
		}

		public ZDecimal TotalOriginalCotton
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton); }
		}

		public ZDecimal TotalReconCotton
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton); }
		}

		public ZDecimal TotalCottonDifference
		{
			get { return TotalReconCotton - TotalOriginalCotton; }
		}

		public ZDecimal TotalOriginalDairy
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.DairyFee); }
		}

		public ZDecimal TotalReconDairy
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.DairyFee); }
		}

		public ZDecimal TotalDairyDifference
		{
			get { return TotalReconDairy - TotalOriginalDairy; }
		}

		public ZDecimal TotalOriginalSpirits
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits); }
		}

		public ZDecimal TotalReconSpirits
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits); }
		}

		public ZDecimal TotalSpiritsDifference
		{
			get { return TotalReconSpirits - TotalOriginalSpirits; }
		}

		public ZDecimal TotalOriginalDutiableMail
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		public ZDecimal TotalReconDutiableMail
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail); }
		}

		public ZDecimal TotalDutiableMailDifference
		{
			get { return TotalReconDutiableMail - TotalOriginalDutiableMail; }
		}

		public ZDecimal TotalOriginalLimes
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes); }
		}

		public ZDecimal TotalReconLimes
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes); }
		}

		public ZDecimal TotalLimesDifference
		{
			get { return TotalReconLimes - TotalOriginalLimes; }
		}

		public ZDecimal TotalOriginalHoney
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey); }
		}

		public ZDecimal TotalReconHoney
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey); }
		}

		public ZDecimal TotalHoneyDifference
		{
			get { return TotalReconHoney - TotalOriginalHoney; }
		}

		public ZDecimal TotalOriginalMango
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Mango); }
		}

		public ZDecimal TotalReconMango
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Mango); }
		}

		public ZDecimal TotalMangoDifference
		{
			get { return TotalReconMango - TotalOriginalMango; }
		}

		public ZDecimal TotalOriginalInformal
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		public ZDecimal TotalReconInformal
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal); }
		}

		public ZDecimal TotalInformalDifference
		{
			get { return TotalReconInformal - TotalOriginalInformal; }
		}

		public ZDecimal TotalOriginalSurcharge
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge); }
		}

		public ZDecimal TotalReconSurcharge
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge); }
		}

		public ZDecimal TotalSurchargeDifference
		{
			get { return TotalReconSurcharge - TotalOriginalSurcharge; }
		}

		public ZDecimal TotalOriginalMushroom
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Mushroom); }
		}

		public ZDecimal TotalReconMushroom
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Mushroom); }
		}

		public ZDecimal TotalMushroomDifference
		{
			get { return TotalReconMushroom - TotalOriginalMushroom; }
		}

		public ZDecimal TotalOriginalOtherExcise
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise); }
		}

		public ZDecimal TotalReconOtherExcise
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise); }
		}

		public ZDecimal TotalOtherExciseDifference
		{
			get { return TotalReconOtherExcise - TotalOriginalOtherExcise; }
		}

		public ZDecimal TotalOriginalRaspberry
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Raspberry); }
		}

		public ZDecimal TotalReconRaspberry
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Raspberry); }
		}

		public ZDecimal TotalRaspberryDifference
		{
			get { return TotalReconRaspberry - TotalOriginalRaspberry; }
		}

		public ZDecimal TotalOriginalPork
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Pork); }
		}

		public ZDecimal TotalReconPork
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Pork); }
		}

		public ZDecimal TotalPorkDifference
		{
			get { return TotalReconPork - TotalOriginalPork; }
		}

		public ZDecimal TotalOriginalPotato
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Potato); }
		}

		public ZDecimal TotalReconPotato
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Potato); }
		}

		public ZDecimal TotalPotatoDifference
		{
			get { return TotalReconPotato - TotalOriginalPotato; }
		}

		public ZDecimal TotalOriginalSoftwoodLumber
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber); }
		}

		public ZDecimal TotalReconSoftwoodLumber
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber); }
		}

		public ZDecimal TotalSoftwoodLumberDifference
		{
			get { return TotalReconSoftwoodLumber - TotalOriginalSoftwoodLumber; }
		}

		public ZDecimal TotalOriginalSugar
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Sugar); }
		}

		public ZDecimal TotalReconSugar
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Sugar); }
		}

		public ZDecimal TotalSugarDifference
		{
			get { return TotalReconSugar - TotalOriginalSugar; }
		}

		public ZDecimal TotalOriginalTobacco
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Tobacco); }
		}

		public ZDecimal TotalReconTobacco
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Tobacco); }
		}

		public ZDecimal TotalTobaccoDifference
		{
			get { return TotalReconTobacco - TotalOriginalTobacco; }
		}

		public ZDecimal TotalOriginalWatermelon
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Watermelon); }
		}

		public ZDecimal TotalReconWatermelon
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Watermelon); }
		}

		public ZDecimal TotalWatermelonDifference
		{
			get { return TotalReconWatermelon - TotalOriginalWatermelon; }
		}

		public ZDecimal TotalOriginalWines
		{
			get { return OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Wines); }
		}

		public ZDecimal TotalReconWines
		{
			get { return OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Wines); }
		}

		public ZDecimal TotalWinesDifference
		{
			get { return TotalReconWines - TotalOriginalWines; }
		}

		#endregion

		/// <summary>
		/// This is a non-persisted proprerty to which users set to filter invoices & invoice lines for a particular entry
		/// </summary>
		[List(nameof(OriginalEntries))]
		public ZGuid SelectedOriginalEntry
		{
			get { return fSelectedOriginalEntry; }
			set
			{
				fSelectedOriginalEntry = value;
				SelectedOriginalEntryInfo.RefreshBinding();
				ActiveBusinessObjectCollection<JobComInvoiceHeader>.RefreshAll(Factory);
				FilteredInvoiceLines.Rebuild();
			}
		}
		ZGuid fSelectedOriginalEntry;

		public ZPropertyInfo SelectedOriginalEntryInfo
		{
			get { return GetZPropertyInfo("SelectedOriginalEntry"); }
		}

		#region Data Lodged In Customs

		public ZString US_R_ImporterIDLodged
		{
			get { return AddInfo.US_R_ImporterIDLodged; }
			set { AddInfo.US_R_ImporterIDLodged = value; }
		}
		public ZPropertyInfo US_R_ImporterIDLodgedInfo => GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_R_ImporterIDLodged, x => AddInfo.US_R_ImporterIDLodgedInfo);

		public ZString US_R_TeamNoLodged
		{
			get { return AddInfo.US_R_TeamNoLodged; }
			set { AddInfo.US_R_TeamNoLodged = value; }
		}
		public ZPropertyInfo US_R_TeamNoLodgedInfo => GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_R_TeamNoLodged, x => AddInfo.US_R_TeamNoLodgedInfo);

		public ZDateTime US_R_EntrySumDateLodged
		{
			get { return AddInfo.US_R_EntrySumDateLodged; }
			set { AddInfo.US_R_EntrySumDateLodged = value; }
		}
		public ZPropertyInfo US_R_EntrySumDateLodgedInfo => GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_R_EntrySumDateLodged, x => AddInfo.US_R_EntrySumDateLodgedInfo);

		#endregion

		public ZDateTime JE_SystemCreateTimeUtc
		{
			get { return ReconWrappedJobDeclaration != null ? ReconWrappedJobDeclaration.JE_SystemCreateTimeUtc : ZDateTime.Empty; }
		}

		public ZString JE_SystemCreateUser
		{
			get { return ReconWrappedJobDeclaration != null ? ReconWrappedJobDeclaration.JE_SystemCreateUser : ZString.Empty; }
		}

		public ZString JE_SystemCreateBranch
		{
			get { return ReconWrappedJobDeclaration != null ? ReconWrappedJobDeclaration.JE_SystemCreateBranch : ZString.Empty; }
		}

		public ZString JE_SystemCreateDepartment
		{
			get { return ReconWrappedJobDeclaration != null ? ReconWrappedJobDeclaration.JE_SystemCreateDepartment : ZString.Empty; }
		}

		public ZString CreatedUserName
		{
			get { return ReconWrappedJobDeclaration != null ? GetUserNameFromCode(ReconWrappedJobDeclaration.JE_SystemCreateUser) : ZString.Empty; }
		}

		public ZDateTime JE_SystemLastEditTimeUtc
		{
			get { return ReconWrappedJobDeclaration != null ? ReconWrappedJobDeclaration.JE_SystemLastEditTimeUtc : ZDateTime.Empty; }
		}

		public ZString JE_SystemLastEditUser
		{
			get { return ReconWrappedJobDeclaration != null ? ReconWrappedJobDeclaration.JE_SystemLastEditUser : ZString.Empty; }
		}

		public ZString LastEditUserName
		{
			get { return ReconWrappedJobDeclaration != null ? GetUserNameFromCode(ReconWrappedJobDeclaration.JE_SystemLastEditUser) : ZString.Empty; }
		}

		ZString GetUserNameFromCode(ZString userCode)
		{
			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, userCode);
			return staff != null ? staff.GS_FullName : ZString.Empty;
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.ServiceLevels))]
		public ZString JE_RS_NKServiceLevel
		{
			get { return ReconWrappedJobDeclaration != null ? ReconWrappedJobDeclaration.JE_RS_NKServiceLevel : ZString.Empty; }
			set
			{
				if (ReconWrappedJobDeclaration != null)
				{
					ReconWrappedJobDeclaration.JE_RS_NKServiceLevel = value;
				}
			}
		}

		public ZPropertyInfo JE_RS_NKServiceLevelInfo
		{
			get { return ReconWrappedJobDeclaration == null ? null : GetWrappedZPropertyInfo(JobDeclarationSchema.Constants.JE_RS_NKServiceLevel, x => ReconWrappedJobDeclaration.JE_RS_NKServiceLevelInfo); }
		}

		public ZBool SummaryDocProvidedStatement => US_IssueCode == ReconIssueCodeList.Codes.FTA && SummaryDocRecipientAddress != null && !SummaryDocRecipientAddress.IsEmpty && !SummaryDocRecipientAddress.IsOverridenButEmpty;

		public ZBool PriorDisclosureIndicator => OriginalEntries.Cast<ReconOriginalEntryHeader>().Any(x => x.US_PriorDisclosure);

		public ZBool NAFTA303ClaimStatement => OriginalEntries.Cast<ReconOriginalEntryHeader>().Any(x => x.US_NAFTAClaimStat);
		public ZBool ProtestOrPetitionFiledStatement => OriginalEntries.Cast<ReconOriginalEntryHeader>().Any(x => x.US_ProtestStat);

		#endregion

		#region Related Objects/Collections

		public override string ToString()
		{
			return JE_DeclarationReference;
		}

		[ChildEditable(true)]
		public Customs.Business.BaseJobComInvoiceGroupHeaderSingleElementCollection ActiveGroupHeader
		{
			get { return ReconWrappedJobDeclaration.ActiveGroupHeader; }
		}

		[ChildEditable(true)]
		public ReconOriginalEntryHeaderCollection OriginalEntries
		{
			get
			{
				if (reconOriginalEntries == null)
				{
					reconOriginalEntries = new ReconOriginalEntryHeaderCollection(ReconWrappedJobDeclaration, this);
					RegisterEditableChildObject(reconOriginalEntries);
				}
				return reconOriginalEntries;
			}
		}
		ReconOriginalEntryHeaderCollection reconOriginalEntries;

		[ChildEditable(true)]
		public InvoiceHeaderActiveCollection Invoices
		{
			get { return ReconWrappedJobDeclaration.Invoices; }
		}

		[ChildEditable(true)]
		public InvoiceHeaderFilteredActiveCollection FilteredInvoices
		{
			get { return ReconWrappedJobDeclaration.FilteredInvoices; }
		}

		[ChildEditable(false)]
		public ReconEDIMessageViewCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new ReconEDIMessageViewCollection(Factory);
					ReconEntry.Messages.CountChanged += ReconEntryMessage_CountChanged;
					reloadReconMessages = true;
				}

				if (reloadReconMessages)
				{
					fMessages.RemoveAll();
					fMessages.AddRange(ReconEntry.Messages);
					var wrappedDeclaration = ReconWrappedJobDeclaration;
					if (wrappedDeclaration != null)
					{
						foreach (var liquidation in wrappedDeclaration.Liquidations)
						{
							if (liquidation.Message != null)
							{
								fMessages.Add(liquidation.Message);
							}
						}
					}
					fMessages.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc, System.ComponentModel.ListSortDirection.Ascending);
					reloadReconMessages = false;
				}

				return fMessages;
			}
		}
		ReconEDIMessageViewCollection fMessages;
		ZBool reloadReconMessages;

		void ReconEntryMessage_CountChanged(object sender, CollectionCountChangedEventArgs args)
		{
			reloadReconMessages = true;
			Messages.RefreshBinding();
		}

		[ChildEditable(true)]
		public InvoiceLineCompleteCollection InvoiceLines
		{
			get { return ReconWrappedJobDeclaration.InvoiceLines; }
		}

		[ChildEditable(true)]
		public IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines
		{
			get { return ReconWrappedJobDeclaration.FilteredInvoiceLines; }
		}

		public JobComInvoiceGroupHeader TopGroupInvoice
		{
			get { return (JobComInvoiceGroupHeader)ReconWrappedJobDeclaration.TopGroupInvoice; }
		}

		IReconDeclarationAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = ReconWrappedJobDeclaration.GetAddInfo();
				}
				return fAddInfo;
			}
		}
		IReconDeclarationAddInfo fAddInfo;

		public bool HasAddInfoChangesSinceLastSaving(SchemaColumn addInfoColumn)
		{
			return AddInfo.HasChangesSinceLastSaving(addInfoColumn);
		}

		public ReconDeclarationLookups Lookups
		{
			get { return new ReconDeclarationLookups(this, ReconWrappedJobDeclaration); }
		}

		public ReconEntryHeader ReconEntry
		{
			get
			{
				if (reconEntry == null)
				{
					CusEntryHeader entry = GetEntryWithReconType();
					if (entry != null)
					{
						reconEntry = new ReconEntryHeader(entry);
					}
				}
				return reconEntry;
			}
		}
		ReconEntryHeader reconEntry;

		CusEntryHeader GetEntryWithReconType()
		{
			CusEntryHeader result = null;
			foreach (CusEntryHeader entry in ReconWrappedJobDeclaration.ActiveEntryHeaders)
			{
				if (entry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ReconEntry)
				{
					result = entry;
					break;
				}
			}
			return result;
		}

		[ChildEditable(true)]
		public CusLiquidationCollection Liquidations
		{
			get { return ReconWrappedJobDeclaration.Liquidations; }
		}

		[ChildEditable(true)]
		public ReconRefundedChargeCollection AggregateRefundedFees
		{
			get
			{
				if (originalCharges == null)
				{
					originalCharges = new ReconRefundedChargeCollection(ReconWrappedJobDeclaration);
					originalCharges.Load();
					RegisterEditableChildObject(originalCharges);
					originalCharges.Sort(CusCodeDataSchema.CY_Code.Name, System.ComponentModel.ListSortDirection.Ascending);
				}

				return originalCharges;
			}
		}
		ReconRefundedChargeCollection originalCharges;

		internal ReconOriginalDeclarationCollection OriginalDeclarations
		{
			get
			{
				if (originalDeclarations == null)
				{
					originalDeclarations = new ReconOriginalDeclarationCollection(this);

					if (!IsOriginalDeclarationLoadSuspended)
					{
						originalDeclarations.Load();
					}
				}
				return originalDeclarations;
			}
		}
		ReconOriginalDeclarationCollection originalDeclarations;

		internal bool IsOriginalDeclarationLoadSuspended
		{
			get { return OriginalDeclarationLoadSuspenderIndex > 0; }
		}

		public IDisposable SuspendOriginalDeclarationLoad()
		{
			return new OriginalDeclarationLoadSuspender(this);
		}

		class OriginalDeclarationLoadSuspender : IDisposable
		{
			public OriginalDeclarationLoadSuspender(ReconDeclaration reconDeclaration)
			{
				this.reconDeclaration = reconDeclaration;
				this.reconDeclaration.OriginalDeclarationLoadSuspenderIndex++;
			}

			readonly ReconDeclaration reconDeclaration;

			void IDisposable.Dispose()
			{
				reconDeclaration.OriginalDeclarationLoadSuspenderIndex--;

				if (!reconDeclaration.IsOriginalDeclarationLoadSuspended)
				{
					reconDeclaration.OriginalDeclarations.Load();
				}
			}
		}
		byte OriginalDeclarationLoadSuspenderIndex;

		#endregion

		#region Document printing

		public ZDate EarliestImportDate
		{
			get
			{
				if (!earliestLatestImportDateUpToDate)
				{
					earliestLatestImportDateUpToDate = true;
					GetEarliestAndLatestImportDate(out earliestImportDate, out latestImportDate);
				}
				return earliestImportDate;
			}
		}
		ZDate earliestImportDate;

		public ZDate LatestImportDate
		{
			get
			{
				if (!earliestLatestImportDateUpToDate)
				{
					earliestLatestImportDateUpToDate = true;
					GetEarliestAndLatestImportDate(out earliestImportDate, out latestImportDate);
				}
				return latestImportDate;
			}
		}
		ZDate latestImportDate;

		void GetEarliestAndLatestImportDate(out ZDate earliestImportDate, out ZDate latestImportDate)
		{
			earliestImportDate = ZDate.Empty;
			latestImportDate = ZDate.Empty;

			foreach (ReconOriginalEntryHeader entry in OriginalEntries)
			{
				if (entry.US_ImportDate.IsValid)
				{
					if (earliestImportDate.IsEmpty || entry.US_ImportDate < earliestImportDate)
					{
						earliestImportDate = entry.US_ImportDate.Date;
					}

					if (latestImportDate.IsEmpty || entry.US_ImportDate > latestImportDate)
					{
						latestImportDate = entry.US_ImportDate.Date;
					}
				}
			}
		}

		bool earliestLatestImportDateUpToDate;

		public void RefreshEarliestLatestImportDate()
		{
			earliestLatestImportDateUpToDate = false;
		}

		public ZDate EarliestEntryDate
		{
			get
			{
				if (earliestEntryDate == null)
				{
					earliestEntryDate = new CachedProperty<ZDate>(Factory, delegate
					{
						ZDateTime result = ZDateTime.Empty;

						foreach (ReconOriginalEntryHeader originalEntry in OriginalEntries)
						{
							if (result.IsEmpty || originalEntry.US_PaymentDate.IsValid && originalEntry.US_PaymentDate < result)
							{
								result = originalEntry.US_PaymentDate;
							}
						}

						return result.Date;
					});
				}

				return earliestEntryDate.Value;
			}
		}
		CachedProperty<ZDate> earliestEntryDate;

		public ZString ReconYears
		{
			get
			{
				if (ReconYearsCached == null)
				{
					ReconYearsCached = new CachedProperty<ZString>(Factory, delegate
						{
							ZDateTime first = ZDateTime.Empty;
							ZDateTime last = ZDateTime.Empty;

							foreach (ReconOriginalEntryHeader originalEntry in OriginalEntries)
							{
								var date = originalEntry.US_PaymentDate;
								if (date.IsValid)
								{
									if (first.IsEmpty || first > date)
									{
										first = date;
									}
									if (last.IsEmpty || last < date)
									{
										last = date;
									}
								}
							}

							ZStringBuilder builder = new ZStringBuilder();
							if (first.IsValid)
							{
								builder.Append(first.Year.ToString());
							}
							if (last.IsValid && first.Year != last.Year)
							{
								builder.Append(last.Year.ToString());
							}
							return builder.ToStringWithDelimiterBetweenAppends(" - ");
						});
				}
				return ReconYearsCached.Value;
			}
		}
		CachedProperty<ZString> ReconYearsCached;

		/// <summary>
		/// This is for printing a document where a row has two original entry details flattened.
		/// Once it is constructed, it does not refresh itself with 'OriginalEntries' unless RefreshAggregatedEntries gets called. 
		/// </summary>
		public AggregateReconEntryDocumentDataCollection AggregatedEntries
		{
			get { return aggregatedEntries ?? (aggregatedEntries = new AggregateReconEntryDocumentDataCollection(this)); }
		}
		AggregateReconEntryDocumentDataCollection aggregatedEntries;

		public void RefreshAggregatedEntries()
		{
			aggregatedEntries = null;
		}

		/// <summary>
		/// For document printing 
		/// This collection is populated by an explicit call for ReconChangedLinesMerger.DoMerge before a document is printed.
		/// </summary>
		public ReconChangedLineCollection ChangedLines
		{
			get { return reconChangedLines ?? (reconChangedLines = new ReconChangedLineCollection(Factory)); }
		}
		ReconChangedLineCollection reconChangedLines;

		public ZDecimal TotalCustomsValue { get; set; }
		public ZDecimal TotalOriginalValue { get; set; }
		public ZDecimal TotalValueChange => TotalCustomsValue - TotalOriginalValue;

		/// <summary>
		/// For document printing 
		/// This collection is populated by an explicit call for ReconChangedLinesMerger.DoMerge before a document is printed.
		/// only lines with decrease are shown - for aggregate recon.
		/// </summary>
		public ReconChangedLineCollection ChangedLinesWithDecrease
		{
			get { return changedLinesWithDecrease ?? (changedLinesWithDecrease = new ReconChangedLineCollection(Factory)); }
		}
		ReconChangedLineCollection changedLinesWithDecrease;

		public ZDecimal TotalDecreaseCustomsValue { get; set; }
		public ZDecimal TotalDecreaseOriginalValue { get; set; }
		public ZDecimal TotalDecreaseValueChange => TotalDecreaseCustomsValue - TotalDecreaseOriginalValue;

		public ZString ImporterName
		{
			get { return ImporterAddress.Organisation != null ? ImporterAddress.Organisation.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZString ImporterContactName
		{
			get { return ImporterAddress.Contact != null ? ImporterAddress.Contact.OC_ContactName : ZString.Empty; }
		}

		public ZString ImporterContactPhone
		{
			get { return ImporterAddress.Contact != null ? ImporterAddress.Contact.OC_Phone : ZString.Empty; }
		}

		public ZString ImporterContactEmail
		{
			get { return ImporterAddress.Contact != null ? ImporterAddress.Contact.OC_Email : ZString.Empty; }
		}

		public ZString ReconEntryNumberFormatted
		{
			get
			{
				ZString result = US_EntryFilerCode;
				if (!ReconEntryNumber.IsEmpty)
				{
					var entryNum = ReconEntryNumber.Length < 8 ? ReconEntryNumber.PadLeft(8, '0') : ReconEntryNumber;
					result += "-" + entryNum.SubstringSafe(0, 7) + "-" + entryNum.SubstringSafe(7, 1);
				}
				return result;
			}
		}

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("3571BCA9-BF65-4E40-A945-79643FC0FF5A", "Recon. Declaration {0}", JE_DeclarationReference);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var fullName = (importerAddress != null && importerAddress.Organisation != null && !importerAddress.Organisation.OH_FullName.IsEmpty) ? " - " + Importer.OH_FullName : string.Empty;
				return Res.GetString("E1D222C5-79A9-4C55-AB44-638FE58FA987", "Recon. - {0}{1}", JE_DeclarationReference, fullName);
			}
		}

		protected override BusinessObject LogsAndNotesTarget
		{
			get { return ReconWrappedJobDeclaration; }
		}

		public ReconDeclarationValidation Validation
		{
			get { return new ReconDeclarationValidation(this); }
		}

		public override void Delete()
		{
			base.Delete();
			ReconWrappedJobDeclaration.Delete();
		}

		protected override IDisposable SuspendSettingHasChangesCore()
		{
			IDisposable thisOne = base.SuspendSettingHasChangesCore();
			IDisposable declarationSuspender = null;

			if (ReconWrappedJobDeclaration != null)
			{
				declarationSuspender = ReconWrappedJobDeclaration.SuspendSettingHasChanges();
			}

			return new DisposableAction(delegate
			{
				thisOne.Dispose();

				if (declarationSuspender != null)
				{
					declarationSuspender.Dispose();
				}
			});
		}

		protected override void RunPreSaveValidationCore()
		{
			ReleaseReadFactory();
			base.RunPreSaveValidationCore();
			Validation.ValidateReconEntryNumberWithEntryFilerCode();
		}

		#endregion

		#region Implementation

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new Strategy(this);
		}

		class Strategy : BusinessObjectFetchStrategy
		{
			public Strategy(ReconDeclaration reconDec)
				: base(reconDec)
			{
			}

			ReconDeclaration reconDec
			{
				get { return (ReconDeclaration)base.BusinessObject; }
			}

			JobDeclaration declaration
			{
				get { return reconDec.ReconWrappedJobDeclaration; }
			}

			protected override void FetchForFactorySaveCore()
			{
				base.FetchForFactorySaveCore();
				declaration.FetchStrategy.FetchForFactorySave();
			}

			protected override void FetchForDeleteCore()
			{
				base.FetchForDeleteCore();
				declaration.FetchStrategy.FetchForDelete();
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				declaration.FetchStrategy.FetchForValidate();
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				var reducedColumns = new List<TableColumn>();
				foreach (var column in columns)
				{
					switch (column.ColumnName)
					{
						case JobDeclaration.Schema.JE_OH_Importer:
						case JobDeclaration.Schema.ImporterName:
							Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, declaration.PK);
							break;
						case ReconDeclaration.Schema.ReconEntryNumber:
							Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, declaration.PK);
							break;
						case ReconDeclaration.Schema.MessageStatus:
						case ReconDeclaration.Schema.MessageStatusDescription:
						case ReconDeclaration.Schema.US_AnticipatedLiquidationDate:
							Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, declaration.PK);
							break;

						default:
							reducedColumns.Add(column);
							break;
					}
				}

				base.FetchForViewCore(reducedColumns.ToArray());
				declaration.FetchStrategy.FetchForView(columns);
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				declaration.FetchStrategy.FetchForLoadChildEditableObjects();
			}
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get
			{
				return JobDeclarationSchema.PK;
			}
		}

		void DefaultDataFromIORIfNeeded()
		{
			var iorWrapper = OrgHeaderWrapper.New(ImporterOfRecord);
			if (iorWrapper != null)
			{
				US_PaymentType = iorWrapper.ZO_ReconPaymentType;
				BrokerToPayIndicator = iorWrapper.ZO_ReconBrokerToPay;
				US_IssueCode = iorWrapper.ZO_OtherReconIndicator;
				if (US_IssueCode.IsEmpty && iorWrapper.ZO_NAFTAReconIndicator)
				{
					US_IssueCode = ReconIssueCodeList.Codes.FTA;
				}
				if (!iorWrapper.ZO_ReconFilingPort.IsEmpty)
				{
					US_SchDEntry = iorWrapper.ZO_ReconFilingPort;
				}
				new BondDetailsDefaulter().Default(this, BondTypeList.Codes.ContinuousBond);
				if (!iorWrapper.ZO_ImportSource.IsEmpty)
				{
					US_ImportEntrySource = iorWrapper.ZO_ImportSource;
				}
			}
		}

		void DefaultIORIfNeeded()
		{
			if (!IOROrgPK.IsValid)
			{
				OrgHeader importer = Importer;
				if (importer != null)
				{
					IOROrgPK = importer.PK;
				}
			}
		}

		#endregion

		#region TemplateCopy()

		public IBusiness TemplateReconDeclarationCopyCore(CloneType cloneType)
		{
			BaseJobDeclaration result = (BaseJobDeclaration)GetTemplateCopyStrategy(Factory, cloneType).Clone();

			using (result.GetValidationSuspender())
			{
				var dec = (JobDeclaration)result;
				ResetValuesOnTemplateCopyAfterClone(result, cloneType);
			}

			result.HasChanges = false;
			return result;
		}

		protected virtual void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration, CloneType cloneType)
		{
			declaration.JE_ContainerCount = (short)declaration.CusContainers.Count;
			declaration.JE_IsCancelled = false;

			var reconDec = ((JobDeclaration)declaration).ReconDeclaration;
			reconDec.US_Comment = string.Empty;

			foreach (ReconOriginalEntryHeader originalEntryH in reconDec.OriginalEntries)
			{
				using (originalEntryH.OriginalCharges.SuspendSettingHasChanges())
				using (originalEntryH.ReconCharges.SuspendSettingHasChanges())
				{
					foreach (CusEntryHeaderCharges charge in originalEntryH.ReconCharges)
					{
						originalEntryH.OriginalCharges.SetAmount(charge.C1_ChargeType, charge.C1_ChargeAmount);
					}
				}

				foreach (ZPropertyInfo propertyInfo in declaration.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTime.Empty;
					}
				}

				if (US_IssueCode == ReconIssueCodeList.Codes.FTA)
				{
					originalEntryH.US_NAFTAReconIndicator = true;
				}

				originalEntryH.UpdateChargesReadOnlyState();
			}

			foreach (BaseJobComInvoiceGroupHeader groupInvoice in declaration.AllGroupHeaders)
			{
				using (groupInvoice.GetValidationSuspender())
				using (groupInvoice.SuspendSettingHasChanges())
				{
					groupInvoice.JZ_InvoiceDate = ZDateTime.Today;
				}
				groupInvoice.HasChanges = false;
			}
		}

		#region SupportsCloneCore
		protected override bool SupportsCloneCore()
		{
			return true;
		}
		#endregion

		protected virtual JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new ReconDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}
		#endregion

		#region Refresh Tariff

		public void RefreshTariff()
		{
			InvoiceLines.RefreshTariff();
			HasChanges = true;
		}

		#endregion

		#region IDeclaration Members

		ZString IStatementLineDeclaration.LocalCurrencyCode
		{
			get { return ReconWrappedJobDeclaration.LocalCurrencyCode; }
		}

		ZDateTime IStatementLineDeclaration.US_PSDAccepted
		{
			get;//Do nothing. An irrelevant field for recon. US_PSDAccepted is an internal date only that is required for 7501 for automatic sending. 
			set;
		}

		ZString IStatementLineDeclaration.US_ClientBranchDesignation
		{
			get { return US_ClientBranchDesignation; }
			set { US_ClientBranchDesignation = value; }
		}

		ZDateTime IStatementLineDeclaration.US_PaymentDate
		{
			get { return US_PaymentDate; }
			set { US_PaymentDate = value; }
		}

		ZBool IStatementLineDeclaration.US_ConsolACE
		{
			get;//Do nothing. An irrelevant field for recon
			set;
		}

		ZString IStatementLineDeclaration.US_PaperlessEntry
		{
			get;//Do nothing. An irrelevant field for recon
			set;
		}

		ZString IStatementLineDeclaration.US_PeriodicStatementMM
		{
			get;//Do nothing. An irrelevant field for recon
			set;
		}

		ZDateTime IStatementLineDeclaration.US_PreliminaryStatementPrintDate
		{
			get { return US_PreliminaryStatementPrintDate; }
			set { US_PreliminaryStatementPrintDate = value; }
		}

		ZString IStatementLineDeclaration.FormattedEntryNumber
		{
			get { return CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(US_EntryFilerCode, ReconEntryNumber); }
		}

		ZString IStatementLineDeclaration.ProcessingDistrictPort
		{
			get { return US_SchDEntry; }
		}

		bool IStatementLineDeclaration.HasEntryBeenWithdrawn
		{
			get
			{
				var entry = ReconEntry.GetEntry();
				return entry.HasBeenWithdrawn;
			}
		}

		bool IStatementLineDeclaration.IsRemoteLocationFiling => IsRemoteLocationFiling;

		ZString IStatementLineDeclaration.US_PreparerDistrictPort => PreparerDistrictPort;

		ZString IStatementLineDeclaration.US_PreparerOfficeCode => ((IStatementDeleteTransaction)this).PreparerOfficeCode;

		bool IStatementLineDeclaration.IsACE => IsACE;

		ZString IStatementLineDeclaration.BrokerReferenceNumber
		{
			get { return JE_DeclarationReference; }
		}

		ZString IStatementLineDeclaration.EntryNumber
		{
			get { return ReconEntryNumber; }
		}

		ZString IStatementLineDeclaration.EntryFilerCode
		{
			get { return US_EntryFilerCode; }
		}

		OrgHeader IStatementLineDeclaration.IOR
		{
			get { return ImporterOfRecord; }
		}

		ZString IStatementLineDeclaration.ReleaseStatus
		{
			get { return CRLReleaseStatusList.Codes.NRT; }
		}

		ZString IStatementLineDeclaration.ReleaseStatusDescription
		{
			get { return CRLReleaseStatusList.Descriptions.NRT; }
		}

		EDIMessageCollection IStatementLineDeclaration.Messages
		{
			get { return ReconEntry.Messages; }
		}

		#endregion

		#region IReconOriginalChargeParent Members

		CodeDescriptionPairList IReconOriginalChargeParent.FeeAndChargeList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("Recon Aggregate FeeAndChargeList", delegate
				{
					var list = new CodeDescriptionPairList();
					var codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);
					list.AddRange(codes);
					list.RemoveCode(Core.Constants.USCustoms.FeeCodes.DistilledSpirits);
					list.RemoveCode(Core.Constants.USCustoms.FeeCodes.Tobacco);
					list.RemoveCode(Core.Constants.USCustoms.FeeCodes.Wines);
					list.RemoveCode(Core.Constants.USCustoms.FeeCodes.OtherExcise);
					list.Sort();
					return list;
				});
			}
		}

		bool IReconOriginalChargeParent.DefaultValueForOverridenForNewChild
		{
			get { return false; }
		}

		USCTariff IReconOriginalChargeParent.Tariff
		{
			get { return null; }
		}

		void IReconOriginalChargeParent.SynchroniseOnNonCommittedAdded(ReconEntryOriginalCharge charge)
		{
		}

		bool IReconOriginalChargeParent.ShouldCalculateOrigDuty
		{
			get { return false; }
		}

		bool IReconOriginalChargeParent.MonthlyFiling
		{
			get { return false; }
		}

		void IReconOriginalChargeParent.UpdateChargeDetails()
		{
		}

		BusinessObject IReconOriginalChargeParent.ParentAsBusinessObject
		{
			get { return ReconWrappedJobDeclaration; }
		}

		#endregion

		#region IInvoicesProvider Members

		ZBool IInvoicesProvider.ShouldElectronicInvoicesBeVisible
		{
			get { return false; }
		}

		bool IInvoicesProvider.IsImportByExternalBroker
		{
			get { return false; }
		}

		bool IInvoicesProvider.IsConsumptionFTZ
		{
			get { return false; }
		}

		bool IInvoicesProvider.IsInwardBondedWarehousingEnabled
		{
			get { return false; }
		}

		bool IInvoicesProvider.IsOutwardBondedWarehousingEnabled
		{
			get { return false; }
		}

		ZPropertyInfo Customs.Business.IInvoicesProvider.MessageTypeInfo => ReconWrappedJobDeclaration.JE_MessageTypeInfo;

		OrgHeader Customs.Business.IInvoicesProvider.NewOwner => null;

		event EventHandler Customs.Business.IInvoicesProvider.OnNewOwnerPartAttributeCaptionDetailsChanged
		{
			add { }
			remove { }
		}

		IInvoiceLineViewCollection<BaseJobComInvoiceLine> Customs.Business.IInvoicesProvider.FilteredInvoiceLines
		{
			get { return FilteredInvoiceLines; }
		}

		Customs.Business.IInvoicesProviderValueChangedAnnouncer Customs.Business.IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
		{
			return null;
		}

		bool IInvoicesProvider.UseScheduleB
		{
			get { return false; }
		}

		bool IInvoicesProvider.IsExWarehouse
		{
			get { return false; }
		}

		bool IInvoicesProvider.IsLineGroupingSupported
		{
			get { return false; }
		}

		ZBool IInvoicesProvider.IsImport
		{
			get { return ZBool.False; }
		}

		bool Enterprise.Customs.Business.IInvoicesProvider.ApportionmentDirty
		{
			get { return ReconWrappedJobDeclaration.ApportionmentDirty; }
		}

		IEnumerable<Enterprise.Customs.Business.BaseJobComInvoiceHeader> Enterprise.Customs.Business.IInvoicesProvider.Invoices
		{
			get { return Invoices; }
		}

		event Enterprise.Customs.Business.BaseJobDeclaration.ApportionmentDirtyChangedEventHandler Enterprise.Customs.Business.IInvoicesProvider.OnApportionmentDirtyChanged
		{
			add { ReconWrappedJobDeclaration.OnApportionmentDirtyChanged += value; }
			remove { ReconWrappedJobDeclaration.OnApportionmentDirtyChanged -= value; }
		}

		public IDisposable SuspendMarkingApportionmentDirty()
		{
			return ReconWrappedJobDeclaration.SuspendMarkApportionmentDirty();
		}

		public void MarkApportionmentDirty()
		{
			ReconWrappedJobDeclaration.MarkApportionmentDirty();
		}

		event Enterprise.Customs.Business.BaseJobDeclaration.ApportionmentProgressEventHandler Enterprise.Customs.Business.IInvoicesProvider.OnApportionmentProgressChanged
		{
			add { ReconWrappedJobDeclaration.OnApportionmentProgressChanged += value; }
			remove { ReconWrappedJobDeclaration.OnApportionmentProgressChanged -= value; }
		}

		Enterprise.Customs.Business.IInvoicesProviderLookups Enterprise.Customs.Business.IInvoicesProvider.Lookups
		{
			get { return Lookups; }
		}

		bool Customs.Business.IInvoicesProvider.ShouldCreateDummyInvoiceLinesForMerge
		{
			get { return false; }
		}

		OrgHeader Customs.Business.IInvoicesProvider.Importer
		{
			get { return null; }
		}

		ZBool Customs.Business.IInvoicesProvider.ContainersRequired
		{
			get { return false; }
		}

		void Customs.Business.IInvoicesProvider.SelectContainerForAllInvoiceLines(Customs.Business.NonPersistentCusContainer selectedContainer)
		{
			//nothing to do
		}

		void Customs.Business.IInvoicesProvider.UnSelectContainerForAllInvoiceLines(Customs.Business.NonPersistentCusContainer selectedContainer)
		{
			//nothing to do
		}

		event EventHandler Customs.Business.IInvoicesProvider.OnContainerDetailsChanged
		{
			add { }
			remove { }
		}

		event EventHandler Customs.Business.IInvoicesProvider.OnPartAttributeCaptionDetailsChanged
		{
			add { }
			remove { }
		}

		event EventHandler Customs.Business.IInvoicesProvider.OnInvoiceLinesVisibilityChanged
		{
			add { }
			remove { }
		}

		void IInvoicesProvider.AddDefaultInvoice()
		{
			if (OriginalEntries.Count == 0)
			{
				OriginalEntries.AddNew();
			}
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return ((ICustomLabelsConfigOrgProvider)ReconWrappedJobDeclaration).ConfigOrg; }
		}

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { ((ICustomLabelsConfigOrgProvider)ReconWrappedJobDeclaration).ConfigOrgChanged += value; }
			remove { ((ICustomLabelsConfigOrgProvider)ReconWrappedJobDeclaration).ConfigOrgChanged -= value; }
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new ReconDeclarationDocumentSupporter(this); }
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return ((IEDocsProvider)ReconWrappedJobDeclaration).GetEDocsProviderSupporter();
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return ReconWrappedJobDeclaration.DocManagerInfo; }
		}

		#endregion

		#region IDocsAndCartageParent

		IHaveRequiredDocuments IDocsAndCartageParent.RequiredDocumentsProvider
		{
			get { return ReconWrappedJobDeclaration.DocsAndCartage; }
		}

		public Type DocsAndCartageType
		{
			get { return typeof(Freight.Forwarding.Business.ForwardingDocsAndCartage); }
		}

		public Type DocsAndCartageParentType
		{
			get { return ReconWrappedJobDeclaration.GetType(); }
		}

		#endregion

		#region IEDocsPluginHostDecider

		IBusiness IEDocsPluginHostDecider.HostBusinessEntity
		{
			get { return ReconWrappedJobDeclaration; }
		}

		#endregion

		#region IDutyDataLineHeaderProvider Members

		IEnumerable<IDutyDataLineHeader> IDutyDataLineHeaderProvider.EntriesToCalculateDutyFeeTax
		{
			get
			{
				foreach (ReconOriginalEntryHeader entry in OriginalEntries)
				{
					if (entry.ShouldDutiesFeesBeCalculated)
					{
						yield return new ReconCurrentDutyDataLineHeader(entry);

						yield return new ReconOriginalDutyDataLineHeader(entry);
					}
				}
			}
		}

		bool IDutyDataLineHeaderProvider.IsCustomsChargeRelevantForDecType(string chargeCode)
		{
			return true;
		}

		ZDecimal? IDutyDataLineHeaderProvider.OverridenTotalMPFPayable
		{
			get { return null; }
		}

		#endregion

		#region IPrelimStatementDetailsDefault Members

		bool IPrelimStatementDetailsDefault.IsManualPayment
		{
			get { return ReconWrappedJobDeclaration.IsManualPayment; }
		}

		bool IPrelimStatementDetailsDefault.FixPSD
		{
			get { return ReconWrappedJobDeclaration.US_FixPSD; }
		}

		bool IPrelimStatementDetailsDefault.RegistryAllowsDefaulting
		{
			get { return true; } // always default Prelim Statement Print Date
		}

		ZDate IPrelimStatementDetailsDefault.BaseDateToCalculateOn
		{
			get { return US_EstimatedEntryDate.IsValid ? US_EstimatedEntryDate.Date : ZDate.Today; }
		}

		OrgHeader IPrelimStatementDetailsDefault.IOR
		{
			get { return null; } //we should not fallback to Importer Of Record for ZO_SPDNumberOfDays. PSD for Reconciliation should be Est. Recon. Date + 1
		}

		GlbBranch IPrelimStatementDetailsDefault.Branch
		{
			get { return Branch ?? Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		ZString IPrelimStatementDetailsDefault.US_PeriodicStatementMM
		{
			get { return null; }
			set { }//Do nothing. An irrelevant field for recon
		}

		bool IPrelimStatementDetailsDefault.ShouldValidatePastDate
		{
			get { return !CanSendWithdrawal; }
		}

		string IPrelimStatementDetailsDefault.DatePrecedenceMessage
		{
			get { return PrelimStatementPrintDateValidator.ReconDeclarationPrecedence; }
		}

		int IPrelimStatementDetailsDefault.DaysToAddToStatementDate
		{
			get
			{
				int daysToAddToStatementDate = 0;
				ZDate baseDateToCalculateOn = ((IPrelimStatementDetailsDefault)this).BaseDateToCalculateOn;

				var workingDays = CustomsWorkingDays.GetInstance(Factory);
				if (baseDateToCalculateOn <= ZDate.Today ||
					!workingDays.IsWorkDay(new DateTime(baseDateToCalculateOn.Year, baseDateToCalculateOn.Month, baseDateToCalculateOn.Day)))
				{
					daysToAddToStatementDate = 1;
				}
				return daysToAddToStatementDate;
			}
		}

		#endregion

		#region IRegistryAccessingSupporter Members

		public virtual Guid RegistryBranchPK
		{
			get { return ReconWrappedJobDeclaration.RegistryBranchPK; }
		}

		public virtual Guid RegistryCompanyPK
		{
			get { return ReconWrappedJobDeclaration.RegistryCompanyPK; }
		}

		#endregion

		#region IClientBranchDesignationDefault Members

		GlbBranch IClientBranchDesignationDefault.Branch
		{
			get { return Branch ?? Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		#endregion

		#region IStatementDeleteTransaction

		ZString IStatementDeleteTransaction.PeriodicStatementMonth
		{
			get { return ZString.Empty; }
		}

		ZString IStatementDeleteTransaction.ClientBranchDesignation
		{
			get { return US_ClientBranchDesignation; }
		}

		ZDateTime IStatementDeleteTransaction.PreliminaryStatementPrintDate
		{
			get { return US_PreliminaryStatementPrintDate; }
		}

		ZDateTime IStatementDeleteTransaction.ReleaseDate
		{
			get { return ZDateTime.Empty; }
		}

		ZString IStatementDeleteTransaction.PaymentType
		{
			get { return US_PaymentType; }
		}

		ZString IStatementDeleteTransaction.EntryFilerCode
		{
			get { return US_EntryFilerCode; }
		}

		ZString IStatementDeleteTransaction.PortOfEntry
		{
			get { return US_SchDEntry; }
		}

		public ZString PreparerDistrictPort
		{
			get { return Branch == null ? string.Empty : USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(Branch.GB_GC.ToGuid(), Branch.PK.ToGuid(), Guid.Empty); }
		}

		ZString IStatementDeleteTransaction.ProcessingPort
		{
			get { return US_SchDEntry; }
		}

		ZString IStatementDeleteTransaction.PreparerPort
		{
			get { return PreparerDistrictPort; }
		}

		ZString IStatementDeleteTransaction.PreparerOfficeCode
		{
			get { return Branch == null ? string.Empty : USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty); }
		}

		bool IStatementDeleteTransaction.ShouldPopulatePreparerSite
		{
			get { return IsRemoteLocationFiling; }
		}

		bool IStatementDeleteTransaction.IsACE
		{
			get { return IsACE; }
		}

		ZString IStatementDeleteTransaction.EntryNumber
		{
			get { return ReconEntryNumber; }
		}

		void IStatementDeleteTransaction.AddMessages(MQEDIMessage message)
		{
			((IMessageAttachee)this).Messages.Add(message);
		}

		bool IStatementDeleteTransaction.ShouldGenerateACEStatementMessage
		{
			get { return true; }
		}

		bool IStatementDeleteTransaction.IsStatementUpdateMessagePending
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region INeedDataSet Members

		System.Data.DataSet INeedDataSet.Data // SuppressCodeSmell Reason = DataSet is a return type of this interface member.
		{
			get { return ((INeedDataSet)ReconWrappedJobDeclaration).Data; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new ReconDeclarationInvoicingSupporter(this)); }
		}
		ReconDeclarationInvoicingSupporter fInvoicingSupporter;

		#endregion

		#region IJobHeaderParent Members

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return Factory; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get { return ReconWrappedJobDeclaration.PK; }
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			ReconWrappedJobDeclaration.PopulateJE_DeclarationReferenceIfNeeded();
		}

		string IJobHeaderParentCore.TableName
		{
			get { return JobDeclarationSchema.Constants.TableName; }
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return ReconWrappedJobDeclaration.JE_DeclarationReference; }
		}

		#endregion

		public CusStatementHeader RelatedStatement
		{
			get
			{
				if (!ReconEntry.EntryNumber.IsEmpty && relatedStatement == null && Branch != null)
				{
					relatedStatement = new CusStatementHeader.Loader(Factory).Load(US_EntryFilerCode, ReconEntry.EntryNumber, Branch.GB_GC);
				}

				return relatedStatement;
			}
		}
		CusStatementHeader relatedStatement;

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.StatementList))]
		public ZGuid RelatedStatementPK
		{
			get
			{
				var result = RelatedStatement;
				return result != null ? result.PK : ZGuid.Empty;
			}
		}

		#region ICustomsJobInfo Members

		GlbBranch ICustomsJobInfo.Branch
		{
			get { return Branch ?? Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		IJobInvoicingPlugIn ICustomsJobInfo.TopLevelObjectForJobToReference
		{
			get { return this; }
		}

		AutoPostingNotification ICustomsJobInfo.AutoPostingNotification
		{
			get
			{
				var emailRecipients = new List<ZGuid>();
				GlbBranch branch = Branch;
				var groupNotificaiton = branch != null ? CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty) : null;
				GlbStaff staff = CusAgent;
				if (staff != null && !staff.GS_EmailAddress.IsEmpty)
				{
					emailRecipients.Add(staff.PK);
				}
				else
				{
					if (groupNotificaiton != null)
					{
						ZGuid notificationGroup = groupNotificaiton.SendGroupPK;
						if (notificationGroup.IsValid)
						{
							emailRecipients.Add(notificationGroup);
						}
					}
				}

				return new AutoPostingNotification(emailRecipients.ToArray(), groupNotificaiton?.SuppressUnpostARNotificaiton ?? false);
			}
		}

		ZString[] ICustomsJobInfo.GetValidAPInvoiceNumsToMatchAndValidateAgainst()
		{
			return Array.Empty<ZString>();
		}

		ZGuid ICustomsJobInfo.CreditorPK
		{
			get
			{
				var org = Factory.Load<OrgHeader>(RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty));
				return org == null ? ZGuid.Empty : org.PK;
			}
		}

		#region Entries

		public EntryInfoCollection Entries
		{
			get
			{
				var result = new EntryInfoCollection();

				foreach (ReconOriginalEntryHeader ent in OriginalEntries)
				{
					if (ent.Invoice != null)
					{
						result.AddNew(ent.Invoice.InvoiceLines.Count, 0, ReconWrappedJobDeclaration.TotalFOBInLocalCurrency.Amount);
					}
				}
				return result;
			}
		}

		#endregion

		public Directions JobDirection
		{
			get { return Directions.Import; }
		}

		ICustomsJobInfo ICustomsJobInfoProvider.GetCustomsJobInfo(ZGuid companyPK)
		{
			return this;
		}

		#endregion

		#region IServiceLocator Members

		public virtual object GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				return new ReconDeclarationCustomsCharges(this);
			}
			return null;
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return ReconWrappedJobDeclaration.PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.US.Recon; }
		}

		#endregion

		#region IMessageAttachee

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return ReconEntry.Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return ReconWrappedJobDeclaration; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return ReconWrappedJobDeclaration.JE_DeclarationReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return ReconWrappedJobDeclaration.Logs; }
		}

		#endregion

		#region IBondDetailsDefault Members

		IDisposable IBondDetailsDefault.SuspendAddInfoPropertySetting
		{
			get { return null; }
		}

		ZBool IBondDetailsDefault.IsReconMessageType
		{
			get { return true; }
		}

		public OrgHeaderWrapper IORWrapper
		{
			get { return OrgHeaderWrapper.New(ImporterOfRecord); }
		}

		ZString IBondDetailsDefault.ActivityCode
		{
			get { return ActivityCodeList.Codes._1; }
		}

		ZDateTime IBondDetailsDefault.EffectiveDate
		{
			get { return new DutyFeeDateCalculator().GetDutyFeeDateForBond(ReconWrappedJobDeclaration); }
		}

		ZString IBondDetailsDefault.EntryType
		{
			get { return EntryTypeList.Codes.ReconciliationSummary; }
		}

		ZDecimal IBondDetailsDefault.US_BondAmount
		{
			set { }
		}

		ZString IBondDetailsDefault.US_BondType
		{
			get { return BondTypeList.Codes.ContinuousBond; }
			set { }
		}

		ZString IBondDetailsDefault.US_BondProducerAccNo
		{
			set { }
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return ReconWorkflowHelper.WorkflowType; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new JobDeclarationProcessTaskCollection<JobDeclaration>(ReconWrappedJobDeclaration));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return ReconWorkflowHelper.GetTemplateSelectionCriteria(ReconWrappedJobDeclaration);
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IAllocateNumberSupporter

		void IAllocateNumberSupporter.UnlockNumberAllocationMutex()
		{
			this.ReconWrappedJobDeclaration.UnlockImportEntryNumberAllocationMutex();
		}

		bool IAllocateNumberSupporter.LockNumberAllocationMutex
		{
			get { return this.ReconWrappedJobDeclaration.LockImportEntryNumberAllocationMutex; }
		}

		string IAllocateNumberSupporter.GetNumberAllocationMutexLockInfo()
		{
			return this.ReconWrappedJobDeclaration.GetImportEntryNumberAllocationMutexLockInfo();
		}

		ZString IAllocateNumberSupporter.GetReasonToStopProceeding()
		{
			var currectReconEntryNumber = ReconEntryNumber;
			this.ReconWrappedJobDeclaration.ReloadExistingDataRelatedToImportEntryNumberAllocation();
			var result = ACEEntryStmNumsSetting.GetAnyReasonForNotAbleToAllocateNumber(Branch, US_EntryFilerCode);

			if (string.IsNullOrEmpty(result))
			{
				var reconEntryNumber = ReconEntryNumber;
				if (!reconEntryNumber.IsEmpty && (HasBeenLodgedAtCustoms || IsWaitingForRespones))
				{
					result = JobDeclaration.Constants.DisallowEntryNumberAllocation.EntryNumberAlreadyAllocated(reconEntryNumber);
				}
				if (currectReconEntryNumber != reconEntryNumber)
				{
					ReconEntryNumberInfo.RefreshBinding(currectReconEntryNumber);
				}
			}

			return result;
		}

		ZString IAllocateNumberSupporter.GetExistingNumber()
		{
			return ReconEntryNumber;
		}

		AllocateNumber IAllocateNumberSupporter.GetNewAllocateNumber()
		{
			var args = new AllocateNumberArgs();
			args.MaxLength = 8;
			args.EntryFilerCode = US_EntryFilerCode;
			args.Branch = Branch;
			args.Factory = Factory;
			args.ValidateNumber = (info, validator) => validator.ValidateFormalEntryNumber(info);

			return new AllocateNumber(args);
		}

		void IAllocateNumberSupporter.DoAllocate(ZString userEnteredNumber)
		{
			this.ReconWrappedJobDeclaration.AllocateEntryNumber(userEnteredNumber);
		}

		void IAllocateNumberSupporter.OnAllocatedNumberSaved()
		{
			ReconEntryNumberInfo.RefreshBinding();
		}

		ZString IAllocateNumberSupporter.NumberType
		{
			get { return "Entry Number"; }
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new ReconDeclarationRatingAdaptersProvider(this); }
		}

		#endregion

		#region IRatingSupporterWithAdapter Members

		IAutoRating IRatingSupporterWithAdapter.RatingAdapter
		{
			get { return new ReconDeclarationRatingAdapter(this); }
		}

		#endregion

		#region ICancellable Members

		public string CanCancel()
		{
			var result = JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(ReconWrappedJobDeclaration.PK, HumanReadableName);

			if (string.IsNullOrEmpty(result) && !DoesCustomsEntryStatusAllowCancellation)
			{
				result = ReasonUnableToCancel;
			}
			return result;
		}
		internal const string ReasonUnableToCancel = "This reconciliation job must not be deactivated because this job has been accepted by Customs or is awaiting a response from Customs. If it has been accepted and you need to deactivate this job, then you need to send a delete message before it can be deactivated.";

		protected bool DoesCustomsEntryStatusAllowCancellation
		{
			get
			{
				var entry = ReconEntry.GetEntry();
				return !entry.IsWaitingForResponse && (!entry.HasBeenLodgedAtCustoms || entry.HasBeenWithdrawn);
			}
		}

		public string CanReactivate()
		{
			return null;
		}

		public bool IsCancelled
		{
			get { return ReconWrappedJobDeclaration.JE_IsCancelled; }
			set
			{
				ReconWrappedJobDeclaration.JE_IsCancelled = value;
				if (value)
				{
					JobHeader.DeactivateAllJobs(this, true);
				}
			}
		}

		public bool IsCancelledHasChanged
		{
			get { return ReconWrappedJobDeclaration.JE_IsCancelledInfo.HasChanges; }
		}

		void UpdateReadOnlyForWhenCancelled()
		{
			if (!IsDeleted)
			{
				((IBusiness)this).SetCountedReadOnlyIncludingChildren(ReconWrappedJobDeclaration.JE_IsCancelled);
			}
		}

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);
			if (!IsDeleted && ReconWrappedJobDeclaration.JE_IsCancelled)
			{
				child.IncrementReadOnlyIncludingChildren();
			}
		}

		#endregion

		#region ICustomFieldProvider Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var declarationProvider = (ICustomFieldProvider)ReconWrappedJobDeclaration;
			return declarationProvider?.GetCustomBusinessObject(shouldRefresh);
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (!ReconWrappedJobDeclaration.SuspendAddingWorkflow)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}
			base.OnFactorySavingBeforeTransactionCore();
		}

		public override string TablePrefix
		{
			get { return ReconWrappedJobDeclaration.TablePrefix; }
		}

		public bool IsChangedForReconForOneLine()
		{
			return InvoiceLines.OfType<JobComInvoiceLine>().Any(line => line.IsChangedForReconForOneLine());
		}

		IStaff IReconDeclaration.CusAgent
		{
			get { return CusAgent; }
		}

		IUSOrgHeader IReconDeclaration.ImporterOfRecord
		{
			get { return ImporterOfRecord; }
		}

		IUSIORWrapper IReconDeclaration.IORWrapper
		{
			get { return IORWrapper; }
		}
	}

	public class ReconDeclarationInvoicingSupporter : JobInvoicingSupporter, IServiceDirection
	{
		public ReconDeclarationInvoicingSupporter(ReconDeclaration parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly ReconDeclaration Parent;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return ((IJobInvoicingPlugIn)Parent.ReconWrappedJobDeclaration).InvoicingSupporter.AuditSecurity;
		}

		public override ZString ConsolType
		{
			get { return Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Brokerage; }
		}

		protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
		{
			return ((IJobInvoicingPlugIn)Parent.ReconWrappedJobDeclaration).InvoicingSupporter.EditSecurityCheckpoint;
		}

		public override bool EditSecurityLock
		{
			get { return ((IJobInvoicingPlugIn)Parent.ReconWrappedJobDeclaration).InvoicingSupporter.EditSecurityLock; }
		}

		public override ZString EditSecurityMessage
		{
			get { return ((IJobInvoicingPlugIn)Parent.ReconWrappedJobDeclaration).InvoicingSupporter.EditSecurityMessage; }
		}

		public override bool IsImport
		{
			get { return true; }
		}

		public override OrgHeader Consignee
		{
			get { return Parent.Importer; }
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return ((IJobInvoicingPlugIn)Parent.ReconWrappedJobDeclaration).InvoicingSupporter.JobInvoicingSecurity;
		}

		public override ZGuid OverriddenDepartmentPK
		{
			get { return ((IJobInvoicingPlugIn)Parent.ReconWrappedJobDeclaration).InvoicingSupporter.OverriddenDepartmentPK; }
		}

		public override string GetReasonNotToAllowChangeOnCostDetails(ZGuid chargeCodePK)
		{
			string result = null;

			if (CustomsDataRegistry.Instance.EnableAccountingIntegration.Value.EnableAccountingIntegration && RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetAllChargeCodesIncludingDefault().Contains(chargeCodePK))
			{
				if (Parent.US_PaymentType != PaymentTypeList.Codes.IndividualBasis)
				{
					CusStatementHeader relatedStatement = Parent.RelatedStatement;
					if (relatedStatement == null || relatedStatement.B2_Status != StatementHeaderStatusList.Codes.Final)
					{
						result = JobDeclarationInvoicingSupporter.ShouldNotChangeCostOrPostWhileOnStatement;
					}
				}
			}

			return result;
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			ZDateTime result = ZDateTime.Empty;

			if (significantDateCode == AccountingMasterFilesConstants.SignificantDateCodes.CustomsClearanceDate)
			{
				result = GetCustomsClearanceDate();
			}

			return result;
		}

		public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return GetOperationsSignificantDate(significantDateCode);
		}

		public override ZDateTime GetCustomsClearanceDate()
		{
			ZDateTime result = ZDateTime.Empty;

			StmALog mostRecentClearedLog = Parent.Logs.MostRecentLogByEventTime(Events.CustomsCleared);

			if (mostRecentClearedLog != null)
			{
				result = mostRecentClearedLog.SL_EventTime;
			}

			return result;
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Parent.IsInDatabaseIncludingChildren; }
		}

		public ZString ServiceDirection
		{
			get
			{
				return Parent.ReconWrappedJobDeclaration.JE_MessageType;
			}
		}
	}
}
