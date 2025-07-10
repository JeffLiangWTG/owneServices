using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupModule))]
	sealed class AccTaxOverrideGroupModuleTest : ZModuleBasherTest
	{
		public AccTaxOverrideGroupModuleTest()
			: base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccTaxOverrideGroup;
		}

		public void TestCheckpoints()
		{
			using (AccTaxOverrideGroupModule module = new AccTaxOverrideGroupModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.TaxOverrideGroups, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccTaxOverrideGroupModuleForTest module = new AccTaxOverrideGroupModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccTaxOverrideGroupFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccTaxOverrideGroupModuleForTest module = new AccTaxOverrideGroupModuleForTest())
			{
				IBusinessObjectCollection chargeCodeCollection = module.NewGridCollection;
				Assert("Invalid type", chargeCodeCollection is AccTaxOverrideGroupCollectionCompany);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccTaxOverrideGroupModuleForTest module = new AccTaxOverrideGroupModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccTaxOverrideGroupFilterBusinessObject);
			}
		}

		[RequiresSTA]
		public void TestNewMenuItemCollection()
		{
			using (var moduleForm = new ZForm())
			using (var module = new AccTaxOverrideGroupModuleForTest())
			{
				moduleForm.Controls.Add(module.EmbeddedControl);
				moduleForm.Show();

				AssertEquals("Precondition: There should be a new menu item", 1, module.NewMenuItem.MenuItems.Count);

				var newMenuItem = module.NewMenuItem.MenuItems[0];
				AssertEquals("Name for menu item must be 'New GST Tax Override Group'", "New GST Tax Override Group", newMenuItem.Text);
				AssertEquals("'New GST Tax Override Group' menu item must be default item", true, newMenuItem.DefaultItem);
			}
		}

		[RequiresSTA]
		public void TestNewMenuItemOtherTaxesOverrideGroup()
		{
			var taxConfigs = Factory.Load<AccTaxConfiguration>(new ZQuery(AccTaxConfigurationSchema.ETC_ParentId, GlbCompany.CurrentCompany.PK));
			AssertEquals("Precondition: taxConfigs.Length", 0, taxConfigs.Length);

			using (var moduleForm = new ZForm())
			using (var module = new AccTaxOverrideGroupModuleForTest())
			{
				moduleForm.Controls.Add(module.EmbeddedControl);
				moduleForm.Show();

				var menuItem = module.NewMenuItem.MenuItems.FindByText("New Tax Configuration Override Group");
				AssertNull(menuItem);
			}

			AccTaxConfiguration taxConfigurationAR = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationAR.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			taxConfigurationAR.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			AccTaxConfiguration taxConfigurationAP = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationAP.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfigurationAP.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			taxConfigs = Factory.Load<AccTaxConfiguration>(new ZQuery(AccTaxConfigurationSchema.ETC_ParentId, GlbCompany.CurrentCompany.PK));
			AssertGreaterThan("Precondition: taxConfigs.Length", taxConfigs.Length, 0);

			using (var moduleForm = new ZForm())
			using (var module = new AccTaxOverrideGroupModuleForTest())
			{
				moduleForm.Controls.Add(module.EmbeddedControl);
				moduleForm.Show();

				var menuItem = module.NewMenuItem.MenuItems.FindByText("New Tax Configuration Override Group");
				AssertNotNull("There should be a 'New Tax Configuration Override Group' menu item", menuItem);
				Assert(!menuItem.DefaultItem);
			}
		}

		[RequiresSTA]
		public void TestConsumptionTaxDescriptionInNewMenuItem()
		{
			AssertConsumptionTaxDescriptionInNewMenuItem(Core.Constants.CountryCodes.Australia, "GST");
			AssertConsumptionTaxDescriptionInNewMenuItem(Core.Constants.CountryCodes.Argentina, "IVA");
			AssertConsumptionTaxDescriptionInNewMenuItem(Core.Constants.CountryCodes.Bangladesh, "VAT");

			void AssertConsumptionTaxDescriptionInNewMenuItem(string countryCode, string taxName)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var moduleForm = new ZForm())
				using (var module = new AccTaxOverrideGroupModuleForTest())
				{
					moduleForm.Controls.Add(module.EmbeddedControl);
					moduleForm.Show();

					var newMenuItem = module.NewMenuItem.MenuItems[0];

					AssertEquals($"Name for menu item must be 'New {taxName} Tax Override Group'", $"New {taxName} Tax Override Group", newMenuItem.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestNewMenuItem_Click()
		{
			using (var module = new AccTaxOverrideGroupModuleForTest())
			using (var moduleForm = new ZForm())
			{
				moduleForm.Controls.Add(module.EmbeddedControl);
				moduleForm.Show();

				module.NewMenuItem.PerformClick();
				AssertFormTypeIsTaxOverrideGroup();

				var newMenuItem = module.NewMenuItem.MenuItems.FindByText("New GST Tax Override Group");
				AssertNotNull("Precondition: There should be a 'Tax Override group' menu item", newMenuItem);
				newMenuItem.PerformClick();
				AssertFormTypeIsTaxOverrideGroup();

				void AssertFormTypeIsTaxOverrideGroup()
				{
					using (var form = ZApplication.GetOpenForms().ToList().Last())
					{
						AssertType("Form Type", typeof(AccTaxOverrideGroupForm), form);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestNewTaxFrameworkTaxOverrideGroupMenuItem_Click()
		{
			AccTaxConfiguration taxConfigurationAR = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationAR.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			taxConfigurationAR.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			AccTaxConfiguration taxConfigurationAP = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationAP.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfigurationAP.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (var module = new AccTaxOverrideGroupModuleForTest())
			using (var moduleForm = new ZForm())
			{
				moduleForm.Controls.Add(module.EmbeddedControl);
				moduleForm.Show();

				var newMenuItem = module.NewMenuItem.MenuItems.FindByText("New Tax Configuration Override Group");
				AssertNotNull("Precondition: There should be a 'New Tax Configuration Override Group' menu item", newMenuItem);
				newMenuItem.PerformClick();

				using (var form = ZApplication.GetOpenForms().ToList().Last())
				{
					AssertType("Form Type", typeof(TaxFrameworkAccTaxOverrideGroupForm), form);
				}
			}
		}

		public void TestGetNewController()
		{
			using (AccTaxOverrideGroupModuleForTest module = new AccTaxOverrideGroupModuleForTest())
			{
				var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
				AssertEquals("Controller", typeof(AccTaxOverrideGroupController), module.GetNewController(taxOverrideGroup).GetType());

				var taxFrameworkTaxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
				taxFrameworkTaxOverrideGroup.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
				Assert("Precondition: Tax Override Group is TaxFrameworkRelated when SetContext to Tax Framework", taxFrameworkTaxOverrideGroup.IsTaxFrameworkRelated);
				AssertEquals("Controller", typeof(TaxFrameworkAccTaxOverrideGroupController), module.GetNewController(taxFrameworkTaxOverrideGroup).GetType());

				var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
				var taxOverrideGroupTaxConfigurationPivot = Factory.NewWithValidTestData<AccTaxOverrideGroupTaxConfigurationPivot>();
				taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxFrameworkTaxOverrideGroup.PK;
				taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
				Assert("Precondition: Tax Override Group is TaxFrameworkRelated when TaxOverrideGroupTaxConfigurationPivot exist", taxFrameworkTaxOverrideGroup.IsTaxFrameworkRelated);
				AssertEquals("Controller", typeof(TaxFrameworkAccTaxOverrideGroupController), module.GetNewController(taxFrameworkTaxOverrideGroup).GetType());

				taxFrameworkTaxOverrideGroup.RemoveContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
				AssertEquals("Controller", typeof(TaxFrameworkAccTaxOverrideGroupController), module.GetNewController(taxFrameworkTaxOverrideGroup).GetType());
			}
		}

		[RequiresSTA]
		public void TestPreviousNextItemButtonsOpensCorrectFormType()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			var taxFrameworkTaxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroupTaxConfigurationPivot = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxFrameworkTaxOverrideGroup.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;

			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.AccTaxOverrideGroup))
			{
				((IFilterGridModuleInternalsForTesting)module).PerformSearch();

				var taxOverrideGroupCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.AccTaxOverrideGroup);
				AssertEquals("Tax override group count", 2, taxOverrideGroupCollection.Count);
				AssertEquals(taxFrameworkTaxOverrideGroup.PK, taxOverrideGroupCollection[0].PK);
				AssertEquals(taxOverrideGroup.PK, taxOverrideGroupCollection[1].PK);

				using (var form = ((IFilterGridModuleInternalsForTesting)module).ShowEditForm(taxFrameworkTaxOverrideGroup))
				{
					AssertEquals("Form type for Tax framework related override group", typeof(TaxFrameworkAccTaxOverrideGroupForm), form.GetType());
					((IPreviousNextControlProvider)form).PreviousNextControlForTesting.FireNextButtonForTesting();
					using (var form1 = OpenedFormCache.GetInstance().GetForm(taxOverrideGroup.PK.ToGuid(), ControllerIDs.AccTaxOverrideGroup.ToString()))
					{
						AssertNotNull("Next form for Controller ID AccTaxOverrideGroup exists in cache", form1);
						AssertType<AccTaxOverrideGroupForm>("Next form", form1);
					}
				}
			}
		}

		#endregion
	}
}
