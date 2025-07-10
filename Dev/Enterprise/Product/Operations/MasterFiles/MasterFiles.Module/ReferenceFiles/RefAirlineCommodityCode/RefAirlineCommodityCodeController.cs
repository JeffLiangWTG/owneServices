using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for RefAirlineCommodityCode.
	/// </summary>
	public class RefAirlineCommodityCodeController : ZController
	{
		public RefAirlineCommodityCodeController()
		{
		}

		public override ControllerID ID => ControllerIDs.RefAirlineCommodityCode;

		public override ModuleIdentifier ModuleID => ModuleIDs.RefAirlineCommodityCode;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefAirlineCommodityCode);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.Commodity;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CommodityModify;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CommodityModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CommodityModify;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("Does not support New functionality");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowEditFormNotSupportedException("Does not support Edit functionality");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowViewFormNotSupportedException("Does not support View functionality");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("Does not support Delete functionality");
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleTemplateCopyNotSupportedException("Does not support Template Copy functionality");
		}
	}
}
