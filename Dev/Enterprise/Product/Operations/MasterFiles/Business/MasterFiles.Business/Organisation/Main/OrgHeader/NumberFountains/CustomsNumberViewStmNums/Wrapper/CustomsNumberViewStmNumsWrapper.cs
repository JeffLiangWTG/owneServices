using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsWrapper : NonPersistentBusinessObject<CustomsNumberViewStmNumsWrapperValidation>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CustomsNumberViewStmNumsWrapper(CustomsNumberViewStmNums stmNums)
			: base(stmNums.Factory)
		{
			this.StmNums = stmNums;
			RegisterEditableChildObject(StmNums);
			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				base.AddToFactoryCache();
			}

			InitializeData();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Class is inherited and cannot be static")]
		public class Schema
		{
			public const string SN_AvailableNumbers = CustomsNumberViewStmNums.Schema.SN_AvailableNumbers;
			public const string SN_Count = CustomsNumberViewStmNums.Schema.SN_Count;
			public const string SN_FountainName = CustomsNumberViewStmNums.Schema.SN_FountainName;
			public const string SN_MaximumValue = CustomsNumberViewStmNums.Schema.SN_MaximumValue;
			public const string SN_MinimumValue = CustomsNumberViewStmNums.Schema.SN_MinimumValue;
			public const string SN_Owner = CustomsNumberViewStmNums.Schema.SN_Owner;
			public const string SN_OwnerForDisplay = CustomsNumberViewStmNums.Schema.SN_OwnerForDisplay;
			public const string SN_Type = CustomsNumberViewStmNums.Schema.SN_Type;
			public const string SN_TypeDescription = CustomsNumberViewStmNums.Schema.SN_TypeDescription;
			public const string SN_ValueForDisplay = CustomsNumberViewStmNums.Schema.SN_ValueForDisplay;
			public const string SN_SystemCreateTimeUtc = CustomsNumberViewStmNums.Schema.SN_SystemCreateTimeUtc;
		}

		#region Properties

		public ZString Detail => StmNums.Provider?.GetDetail(this) ?? ZString.Empty;

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_AvailableNumbers", Caption = "Available Numbers", ShortCaption = "Available")]
		public ZLong SN_AvailableNumbers
		{
			get => StmNums.SN_AvailableNumbers;
		}

		public ZPropertyInfo SN_AvailableNumbersInfo => GetWrappedZPropertyInfo(Schema.SN_AvailableNumbers, (x) => StmNums.SN_AvailableNumbersInfo);

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_Count", Caption = "Count")]
		public ZLong SN_Count
		{
			get => StmNums.SN_Count;
			set
			{
				var oldValue = SN_Count;
				StmNums.SN_Count = value;
				if (!IsCopying && oldValue != SN_Count)
				{
					SN_CountInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo SN_CountInfo => GetWrappedZPropertyInfo(Schema.SN_Count, (x) => StmNums.SN_CountInfo);

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_FountainName", Caption = "Name")]
		public virtual ZString SN_FountainName
		{
			get => StmNums.SN_FountainName;
			set
			{
				var oldValue = SN_FountainName;
				StmNums.SN_FountainName = value;
				if (!IsCopying && oldValue != SN_FountainName)
				{
					SN_FountainNameInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo SN_FountainNameInfo => GetWrappedZPropertyInfo(Schema.SN_FountainName, (x) => StmNums.SN_FountainNameInfo);

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_MaximumValue", Caption = "Range End", ShortCaption = "End")]
		public ZLong SN_MaximumValue
		{
			get => StmNums.SN_MaximumValue;
			set
			{
				var oldValue = SN_MaximumValue;
				StmNums.SN_MaximumValue = value;
				if (!IsCopying && oldValue != SN_MaximumValue)
				{
					SN_MaximumValueInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo SN_MaximumValueInfo => GetWrappedZPropertyInfo(Schema.SN_MaximumValue, (x) => StmNums.SN_MaximumValueInfo);

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_MinimumValue", Caption = "Range Start", ShortCaption = "Start")]
		public ZLong SN_MinimumValue
		{
			get => StmNums.SN_MinimumValue;
			set
			{
				var oldValue = SN_MinimumValue;
				StmNums.SN_MinimumValue = value;
				if (!IsCopying && oldValue != SN_MinimumValue)
				{
					SN_MinimumValueInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo SN_MinimumValueInfo => GetWrappedZPropertyInfo(Schema.SN_MinimumValue, (x) => StmNums.SN_MinimumValueInfo);

		[ReadOnlyMember(nameof(SN_Owner_ReadOnly))]
		[List("Lookups.OwnerCollection")]
		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_Owner", Caption = "Owner")]
		public virtual ZGuid SN_Owner
		{
			get => StmNums.SN_Owner;
			set
			{
				var oldValue = SN_Owner;
				StmNums.SN_Owner = value;
				if (!IsCopying && oldValue != SN_Owner)
				{
					SN_OwnerInfo.RefreshBinding(oldValue);
				}
			}
		}

		protected bool SN_Owner_ReadOnly => StmNums.IsInDatabase;

		public ZPropertyInfo SN_OwnerInfo => GetWrappedZPropertyInfo(Schema.SN_Owner, (x) => StmNums.SN_OwnerInfo);

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_OwnerForDisplay", Caption = "Owner")]
		public ZString SN_OwnerForDisplay
		{
			get => StmNums.SN_OwnerForDisplay;
		}

		public ZPropertyInfo SN_OwnerForDisplayInfo => GetWrappedZPropertyInfo(Schema.SN_OwnerForDisplay, (x) => StmNums.SN_OwnerForDisplayInfo);

		[List("Lookups.TypeList")]
		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_Type", Caption = "Range Type", ShortCaption = "Type")]
		public virtual ZString SN_Type
		{
			get => StmNums.SN_Type;
			set
			{
				var oldValue = SN_Type;
				StmNums.SN_Type = value;
				if (!IsCopying && oldValue != SN_Type)
				{
					SN_TypeInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo SN_TypeInfo => GetWrappedZPropertyInfo(Schema.SN_Type, (x) => StmNums.SN_TypeInfo);

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_TypeDescription", Caption = "Range Type Description", ShortCaption = "Description")]
		public ZString SN_TypeDescription
		{
			get => StmNums.SN_TypeDescription;
		}

		public ZPropertyInfo SN_TypeDescriptionInfo => GetWrappedZPropertyInfo(Schema.SN_TypeDescription, (x) => StmNums.SN_TypeDescriptionInfo);

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_ValueForDisplay", Caption = "Current Value")]
		public ZLong SN_ValueForDisplay
		{
			get => StmNums.SN_ValueForDisplay;
		}

		public ZPropertyInfo SN_ValueForDisplayInfo => GetWrappedZPropertyInfo(Schema.SN_ValueForDisplay, (x) => StmNums.SN_ValueForDisplayInfo);

		[ResourceStringData("CustomsNumberViewStmNumsWrapper|SN_SystemCreateTimeUtc", Caption = "Created Time", ShortCaption = "Created")]
		public ZDateTime SN_SystemCreateTimeUtc
		{
			get => StmNums.SN_SystemCreateTimeUtc;
			set
			{
				var oldValue = SN_SystemCreateTimeUtc;
				StmNums.SN_SystemCreateTimeUtc = value;
				if (!IsCopying && oldValue != SN_SystemCreateTimeUtc)
				{
					SN_SystemCreateTimeUtcInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo SN_SystemCreateTimeUtcInfo => GetWrappedZPropertyInfo(Schema.SN_SystemCreateTimeUtc, (x) => StmNums.SN_SystemCreateTimeUtcInfo);

		#endregion

		public readonly CustomsNumberViewStmNums StmNums;

		public override bool IsInDatabase => true;
		public override string TablePrefix => ViewStmNumsSchema.Constants.Prefix;
		public override string TableName => ViewStmNumsSchema.Constants.TableName;
		public override bool CanDelete => StmNums.CanDelete;
		public override MultilingualString ReasonForNotAbleToDelete => StmNums.ReasonForNotAbleToDelete;

		#region Lookups

		public CustomsNumberViewStmNumsWrapperLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected virtual CustomsNumberViewStmNumsWrapperLookups GetNewLookups()
		{
			return new CustomsNumberViewStmNumsWrapperLookups(this);
		}

		CustomsNumberViewStmNumsWrapperLookups fLookups;

		#endregion

		public override CustomsNumberViewStmNumsWrapperValidation GetNewValidation()
		{
			return new CustomsNumberViewStmNumsWrapperValidation(this);
		}

		internal void InitializeData()
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			using (StmNums.SuspendSettingHasChanges())
			using (StmNums.GetValidationSuspender())
			{
				InitializeDataCore();
			}
		}

		protected virtual void InitializeDataCore()
		{ }

		protected override void AddToFactoryCache()
		{
			// should be called after parent is set
		}

		protected override ZGuid GetPK()
		{
			return StmNums.PK;
		}
	}
}
