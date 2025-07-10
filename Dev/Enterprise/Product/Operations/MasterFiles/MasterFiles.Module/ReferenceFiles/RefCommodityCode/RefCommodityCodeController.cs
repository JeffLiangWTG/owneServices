using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for RefCommodityCode.
	/// </summary>
	public class RefCommodityCodeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefCommodityCodeController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.RefCommodityCode;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefCommodityCode; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefCommodityCode); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCommodityCodeForm((RefCommodityCode)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CommodityModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CommodityModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CommodityModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Commodity; }
		}
	}
}
