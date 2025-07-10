using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ITemplateApplicationRaceConditionHandler
	{
		event EventHandler OnAfterLock;
		event EventHandler OnBeforeLock;
		event EventHandler OnBeforeProcess;

		bool ShouldProcess();
		void Process(IEnumerable<BusinessObject> businesObjects);
	}
}
