using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccSurchargeApplicationUserControl : ZUserControl
	{
		public AccSurchargeApplicationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				var company = this.DataSource as GlbCompany;
				if (!PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(company))
				{
					surchargeApplicationGrid.RemoveFromAvailableColumns(AccSurchargeApplication.Schema.ASP_PlaceOfSupply);
				}
				if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					surchargeApplicationGrid.RemoveFromAvailableColumns(AccSurchargeApplication.Schema.ASP_SupplyType);
				}
			}
		}
	}
}

