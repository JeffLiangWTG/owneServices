using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.PL.Business;

public class BaseMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<BaseMessageSendingObject>
	, IJobDeclarationMessageSendingObjectParent
{
	public BaseMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
	{
		using var suspender = GetValidationSuspender();
		using var changes = SuspendSettingHasChanges();
		CustomsOffice = declaration.JE_CustomsOffice;
	}

	public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

	protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
	{
		return new BaseMessageSendingObject((Declaration.CusEntryHeader)header);
	}

	public BaseMessageSendingObjectParentValidation Validation => GetNewValidation();

	protected virtual BaseMessageSendingObjectParentValidation GetNewValidation() => new BaseMessageSendingObjectParentValidation(this);

	protected override void RunPreSaveValidationCore()
	{
		base.RunPreSaveValidationCore();
		Validation.ValidateAll();
	}

	public IEnumerable<BaseMessageSendingObject> ObjectsToSend
	{
		get => SendingObjectsCollection.OfType<BaseMessageSendingObject>().Where(x => x.ShouldSend);
	}

	protected override NonPersistentBusinessObjectCollection<BaseMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		var sendingObjectsCollection = new BaseMessageSendingObjectCollection(Factory);
		ParentDeclaration.ActiveEntryHeaders
			.Cast<Declaration.CusEntryHeader>()
			.ForEach(x => sendingObjectsCollection
				.Add(CreateNewJobDeclarationMessageSendingObject(x)));
		return sendingObjectsCollection;
	}

	protected override void HookMessageSendingObjectEvents(Customs.Business.BaseMessageSendingObject bo)
	{
		base.HookMessageSendingObjectEvents(bo);
		if (bo is BaseMessageSendingObject baseMessageSendingObject)
		{
			baseMessageSendingObject.ActionInfo.ValueChanged += ActionInfo_ValueChanged;
		}
	}

	void ActionInfo_ValueChanged(object sender, EventArgs e)
	{
		ResetValidationMessages();
	}

	IEnumerable<Customs.Business.BaseMessageSendingObject> IJobDeclarationMessageSendingObjectParent.SendingObjectsCollection => SendingObjectsCollection.Cast<BaseMessageSendingObject>();

	[ResourceStringData("PLJobDeclarationMessageSendingObjectParent|AllowSendWithError", Caption = "Continue to send even though the selected message(s) contains validation errors?")]
	public ZBool AllowSendWithError
	{
		get => allowSendWithError;
		set => SetNonPersistentPropertyValue(AllowSendWithErrorInfo, ref allowSendWithError, value);
	}
	ZBool allowSendWithError;

	public ZPropertyInfo AllowSendWithErrorInfo => GetZPropertyInfo(nameof(AllowSendWithError));

	[ResourceStringData("PLJobDeclarationMessageSendingObjectParent|FallbackSystem", Caption = "Send using fallback system - KOMUNIKATOR+")]
	public ZBool FallbackSystem
	{
		get => fallbackSystem;
		set => SetNonPersistentPropertyValue(FallbackSystemInfo, ref fallbackSystem, value);
	}
	ZBool fallbackSystem;

	[ChildEditable(true)]
	public AlternativeEvidenceCollection AlternativeEvidences
	{
		get
		{
			if (alternativeEvidences == null)
			{
				alternativeEvidences = new AlternativeEvidenceCollection(this);
				RegisterEditableChildObject(alternativeEvidences);
			}
			return alternativeEvidences;
		}
	}
	AlternativeEvidenceCollection alternativeEvidences;

	public ZBool HasEvidences => SendingObjectsCollection.OfType<BaseMessageSendingObject>().Any(x => AESMessageSender.MessageWithEvidences.Contains(x.Action.ToString()));

	public ZPropertyInfo FallbackSystemInfo => GetZPropertyInfo(nameof(FallbackSystem));

	public BaseMessageSendingObjectParentLookups Lookups
	{
		get
		{
			if (lookups == null || !IsLookupsCachedInBase)
			{
				lookups = GetNewLookups();
			}

			return lookups;
		}
	}
	BaseMessageSendingObjectParentLookups lookups;

	protected virtual BaseMessageSendingObjectParentLookups GetNewLookups() => new BaseMessageSendingObjectParentLookups(this);

	public ZBool IsExport => ParentDeclaration.IsExport;

	[ChildEditable(true)]
	public JobDeclarationMessageSendingEDocsCollection EDocs
	{
		get
		{
			if (edocs == null)
			{
				edocs = CreateNewEDocsCollection();
				edocs.CountChanged += (s, e) => { HasEDocsInfo.RefreshBinding(); };
				RegisterEditableChildObject(edocs);
			}
			return edocs;
		}
	}
	JobDeclarationMessageSendingEDocsCollection edocs;

	public ZBool HasEDocs => EDocs.Count != 0;

	public ZPropertyInfo HasEDocsInfo => GetZPropertyInfo(nameof(HasEDocs));

	protected JobDeclarationMessageSendingEDocsCollection CreateNewEDocsCollection() => new JobDeclarationMessageSendingEDocsCollection(Factory, GetAvailableEDocList, this, GetDocumentNotes);

	AvailableEDocList GetAvailableEDocList() => new AvailableEDocList(ExtensionFilter, ParentDeclaration.DocManagerInfo.AllEDocs);

	List<ZString> ExtensionFilter => new List<ZString> { Core.Constants.FileFormats.PDF };

	Dictionary<ZGuid, ZString> GetDocumentNotes()
	{
		var typesDictionary = new Dictionary<ZGuid, ZString>();
		foreach (var eDoc in GetExistingEDocs())
		{
			var fileExtension = Path.GetExtension(eDoc.FileName).Replace(".", "");
			if (!ExtensionFilter.Any() || ExtensionFilter.Any(e => e.EqualsIgnoringCase(fileExtension)))
			{
				var requiredDocument = ParentDeclaration.DocsAndCartage.RequiredDocuments.GetDocByType(eDoc.DocType);
				if (requiredDocument != null && !requiredDocument.EQ_DocumentNotes.IsEmpty)
				{
					typesDictionary.Add(eDoc.UniqueKey, requiredDocument.EQ_DocumentNotes);
				}
			}
		}

		return typesDictionary;

		IEnumerable<IeDoc> GetExistingEDocs() => ParentDeclaration.DocManagerInfo.AllEDocs is IStorageDocsBaseCollection storageDocsBaseCollection
			? storageDocsBaseCollection
				.Cast<IeDoc>()
				.Where(x => x != null && !x.IsDeleted)
			: Array.Empty<IeDoc>();
	}

	[List(nameof(ParentDeclaration) + "." + nameof(JobDeclaration.Lookups) + "." + nameof(Declaration.JobDeclarationLookups.CustomsOffices))]
	[ResourceStringData("92d1b80c-7129-4d11-9e34-7d42e7bcfcf5", Caption = "Customs Office")]
	public ZString CustomsOffice
	{
		get => customsOffice;
		set
		{
			SetNonPersistentPropertyValue(CustomsOfficeInfo, ref customsOffice, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateCustomsOffice();
			}
		}
	}

	ZString customsOffice;

	public ZPropertyInfo CustomsOfficeInfo => GetZPropertyInfo(nameof(CustomsOffice));

	[List(nameof(Lookups) + "." + nameof(BaseMessageSendingObjectParentLookups.PurposeOfSendingList))]
	[ResourceStringData("3b9d15d0-fc43-4fd7-a8b9-dc1ae55cdcc6", Caption = "Purpose of Sending")]
	public ZString PurposeOfSending
	{
		get => purposeOfSending;
		set
		{
			SetNonPersistentPropertyValue(PurposeOfSendingInfo, ref purposeOfSending, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidatePurposeOfSending();
			}
		}
	}

	ZString purposeOfSending;

	public ZPropertyInfo PurposeOfSendingInfo => GetZPropertyInfo(nameof(PurposeOfSending));

	[ResourceStringData("64c6dd55-278f-4be3-8f54-2fc8747c1fa9", Caption = "Ref number")]
	public ZString RefNumber
	{
		get => refNumber;
		set
		{
			SetNonPersistentPropertyValue(RefNumberInfo, ref refNumber, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateRefNumber();
			}
		}
	}

	ZString refNumber;

	public ZPropertyInfo RefNumberInfo => GetZPropertyInfo(nameof(RefNumber));

	[ResourceStringData("6ed87091-7b94-41f9-9ec6-ad499d917c0b", Caption = "MRN number")]
	public ZString MrnNumber
	{
		get => mrnNumber;
		set
		{
			SetNonPersistentPropertyValue(MrnNumberInfo, ref mrnNumber, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateMrnNumber();
			}
		}
	}

	ZString mrnNumber;

	public ZPropertyInfo MrnNumberInfo => GetZPropertyInfo(nameof(MrnNumber));

	[ResourceStringData("47125b5a-1f77-453e-8fa8-e3129ee102eb", Caption = "Procedure")]
	[List(nameof(Lookups) + "." + nameof(BaseMessageSendingObjectParentLookups.ProcedureList))]
	public ZString Procedure
	{
		get => procedure;
		set
		{
			SetNonPersistentPropertyValue(ProcedureInfo, ref procedure, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateProcedure();
			}
		}
	}

	ZString procedure;

	public ZPropertyInfo ProcedureInfo => GetZPropertyInfo(nameof(Procedure));

	[ResourceStringData("90C825DF-99EA-433F-9590-B3239A02D934", Caption = "Enquiry Information Code")]
	[List(nameof(Lookups) + "." + nameof(BaseMessageSendingObjectParentLookups.EnquiryInformationCodeList))]
	public ZString EnquiryInformationCode
	{
		get => enquiryInformationCode;
		set
		{
			SetNonPersistentPropertyValue(EnquiryInformationCodeInfo, ref enquiryInformationCode, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateEnquiryInformationCode();
			}
		}
	}

	ZString enquiryInformationCode;

	public ZPropertyInfo EnquiryInformationCodeInfo => GetZPropertyInfo(nameof(EnquiryInformationCode));

	[List(nameof(ParentDeclaration) + "." + nameof(JobDeclaration.Lookups) + "." + nameof(Declaration.JobDeclarationLookups.CustomsOffices))]
	[ResourceStringData("2EEB4627-5A44-4431-834D-F2000000F6CE", Caption = "Office of Exit Actual")]
	public ZString OfficeOfExitActual
	{
		get
		{
			if (officeOfExitActual.IsEmpty)
			{
				officeOfExitActual = cusExitControlHeader?.CusExitDetails.OfType<CusExitDetail>().First()?.CED_CustomsOffice ?? ZString.Empty;
			}
			return officeOfExitActual;
		}
		set
		{
			SetNonPersistentPropertyValue(OfficeOfExitActualInfo, ref officeOfExitActual, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateOfficeOfExitActual();
			}
		}
	}

	ZString officeOfExitActual;

	public ZPropertyInfo OfficeOfExitActualInfo => GetZPropertyInfo(nameof(OfficeOfExitActual));

	[ResourceStringData("3953790E-6FC7-4635-94B3-B2838E965F20", Caption = "Exit Date")]
	public ZDateTime ExitDate
	{
		get
		{
			if (exitDate.IsEmpty)
			{
				exitDate = cusExitControlHeader?.CusExitDetails.OfType<CusExitDetail>().First()?.CED_ExitDate ?? ZDateTime.Empty;
			}
			return exitDate;
		}
		set
		{
			SetNonPersistentPropertyValue(ExitDateInfo, ref exitDate, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateExitDate();
			}
		}
	}

	ZDateTime exitDate;

	public ZPropertyInfo ExitDateInfo => GetZPropertyInfo(nameof(ExitDate));

	[ResourceStringData("8c29779a-c379-42c4-b9e1-94be0945e2ab", Caption = "Comments")]
	public ZString Comments
	{
		get => comments;
		set => SetNonPersistentPropertyValue(CommentsInfo, ref comments, value);
	}
	ZString comments;

	public ZPropertyInfo CommentsInfo => GetZPropertyInfo(nameof(Comments));

	public override bool AllowEmptyDeclaration => HasEDocs;

	public ZBool AnySelectedSendingObjects => SelectedSendingObjects.Any();

	public ZBool SendButtonEnabled => HasEDocs || AllowSendWithError || BizObjValidationMessageErrors.IsEmpty;

	protected CusExitControlHeader cusExitControlHeader => ParentDeclaration.GetExitControlHeader();
}
