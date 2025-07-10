using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public abstract class OrgSupplierPartController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SupplierPart; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.SupplierPart; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSupplierPart); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrgSupplierPartForm((OrgSupplierPart)businessEntity);
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return null;
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.MasterFiles.Module.Res.GetData("PlugInTabPage|SupplierPart", "Customs"); } }

		#region CheckPoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsSupplierPart.IsAllowed ? Env.Security.CustomsSupplierPart : Env.Security.WhsConfigProductView; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsSupplierPartModify.IsAllowed ? Env.Security.CustomsSupplierPartModify : Env.Security.WhsConfigProductEdit; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsSupplierPartModify.IsAllowed ? Env.Security.CustomsSupplierPartModify : Env.Security.WhsConfigProductNew; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsSupplierPartModify.IsAllowed ? Env.Security.CustomsSupplierPartModify : Env.Security.WhsConfigProductDelete; }
		}

		#endregion

#if DEBUG
		#region For Testing

		internal SecurityCheckpoint CheckPointForDeleteForTesting => CheckPointForDelete;

		internal SecurityCheckpoint CheckPointForEditForTesting => CheckPointForEdit;

		internal SecurityCheckpoint CheckPointForNewForTesting => CheckPointForNew;

		internal SecurityCheckpoint CheckPointForViewForTesting => CheckPointForView;

		#endregion
#endif
	}
}
