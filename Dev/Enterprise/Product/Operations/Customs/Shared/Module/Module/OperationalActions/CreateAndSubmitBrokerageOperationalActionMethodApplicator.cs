
namespace Enterprise.Customs.Module.OperationalActions
{
	public class CreateAndSubmitBrokerageOperationalActionMethodApplicator : CreateBrokerageJobOperationalActionMethodApplicator
	{
		public CreateAndSubmitBrokerageOperationalActionMethodApplicator()
			: base(Res.GetString("7283cfe0-f675-42b1-90e4-5dd66615394c", "Create and Submit Brokerage Job operational action"))
		{
			actionRunner.ExecuteSubmit = true;
		}
	}
}
