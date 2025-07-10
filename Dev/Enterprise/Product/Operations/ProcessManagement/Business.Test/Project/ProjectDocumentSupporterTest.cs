using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectDocumentSupporter))]
	class ProjectDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.ProjectCustomiseDocuments, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestBusinessObject()
		{
			AssertEquals(BusinessContext.Project, DocumentSupporter.BusinessContext);
		}

		public void TestGetDocBusinessObject()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.Project, DocumentSupporter.BusinessContext);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			base.TestRunningDocumentsShouldNotCauseException();
			Assert(true);
		}

		#region Implementation

		ProjectDocumentSupporter DocumentSupporter
		{
			get { return new ProjectDocumentSupporter((Project)GetDocumentSupportableBusinessObject()); }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<Project>();
		}

		#endregion
	}
}
