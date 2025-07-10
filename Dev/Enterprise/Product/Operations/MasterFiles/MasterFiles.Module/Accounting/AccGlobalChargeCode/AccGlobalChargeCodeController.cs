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
	public class AccGlobalChargeCodeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccGlobalChargeCodeController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccGlobalChargeCode;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccGlobalChargeCode; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccChargeCode); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccGlobalChargeCodeForm((AccChargeCode)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return AccChargeCode.CreateGlobalChargeCode(Factory);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GlobalChargeCodesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GlobalChargeCodesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GlobalChargeCodesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GlobalChargeCodes; }
		}

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity)
		{
			return Env.Security.GlobalChargeCodesCopy;
		}

		#endregion
	}
}
