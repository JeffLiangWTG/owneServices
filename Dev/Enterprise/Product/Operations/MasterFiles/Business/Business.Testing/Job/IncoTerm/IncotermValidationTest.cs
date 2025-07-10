using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IncotermValidationTest : TestCaseWithFactory
	{
		#region Tests

		[TestDate(2019, 1, 1)]
		public void TestWarningIfExpiredPre2020()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => IncotermValidation.Instance.WarningIfExpired(null));

			DummyBizOWithIncoTerm bizOWithIncoTerm = Factory.New<DummyBizOWithIncoTerm>();
			AssertNoWarnings(bizOWithIncoTerm.Z0_IncotermInfo);

			bizOWithIncoTerm.Z0_Incoterm = "FOB";
			AssertNoWarnings(bizOWithIncoTerm.Z0_IncotermInfo);

			foreach (ZString expiringIncoterm in new ZString[] { "DAF", "DES", "DEQ", "DDU" })
			{
				bizOWithIncoTerm.Z0_Incoterm = expiringIncoterm;
				AssertHasWarning(bizOWithIncoTerm.Z0_IncotermInfo, "This Incoterm is obsolete from 1 January 2011 according to the Incoterms 2010 rules.");
			}

			bizOWithIncoTerm.Z0_Incoterm = "DAT";
			AssertNoWarnings(bizOWithIncoTerm.Z0_IncotermInfo);
		}

		[TestDate(2020, 1, 1)]
		public void TestWarningIfExpired2020()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => IncotermValidation.Instance.WarningIfExpired(null));

			var bizOWithIncoTerm = Factory.New<DummyBizOWithIncoTerm>();
			AssertNoWarnings(bizOWithIncoTerm.Z0_IncotermInfo);

			bizOWithIncoTerm.Z0_Incoterm = IncoTerms.FreeOnBoard;
			AssertNoWarnings(bizOWithIncoTerm.Z0_IncotermInfo);

			bizOWithIncoTerm.Z0_Incoterm = IncoTerms.DeliveredAtTerminal;
			AssertHasWarning(bizOWithIncoTerm.Z0_IncotermInfo, "This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules.");

			bizOWithIncoTerm.Z0_Incoterm = IncoTerms.ExWorks;
			AssertNoWarnings(bizOWithIncoTerm.Z0_IncotermInfo);
		}

		#endregion
	}
}
