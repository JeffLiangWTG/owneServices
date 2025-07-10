using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentRecipientWrapperForJobDocumentDelivery : JobDocumentRecipientWrapperBase
	{
		public JobDocumentRecipientWrapperForJobDocumentDelivery(JobDocumentDelivery jobDocumentDelivery, JobDocumentRecipientConfiguration configuration)
			: base(configuration, jobDocumentDelivery.PK)
		{
			JobDocumentDelivery = jobDocumentDelivery;
		}

		public JobDocumentDelivery JobDocumentDelivery { get; }

		public override void Delete()
		{
			JobDocumentDelivery.Delete();
			base.Delete();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("24124526-2e90-4018-a150-6d303dfcbe79", "Recipient");

		#region Properties

		#region Organisation

		public override ZGuid OrganisationPK
		{
			get => JobDocumentDelivery.JDC_OH;
			set => JobDocumentDelivery.JDC_OH = value;
		}

		public override ZPropertyInfo OrganisationPKInfo => GetWrappedZPropertyInfo(nameof(OrganisationPK), _ => JobDocumentDelivery.JDC_OHInfo);

		public override OrgHeaderCollection Organisations => JobDocumentDelivery.Lookups.Headers;

		#endregion

		#region Contact Name

		[MaxLength(AutoOrgContact.Schema.OC_ContactNameMaxLength)]
		public override ZString ContactName
		{
			get
			{
				if (!contactName.HasValue)
				{
					contactName = JobDocumentDelivery.Contact?.OC_ContactName ?? JobDocumentDelivery.JDC_ContactName;
				}
				return contactName.Value;
			}
			set
			{
				var hasChanges = !contactName.HasValue || contactName.Value != value;
				SetNonPersistentPropertyValue(ContactNameInfo, ref contactName, value);
				if (hasChanges)
				{
					var contact = GetContactByName(value);
					if (contact != null)
					{
						JobDocumentDelivery.JDC_OC_Contact = contact.PK;
						JobDocumentDelivery.JDC_ContactName = ZString.Empty;
					}
					else
					{
						JobDocumentDelivery.JDC_ContactName = value;
						JobDocumentDelivery.JDC_OC_Contact = ZGuid.Empty;
					}
				}
			}
		}
		ZString? contactName;

		public override ZPropertyInfo ContactNameInfo => GetZPropertyInfo(nameof(ContactName));

		OrgContact GetContactByName(ZString name)
		{
			OrgContact contact = null;
			var foundContacts = Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, name));
			if (foundContacts.Length > 0)
			{
				contact = (OrgContact)foundContacts[0];
			}
			return contact;
		}

		#endregion

		#region Delivery Method

		public override ZString DeliveryMethod
		{
			get => JobDocumentDelivery.JDC_DeliveryMethod;
			set => JobDocumentDelivery.JDC_DeliveryMethod = value;
		}

		public override ZPropertyInfo DeliveryMethodInfo => GetWrappedZPropertyInfo(nameof(DeliveryMethod), _ => JobDocumentDelivery.JDC_DeliveryMethodInfo);

		public override CodeDescriptionPairList DeliveryMethods => JobDocumentDelivery.Lookups.JDC_DeliveryMethod_List;

		#endregion

		#region Attachment Type

		public override ZString AttachmentType
		{
			get => JobDocumentDelivery.JDC_AttachmentType;
			set => JobDocumentDelivery.JDC_AttachmentType = value;
		}

		public override ZPropertyInfo AttachmentTypeInfo => GetWrappedZPropertyInfo(nameof(AttachmentType), _ => JobDocumentDelivery.JDC_AttachmentTypeInfo);

		public override CodeDescriptionPairList AttachmentTypes => JobDocumentDelivery.Lookups.JDC_AttachmentType_List;

		#endregion

		#region Fax Number

		public override ZString FaxNumber
		{
			get => JobDocumentDelivery.JDC_FaxNumber;
			set => JobDocumentDelivery.JDC_FaxNumber = value;
		}

		public override ZPropertyInfo FaxNumberInfo => GetWrappedZPropertyInfo(nameof(FaxNumber), _ => JobDocumentDelivery.JDC_FaxNumberInfo);

		#endregion

		#region Document Group

		public override ZString DocumentGroup
		{
			get => JobDocumentDelivery.JDC_DocumentGroup;
			set => JobDocumentDelivery.JDC_DocumentGroup = value;
		}

		public override ZPropertyInfo DocumentGroupInfo => GetWrappedZPropertyInfo(nameof(DocumentGroup), _ => JobDocumentDelivery.JDC_DocumentGroupInfo);

		public override CodeDescriptionPairList DocumentGroups => JobDocumentDelivery.Lookups.JDC_DocumentGroup_List;

		#endregion

		#region Document

		public override ZGuid DocumentPK
		{
			get => JobDocumentDelivery.JDC_SU_MenuItem;
			set => JobDocumentDelivery.JDC_SU_MenuItem = value;
		}

		public override ZPropertyInfo DocumentPKInfo => GetWrappedZPropertyInfo(nameof(DocumentPK), _ => JobDocumentDelivery.JDC_SU_MenuItemInfo);

		#endregion

		#region Email Recipients

		#region Email TO Recipients

		public override ZString EmailToRecipientsAsString
		{
			get => JobDocumentDelivery.JDC_EmailToRecipientsAsString;
			set => JobDocumentDelivery.JDC_EmailToRecipientsAsString = value;
		}

		public override ZPropertyInfo EmailToRecipientsAsStringInfo => GetWrappedZPropertyInfo(nameof(EmailToRecipientsAsString), _ => JobDocumentDelivery.JDC_EmailToRecipientsAsStringInfo);

		public override NonPersistentCopyRecipientCollection EmailToRecipients
		{
			get
			{
				if (emailToRecipients == null)
				{
					emailToRecipients = new NonPersistentCopyRecipientCollection(Organisation, Core.Constants.CopyRecipientType.EmailToRecipient);
					FillEmailToRecipients();
					RegisterEditableChildObject(emailToRecipients);
					JobDocumentDelivery.EmailToRecipients.Updated += EmailToRecipients_OnUpdated;
				}
				return emailToRecipients;
			}
		}
		NonPersistentCopyRecipientCollection emailToRecipients;

		void FillEmailToRecipients()
		{
			foreach (var email in JobDocumentDelivery.EmailToRecipients)
			{
				var recipient = emailToRecipients.AddNew();
				recipient.EmailAddress = email.JDR_EmailAddress;
			}
			EmailToRecipientsAsStringInfo.RefreshBinding();
		}

		void EmailToRecipients_OnUpdated(object sender, EventArgs eventArgs)
		{
			emailToRecipients.RemoveAndDeleteAll();
			FillEmailToRecipients();
		}

		#endregion

		#region Carbon Copy Recipients

		public override ZString CarbonCopyRecipientsAsString
		{
			get => JobDocumentDelivery.JDC_CarbonCopyRecipientsAsString;
			set => JobDocumentDelivery.JDC_CarbonCopyRecipientsAsString = value;
		}

		public override ZPropertyInfo CarbonCopyRecipientsAsStringInfo => GetWrappedZPropertyInfo(nameof(CarbonCopyRecipientsAsString), _ => JobDocumentDelivery.JDC_CarbonCopyRecipientsAsStringInfo);

		public override NonPersistentCopyRecipientCollection CarbonCopyRecipients
		{
			get
			{
				if (carbonCopyRecipients == null)
				{
					carbonCopyRecipients = new NonPersistentCopyRecipientCollection(Organisation, Core.Constants.CopyRecipientType.CarbonCopyRecipient);
					FillCarbonCopyRecipients();
					RegisterEditableChildObject(carbonCopyRecipients);
					JobDocumentDelivery.CarbonCopyRecipients.Updated += CarbonCopyRecipients_OnUpdated;
				}
				return carbonCopyRecipients;
			}
		}
		NonPersistentCopyRecipientCollection carbonCopyRecipients;

		void FillCarbonCopyRecipients()
		{
			foreach (var email in JobDocumentDelivery.CarbonCopyRecipients)
			{
				var recipient = carbonCopyRecipients.AddNew();
				recipient.EmailAddress = email.JDR_EmailAddress;
			}
			CarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		void CarbonCopyRecipients_OnUpdated(object sender, EventArgs eventArgs)
		{
			carbonCopyRecipients.RemoveAndDeleteAll();
			FillCarbonCopyRecipients();
		}

		#endregion

		#region Blind Carbon Copy Recipients

		public override ZString BlindCarbonCopyRecipientsAsString
		{
			get => JobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString;
			set => JobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString = value;
		}

		public override ZPropertyInfo BlindCarbonCopyRecipientsAsStringInfo => GetWrappedZPropertyInfo(nameof(BlindCarbonCopyRecipientsAsString), _ => JobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsStringInfo);

		public override NonPersistentCopyRecipientCollection BlindCarbonCopyRecipients
		{
			get
			{
				if (blindCarbonCopyRecipients == null)
				{
					blindCarbonCopyRecipients = new NonPersistentCopyRecipientCollection(Organisation, Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient);
					FillBlindCarbonCopyRecipients();
					RegisterEditableChildObject(blindCarbonCopyRecipients);
					JobDocumentDelivery.BlindCarbonCopyRecipients.Updated += BlindCarbonCopyRecipients_OnUpdated;
				}
				return blindCarbonCopyRecipients;
			}
		}
		NonPersistentCopyRecipientCollection blindCarbonCopyRecipients;

		void FillBlindCarbonCopyRecipients()
		{
			foreach (var email in JobDocumentDelivery.BlindCarbonCopyRecipients)
			{
				var recipient = blindCarbonCopyRecipients.AddNew();
				recipient.EmailAddress = email.JDR_EmailAddress;
			}
			BlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		void BlindCarbonCopyRecipients_OnUpdated(object sender, EventArgs eventArgs)
		{
			blindCarbonCopyRecipients.RemoveAndDeleteAll();
			FillBlindCarbonCopyRecipients();
		}

		#endregion

		#endregion

		#region Email Subject Macro

		public override ZString EmailSubjectMacro
		{
			get => JobDocumentDelivery.JDC_EmailSubjectMacro;
			set => JobDocumentDelivery.JDC_EmailSubjectMacro = value;
		}

		public override ZPropertyInfo EmailSubjectMacroInfo => GetWrappedZPropertyInfo(nameof(EmailSubjectMacro), _ => JobDocumentDelivery.JDC_EmailSubjectMacroInfo);

		#endregion

		#region Defaulted From Organisation Record

		[ReadOnly(true)]
		public override ZBool DefaultedFromOrganisationRecord => false;

		#endregion

		#region Exclude

		public override ZBool IsExclusion => DeliveryMethod == Core.Constants.ContactNotifyModes.DoNotDeliver;

		#endregion

		#endregion
	}
}
