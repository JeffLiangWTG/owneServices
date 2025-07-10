using System.Linq;
using System.Windows.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class WiseRatesForm : ZChildForm
	{
		public WiseRatesForm(WiseRatingHeaderView wiseRatingHeaderView)
			: base(wiseRatingHeaderView)
		{
			InitializeComponent();
			CreateFilters();
		}

		WiseRatingHeaderView WiseRatesContainer => (WiseRatingHeaderView)BusinessEntity;
		RateProviderAccessAndMenu rateProviderAccess;
		readonly SupportedTransportAndContainerModes supportedModes = new SupportedTransportAndContainerModes();
		protected internal WiseRatesFilterStripControl FilterControl { get; private set; }

		void CreateFilters()
		{
			FilterControl = new WiseRatesFilterStripControl(WiseRatesContainer);
			Controls.Add(FilterControl);
			FilterControl.Dock = DockStyle.Fill;
			FilterControl.PerformSearch += FilterControl_PerformSearch;
		}

		protected override void AddAdornments()
		{
			base.AddAdornments();
			var mainMenu = new ZMainMenu();
			this.Menu = mainMenu;
			var manageMenu = new ZMenuItem(ResString.GetMultilingualString("e053b817-2c6e-4cad-b600-7414d05b42bb", "Manage Rates Service"));
			mainMenu.MenuItems.Add(manageMenu);
			var settingsMenu = new ZMenuItem(ResString.GetMultilingualString("922abc75-32ad-4c5b-816e-ac5ad026c11d", "Providers and Settings"), ProvidersAndSettings_Click);
			manageMenu.MenuItems.Add(settingsMenu);

			rateProviderAccess = new RateProviderAccessAndMenu(manageMenu);
		}

		public override string FormCaption => ResString.GetMultilingualString("e187de8f-e905-4be3-999a-b22b4be6eade", "Search Costs");
		public override string FormVerb => string.Empty;

		void FilterControl_PerformSearch(object sender, System.EventArgs e)
		{
			if (FilterControl.HasDatesFilterSelectedMoreThanOnce)
			{
				var errorMessage = ResString.GetMultilingualString("890329d3-6154-4f9a-a92c-737ccb06120c", "An effective date filter can be only selected once.");
				Globals.Message.ShowError(errorMessage);
			}
			else
			{
				WiseRatesContainer.Logger.ClearLogs();

				var filterBusinessStripObject = (WiseRatesFilterStripBusinessObject)FilterControl.FilterBusinessObject;
				if (ValidateTransportAndContainerModes(filterBusinessStripObject))
				{
					var ratesQueries = filterBusinessStripObject.BuildRatesQueries(WiseRatesContainer.Logger);

					WiseRatesContainer.SendRatesRequest(ratesQueries);

					var logs = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value
						? WiseRatesContainer.Logger.GetAllLogs()
						: WiseRatesContainer.Logger.Warnings;

					FilterControl.ShowResultsChart(WiseRatesContainer.SearchResponse, logs);
				}
			}
		}

		bool ValidateTransportAndContainerModes(WiseRatesFilterStripBusinessObject filterBusinessStripObject)
		{
			if (filterBusinessStripObject.TransportModes.Count() == 1 && filterBusinessStripObject.ContainerModes.Count() == 1)
			{
				string transportMode = filterBusinessStripObject.TransportModes.First();
				string containerMode = filterBusinessStripObject.ContainerModes.First();

				if (supportedModes.GetValidity(transportMode, containerMode) == SupportedTransportAndContainerModes.Validity.Invalid)
				{
					var errorMessage = ResString.GetMultilingualString("890329d3-6154-4f9a-a92c-737aab06174u", "Container mode of {0} can not be selected with {1} transport mode.", containerMode, transportMode);
					Globals.Message.ShowError(errorMessage);
					return false;
				}
			}
			return true;
		}

		void ProvidersAndSettings_Click(object sender, System.EventArgs e)
		{
			Globals.Message.Show(ResString.GetMultilingualString("380f2f4d-5407-4787-ab71-545017ccb699", @"To manage Rates Service Providers and Settings please navigate to:
Maintain > System > Registry > AutoRating > Rates Service"),
									 ResString.GetMultilingualString("1cb0c9c5-020d-4229-ab51-55a8d504f541", "Providers and Settings"),
									 MessageBoxButtons.OK,
									 MessageBoxIcon.Information);
		}
	}
}
