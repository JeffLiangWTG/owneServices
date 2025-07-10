using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoAccQueryClaim.Schema.AY_ShortDescriptionOfClaim), CodeProperty(AutoAccQueryClaim.Schema.AY_QueryClaimReference)]
	public abstract class AccQueryClaim : AutoAccQueryClaim, IQueryClaim
	{
		public new class Schema : AutoAccQueryClaim.Schema
		{
			public const string Details = "Details";
			public const string AY_GS_NKCreator = "AY_GS_NKCreator";
			public const string AY_RX_TransactionCurrencyCode = "AY_RX_TransactionCurrencyCode";
			public const string AY_OH_TransactionBranchOrgProxy = "AY_OH_TransactionBranchOrgProxy";
			public const string AY_GB_TransactionBranch = "AY_GB_TransactionBranch";
		}

		public AccQueryClaim(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AY_QueryClaimAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AY_AH), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AY_OH_Debtor), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AY_QueryClaimStatus), ConcurrencyPolicy.Strict);
		}

		public static readonly AccQueryClaimTypeDecider TypeDecider = new AccQueryClaimTypeDecider();

		#region Overriden

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!fDetails.IsEmpty)
			{
				LoadOrCreateDetailsNote().ST_NoteDataAsText = fDetails;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region Internal Properties

		internal bool IsAutoLoggedInternal => IsAutoLogged;
		internal bool IsPropertiesReadOnlyInternal => IsPropertiesReadOnly;

		#endregion

		#endregion

		#region Properties

		#region Overriden

		#region AY_HoldOption

		[List("Lookups.HoldOptions")]
		public override ZString AY_HoldOption
		{
			get => base.AY_HoldOption;
			set => base.AY_HoldOption = value;
		}

		public virtual bool AY_HoldOption_ReadOnly
		{
			get => IsPropertiesReadOnly;
		}

		public virtual bool IsHoldOptionVisible
		{
			get => false;
		}

		#endregion

		[ReadOnlyMember(nameof(IsPropertiesReadOnly))]
		[List("Lookups.ClaimType")]
		public override ZString AY_QueryClaimType
		{
			get
			{
				return base.AY_QueryClaimType;
			}
			set
			{
				base.AY_QueryClaimType = value;
			}
		}

		[ReadOnlyMember(nameof(IsPropertiesReadOnly))]
		[List("Lookups.ClaimReason")]
		public override ZString AY_QueryClaimReasonCode
		{
			get
			{
				return base.AY_QueryClaimReasonCode;
			}
			set
			{
				base.AY_QueryClaimReasonCode = value;
			}
		}

		[List("Lookups.ClaimStatus")]
		public override ZString AY_QueryClaimStatus
		{
			get
			{
				return base.AY_QueryClaimStatus;
			}
			set
			{
				base.AY_QueryClaimStatus = value;
			}
		}

		protected virtual bool AY_QueryClaimStatus_ReadOnly
		{
			get { return IsPropertiesReadOnly; }
		}

		[List("Lookups.TransactionHeaders")]
		public override ZGuid AY_AH
		{
			get
			{
				return base.AY_AH;
			}
			set
			{
				base.AY_AH = value;
			}
		}

		protected virtual bool AY_AH_ReadOnly
		{
			get { return IsPropertiesReadOnly; }
		}

		[ReadOnly(true)]
		[List("Lookups.Branches")]
		public override ZGuid AY_GB
		{
			get
			{
				return base.AY_GB;
			}
			set
			{
				base.AY_GB = value;
			}
		}

		#region AY_OC

		[ReadOnlyMember(nameof(IsPropertiesReadOnly))]
		[List("Lookups.Contacts")]
		public override ZGuid AY_OC
		{
			get
			{
				return base.AY_OC;
			}
			set
			{
				base.AY_OC = value;
			}
		}

		protected void SetDefaultContact()
		{
			if (Debtor != null)
			{
				OrgContact contact = new DefaultContactFinder(Debtor).DefaultContact(DefaultContactType);
				if (contact != null)
				{
					AY_OC = contact.PK;
				}
			}
		}

		protected abstract ContactType DefaultContactType { get; }

		#endregion

		#region AY_OH_Debtor

		[List("Lookups.Debtors")]
		public override ZGuid AY_OH_Debtor
		{
			get
			{
				return IsIntercompanyClaim ? AY_OH_TransactionBranchOrgProxy : base.AY_OH_Debtor;
			}
			set
			{
				base.AY_OH_Debtor = value;
				SetDefaultContact();
			}
		}

		public ZGuid AY_OH_Debtor_Original
		{
			get
			{
				return base.AY_OH_Debtor;
			}
		}

		#region AY_OH_Debtor_ReadOnly

		protected virtual bool AY_OH_Debtor_ReadOnly
		{
			get { return aY_OH_Debtor_ReadOnly || IsPropertiesReadOnly; }
			private set { aY_OH_Debtor_ReadOnly = value; }
		}

		public void SetDebtorReadOnly(bool readOnly)
		{
			aY_OH_Debtor_ReadOnly = readOnly;
		}

		bool aY_OH_Debtor_ReadOnly;

		#endregion

		#endregion

		[ReadOnly(true)]
		public override ZString AY_GS_NKStaffAssignedTo
		{
			get
			{
				return base.AY_GS_NKStaffAssignedTo;
			}
			set
			{
				base.AY_GS_NKStaffAssignedTo = value;
			}
		}

		[List("Lookups.TaskAssignedToList")]
		public override GlbStaff StaffAssignedTo
		{
			get
			{
				return base.StaffAssignedTo;
			}
		}

		[ReadOnlyMember(nameof(IsPropertiesReadOnly))]
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal AY_QueryClaimAmount
		{
			get
			{
				return base.AY_QueryClaimAmount;
			}
			set
			{
				base.AY_QueryClaimAmount = value;
			}
		}

		[ReadOnlyMember(nameof(IsPropertiesReadOnly))]
		public override ZString AY_ShortDescriptionOfClaim
		{
			get
			{
				return base.AY_ShortDescriptionOfClaim;
			}
			set
			{
				base.AY_ShortDescriptionOfClaim = value;
			}
		}

		[ReadOnlyMember(nameof(IsPropertiesReadOnly))]
		public override ZDateTime AY_QueryClaimNextFollowUp
		{
			get
			{
				return base.AY_QueryClaimNextFollowUp;
			}
			set
			{
				base.AY_QueryClaimNextFollowUp = value;
			}
		}

		[ReadOnly(true)]
		public override ZString AY_MasterBillNumber { get => base.AY_MasterBillNumber; set => base.AY_MasterBillNumber = value; }

		#endregion

		#region Creator

		[ReadOnlyMember(nameof(IsPropertiesReadOnly))]
		[List("Lookups.Staff")]
		public ZString AY_GS_NKCreator
		{
			get
			{
				ZString result = default;
				if (IsInDatabase)
				{
					StmALog addedLog = Logs.AddedLog;
					if (addedLog != null)
					{
						result = addedLog.SL_GS_NKUser;
					}
				}

				if (result.IsEmpty)
				{
					result = GlbStaff.CurrentUser.GS_Code;
				}

				return result;
			}
		}

		public ZPropertyInfo AY_GS_NKCreatorInfo
		{
			get { return GetZPropertyInfo(Schema.AY_GS_NKCreator); }
		}

		#endregion

		#region Details

		[ReadOnly(true)]
		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString Details
		{
			get
			{
				if (!fHasSetfDetails && IsInDatabase && HasDetailsNote)
				{
					fDetails = GetDetailsNoteIfItExists().ST_NoteDataAsText;
				}
				return fDetails;
			}
			set
			{
				CheckMaximumLength(DetailsInfo, value);
				fHasSetfDetails = true;
				HasChanges = true;
				SetNonPersistentPropertyValue(DetailsInfo, ref fDetails, value);
			}
		}

		public ZPropertyInfo DetailsInfo
		{
			get { return GetZPropertyInfo(Schema.Details); }
		}

		bool fHasSetfDetails;
		ZString fDetails;

		HiddenStmNote LoadOrCreateDetailsNote()
		{
			HiddenStmNote note = GetDetailsNoteIfItExists();
			if (note == null)
			{
				note = Factory.New<HiddenStmNote>();
				note.ST_Description = Schema.Details;
				note.ST_Table = AccQueryClaimSchema.Constants.TableName;
				note.ST_ParentID = this.PK;
			}
			fHasSetfDetails = true;
			return note;
		}

		bool HasDetailsNote
		{
			get
			{
				return GetDetailsNoteIfItExists() != null;
			}
		}

		HiddenStmNote GetDetailsNoteIfItExists()
		{
			ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, Schema.Details);
			filter.AddToFilter(StmNoteSchema.ST_ParentID, this.PK);
			//Filter.AddToFilter(StmNoteSchema.ST_Table, this.TableName);
			return (HiddenStmNote)Factory.LoadTop1(typeof(HiddenStmNote), filter);
		}

		#endregion

		#region Currency

		public virtual RefCurrency Currency
		{
			get { return TransactionHeader != null && TransactionHeader.TransactionCurrency != null ? TransactionHeader.TransactionCurrency : null; }
		}

		public int CurrencyDecimals => Currency != null ? Currency.Decimals : GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		#endregion

		#region AY_RX_TransactionCurrencyCode

		public ZString AY_RX_TransactionCurrencyCode => Currency?.RX_Code ?? ZString.Empty;

		public ZPropertyInfo AY_RX_TransactionCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AY_RX_TransactionCurrencyCode); }
		}

		#endregion

		#region AY_OH_TransactionBranchOrgProxy

		[List("Lookups.Debtors")]
		public ZGuid AY_OH_TransactionBranchOrgProxy
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (TransactionHeader != null && TransactionHeader.Branch != null)
				{
					result = TransactionHeader.Branch.GB_OH_OrgProxy;
				}
				return result;
			}
		}

		public ZPropertyInfo AY_OH_TransactionBranchOrgProxyInfo
		{
			get { return GetZPropertyInfo(Schema.AY_OH_TransactionBranchOrgProxy); }
		}

		#endregion

		#region AY_GB_TransactionBranch

		[List("Lookups.Branches")]
		public ZGuid AY_GB_TransactionBranch
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (TransactionHeader != null && TransactionHeader.Branch != null)
				{
					result = TransactionHeader.Branch.PK;
				}
				return result;
			}
		}

		public ZPropertyInfo AY_GB_TransactionBranchInfo
		{
			get { return GetZPropertyInfo(Schema.AY_GB_TransactionBranch); }
		}

		#endregion

		protected virtual bool IsPropertiesReadOnly
		{
			get
			{
				return IsIntercompanyClaim;
			}
		}
		public virtual bool IsCreatedFromCASS => false;

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AY_GB = Env.CurrentBranch.PK;
			AY_GS_NKStaffAssignedTo = Env.CurrentUser.Initials;

			if (Lookups.ClaimStatus.Count > 0)
			{
				AY_QueryClaimStatus = Lookups.ClaimStatus.DefaultCode;
			}
			if (Lookups.ClaimType.Count > 0)
			{
				AY_QueryClaimType = Lookups.ClaimType.DefaultCode;
			}
			if (Lookups.ClaimReason.Count > 0)
			{
				AY_QueryClaimReasonCode = Lookups.ClaimReason.DefaultCode;
			}
			AY_QueryClaimNextFollowUp = ZDateTime.Today.AddDays(7);
		}

		#endregion

		public void AddToLog(ZString comment)
		{
			if (!comment.Trim().IsEmpty)
			{
				Details = ZDateTime.Now.ToLongTimeString() + " " +
					GlbStaff.CurrentUser.GS_Code + " - " + GlbCompany.CurrentCompany.GC_Code + " - " +
					comment + "\r\n" + ZString.Replicate('-', 118) + "\r\n" + Details;
			}
		}

		public bool IsIntercompanyClaim
		{
			get { return TransactionHeader != null && TransactionHeader.Company != null && TransactionHeader.Company.PK != GlbCompany.CurrentCompany.PK; }
		}

		#region IQueryClaim Members

		public abstract ZString Ledger { get; }

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if ((kind & TestBusinessObjectKind.MinimumRequiredToSave) != 0 && AY_GB.IsEmpty)
			{
				AY_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK; //do this before base call to avoid creation of new GlbCompany
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
	}
}
