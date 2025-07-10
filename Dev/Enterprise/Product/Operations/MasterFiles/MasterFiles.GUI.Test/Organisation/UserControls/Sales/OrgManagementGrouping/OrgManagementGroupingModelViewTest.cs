using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgManagementGroupingModelView))]
	sealed class OrgManagementGroupingModelViewTest : NonPersistentBusinessObjectTestCase
	{
		#region Security

		public void TestSecurityCheckpointForEdit()
		{
			var org = Factory.New<OrgHeader>();
			var view = new OrgManagementGroupingModelView(org.OrgManagementGroupingModel);
			AssertEquals(Env.Security.OrgDetailsModifyRelatedParties, view.SecurityCheckpointForEdit);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			return new OrgManagementGroupingModelView(org.OrgManagementGroupingModel);
		}

		#endregion
	}
}
