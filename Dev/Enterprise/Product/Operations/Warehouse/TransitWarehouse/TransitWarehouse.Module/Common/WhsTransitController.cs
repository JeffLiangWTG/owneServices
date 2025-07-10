using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public abstract class WhsTransitController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException("Can only use this module to access Documents menu.");
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TransitWarehouse; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TransitWarehouse; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TransitWarehouse; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TransitWarehouse; }
		}

		#endregion
	}
}
