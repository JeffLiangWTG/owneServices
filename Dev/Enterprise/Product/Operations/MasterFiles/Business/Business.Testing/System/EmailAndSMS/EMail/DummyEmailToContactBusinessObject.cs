using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class DummyEmailToContactBusinessObject : EmailToContactBusinessObject
	{
		public DummyEmailToContactBusinessObject(BusinessObject @object)
			: base(@object)
		{
		}

		protected override void SendEmailCore(bool systemCommunication)
		{
			IsSendEmailCalled = true;
		}

		public bool IsSendEmailCalled;

		public void FillWithInvalidData()
		{
			Attachment = "!@#";
			Cc = "!@#";
			FromEmailAddress = "!@#";
			Priority = "!@#";
			ToEmailAddress = "!@#";
		}

		public void FillWithValidData()
		{
			Attachment = "";
			Cc = "cc@cc.com";
			FromEmailAddress = "from@from.com";
			Priority = "MED";
			ToEmailAddress = "to@to.com";
		}
	}
}
