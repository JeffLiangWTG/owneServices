using System;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.ServiceManager;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEmailAddress : AutoGlbStaffEmailAddress
	{
		public GlbStaffEmailAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.SelectableEmailTypeList")]
		public ZString EmailType
		{
			get
			{
				ZString result = default;

				if (GSE_TypeInfo.HasChanges || GSE_GC_Company == GlbCompany.CurrentCompany.PK)
				{
					result = Lookups.AllEmailTypeList.GetDescriptionFromCode(GSE_Type) ?? GSE_Type;
				}
				else
				{
					if (EmailTypeFromOtherCompany.HasValue)
					{
						result = EmailTypeFromOtherCompany.Value;
					}
				}

				return result;
			}

			set
			{
				if (EmailTypeFromOtherCompany.HasValue && EmailTypeFromOtherCompany.Value == value)
				{
					GSE_GC_Company = (ZGuid)GSE_GC_CompanyInfo.OriginalValue;
					GSE_Type = (ZString)GSE_TypeInfo.OriginalValue;
				}
				else
				{
					GSE_GC_Company = GlbCompany.CurrentCompany.PK;
					var code = Lookups.SelectableEmailTypeList.GetCodeFromDescription(value);
					GSE_Type = !string.IsNullOrEmpty(code) ? (ZString)code : value;
				}
			}
		}

		public override ZString GSE_EmailAddress
		{
			get => base.GSE_EmailAddress;

			set
			{
				if (base.GSE_EmailAddress != value)
				{
					base.GSE_EmailAddress = value;

					if (GSE_Type == Core.Constants.EmailFromAddressTypes.Codes.Main)
					{
						Staff.GS_EmailAddress = value;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsNDR();
				}

				IsNDRInfo.RefreshBinding();
			}
		}

		protected bool GSE_EmailAddress_ReadOnly
		{
			get
			{
				return GSE_Type == Core.Constants.EmailFromAddressTypes.Codes.Main && Staff != null && Staff.IsControlledByScim;
			}
		}

		public ZString? EmailTypeFromOtherCompany
		{
			get
			{
				if (EmailTypeFromOtherCompanyList == null)
				{
					return null;
				}

				var originalType = (ZString)GSE_TypeInfo.OriginalValue;
				var result = EmailTypeFromOtherCompanyList.GetDescriptionFromCode(originalType);
				result = result ?? originalType;
				result += " (" + Company.GC_Code + ")";

				return result;
			}
		}

		public CodeDescriptionPairList EmailTypeFromOtherCompanyList
		{
			get
			{
				var originalCompanyPK = (ZGuid)GSE_GC_CompanyInfo.OriginalValue;

				if (!originalCompanyPK.IsValid || originalCompanyPK == GlbCompany.CurrentCompany.PK)
				{
					return null;
				}

				return new CodeDescriptionPairList(SystemDataRegistry.Instance.StaffEmailTypeList.GetFallBackValueAtAllLevels(originalCompanyPK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		public ZWrappedPropertyInfo EmailTypeInfo => GetWrappedZPropertyInfo(nameof(EmailType), x => GSE_TypeInfo);

		internal bool EmailType_ReadOnly => GSE_Type == Core.Constants.EmailFromAddressTypes.Codes.Main;

		public override bool IsSavedByFactory => base.IsSavedByFactory && (IsDeleted || GSE_Type != Core.Constants.EmailFromAddressTypes.Codes.Main);

		public override bool CanDelete => GSE_Type != Core.Constants.EmailFromAddressTypes.Codes.Main && !IsUsedByScheduleTaskRecipients;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("B41AD2F1-71C5-4631-851F-28A6A7378902", "Main email address cannot be deleted or this email address has been used in scheduled task email from address.");

		public override void OnSaving()
		{
			base.OnSaving();

			if (GSE_EmailAddressInfo.HasChanges && !GSE_EmailAddressInfo.OriginalValue.IsEmpty)
			{
				UpdateScheduleTaskRecipientsIfUsedByEmailFromAddress(ScheduleTaskRecipientsIfUsedByEmailFromAddress, (ZString)GSE_EmailAddressInfo.Value);
			}
		}

		static void UpdateScheduleTaskRecipientsIfUsedByEmailFromAddress(IStmScheduleTaskRecipient[] scheduleTaskRecipientsIfUsedByEmailFromAddress, ZString newEmailAddress)
		{
			foreach (var taskRecipient in scheduleTaskRecipientsIfUsedByEmailFromAddress)
			{
				taskRecipient.S6_EmailFromAddress = newEmailAddress;
			}
		}

		IStmScheduleTaskRecipient[] ScheduleTaskRecipientsIfUsedByEmailFromAddress
		{
			get
			{
				if (Staff != null)
				{
					return GetScheduleTaskRecipientsIfUsedByEmailFromAddress((ZString)GSE_EmailAddressInfo.OriginalValue, Staff.GS_Code, Factory);
				}
				return Array.Empty<IStmScheduleTaskRecipient>();
			}
		}

		static IStmScheduleTaskRecipient[] GetScheduleTaskRecipientsIfUsedByEmailFromAddress(IZType originalEmailAddress, ZString staffCode, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(IStmScheduleTaskRecipient));
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_DeliveryMethod, Core.Constants.ContactNotifyModes.Email);
			query.AddToFilter(StmScheduleTaskRecipientSchema.S6_EmailFromAddress, originalEmailAddress);

			var subQuery = new ZDBOnlySubQuery(typeof(IStmScheduleTask), StmScheduleTaskRecipientSchema.S6_S5);
			subQuery.AddToFilter(StmScheduleTaskSchema.S5_GS_NKPrintUser, staffCode);
			subQuery.AddToFilter(StmScheduleTaskSchema.S5_ScheduleType, "REP");
			query.AddSubQuery(subQuery, JoinCondition.And);

			var scheduleTasks = factory.Load(ObjectFactory.GetType<IStmScheduleTaskRecipient>(), query);
			return (IStmScheduleTaskRecipient[])scheduleTasks;
		}

		public static void UpdateScheduleTaskRecipientsIfUsedByEmailFromAddress(IZType originalEmailAddress, ZString newEmailAddress, ZString staffCode, BusinessObjectFactory factory)
		{
			var scheduleTaskRecipientsIfUsedByEmailFromAddress = GetScheduleTaskRecipientsIfUsedByEmailFromAddress(originalEmailAddress, staffCode, factory);
			UpdateScheduleTaskRecipientsIfUsedByEmailFromAddress(scheduleTaskRecipientsIfUsedByEmailFromAddress, newEmailAddress);
		}

		public bool IsUsedByScheduleTaskRecipients
		{
			get
			{
				if (Staff != null)
				{
					if (GSE_Type == Core.Constants.EmailFromAddressTypes.Codes.Main)
					{
						return GetScheduleTaskRecipientsIfUsedByEmailFromAddress(Staff.GS_EmailAddressInfo.OriginalValue, Staff.GS_Code, Factory).Length > 0;
					}
					else
					{
						return GetScheduleTaskRecipientsIfUsedByEmailFromAddress(GSE_EmailAddressInfo.OriginalValue, Staff.GS_Code, Factory).Length > 0;
					}
				}
				return false;
			}
		}

		#region Non-Delivery Report

		internal LazyGlbEmailAddress EmailAddress
		{
			get { return emailAddress ?? (emailAddress = new LazyGlbEmailAddress(Factory, () => GSE_EmailAddress)); }
		}
		LazyGlbEmailAddress emailAddress;

		[ResourceStringData("GlbStaffEmailAddress|IsNDR", Caption = "Non-Delivery Receipt", ShortCaption = "NDR")]
		public ZBool IsNDR
		{
			get => EmailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			set
			{
				if (IsNDR == value)
				{
					return;
				}

				EmailAddress.GI_DeliveryStatus = value ? (ZString)EmailDeliveryReportStatus.Codes.NonDeliveryReport : ZString.Empty;
				IsNDRInfo.RefreshBinding();
				HasChanges = true;
			}
		}

		public ZPropertyInfo IsNDRInfo
		{
			get { return GetZPropertyInfo(nameof(IsNDR)); }
		}

		internal bool IsNDR_ReadOnly
		{
			get { return IsNDR == ZBool.False; }
		}

		internal ZDateTime DeliveryReportTimeUtc
		{
			get { return EmailAddress.GI_DeliveryReportTimeUtc; }
		}

		#endregion

		#region Visible
		[ResourceStringData("GlbStaffEmailAddress|IsVisible", Caption = "Is Publicly Visible", ShortCaption = "Visible")]
		public ZBool IsVisible
		{
			get { return base.GSE_IsVisible; }
			set
			{
				if (IsVisible == value)
				{
					return;
				}

				base.GSE_IsVisible = value;
				IsVisibleInfo.RefreshBinding();
				HasChanges = true;
			}
		}

		public ZPropertyInfo IsVisibleInfo
		{
			get { return GetZPropertyInfo(nameof(IsVisible)); }
		}

		#endregion
	}
}
