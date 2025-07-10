using System;
using System.Collections.Generic;
using System.Net;
using Enterprise.Integration;
using Enterprise.Rating.Web.Model;

namespace Enterprise.Rating.Web
{
	/// <summary>
	/// ICWServiceProvider
	/// </summary>
	public interface ICWServiceProvider
	{
		/// <summary>
		/// GetCosts
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="query"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		IReadOnlyCollection<Rate> GetCosts(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger);

		/// <summary>
		/// GetCompanyTariffs
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="query"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		IReadOnlyCollection<Rate> GetCompanyTariffs(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger);

		/// <summary>
		/// GetIntercompanyTariffs
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="query"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		IReadOnlyCollection<Rate> GetIntercompanyTariffs(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger);

		/// <summary>
		/// GetClientRates
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="query"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		IReadOnlyCollection<Rate> GetClientRates(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger);

		/// <summary>
		/// GetJobCharges
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="query"></param>
		/// <param name="logger"></param>
		/// <param name="ratesAPIAutoRater"></param>
		/// <returns></returns>
		IReadOnlyCollection<Rate> GetJobCharges(string userName, string branchCode, string departmentCode, RateQuery query, ILogger logger, RatesAPIsAutoRater ratesAPIAutoRater = null);

		/// <summary>
		/// ReportUsage
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="endpoint"></param>
		/// <param name="status"></param>
		/// <param name="rates"></param>
		/// <param name="processTime"></param>
		void ReportUsage(string userName, string branchCode, string departmentCode, SourceEndpoint endpoint, HttpStatusCode status, IEnumerable<Rate> rates, TimeSpan processTime);
	}
}
