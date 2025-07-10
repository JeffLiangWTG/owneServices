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
	public class ProofOfPaymentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ZAControllerIDs.ZA404ProofOfPayment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ZAModuleIDs.ZA404ProofOfPayment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusEntryPayInfo); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZA404ProofOfPayment); }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZA404ProofOfPayment); }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZA404ProofOfPayment); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZA404ProofOfPayment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ProofOfPaymentForm((CusEntryPayInfo)businessEntity);
		}
	}
}
