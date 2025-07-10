using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class DocDataObjectSendingMessageSettingsValidation : ZValidation
	{
		public DocDataObjectSendingMessageSettingsValidation(DocDataObjectSendingMessageSettings parent)
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
			ValidateErrorAction();
		}

		#endregion

		#region ValidateOnErrorMessageError

		public void ValidateErrorAction()
		{
			zValidationInternals.Validate(Parent.ErrorActionInfo, new RunValidationInvoker(this.ErrorActionValidationInvoker));
		}

		void ErrorActionValidationInvoker()
		{
			CheckErrorActionIsWesternEuropean();
			CheckErrorAction();
		}

		protected virtual void CheckErrorActionIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.ErrorActionInfo);
		}

		protected virtual void CheckErrorAction()
		{
			MandatoryValidation.CheckEntered(Parent.ErrorActionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ErrorActionInfo, Parent.ErrorAction_List);
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(DocDataObjectSendingMessageSettingsValidation); }
		}

		public DocDataObjectSendingMessageSettings Parent
		{
			get { return parent; }
		}
		readonly DocDataObjectSendingMessageSettings parent;

		readonly IValidationInternals zValidationInternals;
		readonly ISingleElementListInternal parentListInternals;

		#endregion
	}
}
