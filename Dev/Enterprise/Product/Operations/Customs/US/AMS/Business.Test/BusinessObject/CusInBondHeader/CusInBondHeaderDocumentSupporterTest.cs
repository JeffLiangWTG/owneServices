using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderDocumentSupporter))]
	sealed class CusInBondHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("AMS Header cannot be found.", header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusInBondHeader), null));
			AssertEquals("AMS Header cannot be found.", header.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.Notes), null));
		}

		public void TestShowReasonForNotPrinting()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals(true, header.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.CusInBondHeader, null));
			AssertEquals(true, header.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.Notes, null));
			AssertEquals(false, header.DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		public void TestSupportedDataContexts()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("DataContext.CusInBondHeader is Supported", true, header.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CusInBondHeader)));
			AssertEquals("DataContext.Notes is Supported", true, header.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Notes)));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals(Env.Security.ConsolAMSReportingCustomiseDocuments, header.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetBODocDataProviders()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.InBondMovementHeaders.AddNew();
			var providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusInBondHeaderDocumentSupporter.CBPForm1302DataContextValue), null);
			AssertEquals(1, providers.Length);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MWB123";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MWB456";
			var moveHeader = header.InBondMovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			return header;
		}
	}
}

