using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[SingleObjectAroundARow]
	public abstract class ViewStmNums : AutoViewStmNums
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ViewStmNums(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SN_ID), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SN_Value), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, ViewStmNumsSchema.Constants.PK, ConcurrencyPolicy.Ignore);
		}

		#region Schema

		public new class Schema : AutoViewStmNums.Schema
		{
			public const string SN_Count = "SN_Count";
			public const string SN_TypeDescription = "SN_TypeDescription";
			public const string SN_ValueForDisplay = "SN_ValueForDisplay";

			public const long MinimumValue = NumberFountain.FountainUtils.MinNumber;
			public const long DefaultFountainMaximumValue = NumberFountain.FountainUtils.MaxNumber;
		}

		#endregion

		public static readonly TypeDecider TypeDecider = new ViewStmNumsTypeDecider();

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SN_MinimumValue = Schema.MinimumValue;
			SN_MaximumValue = DefaultTypeRangeMax;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			InitializeTypeAndPrefix();
			InitializeCount();
		}

		protected void InitializeCount()
		{
			count = SN_MaximumValue - SN_MinimumValue + 1;
		}

		void InitializeTypeAndPrefix()
		{
			using (GetSetNameAndPrefixInProgress())
			using (SuspendSettingHasChanges())
			{
				InitializeTypeAndPrefixCore();
			}
		}

		protected virtual void InitializeTypeAndPrefixCore()
		{
		}

		#region SetNameAndPrefixInProgress

		public IDisposable GetSetNameAndPrefixInProgress()
		{
			return new SetNameAndPrefixInProgress(this);
		}

		public bool IsSetNameAndPrefixInProgress
		{
			get { return setNameAndPrefixInProgressIndex > byte.MinValue; }
		}

		byte setNameAndPrefixInProgressIndex;

		class SetNameAndPrefixInProgress : IDisposable
		{
			public SetNameAndPrefixInProgress(ViewStmNums stmNums)
			{
				this.stmNums = stmNums;
				stmNums.setNameAndPrefixInProgressIndex++;
			}

			readonly ViewStmNums stmNums;

			void IDisposable.Dispose()
			{
				stmNums.setNameAndPrefixInProgressIndex--;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region Long Values

		[ReadOnly(true)]
		public override ZLong SN_Value
		{
			get { return base.SN_Value; }
			set
			{
				value = (long)value; //verify valid long
				base.SN_Value = value;
			}
		}

		[ResourceStringData("ViewStmNums|SN_ValueForDisplay", Caption = "Current Value")]
		public ZLong SN_ValueForDisplay
		{
			get
			{
				var value = GetFountainValue();
				if (value.HasValue)
				{
					return value.Value > SN_MaximumValue ? new ZLong(-1) : value.Value;
				}

				return SN_Value;
			}
		}

		ZLong? GetFountainValue()
		{
			var fountain = TryGetNumberFountain();
			if (fountain != null)
			{
				fountainValue = fountain.PeekPreliminaryOrDefault(Factory, (long)SN_Value);
			}
			return fountainValue;
		}
		ZLong? fountainValue;

		public ZPropertyInfo SN_ValueForDisplayInfo
		{
			get { return GetZPropertyInfo(Schema.SN_ValueForDisplay); }
		}

		public override ZLong SN_MinimumValue
		{
			get { return base.SN_MinimumValue; }
			set
			{
				var oldValue = SN_MinimumValue;
				value = (long)value; //verify valid long
				if (SN_MinimumValue != value)
				{
					GetFountainValue();
				}
				base.SN_MinimumValue = value;
				if (!IsCopying && oldValue != SN_MinimumValue)
				{
					SN_Value = SN_MinimumValue;
					TrySetMaximumValueSafe();
				}
			}
		}

		[ResourceStringData("ViewStmNums|SN_Count", Caption = "Count")]
		public ZLong SN_Count
		{
			get { return count; }
			set
			{
				value = (long)value; //verify valid long
				if (count != value)
				{
					SetNonPersistentPropertyValue(SN_CountInfo, ref count, value);
					TrySetMaximumValueSafe();

					if (!IsValidationSuspended)
					{
						Validation.ValidateSN_Count();
					}
				}
			}
		}
		ZLong count;

		public ZPropertyInfo SN_CountInfo
		{
			get { return GetZPropertyInfo(Schema.SN_Count); }
		}

		void TrySetMaximumValueSafe()
		{
			try
			{
				SN_MaximumValue = CalculatedMaximumValue;
			}
			catch (OverflowException)
			{
				SN_MaximumValue = DefaultTypeRangeMax;
			}
		}

		ZLong CalculatedMaximumValue
		{
			get { return SN_MinimumValue + SN_Count - 1; }
		}

		[ReadOnly(true)]
		public override ZLong SN_MaximumValue
		{
			get { return base.SN_MaximumValue; }
			set
			{
				value = (long)value; //verify valid long
				if (SN_MaximumValue != value)
				{
					GetFountainValue();
				}
				base.SN_MaximumValue = value;
			}
		}

		public ZLong DefaultTypeRangeMax => DefaultTypeRangeMaxCore;

		protected virtual ZLong DefaultTypeRangeMaxCore
		{
			get { return Schema.DefaultFountainMaximumValue; }
		}

		#endregion

		public override ZString SN_Name
		{
			get { return base.SN_Name; }
			set
			{
				if (SN_Name != value)
				{
					GetFountainValue();
				}
				base.SN_Name = value;
			}
		}

		public override ZGuid SN_Owner
		{
			get { return base.SN_Owner; }
			set
			{
				if (SN_Owner != value)
				{
					GetFountainValue();
				}
				base.SN_Owner = value;
			}
		}

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public sealed override ZInt SN_ID
		{
			get { return base.SN_ID; }
			set
			{
				ErrorReporter.ReportOnce("ViewStmNums.SN_ID should never be set as it won't be saved");
			}
		}

		[List("Lookups.TypeList")]
		[MaxLength(nameof(SN_Type_MaxLength))]
		[ResourceStringData("ViewStmNums|SN_Type", Caption = "Range Type")]
		public override ZString SN_Type
		{
			get { return base.SN_Type; }
			set
			{
				var oldValue = SN_Type;
				base.SN_Type = value;
				if (!IsCopying && oldValue != SN_Type)
				{
					UpdateMaxiumValueIfNeeded();
					UpdateNameFromTypeAndPrefix();
				}
			}
		}

		protected virtual int SN_Type_MaxLength => 3;
		protected virtual bool SN_Type_ReadOnly => IsInDatabase;

		[ResourceStringData("ViewStmNums|SN_TypeDescription", Caption = "Description")]
		public ZString SN_TypeDescription
		{
			get { return Lookups.TypeList.GetMultilingualDescriptionFromCode(SN_Type); }
		}

		public ZPropertyInfo SN_TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.SN_TypeDescription); }
		}

		[MaxLength(nameof(SN_Prefix_MaxLength))]
		[ReadOnlyMember(nameof(SN_Prefix_ReadOnly))]
		[ResourceStringData("ViewStmNums|SN_Prefix", Caption = "Prefix")]
		public override ZString SN_Prefix
		{
			get { return base.SN_Prefix; }
			set
			{
				if (base.SN_Prefix != value)
				{
					base.SN_Prefix = value;
					UpdateNameFromTypeAndPrefix();
				}
			}
		}

		protected virtual int SN_Prefix_MaxLength => AutoViewStmNums.Schema.SN_PrefixMaxLength;

		protected virtual bool SN_Prefix_ReadOnly
		{
			get { return IsInDatabase; }
		}

		protected void UpdateNameFromTypeAndPrefix()
		{
			if (!IsSetNameAndPrefixInProgress)
			{
				SN_Name = GenerateName();
			}
		}

		internal ZString GenerateName()
		{
			var name = NamePrefix + SN_Type.PadRight(SN_TypeInfo.MaxLength);

			if (!SN_Prefix.IsEmpty)
			{
				name += "_" + SN_Prefix;
			}
			return name;
		}

		protected abstract ZString NamePrefix { get; }

		#endregion

		protected void UpdateMaxiumValueIfNeeded()
		{
			if (IsInDatabase || SN_MaximumValue != CalculatedMaximumValue)
			{
				SN_MaximumValue = DefaultTypeRangeMax;
			}
		}

		public override void OnSaving()
		{
			needsToReloadSN_ID = !IsInDatabase && !IsDeleted;
			base.OnSaving();
#if DEBUG
			if (Globals.IsTest)
			{
				Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess(SN_Name, SN_Owner.IsValid ? SN_Owner.ToGuid() : Guid.Empty);
			}
#endif
		}
		bool needsToReloadSN_ID;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (needsToReloadSN_ID && saveSucceeded)
			{
				var row = ((INeedRow)this).Row;
				if (row.RowState == DataRowState.Unchanged)
				{
					needsToReloadSN_ID = false;
					ZInt id;
					if (TryGetValueFromDB(ViewStmNumsSchema.Constants.SN_ID, out id))
					{
						using (SuspendSettingHasChanges())
						{
							base.SN_ID = id;
							row.AcceptChanges();
							disableHasChanges = true;
						}
					}
				}
			}
		}
		bool disableHasChanges;

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (value && disableHasChanges && !IsDeleted)
				{
					ErrorReporter.ReportOnce("Changes should not be maded to ViewStmNums which was not loaded from DB as the SN_PK and SN_ID will most likely be different to what's stored in DB.");
				}
			}
		}

		protected bool TryGetValueFromDB<T>(string columnName, out T value)
			where T : IZType
		{
			var result = false;
			value = default(T);
			var collection = new DynamicBusinessObjectCollection(Factory);
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@Owner", SN_Owner, ViewStmNumsSchema.SN_Owner);
			sqlParams.Add("@Name", SN_Name, ViewStmNumsSchema.SN_Name);
			collection.Load(string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 {0} FROM dbo.StmNums WHERE {1} = @Owner AND {2} = @Name;", columnName, ViewStmNumsSchema.Constants.SN_Owner, ViewStmNumsSchema.Constants.SN_Name), sqlParams);
			if (collection.Count == 1)
			{
				value = (T)collection[0][columnName];
				result = true;
			}

			return result;
		}

		#region Related Business Objects

		public virtual BusinessObject Owner
		{
			get { return null; }
		}

		#endregion

		#region Number Fountain

		public INumberFountainProxy TryGetNumberFountain()
		{
			INumberFountainProxy result = null;
			if (IsInDatabase && !HasChanges)
			{
				var owner = Owner;
				if (owner != null)
				{
					var fountainFactory = GetNumberFountainFactory(owner);
					if (fountainFactory != null)
					{
						var fountain = fountainFactory.New();
						result = fountain.Wrap();
					}
				}
			}
			return result;
		}

		protected abstract NumberFountain.FormattedNumberFountainFactory GetNumberFountainFactory(BusinessObject ownerBizObj);

		#endregion
	}
}
