using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[ModuleID(ModuleId.ExamSetting)]
	public class ExamSettingCollection : ActiveBusinessObjectCollection<ExamSetting>
	{
		readonly LearningCentreCampaign exam;

		public ExamSettingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ExamSettingCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ExamSettingCollection(LearningCentreCampaign exam)
			: this(exam.Factory, new ZQuery(ExamSettingSchema.EXS_G0, exam.PK))
		{
			this.exam = exam;
		}

		protected override void SetDefaultsForNewElementCore(ExamSetting newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (exam != null)
			{
				newElement.EXS_G0 = exam.PK;
			}
		}
	}
}
