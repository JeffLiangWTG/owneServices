using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class TWCustomsNumberViewStmNumsWrapper : CustomsNumberViewStmNumsCompanyWrapper
	{
		public TWCustomsNumberViewStmNumsWrapper(CustomsNumberViewStmNums stmNums)
			: base(stmNums)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : CustomsNumberViewStmNumsCompanyWrapper.Schema
		{
			public const string MessageType = "MessageType";
			public const string RangeType = "RangeType";
			public const string RangeTypeDescription = "RangeTypeDescription";
			public const string Count = "Count";
			public const string CurrentValue = "CurrentValue";
			public const string StartNumber = "StartNumber";
			public const string EndNumber = "EndNumber";
			public const char CheckSeparator = '_';
			public const string PreFountainName = "TWEntryNum";

			public const int MessageTypeMaxLength = 3;
			public const int RangeTypeMaxLength = 3;
			public const int StartNumberMaxLength = 7;
			public const int EndNumberMaxLength = 7;
			public const int CurrentValueMaxLength = 7;
		}

		#region MessageType

		[ReadOnlyMember(nameof(MessageType_ReadOnly))]
		[MaxLength(Schema.MessageTypeMaxLength)]
		[List(nameof(MessageTypeList))]
		[ResourceStringData("TWCustomsNumberViewStmNumsWrapper|MessageType", Caption = "Message Type")]
		public ZString MessageType
		{
			get => fMessageType;
			set
			{
				var oldValue = MessageType;
				if (SetNonPersistentPropertyValue(MessageTypeInfo, ref fMessageType, value))
				{
					UpdateSN_FountainName();
				}
				if (oldValue != MessageType && MessageType == CustomsNumberMessageTypeList.Codes.Transhipment)
				{
					RangeType = RangeTypeList.Codes.T;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateMessageType();
				}
				MessageTypeInfo.RefreshBinding();
			}
		}
		ZString fMessageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(Schema.MessageType);

		bool MessageType_ReadOnly => StmNums.IsInDatabase;

		public CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<CustomsNumberMessageTypeList>();
		#endregion

		#region StartNumber
		[BusinessObjectTestExclude]
		[MaxLength(Schema.StartNumberMaxLength)]
		[ResourceStringData("TWCustomsNumberViewStmNumsWrapper|StartNumber", Caption = "Start Number", ShortCaption = "Start")]
		public ZString StartNumber
		{
			get
			{
				return DisplayNumberToString(SN_MinimumValue, fStartNumber);
			}
			set
			{
				CheckMaximumLength(StartNumberInfo, value);
				fStartNumber = value;
				var maximumValue = SN_MaximumValue;
				SN_MinimumValue = Sequenceformatter?.FormatStringToInt(fStartNumber) ?? ZInt.Zero;
				SN_MaximumValue = maximumValue;
				UpdateCount();
				if (!IsValidationSuspended)
				{
					Validation.ValidateStartNumber();
					Validation.ValidateRangeType();
				}
				fCurrentValueString = DisplayNumberToString(SN_Value, ZString.Empty);
				StartNumberInfo.RefreshBinding();
				CurrentValueInfo.RefreshBinding();
			}
		}
		ZString fStartNumber;

		public ZPropertyInfo StartNumberInfo => GetZPropertyInfo(Schema.StartNumber);
		#endregion

		#region EndNumber
		[BusinessObjectTestExclude]
		[MaxLength(Schema.EndNumberMaxLength)]
		[ResourceStringData("TWCustomsNumberViewStmNumsWrapper|EndNumber", Caption = "End Number", ShortCaption = "End")]
		public ZString EndNumber
		{
			get
			{
				return DisplayNumberToString(SN_MaximumValue, fEndNumber);
			}
			set
			{
				CheckMaximumLength(EndNumberInfo, value);
				fEndNumber = value;
				SN_MaximumValue = Sequenceformatter?.FormatStringToInt(fEndNumber) ?? ZInt.Zero;
				UpdateCount();
				if (!IsValidationSuspended)
				{
					Validation.ValidateEndNumber();
					Validation.ValidateRangeType();
				}
				EndNumberInfo.RefreshBinding();
			}
		}

		ZString fEndNumber;

		public ZPropertyInfo EndNumberInfo => GetZPropertyInfo(Schema.EndNumber);
		#endregion

		#region CurrentValue
		[BusinessObjectTestExclude]
		[MaxLength(Schema.CurrentValueMaxLength)]
		[ResourceStringData("TWCustomsNumberViewStmNumsWrapper|CurrentValue", Caption = "Current Value")]
		public ZString CurrentValue
		{
			get
			{
				return fCurrentValueString ?? DisplayNumberToString(SN_ValueForDisplay, ZString.Empty);
			}
			set
			{
				CheckMaximumLength(CurrentValueInfo, value);
				fCurrentValueString = value;
				StmNums.SN_Value = Sequenceformatter?.FormatStringToInt(fCurrentValueString) ?? ZInt.Zero;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCurrentValue();
				}
				CurrentValueInfo.RefreshBinding();
			}
		}
		ZString? fCurrentValueString;

		public ZPropertyInfo CurrentValueInfo => GetZPropertyInfo(Schema.CurrentValue);
		#endregion

		#region Count
		[ResourceStringData("TWCustomsNumberViewStmNumsWrapper|Count", Caption = "Count")]
		public ZLong Count => SN_Count;

		public ZPropertyInfo CountInfo => GetZPropertyInfo(Schema.Count);

		void UpdateCount()
		{
			SN_Count = SN_MaximumValue - SN_MinimumValue + 1;
		}
		#endregion

		#region RangeType
		[ReadOnlyMember(nameof(RangeType_ReadOnly))]
		[MaxLength(Schema.RangeTypeMaxLength)]
		[List(nameof(RangeTypeCodeList))]
		[ResourceStringData("TWCustomsNumberViewStmNumsWrapper|RangeType", Caption = "Range Type")]
		public ZString RangeType
		{
			get => fRangeType;
			set
			{
				var oldValue = RangeType;
				if (SetNonPersistentPropertyValue(RangeTypeInfo, ref fRangeType, value))
				{
					UpdateSN_FountainName();
				}
				if (oldValue != RangeType)
				{
					fSequenceformatter = null;
					SN_MinimumValue = 1L;
					SN_MaximumValue = Sequenceformatter?.MaximumValue ?? ZLong.Zero;
					UpdateCount();
					RefreshNumber();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateRangeType();
				}
				RangeTypeInfo.RefreshBinding();
			}
		}
		ZString fRangeType;

		internal void AddAdditionalRangeTypeValidation()
		{
			var rangeTypeInfo = StmNums.SN_MinimumValueInfo;
			if (rangeTypeInfo.HasErrors())
			{
				RangeTypeInfo.AddAllNotificationsFrom(rangeTypeInfo);
			}
			StmNums.Validation.ValidateSN_Type();
			rangeTypeInfo = StmNums.SN_TypeInfo;
			if (rangeTypeInfo.HasErrors())
			{
				RangeTypeInfo.AddAllNotificationsFrom(rangeTypeInfo);
			}
		}

		public ZPropertyInfo RangeTypeInfo => GetZPropertyInfo(Schema.RangeType);

		bool RangeType_ReadOnly => StmNums.IsInDatabase || MessageType == CustomsNumberMessageTypeList.Codes.Transhipment;

		public CodeDescriptionPairList RangeTypeCodeList => Factory.GetCachedValue(ZString.Format("TWCustomsNumberViewStmNumsWrapper|RangeTypeList|{0}", MessageType),
			() =>
		{
			CodeDescriptionPairList list;
			if (MessageType == CustomsNumberMessageTypeList.Codes.Transhipment)
			{
				list = new CodeDescriptionPairList();
				list.AddPair(RangeTypeList.Codes.T, RangeTypeList.Descriptions.T);
			}
			else
			{
				list = new RangeTypeList();
				list.RemoveCode(RangeTypeList.Codes.T);
			}
			return list;
		});
		#endregion

		#region RangeTypeDescription
		[ResourceStringData("TWCustomsNumberViewStmNumsWrapper|RangeTypeDescription", Caption = "Range Type Description")]
		public ZString RangeTypeDescription => RangeTypeCodeList.GetDescriptionFromCode(RangeType);

		public ZPropertyInfo RangeTypeDescriptionInfo => GetZPropertyInfo(Schema.RangeTypeDescription);
		#endregion

		public ZLong SN_Value => StmNums.SN_Value;

		public new TWCustomsNumberViewStmNumsWrapperValidation Validation => (TWCustomsNumberViewStmNumsWrapperValidation)base.Validation;

		public override CustomsNumberViewStmNumsWrapperValidation GetNewValidation()
		{
			return new TWCustomsNumberViewStmNumsWrapperValidation(this);
		}

		#region Implementation
		internal ITWSequenceformatter Sequenceformatter => fSequenceformatter ?? (fSequenceformatter = BaseTWSequenceformatter.New(RangeType));
		ITWSequenceformatter fSequenceformatter;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && !IsDeleted && !StmNums.IsDeleted && SN_Value != oldSN_Value)
			{
				StmNums.SetNextNumber(SN_Value);
			}
		}

		void RefreshNumber()
		{
			fEndNumber = null;
			fStartNumber = null;
			fCurrentValueString = null;
			StartNumberInfo.RefreshBinding();
			EndNumberInfo.RefreshBinding();
			CurrentValueInfo.RefreshBinding();
		}

		ZString DisplayNumberToString(ZLong val, ZString defaultString)
		{
			var result = defaultString;
			var formatter = Sequenceformatter;
			if (val >= ZLong.Zero && formatter != null && val <= formatter.MaximumValue)
			{
				result = formatter.FormatIntToString(val.ToZInt()) ?? ZString.Empty;
			}
			return result;
		}

		(ZString, ZString) ExtraDataFromStmNumsName()
		{
			var messageType = ZString.Empty;
			var rangeType = ZString.Empty;
			var appliesToPortion = StmNums.SN_FountainName.ToUpper();
			var datas = appliesToPortion.Split(new char[] { Schema.CheckSeparator });
			if (datas.Length > 1)
			{
				messageType = datas[1].Left(Schema.MessageTypeMaxLength).TrimEnd();
			}
			if (datas.Length > 2)
			{
				rangeType = datas[2].Left(Schema.RangeTypeMaxLength).TrimEnd();
			}
			return (messageType, rangeType);
		}

		ZLong oldSN_Value;
		protected override void InitializeDataCore()
		{
			if (StmNums.SN_Type.IsEmpty)
			{
				StmNums.SN_Type = BaseEntryNumberGenerator.CustomsStmNumsType;
			}
			oldSN_Value = SN_Value;
			LoadFromSN_FountainName();
			StmNums.SN_FountainNameInfo.ValueChanged -= SN_FountainNameInfo_ValueChanged;
			StmNums.SN_FountainNameInfo.ValueChanged += SN_FountainNameInfo_ValueChanged;
		}

		void SN_FountainNameInfo_ValueChanged(object sender, System.EventArgs e)
		{
			var ve = e as ValueChangedEventArgs;
			if (ve != null && ve.OldValue != ve.NewValue)
			{
				LoadFromSN_FountainName();
			}
		}

		void LoadFromSN_FountainName()
		{
			if (!updatingSN_FountainNameInProgress)
			{
				try
				{
					loadingFromSN_FountainNameInProgress = true;
					var extraData = ExtraDataFromStmNumsName();
					fMessageType = extraData.Item1;
					fRangeType = extraData.Item2;
				}
				finally
				{
					loadingFromSN_FountainNameInProgress = false;
				}
			}
		}
		bool loadingFromSN_FountainNameInProgress;

		void UpdateSN_FountainName()
		{
			if (!loadingFromSN_FountainNameInProgress)
			{
				try
				{
					updatingSN_FountainNameInProgress = true;
					UpdateFountainName();
				}
				finally
				{
					updatingSN_FountainNameInProgress = false;
				}
			}
		}
		bool updatingSN_FountainNameInProgress;

		void UpdateFountainName()
		{
			if (!StmNums.IsSetNameAndPrefixInProgress)
			{
				SN_FountainName = ZString.Format("{0}_{1}_{2}", Schema.PreFountainName, MessageType, RangeType);
			}
		}

		internal BaseEntryNumberGenerator EntryNumberGenerator { get; set; }

		internal bool ExistingEntry(string number) => EntryNumberGenerator?.ExistingEntry(number) ?? false;
		#endregion
	}
}
