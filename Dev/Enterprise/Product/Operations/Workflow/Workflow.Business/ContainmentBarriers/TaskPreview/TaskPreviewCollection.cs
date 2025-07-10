using System;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class TaskPreviewCollection : NonPersistentBusinessObjectCollection<TaskPreviewCopy>
	{
		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}
