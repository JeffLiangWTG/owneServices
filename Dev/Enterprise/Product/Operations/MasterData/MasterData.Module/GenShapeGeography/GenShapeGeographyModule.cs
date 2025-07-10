using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterData.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.Module
{
	[CodeAlive("WIP For Geography Module")]
	public class GenShapeGeographyModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GenShapeGeography;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Geography;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.GenShapeGeography);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GenShapeGeographyFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GenShapeGeographyFilterControl(GridCollection, (GenShapeGeographyFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new GenShapeGeographyCollection(Factory);
	}
}
