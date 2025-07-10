using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWControllingMessageHeaderDocumentSupporter))]
	sealed class CusTWControllingMessageHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		[ExpectNoExceptions]
		public void TestBusinessContext()
		{
			var header = Factory.New<CusTWControllingMessageHeader>();
			NUnit.Framework.Assert.That(header.DocumentSupporter.BusinessContext, NUnit.Framework.Is.EqualTo(BusinessContext.TWControllingMessage), "var has a business context of CusContainer");
		}

		[ExpectNoExceptions]
		public void TestGetBODocDataProviders()
		{
			CombineAssertions("Message for N5116EDIMessage", () =>
			{
				var controllingMessageHeader = GetDocumentSupportableBusinessObject();
				var providers = controllingMessageHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusTWControllingMessageHeaderDocumentSupporter.NX101ControllingMessageHeaderPair), null);
				NUnit.Framework.Assert.That(providers.Length, NUnit.Framework.Is.EqualTo(1), "Provider for NX101ControllingMessageHeaderPair");
				providers = controllingMessageHeader.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".AA"), null);
				NUnit.Framework.Assert.That(providers, NUnit.Framework.Is.EqualTo(default(Enterprise.DocumentEngineCore.DocWrappers.IBODocDataProvider[])), "Provider for AA - should be [null]");
			});
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return header;
		}
	}
}
