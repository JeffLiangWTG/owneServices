using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDelivery : AutoJobDocumentDelivery
	{
		public JobDocumentDelivery(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			JDC_AttachmentType = OrgConstants.AttachmentType.PDF;
		}

		public DocDeliveryContact GetDocDeliveryDetails(IStmMenuItem menuItem)
		{
			var result = new DocDeliveryContact(Factory);
			result.Initialise(menuItem, null);
			if (JDC_OH.IsValid)
			{
				result.OrgHeaderPK = JDC_OH;
			}

			result.DeliveryMethod = JDC_DeliveryMethod;
			result.AttachmentType = JDC_AttachmentType;
			result.EmailSubjectMacro = JDC_EmailSubjectMacro;
			result.Name = Contact?.OC_ContactName ?? JDC_ContactName;

			if (result.DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				result.PopulateToCcAndBccRecipients(this);
			}
			else if (result.DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
			{
				result.DeliveryAddress = JDC_FaxNumber;
			}

			return result;
		}

		public static ZQuery GetDocumentExclusionQuery(ZGuid jobPk, string jobTableCode, OrgDocument orgDocument, IStmMenuItem menuItem)
		{
			var query = new ZDBOnlyQuery(typeof(JobDocumentDelivery));
			query.AddToFilter(JobDocumentDeliverySchema.JDC_ParentID, jobPk);
			query.AddToFilter(JobDocumentDeliverySchema.JDC_ParentTableCode, jobTableCode);
			query.AddToFilter(JobDocumentDeliverySchema.JDC_DeliveryMethod, Core.Constants.ContactNotifyModes.DoNotDeliver);
			query.AddToFilter(JobDocumentDeliverySchema.JDC_OC_Contact, orgDocument.OD_OC);

			var documentOrGroupQuery = new ZDBOnlyQuery(typeof(JobDocumentDelivery));

			if (!string.IsNullOrEmpty(orgDocument.OD_DocumentGroup))
			{
				documentOrGroupQuery.AddToFilter(JobDocumentDeliverySchema.JDC_DocumentGroup, orgDocument.OD_DocumentGroup);
				documentOrGroupQuery.AddToFilter(new ZQuery(JobDocumentDeliverySchema.JDC_SU_MenuItem, menuItem.PK), JoinCondition.Or);
			}
			else if (orgDocument.OD_SU_MenuItem.IsValid)
			{
				documentOrGroupQuery.AddToFilter(JobDocumentDeliverySchema.JDC_SU_MenuItem, orgDocument.OD_SU_MenuItem);

				var subQuery = new ZDBOnlySubQuery(typeof(StmMenuItem), StmMenuItemSchema.SU_ContactType, JobDocumentDeliverySchema.JDC_DocumentGroup);
				subQuery.AddToFilter(StmMenuItemSchema.PK, orgDocument.OD_SU_MenuItem);
				documentOrGroupQuery.AddSubQuery(subQuery, JoinCondition.Or);
			}

			if (!documentOrGroupQuery.IsEmpty)
			{
				query.AddToFilter(documentOrGroupQuery);
			}

			return query;
		}

		public override ZGuid JDC_OC_Contact
		{
			get { return base.JDC_OC_Contact; }
			set
			{
				base.JDC_OC_Contact = value;
				if (!JDC_OC_Contact.IsEmpty)
				{
					if (JDC_DeliveryMethod == Core.Constants.ContactNotifyModes.Email && Contact != null)
					{
						JDC_EmailToRecipientsAsString = Contact.OC_Email;
					}
				}

				Validation.ValidateJDC_DeliveryMethod();
			}
		}

		#region JDC_DocumentGroup

		[List("Lookups.JDC_DocumentGroup_List")]
		public override ZString JDC_DocumentGroup
		{
			get { return base.JDC_DocumentGroup; }
			set
			{
				base.JDC_DocumentGroup = value;
				Validation.ValidateJDC_SU_MenuItem();
			}
		}

		#endregion

		#region JDC_SU_MenuItem

		public override ZGuid JDC_SU_MenuItem
		{
			get { return base.JDC_SU_MenuItem; }
			set
			{
				base.JDC_SU_MenuItem = value;
				Validation.ValidateJDC_DocumentGroup();
			}
		}

		#endregion

		#region JDC_DeliveryMethod

		[List("Lookups.JDC_DeliveryMethod_List")]
		public override ZString JDC_DeliveryMethod
		{
			get { return base.JDC_DeliveryMethod; }
			set
			{
				base.JDC_DeliveryMethod = value;

				if (!DeliveryMethodHelper.IsEmail(JDC_DeliveryMethod))
				{
					previousEmailSubjectMacro = JDC_EmailSubjectMacro;
					JDC_EmailSubjectMacro = string.Empty;
				}
				else if (JDC_EmailSubjectMacro.IsEmpty && !previousEmailSubjectMacro.IsEmpty)
				{
					JDC_EmailSubjectMacro = previousEmailSubjectMacro;
				}

				if (DeliveryMethodHelper.IsEmailOrEPrint(JDC_DeliveryMethod))
				{
					JDC_AttachmentType = Contact != null ? Contact.OC_AttachmentType.ToString() : OrgConstants.AttachmentType.PDF;
				}
				else
				{
					JDC_AttachmentType = ZString.Empty;
				}

				Validation.ValidateJDC_FaxNumber();
				Validation.ValidateJDC_EmailToRecipientsAsString();
				Validation.ValidateJDC_CarbonCopyRecipientsAsString();
				Validation.ValidateJDC_BlindCarbonCopyRecipientsAsString();
			}
		}

		#endregion

		#region JDC_AttachmentType

		[List("Lookups.JDC_AttachmentType_List")]
		public override ZString JDC_AttachmentType
		{
			get { return base.JDC_AttachmentType; }
			set { base.JDC_AttachmentType = value; }
		}

		protected bool JDC_AttachmentType_ReadOnly => !DeliveryMethodHelper.IsEmailOrEPrint(JDC_DeliveryMethod);

		#endregion

		#region JDC_FaxNumber

		protected bool JDC_FaxNumber_ReadOnly => JDC_DeliveryMethod != Core.Constants.ContactNotifyModes.Fax;

		#endregion

		#region JDC_EmailSubjectMacro

		protected bool JDC_EmailSubjectMacro_ReadOnly => !DeliveryMethodHelper.IsEmail(JDC_DeliveryMethod);

		ZString previousEmailSubjectMacro;

		#endregion

		#region CopyRecipients

		#region EmailToRecipients

		[ChildEditable(true)]
		public JobDocumentDeliveryCopyRecipientCollection EmailToRecipients
		{
			get
			{
				if (emailToRecipients == null)
				{
					emailToRecipients = new JobDocumentDeliveryCopyRecipientCollection(this, Core.Constants.CopyRecipientType.EmailToRecipient);
					RegisterEditableChildObject(emailToRecipients);
					emailToRecipients.Updated += OnEmailToRecipientsUpdated;
				}
				return emailToRecipients;
			}
		}
		JobDocumentDeliveryCopyRecipientCollection emailToRecipients;

		[BusinessObjectTestExclude]
		[List(nameof(CopyRecipientList))]
		[ReadOnlyMember(nameof(IsRecipientsAsStringReadOnly))]
		public ZString JDC_EmailToRecipientsAsString
		{
			get
			{
				var result = string.Empty;

				if (JDC_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					if (emailToRecipientsAsString == null)
					{
						emailToRecipientsAsString = EmailToRecipients.Value;
					}
					result = emailToRecipientsAsString;
				}

				return result;
			}
			set
			{
				if (emailToRecipientsAsString != value)
				{
					isUpdatingEmailToRecipientsAsString = true;
					try
					{
						CheckMaximumLength(JDC_EmailToRecipientsAsStringInfo, value);
						emailToRecipientsAsString = value;
						EmailToRecipients.Value = value;
						Validation.ValidateJDC_EmailToRecipientsAsString();
						if (!JDC_EmailToRecipientsAsStringInfo.HasErrors())
						{
							emailToRecipientsAsString = EmailToRecipients.Value;
						}
						JDC_EmailToRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingEmailToRecipientsAsString = false;
					}
				}
			}
		}
		string emailToRecipientsAsString;

		public ZPropertyInfo JDC_EmailToRecipientsAsStringInfo => GetZPropertyInfo(nameof(JDC_EmailToRecipientsAsString));

		void OnEmailToRecipientsUpdated(object sender, EventArgs e)
		{
			if (!isUpdatingEmailToRecipientsAsString)
			{
				emailToRecipientsAsString = null;
			}
			JDC_EmailToRecipientsAsStringInfo.RefreshBinding();
		}

		bool isUpdatingEmailToRecipientsAsString;

		#endregion

		#region CarbonCopyRecipients

		[ChildEditable(true)]
		public JobDocumentDeliveryCopyRecipientCollection CarbonCopyRecipients
		{
			get
			{
				if (carbonCopyRecipients == null)
				{
					carbonCopyRecipients = new JobDocumentDeliveryCopyRecipientCollection(this, Core.Constants.CopyRecipientType.CarbonCopyRecipient);
					RegisterEditableChildObject(carbonCopyRecipients);
					carbonCopyRecipients.Updated += OnCarbonCopyRecipientsUpdated;
				}
				return carbonCopyRecipients;
			}
		}
		JobDocumentDeliveryCopyRecipientCollection carbonCopyRecipients;

		[BusinessObjectTestExclude]
		[List(nameof(CopyRecipientList))]
		[ReadOnlyMember(nameof(IsRecipientsAsStringReadOnly))]
		public ZString JDC_CarbonCopyRecipientsAsString
		{
			get
			{
				var result = string.Empty;

				if (JDC_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					if (carbonCopyRecipientsAsString == null)
					{
						carbonCopyRecipientsAsString = CarbonCopyRecipients.Value;
					}
					result = carbonCopyRecipientsAsString;
				}

				return result;
			}
			set
			{
				if (carbonCopyRecipientsAsString != value)
				{
					isUpdatingCarbonCopyRecipientsAsString = true;
					try
					{
						CheckMaximumLength(JDC_CarbonCopyRecipientsAsStringInfo, value);
						carbonCopyRecipientsAsString = value;
						CarbonCopyRecipients.Value = value;
						Validation.ValidateJDC_CarbonCopyRecipientsAsString();
						if (!JDC_CarbonCopyRecipientsAsStringInfo.HasErrors())
						{
							carbonCopyRecipientsAsString = CarbonCopyRecipients.Value;
						}
						JDC_CarbonCopyRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingCarbonCopyRecipientsAsString = false;
					}
				}
			}
		}
		string carbonCopyRecipientsAsString;

		public ZPropertyInfo JDC_CarbonCopyRecipientsAsStringInfo => GetZPropertyInfo(nameof(JDC_CarbonCopyRecipientsAsString));

		void OnCarbonCopyRecipientsUpdated(object sender, EventArgs e)
		{
			if (!isUpdatingCarbonCopyRecipientsAsString)
			{
				carbonCopyRecipientsAsString = null;
			}
			JDC_CarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		bool isUpdatingCarbonCopyRecipientsAsString;

		#endregion

		#region BlindCarbonCopyRecipients

		[ChildEditable(true)]
		public JobDocumentDeliveryCopyRecipientCollection BlindCarbonCopyRecipients
		{
			get
			{
				if (blindCarbonCopyRecipients == null)
				{
					blindCarbonCopyRecipients = new JobDocumentDeliveryCopyRecipientCollection(this, Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient);
					RegisterEditableChildObject(blindCarbonCopyRecipients);
					blindCarbonCopyRecipients.Updated += OnBlindCarbonCopyRecipientsUpdated;
				}
				return blindCarbonCopyRecipients;
			}
		}
		JobDocumentDeliveryCopyRecipientCollection blindCarbonCopyRecipients;

		[BusinessObjectTestExclude]
		[List(nameof(CopyRecipientList))]
		[ReadOnlyMember(nameof(IsRecipientsAsStringReadOnly))]
		public ZString JDC_BlindCarbonCopyRecipientsAsString
		{
			get
			{
				var result = string.Empty;

				if (JDC_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					if (blindCarbonCopyRecipientsAsString == null)
					{
						blindCarbonCopyRecipientsAsString = BlindCarbonCopyRecipients.Value;
					}
					result = blindCarbonCopyRecipientsAsString;
				}

				return result;
			}
			set
			{
				if (blindCarbonCopyRecipientsAsString != value)
				{
					isUpdatingBlindCarbonCopyRecipientsAsString = true;
					try
					{
						CheckMaximumLength(JDC_BlindCarbonCopyRecipientsAsStringInfo, value);
						blindCarbonCopyRecipientsAsString = value;
						BlindCarbonCopyRecipients.Value = value;
						Validation.ValidateJDC_BlindCarbonCopyRecipientsAsString();
						if (!JDC_BlindCarbonCopyRecipientsAsStringInfo.HasErrors())
						{
							blindCarbonCopyRecipientsAsString = BlindCarbonCopyRecipients.Value;
						}
						JDC_BlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingBlindCarbonCopyRecipientsAsString = false;
					}
				}
			}
		}
		string blindCarbonCopyRecipientsAsString;

		public ZPropertyInfo JDC_BlindCarbonCopyRecipientsAsStringInfo => GetZPropertyInfo(nameof(JDC_BlindCarbonCopyRecipientsAsString));

		void OnBlindCarbonCopyRecipientsUpdated(object sender, EventArgs e)
		{
			if (!isUpdatingBlindCarbonCopyRecipientsAsString)
			{
				blindCarbonCopyRecipientsAsString = null;
			}
			JDC_BlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		bool isUpdatingBlindCarbonCopyRecipientsAsString;

		#endregion

		bool IsRecipientsAsStringReadOnly => JDC_DeliveryMethod != Core.Constants.ContactNotifyModes.Email;

		public List<string> CopyRecipientList => new List<string>();

		#endregion

		#region Save

		public override void OnSaving()
		{
			if (JDC_EmailToRecipientsAsStringInfo.ReadOnly)
			{
				EmailToRecipients.DeleteAll();
			}
			if (JDC_CarbonCopyRecipientsAsStringInfo.ReadOnly)
			{
				CarbonCopyRecipients.DeleteAll();
			}
			if (JDC_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly)
			{
				BlindCarbonCopyRecipients.DeleteAll();
			}
			base.OnSaving();
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			EmailToRecipients.Updated -= OnEmailToRecipientsUpdated;
			EmailToRecipients.DeleteAll();
			CarbonCopyRecipients.Updated -= OnCarbonCopyRecipientsUpdated;
			CarbonCopyRecipients.DeleteAll();
			BlindCarbonCopyRecipients.Updated -= OnBlindCarbonCopyRecipientsUpdated;
			BlindCarbonCopyRecipients.DeleteAll();

			base.Delete();
		}

		#endregion
	}
}
