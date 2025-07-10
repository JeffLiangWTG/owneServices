using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPickupHeaderDocumentSupporter))]
	public class CYDPickupHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CYDPickupHeader)));
		}

		public void TestGetDocBusinessObject()
		{
			var wrappers = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CYDPickupHeader, null);
			AssertNotNull(wrappers);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CYDPickupHeader, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter = documentSupporter = new CYDPickupHeaderDocumentSupporter((CYDPickupHeader)GetDocumentSupportableBusinessObject()); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in DocumentSupporter")]
		DocumentSupporter documentSupporter;

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDPickupHeader>();
		}

		#endregion
	}
}
