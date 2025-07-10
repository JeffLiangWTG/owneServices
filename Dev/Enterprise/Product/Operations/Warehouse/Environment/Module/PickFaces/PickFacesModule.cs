using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class PickFacesModule : RefreshableGridModule, IOperationalActionSupportable
	{
		public PickFacesModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsConfigPickFaces;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsConfigPickFaces;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerCoreAnd4PL;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		protected override bool AllowAdvancedDataAutomationWizard => true;

		protected override Type TypeOfElementsForImportWizard => typeof(WhsPickFace);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigPickFaces);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PickFacesFilterBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new PickFacesFilterControl((WhsPickFaceViewCollection)GridCollection, (PickFacesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsPickFaceViewCollection(Factory, this);
		}

		public override string[] GetTableNamesToMonitor() => new[] { WhsPickFaceSchema.Constants.TableName };

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new PickFacesOperationalActionsSupporter(); }
		}
	}
}
