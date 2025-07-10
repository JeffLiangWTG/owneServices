using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ProductController : OrgSupplierPartController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsConfigProduct; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.WhsConfigProduct; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var form = (ZForm)base.GetForm(businessEntity);
			form.PlugInIDToSelectOnLoaded = ControllerIDs.WhsConfigProduct;
			form.ControllerID = ControllerIDs.WhsConfigProduct;
			return form;
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartFormWhsPlugin((OrgSupplierPart)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Warehouse.Transactions.Module.Res.GetData("PlugInTabPage|SupplierPart|Warehouse", "Warehouse"); } }
	}
}
