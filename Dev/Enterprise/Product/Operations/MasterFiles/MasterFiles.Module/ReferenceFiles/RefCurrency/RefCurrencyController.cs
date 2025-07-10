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
	public class RefCurrencyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefCurrencyController()
		{
		}

		#region Standard Controller Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefCurrency; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefCurrency; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefCurrency); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCurrencyForm((RefCurrency)businessEntity);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CurrenciesModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CurrenciesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CurrenciesModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Currencies; }
		}

		#endregion
	}
}
