using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.Freight.SailingDataVendor.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.SailingDataVendor.Module
{
	public class SailingScheduleImportingController : ZSingletonController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new VesselRoutingVoyagesImportPreviewForm(new VesselRoutingVoyagesFilter(Factory));
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SailingScheduleImporting; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.SailingScheduleImporting; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(VesselRoutingVoyagesFilter); }
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
