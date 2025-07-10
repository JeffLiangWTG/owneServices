using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Integration;

namespace Enterprise.Customs.US.Business
{
	public abstract class NumberSetting : Customs.Business.NumberSetting
	{
		#region Schema

		public new class Schema : Customs.Business.NumberSetting.Schema
		{
			public const string StartNumber = "StartNumber";
			public const string LastNumber = "LastNumber";
			public const string AvailableNumbers = "AvailableNumbers";
		}

		#endregion

		protected NumberSetting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region public interface

		public void EnsureCurrentNextNumberIsCorrect()
		{
			long number = (long)CurrentNextNumber;
			if (number < StartNumber)
			{
				NextNumber = StartNumber;
				PostNextNumber();
			}
		}

		public void PostNextNumber()
		{
			if (IsNextNumberValid)
			{
				SetNextNumber(NextNumber);
				CurrentNextNumberInfo.RefreshBinding();
			}
		}

		public bool IsNumberFountainValid
		{
			get { return NumberFountain != null; }
		}

		public ZString GenerateCurrentNextNumberWithCheckDigit()
		{
			var numberFountain = NumberFountain;
			return numberFountain != null ? GenerateNumberWithCheckDigit(numberFountain.GetNext(Factory)) : ZString.Empty;
		}

		public bool HasReachedLimit
		{
			get { return AvailableNumbers <= WarningLimitMark; }
		}

		public abstract ZDecimal WarningLimitMark { get; }

		#endregion

		#region Properties

		#region Next Number

		protected override void ValidateNextNumberCore()
		{
			base.ValidateNextNumberCore();
			if (!IsNextNumberValid)
			{
				if (LastNumber > MaximumNextNumberRestriction)
				{
					NextNumberInfo.AddError("Because of a restriction in the System, the Next Number must be greater than or equal to " + StartNumber + " and less than or equal to " + MaximumNextNumberRestriction + ".");
				}
				else
				{
					NextNumberInfo.AddError("Next Number must be greater than or equal to " + StartNumber + " and less than or equal to " + LastNumber + ".");
				}
			}
		}

		protected bool IsNextNumberValid
		{
			get { return NextNumber >= StartNumber && NextNumber <= Math.Min(LastNumber, MaximumNextNumberRestriction); }
		}

		internal long MaximumNextNumberRestriction
		{
			get { return MaximumAllowForNumberFountain - Enterprise.NumberFountain.FountainUtils.CacheSize - 1; }
		}

		#endregion

		#region Start Number

		public abstract ZDecimal StartNumber { get; }

		public ZPropertyInfo StartNumberInfo
		{
			get { return GetZPropertyInfo(Schema.StartNumber); }
		}

		#endregion

		#region Last Number

		public abstract ZDecimal LastNumber { get; }

		public ZPropertyInfo LastNumberInfo
		{
			get { return GetZPropertyInfo(Schema.LastNumber); }
		}

		#endregion

		#region Number of Available Entry Numbers

		public ZDecimal AvailableNumbers
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				ZDecimal nextNumber = CurrentNextNumber;
				if (nextNumber > ZDecimal.Zero && nextNumber <= MaximumAllowForNumberFountain)
				{
					result = Math.Max(LastNumber - nextNumber + 1, 0);
				}
				return result;
			}
		}

		public ZPropertyInfo AvailableNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.AvailableNumbers); }
		}

		#endregion

		#endregion

		#region Implementation

		public override Customs.Business.BaseJobDeclaration GetJobDeclaration(CusEntryNumber entryNumber)
		{
			return entryNumber.GetJobDeclaration();
		}

		internal protected abstract long MaximumAllowForNumberFountain { get; }

		protected string RegistryLocation(IRegistryItemInternals registryItem)
		{
			return "Admin -> System -> Registry -> " + registryItem.Location;
		}

		#endregion
	}
}
