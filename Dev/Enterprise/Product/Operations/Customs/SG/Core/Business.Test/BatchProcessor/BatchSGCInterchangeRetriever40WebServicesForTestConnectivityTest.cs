using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class BatchSGCInterchangeRetriever40WebServicesForTestConnectivityTest : TestCaseWithFactory
	{
		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestRetrieveWithConnectivityErrors()
		{
			var digest = new StringEffectiveDate();
			digest.PreviousValue = "Mastication";
			digest.NewValue = "E73A42A55EF307A8F277D49BFBC90E40C4E378F97D1989D259BA13C28D6E591D";
			digest.EffectiveDate = ZDateTime.Now;
			var mhAccessVersion = new StringEffectiveDate();
			mhAccessVersion.PreviousValue = "FourZeroTwoOne";
			mhAccessVersion.NewValue = "4.0.4";
			mhAccessVersion.EffectiveDate = ZDateTime.Now;
			SGCustomsDataRegistry.Instance.DigestForLibs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, digest);
			SGCustomsDataRegistry.Instance.MhaccessVersionString.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mhAccessVersion);
			SGCustomsDataRegistry.Instance.VendorID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Daniel");
			var retriever = new BatchSGCInterchangeRetriever40WebServicesForTest();
			retriever.LoginAndRetrieveAndLogoutExposed();
			var lastLog = retriever.Logger.DebugLogStrings[1];
			AssertContains("Web sez no", lastLog);
			AssertContains("A connection attempt failed", lastLog);
			AssertNotContains("Exception", lastLog);
			AssertEquals(null, ErrorReporter.LastExceptionReported);
		}

		[TestDate(2017, 07, 25)]
		public void TestRetrieveWithNewValues()
		{
			SGCustomsDataRegistry.Instance.VendorID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Daniel");
			var retriever = new BatchSGCInterchangeRetriever40WebServicesForTest();
			retriever.LoginAndRetrieveAndLogoutExposed();
			var lastLog = retriever.Logger.DebugLogStrings[1];
			AssertContains("Web sez no", lastLog);
			AssertContains("A connection attempt failed", lastLog);
			AssertNotContains("Exception", lastLog);
			AssertEquals(null, ErrorReporter.LastExceptionReported);
		}
	}
}
