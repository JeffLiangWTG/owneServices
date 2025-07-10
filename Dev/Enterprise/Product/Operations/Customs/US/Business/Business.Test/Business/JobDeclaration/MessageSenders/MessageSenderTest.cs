namespace Enterprise.Customs.US.Business.Testing
{
	public abstract class MessageSenderTest : Customs.Business.Testing.MessageSenderTest
	{
		public virtual void TestSendMessageIfAllOK()
		{
			foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
			{
				entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			}

			bool sentSuccessfuly = Sender.SendMessage();
			Assert(sentSuccessfuly);
			CargoWise.Common.ErrorReporter.Clear();
		}

		#region implementation

		protected abstract MessageSender Sender { get; }

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}
		protected abstract JobDeclaration Declaration { get; }

		#endregion
	}
}
