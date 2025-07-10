using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Business
{
	public interface IRateLine : IIndexer, IRate, IGetZPropertyInfo, IFactoryProvider
	{
		ZGuid PK { get; }
		IRateEntry ParentRateEntry { get; }
		IEnumerable<IRateLineItem> ChildRateLineItems { get; }
		IList<IRateLine> IncludedLines { get; }
		Calculator Calculator { get; }

		CalculatorType RateCalculatorType { get; }

		ZString TL_RateCalculator { get; }
		ZString TL_Rounding { get; }
		AccChargeCode ChargeCode { get; }
		ZGuid TL_AC { get; }
		ZString TL_WeightVolume { get; }
		ZString TL_FeeChargeType { get; }
		ZString TL_FeeChargeLevel { get; }
		RefCurrency Currency { get; }
		ZByte TL_CompanyTariffLevel { get; }
		ZBool TL_IsOnPallets { get; }
		ZString TL_Condition { get; }
		ZBool TL_IsWhsJobLevelCharge { get; }
		ZGuid TL_OP_ProductNumber { get; }
		ZString TL_ConditionalExpression { get; }
		ZString TL_ContainerOwnership { get; }
		ZByte TL_ActualPercentage { get; }
		ConversionFactor ConversionFactor { get; }
		ConversionFactorViewModel ConversionFactorForBinding { get; }
		ZString TL_RX_NKCurrency { get; }
		ZDecimal TL_WeightVolumeMultiple { get; }
		ZDecimal TL_RoundingFactor { get; }
		ZDecimal RoundingFactor { get; }
		ZString TL_UnitFactor { get; }
		ZString TL_RateDesc { get; }
		ZString TL_RateDescLocal { get; }
		ZString Comment { get; }

		//ToDo: remove those below
		ZBool ViewAgentRates { get; set; }

		/// <summary>
		/// This is much faster then ViewAgentRates = value
		/// </summary>
		void SetViewAgentRatesWithoutRefreshBinding(bool viewAgent);

		bool IsCalculatorInitialized { get; }

		ZString UniversalChargeCodes { get; }

		ZString CarrierChargeCode { get; }
		ZString CarrierChargeCodeDescription { get; }
		ZString ChargeType { get; }
		ZString UnitMultipleAsString { get; }

		/// <summary>
		/// In the UI this is the 'public note' of a RateLine
		/// </summary>
		ZString ChargeInformationNoteText { get; }

		/// <summary>
		/// In the UI this is the 'internal note' of a RateLine
		/// </summary>
		ZString ChargeInternalNoteText { get; }

		ZDate TL_RateStartDate { get; }
		ZDate TL_RateEndDate { get; }
	}
}
