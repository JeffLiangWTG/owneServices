using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business
{
	[TestedType(typeof(TallyContainerDocumentSupporter))]
	class TallyContainerDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<TallyContainer>();
		}

		public void TestSupportedDataContext()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			AssertEquals("Core.Constants.DataContext.PackUnpackContainerRego is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.PackUnpackContainerRego)));
			AssertEquals("Core.Constants.DataContext.TallyContainer is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.TallyContainer)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
		}

		public void TestGetDocumentWrappers()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			DocumentWrapper[] wrapper = container.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.PackUnpackContainerRego, null);
			AssertEquals("wrapper should be DocTallyContainer", "DocTallyContainer", wrapper[0].GetType().Name);
			wrapper = container.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.TallyContainer, null);
			AssertEquals("wrapper should be DocTallyContainer", "DocTallyContainer", wrapper[0].GetType().Name);
			wrapper = container.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertEquals("wrapper should be FreightWrapperFromCFSContainer", "FreightWrapperFromCFSContainer", wrapper[0].GetType().Name);
		}
	}
}
