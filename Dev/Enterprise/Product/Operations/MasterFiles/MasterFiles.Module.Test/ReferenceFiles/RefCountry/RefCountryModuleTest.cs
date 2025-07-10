using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCountryModule))]
	sealed class RefCountryModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefCountry;
		}

		public void TestCheckpoints()
		{
			using (RefCountryModule module = new RefCountryModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.Countries, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefCountryModuleForTest module = new RefCountryModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefCountryFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefCountryModuleForTest module = new RefCountryModuleForTest())
			{
				IBusinessObjectCollection countriesCollection = module.NewGridCollection;
				Assert("Invalid type", countriesCollection is RefCountryCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefCountryModuleForTest module = new RefCountryModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefCountryFilterBusinessObject);
			}
		}

		[RequiresSTA]
		public void TestIsSanctionedColumnDefined()
		{
			using (var module = new RefCountryModuleForTest())
			using (var filterControl = (RefCountryFilterControl)module.NewFilterControl)
			using (var form = new ZForm())
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				var column = filterControl.FilteredGrid.Columns[RefCountry.Schema.RN_IsSanctioned];
				column.IsVisible = true;

				filterControl.FilteredGrid.RefreshTableStyles();

				AssertNotNull("Column 'Is Sanctioned' should be visible and bound", column.ColumnStyle.PropertyDescriptor);
			}
		}

		#endregion
	}
}
