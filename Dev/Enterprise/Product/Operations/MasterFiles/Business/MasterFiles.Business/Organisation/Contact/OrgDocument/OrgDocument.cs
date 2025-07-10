using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgDocumentLookups;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgDocument : AutoOrgDocument
	{
		public OrgDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OD_AttachmentType = OrgConstants.AttachmentType.PDF;
			OD_FilterShipmentMode = Constants.TransportModes.All;
			OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			CarbonCopyRecipients.Updated -= CarbonCopyRecipients_Updated;
			CarbonCopyRecipients.DeleteAll();
			BlindCarbonCopyRecipients.Updated -= BlindCarbonCopyRecipientsOnUpdated;
			BlindCarbonCopyRecipients.DeleteAll();
			DeleteJobDocumentExclusion();
			base.Delete();
		}

		void DeleteJobDocumentExclusion()
		{
			Factory.Load<JobDocumentExclusion>(new ZQuery(JobDocumentExclusionSchema.JDE_OD_Document, PK)).ForEach(b => b.Delete());
		}

		#endregion

		#region Properties

		#region OD_DocumentGroup
		[List("Lookups.OD_DocumentGroup_List")]
		public override ZString OD_DocumentGroup
		{
			get { return base.OD_DocumentGroup; }
			set
			{
				bool isDifferent = base.OD_DocumentGroup != value;
				base.OD_DocumentGroup = value;
				if (Contact != null && isDifferent)
				{
					OD_DefaultContact = Contact.SetDocGroupAsDefault(OD_DocumentGroup);
					Contact.IsCustomerServiceContactInfo.RefreshBinding();
				}
				if (RelatedPartyNotAvailable)
				{
					OD_OH_RelatedFilterByParty = ZGuid.Empty;
				}
				if (FilterPortNotAvailable)
				{
					OD_FilterLocalPort = ZString.Empty;
					OD_FilterForeignPort = ZString.Empty;
				}
				RunDocGroupValidationForOrg();
				Validation.ValidateOD_SU_MenuItem();
			}
		}

		#endregion

		#region OD_DefaultContact

		public override ZBool OD_DefaultContact
		{
			get { return base.OD_DefaultContact; }
			set
			{
				base.OD_DefaultContact = value;
				RunDefaultContactValidationForOrg();
			}
		}

		void RunDefaultContactValidationForOrg()
		{
			if (Contact != null && Contact.Header != null)
			{
				foreach (OrgContact otherContact in Contact.Header.Contacts)
				{
					foreach (OrgDocument otherContactDocument in otherContact.Documents)
					{
						otherContactDocument.Validation.ValidateOD_DefaultContact();
					}
				}
			}
		}

		void RunDocGroupValidationForOrg()
		{
			if (Contact != null && Contact.Header != null)
			{
				foreach (OrgContact otherContact in Contact.Header.Contacts)
				{
					foreach (OrgDocument otherContactDocument in otherContact.Documents)
					{
						otherContactDocument.Validation.ValidateOD_DocumentGroup();
					}
				}
			}
		}

		#endregion

		#region OD_SU_MenuItem
		[List("Lookups.Documents")]
		public override ZGuid OD_SU_MenuItem
		{
			get { return base.OD_SU_MenuItem; }
			set
			{
				base.OD_SU_MenuItem = value;
				Validation.ValidateOD_DocumentGroup();

				if (MustSendViaEmailPDF)
				{
					OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
					OD_AttachmentType = OrgConstants.AttachmentType.PDF;
				}
				if (FilterPortNotAvailable)
				{
					OD_FilterLocalPort = ZString.Empty;
					OD_FilterForeignPort = ZString.Empty;
				}
			}
		}

		#endregion

		#region OD_DeliverBy

		[ReadOnlyMember(nameof(MustSendViaEmailPDF))]
		[List("Lookups.OD_DeliverBy_List")]
		public override ZString OD_DeliverBy
		{
			get { return base.OD_DeliverBy; }
			set
			{
				var hasChanges = base.OD_DeliverBy != value;
				base.OD_DeliverBy = value;

				if (hasChanges)
				{
					OD_SendIndividually = false;
				}

				if (!DeliveryMethodHelper.IsEmail(OD_DeliverBy))
				{
					previousEmailSubjectMacro = OD_EmailSubjectMacro;
					OD_EmailSubjectMacro = string.Empty;
				}
				else if (OD_EmailSubjectMacro.IsEmpty && !previousEmailSubjectMacro.IsEmpty)
				{
					OD_EmailSubjectMacro = previousEmailSubjectMacro;
				}

				if (DeliveryMethodHelper.IsEmailOrEPrint(OD_DeliverBy))
				{
					OD_AttachmentType = Contact != null ? Contact.OC_AttachmentType.ToString() : OrgConstants.AttachmentType.PDF;
				}
				else
				{
					OD_AttachmentType = ZString.Empty;
				}

				if (Contact != null)
				{
					Contact.Validation.ValidateOC_Email();
					Contact.Validation.ValidateOC_Fax();
				}
			}
		}

		#endregion

		#region OD_AttachmentType
		[List("Lookups.OD_AttachmentType_List")]
		public override ZString OD_AttachmentType
		{
			get { return base.OD_AttachmentType; }
			set { base.OD_AttachmentType = value; }
		}

		protected bool OD_AttachmentType_ReadOnly
		{
			get { return MustSendViaEmailPDF || !DeliveryMethodHelper.IsEmailOrEPrint(OD_DeliverBy); }
		}

		#endregion

		#region OD_SendIndividually

		public override ZBool OD_SendIndividually
		{
			get { return OD_DeliverBy == Constants.ContactNotifyModes.Print ? true : base.OD_SendIndividually; }
			set { base.OD_SendIndividually = value; }
		}

		internal bool OD_SendIndividually_ReadOnly => OD_DeliverBy == Constants.ContactNotifyModes.Print;

		#endregion

		#region OD_EmailSubjectMacro

		[BusinessObjectTestExclude]
		public override ZString OD_EmailSubjectMacro
		{
			get => base.OD_EmailSubjectMacro;
			set => base.OD_EmailSubjectMacro = value;
		}

		protected internal bool OD_EmailSubjectMacro_ReadOnly => !DeliveryMethodHelper.IsEmail(OD_DeliverBy);
		ZString previousEmailSubjectMacro;

		#endregion

		#region OD_FilterDirection
		[List("Lookups.FilterDirections")]
		public override ZString OD_FilterDirection
		{
			get { return base.OD_FilterDirection; }
			set { base.OD_FilterDirection = value; }
		}
		#endregion

		#region OD_FilterForeignPort

		[List("Lookups.Locations")]
		public override ZString OD_FilterForeignPort
		{
			get { return base.OD_FilterForeignPort; }
			set { base.OD_FilterForeignPort = value; }
		}

		protected bool OD_FilterForeignPort_ReadOnly
		{
			get { return FilterPortNotAvailable; }
		}

		#endregion

		#region OD_FilterLocalPort
		[List("Lookups.Locations")]
		public override ZString OD_FilterLocalPort
		{
			get { return base.OD_FilterLocalPort; }
			set { base.OD_FilterLocalPort = value; }
		}

		protected bool OD_FilterLocalPort_ReadOnly
		{
			get { return FilterPortNotAvailable; }
		}

		bool FilterPortNotAvailable
		{
			get
			{
				var result = false;
				var isExpOrImp = OD_FilterDirection != FilterDirectionConstants.Codes.Export && OD_FilterDirection != FilterDirectionConstants.Codes.Import;
				if (!OD_DocumentGroup.IsEmpty)
				{
					if (OD_DocumentGroup == ContactType.Receivables.Code || OD_DocumentGroup == ContactType.Payables.Code)
					{
						result = isExpOrImp;
					}
				}
				else
				{
					var menuItem = Factory.Load<StmMenuItem>(OD_SU_MenuItem);
					if (menuItem != null)
					{
						if (menuItem.SU_ContactType == ContactType.Receivables.Code || menuItem.SU_ContactType == ContactType.Payables.Code)
						{
							result = isExpOrImp;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region OD_FilterShipmentMode

		[List("Lookups.OD_FilterShipmentMode_List")]
		public override ZString OD_FilterShipmentMode
		{
			get { return base.OD_FilterShipmentMode; }
			set { base.OD_FilterShipmentMode = value; }
		}

		#endregion

		#region OD_OH_RelatedFilterByParty

		[List("Lookups.RelatedOrganisations")]
		public override ZGuid OD_OH_RelatedFilterByParty
		{
			get { return base.OD_OH_RelatedFilterByParty; }
			set { base.OD_OH_RelatedFilterByParty = value; }
		}

		protected bool OD_OH_RelatedFilterByParty_ReadOnly
		{
			get { return RelatedPartyNotAvailable; }
		}

		bool RelatedPartyNotAvailable
		{
			get { return OD_DocumentGroup == ContactType.Receivables || OD_DocumentGroup == ContactType.Payables; }
		}

		#endregion

		#region OD_GB_FilterBranch

		protected bool OD_GB_FilterBranch_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region OD_GC_FilterCompany

		protected bool OD_GC_FilterCompany_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region OD_GE_FilterDepartment

		protected bool OD_GE_FilterDepartment_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Must Send via Email / PDF

		bool MustSendViaEmailPDF
		{
			get { return MenuItem != null && MenuItem.SU_MenuName.ToUpper().StartsWith((NoResString)"SEND ELECTRONIC ORIGINAL BILL OF LADING"); }
		}

		#endregion

		#region SuppressDocument

		public ZBool SuppressDocument
		{
			get { return OD_DeliverBy == Constants.ContactNotifyModes.DoNotDeliver; }
		}

		public bool IsSuppressedForSpecificJob(ZGuid jobPk, string jobTableCode, IStmMenuItem menuItem)
		{
			return Factory.ExistsInDatabase(AutoJobDocumentExclusion.Schema.TableName, JobDocumentExclusion.GetJobDocumentExclusionQuery(PK, jobPk, jobTableCode))
				|| Factory.ExistsInDatabase(AutoJobDocumentDelivery.Schema.TableName, JobDocumentDelivery.GetDocumentExclusionQuery(jobPk, jobTableCode, this, menuItem));
		}

		#endregion

		#region CopyRecipients

		#region CarbonCopyRecipients

		[ChildEditable(true)]
		public OrgDocumentCopyRecipientCollection CarbonCopyRecipients
		{
			get
			{
				if (carbonCopyRecipients == null)
				{
					carbonCopyRecipients = new OrgDocumentCopyRecipientCollection(this, Constants.CopyRecipientType.CarbonCopyRecipient);
					RegisterEditableChildObject(carbonCopyRecipients);
					carbonCopyRecipients.Updated += CarbonCopyRecipients_Updated;
				}
				return carbonCopyRecipients;
			}
		}

		[BusinessObjectTestExclude]
		[List("CopyRecipientList")]
		public ZString OD_CarbonCopyRecipientsAsString
		{
			get
			{
				var result = string.Empty;

				if (OD_DeliverBy == Constants.ContactNotifyModes.Email)
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
						CheckMaximumLength(OD_CarbonCopyRecipientsAsStringInfo, value);
						carbonCopyRecipientsAsString = value;
						CarbonCopyRecipients.Value = value;
						Validation.ValidateOD_CarbonCopyRecipientsAsString();
						if (!OD_CarbonCopyRecipientsAsStringInfo.HasErrors())
						{
							carbonCopyRecipientsAsString = CarbonCopyRecipients.Value;
						}
						OD_CarbonCopyRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingCarbonCopyRecipientsAsString = false;
					}
				}
			}
		}

		public ZPropertyInfo OD_CarbonCopyRecipientsAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(OD_CarbonCopyRecipientsAsString)); }
		}

		protected bool OD_CarbonCopyRecipientsAsString_ReadOnly
		{
			get { return OD_DeliverBy != Constants.ContactNotifyModes.Email; }
		}

		void CarbonCopyRecipients_Updated(object sender, EventArgs e)
		{
			if (!isUpdatingCarbonCopyRecipientsAsString)
			{
				carbonCopyRecipientsAsString = null;
			}
			OD_CarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		OrgDocumentCopyRecipientCollection carbonCopyRecipients;
		string carbonCopyRecipientsAsString;
		bool isUpdatingCarbonCopyRecipientsAsString;

		#endregion

		#region BlindCarbonCopyRecipients

		[ChildEditable(true)]
		public OrgDocumentCopyRecipientCollection BlindCarbonCopyRecipients
		{
			get
			{
				if (blindCarbonCopyRecipients == null)
				{
					blindCarbonCopyRecipients = new OrgDocumentCopyRecipientCollection(this, Constants.CopyRecipientType.BlindCarbonCopyRecipient);
					RegisterEditableChildObject(blindCarbonCopyRecipients);
					blindCarbonCopyRecipients.Updated += BlindCarbonCopyRecipientsOnUpdated;
				}
				return blindCarbonCopyRecipients;
			}
		}

		[BusinessObjectTestExclude]
		[List("CopyRecipientList")]
		public ZString OD_BlindCarbonCopyRecipientsAsString
		{
			get
			{
				string result = string.Empty;

				if (OD_DeliverBy == Constants.ContactNotifyModes.Email)
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
						CheckMaximumLength(OD_BlindCarbonCopyRecipientsAsStringInfo, value);
						blindCarbonCopyRecipientsAsString = value;
						BlindCarbonCopyRecipients.Value = value;
						Validation.ValidateOD_BlindCarbonCopyRecipientsAsString();
						if (!OD_BlindCarbonCopyRecipientsAsStringInfo.HasErrors())
						{
							blindCarbonCopyRecipientsAsString = BlindCarbonCopyRecipients.Value;
						}
						OD_BlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
					}
					finally
					{
						isUpdatingBlindCarbonCopyRecipientsAsString = false;
					}
				}
			}
		}

		public ZPropertyInfo OD_BlindCarbonCopyRecipientsAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(OD_BlindCarbonCopyRecipientsAsString)); }
		}

		protected bool OD_BlindCarbonCopyRecipientsAsString_ReadOnly
		{
			get { return OD_DeliverBy != Constants.ContactNotifyModes.Email; }
		}

		void BlindCarbonCopyRecipientsOnUpdated(object sender, EventArgs eventArgs)
		{
			if (!isUpdatingBlindCarbonCopyRecipientsAsString)
			{
				blindCarbonCopyRecipientsAsString = null;
			}
			OD_BlindCarbonCopyRecipientsAsStringInfo.RefreshBinding();
		}

		OrgDocumentCopyRecipientCollection blindCarbonCopyRecipients;
		string blindCarbonCopyRecipientsAsString;
		bool isUpdatingBlindCarbonCopyRecipientsAsString;

		#endregion

		public List<string> CopyRecipientList
		{
			get { return new List<string>(); }
		}

		#endregion

		#endregion

		#region Save

		public override void OnSaving()
		{
			if (OD_CarbonCopyRecipientsAsString_ReadOnly)
			{
				CarbonCopyRecipients.DeleteAll();
			}
			if (OD_BlindCarbonCopyRecipientsAsString_ReadOnly)
			{
				BlindCarbonCopyRecipients.DeleteAll();
			}
			base.OnSaving();
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Logging

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Custom log reference")]
		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString logReference = (NoResString)"Document";

				if (Contact != null)
				{
					logReference += " for: " + Contact.OC_ContactName;
				}

				if (MenuItem != null)
				{
					logReference += " Document: " + MenuItem.SU_MenuName;
				}

				if (!OD_DocumentGroup.IsEmpty)
				{
					logReference += " Group: " + OD_DocumentGroup + (OD_DocumentGroupInfo.HasChanges ? " (" + OD_DocumentGroupInfo.OriginalValue + ")" : "");
				}

				return logReference;
			}
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result =
					Contact != null &&
					Contact.Header != null &&
					!Contact.Header.SecurityProvider.HasModifyContactDocDeliveryDetailsSecurity;
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

	}
}
