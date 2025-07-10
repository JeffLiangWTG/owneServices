using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(MessageActionRelatedRecordWrapper))]
	sealed class MessageActionRelatedRecordWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRecordTypeDescription()
		{
			var wrapper = new MessageActionRelatedRecordWrapper(MovementHeader);
			AssertEquals(SubApplicationCodeList.Descriptions.AMS, wrapper.RecordTypeDescription);
		}

		CusInBondMoveHeader MovementHeader
		{
			get
			{
				if (movementHeader == null)
				{
					var consol = Factory.New<ForwardingConsol>();
					var header = Factory.New<CusInBondHeader>();
					header.BH_ParentID = consol.PK;
					header.BH_ParentTableCode = consol.TablePrefix;
					movementHeader = header.MovementHeader;
				}
				return movementHeader;
			}
		}

		CusInBondMoveHeader movementHeader;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MessageActionRelatedRecordWrapper(MovementHeader);
		}
	}
}
