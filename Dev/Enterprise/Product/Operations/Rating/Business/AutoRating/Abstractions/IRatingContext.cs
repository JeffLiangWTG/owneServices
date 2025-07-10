using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public interface IRatingContext
	{
		IRatesProvider RatesProvider { get; }
		ILogger Logger { get; }
		IDialogService DialogService { get; }
		BusinessObjectFactory Factory { get; }
		bool SearchForRatesMode { get; set; }
		bool IsInRebateCalculationMode { get; set; }
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		Dictionary<ZGuid, HashSet<ZGuid>> PercentageLinesApplied { get; }
		string RawResponse { get; }
		bool IsManualCostSelectMode { get; }

		// For ProviderUrsRates, Both legacy Rate Service (WiseRatesProvider) and URS (UrsRatesProvider) are available. The one used depends on whether URS is enabled.
		// Note: The legacy Rate Service will be deprecated in the future, and URS will become the sole option.
		IUrsRatesProvider ProviderUrsRates { get; }
		ICW1RatesProvider ProviderCW1Rates { get; }

		/// <summary>
		///		Stores the calculated charges during autorating so far. Basically used to access customs charges during
		///		calculation of non-customs charges in case of CST/PER calculators which depend on other charges.
		///
		///		We have 2 different flows between calculation of customs and non-customs charges, so, need to use a hack
		///		like this to synchronize combined charges.
		///
		///		Please note, that we have AutoRating Results on AutoratingCalculatorParameters level as well, but
		///		AutoratingCalculatorParameters is created per adapter being autorated and thus has rating adapter
		///		scope rather than the whole autorating session scope.
		/// </summary>
		List<AutoRateInfo> InterimAutoRatingResults { get; }
		CalculationLogsWrapper RateCalculationLogWrapper { get; }
	}
}
