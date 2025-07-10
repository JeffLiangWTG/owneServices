using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class BorderLineReleaseAllocator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BorderLineReleaseAllocator(USCustomsNumberViewStmNumsWrapper wrapper)
			: base(wrapper.Factory)
		{
			this.Wrapper = wrapper;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Schema
		{
			public const string AppliesTo = "AppliesTo";
			public const string CurrentValue = "CurrentValue";
			public const string NumberToAllocate = "NumberToAllocate";
			public const string TotalAvailableNumbers = "TotalAvailableNumbers";
		}

		#region Properties

		[ResourceStringData("BorderLineReleaseAllocator|AppliesTo", Caption = "Applies To")]
		public ZString AppliesTo
		{
			get { return Wrapper.AppliesTo; }
		}

		public ZPropertyInfo AppliesToInfo => GetWrappedZPropertyInfo(Schema.AppliesTo, (x) => Wrapper.AppliesToInfo);

		[ResourceStringData("BorderLineReleaseAllocator|CurrentValue", Caption = "Current Value")]
		public ZLong CurrentValue
		{
			get { return Wrapper.SN_ValueForDisplay; }
		}

		public ZPropertyInfo CurrentValueInfo => GetWrappedZPropertyInfo(Schema.CurrentValue, (x) => Wrapper.SN_ValueForDisplayInfo);

		[ResourceStringData("BorderLineReleaseAllocator|TotalAvailable", Caption = "Total Available")]
		public ZLong TotalAvailableNumbers
		{
			get { return NumberRange?.TotalAvailableNumbers ?? ZLong.Zero; }
		}

		public ZPropertyInfo TotalAvailableNumbersInfo => GetZPropertyInfo(Schema.TotalAvailableNumbers);

		#region Number To Allocate

		[ResourceStringData("BorderLineReleaseAllocator|NumberToAllocate", Caption = "Number To Allocate")]
		public ZLong NumberToAllocate
		{
			get { return numberToAllocate; }
			set
			{
				if (SetNonPersistentPropertyValue(NumberToAllocateInfo, ref numberToAllocate, (long)value)) //ensure valid long
				{
					ValidateNumberToAllocate();
				}
			}
		}
		ZLong numberToAllocate;

		public ZPropertyInfo NumberToAllocateInfo => GetZPropertyInfo(Schema.NumberToAllocate);

		bool IsNumberToAllocateWithInRange => NumberToAllocate >= 1 && NumberToAllocate <= TotalAvailableNumbers;

		public void ValidateNumberToAllocate()
		{
			NumberToAllocateInfo.ClearAllNotifications();
			ValidateNumberToAllocateCore();
		}

		protected virtual void ValidateNumberToAllocateCore()
		{
			var total = TotalAvailableNumbers;
			if (!IsNumberToAllocateWithInRange)
			{
				NumberToAllocateInfo.AddError("Number to allocate must be greater than 0 and less than or equal to " + total + ".");
			}
		}

		public ZString GetReasonForNotAbleToAllocate()
		{
			var result = ZString.Empty;
			if (!IsNumberToAllocateWithInRange)
			{
				result = NumberToAllocateCannotBeGreaterThanAvailableNumbers(TotalAvailableNumbers);
			}
			return result;
		}

		public static ResourceString NumberToAllocateCannotBeGreaterThanAvailableNumbers(ZLong availableNumbers)
		{
			return ResString.GetMultilingualString("{60FC023C-19E6-42E3-808D-E8E273F25A8B}", "Number To Allocate must be greater than 0 and less than or equal to the available numbers for this sequence ({0}).", availableNumbers);
		}

		public string[] AllocateNumbers()
		{
			string[] result = null;
			if (IsNumberToAllocateWithInRange)
			{
				var number = (long)NumberToAllocate;
				var stmNums = NumberRange?.GetStmNums();
				if (stmNums != null && stmNums.Length > 0)
				{
					var entryNumbers = new List<string>((int)number);
					foreach (var stmNum in stmNums)
					{
						var allocatedNumbers = GetAllocatedNumbers(stmNum, number);
						if (allocatedNumbers.Length > 0)
						{
							number -= allocatedNumbers.Length;
							entryNumbers.AddRange(allocatedNumbers);
						}
						if (number == ZDecimal.Zero)
						{
							break;
						}
					}
					result = entryNumbers.ToArray();
				}
			}
			return result ?? Array.Empty<string>();
		}

		string[] GetAllocatedNumbers(CustomsNumberViewStmNums stmNum, long number)
		{
			string[] numbers = null;
			var availableNumbers = (long)stmNum.SN_AvailableNumbers;
			if (availableNumbers > ZDecimal.Zero)
			{
				long total = Math.Min(number, availableNumbers);
				if (total > 0)
				{
					var currentNextNumber = (long)stmNum.SN_ValueForDisplay;
					if (currentNextNumber > 0)
					{
						var lastNumber = currentNextNumber + total;
						var entryNumbers = new List<string>((int)total);
						for (; currentNextNumber < lastNumber; currentNextNumber++)
						{
							entryNumbers.Add(stmNum.GenerateCustomsNumber(currentNextNumber));
						}
						if (currentNextNumber > stmNum.SN_MaximumValue)
						{
							stmNum.SetNextNumber(stmNum.SN_MaximumValue);
							var factory = new BusinessObjectFactory();
							var connection = ((IDbConnected)factory).Connection;
							using (var transactionManager = connection.BeginTransactionWithManager())
							{
								stmNum.GenerateNextCustomsNumber(factory);
								transactionManager.CommitTransaction();
							}
						}
						else
						{
							stmNum.SetNextNumber(currentNextNumber);
						}
						numbers = entryNumbers.ToArray();
					}
				}
			}
			return numbers ?? Array.Empty<string>();
		}

		#endregion

		#endregion

		public readonly USCustomsNumberViewStmNumsWrapper Wrapper;

		public CustomsNumberStmNumberRange NumberRange
		{
			get
			{
				if (numberRangeCached == null)
				{
					numberRangeCached = new CachedProperty<CustomsNumberStmNumberRange>(Factory, () => Wrapper.StmNums?.FirstNumberRange);
				}
				return numberRangeCached.Value;
			}
		}
		CachedProperty<CustomsNumberStmNumberRange> numberRangeCached;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateNumberToAllocate();
		}
	}
}
