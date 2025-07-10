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
	class AccInvMsgController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.InvoiceMessagesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.InvoiceMessagesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.InvoiceMessagesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.InvoiceMessages; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccInvMsgForm((AccInvMsg)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccInvMsg; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccInvMsg); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccInvMsg; }
		}
	}
}
