using System;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ShipmentDetailsScreeningUserControl : ZUserControl
	{
		public ShipmentDetailsScreeningUserControl()
		{
			InitializeComponent();
		}

		async void ScreenButton_Click(object sender, EventArgs e)
		{
			var jobDeclaration = JobDeclaration;
			if (jobDeclaration is IScreeningPartyProvider)
			{
				await new DeniedPartyScreeningPresentationManager().PerformScreening((ZForm)ParentForm, false, !DeniedPartyScreenerAsync.HasExcludedList(JobDeclaration.Factory));
			}
		}

		BaseJobDeclaration JobDeclaration => (BaseJobDeclaration)BindingSource.DataSource;
	}
}
