using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI.Testing
{
	public class BasePackingControlTest : TestCaseWithFactory
	{
		public void TestPackingInformationCollectionReadOnlyIfPluggedIn()
		{
			var declaration = GetDeclarationRelevantForPacking();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(false, ((BusinessObjectCollection)declaration.PackingInformationCollection).ReadOnly);
			using (var form = new ZArchitecture.GUI.ZForm(declaration))
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, ((BusinessObjectCollection)declaration.PackingInformationCollection).ReadOnly);
				declaration.JE_OverrideFreightDefaults = true;
				AssertEquals(false, ((BusinessObjectCollection)declaration.PackingInformationCollection).ReadOnly);
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			var declaration = GetDeclarationRelevantForPacking();
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = form.CustomsBrokerageUserControl;
				userControl.MainTabControl.SelectedTab = userControl.PackingTabPage;
				var packingUserControl = (BasePackingControl)userControl.Packing;
				AssertNotNull("CU_IssueDate should appear", packingUserControl.HouseBillsGrid.Columns[CusDecHouseBillSchema.CU_IssueDate.Name]);
			}
		}

		protected virtual BaseJobDeclaration GetDeclarationRelevantForPacking() => Factory.New<BaseJobDeclaration>();
	}
}
