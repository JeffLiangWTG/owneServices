using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDAdHocServiceOrderDocumentSupporter))]
	public class CYDAdHocServiceOrderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			Assert(DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CYDAdHocServiceOrder)));
		}

		public void TestGetDocBusinessObject()
		{
			var wrappers = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CYDAdHocServiceOrder, null);
			AssertNotNull(wrappers);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CYDAdHocServiceOrder, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		DocumentSupporter DocumentSupporter => new CYDAdHocServiceOrderDocumentSupporter((CYDAdHocServiceOrder)GetDocumentSupportableBusinessObject());

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDAdHocServiceOrder>();
		}

		#endregion
	}
}
