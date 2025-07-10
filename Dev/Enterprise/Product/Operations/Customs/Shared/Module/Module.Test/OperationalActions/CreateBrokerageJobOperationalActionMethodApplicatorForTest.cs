using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Module.OperationalActions.Testing
{
	sealed class CreateBrokerageJobOperationalActionMethodApplicatorForTest : CreateBrokerageJobOperationalActionMethodApplicator
	{
		public CreateBrokerageJobOperationalActionMethodApplicatorForTest() : base()
		{
			actionRunner = new CreateAndSubmitBrokerageJobRunnerForTest();
		}
	}

	sealed class CreateAndSubmitBrokerageJobRunnerForTest : CreateAndSubmitBrokerageJobRunner
	{
		protected override CreateDeclarationHelper GetCreateDeclarationHelperCore() => new CreateDeclarationHelperForTesting();
	}

	sealed class CreateDeclarationHelperForTesting : CreateDeclarationHelper
	{
		protected override ICollection<CreateBrokerageQuestion> GetQuestionsToCreateDeclarationCore()
		{
			return new List<CreateBrokerageQuestion>()
			{ new CreateBrokerageQuestion()
			{ Question = "Question1", DefaultAnswer = true, }, new CreateBrokerageQuestion()
			{ Question = "Question2", DefaultAnswer = false, } };
		}

		protected override ICollection<CreateBrokerageQuestion> GetQuestionsToCreateDeclarationWarningCore()
		{
			return new List<CreateBrokerageQuestion>()
			{ new CreateBrokerageQuestion()
			{ Question = "Question3", DefaultAnswer = true, }, new CreateBrokerageQuestion()
			{ Question = "Question4", DefaultAnswer = false, } };
		}

		protected override ICollection<CreateBrokerageQuestion> GetQuestionsToImportDecFromOtherCountryCore()
		{
			return new List<CreateBrokerageQuestion>()
			{ new CreateBrokerageQuestion()
			{ Question = "Question5", DefaultAnswer = true, }, new CreateBrokerageQuestion()
			{ Question = "Question6", DefaultAnswer = false, } };
		}
	}
}
