using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(CusClassificationModule))]
	sealed class CusClassificationModuleTest : ZModuleBasherTest
	{
		public void TestGetNewFilterControl()
		{
			using (CusClassificationModuleForTest module = new CusClassificationModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is CusClassificationFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (CusClassificationModuleForTest module = new CusClassificationModuleForTest())
			{
				IBusinessObjectCollection collection = module.NewGridCollection;
				Assert("Invalid type", collection is Customs.Business.BaseClassificationCollection<CusClassification>);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (CusClassificationModuleForTest module = new CusClassificationModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is CusClassificationFilterBusinessObject);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (CusClassificationModuleForTest module = new CusClassificationModuleForTest())
			{
				AssertEquals(Env.Security.CusClassification, module.SecurityCheckpoint);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (CusClassificationModuleForTest module = new CusClassificationModuleForTest())
			{
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestCSVImportMenuItemHasBeenAdded()
		{
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(testCusClassificationModule.EmbeddedControl);
				form.Show();
				MenuItem result = null;
				foreach (MenuItem item in testCusClassificationModule.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems)
				{
					if (item.Text == "Import From CSV")
					{
						result = item;
						break;
					}
				}

				AssertNotNull(result);
				result.PerformClick();
				NZClassificationImportFromCSVForm popupForm = ZFormModaliser.ActiveForm as NZClassificationImportFromCSVForm;
				AssertNotNull(popupForm);
				popupForm.Dispose();
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.NewZealand;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SingleTariffClassification;
		}

		protected override void SetUp()
		{
			base.SetUp();
			testCusClassificationModule = new CusClassificationModule();
		}

		protected override void TearDown()
		{
			if (testCusClassificationModule != null)
			{
				testCusClassificationModule.Dispose();
			}

			base.TearDown();
		}

		CusClassificationModule testCusClassificationModule;

		class CusClassificationModuleForTest : CusClassificationModule
		{
			public CusClassificationModuleForTest()
			{
			}

			public IFilterControl NewFilterControl
			{
				get
				{
					return GetNewFilterControl();
				}
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get
				{
					return GetNewGridCollection();
				}
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get
				{
					return GetNewFilterBusinessObject();
				}
			}
		}
	}
}
