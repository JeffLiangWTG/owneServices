using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class QuestionCategory : AutoQuestionCategory
	{
		public const string DefaultCode = "DEF";
		public static string DefaultDescription { get { return Res.GetString("487cf8ab-fd84-4fec-8dbb-1b456b18b9ac", "Default Category"); } }

		public QuestionCategory(LearningCentreCampaign campaign)
			: base(campaign.Factory)
		{
			this.campaign = campaign;
		}

		public override ZString Code
		{
			get { return base.Code; }
			set
			{
				if (base.Code != value)
				{
					using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
					{
						foreach (LearningCentreQuestion scaleRange in ScaleRanges.ToArray())
						{
							scaleRange.HY_QuestionCategory = value;
						}
					}

					base.Code = value;
					((IActiveBusinessObjectCollection)ScaleRanges).Refresh();
				}
			}
		}

		public LearningCentreQuestionCollection Questions
		{
			get
			{
				ZQuery filter = new ZQuery(VoteExamSurveyQuestionSchema.HY_QuestionCategory, Code);
				return new LearningCentreQuestionCollection(campaign, filter);
			}
		}

		[ChildEditable(true)]
		public ScaleRangeCollection ScaleRanges
		{
			get
			{
				if (scaleRanges == null)
				{
					scaleRanges = new ScaleRangeCollection(this);
					RegisterEditableChildObject(scaleRanges);
				}
				return scaleRanges;
			}
		}

		ScaleRangeCollection scaleRanges;
		public readonly LearningCentreCampaign campaign;
	}
}
