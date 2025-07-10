using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module.OperationalActions
{
	public class CreateBrokerageJobOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		protected CreateBrokerageJobOperationalActionMethodApplicator(string name)
			: base(name)
		{
			actionRunner = new CreateAndSubmitBrokerageJobRunner();
		}

		public CreateBrokerageJobOperationalActionMethodApplicator()
			: this(Res.GetString("6fe3e789-5e4d-4f84-9f94-2b101684d2d8", "Create Brokerage Job operational action"))
		{
			actionRunner.ExecuteSubmit = false;
		}

		protected CreateAndSubmitBrokerageJobRunner actionRunner;

		public CreateAndSubmitBrokerageJobRunner ActionRunner_Exposed
		{
			get
			{
				GetAnswerToQuestions();
				return actionRunner;
			}
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, CargoWise.EntityFramework.BusinessObject[] targets)
		{
			GetAnswerToQuestions();
			log.SetSectionProgressMax(targets.Length);
			actionRunner.Log = log;
			foreach (ForwardingShipment shipment in targets)
			{
				actionRunner.Execute(shipment);
				log.BumpSectionProgress();
			}
		}

		public ZBoolDescriptionPairList QuestionList
		{
			get
			{
				if (fQuestionList == null)
				{
					fQuestionList = new ZBoolDescriptionPairList();
					foreach (var question in actionRunner.AllQuestions)
					{
						fQuestionList.Add(new ZBoolDescriptionPair(question.Question, question.DefaultAnswer));
					}
				}
				return fQuestionList;
			}
		}
		ZBoolDescriptionPairList fQuestionList;

		void GetAnswerToQuestions()
		{
			for (int i = 0; i < QuestionList.Count; i++)
			{
				var questions = actionRunner.AllQuestions.ToList();
				questions[i].Answer = QuestionList[i].Value;
			}
		}
	}
}
