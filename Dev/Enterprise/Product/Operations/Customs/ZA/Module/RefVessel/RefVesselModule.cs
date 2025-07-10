using System;
using Enterprise.Customs.ZA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module
{
	public class RefVesselModule : MasterFiles.Module.RefVesselModule
	{
		public RefVesselModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem(Res.GetString("233CCFDF-65EE-4B36-AC22-5E646B10A182", "From CSV (ZA special template)"), ImportFromCsv, false);
		}

		#region Import from CSV

		protected virtual void ImportFromCsv(object sender, EventArgs e)
		{
			ZFormModaliser.Show(GetCsvForm(), EmbeddedControl.FindForm());
		}

		protected ImportVesselsFromCSVForm GetCsvForm()
		{
			return new ImportVesselsFromCSVForm();
		}

		#endregion

	}
}
