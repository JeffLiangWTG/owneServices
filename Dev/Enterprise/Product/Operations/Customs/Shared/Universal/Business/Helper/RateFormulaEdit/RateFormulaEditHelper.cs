using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class RateFormulaEditHelper : NonPersistentBusinessObject, IObsoleteValidation, IUniversalRateCalcData
	{
		public RateFormulaEditHelper(IUnitListForRateFormulaEditProvider unitListProvider = null)
		{
			this.unitListProvider = unitListProvider;
		}
		readonly IUnitListForRateFormulaEditProvider unitListProvider;

		protected override ZString HumanReadableNameCore => Res.GetString("Enterprise.Customs.Universal.RateFormulaEditHelper|HumanReadableName", "Rate Formula");

		#region Formula

		const string FreeRateFormula = "0";
		const string VFD = "VFD";

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|Formula", Caption = "Formula")]
		public ZString Formula
		{
			get { return fFormula; }
			set
			{
				SetNonPersistentPropertyValue(FormulaInfo, ref fFormula, value);
				if (!IsValidationSuspended)
				{
					ValidateFormula();
				}
				FormulaInfo.RefreshBinding();
			}
		}
		ZString fFormula;

		public ZPropertyInfo FormulaInfo => GetZPropertyInfo(nameof(Formula));

		public bool Formula_ReadOnly => !IsFreeFormat;

		void RebuildRateFormula()
		{
			if (IsFree)
			{
				Formula = FreeRateFormula;
			}
			else if (IsPercentageOfCustomsValue)
			{
				Formula = GetPercentageOfCustomsValue();
			}
			else if (IsRatePerUnit)
			{
				Formula = GetRatePerUnit();
			}
			else if (IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit)
			{
				Formula = "MAX(" + GetPercentageOfCustomsValue() + ", " + "" + GetRatePerUnit() + ")";
			}
			else if (IsPercentageOfCustomsValueAndRatePerUnit)
			{
				Formula = "(" + GetPercentageOfCustomsValue() + ") + " + "(" + GetRatePerUnit() + ")";
			}
		}

		ZString GetPercentageOfCustomsValue() => (PercentageOfCustomsValue / 100) + "*" + VFD;

		ZString GetRatePerUnit() => RatePerUnit + "*[" + Unit + "]";

		#endregion

		#region PercentageOfCustomsValue

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|PercentageOfCustomsValue", Caption = "% Customs Value")]
		[DecimalPlaces(2)]
		public ZDecimal PercentageOfCustomsValue
		{
			get { return fPercentageOfCustomsValue; }
			set
			{
				var oldValue = PercentageOfCustomsValue;
				SetNonPersistentPropertyValue(PercentageOfCustomsValueInfo, ref fPercentageOfCustomsValue, value);
				if (!IsValidationSuspended)
				{
					ValidatePercentageOfCustomsValue();
				}
				if (!IsCopying && oldValue != PercentageOfCustomsValue)
				{
					RebuildRateFormula();
				}
				PercentageOfCustomsValueInfo.RefreshBinding();
			}
		}
		ZDecimal fPercentageOfCustomsValue;

		public ZPropertyInfo PercentageOfCustomsValueInfo => GetZPropertyInfo(nameof(PercentageOfCustomsValue));

		public ZBool PercentageOfCustomsValueAvailable => IsPercentageOfCustomsValue || IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit || IsPercentageOfCustomsValueAndRatePerUnit;

		#endregion

		#region Unit and RatePerUnit

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|RatePerUnit", Caption = "Rate")]
		[DecimalPlaces(4)]
		public ZDecimal RatePerUnit
		{
			get { return fRatePerUnit; }
			set
			{
				var oldValue = RatePerUnit;
				SetNonPersistentPropertyValue(RatePerUnitInfo, ref fRatePerUnit, value);
				if (!IsValidationSuspended)
				{
					ValidateRatePerUnit();
				}
				if (!IsCopying && oldValue != RatePerUnit)
				{
					RebuildRateFormula();
				}
				RatePerUnitInfo.RefreshBinding();
			}
		}
		ZDecimal fRatePerUnit;

		public ZPropertyInfo RatePerUnitInfo => GetZPropertyInfo(nameof(RatePerUnit));

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|Unit", Caption = "Unit")]
		[List(nameof(UnitList))]
		public ZString Unit
		{
			get { return fUnit; }
			set
			{
				var oldValue = Unit;
				SetNonPersistentPropertyValue(UnitInfo, ref fUnit, value);
				if (!IsValidationSuspended)
				{
					ValidateUnit();
				}
				if (!IsCopying && oldValue != Unit)
				{
					RebuildRateFormula();
				}
				UnitInfo.RefreshBinding();
			}
		}
		ZString fUnit;

		public ZPropertyInfo UnitInfo => GetZPropertyInfo(nameof(Unit));

		public CodeDescriptionPairList UnitList => unitList ?? (unitList = unitListProvider?.UnitList ?? new CodeDescriptionPairList());
		CodeDescriptionPairList unitList;

		public ZBool RatePerUnitAvailable => IsRatePerUnit || IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit || IsPercentageOfCustomsValueAndRatePerUnit;

		#endregion

		#region Formula Types

		FormulaType FormulaType
		{
			get => fFormulaType;
			set
			{
				fFormulaType = value;
				RebuildRateFormula();
			}
		}
		FormulaType fFormulaType;

		#region IsFree

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|IsFree", Caption = "Free")]
		public ZBool IsFree
		{
			get { return FormulaType == FormulaType.Free; }
			set
			{
				FormulaType = (value ? FormulaType.Free : FormulaType.None);
				IsFreeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsFreeInfo => GetZPropertyInfo(nameof(IsFree));

		#endregion

		#region IsFreeFormat

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|IsFreeFormat", Caption = "Free Format")]
		public ZBool IsFreeFormat
		{
			get { return FormulaType == FormulaType.FreeFormat; }
			set
			{
				FormulaType = (value ? FormulaType.FreeFormat : FormulaType.None);
				IsFreeFormatInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsFreeFormatInfo => GetZPropertyInfo(nameof(IsFreeFormat));

		#endregion

		#region IsPercentageOfCustomsValue

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|IsPercentageOfCustomsValue", Caption = "% of Customs Value")]
		public ZBool IsPercentageOfCustomsValue
		{
			get { return FormulaType == FormulaType.PercentageOfCustomsValue; }
			set
			{
				FormulaType = (value ? FormulaType.PercentageOfCustomsValue : FormulaType.None);
				IsPercentageOfCustomsValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsPercentageOfCustomsValueInfo => GetZPropertyInfo(nameof(IsPercentageOfCustomsValue));

		#endregion

		#region IsRatePerUnit 

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|IsRatePerUnit", Caption = "Rate Per Unit")]
		public ZBool IsRatePerUnit
		{
			get { return FormulaType == FormulaType.RatePerUnit; }
			set
			{
				FormulaType = (value ? FormulaType.RatePerUnit : FormulaType.None);
				IsRatePerUnitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsRatePerUnitInfo => GetZPropertyInfo(nameof(IsRatePerUnit));

		#endregion

		#region IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit 

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit", Caption = "% of Customs Value with a Minimum of Rate Per Unit")]
		public ZBool IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit
		{
			get { return FormulaType == FormulaType.PercentageOfCustomsValueWithAMinimumOfRatePerUnit; }
			set
			{
				FormulaType = (value ? FormulaType.PercentageOfCustomsValueWithAMinimumOfRatePerUnit : FormulaType.None);
				IsPercentageOfCustomsValueWithAMinimumOfRatePerUnitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsPercentageOfCustomsValueWithAMinimumOfRatePerUnitInfo => GetZPropertyInfo(nameof(IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit));

		#endregion

		#region IsPercentageOfCustomsValueAndRatePerUnit 

		[ResourceStringData("Enterprise.Customs.Universal.RateFormulaEditHelper|IsPercentageOfCustomsValueAndRatePerUnit", Caption = "% of Customs Value + Rate Per Unit")]
		public ZBool IsPercentageOfCustomsValueAndRatePerUnit
		{
			get { return FormulaType == FormulaType.PercentageOfCustomsValueAndRatePerUnit; }
			set
			{
				FormulaType = (value ? FormulaType.PercentageOfCustomsValueAndRatePerUnit : FormulaType.None);
				IsPercentageOfCustomsValueAndRatePerUnitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsPercentageOfCustomsValueAndRatePerUnitInfo => GetZPropertyInfo(nameof(IsPercentageOfCustomsValueAndRatePerUnit));

		#endregion

		#endregion

		#region Validation

		public void ValidateAll()
		{
			ValidatePercentageOfCustomsValue();
			ValidateRatePerUnit();
			ValidateUnit();
			ValidateFormula();
		}

		public void ValidatePercentageOfCustomsValue()
		{
			PercentageOfCustomsValueInfo.ClearAllNotifications();
			if (PercentageOfCustomsValueAvailable && (PercentageOfCustomsValue <= 0 || PercentageOfCustomsValue > 1000))
			{
				PercentageOfCustomsValueInfo.AddError(Res.GetString("9DDCF960-EFEE-4DC0-A132-1D1D3A657A76", "The % Customs Value should be greater than Zero and be no more than 1000."));
			}
		}

		public void ValidateRatePerUnit()
		{
			RatePerUnitInfo.ClearAllNotifications();
			if (RatePerUnitAvailable && (RatePerUnit <= 0 || RatePerUnit > 10000))
			{
				RatePerUnitInfo.AddError(Res.GetString("92B3A32A-C3FC-4BD3-BA9E-E8DCAD0B3057", "The Rate Per Unit should be greater than Zero and be no more than 10000."));
			}
		}

		public void ValidateUnit()
		{
			UnitInfo.ClearAllNotifications();
			if (RatePerUnitAvailable)
			{
				MandatoryValidation.CheckEntered(UnitInfo);
				ListValidation.ErrorIfInvalidCode(UnitInfo);
			}
		}

		public void ValidateFormula()
		{
			FormulaInfo.ClearAllNotifications();
			if (IsFreeFormat)
			{
				var calculator = new UniversalRateCalculator(Formula, this);
				if (calculator.Errors.Any())
				{
					FormulaInfo.AddError(Res.GetString("F9E65F9D-4AB8-4E47-859E-E3AB04DE3872", "The Formula is not valid due to the error(s): {0}.", string.Join("\r\n", calculator.Errors.Select(x => x.ErrorMessage))));
				}
			}
		}

		#endregion

		#region IUniversalRateCalcData

		DateTime IUniversalRateCalcData.DateOfValuation => ZDateTime.Today.ToDateTime();

		decimal IUniversalRateCalcData.ValueForDuty => 0m;

		decimal IUniversalRateCalcData.CustomsValue => 0m;

		IDictionary<string, decimal> IUniversalRateCalcData.UnitOfMeasureValueList
		{
			get
			{
				var unitsOfMeasureValues = new Dictionary<string, decimal>();
				foreach (var unitCode in UnitList.GetAllCodes())
				{
					unitsOfMeasureValues.Add(unitCode, 0m);
				}

				return unitsOfMeasureValues;
			}
		}

		IDictionary<string, decimal> IUniversalRateCalcData.CountrySpecificValueList { get; } = new Dictionary<string, decimal>();

		IList<Tuple<string, string>> IUniversalRateCalcData.AdditionalInformationList { get; } = new List<Tuple<string, string>>();

		IDictionary<string, string> IUniversalRateCalcData.MeursingExpressionList { get; } = new Dictionary<string, string>();

		#endregion
	}

	enum FormulaType
	{
		None,
		Free,
		FreeFormat,
		PercentageOfCustomsValue,
		RatePerUnit,
		PercentageOfCustomsValueWithAMinimumOfRatePerUnit,
		PercentageOfCustomsValueAndRatePerUnit
	}
}
