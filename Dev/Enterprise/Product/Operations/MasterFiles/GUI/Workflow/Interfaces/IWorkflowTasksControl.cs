using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public interface IWorkflowTasksControl : IWorkflowItemsControl
	{
		KSplitContainer TasksHintSplitContainer { get; }
		ZLabel TasksHintLabel { get; }
	}

	public static class WorkflowTasksControlExtensions
	{
		public static IDisposable SetupTasksControl(this IWorkflowTasksControl control, IWorkflowProviderCollection collection, bool saveOrder)
		{
			SetupTaskOwnerPasswordService(collection.WorkflowItems);
			return SetupControl(control, collection, saveOrder, GetTaskHints);
		}

		/// <summary>
		/// Note: this is broken for controls who are bound to binding members
		/// </summary>
		public static IDisposable SetupControl(this IWorkflowTasksControl control, IWorkflowProviderCollection collection, bool saveOrder, Func<IWorkflowProviderCollection, string[]> getHints = null)
		{
			ShowTaskHints(control, collection, getHints);
			return IWorkflowItemsControlExtensions.SetupSortOnRebuild(control, saveOrder: saveOrder);
		}

		static void SetupTaskOwnerPasswordService(ProcessTaskCollection taskCollection)
		{
			var service = taskCollection.Factory.ServiceContainer.GetService<TaskOwnerPasswordEventService>()
				?? taskCollection.Factory.ServiceContainer.AddService(new TaskOwnerPasswordEventService());

			service.UnhookPasswordRequestEvent();
			service.HookPasswordRequestEvent(taskCollection);
		}

		static void ShowTaskHints(IWorkflowTasksControl control, IWorkflowProviderCollection taskCollection, Func<IWorkflowProviderCollection, string[]> getHints)
		{
			var hints = GetHints(taskCollection).Concat(getHints?.Invoke(taskCollection) ?? Enumerable.Empty<string>());
			if (hints.Any())
			{
				control.TasksHintSplitContainer.Panel1Collapsed = false;
				control.TasksHintLabel.Text = string.Join(System.Environment.NewLine, hints);
			}
			else
			{
				control.TasksHintSplitContainer.Panel1Collapsed = true;
			}
		}

		static string[] GetHints(IWorkflowProviderCollection collection)
		{
			if (collection is WorkflowItemCollectionIncludingRelatedView view && !view.CanShowRelatedItems())
			{
				var reg = WorkflowDataRegistry.Instance.RelatedWorkflowItemDisplayLimit;
				return new[] { Res.GetString("dd84f7bf-fde7-4302-8b31-6f7c74aeea3b", "Related items are not shown as more than {0} related items were found. (Configurable via {1})", reg.Value, reg.HumanReadableRegistryPath()) };
			}
			else
			{
				return Array.Empty<string>();
			}
		}

		static string[] GetTaskHints(IWorkflowProviderCollection collection)
		{
			var taskCollection = collection.WorkflowItems;
			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, taskCollection.Parent.PK).AddToFilter(ProcessTasksSchema.P9_ParentTableCode, taskCollection.Parent.TablePrefix);
			var allTasksInJob = taskCollection.Factory.Load<ProcessTask>(query).Where(t => t.IsTask).ToArray();
			var thereAreHiddenTasks = allTasksInJob.Length != taskCollection.Tasks.Cast<ProcessTask>().Count();

			var result = new List<string>();
			if (thereAreHiddenTasks)
			{
				var otherCompanies = allTasksInJob.Where(t => t.P9_GC != Env.CurrentCompanyPK).OrderBy(t => t.P9_Sequence).GroupBy(t => t.Company).Where(x => x.Key != null).ToArray();
				if (otherCompanies.Length > 0)
				{
					if (otherCompanies.Length == 1)
					{
						result.Add(Res.GetString("1126dba7-299a-4332-97e6-5328b02f5a20", "There are tasks in this job that are shown only when logged into {0} company.", otherCompanies[0].Key.GC_Code));
					}
					else
					{
						result.Add(Res.GetString("27613df1-03c6-4c10-b382-5c3b61b5b71b", "There are tasks in this job that are shown only when logged into {0} companies.", string.Join(", ", otherCompanies.Select(c => c.Key.GC_Code))));
					}
				}
			}

			return result.ToArray();
		}
	}
}
