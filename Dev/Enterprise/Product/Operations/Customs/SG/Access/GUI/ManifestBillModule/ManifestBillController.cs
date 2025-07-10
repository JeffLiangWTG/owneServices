using System;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.Access.GUI
{
	class ManifestBillController : ASYCUDA.Module.ASYCUDAManifestBillController
	{
		public override ControllerID ID => ControllerIDs.Customs.ASYCUDA.SGAccess.ManifestBill;
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill;
		public override Type TypeOfTopLevelBusinessObject => typeof(AsycudaBill);
	}
}
