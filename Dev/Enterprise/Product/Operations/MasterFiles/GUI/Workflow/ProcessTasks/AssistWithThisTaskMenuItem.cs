using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class AssistWithThisTaskMenuItem : ZMenuItem
	{
		public AssistWithThisTaskMenuItem(IProcessTask task, Func<IBusinessObjectCollection> collectionGetter = null, bool saveAfterActions = false)
			: base(GetName())
		{
			this.task = task;
			this.collectionGetter = collectionGetter;
			this.saveAfterActions = saveAfterActions;
			this.Click += MenuItemClick;
		}

		static ResourceString GetName()
		{
			return ResString.GetMultilingualString("5ffd6645-f6bf-4542-914f-b5f2e97091b6", "Assist With This Task");
		}

		void MenuItemClick(object sender, EventArgs e)
		{
			AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask assistTask);
			assistTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			collectionGetter?.Invoke().Add(assistTask);

			if (saveAfterActions)
			{
				((BusinessObject)assistTask).Factory.Save();
			}
		}

		readonly IProcessTask task;
		readonly Func<IBusinessObjectCollection> collectionGetter;
		readonly bool saveAfterActions;

		public static bool ShouldAddMenuItem(IProcessTask task)
		{
			return AssistWithThisTaskHelper.CanBeUsedForTargetTaskWhenCreatingAssistanceTaskForCurrentUser(task);
		}

		public static IProcessTask GetSingleSelectedTask(ZGrid parentGrid)
		{
			BusinessObject[] selectedElements = parentGrid.SelectedElements.Length != 0
				? parentGrid.SelectedElements
				: new[] { parentGrid.GetCurrent() };

			return selectedElements.Length == 1 ? (IProcessTask)selectedElements.Single() : null;
		}
	}
}
