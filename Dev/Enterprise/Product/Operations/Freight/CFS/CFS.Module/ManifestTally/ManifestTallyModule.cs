using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	public class ManifestTallyModule : ZFilterGridModule
	{
		public ManifestTallyModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ManifestTally; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.CFSTallySheet }; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CFSTally; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.CFSManager; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ManifestTally);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ManifestTallyFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new TallyContainerCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ManifestTallyFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}
	}
}
