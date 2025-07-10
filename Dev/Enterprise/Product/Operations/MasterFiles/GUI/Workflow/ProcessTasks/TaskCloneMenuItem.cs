using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Workflow.ProcessTasks
{
	class TaskCloneMenuItem : CloneMenuItem
	{
		public TaskCloneMenuItem(ZGrid parentGrid, Func<IBusinessObjectCollection> collectionGetter, Shortcut shortcut)
			: base(parentGrid, collectionGetter, shortcut)
		{
		}

		protected override void OnElementsCloned(Collection<CloneResult> cloneResults)
		{
			base.OnElementsCloned(cloneResults);

			this.parentGrid.CurrentCell = new DataGridCell(this.parentGrid.List.Count - cloneResults.Count, 0);
		}
	}
}
