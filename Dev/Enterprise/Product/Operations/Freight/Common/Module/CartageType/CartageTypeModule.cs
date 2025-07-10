using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Common.Module
{
	public class CartageTypeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CartageType; }
		}

		protected internal IBusinessObjectCollection GetNewGridCollectionInternal() => GetNewGridCollection();

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CommonCartageTypeCollection(Factory);
		}

		protected internal IFilterControl GetNewFilterControlInternal() => GetNewFilterControl();

		protected override IFilterControl GetNewFilterControl()
		{
			return new LocalCartageJobTypeFilterControl(GridCollection, (CartageTypeFilterBusinessObject)FilterBusinessObject);
		}

		protected internal FilterBusinessObject GetNewFilterBusinessObjectInternal() => GetNewFilterBusinessObject();

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CartageTypeFilterBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CartageType);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.LocalCartageJobType; }
		}
	}
}
