using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	class TemplateApplicationHasChangesStrategy
	{
		public bool JobHasChanges(IWorkflowProvider job, TemplateApplicationParameters parameters)
		{
			if (parameters?.IgnoreHasChanges == true)
			{
				return true;
			}
			else
			{
				var bizo = (BusinessObject)job;
				var suppressHasChangesForObjectsNotAffectingTemplateApplication = bizo.HasChanges && bizo.IsInDatabase && !WorkflowDataRegistry.Instance.ApplyTemplatesWhenEventsAreAddedToAJob.Value;
				using (suppressHasChangesForObjectsNotAffectingTemplateApplication ? new SuppressHasChangesForObjectsNotAffectingTemplateApplication() : null)
				{
					return bizo.HasChanges;
				}
			}
		}

		class SuppressHasChangesForObjectsNotAffectingTemplateApplication : IDisposable
		{
			public SuppressHasChangesForObjectsNotAffectingTemplateApplication()
			{
				disposables.Add(StmALog.SuppressHasChanges());
				disposables.Add(ProcessTask.UseHasChangesForTemplateApplication());
			}

			readonly List<IDisposable> disposables = new List<IDisposable>();

			public void Dispose()
			{
				foreach (var item in disposables)
				{
					item?.Dispose();
				}
			}
		}
	}
}
