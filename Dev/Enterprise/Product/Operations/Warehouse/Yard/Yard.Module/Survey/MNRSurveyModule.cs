using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class MNRSurveyModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.MNRSurvey;

		protected override ControllerID ControllerID => ControllerIDs.MNRSurvey;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new MNRSurveyFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new MNRSurveyFilterControl((MNRSurveyCollection)GridCollection, (MNRSurveyFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new MNRSurveyCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;
	}
}
