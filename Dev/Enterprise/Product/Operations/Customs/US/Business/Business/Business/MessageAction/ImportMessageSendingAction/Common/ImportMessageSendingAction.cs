using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public abstract class ImportMessageSendingAction : AutoUSImportMessageSendingAction
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoUSImportMessageSendingAction.Schema
		{
			public const string US_MessageContents = "US_MessageContents";
			public const string US_MessageDescription = "US_MessageDescription";
			public const string US_Declarant = "US_Declarant";
			public const string US_SE_ReferenceNo = "US_SE_ReferenceNo";
			public const string US_SE_ReasonCode = "US_SE_ReasonCode";
			public const string US_PNActionCode = "US_PNActionCode";

			public const int US_SE_ReferenceNoLength = 50;
			public const int US_SE_ReasonCodeLength = 2;
			public const int US_DeclarantMaxLength = 32;
			public const int US_PNActionCodeLength = 1;
		}

		protected ImportMessageSendingAction(BusinessObject bizObj, ImportMessageStatusList.MessageType messageType, ImportMessageSendingActionCollection actions)
			: base(actions.Factory)
		{
			this.bizObj = bizObj;
			this.messageType = messageType;
			this.actions = actions;

			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				SetDefaults();
			}
		}

		protected ImportMessageSendingAction(ImportMessageSendingActionCollection actions)
			: this(null, ImportMessageStatusList.MessageType.Undefined, actions)
		{
		}

		public JobDeclaration Declaration
		{
			get
			{
				IMessageAttacheeInDeclaration attachee = bizObj as IMessageAttacheeInDeclaration;
				return attachee != null ? Factory.Load<JobDeclaration>(attachee.DeclarationPK) : bizObj as JobDeclaration;
			}
		}

		public bool IsPGAToBeSent(string pgaCode)
		{
			var declaration = Declaration;
			var result = false;
			if ((actions.IsOriginal || actions.IsAmendment) && declaration.IsACECargoCertificationMode)
			{
				var hasPGALines = false;
				switch (pgaCode)
				{
					case GovernmentAgencyProgramCodeList.Codes.ODS:
						hasPGALines = Declaration.PGAFlags.HasInvoiceLinesWithODS;
						break;
					case GovernmentAgencyProgramCodeList.Codes.TSCA:
						hasPGALines = Declaration.PGAFlags.HasInvoiceLinesWithTSCA;
						break;
				}

				var shouldSendPGAInEntrySummary = !declaration.PGAExpeditedInEntrySummaryIndicator.IsEmpty;
				if (hasPGALines)
				{
					result = IsEntrySummary && shouldSendPGAInEntrySummary || IsACECargoRelease;
				}
			}
			return result;
		}
		public virtual void CopyPSCReasonsAndExplanation()
		{
		}

		public bool IsElectronicInvoice
		{
			get { return messageType == ImportMessageStatusList.MessageType.ElectronicInvoice; }
		}

		public bool IsEntrySummary
		{
			get { return messageType == ImportMessageStatusList.MessageType.EntrySummary; }
		}

		public bool IsTemporaryImportationBond
		{
			get { return messageType == ImportMessageStatusList.MessageType.TemporaryImportationBond; }
		}

		public bool IsPSCReasonAndExplanationEnabled
		{
			get { return actions != null && IsEntrySummary && actions.declaration.IsACE && actions.declaration.US_PSC; }
		}

		public bool IsSE13DataRelevant
		{
			get { return actions.declaration.IsACECargoCertificationMode && ((IsEntrySummary && US_CertifyCargoRelease && (actions.IsOriginal || actions.IsAmendment)) || IsACECargoRelease); }
		}

		public bool ShouldPSCReasonAndExplanationBeSaved
		{
			get { return US_SendMessage && IsPSCReasonAndExplanationEnabled; }
		}

		protected virtual bool IsElectronicInvoicing
		{
			get { return false; }
		}

		public bool IsCargoRelease
		{
			get { return messageType == ImportMessageStatusList.MessageType.CargoRelease; }
		}

		public bool IsBorderCargoRelease
		{
			get { return messageType == ImportMessageStatusList.MessageType.BorderCargoRelease; }
		}

		public bool IsACECargoRelease
		{
			get { return messageType == ImportMessageStatusList.MessageType.ACECargoRelease; }
		}

		public bool IsPaidRelevant
		{
			get { return IsPaidRelevantCore; }
		}

		protected virtual bool IsPaidRelevantCore
		{
			get { return false; }
		}

		#region Bindable Properties

		public virtual ZString US_MessageDescription
		{
			get { return ZString.Empty; }
		}

		public ZPropertyInfo US_MessageDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.US_MessageDescription); }
		}

		public virtual ZString US_MessageContents
		{
			get { return MessageContentPreviewNotSupportedYet; }
		}
		internal const string MessageContentPreviewNotSupportedYet = "Message content preview is not supported yet.";

		public ZPropertyInfo US_MessageContentsInfo
		{
			get { return GetZPropertyInfo(Schema.US_MessageContents); }
		}

		public bool US_CertifyCargoRelease_ReadOnly
		{
			get { return !US_SendMessage || GetUS_CertifyCargoReleaseInfoReadOnly(); }
		}

		protected virtual bool GetUS_CertifyCargoReleaseInfoReadOnly()
		{
			return true;
		}

		protected bool US_AcknowledgeAndSign_ReadOnly
		{
			get { return !US_SendMessage; }
		}

		public ZString US_Declarant
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		public ZPropertyInfo US_DeclarantInfo
		{
			get { return GetZPropertyInfo(Schema.US_Declarant); }
		}

		[List(nameof(Lookups) + "." + nameof(USImportMessageSendingActionLookups.TitleOfDeclarantList))]
		public override ZString US_TitleOfDeclarant
		{
			get { return base.US_TitleOfDeclarant; }
			set { base.US_TitleOfDeclarant = value; }
		}

		protected bool US_TitleOfDeclarant_ReadOnly
		{
			get { return !US_SendMessage || GetUS_TitleOfDeclarantInfoReadOnly(); }
		}

		protected virtual bool GetUS_TitleOfDeclarantInfoReadOnly()
		{
			return true;
		}

		protected bool US_DateOfDeclaration_ReadOnly
		{
			get { return !US_SendMessage || GetUS_DateOfDeclarationInfoReadOnly(); }
		}

		protected virtual bool GetUS_DateOfDeclarationInfoReadOnly()
		{
			return true;
		}

		protected bool US_IsCustomsRequested_ReadOnly
		{
			get { return !US_SendMessage || GetUS_IsCustomsRequestedInfoReadOnly(); }
		}

		protected virtual bool GetUS_IsCustomsRequestedInfoReadOnly()
		{
			return true;
		}

		[List(nameof(Lookups) + "." + nameof(USImportMessageSendingActionLookups.YesNoList))]
		public override ZString US_Paid
		{
			get { return base.US_Paid; }
			set { base.US_Paid = value; }
		}

		[List(nameof(Lookups) + "." + nameof(USImportMessageSendingActionLookups.ActionTypeList))]
		public override ZString US_SE_ActionType
		{
			get { return base.US_SE_ActionType; }
			set { base.US_SE_ActionType = value; }
		}

		#region ACE Cargo Release Cancellation Details

		[List(nameof(Lookups) + "." + nameof(USImportMessageSendingActionLookups.ReasonCodeList))]
		[MaxLength(Schema.US_SE_ReasonCodeLength)]
		public virtual ZString US_SE_ReasonCode
		{
			get { return se_ReasonCode; }
			set
			{
				CheckMaximumLength(US_SE_ReasonCodeInfo, value);
				se_ReasonCode = value;
				US_SE_ReasonCodeInfo.RefreshBinding();
			}
		}
		ZString se_ReasonCode;

		public virtual bool US_SE_ReferenceNo_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo US_SE_ReasonCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_SE_ReasonCode); }
		}

		[MaxLength(Schema.US_SE_ReferenceNoLength)]
		public virtual ZString US_SE_ReferenceNo
		{
			get { return se_ReferenceNo; }
			set
			{
				CheckMaximumLength(US_SE_ReferenceNoInfo, value);
				se_ReferenceNo = value;
				US_SE_ReferenceNoInfo.RefreshBinding();
			}
		}
		ZString se_ReferenceNo;

		public ZPropertyInfo US_SE_ReferenceNoInfo
		{
			get { return GetZPropertyInfo(Schema.US_SE_ReferenceNo); }
		}

		public virtual ZBool US_SE_MultipleDispositionsIndic
		{
			get;
			set;
		}

		public ResourceStringData GetReferenceNumberCaption()
		{
			var caption = Res.GetData("336D6414-55BE-48F7-A472-D28A8D91806B", "Reference Number");
			switch (US_SE_ReasonCode)
			{
				case ReasonCodeList.Codes.EntryReplacedBy7512:
					caption = Res.GetData("A69B37F3-D326-4550-895E-3E81668CB6F2", "Replacement In-Bond Num.");
					break;
				case ReasonCodeList.Codes.MerchandiseClearedByAnother:
					caption = Res.GetData("F73F81F4-234F-4408-9288-2DFD5F172B23", "Replacement Entry Number");
					break;
				case ReasonCodeList.Codes.EntryReplacedByFTZ:
					caption = Res.GetData("3EBCE32A-4527-4E15-AD3F-DDC47EE68409", "Replacement FTZ Adm. Num.");
					break;
			}
			return caption;
		}

		#endregion

		#region ACE Prior Notice Details

		[List(nameof(Lookups) + "." + nameof(USImportMessageSendingActionLookups.ACEPNActionCodeList))]
		[MaxLength(Schema.US_PNActionCodeLength)]
		public virtual ZString US_PNActionCode
		{
			get { return pnActionCode; }
			set
			{
				CheckMaximumLength(US_PNActionCodeInfo, value);
				pnActionCode = value;
				US_PNActionCodeInfo.RefreshBinding();
			}
		}
		ZString pnActionCode;

		public ZPropertyInfo US_PNActionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_PNActionCode); }
		}

		#endregion

		public ZString TIBStatement
		{
			get { return TIBStatementCore; }
		}
		const string TIBStatementCore = "I certify that the articles are to be used according to the terms, conditions and provisos of the HTS subheading as declared herein and applies to the articles entered, that the articles will not be used for any other use and that the articles are not imported for sale or sale upon approval. I declare that the articles will be exported or destroyed within the applicable 6-month or 1-year period from the date of importation, unless extended.";

		public ZString CBMAStatement
		{
			get { return CBMAStatementCore; }
		}
		const string CBMAStatementCore = "The information submitted is true and accurate and I assume the responsibility for proving such representations. I understand that I am liable for any false statements or material omissions made on or in connection with this submission. I certify that I have determined and ascertained that the control group of which I am a member (including both domestic and foreign members of the control group) has not exceeded the quantitative limit applicable to the tax rate or tax credit I am claiming.";

		public ZString NonRussianStatement
		{
			get { return NonRussianStatementCore; }
		}
		const string NonRussianStatementCore = "Be sure to send in a DIS Self-Certification Statement that includes the following text:\r\nI certify that any such products in this shipment were not harvested in waters under the jurisdiction of the Russian Federation or by Russia-flagged vessels, notwithstanding whether such product has been incorporated or substantially transformed into another product outside of the Russian Federation.";
		#endregion

		#region Related Validation Mode

		public ValidationModes ValidateMode
		{
			get
			{
				ValidationModes result = ValidationModesMessageTypesTranslator.GetValidationModesRelatedTo(messageType);

				if (messageType == ImportMessageStatusList.MessageType.CargoRelease || messageType == ImportMessageStatusList.MessageType.BorderCargoRelease || US_CertifyCargoRelease)
				{
					result |= ValidationModes.CargoRelease;
				}

				return result;
			}
		}

		#endregion

		protected virtual void SetDefaults()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				var contact = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(Factory, declaration.PK, declaration.CompanyPK.ToGuid(), declaration.Branch.PK.ToGuid());
				if (contact != null)
				{
					US_SE_ContactName = contact.GS_FullName.Left(AutoUSImportMessageSendingAction.Schema.US_SE_ContactNameMaxLength);
					US_SE_ContactPhone = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(contact).Left(AutoUSImportMessageSendingAction.Schema.US_SE_ContactPhoneMaxLength);
				}
			}
		}

		internal readonly BusinessObject bizObj;
		internal readonly ImportMessageStatusList.MessageType messageType;
		internal readonly ImportMessageSendingActionCollection actions;

		internal Customs.Business.SingleMessageManager MessageManager
		{
			get { return messageManager ?? (messageManager = GetMessageManager()); }
		}
		Customs.Business.SingleMessageManager messageManager;

		protected abstract Customs.Business.SingleMessageManager GetMessageManager();

		#region Collections

		public EntryCensusWarningOverrideCollection CensusWarningCodes
		{
			get
			{
				if (censusWarningCodes == null)
				{
					// for binding, this cannot be moved down to EntryHeaderMessageSendingAction
					var entry = bizObj as CusEntryHeader ?? Factory.GetNull<CusEntryHeader>();
					censusWarningCodes = new EntryCensusWarningOverrideCollection(entry);
				}
				return censusWarningCodes;
			}
		}
		EntryCensusWarningOverrideCollection censusWarningCodes;

		public PSCReasonCodeCollection PSCReasonCodes
		{
			get
			{
				if (pscReasonCodes == null)
				{
					// for binding, this cannot be moved down to EntryHeaderMessageSendingAction
					var entry = bizObj as CusEntryHeader ?? Factory.GetNull<CusEntryHeader>();
					pscReasonCodes = new PSCReasonCodeCollection(entry);
				}
				return pscReasonCodes;
			}
		}
		PSCReasonCodeCollection pscReasonCodes;
		#endregion

		#region Lookups

		public USImportMessageSendingActionLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected virtual USImportMessageSendingActionLookups GetNewLookups()
		{
			return new USImportMessageSendingActionLookups(this);
		}

		USImportMessageSendingActionLookups fLookups;

		#endregion
	}
}
