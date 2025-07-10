using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class OrgManagementGroupingModelView : ZTreeModelView<OrgHeader>
	{
		public OrgManagementGroupingModelView(OrgManagementGroupingModel inner)
			: base(inner)
		{
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpointForEdit
		{
			get { return Env.Security.OrgDetailsModifyRelatedParties; }
		}

		#endregion
	}
}
