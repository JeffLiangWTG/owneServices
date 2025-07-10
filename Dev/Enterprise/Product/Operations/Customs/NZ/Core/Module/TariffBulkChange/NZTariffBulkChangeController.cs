using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module
{
	public class NZTariffBulkChangeController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NZTariffBulkChange topTariffBulkChange = new NZTariffBulkChange(factory);
			return new NZTariffBulkChangeStartForm(topTariffBulkChange);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TariffBulkChange; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.TariffBulkChange; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NZTariffBulkChange); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
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
