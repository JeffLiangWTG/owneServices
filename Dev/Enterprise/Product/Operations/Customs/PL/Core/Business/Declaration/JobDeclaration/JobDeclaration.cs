using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class JobDeclaration : AutoPLJobDeclaration
	, Integration.Customs.PL.IJobDeclaration
	, IInvoicesProvider
{
	public JobDeclaration(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new partial class Schema : AutoPLJobDeclaration.Schema
	{
		public const string JE_OfficeOfEntryExit = nameof(JobDeclaration.JE_OfficeOfEntryExit);
		public const string GoodsLocationUNLocode = nameof(JobDeclaration.GoodsLocationUNLocode);
		public const string GoodsLocationCustomsOffice = nameof(JobDeclaration.GoodsLocationCustomsOffice);
		public const string EntryInstructionSubStyle = nameof(JobDeclaration.EntryInstructionSubStyle);
	}

	public override bool IsDeclarantAddressRequired => IsImport
		? JE_OA_Representative.IsEmpty && AllAdditionalInfos.All(x => x.CSI_Code != AdditionalInfoCodes._00500)
		: base.IsDeclarantAddressRequired;

	public IEnumerable<AdditionalInfo> AllAdditionalInfos => AdditionalInfos.Cast<AdditionalInfo>()
		.Concat(Invoices.Cast<JobComInvoiceHeader>().SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()))
		.Concat(InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()));

	public new JobDeclarationCustomsOfficeRequirementHelper CustomsOfficeRequirementHelper => base.CustomsOfficeRequirementHelper as JobDeclarationCustomsOfficeRequirementHelper;

	protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

	public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

	protected override Customs.Business.JobDeclarationValidation GetNewValidation() => this switch
	{
		_ when IsExport => new ExportJobDeclarationValidation(this),
		_ when IsImport => new ImportJobDeclarationValidation(this),
		_ when IsExitSummary => new ExitSummaryJobDeclarationValidation(this),
		_ => new JobDeclarationValidation(this)
	};

	public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

	protected override bool IsLookupsCachedInBase => false;

	protected override Customs.Business.JobDeclarationLookups GetNewLookups() => IsImport
		? new JobDeclarationLookups(this)
		: new ExportJobDeclarationLookups(this);

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

	protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this, new CusEntryInstructionComparer());

	public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

	public override EU.Business.Declaration.EntryCreationStrategy CreateEntryCreationStrategy()
	{
		if (IsExport)
		{
			return new ExportEntryCreationStrategy(this);
		}
		return new ImportEntryCreationStrategy(this);
	}

	public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => this switch
	{
		_ when IsExport => new ExportJobDocAddressValidation(addressToValidate, this),
		_ when IsImport => new ImportJobDocAddressValidation(addressToValidate, this),
		_ when IsExitSummary => new ExitSummaryJobDocAddressValidation(addressToValidate),
		_ => base.PiggyBackedDocAddressValidation(addressToValidate)
	};

	[ChildEditable]
	public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

	[ChildEditable(true)]
	public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

	public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

	protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

	protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(this);

	[ChildEditable(false)]
	public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

	public CusExitControlHeader GetExitControlHeader()
	{
		var query = new ZQuery(CusExitControlHeaderSchema.CEH_ParentID, PK);
		return Factory.LoadTop1<CusExitControlHeader>(query);
	}

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

	[ChildEditable(true)]
	public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

	protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

	protected override EU.Business.Declaration.CusEntryHeaderValidation GetCusEntryHeaderValidationCore(EU.Business.Declaration.CusEntryHeader entryHeader) => new CusEntryHeaderValidation(entryHeader);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		JE_DeclarantType = string.Empty;
	}

	protected override bool AgreedPlaceCodeSupportCore => false;
	protected override bool ZG_AgreedPlaceCodeValidationSupportCore => false;

	[ResourceStringData("PLJobDeclaration|ZG_AgreedPlaceCode", Caption = "Incoterm Place Code", MediumCaption = "Inco. Place Code", ShortCaption = "Inco. Place Code")]
	public override ZString ZG_AgreedPlaceCode { get => base.ZG_AgreedPlaceCode; set => base.ZG_AgreedPlaceCode = value; }

	#region CustomsOffices

	[ChildEditable(true)]
	public new PlOfficeCodeCollection CustomsOffices => (PlOfficeCodeCollection)base.CustomsOffices;

	protected override EuOfficeCodeCollection GetCustomsOffices() => new PlOfficeCodeCollection(this);

	#endregion

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		var result = base.GetCusCodeDataTypesCore();
		result[EU.Business.CusCodeDataTypeList.Codes.OfficeCode] = typeof(OfficeCode);
		return result;
	}

	#region CusSupportingInfoTypes

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;
	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	#endregion

	#region AddInfo

	[MaxLength(35)]
	public override ZString JE_UCR { get => base.JE_UCR; set => base.JE_UCR = value; }

	#region JE_MessageType

	protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
	{
		base.JE_MessageTypeChanged(oldValue, newValue);

		if (oldValue != newValue)
		{
			CustomsOffices.RemoveAndDeleteAll();
			ClearPropertiesOnMessageTypeChange();
			ClearAgreedPlaceCode(newValue);
			ReloadAuthorisationUsageMaxCountForValidation();
		}

		if (oldValue == MessageTypeList.Codes.Import)
		{
			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				line.AdditionalProcedureCodes.RemoveAndDeleteAll();
			}
		}
	}

	void ClearAgreedPlaceCode(ZString newMessageType)
	{
		if (newMessageType == MessageTypeList.Codes.Import)
		{
			Invoices.Cast<JobComInvoiceHeader>().ForEach(invoice => invoice.ZG_AgreedPlaceCode = ZString.Empty);
		}
	}

	void ReloadAuthorisationUsageMaxCountForValidation()
	{
		CustomsEntryInstructions.ForEach(instruction => ReloadAuthorisationUsageMaxCountForValidation(instruction));
	}

	void ReloadAuthorisationUsageMaxCountForValidation(CusEntryInstruction instruction)
	{
		if (instruction.CusAuthorizationUsages is CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> authorisations)
		{
			authorisations.ReloadMaxCountValidation();
		}
	}

	void ClearPropertiesOnMessageTypeChange()
	{
		JE_LocationOfGoods = ZString.Empty;
		JE_LocationQualifier = ZString.Empty;
		JE_LocationOtherInformation = ZString.Empty;
		JE_SubLocationOfGoods = ZString.Empty;
	}

	#endregion

	public override ZString OfficeOfExit => IsExport ? JE_OfficeOfEntryExit : ZString.Empty;

	public override ZString OfficeOfEntry => IsImport ? JE_OfficeOfEntryExit : ZString.Empty;

	#region JE_OfficeOfEntryExit

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.AdditionalCustomsOffices))]
	[MaxLength(10)]
	public ZString JE_OfficeOfEntryExit
	{
		get => ExitOffice?.CY_Data ?? ZString.Empty;
		set
		{
			if (!value.IsEmpty)
			{
				if (ExitOffice == null || ExitOffice.IsDeleted)
				{
					this.CustomsOffices.AddNew(codeForsecondaryOffice);
				}
				ExitOffice.CY_Data = value;
			}
			else if (!JE_OfficeOfEntryExit.IsEmpty)
			{
				ExitOffice.Delete();
			}
			Validation.ValidateJE_OfficeOfEntryExit();
			JE_OfficeOfEntryExitInfo.RefreshBinding();
		}
	}

	public ZString CustomsOfficeOfPresentationReferenceNumber() => CustomsOfficesForBinding.OfType<EuOfficeCode>()
																		.FirstOrDefault(o => o.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation && !o.CY_Data.IsEmpty)?.CY_Data
																	?? ZString.Empty;

	ZString codeForsecondaryOffice => IsImport ? EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent : EuOfficeCodesTypes.Codes.OfficeOfExit;

	EuOfficeCode ExitOffice
	{
		get
		{
			if (exitOfficeCache == null || exitOfficeCache.IsDeleted)
			{
				exitOfficeCache = this.CustomsOffices.GetFirstElementHaving(codeForsecondaryOffice);
			}
			return exitOfficeCache;
		}
	}

	EuOfficeCode exitOfficeCache;

	public ZPropertyInfo JE_OfficeOfEntryExitInfo => GetZPropertyInfo(Schema.JE_OfficeOfEntryExit);

	[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
	[ChildEditable(true)]
	public PlOfficeCodeCollectionForBinding CustomsOfficesForBinding
	{
		get
		{
			if (customsOfficesForBinding == null)
			{
				customsOfficesForBinding = new PlOfficeCodeCollectionForBinding(this);
				RegisterEditableChildObject(customsOfficesForBinding);
			}
			return customsOfficesForBinding;
		}
	}

	PlOfficeCodeCollectionForBinding customsOfficesForBinding;

	#endregion

	[ResourceStringData("PL_JobDeclaration|ZG_PresentationStartDate", Caption = "Presentation Start Date")]
	public override ZDateTime ZG_PresentationStartDate { get => base.ZG_PresentationStartDate; set => base.ZG_PresentationStartDate = value; }

	#endregion

	#region JE Properties

	public override ZString JE_GoodsDestination
	{
		get => base.JE_GoodsDestination;
		set
		{
			var oldValue = JE_GoodsDestination;
			base.JE_GoodsDestination = value;
			if (oldValue != JE_GoodsDestination && !IsCopying)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString JE_GoodsOrigin
	{
		get => base.JE_GoodsOrigin;
		set
		{
			var oldValue = JE_GoodsOrigin;
			base.JE_GoodsOrigin = value;
			if (oldValue != JE_GoodsOrigin && !IsCopying)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.PLRepresentationTypeList))]
	public override ZString JE_DeclarantType
	{
		get => base.JE_DeclarantType;
		set => base.JE_DeclarantType = value;
	}

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TypeOfLocationList))]
	[MaxLength(1)]
	public override ZString JE_LocationOtherInformation { get => base.JE_LocationOtherInformation; set => base.JE_LocationOtherInformation = value; }

	[ResourceStringData("PLJobDeclaration|JE_SubLocationOfGoods", Caption = "Sub-location")]
	[MaxLength(4)]
	public override ZString JE_SubLocationOfGoods { get => base.JE_SubLocationOfGoods; set => base.JE_SubLocationOfGoods = value; }

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GoodsLocationTypeList))]
	[MaxLength(nameof(JE_LocationQualifierMaxSize))]
	public override ZString JE_LocationQualifier
	{
		get => base.JE_LocationQualifier;
		set
		{
			var oldValue = JE_LocationQualifier;
			base.JE_LocationQualifier = value;
			if (oldValue != JE_LocationQualifier && !IsCopying)
			{
				JE_LocationOfGoods = IsImport ? JE_LocationOfGoods.Left(JE_LocationOfGoodsMaxSize) : ZString.Empty;
			}
		}
	}

	public int JE_LocationQualifierMaxSize => IsImport ? 3 : 1;

	[ReadOnly(true)]
	public ZString GoodsLocationQualifierDescription =>
		$"{Lookups.TypeOfLocationList.GetDescriptionFromCode(JE_LocationOtherInformation)} / {Lookups.GoodsLocationTypeList.GetDescriptionFromCode(JE_LocationQualifier)}";

	#region JE_LocationOfGoods

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GetAllEuropeanUnionCustomsOffices))]
	[MaxLength(nameof(JE_LocationOfGoodsMaxSize))]
	public override ZString JE_LocationOfGoods { get => base.JE_LocationOfGoods; set => base.JE_LocationOfGoods = value; }

	public int JE_LocationOfGoodsMaxSize => IsImport ? GetImportJE_LocationOfGoodsMaxSize() : GetExportJE_LocationOfGoodsMaxSize();

	int GetImportJE_LocationOfGoodsMaxSize()
	{
		var result = 100;
		switch (this.JE_LocationQualifier)
		{
			case GoodsLocationTypeList.Codes.CUS:
				result = 8;
				break;
			case GoodsLocationTypeList.Codes.GLC:
				result = 35;
				break;
			case GoodsLocationTypeList.Codes.OTH:
				result = 70;
				break;
		}
		return result;
	}

	int GetExportJE_LocationOfGoodsMaxSize()
	{
		var result = 100;
		switch (this.JE_LocationQualifier)
		{
			case QualifierOfTheIdentificationList.Codes.V:
				result = 8;
				break;
			case QualifierOfTheIdentificationList.Codes.U:
				result = 17;
				break;
			case QualifierOfTheIdentificationList.Codes.Y:
				result = 35;
				break;
		}
		return result;
	}

	public ZBool IsLocationOfGoodsFromOfficeList => JE_LocationQualifier == GoodsLocationTypeList.Codes.CUS
													|| JE_LocationQualifier == QualifierOfTheIdentificationList.Codes.V;

	public ZBool IsLocationOfGoodsFromUNLocode => JE_LocationQualifier == QualifierOfTheIdentificationList.Codes.U
												|| JE_LocationQualifier == QualifierOfTheIdentificationList.Codes.W;

	public ZBool IsLocationOfGoodsFreeText => IsImport && JE_LocationQualifier != string.Empty && JE_LocationQualifier != GoodsLocationTypeList.Codes.CUS;

	public ZBool IsAuthorisationNumberQualifier => IsExport && JE_LocationQualifier == QualifierOfTheIdentificationList.Codes.Y;

	public ZBool IsExportGoodsLocatedAtOfficeOfExit => IsExport && IsLocationOfGoodsFromOfficeList && !JE_LocationOfGoods.IsEmpty && JE_LocationOfGoods == JE_OfficeOfEntryExit;

	[BusinessObjectTestExclude]
	[ResourceStringData("PLJobDeclaration|GoodsLocationCustomsOffice", Caption = "Office")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GetAllEuropeanUnionCustomsOffices))]
	[MaxLength(8)]
	public ZString GoodsLocationCustomsOffice
	{
		get => IsLocationOfGoodsFromOfficeList ? JE_LocationOfGoods : ZString.Empty;
		set
		{
			JE_LocationOfGoods = value;
			Validation.ValidateGoodsLocationCustomsOffice();
			GoodsLocationCustomsOfficeInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo GoodsLocationCustomsOfficeInfo => GetZPropertyInfo(Schema.GoodsLocationCustomsOffice);

	[ResourceStringData("PLJobDeclaration|EntryInstructionSubStyle", Caption = "Entry Sub-style")]
	public ZString EntryInstructionSubStyle
	{
		get
		{
			var subStyles = new SortedSet<string>(CustomsEntryInstructions.Where(e => !e.CEI_SubStyle.IsEmpty).Select(e => e.CEI_SubStyle.ToString()));
			return string.Join(",", subStyles);
		}
	}

	[BusinessObjectTestExclude]
	[ResourceStringData("PLJobDeclaration|GoodsLocationUNLocode", Caption = "UNLOCO")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.UNLocodeList))]
	[MaxLength(17)]
	public ZString GoodsLocationUNLocode
	{
		get => IsLocationOfGoodsFromUNLocode ? JE_LocationOfGoods : ZString.Empty;
		set
		{
			JE_LocationOfGoods = value;
			Validation.ValidateGoodsLocationUNLocode();
			GoodsLocationUNLocodeInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo GoodsLocationUNLocodeInfo => GetZPropertyInfo(Schema.GoodsLocationUNLocode);

	public RefUNLOCO GoodsLocationUNLocodeData
	{
		get
		{
			var goodsLocationUNLocode = GoodsLocationUNLocode;
			if (goodsLocationUNLocodeData == null || goodsLocationUNLocodeData.IsDeleted || goodsLocationUNLocodeData.RL_Code != goodsLocationUNLocode)
			{
				goodsLocationUNLocodeData = goodsLocationUNLocode.Length == 5 ? new RefUNLOCO.Loader(Factory).Load(goodsLocationUNLocode) : null;
			}
			return goodsLocationUNLocodeData;
		}
	}
	RefUNLOCO goodsLocationUNLocodeData;

	#endregion

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EntryStatusList))]
	public override ZString JE_EntryStatus { get => base.JE_EntryStatus; set => base.JE_EntryStatus = value; }

	[ResourceStringData("PLJobDeclaration|JE_LloydsIMO", Caption = "IMO No.", FullDescription = "Lloyds / IMO Number")]
	public override ZString JE_LloydsIMO { get => base.JE_LloydsIMO; set => base.JE_LloydsIMO = value; }

	[ResourceStringData("PLJobDeclaration|JE_VesselName", Caption = "Vessel", FullDescription = "Vessel Name")]
	public override ZString JE_VesselName { get => base.JE_VesselName; set => base.JE_VesselName = value; }
	#endregion

	#region DocAddresses

	[ChildEditable(true)]
	[UniversalCopySplitCollection("Goods Location Address", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.GoodsLocation + "'")]
	public override JobDocAddressDependentCollection DocAddresses => base.DocAddresses;

	protected override DocAddressType[] SupportedAddressTypesCore
	{
		get
		{
			var result = new List<DocAddressType>(base.SupportedAddressTypesCore);
			result.Add(DocAddressType.GoodsLocation);
			return result.ToArray();
		}
	}

	public JobDocAddress GoodsLocationAddress
	{
		get
		{
			if (fGoodsLocationAddress == null || fGoodsLocationAddress.IsDeleted)
			{
				fGoodsLocationAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.GoodsLocation);
				OnGoodsLocationAddressFoundOrCreated();
			}
			return fGoodsLocationAddress;
		}
	}
	JobDocAddress fGoodsLocationAddress;

	protected void OnGoodsLocationAddressFoundOrCreated()
	{
		fGoodsLocationAddress.OnRelationshipFieldsChanged += new EventHandler(GoodsLocationAddress_OnRelationshipFieldsChanged);
	}

	void GoodsLocationAddress_OnRelationshipFieldsChanged(object sender, EventArgs e)
	{
		MarkAsNeedingValidation();
	}

	#endregion

	#region Implementation

		#region protected override

	protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;
	protected override bool IsCustomsLineAmendmentATotalReplacement => false;
	protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);
	public new MergeManager MergeManager => (MergeManager)base.MergeManager;
	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Poland;
	protected override bool HasSplitEntriesCore
	{
		get
		{
			if (!GetType().FullName.Contains("PL"))
			{
				ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
			}
			return base.HasSplitEntriesCore;
		}
	}

	#endregion

	IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
	{
		return new DeclarationValueChangedAnnouncer(this);
	}

	#endregion

	public bool HasOfficeOfPresentationPL => Factory.GetValue(ref hasOfficeOfPresentationPL, () => CustomsOfficesForBinding.Cast<EuOfficeCode>()
		.Any(office => office.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation
						&& office.CY_Data.Left(2) == CountryCodes.Poland));
	CachedProperty<bool> hasOfficeOfPresentationPL;

	protected override ZString CustomsVATTypeCaptionCore => IsImport
		? PLCustomsVATTypeCaption
		: base.CustomsVATTypeCaptionCore.ToString();

	protected virtual string PLCustomsVATTypeCaption => Res.GetString("PLJobDeclaration|PLCustomsVATTypeCaption", "PTU");

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MethodOfPaymentList))]
	public override ZString JE_ExciseCode { get => base.JE_ExciseCode; set => base.JE_ExciseCode = value; }

	[ResourceStringData("PLJobDeclaration|ZG_ExciseCode", Caption = "Excise Method of Payment", MediumCaption = "Excise MoP", ShortCaption = "Excise", FullDescription = "Excise Method of Payment Type")]
	public override ZString ZG_ExciseCode { get => base.ZG_ExciseCode; set => base.ZG_ExciseCode = value; }

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MethodOfPaymentList))]
	[MaxLength(1)]
	public override ZString JE_VATDeferType { get => base.JE_VATDeferType; set => base.JE_VATDeferType = value; }

	[ResourceStringData("PLJobDeclaration|ZG_VATDeferType", Caption = "VAT Method of Payment", MediumCaption = "VAT MoP", ShortCaption = "VAT", FullDescription = "VAT Method of Payment Type")]
	public override ZString ZG_VATDeferType { get => base.ZG_VATDeferType; set => base.ZG_VATDeferType = value; }

	protected override bool UseDeclarationContainersIfNoneFoundOnEntryCore => false;

	protected override bool UseDeclarationContainersForSingleContainerWhenMultipleEntriesExistCore => false;

	[ChildEditable(true)]
	public new BaseDeclarationLevelPackageCollection<Package> Packages => (BaseDeclarationLevelPackageCollection<Package>)base.Packages;

	protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new BaseDeclarationLevelPackageCollection<Package>(this);

	protected override ZQuery GetDiscardedMessagesFilter() => new ZQuery(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Discarded);

	protected EDIMessageCollection GetNewAttachmentMessagesCollection() => new EDIMessageCollection(this, AttachmentMessageFilter);

	protected ZQuery AttachmentMessageFilter
	{
		get
		{
			ZQuery zQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, base.PK);
			zQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EdiMessageMessageType.Attachment);

			return zQuery;
		}
	}

	public EDIMessageCollection AttachmentMessages
	{
		get
		{
			if (fAttachmentMessages == null)
			{
				fAttachmentMessages = GetNewAttachmentMessagesCollection();
				fAttachmentMessages.Load();
				fAttachmentMessages.IsManagedForDataRefresh = true;
			}

			return fAttachmentMessages;
		}
	}
	EDIMessageCollection fAttachmentMessages;

	[ChildEditable(true)]
	public new DeclarationLevelPackingGroupCollection PackingGroups => (DeclarationLevelPackingGroupCollection)base.PackingGroups;

	protected override BaseDeclarationLevelPackingGroupCollection CreateNewPackingGroups() => new DeclarationLevelPackingGroupCollection(this);

	protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
	{
		return new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
	}

	protected override IValueSetStrategy GetValueSetStrategy() => new JobDeclarationValueSetStrategy(this);

	protected override EUCommonConstants.TransportModeSource TransportMeansDependencyCore => EUCommonConstants.TransportModeSource.InlandTransportMode;

	public override ZString JE_TransportIDInland
	{
		get => base.JE_TransportIDInland;
		set
		{
			var oldValue = JE_TransportIDInland;
			base.JE_TransportIDInland = value;
			if (oldValue != JE_TransportIDInland && !IsCopying
												&& JE_TransportModeInland == TransportModes.Sea
												&& !(VesselInland?.RV_RN_NKCountryOfReg.IsEmpty ?? true))
			{
				JE_RN_NKTransportNationalityInland = VesselInland.RV_RN_NKCountryOfReg;
			}
		}
	}

	public RefVessel VesselInland => JE_TransportMeans == ExportBorderTransportMeansList.Codes._11
		? Lookups.GetVesselByVesselName(JE_TransportIDInland)
		: Lookups.GetVesselByLloydsNumber(JE_TransportIDInland);

	public ZBool IsExitSummary => JE_MessageType == PLJobMessageTypeList.Codes.ExitSummary;

	internal bool IsSupplementaryDeclaration
	{
		get
		{
			if (!isSupplementaryDeclaration.HasValue)
			{
				LoadIsSupplementaryDeclaration();
			}
			return isSupplementaryDeclaration!.Value;
		}
	}
	bool? isSupplementaryDeclaration;

	internal void LoadIsSupplementaryDeclaration() => isSupplementaryDeclaration
		= JE_MessageType == EUJobMessageTypeList.Codes.Export
		&& CustomsEntryInstructions.Count > 0
		&& CustomsEntryInstructions.All(x => x.IsSupplementary);
}
