using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class AdditionalDocumentMessageSendingObject : MessageSendingObject, IAdditionalSupportingDocument
	{
		public AdditionalDocumentMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		public new static class TWSchema
		{
			public const string ContactOffice = "ContactOffice";
			public const int ContactOfficeMaxLength = 3;
		}

		protected override JobDeclarationMessageSendingObjectValidation GetNewValidation()
		{
			return new AdditionalDocumentMessageSendingObjectValidation(this);
		}

		public new AdditionalDocumentMessageSendingObjectValidation Validation => (AdditionalDocumentMessageSendingObjectValidation)base.Validation;

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			DefaultActionIfRequired();
			SetShouldSendtDefaultValue();
		}

		void DefaultActionIfRequired()
		{
			if (!IsMessageTypeADM)
			{
				if (Header.CusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == CusDispositionStatusKeyList.Codes.RFM
					|| (x.CDI_StatusKey == CusDispositionStatusKeyList.Codes.ARM && (x.CDI_Status.StartsWith(Constants.CDIStatus.B) || x.CDI_Status.StartsWith(Constants.CDIStatus.F)))))
				{
					Action = ActionCodeList.Codes.Update;
				}
				else
				{
					Action = ActionCodeList.Codes.Create;
				}
			}
		}

		protected virtual void SetShouldSendtDefaultValue() => ShouldSend = true;

		public override CodeDescriptionPairList ActionList
		{
			get
			{
				return Factory.GetCachedValue(string.Concat(nameof(AdditionalDocumentMessageSendingObject), nameof(ActionList)), () =>
				{
					var list = new ActionCodeList();

					list.RemoveCode(ActionCodeList.Codes.Update);
					list.RemoveCode(ActionCodeList.Codes.Delete);

					return list;
				});
			}
		}

		public override ZString FriendlyNameForMessageManager => Res.GetString("44E3EDB2-2D95-4593-9660-8936BE5503C9", "Send Additional Document Message");

		public override ZString GetMessageOwner()
		{
			if (Header.Declaration is JobDeclaration declaration)
			{
				var orgHeader = declaration.IsImport ? declaration.Buyer : declaration.Supplier;
				return OrgHeaderHelper.GetVatOrPasOrPid(orgHeader);
			}
			return ZString.Empty;
		}

		#region ContactOffice
		[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObject|ContactOffice", Caption = "Contact Office")]
		[MaxLength(TWSchema.ContactOfficeMaxLength)]
		[List(nameof(ContactOfficeList))]
		public ZString ContactOffice
		{
			get => contactOffice;
			set
			{
				SetNonPersistentPropertyValue(ContactOfficeInfo, ref contactOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactOffice();
				}
			}
		}
		ZString contactOffice;

		public ZPropertyInfo ContactOfficeInfo => GetZPropertyInfo(TWSchema.ContactOffice);

		public CodeDescriptionPairList ContactOfficeList => TWRefCusCodeListTypes.GetContactOfficeList(Factory);
		#endregion

		public ZBool IsMessageTypeADM => MessageType == MessageTypeList.Codes.ADM;

		#region Documents
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(Factory, AvailableEDocs, GetAllEDocs());
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		public IStorageDocsBaseCollection[] GetAllEDocs() => Header.GetAllEDocs().ToArray();

		public IeDoc GetIeDocFromUniqueKey(Guid uniqueKey) => GetAllEDocs().GetFromUniqueKey(uniqueKey);

		public AvailableEDocList AvailableEDocs => GetAvailableEDocList(ExtensionFilter, GetAllEDocs());

		AvailableEDocList GetAvailableEDocList(List<ZString> filter, params IStorageDocsBaseCollection[] eDocCollections)
		{
			return new AvailableEDocList(filter, eDocCollections);
		}

		SupportingDocumentCollection IAdditionalSupportingDocument.GetSupportingDocuments() => SupportingDocuments;

		protected virtual List<ZString> ExtensionFilter => new() { Core.Constants.FileFormats.PDF, Core.Constants.FileFormats.JPG, Core.Constants.FileFormats.GIF };
		#endregion
	}
}
