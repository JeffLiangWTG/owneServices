using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ExchangeRateWrapperController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ExchangeRateWrapperForm((BulkExchangeRateUpdater)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ExchangeRateWrapper; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BulkExchangeRateUpdater); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new BulkExchangeRateUpdater(Factory);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CurrenciesModify; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
