using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class JobDocumentRecipientWrapperBase : NonPersistentBusinessObject
	{
		protected JobDocumentRecipientWrapperBase(JobDocumentRecipientConfiguration configuration, ZGuid wrappedBizoPK)
		{
			Configuration = configuration;
			DocumentSupportable = configuration.DocumentSupportable;
			FactoryForSave = configuration.Factory;
			WrappedBizoPK = wrappedBizoPK;
		}

		protected JobDocumentRecipientConfiguration Configuration;
		protected readonly IDocumentSupportable DocumentSupportable;
		protected readonly BusinessObjectFactory FactoryForSave;
		public readonly ZGuid WrappedBizoPK;

		#region Organisation 

		[List(nameof(Organisations))]
		[ResourceStringData("JobDocumentRecipientWrapperBase.OrganisationPK", Caption = "Organization")]
		public abstract ZGuid OrganisationPK { get; set; }

		public abstract ZPropertyInfo OrganisationPKInfo { get; }

		public abstract OrgHeaderCollection Organisations { get; }

		public OrgHeader Organisation => FactoryForSave.GetCachedReadOnlyFactory().Load<OrgHeader>(OrganisationPK);

		#endregion

		#region Contact Name

		[List(nameof(Contacts))]
		[ResourceStringData("JobDocumentRecipientWrapperBase.ContactName", Caption = "Contact Name", ShortCaption = "Name")]
		public abstract ZString ContactName { get; set; }

		public abstract ZPropertyInfo ContactNameInfo { get; }

		public OrgContactCollectionForDelivery Contacts
		{
			get
			{
				OrgContactDependentCollection contactCollection;
				if (Organisation == null)
				{
					contactCollection = new OrgContactDependentCollection(FactoryForSave.GetCachedReadOnlyFactory());
				}
				else
				{
					var filter = new ZQuery(OrgContactSchema.OC_IsActive, true);
					contactCollection = new OrgContactDependentCollection(Organisation, filter);
					contactCollection.Load();
				}
				return new OrgContactCollectionForDelivery(contactCollection);
			}
		}

		#endregion

		#region Delivery Method

		[List(nameof(DeliveryMethods))]
		[ResourceStringData("JobDocumentRecipientWrapperBase.DeliveryMethod", Caption = "Delivery Method", ShortCaption = "Deliver")]
		public abstract ZString DeliveryMethod { get; set; }

		public abstract ZPropertyInfo DeliveryMethodInfo { get; }

		public abstract CodeDescriptionPairList DeliveryMethods { get; }

		#endregion

		#region Attachment Type

		[List(nameof(AttachmentTypes))]
		[ResourceStringData("JobDocumentRecipientWrapperBase.AttachmentType", Caption = "Attachment Type", ShortCaption = "Type")]
		public abstract ZString AttachmentType { get; set; }

		public abstract ZPropertyInfo AttachmentTypeInfo { get; }

		public abstract CodeDescriptionPairList AttachmentTypes { get; }

		#endregion

		#region Fax Number
		[ResourceStringData("JobDocumentRecipientWrapperBase.FaxNumber", Caption = "Fax Number", ShortCaption = "Fax")]
		public abstract ZString FaxNumber { get; set; }

		public abstract ZPropertyInfo FaxNumberInfo { get; }

		#endregion

		#region Document Group

		[List(nameof(DocumentGroups))]
		[ResourceStringData("JobDocumentRecipientWrapperBase.DocumentGroup", Caption = "Document Group", ShortCaption = "Doc. Group")]
		public abstract ZString DocumentGroup { get; set; }

		public abstract ZPropertyInfo DocumentGroupInfo { get; }

		public abstract CodeDescriptionPairList DocumentGroups { get; }

		#endregion

		#region Document

		[List(nameof(Documents))]
		[ResourceStringData("JobDocumentRecipientWrapperBase.DocumentPK", Caption = "Document")]
		public abstract ZGuid DocumentPK { get; set; }

		public abstract ZPropertyInfo DocumentPKInfo { get; }

		public UniqueDocumentCollectionView Documents => Configuration.Documents;

		public StmMenuItem Document => FactoryForSave.GetCachedReadOnlyFactory().Load<StmMenuItem>(DocumentPK);

		#endregion

		#region Email Recipients

		#region Email TO Recipients

		[ResourceStringData("JobDocumentRecipientWrapperBase.EmailToRecipientsAsString", Caption = "Email TO")]
		[List("CopyRecipientList")]
		[BusinessObjectTestExclude]
		public abstract ZString EmailToRecipientsAsString { get; set; }

		public abstract ZPropertyInfo EmailToRecipientsAsStringInfo { get; }

		[List(nameof(CopyRecipientList))]
		public abstract NonPersistentCopyRecipientCollection EmailToRecipients { get; }

		#endregion

		#region Carbon Copy Recipients

		[ResourceStringData("JobDocumentRecipientWrapperBase.CarbonCopyRecipientsAsString", Caption = "Email CC")]
		[List("CopyRecipientList")]
		[BusinessObjectTestExclude]
		public abstract ZString CarbonCopyRecipientsAsString { get; set; }

		public abstract ZPropertyInfo CarbonCopyRecipientsAsStringInfo { get; }

		[List(nameof(CopyRecipientList))]
		public abstract NonPersistentCopyRecipientCollection CarbonCopyRecipients { get; }

		#endregion

		#region Blind Carbon Copy Recipients

		[ResourceStringData("JobDocumentRecipientWrapperBase.BlindCarbonCopyRecipientsAsString", Caption = "Email BCC")]
		[List("CopyRecipientList")]
		[BusinessObjectTestExclude]
		public abstract ZString BlindCarbonCopyRecipientsAsString { get; set; }

		public abstract ZPropertyInfo BlindCarbonCopyRecipientsAsStringInfo { get; }

		[List(nameof(CopyRecipientList))]
		public abstract NonPersistentCopyRecipientCollection BlindCarbonCopyRecipients { get; }

		#endregion

		public List<string> CopyRecipientList => new List<string>();

		#endregion

		#region Email Subject Macro

		[ResourceStringData("JobDocumentRecipientWrapperBase.EmailSubjectMacro", Caption = "Email Subject")]
		public abstract ZString EmailSubjectMacro { get; set; }

		public abstract ZPropertyInfo EmailSubjectMacroInfo { get; }

		#endregion

		#region Defaulted From Organisation Record

		[ResourceStringData("JobDocumentRecipientWrapperBase.DefaultedFromOrganisationRecord", Caption = "Organization Record", ShortCaption = "Org. Record")]
		public abstract ZBool DefaultedFromOrganisationRecord { get; }

		public ZPropertyInfo DefaultedFromOrganisationRecordInfo => GetZPropertyInfo(nameof(DefaultedFromOrganisationRecord));

		#endregion

		#region IsExclusion

		[ResourceStringData("JobDocumentRecipientWrapperBase.IsExclusion", Caption = "Exclusion", FullDescription = "This recipient is excluded from receiving this document. This only applies to recipient records set up on the Organization Contact Document Delivery Details.")]
		[ReadOnly(true)]
		public abstract ZBool IsExclusion { get; }

		public ZPropertyInfo IsExclusionInfo => GetZPropertyInfo(nameof(IsExclusion));

		#endregion
	}
}
