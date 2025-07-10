using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business
{
	public abstract class NumberSetting : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "External solution inherits this class implementation.")]
		public class Schema
		{
			public const string CurrentNextNumber = "CurrentNextNumber";
			public const string NextNumber = "NextNumber";
		}

		#endregion

		protected NumberSetting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region Current Next Number

		public virtual ZDecimal CurrentNextNumber
		{
			get
			{
				var numberFountain = NumberFountain;
				ZDecimal result = numberFountain == null ? 0 : numberFountain.PeekPreliminary(Factory);
				return result;
			}
		}

		public ZPropertyInfo CurrentNextNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentNextNumber); }
		}

		#endregion

		#region Current Min Number

		public virtual ZDecimal CurrentMinNumber
		{
			get
			{
				var numberFountain = NumberFountain;
				if (numberFountain == null)
				{
					return 0;
				}
				long minValue, maxValue;
				numberFountain.GetMinAndMaxValues(Factory, out minValue, out maxValue);
				return minValue;
			}
		}

		public ZPropertyInfo CurrentMinNumberInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentMinNumber)); }
		}

		#endregion

		#region Current Max Number

		public virtual ZDecimal CurrentMaxNumber
		{
			get
			{
				var numberFountain = NumberFountain;
				if (numberFountain == null)
				{
					return 0;
				}
				long minValue, maxValue;
				numberFountain.GetMinAndMaxValues(Factory, out minValue, out maxValue);
				return maxValue;
			}
		}

		public ZPropertyInfo CurrentMaxNumberInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentMaxNumber)); }
		}

		#endregion

		#region Next Number

		public ZDecimal NextNumber
		{
			get { return nextNumber; }
			set
			{
				if (SetNonPersistentPropertyValue(NextNumberInfo, ref nextNumber, value))
				{
					ValidateAll(true);
				}
			}
		}

		ZDecimal nextNumber;

		public ZPropertyInfo NextNumberInfo
		{
			get { return GetZPropertyInfo(Schema.NextNumber); }
		}

		protected ZDecimal NewOrCurrentNextNumber
		{
			get
			{
				ZDecimal res = NextNumber;
				if (res == 0)
				{
					res = CurrentNextNumber;
				}
				return res;
			}
		}

		public void ValidateNextNumber()
		{
			NextNumberInfo.ClearAllNotifications();
			ValidateNextNumberCore();
		}

		protected virtual void ValidateNextNumberCore()
		{
			ValidateAsLong(NextNumberInfo, NextNumber);
			if (NextNumber < 0)
			{
				NextNumberInfo.AddError(Res.GetString("305AAA41-A7C3-4540-96FA-330818615FA5", "The next number should not be negative."));
			}
			if (NextNumber != 0 && NextNumber < NewOrCurrentMinNumber)
			{
				string message = Res.GetString("341DBE9C-7FF5-43EF-8CFA-2A3BE925B496", "The next number should not be less than the minimum number.");
				NextNumberInfo.AddError(message);
			}
			if (NextNumber != 0 && NextNumber > NewOrCurrentMaxNumber)
			{
				string message = Res.GetString("2EEB261D-DCB4-445C-9124-347910E4FB82", "The next number should not be greater than the maximum number.");
				NextNumberInfo.AddError(message);
			}
			var entryNumber = GetCusEntryNumberMatching(GenerateNumberWithCheckDigit(NextNumber));
			if (entryNumber != null)
			{
				var declaration = GetJobDeclaration(entryNumber);
				string message = (declaration == null)
					? Res.GetString("1E45DE64-8497-43F2-AE08-1F18F05FCF7E", "This Next Number has already been used.")
					: Res.GetString("8C2B8654-28D2-4C2E-96EA-50D63F7C15D6", "This Next Number has already been used on Declaration '{0}'.", declaration.JE_DeclarationReference);
				NextNumberInfo.AddWarning(message);
			}
		}

		#endregion

		#region Min Number

		public ZDecimal MinNumber
		{
			get { return minNumber; }
			set
			{
				if (SetNonPersistentPropertyValue(MinNumberInfo, ref minNumber, value))
				{
					ValidateAll(true);
					if (value != 0 && NextNumber == 0 && !IsInitialized)
					{
						NextNumber = value;
					}
				}
			}
		}

		ZDecimal minNumber;

		public ZPropertyInfo MinNumberInfo
		{
			get { return GetZPropertyInfo(nameof(MinNumber)); }
		}

		protected ZDecimal NewOrCurrentMinNumber
		{
			get
			{
				ZDecimal res = MinNumber;
				if (res == 0)
				{
					res = CurrentMinNumber;
				}
				return res;
			}
		}

		public void ValidateMinNumber()
		{
			MinNumberInfo.ClearAllNotifications();
			ValidateMinNumberCore();
		}

		protected virtual void ValidateMinNumberCore()
		{
			ValidateAsLong(MinNumberInfo, MinNumber);
			if (MinNumber < 0)
			{
				MinNumberInfo.AddError(Res.GetString("33DD68E4-28BC-4409-B67D-DB6A8BC02A09", "The minimum number should not be negative."));
			}
			if (MinNumber != 0 && MinNumber > NewOrCurrentNextNumber)
			{
				string message = Res.GetString("52A3BB53-1944-4D3D-AA8F-FE2BC5D3F053", "The minimum number should not be greater than the next number.");
				MinNumberInfo.AddError(message);
			}
		}

		#endregion

		#region Max Number

		public ZDecimal MaxNumber
		{
			get { return maxNumber; }
			set
			{
				if (SetNonPersistentPropertyValue(MaxNumberInfo, ref maxNumber, value))
				{
					ValidateAll(true);
				}
			}
		}

		ZDecimal maxNumber;

		public ZPropertyInfo MaxNumberInfo
		{
			get { return GetZPropertyInfo(nameof(MaxNumber)); }
		}

		protected ZDecimal NewOrCurrentMaxNumber
		{
			get
			{
				ZDecimal res = MaxNumber;
				if (res == 0)
				{
					res = CurrentMaxNumber;
				}
				return res;
			}
		}

		public void ValidateMaxNumber()
		{
			MaxNumberInfo.ClearAllNotifications();
			ValidateMaxNumberCore();
		}

		protected virtual void ValidateMaxNumberCore()
		{
			ValidateAsLong(MaxNumberInfo, MaxNumber);
			if (MaxNumber < 0)
			{
				MaxNumberInfo.AddError(Res.GetString("3252C857-FA85-4A84-87F2-C1844208F306", "The maximum number should not be negative."));
			}
			if (MaxNumber != 0 && MaxNumber < NewOrCurrentNextNumber)
			{
				string message = Res.GetString("2372AF90-7D8F-461B-B652-11F976157053", "The maximum number should not be less than the next number.");
				MaxNumberInfo.AddError(message);
			}
			if (MaxNumber != 0 && MaxNumber < CurrentNextNumber)
			{
				string message = Res.GetString("FC323C63-7EDF-4D4C-9272-FD33D547FFEE", "The maximum number should not be less than the current next number.");
				MaxNumberInfo.AddError(message);
			}
		}

		#endregion

		#region IsInitialized

		public ZBool IsInitialized
		{
			get { return NumberFountain.PeekPreliminaryOrDefault(Factory, 0) != 0; }
		}

		public ZPropertyInfo IsInitializedInfo
		{
			get { return GetZPropertyInfo(nameof(IsInitialized)); }
		}

		#endregion

		#endregion

		protected void ValidateAll(bool revalidateRelated)
		{
			ClearAllNotifications();
			ValidateAllCore(revalidateRelated);
		}

		protected virtual void ValidateAllCore(bool revalidateRelated)
		{
			ValidateNextNumber();
			ValidateMinNumber();
			ValidateMaxNumber();
		}

		static void ValidateAsLong(ZPropertyInfo propertyInfo, ZDecimal decimalValue)
		{
			if (decimalValue < long.MinValue || decimalValue > long.MaxValue)
			{
				string message = Res.GetString("F06F01DA-A9D8-4A77-BFF7-F0F0021A55BA", "Invalid number.");
				propertyInfo.AddError(message);
			}
		}

		#region Implementation

		protected abstract ZString GenerateNumberWithCheckDigit(ZDecimal number);
		public abstract BaseJobDeclaration GetJobDeclaration(CusEntryNumber entryNumber);
		public abstract CusEntryNumber GetCusEntryNumberMatching(ZString number);
		protected abstract INumberFountainProxy NumberFountain { get; }

		protected void SetNextNumber(ZDecimal number)
		{
			long numberLong = (long)number; //ensure valid long
			var numberFountain = NumberFountain;
			if (numberFountain != null && numberLong != ZDecimal.Zero)
			{
				using (var connection = Db.NewExtraConnectionToMainDb())
				using (var transactionManager = connection.BeginTransactionWithManager())
				{
					numberFountain.SetNext(connection, numberLong);
					transactionManager.CommitTransaction();
				}
			}
		}

