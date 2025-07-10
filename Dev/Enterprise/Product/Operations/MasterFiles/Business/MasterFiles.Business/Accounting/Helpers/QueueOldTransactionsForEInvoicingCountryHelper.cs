using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public static class QueueOldTransactionsForEInvoicingCountryHelper
	{
		public static IEnumerable<Guid> GetQueueOldTransactionsCountryPKs()
		{
			return GetQueueOldTransactionsCountries().Select(x => x.CountryPK);
		}

		public static IEnumerable<string> GetQueueOldTransactionsCountryCodes()
		{
			return GetQueueOldTransactionsCountries().Select(x => x.CountryCode);
		}

		static IEnumerable<(Guid CountryPK, string CountryCode)> GetQueueOldTransactionsCountries()
		{
			return new (Guid, string)[] {
				(Constants.CountryGuids.Romania, CountryCodes.Romania)
			};
		}
	}
}
