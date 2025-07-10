using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocumentDocumentSupporter))]
	sealed class JobRequiredDocumentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContexts()
		{
			var header = Factory.New<JobRequiredDocument>();
			AssertEquals("DataContext.Organisation is not Supported", false, header.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Organisation)));
		}

		public void TestShowReasonForNotPrinting()
		{
			var header = Factory.New<JobRequiredDocument>();
			AssertEquals(false, header.DocumentSupporter.ShowReasonForNotPrinting(DataContext.Notes, null));
			AssertEquals(false, header.DocumentSupporter.ShowReasonForNotPrinting(DataContext.None, null));
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var header = Factory.New<JobRequiredDocument>();
			AssertEquals(Env.Security.OrganisationCustomiseDocuments, header.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestTestBusinessContext()
		{
			var header = new JobRequiredDocumentDocumentSupporter(Factory.NewWithValidTestData<JobRequiredDocument>());
			AssertEquals(BusinessContext.JobRequiredDocument, header.BusinessContext);
		}

		public void TestGetDocumentWrappers()
		{
			var header = Factory.NewWithValidTestData<JobRequiredDocument>();
			var wrapper = header.DocumentSupporter.GetDocumentWrappers(DataContext.Organisation, null);
			AssertNull(wrapper);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<JobRequiredDocument>();
		}

		public void TestConstructionFunction()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var requiredDocument = orgHeader.RequiredDocuments.AddNew();
			var supporter = new JobRequiredDocumentDocumentSupporterForTest(orgHeader);
			AssertEquals(orgHeader, supporter.OrgHeaderForTest);
			AssertNull(supporter.RequiredDocumentForTest);

			supporter = new JobRequiredDocumentDocumentSupporterForTest(requiredDocument);
			AssertEquals(orgHeader, supporter.OrgHeaderForTest);
			AssertEquals(requiredDocument, supporter.RequiredDocumentForTest);
		}
	}
}
