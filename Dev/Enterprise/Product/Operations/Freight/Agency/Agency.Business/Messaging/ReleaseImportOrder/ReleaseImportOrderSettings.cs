using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseImportOrderSettings : AutoReleaseImportOrderSettings
	{
		[List("ErrorBehaviour_List")]
		public override ZString ErrorBehaviour
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ErrorBehaviour; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.ErrorBehaviour = value; }
		}

		public OperationalActionErrorBehaviourList ErrorBehaviour_List
		{
			get { return new OperationalActionErrorBehaviourList(); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ErrorBehaviour = OperationalActionErrorBehaviourList.Codes.Abort;
		}
	}
}
