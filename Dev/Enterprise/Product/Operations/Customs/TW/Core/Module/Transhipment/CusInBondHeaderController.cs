using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Customs.TW.Module.Res;

namespace Enterprise.Customs.TW.Transhipment.Module
{
	public class CusInBondHeaderController : ZController
	{
		public CusInBondHeaderController()
		{
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity) => new TranshipmentForm((CusInBondHeader)businessEntity);

		public override ControllerID ID => ControllerIDs.Customs.TW.Transhipment;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.TW.Transhipment;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusInBondHeader);

		#region Security
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.TWTranshipmentEdit;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.TWTranshipmentView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.TWTranshipmentNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
		#endregion

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|TWTranshipment", "Transhipment");
	}
}
