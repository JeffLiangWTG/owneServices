using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	class WarningAcknowledgementController : ZController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WarningAcknowledgement; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.WarningAcknowledgement; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GenCustomAddOnRuleAck); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WarningAcknowledgementForm((GenCustomAddOnRuleAck)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WarningAcknowledgement; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WarningAcknowledgementEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WarningAcknowledgement; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WarningAcknowledgementView; }
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;
	}
}
