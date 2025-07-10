using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(EDIMessageForDisplayCollection<EDIMessage>))]
	class EDIMessageForDisplayCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var entry = Factory.New<CusEntryHeader>();
			var message1 = entry.Messages.AddNew();
			var message2 = entry.Messages.AddNew();

			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entry.PK);

			return new EDIMessageForDisplayCollection<EDIMessage>(Factory, query);
		}
	}
}
