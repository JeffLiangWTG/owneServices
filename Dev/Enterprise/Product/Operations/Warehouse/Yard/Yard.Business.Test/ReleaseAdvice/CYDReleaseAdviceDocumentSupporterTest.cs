using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReleaseAdviceDocumentSupporter))]
	class CYDReleaseAdviceDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CYDReleaseAdvice)));
		}

		public void TestGetDocBusinessObject()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CYDReleaseAdvice, null);
			AssertNotNull(wrappers);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CYDReleaseAdvice, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter = (documentSupporter = new CYDReleaseAdviceDocumentSupporter((CYDReleaseAdvice)GetDocumentSupportableBusinessObject())); }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in DocumentSupporter")]
		DocumentSupporter documentSupporter;

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDReleaseAdvice>();
		}

		#endregion
	}
}

