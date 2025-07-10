using System;
using Enterprise.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class QueueOldTransactionsForEInvoicingCountryHelperTest : TestCase
	{
		public void TestGetQueueOldTransactionsCountryPKs()
		{
			var countryPKs = QueueOldTransactionsForEInvoicingCountryHelper.GetQueueOldTransactionsCountryPKs().ToList<Guid>();
			AssertEquals(1, countryPKs.Count);
			AssertEquals(Constants.CountryGuids.Romania, countryPKs[0]);
		}

		public void TestGetQueueNewTransactionsCountryCodes()
		{
			var countryCodes = QueueOldTransactionsForEInvoicingCountryHelper.GetQueueOldTransactionsCountryCodes().ToList<string>();
			AssertEquals(1, countryCodes.Count);
			AssertEquals(CountryCodes.Romania, countryCodes[0]);
		}
	}
}
