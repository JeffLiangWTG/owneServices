using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TransportContractDocumentWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTransportContractDocument()
		{
			ITransportContractDocument transportContractDocument = new TransportContractDocumentWrapper("A", "BB");
			NUnit.Framework.Assert.That(transportContractDocument.ID, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "TransportContractDocument.ID should be");
			NUnit.Framework.Assert.That(transportContractDocument.TypeCode, NUnit.Framework.Is.EqualTo("BB").Using(CustomComparers.TypeComparison), "TransportContractDocument.TypeCode should be");
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			ITransportContractDocument transportContractDocument = new TransportContractDocumentWrapper("A", "BB");
			NUnit.Framework.Assert.That(transportContractDocument.Deconsolidator, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
		}
	}
}
