using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class LocationFilterControlTest : TestCaseWithFactory
	{
		#region TestStateColumnIsShownForPort

		[RequiresSTA]
		public void TestStateColumnIsShownForPort()
		{
			LocationFilterBusinessObject searchBO;

			searchBO = new LocationFilterBusinessObject();
			((ModuleTextFilter)searchBO["Description"]).Property = "Mansfield";
			((ModuleTextFilter)searchBO["Description"]).IsActive = true;

			LocationCollection locoCollection = new LocationCollection(Factory);
			LocationFilterControl filterControl = new LocationFilterControl(locoCollection, searchBO);

			using (ZForm testForm = new ZForm())
			{
				testForm.Controls.Add(filterControl);
				testForm.Show();

				Application.DoEvents();
				((ModuleTextFilter)searchBO["Location Type"]).Property = "Port";
				Assert("State column still visible for Port search", filterControl.FilteredGrid.Columns[LocationFilterControl.StateColumnKey].IsVisible);
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		[RequiresSTA]
		public void TestLocationSearchDoesNotRaiseExceptionWhenSearchingForUNLOCO()
		{
			using (ZForm form = new ZForm())
			using (var locModule = new LocationModule())
			{
				LocationFilterControl filter = (LocationFilterControl)locModule.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
			}
		}

		#endregion

		#region TestStateColumnIsHiddenForRegion

		[RequiresSTA]
		public void TestStateColumnIsHiddenForRegion()
		{
			LocationFilterBusinessObject searchBO;

			searchBO = new LocationFilterBusinessObject();
			((ModuleTextFilter)searchBO["Description"]).Property = "AUEC";
			((ModuleTextFilter)searchBO["Description"]).IsActive = true;

			LocationCollection locoCollection = new LocationCollection(Factory);
			LocationFilterControl filterControl = new LocationFilterControl(locoCollection, searchBO);

			using (ZForm testForm = new ZForm())
			{
				testForm.Controls.Add(filterControl);
				testForm.Show();

				Application.DoEvents();
				((ModuleTextFilter)searchBO["Location Type"]).Property = "Zone";
				Assert("State column hidden after region search", !filterControl.FilteredGrid.Columns[LocationFilterControl.StateColumnKey].IsVisible);
			}
		}

		#endregion

		#region TestStateColumnIsHiddenForCountry

		[RequiresSTA]
		public void TestStateColumnIsHiddenForCountry()
		{
			LocationFilterBusinessObject searchBO;

			searchBO = new LocationFilterBusinessObject();
			((ModuleTextFilter)searchBO["Description"]).Property = "AE";
			((ModuleTextFilter)searchBO["Description"]).IsActive = true;

			LocationCollection locoCollection = new LocationCollection(Factory);
			LocationFilterControl filterControl = new LocationFilterControl(locoCollection, searchBO);

			using (ZForm testForm = new ZForm())
			{
				testForm.Controls.Add(filterControl);
				testForm.Show();

				Application.DoEvents();
				((ModuleTextFilter)searchBO["Location Type"]).Property = "Country";
				Assert("State column hidden after country search", !filterControl.FilteredGrid.Columns[LocationFilterControl.StateColumnKey].IsVisible);
			}
		}

		[RequiresSTA]
		public void TestStateColumnIsHiddenForZone()
		{
			LocationFilterBusinessObject searchBO;

			searchBO = new LocationFilterBusinessObject();
			((ModuleTextFilter)searchBO["Description"]).Property = "AE";
			((ModuleTextFilter)searchBO["Description"]).IsActive = true;

			LocationCollection locoCollection = new LocationCollection(Factory);
			LocationFilterControl filterControl = new LocationFilterControl(locoCollection, searchBO);

			using (ZForm testForm = new ZForm())
			{
				testForm.Controls.Add(filterControl);
				testForm.Show();

				Application.DoEvents();
				((ModuleTextFilter)searchBO["Location Type"]).Property = "Zone";
				Assert("State column hidden after country search", !filterControl.FilteredGrid.Columns[LocationFilterControl.StateColumnKey].IsVisible);
			}
		}

		#endregion
	}
}
