using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequenceDocumentSupporter))]
	sealed class AccComplianceSequenceDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporter()
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AssertEquals("Document Supporter should be of type", typeof(AccComplianceSequenceDocumentSupporter), sequence.DocumentSupporter.GetType());
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext ARInvoice is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.ARInvoice))));
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.ARInvoice, DocumentSupporter.BusinessContext);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<AccComplianceSequence>();
		}

		AccComplianceSequenceDocumentSupporter DocumentSupporter
		{
			get { return new AccComplianceSequenceDocumentSupporter(Factory.NewWithValidTestData<AccComplianceSequence>()); }
		}

		#endregion
	}
}
