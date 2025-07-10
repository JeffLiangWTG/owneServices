using System;
using System.ComponentModel;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USCustomsNumberViewStmNumsWrapper : CustomsNumberViewStmNumsCompanyWrapper
	{
		public USCustomsNumberViewStmNumsWrapper(CustomsNumberViewStmNums stmNums)
			: base(stmNums)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : CustomsNumberViewStmNumsCompanyWrapper.Schema
		{
			public const string AppliesTo = "AppliesTo";
			public const string CheckDigitAddition = "CheckDigitAddition";

			public const int AppliesToMaxLength = 3;
			public const char CheckDigitAdditionSeparator = ':';
		}

		public override ZString SN_Type
		{
			get => base.SN_Type;
			set
			{
				var oldValue = SN_Type;
				base.SN_Type = value;
				if (!IsCopying && oldValue != SN_Type)
				{
					PopulateAppliesTo();
					if (!IsCustomsEntry)
					{
						CheckDigitAddition = ZByte.Zero;
					}
				}
			}
		}

		public override ZGuid SN_Owner
		{
			get => base.SN_Owner;
			set
			{
				var oldValue = SN_Owner;
				base.SN_Owner = value;
				if (!IsCopying && oldValue != SN_Owner)
				{
					PopulateAppliesTo();
				}
			}
		}

		#region AppliesTo

		[ReadOnly(true)]
		[MaxLength(Schema.AppliesToMaxLength)]
		[ResourceStringData("USCustomsNumberViewStmNumsWrapper|AppliesTo", Caption = "Applies To")]
		public ZString AppliesTo
		{
			get => appliesTo;
			set
			{
				if (SetNonPersistentPropertyValue(AppliesToInfo, ref appliesTo, value))
				{
					UpdateSN_FountainName();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAppliesTo();
				}
			}
		}
		ZString appliesTo;

		public ZPropertyInfo AppliesToInfo
		{
			get { return GetZPropertyInfo(Schema.AppliesTo); }
		}

		#endregion

		#region CheckDigitAddition

		[ReadOnlyMember(nameof(CheckDigitAddition_ReadOnly))]
		[ResourceStringData("USCustomsNumberViewStmNumsWrapper|CheckDigitAddition", Caption = "Check Digit Addition", ShortCaption = "Check Digit")]
		[BusinessObjectTestExclude]
		public ZByte CheckDigitAddition
		{
			get => checkDigitAddition;
			set
			{
				value = Math.Max(ZByte.Zero, Math.Min((ZByte)9, value));
				if (SetNonPersistentPropertyValue(CheckDigitAdditionInfo, ref checkDigitAddition, value))
				{
					UpdateSN_FountainName();
				}
			}
		}
		ZByte checkDigitAddition;

		public ZPropertyInfo CheckDigitAdditionInfo
		{
			get { return GetZPropertyInfo(Schema.CheckDigitAddition); }
		}

		bool CheckDigitAddition_ReadOnly => StmNums.IsInDatabase || !IsCustomsEntry;

		#endregion

		public static ZString GenerateFountainName(ZString appliesTo, ZByte checkDigit)
		{
			return appliesTo.Left(Schema.AppliesToMaxLength).PadRight(Schema.AppliesToMaxLength) + Schema.CheckDigitAdditionSeparator + new ZString(checkDigit.ToString()).Left(1).PadLeft(1);
		}

		public bool IsCustomsEntry => StmNums.SN_Type == NumberRangeTypeList.Codes.CustomsEntry;
		public bool IsInbond => StmNums.SN_Type == NumberRangeTypeList.Codes.InBond;

		public new USCustomsNumberViewStmNumsWrapperValidation Validation => (USCustomsNumberViewStmNumsWrapperValidation)base.Validation;

		public override CustomsNumberViewStmNumsWrapperValidation GetNewValidation()
		{
			return new USCustomsNumberViewStmNumsWrapperValidation(this);
		}

		public ZString GetCheckDigit(ZString number)
		{
			var result = ZString.Empty;
			switch (SN_Type)
			{
				case NumberRangeTypeList.Codes.CustomsEntry:
					result = EntryNumberCheckDigitCalculator.GetCheckDigit(AppliesTo, number, (int)CheckDigitAddition).ToString(CultureInfo.InvariantCulture);
					break;
				case NumberRangeTypeList.Codes.InBond:
					result = InBondNumberCheckDigitCalculator.GetCheckDigit(number);
					break;
			}
			return result;
		}

		#region Implementation

		void PopulateAppliesTo()
		{
			ZGuid companyPK;
			var ownerPK = StmNums.SN_Owner.IsValid ? StmNums.SN_Owner : GlbCompany.CurrentCompany.PK;
			var company = Factory.Load<GlbCompany>(ownerPK);
			if (company == null)
			{
				companyPK = Factory.Load<GlbBranch>(ownerPK)?.GB_GC ?? ZGuid.Empty;
			}
			else
			{
				companyPK = ownerPK;
			}
			AppliesTo = IsCustomsEntry ? USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode : ZString.Empty;
		}

		protected override void InitializeDataCore()
		{
			LoadFromSN_FountainName();
			StmNums.SN_FountainNameInfo.ValueChanged -= SN_FountainNameInfo_ValueChanged;
			StmNums.SN_FountainNameInfo.ValueChanged += SN_FountainNameInfo_ValueChanged;
		}

		void SN_FountainNameInfo_ValueChanged(object sender, EventArgs e)
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
					var digit = ZByte.Zero;
					var applies = ZString.Empty;
					ExtraDataFromFountainName(StmNums.SN_FountainName, out applies, out digit);
					CheckDigitAddition = digit;
					AppliesTo = applies;
				}
				finally
				{
					loadingFromSN_FountainNameInProgress = false;
				}
			}
		}
		bool loadingFromSN_FountainNameInProgress;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static void ExtraDataFromFountainName(ZString fountainName, out ZString appliesTo, out ZByte digit)
		{
			var appliesToPortion = fountainName.ToUpper();
			var checkDigitPortion = ZString.Empty;
			var checkDigitSeparator = appliesToPortion.IndexOf(Schema.CheckDigitAdditionSeparator);
			if (checkDigitSeparator > 1)
			{
				checkDigitPortion = appliesToPortion.SubstringSafe(checkDigitSeparator + 1);
				appliesToPortion = appliesToPortion.Left(checkDigitSeparator).Left(Schema.AppliesToMaxLength).TrimEnd();
			}

			appliesTo = appliesToPortion.Left(Schema.AppliesToMaxLength).TrimEnd();
			digit = ZByte.ParseSafe(checkDigitPortion.KeepAlphanumericCharacters().Left(1).TrimEnd(), ZByte.Zero);
		}

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
				SN_FountainName = GenerateFountainName(AppliesTo, CheckDigitAddition);
			}
		}
		#endregion
	}
}
