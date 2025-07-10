using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class DynamicPickFacesModule : RefreshableGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WhsConfigDynamicPickFaces;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsConfigDynamicPickFaces;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerCoreAnd4PL;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigDynamicPickFaces);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DynamicPickFacesFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DynamicPickFacesFilterControl((WhsDynamicPickFaceViewCollection)GridCollection, (DynamicPickFacesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsDynamicPickFaceViewCollection(Factory, this);
		}

		public override string[] GetTableNamesToMonitor() => new[] { WhsProductParamsByWhsAndClientSchema.Constants.TableName };
	}
}
