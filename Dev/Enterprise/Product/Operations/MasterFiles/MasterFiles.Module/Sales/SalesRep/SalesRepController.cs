using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class SalesRepController : GlbStaffController
	{
		public SalesRepController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SalesRep; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.SalesRep; }
		}

		protected override void SetControllerID(IZForm form, ControllerID proposedControllerID)
		{
			// The form has its own ControllerID - we don't want to set it to SalesRep.        
		}
	}
}
