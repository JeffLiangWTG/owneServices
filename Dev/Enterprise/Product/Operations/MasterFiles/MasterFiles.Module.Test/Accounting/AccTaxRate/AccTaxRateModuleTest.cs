using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccTaxRateModule))]
	sealed class AccTaxRateModuleTest : ZModuleBasherTest
	{
		public AccTaxRateModuleTest()
			: base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccTaxRate;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestAllowUniversalCopy()
		{
			using (AccTaxRateModuleForTest module = new AccTaxRateModuleForTest())
			{
				GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
				AssertEquals(true, module.AllowUniversalCopy);

				GlbStaff.CurrentUser.GS_LoginName = "TestName";
				AssertEquals(false, module.AllowUniversalCopy);
			}
		}

		[RequiresSTA]
		public void TestCreateApplicableTaxIdsMenuErrorMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Malaysia))
			using (var module = (AccTaxRateModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var newFactory = Factory.CreateNewFactory();
				var taxRates = newFactory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Malaysia));
				taxRates.DeleteAll();
				newFactory.Save();

				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var actionMenuItems = module.GetNewActionMenuItems_Test();
					var menuItem = actionMenuItems.FindByText("Create Applicable Tax IDs");

					menuItem.PerformClick();
					AssertEquals("The Tax ID Set for the current Login Country/Region has successfully updated.", UnitTestUserNotification.Instance.LastMessage.Text);

					menuItem.PerformClick();
					AssertEquals("No changes required. The current Login Country/Region's Tax ID Set is already up to date.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestUpdateTaxRateForTaxFrameworkVisibility()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsDeveloper = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var module = new AccTaxRateModuleForTest())
			{
				AssertUpdateTaxRateForTaxFrameworkMenuItem(module, false);
			}

			Assert("Current user is developer", Env.CurrentUser.IsDeveloper);

			using (var module = new AccTaxRateModuleForTest())
			{
				AssertUpdateTaxRateForTaxFrameworkMenuItem(module, true);
			}
		}

		[RequiresSTA]
		public void TestUpdateTaxRateForTaxFrameworkAlwaysOpensFormWithNewFactory()
		{
			using (var module = new AccTaxRateModuleForTest())
			using (ZForm moduleForm = new ZForm())
			{
				moduleForm.Controls.Add(module.EmbeddedControl);
				moduleForm.Show();
				var actionMenuItems = module.GetNewActionMenuItems_Test();
				var menuItem = AssertUpdateTaxRateForTaxFrameworkMenuItem(module, true);

				BusinessObjectFactory factory;

				menuItem.PerformClick();
				using (var form = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType("Form Type", typeof(TaxFrameworkAccTaxRateForm), form);
					factory = ((TaxFrameworkAccTaxRateLoader)form.LastDataSourceForTest).Factory;
				}

				menuItem.PerformClick();
				using (var form = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType("Form Type", typeof(TaxFrameworkAccTaxRateForm), form);
					AssertNotEquals(factory, ((TaxFrameworkAccTaxRateLoader)form.LastDataSourceForTest).Factory);
					// This also asserts that form has different factory than the module
				}
			}
		}

		MenuItem AssertUpdateTaxRateForTaxFrameworkMenuItem(AccTaxRateModuleForTest module, bool isVisible)
		{
			var actionMenuItems = module.GetNewActionMenuItems_Test();
			var menuItem = actionMenuItems.FindByText("Update Tax Rate for Tax Framework");
			AssertEquals(!isVisible, object.ReferenceEquals(null, menuItem));
			return menuItem;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			accTaxRate = new AccTaxRateModuleForTest();
			IFilterControl controlForTest = accTaxRate.GetNewFilterControlForTest();
			Assert(controlForTest is AccTaxRateFilterControl);
			controlForTest.Dispose();
			accTaxRate.Dispose();
		}

		public void TestGridCollection()
		{
			accTaxRate = new AccTaxRateModuleForTest();
			IBusinessObjectCollection collectionForTest = accTaxRate.GetNewGridCollectionForTest();
			Assert(collectionForTest is BusinessObjectCollection);
			accTaxRate.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			accTaxRate = new AccTaxRateModuleForTest();
			FilterBusinessObject businessForTest = accTaxRate.GetNewFilterBusinessObjectForTest();
			Assert(businessForTest is FilterBusinessObject);
			accTaxRate.Dispose();
		}

		public void TestDeleteMenuItemText()
		{
			using (AccTaxRateModuleForTest module = new AccTaxRateModuleForTest())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Deactivate"));
			}
		}

		[RequiresSTA]
		public void TestFilterControl_WhenAuxiliaryRateGridColumnsAreVisible()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			using (var accTaxRateModule = new AccTaxRateModuleForTest())
			using (var controlForTest = (AccTaxRateFilterControl)accTaxRateModule.GetNewFilterControlForTest())
			{
				var gridColumnNames = controlForTest.Grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Select(c => c.ColumnName);
				Assert("Extra Tax Type should be visible", gridColumnNames.Contains("AT_ExtraTaxRateType"));
				Assert("Extra Tax Rate should be visible", gridColumnNames.Contains("ExtraRateForTodayForUIBinding"));
			}
		}

		[RequiresSTA]
		public void TestFilterControl_WhenAuxiliaryRateGridColumnsAreInvisible()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			using (var accTaxRateModule = new AccTaxRateModuleForTest())
			using (var controlForTest = (AccTaxRateFilterControl)accTaxRateModule.GetNewFilterControlForTest())
			{
				var gridColumnNames = controlForTest.Grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>().Select(c => c.ColumnName);
				Assert("Extra Tax Type should not be visible", !gridColumnNames.Contains("AT_ExtraTaxRateType"));
				Assert("Extra Tax Rate should not be visible", !gridColumnNames.Contains("ExtraRateForToday"));
			}
		}

		[RequiresSTA]
		public void TestLoadDefaultPostingGroupsMenu()
		{
			var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery());
			foreach (var chargeCode in chargeCodes)
			{
				chargeCode.Delete();
			}

			var taxRates = Factory.Load<AccTaxRate>(new ZQuery());
			foreach (var tax in taxRates)
			{
				tax.Delete();
			}

			Factory.Save();
			taxRates = Factory.Load<AccTaxRate>(new ZQuery());
			AssertEquals(0, taxRates.Length);

			var allCountries = new RefCountryCollection(Factory);
			foreach (RefCountry country in allCountries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
				{
					AccTaxRate.CreateApplicableTaxIds(Factory);
				}
			}
			Factory.Save();

			var query = new ZQuery(AccTaxRateSchema.AT_PostingGroupId, SQLComparisonOperator.NotEqual, ZShort.Zero);
			AssertEquals(0, Factory.Load<AccTaxRate>(query).Length);

			foreach (RefCountry country in allCountries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
				{
					using (var module = (AccTaxRateModule)ZModuleFactory.Instance.Create(ModuleID))
					{
						using (ZForm form = new ZForm())
						{
							form.Controls.Add(module.EmbeddedControl);
							form.Show();
							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

							var actionMenuItems = module.GetNewActionMenuItems_Test();
							var menuItem = actionMenuItems.FindByText("Load Default Posting Groups");

							if (!AccTaxRate.IsPostingGroupsEnabled(country.Code))
							{
								AssertNull(menuItem);
							}
							else
							{
								AssertNotNull(menuItem);
								Env.Security.GSTTaxRatesAllowLoadDefaultPostingGroups.IsAllowed = false;
								menuItem.PerformClick();

								AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Account -> Tax ID -> Load Default Posting Groups", UnitTestUserNotification.Instance.LastMessage.Text);

								query = new ZQuery(AccTaxRateSchema.AT_PostingGroupId, SQLComparisonOperator.NotEqual, AccTaxRate.DefaultPostingGroupID);
								query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, country.Code);

								AssertEquals(true, Factory.Load<AccTaxRate>(query).Length == 0);

								Env.Security.GSTTaxRatesAllowLoadDefaultPostingGroups.IsAllowed = true;
								UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
								UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
								menuItem.PerformClick();
								AssertEquals(@"The 'Load Default Posting Groups' function will re-default Posting Group values on Tax IDs.
Are you sure you want to update your Tax ID Posting Groups to their default values?",
									UnitTestUserNotification.Instance.LastMessage.Text);

								AssertEquals(false, Factory.Load<AccTaxRate>(query).Length != 0);

								UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
								UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
								menuItem.PerformClick();
								AssertEquals(string.Format("Logged into company of country: {0}", country.Code), @"The Tax ID Default Posting Group values for the current Login Country/Region have successfully updated.",
									UnitTestUserNotification.Instance.LastMessage.Text);
								AssertEquals(true, Factory.Load<AccTaxRate>(query).Length != 0);
							}
						}
					}
				}
			}
		}

		#region Implementation

		AccTaxRateModuleForTest accTaxRate;

		#endregion
	}
}
