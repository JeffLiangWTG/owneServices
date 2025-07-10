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
	public class AccChargeCodeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccChargeCodeController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccChargeCode;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccChargeCode; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccChargeCode); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccChargeCodeForm((AccChargeCode)businessEntity);
		}

		#region Security Checkpoints

		static bool IsLinkedToGlobalChargeCode(BusinessObject bizObject)
		{
			var chargeCode = bizObject as AccChargeCode;
			return chargeCode != null && chargeCode.IsLinkedToGlobalChargeCode;
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ChargeCodesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ChargeCodesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ChargeCodesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ChargeCodes; }
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizo)
		{
			return IsLinkedToGlobalChargeCode(bizo) ? Env.Security.ChargeCodesLTGDelete : CheckPointForDelete;
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizo)
		{
			return IsLinkedToGlobalChargeCode(bizo) ? Env.Security.ChargeCodesLTGModify : CheckPointForEdit;
		}

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizo)
		{
			return IsLinkedToGlobalChargeCode(bizo) ? Env.Security.ChargeCodesLTGNew : CheckPointForNew;
		}

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizo)
		{
			return IsLinkedToGlobalChargeCode(bizo) ? Env.Security.ChargeCodesLTG : CheckPointForView;
		}

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject bizo)
		{
			return IsLinkedToGlobalChargeCode(bizo) ? Env.Security.ChargeCodesLTGCopy : Env.Security.ChargeCodesCopy;
		}

		#endregion
	}
}
