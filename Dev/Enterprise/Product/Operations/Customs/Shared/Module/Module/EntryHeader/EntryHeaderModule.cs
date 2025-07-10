using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class EntryHeaderModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public EntryHeaderModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.EntryHeader;

		protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, (EntryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ModuleEntryHeaderCollection(Factory, GlbCompany.CurrentCompany);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();

		public override ZBool HasActions => false;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsDeclarationEntries;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EntryHeader);

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => operationalActionSupporter ?? (operationalActionSupporter = GetNewOperationalActionSupporter());
		OperationalActionSupporter operationalActionSupporter;

		protected virtual EntryHeaderOperationalActionSupporter GetNewOperationalActionSupporter() => new EntryHeaderOperationalActionSupporter();
	}
}
