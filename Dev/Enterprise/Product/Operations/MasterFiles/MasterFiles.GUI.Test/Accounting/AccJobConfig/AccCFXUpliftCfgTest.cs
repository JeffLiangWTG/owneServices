using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccCFXUpliftCfg))]
	sealed class AccCFXUpliftCfgTest : BasherTest
	{
		public override Form GetFormToBash() => CreateFormForTest();

		[RequiresSTA]
		public void TestSettingBusinessContext()
		{
			BusinessObjectFactory formFactory;
			using (var form = CreateFormForTest())
			{
				formFactory = ((IBusiness)form.DataSource).Factory;
				form.Show();
				Assert("Must have PermittedToDeleteCFXUpliftConfig business context", formFactory.HasContext(BusinessContext.PermittedToDeleteCFXUpliftConfig));
			}
			Assert("Must NOT have PermittedToDeleteCFXUpliftConfig business context after the form is closed", !formFactory.HasContext(BusinessContext.PermittedToDeleteCFXUpliftConfig));
		}

		public void TestOriginCountryColumn()
		{
			using (var form = new ZForm())
			using (var control = new AccCFXUpliftCfg())
			{
				form.Controls.Add(control);
				form.Show();

				var originCountryColumn = control.Grid.GetColumnStyle(AccCFXUpliftConfiguration.Schema.JCF_RN_NKOriginCountry);

				AssertNotNull(originCountryColumn);
				Assert("Origin Country column should be hidden by default", !originCountryColumn.IsVisible);
			}
		}

		public void TestDestinationCountryColumn()
		{
			using (var form = new ZForm())
			using (var control = new AccCFXUpliftCfg())
			{
				form.Controls.Add(control);
				form.Show();

				var destinationCountryColumn = control.Grid.GetColumnStyle(AccCFXUpliftConfiguration.Schema.JCF_RN_NKDestinationCountry);

				AssertNotNull(destinationCountryColumn);
				Assert("Destination Country column should be hidden by default", !destinationCountryColumn.IsVisible);
			}
		}

		public void TestStartDateColumn()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			using (AccountingMasterFilesRegistry.Instance.EnableCFXUpliftStartAndExpiryDateColumns.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new AccCFXUpliftCfg())
			{
				form.Controls.Add(control);
				form.SetDataBinding(company, nameof(GlbCompany.AccCFXConfigurations));
				form.Show();

				var column = control.Grid.GetColumnStyle(AccCFXUpliftConfiguration.Schema.JCF_StartDate);

				AssertNotNull(column);
				Assert("Start Date column should be available", !column.IsUnavailable);
				Assert("Start Date column should be visible", column.IsVisible);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableCFXUpliftStartAndExpiryDateColumns.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var control = new AccCFXUpliftCfg())
			{
				form.Controls.Add(control);
				form.SetDataBinding(company, nameof(GlbCompany.AccCFXConfigurations));
				form.Show();

				var column = control.Grid.GetColumnStyle(AccCFXUpliftConfiguration.Schema.JCF_StartDate);

				AssertNotNull(column);
				Assert("Start Date column should be unavailable", column.IsUnavailable);
			}
		}

		public void TestExpiryDateColumn()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			using (AccountingMasterFilesRegistry.Instance.EnableCFXUpliftStartAndExpiryDateColumns.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new AccCFXUpliftCfg())
			{
				form.Controls.Add(control);
				form.SetDataBinding(company, nameof(GlbCompany.AccCFXConfigurations));
				form.Show();

				var column = control.Grid.GetColumnStyle(AccCFXUpliftConfiguration.Schema.JCF_ExpiryDate);

				AssertNotNull(column);
				Assert("Expiry Date column should be available", !column.IsUnavailable);
				Assert("Expiry Date column should be visible", column.IsVisible);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableCFXUpliftStartAndExpiryDateColumns.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var control = new AccCFXUpliftCfg())
			{
				form.Controls.Add(control);
				form.SetDataBinding(company, nameof(GlbCompany.AccCFXConfigurations));
				form.Show();

				var column = control.Grid.GetColumnStyle(AccCFXUpliftConfiguration.Schema.JCF_ExpiryDate);

				AssertNotNull(column);
				Assert("Expiry Date column should be unavailable", column.IsUnavailable);
			}
		}

		#region Implementation

		ZChildForm CreateFormForTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();     // Required to prevent basher test failures due to HasChanges = true

			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new AccCFXUpliftCfg();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(company, nameof(GlbCompany.AccCFXConfigurations));
			return form;
		}

		#endregion
	}
}
