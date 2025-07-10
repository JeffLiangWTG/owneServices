using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class PersonIdentityInformationSequenceNumberHeaderTest : TestCaseWithFactory
	{
		public void TestISequenceNumberHeader()
		{
			var organization = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(organization);
			var messageData = new OrgAddressMessageData(wrapper);
			var piiCollection = new PersonIdentityInformationDataCollection(messageData);
			var pii1 = piiCollection.AddNew();
			var pii2 = piiCollection.AddNew();

			var sequenceNumberHeader = new PersonIdentityInformationSequenceNumberHeader(() => new TypedEnumerable<ISequenceNumberLine>(piiCollection));
			AssertEquals(2, ((ISequenceNumberHeader)sequenceNumberHeader).Lines.Count());
			piiCollection.RemoveAndDelete(pii1);
			AssertEquals(1, ((ISequenceNumberHeader)sequenceNumberHeader).Lines.Count());

			var pii3 = piiCollection.AddNew();
			AssertEquals(2, ((ISequenceNumberHeader)sequenceNumberHeader).Lines.Count());
		}
	}
}
