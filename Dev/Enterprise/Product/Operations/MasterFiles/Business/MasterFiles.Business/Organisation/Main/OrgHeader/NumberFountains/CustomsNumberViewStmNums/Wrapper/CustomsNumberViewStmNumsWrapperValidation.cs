using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsWrapperValidation : ZValidation
	{
		public CustomsNumberViewStmNumsWrapperValidation(CustomsNumberViewStmNumsWrapper parent)
			: base(parent)
		{
			this.ParentListInternals = parent;
		}

		public override Type AutoValidationType => typeof(CustomsNumberViewStmNumsWrapper);

		protected CustomsNumberViewStmNumsWrapper Parent
		{
			get { return (CustomsNumberViewStmNumsWrapper)base.ParentFilter; }
		}

		public sealed override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateAllCore();
			}
		}

		protected virtual void ValidateAllCore()
		{
		}

		readonly ISingleElementListInternal ParentListInternals;
	}
}
