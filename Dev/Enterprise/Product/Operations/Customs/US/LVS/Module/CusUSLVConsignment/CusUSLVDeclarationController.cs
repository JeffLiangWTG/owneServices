using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.LVS.Module
{
	public class CusUSLVDeclarationController : CusUSLVConsignmentController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var result = base.GetForm(businessEntity);
			ModuleResultsPKCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.US.USLowValueEntriesBill);
			return result;
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(USConsignmentCombined);

		public override ControllerID ID => ControllerIDs.Customs.US.USLowValueEntriesDeclaration;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.USLowValueEntriesDeclaration;
	}
}
