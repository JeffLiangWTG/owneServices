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
	public class EDICodeMappingModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Messaging.EDICodeMapping; }
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EDICodeMapping;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Messaging.EDICodeMapping);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new OrgPatternMatchOverrideCollectionForOrgs(Factory, new ZQuery());

		protected override IFilterControl GetNewFilterControl() => new EDICodeMappingUserControl(GridCollection, (EDICodeMappingFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EDICodeMappingFilterBusinessObject();
	}
}
