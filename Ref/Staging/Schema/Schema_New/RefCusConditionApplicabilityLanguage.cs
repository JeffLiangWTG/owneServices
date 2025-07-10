using System;
using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	[NonPersistentObject]
	public class RefCusConditionApplicabilityLanguage
	{
		public RefCusConditionApplicabilityLanguage()
		{
			S09_PK = Guid.NewGuid();
		}

		public RefCusConditionApplicabilityLanguage(RefCusConditionLanguage lang)
		{
			S09_ZX1_Condition = lang.ZXJ_ZX1_Condition;
			S09_ZX6_NKLanguage = lang.ZXJ_ZX6_NKLanguage;
			S09_Comment = lang.ZXJ_Comment;
			S09_Source = lang.ZXJ_Source;
			S09_AdditionalComment = lang.ZXJ_AdditionalComment;
		}

		public Guid S09_PK { get; set; }
		public Guid S09_S07_ConditionApplicability { get; set; }
		public Guid S09_ZX1_Condition { get; set; }
		public string S09_ZX6_NKLanguage { get; set; }
		public string S09_Comment { get; set; }
		public string S09_Source { get; set; }
		public string S09_AdditionalComment { get; set; }

		public RefCusCondition RefCusCondition { get; set; }
	}
}
