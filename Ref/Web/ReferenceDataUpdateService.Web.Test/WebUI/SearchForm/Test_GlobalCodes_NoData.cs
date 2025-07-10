using System;
using System.Collections.Generic;
using System.Data;

namespace ReferenceDataUpdateService.Web.Test.WebUI.SearchForm
{
	public class Test_GlobalCodes_NoData : IPortalSearchFormTest
	{
		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
		}

		public IEnumerable<PortalSearchFormFilter> GetFilters()
		{
			return Array.Empty<PortalSearchFormFilter>();
		}

		public Uri GetPageUrl()
		{
			return new Uri("/RefCusCodeListUserViewSearchForm", UriKind.Relative);
		}

		public IEnumerable<IEnumerable<string>> GetExpectedResult()
		{
			return Array.Empty<IEnumerable<string>>();
		}
	}
}
