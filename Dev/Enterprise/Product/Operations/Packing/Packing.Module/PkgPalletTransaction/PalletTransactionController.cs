using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Module
{
	public class PalletTransactionController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.PalletTransaction; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.PalletTransaction; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(PkgPalletTransaction); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new PalletTransactionForm((PkgPalletTransaction)businessEntity);
		}

		#region Security CheckPoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PalletTransactionView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PalletTransactionNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PalletTransactionEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PalletTransactionDelete; }
		}

		#endregion
	}
}
