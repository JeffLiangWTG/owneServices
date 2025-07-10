using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentRecipientWrapperForOrgDocument : JobDocumentRecipientWrapperBase
	{
		public JobDocumentRecipientWrapperForOrgDocument(OrgDocument orgDocument, JobDocumentRecipientConfiguration configuration)
			: base(configuration, orgDocument.PK)
		{
			OrgDocument = orgDocument;
		}

		public OrgDocument OrgDocument { get; }

		public override bool CanDelete => false;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("46332555-968d-4108-aeec-357a08444e1a", "The selected recipient is defaulted from the Organization record and cannot be deleted. Please edit the organization directly."); }
		}

		#region Properties

		#region Organisation

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZGuid OrganisationPK
		{
			get => OrgDocument.Contact.OC_OH;
			set { }
		}

		public override ZPropertyInfo OrganisationPKInfo => GetZPropertyInfo(nameof(OrganisationPK));

		public override OrgHeaderCollection Organisations => new OrgHeaderCollection(OrgDocument.Factory);

		#endregion

		#region Contact Name

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString ContactName
		{
			get => OrgDocument.Contact.OC_ContactName;
			set { }
		}

		public override ZPropertyInfo ContactNameInfo => GetZPropertyInfo(nameof(ContactName));

		#endregion

		#region DeliveryMethod

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString DeliveryMethod
		{
			get => OrgDocument.OD_DeliverBy;
			set { }
		}

		public override ZPropertyInfo DeliveryMethodInfo => GetZPropertyInfo(nameof(DeliveryMethod));

		public override CodeDescriptionPairList DeliveryMethods => OrgDocument.Lookups.OD_DeliverBy_List;

		#endregion

		#region Attachment Type

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString AttachmentType
		{
			get => OrgDocument.OD_AttachmentType;
			set { }
		}

		public override ZPropertyInfo AttachmentTypeInfo => GetZPropertyInfo(nameof(AttachmentType));

		public override CodeDescriptionPairList AttachmentTypes => OrgDocument.Lookups.OD_AttachmentType_List;

		#endregion

		#region Fax Number

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString FaxNumber
		{
			get => OrgDocument.Contact.OC_Fax;
			set { }
		}

		public override ZPropertyInfo FaxNumberInfo => GetZPropertyInfo(nameof(FaxNumber));

		#endregion

		#region Document Group

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString DocumentGroup
		{
			get => OrgDocument.OD_DocumentGroup;
			set { }
		}

		public override ZPropertyInfo DocumentGroupInfo => GetZPropertyInfo(nameof(DocumentGroup));

		public override CodeDescriptionPairList DocumentGroups => OrgDocument.Lookups.OD_DocumentGroup_List;

		#endregion

		#region Document

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZGuid DocumentPK
		{
			get => OrgDocument.OD_SU_MenuItem;
			set { }
		}

		public override ZPropertyInfo DocumentPKInfo => GetZPropertyInfo(nameof(DocumentPK));

		#endregion

		#region Email Recipients

		#region Email TO Recipients

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString EmailToRecipientsAsString
		{
			get => OrgDocument.Contact.OC_Email;
			set { }
		}

		public override ZPropertyInfo EmailToRecipientsAsStringInfo => GetZPropertyInfo(nameof(EmailToRecipientsAsString));

		public override NonPersistentCopyRecipientCollection EmailToRecipients => new NonPersistentCopyRecipientCollection(Organisation, Core.Constants.CopyRecipientType.EmailToRecipient);

		#endregion

		#region Carbon Copy Recipients

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString CarbonCopyRecipientsAsString
		{
			get => OrgDocument.OD_CarbonCopyRecipientsAsString;
			set { }
		}

		public override ZPropertyInfo CarbonCopyRecipientsAsStringInfo => GetZPropertyInfo(nameof(CarbonCopyRecipientsAsString));

		public override NonPersistentCopyRecipientCollection CarbonCopyRecipients => new NonPersistentCopyRecipientCollection(Organisation, Core.Constants.CopyRecipientType.CarbonCopyRecipient);

		#endregion

		#region Blind Carbon Copy Recipients

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString BlindCarbonCopyRecipientsAsString
		{
			get => OrgDocument.OD_BlindCarbonCopyRecipientsAsString;
			set { }
		}

		public override ZPropertyInfo BlindCarbonCopyRecipientsAsStringInfo => GetZPropertyInfo(nameof(BlindCarbonCopyRecipientsAsString));

		public override NonPersistentCopyRecipientCollection BlindCarbonCopyRecipients => new NonPersistentCopyRecipientCollection(Organisation, Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient);

		#endregion

		#endregion

		#region Email Subject Macro

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZString EmailSubjectMacro
		{
			get => OrgDocument.OD_EmailSubjectMacro;
			set { }
		}

		public override ZPropertyInfo EmailSubjectMacroInfo => GetZPropertyInfo(nameof(EmailSubjectMacro));

		#endregion

		#region Defaulted From Organisation Record

		[ReadOnly(true)]
		public override ZBool DefaultedFromOrganisationRecord => true;

		#endregion

		#region Exclude

		public override ZBool IsExclusion => false;

		#endregion

		#endregion
	}
}
