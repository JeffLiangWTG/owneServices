using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Classification = Enterprise.Customs.SG.V4.Business.Classification;

namespace Enterprise.Customs.SG.V4.Module
{
	/// <summary>
	/// Module Controller for SGPlaces.
	/// </summary>
	public class ClassificationModule : Customs.Module.SingleTariffClassificationModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.SG.SG4Classification);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BaseClassificationCollection<Classification>(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ClassificationFilterStripControl(GridCollection, (ClassificationFilterStripBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ClassificationFilterStripBusinessObject();
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.SG.SG4Classification; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CustomsClassification; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}
	}
}
