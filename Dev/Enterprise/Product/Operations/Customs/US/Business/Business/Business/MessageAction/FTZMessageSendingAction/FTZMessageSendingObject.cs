using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	public class FTZMessageSendingObject : NonPersistentBusinessObject, IObsoleteValidation, IFTZHeader
	{
		#region Schema

		class Schema
		{
			public const string US_FTZContactName = "US_FTZContactName";
			public const string US_FTZContactPhone = "US_FTZContactPhone";
			public const string US_ChangeOrAddConveyance = "US_ChangeOrAddConveyance";
			public const string US_DeleteConveyance = "US_DeleteConveyance";
			public const string US_ChangeOrAddBillOfLading = "US_ChangeOrAddBillOfLading";
			public const string US_DeleteBillOfLading = "US_DeleteBillOfLading";
			public const string US_ChangeOrAddHTSLine = "US_ChangeOrAddHTSLine";
			public const string US_DeleteHTSLine = "US_DeleteHTSLine";
			public const string US_ChangeAdmittedQuantity = "US_ChangeAdmittedQuantity";
			public const string US_CancelOrAddPTT = "US_CancelOrAddPTT";
			public const string US_OtherReason = "US_OtherReason";
			public const string US_Remarks = "US_Remarks";

			public const int US_FTZContactNameMaxLegnth = 40;
			public const int US_FTZContactPhoneMaxLength = 15;
			public const int US_RemarksMaxLegnth = 78;
		}

		#endregion

		public FTZMessageSendingObject(JobDeclaration declaration, UpdateActionCode actionCode)
			: base(declaration.Factory)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
			ActionCode = actionCode;

			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				SetDefaults();
			}
		}

		public readonly JobDeclaration Declaration;
		public readonly UpdateActionCode ActionCode;

		void SetDefaults()
		{
			var contact = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(Factory, Declaration.PK, Declaration.RegistryCompanyPK, Declaration.RegistryBranchPK);
			if (contact != null)
			{
				US_FTZContactName = contact.GS_FullName.Left(Schema.US_FTZContactNameMaxLegnth);
				US_FTZContactPhone = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(contact).Left(Schema.US_FTZContactPhoneMaxLength);
			}
		}

		#region Properties

		public bool ShouldSendMessage { get; set; }

		#region US_FTZContactName

		[MaxLength(Schema.US_FTZContactNameMaxLegnth)]
		public ZString US_FTZContactName
		{
			get => fUS_FTZContactName;
			set
			{
				CheckMaximumLength(US_FTZContactNameInfo, value);
				SetNonPersistentPropertyValue(US_FTZContactNameInfo, ref fUS_FTZContactName, value);
				if (!IsValidationSuspended)
				{
					ValidateUS_FTZContactName();
				}
			}
		}
		ZString fUS_FTZContactName;

		public ZPropertyInfo US_FTZContactNameInfo => this.GetZPropertyInfo(Schema.US_FTZContactName);

		#endregion

		#region US_FTZContactPhone

		[MaxLength(Schema.US_FTZContactPhoneMaxLength)]
		public ZString US_FTZContactPhone
		{
			get => fUS_FTZContactPhone;
			set
			{
				CheckMaximumLength(US_FTZContactPhoneInfo, value);
				SetNonPersistentPropertyValue(US_FTZContactPhoneInfo, ref fUS_FTZContactPhone, value);
				if (!IsValidationSuspended)
				{
					ValidateUS_FTZContactName();
				}
			}
		}
		ZString fUS_FTZContactPhone;

		public ZPropertyInfo US_FTZContactPhoneInfo => this.GetZPropertyInfo(Schema.US_FTZContactPhone);

		#endregion

		#region US_ChangeOrAddConveyance

		public ZBool US_ChangeOrAddConveyance
		{
			get => fUS_ChangeOrAddConveyance;
			set => SetNonPersistentPropertyValue(US_ChangeOrAddConveyanceeInfo, ref fUS_ChangeOrAddConveyance, value);
		}
		ZBool fUS_ChangeOrAddConveyance;

		public ZPropertyInfo US_ChangeOrAddConveyanceeInfo => GetZPropertyInfo(Schema.US_ChangeOrAddConveyance);

		#endregion

		#region US_DeleteConveyance
		public ZBool US_DeleteConveyance
		{
			get => fUS_DeleteConveyance;
			set => SetNonPersistentPropertyValue(US_DeleteConveyanceInfo, ref fUS_DeleteConveyance, value);
		}
		ZBool fUS_DeleteConveyance;

		public ZPropertyInfo US_DeleteConveyanceInfo => GetZPropertyInfo(Schema.US_DeleteConveyance);

		#endregion

		#region US_ChangeOrAddBillOfLading

		public ZBool US_ChangeOrAddBillOfLading
		{
			get => fUS_ChangeOrAddBillOfLading;
			set => SetNonPersistentPropertyValue(US_ChangeOrAddBillOfLadingInfo, ref fUS_ChangeOrAddBillOfLading, value);
		}
		ZBool fUS_ChangeOrAddBillOfLading;

		public ZPropertyInfo US_ChangeOrAddBillOfLadingInfo => GetZPropertyInfo(Schema.US_ChangeOrAddBillOfLading);

		#endregion

		#region US_DeleteBillOfLading

		public ZBool US_DeleteBillOfLading
		{
			get => fUS_DeleteBillOfLading;
			set => SetNonPersistentPropertyValue(US_DeleteBillOfLadingInfo, ref fUS_DeleteBillOfLading, value);
		}
		ZBool fUS_DeleteBillOfLading;

		public ZPropertyInfo US_DeleteBillOfLadingInfo => GetZPropertyInfo(Schema.US_DeleteBillOfLading);

		#endregion

		#region US_ChangeOrAddHTSLine

		public ZBool US_ChangeOrAddHTSLine
		{
			get => fUS_ChangeOrAddHTSLine;
			set => SetNonPersistentPropertyValue(US_ChangeOrAddHTSLineInfo, ref fUS_ChangeOrAddHTSLine, value);
		}
		ZBool fUS_ChangeOrAddHTSLine;

		public ZPropertyInfo US_ChangeOrAddHTSLineInfo => GetZPropertyInfo(Schema.US_ChangeOrAddHTSLine);

		#endregion

		#region US_DeleteHTSLine

		public ZBool US_DeleteHTSLine
		{
			get => fUS_DeleteHTSLine;
			set => SetNonPersistentPropertyValue(US_DeleteHTSLineInfo, ref fUS_DeleteHTSLine, value);
		}
		ZBool fUS_DeleteHTSLine;

		public ZPropertyInfo US_DeleteHTSLineInfo => GetZPropertyInfo(Schema.US_DeleteHTSLine);

		#endregion

		#region US_ChangeAdmittedQuantity

		public ZBool US_ChangeAdmittedQuantity
		{
			get => fUS_ChangeAdmittedQuantity;
			set => SetNonPersistentPropertyValue(US_ChangeAdmittedQuantityInfo, ref fUS_ChangeAdmittedQuantity, value);
		}
		ZBool fUS_ChangeAdmittedQuantity;

		public ZPropertyInfo US_ChangeAdmittedQuantityInfo => GetZPropertyInfo(Schema.US_ChangeAdmittedQuantity);

		#endregion

		#region US_ChangeAdmittedQuantity

		public ZBool US_CancelOrAddPTT
		{
			get => fUS_CancelOrAddPTT;
			set => SetNonPersistentPropertyValue(US_CancelOrAddPTTInfo, ref fUS_CancelOrAddPTT, value);
		}
		ZBool fUS_CancelOrAddPTT;

		public ZPropertyInfo US_CancelOrAddPTTInfo => GetZPropertyInfo(Schema.US_CancelOrAddPTT);

		#endregion

		#region US_OtherReason

		public ZBool US_OtherReason
		{
			get => fUS_OtherReason;
			set
			{
				var hasChanges = US_OtherReason != value;
				SetNonPersistentPropertyValue(US_OtherReasonInfo, ref fUS_OtherReason, value);
				if (!IsValidationSuspended)
				{
					ValidateUS_Remarks();
				}
				if (hasChanges && !US_OtherReason)
				{
					US_Remarks = ZString.Empty;
				}
			}
		}
		ZBool fUS_OtherReason;

		public ZPropertyInfo US_OtherReasonInfo => GetZPropertyInfo(Schema.US_OtherReason);

		#endregion

		#region US_Remarks

		[MaxLength(Schema.US_RemarksMaxLegnth)]
		public ZString US_Remarks
		{
			get => fUS_Remarks;
			set
			{
				CheckMaximumLength(US_RemarksInfo, value);
				SetNonPersistentPropertyValue(US_RemarksInfo, ref fUS_Remarks, value);
				if (!IsValidationSuspended)
				{
					ValidateUS_Remarks();
				}
			}
		}
		ZString fUS_Remarks;

		public ZPropertyInfo US_RemarksInfo => GetZPropertyInfo(Schema.US_Remarks);

		#endregion

		ZDecimal IFTZConcurrence.FTZConcurrenceQty { get; set; }

		static class ReplaceReasonCode
		{
			public const string ChangeOrAddConveyance = "01";
			public const string DeleteConveyance = "02";
			public const string ChangeOrAddBillOfLading = "03";
			public const string DeleteBillOfLading = "04";
			public const string ChangeOrAddHTSLine = "05";
			public const string DeleteHTSLine = "06";
			public const string ChangeAdmittedQuantity = "07";
			public const string OtherReason = "08";
			public const string CancelOrAddPTT = "09";
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUS_FTZContactName();
			ValidateUS_Remarks();
		}

		void ValidateUS_FTZContactName()
		{
			US_FTZContactNameInfo.ClearAllNotifications();
			if (US_FTZContactName.IsEmpty && !US_FTZContactPhone.IsEmpty)
			{
				US_FTZContactNameInfo.AddMessageError(FTZContactNameIsMandatory);
			}
			if (!US_FTZContactName.IsEmpty && US_FTZContactName.ContainsAnyChar(FTZContactNameInvalidPattern))
			{
				US_FTZContactNameInfo.AddMessageError(FTZContactNameWithPeriod);
			}
		}
		const string FTZContactNameIsMandatory = "Contact Name is mandatory when Phone Number is entered.";
		const string FTZContactNameInvalidPattern = ".";
		internal const string FTZContactNameWithPeriod = "Period \".\" not allowed in Contact Name.";

		void ValidateUS_Remarks()
		{
			US_RemarksInfo.ClearAllNotifications();
			if (US_OtherReason && US_Remarks.IsEmpty)
			{
				US_RemarksInfo.AddMessageError(RemarksIsMandatory);
			}
		}
		const string RemarksIsMandatory = "Remarks is mandatory when Other/Remarks is ticked.";

		#endregion

		#region IFTZHeader Members

		IFTZCommonHeader header => Declaration;

		void IFTZCommonHeader.AddMessage(MQEDIMessage message)
		{
			header.AddMessage(message);
		}

		void IFTZCommonHeader.SetMessageStatus(ZString subType, ZString status)
		{
			header.SetMessageStatus(subType, status);
		}

		bool IFTZCommonHeader.HasBeenLodgedAtCustoms => header.HasBeenLodgedAtCustoms;
		bool IFTZCommonHeader.IncludePTTInAdmission => header.IncludePTTInAdmission;
		ZString IFTZCommonHeader.FTZAdmissionNumber => header.FTZAdmissionNumber;
		ZString IFTZCommonHeader.ZoneID => header.ZoneID;
		ZInt IFTZCommonHeader.CalendarYear => header.CalendarYear;
		ZString IFTZCommonHeader.ControlNumber => header.ControlNumber;
		bool IFTZCommonHeader.DirectDeliveryIndicator => header.DirectDeliveryIndicator;
		IEnumerable<IFTZBillCommon> IFTZCommonHeader.Bills => header.Bills;
		IEnumerable<IFTZBillCommon> IFTZCommonHeader.LowestBills => header.LowestBills;

		ZString IFTZHeader.PortCode => Declaration.US_SchDEntry;
		ZString IFTZHeader.ABIRoutingCode => Declaration.US_F_RoutingDetails;
		ZString IFTZHeader.IRSIdentifier => OrgHeaderWrapper.GetCustomsRelatedCode(Declaration.WarehouseDocAddress?.Organisation, OrgMatchedCustomsRegNoType.EIN);
		ZString IFTZHeader.AdmissionType => Declaration.US_F_AdmissionType;
		ZString IFTZHeader.FirmsIdentifier => Declaration.US_US_NKLocationOfGoods;
		ZString IFTZHeader.ImporterOfRecordID => Declaration.ImporterOfRecordNumber;
		ZString IFTZHeader.ContactName => US_FTZContactName;
		ZString IFTZHeader.ContactPhone => US_FTZContactPhone;
		IEnumerable<ZString> IFTZHeader.ReasonCodes
		{
			get
			{
				if (US_ChangeOrAddConveyance)
				{
					yield return ReplaceReasonCode.ChangeOrAddConveyance;
				}
				if (US_DeleteConveyance)
				{
					yield return ReplaceReasonCode.DeleteConveyance;
				}
				if (US_ChangeOrAddBillOfLading)
				{
					yield return ReplaceReasonCode.ChangeOrAddBillOfLading;
				}
				if (US_DeleteBillOfLading)
				{
					yield return ReplaceReasonCode.DeleteBillOfLading;
				}
				if (US_ChangeOrAddHTSLine)
				{
					yield return ReplaceReasonCode.ChangeOrAddHTSLine;
				}
				if (US_DeleteHTSLine)
				{
					yield return ReplaceReasonCode.DeleteHTSLine;
				}
				if (US_ChangeAdmittedQuantity)
				{
					yield return ReplaceReasonCode.ChangeAdmittedQuantity;
				}
				if (US_OtherReason)
				{
					yield return ReplaceReasonCode.OtherReason;
				}
				if (US_CancelOrAddPTT)
				{
					yield return ReplaceReasonCode.CancelOrAddPTT;
				}
			}
		}
		ZString IFTZHeader.Remarks => US_Remarks;

		bool IFTZHeader.IsWaitingForResponse => Declaration.IsFTZWaitingForResponse;

		ZString IMessageAttacheeWithCBPSenderReference.EntryFilerCode => header.EntryFilerCode;
		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort => header.ProcessingDistrictPort;
		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode => header.ProcessingOfficeCode;
		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK => header.CompanyPK;
		ZString IMessageAttacheeWithCBPSenderReference.TransportMode => header.TransportMode;

		ZString IMessageAttachee.MessageStatus
		{
			get => header.MessageStatus;
			set => header.MessageStatus = value;
		}
		CBPEDIMessageCollection IMessageAttachee.Messages => header.Messages;
		GlbBranch IMessageAttachee.Branch => ((IMessageAttachee)Declaration).Branch;
		BusinessObject IMessageAttachee.TopLevelBusinessObject => header.TopLevelBusinessObject;
		string IMessageAttachee.TopLevelBizObjReferenceNumber => header.TopLevelBizObjReferenceNumber;
		Logs IMessageAttachee.TopLevelBusinessObjectLogs => header.TopLevelBusinessObjectLogs;
		ControllerID IControllerIDProvider.ControllerID => header.ControllerID;
		Guid IControllerIDProvider.BusinessObjectPK => header.BusinessObjectPK;

		IEnumerable<IFTZConveyance> IFTZHeader.Conveyances
		{
			get
			{
				var result = new List<IFTZConveyance>();
				Action<IFTZConveyance> addConveyanceObjectIntoList = (conveyance) =>
				{
					if (conveyance.Bills.Any())
					{
						result.Add(conveyance);
					}
				};

				var declarationConveyanceObject = (IFTZConveyance)new FTZMessageConveyanceObjectWrapper(Declaration);
				addConveyanceObjectIntoList(declarationConveyanceObject);

				foreach (Bill bill in Declaration.Bills)
				{
					if (bill.US_SESplitShip)
					{
						foreach (ITAndSplitDetails splitDetail in bill.ITAndSplitDetails)
						{
							var splitConveyanceObject = (IFTZConveyance)new FTZMessageConveyanceObjectWrapper(Declaration, splitDetail);
							addConveyanceObjectIntoList(splitConveyanceObject);
						}

						if (bill.Invoices.OfType<JobComInvoiceHeader>().Any(invoice => invoice.US_SplitShipmentDetail.IsEmpty))
						{
							var emptyConveyanceBillObject = (IFTZConveyance)new FTZMessageConveyanceObjectWrapper(Declaration, bill);
							addConveyanceObjectIntoList(emptyConveyanceBillObject);
						}
					}
				}

				return result;
			}
		}

		#endregion
	}
}
