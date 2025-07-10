using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Module
{
	public class PRASettingsValidation : ZValidation
	{
		public PRASettingsValidation(PRASettings parent)
			: base(parent)
		{
			this.parent = parent;
			this.zValidationInternals = this;
			this.parentListInternals = parent;
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			IDisposable suspender = parentListInternals.SuspendListChanged();
			try
			{
				ValidateAllCore();
			}
			finally
			{
				suspender.Dispose();
			}
		}

		protected virtual void ValidateAllCore()
		{
			ValidateErrorBehaviour();
		}

		#endregion

		#region ValidateErrorBehaviour

		public void ValidateErrorBehaviour()
		{
			zValidationInternals.Validate(Parent.ErrorBehaviourInfo, new RunValidationInvoker(this.ErrorBehaviourValidationInvoker));
		}

		void ErrorBehaviourValidationInvoker()
		{
			CheckErrorBehaviourIsWesternEuropean();
			CheckErrorBehaviour();
		}

		protected virtual void CheckErrorBehaviourIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.ErrorBehaviourInfo);
		}

		protected virtual void CheckErrorBehaviour()
		{
			MandatoryValidation.CheckEntered(Parent.ErrorBehaviourInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ErrorBehaviourInfo, Parent.ErrorBehaviour_List);
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(PRASettingsValidation); }
		}

		public PRASettings Parent
		{
			get { return parent; }
		}
		readonly PRASettings parent;

		readonly IValidationInternals zValidationInternals;
		readonly ISingleElementListInternal parentListInternals;

		#endregion
	}
}
