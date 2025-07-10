using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefVesselModule : ZFilterGridModule
	{
		public RefVesselModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem(Res.GetString("9127391e-8cbe-437b-945f-0e3e7e2081a9", "From CSV"), GetImportCSVEventHandler(), false);
		}

		#region Import from CSV

		void ImportFromCSV(object sender, EventArgs e)
		{
			ZFormModaliser.Show(GetImportVesselsFromCSVForm(), EmbeddedControl.FindForm());
		}

		protected virtual ImportVesselsFromCSVForm GetImportVesselsFromCSVForm()
		{
			return new ImportVesselsFromCSVForm();
		}

		protected virtual EventHandler GetImportCSVEventHandler()
		{
			return new EventHandler(ImportFromCSV);
		}

		#endregion

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefVessel; }
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefVessel);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefVesselFilterControl(GridCollection, (RefVesselFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefVesselCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefVesselFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForScreeningEntity(this, result);
			return result.ToArray();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Vessels; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
