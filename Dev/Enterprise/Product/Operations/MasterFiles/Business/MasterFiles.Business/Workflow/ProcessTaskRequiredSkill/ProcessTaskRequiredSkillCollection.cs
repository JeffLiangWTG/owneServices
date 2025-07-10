using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskRequiredSkillCollection : DependentBusinessObjectCollection<ProcessTaskRequiredSkill, ProcessTask>
	{
		public ProcessTaskRequiredSkillCollection(ProcessTask task) : base(task, GetAdditionaQuery(task))
		{
		}

		static ZQuery GetAdditionaQuery(ProcessTask task)
		{
			return task.IsTask ? new ZQuery() : ZQuery.NoResultQuery;
		}

		public void AddJobSkill(ZGuid jobSkillPK)
		{
			if (!this.Cast<ProcessTaskRequiredSkill>().Any(a => a.P9S_P9 == Master.PK && a.P9S_HS == jobSkillPK))
			{
				var item = AddNew();
				item.P9S_P9 = Master.PK;
				item.P9S_HS = jobSkillPK;
			}
		}

		public void AddAspect(Guid aspectPK)
		{
			if (!this.Cast<ProcessTaskRequiredSkill>().Any(a => a.P9S_P9 == Master.PK && a.P9S_Aspect == aspectPK))
			{
				var item = AddNew();
				item.P9S_P9 = Master.PK;
				item.P9S_WiseTechAcademySubjectCode = ZString.Empty;
				item.P9S_Aspect = aspectPK;
			}
		}

		public void RemoveJobSkill(ZGuid jobSkillPK)
		{
			this.Cast<ProcessTaskRequiredSkill>().FirstOrDefault(a => a.P9S_P9 == Master.PK && a.P9S_HS == jobSkillPK)?.Delete();
		}
	}
}
