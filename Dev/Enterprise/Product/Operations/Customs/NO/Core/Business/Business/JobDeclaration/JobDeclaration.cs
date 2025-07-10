using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business;

public class JobDeclaration : AutoNOJobDeclaration, IMessageManageableBizObj
{
	public JobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoNOJobDeclaration.Schema
	{
		public const int JE_GoodsNumberMaxLength = 15;
		public const int JE_PositionMaxLength = 10;
		public const string JE_GoodsNumber = "JE_GoodsNumber";
		public const string JE_Position = "JE_Position";
		public const string JE_CopyStatus = "JE_CopyStatus";
	}

	public new JobDeclaration Clone() => (JobDeclaration)base.Clone();

	[ChildEditable(true)]
	public new ICusContainerCollection<CusContainer> CusContainers => (ICusContainerCollection<CusContainer>)base.CusContainers;

	[ChildEditable(false)]
	public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

	public new JobComInvoiceGroupHeader TopGroupInvoice => (JobComInvoiceGroupHeader)base.TopGroupInvoice;

	public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

	public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

	protected override bool IsLookupsCachedInBase => false;

	[ChildEditable(true)]
	public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

	public new JobDeclaration ParentRelatedDeclaration => (JobDeclaration)base.ParentRelatedDeclaration;

	[ChildEditable]
	public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

	public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

	[ChildEditable(true)]
	public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

	[ChildEditable(true)]
	public new BillCollection<Bill, JobDeclaration> Bills => (BillCollection<Bill, JobDeclaration>)base.Bills;

	public new ActiveCusEntryHeaderCollection ActiveEntryHeaders => (ActiveCusEntryHeaderCollection)base.ActiveEntryHeaders;

	public override ZBool AreMultipleEntryInstructionsAllowed => false;

	protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new BaseCusContainerCollection<CusContainer>(this, Factory);

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

	protected override Customs.Business.JobDeclarationValidation GetNewValidation()
	{
		if (IsImport)
		{
			return new ImportJobDeclarationValidation(this);
		}
		if (IsExport)
		{
			return new ExportJobDeclarationValidation(this);
		}
		return new JobDeclarationValidation(this);
	}

	protected override Customs.Business.JobDeclarationLookups GetNewLookups()
	{
		if (IsExport)
		{
			return new ExportJobDeclarationLookups(this);
		}
		return new JobDeclarationLookups(this);
	}

	protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

	protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection<JobComInvoiceLine>(this);

	protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(this, Factory);

	protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

	protected override Customs.Business.ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection() => new ActiveCusEntryHeaderCollection(this);

	protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType) => new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);

	protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

	protected override bool IsCustomsLineAmendmentATotalReplacement => false;

	protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Norway;

	public new EntryInstructionProvider CustomsEntryInstructionProvider => (EntryInstructionProvider)base.CustomsEntryInstructionProvider;
	protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);

	public EntryCreationStrategy CreateEntryCreationStrategy() => new EntryCreationStrategy(this);

	[ChildEditable(true)]
	[ChildEditableTestExclude]
	[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
	public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

	CusEntryInstruction RandomCustomsEntryInstruction =>
		randomCustomsEntryInstruction is null or { IsRowDeletedOrDetachedOrNull: true }
			? randomCustomsEntryInstruction = CustomsEntryInstructions.FirstOrDefault() ?? CustomsEntryInstructions.AddNew()
			: randomCustomsEntryInstruction;
	CusEntryInstruction randomCustomsEntryInstruction;

	protected override bool HasSplitEntriesCore
	{
		get
		{
			if (!GetType().FullName.Contains("NO"))
			{
				ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
			}
			return base.HasSplitEntriesCore;
		}
	}

	public ZBool HasDigitollGoodsNumber => Factory.GetValue(ref hasDigitollGoodsNumberCached, GetHasDigitollGoodsNumber);
	CachedProperty<ZBool> hasDigitollGoodsNumberCached;

	protected virtual ZBool GetHasDigitollGoodsNumber() =>
		JE_GoodsNumber is { Length: 7 or 8 } goodsNumber &&
		(string)goodsNumber.Substring(6) is "DT" or "D";

	public bool HasImporterWithDeferredCustomsPaymentAccount => Factory.GetValue(ref hasImporterWithDeferredCustomsPaymentAccountCached, () => !Importer.GetDefermentApprovalNumberCodeOrEmpty().IsEmpty);
	CachedProperty<bool> hasImporterWithDeferredCustomsPaymentAccountCached;

	public bool HasImporterWithMVARegistration => Factory.GetValue(ref hasImporterWithMVARegistrationCached, () => !Importer.GetMVACodeOrEmpty().IsEmpty);
	CachedProperty<bool> hasImporterWithMVARegistrationCached;

	public bool HasImporterWithSocialSecurityNumber => Factory.GetValue(ref hasImporterWithSocialSecurityNumberCached, () => !Importer.GetSocialSecurityNumberOrEmpty().IsEmpty);
	CachedProperty<bool> hasImporterWithSocialSecurityNumberCached;

	public bool HasImporterWithOrganizationNumber => Factory.GetValue(ref hasImporterWithOrganizationNumberCached, () => !Importer.GetOrganizationNumberOrEmpty().IsEmpty);
	CachedProperty<bool> hasImporterWithOrganizationNumberCached;

	public bool HasRelatedRecalculations => Factory.GetValue(ref hasRelatedRecalculations, () => RelatedDeclarations.Cast<JobDeclaration>().Any(x => x.JE_CopyStatus == NODeclarationCopyStatus.Codes.Recalculation));
	CachedProperty<bool> hasRelatedRecalculations;

	public override ZString JE_AddInfo
	{
		get => base.JE_AddInfo;
		set
		{
			base.JE_AddInfo = value;
			CustomsEntryHeaders?.MarkAsNeedingValidation();
		}
	}

	protected CusEntryNumberWrapper GoodsNumberAndPosition => goodsnumberandposition ??=
		new CusEntryNumberWrapper(this, CusEntryNumberTypes.Norway.GoodsNumber, new Dictionary<ZString, ZInt> { { Schema.JE_GoodsNumber, 0 }, { Schema.JE_Position, 1 } });
	CusEntryNumberWrapper goodsnumberandposition;

	[MaxLength(Schema.JE_GoodsNumberMaxLength)]
	public ZString JE_GoodsNumber
	{
		get => GoodsNumberAndPosition.GetEntryNumberPart(Schema.JE_GoodsNumber);
		set
		{
			if (JE_GoodsNumber != value)
			{
				CheckMaximumLength(JE_GoodsNumberInfo, value);
				GoodsNumberAndPosition.SetEntryNumberPart(value, Schema.JE_GoodsNumber, JE_GoodsNumberInfo);
				CustomsEntryInstructions.RefreshBindingIncludingChildren();
				if (!IsValidationSuspended && !IsCopying)
				{
					Validation.ValidateJE_GoodsNumber();
				}
			}
		}
	}
	public ZPropertyInfo JE_GoodsNumberInfo => GetZPropertyInfo(nameof(JE_GoodsNumber));

	[MaxLength(Schema.JE_PositionMaxLength)]
	public ZString JE_Position
	{
		get => GoodsNumberAndPosition.GetEntryNumberPart(Schema.JE_Position);
		set
		{
			if (JE_Position != value)
			{
				CheckMaximumLength(JE_PositionInfo, value);
				GoodsNumberAndPosition.SetEntryNumberPart(value, Schema.JE_Position, JE_PositionInfo);
				CustomsEntryInstructions.RefreshBindingIncludingChildren();
			}
		}
	}
	public ZPropertyInfo JE_PositionInfo => GetZPropertyInfo(nameof(JE_Position));

	[ResourceStringData("NO.JobDeclaration.JE_OA_DeclarantAddress", Caption = "Declarant")]
	public override ZGuid JE_OA_DeclarantAddress
	{
		get => base.JE_OA_DeclarantAddress;
		set => base.JE_OA_DeclarantAddress = value;
	}

	public override ZString JE_RL_NKOrigin
	{
		get => base.JE_RL_NKOrigin;
		set
		{
			base.JE_RL_NKOrigin = value;
			if (!IsCopying)
			{
				JE_GoodsOrigin = JE_RL_NKOrigin.SubstringSafe(0, 2);
				if (IsImport && JE_MessageSubType.IsEmpty)
				{
					JE_MessageSubType = IsCountryInEuOrEfta(JE_GoodsOrigin) ? ShipmentTypeImport.Codes.ImportOfGoodsFromAnEuEeaOrEftaMemberState : ShipmentTypeImport.Codes.ImportOfGoodsFromAllOtherNotCoveredByEu;
				}
			}
		}
	}

	public override ZString JE_RL_NKFinalDestination
	{
		get => base.JE_RL_NKFinalDestination;
		set
		{
			base.JE_RL_NKFinalDestination = value;
			if (!IsCopying)
			{
				JE_GoodsDestination = JE_RL_NKFinalDestination.SubstringSafe(0, 2);
				if (IsExport && JE_MessageSubType.IsEmpty)
				{
					JE_MessageSubType = IsCountryInEuOrEfta(JE_GoodsDestination) ? ShipmentTypeExport.Codes.ExportOfGoodsToAnEuEeaOrEftaMemberState : ShipmentTypeExport.Codes.ExportOfGoodsToAllOtherNotCoveredByEu;
				}
			}
		}
	}

	protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
	{
		base.JE_MessageTypeChanged(oldValue, newValue);

		if (newValue == JobMessageTypeList.Codes.Export)
		{
			FilteredInvoiceLines.ForEach(x =>
			{
				x.JI_ZZF_NKTaxType = ZString.Empty;
			});
		}
	}

	public override ZInt JE_TotalNoOfPieces
	{
		get => base.JE_TotalNoOfPieces;
		set
		{
			var oldValue = base.JE_TotalNoOfPieces;
			base.JE_TotalNoOfPieces = value;
			if (oldValue != value && !IsValidationSuspended)
			{
				CustomsEntryHeaders?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString JE_TransportMode
	{
		get => base.JE_TransportMode;
		set
		{
			if (base.JE_TransportMode != value)
			{
				base.JE_TransportMode = value;
				JE_CustomsTransportMode = Lookups.NOCustomsTransportModeList.DefaultCode;

				if (IsCopying)
				{
					return;
				}

				if (value == Customs.Business.TransportTypeList.Codes.FixedTransportInstallations)
				{
					JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
				}

				if (!TransportModeRequiresTransportNationality)
				{
					JE_RN_NKTransportNationality = ZString.Empty;
				}
			}
		}
	}

	[MaxLength(3)]
	public ZString JE_CopyStatus
	{
		get => RandomCustomsEntryInstruction.CEI_SubStyle;
		set
		{
			var instruction = RandomCustomsEntryInstruction;
			var oldValue = instruction.CEI_SubStyle;
			if (oldValue != value)
			{
				instruction.CEI_SubStyle = value;
				JE_CopyStatusInfo.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo JE_CopyStatusInfo => GetZPropertyInfo(Schema.JE_CopyStatus);

	[ResourceStringData("E96E3CE7-80F9-4AD3-B494-5A3B65B3A486", Caption = "Type")]
	public ZString JE_CopyStatusCaption => (string)JE_CopyStatus switch
	{
		NODeclarationCopyStatus.Codes.Recalculation => ResString.GetMultilingualString("3B2CB77C-2B86-4937-AACE-757F709E0D53", "Recalculation"),
		NODeclarationCopyStatus.Codes.ReExport => ResString.GetMultilingualString("8478C995-74D4-4144-9C18-BC47228BDFB1", "Re-export"),
		NODeclarationCopyStatus.Codes.FinalImport => ResString.GetMultilingualString("91C947C6-F024-4694-9102-085C85E28F87", "Final import"),
		_ => ZString.Empty,
	};

	public ZPropertyInfo JE_CopyStatusCaptionInfo => GetWrappedZPropertyInfo(nameof(JE_CopyStatusCaption), x => JE_CopyStatusInfo);

	public override ZGuid JE_OH_Importer
	{
		get => base.JE_OH_Importer;
		set
		{
			var oldValue = base.JE_OH_Importer;
			base.JE_OH_Importer = value;
			if (oldValue != value)
			{
				CustomsEntryHeaders?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("D2D158FF-53BB-E08F-429C-72A9B41A71C1", Caption = "Entry Status")]
	public override ZString JE_EntryStatusDescription
	{
		get => base.JE_EntryStatusDescription;
	}

	[ResourceStringData("A17C5D93-BBF7-D6B8-4485-E676AED21C30", Caption = "Message Status")]
	public ZString MessageStatus => Factory.GetValue(ref messageStatusCached, GetMessageStatus);

	CachedProperty<ZString> messageStatusCached;

	ZString GetMessageStatus()
	{
		var messageStatuses = ActiveEntryHeaders.Cast<CusEntryHeader>().Select(entry => entry.CH_Status).Distinct().Take(2).ToArray();
		return messageStatuses.Length switch
		{
			2 => CommonEntryStatusList.Descriptions.MultipleEntryStatus,
			1 => Lookups.MessageStatusList.GetDescriptionFromCode(messageStatuses[0]),
			_ => ZString.Empty
		};
	}

	public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(nameof(MessageStatus));

	[ResourceStringData("8A116003-CBE9-B28C-4955-71AB941AC2E3", Caption = "Customs Transport Mode", ShortCaption = "Cus. Transp. Mode", FullDescription = "Transport mode at border passing for the EDI to Norwegian customs.")]
	[MaxLength(2)]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.NOCustomsTransportModeList))]
	public override ZString JE_CustomsTransportMode
	{
		get => base.JE_CustomsTransportMode;
		set => base.JE_CustomsTransportMode = value;
	}

	public bool TransportModeRequiresTransportNationality => !transportModesNotRequiringTransportNationality.Contains(JE_TransportMode);

	static ImmutableHashSet<string> transportModesNotRequiringTransportNationality => ImmutableHashSet.Create(
		TransportTypeList.Codes.FixedTransportInstallations,
		TransportTypeList.Codes.Mail,
		TransportTypeList.Codes.Rail);

	public override ZString JE_GS_NKCusAgent
	{
		get => base.JE_GS_NKCusAgent;
		set
		{
			base.JE_GS_NKCusAgent = value;
			if (value.IsEmpty)
			{
				SetDefaultValueForBroker();
			}
		}
	}

	public override void OnSaving()
	{
		if(JE_GS_NKCusAgent.IsEmpty)
		{
			SetDefaultValueForBroker();
		}
		base.OnSaving();
	}

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobDeclarationFetchStrategy(this);

	protected override bool IsEntryInstructionRequiredCore => true;

	protected override string DefaultTotalNoOfPacksPackType => string.Empty;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
		JE_MergeBy = Env.Registry.CommercialInvoiceLineMergeMethod;
		SetDefaultValueForBroker();
		SetDefaultValuesForDeclarantWithOrgProxy();
	}

	void SetDefaultValueForBroker()
	{
		if(!Env.CurrentUser.IsSystemAccount)
		{
			base.JE_GS_NKCusAgent = Env.CurrentUser.Initials;
		}
	}

	void SetDefaultValuesForDeclarantWithOrgProxy()
	{
		if (GlbBranch.CurrentBranch.OrgProxy is { } orgProxy1)
		{
			JE_OA_DeclarantAddress = orgProxy1.MainAddress.PK;
		}
		else if (GlbCompany.CurrentCompany.OrgProxy is { } orgProxy2)
		{
			JE_OA_DeclarantAddress = orgProxy2.MainAddress.PK;
		}
	}

	protected override ZAddress GetNewJE_OA_DeclarantAddress_ZAddress()
	{
		var result = base.GetNewJE_OA_DeclarantAddress_ZAddress();
		result.GetDefaultAddress = GetMainAddressOfOrganisation;
		return result;
	}

	protected override Customs.Business.MergeManager GetMergeManager()
	{
		return new MergeManager(this);
	}

	protected override DocumentSupporter CreateNewDocumentSupporter()
	{
		return new JobDeclarationDocumentSupporter(this);
	}

	ZGuid GetMainAddressOfOrganisation(IOrgHeader orgHeader)
	{
		return orgHeader?.MainAddress?.PK ?? ZGuid.Empty;
	}

	ZBool IsCountryInEuOrEfta(ZString countryCode)
	{
		var loader = Factory.GetCachedValue("AEBB3D5D-CE78-E9A7-4D11-48AA02830106", () => new CusRefTradeGroupView.Loader(Factory));
		var result =  loader.IsCountryPartOfTradeGroup(countryCode, TradeGroupCodeConstans.CodeTEF, Core.Constants.CountryCodes.Norway, ZDateTime.Today)
			|| loader.IsCountryPartOfTradeGroup(countryCode, TradeGroupCodeConstans.CodeTEFT, Core.Constants.CountryCodes.Norway, ZDateTime.Today)
			|| loader.IsCountryPartOfTradeGroup(countryCode, TradeGroupCodeConstans.CodeTOES, Core.Constants.CountryCodes.Norway, ZDateTime.Today);
		return result;
	}

	protected override void CleanUpNewDeclarationAfterCloneCore(BaseJobDeclaration newDeclaration, CloneType cloneType)
	{
		base.CleanUpNewDeclarationAfterCloneCore (newDeclaration, cloneType);
		newDeclaration.JE_GS_NKCusAgent = JE_GS_NKCusAgent;
	}

	public bool IsInAStatusAmendmentSendable => false;

	public IMessageManager GetMessageManagerForAmendmentDetection()
	{
		throw new NotImplementedException();
	}
	public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue()
	{
		throw new NotImplementedException();
	}

	[ResourceStringData("01892556-2261-AEAD-4E3C-4E5B0666E4BE", Caption = "Phase Status")]
	public override ZString PhaseStatusDescription => base.PhaseStatusDescription;

	internal ZString GoodsOriginName => GetGoodsOriginNameCore();

	ZString GetGoodsOriginNameCore()
	{
		if (!JE_GoodsOrigin.IsEmpty && Lookups.GoodsOrigin is RefCountryCollection countryCollection)
		{
			return countryCollection.Cast<RefCountry>().FirstOrDefault(c => c.Code == JE_GoodsOrigin)?.Description ?? ZString.Empty;
		}
		return ZString.Empty;
	}

	internal OrgHeader Declarant => DeclarantAddress?.Header;

	internal ZString CustomsOfficeName => JE_CustomsOffice.IsEmpty
		? ZString.Empty
		: Lookups.CustomsOfficeList.GetDescriptionFromCode(JE_CustomsOffice);

	internal ZString LocationOfGoodsDescription => JE_LocationOfGoods.IsEmpty
		? ZString.Empty
		: Lookups.LocationOfGoodsCollection.GetDescriptionFromCode(JE_LocationOfGoods);
}
