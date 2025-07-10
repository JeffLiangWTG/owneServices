using System;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class CalculatorGeneratorFactory
	{
		#region Instance

		protected CalculatorGeneratorFactory() { }

		public static CalculatorGeneratorFactory Instance
		{
			get { return (calculatorGeneratorFactory) ?? (calculatorGeneratorFactory = new CalculatorGeneratorFactory()); }
		}

		[ThreadStatic]
		static CalculatorGeneratorFactory calculatorGeneratorFactory;

		#endregion

		public IRateCalculatorGenerator GetCalculatorGenerator(Xsd.RateCalculator calculator)
		{
			return GetCalculactorGeneratorFromType(GetCalculatorType(calculator));
		}

		public IRateCalculatorGenerator GetCalculatorGenerator(ZString calculatorCode)
		{
			return GetCalculactorGeneratorFromType(GetCalculatorType(calculatorCode));
		}

		IRateCalculatorGenerator GetCalculactorGeneratorFromType(Type calculatorGeneratorType)
		{
			IRateCalculatorGenerator generator = null;

			if (calculatorGeneratorType != null)
			{
				generator = (IRateCalculatorGenerator)Activator.CreateInstance(calculatorGeneratorType, Array.Empty<object>());
			}

			return generator;
		}

		public Type GetCalculatorType(Xsd.RateCalculator item)
		{
			if (item is Xsd.AGYCalculator)
			{
				return typeof(AGYCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.CTGCalculator)
			{
				return typeof(CTGCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.CTZCalculator)
			{
				return typeof(CTZCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.FPACalculator)
			{
				return typeof(FPACalculatorGeneratorFromXSD);
			}

			if (item is Xsd.FLTCalculator)
			{
				return typeof(FLTCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.FPUCalculator)
			{
				return typeof(FPUCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.HRCCalculator)
			{
				return typeof(HRCCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.IATCalculator)
			{
				return typeof(IATCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.MPUCalculator)
			{
				return typeof(MPUCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.PERCalculator)
			{
				return typeof(PERCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.PEBCalculator)
			{
				return typeof(PEBCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.PSRCalculator)
			{
				return typeof(PSRCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.UNTCalculator)
			{
				return typeof(UNTCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.CMBCalculator)
			{
				return typeof(CMBCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.CBICalculator)
			{
				return typeof(CBICalculatorGeneratorFromXSD);
			}

			if (item is Xsd.IXCCalculator)
			{
				return typeof(IXCCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.CSTCalculator)
			{
				return typeof(CSTCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.CTBCalculator)
			{
				return typeof(CTBCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.DINCalculator)
			{
				return typeof(DINCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.TMECalculator)
			{
				return typeof(TMECalculatorGeneratorFromXSD);
			}

			if (item is Xsd.NTECalculator)
			{
				return typeof(NTECalculatorGeneratorFromXSD);
			}

			if (item is Xsd.HRTCalculator)
			{
				return typeof(HRTCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.EQHCalculator)
			{
				return typeof(EQHCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.FRTCalculator)
			{
				return typeof(FRTCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.VEDCalculator)
			{
				return typeof(VEDCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.WPKCalculator)
			{
				return typeof(WPKCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.WLTCalculator)
			{
				return typeof(WLTCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.MINCalculator)
			{
				return typeof(MINCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.SMBCalculator)
			{
				return typeof(SMBCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.EXLCalculator)
			{
				return typeof(EXLCalculatorGeneratorFromXSD);
			}

			if (item is Xsd.HCCCalculator)
			{
				return typeof(HCCCalculatorGeneratorFromXSD);
			}

			return null;
		}

		public Type GetCalculatorType(ZString calculatorCode)
		{
			switch (calculatorCode)
			{
				case AgencyCalculator.Code:
					return typeof(AGYCalculatorGeneratorFromXSD);
				case CartageCalculator.Code:
					return typeof(CTGCalculatorGeneratorFromXSD);
				case CartageZoneDistanceCalculator.Code:
					return typeof(CTZCalculatorGeneratorFromXSD);
				case FirstPlusAdditionalCalculator.Code:
					return typeof(FPACalculatorGeneratorFromXSD);
				case FlatCalculator.Code:
					return typeof(FLTCalculatorGeneratorFromXSD);
				case FlatPlusPerUnitCalculator.Code:
					return typeof(FPUCalculatorGeneratorFromXSD);
				case HighestRateCalculator.Code:
					return typeof(HRCCalculatorGeneratorFromXSD);
				case PackageCountCalculator.Code:
					return typeof(IATCalculatorGeneratorFromXSD);
				case MinimumOrPerUnitCalculator.Code:
					return typeof(MPUCalculatorGeneratorFromXSD);
				case PercentageCalculator.Code:
					return typeof(PERCalculatorGeneratorFromXSD);
				case PercentageBreaksCalculator.Code:
					return typeof(PEBCalculatorGeneratorFromXSD);
				case ProfitShareRebateCalculator.Code:
					return typeof(PSRCalculatorGeneratorFromXSD);
				case UnitCalculator.Code:
					return typeof(UNTCalculatorGeneratorFromXSD);
				case CombinedCalculator.Code:
					return typeof(CMBCalculatorGeneratorFromXSD);
				case CombinedBreaksWithIncrementCalculator.Code:
					return typeof(CBICalculatorGeneratorFromXSD);
				case ValueRangeCalculator.Code:
					return typeof(IXCCalculatorGeneratorFromXSD);
				case CompanyTariffOrCostBasedCalculator.CostBasedCode:
					return typeof(CSTCalculatorGeneratorFromXSD);
				case CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode:
					return typeof(CTBCalculatorGeneratorFromXSD);
				case DisbursementInterestCalculator.Code:
					return typeof(DINCalculatorGeneratorFromXSD);
				case TimeCalculator.Code:
					return typeof(TMECalculatorGeneratorFromXSD);
				case NoteCalculator.Code:
					return typeof(NTECalculatorGeneratorFromXSD);
				case FreightInclusiveCalculator.Code:
					return typeof(FRTCalculatorGeneratorFromXSD);
				case HousebillReleaseTypeCalculator.Code:
					return typeof(HRTCalculatorGeneratorFromXSD);
				case EquipmentHireCalculator.Code:
					return typeof(EQHCalculatorGeneratorFromXSD);
				case EqualizationCalculator.Code:
					return typeof(VEDCalculatorGeneratorFromXSD);
				case WarehousePackCalculator.Code:
					return typeof(WPKCalculatorGeneratorFromXSD);
				case WarehouseLocationTypeCalculator.Code:
					return typeof(WLTCalculatorGeneratorFromXSD);
				case MinimumCalculator.Code:
					return typeof(MINCalculatorGeneratorFromXSD);
				case SplitMonthBillingCalculator.Code:
					return typeof(SMBCalculatorGeneratorFromXSD);
				case ExcludeCompanyTariffsCalculator.Code:
					return typeof(EXLCalculatorGeneratorFromXSD);
				case HighestChargeCalculator.Code:
					return typeof(HCCCalculatorGeneratorFromXSD);
				default:
					return null;
			}
		}
	}
}

