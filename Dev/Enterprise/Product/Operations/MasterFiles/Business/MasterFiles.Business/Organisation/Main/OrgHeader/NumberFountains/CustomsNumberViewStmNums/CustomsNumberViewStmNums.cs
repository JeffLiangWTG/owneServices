using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNums : ViewStmNums
	{
		public CustomsNumberViewStmNums(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : ViewStmNums.Schema
		{
			public const string SN_NamePrefix = "C#";

			public const string SN_AvailableNumbers = "SN_AvailableNumbers";
			public const string SN_FountainName = "SN_FountainName";
			public const string SN_OwnerForDisplay = "SN_OwnerForDisplay";
			public const string Sequence = "Sequence";

			public const int ProviderKeyLength = 2;
			public const char NamePrefixSeparator = '-';
			public const int SN_FountainNameMaxLength = 26;
			public const char SequenceSeparator = '|';
			public new const int SN_PrefixMaxLength = 30;
			public new const int SN_TypeMaxLength = 4;
		}

		#endregion

		#region public interface

		public static ZString GenerateNamePrefix(ZString providerKey)
		{
			return Schema.SN_NamePrefix + providerKey.Left(Schema.ProviderKeyLength).PadRight(Schema.ProviderKeyLength) + Schema.NamePrefixSeparator;
		}

		public static ZString GeneratePrefix(ZString fountainName, ZInt? sequence)
		{
			var result = fountainName;
			if (sequence.HasValue)
			{
				result += Schema.SequenceSeparator + GetLetterRepresentation(sequence.Value);
			}
			return result;
		}

		public bool IsNumberFountainValid
		{
			get { return TryGetNumberFountain() != null; }
		}

		public bool IsValid => !SN_Owner.IsEmpty && !SN_Type.IsEmpty && !SN_FountainName.IsEmpty;

		public CustomsNumberViewStmNumsWrapper Wrapper => Provider?.GetOrCreateWrapper(this);

		public CustomsNumberViewStmNumsBusinessProvider Provider
		{
			get;
			set;
		}

		public ZString ProviderKey => Provider?.ProviderKey ?? ZString.Empty;

		public CustomsNumberViewStmNumsSetting Setting
		{
			get
			{
				if (setting == null || setting.RangeType != SN_Type)
				{
					setting = Provider?.GetSetting(SN_Type);
				}
				return setting;
			}
		}
		CustomsNumberViewStmNumsSetting setting;

		#endregion

		#region Properties

		#region SN_AvailableNumbers

		[ResourceStringData("CustomsNumberViewStmNums|SN_AvailableNumbers", Caption = "Available Numbers", ShortCaption = "Available")]
		public ZLong SN_AvailableNumbers
		{
			get
			{
				var result = ZLong.Zero;
				if (IsInDatabase)
				{
					var valueForDisplay = SN_ValueForDisplay;
					if (valueForDisplay > ZLong.Zero && valueForDisplay <= SN_MaximumValue)
					{
						result = Math.Max(SN_MaximumValue - valueForDisplay + 1, 0);
					}
				}
				else if (SN_MaximumValue > ZLong.Zero && SN_MinimumValue > ZLong.Zero)
				{
					result = Math.Max(SN_MaximumValue - SN_MinimumValue + 1, 0);
				}
				return result;
			}
		}

		public ZPropertyInfo SN_AvailableNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.SN_AvailableNumbers); }
		}

		#endregion

		#region Sequence

		public ZInt Sequence
		{
			get { return sequence; }
			set
			{
				if (value > 0)
				{
					var oldValue = Sequence;
					if (SetNonPersistentPropertyValue(SequenceInfo, ref sequence, value) && !IsCopying && oldValue != Sequence)
					{
						UpdatePrefix();
					}
				}
			}
		}
		ZInt sequence;

		public ZPropertyInfo SequenceInfo
		{
			get { return GetZPropertyInfo(Schema.Sequence); }
		}

		#endregion

		#region SN_Prefix

		protected override bool SN_Prefix_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region SN_Name

		public ZString SN_NameWithoutSequence
		{
			get
			{
				if (!nameWithoutSequenceCached.HasValue)
				{
					var sequenceSeparator = SN_Name.LastIndexOf(Schema.SequenceSeparator);
					nameWithoutSequenceCached = sequenceSeparator > 1 ? SN_Name.Left(sequenceSeparator) : SN_Name;
				}
				return nameWithoutSequenceCached.Value;
			}
		}
		ZString? nameWithoutSequenceCached;

		[ReadOnly(true)]
		public override ZString SN_Name
		{
			get { return base.SN_Name; }
			set
			{
				base.SN_Name = value;
				nameWithoutSequenceCached = null;
			}
		}

		#endregion

		#region SN_OwnerForDisplay

		public ZString SN_OwnerForDisplay => Provider?.GetOwnerForDisplay(Owner) ?? ZString.Empty;

		public ZPropertyInfo SN_OwnerForDisplayInfo
		{
			get { return GetZPropertyInfo(Schema.SN_OwnerForDisplay); }
		}

		#endregion

		#region SN_Owner

		[List("Lookups.OwnerCollection")]
		public override ZGuid SN_Owner
		{
			get { return base.SN_Owner; }
			set
			{
				var oldValue = SN_Owner;
				var oldOwnerForDisplay = SN_OwnerForDisplay;
				if (!IsCopying && oldValue != value)
				{
					base.SN_Owner = value;
					Setting?.DefaultDataOnSettingOwner(this);
					SN_OwnerForDisplayInfo.RefreshBinding(oldOwnerForDisplay);
				}
			}
		}

		public override BusinessObject Owner
		{
			get
			{
				var pk = SN_Owner;
				if (fOwner == null || fOwner.IsDeleted || fOwner.PK != pk)
				{
					fOwner = null;
					if (pk.IsValid)
					{
						fOwner = Provider?.GetOwner(Factory, pk);
					}
				}
				return fOwner;
			}
		}
		BusinessObject fOwner;

		#endregion

		#region SN_FountainName

		[ReadOnlyMember(nameof(SN_FountainName_ReadOnly))]
		[MaxLength(Schema.SN_FountainNameMaxLength)]
		public virtual ZString SN_FountainName
		{
			get { return fountainName; }
			set
			{
				if (SetNonPersistentPropertyValue(SN_FountainNameInfo, ref fountainName, value))
				{
					UpdatePrefix();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateSN_FountainName();
				}
			}
		}
		ZString fountainName;

		public ZPropertyInfo SN_FountainNameInfo
		{
			get { return GetZPropertyInfo(Schema.SN_FountainName); }
		}

		protected bool SN_FountainName_ReadOnly => IsInDatabase;

		#endregion

		#endregion

		#region Methods

		public new CustomsNumberViewStmNumsValidation Validation => (CustomsNumberViewStmNumsValidation)base.Validation;

		protected sealed override ViewStmNumsValidation GetNewValidation()
		{
			return Provider?.GetNewValidation(this) ?? new CustomsNumberViewStmNumsValidation(this);
		}

		protected sealed override bool IsLookupsCachedInBase => false;

		public new CustomsNumberViewStmNumsLookups Lookups => (CustomsNumberViewStmNumsLookups)base.Lookups;

		protected sealed override ViewStmNumsLookups GetNewLookups()
		{
			return Provider?.GetNewLookups(this) ?? new CustomsNumberViewStmNumsLookups(this);
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				if (Sequence.IsEmpty)
				{
					Sequence = GetNextSequence();
				}
				if (IsValid && GetNumberRanges().Length == 0)
				{
					CreateNumberRange();
				}
				if (!SN_CanRollover)
				{
					SN_CanRollover = Setting?.CanRollover() ?? false;
				}
			}
			base.OnSaving();
		}

		public CustomsNumberViewStmNums[] GetAllMatchingNameAndOwner(bool relatedOnly = false)
		{
			return GetAllMatchingNameAndOwner(new[] { SN_Owner }, relatedOnly);
		}

		public CustomsNumberViewStmNums[] GetAllMatchingNameAndOwner(IEnumerable<ZGuid> ownerPKs, bool relatedOnly = false)
		{
			CustomsNumberViewStmNums[] result = null;
			if (!SN_Name.IsEmpty)
			{
				var query = new ZQuery(ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, SN_NameWithoutSequence + Schema.SequenceSeparator);
				query.AddToFilter(ViewStmNumsSchema.SN_Owner, ownerPKs);
				if (SN_ID.IsEmpty)
				{
					query.AddToFilter(ViewStmNumsSchema.PK, SQLComparisonOperator.NotEqual, PK);
				}
				else
				{
					query.AddToFilter(ViewStmNumsSchema.SN_ID, SQLComparisonOperator.NotEqual, SN_ID);
				}
				result = CustomsNumberViewStmNumsHelper.LoadStmNums(Factory, query);
				if (!relatedOnly)
				{
					result = result.Concat(new[] { this }).ToArray();
				}
			}
			return result ?? (Array.Empty<CustomsNumberViewStmNums>());
		}

		public override void Delete()
		{
			if (GetAllMatchingNameAndOwner(true).Length == 0)
			{
				GetNumberRanges().DeleteAll();
			}
			base.Delete();
		}

		protected override void OnUpdatedByDataRefresh()
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				base.OnUpdatedByDataRefresh();
				InitializeCount();
				if (Provider != null)
				{
					var wrapper = (CustomsNumberViewStmNumsWrapper)Provider.CustomsNumberWrappers.FindByPK(PK);
					wrapper.InitializeData();
					wrapper.RefreshBinding();
				}
			}
		}

		public string Detail => GetDetail();
		protected virtual string GetDetail() => ResString.GetMultilingualString("{EA431074-93A9-421F-93B7-813E0F6D4661}", "Range '{0}' (Start:{1}, End:{2})", SN_OwnerForDisplay, SN_MinimumValue, SN_MaximumValue);

		public bool IsNumberUsed(ZString number)
		{
			return Setting?.IsNumberUsed(this, number) ?? false;
		}

		public ZString GenerateNextCustomsNumber(BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			var numberFountain = TryGetNumberFountain();
			if (numberFountain != null)
			{
				try
				{
					result = GenerateCustomsNumber(numberFountain.GetNext(factory));
				}
				catch (NumberFountainMaximumValueReachedException)
				{
					// do nothing
				}
			}
			return result;
		}

		public ZString GenerateCustomsNumber(long number)
		{
			var result = ZString.Empty;
			if (number > 0)
			{
				result = number.ToString(CultureInfo.InvariantCulture).PadLeft(RequiredDigit(), '0');
				var currentSetting = Setting;
				if (currentSetting != null)
				{
					result = currentSetting.GenerateCustomsNumber(this, result);
				}
			}
			return result;
		}

		public bool SetNextNumber(ZLong number)
		{
			var result = false;
			var factory = new BusinessObjectFactory();
			var connection = ((IDbConnected)factory).Connection;
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				if (SetNextNumber(factory, number))
				{
					transactionManager.CommitTransaction();
					result = true;
				}
			}
			return result;
		}

		public bool SetNextNumber(BusinessObjectFactory factory, ZLong number)
		{
			var result = false;
			long numberLong = (long)number; //ensure valid long
			var numberFountain = TryGetNumberFountain();
			if (numberFountain != null && numberLong != ZLong.Zero)
			{
				numberFountain.SetNext(factory, numberLong);
				result = true;
			}
			return result;
		}

		#endregion

		#region HasCustomsNumberStmNumberRange

		public bool HasReachedLimit => FirstNumberRange?.HasReachedLimit ?? false;
		public ZLong TotalAvailableNumbers => FirstNumberRange?.TotalAvailableNumbers ?? ZLong.Zero;
		public CustomsNumberStmNumberRange FirstNumberRange => GetNumberRanges().FirstOrDefault();

		public CustomsNumberStmNumberRange[] GetNumberRanges()
		{
			if (numberRangesCached == null)
			{
				numberRangesCached = new CachedProperty<CustomsNumberStmNumberRange[]>(Factory, () =>
				{
					CustomsNumberStmNumberRange[] result = null;
					if (IsValid)
					{
						var query = new ZQuery(StmNumberRangeSchema.SNR_Owner, SN_Owner);
						query.AddToFilter(StmNumberRangeSchema.SNR_Name, SN_NameWithoutSequence);
						result = CustomsNumberViewStmNumsHelper.LoadStmNumRanges(Factory, query).OrderBy(x => x.PK).ToArray();
					}
					return result ?? Array.Empty<CustomsNumberStmNumberRange>();
				});
			}
			return numberRangesCached.Value;
		}
		CachedProperty<CustomsNumberStmNumberRange[]> numberRangesCached;

		#endregion

		#region Implementation

		protected override int SN_Type_MaxLength => CustomsNumberViewStmNums.Schema.SN_TypeMaxLength;
		protected override int SN_Prefix_MaxLength => CustomsNumberViewStmNums.Schema.SN_PrefixMaxLength;

		const string AlphanumericCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const int MaximumSequence = 46658;
		const string MaximumSequenceLetterRepresentation = "000";
		const int SecondMaximumSequence = 46657;
		const string SecondMaximumSequenceLetterRepresentation = "00";
		const int ThirdMaximumSequence = 46656;
		const string ThirdMaximumSequenceLetterRepresentation = "0";
		const int Length = 36;
		const int MaxEncodedLength = 3;

		static ZString GetLetterRepresentation(int number)
		{
			var result = ZString.Empty;
			if (number > 0 && number <= MaximumSequence)
			{
				switch (number)
				{
					case MaximumSequence:
						result = MaximumSequenceLetterRepresentation;
						break;
					case SecondMaximumSequence:
						result = SecondMaximumSequenceLetterRepresentation;
						break;
					case ThirdMaximumSequence:
						result = ThirdMaximumSequenceLetterRepresentation;
						break;
					default:
						var buffer = new char[MaxEncodedLength];
						int startIndex = MaxEncodedLength;
						do
						{
							buffer[--startIndex] = AlphanumericCharacters[number % Length];
							number /= Length;
						} while (number > 0 && startIndex > 0);

						result = new string(buffer, startIndex, MaxEncodedLength - startIndex);
						break;
				}
			}
			return result;
		}

		static int GetNumberRepresentation(ZString value)
		{
			var entry = value.Trim();
			var result = 0;

			if (entry.Length > 0 && entry.Length <= MaxEncodedLength)
			{
				switch (entry)
				{
					case ThirdMaximumSequenceLetterRepresentation:
						result = ThirdMaximumSequence;
						break;
					case SecondMaximumSequenceLetterRepresentation:
						result = SecondMaximumSequence;
						break;
					case MaximumSequenceLetterRepresentation:
						result = MaximumSequence;
						break;
					default:
						int working = 0;
						for (int i = 0; i < entry.Length; ++i)
						{
							int digit = AlphanumericCharacters.IndexOf(entry[i]);
							if (digit == -1)
							{
								working = -1;
								break;
							}

							working = working * Length + digit;
						}
						result = working;
						break;
				}
			}
			return result;
		}

		ZInt GetNextSequence()
		{
			var result = ZInt.Zero;
			var query = new ZQuery(ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, SN_NameWithoutSequence);
			query.AddToFilter(ViewStmNumsSchema.SN_Owner, SN_Owner);
			query.AddToFilter(ViewStmNumsSchema.PK, SQLComparisonOperator.NotEqual, PK);
			query.FetchOnlyFromLocalCache = !(Owner?.IsInDatabase ?? false);
			var stmNums = CustomsNumberViewStmNumsHelper.LoadStmNums(Factory, query) ?? Array.Empty<CustomsNumberViewStmNums>();
			var maxSeq = stmNums.Length == 0 ? ZInt.Zero : stmNums.Max(x => x.Sequence);
			if (maxSeq < MaximumSequence)
			{
				result = maxSeq + 1;
			}
			return result;
		}

		CustomsNumberStmNumberRange CreateNumberRange()
		{
			var numberRange = CustomsNumberViewStmNumsHelper.NewStmNumRange(Factory, Provider, SN_Owner);
			numberRange.SNR_Name = SN_NameWithoutSequence;
			numberRange.SNR_ThresholdRunOutWarning = Setting?.GetThresholdRunOutWarning() ?? ZLong.Zero;
			return numberRange;
		}

		protected override FormattedNumberFountainFactory GetNumberFountainFactory(BusinessObject ownerBizObj)
		{
			FormattedNumberFountainFactory fountainFactory = null;
			if (Lookups.TypeList.ContainsCode(SN_Type))
			{
				fountainFactory = new FormattedNumberFountainFactory(SN_Name, ownerBizObj.PK.ToGuid(), GetPrefix(), SN_CanRollover, (long)SN_MinimumValue, (long)SN_MaximumValue, RequiredDigit());
			}
			return fountainFactory;
		}

		string GetPrefix()
		{
			return Setting?.GetPrefix() ?? string.Empty;
		}

		int RequiredDigit()
		{
			var result = Setting?.RequiredDigit() ?? 0;
			return result == 0 ? ((long)SN_MaximumValue).ToString(Culture.Invariant).Length : result;
		}

		protected override ZLong DefaultTypeRangeMaxCore
		{
			get
			{
				var result = Setting?.DefaultTypeRangeMax() ?? ZLong.Zero;
				return result.IsEmpty ? base.DefaultTypeRangeMaxCore : result;
			}
		}

		protected override ZString NamePrefix => GenerateNamePrefix(ProviderKey);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SN_Name = NamePrefix;
			SN_Value = SN_MinimumValue;
		}

		protected override void InitializeTypeAndPrefixCore()
		{
			var sequenceSeparator = SN_Prefix.LastIndexOf(Schema.SequenceSeparator);
			if (sequenceSeparator > -1)
			{
				SN_FountainName = SN_Prefix.Left(sequenceSeparator).Left(Schema.SN_FountainNameMaxLength);
				Sequence = GetNumberRepresentation(SN_Prefix.SubstringSafe(sequenceSeparator + 1));
			}
			else
			{
				SN_FountainName = SN_Prefix.Left(Schema.SN_FountainNameMaxLength);
			}
		}

		void UpdatePrefix()
		{
			if (!IsSetNameAndPrefixInProgress)
			{
				SN_Prefix = GeneratePrefix(SN_FountainName, Sequence);
			}
		}

		#endregion
	}
}
