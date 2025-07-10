using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class StatusCalculator<ParentT> : NonPersistentBusinessObject, IObsoleteValidation where ParentT : BusinessObject, IStatusNeedsRecalculationProvider
	{
		protected StatusCalculator(ParentT parent) : base(parent.Factory)
		{
			this.Parent = parent;
		}

		public void DeriveStatusNow()
		{
			derivingStatus = true;
			DeriveStatus();
			derivingStatus = false;
			OnStatusCalculated();
		}

		public void DeriveStatusIfRequired()
		{
			if (!derivingStatus && Parent.StatusNeedsRecalculation)
			{
				DeriveStatusNow();
			}
		}

		#region Implementation

		protected virtual void OnStatusCalculated()
		{
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!Parent.IsDeleted)
			{
				if (AutoDeriveStatusOnFactorySaving)
				{
					DeriveStatusIfRequired();
				}
			}
		}

		protected virtual bool AutoDeriveStatusOnFactorySaving
		{
			get { return true; }
		}

		protected abstract void DeriveStatus();

		protected readonly ParentT Parent;

		protected bool derivingStatus;

		#endregion
	}
}
