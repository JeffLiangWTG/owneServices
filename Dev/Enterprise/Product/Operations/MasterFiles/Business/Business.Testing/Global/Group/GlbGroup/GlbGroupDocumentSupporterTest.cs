using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupDocumentSupporter))]
	sealed class GlbGroupDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext.GlbGroup is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Constants.DataContext.GlbGroup))));
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<GlbGroup>();
		}

		GlbGroupDocumentSupporter DocumentSupporter
		{
			get { return new GlbGroupDocumentSupporter(Factory.New<GlbGroup>()); }
		}

		#endregion
	}
}
