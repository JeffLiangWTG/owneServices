using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	sealed class EntryHeaderModuleStripTest : TestCase
	{
		public void TestEntryHeaderModuleFilter()
		{
			var filter = new ReferenceNumberFilter(DeclarationFilterConstants.NumberFilterTypes.AdditionalReferenceNumber, (@operator, country, type, number) => new ZQuery(), new MasterFiles.Business.RefCountryCollection(new BusinessObjectFactory()));
			using (var strip = new EntryHeaderModuleStripForTest())
			{
				var controls = strip.GetCurrentFilterControlsForTest(filter);
				try
				{
					CombineAssertions(() =>
					{
						AssertEquals("controls length", 6, controls.Length);
						AssertEquals("control 1", "operatorDropEdit", controls[0].Name);
						AssertEquals("control 2", "numberTextBox", controls[1].Name);
						AssertEquals("control 3", "countryLabel", controls[2].Name);
						AssertEquals("control 4", "countryFindBox", controls[3].Name);
						AssertEquals("control 5", "typeLabel", controls[4].Name);
						AssertEquals("control 6", "typeDropEdit", controls[5].Name);
					});
				}
				finally
				{
					foreach (var c in controls)
					{
						c.Dispose();
					}
				}
			}
		}

		sealed class EntryHeaderModuleStripForTest : EntryHeaderModuleStrip
		{
			public Control[] GetCurrentFilterControlsForTest(ModuleFilter filter) => GetCurrentFilterControls(filter);
		}
	}
}
