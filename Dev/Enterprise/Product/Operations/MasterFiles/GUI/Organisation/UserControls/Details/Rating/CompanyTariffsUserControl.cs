using System.Linq;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CompanyTariffsUserControl : ZUserControl
	{
		public CompanyTariffsUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			DefaultTariffLevel.Text = Res.GetString("b690b2da-75d4-4173-88be-0714a175e364", "Default Company Tariff Level: {0}", Env.Registry.GlobalTariffDefault);
			if (dataSource is OrgHeader orgHeader)
			{
				DefaultTariffLevel.Visible = !IsDefaultLevelSet(orgHeader);
				orgHeader.CompanyData.RateTariffLevels.HasChangesChanged += RateTariffLevels_HasChangesChanged;
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		void RateTariffLevels_HasChangesChanged(object sender, CargoWise.EntityFramework.HasChangesChangedEventArgs e)
		{
			DefaultTariffLevel.Visible = DataSource is OrgHeader orgHeader && !IsDefaultLevelSet(orgHeader);
		}

		bool IsDefaultLevelSet(OrgHeader org)
		{
			// When org is deleted, org.CompanyData is just an empty place holder - WI00377465
			// => org.CompanyData.RateTariffLevels can be null
			// RateTariffLevels registers for HasChangesChanged event. It is notified when Org is deleted => CompanyData is cleared => RateTariffLevels is cleared.
			// SuspendSettingHasChanges in DeleteRateTariffLevels should have stopped the notification but let's just keep this checking for safety.
			var rateTariffLevels = org?.CompanyData?.RateTariffLevels;
			if (rateTariffLevels == null)
			{
				return false;
			}

			return rateTariffLevels
				.Cast<OrgRateTariffLevel>()
				.Any(x =>
					x.P7_TariffType == OrgRateTariffLevel.DefaultTariffType &&
					x.P7_Direction == nameof(OrgRateTariffLevel.Directions.ALL) &&
					x.P7_Mode == nameof(OrgRateTariffLevel.Directions.ALL));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				if (DataSource is OrgHeader orgHeader)
				{
					orgHeader.CompanyData.RateTariffLevels.HasChangesChanged -= RateTariffLevels_HasChangesChanged;
				}
			}
			base.Dispose(disposing);
		}
	}
}
