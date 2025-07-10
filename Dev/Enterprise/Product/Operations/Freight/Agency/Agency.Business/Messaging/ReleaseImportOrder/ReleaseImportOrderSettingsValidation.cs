using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseImportOrderSettingsValidation : AutoReleaseImportOrderSettingsValidation
	{
		public ReleaseImportOrderSettingsValidation(AutoReleaseImportOrderSettings parent)
			: base(parent) { }

		#region ErrorBehaviour

		protected override void CheckErrorBehaviour()
		{
			base.CheckErrorBehaviour();
			MandatoryValidation.CheckEntered(Parent.ErrorBehaviourInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ErrorBehaviourInfo, Parent.ErrorBehaviour_List);
		}

		#endregion

		#region Implementation

		public new ReleaseImportOrderSettings Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseImportOrderSettings)base.Parent; }
		}

		#endregion
	}
}
