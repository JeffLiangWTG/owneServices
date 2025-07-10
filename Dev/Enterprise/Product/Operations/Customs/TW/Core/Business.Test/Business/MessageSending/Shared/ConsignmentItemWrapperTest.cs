using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ConsignmentItemWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConsignmentItem_Split()
		{
			IConsignmentItem consignmentItem = new ConsignmentItemWrapper("");
			NUnit.Framework.Assert.That(consignmentItem.Split, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ConsignmentItem.Split should be");
			consignmentItem = new ConsignmentItemWrapper("P");
			NUnit.Framework.Assert.That(consignmentItem.Split, NUnit.Framework.Is.EqualTo("P").Using(CustomComparers.TypeComparison), "ConsignmentItem.Split should be");
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			IConsignmentItem consignmentItem = new ConsignmentItemWrapper("");
			NUnit.Framework.Assert.That(consignmentItem.Commodity, NUnit.Framework.Is.EqualTo(default(ICommodity)));
			NUnit.Framework.Assert.That(consignmentItem.GoodsMeasure, NUnit.Framework.Is.EqualTo(default(IGoodsMeasure)));
			NUnit.Framework.Assert.That(consignmentItem.Packaging, NUnit.Framework.Is.EqualTo(default(IPackaging)));
			NUnit.Framework.Assert.That(consignmentItem.TransportContractDocuments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ITransportContractDocument>)));
		}
	}
}
