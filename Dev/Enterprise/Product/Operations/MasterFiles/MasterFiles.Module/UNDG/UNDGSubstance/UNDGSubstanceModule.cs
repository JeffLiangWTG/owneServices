using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class UNDGSubstanceModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.UNDGSubstance; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.UNDGSubstance);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UNDGSubstanceFilterControl(GridCollection, (UNDGSubstanceFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new UNDGSubstanceCollection(Factory, new ZQuery());
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UNDGSubstanceFilterBusinessObject();
		}

		public override bool AllowNew => false;

		#endregion

		#region Data Transfer Menu Items

		protected override EventHandler DefaultImportExportAction => new EventHandler(OnExport_ShowErrorMessage);

		protected override EventHandler DefaultVisibleExportAction => new EventHandler(OnExport_ShowErrorMessage);

		void OnExport_ShowErrorMessage(object sender, EventArgs e)
		{
			Globals.Message.Show(
				Res.GetString(
					"b19496db-b932-4ffd-8373-ac83edf1ba9c",
					"Exporting data from the Dangerous Goods module is not permitted under current agreements with our data providers."
				),
				Res.GetString("4684e142-300d-4b9a-9448-f2915ae809a4", "Not supported"),
				MessageBoxButtons.OK,
				MessageBoxIcon.Information
			);
		}

		#endregion

		#region Licence/Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.UNDGSubstance; }
		}

		#endregion

		#region Delete

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			if (!selectedBusinessObject.CanDelete)
			{
				Globals.Message.ShowWarning(selectedBusinessObject.ReasonForNotAbleToDelete, Enterprise.MasterFiles.Module.Res.GetString("4478f7cf-c1bc-4b83-9ef9-9937dffdc910", "Unable to delete record"));
				return null;
			}
			else
			{
				return base.ShowDeleteForm(selectedBusinessObject);
			}
		}

		#endregion
	}
}
