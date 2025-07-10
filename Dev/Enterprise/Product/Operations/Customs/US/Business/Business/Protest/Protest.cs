using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Protest
{
	[TestExcludeWorkflowProviderHasTestCase]
	[CodeProperty(Schema.US_P_CBPAssignedProtestNumber), DescriptionProperty(Schema.US_P_CBPAssignedProtestNumber)]
	public partial class Protest : NonPersistentBusinessObjectWithLogsAndNotes,
		ICustomsJobInfo,
		ICustomsJobInfoProvider,
		IWorkflowProvider,
		IEDocsProvider,
		IEDocsPluginHostDecider,
		IAllocateNumberSupporter,
		IControllerIDProvider,
		IWrapPersistentBizO
	{
		#region Schema

		public static class Schema
		{
			public const string BrokerName = "BrokerName";
			public const string FilingDDPPName = "FilingDDPPName";
			public const string FilerReferenceNo = "FilerReferenceNo";
			public const string TariffActCitation = "TariffActCitation";
			public const string US_P_CBPAssignedProtestNumber = "US_P_CBPAssignedProtestNumber";
			public const string ProtestNumber = "Protest #";
			public const string LinkedEntriesConcatenatedList = "LinkedEntriesConcatenatedList";
			public const string ProtestantNameForModuleFilter = "ProtestantNameForModuleFilter";
			public const string JustificationNote = "JustificationNote";
			public const string RefundPartyOrgPK = "RefundPartyOrgPK";
			public const string TableName = JobDeclaration.Schema.TableName;
		}

		#endregion

		#region Constants

		public static class Constants
		{
			public static string CBPNumberAlreadyAllocated(string number)
			{
				return string.Format("A CBP Assigned Number ({0}) is already allocated to this job.\nOnce CBP number is used in the message, a new number should not be allocated.", number);
			}
		}

		#endregion

		public Protest(JobDeclaration declaration)
			: base(declaration.Factory, ((INeedRow)declaration).Row)
		{
			this.Declaration = declaration;
			Declaration.Protest = this;
			RegisterEditableChildObject(Declaration);

			//note: This can't be in SetDefaultValues because being backed by a persistent BizO means you have a non-Empty PK from the get go
			if (!IsInDatabase)
			{
				using (SuspendSettingHasChanges())
				using (Declaration.SuspendSettingHasChanges())
				{
					Declaration.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
					Declaration.JE_RS_NKServiceLevel = ZString.Empty;
					US_P_FilingDDPP = USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetValueWithoutFallback(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty);
					TariffActCitation = TariffActCitationList.Codes.A_Section514;

					if (!USCustomsDataRegistry.Instance.EntryDeclarant.GetFallBackValueAtAllLevels(Declaration.RegistryCompanyPK, Declaration.RegistryBranchPK, Guid.Empty))
					{
						if (!GlbStaff.CurrentUser.GS_IsSystemAccount)
						{
							JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
						}
					}
				}
			}
		}

		public Protest(BusinessObjectFactory factory, System.Data.DataRow row)
			: this(new JobDeclaration(factory, row))
		{
		}

		#region Declaration

		public readonly JobDeclaration Declaration;
		BusinessObject IWrapPersistentBizO.Parent => Declaration;

		#endregion

		#region Overrides

		public override CargoWise.Schema.SchemaGuidColumn PKSchemaColumn
		{
			get { return JobDeclarationSchema.PK; }
		}

		protected override BusinessObject LogsAndNotesTarget
		{
			get { return Declaration; }
		}

		public override void Delete()
		{
			base.Delete();
			Declaration.Delete();

			if (justificationStmNote != null)
			{
				justificationStmNote.Delete();
				justificationStmNote = null;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (JustificationNote.IsEmpty && justificationStmNote != null)
			{
				justificationStmNote.Delete();
				justificationStmNote = null;
				justificationNote = null;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateEntries();
		}

		#endregion

		#region FetchStrategy

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new Strategy(this);
		}

		class Strategy : BusinessObjectFetchStrategy
		{
			public Strategy(Protest protest)
				: base(protest)
			{
			}

			Protest protest
			{
				get { return (Protest)base.BusinessObject; }
			}

			JobDeclaration declaration
			{
				get { return protest.Declaration; }
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
				base.FetchForViewCore(columns);
				declaration.FetchStrategy.FetchForView(columns);
				foreach (var column in columns)
				{
					switch (column.ColumnName)
					{
						case Protest.Schema.ProtestantNameForModuleFilter:
							Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, declaration.PK);
							break;
						case Protest.Schema.RefundPartyOrgPK:
							Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, declaration.PK);
							break;
						case Protest.Schema.US_P_CBPAssignedProtestNumber:
							Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, declaration.PK);
							break;
						case Protest.Schema.LinkedEntriesConcatenatedList:
							Factory.AddFetchHint(CusAddInfoSchema.Instance, new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USLinkedEntry), new ZQuery(CusAddInfoSchema.B7_ParentID, declaration.PK));
							break;
						case Protest.Schema.FilingDDPPName:
							Factory.AddFetchHint(ZZRefCusCodeListCombinedSchema.ZZD_Code, protest.US_P_FilingDDPP);
							break;
					}
				}
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				declaration.FetchStrategy.FetchForLoadChildEditableObjects();
			}
		}

		#endregion

		#region Bindable Properties

		#region FilerReferenceNo

		[MaxLength(12)]
		public ZString FilerReferenceNo
		{
			get { return Declaration.JE_DeclarationReference; }
		}

		public ZPropertyInfo FilerReferenceNoInfo
		{
			get { return GetZPropertyInfo(Schema.FilerReferenceNo); }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.Branches))]
		public ZGuid JE_GB
		{
			get { return Declaration.JE_GB; }
			set { Declaration.JE_GB = value; }
		}

		public ZPropertyInfo JE_GBInfo
		{
			get { return GetWrappedZPropertyInfo(JobDeclarationSchema.JE_GB.Name, x => Declaration.JE_GBInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.CusAgents))]
		public ZString JE_GS_NKCusAgent
		{
			get { return Declaration.JE_GS_NKCusAgent; }
			set { Declaration.JE_GS_NKCusAgent = value; }
		}

		public ZPropertyInfo JE_GS_NKCusAgentInfo
		{
			get { return GetWrappedZPropertyInfo(JobDeclarationSchema.JE_GS_NKCusAgent.Name, x => Declaration.JE_GS_NKCusAgentInfo); }
		}

		#region US_P_ApplicationFurtherReview

		public ZBool US_P_ApplicationFurtherReview
		{
			get { return AddInfo.US_P_ApplicationFurtherReview; }
			set { AddInfo.US_P_ApplicationFurtherReview = value; }
		}

		public ZPropertyInfo US_P_ApplicationFurtherReviewInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_ApplicationFurtherReview, x => AddInfo.US_P_ApplicationFurtherReviewInfo); }
		}

		#endregion

		#region US_P_AcceleratedDispositionInd

		public ZBool US_P_AcceleratedDispositionInd
		{
			get { return AddInfo.US_P_AcceleratedDispositionInd; }
			set { AddInfo.US_P_AcceleratedDispositionInd = value; }
		}

		public ZPropertyInfo US_P_AcceleratedDispositionIndInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_AcceleratedDispositionInd, x => AddInfo.US_P_AcceleratedDispositionIndInfo); }
		}

		#endregion

		#region US_P_HardCopySent

		public ZBool US_P_HardCopySent
		{
			get { return AddInfo.US_P_HardCopySent; }
			set { AddInfo.US_P_HardCopySent = value; }
		}

		public ZPropertyInfo US_P_HardCopySentInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_HardCopySent, x => AddInfo.US_P_HardCopySentInfo); }
		}

		#endregion

		#region US_P_SampleSent

		public ZBool US_P_SampleSent
		{
			get { return AddInfo.US_P_SampleSent; }
			set { AddInfo.US_P_SampleSent = value; }
		}

		public ZPropertyInfo US_P_SampleSentInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_SampleSent, x => AddInfo.US_P_SampleSentInfo); }
		}

		#endregion

		#region US_P_FaxSent

		public ZBool US_P_FaxSent
		{
			get { return AddInfo.US_P_FaxSent; }
			set { AddInfo.US_P_FaxSent = value; }
		}

		public ZPropertyInfo US_P_FaxSentInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_FaxSent, x => AddInfo.US_P_FaxSentInfo); }
		}

		#endregion

		#region US_P_FaxSentDate

		public ZDateTime US_P_FaxSentDate
		{
			get { return AddInfo.US_P_FaxSentDate; }
			set { AddInfo.US_P_FaxSentDate = value; }
		}

		public ZPropertyInfo US_P_FaxSentDateInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_FaxSentDate, x => AddInfo.US_P_FaxSentDateInfo); }
		}

		#endregion

		#region US_P_FilingDDPP

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.SchDPortList))]
		public ZString US_P_FilingDDPP
		{
			get { return AddInfo.US_P_FilingDDPP; }
			set { AddInfo.US_P_FilingDDPP = value; }
		}

		public ZPropertyInfo US_P_FilingDDPPInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_FilingDDPP, x => AddInfo.US_P_FilingDDPPInfo); }
		}

		#endregion

		#region US_P_AddressTeam

		public ZString US_P_AddressTeam
		{
			get { return AddInfo.US_P_AddressTeam; }
			set { AddInfo.US_P_AddressTeam = value; }
		}

		public ZPropertyInfo US_P_AddressTeamInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_AddressTeam, x => AddInfo.US_P_AddressTeamInfo); }
		}

		#endregion

		#region TariffActCitation

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.TariffActCitations))]
		[MaxLength(1)]
		public ZString TariffActCitation
		{
			get { return Declaration.JE_MessageSubType; }
			set
			{
				CheckMaximumLength(TariffActCitationInfo, value);
				Declaration.JE_MessageSubType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTariffActCitation();
				}
				TariffActCitationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TariffActCitationInfo
		{
			get { return GetZPropertyInfo(Schema.TariffActCitation); }
		}

		internal bool Is181115Intervention
		{
			get { return TariffActCitation == TariffActCitationList.Codes.D_Section181; }
		}

		#endregion

		#region US_P_Assoc514ProtestNo

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.Associated514Protests))]
		public ZString US_P_Assoc514ProtestNo
		{
			get { return AddInfo.US_P_Assoc514ProtestNo; }
			set { AddInfo.US_P_Assoc514ProtestNo = value; }
		}

		public ZPropertyInfo US_P_Assoc514ProtestNoInfo
		{
			get { return (AddInfo == null) ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_Assoc514ProtestNo, x => AddInfo.US_P_Assoc514ProtestNoInfo); }
		}

		#endregion

		#region US_P_Assoc520PetitionNo

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.Associated5204Protests))]
		public ZString US_P_Assoc520PetitionNo
		{
			get { return AddInfo.US_P_Assoc520PetitionNo; }
			set { AddInfo.US_P_Assoc520PetitionNo = value; }
		}

		public ZPropertyInfo US_P_Assoc520PetitionNoInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_Assoc520PetitionNo, x => AddInfo.US_P_Assoc520PetitionNoInfo); }
		}

		#endregion

		#region US_P_PeriodBaseDate

		public ZDateTime US_P_PeriodBaseDate
		{
			get { return AddInfo.US_P_PeriodBaseDate; }
			set { AddInfo.US_P_PeriodBaseDate = value; }
		}

		public ZPropertyInfo US_P_PeriodBaseDateInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_PeriodBaseDate, x => AddInfo.US_P_PeriodBaseDateInfo); }
		}

		#endregion

		#region US_P_PeriodBaseDateQualifier

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.ProtestPeriodBaseDateQualifiers))]
		public ZString US_P_PeriodBaseDateQualifier
		{
			get { return AddInfo.US_P_PeriodBaseDateQualifier; }
			set { AddInfo.US_P_PeriodBaseDateQualifier = value; }
		}

		public ZPropertyInfo US_P_PeriodBaseDateQualifierInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_PeriodBaseDateQualifier, x => AddInfo.US_P_PeriodBaseDateQualifierInfo); }
		}

		#endregion

		#region US_P_ApplicationQuestion1

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.FurtherReviewAnswers))]
		public ZString US_P_ApplicationQuestion1
		{
			get { return AddInfo.US_P_ApplicationQuestion1; }
			set { AddInfo.US_P_ApplicationQuestion1 = value; }
		}

		public ZPropertyInfo US_P_ApplicationQuestion1Info
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_ApplicationQuestion1, x => AddInfo.US_P_ApplicationQuestion1Info); }
		}

		#endregion

		#region US_P_ApplicationQuestion2

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.FurtherReviewAnswers))]
		public ZString US_P_ApplicationQuestion2
		{
			get { return AddInfo.US_P_ApplicationQuestion2; }
			set { AddInfo.US_P_ApplicationQuestion2 = value; }
		}

		public ZPropertyInfo US_P_ApplicationQuestion2Info
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_ApplicationQuestion2, x => AddInfo.US_P_ApplicationQuestion2Info); }
		}

		#endregion

		#region US_P_ApplicationQuestion3

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.FurtherReviewAnswers))]
		public ZString US_P_ApplicationQuestion3
		{
			get { return AddInfo.US_P_ApplicationQuestion3; }
			set { AddInfo.US_P_ApplicationQuestion3 = value; }
		}

		public ZPropertyInfo US_P_ApplicationQuestion3Info
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_ApplicationQuestion3, x => AddInfo.US_P_ApplicationQuestion3Info); }
		}

		#endregion

		#region US_P_InternalAdviceNo

		public ZString US_P_InternalAdviceNo
		{
			get { return AddInfo.US_P_InternalAdviceNo; }
			set { AddInfo.US_P_InternalAdviceNo = value; }
		}

		public ZPropertyInfo US_P_InternalAdviceNoInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_InternalAdviceNo, x => AddInfo.US_P_InternalAdviceNoInfo); }
		}

		#endregion

		#region US_P_LeadProtestNo

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.LeadProtests))]
		public ZString US_P_LeadProtestNo
		{
			get { return AddInfo.US_P_LeadProtestNo; }
			set { AddInfo.US_P_LeadProtestNo = value; }
		}

		public ZPropertyInfo US_P_LeadProtestNoInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_LeadProtestNo, x => AddInfo.US_P_LeadProtestNoInfo); }
		}

		#endregion

		#region US_P_TestSummonsNo

		public ZString US_P_TestSummonsNo
		{
			get { return AddInfo.US_P_TestSummonsNo; }
			set { AddInfo.US_P_TestSummonsNo = value; }
		}

		public ZPropertyInfo US_P_TestSummonsNoInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_TestSummonsNo, x => AddInfo.US_P_TestSummonsNoInfo); }
		}

		#endregion

		#region US_P_ProtestantType

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.ProtestantTypes))]
		public ZString US_P_ProtestantType
		{
			get { return AddInfo.US_P_ProtestantType; }
			set { AddInfo.US_P_ProtestantType = value; }
		}

		public ZPropertyInfo US_P_ProtestantTypeInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_ProtestantType, x => AddInfo.US_P_ProtestantTypeInfo); }
		}

		#endregion

		#region Protestant

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.Organisations))]
		public JobDocAddress Protestant
		{
			get
			{
				if (protestant == null || protestant.IsDeleted)
				{
					protestant = Declaration.DocAddresses.FindOrCreateWithRequirement(ProtestantAddressRequirement);
				}

				return protestant;
			}
		}
		JobDocAddress protestant;

		#region Protestant Address Requirement

		JobDocAddressRequirement ProtestantAddressRequirement
		{
			get
			{
				if (protestantAddressRequirement == null)
				{
					protestantAddressRequirement = new JobDocAddressRequirement(DocAddressType.ProtestantAddress, ContactType.Miscellaneous);

					protestantAddressRequirement.GetRegistrationNumberResult = Validation.GetRegistrationNumberResult;
					protestantAddressRequirement.ValidateOrganisationPK = Validation.ValidateProtestantOrganisationPK;
					protestantAddressRequirement.ValidateCountry = Validation.ValidateCountry;
					protestantAddressRequirement.ValidateGovRegNo = Validation.ValidateGovRegNo;
					Declaration.DocAddressManager.AddRequirement(protestantAddressRequirement);
				}
				return protestantAddressRequirement;
			}
		}
		JobDocAddressRequirement protestantAddressRequirement;

		#endregion

		#endregion

		#region Refund Party

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.ProtestRefundCOPartyTypes))]
		public ZString US_P_RefundCOPartyType
		{
			get { return AddInfo.US_P_RefundCOPartyType; }
			set { AddInfo.US_P_RefundCOPartyType = value; }
		}

		public ZPropertyInfo US_P_RefundCOPartyTypeInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_RefundCOPartyType, x => AddInfo.US_P_RefundCOPartyTypeInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.Organisations))]
		public JobDocAddress RefundPartyAddress
		{
			get
			{
				if (refundPartyAddress == null || refundPartyAddress.IsDeleted)
				{
					refundPartyAddress = Declaration.DocAddresses.FindOrCreateWithRequirement(RefundPartyAddressRequirement);
				}

				return refundPartyAddress;
			}
		}
		JobDocAddress refundPartyAddress;

		JobDocAddressRequirement RefundPartyAddressRequirement
		{
			get
			{
				if (refundPartyAddressRequirement == null)
				{
					refundPartyAddressRequirement = new JobDocAddressRequirement(DocAddressType.RefundParty, ContactType.Miscellaneous);
					refundPartyAddressRequirement.ValidateOrganisationPK = Validation.ValidateRefundPartyOrganisationPK;
					Declaration.DocAddressManager.AddRequirement(refundPartyAddressRequirement);
				}
				return refundPartyAddressRequirement;
			}
		}
		JobDocAddressRequirement refundPartyAddressRequirement;

		internal bool HasRefundPartyDetails
		{
			get { return !US_P_RefundCOPartyType.IsEmpty || !RefundPartyId.IsEmpty; }
		}

		public ZString RefundPartyId
		{
			get
			{
				OrgCusCode registrationCusCode = null;

				if (RefundPartyAddress.IsValidAddress)
				{
					if (RefundParty != null && RefundParty.CustomsCodes != null)
					{
						registrationCusCode = RefundParty.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(
							Core.Constants.CountryCodes.UnitedStates,
							OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
							OrgCusCode.USACodeTypes.SocialSecurityNumber,
							OrgCusCode.USACodeTypes.CBPAssignedNumber);
					}
				}
				return registrationCusCode?.OK_CustomsRegNo ?? ZString.Empty;
			}
		}

		public OrgHeader RefundParty => Factory.Load<OrgHeader>(RefundPartyOrgPK);

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.Organisations))]
		public ZGuid RefundPartyOrgPK
		{
			get { return RefundPartyAddress.OrganisationPK; }
		}

		public ZPropertyInfo RefundPartyOrgPKInfo
		{
			get { return GetZPropertyInfo(Schema.RefundPartyOrgPK); }
		}

		#endregion

		#region US_P_SubstituteDDPP

		[List(nameof(Lookups) + "." + nameof(ProtestLookups.SchDPortList))]
		public ZString US_P_SubstituteDDPP
		{
			get { return AddInfo.US_P_SubstituteDDPP; }
			set { AddInfo.US_P_SubstituteDDPP = value; }
		}

		public ZPropertyInfo US_P_SubstituteDDPPInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_SubstituteDDPP, x => AddInfo.US_P_SubstituteDDPPInfo); }
		}

		public ZString US_P_SubstituteFilerCode
		{
			get { return AddInfo.US_P_SubstituteFilerCode; }
			set { AddInfo.US_P_SubstituteFilerCode = value; }
		}

		public ZPropertyInfo US_P_SubstituteFilerCodeInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_SubstituteFilerCode, x => AddInfo.US_P_SubstituteFilerCodeInfo); }
		}

		public ZString US_P_SubstituteOfficeCode
		{
			get { return AddInfo.US_P_SubstituteOfficeCode; }
			set { AddInfo.US_P_SubstituteOfficeCode = value; }
		}

		public ZPropertyInfo US_P_SubstituteOfficeCodeInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_SubstituteOfficeCode, x => AddInfo.US_P_SubstituteOfficeCodeInfo); }
		}

		internal bool HasSubstitutePartyDetails
		{
			get { return !US_P_SubstituteDDPP.IsEmpty || !US_P_SubstituteFilerCode.IsEmpty || !US_P_SubstituteOfficeCode.IsEmpty; }
		}

		#endregion

		#region US_P_ProtestedDecision

		public ZString US_P_ProtestedDecision
		{
			get { return AddInfo.US_P_ProtestedDecision; }
			set { AddInfo.US_P_ProtestedDecision = value; }
		}

		public ZPropertyInfo US_P_ProtestedDecisionInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_ProtestedDecision, x => AddInfo.US_P_ProtestedDecisionInfo); }
		}

		#endregion

		#region US_P_MerchandiseDesc

		public ZString US_P_MerchandiseDesc
		{
			get { return AddInfo.US_P_MerchandiseDesc; }
			set { AddInfo.US_P_MerchandiseDesc = value; }
		}

		public ZPropertyInfo US_P_MerchandiseDescInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_MerchandiseDesc, x => AddInfo.US_P_MerchandiseDescInfo); }
		}

		#endregion

		#region US_P_CBPAssignedProtestNumber

		[MaxLength(12)]
		public ZString US_P_CBPAssignedProtestNumber
		{
			get { return ProtestEntryNumber == null ? ZString.Empty : ProtestEntryNumber.CE_EntryNum; }
			set
			{
				CheckMaximumLength(US_P_CBPAssignedProtestNumberInfo, value);

				if (ProtestEntryNumber == null && !value.IsEmpty)
				{
					protestEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.UnitedStates.Protest, Core.Constants.CountryCodes.UnitedStates);
				}

				if (ProtestEntryNumber != null)
				{
					ProtestEntryNumber.CE_EntryNum = value;
				}

				US_P_CBPAssignedProtestNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo
			US_P_CBPAssignedProtestNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_P_CBPAssignedProtestNumber); }
		}

		public void ReloadProtestEntryNumber()
		{
			if (Declaration.IsInDatabase)
			{
				var oldValue = protestEntryNumber == null ? ZString.Empty : protestEntryNumber.CE_EntryNum;
				if (protestEntryNumber == null)
				{
					protestEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.UnitedStates.Protest, Core.Constants.CountryCodes.UnitedStates, true); // reload in case another users has assigned a number
				}
				else if (protestEntryNumber.IsInDatabase)
				{
					protestEntryNumber.Reload();
				}

				if (protestEntryNumber != null && !protestEntryNumber.IsDeleted && protestEntryNumber.CE_EntryNum != oldValue)
				{
					US_P_CBPAssignedProtestNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		CusEntryNumber ProtestEntryNumber
		{
			get { return protestEntryNumber ?? (protestEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.UnitedStates.Protest, Core.Constants.CountryCodes.UnitedStates)); }
		}
		CusEntryNumber protestEntryNumber;

		public bool US_P_CBPAssignedProtestNumber_ReadOnly
		{
			get { return true; }
		}

		internal void AllocateCBPAssignedNumber(string userEnteredNumber)
		{
			if (!string.IsNullOrEmpty(userEnteredNumber))
			{
				US_P_CBPAssignedProtestNumber = userEnteredNumber;
				US_P_CBPAssignedProtestNumberInfo.RefreshBinding();
			}
		}

		#endregion

		#region Justification Note

		[MaxLength(720000)]
		public ZString JustificationNote
		{
			get
			{
				if (!justificationNote.HasValue)
				{
					var query = new ZQuery(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
					query.AddToFilter(StmNoteSchema.ST_ParentID, Declaration.PK);
					query.AddToFilter(StmNoteSchema.ST_Table, Declaration.TableName);
					query.AddToFilter(StmNoteSchema.ST_Description, JustificationNoteDescription);
					query.FetchOnlyFromLocalCache = !Declaration.IsInDatabase;
					justificationStmNote = Factory.LoadTop1<StmNote>(query);

					justificationNote = justificationStmNote != null ?
										justificationStmNote.ST_NoteDataAsText : ZString.Empty;
				}
				return justificationNote.Value;
			}
			set
			{
				if (JustificationNote != value)
				{
					CheckMaximumLength(JustificationNoteInfo, value);
					justificationNote = value;
					HasChanges = true;

					if (!justificationNote.Value.IsEmpty && justificationStmNote == null)
					{
						justificationStmNote = Factory.New<StmNote>();
						justificationStmNote.ST_ParentID = Declaration.PK;
						justificationStmNote.ST_Table = Declaration.TableName;
						justificationStmNote.ST_NoteType = nameof(StmNoteVisibility.DOC);
						justificationStmNote.ST_Description = JustificationNoteDescription;
					}

					justificationStmNote.ST_NoteDataAsText = justificationNote.Value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJustificationNote();
					}
					JustificationNoteInfo.RefreshBinding();
					justificationNoteFormatted = null;
				}
			}
		}
		ZString? justificationNote;
		StmNote justificationStmNote;
		const string JustificationNoteDescription = "Justification Note";

		public ZPropertyInfo JustificationNoteInfo
		{
			get { return GetZPropertyInfo(Schema.JustificationNote); }
		}

		ZString JustificationNoteFormatted
		{
			get
			{
				if (!justificationNoteFormatted.HasValue)
				{
					justificationNoteFormatted = JustificationNote.Replace("\r", "").Replace("\n", "").TrimEnd(' ');
				}
				return justificationNoteFormatted.Value;
			}
		}
		ZString? justificationNoteFormatted;

		#endregion

		#region Linked Entries

		[ChildEditable(true)]
		public LinkedEntryCollection LinkedEntries
		{
			get { return Declaration.LinkedEntryNumbers; }
		}

		internal bool HasEntriesNotLiquidated
		{
			get
			{
				return (from LinkedEntry entry
						in LinkedEntries
						where entry.US_LE_LiquidationDate.IsEmpty
						select entry).Any();
			}
		}

		#endregion

		#endregion

		#region Protest Status

		public ZString ProtestStatus
		{
			get { return Declaration.JE_EntryStatus; }
		}

		public ZString ProtestStatusDescription
		{
			get { return Declaration.JE_EntryStatusDescription; }
		}

		public ZDateTime US_P_StatusDate
		{
			get { return AddInfo.US_P_StatusDate; }
			set { AddInfo.US_P_StatusDate = value; }
		}

		public ZPropertyInfo US_P_StatusDateInfo
		{
			get { return AddInfo == null ? null : GetWrappedZPropertyInfo(USAddInfoSchema.Constants.US_P_StatusDate, x => AddInfo.US_P_StatusDateInfo); }
		}

		public bool US_P_StatusDate_ReadOnly
		{
			get { return true; }
		}

		public ZString StatusDisposition
		{
			get
			{
				if (statusDispositionCached == null)
				{
					statusDispositionCached = new CachedProperty<ZString>(Factory, delegate
					{
						var result = ZString.Empty;
						var query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ProtestAutomaticNotificationandResponsetoFilerQuery);
						query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
						query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";
						var notificationMessages = new List<MQEDIMessage>(new TypedEnumerable<MQEDIMessage>(Messages.Find(query)));
						if (notificationMessages.Count > 0)
						{
							var latestSSMessage = notificationMessages[0];
							var p12Block = latestSSMessage.MessageBlock.MessageBlocks.OfType<PROP12SSSS>().FirstOrDefault();

							if (p12Block != null)
							{
								result = p12Block.ProcessingStatusCode + " " + p12Block.ProcessingStatusDescription;
							}
						}
						return result;
					});
				}
				return statusDispositionCached.Value;
			}
		}
		CachedProperty<ZString> statusDispositionCached;

		#endregion

		#region Module Grid Properties

		public ZString JobStatus
		{
			get
			{
				var job = Declaration.Job;
				return job != null ? job.JH_Status : ZString.Empty;
			}
		}

		public ZString JE_MessageStatus
		{
			get { return Declaration.JE_MessageStatus; }
		}

		public ZString MessageStatusDescription
		{
			get { return Declaration.JE_MessageStatusDescription; }
		}

		public ZString FilingDDPPName
		{
			get
			{
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_P_FilingDDPP, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				return (port == null) ? ZString.Empty : port.ZZD_Description;
			}
		}

		public ZPropertyInfo FilingDDPPNameInfo
		{
			get { return GetZPropertyInfo(Schema.FilingDDPPName); }
		}

		public ZString LinkedEntriesConcatenatedList
		{
			get
			{
				var result = (LinkedEntries.Count < 5) ?
					new ZStringBuilder(from LinkedEntry le in LinkedEntries select le.US_LE_EntryNumber) :
					new ZStringBuilder("Multiple. Open form to view.");
				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public ZPropertyInfo LinkedEntriesConcatenatedListInfo
		{
			get { return GetZPropertyInfo(Schema.LinkedEntriesConcatenatedList); }
		}

		public ZDateTime JE_SystemCreateTimeUtc
		{
			get { return Declaration.JE_SystemCreateTimeUtc; }
		}

		public ZString JE_SystemCreateUser
		{
			get { return Declaration.JE_SystemCreateUser; }
		}

		public ZString JE_SystemCreateBranch
		{
			get { return Declaration.JE_SystemCreateBranch; }
		}

		public ZString JE_SystemCreateDepartment
		{
			get { return Declaration.JE_SystemCreateDepartment; }
		}

		public ZDateTime JE_SystemLastEditTimeUtc
		{
			get { return Declaration.JE_SystemLastEditTimeUtc; }
		}

		public ZString JE_SystemLastEditUser
		{
			get { return Declaration.JE_SystemLastEditUser; }
		}

		public ZString ActiveStatus
		{
			get { return (Declaration.JE_IsCancelled) ? "Cancelled" : "Active"; }
		}

		public ZString BrokerName
		{
			get { return Declaration.BrokerName; }
		}

		public ZPropertyInfo BrokerNameInfo
		{
			get { return GetZPropertyInfo(Schema.BrokerName); }
		}

		public ZString BranchName
		{
			get { return (Declaration.Branch == null) ? ZString.Empty : Branch.GB_BranchName; }
		}

		public ZString ProtestantNameForModuleFilter
		{
			get { return Protestant.E2_CompanyNameTruncated; }
		}

		public ZPropertyInfo ProtestantNameForModuleFilterInfo
		{
			get { return GetZPropertyInfo(Schema.ProtestantNameForModuleFilter); }
		}

		#endregion

		#region AddInfo

		AddInfoJobDeclaration AddInfo
		{
			get { return Declaration == null ? null : addInfo ?? (addInfo = Declaration.GetAddInfo()); }
		}
		AddInfoJobDeclaration addInfo;

		#endregion

		#region Lookups

		public ProtestLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new ProtestLookups(this);
				}

				return lookups;
			}
		}
		ProtestLookups lookups;

		#endregion

		#region Validation

		public ProtestValidation Validation
		{
			get { return new ProtestValidation(this); }
		}

		#endregion

		#region For Message Sending

		public EDIMessageCollection Messages
		{
			get { return Declaration.Messages; }
		}

		internal bool CanSendOriginal
		{
			get { return US_P_CBPAssignedProtestNumber.IsEmpty; }
		}

		internal bool CanSendWithdrawal
		{
			get { return !US_P_CBPAssignedProtestNumber.IsEmpty; }
		}

		internal PROP11PJPJ GetP11BlockFromLatestOriginalAcceptedMessage()
		{
			return LastAcceptedInitialFilingMessage != null
					? LastAcceptedInitialFilingMessage.MessageBlock.MessageBlocks.OfType<PROP11PJPJ>().FirstOrDefault()
					: null;
		}

		internal MQEDIMessage LastAcceptedInitialFilingMessage
		{
			get
			{
				if (latestInitialFilingMessage == null)
				{
					var query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ProtestInitialFilingResponse);
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";

					var responseMessages = Messages.Find(query);

					foreach (MQEDIMessage message in responseMessages)
					{
						if (message.IsProtestCleared)
						{
							latestInitialFilingMessage = message.OriginalMessage;
							break;
						}
					}
				}
				return latestInitialFilingMessage;
			}
		}
		MQEDIMessage latestInitialFilingMessage;

		#endregion

		#region IJobInvoicingPlugIn Members

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new ProtestInvoicingSupporter(this)); }
		}
		ProtestInvoicingSupporter invoicingSupporter;

		string IJobNumber.JobNumber
		{
			get { return ((IJobHeaderParent)Declaration).JobNumber; }
		}

		#endregion

		#region IJobHeaderParent Members

		ZGuid IJobHeaderParentCore.PK
		{
			get { return ((IJobHeaderParent)Declaration).PK; }
		}

		string IJobHeaderParentCore.TableName
		{
			get { return ((IJobHeaderParent)Declaration).TableName; }
		}

		BusinessObjectFactory IJobHeaderParentCore.Factory
		{
			get { return ((IJobHeaderParent)Declaration).Factory; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			((IJobHeaderParent)Declaration).OnJobCreating(job);
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

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			((IJobHeaderParent)Declaration).SetJobNumberFieldOnSaving();
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region ICustomsJobInfo

		AutoPostingNotification ICustomsJobInfo.AutoPostingNotification
		{
			get
			{
				var emailRecipients = new List<ZGuid>();
				var branch = Declaration.Branch;
				var groupNotification = branch != null ? CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty) : null;
				var staff = Declaration.CusAgent;
				if (staff != null && !staff.GS_EmailAddress.IsEmpty)
				{
					emailRecipients.Add(staff.PK);
				}
				else
				{
					if (groupNotification != null)
					{
						var notificationGroup = groupNotification.SendGroupPK;
						if (notificationGroup.IsValid)
						{
							emailRecipients.Add(notificationGroup);
						}
					}
				}

				return new AutoPostingNotification(emailRecipients.ToArray(), groupNotification?.SuppressUnpostARNotificaiton ?? false);
			}
		}

		public GlbBranch Branch
		{
			get { return Declaration.Branch ?? Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		ZGuid ICustomsJobInfo.CreditorPK
		{
			get
			{
				var org = Factory.Load<OrgHeader>(Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(Declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty));
				return (org == null) ? ZGuid.Empty : org.PK;
			}
		}

		ZString[] ICustomsJobInfo.GetValidAPInvoiceNumsToMatchAndValidateAgainst()
		{
			return Array.Empty<ZString>();
		}

		IJobInvoicingPlugIn ICustomsJobInfo.TopLevelObjectForJobToReference
		{
			get { return this; }
		}

		ICustomsJobInfo ICustomsJobInfoProvider.GetCustomsJobInfo(ZGuid companyPK)
		{
			return this;
		}
		public Directions JobDirection
		{
			get { return Directions.Import; }
		}

		public EntryInfoCollection Entries
		{
			get { return new EntryInfoCollection(); }
		}

		#endregion

		#region IEDocsProvider members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return ((IEDocsProvider)Declaration).GetEDocsProviderSupporter();
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return Declaration.DocManagerInfo; }
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new ProtestDocumentSupporter(this); }
		}

		#endregion

		#region IEDocsPluginHostDecider

		IBusiness IEDocsPluginHostDecider.HostBusinessEntity
		{
			get { return Declaration; }
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return ProtestWorkflowHelper.WorkflowType; }
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new JobDeclarationProcessTaskCollection<JobDeclaration>(Declaration)); // Should this be a shared collection with the Declaration?
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return ProtestWorkflowHelper.GetTemplateSelectionCriteria(Declaration);
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IAllocateNumber

		void IAllocateNumberSupporter.UnlockNumberAllocationMutex()
		{
			Declaration.UnlockImportEntryNumberAllocationMutex();
		}

		bool IAllocateNumberSupporter.LockNumberAllocationMutex
		{
			get { return Declaration.LockImportEntryNumberAllocationMutex; }
		}

		string IAllocateNumberSupporter.GetNumberAllocationMutexLockInfo()
		{
			return Declaration.GetImportEntryNumberAllocationMutexLockInfo();
		}

		ZString IAllocateNumberSupporter.GetReasonToStopProceeding()
		{
			ReloadProtestEntryNumber();
			var result = "";
			if (!US_P_CBPAssignedProtestNumber.IsEmpty)
			{
				result = Constants.CBPNumberAlreadyAllocated(US_P_CBPAssignedProtestNumber);
			}
			return result;
		}

		ZString IAllocateNumberSupporter.GetExistingNumber()
		{
			return US_P_CBPAssignedProtestNumber;
		}

		AllocateNumber IAllocateNumberSupporter.GetNewAllocateNumber()
		{
			var args = new AllocateNumberArgs();
			args.MaxLength = 12;
			args.Branch = Branch;
			args.Factory = Factory;
			args.ValidateNumber = (info, validator) => validator.ValidateProtestCBPAssignedNumber(info);
			return new AllocateNumber(args);
		}

		void IAllocateNumberSupporter.DoAllocate(ZString userEnteredNumber)
		{
			AllocateCBPAssignedNumber(userEnteredNumber);
		}

		void IAllocateNumberSupporter.OnAllocatedNumberSaved()
		{
			US_P_CBPAssignedProtestNumberInfo.RefreshBinding();
		}

		ZString IAllocateNumberSupporter.NumberType
		{
			get { return "CBP Assigned Number"; }
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new ProtestRatingAdaptersProvider(this); }
		}

		#endregion

		#region Properties For Document Printing

		public ZString JustificationArgumentsSection5
		{
			get { return JustificationNoteFormatted.SubstringSafe(0, 700); }
		}

		public ZString JustificationArgumentsContinuation
		{
			get { return JustificationNoteFormatted.SubstringSafe(700); }
		}

		public GlbStaff Broker
		{
			get { return Declaration.CusAgent; }
		}

		#region Linked Entry Ports 1-5

		public ZString LinkedEntryPort1
		{
			get { return LinkedEntries.Count > 0 ? LinkedEntries[0].US_LE_PortCode : ZString.Empty; }
		}

		public ZString LinkedEntryPort2
		{
			get { return LinkedEntries.Count > 1 ? LinkedEntries[1].US_LE_PortCode : ZString.Empty; }
		}

		public ZString LinkedEntryPort3
		{
			get { return LinkedEntries.Count > 2 ? LinkedEntries[2].US_LE_PortCode : ZString.Empty; }
		}

		public ZString LinkedEntryPort4
		{
			get { return LinkedEntries.Count > 3 ? LinkedEntries[3].US_LE_PortCode : ZString.Empty; }
		}

		public ZString LinkedEntryPort5
		{
			get { return LinkedEntries.Count > 4 ? LinkedEntries[4].US_LE_PortCode : ZString.Empty; }
		}

		#endregion

		#region Entry Filer Codes 1-5

		public ZString LinkedEntryFiler1
		{
			get { return LinkedEntries.Count > 0 ? LinkedEntries[0].EntryFilerCode : ZString.Empty; }
		}

		public ZString LinkedEntryFiler2
		{
			get { return LinkedEntries.Count > 1 ? LinkedEntries[1].EntryFilerCode : ZString.Empty; }
		}

		public ZString LinkedEntryFiler3
		{
			get { return LinkedEntries.Count > 2 ? LinkedEntries[2].EntryFilerCode : ZString.Empty; }
		}

		public ZString LinkedEntryFiler4
		{
			get { return LinkedEntries.Count > 3 ? LinkedEntries[3].EntryFilerCode : ZString.Empty; }
		}

		public ZString LinkedEntryFiler5
		{
			get { return LinkedEntries.Count > 4 ? LinkedEntries[4].EntryFilerCode : ZString.Empty; }
		}

		#endregion

		#region Entry Number Truncated 1-5

		public ZString LinkedEntryNumTruncated1
		{
			get { return LinkedEntries.Count > 0 ? LinkedEntries[0].EntryNumber : ZString.Empty; }
		}

		public ZString LinkedEntryNumTruncated2
		{
			get { return LinkedEntries.Count > 1 ? LinkedEntries[1].EntryNumber : ZString.Empty; }
		}

		public ZString LinkedEntryNumTruncated3
		{
			get { return LinkedEntries.Count > 2 ? LinkedEntries[2].EntryNumber : ZString.Empty; }
		}

		public ZString LinkedEntryNumTruncated4
		{
			get { return LinkedEntries.Count > 3 ? LinkedEntries[3].EntryNumber : ZString.Empty; }
		}

		public ZString LinkedEntryNumTruncated5
		{
			get { return LinkedEntries.Count > 4 ? LinkedEntries[4].EntryNumber : ZString.Empty; }
		}

		#endregion

		#region Entry Number Check Digit 1-5

		public ZString LinkedEntryNumCheckDigit1
		{
			get { return LinkedEntries.Count > 0 ? LinkedEntries[0].EntryNumberCheckDigit : ZString.Empty; }
		}

		public ZString LinkedEntryNumCheckDigit2
		{
			get { return LinkedEntries.Count > 1 ? LinkedEntries[1].EntryNumberCheckDigit : ZString.Empty; }
		}

		public ZString LinkedEntryNumCheckDigit3
		{
			get { return LinkedEntries.Count > 2 ? LinkedEntries[2].EntryNumberCheckDigit : ZString.Empty; }
		}

		public ZString LinkedEntryNumCheckDigit4
		{
			get { return LinkedEntries.Count > 3 ? LinkedEntries[3].EntryNumberCheckDigit : ZString.Empty; }
		}

		public ZString LinkedEntryNumCheckDigit5
		{
			get { return LinkedEntries.Count > 4 ? LinkedEntries[4].EntryNumberCheckDigit : ZString.Empty; }
		}

		#endregion

		#region Date Of Entry 1-5

		public ZDateTime LinkedEntryDate1
		{
			get { return LinkedEntries.Count > 0 ? LinkedEntries[0].US_LE_EntryDate : ZDateTime.Empty; }
		}

		public ZDateTime LinkedEntryDate2
		{
			get { return LinkedEntries.Count > 1 ? LinkedEntries[1].US_LE_EntryDate : ZDateTime.Empty; }
		}

		public ZDateTime LinkedEntryDate3
		{
			get { return LinkedEntries.Count > 2 ? LinkedEntries[2].US_LE_EntryDate : ZDateTime.Empty; }
		}

		public ZDateTime LinkedEntryDate4
		{
			get { return LinkedEntries.Count > 3 ? LinkedEntries[3].US_LE_EntryDate : ZDateTime.Empty; }
		}

		public ZDateTime LinkedEntryDate5
		{
			get { return LinkedEntries.Count > 4 ? LinkedEntries[4].US_LE_EntryDate : ZDateTime.Empty; }
		}

		#endregion

		#region Date Of Liquidation 1-5

		public ZDateTime LinkedEntryLiquidationDate1
		{
			get { return LinkedEntries.Count > 0 ? LinkedEntries[0].US_LE_LiquidationDate : ZDateTime.Empty; }
		}

		public ZDateTime LinkedEntryLiquidationDate2
		{
			get { return LinkedEntries.Count > 1 ? LinkedEntries[1].US_LE_LiquidationDate : ZDateTime.Empty; }
		}

		public ZDateTime LinkedEntryLiquidationDate3
		{
			get { return LinkedEntries.Count > 2 ? LinkedEntries[2].US_LE_LiquidationDate : ZDateTime.Empty; }
		}

		public ZDateTime LinkedEntryLiquidationDate4
		{
			get { return LinkedEntries.Count > 3 ? LinkedEntries[3].US_LE_LiquidationDate : ZDateTime.Empty; }
		}

		public ZDateTime LinkedEntryLiquidationDate5
		{
			get { return LinkedEntries.Count > 4 ? LinkedEntries[4].US_LE_LiquidationDate : ZDateTime.Empty; }
		}

		#endregion

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (!Declaration.SuspendAddingWorkflow)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}
			base.OnFactorySavingBeforeTransactionCore();
		}

		public override string TablePrefix
		{
			get { return Declaration.TablePrefix; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = string.Format("Protest - {0}", Declaration.JE_DeclarationReference);
				if (Protestant != null && Protestant.Organisation != null && !Protestant.Organisation.OH_FullName.IsEmpty)
				{
					result += string.Format(" - {0}", Protestant.Organisation.OH_FullName);
				}

				return Res.GetString("3A6E0511-ED46-48CF-8D20-99C1EAE7CBD9", "{0}", result);
			}
		}

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return Declaration.PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.US.Protest; }
		}

		#endregion
	}
}
