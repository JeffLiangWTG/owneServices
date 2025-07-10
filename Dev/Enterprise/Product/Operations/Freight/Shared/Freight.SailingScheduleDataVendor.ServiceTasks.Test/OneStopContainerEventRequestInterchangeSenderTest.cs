using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Freight.SailingScheduleDataVendor.ServiceTasks.Test
{
	sealed class OneStopContainerEventRequestInterchangeSenderTest : BaseInterchangeSenderTest
	{
		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		protected override EDIInterchange GetNewEDIInterchangeReadyToSend()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(GetNewEDIMessageReadyToSend());

			return new OneStopContainerEventRequestInterchangeProvider(messages).Interchanges[0];
		}

		protected override EDIMessage GetNewEDIMessageReadyToSend()
		{
			EDIMessage message = Helper.Container.ComTracMessages.AddNew();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = "BODYTEXT";
			message.EM_LinkTable = CommonContainer.Schema.TableName;
			Factory.Save();
			return message;
		}

		protected override BaseInterchangeSender GetNewSender()
		{
			return new OneStopContainerEventRequestInterchangeSender();
		}
	}
}
