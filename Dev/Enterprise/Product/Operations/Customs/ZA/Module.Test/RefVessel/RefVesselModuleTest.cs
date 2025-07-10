using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(RefVesselModuleForTest))]
	sealed class RefVesselModuleTest : MasterFiles.Module.Testing.RefVesselModuleTest
	{
		public void TestZACSVImportMenuItem()
		{
			using (var module = new RefVesselModuleForTest())
			{
				var result = false;
				foreach (MenuItem item in module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems)
				{
					if (item.Text.Equals("Import From CSV (ZA special template)"))
					{
						result = true;
						item.PerformClick();
						break;
					}
				}

				Assert(result);
				Assert(module.GetSpecialFormEventRaised);
			}
		}

		protected override MasterFiles.Module.RefVesselModule GetNewVesselModule() => new RefVesselModule();

		sealed class RefVesselModuleForTest : RefVesselModule
		{
			public bool GetSpecialFormEventRaised;

			protected override void ImportFromCsv(object sender, EventArgs e)
			{
				GetSpecialFormEventRaised = true;
			}
		}
	}
}
