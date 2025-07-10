using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class ConsignorDetailsControlTest : TestCaseWithFactory
	{
		public void TestMergeCustomsInvoiceLinesByBoundDropEditVisibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			using (var form = new ZForm(Org))
			using (var control = new DetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("MergeCustomsInvoiceLinesByDropEdit is visible for AU", true, control.MergeCustomsInvoiceLinesByDropEditVisible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			using (var form = new ZForm(Org))
			using (var control = new DetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("MergeCustomsInvoiceLinesByDropEdit is not visible", false, control.MergeCustomsInvoiceLinesByDropEditVisible);
			}
		}

		public void TestPaymentMethodControlVisibility()
		{
			foreach (var countryCode in new[] { Constants.CountryCodes.NewZealand, Constants.CountryCodes.Singapore })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var form = new ZForm(Org))
				using (var control1 = new DetailsControlForTest())
				{
					form.Controls.Add(control1);
					form.Show();

					AssertEquals("OM_IMPaymentMethodDropEdit is visible for " + countryCode, true, control1.OM_IMPaymentMethodDropEditVisible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			using (var form = new ZForm(Org))
			using (var control = new DetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("OM_IMPaymentMethodDropEdit is not visible", !control.OM_IMPaymentMethodDropEditVisible);
			}
		}

		#region Implementation

		protected OrgHeader Org;
		protected DetailsControlForTest Control;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.New<OrgHeader>();
			Control = new DetailsControlForTest();
			Control.SetDataBinding(Org, "");
		}

		protected override void TearDown()
		{
			base.TearDown();
			Control.Dispose();
		}

		#endregion

		#region Mock Control

		public class DetailsControlForTest : ConsignorDetailsUserControl
		{
			public bool OM_IMPaymentMethodDropEditVisible
			{
				get { return OM_IMPaymentMethodDropEdit.Visible; }
			}

			public bool MergeCustomsInvoiceLinesByDropEditVisible
			{
				get { return MergeCustomsInvoiceLinesByDropEdit.Visible; }
			}
		}

		#endregion

		#region Test the column "LastFreeDay" had been added

		public void TestLastFreeDayColumnAdded()
		{
			using (var form = new ZForm(Org))
			using (var detailsControl = new DetailsControlForTest())
			{
				form.Controls.Add(detailsControl);
				form.Show();

				var grid = detailsControl.Controls.Find("ConsignorCTOStoragesGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull("ConsignorCTOStoragesGrid should not be null", grid);

				var freeDayTypeColumn = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(column => column.ColumnName == "PD_FreeDayType");
				AssertNotNull("PD_FreeDayType column should not be null", freeDayTypeColumn);
			}
		}

		#endregion
	}
}
