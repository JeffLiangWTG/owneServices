using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public partial class SupportingDocSendingObject : AutoSupportingDocSendingObject
	{
		public SupportingDocSendingObject(ISupportingDocObject supportingDocObject) : base(supportingDocObject.Factory)
		{
			using (SuspendSettingHasChanges())
			{
				SupportingDocObject = Argument.NotNull(supportingDocObject, "supportingDocObject");
			}
		}

		public static SupportingDocSendingObject New(BaseJobDeclaration declaration)
		{
			var result = declaration.GetSupportingDocSendingObject();
			result.ShouldSend = true;
			result.DefaultLocalReferenceNumber();
			return result;
		}

		public ISupportingDocObject SupportingDocObject { get; }

		#region Override Properties

		[List(nameof(AvailableEDocs), nameof(ICodeDescription.PK), nameof(ICodeDescription.Code), AllowOnlyTheseValues = true)]
		public override ZGuid EDoc
		{
			get { return base.EDoc; }
			set
			{
				var oldValue = EDoc;
				if (oldValue != value)
				{
					document = null;
				}
				base.EDoc = value;
				if (oldValue != EDoc)
				{
					DefaultDocType();
					eDocFileSizeInMB = GetFileSizeInMB();
					Validation.ValidateEDocFileSizeInMB();
				}
			}
		}

		[List(nameof(DocumentTypeList))]
		public override ZString DocumentType
		{
			get { return base.DocumentType; }
			set { base.DocumentType = value; }
		}

		[List(nameof(Entries))]
		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.Business.SupportingDocSendingObject|ReferenceNumber", ShortCaption = "Entry (MRN or functional reference)", Caption = "Entry Reference")]
		public override ZString LocalReferenceNumber
		{
			get { return base.LocalReferenceNumber; }
			set { base.LocalReferenceNumber = value; }
		}

		[List(nameof(CaseNumbers))]
		public override ZString CaseNumber
		{
			get { return base.CaseNumber; }
			set { base.CaseNumber = value; }
		}

		public override ZDecimal EDocFileSizeInMB => eDocFileSizeInMB;
		ZDecimal eDocFileSizeInMB;

		#endregion

		#region New Properties

		public IeDoc Document
		{
			get
			{
				if (document == null && EDoc.IsValid)
				{
					var edocKey = EDoc.ToGuid();
					document = AllEDocsList.Select(x => x.GetFromUniqueKey(edocKey)).WhereNotNull().FirstOrDefault();
					GetExtraDocsIfNoneAvailable(edocKey);
				}
				return document;
			}
		}

		protected IeDoc document;

		protected virtual void GetExtraDocsIfNoneAvailable(System.Guid edocKey)
		{
		}

		public virtual bool ShouldCheckSizeInEdocField => true;

		public virtual bool ShouldCheckFileNameInEdocField => false;

		#endregion

		#region Lookup Lists

		public virtual CodeDescriptionPairList Entries => new CodeDescriptionPairList();

		public CodeDescriptionPairList DocumentTypeList => Universal.RefCusCodeListTypes.GetCachedList(Factory, SupportingDocObject.CountryCode, DocumentTypeCode, ZDateTime.Today);

		protected virtual ZString DocumentTypeCode => string.Empty;

		public AvailableEDocList AvailableEDocs => GetAvailableEDocList(ExtensionFilter, AllEDocsList.ToArray());

		protected virtual IEnumerable<IStorageDocsBaseCollection> AllEDocsList => EDocsHelper.GetEDocCollections(SupportingDocObject);

		protected virtual AvailableEDocList GetAvailableEDocList(List<ZString> filter, params IStorageDocsBaseCollection[] eDocCollections)
		{
			return new AvailableEDocList(filter, eDocCollections);
		}

		protected virtual List<ZString> ExtensionFilter => new List<ZString>();

		public virtual CodeDescriptionPairList CaseNumbers => new CodeDescriptionPairList();

		#endregion

		#region Implementation

		protected virtual void DefaultDocType() { }

		protected virtual void DefaultLocalReferenceNumber() { }

		protected ZDecimal GetFileSizeInMB() => Document?.FileSizeInMB ?? ZDecimal.Zero;

		#endregion

		public virtual SupportingDocUniversalEventBuilder GetSupportingDocUniversalEventBuilder()
		{
			return this is ISupportingDocumentMessageDataProvider dataWrapper ? new SupportingDocUniversalEventBuilder(dataWrapper) : null;
		}
	}
}
