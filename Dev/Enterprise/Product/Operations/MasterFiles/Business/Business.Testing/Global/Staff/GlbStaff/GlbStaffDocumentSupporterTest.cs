using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffDocumentSupporter))]
	sealed class GlbStaffDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext.GlbStaff is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Constants.DataContext.GlbStaff))));
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<GlbStaff>();
		}

		GlbStaffDocumentSupporter DocumentSupporter
		{
			get { return new GlbStaffDocumentSupporter(Factory.New<GlbStaff>()); }
		}

		#endregion
	}
}
