using System.IO;

using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Recruiter.Business
{
	internal abstract class ExamQuestionsImporter
	{
		public static ExamQuestionsImporter New(ExamSurveyQuestionImporterBizO importerBizO)
		{
			return importerBizO.IsXmlFile
				? new XmlExamQuestionsImporter(importerBizO)
				: new CsvExamQuestionsImporter(importerBizO);
		}

		protected ExamQuestionsImporter(ExamSurveyQuestionImporterBizO importerBizO)
		{
			this.importerBizO = importerBizO;
		}

		public int ImportQuestions(INotifications notificationSubscriber)
		{
			Argument.NotNull(notificationSubscriber, "notificationSubscriber");

			int result = 0;

			if (importerBizO.ShouldClearExistingQuestions)
			{
				ClearExistingQuestions();
			}

			try
			{
				result = ImportQuestionsCore(notificationSubscriber);
			}
			catch (IOException ex)
			{
				notificationSubscriber.Notify(new ErrorNotification(ErrorType.IOError, ex.Message));
			}

			return result;
		}

		void ClearExistingQuestions()
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(importerBizO.campaign.Factory))
			{
				foreach (VoteExamSurveyQuestion question in importerBizO.campaign.Questions.ToArray())
				{
					question.IsActiveForBinding = false;
				}
			}
		}

		protected abstract int ImportQuestionsCore(INotifications notificationSubscriber);

		public readonly ExamSurveyQuestionImporterBizO importerBizO;
	}
}