#if DEBUG
		protected abstract string NumberFountainNameForTestig { get; }
		protected abstract Guid NumberFountainOwnerForTestig { get; }
#endif
		bool HasChangesForMinAndMaxValue
		{
			get { return MinNumber > 0 && CurrentMinNumber != MinNumber || MaxNumber > 0 && CurrentMaxNumber != MaxNumber; }
		}

		bool HasChangesForNextValue
		{
			get { return NextNumber > 0 && CurrentNextNumber != NextNumber; }
		}

		public bool HasChangesToSave
		{
			get { return HasChangesForNextValue || HasChangesForMinAndMaxValue; }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			var numberFountain = NumberFountain;
			if (numberFountain == null)
			{
				return;
			}

			long minNumberLong = (long)MinNumber;
			long nextNumberLong = (long)NextNumber;
			long maxNumberLong = (long)MaxNumber;

			if (!HasChangesToSave)
			{
				// nothing to save
				return;
			}

			numberFountain.SetValues(Factory, minValue: minNumberLong, nextValue: nextNumberLong, maxValue: maxNumberLong);
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				MinNumber = 0;
				NextNumber = 0;
				MaxNumber = 0;
			}

			ResetCachedValues();
			IsInitializedInfo.RefreshBinding();
			CurrentNextNumberInfo.RefreshBinding();
			CurrentMinNumberInfo.RefreshBinding();
			CurrentMaxNumberInfo.RefreshBinding();
		}

		void ResetCachedValues()
		{
			ResetCachedValuesCore();
		}

		protected virtual void ResetCachedValuesCore()
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll(false);
		}

		#endregion
	}
}
