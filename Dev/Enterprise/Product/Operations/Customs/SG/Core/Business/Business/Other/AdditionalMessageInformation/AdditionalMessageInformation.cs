using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AdditionalMessageInformation : NonPersistentBusinessObject, IAdditionalMessageInformation, IObsoleteValidation
	{
		#region Schema

		public class Schema
		{
			public const string AM_ReasonForAmending = "AM_ReasonForAmending";
			public const string AM_ExtendingTemporaryImportPeriod = "AM_ExtendingTemporaryImportPeriod";
			public const string AM_ReasonForExtendingTemporaryImportPeriod = "AM_ReasonForExtendingTemporaryImportPeriod";
			public const string AM_CancellationCode = "AM_CancellationCode";
			public const string AM_Broker = "AM_Broker";
			public const string AM_BrokerPassword = "AM_BrokerPassword";

			public const int AM_ReasonForAmendingMaxLength = 280;
			public const int AM_CancellationCodeMaxLength = 3;
			public const int AM_BrokerMaxLength = 3;
			public const int AM_BrokerPasswordMaxLength = 32;
			public const int AM_ReasonForExtendingTemporaryImportPeriodMaxLength = 280;
		}

		#endregion

		public enum BoundFormTypes { Declaration, Amendment, Refund, Cancellation }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AdditionalMessageInformation(CUSDECEDIMessage lastMessage, IStorageDocsBaseCollection eDocs, BoundFormTypes boundFormType, BusinessObjectFactory factory, string applicationCode)
			: this(boundFormType, factory, applicationCode)
		{
			originalDocs = eDocs;
			StorageDocs = eDocs;

			if (lastMessage != null)
			{
				foreach (CodeDescriptionPair lastMessagePair in lastMessage.SupportingDocuments)
				{
					ICodeDescription storageDocPair = SupportingDocuments.StorageDocs[lastMessagePair.Description, StringComparison.OrdinalIgnoreCase];
					if (storageDocPair != null)
					{
						SupportingDocument supportingDocument = SupportingDocuments.AddNew();
						supportingDocument.DocumentType = lastMessagePair.Code;
						supportingDocument.eDoc = new ZGuid(storageDocPair.PK);
						supportingDocument.AddRowWarning("Verify that these supporting documents should be resent.");
					}
				}
			}
		}

		readonly IStorageDocsBaseCollection originalDocs;

		public AdditionalMessageInformation(BoundFormTypes boundFormType, BusinessObjectFactory factory, string applicationCode = "SG4")
			: base(factory)
		{
			this.boundFormType = boundFormType;
			ApplicationCode = applicationCode;
			SetBrokerPassword();
		}

		readonly BoundFormTypes boundFormType;
		public readonly string ApplicationCode;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SetBrokerPassword();
		}

		public void SetBrokerPassword()
		{
			var glbExternalPassword = GetGlbExternalPassword(GlbStaff.CurrentUser);

			if (glbExternalPassword != null && !glbExternalPassword.GP_UserID.IsEmpty && !glbExternalPassword.GP_CurrentPassword.IsEmpty)
			{
				AM_Broker = GlbStaff.CurrentUser.GS_Code;
				AM_BrokerPassword = glbExternalPassword.CurrentDecryptedPassword.Left(AM_BrokerPasswordInfo.MaxLength);
			}
		}

		#region Properties

		#region AM_Declaration

		[MaxLength(500)]
		public ZString AM_Declaration
		{
			get
			{
				return "I/We declare that all the particulars in this Application are true and correct."
				+ "\r\n\r\n"
				+ "(For Certificate of Origin only:)"
				+ "\r\n"
				+ "I/We declare that all the product(s) to be exported in this Application has/have been registered with the TTSB of Singapore Customs and qualify(s) for the respective Certificates applied for.";
			}
		}

		public ZPropertyInfo AM_DeclarationInfo
		{
			get { return GetZPropertyInfo(nameof(AM_Declaration)); }
		}

		#endregion

		#region AM_Broker

		[MaxLength(Schema.AM_BrokerMaxLength)]
		public ZString AM_Broker
		{
			get { return fAM_Broker; }
			set
			{
				CheckMaximumLength(AM_BrokerInfo, value);
				SetNonPersistentPropertyValue(AM_BrokerInfo, ref fAM_Broker, value);
				broker = null;
				AM_BrokerPassword = "";
				if (!IsValidationSuspended)
				{
					ValidateAM_Broker();
				}
			}
		}
		ZString fAM_Broker;

		public ZPropertyInfo AM_BrokerInfo
		{
			get { return GetZPropertyInfo(Schema.AM_Broker); }
		}

		void ValidateAM_Broker()
		{
			AM_BrokerInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCodeOrEmpty(AM_BrokerInfo, Lookups.Brokers);

			if (Broker != null)
			{
				var stringBuilder = new StringBuilder();
				var glbExternalPassword = GetGlbExternalPassword(Broker);

				if (Broker.GS_WorkPhone.IsEmpty)
				{
					stringBuilder.Append("The selected broker does not have a Work Telephone Number\r\n");
					stringBuilder.Append("Please edit the broker Staff record to add a Work Telephone Number\r\n");
				}

				if (glbExternalPassword.GP_MailBoxID.IsEmpty)
				{
					stringBuilder.Append("\r\n");
					stringBuilder.Append("The selected broker does not have Declarant Code\r\n");
					stringBuilder.Append("Please edit the broker Staff record (Brokerage Details) to add a Declarant Code\r\n");
				}

				if (stringBuilder.Length > 0)
				{
					AM_BrokerInfo.AddError(stringBuilder.ToString());
				}
			}
		}

		GlbStaff Broker
		{
			get { return broker ?? (broker = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, AM_Broker)); }
		}
		GlbStaff broker;

		#endregion

		#region AM_BrokerPassword

		[MaxLength(Schema.AM_BrokerPasswordMaxLength)]
		[Password]
		public ZString AM_BrokerPassword
		{
			get { return fAM_BrokerPassword; }
			set
			{
				CheckMaximumLength(AM_BrokerPasswordInfo, value);
				SetNonPersistentPropertyValue(AM_BrokerPasswordInfo, ref fAM_BrokerPassword, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_BrokerPassword();
				}
			}
		}
		ZString fAM_BrokerPassword;

		public ZPropertyInfo AM_BrokerPasswordInfo
		{
			get { return GetZPropertyInfo(Schema.AM_BrokerPassword); }
		}

		void ValidateAM_BrokerPassword()
		{
			AM_BrokerPasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AM_BrokerPasswordInfo);

			if (Broker != null)
			{
				var glbExternalPassword = GetGlbExternalPassword(Broker);
				if (glbExternalPassword != null && glbExternalPassword.CurrentDecryptedPassword != AM_BrokerPassword)
				{
					AM_BrokerPasswordInfo.AddError(Res.GetString("437c659d-7ee5-4c87-a128-49e8c128053d", "Please enter the current {0} password", GetGlbExternalPassword()));
				}
			}
		}

		string GetGlbExternalPassword()
		{
			return "TradeNet";
		}

		#endregion

		#region AM_ReasonForAmending

		[MaxLength(Schema.AM_ReasonForAmendingMaxLength)]
		public ZString AM_ReasonForAmending
		{
			get { return fAM_ReasonForAmending; }
			set
			{
				CheckMaximumLength(AM_ReasonForAmendingInfo, value);
				SetNonPersistentPropertyValue(AM_ReasonForAmendingInfo, ref fAM_ReasonForAmending, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_ReasonForAmending();
				}
			}
		}
		ZString fAM_ReasonForAmending;

		public ZPropertyInfo AM_ReasonForAmendingInfo
		{
			get { return GetZPropertyInfo(Schema.AM_ReasonForAmending); }
		}

		void ValidateAM_ReasonForAmending()
		{
			AM_ReasonForAmendingInfo.ClearAllNotifications();
			if (boundFormType == BoundFormTypes.Amendment)
			{
				MandatoryValidation.CheckEntered(AM_ReasonForAmendingInfo);
			}
		}

		#endregion

		#region AM_ExtendingTemporaryImportPeriod

		public ZBool AM_ExtendingTemporaryImportPeriod
		{
			get { return fAM_ExtendingTemporaryImportPeriod; }
			set { SetNonPersistentPropertyValue(AM_ExtendingTemporaryImportPeriodInfo, ref fAM_ExtendingTemporaryImportPeriod, value); }
		}
		ZBool fAM_ExtendingTemporaryImportPeriod;

		public ZPropertyInfo AM_ExtendingTemporaryImportPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.AM_ExtendingTemporaryImportPeriod); }
		}

		#endregion

		#region AM_ReasonForExtendingTemporaryImportPeriod

		[MaxLength(Schema.AM_ReasonForExtendingTemporaryImportPeriodMaxLength)]
		public ZString AM_ReasonForExtendingTemporaryImportPeriod
		{
			get { return fAM_ReasonForExtendingTemporaryImportPeriod; }
			set
			{
				CheckMaximumLength(AM_ReasonForExtendingTemporaryImportPeriodInfo, value);
				SetNonPersistentPropertyValue(AM_ReasonForExtendingTemporaryImportPeriodInfo, ref fAM_ReasonForExtendingTemporaryImportPeriod, value);
			}
		}
		ZString fAM_ReasonForExtendingTemporaryImportPeriod;

		public ZPropertyInfo AM_ReasonForExtendingTemporaryImportPeriodInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.AM_ReasonForExtendingTemporaryImportPeriod);
			}
		}

		public bool AM_ReasonForExtendingTemporaryImportPeriod_ReadOnly
		{
			get { return !AM_ExtendingTemporaryImportPeriod; }
		}

		#endregion

		#region GetGlbExternalPassword

		GlbExternalPassword_SGv4 GetGlbExternalPassword(GlbStaff staff)
		{
			var wrapper = SGGlbStaffWrapper.Get(staff);
			return wrapper.Tradenetv4Password;
		}

		#endregion

		#region Cancellation

		#region AM_CancellationCode

		[MaxLength(Schema.AM_CancellationCodeMaxLength)]
		public ZString AM_CancellationCode
		{
			get { return fAM_CancellationCode; }
			set
			{
				CheckMaximumLength(AM_CancellationCodeInfo, value);
				SetNonPersistentPropertyValue(AM_CancellationCodeInfo, ref fAM_CancellationCode, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_CancellationCode();
				}
			}
		}
		ZString fAM_CancellationCode;

		public ZPropertyInfo AM_CancellationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AM_CancellationCode); }
		}

		void ValidateAM_CancellationCode()
		{
			AM_CancellationCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCodeOrEmpty(AM_CancellationCodeInfo, Lookups.CancellationCodeList);
		}

		#endregion

		#endregion

		#endregion

		#region SupportingDocuments

		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(Factory);
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		#endregion

		#region StorageDocs

		IStorageDocsBaseCollection StorageDocs
		{
			set { SupportingDocuments.SetStorageDocs(value); }
		}

		public ZDecimal GetDocSizeInMB(ZGuid uniqueKey)
		{
			return originalDocs?.Cast<IeDoc>().FirstOrDefault(c => c.UniqueKey == uniqueKey)?.FileSizeInMB ?? 0m;
		}

		#endregion

		#region Lookups

		public AdditionalMessageInformationLookups Lookups
		{
			get { return lookups ?? (lookups = new AdditionalMessageInformationLookups(this)); }
		}
		AdditionalMessageInformationLookups lookups;

		#endregion

		public bool HasBrokerPasswordWithType(string passwordType)
		{
			var result = false;
			var wrapper = SGGlbStaffWrapper.Get(broker);

			if (wrapper != null)
			{
				var glbExternalPassword = wrapper.GetGlbExternalPasswordOrCreateNew<GlbExternalPassword>(passwordType);
				result = glbExternalPassword != null && !glbExternalPassword.GP_UserID.IsEmpty && !glbExternalPassword.GP_CurrentPassword.IsEmpty;
			}

			return result;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (boundFormType == BoundFormTypes.Cancellation)
			{
				ValidateAM_CancellationCode();
			}
			else if (boundFormType == BoundFormTypes.Amendment)
			{
				ValidateAM_ReasonForAmending();
			}

			ValidateAM_Broker();
			ValidateAM_BrokerPassword();
		}

		#region IAdditionalMessageInformation Members

		ZString IAdditionalMessageInformation.ReasonForAmending
		{
			get { return AM_ReasonForAmending; }
		}

		ZString IAdditionalMessageInformation.RefundCode
		{
			get { return RefundCode; }
		}

		protected virtual ZString RefundCode
		{
			get { return ""; }
		}

		ZString IAdditionalMessageInformation.ReasonForRefund
		{
			get { return ReasonForRefund; }
		}

		protected virtual ZString ReasonForRefund
		{
			get { return ""; }
		}

		ZBool IAdditionalMessageInformation.ExtendingTemporaryImportPeriod
		{
			get { return AM_ExtendingTemporaryImportPeriod; }
		}

		ZString IAdditionalMessageInformation.ReasonForExtendingTemporaryImportPeriod
		{
			get { return AM_ReasonForExtendingTemporaryImportPeriod; }
		}

		ZString IAdditionalMessageInformation.CancellationCode
		{
			get { return AM_CancellationCode; }
		}

		ZString IAdditionalMessageInformation.UpdateIndicator
		{
			get { return UpdateIndicator; }
		}

		protected virtual ZString UpdateIndicator
		{
			get { return ""; }
		}

		ZString IAdditionalMessageInformation.Broker
		{
			get { return AM_Broker; }
		}

		IEnumerable<ICusAttachment> IAdditionalMessageInformation.SupportingDocuments
		{
			get
			{
				foreach (ICusAttachment cusAttachment in SupportingDocuments)
				{
					yield return cusAttachment;
				}
			}
		}

		ZDecimal IAdditionalMessageInformation.GSTRefundAmount
		{
			get { return GSTRefundAmount; }
		}

		public virtual ZDecimal GSTRefundAmount
		{
			get { return 0m; }
		}

		ZDecimal IAdditionalMessageInformation.DutyRefundAmount
		{
			get { return DutyRefundAmount; }
		}

		public virtual ZDecimal DutyRefundAmount
		{
			get { return 0m; }
		}

		ZDecimal IAdditionalMessageInformation.ExciseRefundAmount
		{
			get { return ExciseRefundAmount; }
		}

		public virtual ZDecimal ExciseRefundAmount
		{
			get { return 0m; }
		}

		#endregion

		#region MessagePreviewForm;

		public event MessageEventHandler OnMessageCreated;

		public string MessageCreated(string messageText)
		{
			string result = messageText;
			if (OnMessageCreated != null)
			{
				MessageEventArgs args = new MessageEventArgs(messageText);
				OnMessageCreated(args);
				result = args.MessageText;
			}
			return result;
		}

		#endregion
	}
}
