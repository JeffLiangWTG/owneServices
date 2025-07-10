using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public abstract partial class LicensingMessageSendingObject : ControllingMessageSendingObject,
		INXDeclaration,
		IAdditionalSupportingDocument
	{
		public static class TWSchema
		{
			public const string Action = nameof(LicensingMessageSendingObject.Action);
			public const int ActionMaxLength = 2;
			public const string BusinessType = nameof(LicensingMessageSendingObject.BusinessType);
			public const int BusinessTypeMaxLength = 0;
			public const string ProcessingUnit = nameof(LicensingMessageSendingObject.ProcessingUnit);
			public const int ProcessingUnitMaxLength = 0;
			public const string ReasonDescription = nameof(LicensingMessageSendingObject.ReasonDescription);
			public const int ReasonDescriptionMaxLength = 240;
		}

		public LicensingMessageSendingObject(CusTWControllingMessageHeader header) : base(header)
		{
			Declaration = Argument.NotNull(header.Declaration, nameof(header.Declaration));
		}

		protected JobDeclaration Declaration { get; }

		protected bool IsImport => Declaration.IsImport;

		protected bool IsExport => Declaration.IsExport;

		public ZDate AcceptanceDateTime => GetAcceptanceDateTimeCore();
		public ZString FunctionalReferenceID => GetFuncaionalReferenceIDCore();
		public ZString FunctionCode => GetFunctionCodeCore();
		public ZString ID => GetIDCore();
		public ZDateTime IssueDateTime => ZDateTime.Now;
		public ZString TypeCode => GetTypeCodeCore();
		public ZString EM_MessageType => GetEM_MessageTypeCore();

		protected virtual ZDate GetAcceptanceDateTimeCore() => Declaration.CusEntryInstruction.CEI_DateForDuty.Date;
		protected virtual ZString GetFuncaionalReferenceIDCore() => MessageConstants.FunctionalReferenceIDPlaceHolder;
		protected virtual ZString GetFunctionCodeCore() => Action;
		protected virtual ZString GetIDCore() => Header.EntryNumberForSendingObject;
		protected virtual ZString GetTypeCodeCore() => Declaration.CusEntryInstruction.CEI_Style;
		protected abstract ZString GetEM_MessageTypeCore();

		protected virtual ZString DefaultAction => NXCommonActionCodeList.Codes._9;

		public IDeclarationAdditionalDocument AdditionalDocument => GetAdditionalDocumentCore();
		public IAdditionalInformation AdditionalInformation => GetAdditionalInformationCore();
		public IDeclarationAgent Agent => GetAgentCore();
		public ITransportMeans BorderTransportMeans => GetTransportMeansCore();
		public IConsignment Consignment => GetConsignmentCore();
		public ICurrencyExchange CurrencyExchange => GetCurrencyExchangeCore();
		public IGoodsShipment GoodsShipment => GetGoodsShipmentCore();
		public IPartyDetails Importer => GetImporterCore();
		public IApplication Application => GetApplicationCore();
		public IPackaging Packaging => GetPackagingCore();

		protected virtual IDeclarationAdditionalDocument GetAdditionalDocumentCore() => new LicensingMessageAdditionalDocument(Header);
		protected virtual IAdditionalInformation GetAdditionalInformationCore() => new AdditionalInformationWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ReasonDescription);
		protected virtual IDeclarationAgent GetAgentCore() => new LicensingMessageAgent(Header);
		protected virtual ITransportMeans GetTransportMeansCore() => new LicensingMessageTransportMeans(Declaration);
		protected virtual IConsignment GetConsignmentCore() => new LicensingMessageConsignment(Header);
		protected virtual ICurrencyExchange GetCurrencyExchangeCore() => new LicensingMessageCurrencyExchange(Header);
		protected virtual IGoodsShipment GetGoodsShipmentCore() => IsExport ? new ExportLicensingMessageGoodsShipment(Header) : new LicensingMessageGoodsShipment(Header);

		protected virtual IPartyDetails GetImporterCore() => new LicensingMessageImporter(Header.ImporterDocumentaryAddress);
		protected virtual IApplication GetApplicationCore() => new LicensingMessageApplication(Header, this);
		protected virtual IPackaging GetPackagingCore() => new LicensingMessagePackaging(Header);

		public override ZString MessageType => Header.TW1_ControllingMessageType;

		public override ZString Description => Header.ControllingMessageTypeDescription;

		public override ZString MessageNumber => Header.TW1_FunctionalReferenceId;

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.LicensingMessageSendingObject|BusinessType", Caption = "Business Type")]
		[MaxLength(TWSchema.BusinessTypeMaxLength)]
		public virtual ZString BusinessType => Header.TW1_BusinessType;

		public virtual ZPropertyInfo BusinessTypeInfo => GetZPropertyInfo(TWSchema.BusinessType);

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.LicensingMessageSendingObject|ProcessingUnit", Caption = "Processing Unit")]
		[MaxLength(TWSchema.ProcessingUnitMaxLength)]
		public virtual ZString ProcessingUnit => Header.TW1_ProcessingUnit;

		public virtual ZPropertyInfo ProcessingUnitInfo => GetZPropertyInfo(TWSchema.ProcessingUnit);

		public CodeDescriptionPairList TypeList => GetTypeListCore();

		protected virtual CodeDescriptionPairList GetTypeListCore() => new CodeDescriptionPairList();

		#region Validation

		protected override ControllingMessageSendingObjectValidation GetNewValidation() => new LicensingMessageSendingObjectValidation(this);

		public new LicensingMessageSendingObjectValidation Validation => base.Validation as LicensingMessageSendingObjectValidation;

		#endregion

		public override void ValidateShouldSend()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateShouldSend();
			}
		}

		#region Action

		ZString action;

		[List(nameof(Lookups) + "." + nameof(LicensingMessageSendingObjectLookups.ActionList))]
		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.LicensingMessageSendingObject|Action", Caption = "Action")]
		[MaxLength(TWSchema.ActionMaxLength)]
		public ZString Action
		{
			get => action;
			set
			{
				CheckMaximumLength(ActionInfo, value);
				SetNonPersistentPropertyValue(ActionInfo, ref action, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAction();
				}
			}
		}

		public ZPropertyInfo ActionInfo => GetZPropertyInfo(TWSchema.Action);

		#endregion

		#region Documents

		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					var allEDocs = GetAllEDocs();
					var availableEDocList = new AvailableEDocList(ExtensionFilter, allEDocs);
					supportingDocuments = new SupportingDocumentCollection(Factory, availableEDocList, allEDocs, TypeList);
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		public IStorageDocsBaseCollection[] GetAllEDocs() => Header.GetAllEDocs().ToArray();

		List<ZString> ExtensionFilter => new List<ZString>() { Core.Constants.FileFormats.PDF, Core.Constants.FileFormats.JPG, Core.Constants.FileFormats.GIF };

		#endregion

		#region ReasonDescription

		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.LicensingMessageSendingObject|ReasonDescription", ShortCaption = "Reason Description", Caption = "Reason Description")]
		[MaxLength(TWSchema.ReasonDescriptionMaxLength)]
		[ReadOnlyMember(nameof(ReasonDescription_Readonly))]
		public virtual ZString ReasonDescription
		{
			get
			{
				return reasonDescription;
			}
			set
			{
				CheckMaximumLength(ReasonDescriptionInfo, value);
				SetNonPersistentPropertyValue(ReasonDescriptionInfo, ref reasonDescription, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReasonDescription();
				}
			}
		}

		public virtual ZPropertyInfo ReasonDescriptionInfo => GetZPropertyInfo(TWSchema.ReasonDescription);

		ZString reasonDescription;

		protected virtual ZBool ReasonDescription_Readonly => ZBool.True;

		#endregion

		#region Lookups

		public LicensingMessageSendingObjectLookups Lookups => lookups ??= GetNewLookups();
		LicensingMessageSendingObjectLookups lookups;

		protected virtual LicensingMessageSendingObjectLookups GetNewLookups() => new LicensingMessageSendingObjectLookups(this);

		#endregion

		public ZString SerializeToMessageString() => GetMessageBuilder().SerializeToMessageString(this, FunctionCode);

		protected abstract ITWMessageBuilder GetMessageBuilder();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Action = DefaultAction;
		}

		SupportingDocumentCollection IAdditionalSupportingDocument.GetSupportingDocuments() => SupportingDocuments;
	}
}
