using System;

namespace Enterprise.Recruitment.Registry
{
	public interface IRecruitmentRegistry
	{
		bool RecruitmentModuleEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		string ConvertApiSecretKey
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool RunConvertApplicationStatusToCandidateRating
		{
			get;
#if DEBUG
			set;
#endif
		}

		DateTime ResumeBacklogConversionHighWaterMark
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool IsExamsModuleEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool IsAccreditationModuleEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool IsLearningCentreModuleEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}
	}
}
