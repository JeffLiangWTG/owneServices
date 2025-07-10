using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class ConsigneeRelationshipsUserControlTest : TestCaseWithFactory
	{
		public void TestRenameValuationBasisforCA()
		{
			OrgFormForTest form;
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsConsignee = true;

			using (form = new OrgFormForTest(organisation))
			{
				AssertNotNull("Pre-condition", form);
				form.Show();
				form.OrgTabControl.SelectedTab = form.ConsigneeTabPage;
				form.ConsigneeControl.RelationshipsTabPage.Show();

				var valuationBasisBoundDropEdit = (ZDropEdit)form.ConsigneeControl.RelationshipsControl.Controls.Find("OL_ValuationBasisBoundDropEdit", true)[0];
				var transactionsRelatedDropEdit = (ZDropEdit)form.ConsigneeControl.RelationshipsControl.Controls.Find("TransactionsRelatedDropEdit", true)[0];
				AssertEquals("Valuation Basis", valuationBasisBoundDropEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				Assert("Trans. Related", transactionsRelatedDropEdit.Visible);

				var link = organisation.SupplierLinks.AddNew();
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Canada;
				form.ConsigneeControl.RelationshipsControl.OrgSupplierLinkBoundGrid.SelectSingleElement(link);
				AssertEquals("Value for Duty Code", valuationBasisBoundDropEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				Assert("Trans. Related", !transactionsRelatedDropEdit.Visible);

				link = organisation.SupplierLinks.AddNew();
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
				form.ConsigneeControl.RelationshipsControl.OrgSupplierLinkBoundGrid.SelectSingleElement(link);
				AssertEquals("Valuation Basis", valuationBasisBoundDropEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				Assert("Trans. Related", transactionsRelatedDropEdit.Visible);
			}

			using (form = new OrgFormForTest(organisation))
			{
				AssertNotNull("Pre-condition", form);
				form.Show();
				form.OrgTabControl.SelectedTab = form.ConsigneeTabPage;
				form.ConsigneeControl.RelationshipsTabPage.Show();

				var valuationBasisBoundDropEdit = (ZDropEdit)form.ConsigneeControl.RelationshipsControl.Controls.Find("OL_ValuationBasisBoundDropEdit", true)[0];
				var transactionsRelatedDropEdit = (ZDropEdit)form.ConsigneeControl.RelationshipsControl.Controls.Find("TransactionsRelatedDropEdit", true)[0];
				AssertEquals("Value for Duty Code", valuationBasisBoundDropEdit.Extensions.Get<ILabelCaptionRenderer>().Caption);
				Assert("Trans. Related", !transactionsRelatedDropEdit.Visible);
			}
		}

		public void TestBuyingCommissionPercentageNumericUpDown()
		{
			using (var form = new ZForm())
			using (var control = new ConsigneeRelationshipsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertNotNull("Should be able to find BuyingCommissionPercentageNumericUpDown.", control.FindSingle<ZNumericUpDown>("BuyingCommissionPercentageNumericUpDown"));
			}
		}
	}
}
