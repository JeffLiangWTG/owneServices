using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefDomesticCartageZoneModule : ZFilterGridModule
	{
		public RefDomesticCartageZoneModule()
		{
			AddImportDataMenuItem(Res.GetString("db171565-4147-436e-a5f4-de1ab7b46619", "US/Canada ACI Zone Information"), delegate
				{
					new ACIZoneInformationImporter(EmbeddedControl.FindForm()).PromptUserAndImport();
				}
			);
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefDomesticCartageZone; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new RefDomesticCartageZoneController();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefDomesticCartageZoneFilterControl(GridCollection, (RefDomesticCartageZoneFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefDomesticCartageZoneCollection(Factory);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> collection = new List<MenuItem>(base.GetNewStandardMenuItems());
			collection.Remove(ViewMenuItem);
			return collection.ToArray();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefDomesticCartageZoneFilterBusinessObject();
		}

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RefDomesticCartageZone; }
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
