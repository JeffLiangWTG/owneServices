using System;
using System.Collections.Generic;
using System.Data;

namespace ReferenceDataUpdateService.Web.Test.WebUI.SearchForm
{
	public class PortalSearchFormFilter
	{
		public enum FilterOperation
		{
			Equals,
			Contains
		}
		public string Name { get; set; }
		public string Data { get; set; }
		public FilterOperation Operation { get; set; }
	}

	public interface IPortalSearchFormTest
	{
		/// <summary>
		/// Prepare data for the test in staging and safe databases
		/// </summary>
		/// <param name="stagingCommand"></param>
		/// <param name="safeCommand"></param>
		void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand);

		/// <summary>
		/// The list of data to fill in the filters on the search form
		/// Return an empty list if there are no filters
		/// </summary>
		/// <returns></returns>
		IEnumerable<PortalSearchFormFilter> GetFilters();

		/// <summary>
		/// The url of the Search Form page to test
		/// </summary>
		/// <returns></returns>
		Uri GetPageUrl();

		/// <summary>
		/// The data to compare with the result table on the UI
		/// Each item represents a row in the table on the UI
		/// For each row, the order of the columns should match the order of the columns in the table on the UI
		/// For text, number and boolean columns, use ToString().
		/// For link columns, use string representing its href value.
		/// </summary>
		/// <example>
		/// return new[]
		/// {
		///		new object[]
		///		{
		///		"CUSOF", "4701", "ZA", true.ToString(), "/RefCusCodeListUserViewDetailsForm/37ed2150-7da4-4130-a206-77e04f74edb3"
		///		}
		/// }
		/// </example>
		/// <returns></returns>
		IEnumerable<IEnumerable<string>> GetExpectedResult();
	}
}
