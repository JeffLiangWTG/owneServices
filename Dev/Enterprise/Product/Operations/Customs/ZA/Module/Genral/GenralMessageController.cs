using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.GUI;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class GenralMessageController : ZController
	{
		public override ControllerID ID => ZAControllerIDs.GenralMessage;

		public override ModuleIdentifier ModuleID => ZAModuleIDs.GenralMessage;

		public override Type TypeOfTopLevelBusinessObject => typeof(GENRALMessage);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAGenralMessage);

		protected override SecurityCheckpoint CheckPointForNew => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAGenralMessage);

		protected override SecurityCheckpoint CheckPointForEdit => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAGenralMessage);

		protected override SecurityCheckpoint CheckPointForDelete => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAGenralMessage);

		protected override IZForm GetForm(IBusiness businessEntity) => new GenralMessageForm((GENRALMessage)businessEntity);
	}
}
