using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(TripDocumentSupporter))]
	sealed class TripDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestShowReasonForNotPrinting()
		{
			var trip = Factory.New<Trip>();
			AssertEquals(false, ((IDocumentSupportable)trip).DocumentSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		public void TestDocumentSupporter()
		{
			var trip = Factory.New<Trip>();
			var dataContextValue = new DataContextValue(".e-Manifest");
			Assert(((IDocumentSupportable)trip).DocumentSupporter.IsDataContextSupported(dataContextValue));
			AssertEquals("BusinessContext", BusinessContext.eManifest, ((IDocumentSupportable)trip).DocumentSupporter.BusinessContext);
			AssertEquals("Security", Env.Security.USeManifestCustomiseDocuments, ((IDocumentSupportable)trip).DocumentSupporter.CustomisationSecurityCheckpoint);
			var providers = ((IDocumentSupportable)trip).DocumentSupporter.GetBODocDataProviders(dataContextValue, null);
			AssertEquals("BODocDataProviders Count", 1, providers.Length);
			AssertEquals("BODocDataProvider Type", typeof(eManifestDocumentWrapper), providers[0].ParentBusinessObject.GetType());
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<Trip>();
	}
}
