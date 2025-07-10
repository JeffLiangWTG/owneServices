using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCTeamSpecialistModule : USCFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.TeamSpecialist;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override IFilterControl GetNewFilterControl() => new USCTeamSpecialistFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCTeamSpecialistCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCTeamSpecialistFilterStripBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
