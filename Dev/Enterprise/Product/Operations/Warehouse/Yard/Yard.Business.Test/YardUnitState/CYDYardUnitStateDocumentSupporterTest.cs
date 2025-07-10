using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDYardUnitStateDocumentSupporter))]
	public class CYDYardUnitStateDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CYDYardUnitState)));
		}

		public void TestGetDocBusinessObject()
		{
			var wrappers = DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CYDYardUnitState, null);

			AssertNotNull(wrappers);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CYDYardUnitState, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter = documentSupporter = new CYDYardUnitStateDocumentSupporter((CYDYardUnitState)GetDocumentSupportableBusinessObject()); }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in DocumentSupporter")]
		DocumentSupporter documentSupporter;

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDYardUnitState>();
		}

		#endregion
	}
}
