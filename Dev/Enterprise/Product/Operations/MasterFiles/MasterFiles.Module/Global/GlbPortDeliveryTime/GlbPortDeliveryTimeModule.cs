using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbPortDeliveryTimeModule : ZFilterGridModule
	{
		public GlbPortDeliveryTimeModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbPortDeliveryTime; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbPortDeliveryTime);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbPortDeliveryTimeFilterControl(GridCollection, (GlbPortDeliveryTimeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			//TODO: Return a BusinessObjectCollection to be used for the MainForm grid.
			return new GlbPortDeliveryTimeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbPortDeliveryTimeFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GlbPortDeliveryTime; }
		}
	}
}
