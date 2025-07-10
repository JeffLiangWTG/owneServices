using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	abstract class AppendixFCalculator
	{
		protected AppendixFCalculator(IDutyData dutyData, BusinessObjectFactory factory)
		{
			if (dutyData == null)
			{
				throw new ArgumentNullException(nameof(dutyData));
			}
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}
			this.dutyData = dutyData;
			this.factory = factory;
		}
		protected readonly IDutyData dutyData;
		protected readonly BusinessObjectFactory factory;

		public IDutyResult DutyResult
		{
			get
			{
				if (dutyResult == null)
				{
					dutyResult = Calculate();

					dutyResult.ClearIfDutyIsNotToBeCalculated();
				}
				return dutyResult;
			}
		}
		IDutyResult dutyResult;

		protected abstract IDutyResult Calculate();

		protected IDutyResult Calculate(IRateWrapper wrapper, string computationCode)
		{
			PQWrapper pqWrapper = new PQWrapper(wrapper, dutyData);
			IDutyResult result = new DutyResult();
			try
			{
				switch (computationCode)
				{
					case ComputationCodeList.Codes.Free:
					case "":
						result = new DutyResult();
						break;
					case ComputationCodeList.Codes.SpecificRateFirstQuantity:
						result = GetSpecificRateQ1(pqWrapper);
						break;
					case ComputationCodeList.Codes.SpecificRateSecondQuantity:
						result = GetSpecificRateQ2(pqWrapper);
						break;
					case ComputationCodeList.Codes.MultipleSpecific:
						result = GetMultipleSpecific(pqWrapper);
						break;
					case ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity:
						result = GetCompoundSpecificAndAdvalorem(pqWrapper);
						break;
					case ComputationCodeList.Codes.CompoundSpecificAndAdValoremSecondQuantity:
						result = GetCompoundSpecificAndAdvalorem2(pqWrapper);
						break;
					case ComputationCodeList.Codes.SpecificPlusCompound:
						result = GetSpecificPlusCompound(pqWrapper);
						break;
					case ComputationCodeList.Codes.AdValorem:
						result = GetAdvalorem(pqWrapper);
						break;
					case ComputationCodeList.Codes.Derived:
						result = new DutyResult();
						break;
					case ComputationCodeList.Codes.FunctionalAdValorem:
						result = GetFunctionalAdValorem(pqWrapper);
						break;
					case ComputationCodeList.Codes.SpecificFunctionalAdValorem:
						result = GetSpecificFunctionalAdValorem(pqWrapper);
						break;
					case ComputationCodeList.Codes.SpecificSpecific:
						result = GetSpecificSpecific(pqWrapper);
						break;
					case ComputationCodeList.Codes.CompoundSpecificAdValorem:
						result = GetCompoundSpecificAdValorem(pqWrapper);
						break;
					case ComputationCodeList.Codes.SpecificCompound:
						result = GetSpecificCompound(pqWrapper);
						break;
					case ComputationCodeList.Codes.SpecificPyrotechnics:
						result = GetSpecificPyrotechnics(pqWrapper);
						break;
					case ComputationCodeList.Codes.SpecificSugarJ:
						result = GetSpecificSugarJ(pqWrapper);
						break;
					case ComputationCodeList.Codes.SpecificSugarK:
						result = GetSpecificSugarK(pqWrapper);
						break;
					case ComputationCodeList.Codes.NoComputationFormulaAvailable:
						result = new DutyResult();
						break;
					default:
						throw new NotImplementedException("Calculation method not currently supported : " + computationCode);
				}
			}
			catch (OverflowException)
			{
				throw new CustomsMergeException($@"Computation error because data is too large.(Tariff:'{dutyData.Tariff}',Rate Specific:'{pqWrapper.P1}',Rate Advalorem:'{pqWrapper.P2}',Rate Other:'{pqWrapper.P3}',Quantity1:'{dutyData.Quantity1}',Quantity2:'{dutyData.Quantity2}',Quantity3:'{dutyData.Quantity3}',LinePrice:'{dutyData.GetEffectiveCustomsValue()}')");
			}

			return result;
		}

		class PQWrapper
		{
			public PQWrapper(IRateWrapper dutyRateWrapper, IDutyData dutyData)
			{
				this.dutyRateWrapper = dutyRateWrapper;
				this.dutyData = dutyData;
			}
			readonly IRateWrapper dutyRateWrapper;
			readonly IDutyData dutyData;

			public ZDecimal P1 { get { return dutyRateWrapper.Specific; } }
			public ZDecimal P2 { get { return dutyRateWrapper.Advalorem; } }
			public ZDecimal P3 { get { return dutyRateWrapper.Other; } }
			public ZDecimal Q1 { get { return dutyData.Quantity1; } }
			public ZDecimal Q2 { get { return dutyData.Quantity2; } }
			public ZDecimal Q3 { get { return dutyData.Quantity3; } }
			public ZString UQ1 { get { return dutyData.UQ1; } }
			public ZString UQ2 { get { return dutyData.UQ2; } }
			public ZString UQ3 { get { return dutyData.UQ3; } }

			public ZDecimal SelectedP
			{
				get
				{
					ZDecimal result = ZDecimal.Zero;
					switch (dutyData.SelectedRateType)
					{
						case "P":
							result = P1;
							break;
						case "S":
							result = P2;
							break;
					}
					return result;
				}
			}
		}

		#region Calculation Methods

		protected string GetPercentage(ZDecimal rawValue)
		{
			ZDecimal percentage = rawValue * 100m;
			return percentage == 0m ? "" : percentage.ToStringTrimZeros() + "%";
		}

		/// <summary>
		/// Calculation 1 'Specific Rate * Q1' : P1 * Q1
		/// </summary>
		/// <param name="tariff"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetSpecificRateQ1(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.PerUnitAmount = wrapper.P1;
			result.PerUnitUQ = wrapper.UQ1;
			result.TotalAmount = GetLocalAmount(wrapper.P1 * wrapper.Q1).Round(2);
			result.RateString = GetAmountPerUnitString(wrapper.P1, wrapper.UQ1);
			return result;
		}

		/// <summary>
		/// Calculation 2 'Specific Rate * Q2' : P1 * Q2
		/// </summary>
		/// <param name="tariff"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetSpecificRateQ2(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.PerUnitAmount = wrapper.P1;
			result.PerUnitUQ = wrapper.UQ2;
			result.TotalAmount = GetLocalAmount(wrapper.P1 * wrapper.Q2).Round(2);
			result.RateString = GetAmountPerUnitString(wrapper.P1, wrapper.UQ2);
			return result;
		}

		/// <summary>
		/// Calculation 3 'Multiple specific' : (P1*Q1) + (P3*Q2)
		/// </summary>
		/// <param name="tariff"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetMultipleSpecific(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.TotalAmount = GetLocalAmount(wrapper.P1 * wrapper.Q1 + wrapper.P3 * wrapper.Q2).Round(2);
			result.RateString = CombineTwoRateStrings(GetAmountPerUnitString(wrapper.P1, wrapper.UQ1), GetAmountPerUnitString(wrapper.P3, wrapper.UQ2));
			return result;
		}

		/// <summary>
		/// Calculation 4 - 'Compound (Specific & Advalorem)' : (P1*Q1) + (P2*Value)
		/// </summary>
		/// <param name="tariff"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetCompoundSpecificAndAdvalorem(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.PerUnitAmount = wrapper.P1;
			result.PerUnitUQ = wrapper.UQ1;
			result.PercentOfValue = wrapper.P2 * 100m;
			result.NoneCustomsValueAmount = GetLocalAmount(wrapper.P1 * wrapper.Q1).Amount;
			result.TotalAmount = GetLocalAmount((wrapper.P1 * wrapper.Q1) + (wrapper.P2 * dutyData.GetEffectiveCustomsValue())).Round(2);
			result.RateString = CombineTwoRateStrings(GetAmountPerUnitString(wrapper.P1, wrapper.UQ1), GetPercentage(wrapper.P2));
			return result;
		}

		/// <summary>
		/// Calculation 5 - 'Compound (Specific & Advalorem)' : (P1*Q2) + (P2*Value)
		/// </summary>
		/// <param name="tariff"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetCompoundSpecificAndAdvalorem2(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.PerUnitAmount = wrapper.P1;
			result.PerUnitUQ = wrapper.UQ2;
			result.PercentOfValue = wrapper.P2 * 100m;
			result.NoneCustomsValueAmount = GetLocalAmount(wrapper.P1 * wrapper.Q2).Amount;
			result.TotalAmount = GetLocalAmount((wrapper.P1 * wrapper.Q2) + (wrapper.P2 * dutyData.GetEffectiveCustomsValue())).Round(2);
			result.RateString = CombineTwoRateStrings(GetAmountPerUnitString(wrapper.P1, wrapper.UQ2), GetPercentage(wrapper.P2));
			return result;
		}

		/// <summary>
		/// Calculation 6 - 'Specific plus compound' : (P1*Q1)+(P3*Q2) + (P2*Value)
		/// </summary>
		IDutyResult GetSpecificPlusCompound(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.PercentOfValue = wrapper.P2 * 100m;
			result.NoneCustomsValueAmount = GetLocalAmount((wrapper.P1 * wrapper.Q1) + (wrapper.P3 * wrapper.Q2)).Amount;
			result.TotalAmount = GetLocalAmount((wrapper.P1 * wrapper.Q1) + (wrapper.P3 * wrapper.Q2) + (wrapper.P2 * dutyData.GetEffectiveCustomsValue())).Round(2);

			ZString firstPartString = CombineTwoRateStrings(GetAmountPerUnitString(wrapper.P1, wrapper.UQ1), GetAmountPerUnitString(wrapper.P3, wrapper.UQ2));
			result.RateString = CombineTwoRateStrings(firstPartString, GetPercentage(wrapper.P2));
			return result;
		}

		/// <summary>
		/// Calculation 7 'Advalorem' : P2 * Value
		/// </summary>
		/// <param name="tariff"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetAdvalorem(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.PercentOfValue = wrapper.P2 * 100m;
			result.TotalAmount = GetLocalAmount(wrapper.P2 * dutyData.GetEffectiveCustomsValue()).Round(2);
			result.RateString = GetPercentage(wrapper.P2);
			return result;
		}

		/// <summary>
		/// Calculation A 'Functional Advalorem' : (P2+P3*Q3)* Value
		/// </summary>
		IDutyResult GetFunctionalAdValorem(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			ZDecimal percentage = wrapper.P2 + wrapper.P3 * wrapper.Q3;
			result.PercentOfValue = percentage * 100;
			result.TotalAmount = GetLocalAmount(percentage * dutyData.GetEffectiveCustomsValue()).Round(2);
			result.RateString = GetPercentage(percentage);
			return result;
		}

		/// <summary>
		/// Calculation B 'Specific Functional Advalorem' : (P1*Q2)+(P2+P3*Q3)*Value
		/// </summary>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetSpecificFunctionalAdValorem(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			ZDecimal percentage = wrapper.P2 + wrapper.P3 * wrapper.Q3;
			result.PercentOfValue = percentage * 100;
			result.PerUnitAmount = wrapper.P1;
			result.PerUnitUQ = wrapper.UQ2;
			result.TotalAmount = GetLocalAmount(wrapper.P1 * wrapper.Q2 + percentage * dutyData.GetEffectiveCustomsValue()).Round(2);
			result.NoneCustomsValueAmount = GetLocalAmount(wrapper.P1 * wrapper.Q2).Amount;
			result.RateString = CombineTwoRateStrings(GetAmountPerUnitString(wrapper.P1, wrapper.UQ2), GetPercentage(percentage));
			return result;
		}

		/// <summary>
		/// Calculation C - 'Specific/Specific' : (P1*Q1) or (P2*Q1)
		/// </summary>
		IDutyResult GetSpecificSpecific(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.TotalAmount = GetLocalAmount(wrapper.SelectedP * wrapper.Q1).Round(2);
			result.PerUnitAmount = wrapper.SelectedP;
			result.PerUnitUQ = wrapper.UQ1;
			result.RateString = GetAmountPerUnitString(result.PerUnitAmount, wrapper.UQ1);
			return result;
		}

		/// <summary>
		/// Calculation D 'Compound (Specific + Ad Valorem)' : (P1*Q3)+(P2*Value)
		/// </summary>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetCompoundSpecificAdValorem(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.PercentOfValue = wrapper.P2 * 100;
			result.PerUnitAmount = wrapper.P1;
			result.PerUnitUQ = wrapper.UQ3;
			result.TotalAmount = GetLocalAmount(wrapper.P1 * wrapper.Q3 + wrapper.P2 * dutyData.GetEffectiveCustomsValue()).Round(2);
			result.NoneCustomsValueAmount = GetLocalAmount(wrapper.P1 * wrapper.Q3).Amount;
			result.RateString = CombineTwoRateStrings(GetAmountPerUnitString(wrapper.P1, wrapper.UQ3), GetPercentage(wrapper.P2));
			return result;
		}

		/// <summary>
		/// Calculation E 'Specific + Compound' : (P1*Q2)+(P3*Q3)+ (P2*Value)
		/// </summary>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetSpecificCompound(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			result.PercentOfValue = wrapper.P2 * 100;
			result.TotalAmount = GetLocalAmount(wrapper.P1 * wrapper.Q2 + wrapper.P3 * wrapper.Q3 + wrapper.P2 * dutyData.GetEffectiveCustomsValue()).Round(2);
			result.NoneCustomsValueAmount = GetLocalAmount(wrapper.P1 * wrapper.Q2 + wrapper.P3 * wrapper.Q3).Amount;
			ZString rate = CombineTwoRateStrings(GetAmountPerUnitString(wrapper.P1, wrapper.UQ2), GetAmountPerUnitString(wrapper.P3, wrapper.UQ3));
			result.RateString = CombineTwoRateStrings(rate, GetPercentage(wrapper.P2));
			return result;
		}

		/// <summary>
		/// Calculation F 'Specific/ Pyrotechnics' : Q1*(P1+P3*Q2)
		/// </summary>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetSpecificPyrotechnics(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();

			ZDecimal rate = wrapper.P1 + wrapper.P3 * wrapper.Q2;
			result.TotalAmount = GetLocalAmount(rate * wrapper.Q1).Round(2);
			result.RateString = GetAmountPerUnitString(rate, wrapper.UQ1);
			return result;
		}

		/// <summary>
		/// Calculation J - 'Specific/Sugar' : Greater of Q2*(P1-P2*(100-Q3)) or P3*Q2
		/// </summary>
		/// <param name="tariff"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetSpecificSugarJ(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			ZDecimal option1Rate = wrapper.P1 - wrapper.P2 * (100 - wrapper.Q3);
			ZDecimal option1 = wrapper.Q2 * option1Rate;
			ZDecimal option2 = wrapper.P3 * wrapper.Q2;
			if (option1 > option2)
			{
				result.TotalAmount = GetLocalAmount(option1).Round(2);
				result.RateString = GetAmountPerUnitString(option1Rate, wrapper.UQ2);
			}
			else
			{
				result.PerUnitAmount = wrapper.P3;
				result.PerUnitUQ = wrapper.UQ2;
				result.TotalAmount = GetLocalAmount(option2).Round(2);
				result.RateString = GetAmountPerUnitString(wrapper.P3, wrapper.UQ2);
			}
			return result;
		}

		/// <summary>
		/// Calculation K - 'Specific/Sugar' : Greater of Q1*(P1-P2*(100-Q2)) or P3 * Q1
		/// </summary>
		/// <param name="tariff"></param>
		/// <param name="wrapper"></param>
		/// <returns></returns>
		IDutyResult GetSpecificSugarK(PQWrapper wrapper)
		{
			DutyResult result = new DutyResult();
			ZDecimal option1Rate = wrapper.P1 - wrapper.P2 * (100 - wrapper.Q2);
			ZDecimal option1 = wrapper.Q1 * option1Rate;
			ZDecimal option2 = wrapper.P3 * wrapper.Q1;
			if (option1 > option2)
			{
				result.TotalAmount = GetLocalAmount(option1).Round(2);
				result.RateString = GetAmountPerUnitString(option1Rate, wrapper.UQ1);
			}
			else
			{
				result.PerUnitAmount = wrapper.P3;
				result.PerUnitUQ = wrapper.UQ1;
				result.TotalAmount = GetLocalAmount(option2).Round(2);
				result.RateString = GetAmountPerUnitString(wrapper.P3, wrapper.UQ1);
			}
			return result;
		}
		#endregion

		protected Money GetLocalAmount(ZDecimal amount)
		{
			if (amount.IsEmpty)
			{
				return Money.Empty;
			}
			else
			{
				return new Money(amount, JobDeclaration.GetLocalCurrency());
			}
		}

		protected ZString GetAmountPerUnitString(ZDecimal ratePerUQ, ZString uq)
		{
			return ratePerUQ > 0m ? IFeeCalculationDataProviderExtensionMethods.AmountPerUnit(ratePerUQ, uq) : ZString.Empty;
		}

		internal static ZString CombineTwoRateStrings(ZString firstPart, ZString secondPart)
		{
			firstPart = firstPart.EqualsIgnoringCase(US.Business.DutyResult.DutyFreeString) ? ZString.Empty : firstPart;
			secondPart = secondPart.EqualsIgnoringCase(US.Business.DutyResult.DutyFreeString) ? ZString.Empty : secondPart;

			bool bothHaveRate = !firstPart.IsEmpty && !secondPart.IsEmpty;

			return firstPart + (bothHaveRate ? " + " : "") + secondPart;
		}
	}
}
