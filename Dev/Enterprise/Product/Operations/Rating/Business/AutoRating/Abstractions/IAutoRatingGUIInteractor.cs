using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public interface IAutoRatingGUIInteractor : Enterprise.Integration.Rating.IAutoRatingInteractor, ILoggerWithSession
	{
		void ShowException(Exception ex); //TODO: Remove this
		void SuspendLayout();
		void ResumeLayout();
		void ShowPossibleMatchesDialog(AutoRater.AdditionalRatesNotifications notifications);
		Quote SelectQuote(QuoteCollection quotes);
		ZDialogResult ShowNamedAccountMessageBox(string jobNamedAccount, string rateNamedAccount);
		void SetJobInvoicingSecurityOverrideProvider(JobHeader job);
		void ReportProgress(string currentProcessName, int totalItems, int done, TimeSpan eta, decimal speedPerTick);
		IDisposable DeferErrorPopup();

		/// <summary>
		/// Prompt user to manually select costs.
		/// Only called if IRatingContext.IsManualCostSelectMode is true.
		/// </summary>
		/// <exception cref="AutoRater.RatingCancelledException">thrown when user cancels</exception>
		/// <returns>selected costs</returns>
		IEnumerable<AutoRateInfo> SelectRate(IRatingContext ratingContext, RatingCriteria criteria);
	}

	public interface ILoggerWithSession : ILogger
	{
		void ShowRatesAdditionResult(AutoRatesAdditionResult additionResult, string entityName, CostSell costOrSell);
		IDisposable StartRatingSession();
	}
}
