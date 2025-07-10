using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class RelatedBusinessSequenceNumberHeaderTest : TestCaseWithFactory
	{
		public void TestISequenceNumberHeader()
		{
			var organization = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(organization);
			var messageData = new OrgAddressMessageData(wrapper);
			var relatedBusinessCollection = new RelatedBusinessDataCollection(messageData);
			var relatedBO1 = relatedBusinessCollection.AddNew();
			var relatedBO2 = relatedBusinessCollection.AddNew();

			var sequenceNumberHeader = new RelatedBusinessSequenceNumberHeader(() => new TypedEnumerable<ISequenceNumberLine>(relatedBusinessCollection));
			AssertEquals(2, ((ISequenceNumberHeader)sequenceNumberHeader).Lines.Count());
			relatedBusinessCollection.RemoveAndDelete(relatedBO1);
			AssertEquals(1, ((ISequenceNumberHeader)sequenceNumberHeader).Lines.Count());

			var relatedBO3 = relatedBusinessCollection.AddNew();
			AssertEquals(2, ((ISequenceNumberHeader)sequenceNumberHeader).Lines.Count());
		}
	}
}
