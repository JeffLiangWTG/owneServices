using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefCusTariffModule))]
	public class RefCusTariffModuleTest : ZModuleBasherTest
	{
		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			return Factory.NewWithValidTestData<TariffView>();
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestCSVImportMenuItemHasBeenAdded()
		{
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
				using (var module = new RefCusTariffModule())
				{
					AssertNotNull("Show Import Tariff From CSV", module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import Tariff From CSV"));
					AssertNotNull("Show Import Rate From CSV", module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import Rate From CSV"));
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var module = new RefCusTariffModule())
				{
					AssertNull("Not show Import Tariff From CSV", module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import Tariff From CSV"));
					AssertNull("Not show Import Rate From CSV", module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import Rate From CSV"));
				}
			}

			);
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestImportRateFromCSV()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			using (var module = new RefCusTariffModuleForUnitTest())
			{
				_ = module.ContextMenuExposed;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var handler1 = ((IFilterModuleInternalsForTesting)module).ImportMenuItems["Import Rate From CSV"];
				handler1.Invoke(null, EventArgs.Empty);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var form = ZApplication.GetOpenForms().OfType<ImportRateFromCSVForm>().Single();
				form.Dispose();
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestImportTariffFromCSV()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			using (var module = new RefCusTariffModuleForUnitTest())
			{
				_ = module.ContextMenuExposed;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var handler1 = ((IFilterModuleInternalsForTesting)module).ImportMenuItems["Import Tariff From CSV"];
				handler1.Invoke(null, EventArgs.Empty);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var form = ZApplication.GetOpenForms().OfType<ImportTariffFromCSVForm>().Single();
				form.Dispose();
			}
		}

		public void TestTariffViewFilterData()
		{
			using (var module = new RefCusTariffModule())
			{
				AssertNull("Default is null", module.TariffViewFilterData);
			}
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("TariffView is a view", true);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.Universal.RefCusTariff;
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestWhichItemsAllowedWhenSelfManagedCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			using (var module = new RefCusTariffModule())
			{
				AssertEquals("View is allowed.", true, module.AllowView);
				AssertEquals("New is allowed", true, module.AllowNew);
				AssertEquals("Edit is allowed.", true, module.AllowEdit);
				AssertEquals("Delete is allowed.", true, module.AllowDelete);
				AssertEquals("UniversalCopy is not allowed.", false, module.AllowUniversalCopy);
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestWhichItemsAllowedWhenNotSelfManagedCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var module = new RefCusTariffModule())
			{
				AssertEquals("View is allowed.", true, module.AllowView);
				AssertEquals("New is not allowed.", false, module.AllowNew);
				AssertEquals("Edit is not allowed.", false, module.AllowEdit);
				AssertEquals("Delete is not allowed.", false, module.AllowDelete);
				AssertEquals("UniversalCopy is not allowed.", false, module.AllowUniversalCopy);
			}
		}

		public void TestShowEditForm()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("AU", "TTX");
			var tariff = helper.LoadOrCreateNewTariff("AU", tariffType.PK, "122231", new CargoWise.Types.ZDateTime(2020, 12, 4), new CargoWise.Types.ZDateTime(2020, 12, 5));
			tariff.ZZ1_IsSystem = false;
			using (var module = new RefCusTariffModuleForUnitTest())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					using (var form = module.GetEditForm(tariff))
					{
						AssertNotNull(form);
					}

					tariff.ZZ1_IsSystem = true;
					using (var form = module.GetEditForm(tariff))
					{
						AssertNull(form);
						AssertEquals("System defined tariffs cannot be edited.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestShowEditFormForMisMatchingCusRefTariffCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("AU", "TTX");
			var tariff = helper.LoadOrCreateNewTariff("AU", tariffType.PK, "122231", new CargoWise.Types.ZDateTime(2020, 12, 4), new CargoWise.Types.ZDateTime(2020, 12, 5));
			tariff.ZZ1_IsSystem = false;
			using (var module = new RefCusTariffModuleForUnitTest())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					using (var form = module.GetEditForm(tariff))
					{
						AssertNotNull(form);
					}
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
				{
					using (var form = module.GetEditForm(tariff))
					{
						AssertNull(form);
						AssertEquals("Tariffs that do not belong to your country cannot be edited.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestShowDeleteFrom()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("AU", "TTX");
			var tariff = helper.LoadOrCreateNewTariff("AU", tariffType.PK, "122231", new CargoWise.Types.ZDateTime(2020, 12, 4), new CargoWise.Types.ZDateTime(2020, 12, 5));
			tariff.ZZ1_IsSystem = false;
			using (var module = new RefCusTariffModuleForUnitTest())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					using (var form = module.GetDeleteForm(tariff))
					{
						AssertNotNull(form);
					}

					tariff.ZZ1_IsSystem = true;
					using (var form = module.GetDeleteForm(tariff))
					{
						AssertNull(form);
						AssertEquals("System defined tariffs cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestShowDeleteFormForMisMatchingCusRefTariffCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("AU", "TTX");
			var tariff = helper.LoadOrCreateNewTariff("AU", tariffType.PK, "122231", new CargoWise.Types.ZDateTime(2020, 12, 4), new CargoWise.Types.ZDateTime(2020, 12, 5));
			tariff.ZZ1_IsSystem = false;
			using (var module = new RefCusTariffModuleForUnitTest())
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					using (var form = module.GetDeleteForm(tariff))
					{
						AssertNotNull(form);
					}
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
				{
					using (var form = module.GetDeleteForm(tariff))
					{
						AssertNull(form);
						AssertEquals("Tariffs that do not belong to your country cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public class RefCusTariffModuleForUnitTest : RefCusTariffModule
		{
			public IZForm GetDeleteForm(TariffView tariff) => ShowDeleteForm(tariff);
			public IZForm GetEditForm(TariffView tariff) => ShowEditForm(tariff);
			public MenuItem[] ContextMenuExposed
			{
				get
				{
					return ContextMenu;
				}
			}
		}
	}
}
