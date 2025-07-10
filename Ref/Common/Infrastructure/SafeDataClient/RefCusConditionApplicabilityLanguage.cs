using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	[NonPersistentObject]
	public class RefCusConditionApplicabilityLanguage : INonPersistentBusinessObject
	{
		public RefCusConditionApplicabilityLanguage(INonPersistentBusinessObjectFlatten topLevelNonPersistentObject)
		{
			S09_PK = Guid.NewGuid();
			TopLevelNonPersistentObjects.Add(topLevelNonPersistentObject);
		}

		public RefCusConditionApplicabilityLanguage(RefCusConditionLanguage lang, INonPersistentBusinessObjectFlatten topLevelNonPersistentObject) : this(topLevelNonPersistentObject)
		{
			S09_ZX6_NKLanguage = lang.ZXJ_ZX6_NKLanguage;
			S09_Comment = lang.ZXJ_Comment;
			S09_Source = lang.ZXJ_Source;
			S09_AdditionalComment = lang.ZXJ_AdditionalComment;
			RefCusConditionLanguage = lang;
			lang.RefCusConditionApplicabilityLanguages.Add(this);
		}

		public void Link(RefCusConditionLanguage lang)
		{
			RefCusConditionLanguage = lang;
			if (!lang.RefCusConditionApplicabilityLanguages.Contains(this))
			{
				lang.RefCusConditionApplicabilityLanguages.Add(this);
			}
		}

		public void Update()
		{
			var lang = RefCusConditionLanguage;
			lang.ZXJ_ZX6_NKLanguage = S09_ZX6_NKLanguage;
			lang.ZXJ_Comment = S09_Comment;
			lang.ZXJ_Source = S09_Source;
			lang.ZXJ_AdditionalComment = S09_AdditionalComment;
		}

		public IEnumerable<object> Unlink()
		{
			var lang = RefCusConditionLanguage;
			lang?.RefCusConditionApplicabilityLanguages.Remove(this);
			RefCusConditionLanguage = null;
			return new [] { lang };
		}

		public ICollection<INonPersistentBusinessObjectFlatten> TopLevelNonPersistentObjects { get; } = new HashSet<INonPersistentBusinessObjectFlatten>();

		public Guid S09_PK { get; set; }
		public Guid S09_S07_ConditionApplicability { get; set; }
		public string S09_ZX6_NKLanguage { get; set; }
		public string S09_Comment { get; set; }
		public string S09_Source { get; set; }
		public string S09_AdditionalComment { get; set; }
		public virtual RefCusConditionLanguage RefCusConditionLanguage { get; set; }
	}
}
