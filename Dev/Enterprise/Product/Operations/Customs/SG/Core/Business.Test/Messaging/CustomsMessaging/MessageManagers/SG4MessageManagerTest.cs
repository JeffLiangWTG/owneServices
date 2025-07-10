namespace Enterprise.Customs.SG.V4.Business.Testing
{
	sealed class SG4MessageManagerTest : MessageManagerTest
	{
		protected override IMessageManager GetMessageManagerCore(JobDeclaration declaration)
		{
			return new SG4MessageManager(declaration);
		}
	}
}
