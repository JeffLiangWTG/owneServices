using System;

namespace Enterprise.Recruitment.Registry
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "ObjectFactory will instantiate this")]
	class RecruitmentRegistryProvider : IRecruitmentRegistry
	{
		bool IRecruitmentRegistry.RecruitmentModuleEnabled
		{
			get => RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.Value;
#if DEBUG
			set => RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		string IRecruitmentRegistry.ConvertApiSecretKey
		{
			get => RecruitmentDataRegistry.Instance.ConvertApiSecretKey.Value;
#if DEBUG
			set => RecruitmentDataRegistry.Instance.ConvertApiSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IRecruitmentRegistry.RunConvertApplicationStatusToCandidateRating
		{
			get => RecruitmentDataRegistry.Instance.RunConvertApplicationStatusToCandidateRating.Value;
#if DEBUG
			set => RecruitmentDataRegistry.Instance.RunConvertApplicationStatusToCandidateRating.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		DateTime IRecruitmentRegistry.ResumeBacklogConversionHighWaterMark
		{
			get => RecruitmentDataRegistry.Instance.ResumeBacklogConversionHighWaterMark.Value;
#if DEBUG
			set => RecruitmentDataRegistry.Instance.ResumeBacklogConversionHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IRecruitmentRegistry.IsExamsModuleEnabled
		{
			get => RecruitmentDataRegistry.Instance.IsExamsModuleEnabled.Value;
#if DEBUG
			set => RecruitmentDataRegistry.Instance.IsExamsModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IRecruitmentRegistry.IsAccreditationModuleEnabled
		{
			get => RecruitmentDataRegistry.Instance.IsAccreditationModuleEnabled.Value;
#if DEBUG
			set => RecruitmentDataRegistry.Instance.IsAccreditationModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IRecruitmentRegistry.IsLearningCentreModuleEnabled
		{
			get => RecruitmentDataRegistry.Instance.IsLearningCentreModuleEnabled.Value;
#if DEBUG
			set => RecruitmentDataRegistry.Instance.IsLearningCentreModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}
	}
}
