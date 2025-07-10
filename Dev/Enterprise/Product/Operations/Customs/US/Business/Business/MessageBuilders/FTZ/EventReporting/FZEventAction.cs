
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public enum FZEventType { Concur, Unconcur, Delivery }

	public class FZEventAction : NonPersistentBusinessObject
	{
		#region Schema
		public static class Schema
		{
			public const string US_ActionCode = "US_ActionCode";
			public const string US_ReasonCode = "US_ReasonCode";
			public const string US_FTZContactName = "US_FTZContactName";
			public const string US_FTZContactPhone = "US_FTZContactPhone";
			public const string US_Reasons = "US_Reasons";
			public const int US_ActionCodeMaxLength = 1;
			public const int US_ReasonCodeMaxLength = 2;
			public const int US_FTZContactNameMaxLegnth = 40;
			public const int US_FTZContactPhoneMaxLength = 15;
			public const int US_ReasonsMaxLength = 50;
		}
		#endregion

		public FZEventAction(IFZEventHeader header, FZEventType eventType)
			: base(((Messaging.Business.IMessageAttachee)header).Factory)
		{
			this.EventType = eventType;
			this.EventHeader = header;

			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				SetDefaults();
			}
		}
		public readonly FZEventType EventType;
		internal readonly IFZEventHeader EventHeader;

		void SetDefaults()
		{
			var contact = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(Factory, EventHeader.BusinessObjectPK, EventHeader.CompanyPK, EventHeader.Branch.PK.ToGuid());
			if (contact != null)
			{
				US_FTZContactName = contact.GS_FullName.Left(Schema.US_FTZContactNameMaxLegnth);
				US_FTZContactPhone = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(contact).Left(Schema.US_FTZContactPhoneMaxLength);
			}
		}

		public ZBool IsCancelled
		{
			get;
			set;
		}

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
					Validation.ValidateUS_FTZContactName();
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
					Validation.ValidateUS_FTZContactPhone();
				}
			}
		}
		ZString fUS_FTZContactPhone;

		public ZPropertyInfo US_FTZContactPhoneInfo => this.GetZPropertyInfo(Schema.US_FTZContactPhone);

		#endregion

		#region US_ActionCode

		[List(nameof(Lookups) + "." + nameof(FZEventActionLookups.ActionCodeList))]
		[MaxLength(Schema.US_ActionCodeMaxLength)]
		public ZString US_ActionCode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fUS_ActionCode; }
			set
			{
				CheckMaximumLength(US_ActionCodeInfo, value);
				SetNonPersistentPropertyValue(US_ActionCodeInfo, ref fUS_ActionCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_ActionCode();
				}
				MessageSendingObjectsView.FilterBy = value;
			}
		}

		public ZPropertyInfo US_ActionCodeInfo
		{
			get { return this.GetZPropertyInfo(Schema.US_ActionCode); }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZString fUS_ActionCode;

		#endregion

		#region US_ReasonCode

		[List(nameof(Lookups) + "." + nameof(FZEventActionLookups.ReasonCodeList))]
		[MaxLength(Schema.US_ReasonCodeMaxLength)]
		public ZString US_ReasonCode
		{
			get => fUS_ReasonCode;
			set
			{
				var hasChanges = US_ReasonCode != value;
				CheckMaximumLength(US_ReasonCodeInfo, value);
				SetNonPersistentPropertyValue(US_ReasonCodeInfo, ref fUS_ReasonCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_ReasonCode();
				}

				if (hasChanges)
				{
					US_Reasons = ZString.Empty;
				}
			}
		}
		ZString fUS_ReasonCode;

		public ZPropertyInfo US_ReasonCodeInfo => GetZPropertyInfo(Schema.US_ReasonCode);

		internal bool IsAC_ActionCode
		{
			get { return US_ActionCode == FTZActionCodeList.Codes.A || US_ActionCode == FTZActionCodeList.Codes.C; }
		}

		#endregion

		#region US_Reasons

		[MaxLength(Schema.US_ReasonsMaxLength)]
		public ZString US_Reasons
		{
			get => fUS_Reasons;
			set
			{
				CheckMaximumLength(US_ReasonsInfo, value);
				SetNonPersistentPropertyValue(US_ReasonsInfo, ref fUS_Reasons, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUS_Reasons();
				}
			}
		}
		ZString fUS_Reasons;

		public ZPropertyInfo US_ReasonsInfo => this.GetZPropertyInfo(Schema.US_Reasons);

		public ResourceStringData GetReasonsCaption()
		{
			ResourceStringData caption = null;
			switch (US_ReasonCode)
			{
				case FTZUnconcurrenceReasonCodeList.Codes._02:
					caption = Res.GetData("66971C64-5FFD-4058-867B-438423148F1D", "Replacement In Bond Number");
					break;
				case FTZUnconcurrenceReasonCodeList.Codes._03:
					caption = Res.GetData("0A6B7C81-19CA-4BCD-8DBC-5C92FE733F6A", "Replacement FTZ Admission Number");
					break;
				case FTZUnconcurrenceReasonCodeList.Codes._04:
				case FTZUnconcurrenceReasonCodeList.Codes._05:
					caption = Res.GetData("7D20FF15-55A5-464E-8F28-214758464A62", "Replacement Entry Number");
					break;
			}
			return caption;
		}

		#endregion

		#region message sending object

		public FZConcurrenceMessageSendingObject[] GetMessageSendingObjectsNeedSending()
		{
			return MessageSendingObjectsView.Cast<FZConcurrenceMessageSendingObject>().Where(a => a.MB_Send && a.MB_US_ActionCode == US_ActionCode).ToArray();
		}

		public void UpdateFTZConcurrenceQty()
		{
			foreach (var messageSendingObject in GetMessageSendingObjectsNeedSending())
			{
				messageSendingObject.UpdateFTZConcurrenceQtyIfNeeded();
			}
		}

		public FZConcurrenceMessageSendingObjectCollection MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					messageSendingObjects = new FZConcurrenceMessageSendingObjectCollection(Factory);
					var objA = new FZConcurrenceMessageSendingObject(EventHeader);

					objA.MB_US_ActionCode = FTZActionCodeList.Codes.A;
					objA.MB_Send = true;
					objA.MB_Identifier = EventHeader.FTZAdmissionNumber;
					objA.MB_ManifestQty = 0;
					messageSendingObjects.Add(objA);
					var totalFTZConcurrenceQty = 0m;
					foreach (var bill in EventHeader.Bills)
					{
						var objB = new FZConcurrenceMessageSendingObject(bill);
						objB.MB_US_ActionCode = FTZActionCodeList.Codes.B;
						objB.MB_Send = true;
						objB.MB_Identifier = bill.CU_BillNum;
						objB.MB_ManifestQty = bill.CU_NoOfPacks;
						if (objB.MB_ConcurrenceQty.IsEmpty)
						{
							objB.MB_ConcurrenceQty = objB.MB_ManifestQty;
						}
						totalFTZConcurrenceQty += objB.MB_ManifestQty;
						objB.MB_ManifestUQ = bill.CU_PackType;
						objB.MB_ConcurrenceUQ = objB.MB_ManifestUQ;
						messageSendingObjects.Add(objB);
					}

					foreach (var bill in EventHeader.LowestBills)
					{
						foreach (ITAndSplitDetails itDetail in bill.ITAndSplitDetails)
						{
							var objC = new FZConcurrenceMessageSendingObject(itDetail);
							objC.MB_US_ActionCode = FTZActionCodeList.Codes.C;
							objC.MB_Send = true;
							objC.MB_Identifier = itDetail.US_ITNumber;
							objC.MB_ManifestQty = (ZDecimal)itDetail.US_NoOfPacks;
							if (objC.MB_ConcurrenceQty.IsEmpty)
							{
								objC.MB_ConcurrenceQty = objC.MB_ManifestQty;
							}
							objC.MB_ManifestUQ = bill.CU_PackType;
							objC.MB_ConcurrenceUQ = objC.MB_ManifestUQ;
							messageSendingObjects.Add(objC);
						}
					}

					objA.MB_ManifestQty = totalFTZConcurrenceQty;
					if (objA.MB_ConcurrenceQty.IsEmpty)
					{
						objA.MB_ConcurrenceQty = objA.MB_ManifestQty;
					}

					RegisterEditableChildObject(messageSendingObjects);
				}
				return messageSendingObjects;
			}
		}
		FZConcurrenceMessageSendingObjectCollection messageSendingObjects;

		/// <summary>
		/// This is the collection that is exposed on the form
		/// </summary>
		public FZConcurrenceMessageSendingObjectCollectionView MessageSendingObjectsView => messageSendingObjectsView ?? (messageSendingObjectsView = new FZConcurrenceMessageSendingObjectCollectionView(this));
		FZConcurrenceMessageSendingObjectCollectionView messageSendingObjectsView;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public FZEventActionValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual FZEventActionValidation GetNewValidation()
		{
			return new FZEventActionValidation(this);
		}

		#endregion

		public FZEventActionLookups Lookups
		{
			get { return new FZEventActionLookups(this); }
		}
	}
}
