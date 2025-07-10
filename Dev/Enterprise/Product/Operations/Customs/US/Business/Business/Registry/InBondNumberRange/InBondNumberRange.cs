using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class InBondNumberRange : RegistryBusinessObjectTemplate
	{
		#region Schema
		public static class Schema
		{
			public const string StartNumber = "StartNumber";
			public const string LastNumber = "LastNumber";
			public const string BranchPK = "BranchPK";
			public const string CompanyLevelPK = "CompanyLevelPK";
			public const string RunOutWarningLimitNumber = "RunOutWarningLimitNumber";
			public const int NumberMaxLength = 8;
		}
		#endregion

		public bool IsInBondNumberRangeValidationEnabled
		{
			get { return isInBondNumberRangeValidationEnabled ?? true; }
			set { isInBondNumberRangeValidationEnabled = value; }
		}
		bool? isInBondNumberRangeValidationEnabled;

		public Guid BranchPK;
		public Guid CompanyLevelPK;

		public ZString BranchName
		{
			get { return (Branch == null) ? ZString.Empty : (ZString)Branch[GlbBranchSchema.GB_BranchName]; }
		}

		public BusinessObject Branch
		{
			get
			{
				if (branch == null || branch.PK != BranchPK)
				{
					branch = (BusinessObject)CurrentFactory.Load<IGlbBranch>(BranchPK);
				}
				return branch;
			}
		}
		BusinessObject branch;

		public Guid CompanyPK
		{
			get
			{
				Guid result = Guid.Empty;
				if (Branch != null)
				{
					result = ((ZGuid)Branch[GlbBranchSchema.GB_GC]).ToGuid();
				}
				return result;
			}
		}

		#region Bound Properties

		#region StartNumber
		public ZDecimal StartNumber
		{
			get { return startNumber; }
			set
			{
				value = (long)value; //ensure valid long
				SetNonPersistentPropertyValue(StartNumberInfo, ref startNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateStartNumber();
				}
			}
		}
		ZDecimal startNumber;

		public ZPropertyInfo StartNumberInfo
		{
			get { return GetZPropertyInfo(Schema.StartNumber); }
		}

		bool validateStartNumberStarted;
		public void ValidateStartNumber()
		{
			if (!validateStartNumberStarted)
			{
				try
				{
					validateStartNumberStarted = true;
					StartNumberInfo.ClearAllNotifications();
					if (IsInBondNumberRangeValidationEnabled)
					{
						MandatoryValidation.CheckEntered(StartNumberInfo, "Start Number");
						if (!StartNumber.IsEmpty)
						{
							if (BranchRegistryAndCompanyRegistryConflicts())
							{
								StartNumberInfo.AddError(ConflictingCompanyAndBranchRanges);
							}

							if (StartNumber < MinimumNumberRange)
							{
								StartNumberInfo.AddError(StartNumberMustBeGreaterThanOrEqualMinimum + MinimumNumberRange.ToString(CultureInfo.CurrentCulture) + ".");
							}

							long maximumMinusOne = MaximumNumberRange - 1;
							if (StartNumber > maximumMinusOne)
							{
								StartNumberInfo.AddError(StartNumberMustBeLessOrEqualToMaximumMinusOne + maximumMinusOne.ToString(CultureInfo.CurrentCulture) + ".");
							}
							if (EnteringAtBranchLevel)
							{
								ValidateNumberNotOverlappingOtherBranchSettings((ZPropertyInfoDecimal)StartNumberInfo);
							}
						}

						ValidateLastNumber();
					}
				}
				finally
				{
					validateStartNumberStarted = false;
				}
			}
		}
		internal const string StartNumberMustBeGreaterThanOrEqualMinimum = "Start Number must be greater than or equal ";
		internal const string StartNumberMustBeLessOrEqualToMaximumMinusOne = "Start Number must be less or equal to ";
		internal string ConflictingCompanyAndBranchRanges = "In-Bond number range can only be added at the Company level OR at Branch level.\r\nIf you wish to add Branch level number ranges, you need to remove the number range at the Company level first.\r\nIf you want to have the In-Bond number range for the whole company, you need to remove any Branch number ranges currently established.\r\n\r\nNOTE: When removing an existing range at Company level or ranges at Branch level, you need to untick the 'Override Default' check box and 'Save' before entering the range at the alternative fallback level.";
		#endregion

		#region StartNumberForDisplay
		public ZString StartNumberForDisplay
		{
			get
			{
				return ((long)StartNumber).ToString($"D{Schema.NumberMaxLength}");
			}
			set
			{
				value = value.KeepNumericCharacters();
				StartNumber = ZDecimal.ParseSafe(value, 0);
			}
		}

		public ZPropertyInfo StartNumberForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(StartNumberForDisplay), o => StartNumberInfo); }
		}
		#endregion

		#region LastNumber
		public ZDecimal LastNumber
		{
			get { return lastNumber; }
			set
			{
				value = (long)value; //ensure valid long
				SetNonPersistentPropertyValue<ZDecimal>(LastNumberInfo, ref lastNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateLastNumber();
				}
			}
		}
		ZDecimal lastNumber;

		public ZPropertyInfo LastNumberInfo
		{
			get { return GetZPropertyInfo(Schema.LastNumber); }
		}

		bool validateLastNumberStarted;
		public void ValidateLastNumber()
		{
			if (!validateLastNumberStarted)
			{
				try
				{
					validateLastNumberStarted = true;
					LastNumberInfo.ClearAllNotifications();
					if (IsInBondNumberRangeValidationEnabled)
					{
						if (!LastNumber.IsEmpty || StartNumber > 0)
						{
							if (LastNumber <= StartNumber)
							{
								LastNumberInfo.AddError(LastNumberMustBeGreaterThanStartNumber);
							}

							if (LastNumber > MaximumNumberRange)
							{
								LastNumberInfo.AddError(LastNumberMustBeLessOrEqualToMaximum + MaximumNumberRange.ToString(CultureInfo.CurrentCulture));
							}
							ValidateNumberNotOverlappingOtherBranchSettings((ZPropertyInfoDecimal)LastNumberInfo);
						}

						ValidateStartNumber();
					}
				}
				finally
				{
					validateLastNumberStarted = false;
				}
			}
		}
		internal const string LastNumberMustBeGreaterThanStartNumber = "Last Number must be greater than Start Number";
		internal const string LastNumberMustBeLessOrEqualToMaximum = "Last Number must be less or equal to ";
		#endregion

		#region LastNumberForDisplay
		public ZString LastNumberForDisplay
		{
			get
			{
				return ((long)LastNumber).ToString($"D{Schema.NumberMaxLength}");
			}
			set
			{
				value = value.KeepNumericCharacters();
				LastNumber = ZDecimal.ParseSafe(value, 0);
			}
		}

		public ZPropertyInfo LastNumberForDisplayInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LastNumberForDisplay), o => LastNumberInfo); }
		}
		#endregion

		#region RunOutWarningLimitNumber
		public ZDecimal RunOutWarningLimitNumber
		{
			get { return runOutWarningLimitNumber; }
			set
			{
				value = (long)value; //ensure valid long
				SetNonPersistentPropertyValue(RunOutWarningLimitNumberInfo, ref runOutWarningLimitNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateRunOutWarningLimitNumber();
				}
			}
		}
		ZDecimal runOutWarningLimitNumber;

		public ZPropertyInfo RunOutWarningLimitNumberInfo
		{
			get { return GetZPropertyInfo(Schema.RunOutWarningLimitNumber); }
		}

		public void ValidateRunOutWarningLimitNumber()
		{
			RunOutWarningLimitNumberInfo.ClearAllNotifications();
			if (IsInBondNumberRangeValidationEnabled)
			{
				MandatoryValidation.CheckEntered(RunOutWarningLimitNumberInfo, "Run-out warning limit number");
				if (!RunOutWarningLimitNumber.IsEmpty)
				{
					decimal currentRange = LastNumber - StartNumber;
					if (RunOutWarningLimitNumber > currentRange)
					{
						RunOutWarningLimitNumberInfo.AddWarning(RunOutWarningLimitGreaterThanCurrentRange + currentRange.ToString(CultureInfo.CurrentCulture) + ").");
					}
				}
			}
		}
		internal const string RunOutWarningLimitGreaterThanCurrentRange = "Runout Warning Number is more than the range of numbers being established. (";
		#endregion

		#endregion

		public bool IsRangeValidForNumberFountain
		{
			get { return StartNumber > 0 && LastNumber > StartNumber; }
		}

		#region Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			startNumber = 0;
			lastNumber = 0;
			runOutWarningLimitNumber = 0;
		}

		void ValidateNumberNotOverlappingOtherBranchSettings(ZPropertyInfoDecimal info)
		{
			ZDecimal value = info.Value;
			string infoName = info.HumanReadableName;
			foreach (InBondNumberRange otherRange in GetNumberRangeForOtherBranches())
			{
				if (value >= otherRange.StartNumber && value <= otherRange.LastNumber)
				{
					info.AddError(string.Format(CultureInfo.CurrentCulture, NumberOverlappingError, infoName, otherRange.BranchName, otherRange.StartNumber, otherRange.LastNumber));
				}
			}
		}
		internal const string NumberOverlappingError = "This {0} overlaps {1}'s Start Number ({2}) and Last Number ({3}).";

		IEnumerable<InBondNumberRange> GetNumberRangeForOtherBranches()
		{
			if (Branch != null)
			{
				ZQuery branchQuery = new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, Branch.PK);
				branchQuery.AddToFilter(GlbBranchSchema.GB_GC, CompanyPK);
				foreach (BusinessObject otherBranch in CurrentFactory.Load<IGlbBranch>(branchQuery))
				{
					Guid otherBranchPK = otherBranch.PK.ToGuid();
					object proposedValue = ((IRegistryItemInternals)NumberRangeRegistryItem).GetProposedValue(Guid.Empty, otherBranchPK, Guid.Empty);
					yield return proposedValue == null ? NumberRangeRegistryItem.GetValueWithoutFallback(Guid.Empty, otherBranchPK, Guid.Empty) : (InBondNumberRange)proposedValue;
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateStartNumber();
			ValidateLastNumber();
			ValidateRunOutWarningLimitNumber();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BranchPK, BranchPK.ToString());
			writer.WriteElementString(Schema.StartNumber, StartNumber.ToString());
			writer.WriteElementString(Schema.LastNumber, LastNumber.ToString());
			writer.WriteElementString(Schema.CompanyLevelPK, CompanyLevelPK.ToString());
			writer.WriteElementString(Schema.RunOutWarningLimitNumber, RunOutWarningLimitNumber.ToString());
			ClearAllNotifications();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			BranchPK = new Guid(reader.ReadElementString(Schema.BranchPK));
			StartNumber = reader.ReadElementStringAsZDecimal(Schema.StartNumber);
			LastNumber = reader.ReadElementStringAsZDecimal(Schema.LastNumber);
			CompanyLevelPK = Guid.Empty;
			try
			{
				CompanyLevelPK = new Guid(reader.ReadElementString(Schema.CompanyLevelPK));
			}
			catch (FormatException)
			{ }
			RunOutWarningLimitNumber = reader.ReadElementStringAsZDecimal(Schema.RunOutWarningLimitNumber);
		}

		#endregion

		protected InBondNumberRangeRegistryItem NumberRangeRegistryItem
		{
			get { return USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange; }
		}

		protected long MinimumNumberRange
		{
			get { return NumberFountains.USMinimumInBondNumber; }
		}

		protected long MaximumNumberRange
		{
			get { return NumberFountains.USMaximumInBondNumber; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new InBondNumberRange();
			if (fallbackLevel == null)
			{
				result.CompanyLevelPK = Env.CurrentCompany.PK;
				result.BranchPK = BranchPK;
			}
			else
			{
				result.BranchPK = fallbackLevel.BranchPK;
				result.CompanyLevelPK = fallbackLevel.CompanyPK(true);
			}
			return result;
		}

		ZBool EnteringAtCompanyLevel
		{
			get { return CompanyLevelPK != Guid.Empty && BranchPK == ZGuid.Empty; }
		}

		ZBool EnteringAtBranchLevel
		{
			get { return BranchPK != ZGuid.Empty; }
		}

		ZBool BranchRegistryAndCompanyRegistryConflicts()
		{
			var enteringAtCompanyLevel = EnteringAtCompanyLevel;
			var enteringAtBranchLevel = EnteringAtBranchLevel;
			var companyPK = enteringAtCompanyLevel ? CompanyLevelPK : CompanyPK;

			ZQuery companyRegistryQuery = new ZQuery(StmDataSchema.SD_Name, "CompanyOrBranchInBondNumberRange");
			companyRegistryQuery.AddToFilter(StmDataSchema.SD_Owner, companyPK);
			companyRegistryQuery.AddToFilter(StmDataSchema.SD_BinaryValue, SQLComparisonOperator.NotEqual, null);
			var companyRegistryExists = CurrentFactory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(StmData)), companyRegistryQuery);

			var branchRegistryExists = false;
			ZQuery branchQuery = new ZQuery(GlbBranchSchema.GB_GC, companyPK);
			foreach (var companyBranch in CurrentFactory.Load<IGlbBranch>(branchQuery))
			{
				ZQuery branchRegistryQuery = new ZQuery(StmDataSchema.SD_Name, "CompanyOrBranchInBondNumberRange");
				branchRegistryQuery.AddToFilter(StmDataSchema.SD_Owner, companyBranch.PK);
				branchRegistryQuery.AddToFilter(StmDataSchema.SD_BinaryValue, SQLComparisonOperator.NotEqual, null);
				branchRegistryExists = branchRegistryExists || CurrentFactory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(StmData)), branchRegistryQuery);
			}

			return ((companyRegistryExists || enteringAtCompanyLevel) && branchRegistryExists) || (companyRegistryExists && enteringAtBranchLevel);
		}

		public override bool Equals(object obj)
		{
			var b = obj as InBondNumberRange;
			return b != null &&
				this.BranchPK == b.BranchPK &&
				this.StartNumber == b.StartNumber &&
				this.LastNumber == b.LastNumber &&
				this.RunOutWarningLimitNumber == b.RunOutWarningLimitNumber;
		}

		public override int GetHashCode()
		{
			return BranchPK.GetHashCode();
		}
	}
}
