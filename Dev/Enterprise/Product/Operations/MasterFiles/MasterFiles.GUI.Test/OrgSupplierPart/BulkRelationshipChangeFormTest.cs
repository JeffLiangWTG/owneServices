using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(BulkRelationshipChangeForm))]
	sealed class BulkRelationshipChangeFormTest : ZFormBasherTest
	{
		public void ContinueBtn_ClickTest()
		{
			OrgSupplierBulkRelationshipChanger businessObject = new OrgSupplierBulkRelationshipChanger(Factory);
			using (BulkRelationshipChangeForm form = new BulkRelationshipChangeForm(businessObject))
			{
				form.AcceptButton.PerformClick();

				AssertEquals(DialogResult.None, form.DialogResult);

				businessObject.ToOrganisationPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
				businessObject.ToRelationship = businessObject.RelationshipTypeList[0].Code;

				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new BulkRelationshipChangeForm(new OrgSupplierBulkRelationshipChanger(Factory));
		}
	}
}
