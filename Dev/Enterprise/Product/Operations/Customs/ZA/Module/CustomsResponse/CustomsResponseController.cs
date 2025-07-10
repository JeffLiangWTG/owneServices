using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class CustomsResponseController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ZAControllerIDs.CustomsResponse; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ZAModuleIDs.CustomsResponse; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CUSRESEDIMessage); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsResponse); }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsResponse); }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsResponse); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsResponse); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIMessageForm((EDIMessage)businessEntity);
		}
	}
}
